using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.RemoteConfig;
using Firebase.Extensions;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Remote Config Manager for Circuit Runners
    /// Handles A/B testing, feature flags, dynamic configuration, and player segmentation
    /// Enables data-driven optimization and live game tuning without app updates
    /// Optimized for free-to-play mobile games with rapid iteration cycles
    /// </summary>
    public class RemoteConfigManager : MonoBehaviour
    {
        #region Events and Delegates
        
        /// <summary>
        /// Remote Config events for game system updates
        /// </summary>
        public static event Action<Dictionary<string, object>> OnConfigFetched;
        public static event Action<string, object> OnConfigValueChanged;
        public static event Action<string> OnRemoteConfigError;
        public static event Action<ABTestResult> OnABTestAssigned;
        public static event Action<bool> OnRemoteConfigReady;
        
        /// <summary>
        /// A/B test result structure
        /// </summary>
        public struct ABTestResult
        {
            public string testName;
            public string variant;
            public Dictionary<string, object> parameters;
            public DateTime assignmentTime;
            public string playerSegment;
        }
        
        #endregion
        
        #region Configuration Classes
        
        /// <summary>
        /// Default configuration values (fallbacks)
        /// </summary>
        [Serializable]
        public class DefaultConfig
        {
            [Header("Gameplay Balance")]
            public int dailyEnergyLimit = 100;
            public float energyRefillRate = 1.0f; // per minute
            public int maxBotLevel = 50;
            public float difficultyScaling = 1.0f;
            public bool enablePowerUps = true;
            
            [Header("Monetization")]
            public float starterPackPrice = 4.99f;
            public int starterPackGems = 500;
            public bool enableSubscriptions = true;
            public int adFrequencyLevel = 3; // Games between ads
            public float premiumDiscountPercent = 0.0f;
            
            [Header("Social Features")]
            public bool enableTournaments = true;
            public int maxTournamentParticipants = 1000;
            public bool enableFriendInvites = true;
            public bool enableBotSharing = true;
            
            [Header("Events and Promotions")]
            public bool weekendBonusActive = false;
            public float weekendBonusMultiplier = 2.0f;
            public bool specialEventActive = false;
            public string eventName = "Default Event";
            public DateTime eventEndTime = DateTime.MaxValue;
            
            [Header("UI and UX")]
            public bool enableOnboarding = true;
            public bool enableTutorial = true;
            public int maxNotifications = 3;
            public bool enablePushNotifications = true;
            
            [Header("Performance")]
            public int maxParticleEffects = 100;
            public bool enableAdvancedGraphics = true;
            public float targetFrameRate = 60f;
            public bool enableDataSaving = false;
            
            [Header("Feature Flags")]
            public bool newBotUnlockSystem = false;
            public bool enhancedLeaderboards = false;
            public bool voiceChatEnabled = false;
            public bool crossPlatformPlay = false;
        }
        
        /// <summary>
        /// A/B test configuration
        /// </summary>
        [Serializable]
        public class ABTestConfig
        {
            public string testName;
            public List<string> variants = new List<string>();
            public Dictionary<string, float> weights = new Dictionary<string, float>();
            public Dictionary<string, Dictionary<string, object>> variantParams = 
                new Dictionary<string, Dictionary<string, object>>();
            public List<string> targetSegments = new List<string>();
            public DateTime startTime;
            public DateTime endTime;
            public bool isActive = true;
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Remote Config Settings")]
        [SerializeField] private DefaultConfig defaultConfig = new DefaultConfig();
        [SerializeField] private bool enableRemoteConfig = true;
        [SerializeField] private bool enableABTesting = true;
        [SerializeField] private float fetchIntervalMinutes = 30f;
        [SerializeField] private float fetchTimeoutSeconds = 60f;
        
        [Header("A/B Testing Configuration")]
        [SerializeField] private List<ABTestConfig> abTests = new List<ABTestConfig>();
        [SerializeField] private bool enablePlayerSegmentation = true;
        [SerializeField] private bool trackABTestEvents = true;
        
        [Header("Caching and Offline")]
        [SerializeField] private bool enableConfigCaching = true;
        [SerializeField] private float cacheExpirationHours = 24f;
        [SerializeField] private bool useCacheWhenOffline = true;
        [SerializeField] private bool preloadCriticalConfigs = true;
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableBatchUpdates = true;
        [SerializeField] private int maxConcurrentFetches = 3;
        [SerializeField] private bool enableConfigDifferential = true;
        [SerializeField] private float configChangeThreshold = 0.01f;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool logConfigChanges = true;
        [SerializeField] private bool simulateSlowNetwork = false;
        [SerializeField] private bool forceDefaultValues = false; // For testing
        
        // System references
        private FirebaseAuthManager _authManager;
        private AnalyticsManager _analyticsManager;
        
        // Remote Config state
        private bool _isInitialized;
        private bool _isFetching;
        private DateTime _lastFetchTime;
        private Dictionary<string, object> _currentConfig;
        private Dictionary<string, object> _previousConfig;
        
        // A/B Testing
        private Dictionary<string, ABTestResult> _activeABTests;
        private string _playerSegment;
        private Dictionary<string, object> _segmentationCriteria;
        
        // Caching
        private Dictionary<string, ConfigCacheEntry> _configCache;
        
        // Performance tracking
        private int _totalConfigFetches;
        private int _totalConfigChanges;
        private float _averageFetchTime;
        private DateTime _initializationTime;
        
        #endregion
        
        #region Cache Entry Class
        
        [Serializable]
        private class ConfigCacheEntry
        {
            public object value;
            public DateTime cacheTime;
            public bool isExpired => DateTime.UtcNow - cacheTime > TimeSpan.FromHours(24);
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether Remote Config is initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether a config fetch is in progress
        /// </summary>
        public bool IsFetching => _isFetching;
        
        /// <summary>
        /// Current configuration values
        /// </summary>
        public Dictionary<string, object> CurrentConfig => 
            new Dictionary<string, object>(_currentConfig ?? new Dictionary<string, object>());
        
        /// <summary>
        /// Active A/B tests for current user
        /// </summary>
        public Dictionary<string, ABTestResult> ActiveABTests => 
            new Dictionary<string, ABTestResult>(_activeABTests ?? new Dictionary<string, ABTestResult>());
        
        /// <summary>
        /// Current player segment
        /// </summary>
        public string PlayerSegment => _playerSegment ?? "default";
        
        /// <summary>
        /// Last successful fetch time
        /// </summary>
        public DateTime LastFetchTime => _lastFetchTime;
        
        /// <summary>
        /// Total configuration fetches performed
        /// </summary>
        public int TotalConfigFetches => _totalConfigFetches;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (FindObjectsOfType<RemoteConfigManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize data structures
            _currentConfig = new Dictionary<string, object>();
            _previousConfig = new Dictionary<string, object>();
            _activeABTests = new Dictionary<string, ABTestResult>();
            _segmentationCriteria = new Dictionary<string, object>();
            _configCache = new Dictionary<string, ConfigCacheEntry>();
            
            _initializationTime = DateTime.UtcNow;
            
            // Set up default A/B tests if none configured
            if (abTests.Count == 0)
            {
                SetupDefaultABTests();
            }
        }
        
        private void Start()
        {
            // Get system references
            _authManager = FindObjectOfType<FirebaseAuthManager>();
            _analyticsManager = FindObjectOfType<AnalyticsManager>();
            
            // Initialize Remote Config
            InitializeRemoteConfig();
            
            // Start periodic fetch cycle
            if (fetchIntervalMinutes > 0)
            {
                InvokeRepeating(nameof(PeriodicConfigFetch), fetchIntervalMinutes * 60f, fetchIntervalMinutes * 60f);
            }
        }
        
        private void OnDestroy()
        {
            // Save config cache
            SaveConfigCache();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize Firebase Remote Config system
        /// Sets up default values, player segmentation, and A/B testing
        /// </summary>
        private async void InitializeRemoteConfig()
        {
            try
            {
                LogDebug("Initializing Remote Config Manager...");
                
                if (!enableRemoteConfig)
                {
                    LogDebug("Remote Config disabled, using default values only");
                    await LoadDefaultConfiguration();
                    _isInitialized = true;
                    OnRemoteConfigReady?.Invoke(false);
                    return;
                }
                
                // Load cached configuration first
                LoadConfigCache();
                
                // Set up default configuration values
                await SetupDefaultConfiguration();
                
                // Determine player segmentation
                await DeterminePlayerSegmentation();
                
                // Set up A/B tests
                if (enableABTesting)
                {
                    SetupABTesting();
                }
                
                // Perform initial configuration fetch
                await FetchRemoteConfiguration();
                
                _isInitialized = true;
                LogDebug("Remote Config Manager initialized successfully");
                
                OnRemoteConfigReady?.Invoke(true);
                
            }
            catch (Exception ex)
            {
                LogError($"Remote Config initialization failed: {ex.Message}");
                
                // Fall back to default configuration
                await LoadDefaultConfiguration();
                _isInitialized = true;
                
                OnRemoteConfigError?.Invoke($"Initialization failed: {ex.Message}");
                OnRemoteConfigReady?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Set up default Firebase Remote Config values
        /// </summary>
        private async Task SetupDefaultConfiguration()
        {
            try
            {
                LogDebug("Setting up default configuration...");
                
                var defaults = new Dictionary<string, object>();
                
                // Convert DefaultConfig to dictionary
                var configType = typeof(DefaultConfig);
                var fields = configType.GetFields();
                
                foreach (var field in fields)
                {
                    if (field.IsPublic && !field.IsStatic)
                    {
                        var value = field.GetValue(defaultConfig);
                        defaults[field.Name] = value;
                    }
                }
                
                // Set defaults in Firebase Remote Config
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        LogDebug($"Default configuration set: {defaults.Count} parameters");
                    }
                    else
                    {
                        LogError($"Failed to set default configuration: {task.Exception?.Message}");
                    }
                });
                
                // Store in current config
                _currentConfig = new Dictionary<string, object>(defaults);
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error setting up default configuration: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Load default configuration without Remote Config
        /// </summary>
        private async Task LoadDefaultConfiguration()
        {
            try
            {
                _currentConfig.Clear();
                
                // Use reflection to populate from DefaultConfig
                var configType = typeof(DefaultConfig);
                var fields = configType.GetFields();
                
                foreach (var field in fields)
                {
                    if (field.IsPublic && !field.IsStatic)
                    {
                        var value = field.GetValue(defaultConfig);
                        _currentConfig[field.Name] = value;
                    }
                }
                
                LogDebug($"Default configuration loaded: {_currentConfig.Count} parameters");
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error loading default configuration: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Configuration Fetching
        
        /// <summary>
        /// Fetch remote configuration from Firebase
        /// Handles caching, player segmentation, and A/B test assignment
        /// </summary>
        public async Task<bool> FetchRemoteConfiguration()
        {
            if (_isFetching || !enableRemoteConfig)
            {
                LogDebug("Fetch already in progress or Remote Config disabled");
                return false;
            }
            
            try
            {
                _isFetching = true;
                var fetchStartTime = DateTime.UtcNow;
                
                LogDebug("Fetching remote configuration...");
                
                // Set fetch timeout
                var settings = FirebaseRemoteConfig.DefaultInstance.ConfigSettings;
                settings.FetchTimeoutInMilliseconds = (ulong)(fetchTimeoutSeconds * 1000);
                FirebaseRemoteConfig.DefaultInstance.ConfigSettings = settings;
                
                // Add player segmentation parameters
                var fetchData = new Dictionary<string, object>();
                if (enablePlayerSegmentation)
                {
                    foreach (var criteria in _segmentationCriteria)
                    {
                        fetchData[criteria.Key] = criteria.Value;
                    }
                }
                
                // Perform fetch with caching consideration
                var cacheExpiration = TimeSpan.FromMinutes(fetchIntervalMinutes);
                
                var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(cacheExpiration);
                
                // Add timeout handling
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(fetchTimeoutSeconds));
                var completedTask = await Task.WhenAny(fetchTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    LogWarning("Remote Config fetch timed out");
                    return false;
                }
                
                await fetchTask; // Ensure the fetch task completes
                
                // Activate fetched configuration
                var activateTask = FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                await activateTask;
                
                if (activateTask.IsCompletedSuccessfully)
                {
                    await ProcessFetchedConfiguration();
                    
                    _lastFetchTime = DateTime.UtcNow;
                    _totalConfigFetches++;
                    
                    var fetchDuration = (DateTime.UtcNow - fetchStartTime).TotalSeconds;
                    _averageFetchTime = (_averageFetchTime * (_totalConfigFetches - 1) + (float)fetchDuration) / _totalConfigFetches;
                    
                    LogDebug($"Remote configuration fetched successfully ({fetchDuration:F1}s)");
                    return true;
                }
                else
                {
                    LogError("Failed to activate fetched configuration");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error fetching remote configuration: {ex.Message}");
                OnRemoteConfigError?.Invoke($"Fetch failed: {ex.Message}");
                return false;
            }
            finally
            {
                _isFetching = false;
            }
        }
        
        /// <summary>
        /// Process fetched configuration and detect changes
        /// </summary>
        private async Task ProcessFetchedConfiguration()
        {
            try
            {
                _previousConfig = new Dictionary<string, object>(_currentConfig);
                _currentConfig.Clear();
                
                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                var allKeys = remoteConfig.Keys;
                
                foreach (string key in allKeys)
                {
                    var configValue = remoteConfig.GetValue(key);
                    
                    // Convert ConfigValue to appropriate type
                    object value = ConvertConfigValue(configValue);
                    _currentConfig[key] = value;
                    
                    // Cache the value
                    if (enableConfigCaching)
                    {
                        _configCache[key] = new ConfigCacheEntry
                        {
                            value = value,
                            cacheTime = DateTime.UtcNow
                        };
                    }
                    
                    // Check for changes
                    if (_previousConfig.ContainsKey(key))
                    {
                        if (!_previousConfig[key].Equals(value))
                        {
                            _totalConfigChanges++;
                            
                            if (logConfigChanges)
                            {
                                LogDebug($"Config changed: {key} = {_previousConfig[key]} -> {value}");
                            }
                            
                            OnConfigValueChanged?.Invoke(key, value);
                        }
                    }
                    else
                    {
                        // New configuration key
                        if (logConfigChanges)
                        {
                            LogDebug($"New config: {key} = {value}");
                        }
                        
                        OnConfigValueChanged?.Invoke(key, value);
                    }
                }
                
                // Update A/B test assignments based on new config
                if (enableABTesting)
                {
                    await UpdateABTestAssignments();
                }
                
                // Save configuration cache
                if (enableConfigCaching)
                {
                    SaveConfigCache();
                }
                
                // Notify listeners of complete configuration update
                OnConfigFetched?.Invoke(new Dictionary<string, object>(_currentConfig));
                
                // Track analytics event
                if (_analyticsManager != null)
                {
                    _analyticsManager.TrackEvent("remote_config_fetched", new Dictionary<string, object>
                    {
                        ["config_count"] = _currentConfig.Count,
                        ["changes_detected"] = _totalConfigChanges,
                        ["player_segment"] = _playerSegment,
                        ["fetch_duration"] = _averageFetchTime
                    });
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error processing fetched configuration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Convert Firebase ConfigValue to appropriate C# type
        /// </summary>
        private object ConvertConfigValue(ConfigValue configValue)
        {
            try
            {
                // Try different type conversions based on the value
                var stringValue = configValue.StringValue;
                
                // Boolean
                if (bool.TryParse(stringValue, out bool boolResult))
                {
                    return boolResult;
                }
                
                // Integer
                if (long.TryParse(stringValue, out long longResult))
                {
                    // Check if it fits in int
                    if (longResult <= int.MaxValue && longResult >= int.MinValue)
                    {
                        return (int)longResult;
                    }
                    return longResult;
                }
                
                // Float/Double
                if (double.TryParse(stringValue, out double doubleResult))
                {
                    // Check if it fits in float
                    if (doubleResult <= float.MaxValue && doubleResult >= float.MinValue)
                    {
                        return (float)doubleResult;
                    }
                    return doubleResult;
                }
                
                // DateTime
                if (DateTime.TryParse(stringValue, out DateTime dateResult))
                {
                    return dateResult;
                }
                
                // Default to string
                return stringValue;
            }
            catch (Exception ex)
            {
                LogError($"Error converting config value: {ex.Message}");
                return configValue.StringValue;
            }
        }
        
        /// <summary>
        /// Periodic configuration fetch
        /// </summary>
        private async void PeriodicConfigFetch()
        {
            if (_isInitialized && !_isFetching)
            {
                await FetchRemoteConfiguration();
            }
        }
        
        #endregion
        
        #region Configuration Access
        
        /// <summary>
        /// Get configuration value with type safety and fallback
        /// </summary>
        public T GetConfig<T>(string key, T defaultValue = default(T))
        {
            try
            {
                if (forceDefaultValues)
                {
                    return defaultValue;
                }
                
                // Check current configuration first
                if (_currentConfig.ContainsKey(key))
                {
                    var value = _currentConfig[key];
                    if (value is T directValue)
                    {
                        return directValue;
                    }
                    
                    // Try to convert the value
                    try
                    {
                        var convertedValue = (T)Convert.ChangeType(value, typeof(T));
                        return convertedValue;
                    }
                    catch (Exception convertEx)
                    {
                        LogWarning($"Failed to convert config value '{key}': {convertEx.Message}");
                    }
                }
                
                // Check cache if offline
                if (useCacheWhenOffline && _configCache.ContainsKey(key))
                {
                    var cacheEntry = _configCache[key];
                    if (!cacheEntry.isExpired && cacheEntry.value is T cachedValue)
                    {
                        LogDebug($"Using cached value for '{key}'");
                        return cachedValue;
                    }
                }
                
                // Return default value
                LogDebug($"Using default value for config '{key}': {defaultValue}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                LogError($"Error getting config value '{key}': {ex.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Get configuration value as string
        /// </summary>
        public string GetConfigString(string key, string defaultValue = "")
        {
            return GetConfig(key, defaultValue);
        }
        
        /// <summary>
        /// Get configuration value as integer
        /// </summary>
        public int GetConfigInt(string key, int defaultValue = 0)
        {
            return GetConfig(key, defaultValue);
        }
        
        /// <summary>
        /// Get configuration value as float
        /// </summary>
        public float GetConfigFloat(string key, float defaultValue = 0f)
        {
            return GetConfig(key, defaultValue);
        }
        
        /// <summary>
        /// Get configuration value as boolean
        /// </summary>
        public bool GetConfigBool(string key, bool defaultValue = false)
        {
            return GetConfig(key, defaultValue);
        }
        
        /// <summary>
        /// Check if configuration key exists
        /// </summary>
        public bool HasConfig(string key)
        {
            return _currentConfig.ContainsKey(key) || _configCache.ContainsKey(key);
        }
        
        #endregion
        
        #region A/B Testing
        
        /// <summary>
        /// Set up A/B testing system
        /// </summary>
        private void SetupABTesting()
        {
            try
            {
                LogDebug("Setting up A/B testing system...");
                
                foreach (var test in abTests)
                {
                    if (test.isActive && IsTestActive(test))
                    {
                        AssignPlayerToTest(test);
                    }
                }
                
                LogDebug($"A/B testing setup complete. Active tests: {_activeABTests.Count}");
            }
            catch (Exception ex)
            {
                LogError($"Error setting up A/B testing: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check if A/B test is currently active
        /// </summary>
        private bool IsTestActive(ABTestConfig test)
        {
            var now = DateTime.UtcNow;
            return now >= test.startTime && now <= test.endTime && test.isActive;
        }
        
        /// <summary>
        /// Assign player to A/B test variant
        /// </summary>
        private void AssignPlayerToTest(ABTestConfig test)
        {
            try
            {
                // Check if player is in target segments
                if (test.targetSegments.Count > 0 && !test.targetSegments.Contains(_playerSegment))
                {
                    return;
                }
                
                // Check if already assigned to this test
                if (_activeABTests.ContainsKey(test.testName))
                {
                    return;
                }
                
                // Determine variant based on player ID hash and weights
                var playerHash = GetPlayerHash();
                var variant = SelectVariant(test, playerHash);
                
                if (string.IsNullOrEmpty(variant))
                {
                    return;
                }
                
                // Create test result
                var testResult = new ABTestResult
                {
                    testName = test.testName,
                    variant = variant,
                    parameters = test.variantParams.GetValueOrDefault(variant, new Dictionary<string, object>()),
                    assignmentTime = DateTime.UtcNow,
                    playerSegment = _playerSegment
                };
                
                _activeABTests[test.testName] = testResult;
                
                // Track analytics event
                if (_analyticsManager != null && trackABTestEvents)
                {
                    _analyticsManager.TrackEvent("ab_test_assigned", new Dictionary<string, object>
                    {
                        ["test_name"] = test.testName,
                        ["variant"] = variant,
                        ["player_segment"] = _playerSegment,
                        ["assignment_time"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                
                OnABTestAssigned?.Invoke(testResult);
                LogDebug($"Player assigned to A/B test '{test.testName}': {variant}");
            }
            catch (Exception ex)
            {
                LogError($"Error assigning player to A/B test '{test.testName}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Select variant based on player hash and weights
        /// </summary>
        private string SelectVariant(ABTestConfig test, int playerHash)
        {
            try
            {
                if (test.variants.Count == 0) return null;
                
                // Use consistent hashing for stable assignment
                var normalizedHash = Math.Abs(playerHash % 100000) / 100000.0f;
                
                var cumulativeWeight = 0f;
                foreach (var variant in test.variants)
                {
                    var weight = test.weights.GetValueOrDefault(variant, 1f / test.variants.Count);
                    cumulativeWeight += weight;
                    
                    if (normalizedHash <= cumulativeWeight)
                    {
                        return variant;
                    }
                }
                
                // Fallback to first variant
                return test.variants[0];
            }
            catch (Exception ex)
            {
                LogError($"Error selecting A/B test variant: {ex.Message}");
                return test.variants.Count > 0 ? test.variants[0] : null;
            }
        }
        
        /// <summary>
        /// Get stable hash for current player
        /// </summary>
        private int GetPlayerHash()
        {
            var playerId = _authManager?.UserId ?? "anonymous";
            return playerId.GetHashCode();
        }
        
        /// <summary>
        /// Update A/B test assignments after config fetch
        /// </summary>
        private async Task UpdateABTestAssignments()
        {
            try
            {
                // Re-evaluate all tests
                foreach (var test in abTests)
                {
                    if (test.isActive && IsTestActive(test))
                    {
                        AssignPlayerToTest(test);
                    }
                    else if (_activeABTests.ContainsKey(test.testName))
                    {
                        // Remove inactive tests
                        _activeABTests.Remove(test.testName);
                        LogDebug($"A/B test '{test.testName}' deactivated");
                    }
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error updating A/B test assignments: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get A/B test variant for specified test
        /// </summary>
        public string GetABTestVariant(string testName)
        {
            if (_activeABTests.ContainsKey(testName))
            {
                return _activeABTests[testName].variant;
            }
            
            return "control"; // Default variant
        }
        
        /// <summary>
        /// Get A/B test parameter value
        /// </summary>
        public T GetABTestParameter<T>(string testName, string parameterName, T defaultValue = default(T))
        {
            try
            {
                if (_activeABTests.ContainsKey(testName))
                {
                    var test = _activeABTests[testName];
                    if (test.parameters.ContainsKey(parameterName))
                    {
                        var value = test.parameters[parameterName];
                        if (value is T directValue)
                        {
                            return directValue;
                        }
                        
                        // Try conversion
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
                
                return defaultValue;
            }
            catch (Exception ex)
            {
                LogError($"Error getting A/B test parameter: {ex.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Track A/B test conversion event
        /// </summary>
        public void TrackABTestConversion(string testName, string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_activeABTests.ContainsKey(testName) || _analyticsManager == null)
                return;
            
            try
            {
                var test = _activeABTests[testName];
                var eventParams = new Dictionary<string, object>
                {
                    ["test_name"] = testName,
                    ["variant"] = test.variant,
                    ["conversion_event"] = eventName,
                    ["player_segment"] = _playerSegment
                };
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        eventParams[$"custom_{param.Key}"] = param.Value;
                    }
                }
                
                _analyticsManager.TrackEvent("ab_test_conversion", eventParams);
                LogDebug($"A/B test conversion tracked: {testName}.{eventName}");
            }
            catch (Exception ex)
            {
                LogError($"Error tracking A/B test conversion: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Player Segmentation
        
        /// <summary>
        /// Determine player segmentation for targeted configurations
        /// </summary>
        private async Task DeterminePlayerSegmentation()
        {
            try
            {
                LogDebug("Determining player segmentation...");
                
                _segmentationCriteria.Clear();
                
                // Basic segmentation criteria
                _segmentationCriteria["platform"] = Application.platform.ToString();
                _segmentationCriteria["app_version"] = Application.version;
                _segmentationCriteria["first_session"] = !_authManager?.IsAuthenticated == true;
                
                // Time-based segmentation
                var now = DateTime.UtcNow;
                _segmentationCriteria["day_of_week"] = now.DayOfWeek.ToString();
                _segmentationCriteria["hour_of_day"] = now.Hour;
                _segmentationCriteria["is_weekend"] = now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;
                
                // Player progression (if available from game state)
                if (_authManager?.IsAuthenticated == true)
                {
                    // These would come from player profile data
                    _segmentationCriteria["user_authenticated"] = true;
                    _segmentationCriteria["user_id_hash"] = GetPlayerHash();
                    
                    // Additional criteria could include:
                    // - Player level
                    // - Purchase history
                    // - Session count
                    // - Retention tier
                }
                else
                {
                    _segmentationCriteria["user_authenticated"] = false;
                }
                
                // Device-based segmentation
                _segmentationCriteria["device_model"] = SystemInfo.deviceModel;
                _segmentationCriteria["operating_system"] = SystemInfo.operatingSystem;
                _segmentationCriteria["memory_size"] = SystemInfo.systemMemorySize;
                _segmentationCriteria["graphics_memory"] = SystemInfo.graphicsMemorySize;
                
                // Network-based segmentation
                _segmentationCriteria["network_reachability"] = Application.internetReachability.ToString();
                
                // Determine primary segment
                _playerSegment = DetermineSegmentFromCriteria();
                
                LogDebug($"Player segmentation complete. Segment: {_playerSegment}");
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error determining player segmentation: {ex.Message}");
                _playerSegment = "default";
            }
        }
        
        /// <summary>
        /// Determine primary player segment from criteria
        /// </summary>
        private string DetermineSegmentFromCriteria()
        {
            try
            {
                // Define segment hierarchy (most specific first)
                if (!_segmentationCriteria.GetValueOrDefault("user_authenticated", false).Equals(true))
                {
                    return "new_user";
                }
                
                if (_segmentationCriteria.GetValueOrDefault("is_weekend", false).Equals(true))
                {
                    return "weekend_player";
                }
                
                var platform = _segmentationCriteria.GetValueOrDefault("platform", "").ToString().ToLower();
                if (platform.Contains("ios"))
                {
                    return "ios_player";
                }
                else if (platform.Contains("android"))
                {
                    return "android_player";
                }
                
                return "default";
            }
            catch (Exception ex)
            {
                LogError($"Error determining segment: {ex.Message}");
                return "default";
            }
        }
        
        #endregion
        
        #region Configuration Caching
        
        /// <summary>
        /// Load configuration cache from persistent storage
        /// </summary>
        private void LoadConfigCache()
        {
            try
            {
                if (!enableConfigCaching) return;
                
                var cacheJson = PlayerPrefs.GetString("RemoteConfigCache", "");
                if (string.IsNullOrEmpty(cacheJson)) return;
                
                var cacheData = JsonUtility.FromJson<ConfigCacheData>(cacheJson);
                if (cacheData?.entries != null)
                {
                    foreach (var entry in cacheData.entries)
                    {
                        _configCache[entry.key] = new ConfigCacheEntry
                        {
                            value = entry.value,
                            cacheTime = DateTime.Parse(entry.cacheTime)
                        };
                    }
                    
                    LogDebug($"Configuration cache loaded: {_configCache.Count} entries");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading configuration cache: {ex.Message}");
                _configCache.Clear();
            }
        }
        
        /// <summary>
        /// Save configuration cache to persistent storage
        /// </summary>
        private void SaveConfigCache()
        {
            try
            {
                if (!enableConfigCaching || _configCache.Count == 0) return;
                
                var cacheData = new ConfigCacheData
                {
                    entries = new List<ConfigCacheEntrySerializable>()
                };
                
                foreach (var kvp in _configCache)
                {
                    cacheData.entries.Add(new ConfigCacheEntrySerializable
                    {
                        key = kvp.Key,
                        value = kvp.Value.value,
                        cacheTime = kvp.Value.cacheTime.ToString()
                    });
                }
                
                var cacheJson = JsonUtility.ToJson(cacheData);
                PlayerPrefs.SetString("RemoteConfigCache", cacheJson);
                PlayerPrefs.Save();
                
                LogDebug($"Configuration cache saved: {cacheData.entries.Count} entries");
            }
            catch (Exception ex)
            {
                LogError($"Error saving configuration cache: {ex.Message}");
            }
        }
        
        [Serializable]
        private class ConfigCacheData
        {
            public List<ConfigCacheEntrySerializable> entries;
        }
        
        [Serializable]
        private class ConfigCacheEntrySerializable
        {
            public string key;
            public object value;
            public string cacheTime;
        }
        
        #endregion
        
        #region Default A/B Tests Setup
        
        /// <summary>
        /// Set up default A/B tests for common optimization scenarios
        /// </summary>
        private void SetupDefaultABTests()
        {
            abTests = new List<ABTestConfig>
            {
                // Onboarding optimization test
                new ABTestConfig
                {
                    testName = "onboarding_flow",
                    variants = new List<string> { "control", "simplified", "gamified" },
                    weights = new Dictionary<string, float> 
                    { 
                        ["control"] = 0.4f, 
                        ["simplified"] = 0.3f, 
                        ["gamified"] = 0.3f 
                    },
                    variantParams = new Dictionary<string, Dictionary<string, object>>
                    {
                        ["control"] = new Dictionary<string, object> { ["tutorial_steps"] = 5, ["skip_allowed"] = true },
                        ["simplified"] = new Dictionary<string, object> { ["tutorial_steps"] = 3, ["skip_allowed"] = true },
                        ["gamified"] = new Dictionary<string, object> { ["tutorial_steps"] = 4, ["skip_allowed"] = false, ["rewards_shown"] = true }
                    },
                    targetSegments = new List<string> { "new_user" },
                    startTime = DateTime.UtcNow,
                    endTime = DateTime.UtcNow.AddDays(30),
                    isActive = true
                },
                
                // Monetization optimization test
                new ABTestConfig
                {
                    testName = "starter_pack_price",
                    variants = new List<string> { "control", "discount_10", "discount_20" },
                    weights = new Dictionary<string, float> 
                    { 
                        ["control"] = 0.33f, 
                        ["discount_10"] = 0.33f, 
                        ["discount_20"] = 0.34f 
                    },
                    variantParams = new Dictionary<string, Dictionary<string, object>>
                    {
                        ["control"] = new Dictionary<string, object> { ["price"] = 4.99f, ["discount"] = 0f },
                        ["discount_10"] = new Dictionary<string, object> { ["price"] = 4.49f, ["discount"] = 0.1f },
                        ["discount_20"] = new Dictionary<string, object> { ["price"] = 3.99f, ["discount"] = 0.2f }
                    },
                    targetSegments = new List<string> { "default", "ios_player", "android_player" },
                    startTime = DateTime.UtcNow,
                    endTime = DateTime.UtcNow.AddDays(14),
                    isActive = true
                },
                
                // Gameplay balance test
                new ABTestConfig
                {
                    testName = "energy_system",
                    variants = new List<string> { "current", "generous", "restrictive" },
                    weights = new Dictionary<string, float> 
                    { 
                        ["current"] = 0.5f, 
                        ["generous"] = 0.25f, 
                        ["restrictive"] = 0.25f 
                    },
                    variantParams = new Dictionary<string, Dictionary<string, object>>
                    {
                        ["current"] = new Dictionary<string, object> { ["max_energy"] = 100, ["refill_rate"] = 1.0f },
                        ["generous"] = new Dictionary<string, object> { ["max_energy"] = 150, ["refill_rate"] = 1.5f },
                        ["restrictive"] = new Dictionary<string, object> { ["max_energy"] = 75, ["refill_rate"] = 0.8f }
                    },
                    targetSegments = new List<string> { "default" },
                    startTime = DateTime.UtcNow,
                    endTime = DateTime.UtcNow.AddDays(21),
                    isActive = true
                }
            };
            
            LogDebug("Default A/B tests configured");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force refresh configuration from server
        /// </summary>
        public async Task<bool> RefreshConfiguration()
        {
            LogDebug("Force refreshing configuration...");
            return await FetchRemoteConfiguration();
        }
        
        /// <summary>
        /// Get configuration performance summary
        /// </summary>
        public string GetPerformanceSummary()
        {
            var uptime = DateTime.UtcNow - _initializationTime;
            
            var summary = "Remote Config Performance Summary:\n";
            summary += $"Uptime: {uptime.TotalHours:F1} hours\n";
            summary += $"Total Fetches: {_totalConfigFetches}\n";
            summary += $"Config Changes: {_totalConfigChanges}\n";
            summary += $"Average Fetch Time: {_averageFetchTime:F1}s\n";
            summary += $"Last Fetch: {_lastFetchTime:yyyy-MM-dd HH:mm:ss}\n";
            summary += $"Active A/B Tests: {_activeABTests.Count}\n";
            summary += $"Player Segment: {_playerSegment}\n";
            summary += $"Cached Configs: {_configCache.Count}\n";
            
            return summary;
        }
        
        /// <summary>
        /// Check if feature flag is enabled
        /// </summary>
        public bool IsFeatureEnabled(string featureName)
        {
            return GetConfigBool(featureName, false);
        }
        
        /// <summary>
        /// Get all configuration keys
        /// </summary>
        public List<string> GetAllConfigKeys()
        {
            return new List<string>(_currentConfig.Keys);
        }
        
        /// <summary>
        /// Export current configuration for debugging
        /// </summary>
        public Dictionary<string, object> ExportCurrentConfiguration()
        {
            return new Dictionary<string, object>(_currentConfig);
        }
        
        #endregion
        
        #region Debug and Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[RemoteConfigManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[RemoteConfigManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[RemoteConfigManager] {message}");
        }
        
        #endregion
    }
}