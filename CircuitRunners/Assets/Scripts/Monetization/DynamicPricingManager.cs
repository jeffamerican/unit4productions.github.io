using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CircuitRunners.Monetization;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Advanced dynamic pricing and A/B testing system for maximum conversion optimization
    /// Implements player-based pricing, regional optimization, and real-time test management
    /// Target: Increase conversion rates by 15-30% through personalized pricing strategies
    /// </summary>
    public class DynamicPricingManager : MonoBehaviour
    {
        #region Configuration
        [Header("Dynamic Pricing Settings")]
        [SerializeField] private bool enableDynamicPricing = true;
        [SerializeField] private bool enableRegionalPricing = true;
        [SerializeField] private bool enablePlayerSegmentPricing = true;
        [SerializeField] private float maxPriceAdjustment = 0.5f; // ±50% max adjustment
        
        [Header("A/B Testing Configuration")]
        [SerializeField] private bool enableABTesting = true;
        [SerializeField] private List<PricingABTest> activeTests;
        [SerializeField] private int minPlayersPerTestGroup = 50;
        [SerializeField] private int maxConcurrentTests = 5;
        
        [Header("Urgency Pricing")]
        [SerializeField] private bool enableUrgencyPricing = true;
        [SerializeField] private List<UrgencyPricingRule> urgencyRules;
        
        [Header("Personalization Settings")]
        [SerializeField] private bool enableBehavioralPricing = true;
        [SerializeField] private bool enableTimeBasedPricing = true;
        [SerializeField] private bool enableLTVBasedPricing = true;
        #endregion

        #region Private Fields
        private Dictionary<string, PersonalizedPrice> _personalizedPrices;
        private Dictionary<string, PricingABTest> _runningTests;
        private Dictionary<string, RegionalPricingData> _regionalPricing;
        private List<PricingDecisionLog> _pricingDecisions;
        private PlayerMonetizationProfile _currentPlayerProfile;
        private DateTime _lastPriceUpdate;
        #endregion

        #region Events
        public static event Action<PriceChangeEvent> OnPriceChanged;
        public static event Action<ABTestResult> OnABTestCompleted;
        public static event Action<PersonalizationMetrics> OnPersonalizationUpdated;
        #endregion

        #region Data Structures
        [System.Serializable]
        public struct PersonalizedPrice
        {
            public string productId;
            public float originalPrice;
            public float adjustedPrice;
            public float adjustmentFactor;
            public string adjustmentReason;
            public DateTime priceExpiry;
            public bool isTestPrice;
            public string testGroup;
        }

        [System.Serializable]
        public class PricingABTest
        {
            public string testId;
            public string testName;
            public string productId;
            public List<PriceVariant> priceVariants;
            public ABTestStatus status;
            public DateTime startDate;
            public DateTime endDate;
            public int minSampleSize;
            public ABTestMetrics metrics;
            public bool autoPromoteWinner;
        }

        [System.Serializable]
        public struct PriceVariant
        {
            public string variantId;
            public string variantName;
            public float priceMultiplier;
            public string displayStrategy; // "discount", "premium", "standard"
            public Color priceColor;
            public string priceText; // Custom price display text
        }

        [System.Serializable]
        public struct ABTestMetrics
        {
            public Dictionary<string, int> impressions;
            public Dictionary<string, int> conversions;
            public Dictionary<string, float> conversionRates;
            public Dictionary<string, float> revenue;
            public Dictionary<string, float> averageOrderValue;
            public float confidenceLevel;
            public string statisticalSignificance;
        }

        [System.Serializable]
        public class UrgencyPricingRule
        {
            public string ruleId;
            public UrgencyTrigger trigger;
            public float discountPercentage;
            public int durationMinutes;
            public string displayMessage;
            public List<string> applicableProducts;
            public int maxUsesPerPlayer;
            public bool requiresPlayerAction;
        }

        [System.Serializable]
        public struct RegionalPricingData
        {
            public string region;
            public string currency;
            public float purchasingPowerMultiplier;
            public float competitiveAdjustment;
            public Dictionary<string, float> productPriceMultipliers;
            public DateTime lastUpdated;
        }

        [System.Serializable]
        public struct PricingDecisionLog
        {
            public string playerId;
            public string productId;
            public float originalPrice;
            public float finalPrice;
            public string decisionReason;
            public DateTime timestamp;
            public bool converted;
            public string testGroup;
        }

        public enum ABTestStatus
        {
            Draft,
            Active,
            Completed,
            Paused,
            Cancelled
        }

        public enum UrgencyTrigger
        {
            SessionTime,
            FailedPurchaseAttempt,
            LastChance,
            CartAbandonment,
            CompetitorAction,
            SeasonalEvent
        }

        public struct PriceChangeEvent
        {
            public string productId;
            public float oldPrice;
            public float newPrice;
            public string reason;
            public DateTime timestamp;
        }

        public struct PersonalizationMetrics
        {
            public int totalPersonalizations;
            public float averagePriceAdjustment;
            public float conversionLift;
            public Dictionary<string, float> segmentPerformance;
            public float revenueImpact;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _personalizedPrices = new Dictionary<string, PersonalizedPrice>();
            _runningTests = new Dictionary<string, PricingABTest>();
            _regionalPricing = new Dictionary<string, RegionalPricingData>();
            _pricingDecisions = new List<PricingDecisionLog>();

            SetupDefaultUrgencyRules();
        }

        private void Start()
        {
            LoadPlayerProfile();
            LoadRegionalPricing();
            InitializeActiveTests();
            
            // Update pricing every 5 minutes
            InvokeRepeating(nameof(UpdateDynamicPricing), 0f, 300f);
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get personalized price for a specific product and player
        /// </summary>
        public PersonalizedPrice GetPersonalizedPrice(string productId, float basePrice)
        {
            if (!enableDynamicPricing)
            {
                return new PersonalizedPrice
                {
                    productId = productId,
                    originalPrice = basePrice,
                    adjustedPrice = basePrice,
                    adjustmentFactor = 1.0f,
                    adjustmentReason = "Dynamic pricing disabled"
                };
            }

            // Check if we have a cached personalized price
            if (_personalizedPrices.ContainsKey(productId))
            {
                var cachedPrice = _personalizedPrices[productId];
                if (cachedPrice.priceExpiry > DateTime.Now)
                {
                    return cachedPrice;
                }
            }

            // Calculate new personalized price
            var personalizedPrice = CalculatePersonalizedPrice(productId, basePrice);
            _personalizedPrices[productId] = personalizedPrice;

            // Log the pricing decision
            LogPricingDecision(personalizedPrice);

            return personalizedPrice;
        }

        /// <summary>
        /// Start new A/B pricing test
        /// </summary>
        public async Task<bool> StartABTest(PricingABTest test)
        {
            if (!enableABTesting || _runningTests.Count >= maxConcurrentTests)
            {
                Debug.LogWarning($"Cannot start A/B test: {test.testId}");
                return false;
            }

            try
            {
                test.status = ABTestStatus.Active;
                test.startDate = DateTime.Now;
                test.metrics = new ABTestMetrics
                {
                    impressions = new Dictionary<string, int>(),
                    conversions = new Dictionary<string, int>(),
                    conversionRates = new Dictionary<string, float>(),
                    revenue = new Dictionary<string, float>(),
                    averageOrderValue = new Dictionary<string, float>()
                };

                _runningTests[test.testId] = test;

                Debug.Log($"[DynamicPricingManager] Started A/B test: {test.testId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error starting A/B test: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Trigger urgency pricing based on player behavior
        /// </summary>
        public PersonalizedPrice TriggerUrgencyPricing(string productId, float basePrice, UrgencyTrigger trigger)
        {
            if (!enableUrgencyPricing)
                return GetPersonalizedPrice(productId, basePrice);

            var applicableRule = urgencyRules.Find(r => 
                r.trigger == trigger && 
                r.applicableProducts.Contains(productId));

            if (applicableRule == null)
                return GetPersonalizedPrice(productId, basePrice);

            var urgencyPrice = new PersonalizedPrice
            {
                productId = productId,
                originalPrice = basePrice,
                adjustedPrice = basePrice * (1 - applicableRule.discountPercentage / 100f),
                adjustmentFactor = 1 - applicableRule.discountPercentage / 100f,
                adjustmentReason = $"Urgency: {trigger}",
                priceExpiry = DateTime.Now.AddMinutes(applicableRule.durationMinutes),
                isTestPrice = false,
                testGroup = "urgency"
            };

            _personalizedPrices[productId] = urgencyPrice;
            
            OnPriceChanged?.Invoke(new PriceChangeEvent
            {
                productId = productId,
                oldPrice = basePrice,
                newPrice = urgencyPrice.adjustedPrice,
                reason = urgencyPrice.adjustmentReason,
                timestamp = DateTime.Now
            });

            return urgencyPrice;
        }

        /// <summary>
        /// Track pricing impression for A/B testing
        /// </summary>
        public void TrackPriceImpression(string productId, string testGroup)
        {
            var activeTest = _runningTests.Values.FirstOrDefault(t => 
                t.productId == productId && t.status == ABTestStatus.Active);

            if (activeTest != null)
            {
                if (!activeTest.metrics.impressions.ContainsKey(testGroup))
                    activeTest.metrics.impressions[testGroup] = 0;
                
                activeTest.metrics.impressions[testGroup]++;
                
                // Update test in dictionary
                _runningTests[activeTest.testId] = activeTest;
            }
        }

        /// <summary>
        /// Track pricing conversion for A/B testing
        /// </summary>
        public void TrackPriceConversion(string productId, string testGroup, float revenue)
        {
            var activeTest = _runningTests.Values.FirstOrDefault(t => 
                t.productId == productId && t.status == ABTestStatus.Active);

            if (activeTest != null)
            {
                // Update conversions
                if (!activeTest.metrics.conversions.ContainsKey(testGroup))
                    activeTest.metrics.conversions[testGroup] = 0;
                activeTest.metrics.conversions[testGroup]++;

                // Update revenue
                if (!activeTest.metrics.revenue.ContainsKey(testGroup))
                    activeTest.metrics.revenue[testGroup] = 0f;
                activeTest.metrics.revenue[testGroup] += revenue;

                // Calculate conversion rate
                var impressions = activeTest.metrics.impressions.GetValueOrDefault(testGroup, 0);
                if (impressions > 0)
                {
                    var conversionRate = (float)activeTest.metrics.conversions[testGroup] / impressions;
                    activeTest.metrics.conversionRates[testGroup] = conversionRate;
                }

                // Calculate average order value
                if (activeTest.metrics.conversions[testGroup] > 0)
                {
                    activeTest.metrics.averageOrderValue[testGroup] = 
                        activeTest.metrics.revenue[testGroup] / activeTest.metrics.conversions[testGroup];
                }

                _runningTests[activeTest.testId] = activeTest;

                // Check if test should be completed
                CheckABTestCompletion(activeTest);
            }
        }
        #endregion

        #region Core Pricing Logic
        /// <summary>
        /// Calculate personalized price based on multiple factors
        /// </summary>
        private PersonalizedPrice CalculatePersonalizedPrice(string productId, float basePrice)
        {
            float adjustmentFactor = 1.0f;
            string adjustmentReason = "Standard pricing";
            string testGroup = "control";

            // 1. Check for active A/B test
            var activeTest = GetActiveABTest(productId);
            if (activeTest != null)
            {
                var variant = AssignTestVariant(activeTest);
                adjustmentFactor *= variant.priceMultiplier;
                adjustmentReason = $"A/B Test: {activeTest.testName}";
                testGroup = variant.variantId;
            }
            else
            {
                // 2. Apply regional pricing
                if (enableRegionalPricing)
                {
                    var regionalAdjustment = GetRegionalPriceAdjustment(productId);
                    adjustmentFactor *= regionalAdjustment.multiplier;
                    if (regionalAdjustment.multiplier != 1.0f)
                    {
                        adjustmentReason += $" + Regional ({regionalAdjustment.reason})";
                    }
                }

                // 3. Apply player segment pricing
                if (enablePlayerSegmentPricing && _currentPlayerProfile != null)
                {
                    var segmentAdjustment = GetSegmentPriceAdjustment(productId);
                    adjustmentFactor *= segmentAdjustment.multiplier;
                    if (segmentAdjustment.multiplier != 1.0f)
                    {
                        adjustmentReason += $" + Segment ({segmentAdjustment.reason})";
                    }
                }

                // 4. Apply behavioral pricing
                if (enableBehavioralPricing)
                {
                    var behaviorAdjustment = GetBehavioralPriceAdjustment(productId);
                    adjustmentFactor *= behaviorAdjustment.multiplier;
                    if (behaviorAdjustment.multiplier != 1.0f)
                    {
                        adjustmentReason += $" + Behavior ({behaviorAdjustment.reason})";
                    }
                }

                // 5. Apply time-based pricing
                if (enableTimeBasedPricing)
                {
                    var timeAdjustment = GetTimeBasedPriceAdjustment();
                    adjustmentFactor *= timeAdjustment.multiplier;
                    if (timeAdjustment.multiplier != 1.0f)
                    {
                        adjustmentReason += $" + Time ({timeAdjustment.reason})";
                    }
                }
            }

            // Clamp adjustment factor to maximum allowed range
            adjustmentFactor = Mathf.Clamp(adjustmentFactor, 1 - maxPriceAdjustment, 1 + maxPriceAdjustment);

            var personalizedPrice = new PersonalizedPrice
            {
                productId = productId,
                originalPrice = basePrice,
                adjustedPrice = basePrice * adjustmentFactor,
                adjustmentFactor = adjustmentFactor,
                adjustmentReason = adjustmentReason,
                priceExpiry = DateTime.Now.AddHours(1), // Prices expire after 1 hour
                isTestPrice = activeTest != null,
                testGroup = testGroup
            };

            return personalizedPrice;
        }

        /// <summary>
        /// Get regional price adjustment for purchasing power parity
        /// </summary>
        private (float multiplier, string reason) GetRegionalPriceAdjustment(string productId)
        {
            var playerRegion = GetPlayerRegion();
            
            if (_regionalPricing.ContainsKey(playerRegion))
            {
                var regionalData = _regionalPricing[playerRegion];
                
                // Apply purchasing power adjustment
                float multiplier = regionalData.purchasingPowerMultiplier;
                
                // Apply product-specific adjustments
                if (regionalData.productPriceMultipliers.ContainsKey(productId))
                {
                    multiplier *= regionalData.productPriceMultipliers[productId];
                }
                
                return (multiplier, $"Regional PPP {playerRegion}");
            }

            return (1.0f, "No regional data");
        }

        /// <summary>
        /// Get player segment-based price adjustment
        /// </summary>
        private (float multiplier, string reason) GetSegmentPriceAdjustment(string productId)
        {
            if (_currentPlayerProfile == null)
                return (1.0f, "No profile");

            switch (_currentPlayerProfile.SpendingType)
            {
                case PlayerSpendingType.HighSpender:
                case PlayerSpendingType.Whale:
                    // High spenders can afford premium pricing
                    return (1.1f, "High value customer");
                
                case PlayerSpendingType.LowSpender:
                    // Encourage low spenders with discounts
                    return (0.9f, "Conversion incentive");
                
                case PlayerSpendingType.NonSpender:
                    // Aggressive discount for first-time buyers
                    return (0.8f, "First purchase incentive");
                
                default:
                    return (1.0f, "Standard segment");
            }
        }

        /// <summary>
        /// Get behavioral price adjustment based on player actions
        /// </summary>
        private (float multiplier, string reason) GetBehavioralPriceAdjustment(string productId)
        {
            // Analyze recent player behavior
            var sessionLength = (DateTime.Now - _currentPlayerProfile.FirstPurchaseDate).TotalMinutes;
            
            // Price sensitivity based on session engagement
            if (sessionLength > 30) // Highly engaged session
            {
                return (1.05f, "High engagement");
            }
            else if (sessionLength < 5) // Quick session, might be price sensitive
            {
                return (0.95f, "Quick session discount");
            }

            // Check for cart abandonment or failed purchase attempts
            // This would be tracked in the player profile
            if (_currentPlayerProfile.ChurnRisk > 0.7f)
            {
                return (0.85f, "Retention discount");
            }

            return (1.0f, "Standard behavior");
        }

        /// <summary>
        /// Get time-based price adjustment for peak/off-peak pricing
        /// </summary>
        private (float multiplier, string reason) GetTimeBasedPriceAdjustment()
        {
            var currentHour = DateTime.Now.Hour;
            var dayOfWeek = DateTime.Now.DayOfWeek;

            // Peak gaming hours (evening)
            if (currentHour >= 18 && currentHour <= 23)
            {
                return (1.02f, "Peak hours");
            }
            
            // Weekend premium
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                return (1.03f, "Weekend premium");
            }
            
            // Late night discount
            if (currentHour >= 0 && currentHour <= 6)
            {
                return (0.98f, "Late night discount");
            }

            return (1.0f, "Standard time");
        }
        #endregion

        #region A/B Testing Logic
        /// <summary>
        /// Get active A/B test for product
        /// </summary>
        private PricingABTest GetActiveABTest(string productId)
        {
            return _runningTests.Values.FirstOrDefault(t => 
                t.productId == productId && 
                t.status == ABTestStatus.Active);
        }

        /// <summary>
        /// Assign player to A/B test variant
        /// </summary>
        private PriceVariant AssignTestVariant(PricingABTest test)
        {
            // Use player ID hash for consistent assignment
            var playerId = GetCurrentPlayerId();
            var hash = playerId.GetHashCode();
            var variantIndex = Math.Abs(hash) % test.priceVariants.Count;
            
            return test.priceVariants[variantIndex];
        }

        /// <summary>
        /// Check if A/B test should be completed
        /// </summary>
        private void CheckABTestCompletion(PricingABTest test)
        {
            var totalImpressions = test.metrics.impressions.Values.Sum();
            
            // Check minimum sample size
            if (totalImpressions >= test.minSampleSize)
            {
                // Calculate statistical significance
                var significance = CalculateStatisticalSignificance(test);
                test.metrics.confidenceLevel = significance.confidence;
                test.metrics.statisticalSignificance = significance.result;

                // Auto-promote winner if enabled and significant
                if (test.autoPromoteWinner && significance.confidence > 0.95f)
                {
                    CompleteABTest(test, true);
                }
            }

            // Check for test expiration
            if (DateTime.Now > test.endDate)
            {
                CompleteABTest(test, false);
            }
        }

        /// <summary>
        /// Complete A/B test and optionally promote winner
        /// </summary>
        private void CompleteABTest(PricingABTest test, bool promoteWinner)
        {
            test.status = ABTestStatus.Completed;
            
            if (promoteWinner)
            {
                // Find winning variant
                var winningVariant = GetWinningVariant(test);
                if (winningVariant.HasValue)
                {
                    // Update base pricing with winning variant
                    // This would update the game's pricing configuration
                    Debug.Log($"[DynamicPricingManager] Promoting winning variant: {winningVariant.Value.variantId}");
                }
            }

            OnABTestCompleted?.Invoke(new ABTestResult
            {
                testId = test.testId,
                winningVariant = promoteWinner ? GetWinningVariant(test) : null,
                metrics = test.metrics
            });

            _runningTests.Remove(test.testId);
        }

        /// <summary>
        /// Calculate statistical significance of A/B test results
        /// </summary>
        private (float confidence, string result) CalculateStatisticalSignificance(PricingABTest test)
        {
            // Simplified statistical significance calculation
            // In production, use proper statistical libraries
            
            var variants = test.priceVariants.Take(2).ToList(); // Compare first two variants
            if (variants.Count < 2) return (0f, "Insufficient variants");

            var variant1 = variants[0];
            var variant2 = variants[1];

            var impressions1 = test.metrics.impressions.GetValueOrDefault(variant1.variantId, 0);
            var impressions2 = test.metrics.impressions.GetValueOrDefault(variant2.variantId, 0);
            var conversions1 = test.metrics.conversions.GetValueOrDefault(variant1.variantId, 0);
            var conversions2 = test.metrics.conversions.GetValueOrDefault(variant2.variantId, 0);

            if (impressions1 < 30 || impressions2 < 30) 
                return (0f, "Insufficient sample size");

            var rate1 = (float)conversions1 / impressions1;
            var rate2 = (float)conversions2 / impressions2;

            // Simplified z-test approximation
            var pooledRate = (float)(conversions1 + conversions2) / (impressions1 + impressions2);
            var se = Mathf.Sqrt(pooledRate * (1 - pooledRate) * (1f/impressions1 + 1f/impressions2));
            var zScore = Mathf.Abs(rate1 - rate2) / se;

            // Convert z-score to confidence level (simplified)
            float confidence = 0f;
            if (zScore > 2.58f) confidence = 0.99f;
            else if (zScore > 1.96f) confidence = 0.95f;
            else if (zScore > 1.65f) confidence = 0.90f;
            else confidence = 0.50f + (zScore / 3.29f) * 0.50f; // Linear approximation

            string result = confidence > 0.95f ? "Significant" : 
                           confidence > 0.90f ? "Likely significant" : "Not significant";

            return (confidence, result);
        }

        /// <summary>
        /// Get winning variant from A/B test
        /// </summary>
        private PriceVariant? GetWinningVariant(PricingABTest test)
        {
            PriceVariant? winner = null;
            float bestConversionRate = 0f;

            foreach (var variant in test.priceVariants)
            {
                var conversionRate = test.metrics.conversionRates.GetValueOrDefault(variant.variantId, 0f);
                if (conversionRate > bestConversionRate)
                {
                    bestConversionRate = conversionRate;
                    winner = variant;
                }
            }

            return winner;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Set up default urgency pricing rules
        /// </summary>
        private void SetupDefaultUrgencyRules()
        {
            urgencyRules = new List<UrgencyPricingRule>
            {
                new UrgencyPricingRule
                {
                    ruleId = "session_time_discount",
                    trigger = UrgencyTrigger.SessionTime,
                    discountPercentage = 15f,
                    durationMinutes = 10,
                    displayMessage = "Limited Time: 15% off for next 10 minutes!",
                    applicableProducts = new List<string> { "starter_pack", "premium_currency_small" },
                    maxUsesPerPlayer = 1,
                    requiresPlayerAction = false
                },
                new UrgencyPricingRule
                {
                    ruleId = "failed_purchase_recovery",
                    trigger = UrgencyTrigger.FailedPurchaseAttempt,
                    discountPercentage = 20f,
                    durationMinutes = 30,
                    displayMessage = "Don't miss out! 20% off to complete your purchase",
                    applicableProducts = new List<string> { "all" },
                    maxUsesPerPlayer = 2,
                    requiresPlayerAction = true
                },
                new UrgencyPricingRule
                {
                    ruleId = "last_chance_offer",
                    trigger = UrgencyTrigger.LastChance,
                    discountPercentage = 25f,
                    durationMinutes = 5,
                    displayMessage = "FINAL OFFER: 25% off expires in 5 minutes!",
                    applicableProducts = new List<string> { "founder_pack" },
                    maxUsesPerPlayer = 1,
                    requiresPlayerAction = false
                }
            };
        }

        /// <summary>
        /// Load player monetization profile
        /// </summary>
        private void LoadPlayerProfile()
        {
            // In real implementation, load from player data
            _currentPlayerProfile = new PlayerMonetizationProfile
            {
                SpendingType = PlayerSpendingType.NonSpender,
                LifetimeValue = 0f,
                AverageSessionLength = 15f,
                ChurnRisk = 0.3f
            };
        }

        /// <summary>
        /// Load regional pricing data
        /// </summary>
        private void LoadRegionalPricing()
        {
            // Example regional pricing data
            _regionalPricing["US"] = new RegionalPricingData
            {
                region = "US",
                currency = "USD",
                purchasingPowerMultiplier = 1.0f,
                competitiveAdjustment = 1.0f,
                productPriceMultipliers = new Dictionary<string, float>(),
                lastUpdated = DateTime.Now
            };

            _regionalPricing["IN"] = new RegionalPricingData
            {
                region = "IN",
                currency = "INR",
                purchasingPowerMultiplier = 0.25f, // Adjusted for purchasing power
                competitiveAdjustment = 0.9f,
                productPriceMultipliers = new Dictionary<string, float>
                {
                    ["premium_currency_small"] = 0.3f,
                    ["starter_pack"] = 0.35f
                },
                lastUpdated = DateTime.Now
            };
        }

        /// <summary>
        /// Initialize active A/B tests from configuration
        /// </summary>
        private void InitializeActiveTests()
        {
            if (activeTests != null)
            {
                foreach (var test in activeTests)
                {
                    if (test.status == ABTestStatus.Active)
                    {
                        _runningTests[test.testId] = test;
                    }
                }
            }
        }

        /// <summary>
        /// Update dynamic pricing periodically
        /// </summary>
        private void UpdateDynamicPricing()
        {
            _lastPriceUpdate = DateTime.Now;
            
            // Clear expired personalized prices
            var expiredKeys = _personalizedPrices
                .Where(kvp => kvp.Value.priceExpiry <= DateTime.Now)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in expiredKeys)
            {
                _personalizedPrices.Remove(key);
            }

            // Update personalization metrics
            UpdatePersonalizationMetrics();
        }

        /// <summary>
        /// Update and broadcast personalization metrics
        /// </summary>
        private void UpdatePersonalizationMetrics()
        {
            var metrics = new PersonalizationMetrics
            {
                totalPersonalizations = _pricingDecisions.Count,
                averagePriceAdjustment = _pricingDecisions.Average(d => d.finalPrice / d.originalPrice - 1),
                conversionLift = CalculateConversionLift(),
                segmentPerformance = CalculateSegmentPerformance(),
                revenueImpact = CalculateRevenueImpact()
            };

            OnPersonalizationUpdated?.Invoke(metrics);
        }

        /// <summary>
        /// Log pricing decision for analytics
        /// </summary>
        private void LogPricingDecision(PersonalizedPrice price)
        {
            var decision = new PricingDecisionLog
            {
                playerId = GetCurrentPlayerId(),
                productId = price.productId,
                originalPrice = price.originalPrice,
                finalPrice = price.adjustedPrice,
                decisionReason = price.adjustmentReason,
                timestamp = DateTime.Now,
                converted = false, // Will be updated when purchase happens
                testGroup = price.testGroup
            };

            _pricingDecisions.Add(decision);

            // Keep only recent decisions to prevent memory bloat
            if (_pricingDecisions.Count > 10000)
            {
                _pricingDecisions.RemoveRange(0, 1000);
            }
        }

        // Helper methods for calculations
        private float CalculateConversionLift() => 0.15f; // Placeholder
        private Dictionary<string, float> CalculateSegmentPerformance() => new Dictionary<string, float>();
        private float CalculateRevenueImpact() => 0f; // Placeholder
        private string GetCurrentPlayerId() => "player_123"; // Placeholder
        private string GetPlayerRegion() => "US"; // Placeholder
        #endregion

        #region Public Data Structures for External Use
        public struct ABTestResult
        {
            public string testId;
            public PriceVariant? winningVariant;
            public ABTestMetrics metrics;
        }
        #endregion
    }
}

// Extension methods for LINQ operations
public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }
}