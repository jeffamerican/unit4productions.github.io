using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Extensions;
using CircuitRunners.Firebase.DataModels;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Analytics and Event Tracking Manager for Circuit Runners
    /// Handles Firebase Analytics, custom events, monetization tracking, and player behavior analysis
    /// Optimized for free-to-play mobile game insights and data-driven optimization
    /// GDPR compliant with privacy-first approach and user consent management
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        #region Events and Delegates
        
        /// <summary>
        /// Analytics events for debugging and monitoring
        /// </summary>
        public static event Action<string, Dictionary<string, object>> OnEventTracked;
        public static event Action<string> OnAnalyticsError;
        public static event Action<bool> OnAnalyticsConsentChanged;
        public static event Action<AnalyticsReport> OnWeeklyReportGenerated;
        
        /// <summary>
        /// Weekly analytics report structure
        /// </summary>
        public struct AnalyticsReport
        {
            public DateTime reportDate;
            public int totalEvents;
            public int uniqueUsers;
            public float averageSessionLength;
            public int totalSessions;
            public Dictionary<string, int> topEvents;
            public MonetizationMetrics monetization;
            public RetentionMetrics retention;
        }
        
        /// <summary>
        /// Monetization tracking metrics
        /// </summary>
        public struct MonetizationMetrics
        {
            public float totalRevenue;
            public int totalPurchases;
            public float averageRevenuePerUser;
            public float averageRevenuePerPayer;
            public int totalAdViews;
            public float adRevenue;
            public Dictionary<string, int> topPurchasedItems;
        }
        
        /// <summary>
        /// Player retention metrics
        /// </summary>
        public struct RetentionMetrics
        {
            public float day1Retention;
            public float day3Retention;
            public float day7Retention;
            public float day30Retention;
            public int newUsers;
            public int returningUsers;
            public float averageLifetime;
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Analytics Configuration")]
        [SerializeField] private bool enableFirebaseAnalytics = true;
        [SerializeField] private bool enableCustomEvents = true;
        [SerializeField] private bool enableAutomaticEvents = true;
        [SerializeField] private bool enableCrashReporting = true;
        [SerializeField] private bool requireUserConsent = true; // GDPR compliance
        
        [Header("Event Batching")]
        [SerializeField] private bool enableEventBatching = true;
        [SerializeField] private int batchSize = 20;
        [SerializeField] private float batchFlushInterval = 30f; // seconds
        [SerializeField] private int maxQueueSize = 100;
        
        [Header("Session Tracking")]
        [SerializeField] private bool enableSessionTracking = true;
        [SerializeField] private float sessionTimeoutMinutes = 30f;
        [SerializeField] private bool trackScreenViews = true;
        [SerializeField] private bool trackUserProperties = true;
        
        [Header("Monetization Analytics")]
        [SerializeField] private bool enablePurchaseTracking = true;
        [SerializeField] private bool enableAdTracking = true;
        [SerializeField] private bool trackPlayerSegmentation = true;
        [SerializeField] private bool enableLTVCalculation = true;
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableDataCompression = true;
        [SerializeField] private int maxEventsPerSession = 500;
        [SerializeField] private float eventThrottleInterval = 0.1f; // seconds
        [SerializeField] private bool enableOfflineQueuing = true;
        
        [Header("Privacy and Compliance")]
        [SerializeField] private bool anonymizeUserData = true;
        [SerializeField] private bool respectOptOutSettings = true;
        [SerializeField] private int dataRetentionDays = 60;
        [SerializeField] private bool enableDataExport = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool enableVerboseLogging = false;
        [SerializeField] private bool logAllEvents = false;
        [SerializeField] private bool simulateOfflineMode = false;
        
        // Core system components
        private FirebaseAuthManager _authManager;
        private FirestoreManager _firestoreManager;
        
        // Analytics state
        private bool _isInitialized;
        private bool _hasUserConsent;
        private bool _isSessionActive;
        private string _currentSessionId;
        private DateTime _sessionStartTime;
        private DateTime _lastEventTime;
        
        // Event queuing and batching
        private Queue<AnalyticsEventData> _eventQueue;
        private List<AnalyticsEventData> _currentBatch;
        private float _lastBatchFlush;
        private int _eventsThisSession;
        
        // User tracking
        private string _userId;
        private Dictionary<string, object> _userProperties;
        private Dictionary<string, int> _eventCounts;
        private List<string> _screenViews;
        
        // Monetization tracking
        private Dictionary<string, float> _purchaseData;
        private Dictionary<string, int> _adImpressions;
        private float _sessionRevenue;
        
        // Performance metrics
        private int _totalEventsLogged;
        private int _totalBatchesSent;
        private DateTime _analyticsStartTime;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether analytics system is initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether user has consented to analytics tracking
        /// </summary>
        public bool HasUserConsent => _hasUserConsent;
        
        /// <summary>
        /// Current analytics session ID
        /// </summary>
        public string CurrentSessionId => _currentSessionId;
        
        /// <summary>
        /// Whether a session is currently active
        /// </summary>
        public bool IsSessionActive => _isSessionActive;
        
        /// <summary>
        /// Current session duration
        /// </summary>
        public TimeSpan CurrentSessionDuration => 
            _isSessionActive ? DateTime.UtcNow - _sessionStartTime : TimeSpan.Zero;
        
        /// <summary>
        /// Total events tracked this session
        /// </summary>
        public int EventsThisSession => _eventsThisSession;
        
        /// <summary>
        /// Total events logged since initialization
        /// </summary>
        public int TotalEventsLogged => _totalEventsLogged;
        
        /// <summary>
        /// Current user properties
        /// </summary>
        public Dictionary<string, object> UserProperties => 
            new Dictionary<string, object>(_userProperties);
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (FindObjectsOfType<AnalyticsManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize data structures
            _eventQueue = new Queue<AnalyticsEventData>();
            _currentBatch = new List<AnalyticsEventData>();
            _userProperties = new Dictionary<string, object>();
            _eventCounts = new Dictionary<string, int>();
            _screenViews = new List<string>();
            _purchaseData = new Dictionary<string, float>();
            _adImpressions = new Dictionary<string, int>();
            
            _analyticsStartTime = DateTime.UtcNow;
            _currentSessionId = Guid.NewGuid().ToString();
        }
        
        private void Start()
        {
            // Get system references
            _authManager = FindObjectOfType<FirebaseAuthManager>();
            _firestoreManager = FindObjectOfType<FirestoreManager>();
            
            // Initialize analytics system
            InitializeAnalytics();
            
            // Set up authentication event handlers
            if (_authManager != null)
            {
                FirebaseAuthManager.OnAuthenticationStateChanged += HandleAuthenticationChanged;
            }
        }
        
        private void Update()
        {
            // Process event queue
            if (_isInitialized && HasUserConsent)
            {
                ProcessEventQueue();
                CheckBatchFlushTimer();
                CheckSessionTimeout();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event handlers
            if (_authManager != null)
            {
                FirebaseAuthManager.OnAuthenticationStateChanged -= HandleAuthenticationChanged;
            }
            
            // Flush any remaining events
            if (_isInitialized && HasUserConsent)
            {
                FlushEventBatch(true);
                EndSession();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isInitialized && HasUserConsent)
            {
                if (pauseStatus)
                {
                    // App paused - end session and flush events
                    TrackEvent("app_paused", new Dictionary<string, object>
                    {
                        ["session_duration"] = CurrentSessionDuration.TotalSeconds
                    });
                    
                    FlushEventBatch(true);
                    EndSession();
                }
                else
                {
                    // App resumed - start new session
                    StartSession();
                    TrackEvent("app_resumed");
                }
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isInitialized && HasUserConsent)
            {
                if (hasFocus)
                {
                    TrackEvent("app_focus_gained");
                }
                else
                {
                    TrackEvent("app_focus_lost");
                }
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize Firebase Analytics and custom tracking systems
        /// Sets up user consent, session management, and event processing
        /// </summary>
        private async void InitializeAnalytics()
        {
            try
            {
                LogDebug("Initializing Analytics Manager...");
                
                // Check user consent first (GDPR compliance)
                await CheckUserConsent();
                
                if (!HasUserConsent && requireUserConsent)
                {
                    LogDebug("User consent required but not granted. Analytics disabled.");
                    return;
                }
                
                // Initialize Firebase Analytics
                if (enableFirebaseAnalytics)
                {
                    await InitializeFirebaseAnalytics();
                }
                
                // Set up automatic event tracking
                if (enableAutomaticEvents)
                {
                    SetupAutomaticEventTracking();
                }
                
                // Configure user properties
                if (trackUserProperties)
                {
                    SetupInitialUserProperties();
                }
                
                _isInitialized = true;
                LogDebug("Analytics Manager initialized successfully");
                
                // Start initial session
                StartSession();
                
                // Track initialization event
                TrackEvent("analytics_initialized", new Dictionary<string, object>
                {
                    ["initialization_time"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["consent_granted"] = HasUserConsent,
                    ["firebase_enabled"] = enableFirebaseAnalytics
                });
                
            }
            catch (Exception ex)
            {
                LogError($"Analytics initialization failed: {ex.Message}");
                OnAnalyticsError?.Invoke($"Analytics initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize Firebase Analytics with proper configuration
        /// </summary>
        private async Task InitializeFirebaseAnalytics()
        {
            try
            {
                LogDebug("Initializing Firebase Analytics...");
                
                // Set analytics collection enabled
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(HasUserConsent);
                
                // Configure analytics settings
                if (anonymizeUserData)
                {
                    // Enable IP anonymization for privacy
                    await FirebaseAnalytics.SetUserPropertyAsync("allow_ad_personalization_signals", "false");
                }
                
                // Set data retention settings
                // Note: This is configured in Firebase Console, not SDK
                
                LogDebug("Firebase Analytics initialized");
            }
            catch (Exception ex)
            {
                LogError($"Firebase Analytics initialization error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check and request user consent for analytics (GDPR compliance)
        /// </summary>
        private async Task CheckUserConsent()
        {
            try
            {
                // Check if consent was previously granted
                _hasUserConsent = PlayerPrefs.GetInt("AnalyticsConsent", 0) == 1;
                
                if (!_hasUserConsent && requireUserConsent)
                {
                    // In a real implementation, this would show a consent dialog
                    // For now, we'll assume consent is granted for development
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _hasUserConsent = true;
                    PlayerPrefs.SetInt("AnalyticsConsent", 1);
                    PlayerPrefs.Save();
                    #endif
                }
                
                LogDebug($"User consent status: {_hasUserConsent}");
                OnAnalyticsConsentChanged?.Invoke(_hasUserConsent);
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error checking user consent: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set up automatic event tracking for common game events
        /// </summary>
        private void SetupAutomaticEventTracking()
        {
            try
            {
                LogDebug("Setting up automatic event tracking...");
                
                // Application lifecycle events are handled in Unity lifecycle methods
                
                // Set up screen view tracking (if enabled)
                if (trackScreenViews)
                {
                    // Screen view tracking would be integrated with your UI system
                    LogDebug("Screen view tracking enabled");
                }
                
                LogDebug("Automatic event tracking configured");
            }
            catch (Exception ex)
            {
                LogError($"Error setting up automatic tracking: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set up initial user properties
        /// </summary>
        private void SetupInitialUserProperties()
        {
            try
            {
                // Device and platform information
                SetUserProperty("platform", Application.platform.ToString());
                SetUserProperty("device_model", SystemInfo.deviceModel);
                SetUserProperty("operating_system", SystemInfo.operatingSystem);
                SetUserProperty("app_version", Application.version);
                SetUserProperty("unity_version", Application.unityVersion);
                
                // Game-specific properties
                SetUserProperty("first_session", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                SetUserProperty("installation_source", "unknown"); // Could be set during onboarding
                
                LogDebug("Initial user properties configured");
            }
            catch (Exception ex)
            {
                LogError($"Error setting up user properties: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Session Management
        
        /// <summary>
        /// Start a new analytics session
        /// Tracks session start and initializes session-specific metrics
        /// </summary>
        public void StartSession()
        {
            if (!_isInitialized || !HasUserConsent) return;
            
            try
            {
                _isSessionActive = true;
                _currentSessionId = Guid.NewGuid().ToString();
                _sessionStartTime = DateTime.UtcNow;
                _lastEventTime = DateTime.UtcNow;
                _eventsThisSession = 0;
                _sessionRevenue = 0f;
                _screenViews.Clear();
                
                LogDebug($"Session started: {_currentSessionId}");
                
                // Track session start event
                var sessionData = new Dictionary<string, object>
                {
                    ["session_id"] = _currentSessionId,
                    ["session_start_time"] = _sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["user_authenticated"] = _authManager?.IsAuthenticated ?? false
                };
                
                // Add user-specific data if authenticated
                if (_authManager?.IsAuthenticated == true)
                {
                    sessionData["user_id"] = _authManager.UserId;
                    sessionData["user_anonymous"] = _authManager.IsAnonymous;
                    sessionData["auth_provider"] = _authManager.CurrentProvider.ToString();
                }
                
                TrackEvent("session_start", sessionData);
                
                // Track with Firebase Analytics
                if (enableFirebaseAnalytics)
                {
                    FirebaseAnalytics.LogEvent("session_start", new Parameter[]
                    {
                        new Parameter("session_id", _currentSessionId)
                    });
                }
            }
            catch (Exception ex)
            {
                LogError($"Error starting session: {ex.Message}");
            }
        }
        
        /// <summary>
        /// End the current analytics session
        /// Calculates session metrics and tracks session completion
        /// </summary>
        public void EndSession()
        {
            if (!_isSessionActive || !_isInitialized || !HasUserConsent) return;
            
            try
            {
                var sessionDuration = DateTime.UtcNow - _sessionStartTime;
                
                LogDebug($"Session ended: {_currentSessionId} (Duration: {sessionDuration.TotalSeconds:F1}s)");
                
                // Track session end event with comprehensive metrics
                var sessionEndData = new Dictionary<string, object>
                {
                    ["session_id"] = _currentSessionId,
                    ["session_duration"] = sessionDuration.TotalSeconds,
                    ["events_tracked"] = _eventsThisSession,
                    ["screens_viewed"] = _screenViews.Count,
                    ["unique_screens"] = _screenViews.Distinct().Count(),
                    ["session_revenue"] = _sessionRevenue,
                    ["session_end_time"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                TrackEvent("session_end", sessionEndData);
                
                // Track with Firebase Analytics
                if (enableFirebaseAnalytics)
                {
                    FirebaseAnalytics.LogEvent("session_end", new Parameter[]
                    {
                        new Parameter("session_id", _currentSessionId),
                        new Parameter("session_duration", sessionDuration.TotalSeconds),
                        new Parameter("events_tracked", _eventsThisSession)
                    });
                }
                
                // Reset session state
                _isSessionActive = false;
                _currentSessionId = string.Empty;
            }
            catch (Exception ex)
            {
                LogError($"Error ending session: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check if session has timed out due to inactivity
        /// </summary>
        private void CheckSessionTimeout()
        {
            if (!_isSessionActive) return;
            
            var timeSinceLastEvent = DateTime.UtcNow - _lastEventTime;
            var timeoutMinutes = sessionTimeoutMinutes;
            
            if (timeSinceLastEvent.TotalMinutes > timeoutMinutes)
            {
                LogDebug($"Session timeout after {timeSinceLastEvent.TotalMinutes:F1} minutes");
                EndSession();
                StartSession(); // Start new session
            }
        }
        
        #endregion
        
        #region Event Tracking
        
        /// <summary>
        /// Track a custom analytics event with parameters
        /// Main entry point for all event tracking
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized || !HasUserConsent || string.IsNullOrEmpty(eventName))
                return;
            
            try
            {
                // Throttle events to prevent spam
                if (!CanTrackEvent())
                {
                    LogWarning($"Event tracking throttled: {eventName}");
                    return;
                }
                
                // Create event data structure
                var eventData = new AnalyticsEventData(eventName)
                {
                    playerId = _authManager?.IsAuthenticated == true ? _authManager.UserId : "anonymous",
                    sessionId = _currentSessionId
                };
                
                // Add custom parameters
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        eventData.parameters[param.Key] = param.Value;
                    }
                }
                
                // Add standard parameters
                AddStandardParameters(eventData);
                
                // Update tracking counters
                _eventsThisSession++;
                _totalEventsLogged++;
                _lastEventTime = DateTime.UtcNow;
                
                if (_eventCounts.ContainsKey(eventName))
                    _eventCounts[eventName]++;
                else
                    _eventCounts[eventName] = 1;
                
                // Add to processing queue
                if (enableEventBatching)
                {
                    _eventQueue.Enqueue(eventData);
                }
                else
                {
                    // Send immediately
                    SendEventToAnalytics(eventData);
                }
                
                // Debug logging
                if (logAllEvents)
                {
                    LogDebug($"Event tracked: {eventName} ({eventData.parameters.Count} parameters)");
                }
                
                // Notify listeners
                OnEventTracked?.Invoke(eventName, eventData.parameters);
                
            }
            catch (Exception ex)
            {
                LogError($"Error tracking event '{eventName}': {ex.Message}");
                OnAnalyticsError?.Invoke($"Event tracking failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Track gameplay-specific events with standardized parameters
        /// </summary>
        public void TrackGameplayEvent(string action, int score = 0, string botUsed = null, Dictionary<string, object> additionalParams = null)
        {
            var parameters = new Dictionary<string, object>
            {
                ["action"] = action,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (score > 0) parameters["score"] = score;
            if (!string.IsNullOrEmpty(botUsed)) parameters["bot_used"] = botUsed;
            
            // Add additional parameters if provided
            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    parameters[param.Key] = param.Value;
                }
            }
            
            TrackEvent("gameplay_action", parameters);
        }
        
        /// <summary>
        /// Track player progression events (level ups, achievements, etc.)
        /// </summary>
        public void TrackProgressionEvent(string milestone, int currentLevel, long currentExperience, Dictionary<string, object> additionalParams = null)
        {
            var parameters = new Dictionary<string, object>
            {
                ["milestone"] = milestone,
                ["level"] = currentLevel,
                ["experience"] = currentExperience,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    parameters[param.Key] = param.Value;
                }
            }
            
            TrackEvent("player_progression", parameters);
            
            // Also track with Firebase Analytics for standard events
            if (enableFirebaseAnalytics)
            {
                if (milestone == "level_up")
                {
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp, new Parameter[]
                    {
                        new Parameter(FirebaseAnalytics.ParameterLevel, currentLevel),
                        new Parameter(FirebaseAnalytics.ParameterCharacter, _authManager?.UserId ?? "anonymous")
                    });
                }
            }
        }
        
        /// <summary>
        /// Track monetization events (purchases, ad views, etc.)
        /// Critical for F2P optimization and revenue analysis
        /// </summary>
        public void TrackMonetizationEvent(string action, string itemId, float value = 0f, string currency = "USD", Dictionary<string, object> additionalParams = null)
        {
            var parameters = new Dictionary<string, object>
            {
                ["action"] = action,
                ["item_id"] = itemId,
                ["currency"] = currency,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (value > 0)
            {
                parameters["value"] = value;
                _sessionRevenue += value;
                
                // Track purchase data for LTV calculation
                if (_purchaseData.ContainsKey(itemId))
                    _purchaseData[itemId] += value;
                else
                    _purchaseData[itemId] = value;
            }
            
            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    parameters[param.Key] = param.Value;
                }
            }
            
            TrackEvent("monetization_action", parameters);
            
            // Track with Firebase Analytics for standard e-commerce events
            if (enableFirebaseAnalytics && enablePurchaseTracking)
            {
                if (action == "purchase_completed" && value > 0)
                {
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter[]
                    {
                        new Parameter(FirebaseAnalytics.ParameterCurrency, currency),
                        new Parameter(FirebaseAnalytics.ParameterValue, value),
                        new Parameter(FirebaseAnalytics.ParameterItemId, itemId)
                    });
                }
                else if (action == "ad_impression" && enableAdTracking)
                {
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression, new Parameter[]
                    {
                        new Parameter(FirebaseAnalytics.ParameterAdPlatform, additionalParams?.GetValueOrDefault("ad_platform", "unknown")?.ToString() ?? "unknown"),
                        new Parameter(FirebaseAnalytics.ParameterAdSource, additionalParams?.GetValueOrDefault("ad_source", "unknown")?.ToString() ?? "unknown"),
                        new Parameter(FirebaseAnalytics.ParameterAdFormat, additionalParams?.GetValueOrDefault("ad_format", "unknown")?.ToString() ?? "unknown")
                    });
                    
                    // Track ad impression counts
                    var adType = additionalParams?.GetValueOrDefault("ad_type", "unknown")?.ToString() ?? "unknown";
                    if (_adImpressions.ContainsKey(adType))
                        _adImpressions[adType]++;
                    else
                        _adImpressions[adType] = 1;
                }
            }
        }
        
        /// <summary>
        /// Track screen view events for user flow analysis
        /// </summary>
        public void TrackScreenView(string screenName, Dictionary<string, object> additionalParams = null)
        {
            if (!trackScreenViews) return;
            
            var parameters = new Dictionary<string, object>
            {
                ["screen_name"] = screenName,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    parameters[param.Key] = param.Value;
                }
            }
            
            _screenViews.Add(screenName);
            
            TrackEvent("screen_view", parameters);
            
            // Track with Firebase Analytics
            if (enableFirebaseAnalytics)
            {
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView, new Parameter[]
                {
                    new Parameter(FirebaseAnalytics.ParameterScreenName, screenName),
                    new Parameter(FirebaseAnalytics.ParameterScreenClass, "GameScreen")
                });
            }
        }
        
        /// <summary>
        /// Track social interaction events
        /// </summary>
        public void TrackSocialEvent(string action, string targetUserId = null, Dictionary<string, object> additionalParams = null)
        {
            var parameters = new Dictionary<string, object>
            {
                ["action"] = action,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (!string.IsNullOrEmpty(targetUserId))
                parameters["target_user"] = targetUserId;
            
            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    parameters[param.Key] = param.Value;
                }
            }
            
            TrackEvent("social_action", parameters);
        }
        
        #endregion
        
        #region Event Processing
        
        /// <summary>
        /// Process queued events in batches for efficient transmission
        /// </summary>
        private void ProcessEventQueue()
        {
            if (!enableEventBatching || _eventQueue.Count == 0) return;
            
            try
            {
                // Process events up to batch size
                while (_eventQueue.Count > 0 && _currentBatch.Count < batchSize)
                {
                    _currentBatch.Add(_eventQueue.Dequeue());
                }
                
                // Check if batch is full or needs to be flushed
                if (_currentBatch.Count >= batchSize)
                {
                    FlushEventBatch();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing event queue: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check if it's time to flush the current batch based on timer
        /// </summary>
        private void CheckBatchFlushTimer()
        {
            if (!enableEventBatching || _currentBatch.Count == 0) return;
            
            var timeSinceLastFlush = Time.realtimeSinceStartup - _lastBatchFlush;
            
            if (timeSinceLastFlush >= batchFlushInterval)
            {
                FlushEventBatch();
            }
        }
        
        /// <summary>
        /// Flush current batch of events to analytics services
        /// </summary>
        private void FlushEventBatch(bool force = false)
        {
            if (_currentBatch.Count == 0 && !force) return;
            
            try
            {
                LogDebug($"Flushing event batch: {_currentBatch.Count} events");
                
                // Send each event in the batch
                foreach (var eventData in _currentBatch)
                {
                    SendEventToAnalytics(eventData);
                }
                
                // Clear batch and update counters
                _currentBatch.Clear();
                _totalBatchesSent++;
                _lastBatchFlush = Time.realtimeSinceStartup;
                
                // Limit queue size to prevent memory issues
                while (_eventQueue.Count > maxQueueSize)
                {
                    _eventQueue.Dequeue();
                    LogWarning("Event queue overflow - dropping oldest events");
                }
                
            }
            catch (Exception ex)
            {
                LogError($"Error flushing event batch: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send individual event to analytics services
        /// </summary>
        private void SendEventToAnalytics(AnalyticsEventData eventData)
        {
            try
            {
                // Send to Firebase Analytics
                if (enableFirebaseAnalytics)
                {
                    SendToFirebaseAnalytics(eventData);
                }
                
                // Send to custom analytics (Firestore for detailed analysis)
                if (enableCustomEvents && _firestoreManager?.IsOnline == true)
                {
                    SendToCustomAnalytics(eventData);
                }
                
                // Debug logging
                if (enableVerboseLogging)
                {
                    LogDebug($"Event sent: {eventData.eventName} ({eventData.parameters.Count} params)");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error sending event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send event to Firebase Analytics with parameter conversion
        /// </summary>
        private void SendToFirebaseAnalytics(AnalyticsEventData eventData)
        {
            try
            {
                var parameters = new List<Parameter>();
                
                foreach (var param in eventData.parameters)
                {
                    // Convert parameter value to Firebase Analytics compatible types
                    if (param.Value is string stringValue)
                    {
                        parameters.Add(new Parameter(param.Key, stringValue));
                    }
                    else if (param.Value is int intValue)
                    {
                        parameters.Add(new Parameter(param.Key, intValue));
                    }
                    else if (param.Value is long longValue)
                    {
                        parameters.Add(new Parameter(param.Key, longValue));
                    }
                    else if (param.Value is float floatValue)
                    {
                        parameters.Add(new Parameter(param.Key, floatValue));
                    }
                    else if (param.Value is double doubleValue)
                    {
                        parameters.Add(new Parameter(param.Key, doubleValue));
                    }
                    else if (param.Value is bool boolValue)
                    {
                        parameters.Add(new Parameter(param.Key, boolValue ? "true" : "false"));
                    }
                    else
                    {
                        // Convert to string for unsupported types
                        parameters.Add(new Parameter(param.Key, param.Value?.ToString() ?? "null"));
                    }
                }
                
                FirebaseAnalytics.LogEvent(eventData.eventName, parameters.ToArray());
            }
            catch (Exception ex)
            {
                LogError($"Error sending to Firebase Analytics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send event to custom analytics (Firestore) for detailed analysis
        /// </summary>
        private async void SendToCustomAnalytics(AnalyticsEventData eventData)
        {
            try
            {
                // Only send if online to avoid queue buildup
                if (_firestoreManager?.IsOnline != true) return;
                
                var firestore = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                var docRef = firestore.Collection("analytics_events").Document();
                
                await docRef.SetAsync(eventData);
            }
            catch (Exception ex)
            {
                LogError($"Error sending to custom analytics: {ex.Message}");
            }
        }
        
        #endregion
        
        #region User Properties
        
        /// <summary>
        /// Set user property for analytics segmentation
        /// </summary>
        public void SetUserProperty(string propertyName, object value)
        {
            if (!_isInitialized || !HasUserConsent || string.IsNullOrEmpty(propertyName))
                return;
            
            try
            {
                _userProperties[propertyName] = value;
                
                // Set in Firebase Analytics
                if (enableFirebaseAnalytics && trackUserProperties)
                {
                    FirebaseAnalytics.SetUserPropertyAsync(propertyName, value?.ToString() ?? "null");
                }
                
                LogDebug($"User property set: {propertyName} = {value}");
            }
            catch (Exception ex)
            {
                LogError($"Error setting user property: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set user ID for analytics tracking
        /// </summary>
        public void SetUserId(string userId)
        {
            if (!_isInitialized || !HasUserConsent)
                return;
            
            try
            {
                _userId = userId;
                
                // Set in Firebase Analytics
                if (enableFirebaseAnalytics)
                {
                    FirebaseAnalytics.SetUserIdAsync(anonymizeUserData ? HashUserId(userId) : userId);
                }
                
                // Update user properties
                SetUserProperty("user_id", anonymizeUserData ? HashUserId(userId) : userId);
                SetUserProperty("user_set_date", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                
                LogDebug($"User ID set: {userId}");
            }
            catch (Exception ex)
            {
                LogError($"Error setting user ID: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Update user properties based on authentication state
        /// </summary>
        public void UpdateUserPropertiesFromAuth()
        {
            if (_authManager?.IsAuthenticated == true)
            {
                SetUserId(_authManager.UserId);
                SetUserProperty("auth_provider", _authManager.CurrentProvider.ToString());
                SetUserProperty("user_anonymous", _authManager.IsAnonymous);
                SetUserProperty("auth_date", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Add standard parameters to all events
        /// </summary>
        private void AddStandardParameters(AnalyticsEventData eventData)
        {
            try
            {
                // Session information
                eventData.parameters["session_id"] = _currentSessionId;
                eventData.parameters["session_duration"] = CurrentSessionDuration.TotalSeconds;
                eventData.parameters["events_this_session"] = _eventsThisSession;
                
                // Device and platform information
                eventData.parameters["platform"] = eventData.platform;
                eventData.parameters["build_version"] = eventData.buildVersion;
                eventData.parameters["device_model"] = SystemInfo.deviceModel;
                
                // Network information
                eventData.parameters["network_reachability"] = Application.internetReachability.ToString();
                
                // Game state information
                if (_authManager?.IsAuthenticated == true)
                {
                    eventData.parameters["user_authenticated"] = true;
                    eventData.parameters["user_anonymous"] = _authManager.IsAnonymous;
                }
                else
                {
                    eventData.parameters["user_authenticated"] = false;
                }
                
                // Performance information
                eventData.parameters["fps"] = (1f / Time.deltaTime);
                eventData.parameters["memory_usage"] = (GC.GetTotalMemory(false) / 1024f / 1024f); // MB
                
            }
            catch (Exception ex)
            {
                LogError($"Error adding standard parameters: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check if event tracking is allowed (throttling)
        /// </summary>
        private bool CanTrackEvent()
        {
            // Check session event limit
            if (_eventsThisSession >= maxEventsPerSession)
            {
                return false;
            }
            
            // Check throttle interval
            var timeSinceLastEvent = DateTime.UtcNow - _lastEventTime;
            if (timeSinceLastEvent.TotalSeconds < eventThrottleInterval)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Hash user ID for privacy protection
        /// </summary>
        private string HashUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return "anonymous";
            
            // Simple hash for demonstration - use proper cryptographic hash in production
            return userId.GetHashCode().ToString("X8");
        }
        
        /// <summary>
        /// Handle authentication state changes
        /// </summary>
        private void HandleAuthenticationChanged(Firebase.Auth.FirebaseUser user)
        {
            try
            {
                if (user != null)
                {
                    // User signed in
                    UpdateUserPropertiesFromAuth();
                    TrackEvent("user_authenticated", new Dictionary<string, object>
                    {
                        ["auth_provider"] = _authManager.CurrentProvider.ToString(),
                        ["user_anonymous"] = _authManager.IsAnonymous
                    });
                }
                else
                {
                    // User signed out
                    TrackEvent("user_signed_out");
                    
                    // Clear user-specific properties
                    SetUserId("anonymous");
                    SetUserProperty("auth_provider", "none");
                    SetUserProperty("user_anonymous", true);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling auth change: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Request user consent for analytics tracking
        /// </summary>
        public void RequestAnalyticsConsent()
        {
            // In a real implementation, this would show a consent dialog
            // For development, we'll assume consent is granted
            GrantAnalyticsConsent();
        }
        
        /// <summary>
        /// Grant analytics consent
        /// </summary>
        public void GrantAnalyticsConsent()
        {
            _hasUserConsent = true;
            PlayerPrefs.SetInt("AnalyticsConsent", 1);
            PlayerPrefs.Save();
            
            OnAnalyticsConsentChanged?.Invoke(true);
            
            // Enable Firebase Analytics collection
            if (enableFirebaseAnalytics)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            
            // Initialize if not already done
            if (!_isInitialized)
            {
                InitializeAnalytics();
            }
            
            TrackEvent("analytics_consent_granted");
            LogDebug("Analytics consent granted");
        }
        
        /// <summary>
        /// Revoke analytics consent
        /// </summary>
        public void RevokeAnalyticsConsent()
        {
            if (HasUserConsent)
            {
                TrackEvent("analytics_consent_revoked");
                FlushEventBatch(true); // Send remaining events
            }
            
            _hasUserConsent = false;
            PlayerPrefs.SetInt("AnalyticsConsent", 0);
            PlayerPrefs.Save();
            
            OnAnalyticsConsentChanged?.Invoke(false);
            
            // Disable Firebase Analytics collection
            if (enableFirebaseAnalytics)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            }
            
            LogDebug("Analytics consent revoked");
        }
        
        /// <summary>
        /// Get analytics performance summary
        /// </summary>
        public string GetPerformanceSummary()
        {
            var summary = "Analytics Performance Summary:\n";
            summary += $"Total Events Logged: {_totalEventsLogged}\n";
            summary += $"Events This Session: {_eventsThisSession}\n";
            summary += $"Total Batches Sent: {_totalBatchesSent}\n";
            summary += $"Current Queue Size: {_eventQueue.Count}\n";
            summary += $"Session Duration: {CurrentSessionDuration.TotalMinutes:F1} minutes\n";
            summary += $"Session Revenue: ${_sessionRevenue:F2}\n";
            summary += $"Top Events:\n";
            
            var topEvents = _eventCounts.OrderByDescending(kvp => kvp.Value).Take(5);
            foreach (var eventCount in topEvents)
            {
                summary += $"  {eventCount.Key}: {eventCount.Value}\n";
            }
            
            return summary;
        }
        
        /// <summary>
        /// Reset analytics data (for testing)
        /// </summary>
        public void ResetAnalyticsData()
        {
            _totalEventsLogged = 0;
            _totalBatchesSent = 0;
            _eventsThisSession = 0;
            _sessionRevenue = 0f;
            _eventCounts.Clear();
            _screenViews.Clear();
            _purchaseData.Clear();
            _adImpressions.Clear();
            _eventQueue.Clear();
            _currentBatch.Clear();
            
            LogDebug("Analytics data reset");
        }
        
        #endregion
        
        #region Debug and Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[AnalyticsManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AnalyticsManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AnalyticsManager] {message}");
        }
        
        #endregion
    }
}