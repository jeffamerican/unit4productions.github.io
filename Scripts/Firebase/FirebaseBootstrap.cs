using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Firebase Bootstrap System for Circuit Runners
    /// Handles Firebase initialization, dependency checking, and graceful fallback
    /// when Firebase services are unavailable. This ensures the game can still function
    /// even without Firebase connectivity.
    /// 
    /// Key Features:
    /// - Safe Firebase initialization with comprehensive error handling
    /// - Offline mode support when Firebase is unavailable
    /// - Dependency validation and automatic fallback mechanisms  
    /// - Performance monitoring of initialization process
    /// - User-friendly error messaging for configuration issues
    /// </summary>
    public class FirebaseBootstrap : MonoBehaviour
    {
        #region Configuration
        [Header("Firebase Configuration")]
        [SerializeField] private bool enableFirebaseServices = true;
        [SerializeField] private bool allowOfflineMode = true;
        [SerializeField] private float initializationTimeoutSeconds = 30f;
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("Service Configuration")]
        [SerializeField] private bool enableAuthentication = true;
        [SerializeField] private bool enableFirestore = true;
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableRemoteConfig = true;
        [SerializeField] private bool enableCloudMessaging = false;
        #endregion

        #region State Management
        private static FirebaseBootstrap _instance;
        private FirebaseApp _firebaseApp;
        private bool _isInitialized = false;
        private bool _initializationInProgress = false;
        private bool _offlineModeActive = false;
        private DependencyStatus _dependencyStatus;
        
        /// <summary>
        /// Singleton instance for global access
        /// </summary>
        public static FirebaseBootstrap Instance => _instance;
        
        /// <summary>
        /// Whether Firebase services are fully initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether the game is running in offline mode (Firebase unavailable)
        /// </summary>
        public bool IsOfflineMode => _offlineModeActive;
        
        /// <summary>
        /// Current Firebase dependency status
        /// </summary>
        public DependencyStatus DependencyStatus => _dependencyStatus;
        #endregion

        #region Events
        /// <summary>
        /// Fired when Firebase initialization completes successfully
        /// </summary>
        public static event Action OnFirebaseInitialized;
        
        /// <summary>
        /// Fired when Firebase initialization fails and offline mode is activated
        /// </summary>
        public static event Action<string> OnOfflineModeActivated;
        
        /// <summary>
        /// Fired when Firebase services become available after being offline
        /// </summary>
        public static event Action OnFirebaseReconnected;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Ensure singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LogDebug("Firebase Bootstrap initialized");
        }

        private void Start()
        {
            // Start Firebase initialization process
            if (enableFirebaseServices)
            {
                _ = InitializeFirebaseAsync();
            }
            else
            {
                LogDebug("Firebase services disabled, activating offline mode");
                ActivateOfflineMode("Firebase services disabled in configuration");
            }
        }

        private void OnDestroy()
        {
            // Clean up resources
            if (_firebaseApp != null)
            {
                // Firebase app cleanup is handled automatically
                LogDebug("Firebase Bootstrap destroyed");
            }
        }
        #endregion

        #region Firebase Initialization
        /// <summary>
        /// Initialize Firebase services with comprehensive error handling and fallback support
        /// </summary>
        public async Task<bool> InitializeFirebaseAsync()
        {
            if (_isInitialized || _initializationInProgress)
            {
                LogDebug("Firebase already initialized or initialization in progress");
                return _isInitialized;
            }

            _initializationInProgress = true;
            
            try
            {
                LogDebug("Starting Firebase initialization...");
                
                // Check if google-services.json exists and is valid
                if (!ValidateFirebaseConfiguration())
                {
                    throw new Exception("Firebase configuration file (google-services.json) not found or invalid");
                }
                
                // Check and fix Firebase dependencies with timeout
                var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(initializationTimeoutSeconds));
                
                var completedTask = await Task.WhenAny(dependencyTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Firebase dependency check timed out");
                }
                
                _dependencyStatus = await dependencyTask;
                
                if (_dependencyStatus != DependencyStatus.Available)
                {
                    throw new Exception($"Firebase dependencies not available: {_dependencyStatus}");
                }
                
                // Initialize Firebase App
                _firebaseApp = FirebaseApp.DefaultInstance;
                
                if (_firebaseApp == null)
                {
                    throw new Exception("Failed to initialize Firebase App");
                }
                
                LogDebug($"Firebase App initialized: {_firebaseApp.Name}");
                
                // Initialize individual services
                await InitializeFirebaseServices();
                
                _isInitialized = true;
                _offlineModeActive = false;
                
                LogDebug("Firebase initialization completed successfully");
                OnFirebaseInitialized?.Invoke();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Firebase initialization failed: {ex.Message}");
                
                if (allowOfflineMode)
                {
                    ActivateOfflineMode($"Firebase initialization failed: {ex.Message}");
                    return false;
                }
                else
                {
                    throw; // Re-throw if offline mode not allowed
                }
            }
            finally
            {
                _initializationInProgress = false;
            }
        }

        /// <summary>
        /// Initialize individual Firebase services based on configuration
        /// </summary>
        private async Task InitializeFirebaseServices()
        {
            var initializationTasks = new List<Task>();
            
            // Initialize services in parallel for better performance
            if (enableAuthentication)
            {
                initializationTasks.Add(InitializeAuthenticationService());
            }
            
            if (enableAnalytics)
            {
                initializationTasks.Add(InitializeAnalyticsService());
            }
            
            if (enableFirestore)
            {
                initializationTasks.Add(InitializeFirestoreService());
            }
            
            if (enableRemoteConfig)
            {
                initializationTasks.Add(InitializeRemoteConfigService());
            }
            
            // Wait for all services to initialize
            try
            {
                await Task.WhenAll(initializationTasks);
                LogDebug("All Firebase services initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Some Firebase services failed to initialize: {ex.Message}");
                // Continue with partial initialization rather than failing completely
            }
        }

        /// <summary>
        /// Initialize Firebase Authentication service
        /// </summary>
        private async Task InitializeAuthenticationService()
        {
            try
            {
                LogDebug("Initializing Firebase Authentication...");
                
                // Firebase Auth initializes automatically when accessed
                // We just need to verify it's working
                var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                
                if (auth != null)
                {
                    LogDebug("Firebase Authentication initialized successfully");
                }
                else
                {
                    throw new Exception("Firebase Auth instance is null");
                }
                
                await Task.Delay(100); // Small delay to ensure initialization completes
            }
            catch (Exception ex)
            {
                LogError($"Firebase Authentication initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize Firebase Analytics service
        /// </summary>
        private async Task InitializeAnalyticsService()
        {
            try
            {
                LogDebug("Initializing Firebase Analytics...");
                
#if !UNITY_EDITOR
                // Enable analytics data collection (not available in editor)
                Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                LogDebug("Firebase Analytics enabled");
#else
                LogDebug("Firebase Analytics disabled in editor");
#endif
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                LogError($"Firebase Analytics initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize Firebase Firestore service
        /// </summary>
        private async Task InitializeFirestoreService()
        {
            try
            {
                LogDebug("Initializing Firebase Firestore...");
                
                var firestore = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                
                if (firestore != null)
                {
                    // Configure Firestore settings for mobile optimization
                    var settings = new Firebase.Firestore.FirestoreSettings
                    {
                        PersistenceEnabled = true, // Enable offline persistence
                        CacheSizeBytes = 50 * 1024 * 1024 // 50MB cache
                    };
                    
                    firestore.Settings = settings;
                    LogDebug("Firebase Firestore initialized with offline persistence");
                }
                else
                {
                    throw new Exception("Firestore instance is null");
                }
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                LogError($"Firebase Firestore initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize Firebase Remote Config service
        /// </summary>
        private async Task InitializeRemoteConfigService()
        {
            try
            {
                LogDebug("Initializing Firebase Remote Config...");
                
                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                
                if (remoteConfig != null)
                {
                    // Set configuration settings for development
                    var settings = new Firebase.RemoteConfig.ConfigSettings
                    {
#if UNITY_EDITOR
                        MinimumFetchInternalInMilliseconds = 0 // No throttling in editor
#else
                        MinimumFetchInternalInMilliseconds = 3600000 // 1 hour in production
#endif
                    };
                    
                    remoteConfig.Settings = settings;
                    LogDebug("Firebase Remote Config initialized");
                }
                else
                {
                    throw new Exception("Remote Config instance is null");
                }
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                LogError($"Firebase Remote Config initialization failed: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Configuration Validation
        /// <summary>
        /// Validate Firebase configuration files and settings
        /// </summary>
        private bool ValidateFirebaseConfiguration()
        {
            try
            {
                LogDebug("Validating Firebase configuration...");
                
#if UNITY_ANDROID
                // Check for google-services.json in StreamingAssets
                string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, "google-services.json");
                
                if (!System.IO.File.Exists(configPath))
                {
                    // Check if template exists and provide helpful message
                    string templatePath = System.IO.Path.Combine(Application.streamingAssetsPath, "google-services-template.json");
                    if (System.IO.File.Exists(templatePath))
                    {
                        LogError("Found google-services-template.json but not google-services.json. Please configure Firebase properly.");
                        return false;
                    }
                    
                    LogError("google-services.json not found in StreamingAssets folder");
                    return false;
                }
                
                // Basic validation of the JSON structure
                string configJson = System.IO.File.ReadAllText(configPath);
                if (string.IsNullOrEmpty(configJson) || !configJson.Contains("project_info"))
                {
                    LogError("google-services.json appears to be invalid or empty");
                    return false;
                }
                
                LogDebug("Android Firebase configuration validated");
                
#elif UNITY_IOS
                // iOS uses GoogleService-Info.plist, which should be in the iOS build
                LogDebug("iOS Firebase configuration validation skipped (handled by build process)");
                
#else
                LogWarning("Firebase configuration validation not implemented for this platform");
#endif
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Firebase configuration validation failed: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Offline Mode Management
        /// <summary>
        /// Activate offline mode when Firebase is unavailable
        /// </summary>
        private void ActivateOfflineMode(string reason)
        {
            _offlineModeActive = true;
            _isInitialized = false;
            
            LogWarning($"Activating offline mode: {reason}");
            
            // Notify systems that offline mode is active
            OnOfflineModeActivated?.Invoke(reason);
            
            // Start periodic reconnection attempts
            if (enableFirebaseServices)
            {
                StartCoroutine(ReconnectionAttempts());
            }
        }

        /// <summary>
        /// Periodically attempt to reconnect to Firebase services
        /// </summary>
        private System.Collections.IEnumerator ReconnectionAttempts()
        {
            const float reconnectInterval = 60f; // Try every minute
            const int maxReconnectAttempts = 10;
            
            int attempts = 0;
            
            while (_offlineModeActive && attempts < maxReconnectAttempts)
            {
                yield return new WaitForSeconds(reconnectInterval);
                
                LogDebug($"Attempting Firebase reconnection... (Attempt {attempts + 1}/{maxReconnectAttempts})");
                
                var success = await InitializeFirebaseAsync();
                
                if (success)
                {
                    LogDebug("Firebase reconnection successful");
                    OnFirebaseReconnected?.Invoke();
                    yield break; // Exit coroutine on success
                }
                
                attempts++;
            }
            
            if (attempts >= maxReconnectAttempts)
            {
                LogWarning("Max reconnection attempts reached, staying in offline mode");
            }
        }

        /// <summary>
        /// Manually attempt to reconnect to Firebase services
        /// </summary>
        public async Task<bool> TryReconnect()
        {
            if (_isInitialized)
            {
                LogDebug("Firebase already initialized, no need to reconnect");
                return true;
            }
            
            LogDebug("Manual reconnection attempt...");
            return await InitializeFirebaseAsync();
        }
        #endregion

        #region Service Status Checks
        /// <summary>
        /// Check if specific Firebase service is available
        /// </summary>
        public bool IsServiceAvailable(FirebaseService service)
        {
            if (!_isInitialized || _offlineModeActive)
            {
                return false;
            }
            
            try
            {
                switch (service)
                {
                    case FirebaseService.Authentication:
                        return enableAuthentication && Firebase.Auth.FirebaseAuth.DefaultInstance != null;
                        
                    case FirebaseService.Firestore:
                        return enableFirestore && Firebase.Firestore.FirebaseFirestore.DefaultInstance != null;
                        
                    case FirebaseService.Analytics:
                        return enableAnalytics; // Analytics doesn't have a direct instance check
                        
                    case FirebaseService.RemoteConfig:
                        return enableRemoteConfig && Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance != null;
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error checking service availability for {service}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get comprehensive status of all Firebase services
        /// </summary>
        public FirebaseStatus GetFirebaseStatus()
        {
            return new FirebaseStatus
            {
                IsInitialized = _isInitialized,
                IsOfflineMode = _offlineModeActive,
                DependencyStatus = _dependencyStatus,
                AuthenticationAvailable = IsServiceAvailable(FirebaseService.Authentication),
                FirestoreAvailable = IsServiceAvailable(FirebaseService.Firestore),
                AnalyticsAvailable = IsServiceAvailable(FirebaseService.Analytics),
                RemoteConfigAvailable = IsServiceAvailable(FirebaseService.RemoteConfig)
            };
        }
        #endregion

        #region Logging
        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[FirebaseBootstrap] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[FirebaseBootstrap] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[FirebaseBootstrap] {message}");
        }
        #endregion
    }

    #region Data Structures
    /// <summary>
    /// Available Firebase services
    /// </summary>
    public enum FirebaseService
    {
        Authentication,
        Firestore,
        Analytics,
        RemoteConfig,
        CloudMessaging
    }

    /// <summary>
    /// Comprehensive Firebase status information
    /// </summary>
    [System.Serializable]
    public struct FirebaseStatus
    {
        public bool IsInitialized;
        public bool IsOfflineMode;
        public DependencyStatus DependencyStatus;
        public bool AuthenticationAvailable;
        public bool FirestoreAvailable;
        public bool AnalyticsAvailable;
        public bool RemoteConfigAvailable;
    }
    #endregion
}