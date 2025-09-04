using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using CircuitRunners.Firebase.DataModels;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Firebase Authentication Manager for Circuit Runners
    /// Handles anonymous authentication, social login upgrades, and user session management
    /// Optimized for mobile gaming with instant play capabilities and seamless account upgrades
    /// </summary>
    public class FirebaseAuthManager : MonoBehaviour
    {
        #region Events and Delegates
        
        /// <summary>
        /// Authentication state change events for UI and game system updates
        /// </summary>
        public static event Action<FirebaseUser> OnAuthenticationStateChanged;
        public static event Action<string> OnAuthenticationError;
        public static event Action<AuthUpgradeResult> OnAccountUpgradeComplete;
        public static event Action OnSignOutComplete;
        
        /// <summary>
        /// Authentication upgrade result data
        /// </summary>
        public struct AuthUpgradeResult
        {
            public bool success;
            public string message;
            public AuthProviderType newProvider;
            public bool dataPreserved;
        }
        
        /// <summary>
        /// Supported authentication provider types
        /// </summary>
        public enum AuthProviderType
        {
            Anonymous,
            Google,
            Apple,
            GameCenter,  // iOS Game Center integration
            PlayGames    // Google Play Games integration
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Authentication Configuration")]
        [SerializeField] private bool enableAutoSignIn = true;
        [SerializeField] private bool enableAnonymousAuth = true;
        [SerializeField] private bool enableGoogleAuth = true;
        [SerializeField] private bool enableAppleAuth = true;
        [SerializeField] private float authTimeoutSeconds = 30f;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool simulateSlowConnection = false;
        
        private FirebaseAuth _auth;
        private FirebaseUser _currentUser;
        private bool _isInitialized;
        private bool _isAuthenticating;
        private AuthProviderType _currentAuthProvider;
        
        // Session management
        private string _sessionId;
        private DateTime _sessionStartTime;
        private int _authRetryCount;
        private const int MAX_AUTH_RETRIES = 3;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Current authenticated Firebase user
        /// </summary>
        public FirebaseUser CurrentUser => _currentUser;
        
        /// <summary>
        /// Current user's unique ID
        /// </summary>
        public string UserId => _currentUser?.UserId ?? string.Empty;
        
        /// <summary>
        /// Whether user is currently authenticated
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;
        
        /// <summary>
        /// Whether user is using anonymous authentication
        /// </summary>
        public bool IsAnonymous => _currentUser?.IsAnonymous ?? true;
        
        /// <summary>
        /// Current authentication provider type
        /// </summary>
        public AuthProviderType CurrentProvider => _currentAuthProvider;
        
        /// <summary>
        /// Whether authentication system is ready for use
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether authentication operation is in progress
        /// </summary>
        public bool IsAuthenticating => _isAuthenticating;
        
        /// <summary>
        /// Current session ID for analytics tracking
        /// </summary>
        public string SessionId => _sessionId;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern for authentication manager
            if (FindObjectsOfType<FirebaseAuthManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            _sessionId = Guid.NewGuid().ToString();
            _sessionStartTime = DateTime.UtcNow;
        }
        
        private void Start()
        {
            InitializeFirebaseAuth();
        }
        
        private void OnDestroy()
        {
            // Cleanup event handlers to prevent memory leaks
            if (_auth != null)
            {
                _auth.StateChanged -= HandleAuthStateChanged;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle app lifecycle for authentication token refresh
            if (!pauseStatus && IsAuthenticated)
            {
                RefreshAuthTokenIfNeeded();
            }
        }
        
        #endregion
        
        #region Firebase Initialization
        
        /// <summary>
        /// Initialize Firebase Authentication with comprehensive error handling
        /// Sets up authentication state listeners and prepares for user sign-in
        /// </summary>
        private async void InitializeFirebaseAuth()
        {
            try
            {
                LogDebug("Initializing Firebase Authentication...");
                
                // Check Firebase dependency status
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                
                if (dependencyStatus != DependencyStatus.Available)
                {
                    LogError($"Firebase dependencies not available: {dependencyStatus}");
                    OnAuthenticationError?.Invoke($"Firebase initialization failed: {dependencyStatus}");
                    return;
                }
                
                // Initialize Firebase Auth
                _auth = FirebaseAuth.DefaultInstance;
                
                if (_auth == null)
                {
                    LogError("Failed to initialize Firebase Auth instance");
                    OnAuthenticationError?.Invoke("Authentication service unavailable");
                    return;
                }
                
                // Set up authentication state change listener
                _auth.StateChanged += HandleAuthStateChanged;
                
                // Configure authentication settings for mobile gaming
                ConfigureAuthenticationSettings();
                
                _isInitialized = true;
                LogDebug("Firebase Authentication initialized successfully");
                
                // Auto-sign in if enabled and no current user
                if (enableAutoSignIn && _auth.CurrentUser == null)
                {
                    await AutoSignIn();
                }
                else if (_auth.CurrentUser != null)
                {
                    // User already signed in from previous session
                    HandleExistingUser(_auth.CurrentUser);
                }
                
            }
            catch (Exception ex)
            {
                LogError($"Firebase initialization error: {ex.Message}");
                OnAuthenticationError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Configure Firebase Authentication settings for optimal mobile gaming experience
        /// </summary>
        private void ConfigureAuthenticationSettings()
        {
            try
            {
                // Configure authentication persistence for mobile
                #if UNITY_ANDROID || UNITY_IOS
                // Authentication tokens persist automatically on mobile platforms
                LogDebug("Mobile authentication persistence enabled");
                #endif
                
                // Set custom timeout for slow network conditions
                // This is handled at the individual operation level
                LogDebug("Authentication settings configured");
            }
            catch (Exception ex)
            {
                LogError($"Error configuring auth settings: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Authentication Methods
        
        /// <summary>
        /// Automatic sign-in with anonymous authentication for instant play
        /// Provides immediate access to game features while preserving upgrade path
        /// </summary>
        public async Task<bool> AutoSignIn()
        {
            if (!_isInitialized || _isAuthenticating)
            {
                LogDebug("Cannot auto-sign in: not initialized or already authenticating");
                return false;
            }
            
            try
            {
                _isAuthenticating = true;
                LogDebug("Starting automatic sign-in...");
                
                // Check if user prefers to skip anonymous auth
                bool skipAnonymous = PlayerPrefs.GetInt("SkipAnonymousAuth", 0) == 1;
                
                if (!skipAnonymous && enableAnonymousAuth)
                {
                    return await SignInAnonymously();
                }
                else
                {
                    // Show login options UI instead of auto-signing in
                    LogDebug("Anonymous auth disabled, showing login options");
                    return false;
                }
            }
            finally
            {
                _isAuthenticating = false;
            }
        }
        
        /// <summary>
        /// Sign in anonymously for instant game access
        /// Perfect for mobile games where users want to play immediately
        /// </summary>
        public async Task<bool> SignInAnonymously()
        {
            if (!_isInitialized || _isAuthenticating)
            {
                LogWarning("Cannot sign in: system not ready");
                return false;
            }
            
            try
            {
                _isAuthenticating = true;
                _authRetryCount = 0;
                
                LogDebug("Signing in anonymously...");
                
                // Add timeout handling for slow networks
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(authTimeoutSeconds));
                var authTask = _auth.SignInAnonymouslyAsync();
                
                var completedTask = await Task.WhenAny(authTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Anonymous sign-in timed out");
                }
                
                var result = await authTask;
                
                if (result.User != null)
                {
                    _currentUser = result.User;
                    _currentAuthProvider = AuthProviderType.Anonymous;
                    
                    LogDebug($"Anonymous sign-in successful: {result.User.UserId}");
                    
                    // Track analytics for new anonymous user
                    await TrackAuthenticationEvent("anonymous_signin_success");
                    
                    return true;
                }
                else
                {
                    throw new Exception("Anonymous sign-in returned null user");
                }
            }
            catch (Exception ex)
            {
                return await HandleAuthenticationError("Anonymous sign-in failed", ex);
            }
            finally
            {
                _isAuthenticating = false;
            }
        }
        
        /// <summary>
        /// Sign in with Google account
        /// Provides secure authentication with Google Play Games integration
        /// </summary>
        public async Task<bool> SignInWithGoogle()
        {
            if (!enableGoogleAuth)
            {
                LogWarning("Google authentication is disabled");
                return false;
            }
            
            try
            {
                _isAuthenticating = true;
                LogDebug("Starting Google sign-in...");
                
                #if UNITY_ANDROID
                return await SignInWithGoogleAndroid();
                #elif UNITY_IOS
                return await SignInWithGoogleIOS();
                #else
                LogWarning("Google sign-in not supported on this platform");
                return false;
                #endif
            }
            catch (Exception ex)
            {
                return await HandleAuthenticationError("Google sign-in failed", ex);
            }
            finally
            {
                _isAuthenticating = false;
            }
        }
        
        #if UNITY_ANDROID
        /// <summary>
        /// Android-specific Google sign-in implementation
        /// Integrates with Google Play Games Services for enhanced social features
        /// </summary>
        private async Task<bool> SignInWithGoogleAndroid()
        {
            try
            {
                // Initialize Google Play Games authentication
                // This requires Google Play Games SDK integration
                
                LogDebug("Initializing Google Play Games sign-in...");
                
                // For demonstration purposes, showing the structure
                // Real implementation would use Google Play Games SDK
                
                /*
                GooglePlayGames.BasicApi.PlayGamesClientConfiguration config = 
                    new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder()
                        .RequestServerAuthCode(false)
                        .RequestIdToken()
                        .Build();
                
                GooglePlayGames.PlayGamesPlatform.InitializeInstance(config);
                GooglePlayGames.PlayGamesPlatform.ActivateInstance();
                
                var tcs = new TaskCompletionSource<bool>();
                
                Social.localUser.Authenticate((bool success, string message) =>
                {
                    tcs.SetResult(success);
                });
                
                bool playGamesSuccess = await tcs.Task;
                
                if (playGamesSuccess)
                {
                    string idToken = ((GooglePlayGames.PlayGamesPlatform)Social.Active).GetIdToken();
                    var credential = GoogleAuthProvider.GetCredential(idToken, null);
                    
                    var result = await _auth.SignInWithCredentialAsync(credential);
                    
                    if (result.User != null)
                    {
                        await HandleSuccessfulSocialSignIn(result.User, AuthProviderType.Google);
                        return true;
                    }
                }
                */
                
                // Placeholder for Google sign-in
                LogWarning("Google Play Games SDK integration required");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Android Google sign-in error: {ex.Message}");
                throw;
            }
        }
        #endif
        
        #if UNITY_IOS
        /// <summary>
        /// iOS-specific Google sign-in implementation
        /// Provides Google authentication on iOS devices
        /// </summary>
        private async Task<bool> SignInWithGoogleIOS()
        {
            try
            {
                LogDebug("Starting iOS Google sign-in...");
                
                // iOS Google sign-in implementation would go here
                // Requires Google SDK for iOS
                
                LogWarning("Google SDK for iOS integration required");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"iOS Google sign-in error: {ex.Message}");
                throw;
            }
        }
        #endif
        
        /// <summary>
        /// Sign in with Apple ID (iOS 13+ requirement for App Store)
        /// Provides privacy-focused authentication option
        /// </summary>
        public async Task<bool> SignInWithApple()
        {
            if (!enableAppleAuth)
            {
                LogWarning("Apple authentication is disabled");
                return false;
            }
            
            #if UNITY_IOS && !UNITY_EDITOR
            try
            {
                _isAuthenticating = true;
                LogDebug("Starting Apple sign-in...");
                
                // Apple sign-in implementation for iOS
                // Requires Apple Authentication Services framework
                
                LogWarning("Apple Authentication Services integration required");
                return false;
            }
            catch (Exception ex)
            {
                return await HandleAuthenticationError("Apple sign-in failed", ex);
            }
            finally
            {
                _isAuthenticating = false;
            }
            #else
            LogWarning("Apple sign-in only available on iOS");
            return false;
            #endif
        }
        
        /// <summary>
        /// Upgrade anonymous account to social account while preserving game data
        /// Critical for player retention and data continuity
        /// </summary>
        public async Task<AuthUpgradeResult> UpgradeAnonymousAccount(AuthProviderType targetProvider)
        {
            var result = new AuthUpgradeResult
            {
                success = false,
                message = "Unknown error",
                newProvider = targetProvider,
                dataPreserved = false
            };
            
            if (!IsAuthenticated || !IsAnonymous)
            {
                result.message = "No anonymous account to upgrade";
                return result;
            }
            
            try
            {
                _isAuthenticating = true;
                LogDebug($"Upgrading anonymous account to {targetProvider}...");
                
                // Store current user data before upgrade
                var currentUserId = _currentUser.UserId;
                await BackupUserDataForUpgrade(currentUserId);
                
                Credential credential = null;
                
                switch (targetProvider)
                {
                    case AuthProviderType.Google:
                        credential = await GetGoogleCredential();
                        break;
                    case AuthProviderType.Apple:
                        credential = await GetAppleCredential();
                        break;
                    default:
                        result.message = $"Unsupported provider: {targetProvider}";
                        return result;
                }
                
                if (credential == null)
                {
                    result.message = "Failed to obtain authentication credential";
                    return result;
                }
                
                // Link credential to existing anonymous account
                var linkResult = await _currentUser.LinkWithCredentialAsync(credential);
                
                if (linkResult.User != null)
                {
                    _currentUser = linkResult.User;
                    _currentAuthProvider = targetProvider;
                    
                    result.success = true;
                    result.message = $"Successfully upgraded to {targetProvider}";
                    result.dataPreserved = true;
                    
                    LogDebug($"Account upgrade successful: {linkResult.User.UserId}");
                    
                    // Track successful upgrade for analytics
                    await TrackAuthenticationEvent("account_upgrade_success", targetProvider.ToString());
                    
                    OnAccountUpgradeComplete?.Invoke(result);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                LogError($"Account upgrade failed: {ex.Message}");
                
                // Attempt to restore user data if upgrade failed
                await RestoreUserDataAfterFailedUpgrade(result);
                
                return result;
            }
            finally
            {
                _isAuthenticating = false;
            }
        }
        
        #endregion
        
        #region Account Management
        
        /// <summary>
        /// Sign out current user and clear session data
        /// Handles cleanup and state management
        /// </summary>
        public async Task<bool> SignOut()
        {
            try
            {
                LogDebug("Signing out user...");
                
                if (_auth != null && IsAuthenticated)
                {
                    // Track sign out event for analytics
                    await TrackAuthenticationEvent("user_signout");
                    
                    _auth.SignOut();
                    _currentUser = null;
                    _currentAuthProvider = AuthProviderType.Anonymous;
                }
                
                // Clear cached user data
                ClearUserCache();
                
                OnSignOutComplete?.Invoke();
                LogDebug("User signed out successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Sign out error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Delete user account permanently
        /// Handles GDPR compliance and data cleanup
        /// </summary>
        public async Task<bool> DeleteAccount()
        {
            if (!IsAuthenticated)
            {
                LogWarning("No authenticated user to delete");
                return false;
            }
            
            try
            {
                LogDebug("Deleting user account...");
                
                var userId = _currentUser.UserId;
                
                // Track account deletion for analytics (before deletion)
                await TrackAuthenticationEvent("account_deletion_requested");
                
                // Delete user account from Firebase Auth
                await _currentUser.DeleteAsync();
                
                // Clear local data
                ClearUserCache();
                await ClearUserDataFromDevice(userId);
                
                _currentUser = null;
                _currentAuthProvider = AuthProviderType.Anonymous;
                
                LogDebug("Account deleted successfully");
                OnSignOutComplete?.Invoke();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Account deletion error: {ex.Message}");
                OnAuthenticationError?.Invoke($"Account deletion failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Refresh authentication token if needed
        /// Ensures valid session for API calls
        /// </summary>
        public async Task<bool> RefreshAuthTokenIfNeeded()
        {
            if (!IsAuthenticated) return false;
            
            try
            {
                // Force token refresh if token is close to expiry
                var token = await _currentUser.TokenAsync(true);
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex)
            {
                LogError($"Token refresh error: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle Firebase authentication state changes
        /// Updates UI and game systems when authentication status changes
        /// </summary>
        private void HandleAuthStateChanged(object sender, EventArgs eventArgs)
        {
            var auth = sender as FirebaseAuth;
            if (auth == null) return;
            
            var previousUser = _currentUser;
            _currentUser = auth.CurrentUser;
            
            // User signed in
            if (_currentUser != null && previousUser != _currentUser)
            {
                LogDebug($"User signed in: {_currentUser.UserId} (Anonymous: {_currentUser.IsAnonymous})");
                
                // Determine authentication provider
                DetermineAuthenticationProvider(_currentUser);
                
                // Notify systems of authentication state change
                OnAuthenticationStateChanged?.Invoke(_currentUser);
                
                // Track sign-in event
                _ = TrackAuthenticationEvent("user_signin");
            }
            // User signed out
            else if (_currentUser == null && previousUser != null)
            {
                LogDebug("User signed out");
                _currentAuthProvider = AuthProviderType.Anonymous;
                OnAuthenticationStateChanged?.Invoke(null);
            }
        }
        
        /// <summary>
        /// Handle existing user from previous session
        /// Validates session and updates authentication state
        /// </summary>
        private async void HandleExistingUser(FirebaseUser user)
        {
            try
            {
                LogDebug($"Handling existing user: {user.UserId}");
                
                _currentUser = user;
                DetermineAuthenticationProvider(user);
                
                // Verify token is still valid
                var token = await user.TokenAsync(false);
                if (string.IsNullOrEmpty(token))
                {
                    LogWarning("Existing user token invalid, signing out");
                    await SignOut();
                    return;
                }
                
                OnAuthenticationStateChanged?.Invoke(user);
                await TrackAuthenticationEvent("session_resumed");
            }
            catch (Exception ex)
            {
                LogError($"Error handling existing user: {ex.Message}");
                await SignOut();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Determine current authentication provider from user info
        /// Used to track authentication method analytics
        /// </summary>
        private void DetermineAuthenticationProvider(FirebaseUser user)
        {
            if (user.IsAnonymous)
            {
                _currentAuthProvider = AuthProviderType.Anonymous;
            }
            else
            {
                // Check provider data to determine actual provider
                foreach (var provider in user.ProviderData)
                {
                    switch (provider.ProviderId)
                    {
                        case "google.com":
                            _currentAuthProvider = AuthProviderType.Google;
                            return;
                        case "apple.com":
                            _currentAuthProvider = AuthProviderType.Apple;
                            return;
                        case "playgames.google.com":
                            _currentAuthProvider = AuthProviderType.PlayGames;
                            return;
                        case "gamecenter.apple.com":
                            _currentAuthProvider = AuthProviderType.GameCenter;
                            return;
                    }
                }
                
                // Default to anonymous if provider not recognized
                _currentAuthProvider = AuthProviderType.Anonymous;
            }
        }
        
        /// <summary>
        /// Handle authentication errors with retry logic and user feedback
        /// </summary>
        private async Task<bool> HandleAuthenticationError(string operation, Exception ex)
        {
            _authRetryCount++;
            string errorMessage = $"{operation}: {ex.Message}";
            
            LogError(errorMessage);
            
            // Check for specific error types that might be recoverable
            if (ex is FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case (int)AuthError.NetworkRequestFailed:
                        if (_authRetryCount < MAX_AUTH_RETRIES)
                        {
                            LogDebug($"Network error, retrying... (Attempt {_authRetryCount})");
                            await Task.Delay(1000 * _authRetryCount); // Exponential backoff
                            return false; // Allow retry
                        }
                        errorMessage = "Network connection failed. Please check your internet connection.";
                        break;
                        
                    case (int)AuthError.TooManyRequests:
                        errorMessage = "Too many authentication attempts. Please try again later.";
                        break;
                        
                    case (int)AuthError.UserDisabled:
                        errorMessage = "This account has been disabled. Please contact support.";
                        break;
                        
                    case (int)AuthError.InvalidApiKey:
                        errorMessage = "Configuration error. Please update the app.";
                        break;
                        
                    default:
                        errorMessage = $"Authentication failed: {ex.Message}";
                        break;
                }
            }
            
            OnAuthenticationError?.Invoke(errorMessage);
            await TrackAuthenticationEvent("auth_error", errorMessage);
            
            return false;
        }
        
        /// <summary>
        /// Get Google authentication credential for account linking
        /// </summary>
        private async Task<Credential> GetGoogleCredential()
        {
            // This would integrate with Google Play Games or Google SDK
            // For now, return null as placeholder
            await Task.Delay(100); // Prevent compiler warning
            LogWarning("Google credential integration not implemented");
            return null;
        }
        
        /// <summary>
        /// Get Apple authentication credential for account linking
        /// </summary>
        private async Task<Credential> GetAppleCredential()
        {
            // This would integrate with Apple Authentication Services
            // For now, return null as placeholder
            await Task.Delay(100); // Prevent compiler warning
            LogWarning("Apple credential integration not implemented");
            return null;
        }
        
        /// <summary>
        /// Backup user data before account upgrade to prevent data loss
        /// </summary>
        private async Task BackupUserDataForUpgrade(string userId)
        {
            try
            {
                LogDebug($"Backing up user data for upgrade: {userId}");
                
                // Store critical user data in PlayerPrefs as backup
                PlayerPrefs.SetString("BackupUserId", userId);
                PlayerPrefs.SetString("BackupTimestamp", DateTime.UtcNow.ToString());
                PlayerPrefs.Save();
                
                // Additional backup logic would go here
                // e.g., save to local file, cloud backup, etc.
                
                await Task.Delay(100); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Backup failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Restore user data after failed account upgrade
        /// </summary>
        private async Task RestoreUserDataAfterFailedUpgrade(AuthUpgradeResult result)
        {
            try
            {
                LogDebug("Attempting to restore user data after failed upgrade...");
                
                // Restoration logic would go here
                // For now, just clear backup data
                PlayerPrefs.DeleteKey("BackupUserId");
                PlayerPrefs.DeleteKey("BackupTimestamp");
                
                await Task.Delay(100); // Prevent compiler warning
                
                result.dataPreserved = false;
            }
            catch (Exception ex)
            {
                LogError($"Data restoration failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clear cached user data from device
        /// </summary>
        private void ClearUserCache()
        {
            try
            {
                // Clear authentication-related PlayerPrefs
                PlayerPrefs.DeleteKey("LastUserId");
                PlayerPrefs.DeleteKey("LastAuthProvider");
                PlayerPrefs.DeleteKey("BackupUserId");
                PlayerPrefs.DeleteKey("BackupTimestamp");
                
                // Clear session data
                _sessionId = Guid.NewGuid().ToString();
                _sessionStartTime = DateTime.UtcNow;
                _authRetryCount = 0;
                
                LogDebug("User cache cleared");
            }
            catch (Exception ex)
            {
                LogError($"Cache clear error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clear all user data from device after account deletion
        /// </summary>
        private async Task ClearUserDataFromDevice(string userId)
        {
            try
            {
                LogDebug($"Clearing user data from device: {userId}");
                
                // Clear all game-related PlayerPrefs
                PlayerPrefs.DeleteAll();
                
                // Clear any cached files or local databases
                // This would include offline game data, cached images, etc.
                
                await Task.Delay(100); // Prevent compiler warning
                
                LogDebug("Device user data cleared");
            }
            catch (Exception ex)
            {
                LogError($"Device data clear error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Track authentication events for analytics and monitoring
        /// </summary>
        private async Task TrackAuthenticationEvent(string eventName, string additionalData = null)
        {
            try
            {
                // Create analytics event for authentication tracking
                var eventData = new AnalyticsEventData($"auth_{eventName}");
                eventData.parameters["session_id"] = _sessionId;
                eventData.parameters["auth_provider"] = _currentAuthProvider.ToString();
                eventData.parameters["user_anonymous"] = IsAnonymous;
                
                if (!string.IsNullOrEmpty(additionalData))
                {
                    eventData.parameters["additional_data"] = additionalData;
                }
                
                if (IsAuthenticated)
                {
                    eventData.parameters["user_id"] = UserId;
                }
                
                // Send to analytics system (implementation depends on analytics service)
                // For now, just log the event
                LogDebug($"Auth Event: {eventName} - {additionalData}");
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Analytics tracking error: {ex.Message}");
            }
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
                Debug.Log($"[FirebaseAuth] {message}");
            }
        }
        
        /// <summary>
        /// Warning logging
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[FirebaseAuth] {message}");
        }
        
        /// <summary>
        /// Error logging
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[FirebaseAuth] {message}");
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get current user's authentication token for API calls
        /// </summary>
        public async Task<string> GetAuthToken()
        {
            if (!IsAuthenticated) return string.Empty;
            
            try
            {
                return await _currentUser.TokenAsync(false);
            }
            catch (Exception ex)
            {
                LogError($"Token retrieval error: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Check if user email is verified (for social accounts)
        /// </summary>
        public bool IsEmailVerified()
        {
            return IsAuthenticated && _currentUser.IsEmailVerified;
        }
        
        /// <summary>
        /// Get user display name from authentication provider
        /// </summary>
        public string GetDisplayName()
        {
            return IsAuthenticated ? _currentUser.DisplayName ?? "Player" : "Guest";
        }
        
        /// <summary>
        /// Get user profile photo URL from authentication provider
        /// </summary>
        public string GetProfilePhotoUrl()
        {
            return IsAuthenticated && _currentUser.PhotoUrl != null ? _currentUser.PhotoUrl.ToString() : string.Empty;
        }
        
        /// <summary>
        /// Get session duration for analytics
        /// </summary>
        public TimeSpan GetSessionDuration()
        {
            return DateTime.UtcNow - _sessionStartTime;
        }
        
        #endregion
    }
}