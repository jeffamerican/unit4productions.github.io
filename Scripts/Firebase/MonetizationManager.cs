using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using Firebase.Firestore;
using Firebase.Extensions;
using CircuitRunners.Firebase.DataModels;

namespace CircuitRunners.Firebase
{
    /// <summary>
    /// Comprehensive Monetization Manager for Circuit Runners
    /// Handles in-app purchases, receipt validation, premium currency, subscriptions, and ad monetization
    /// Includes anti-fraud measures, purchase restoration, and revenue analytics
    /// Optimized for free-to-play mobile gaming with multiple revenue streams
    /// </summary>
    public class MonetizationManager : MonoBehaviour, IStoreListener
    {
        #region Events and Delegates
        
        /// <summary>
        /// Monetization events for UI updates and analytics
        /// </summary>
        public static event Action<PurchaseResult> OnPurchaseCompleted;
        public static event Action<string, string> OnPurchaseFailed;
        public static event Action<List<Product>> OnProductsLoaded;
        public static event Action<string> OnMonetizationError;
        public static event Action<AdResult> OnAdCompleted;
        public static event Action<SubscriptionStatus> OnSubscriptionStatusChanged;
        public static event Action<CurrencyTransaction> OnCurrencyTransactionCompleted;
        
        /// <summary>
        /// Purchase result structure
        /// </summary>
        public struct PurchaseResult
        {
            public string productId;
            public string transactionId;
            public float price;
            public string currency;
            public Dictionary<string, object> rewards;
            public DateTime purchaseTime;
            public bool wasRestored;
        }
        
        /// <summary>
        /// Ad completion result
        /// </summary>
        public struct AdResult
        {
            public string adType;
            public string adPlacement;
            public bool completed;
            public Dictionary<string, object> rewards;
            public float estimatedRevenue;
        }
        
        /// <summary>
        /// Subscription status information
        /// </summary>
        public struct SubscriptionStatus
        {
            public string productId;
            public bool isActive;
            public DateTime expirationDate;
            public DateTime purchaseDate;
            public bool isInTrialPeriod;
            public bool willRenew;
        }
        
        /// <summary>
        /// Currency transaction record
        /// </summary>
        public struct CurrencyTransaction
        {
            public string transactionId;
            public string currencyType;
            public long amount;
            public string source;
            public string reason;
            public DateTime timestamp;
        }
        
        #endregion
        
        #region Private Fields
        
        [Header("Store Configuration")]
        [SerializeField] private bool enableInAppPurchases = true;
        [SerializeField] private bool enableReceiptValidation = true;
        [SerializeField] private bool enablePurchaseRestoration = true;
        [SerializeField] private bool enableSubscriptions = true;
        
        [Header("Product Configuration")]
        [SerializeField] private List<ProductConfig> products = new List<ProductConfig>();
        [SerializeField] private List<SubscriptionConfig> subscriptions = new List<SubscriptionConfig>();
        [SerializeField] private List<CurrencyPackConfig> currencyPacks = new List<CurrencyPackConfig>();
        
        [Header("Currency Settings")]
        [SerializeField] private bool enableMultipleCurrencies = true;
        [SerializeField] private int maxDailyFreeCurrency = 500;
        [SerializeField] private float currencyExchangeRate = 1.0f;
        [SerializeField] private bool enableCurrencyCapLimits = true;
        
        [Header("Ad Monetization")]
        [SerializeField] private bool enableRewardedAds = true;
        [SerializeField] private bool enableInterstitialAds = true;
        [SerializeField] private bool enableBannerAds = false; // Often not worth the UX cost
        [SerializeField] private int minTimeBetweenAds = 30; // seconds
        [SerializeField] private float adRewardMultiplier = 1.0f;
        
        [Header("Anti-Fraud Settings")]
        [SerializeField] private bool enableFraudDetection = true;
        [SerializeField] private int maxPurchasesPerHour = 10;
        [SerializeField] private float maxSpendingPerDay = 1000f; // USD
        [SerializeField] private bool enablePurchaseThrottling = true;
        
        [Header("Analytics Integration")]
        [SerializeField] private bool trackPurchaseEvents = true;
        [SerializeField] private bool trackCurrencyEvents = true;
        [SerializeField] private bool trackAdEvents = true;
        [SerializeField] private bool enableLTVTracking = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool simulatePurchaseFailures = false;
        [SerializeField] private bool bypassReceiptValidation = false; // For testing only
        
        // Store and purchasing
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private bool _isStoreInitialized;
        
        // System references
        private FirebaseAuthManager _authManager;
        private FirestoreManager _firestoreManager;
        private AnalyticsManager _analyticsManager;
        
        // Purchase tracking
        private Dictionary<string, Product> _availableProducts;
        private Dictionary<string, DateTime> _lastPurchaseTimes;
        private Dictionary<string, int> _purchaseCountsToday;
        private Queue<string> _pendingPurchases;
        
        // Currency management
        private Dictionary<string, long> _currencyBalances;
        private List<CurrencyTransaction> _pendingTransactions;
        private DateTime _lastDailyBonus;
        
        // Subscription tracking
        private Dictionary<string, SubscriptionStatus> _subscriptionStatuses;
        private DateTime _lastSubscriptionCheck;
        
        // Ad tracking
        private DateTime _lastAdShown;
        private Dictionary<string, int> _adImpressionsToday;
        private float _estimatedAdRevenue;
        
        // Fraud detection
        private List<DateTime> _recentPurchases;
        private float _totalSpendingToday;
        private Dictionary<string, int> _suspiciousActivity;
        
        // Performance tracking
        private int _totalPurchases;
        private float _totalRevenue;
        private DateTime _monetizationStartTime;
        
        #endregion
        
        #region Product Configuration Classes
        
        [Serializable]
        public class ProductConfig
        {
            public string productId;
            public ProductType productType;
            public string displayName;
            public string description;
            public Dictionary<string, int> rewards = new Dictionary<string, int>();
            public bool isConsumable = true;
        }
        
        [Serializable]
        public class SubscriptionConfig
        {
            public string productId;
            public string displayName;
            public string description;
            public Dictionary<string, int> monthlyRewards = new Dictionary<string, int>();
            public Dictionary<string, object> benefits = new Dictionary<string, object>();
        }
        
        [Serializable]
        public class CurrencyPackConfig
        {
            public string productId;
            public string currencyType;
            public int baseAmount;
            public int bonusAmount;
            public bool isStarterPack;
            public bool isLimitedTime;
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether the store is initialized and ready for purchases
        /// </summary>
        public bool IsStoreReady => _isStoreInitialized && _storeController != null;
        
        /// <summary>
        /// Available products for purchase
        /// </summary>
        public Dictionary<string, Product> AvailableProducts => 
            new Dictionary<string, Product>(_availableProducts ?? new Dictionary<string, Product>());
        
        /// <summary>
        /// Current currency balances
        /// </summary>
        public Dictionary<string, long> CurrencyBalances => 
            new Dictionary<string, long>(_currencyBalances);
        
        /// <summary>
        /// Active subscription statuses
        /// </summary>
        public Dictionary<string, SubscriptionStatus> SubscriptionStatuses => 
            new Dictionary<string, SubscriptionStatus>(_subscriptionStatuses);
        
        /// <summary>
        /// Total revenue tracked
        /// </summary>
        public float TotalRevenue => _totalRevenue;
        
        /// <summary>
        /// Total purchases made
        /// </summary>
        public int TotalPurchases => _totalPurchases;
        
        /// <summary>
        /// Whether user has made any purchases (affects player segmentation)
        /// </summary>
        public bool HasMadePurchase => _totalPurchases > 0;
        
        /// <summary>
        /// Estimated lifetime value based on current spending
        /// </summary>
        public float EstimatedLTV => CalculateEstimatedLTV();
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (FindObjectsOfType<MonetizationManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            // Initialize data structures
            _availableProducts = new Dictionary<string, Product>();
            _lastPurchaseTimes = new Dictionary<string, DateTime>();
            _purchaseCountsToday = new Dictionary<string, int>();
            _pendingPurchases = new Queue<string>();
            _currencyBalances = new Dictionary<string, long>();
            _pendingTransactions = new List<CurrencyTransaction>();
            _subscriptionStatuses = new Dictionary<string, SubscriptionStatus>();
            _adImpressionsToday = new Dictionary<string, int>();
            _recentPurchases = new List<DateTime>();
            _suspiciousActivity = new Dictionary<string, int>();
            
            _monetizationStartTime = DateTime.UtcNow;
            
            // Initialize default currency balances
            InitializeDefaultCurrencies();
            
            // Set up default product configurations if none provided
            if (products.Count == 0)
            {
                SetupDefaultProducts();
            }
        }
        
        private void Start()
        {
            // Get system references
            _authManager = FindObjectOfType<FirebaseAuthManager>();
            _firestoreManager = FindObjectOfType<FirestoreManager>();
            _analyticsManager = FindObjectOfType<AnalyticsManager>();
            
            // Initialize monetization system
            InitializeMonetization();
            
            // Set up authentication event handlers
            if (_authManager != null)
            {
                FirebaseAuthManager.OnAuthenticationStateChanged += HandleAuthenticationChanged;
            }
        }
        
        private void Update()
        {
            // Process pending purchases
            ProcessPendingPurchases();
            
            // Update subscription statuses periodically
            CheckSubscriptionStatuses();
            
            // Clean up old fraud detection data
            CleanupFraudDetectionData();
        }
        
        private void OnDestroy()
        {
            // Clean up event handlers
            if (_authManager != null)
            {
                FirebaseAuthManager.OnAuthenticationStateChanged -= HandleAuthenticationChanged;
            }
            
            // Save pending transactions
            SavePendingTransactions();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the complete monetization system
        /// Sets up Unity IAP, loads products, and configures payment processing
        /// </summary>
        private async void InitializeMonetization()
        {
            try
            {
                LogDebug("Initializing Monetization Manager...");
                
                // Load saved currency balances and purchase history
                await LoadMonetizationData();
                
                // Initialize Unity IAP if enabled
                if (enableInAppPurchases)
                {
                    InitializeUnityIAP();
                }
                
                // Load subscription statuses
                if (enableSubscriptions)
                {
                    await LoadSubscriptionStatuses();
                }
                
                // Initialize ad system (placeholder - actual implementation depends on ad SDK)
                if (enableRewardedAds || enableInterstitialAds)
                {
                    InitializeAdSystem();
                }
                
                LogDebug("Monetization Manager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Monetization initialization failed: {ex.Message}");
                OnMonetizationError?.Invoke($"Monetization initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize Unity IAP system with product catalog
        /// </summary>
        private void InitializeUnityIAP()
        {
            try
            {
                LogDebug("Initializing Unity IAP...");
                
                if (IsStoreReady)
                {
                    LogDebug("Store already initialized");
                    return;
                }
                
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                
                // Add consumable products
                foreach (var product in products)
                {
                    builder.AddProduct(product.productId, product.productType);
                    LogDebug($"Added product: {product.productId} ({product.productType})");
                }
                
                // Add subscription products
                foreach (var subscription in subscriptions)
                {
                    builder.AddProduct(subscription.productId, ProductType.Subscription);
                    LogDebug($"Added subscription: {subscription.productId}");
                }
                
                // Initialize purchasing
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                LogError($"Unity IAP initialization error: {ex.Message}");
                OnMonetizationError?.Invoke($"Store initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize default currency balances
        /// </summary>
        private void InitializeDefaultCurrencies()
        {
            _currencyBalances["coins"] = 1000;    // Starting coins
            _currencyBalances["gems"] = 50;       // Starting premium currency
            _currencyBalances["energy"] = 100;    // Starting energy
        }
        
        /// <summary>
        /// Set up default product configurations
        /// </summary>
        private void SetupDefaultProducts()
        {
            products = new List<ProductConfig>
            {
                new ProductConfig
                {
                    productId = "coins_small",
                    productType = ProductType.Consumable,
                    displayName = "Coin Pack (Small)",
                    description = "1,000 coins",
                    rewards = new Dictionary<string, int> { ["coins"] = 1000 }
                },
                new ProductConfig
                {
                    productId = "coins_medium",
                    productType = ProductType.Consumable,
                    displayName = "Coin Pack (Medium)",
                    description = "5,000 coins + 500 bonus",
                    rewards = new Dictionary<string, int> { ["coins"] = 5500 }
                },
                new ProductConfig
                {
                    productId = "gems_small",
                    productType = ProductType.Consumable,
                    displayName = "Gem Pack (Small)",
                    description = "100 gems",
                    rewards = new Dictionary<string, int> { ["gems"] = 100 }
                },
                new ProductConfig
                {
                    productId = "starter_pack",
                    productType = ProductType.Consumable,
                    displayName = "Starter Pack",
                    description = "5,000 coins + 200 gems + exclusive bot",
                    rewards = new Dictionary<string, int> { ["coins"] = 5000, ["gems"] = 200 }
                }
            };
            
            subscriptions = new List<SubscriptionConfig>
            {
                new SubscriptionConfig
                {
                    productId = "premium_pass",
                    displayName = "Premium Pass",
                    description = "Monthly premium benefits",
                    monthlyRewards = new Dictionary<string, int> { ["gems"] = 500, ["energy"] = 1000 }
                }
            };
            
            LogDebug("Default product configurations created");
        }
        
        /// <summary>
        /// Initialize ad system (placeholder for actual ad SDK integration)
        /// </summary>
        private void InitializeAdSystem()
        {
            try
            {
                LogDebug("Initializing Ad System...");
                
                // Actual ad SDK initialization would go here
                // Example integrations:
                // - Unity Ads
                // - AdMob
                // - IronSource
                // - AppLovin MAX
                
                LogDebug("Ad System initialized");
            }
            catch (Exception ex)
            {
                LogError($"Ad system initialization error: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Unity IAP Implementation
        
        /// <summary>
        /// Called when Unity IAP initialization is complete
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            try
            {
                LogDebug("Unity IAP initialized successfully");
                
                _storeController = controller;
                _extensionProvider = extensions;
                _isStoreInitialized = true;
                
                // Load available products
                LoadAvailableProducts();
                
                // Restore previous purchases if enabled
                if (enablePurchaseRestoration)
                {
                    RestorePurchases();
                }
                
                OnProductsLoaded?.Invoke(new List<Product>(_availableProducts.Values));
            }
            catch (Exception ex)
            {
                LogError($"Error in OnInitialized: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Called when Unity IAP initialization fails
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LogError($"Unity IAP initialization failed: {error}");
            OnMonetizationError?.Invoke($"Store initialization failed: {error}");
        }
        
        /// <summary>
        /// Called when a purchase is successful
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            try
            {
                var product = args.purchasedProduct;
                LogDebug($"Purchase successful: {product.definition.id}");
                
                // Validate receipt if enabled
                if (enableReceiptValidation && !bypassReceiptValidation)
                {
                    if (!ValidatePurchaseReceipt(product))
                    {
                        LogError($"Receipt validation failed for: {product.definition.id}");
                        OnPurchaseFailed?.Invoke(product.definition.id, "Receipt validation failed");
                        return PurchaseProcessingResult.Pending; // Don't complete the purchase
                    }
                }
                
                // Process the purchase
                ProcessSuccessfulPurchase(product);
                
                return PurchaseProcessingResult.Complete;
            }
            catch (Exception ex)
            {
                LogError($"Error processing purchase: {ex.Message}");
                return PurchaseProcessingResult.Pending;
            }
        }
        
        /// <summary>
        /// Called when a purchase fails
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            LogError($"Purchase failed: {product.definition.id} - {failureReason}");
            
            // Analytics tracking
            if (_analyticsManager != null && trackPurchaseEvents)
            {
                _analyticsManager.TrackMonetizationEvent("purchase_failed", product.definition.id, 0f, "USD", 
                    new Dictionary<string, object>
                    {
                        ["failure_reason"] = failureReason.ToString(),
                        ["product_type"] = product.definition.type.ToString()
                    });
            }
            
            OnPurchaseFailed?.Invoke(product.definition.id, failureReason.ToString());
        }
        
        #endregion
        
        #region Purchase Processing
        
        /// <summary>
        /// Initiate a purchase for the specified product
        /// </summary>
        public void PurchaseProduct(string productId)
        {
            if (!IsStoreReady)
            {
                LogWarning("Store not ready for purchases");
                OnPurchaseFailed?.Invoke(productId, "Store not initialized");
                return;
            }
            
            if (!_authManager?.IsAuthenticated == true)
            {
                LogWarning("User must be authenticated to make purchases");
                OnPurchaseFailed?.Invoke(productId, "Authentication required");
                return;
            }
            
            try
            {
                // Fraud detection checks
                if (!PassesFraudDetection(productId))
                {
                    LogWarning($"Purchase blocked by fraud detection: {productId}");
                    OnPurchaseFailed?.Invoke(productId, "Purchase temporarily unavailable");
                    return;
                }
                
                // Get product
                if (!_availableProducts.ContainsKey(productId))
                {
                    LogError($"Product not found: {productId}");
                    OnPurchaseFailed?.Invoke(productId, "Product not available");
                    return;
                }
                
                var product = _availableProducts[productId];
                
                LogDebug($"Initiating purchase: {productId} - ${product.metadata.localizedPrice}");
                
                // Track purchase attempt
                if (_analyticsManager != null && trackPurchaseEvents)
                {
                    _analyticsManager.TrackMonetizationEvent("purchase_initiated", productId, 
                        (float)product.metadata.localizedPrice, product.metadata.isoCurrencyCode,
                        new Dictionary<string, object>
                        {
                            ["product_type"] = product.definition.type.ToString()
                        });
                }
                
                // Initiate purchase
                _storeController.InitiatePurchase(product);
            }
            catch (Exception ex)
            {
                LogError($"Error initiating purchase: {ex.Message}");
                OnPurchaseFailed?.Invoke(productId, ex.Message);
            }
        }
        
        /// <summary>
        /// Process a successful purchase and grant rewards
        /// </summary>
        private async void ProcessSuccessfulPurchase(Product product)
        {
            try
            {
                var productId = product.definition.id;
                var price = (float)product.metadata.localizedPrice;
                var currency = product.metadata.isoCurrencyCode;
                
                LogDebug($"Processing successful purchase: {productId}");
                
                // Find product configuration
                var productConfig = products.Find(p => p.productId == productId);
                var subscriptionConfig = subscriptions.Find(s => s.productId == productId);
                
                var rewards = new Dictionary<string, object>();
                
                // Grant rewards based on product type
                if (productConfig != null)
                {
                    // Regular product rewards
                    foreach (var reward in productConfig.rewards)
                    {
                        await AddCurrency(reward.Key, reward.Value, $"purchase_{productId}");
                        rewards[reward.Key] = reward.Value;
                    }
                }
                else if (subscriptionConfig != null)
                {
                    // Subscription activation
                    await ActivateSubscription(productId, subscriptionConfig);
                    rewards["subscription"] = productId;
                }
                
                // Update purchase tracking
                _totalPurchases++;
                _totalRevenue += price;
                _lastPurchaseTimes[productId] = DateTime.UtcNow;
                
                if (_purchaseCountsToday.ContainsKey(productId))
                    _purchaseCountsToday[productId]++;
                else
                    _purchaseCountsToday[productId] = 1;
                
                _recentPurchases.Add(DateTime.UtcNow);
                _totalSpendingToday += price;
                
                // Create purchase result
                var result = new PurchaseResult
                {
                    productId = productId,
                    transactionId = product.transactionID,
                    price = price,
                    currency = currency,
                    rewards = rewards,
                    purchaseTime = DateTime.UtcNow,
                    wasRestored = false
                };
                
                // Save purchase data
                await SavePurchaseData(result);
                
                // Analytics tracking
                if (_analyticsManager != null && trackPurchaseEvents)
                {
                    _analyticsManager.TrackMonetizationEvent("purchase_completed", productId, price, currency,
                        new Dictionary<string, object>
                        {
                            ["transaction_id"] = product.transactionID,
                            ["product_type"] = product.definition.type.ToString(),
                            ["total_purchases"] = _totalPurchases,
                            ["total_revenue"] = _totalRevenue
                        });
                }
                
                LogDebug($"Purchase processed successfully: {productId} - ${price}");
                OnPurchaseCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                LogError($"Error processing successful purchase: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validate purchase receipt for fraud prevention
        /// </summary>
        private bool ValidatePurchaseReceipt(Product product)
        {
            try
            {
                // Basic validation - in production, validate with server
                if (string.IsNullOrEmpty(product.receipt))
                {
                    LogWarning($"Empty receipt for product: {product.definition.id}");
                    return false;
                }
                
                // Parse receipt data
                var receiptData = JsonUtility.FromJson<Receipt>(product.receipt);
                
                if (receiptData == null)
                {
                    LogWarning($"Invalid receipt format: {product.definition.id}");
                    return false;
                }
                
                // Additional server-side validation would be performed here
                // using Firebase Cloud Functions or similar service
                
                LogDebug($"Receipt validated for: {product.definition.id}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Receipt validation error: {ex.Message}");
                return false;
            }
        }
        
        [Serializable]
        private class Receipt
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }
        
        #endregion
        
        #region Currency Management
        
        /// <summary>
        /// Add currency to player's balance with transaction tracking
        /// </summary>
        public async Task<bool> AddCurrency(string currencyType, long amount, string source, string reason = "")
        {
            if (amount <= 0) return false;
            
            try
            {
                // Check currency caps if enabled
                if (enableCurrencyCapLimits && !IsWithinCurrencyLimits(currencyType, amount))
                {
                    LogWarning($"Currency addition would exceed limits: {currencyType} +{amount}");
                    return false;
                }
                
                // Update balance
                if (!_currencyBalances.ContainsKey(currencyType))
                    _currencyBalances[currencyType] = 0;
                
                _currencyBalances[currencyType] += amount;
                
                // Create transaction record
                var transaction = new CurrencyTransaction
                {
                    transactionId = Guid.NewGuid().ToString(),
                    currencyType = currencyType,
                    amount = amount,
                    source = source,
                    reason = reason,
                    timestamp = DateTime.UtcNow
                };
                
                _pendingTransactions.Add(transaction);
                
                // Save to database
                await SaveCurrencyTransaction(transaction);
                
                // Analytics tracking
                if (_analyticsManager != null && trackCurrencyEvents)
                {
                    _analyticsManager.TrackEvent("currency_gained", new Dictionary<string, object>
                    {
                        ["currency_type"] = currencyType,
                        ["amount"] = amount,
                        ["source"] = source,
                        ["reason"] = reason,
                        ["new_balance"] = _currencyBalances[currencyType]
                    });
                }
                
                OnCurrencyTransactionCompleted?.Invoke(transaction);
                LogDebug($"Added {amount} {currencyType} from {source}. New balance: {_currencyBalances[currencyType]}");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error adding currency: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Spend currency with validation and tracking
        /// </summary>
        public async Task<bool> SpendCurrency(string currencyType, long amount, string purpose, string reason = "")
        {
            if (amount <= 0) return false;
            
            try
            {
                // Check if player has enough currency
                if (!_currencyBalances.ContainsKey(currencyType) || _currencyBalances[currencyType] < amount)
                {
                    LogWarning($"Insufficient {currencyType}: need {amount}, have {_currencyBalances.GetValueOrDefault(currencyType, 0)}");
                    return false;
                }
                
                // Deduct currency
                _currencyBalances[currencyType] -= amount;
                
                // Create transaction record
                var transaction = new CurrencyTransaction
                {
                    transactionId = Guid.NewGuid().ToString(),
                    currencyType = currencyType,
                    amount = -amount, // Negative for spending
                    source = purpose,
                    reason = reason,
                    timestamp = DateTime.UtcNow
                };
                
                _pendingTransactions.Add(transaction);
                
                // Save to database
                await SaveCurrencyTransaction(transaction);
                
                // Analytics tracking
                if (_analyticsManager != null && trackCurrencyEvents)
                {
                    _analyticsManager.TrackEvent("currency_spent", new Dictionary<string, object>
                    {
                        ["currency_type"] = currencyType,
                        ["amount"] = amount,
                        ["purpose"] = purpose,
                        ["reason"] = reason,
                        ["remaining_balance"] = _currencyBalances[currencyType]
                    });
                }
                
                OnCurrencyTransactionCompleted?.Invoke(transaction);
                LogDebug($"Spent {amount} {currencyType} on {purpose}. Remaining: {_currencyBalances[currencyType]}");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error spending currency: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get currency balance for a specific type
        /// </summary>
        public long GetCurrencyBalance(string currencyType)
        {
            return _currencyBalances.GetValueOrDefault(currencyType, 0);
        }
        
        /// <summary>
        /// Check if currency addition is within limits
        /// </summary>
        private bool IsWithinCurrencyLimits(string currencyType, long amount)
        {
            // Define currency caps (could be loaded from Remote Config)
            var limits = new Dictionary<string, long>
            {
                ["coins"] = 999999999,
                ["gems"] = 99999,
                ["energy"] = 9999
            };
            
            if (limits.ContainsKey(currencyType))
            {
                var currentBalance = _currencyBalances.GetValueOrDefault(currencyType, 0);
                return currentBalance + amount <= limits[currencyType];
            }
            
            return true; // No limit defined
        }
        
        #endregion
        
        #region Ad Monetization
        
        /// <summary>
        /// Show rewarded ad with specified rewards
        /// </summary>
        public void ShowRewardedAd(string placement, Dictionary<string, object> rewards = null)
        {
            if (!enableRewardedAds)
            {
                LogWarning("Rewarded ads are disabled");
                return;
            }
            
            // Check cooldown
            var timeSinceLastAd = DateTime.UtcNow - _lastAdShown;
            if (timeSinceLastAd.TotalSeconds < minTimeBetweenAds)
            {
                LogWarning($"Ad cooldown active: {minTimeBetweenAds - timeSinceLastAd.TotalSeconds:F0}s remaining");
                return;
            }
            
            try
            {
                LogDebug($"Showing rewarded ad: {placement}");
                
                // Track ad attempt
                if (_analyticsManager != null && trackAdEvents)
                {
                    _analyticsManager.TrackMonetizationEvent("ad_requested", placement, 0f, "USD",
                        new Dictionary<string, object>
                        {
                            ["ad_type"] = "rewarded",
                            ["placement"] = placement
                        });
                }
                
                // Simulate ad completion (in real implementation, this would be handled by ad callbacks)
                SimulateAdCompletion(placement, "rewarded", rewards);
            }
            catch (Exception ex)
            {
                LogError($"Error showing rewarded ad: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Show interstitial ad
        /// </summary>
        public void ShowInterstitialAd(string placement)
        {
            if (!enableInterstitialAds)
            {
                LogWarning("Interstitial ads are disabled");
                return;
            }
            
            try
            {
                LogDebug($"Showing interstitial ad: {placement}");
                
                // Track ad impression
                if (_analyticsManager != null && trackAdEvents)
                {
                    _analyticsManager.TrackMonetizationEvent("ad_impression", placement, 0f, "USD",
                        new Dictionary<string, object>
                        {
                            ["ad_type"] = "interstitial",
                            ["placement"] = placement
                        });
                }
                
                // Update impression tracking
                if (_adImpressionsToday.ContainsKey("interstitial"))
                    _adImpressionsToday["interstitial"]++;
                else
                    _adImpressionsToday["interstitial"] = 1;
            }
            catch (Exception ex)
            {
                LogError($"Error showing interstitial ad: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Simulate ad completion (replace with actual ad SDK callbacks)
        /// </summary>
        private async void SimulateAdCompletion(string placement, string adType, Dictionary<string, object> rewards)
        {
            // Simulate ad duration
            await Task.Delay(3000);
            
            try
            {
                _lastAdShown = DateTime.UtcNow;
                
                // Update impression tracking
                if (_adImpressionsToday.ContainsKey(adType))
                    _adImpressionsToday[adType]++;
                else
                    _adImpressionsToday[adType] = 1;
                
                // Calculate estimated revenue (typical mobile game CPM rates)
                var estimatedRevenue = adType == "rewarded" ? 0.02f : 0.01f; // $0.02 for rewarded, $0.01 for interstitial
                _estimatedAdRevenue += estimatedRevenue;
                
                // Grant rewards for rewarded ads
                if (adType == "rewarded" && rewards != null)
                {
                    foreach (var reward in rewards)
                    {
                        if (reward.Value is int intValue)
                        {
                            await AddCurrency(reward.Key, intValue, $"ad_reward_{placement}");
                        }
                    }
                }
                
                // Create ad result
                var result = new AdResult
                {
                    adType = adType,
                    adPlacement = placement,
                    completed = true,
                    rewards = rewards ?? new Dictionary<string, object>(),
                    estimatedRevenue = estimatedRevenue
                };
                
                // Analytics tracking
                if (_analyticsManager != null && trackAdEvents)
                {
                    _analyticsManager.TrackMonetizationEvent("ad_completed", placement, estimatedRevenue, "USD",
                        new Dictionary<string, object>
                        {
                            ["ad_type"] = adType,
                            ["placement"] = placement,
                            ["rewards_granted"] = rewards?.Count ?? 0,
                            ["total_ad_revenue"] = _estimatedAdRevenue
                        });
                }
                
                OnAdCompleted?.Invoke(result);
                LogDebug($"Ad completed: {adType} - ${estimatedRevenue:F3}");
            }
            catch (Exception ex)
            {
                LogError($"Error processing ad completion: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Subscription Management
        
        /// <summary>
        /// Activate subscription and grant benefits
        /// </summary>
        private async Task ActivateSubscription(string productId, SubscriptionConfig config)
        {
            try
            {
                var status = new SubscriptionStatus
                {
                    productId = productId,
                    isActive = true,
                    purchaseDate = DateTime.UtcNow,
                    expirationDate = DateTime.UtcNow.AddDays(30), // Monthly subscription
                    isInTrialPeriod = false,
                    willRenew = true
                };
                
                _subscriptionStatuses[productId] = status;
                
                // Grant monthly rewards
                foreach (var reward in config.monthlyRewards)
                {
                    await AddCurrency(reward.Key, reward.Value, $"subscription_{productId}");
                }
                
                // Save subscription status
                await SaveSubscriptionStatus(status);
                
                OnSubscriptionStatusChanged?.Invoke(status);
                LogDebug($"Subscription activated: {productId}");
            }
            catch (Exception ex)
            {
                LogError($"Error activating subscription: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Check subscription statuses and handle expiration
        /// </summary>
        private void CheckSubscriptionStatuses()
        {
            if (DateTime.UtcNow - _lastSubscriptionCheck < TimeSpan.FromHours(1))
                return; // Check only once per hour
            
            try
            {
                _lastSubscriptionCheck = DateTime.UtcNow;
                
                foreach (var subscription in _subscriptionStatuses.ToList())
                {
                    var status = subscription.Value;
                    
                    if (status.isActive && DateTime.UtcNow > status.expirationDate)
                    {
                        // Subscription expired
                        status.isActive = false;
                        status.willRenew = false;
                        _subscriptionStatuses[subscription.Key] = status;
                        
                        OnSubscriptionStatusChanged?.Invoke(status);
                        LogDebug($"Subscription expired: {subscription.Key}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error checking subscription statuses: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Fraud Detection
        
        /// <summary>
        /// Check if purchase passes fraud detection rules
        /// </summary>
        private bool PassesFraudDetection(string productId)
        {
            if (!enableFraudDetection) return true;
            
            try
            {
                // Check purchase frequency
                if (_purchaseCountsToday.GetValueOrDefault(productId, 0) >= maxPurchasesPerHour)
                {
                    LogWarning($"Purchase frequency limit exceeded for: {productId}");
                    return false;
                }
                
                // Check daily spending limit
                var productPrice = GetProductPrice(productId);
                if (_totalSpendingToday + productPrice > maxSpendingPerDay)
                {
                    LogWarning($"Daily spending limit would be exceeded: ${_totalSpendingToday + productPrice}");
                    return false;
                }
                
                // Check for suspicious patterns
                if (_suspiciousActivity.GetValueOrDefault(productId, 0) > 5)
                {
                    LogWarning($"Suspicious activity detected for: {productId}");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in fraud detection: {ex.Message}");
                return false; // Err on the side of caution
            }
        }
        
        /// <summary>
        /// Clean up old fraud detection data
        /// </summary>
        private void CleanupFraudDetectionData()
        {
            // Clean up old purchase timestamps (keep only last 24 hours)
            var yesterday = DateTime.UtcNow.AddDays(-1);
            _recentPurchases.RemoveAll(p => p < yesterday);
            
            // Reset daily counters at midnight
            if (DateTime.UtcNow.Date > _monetizationStartTime.Date)
            {
                _purchaseCountsToday.Clear();
                _totalSpendingToday = 0f;
                _adImpressionsToday.Clear();
                _monetizationStartTime = DateTime.UtcNow;
                LogDebug("Daily monetization counters reset");
            }
        }
        
        #endregion
        
        #region Data Persistence
        
        /// <summary>
        /// Load monetization data from persistent storage
        /// </summary>
        private async Task LoadMonetizationData()
        {
            try
            {
                LogDebug("Loading monetization data...");
                
                // Load currency balances
                foreach (var currency in _currencyBalances.Keys.ToList())
                {
                    _currencyBalances[currency] = PlayerPrefs.GetInt($"Currency_{currency}", (int)_currencyBalances[currency]);
                }
                
                // Load purchase totals
                _totalPurchases = PlayerPrefs.GetInt("TotalPurchases", 0);
                _totalRevenue = PlayerPrefs.GetFloat("TotalRevenue", 0f);
                _estimatedAdRevenue = PlayerPrefs.GetFloat("EstimatedAdRevenue", 0f);
                
                // Load last daily bonus time
                var lastBonusString = PlayerPrefs.GetString("LastDailyBonus", "");
                if (!string.IsNullOrEmpty(lastBonusString) && DateTime.TryParse(lastBonusString, out var lastBonus))
                {
                    _lastDailyBonus = lastBonus;
                }
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug("Monetization data loaded");
            }
            catch (Exception ex)
            {
                LogError($"Error loading monetization data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save purchase data to database
        /// </summary>
        private async Task SavePurchaseData(PurchaseResult result)
        {
            try
            {
                if (_firestoreManager?.IsOnline == true && _authManager?.IsAuthenticated == true)
                {
                    var firestore = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                    var docRef = firestore.Collection("purchase_receipts").Document();
                    
                    var purchaseData = new Dictionary<string, object>
                    {
                        ["playerId"] = _authManager.UserId,
                        ["productId"] = result.productId,
                        ["transactionId"] = result.transactionId,
                        ["price"] = result.price,
                        ["currency"] = result.currency,
                        ["purchaseTime"] = Firebase.Firestore.Timestamp.FromDateTime(result.purchaseTime),
                        ["platform"] = Application.platform.ToString(),
                        ["rewards"] = result.rewards
                    };
                    
                    await docRef.SetAsync(purchaseData);
                }
                
                // Save to local storage
                PlayerPrefs.SetInt("TotalPurchases", _totalPurchases);
                PlayerPrefs.SetFloat("TotalRevenue", _totalRevenue);
                PlayerPrefs.Save();
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error saving purchase data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save currency transaction to database
        /// </summary>
        private async Task SaveCurrencyTransaction(CurrencyTransaction transaction)
        {
            try
            {
                // Save balance to local storage
                PlayerPrefs.SetInt($"Currency_{transaction.currencyType}", (int)_currencyBalances[transaction.currencyType]);
                PlayerPrefs.Save();
                
                // Save transaction to database if online
                if (_firestoreManager?.IsOnline == true && _authManager?.IsAuthenticated == true)
                {
                    var firestore = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                    var docRef = firestore.Collection("currency_transactions").Document();
                    
                    await docRef.SetAsync(transaction);
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error saving currency transaction: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save subscription status to database
        /// </summary>
        private async Task SaveSubscriptionStatus(SubscriptionStatus status)
        {
            try
            {
                if (_firestoreManager?.IsOnline == true && _authManager?.IsAuthenticated == true)
                {
                    var firestore = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                    var docRef = firestore.Collection("subscriptions").Document(status.productId);
                    
                    var subscriptionData = new Dictionary<string, object>
                    {
                        ["playerId"] = _authManager.UserId,
                        ["productId"] = status.productId,
                        ["isActive"] = status.isActive,
                        ["purchaseDate"] = Firebase.Firestore.Timestamp.FromDateTime(status.purchaseDate),
                        ["expirationDate"] = Firebase.Firestore.Timestamp.FromDateTime(status.expirationDate),
                        ["isInTrialPeriod"] = status.isInTrialPeriod,
                        ["willRenew"] = status.willRenew
                    };
                    
                    await docRef.SetAsync(subscriptionData);
                }
                
                await Task.Delay(1); // Prevent compiler warning
            }
            catch (Exception ex)
            {
                LogError($"Error saving subscription status: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save pending transactions
        /// </summary>
        private async void SavePendingTransactions()
        {
            try
            {
                foreach (var transaction in _pendingTransactions)
                {
                    await SaveCurrencyTransaction(transaction);
                }
                
                _pendingTransactions.Clear();
            }
            catch (Exception ex)
            {
                LogError($"Error saving pending transactions: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Load available products from store
        /// </summary>
        private void LoadAvailableProducts()
        {
            try
            {
                _availableProducts.Clear();
                
                foreach (var product in _storeController.products.all)
                {
                    _availableProducts[product.definition.id] = product;
                }
                
                LogDebug($"Loaded {_availableProducts.Count} products from store");
            }
            catch (Exception ex)
            {
                LogError($"Error loading available products: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Restore previous purchases
        /// </summary>
        private void RestorePurchases()
        {
            try
            {
                LogDebug("Restoring purchases...");
                
                // Platform-specific restore logic
                #if UNITY_IOS
                _extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result) =>
                {
                    if (result)
                    {
                        LogDebug("Purchase restoration successful");
                    }
                    else
                    {
                        LogWarning("Purchase restoration failed");
                    }
                });
                #elif UNITY_ANDROID
                // Google Play doesn't require explicit restore - purchases are automatically restored
                LogDebug("Android purchase restoration not required");
                #endif
            }
            catch (Exception ex)
            {
                LogError($"Error restoring purchases: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process pending purchases queue
        /// </summary>
        private void ProcessPendingPurchases()
        {
            if (_pendingPurchases.Count == 0) return;
            
            try
            {
                while (_pendingPurchases.Count > 0)
                {
                    var productId = _pendingPurchases.Dequeue();
                    PurchaseProduct(productId);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing pending purchases: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load subscription statuses from database
        /// </summary>
        private async Task LoadSubscriptionStatuses()
        {
            try
            {
                if (_firestoreManager?.IsOnline == true && _authManager?.IsAuthenticated == true)
                {
                    // Load from database
                    // Implementation would query user's subscription collection
                }
                
                await Task.Delay(1); // Prevent compiler warning
                LogDebug("Subscription statuses loaded");
            }
            catch (Exception ex)
            {
                LogError($"Error loading subscription statuses: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get product price from store
        /// </summary>
        private float GetProductPrice(string productId)
        {
            if (_availableProducts.ContainsKey(productId))
            {
                return (float)_availableProducts[productId].metadata.localizedPrice;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Calculate estimated lifetime value based on current data
        /// </summary>
        private float CalculateEstimatedLTV()
        {
            try
            {
                if (_totalPurchases == 0) return _estimatedAdRevenue;
                
                var daysSinceFirstPurchase = (DateTime.UtcNow - _monetizationStartTime).Days;
                if (daysSinceFirstPurchase <= 0) return _totalRevenue + _estimatedAdRevenue;
                
                var dailySpendingRate = _totalRevenue / daysSinceFirstPurchase;
                var projectedLifetimeDays = 365f; // 1 year projection
                
                return (dailySpendingRate * projectedLifetimeDays) + _estimatedAdRevenue;
            }
            catch (Exception ex)
            {
                LogError($"Error calculating LTV: {ex.Message}");
                return 0f;
            }
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
                    LogDebug("User authenticated - enabling monetization features");
                    // Load user-specific monetization data
                    _ = LoadMonetizationData();
                }
                else
                {
                    LogDebug("User signed out - saving monetization data");
                    SavePendingTransactions();
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
        /// Get monetization performance summary
        /// </summary>
        public string GetMonetizationSummary()
        {
            var summary = "Monetization Performance Summary:\n";
            summary += $"Total Revenue: ${_totalRevenue:F2}\n";
            summary += $"Total Purchases: {_totalPurchases}\n";
            summary += $"Ad Revenue: ${_estimatedAdRevenue:F2}\n";
            summary += $"Estimated LTV: ${EstimatedLTV:F2}\n";
            summary += $"Has Made Purchase: {HasMadePurchase}\n";
            summary += $"Active Subscriptions: {_subscriptionStatuses.Count(s => s.Value.isActive)}\n";
            
            summary += "\nCurrency Balances:\n";
            foreach (var currency in _currencyBalances)
            {
                summary += $"  {currency.Key}: {currency.Value:N0}\n";
            }
            
            return summary;
        }
        
        /// <summary>
        /// Grant daily bonus currency
        /// </summary>
        public async Task<bool> GrantDailyBonus()
        {
            try
            {
                var now = DateTime.UtcNow;
                var daysSinceLastBonus = (now.Date - _lastDailyBonus.Date).Days;
                
                if (daysSinceLastBonus >= 1)
                {
                    var bonusAmount = maxDailyFreeCurrency;
                    await AddCurrency("coins", bonusAmount, "daily_bonus", "Daily login reward");
                    
                    _lastDailyBonus = now;
                    PlayerPrefs.SetString("LastDailyBonus", now.ToString());
                    PlayerPrefs.Save();
                    
                    LogDebug($"Daily bonus granted: {bonusAmount} coins");
                    return true;
                }
                
                return false; // Already claimed today
            }
            catch (Exception ex)
            {
                LogError($"Error granting daily bonus: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Check if product is available for purchase
        /// </summary>
        public bool IsProductAvailable(string productId)
        {
            return IsStoreReady && _availableProducts.ContainsKey(productId);
        }
        
        /// <summary>
        /// Get product display information
        /// </summary>
        public Product GetProduct(string productId)
        {
            return _availableProducts.GetValueOrDefault(productId, null);
        }
        
        /// <summary>
        /// Check if user has active subscription
        /// </summary>
        public bool HasActiveSubscription(string productId = null)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return _subscriptionStatuses.Values.Any(s => s.isActive);
            }
            
            return _subscriptionStatuses.ContainsKey(productId) && _subscriptionStatuses[productId].isActive;
        }
        
        #endregion
        
        #region Debug and Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[MonetizationManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[MonetizationManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[MonetizationManager] {message}");
        }
        
        #endregion
    }
}