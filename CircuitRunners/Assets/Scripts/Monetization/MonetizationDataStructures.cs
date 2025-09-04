using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Data structures supporting the monetization system
    /// </summary>

    /// <summary>
    /// Configuration for individual IAP products
    /// </summary>
    [System.Serializable]
    public class IAPProduct
    {
        [Header("Product Identity")]
        public string ProductID;
        public string DisplayName;
        public string Description;
        public Sprite ProductIcon;
        
        [Header("Product Details")]
        public ProductType ProductType;
        public CurrencyType RewardCurrency;
        public int RewardAmount;
        public List<ProductBonus> Bonuses = new List<ProductBonus>();
        
        [Header("Display")]
        public bool IsFeatured;
        public bool IsBestValue;
        public string BannerText = "";
        public Color ProductColor = Color.white;
        
        [Header("Analytics")]
        public string AnalyticsCategory = "IAP";
        public Dictionary<string, string> CustomData = new Dictionary<string, string>();
        
        /// <summary>
        /// Get formatted description with bonuses
        /// </summary>
        public string GetFullDescription()
        {
            string description = Description;
            
            if (Bonuses.Count > 0)
            {
                description += "\n\nBonuses:";
                foreach (var bonus in Bonuses)
                {
                    description += $"\n• {bonus.GetDescription()}";
                }
            }
            
            return description;
        }
        
        /// <summary>
        /// Calculate total value including bonuses
        /// </summary>
        public int GetTotalValue()
        {
            int total = RewardAmount;
            
            foreach (var bonus in Bonuses)
            {
                if (bonus.BonusCurrency == RewardCurrency)
                {
                    total += bonus.BonusAmount;
                }
            }
            
            return total;
        }
    }

    /// <summary>
    /// Types of IAP products
    /// </summary>
    public enum ProductType
    {
        Consumable,     // Can be purchased multiple times (currency, items)
        NonConsumable,  // One-time purchase (ad removal, permanent upgrades)
        Subscription    // Recurring payment (premium account)
    }

    /// <summary>
    /// Types of currencies that can be rewarded
    /// </summary>
    public enum CurrencyType
    {
        Scrap,
        DataCores,
        PremiumCurrency,
        Energy
    }

    /// <summary>
    /// Bonus rewards for IAP products
    /// </summary>
    [System.Serializable]
    public class ProductBonus
    {
        public CurrencyType BonusCurrency;
        public int BonusAmount;
        public string BonusDescription;
        
        public string GetDescription()
        {
            return $"{BonusAmount} {BonusCurrency} {BonusDescription}".Trim();
        }
    }

    /// <summary>
    /// Product information for store UI
    /// </summary>
    [System.Serializable]
    public class ProductInfo
    {
        public string ProductId;
        public string Title;
        public string Description;
        public string Price;
        public bool IsAvailable;
        public bool IsFeatured;
        public bool IsBestValue;
        public Sprite Icon;
        public Color ProductColor;
    }

    /// <summary>
    /// Monetization analytics tracking
    /// </summary>
    [System.Serializable]
    public class MonetizationAnalytics
    {
        [Header("Session Metrics")]
        public DateTime SessionStartTime;
        public float SessionDuration;
        public int SessionRunCount;
        public float SessionRevenue;
        
        [Header("Ad Metrics")]
        public Dictionary<string, int> AdImpressions = new Dictionary<string, int>();
        public Dictionary<string, int> AdClicks = new Dictionary<string, int>();
        public Dictionary<string, float> AdRevenue = new Dictionary<string, float>();
        public List<AdEvent> AdEvents = new List<AdEvent>();
        
        [Header("Purchase Metrics")]
        public List<PurchaseEvent> PurchaseEvents = new List<PurchaseEvent>();
        public float TotalRevenue;
        public int TotalPurchases;
        public Dictionary<string, int> ProductPurchaseCounts = new Dictionary<string, int>();
        
        [Header("User Behavior")]
        public Dictionary<string, int> OfferViews = new Dictionary<string, int>();
        public Dictionary<string, float> ConversionRates = new Dictionary<string, float>();
        public List<string> PurchaseFunnelSteps = new List<string>();

        /// <summary>
        /// Track session start
        /// </summary>
        public void TrackSessionStart()
        {
            SessionStartTime = DateTime.Now;
            SessionDuration = 0f;
            SessionRunCount = 0;
            SessionRevenue = 0f;
            
            Debug.Log("[MonetizationAnalytics] Session started");
        }

        /// <summary>
        /// Update session metrics
        /// </summary>
        public void UpdateSessionMetrics(float duration, int runCount, float revenue)
        {
            SessionDuration = duration;
            SessionRunCount = runCount;
            SessionRevenue = revenue;
        }

        /// <summary>
        /// Track ad request
        /// </summary>
        public void TrackAdRequest(string adType, string context)
        {
            var adEvent = new AdEvent
            {
                EventType = "ad_request",
                AdType = adType,
                Context = context,
                Timestamp = DateTime.Now
            };
            
            AdEvents.Add(adEvent);
            Debug.Log($"[MonetizationAnalytics] Ad requested: {adType} in context {context}");
        }

        /// <summary>
        /// Track ad show
        /// </summary>
        public void TrackAdShow(string adType, string context)
        {
            string key = $"{adType}_{context}";
            if (!AdImpressions.ContainsKey(key))
                AdImpressions[key] = 0;
            AdImpressions[key]++;
            
            var adEvent = new AdEvent
            {
                EventType = "ad_show",
                AdType = adType,
                Context = context,
                Timestamp = DateTime.Now
            };
            
            AdEvents.Add(adEvent);
            Debug.Log($"[MonetizationAnalytics] Ad shown: {adType} in context {context}");
        }

        /// <summary>
        /// Track ad click
        /// </summary>
        public void TrackAdClick(string adUnitId)
        {
            if (!AdClicks.ContainsKey(adUnitId))
                AdClicks[adUnitId] = 0;
            AdClicks[adUnitId]++;
            
            Debug.Log($"[MonetizationAnalytics] Ad clicked: {adUnitId}");
        }

        /// <summary>
        /// Track ad completion
        /// </summary>
        public void TrackAdComplete(string adUnitId, string result)
        {
            var adEvent = new AdEvent
            {
                EventType = "ad_complete",
                AdType = adUnitId,
                Context = result,
                Timestamp = DateTime.Now
            };
            
            AdEvents.Add(adEvent);
            
            // Estimate revenue for rewarded videos
            if (adUnitId.Contains("rewarded", StringComparison.OrdinalIgnoreCase) && result == "COMPLETED")
            {
                float estimatedRevenue = 0.02f; // $0.02 per completed rewarded video
                if (!AdRevenue.ContainsKey(adUnitId))
                    AdRevenue[adUnitId] = 0f;
                AdRevenue[adUnitId] += estimatedRevenue;
                SessionRevenue += estimatedRevenue;
            }
            
            Debug.Log($"[MonetizationAnalytics] Ad completed: {adUnitId} - {result}");
        }

        /// <summary>
        /// Track ad load error
        /// </summary>
        public void TrackAdLoadError(string adUnitId, string error)
        {
            var adEvent = new AdEvent
            {
                EventType = "ad_load_error",
                AdType = adUnitId,
                Context = error,
                Timestamp = DateTime.Now
            };
            
            AdEvents.Add(adEvent);
            Debug.Log($"[MonetizationAnalytics] Ad load error: {adUnitId} - {error}");
        }

        /// <summary>
        /// Track ad show error
        /// </summary>
        public void TrackAdShowError(string adUnitId, string error)
        {
            var adEvent = new AdEvent
            {
                EventType = "ad_show_error",
                AdType = adUnitId,
                Context = error,
                Timestamp = DateTime.Now
            };
            
            AdEvents.Add(adEvent);
            Debug.Log($"[MonetizationAnalytics] Ad show error: {adUnitId} - {error}");
        }

        /// <summary>
        /// Track purchase attempt
        /// </summary>
        public void TrackPurchaseAttempt(string productId)
        {
            PurchaseFunnelSteps.Add($"attempt_{productId}_{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Debug.Log($"[MonetizationAnalytics] Purchase attempted: {productId}");
        }

        /// <summary>
        /// Track completed purchase
        /// </summary>
        public void TrackPurchase(string productId, float price)
        {
            var purchaseEvent = new PurchaseEvent
            {
                ProductId = productId,
                Price = price,
                Currency = "USD", // Would be dynamic in real implementation
                Timestamp = DateTime.Now,
                Success = true
            };
            
            PurchaseEvents.Add(purchaseEvent);
            
            if (!ProductPurchaseCounts.ContainsKey(productId))
                ProductPurchaseCounts[productId] = 0;
            ProductPurchaseCounts[productId]++;
            
            TotalRevenue += price;
            TotalPurchases++;
            SessionRevenue += price;
            
            PurchaseFunnelSteps.Add($"success_{productId}_{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            Debug.Log($"[MonetizationAnalytics] Purchase completed: {productId} for ${price:F2}");
        }

        /// <summary>
        /// Track purchase failure
        /// </summary>
        public void TrackPurchaseFailure(string productId, string reason)
        {
            var purchaseEvent = new PurchaseEvent
            {
                ProductId = productId,
                Price = 0f,
                Currency = "USD",
                Timestamp = DateTime.Now,
                Success = false,
                FailureReason = reason
            };
            
            PurchaseEvents.Add(purchaseEvent);
            PurchaseFunnelSteps.Add($"failure_{productId}_{reason}_{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            Debug.Log($"[MonetizationAnalytics] Purchase failed: {productId} - {reason}");
        }

        /// <summary>
        /// Track offer shown
        /// </summary>
        public void TrackOfferShown(string offerId, string context)
        {
            string key = $"{offerId}_{context}";
            if (!OfferViews.ContainsKey(key))
                OfferViews[key] = 0;
            OfferViews[key]++;
            
            Debug.Log($"[MonetizationAnalytics] Offer shown: {offerId} in context {context}");
        }

        /// <summary>
        /// Calculate conversion rate for a specific funnel
        /// </summary>
        public float CalculateConversionRate(string productId)
        {
            int attempts = PurchaseFunnelSteps.Count(step => step.StartsWith($"attempt_{productId}"));
            int successes = PurchaseFunnelSteps.Count(step => step.StartsWith($"success_{productId}"));
            
            if (attempts == 0) return 0f;
            
            float rate = (float)successes / attempts;
            ConversionRates[productId] = rate;
            
            return rate;
        }

        /// <summary>
        /// Get analytics summary
        /// </summary>
        public string GetAnalyticsSummary()
        {
            return $"Session: {SessionDuration:F1}min, {SessionRunCount} runs, ${SessionRevenue:F2} revenue\n" +
                   $"Total: {TotalPurchases} purchases, ${TotalRevenue:F2} lifetime revenue\n" +
                   $"Ads: {AdEvents.Count} events, {AdImpressions.Count} impression types";
        }
    }

    /// <summary>
    /// Individual ad event for analytics
    /// </summary>
    [System.Serializable]
    public class AdEvent
    {
        public string EventType;    // "request", "show", "click", "complete", "error"
        public string AdType;       // "rewarded", "interstitial", "banner"
        public string Context;      // Context or additional info
        public DateTime Timestamp;
        public float Revenue;       // Estimated revenue for this event
        
        public string GetDescription()
        {
            return $"{EventType} - {AdType} - {Context} at {Timestamp:HH:mm:ss}";
        }
    }

    /// <summary>
    /// Individual purchase event for analytics
    /// </summary>
    [System.Serializable]
    public class PurchaseEvent
    {
        public string ProductId;
        public float Price;
        public string Currency;
        public DateTime Timestamp;
        public bool Success;
        public string FailureReason;
        public string Context; // Where the purchase was initiated
        
        public string GetDescription()
        {
            return Success ? 
                $"{ProductId} - ${Price:F2} {Currency} at {Timestamp:HH:mm:ss}" :
                $"{ProductId} - FAILED ({FailureReason}) at {Timestamp:HH:mm:ss}";
        }
    }

    /// <summary>
    /// Current monetization statistics
    /// </summary>
    [System.Serializable]
    public class MonetizationStats
    {
        [Header("Session Data")]
        public double SessionDuration; // in minutes
        public int SessionRunCount;
        public float SessionRevenue;
        
        [Header("Ad Data")]
        public int RewardedAdsWatched;
        public int InterstitialsShown;
        public int BannersShown;
        
        [Header("Purchase Data")]
        public bool HasAdRemoval;
        public bool IsPremiumActive;
        public int TotalPurchases;
        public float TotalSpent;
        
        [Header("Performance")]
        public float ARPDAU; // Average Revenue Per Daily Active User
        public float AdFillRate;
        public float PurchaseConversionRate;
        
        public string GetFormattedSummary()
        {
            return $"Session: {SessionDuration:F1}m | Revenue: ${SessionRevenue:F2} | " +
                   $"Ads: {RewardedAdsWatched}R/{InterstitialsShown}I | " +
                   $"Premium: {(IsPremiumActive ? "Yes" : "No")}";
        }
    }

    /// <summary>
    /// Save data for monetization state
    /// </summary>
    [System.Serializable]
    public class MonetizationSaveData
    {
        public bool HasAdRemoval = false;
        public DateTime LastInterstitialTime = DateTime.MinValue;
        public DateTime LastRewardedAdTime = DateTime.MinValue;
        public int RunsSinceLastInterstitial = 0;
        public int RewardedAdsWatchedThisSession = 0;
        public float SessionRevenue = 0f;
        public List<string> PurchasedProducts = new List<string>();
        
        public bool IsValid()
        {
            return true; // Basic validation - could be expanded
        }
    }

    /// <summary>
    /// A/B test configuration for monetization optimization
    /// </summary>
    [System.Serializable]
    public class MonetizationABTest
    {
        [Header("Test Configuration")]
        public string TestId;
        public string TestName;
        public bool IsActive;
        public float TestPercentage; // 0.0 to 1.0
        
        [Header("Test Variables")]
        public float InterstitialCooldownMultiplier = 1f;
        public float RewardedAdRewardMultiplier = 1f;
        public bool ShowExtraOffers = false;
        public List<string> FeaturedProducts = new List<string>();
        
        [Header("Tracking")]
        public DateTime TestStartDate;
        public DateTime TestEndDate;
        public Dictionary<string, float> TestMetrics = new Dictionary<string, float>();
        
        /// <summary>
        /// Check if user should be in this test
        /// </summary>
        public bool ShouldParticipate(string userId)
        {
            if (!IsActive) return false;
            if (DateTime.Now < TestStartDate || DateTime.Now > TestEndDate) return false;
            
            // Use hash of user ID to determine participation
            int hash = userId.GetHashCode();
            float userPercent = Mathf.Abs(hash % 1000) / 1000f;
            
            return userPercent < TestPercentage;
        }
        
        /// <summary>
        /// Track metric for this test
        /// </summary>
        public void TrackMetric(string metricName, float value)
        {
            TestMetrics[metricName] = value;
        }
    }

    /// <summary>
    /// Store configuration and pricing
    /// </summary>
    [System.Serializable]
    public class StoreConfiguration
    {
        [Header("Store Settings")]
        public string StoreName = "Bot Parts Store";
        public string StoreDescription = "Upgrade your bots with premium parts and currency";
        public Sprite StoreBanner;
        public Color StoreThemeColor = Color.blue;
        
        [Header("Featured Products")]
        public List<string> FeaturedProductIds = new List<string>();
        public List<string> BestValueProductIds = new List<string>();
        public string DailyDealProductId = "";
        
        [Header("Promotions")]
        public List<StorePromotion> ActivePromotions = new List<StorePromotion>();
        
        [Header("Display Settings")]
        public bool ShowPricesInLocalCurrency = true;
        public bool EnableProductComparisons = true;
        public bool ShowValueIndicators = true;
        
        /// <summary>
        /// Get active promotion for product
        /// </summary>
        public StorePromotion GetActivePromotion(string productId)
        {
            return ActivePromotions.Find(p => p.ApplicableProducts.Contains(productId) && p.IsActive());
        }
        
        /// <summary>
        /// Check if product is featured
        /// </summary>
        public bool IsProductFeatured(string productId)
        {
            return FeaturedProductIds.Contains(productId);
        }
        
        /// <summary>
        /// Check if product is marked as best value
        /// </summary>
        public bool IsProductBestValue(string productId)
        {
            return BestValueProductIds.Contains(productId);
        }
    }

    /// <summary>
    /// Store promotion configuration
    /// </summary>
    [System.Serializable]
    public class StorePromotion
    {
        public string PromotionId;
        public string PromotionName;
        public string Description;
        public DateTime StartTime;
        public DateTime EndTime;
        public List<string> ApplicableProducts = new List<string>();
        public float DiscountPercentage; // 0.0 to 1.0
        public int BonusCurrency;
        public string BannerText;
        public Color PromotionColor = Color.red;
        
        public bool IsActive()
        {
            DateTime now = DateTime.Now;
            return now >= StartTime && now <= EndTime;
        }
        
        public float GetDiscountedPrice(float originalPrice)
        {
            return originalPrice * (1f - DiscountPercentage);
        }
        
        public string GetDiscountText()
        {
            return $"{DiscountPercentage * 100:F0}% OFF";
        }
    }

    /// <summary>
    /// Player monetization profile for personalization
    /// </summary>
    [System.Serializable]
    public class PlayerMonetizationProfile
    {
        [Header("Player Classification")]
        public PlayerSpendingType SpendingType = PlayerSpendingType.NonSpender;
        public float LifetimeValue;
        public DateTime FirstPurchaseDate;
        public DateTime LastPurchaseDate;
        
        [Header("Behavior Patterns")]
        public float AverageSessionLength;
        public int AverageRunsPerSession;
        public float AdWatchRate; // Percentage of ad opportunities taken
        public float PurchaseConversionRate;
        
        [Header("Preferences")]
        public List<string> PreferredProductTypes = new List<string>();
        public List<string> PreferredOfferContexts = new List<string>();
        public float OptimalOfferTiming; // Best time in session to show offers
        
        [Header("Engagement")]
        public int DaysSinceFirstSession;
        public int TotalSessions;
        public bool IsHighlyEngaged;
        public float ChurnRisk; // 0.0 to 1.0, higher = more likely to churn
        
        /// <summary>
        /// Update player profile based on recent behavior
        /// </summary>
        public void UpdateProfile(MonetizationStats stats)
        {
            AverageSessionLength = (float)stats.SessionDuration;
            AverageRunsPerSession = stats.SessionRunCount;
            
            // Update spending classification
            if (stats.TotalSpent > 50f)
                SpendingType = PlayerSpendingType.HighSpender;
            else if (stats.TotalSpent > 10f)
                SpendingType = PlayerSpendingType.MediumSpender;
            else if (stats.TotalSpent > 0f)
                SpendingType = PlayerSpendingType.LowSpender;
            else
                SpendingType = PlayerSpendingType.NonSpender;
            
            LifetimeValue = stats.TotalSpent;
        }
        
        /// <summary>
        /// Get recommended offer strategy
        /// </summary>
        public string GetRecommendedOfferStrategy()
        {
            switch (SpendingType)
            {
                case PlayerSpendingType.HighSpender:
                    return "premium_offers";
                case PlayerSpendingType.MediumSpender:
                    return "value_packs";
                case PlayerSpendingType.LowSpender:
                    return "starter_offers";
                default:
                    return "free_content";
            }
        }
    }

    /// <summary>
    /// Player spending classification
    /// </summary>
    public enum PlayerSpendingType
    {
        NonSpender,     // Has never made a purchase
        LowSpender,     // $0.01 - $9.99 lifetime
        MediumSpender,  // $10.00 - $49.99 lifetime
        HighSpender,    // $50.00+ lifetime
        Whale          // $200.00+ lifetime (special treatment)
    }
}