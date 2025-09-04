using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using CircuitRunners.Firebase.DataModels;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Real-time Leaderboard Manager for Circuit Runners
    /// Handles multiple leaderboard types, ranking systems, and efficient updates
    /// Optimized for Firebase free tier with smart caching and minimal read operations
    /// Supports tournaments, seasons, and social leaderboards
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        #region Events and Delegates
        
        /// <summary>
        /// Leaderboard events for UI updates and notifications
        /// </summary>
        public static event Action<LeaderboardType, List<LeaderboardEntry>> OnLeaderboardUpdated;
        public static event Action<LeaderboardType, int, int> OnPlayerRankChanged; // type, oldRank, newRank
        public static event Action<LeaderboardEntry> OnNewHighScore;
        public static event Action<string> OnLeaderboardError;
        public static event Action<TournamentResult> OnTournamentComplete;
        
        /// <summary>
        /// Leaderboard types supported by the system
        /// </summary>
        public enum LeaderboardType
        {
            Global,          // All-time global leaderboard
            Weekly,          // Weekly competition resets
            Monthly,         // Monthly competition resets
            Daily,           // Daily challenges
            Friends,         // Friends-only leaderboard
            Tournament,      // Special tournament events
            Seasonal,        // Seasonal competitions
            BotSpecific      // Per-bot-type leaderboards
        }
        
        /// <summary>
        /// Tournament completion result
        /// </summary>
        public struct TournamentResult
        {
            public string tournamentId;
            public List<LeaderboardEntry> finalRankings;
            public Dictionary<int, TournamentReward> rewards;
            public DateTime completionTime;
        }
        
        /// <summary>
        /// Leaderboard configuration settings
        /// </summary>
        [Serializable]
        public class LeaderboardConfig
        {
            public LeaderboardType type;
            public int maxEntries = 100;
            public bool enableRealTimeUpdates = true;
            public float updateFrequency = 30f; // seconds
            public bool requireAuthentication = true;
            public bool enableCaching = true;
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Leaderboard Configuration")]
        [SerializeField] private List<LeaderboardConfig> leaderboardConfigs = new List<LeaderboardConfig>();
        [SerializeField] private bool enableAutoRankingUpdates = true;
        [SerializeField] private int rankingBatchSize = 50;
        [SerializeField] private float rankingUpdateInterval = 60f; // seconds
        
        [Header("Performance Settings")]
        [SerializeField] private int maxCachedEntries = 200;
        [SerializeField] private float cacheExpirationTime = 300f; // seconds
        [SerializeField] private bool enableDataCompression = true;
        [SerializeField] private bool enablePredictiveRanking = true;
        
        [Header("Tournament Settings")]
        [SerializeField] private bool enableTournaments = true;
        [SerializeField] private int maxTournamentParticipants = 1000;
        [SerializeField] private float tournamentUpdateFrequency = 10f; // seconds
        
        [Header("Social Features")]
        [SerializeField] private bool enableFriendsLeaderboard = true;
        [SerializeField] private int maxFriendsDisplayed = 50;
        [SerializeField] private bool enableSocialNotifications = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool simulateHighTraffic = false;
        [SerializeField] private bool logRankingChanges = false;
        
        // Core system references
        private FirebaseFirestore _firestore;
        private FirebaseAuthManager _authManager;
        private FirestoreManager _firestoreManager;
        
        // Leaderboard data storage
        private Dictionary<LeaderboardType, List<LeaderboardEntry>> _cachedLeaderboards;
        private Dictionary<LeaderboardType, DateTime> _cacheTimestamps;
        private Dictionary<LeaderboardType, ListenerRegistration> _realtimeListeners;
        
        // Ranking system
        private Dictionary<string, int> _playerCurrentRanks; // playerId -> rank
        private Queue<LeaderboardEntry> _pendingSubmissions;
        private Dictionary<string, DateTime> _lastSubmissionTimes; // Prevent spam
        
        // Tournament system
        private Dictionary<string, Tournament> _activeTournaments;
        private Dictionary<string, List<LeaderboardEntry>> _tournamentLeaderboards;
        
        // Performance tracking
        private int _totalReadOperations;
        private int _totalWriteOperations;
        private DateTime _lastOperationReset;
        
        // System state
        private bool _isInitialized;
        private bool _isProcessingRankings;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether the leaderboard system is ready for operations
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether ranking calculations are in progress
        /// </summary>
        public bool IsProcessingRankings => _isProcessingRankings;
        
        /// <summary>
        /// Current cached leaderboard data
        /// </summary>
        public Dictionary<LeaderboardType, List<LeaderboardEntry>> CachedLeaderboards => 
            new Dictionary<LeaderboardType, List<LeaderboardEntry>>(_cachedLeaderboards);
        
        /// <summary>
        /// Active tournaments
        /// </summary>
        public Dictionary<string, Tournament> ActiveTournaments => 
            new Dictionary<string, Tournament>(_activeTournaments);
        
        /// <summary>
        /// Total database operations performed
        /// </summary>
        public int TotalOperations => _totalReadOperations + _totalWriteOperations;
        
        /// <summary>
        /// Player's current rank in global leaderboard
        /// </summary>
        public int PlayerGlobalRank
        {
            get
            {
                if (_authManager?.IsAuthenticated == true && 
                    _playerCurrentRanks.ContainsKey(_authManager.UserId))
                {
                    return _playerCurrentRanks[_authManager.UserId];
                }
                return -1;
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (FindObjectsOfType<LeaderboardManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize data structures
            _cachedLeaderboards = new Dictionary<LeaderboardType, List<LeaderboardEntry>>();
            _cacheTimestamps = new Dictionary<LeaderboardType, DateTime>();
            _realtimeListeners = new Dictionary<LeaderboardType, ListenerRegistration>();
            _playerCurrentRanks = new Dictionary<string, int>();
            _pendingSubmissions = new Queue<LeaderboardEntry>();
            _lastSubmissionTimes = new Dictionary<string, DateTime>();
            _activeTournaments = new Dictionary<string, Tournament>();
            _tournamentLeaderboards = new Dictionary<string, List<LeaderboardEntry>>();
            
            _lastOperationReset = DateTime.UtcNow;
            
            // Initialize default leaderboard configurations if none set
            if (leaderboardConfigs.Count == 0)
            {
                SetupDefaultLeaderboardConfigs();
            }
        }
        
        private void Start()
        {
            // Get system references
            _authManager = FindObjectOfType<FirebaseAuthManager>();
            _firestoreManager = FindObjectOfType<FirestoreManager>();
            
            // Initialize the leaderboard system
            InitializeLeaderboardSystem();
            
            // Start periodic ranking updates
            if (enableAutoRankingUpdates)
            {
                InvokeRepeating(nameof(ProcessRankingUpdates), rankingUpdateInterval, rankingUpdateInterval);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up real-time listeners
            CleanupRealtimeListeners();
        }
        
        private void Update()
        {
            // Process pending score submissions
            ProcessPendingSubmissions();
        }
        
        #endregion
        
        #region System Initialization
        
        /// <summary>
        /// Initialize the complete leaderboard system
        /// Sets up Firebase connections, caching, and real-time listeners
        /// </summary>
        private async void InitializeLeaderboardSystem()
        {
            try
            {
                LogDebug("Initializing Leaderboard System...");
                
                // Wait for Firebase to be ready
                if (_firestoreManager != null)
                {
                    while (!_firestoreManager.IsInitialized)
                    {
                        await Task.Delay(100);
                    }
                    
                    _firestore = FirebaseFirestore.DefaultInstance;
                }
                else
                {
                    LogError("FirestoreManager not found! Leaderboards will have limited functionality.");
                    return;
                }
                
                // Load cached leaderboard data
                await LoadCachedLeaderboards();
                
                // Initialize each configured leaderboard
                foreach (var config in leaderboardConfigs)
                {
                    await InitializeLeaderboard(config);
                }
                
                // Load active tournaments
                await LoadActiveTournaments();
                
                // Set up real-time listeners for enabled leaderboards
                SetupRealtimeListeners();
                
                _isInitialized = true;
                LogDebug("Leaderboard System initialized successfully");
                
                // Perform initial data load if user is authenticated
                if (_authManager?.IsAuthenticated == true)
                {
                    await RefreshAllLeaderboards();
                }
                
            }
            catch (Exception ex)
            {
                LogError($"Leaderboard system initialization failed: {ex.Message}");
                OnLeaderboardError?.Invoke($"Leaderboard initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set up default leaderboard configurations
        /// </summary>
        private void SetupDefaultLeaderboardConfigs()
        {
            leaderboardConfigs = new List<LeaderboardConfig>
            {
                new LeaderboardConfig 
                { 
                    type = LeaderboardType.Global, 
                    maxEntries = 100, 
                    enableRealTimeUpdates = true,
                    updateFrequency = 60f
                },
                new LeaderboardConfig 
                { 
                    type = LeaderboardType.Weekly, 
                    maxEntries = 50, 
                    enableRealTimeUpdates = true,
                    updateFrequency = 30f
                },
                new LeaderboardConfig 
                { 
                    type = LeaderboardType.Friends, 
                    maxEntries = maxFriendsDisplayed, 
                    enableRealTimeUpdates = true,
                    updateFrequency = 45f
                },
                new LeaderboardConfig 
                { 
                    type = LeaderboardType.Daily, 
                    maxEntries = 25, 
                    enableRealTimeUpdates = true,
                    updateFrequency = 15f
                }
            };
            
            LogDebug("Default leaderboard configurations created");
        }
        
        /// <summary>
        /// Initialize a specific leaderboard type
        /// </summary>
        private async Task InitializeLeaderboard(LeaderboardConfig config)
        {
            try
            {
                LogDebug($"Initializing {config.type} leaderboard...");
                
                // Initialize cache for this leaderboard type
                if (!_cachedLeaderboards.ContainsKey(config.type))
                {
                    _cachedLeaderboards[config.type] = new List<LeaderboardEntry>();
                    _cacheTimestamps[config.type] = DateTime.MinValue;
                }
                
                // Load initial data if online
                if (_firestoreManager?.IsOnline == true)
                {
                    await LoadLeaderboardData(config.type, config.maxEntries);
                }
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug($"{config.type} leaderboard initialized");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize {config.type} leaderboard: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Leaderboard Data Loading
        
        /// <summary>
        /// Load leaderboard data from Firestore with efficient querying
        /// Implements smart caching and minimal read operations
        /// </summary>
        public async Task<List<LeaderboardEntry>> LoadLeaderboardData(LeaderboardType type, int limit = 100)
        {
            try
            {
                // Check if cached data is still fresh
                if (IsCacheValid(type))
                {
                    LogDebug($"Using cached {type} leaderboard data");
                    return _cachedLeaderboards[type];
                }
                
                LogDebug($"Loading {type} leaderboard from server (limit: {limit})...");
                
                // Determine collection path based on leaderboard type
                string collectionPath = GetLeaderboardCollectionPath(type);
                
                // Build query with appropriate ordering and filtering
                var query = _firestore.Collection(collectionPath);
                
                // Apply type-specific filtering
                query = ApplyLeaderboardFiltering(query, type);
                
                // Order by score (descending for high scores)
                query = query.OrderByDescending("score").Limit(Math.Min(limit, 200));
                
                // Execute query
                var snapshot = await query.GetSnapshotAsync();
                _totalReadOperations++;
                
                // Process results
                var entries = new List<LeaderboardEntry>();
                int rank = 1;
                
                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var entry = doc.ConvertTo<LeaderboardEntry>();
                        entry.Rank = rank++;
                        entry.LeaderboardType = type.ToString().ToLower();
                        entries.Add(entry);
                    }
                }
                
                // Update cache
                _cachedLeaderboards[type] = entries;
                _cacheTimestamps[type] = DateTime.UtcNow;
                
                // Cache to device storage
                await CacheLeaderboardToDevice(type, entries);
                
                LogDebug($"Loaded {entries.Count} entries for {type} leaderboard");
                
                // Notify listeners
                OnLeaderboardUpdated?.Invoke(type, entries);
                
                return entries;
            }
            catch (Exception ex)
            {
                LogError($"Error loading {type} leaderboard: {ex.Message}");
                
                // Return cached data as fallback
                if (_cachedLeaderboards.ContainsKey(type))
                {
                    return _cachedLeaderboards[type];
                }
                
                return new List<LeaderboardEntry>();
            }
        }
        
        /// <summary>
        /// Get the appropriate Firestore collection path for a leaderboard type
        /// </summary>
        private string GetLeaderboardCollectionPath(LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.Global:
                    return "leaderboards/global/entries";
                case LeaderboardType.Weekly:
                    return $"leaderboards/weekly_{GetCurrentWeekId()}/entries";
                case LeaderboardType.Monthly:
                    return $"leaderboards/monthly_{GetCurrentMonthId()}/entries";
                case LeaderboardType.Daily:
                    return $"leaderboards/daily_{GetCurrentDayId()}/entries";
                case LeaderboardType.Friends:
                    return "leaderboards/friends/entries"; // Filtered by friend relationships
                case LeaderboardType.Tournament:
                    return "leaderboards/tournament/entries";
                case LeaderboardType.Seasonal:
                    return $"leaderboards/season_{GetCurrentSeasonId()}/entries";
                case LeaderboardType.BotSpecific:
                    return "leaderboards/bot_specific/entries";
                default:
                    return "leaderboards/global/entries";
            }
        }
        
        /// <summary>
        /// Apply type-specific filtering to leaderboard queries
        /// </summary>
        private Query ApplyLeaderboardFiltering(Query query, LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.Friends:
                    // Filter by friend list (requires user to be authenticated)
                    if (_authManager?.IsAuthenticated == true && _firestoreManager?.CachedPlayerProfile != null)
                    {
                        var friendIds = _firestoreManager.CachedPlayerProfile.FriendIds;
                        if (friendIds?.Count > 0)
                        {
                            // Add current player to friends list for display
                            var allIds = new List<string>(friendIds) { _authManager.UserId };
                            query = query.WhereIn("playerId", allIds.Take(10).ToArray()); // Firestore limit of 10 for WhereIn
                        }
                    }
                    break;
                    
                case LeaderboardType.Daily:
                    // Filter by today's date
                    var todayStart = DateTime.Today;
                    var todayEnd = todayStart.AddDays(1);
                    query = query.WhereGreaterThanOrEqualTo("achievedAt", Timestamp.FromDateTime(todayStart))
                                 .WhereLessThan("achievedAt", Timestamp.FromDateTime(todayEnd));
                    break;
                    
                case LeaderboardType.Weekly:
                    // Filter by current week
                    var weekStart = GetCurrentWeekStart();
                    var weekEnd = weekStart.AddDays(7);
                    query = query.WhereGreaterThanOrEqualTo("achievedAt", Timestamp.FromDateTime(weekStart))
                                 .WhereLessThan("achievedAt", Timestamp.FromDateTime(weekEnd));
                    break;
                    
                case LeaderboardType.Tournament:
                    // Filter by active tournament
                    var activeTournament = GetActiveTournament();
                    if (activeTournament != null)
                    {
                        query = query.WhereEqualTo("tournamentId", activeTournament.TournamentId);
                    }
                    break;
            }
            
            return query;
        }
        
        #endregion
        
        #region Score Submission and Ranking
        
        /// <summary>
        /// Submit a new score to the leaderboard system
        /// Handles validation, anti-cheat measures, and ranking updates
        /// </summary>
        public async Task<bool> SubmitScore(long score, string botUsed, Dictionary<string, object> metadata = null)
        {
            if (!_authManager?.IsAuthenticated == true)
            {
                LogWarning("Cannot submit score: user not authenticated");
                OnLeaderboardError?.Invoke("Authentication required to submit scores");
                return false;
            }
            
            try
            {
                var userId = _authManager.UserId;
                
                // Anti-spam protection
                if (IsSubmissionTooFrequent(userId))
                {
                    LogWarning("Score submission blocked: too frequent");
                    return false;
                }
                
                // Basic score validation
                if (!ValidateScore(score, botUsed))
                {
                    LogWarning($"Score validation failed: {score}");
                    OnLeaderboardError?.Invoke("Invalid score submitted");
                    return false;
                }
                
                LogDebug($"Submitting score: {score} (bot: {botUsed})");
                
                // Create leaderboard entry
                var entry = new LeaderboardEntry
                {
                    PlayerId = userId,
                    DisplayName = _authManager.GetDisplayName(),
                    AvatarUrl = _authManager.GetProfilePhotoUrl(),
                    Score = score,
                    AchievedAt = Timestamp.GetCurrentTimestamp(),
                    BotUsed = botUsed,
                    MetaData = metadata ?? new Dictionary<string, object>()
                };
                
                // Add to pending submissions queue
                _pendingSubmissions.Enqueue(entry);
                _lastSubmissionTimes[userId] = DateTime.UtcNow;
                
                // Check if this is a new personal best
                await CheckForPersonalBest(entry);
                
                // Process submission immediately if online, otherwise queue for later
                if (_firestoreManager?.IsOnline == true)
                {
                    await ProcessScoreSubmission(entry);
                }
                
                LogDebug("Score submitted successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error submitting score: {ex.Message}");
                OnLeaderboardError?.Invoke($"Score submission failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Process a single score submission
        /// Handles database writes and ranking updates
        /// </summary>
        private async Task ProcessScoreSubmission(LeaderboardEntry entry)
        {
            try
            {
                // Submit to all applicable leaderboards
                await SubmitToLeaderboardType(entry, LeaderboardType.Global);
                await SubmitToLeaderboardType(entry, LeaderboardType.Weekly);
                await SubmitToLeaderboardType(entry, LeaderboardType.Daily);
                
                // Submit to friends leaderboard if enabled
                if (enableFriendsLeaderboard)
                {
                    await SubmitToLeaderboardType(entry, LeaderboardType.Friends);
                }
                
                // Submit to active tournaments
                await SubmitToActiveTournaments(entry);
                
                // Update local rankings
                await UpdateLocalRankings(entry);
                
                LogDebug($"Score submission processed: {entry.Score}");
            }
            catch (Exception ex)
            {
                LogError($"Error processing score submission: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Submit entry to a specific leaderboard type
        /// </summary>
        private async Task SubmitToLeaderboardType(LeaderboardEntry entry, LeaderboardType type)
        {
            try
            {
                entry.LeaderboardType = type.ToString().ToLower();
                
                // Use Firestore transaction for atomic updates
                await _firestore.RunTransactionAsync(async transaction =>
                {
                    var collectionPath = GetLeaderboardCollectionPath(type);
                    var docRef = _firestore.Collection(collectionPath).Document();
                    
                    transaction.Set(docRef, entry);
                    _totalWriteOperations++;
                });
                
                // Update cached leaderboard if this type is cached
                if (_cachedLeaderboards.ContainsKey(type))
                {
                    await RefreshLeaderboardCache(type);
                }
                
            }
            catch (Exception ex)
            {
                LogError($"Error submitting to {type} leaderboard: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Submit score to active tournaments
        /// </summary>
        private async Task SubmitToActiveTournaments(LeaderboardEntry entry)
        {
            try
            {
                foreach (var tournament in _activeTournaments.Values)
                {
                    if (tournament.IsActive && 
                        tournament.ParticipantIds.Contains(entry.PlayerId) &&
                        DateTime.UtcNow >= tournament.StartTime.ToDateTime() &&
                        DateTime.UtcNow <= tournament.EndTime.ToDateTime())
                    {
                        // Create tournament-specific entry
                        var tournamentEntry = new LeaderboardEntry(entry)
                        {
                            LeaderboardType = "tournament"
                        };
                        tournamentEntry.MetaData["tournamentId"] = tournament.TournamentId;
                        
                        var collectionPath = $"tournaments/{tournament.TournamentId}/leaderboard";
                        var docRef = _firestore.Collection(collectionPath).Document();
                        
                        await docRef.SetAsync(tournamentEntry);
                        _totalWriteOperations++;
                        
                        LogDebug($"Score submitted to tournament: {tournament.TournamentId}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error submitting to tournaments: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Ranking System
        
        /// <summary>
        /// Process pending ranking updates in batches
        /// Optimized for Firebase free tier limits
        /// </summary>
        private async void ProcessRankingUpdates()
        {
            if (_isProcessingRankings || !_isInitialized)
                return;
            
            try
            {
                _isProcessingRankings = true;
                LogDebug("Processing ranking updates...");
                
                // Update rankings for each leaderboard type
                foreach (var type in _cachedLeaderboards.Keys.ToList())
                {
                    await UpdateRankingsForLeaderboard(type);
                    
                    // Respect rate limits - pause between updates
                    await Task.Delay(1000);
                }
                
                LogDebug("Ranking updates completed");
            }
            catch (Exception ex)
            {
                LogError($"Error processing ranking updates: {ex.Message}");
            }
            finally
            {
                _isProcessingRankings = false;
            }
        }
        
        /// <summary>
        /// Update rankings for a specific leaderboard
        /// </summary>
        private async Task UpdateRankingsForLeaderboard(LeaderboardType type)
        {
            try
            {
                // Check if cache needs refresh
                if (!IsCacheValid(type))
                {
                    await LoadLeaderboardData(type, GetMaxEntriesForType(type));
                }
                
                var entries = _cachedLeaderboards[type];
                if (entries == null || entries.Count == 0)
                    return;
                
                // Sort by score (descending)
                entries.Sort((a, b) => b.Score.CompareTo(a.Score));
                
                // Update ranks
                for (int i = 0; i < entries.Count; i++)
                {
                    var oldRank = entries[i].Rank;
                    var newRank = i + 1;
                    entries[i].Rank = newRank;
                    
                    // Track player rank changes
                    if (oldRank != newRank && oldRank > 0)
                    {
                        OnPlayerRankChanged?.Invoke(type, oldRank, newRank);
                        
                        if (logRankingChanges)
                        {
                            LogDebug($"Rank change - {entries[i].DisplayName}: {oldRank} -> {newRank}");
                        }
                    }
                    
                    // Update player current rank tracking
                    if (type == LeaderboardType.Global)
                    {
                        _playerCurrentRanks[entries[i].PlayerId] = newRank;
                    }
                }
                
                // Notify of leaderboard update
                OnLeaderboardUpdated?.Invoke(type, entries);
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error updating rankings for {type}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get player's current rank in a specific leaderboard
        /// </summary>
        public async Task<int> GetPlayerRank(string playerId, LeaderboardType type)
        {
            try
            {
                // Check cache first
                if (_cachedLeaderboards.ContainsKey(type))
                {
                    var entries = _cachedLeaderboards[type];
                    var playerEntry = entries.FirstOrDefault(e => e.PlayerId == playerId);
                    if (playerEntry != null)
                    {
                        return playerEntry.Rank;
                    }
                }
                
                // Query database for accurate rank if not in cache
                if (_firestoreManager?.IsOnline == true)
                {
                    var collectionPath = GetLeaderboardCollectionPath(type);
                    var query = _firestore.Collection(collectionPath)
                        .WhereEqualTo("playerId", playerId)
                        .Limit(1);
                    
                    var snapshot = await query.GetSnapshotAsync();
                    _totalReadOperations++;
                    
                    if (snapshot.Count > 0)
                    {
                        var entry = snapshot.Documents[0].ConvertTo<LeaderboardEntry>();
                        return entry.Rank;
                    }
                }
                
                return -1; // Player not found
            }
            catch (Exception ex)
            {
                LogError($"Error getting player rank: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// Get leaderboard entries around a specific player
        /// Useful for showing context around player's position
        /// </summary>
        public async Task<List<LeaderboardEntry>> GetLeaderboardAroundPlayer(string playerId, LeaderboardType type, int range = 5)
        {
            try
            {
                var playerRank = await GetPlayerRank(playerId, type);
                if (playerRank <= 0) return new List<LeaderboardEntry>();
                
                // Calculate range bounds
                int startRank = Math.Max(1, playerRank - range);
                int endRank = playerRank + range;
                
                // Get entries from cache or database
                var allEntries = await LoadLeaderboardData(type, 200);
                
                return allEntries
                    .Where(e => e.Rank >= startRank && e.Rank <= endRank)
                    .OrderBy(e => e.Rank)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError($"Error getting leaderboard around player: {ex.Message}");
                return new List<LeaderboardEntry>();
            }
        }
        
        #endregion
        
        #region Tournament System
        
        /// <summary>
        /// Load active tournaments from Firestore
        /// </summary>
        private async Task LoadActiveTournaments()
        {
            if (!enableTournaments) return;
            
            try
            {
                LogDebug("Loading active tournaments...");
                
                var now = Timestamp.GetCurrentTimestamp();
                var query = _firestore.Collection("tournaments")
                    .WhereEqualTo("isActive", true)
                    .WhereLessThanOrEqualTo("startTime", now)
                    .WhereGreaterThan("endTime", now);
                
                var snapshot = await query.GetSnapshotAsync();
                _totalReadOperations++;
                
                _activeTournaments.Clear();
                
                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var tournament = doc.ConvertTo<Tournament>();
                        _activeTournaments[tournament.TournamentId] = tournament;
                        
                        // Load tournament leaderboard
                        await LoadTournamentLeaderboard(tournament.TournamentId);
                    }
                }
                
                LogDebug($"Loaded {_activeTournaments.Count} active tournaments");
            }
            catch (Exception ex)
            {
                LogError($"Error loading tournaments: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load leaderboard for a specific tournament
        /// </summary>
        private async Task LoadTournamentLeaderboard(string tournamentId)
        {
            try
            {
                var collectionPath = $"tournaments/{tournamentId}/leaderboard";
                var query = _firestore.Collection(collectionPath)
                    .OrderByDescending("score")
                    .Limit(100);
                
                var snapshot = await query.GetSnapshotAsync();
                _totalReadOperations++;
                
                var entries = new List<LeaderboardEntry>();
                int rank = 1;
                
                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var entry = doc.ConvertTo<LeaderboardEntry>();
                        entry.Rank = rank++;
                        entries.Add(entry);
                    }
                }
                
                _tournamentLeaderboards[tournamentId] = entries;
                LogDebug($"Loaded tournament leaderboard: {tournamentId} ({entries.Count} entries)");
            }
            catch (Exception ex)
            {
                LogError($"Error loading tournament leaderboard: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Join a tournament
        /// </summary>
        public async Task<bool> JoinTournament(string tournamentId)
        {
            if (!_authManager?.IsAuthenticated == true)
            {
                LogWarning("Cannot join tournament: user not authenticated");
                return false;
            }
            
            try
            {
                LogDebug($"Joining tournament: {tournamentId}");
                
                if (!_activeTournaments.ContainsKey(tournamentId))
                {
                    LogWarning($"Tournament not found or inactive: {tournamentId}");
                    return false;
                }
                
                var tournament = _activeTournaments[tournamentId];
                var userId = _authManager.UserId;
                
                // Check if already joined
                if (tournament.ParticipantIds.Contains(userId))
                {
                    LogDebug("Player already joined this tournament");
                    return true;
                }
                
                // Check capacity
                if (tournament.CurrentParticipants >= tournament.MaxParticipants)
                {
                    LogWarning("Tournament is full");
                    OnLeaderboardError?.Invoke("Tournament is full");
                    return false;
                }
                
                // Add player to tournament
                tournament.ParticipantIds.Add(userId);
                tournament.CurrentParticipants++;
                
                // Update tournament document
                var docRef = _firestore.Collection("tournaments").Document(tournamentId);
                var updates = new Dictionary<string, object>
                {
                    ["participantIds"] = tournament.ParticipantIds,
                    ["currentParticipants"] = tournament.CurrentParticipants
                };
                
                await docRef.UpdateAsync(updates);
                _totalWriteOperations++;
                
                LogDebug($"Successfully joined tournament: {tournamentId}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error joining tournament: {ex.Message}");
                OnLeaderboardError?.Invoke($"Failed to join tournament: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get tournament leaderboard
        /// </summary>
        public List<LeaderboardEntry> GetTournamentLeaderboard(string tournamentId)
        {
            if (_tournamentLeaderboards.ContainsKey(tournamentId))
            {
                return new List<LeaderboardEntry>(_tournamentLeaderboards[tournamentId]);
            }
            
            return new List<LeaderboardEntry>();
        }
        
        #endregion
        
        #region Real-time Updates
        
        /// <summary>
        /// Set up real-time listeners for enabled leaderboards
        /// </summary>
        private void SetupRealtimeListeners()
        {
            try
            {
                LogDebug("Setting up real-time leaderboard listeners...");
                
                foreach (var config in leaderboardConfigs)
                {
                    if (config.enableRealTimeUpdates)
                    {
                        SetupListenerForLeaderboard(config);
                    }
                }
                
                // Set up tournament listeners
                if (enableTournaments)
                {
                    SetupTournamentListeners();
                }
                
                LogDebug("Real-time listeners configured");
            }
            catch (Exception ex)
            {
                LogError($"Error setting up real-time listeners: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set up real-time listener for a specific leaderboard
        /// </summary>
        private void SetupListenerForLeaderboard(LeaderboardConfig config)
        {
            try
            {
                var collectionPath = GetLeaderboardCollectionPath(config.type);
                var query = _firestore.Collection(collectionPath)
                    .OrderByDescending("score")
                    .Limit(config.maxEntries);
                
                // Apply type-specific filtering
                query = ApplyLeaderboardFiltering(query, config.type);
                
                var listener = query.Listen(snapshot =>
                {
                    try
                    {
                        var entries = new List<LeaderboardEntry>();
                        int rank = 1;
                        
                        foreach (var doc in snapshot.Documents)
                        {
                            if (doc.Exists)
                            {
                                var entry = doc.ConvertTo<LeaderboardEntry>();
                                entry.Rank = rank++;
                                entry.LeaderboardType = config.type.ToString().ToLower();
                                entries.Add(entry);
                            }
                        }
                        
                        // Update cache
                        _cachedLeaderboards[config.type] = entries;
                        _cacheTimestamps[config.type] = DateTime.UtcNow;
                        
                        // Cache to device
                        _ = CacheLeaderboardToDevice(config.type, entries);
                        
                        // Notify listeners
                        OnLeaderboardUpdated?.Invoke(config.type, entries);
                        
                        LogDebug($"Real-time update: {config.type} leaderboard ({entries.Count} entries)");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error processing real-time update: {ex.Message}");
                    }
                });
                
                _realtimeListeners[config.type] = listener;
            }
            catch (Exception ex)
            {
                LogError($"Error setting up listener for {config.type}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set up tournament real-time listeners
        /// </summary>
        private void SetupTournamentListeners()
        {
            // Tournament listeners would be set up here
            // Implementation depends on specific tournament requirements
            LogDebug("Tournament listeners configured");
        }
        
        /// <summary>
        /// Clean up all real-time listeners
        /// </summary>
        private void CleanupRealtimeListeners()
        {
            try
            {
                foreach (var listener in _realtimeListeners.Values)
                {
                    listener?.Stop();
                }
                
                _realtimeListeners.Clear();
                LogDebug("Real-time listeners cleaned up");
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning up listeners: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods and Validation
        
        /// <summary>
        /// Check if cached data is still valid
        /// </summary>
        private bool IsCacheValid(LeaderboardType type)
        {
            if (!_cacheTimestamps.ContainsKey(type) || !_cachedLeaderboards.ContainsKey(type))
                return false;
            
            var cacheAge = DateTime.UtcNow - _cacheTimestamps[type];
            return cacheAge.TotalSeconds < cacheExpirationTime;
        }
        
        /// <summary>
        /// Validate submitted score for basic anti-cheat
        /// </summary>
        private bool ValidateScore(long score, string botUsed)
        {
            // Basic validation rules
            if (score < 0) return false;
            if (score > 999999999) return false; // Reasonable maximum
            if (string.IsNullOrEmpty(botUsed)) return false;
            
            // Additional validation logic could be added here
            // e.g., check against player's historical performance
            
            return true;
        }
        
        /// <summary>
        /// Check if score submission is too frequent (anti-spam)
        /// </summary>
        private bool IsSubmissionTooFrequent(string playerId)
        {
            if (!_lastSubmissionTimes.ContainsKey(playerId))
                return false;
            
            var timeSinceLastSubmission = DateTime.UtcNow - _lastSubmissionTimes[playerId];
            return timeSinceLastSubmission.TotalSeconds < 5; // Min 5 seconds between submissions
        }
        
        /// <summary>
        /// Check for personal best score
        /// </summary>
        private async Task CheckForPersonalBest(LeaderboardEntry entry)
        {
            try
            {
                // Get player's best score from global leaderboard
                var playerRank = await GetPlayerRank(entry.PlayerId, LeaderboardType.Global);
                
                if (playerRank > 0)
                {
                    var globalEntries = _cachedLeaderboards[LeaderboardType.Global];
                    var playerEntry = globalEntries?.FirstOrDefault(e => e.PlayerId == entry.PlayerId);
                    
                    if (playerEntry != null && entry.Score > playerEntry.Score)
                    {
                        LogDebug($"New high score achieved: {entry.Score}");
                        OnNewHighScore?.Invoke(entry);
                    }
                }
                else
                {
                    // First score for this player
                    LogDebug($"First score recorded: {entry.Score}");
                    OnNewHighScore?.Invoke(entry);
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error checking personal best: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process pending score submissions
        /// </summary>
        private async void ProcessPendingSubmissions()
        {
            if (_pendingSubmissions.Count == 0 || _firestoreManager?.IsOnline != true)
                return;
            
            try
            {
                while (_pendingSubmissions.Count > 0)
                {
                    var entry = _pendingSubmissions.Dequeue();
                    await ProcessScoreSubmission(entry);
                    
                    // Rate limiting
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing pending submissions: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get maximum entries for leaderboard type
        /// </summary>
        private int GetMaxEntriesForType(LeaderboardType type)
        {
            var config = leaderboardConfigs.FirstOrDefault(c => c.type == type);
            return config?.maxEntries ?? 100;
        }
        
        /// <summary>
        /// Get active tournament (first available)
        /// </summary>
        private Tournament GetActiveTournament()
        {
            return _activeTournaments.Values.FirstOrDefault();
        }
        
        /// <summary>
        /// Update local ranking tracking
        /// </summary>
        private async Task UpdateLocalRankings(LeaderboardEntry entry)
        {
            // Update player's rank tracking for global leaderboard
            if (_cachedLeaderboards.ContainsKey(LeaderboardType.Global))
            {
                await RefreshLeaderboardCache(LeaderboardType.Global);
            }
            
            await Task.Delay(1); // Prevent compiler warning
        }
        
        /// <summary>
        /// Refresh specific leaderboard cache
        /// </summary>
        private async Task RefreshLeaderboardCache(LeaderboardType type)
        {
            try
            {
                // Force cache refresh
                _cacheTimestamps[type] = DateTime.MinValue;
                await LoadLeaderboardData(type, GetMaxEntriesForType(type));
            }
            catch (Exception ex)
            {
                LogError($"Error refreshing cache for {type}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Refresh all leaderboards
        /// </summary>
        public async Task RefreshAllLeaderboards()
        {
            try
            {
                LogDebug("Refreshing all leaderboards...");
                
                foreach (var config in leaderboardConfigs)
                {
                    await LoadLeaderboardData(config.type, config.maxEntries);
                    await Task.Delay(100); // Rate limiting
                }
                
                LogDebug("All leaderboards refreshed");
            }
            catch (Exception ex)
            {
                LogError($"Error refreshing all leaderboards: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Time-based Utilities
        
        /// <summary>
        /// Get current week ID for weekly leaderboards
        /// </summary>
        private string GetCurrentWeekId()
        {
            var now = DateTime.UtcNow;
            var startOfYear = new DateTime(now.Year, 1, 1);
            var weekNumber = (int)Math.Ceiling((now - startOfYear).TotalDays / 7);
            return $"{now.Year}W{weekNumber:D2}";
        }
        
        /// <summary>
        /// Get current month ID for monthly leaderboards
        /// </summary>
        private string GetCurrentMonthId()
        {
            var now = DateTime.UtcNow;
            return $"{now.Year}M{now.Month:D2}";
        }
        
        /// <summary>
        /// Get current day ID for daily leaderboards
        /// </summary>
        private string GetCurrentDayId()
        {
            return DateTime.UtcNow.ToString("yyyyMMdd");
        }
        
        /// <summary>
        /// Get current season ID for seasonal leaderboards
        /// </summary>
        private string GetCurrentSeasonId()
        {
            var now = DateTime.UtcNow;
            var season = (now.Month - 1) / 3 + 1; // Quarters as seasons
            return $"{now.Year}S{season}";
        }
        
        /// <summary>
        /// Get start of current week
        /// </summary>
        private DateTime GetCurrentWeekStart()
        {
            var now = DateTime.UtcNow;
            var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            return now.AddDays(-1 * diff).Date;
        }
        
        #endregion
        
        #region Device Caching
        
        /// <summary>
        /// Load cached leaderboards from device storage
        /// </summary>
        private async Task LoadCachedLeaderboards()
        {
            try
            {
                foreach (var type in Enum.GetValues(typeof(LeaderboardType)).Cast<LeaderboardType>())
                {
                    var key = $"CachedLeaderboard_{type}";
                    if (PlayerPrefs.HasKey(key))
                    {
                        var json = PlayerPrefs.GetString(key);
                        var wrapper = JsonUtility.FromJson<LeaderboardCacheWrapper>(json);
                        
                        if (wrapper?.entries != null)
                        {
                            _cachedLeaderboards[type] = wrapper.entries;
                            _cacheTimestamps[type] = wrapper.cacheTime;
                        }
                    }
                }
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug("Cached leaderboards loaded from device");
            }
            catch (Exception ex)
            {
                LogError($"Error loading cached leaderboards: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Cache leaderboard to device storage
        /// </summary>
        private async Task CacheLeaderboardToDevice(LeaderboardType type, List<LeaderboardEntry> entries)
        {
            try
            {
                var wrapper = new LeaderboardCacheWrapper
                {
                    entries = entries,
                    cacheTime = DateTime.UtcNow
                };
                
                var json = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString($"CachedLeaderboard_{type}", json);
                PlayerPrefs.Save();
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error caching leaderboard: {ex.Message}");
            }
        }
        
        [Serializable]
        private class LeaderboardCacheWrapper
        {
            public List<LeaderboardEntry> entries;
            public DateTime cacheTime;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get leaderboard entries by type
        /// </summary>
        public List<LeaderboardEntry> GetLeaderboard(LeaderboardType type)
        {
            if (_cachedLeaderboards.ContainsKey(type))
            {
                return new List<LeaderboardEntry>(_cachedLeaderboards[type]);
            }
            
            return new List<LeaderboardEntry>();
        }
        
        /// <summary>
        /// Get performance statistics
        /// </summary>
        public string GetPerformanceStats()
        {
            var stats = $"Leaderboard Performance Stats:\n";
            stats += $"Total Read Operations: {_totalReadOperations}\n";
            stats += $"Total Write Operations: {_totalWriteOperations}\n";
            stats += $"Cached Leaderboards: {_cachedLeaderboards.Count}\n";
            stats += $"Active Tournaments: {_activeTournaments.Count}\n";
            stats += $"Pending Submissions: {_pendingSubmissions.Count}\n";
            stats += $"Last Operation Reset: {_lastOperationReset:yyyy-MM-dd HH:mm:ss}\n";
            
            return stats;
        }
        
        /// <summary>
        /// Reset daily operation counters
        /// </summary>
        public void ResetDailyCounters()
        {
            _totalReadOperations = 0;
            _totalWriteOperations = 0;
            _lastOperationReset = DateTime.UtcNow;
            LogDebug("Daily operation counters reset");
        }
        
        #endregion
        
        #region Debug and Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[LeaderboardManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[LeaderboardManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[LeaderboardManager] {message}");
        }
        
        #endregion
    }
}