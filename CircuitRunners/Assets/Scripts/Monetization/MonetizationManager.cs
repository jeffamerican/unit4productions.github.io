using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Advertisements;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Comprehensive monetization manager for Circuit Runners integrating Unity Ads and IAP.
    /// Handles all revenue streams including rewarded videos, interstitials, and in-app purchases.
    /// 
    /// Key Features:
    /// - Unity Ads integration (rewarded videos, interstitials, banners)
    /// - Unity IAP integration for purchases
    /// - Smart ad timing and frequency capping
    /// - Premium currency and item sales
    /// - Analytics integration for revenue tracking
    /// - A/B testing support for monetization optimization
    /// </summary>
    public class MonetizationManager : MonoBehaviour, IStoreListener, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        #region Configuration
        [Header("Unity Ads Configuration")]
        [SerializeField] private string _androidGameId = "4374822"; // Example ID
        [SerializeField] private string _iosGameId = "4374823";     // Example ID
        [SerializeField] private bool _testMode = true;
        [SerializeField] private bool _enableConsentFlow = true;
        
        [Header("Ad Unit IDs")]
        [SerializeField] private string _rewardedVideoAdUnitId = "Rewarded_Android";
        [SerializeField] private string _interstitialAdUnitId = "Interstitial_Android";
        [SerializeField] private string _bannerAdUnitId = "Banner_Android";
        
        [Header("Ad Timing")]
        [SerializeField] private float _interstitialCooldown = 180f; // 3 minutes between interstitials
        [SerializeField] private int _runsPerInterstitial = 3; // Show interstitial every 3 runs
        [SerializeField] private float _rewardedAdCooldown = 30f; // 30 seconds between rewarded ads
        [SerializeField] private int _maxRewardedAdsPerSession = 10; // Limit per session
        
        /// <summary>
        /// Current game ID based on platform
        /// </summary>
        private string CurrentGameId =>
        #if UNITY_IOS
            _iosGameId;
        #else
            _androidGameId;
        #endif
        #endregion

        #region IAP Configuration
        [Header("In-App Purchase Configuration")]
        [SerializeField] private bool _enableIAP = true;
        [SerializeField] private List<IAPProduct> _iapProducts = new List<IAPProduct>();
        
        // IAP Product IDs (would be configured in Unity Dashboard)
        private const string PREMIUM_CURRENCY_SMALL = "premium_currency_100";
        private const string PREMIUM_CURRENCY_MEDIUM = "premium_currency_500";
        private const string PREMIUM_CURRENCY_LARGE = "premium_currency_1200";
        private const string PREMIUM_ACCOUNT_MONTH = "premium_account_month";
        private const string AD_REMOVAL = "remove_ads_permanent";
        private const string STARTER_PACK = "starter_pack_special";
        
        // Unity IAP
        private IStoreController _storeController;
        private IExtensionProvider _storeExtensionProvider;
        #endregion

        #region State Management
        [Header("Current State")]
        [SerializeField] private bool _isInitialized = false;
        [SerializeField] private bool _adsEnabled = true;
        [SerializeField] private bool _iapEnabled = true;
        [SerializeField] private int _runsSinceLastInterstitial = 0;
        [SerializeField] private int _rewardedAdsWatchedThisSession = 0;
        
        private DateTime _lastInterstitialTime = DateTime.MinValue;
        private DateTime _lastRewardedAdTime = DateTime.MinValue;
        private bool _isShowingAd = false;
        private string _currentRewardedAdContext = "";
        
        /// <summary>
        /// Whether monetization is fully initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Whether ads are enabled (not purchased ad removal)
        /// </summary>
        public bool AdsEnabled => _adsEnabled && !HasAdRemoval;
        
        /// <summary>
        /// Whether player has purchased ad removal
        /// </summary>
        public bool HasAdRemoval { get; private set; } = false;
        #endregion

        #region Revenue Analytics
        [Header("Analytics")]
        [SerializeField] private MonetizationAnalytics _analytics;
        [SerializeField] private bool _enableAnalytics = true;
        
        // Session tracking
        private DateTime _sessionStartTime;
        private int _sessionRunCount = 0;
        private float _sessionRevenue = 0f;
        private Dictionary<string, int> _sessionAdCounts = new Dictionary<string, int>();
        #endregion

        #region Events
        /// <summary>
        /// Fired when rewarded ad is successfully watched
        /// </summary>
        public event Action<string, int> OnRewardedAdCompleted; // context, reward amount
        
        /// <summary>
        /// Fired when interstitial ad is shown
        /// </summary>
        public event Action OnInterstitialAdShown;
        
        /// <summary>
        /// Fired when purchase is completed
        /// </summary>
        public event Action<string, string> OnPurchaseCompleted; // product ID, receipt
        
        /// <summary>
        /// Fired when monetization initializes
        /// </summary>
        public event Action<bool> OnMonetizationInitialized; // success
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Ensure singleton behavior
            if (FindObjectsOfType<MonetizationManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize analytics
            if (_analytics == null)
                _analytics = new MonetizationAnalytics();
                
            _sessionStartTime = DateTime.Now;
        }

        private void Start()
        {
            // Initialize monetization systems
            InitializeMonetization();
        }

        private void Update()
        {
            // Update session tracking
            UpdateSessionTracking();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save monetization state and analytics
                SaveMonetizationData();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize all monetization systems
        /// </summary>
        private void InitializeMonetization()
        {
            Debug.Log("[MonetizationManager] Initializing monetization systems...");
            
            // Load saved monetization state
            LoadMonetizationData();
            
            // Initialize Unity Ads
            InitializeUnityAds();
            
            // Initialize Unity IAP
            if (_enableIAP)
            {
                InitializeUnityIAP();
            }
        }

        /// <summary>
        /// Initialize Unity Ads
        /// </summary>
        private void InitializeUnityAds()
        {
            if (Advertisement.isSupported && !Advertisement.isInitialized)
            {
                Debug.Log($"[MonetizationManager] Initializing Unity Ads with Game ID: {CurrentGameId}");
                Advertisement.Initialize(CurrentGameId, _testMode, _enableConsentFlow, this);
            }
            else
            {
                Debug.LogWarning("[MonetizationManager] Unity Ads not supported or already initialized");
                OnUnityAdsInitializationComplete();
            }
        }

        /// <summary>
        /// Initialize Unity IAP
        /// </summary>
        private void InitializeUnityIAP()
        {
            if (_storeController != null)
            {
                Debug.Log("[MonetizationManager] Unity IAP already initialized");
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
            // Add all IAP products
            builder.AddProduct(PREMIUM_CURRENCY_SMALL, ProductType.Consumable);
            builder.AddProduct(PREMIUM_CURRENCY_MEDIUM, ProductType.Consumable);
            builder.AddProduct(PREMIUM_CURRENCY_LARGE, ProductType.Consumable);
            builder.AddProduct(PREMIUM_ACCOUNT_MONTH, ProductType.Subscription);
            builder.AddProduct(AD_REMOVAL, ProductType.NonConsumable);
            builder.AddProduct(STARTER_PACK, ProductType.Consumable);
            
            Debug.Log("[MonetizationManager] Initializing Unity IAP...");
            UnityPurchasing.Initialize(this, builder);
        }

        /// <summary>
        /// Complete initialization process
        /// </summary>
        private void CompleteInitialization()
        {
            _isInitialized = true;
            OnMonetizationInitialized?.Invoke(true);
            
            Debug.Log("[MonetizationManager] Monetization initialization complete");
            
            // Perform initial setup
            SetupInitialState();
        }

        /// <summary>
        /// Setup initial monetization state
        /// </summary>
        private void SetupInitialState()
        {
            // Check for existing purchases (ad removal, premium account)
            if (_storeController != null)
            {
                CheckExistingPurchases();
            }
            
            // Load banner ad if appropriate
            if (ShouldShowBannerAds())
            {
                LoadBannerAd();
            }
            
            // Track session start
            if (_enableAnalytics)
            {
                _analytics.TrackSessionStart();
            }
        }
        #endregion

        #region Unity Ads Implementation
        /// <summary>
        /// Show rewarded video ad with context
        /// </summary>
        /// <param name="context">Context for analytics (e.g., "energy_refill", "double_rewards")</param>
        /// <param name="callback">Callback with success status</param>
        public void ShowRewardedAd(string context, Action<bool> callback = null)
        {
            if (!CanShowRewardedAd())
            {
                Debug.LogWarning("[MonetizationManager] Cannot show rewarded ad - conditions not met");
                callback?.Invoke(false);
                return;
            }

            _currentRewardedAdContext = context;
            _isShowingAd = true;
            
            Debug.Log($"[MonetizationManager] Showing rewarded ad for context: {context}");
            Advertisement.Show(_rewardedVideoAdUnitId, this);
            
            // Track ad request
            if (_enableAnalytics)
            {
                _analytics.TrackAdRequest("rewarded", context);
            }
        }

        /// <summary>
        /// Show interstitial ad if appropriate
        /// </summary>
        public void ShowInterstitialAd()
        {
            if (!ShouldShowInterstitialAd())
            {
                Debug.Log("[MonetizationManager] Skipping interstitial ad - conditions not met");
                return;
            }

            _isShowingAd = true;
            _lastInterstitialTime = DateTime.Now;
            _runsSinceLastInterstitial = 0;
            
            Debug.Log("[MonetizationManager] Showing interstitial ad");
            Advertisement.Show(_interstitialAdUnitId, this);
            
            // Track ad show
            if (_enableAnalytics)
            {
                _analytics.TrackAdShow("interstitial", "run_completion");
            }
        }

        /// <summary>
        /// Load and show banner ad
        /// </summary>
        private void LoadBannerAd()
        {
            if (!ShouldShowBannerAds())
                return;

            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(_bannerAdUnitId);
            
            Debug.Log("[MonetizationManager] Banner ad loaded and shown");
        }

        /// <summary>
        /// Hide banner ad
        /// </summary>
        public void HideBannerAd()
        {
            Advertisement.Banner.Hide();
        }

        /// <summary>
        /// Check if rewarded ad can be shown
        /// </summary>
        private bool CanShowRewardedAd()
        {
            if (!AdsEnabled || _isShowingAd) return false;
            if (_rewardedAdsWatchedThisSession >= _maxRewardedAdsPerSession) return false;
            if ((DateTime.Now - _lastRewardedAdTime).TotalSeconds < _rewardedAdCooldown) return false;
            
            return Advertisement.IsReady(_rewardedVideoAdUnitId);
        }

        /// <summary>
        /// Check if interstitial ad should be shown
        /// </summary>
        private bool ShouldShowInterstitialAd()
        {
            if (!AdsEnabled || _isShowingAd) return false;
            if ((DateTime.Now - _lastInterstitialTime).TotalSeconds < _interstitialCooldown) return false;
            if (_runsSinceLastInterstitial < _runsPerInterstitial) return false;
            
            return Advertisement.IsReady(_interstitialAdUnitId);
        }

        /// <summary>
        /// Check if banner ads should be shown
        /// </summary>
        private bool ShouldShowBannerAds()
        {
            return AdsEnabled; // Could add more logic here
        }
        #endregion

        #region Unity Ads Callbacks
        public void OnUnityAdsInitializationComplete()
        {
            Debug.Log("[MonetizationManager] Unity Ads initialization complete");
            
            // Load initial ads
            Advertisement.Load(_rewardedVideoAdUnitId, this);
            Advertisement.Load(_interstitialAdUnitId, this);
            
            // Complete initialization if IAP is disabled or also ready
            if (!_enableIAP || _storeController != null)
            {
                CompleteInitialization();
            }
        }

        public void OnUnityAdsInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[MonetizationManager] Unity Ads initialization failed: {error} - {message}");
            _adsEnabled = false;
        }

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            Debug.Log($"[MonetizationManager] Ad loaded: {adUnitId}");
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.LogError($"[MonetizationManager] Ad failed to load: {adUnitId} - {error}: {message}");
            _isShowingAd = false;
            
            // Track ad load failure
            if (_enableAnalytics)
            {
                _analytics.TrackAdLoadError(adUnitId, error.ToString());
            }
        }

        public void OnUnityAdsShowStart(string adUnitId)
        {
            Debug.Log($"[MonetizationManager] Ad show started: {adUnitId}");
            
            // Pause game during ad
            Time.timeScale = 0f;
        }

        public void OnUnityAdsShowClick(string adUnitId)
        {
            Debug.Log($"[MonetizationManager] Ad clicked: {adUnitId}");
            
            // Track ad clicks
            if (_enableAnalytics)
            {
                _analytics.TrackAdClick(adUnitId);
            }
        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"[MonetizationManager] Ad show complete: {adUnitId} - {showCompletionState}");
            
            // Resume game
            Time.timeScale = 1f;
            _isShowingAd = false;
            
            // Handle rewarded ad completion
            if (adUnitId == _rewardedVideoAdUnitId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                HandleRewardedAdCompleted();
            }
            
            // Handle interstitial completion
            if (adUnitId == _interstitialAdUnitId)
            {
                OnInterstitialAdShown?.Invoke();
            }
            
            // Reload the ad for next time
            Advertisement.Load(adUnitId, this);
            
            // Track completion
            if (_enableAnalytics)
            {
                _analytics.TrackAdComplete(adUnitId, showCompletionState.ToString());
            }
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[MonetizationManager] Ad show failed: {adUnitId} - {error}: {message}");
            
            Time.timeScale = 1f;
            _isShowingAd = false;
            
            // Track ad show failure
            if (_enableAnalytics)
            {
                _analytics.TrackAdShowError(adUnitId, error.ToString());
            }
        }

        /// <summary>
        /// Handle successful rewarded ad completion
        /// </summary>
        private void HandleRewardedAdCompleted()
        {
            _lastRewardedAdTime = DateTime.Now;
            _rewardedAdsWatchedThisSession++;
            
            int rewardAmount = CalculateRewardedAdReward(_currentRewardedAdContext);
            
            // Apply reward based on context
            ApplyRewardedAdReward(_currentRewardedAdContext, rewardAmount);
            
            OnRewardedAdCompleted?.Invoke(_currentRewardedAdContext, rewardAmount);
            
            Debug.Log($"[MonetizationManager] Rewarded ad completed - Context: {_currentRewardedAdContext}, Reward: {rewardAmount}");
        }

        /// <summary>
        /// Calculate reward amount for rewarded ad
        /// </summary>
        private int CalculateRewardedAdReward(string context)
        {
            switch (context.ToLower())
            {
                case "double_rewards":
                    return 2; // 2x multiplier
                case "energy_refill":
                    return 1; // 1 energy refill
                case "bonus_scrap":
                    return 200; // 200 bonus scrap
                case "premium_currency":
                    return 5; // 5 premium currency
                default:
                    return 100; // Default scrap amount
            }
        }

        /// <summary>
        /// Apply reward from rewarded ad
        /// </summary>
        private void ApplyRewardedAdReward(string context, int amount)
        {
            var resourceManager = Core.GameManager.Instance?.Resources;
            if (resourceManager == null) return;

            switch (context.ToLower())
            {
                case "double_rewards":
                    // Apply multiplier (would be handled by calling system)
                    break;
                case "energy_refill":
                    resourceManager.RefillEnergy();
                    break;
                case "bonus_scrap":
                case "scrap_reward":
                    resourceManager.AddScrap(amount);
                    break;
                case "premium_currency":
                    resourceManager.AddPremiumCurrency(amount);
                    break;
                default:
                    resourceManager.AddScrap(amount);
                    break;
            }
        }
        #endregion

        #region Unity IAP Implementation
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[MonetizationManager] Unity IAP initialization successful");
            
            _storeController = controller;
            _storeExtensionProvider = extensions;
            _iapEnabled = true;
            
            // Complete initialization if ads are also ready
            if (!_adsEnabled || Advertisement.isInitialized)
            {
                CompleteInitialization();
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[MonetizationManager] Unity IAP initialization failed: {error}");
            _iapEnabled = false;
            
            // Complete initialization with ads only
            if (!_adsEnabled || Advertisement.isInitialized)
            {
                CompleteInitialization();
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[MonetizationManager] Unity IAP initialization failed: {error} - {message}");
            OnInitializeFailed(error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;
            Debug.Log($"[MonetizationManager] Processing purchase: {product.definition.id}");
            
            try
            {
                ProcessPurchaseReward(product);
                
                OnPurchaseCompleted?.Invoke(product.definition.id, product.receipt);
                
                // Track purchase
                if (_enableAnalytics)
                {
                    _analytics.TrackPurchase(product.definition.id, (float)product.metadata.localizedPrice);
                }
                
                return PurchaseProcessingResult.Complete;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MonetizationManager] Error processing purchase: {e.Message}");
                return PurchaseProcessingResult.Pending;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"[MonetizationManager] Purchase failed: {product.definition.id} - {failureReason}");
            
            // Track purchase failure
            if (_enableAnalytics)
            {
                _analytics.TrackPurchaseFailure(product.definition.id, failureReason.ToString());
            }
        }

        /// <summary>
        /// Process purchase rewards
        /// </summary>
        private void ProcessPurchaseReward(Product product)
        {
            var resourceManager = Core.GameManager.Instance?.Resources;
            if (resourceManager == null) return;

            switch (product.definition.id)
            {
                case PREMIUM_CURRENCY_SMALL:
                    resourceManager.AddPremiumCurrency(100);
                    break;
                    
                case PREMIUM_CURRENCY_MEDIUM:
                    resourceManager.AddPremiumCurrency(500);
                    resourceManager.AddScrap(1000); // Bonus scrap
                    break;
                    
                case PREMIUM_CURRENCY_LARGE:
                    resourceManager.AddPremiumCurrency(1200);
                    resourceManager.AddScrap(2500); // Bonus scrap
                    resourceManager.AddDataCores(5); // Bonus data cores
                    break;
                    
                case PREMIUM_ACCOUNT_MONTH:
                    resourceManager.GrantPremiumAccount(TimeSpan.FromDays(30));
                    break;
                    
                case AD_REMOVAL:
                    HasAdRemoval = true;
                    resourceManager.GrantAdRemoval();
                    HideBannerAd();
                    break;
                    
                case STARTER_PACK:
                    resourceManager.AddScrap(2000);
                    resourceManager.AddDataCores(10);
                    resourceManager.AddPremiumCurrency(50);
                    // Could also unlock special parts
                    break;
            }
            
            Debug.Log($"[MonetizationManager] Purchase reward processed: {product.definition.id}");
        }

        /// <summary>
        /// Check for existing purchases on startup
        /// </summary>
        private void CheckExistingPurchases()
        {
            // Check for non-consumable purchases
            var adRemovalProduct = _storeController.products.WithID(AD_REMOVAL);
            if (adRemovalProduct.hasReceipt)
            {
                HasAdRemoval = true;
                HideBannerAd();
                Debug.Log("[MonetizationManager] Ad removal already purchased");
            }
            
            // Check for active subscriptions
            var premiumProduct = _storeController.products.WithID(PREMIUM_ACCOUNT_MONTH);
            if (premiumProduct.hasReceipt)
            {
                // Would need to validate subscription status
                Debug.Log("[MonetizationManager] Premium subscription found");
            }
        }
        #endregion

        #region Public Purchase Interface
        /// <summary>
        /// Initiate purchase of a product
        /// </summary>
        public void PurchaseProduct(string productId)
        {
            if (!_iapEnabled || _storeController == null)
            {
                Debug.LogError("[MonetizationManager] IAP not available");
                return;
            }

            var product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[MonetizationManager] Initiating purchase: {productId}");
                _storeController.InitiatePurchase(product);
                
                // Track purchase attempt
                if (_enableAnalytics)
                {
                    _analytics.TrackPurchaseAttempt(productId);
                }
            }
            else
            {
                Debug.LogError($"[MonetizationManager] Product not available for purchase: {productId}");
            }
        }

        /// <summary>
        /// Get product information for store UI
        /// </summary>
        public ProductInfo GetProductInfo(string productId)
        {
            if (_storeController == null) return null;

            var product = _storeController.products.WithID(productId);
            if (product == null) return null;

            return new ProductInfo
            {
                ProductId = product.definition.id,
                Title = product.metadata.localizedTitle,
                Description = product.metadata.localizedDescription,
                Price = product.metadata.localizedPriceString,
                IsAvailable = product.availableToPurchase
            };
        }

        /// <summary>
        /// Get all available products for store UI
        /// </summary>
        public List<ProductInfo> GetAllProducts()
        {
            var products = new List<ProductInfo>();
            
            if (_storeController != null)
            {
                foreach (var product in _storeController.products.all)
                {
                    var info = GetProductInfo(product.definition.id);
                    if (info != null)
                    {
                        products.Add(info);
                    }
                }
            }
            
            return products;
        }
        #endregion

        #region Smart Monetization
        /// <summary>
        /// Show rewarded ad prompt for double rewards
        /// </summary>
        public void ShowRewardedAdPrompt(int baseScrap, int baseXP)
        {
            if (!CanShowRewardedAd()) return;

            // Calculate potential reward
            int doubleScrap = baseScrap * 2;
            int doubleXP = baseXP * 2;
            
            // Show UI prompt (would be handled by UI system)
            Debug.Log($"[MonetizationManager] Offering double rewards: {doubleScrap} scrap, {doubleXP} XP");
            
            // For MVP, auto-show the ad (in real game, this would be a UI prompt)
            ShowRewardedAd("double_rewards");
        }

        /// <summary>
        /// Show energy purchase prompt when energy depleted
        /// </summary>
        public void ShowEnergyPurchasePrompt()
        {
            if (!CanShowRewardedAd()) return;

            Debug.Log("[MonetizationManager] Offering energy refill via rewarded ad");
            
            // Show energy refill ad option
            ShowRewardedAd("energy_refill");
        }

        /// <summary>
        /// Show premium currency offer at strategic moments
        /// </summary>
        public void ShowPremiumCurrencyOffer(string context)
        {
            if (!_iapEnabled) return;

            Debug.Log($"[MonetizationManager] Showing premium currency offer for: {context}");
            
            // Track offer shown
            if (_enableAnalytics)
            {
                _analytics.TrackOfferShown("premium_currency", context);
            }
            
            // Would show store UI
        }

        /// <summary>
        /// Check if player should see monetization prompts
        /// </summary>
        public bool ShouldShowMonetizationPrompts()
        {
            // Don't show prompts too frequently
            var timeSinceLastAd = DateTime.Now - _lastRewardedAdTime;
            if (timeSinceLastAd.TotalMinutes < 2) return false;
            
            // Don't overwhelm new players
            if (_sessionRunCount < 3) return false;
            
            return true;
        }
        #endregion

        #region Session Tracking
        /// <summary>
        /// Update session tracking data
        /// </summary>
        private void UpdateSessionTracking()
        {
            // Track session metrics for analytics
            if (_enableAnalytics && Time.time % 60f < Time.deltaTime) // Every minute
            {
                var sessionDuration = (DateTime.Now - _sessionStartTime).TotalMinutes;
                _analytics.UpdateSessionMetrics((float)sessionDuration, _sessionRunCount, _sessionRevenue);
            }
        }

        /// <summary>
        /// Track run completion for monetization decisions
        /// </summary>
        public void OnRunCompleted()
        {
            _sessionRunCount++;
            _runsSinceLastInterstitial++;
            
            // Show interstitial if appropriate
            if (ShouldShowInterstitialAd())
            {
                // Delay slightly to avoid interrupting results
                StartCoroutine(ShowInterstitialDelayed(2f));
            }
            
            Debug.Log($"[MonetizationManager] Run completed - Session runs: {_sessionRunCount}, Runs since interstitial: {_runsSinceLastInterstitial}");
        }

        /// <summary>
        /// Show interstitial ad with delay
        /// </summary>
        private IEnumerator ShowInterstitialDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowInterstitialAd();
        }
        #endregion

        #region Data Persistence
        /// <summary>
        /// Save monetization data to persistent storage
        /// </summary>
        private void SaveMonetizationData()
        {
            var data = new MonetizationSaveData
            {
                HasAdRemoval = HasAdRemoval,
                LastInterstitialTime = _lastInterstitialTime,
                LastRewardedAdTime = _lastRewardedAdTime,
                RunsSinceLastInterstitial = _runsSinceLastInterstitial,
                RewardedAdsWatchedThisSession = _rewardedAdsWatchedThisSession,
                SessionRevenue = _sessionRevenue
            };
            
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("CircuitRunners_Monetization", json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load monetization data from persistent storage
        /// </summary>
        private void LoadMonetizationData()
        {
            string json = PlayerPrefs.GetString("CircuitRunners_Monetization", "");
            
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var data = JsonUtility.FromJson<MonetizationSaveData>(json);
                    
                    HasAdRemoval = data.HasAdRemoval;
                    _lastInterstitialTime = data.LastInterstitialTime;
                    _lastRewardedAdTime = data.LastRewardedAdTime;
                    _runsSinceLastInterstitial = data.RunsSinceLastInterstitial;
                    
                    // Reset session-specific data
                    _rewardedAdsWatchedThisSession = 0;
                    _sessionRevenue = 0f;
                    
                    Debug.Log("[MonetizationManager] Monetization data loaded");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MonetizationManager] Failed to load monetization data: {e.Message}");
                }
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get monetization statistics for analytics
        /// </summary>
        public MonetizationStats GetMonetizationStats()
        {
            return new MonetizationStats
            {
                SessionDuration = (DateTime.Now - _sessionStartTime).TotalMinutes,
                SessionRunCount = _sessionRunCount,
                SessionRevenue = _sessionRevenue,
                RewardedAdsWatched = _rewardedAdsWatchedThisSession,
                InterstitialsShown = _sessionAdCounts.ContainsKey("interstitial") ? _sessionAdCounts["interstitial"] : 0,
                HasAdRemoval = HasAdRemoval,
                IsPremiumActive = Core.GameManager.Instance?.Resources?.HasPremiumAccount ?? false
            };
        }

        /// <summary>
        /// Force show store for testing
        /// </summary>
        public void ShowStore()
        {
            Debug.Log("[MonetizationManager] Showing store interface");
            // Would open store UI
        }

        /// <summary>
        /// Restore purchases (for iOS requirement)
        /// </summary>
        public void RestorePurchases()
        {
            if (_storeController != null)
            {
                Debug.Log("[MonetizationManager] Restoring purchases");
                _storeExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(OnTransactionsRestored);
            }
        }

        private void OnTransactionsRestored(bool success)
        {
            Debug.Log($"[MonetizationManager] Transactions restored: {success}");
            
            if (success)
            {
                CheckExistingPurchases();
            }
        }
        #endregion
    }
}