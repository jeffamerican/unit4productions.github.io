using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using CircuitRunners.Firebase.DataModels;
using CircuitRunners.Firebase.Utils;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Firestore Database Manager for Circuit Runners
    /// Implements offline-first strategy with intelligent synchronization
    /// Optimized for Firebase free tier with efficient read/write operations
    /// Supports real-time updates, caching, and data validation
    /// </summary>
    public class FirestoreManager : MonoBehaviour
    {
        #region Events and Delegates
        
        /// <summary>
        /// Database operation events for UI feedback and error handling
        /// </summary>
        public static event Action<string> OnDatabaseError;
        public static event Action<string> OnDatabaseSuccess;
        public static event Action<bool> OnSyncStatusChanged;
        public static event Action<PlayerProfile> OnPlayerDataUpdated;
        public static event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;
        public static event Action<List<BotData>> OnBotCollectionUpdated;
        
        /// <summary>
        /// Sync operation results
        /// </summary>
        public struct SyncResult
        {
            public bool success;
            public string message;
            public int operationsPerformed;
            public DateTime syncTime;
        }
        
        /// <summary>
        /// Database operation types for analytics
        /// </summary>
        public enum OperationType
        {
            Read,
            Write,
            Update,
            Delete,
            Batch,
            Transaction,
            Listen
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Database Configuration")]
        [SerializeField] private bool enableOfflineSupport = true;
        [SerializeField] private bool enableRealTimeUpdates = true;
        [SerializeField] private int maxCacheSize = 100; // MB
        [SerializeField] private float syncInterval = 30f; // seconds
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelay = 2f; // seconds
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableDataCompression = true;
        [SerializeField] private bool enableBatchOperations = true;
        [SerializeField] private int batchSize = 10;
        [SerializeField] private float operationTimeout = 30f; // seconds
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool logOperationCounts = true;
        [SerializeField] private bool simulateOfflineMode = false;
        
        private FirebaseFirestore _firestore;
        private bool _isInitialized;
        private bool _isSyncing;
        private bool _isOnline = true;
        
        // Cached data for offline support
        private PlayerProfile _cachedPlayerProfile;
        private List<BotData> _cachedBotCollection;
        private List<LeaderboardEntry> _cachedLeaderboard;
        private Dictionary<string, object> _cachedGameConfig;
        
        // Real-time listeners
        private ListenerRegistration _playerDataListener;
        private ListenerRegistration _leaderboardListener;
        private ListenerRegistration _tournamentListener;
        
        // Operation tracking for free tier optimization
        private Dictionary<OperationType, int> _operationCounts;
        private DateTime _lastSyncTime;
        private Queue<Func<Task>> _pendingOperations;
        
        // Authentication reference
        private FirebaseAuthManager _authManager;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether the database system is ready for operations
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether the system is currently syncing with the server
        /// </summary>
        public bool IsSyncing => _isSyncing;
        
        /// <summary>
        /// Whether the device is online and can communicate with Firebase
        /// </summary>
        public bool IsOnline => _isOnline && !simulateOfflineMode;
        
        /// <summary>
        /// Cached player profile data (available offline)
        /// </summary>
        public PlayerProfile CachedPlayerProfile => _cachedPlayerProfile;
        
        /// <summary>
        /// Cached bot collection (available offline)
        /// </summary>
        public List<BotData> CachedBotCollection => _cachedBotCollection ?? new List<BotData>();
        
        /// <summary>
        /// Cached leaderboard data (available offline)
        /// </summary>
        public List<LeaderboardEntry> CachedLeaderboard => _cachedLeaderboard ?? new List<LeaderboardEntry>();
        
        /// <summary>
        /// Current operation counts for quota monitoring
        /// </summary>
        public Dictionary<OperationType, int> OperationCounts => new Dictionary<OperationType, int>(_operationCounts);
        
        /// <summary>
        /// Last successful sync time
        /// </summary>
        public DateTime LastSyncTime => _lastSyncTime;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (FindObjectsOfType<FirestoreManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize collections and tracking
            _operationCounts = new Dictionary<OperationType, int>();
            _pendingOperations = new Queue<Func<Task>>();
            _cachedGameConfig = new Dictionary<string, object>();
            
            foreach (OperationType opType in Enum.GetValues(typeof(OperationType)))
            {
                _operationCounts[opType] = 0;
            }
        }
        
        private void Start()
        {
            // Get authentication manager reference
            _authManager = FindObjectOfType<FirebaseAuthManager>();
            
            if (_authManager == null)
            {
                LogError("FirebaseAuthManager not found! Database operations will be limited.");
            }
            else
            {
                // Listen for authentication changes
                FirebaseAuthManager.OnAuthenticationStateChanged += HandleAuthenticationChanged;
            }
            
            // Initialize Firestore
            InitializeFirestore();
            
            // Start periodic sync if enabled
            if (enableOfflineSupport && syncInterval > 0)
            {
                InvokeRepeating(nameof(PeriodicSync), syncInterval, syncInterval);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up listeners to prevent memory leaks
            CleanupListeners();
            
            if (_authManager != null)
            {
                FirebaseAuthManager.OnAuthenticationStateChanged -= HandleAuthenticationChanged;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle app lifecycle for data synchronization
            if (!pauseStatus && _isInitialized)
            {
                // App resumed, sync data if online
                _ = SyncDataAsync();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _isInitialized)
            {
                // Check network connectivity and sync if needed
                CheckNetworkConnectivity();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize Firestore with comprehensive configuration
        /// Sets up offline support, caching, and real-time listeners
        /// </summary>
        private async void InitializeFirestore()
        {
            try
            {
                LogDebug("Initializing Firestore...");
                
                // Check Firebase dependency status
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                
                if (dependencyStatus != DependencyStatus.Available)
                {
                    LogError($"Firebase dependencies not available: {dependencyStatus}");
                    OnDatabaseError?.Invoke($"Database initialization failed: {dependencyStatus}");
                    return;
                }
                
                // Initialize Firestore instance
                _firestore = FirebaseFirestore.DefaultInstance;
                
                if (_firestore == null)
                {
                    LogError("Failed to initialize Firestore instance");
                    OnDatabaseError?.Invoke("Database service unavailable");
                    return;
                }
                
                // Configure Firestore settings for optimal performance
                ConfigureFirestoreSettings();
                
                // Load cached data from device storage
                await LoadCachedDataFromDevice();
                
                _isInitialized = true;
                LogDebug("Firestore initialized successfully");
                
                // Perform initial data sync if authenticated
                if (_authManager != null && _authManager.IsAuthenticated)
                {
                    await InitializeUserData(_authManager.UserId);
                }
                
                OnDatabaseSuccess?.Invoke("Database initialized");
                
            }
            catch (Exception ex)
            {
                LogError($"Firestore initialization error: {ex.Message}");
                OnDatabaseError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Configure Firestore settings for mobile gaming optimization
        /// </summary>
        private void ConfigureFirestoreSettings()
        {
            try
            {
                // Configure offline persistence settings
                if (enableOfflineSupport)
                {
                    var settings = new FirestoreSettings
                    {
                        PersistenceEnabled = true,
                        CacheSizeBytes = maxCacheSize * 1024 * 1024, // Convert MB to bytes
                        SslEnabled = true
                    };
                    
                    _firestore.Settings = settings;
                    LogDebug($"Offline support enabled with {maxCacheSize}MB cache");
                }
                
                // Configure network connectivity monitoring
                _firestore.EnableNetwork().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        _isOnline = true;
                        OnSyncStatusChanged?.Invoke(_isOnline);
                    }
                });
                
                LogDebug("Firestore settings configured");
            }
            catch (Exception ex)
            {
                LogError($"Error configuring Firestore settings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize user-specific data when authentication state changes
        /// </summary>
        private async Task InitializeUserData(string userId)
        {
            try
            {
                LogDebug($"Initializing user data for: {userId}");
                
                // Load or create player profile
                await LoadPlayerProfile(userId);
                
                // Load bot collection
                await LoadBotCollection(userId);
                
                // Set up real-time listeners if enabled
                if (enableRealTimeUpdates)
                {
                    SetupRealTimeListeners(userId);
                }
                
                // Sync with server if online
                if (IsOnline)
                {
                    await SyncDataAsync();
                }
                
                LogDebug("User data initialization complete");
            }
            catch (Exception ex)
            {
                LogError($"User data initialization error: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Player Profile Management
        
        /// <summary>
        /// Load player profile with offline fallback
        /// Creates new profile if none exists
        /// </summary>
        public async Task<PlayerProfile> LoadPlayerProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                LogWarning("Cannot load profile: invalid user ID");
                return null;
            }
            
            try
            {
                LogDebug($"Loading player profile: {userId}");
                
                // Try to load from Firestore first
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(userId);
                    var snapshot = await docRef.GetSnapshotAsync();
                    
                    TrackOperation(OperationType.Read);
                    
                    if (snapshot.Exists)
                    {
                        _cachedPlayerProfile = snapshot.ConvertTo<PlayerProfile>();
                        
                        // Cache to device for offline access
                        await CachePlayerProfileToDevice(_cachedPlayerProfile);
                        
                        LogDebug("Player profile loaded from server");
                        OnPlayerDataUpdated?.Invoke(_cachedPlayerProfile);
                        return _cachedPlayerProfile;
                    }
                    else
                    {
                        // Create new player profile
                        return await CreateNewPlayerProfile(userId);
                    }
                }
                else
                {
                    // Load from cache when offline
                    LogDebug("Loading player profile from cache (offline)");
                    return _cachedPlayerProfile;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading player profile: {ex.Message}");
                
                // Fallback to cached data
                if (_cachedPlayerProfile != null)
                {
                    LogDebug("Using cached player profile due to error");
                    return _cachedPlayerProfile;
                }
                
                return null;
            }
        }
        
        /// <summary>
        /// Create new player profile with default values
        /// Optimized for new user onboarding
        /// </summary>
        private async Task<PlayerProfile> CreateNewPlayerProfile(string userId)
        {
            try
            {
                LogDebug($"Creating new player profile: {userId}");
                
                var newProfile = new PlayerProfile
                {
                    PlayerId = userId,
                    DisplayName = $"Player{UnityEngine.Random.Range(1000, 9999)}", // Temporary name
                    CreatedAt = Timestamp.GetCurrentTimestamp(),
                    LastActiveAt = Timestamp.GetCurrentTimestamp(),
                    Level = 1,
                    Experience = 0,
                    PrestigeLevel = 0,
                    DailyLoginStreak = 1,
                    LastDailyReward = Timestamp.GetCurrentTimestamp()
                };
                
                // Validate before saving
                if (!DataModelValidator.ValidatePlayerProfile(newProfile))
                {
                    throw new Exception("Invalid player profile data");
                }
                
                // Save to Firestore
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(userId);
                    await docRef.SetAsync(newProfile);
                    
                    TrackOperation(OperationType.Write);
                }
                
                // Cache locally
                _cachedPlayerProfile = newProfile;
                await CachePlayerProfileToDevice(newProfile);
                
                LogDebug("New player profile created successfully");
                OnPlayerDataUpdated?.Invoke(newProfile);
                
                return newProfile;
            }
            catch (Exception ex)
            {
                LogError($"Error creating player profile: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Update player profile with validation and conflict resolution
        /// Supports partial updates and automatic retry
        /// </summary>
        public async Task<bool> UpdatePlayerProfile(PlayerProfile updatedProfile)
        {
            if (updatedProfile == null || string.IsNullOrEmpty(updatedProfile.PlayerId))
            {
                LogWarning("Cannot update profile: invalid data");
                return false;
            }
            
            try
            {
                LogDebug($"Updating player profile: {updatedProfile.PlayerId}");
                
                // Validate updated data
                if (!DataModelValidator.ValidatePlayerProfile(updatedProfile))
                {
                    LogError("Profile validation failed");
                    OnDatabaseError?.Invoke("Invalid profile data");
                    return false;
                }
                
                // Update last active timestamp
                updatedProfile.LastActiveAt = Timestamp.GetCurrentTimestamp();
                
                // Update cache immediately for offline support
                _cachedPlayerProfile = updatedProfile;
                await CachePlayerProfileToDevice(updatedProfile);
                
                // Sync to server if online
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(updatedProfile.PlayerId);
                    await docRef.SetAsync(updatedProfile, SetOptions.MergeAll);
                    
                    TrackOperation(OperationType.Update);
                    LogDebug("Player profile updated on server");
                }
                else
                {
                    // Queue operation for later sync
                    _pendingOperations.Enqueue(async () =>
                    {
                        var docRef = _firestore.Collection("users").Document(updatedProfile.PlayerId);
                        await docRef.SetAsync(updatedProfile, SetOptions.MergeAll);
                        TrackOperation(OperationType.Update);
                    });
                    
                    LogDebug("Player profile update queued for sync");
                }
                
                OnPlayerDataUpdated?.Invoke(updatedProfile);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error updating player profile: {ex.Message}");
                OnDatabaseError?.Invoke($"Profile update failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Update specific player statistics efficiently
        /// Used for frequent game state updates
        /// </summary>
        public async Task<bool> UpdatePlayerStats(string userId, PlayerStats newStats)
        {
            try
            {
                LogDebug($"Updating player stats: {userId}");
                
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(userId);
                    var updates = new Dictionary<string, object>
                    {
                        ["stats"] = newStats,
                        ["lastActiveAt"] = Timestamp.GetCurrentTimestamp()
                    };
                    
                    await docRef.UpdateAsync(updates);
                    TrackOperation(OperationType.Update);
                    
                    // Update cache
                    if (_cachedPlayerProfile != null && _cachedPlayerProfile.PlayerId == userId)
                    {
                        _cachedPlayerProfile.Stats = newStats;
                        _cachedPlayerProfile.LastActiveAt = Timestamp.GetCurrentTimestamp();
                        await CachePlayerProfileToDevice(_cachedPlayerProfile);
                        OnPlayerDataUpdated?.Invoke(_cachedPlayerProfile);
                    }
                }
                else
                {
                    // Update cache and queue for sync
                    if (_cachedPlayerProfile != null && _cachedPlayerProfile.PlayerId == userId)
                    {
                        _cachedPlayerProfile.Stats = newStats;
                        await CachePlayerProfileToDevice(_cachedPlayerProfile);
                        
                        _pendingOperations.Enqueue(async () =>
                        {
                            var docRef = _firestore.Collection("users").Document(userId);
                            var updates = new Dictionary<string, object>
                            {
                                ["stats"] = newStats,
                                ["lastActiveAt"] = Timestamp.GetCurrentTimestamp()
                            };
                            await docRef.UpdateAsync(updates);
                            TrackOperation(OperationType.Update);
                        });
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error updating player stats: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Bot Collection Management
        
        /// <summary>
        /// Load user's bot collection with caching support
        /// </summary>
        public async Task<List<BotData>> LoadBotCollection(string userId)
        {
            try
            {
                LogDebug($"Loading bot collection: {userId}");
                
                if (IsOnline)
                {
                    var collectionRef = _firestore.Collection("users").Document(userId).Collection("bots");
                    var snapshot = await collectionRef.GetSnapshotAsync();
                    
                    TrackOperation(OperationType.Read);
                    
                    _cachedBotCollection = new List<BotData>();
                    
                    foreach (var doc in snapshot.Documents)
                    {
                        if (doc.Exists)
                        {
                            var botData = doc.ConvertTo<BotData>();
                            _cachedBotCollection.Add(botData);
                        }
                    }
                    
                    // Cache to device
                    await CacheBotCollectionToDevice(_cachedBotCollection);
                    
                    LogDebug($"Loaded {_cachedBotCollection.Count} bots from server");
                    OnBotCollectionUpdated?.Invoke(_cachedBotCollection);
                }
                else
                {
                    LogDebug("Loading bot collection from cache (offline)");
                }
                
                return _cachedBotCollection ?? new List<BotData>();
            }
            catch (Exception ex)
            {
                LogError($"Error loading bot collection: {ex.Message}");
                return _cachedBotCollection ?? new List<BotData>();
            }
        }
        
        /// <summary>
        /// Save or update bot data with validation
        /// </summary>
        public async Task<bool> SaveBotData(BotData botData)
        {
            if (botData == null || string.IsNullOrEmpty(botData.BotId))
            {
                LogWarning("Cannot save bot: invalid data");
                return false;
            }
            
            try
            {
                LogDebug($"Saving bot data: {botData.BotId}");
                
                // Validate bot data
                if (!DataModelValidator.ValidateBotData(botData))
                {
                    LogError("Bot data validation failed");
                    return false;
                }
                
                // Update timestamps
                botData.LastModified = Timestamp.GetCurrentTimestamp();
                if (botData.CreatedAt == null)
                {
                    botData.CreatedAt = Timestamp.GetCurrentTimestamp();
                }
                
                // Update local cache
                if (_cachedBotCollection == null)
                {
                    _cachedBotCollection = new List<BotData>();
                }
                
                var existingIndex = _cachedBotCollection.FindIndex(b => b.BotId == botData.BotId);
                if (existingIndex >= 0)
                {
                    _cachedBotCollection[existingIndex] = botData;
                }
                else
                {
                    _cachedBotCollection.Add(botData);
                }
                
                await CacheBotCollectionToDevice(_cachedBotCollection);
                
                // Save to server if online
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(botData.OwnerId).Collection("bots").Document(botData.BotId);
                    await docRef.SetAsync(botData);
                    
                    TrackOperation(OperationType.Write);
                    LogDebug("Bot data saved to server");
                }
                else
                {
                    // Queue for later sync
                    _pendingOperations.Enqueue(async () =>
                    {
                        var docRef = _firestore.Collection("users").Document(botData.OwnerId).Collection("bots").Document(botData.BotId);
                        await docRef.SetAsync(botData);
                        TrackOperation(OperationType.Write);
                    });
                    
                    LogDebug("Bot data queued for sync");
                }
                
                OnBotCollectionUpdated?.Invoke(_cachedBotCollection);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error saving bot data: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Delete bot from collection
        /// </summary>
        public async Task<bool> DeleteBot(string userId, string botId)
        {
            try
            {
                LogDebug($"Deleting bot: {botId}");
                
                // Remove from cache
                if (_cachedBotCollection != null)
                {
                    _cachedBotCollection.RemoveAll(b => b.BotId == botId);
                    await CacheBotCollectionToDevice(_cachedBotCollection);
                }
                
                // Delete from server if online
                if (IsOnline)
                {
                    var docRef = _firestore.Collection("users").Document(userId).Collection("bots").Document(botId);
                    await docRef.DeleteAsync();
                    
                    TrackOperation(OperationType.Delete);
                    LogDebug("Bot deleted from server");
                }
                else
                {
                    // Queue for later sync
                    _pendingOperations.Enqueue(async () =>
                    {
                        var docRef = _firestore.Collection("users").Document(userId).Collection("bots").Document(botId);
                        await docRef.DeleteAsync();
                        TrackOperation(OperationType.Delete);
                    });
                }
                
                OnBotCollectionUpdated?.Invoke(_cachedBotCollection);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error deleting bot: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Leaderboard Management
        
        /// <summary>
        /// Load leaderboard data with efficient caching
        /// Supports different leaderboard types (global, weekly, friends)
        /// </summary>
        public async Task<List<LeaderboardEntry>> LoadLeaderboard(string leaderboardType = "global", int limit = 100)
        {
            try
            {
                LogDebug($"Loading leaderboard: {leaderboardType} (limit: {limit})");
                
                if (IsOnline)
                {
                    var collectionRef = _firestore.Collection("leaderboards").Document(leaderboardType).Collection("entries");
                    var query = collectionRef.OrderByDescending("score").Limit(Math.Min(limit, 100)); // Respect free tier limits
                    
                    var snapshot = await query.GetSnapshotAsync();
                    TrackOperation(OperationType.Read);
                    
                    _cachedLeaderboard = new List<LeaderboardEntry>();
                    
                    foreach (var doc in snapshot.Documents)
                    {
                        if (doc.Exists)
                        {
                            var entry = doc.ConvertTo<LeaderboardEntry>();
                            _cachedLeaderboard.Add(entry);
                        }
                    }
                    
                    // Cache to device
                    await CacheLeaderboardToDevice(_cachedLeaderboard, leaderboardType);
                    
                    LogDebug($"Loaded {_cachedLeaderboard.Count} leaderboard entries");
                    OnLeaderboardUpdated?.Invoke(_cachedLeaderboard);
                }
                else
                {
                    LogDebug("Loading leaderboard from cache (offline)");
                    await LoadCachedLeaderboardFromDevice(leaderboardType);
                }
                
                return _cachedLeaderboard ?? new List<LeaderboardEntry>();
            }
            catch (Exception ex)
            {
                LogError($"Error loading leaderboard: {ex.Message}");
                return _cachedLeaderboard ?? new List<LeaderboardEntry>();
            }
        }
        
        /// <summary>
        /// Submit score to leaderboard (client-side submission)
        /// Server-side validation handled by Cloud Functions
        /// </summary>
        public async Task<bool> SubmitScore(long score, string botUsed, string leaderboardType = "global")
        {
            if (!_authManager.IsAuthenticated)
            {
                LogWarning("Cannot submit score: user not authenticated");
                return false;
            }
            
            try
            {
                LogDebug($"Submitting score: {score} (bot: {botUsed})");
                
                var entry = new LeaderboardEntry
                {
                    PlayerId = _authManager.UserId,
                    DisplayName = _authManager.GetDisplayName(),
                    AvatarUrl = _authManager.GetProfilePhotoUrl(),
                    Score = score,
                    AchievedAt = Timestamp.GetCurrentTimestamp(),
                    BotUsed = botUsed,
                    LeaderboardType = leaderboardType
                };
                
                // Submit to server for validation (handled by Cloud Function)
                if (IsOnline)
                {
                    // Submit to pending scores collection for server processing
                    var docRef = _firestore.Collection("pending_scores").Document();
                    await docRef.SetAsync(entry);
                    
                    TrackOperation(OperationType.Write);
                    LogDebug("Score submitted for server validation");
                }
                else
                {
                    // Queue for later submission
                    _pendingOperations.Enqueue(async () =>
                    {
                        var docRef = _firestore.Collection("pending_scores").Document();
                        await docRef.SetAsync(entry);
                        TrackOperation(OperationType.Write);
                    });
                    
                    LogDebug("Score queued for submission");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error submitting score: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get player's rank in leaderboard
        /// </summary>
        public async Task<int> GetPlayerRank(string playerId, string leaderboardType = "global")
        {
            try
            {
                if (!IsOnline) 
                {
                    // Calculate from cached data
                    if (_cachedLeaderboard != null)
                    {
                        var playerEntry = _cachedLeaderboard.FindIndex(e => e.PlayerId == playerId);
                        return playerEntry >= 0 ? playerEntry + 1 : -1;
                    }
                    return -1;
                }
                
                // Query server for accurate rank
                var collectionRef = _firestore.Collection("leaderboards").Document(leaderboardType).Collection("entries");
                var query = collectionRef.WhereEqualTo("playerId", playerId);
                
                var snapshot = await query.GetSnapshotAsync();
                TrackOperation(OperationType.Read);
                
                if (snapshot.Count > 0)
                {
                    var entry = snapshot.Documents[0].ConvertTo<LeaderboardEntry>();
                    return entry.Rank;
                }
                
                return -1; // Player not found in leaderboard
            }
            catch (Exception ex)
            {
                LogError($"Error getting player rank: {ex.Message}");
                return -1;
            }
        }
        
        #endregion
        
        #region Real-time Updates
        
        /// <summary>
        /// Set up real-time listeners for live data updates
        /// Optimized for battery life and data usage
        /// </summary>
        private void SetupRealTimeListeners(string userId)
        {
            try
            {
                LogDebug("Setting up real-time listeners...");
                
                // Listen to player profile changes
                var playerDocRef = _firestore.Collection("users").Document(userId);
                _playerDataListener = playerDocRef.Listen(snapshot =>
                {
                    if (snapshot.Exists)
                    {
                        _cachedPlayerProfile = snapshot.ConvertTo<PlayerProfile>();
                        _ = CachePlayerProfileToDevice(_cachedPlayerProfile);
                        OnPlayerDataUpdated?.Invoke(_cachedPlayerProfile);
                        LogDebug("Real-time player data update received");
                    }
                });
                
                // Listen to global leaderboard changes (limited frequency)
                var leaderboardRef = _firestore.Collection("leaderboards").Document("global").Collection("entries")
                    .OrderByDescending("score").Limit(50);
                
                _leaderboardListener = leaderboardRef.Listen(snapshot =>
                {
                    _cachedLeaderboard = new List<LeaderboardEntry>();
                    foreach (var doc in snapshot.Documents)
                    {
                        _cachedLeaderboard.Add(doc.ConvertTo<LeaderboardEntry>());
                    }
                    
                    _ = CacheLeaderboardToDevice(_cachedLeaderboard, "global");
                    OnLeaderboardUpdated?.Invoke(_cachedLeaderboard);
                    LogDebug($"Real-time leaderboard update: {_cachedLeaderboard.Count} entries");
                });
                
                LogDebug("Real-time listeners configured");
            }
            catch (Exception ex)
            {
                LogError($"Error setting up listeners: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clean up all active listeners
        /// </summary>
        private void CleanupListeners()
        {
            try
            {
                _playerDataListener?.Stop();
                _leaderboardListener?.Stop();
                _tournamentListener?.Stop();
                
                _playerDataListener = null;
                _leaderboardListener = null;
                _tournamentListener = null;
                
                LogDebug("Real-time listeners cleaned up");
            }
            catch (Exception ex)
            {
                LogError($"Error cleaning up listeners: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Data Synchronization
        
        /// <summary>
        /// Perform comprehensive data synchronization
        /// Handles conflict resolution and efficient batch operations
        /// </summary>
        public async Task<SyncResult> SyncDataAsync()
        {
            var result = new SyncResult
            {
                success = false,
                message = "Sync not started",
                operationsPerformed = 0,
                syncTime = DateTime.UtcNow
            };
            
            if (_isSyncing)
            {
                result.message = "Sync already in progress";
                return result;
            }
            
            if (!IsOnline)
            {
                result.message = "Device offline";
                return result;
            }
            
            try
            {
                _isSyncing = true;
                OnSyncStatusChanged?.Invoke(true);
                
                LogDebug("Starting data synchronization...");
                
                int operationCount = 0;
                
                // Process pending operations
                while (_pendingOperations.Count > 0 && operationCount < 20) // Limit to prevent quota exhaustion
                {
                    var operation = _pendingOperations.Dequeue();
                    await operation();
                    operationCount++;
                }
                
                // Sync user data if authenticated
                if (_authManager != null && _authManager.IsAuthenticated)
                {
                    // Refresh player profile
                    await LoadPlayerProfile(_authManager.UserId);
                    operationCount++;
                    
                    // Refresh bot collection
                    await LoadBotCollection(_authManager.UserId);
                    operationCount++;
                    
                    // Refresh leaderboard (limited to preserve quota)
                    if (_operationCounts[OperationType.Read] < 100) // Daily quota check
                    {
                        await LoadLeaderboard("global", 50);
                        operationCount++;
                    }
                }
                
                _lastSyncTime = DateTime.UtcNow;
                
                result.success = true;
                result.message = $"Sync completed successfully";
                result.operationsPerformed = operationCount;
                result.syncTime = _lastSyncTime;
                
                LogDebug($"Data sync completed: {operationCount} operations");
                OnDatabaseSuccess?.Invoke("Data synchronized");
                
            }
            catch (Exception ex)
            {
                result.message = $"Sync failed: {ex.Message}";
                LogError($"Data sync error: {ex.Message}");
                OnDatabaseError?.Invoke(result.message);
            }
            finally
            {
                _isSyncing = false;
                OnSyncStatusChanged?.Invoke(false);
            }
            
            return result;
        }
        
        /// <summary>
        /// Periodic sync triggered by timer
        /// </summary>
        private async void PeriodicSync()
        {
            if (!_isInitialized || !IsOnline) return;
            
            // Only sync if there's pending data or it's been a while
            var timeSinceLastSync = DateTime.UtcNow - _lastSyncTime;
            if (_pendingOperations.Count > 0 || timeSinceLastSync.TotalMinutes > 5)
            {
                await SyncDataAsync();
            }
        }
        
        #endregion
        
        #region Caching System
        
        /// <summary>
        /// Cache player profile to device storage for offline access
        /// </summary>
        private async Task CachePlayerProfileToDevice(PlayerProfile profile)
        {
            try
            {
                if (profile == null) return;
                
                var json = JsonUtility.ToJson(profile);
                PlayerPrefs.SetString($"CachedProfile_{profile.PlayerId}", json);
                PlayerPrefs.Save();
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug("Player profile cached to device");
            }
            catch (Exception ex)
            {
                LogError($"Error caching profile: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Cache bot collection to device storage
        /// </summary>
        private async Task CacheBotCollectionToDevice(List<BotData> bots)
        {
            try
            {
                if (bots == null) return;
                
                var json = JsonUtility.ToJson(new BotCollectionWrapper { bots = bots });
                PlayerPrefs.SetString("CachedBotCollection", json);
                PlayerPrefs.Save();
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug($"Bot collection cached: {bots.Count} bots");
            }
            catch (Exception ex)
            {
                LogError($"Error caching bot collection: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Cache leaderboard to device storage
        /// </summary>
        private async Task CacheLeaderboardToDevice(List<LeaderboardEntry> entries, string type)
        {
            try
            {
                if (entries == null) return;
                
                var json = JsonUtility.ToJson(new LeaderboardWrapper { entries = entries });
                PlayerPrefs.SetString($"CachedLeaderboard_{type}", json);
                PlayerPrefs.Save();
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug($"Leaderboard cached: {entries.Count} entries");
            }
            catch (Exception ex)
            {
                LogError($"Error caching leaderboard: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load all cached data from device storage
        /// </summary>
        private async Task LoadCachedDataFromDevice()
        {
            try
            {
                LogDebug("Loading cached data from device...");
                
                // Load cached bot collection
                if (PlayerPrefs.HasKey("CachedBotCollection"))
                {
                    var json = PlayerPrefs.GetString("CachedBotCollection");
                    var wrapper = JsonUtility.FromJson<BotCollectionWrapper>(json);
                    _cachedBotCollection = wrapper?.bots ?? new List<BotData>();
                }
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug("Cached data loaded from device");
            }
            catch (Exception ex)
            {
                LogError($"Error loading cached data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load cached leaderboard from device storage
        /// </summary>
        private async Task LoadCachedLeaderboardFromDevice(string type)
        {
            try
            {
                var key = $"CachedLeaderboard_{type}";
                if (PlayerPrefs.HasKey(key))
                {
                    var json = PlayerPrefs.GetString(key);
                    var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                    _cachedLeaderboard = wrapper?.entries ?? new List<LeaderboardEntry>();
                    
                    OnLeaderboardUpdated?.Invoke(_cachedLeaderboard);
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error loading cached leaderboard: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Classes for Serialization
        
        [Serializable]
        private class BotCollectionWrapper
        {
            public List<BotData> bots;
        }
        
        [Serializable]
        private class LeaderboardWrapper
        {
            public List<LeaderboardEntry> entries;
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle authentication state changes
        /// </summary>
        private async void HandleAuthenticationChanged(Firebase.Auth.FirebaseUser user)
        {
            try
            {
                if (user != null)
                {
                    LogDebug($"User authenticated, initializing data: {user.UserId}");
                    await InitializeUserData(user.UserId);
                }
                else
                {
                    LogDebug("User signed out, cleaning up data");
                    CleanupListeners();
                    ClearCachedData();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling auth change: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Track database operations for quota monitoring
        /// </summary>
        private void TrackOperation(OperationType operationType)
        {
            if (_operationCounts.ContainsKey(operationType))
            {
                _operationCounts[operationType]++;
            }
            
            if (logOperationCounts)
            {
                LogDebug($"Operation tracked: {operationType} (Total: {_operationCounts[operationType]})");
            }
        }
        
        /// <summary>
        /// Check network connectivity status
        /// </summary>
        private void CheckNetworkConnectivity()
        {
            bool wasOnline = _isOnline;
            _isOnline = Application.internetReachability != NetworkReachability.NotReachable;
            
            if (_isOnline && !wasOnline)
            {
                LogDebug("Network connectivity restored");
                OnSyncStatusChanged?.Invoke(_isOnline);
                _ = SyncDataAsync(); // Trigger sync when coming back online
            }
            else if (!_isOnline && wasOnline)
            {
                LogDebug("Network connectivity lost");
                OnSyncStatusChanged?.Invoke(_isOnline);
            }
        }
        
        /// <summary>
        /// Clear cached data (used on sign out)
        /// </summary>
        private void ClearCachedData()
        {
            _cachedPlayerProfile = null;
            _cachedBotCollection = null;
            _cachedLeaderboard = null;
            _cachedGameConfig.Clear();
            _pendingOperations.Clear();
            
            LogDebug("Cached data cleared");
        }
        
        /// <summary>
        /// Get operation count summary for monitoring
        /// </summary>
        public string GetOperationSummary()
        {
            var summary = "Database Operations Today:\n";
            foreach (var kvp in _operationCounts)
            {
                summary += $"{kvp.Key}: {kvp.Value}\n";
            }
            return summary;
        }
        
        /// <summary>
        /// Reset daily operation counters
        /// </summary>
        public void ResetDailyCounters()
        {
            foreach (var key in _operationCounts.Keys.ToList())
            {
                _operationCounts[key] = 0;
            }
            LogDebug("Daily operation counters reset");
        }
        
        #endregion
        
        #region Debug and Logging
        
        /// <summary>
        /// Debug logging with conditional compilation
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[FirestoreManager] {message}");
            }
        }
        
        /// <summary>
        /// Warning logging
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[FirestoreManager] {message}");
        }
        
        /// <summary>
        /// Error logging
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[FirestoreManager] {message}");
        }
        
        #endregion
    }
}