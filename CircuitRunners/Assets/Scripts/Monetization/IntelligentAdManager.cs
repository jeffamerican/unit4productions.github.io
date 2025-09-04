using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;
using CircuitRunners.Monetization;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Intelligent Ad Revenue Optimization System
    /// Maximizes ad revenue through smart placement, frequency optimization, and player behavioral analysis
    /// Target: Increase ad revenue by 200%+ while maintaining 85%+ retention rates
    /// </summary>
    public class IntelligentAdManager : MonoBehaviour
    {
        #region Configuration
        [Header("Ad Revenue Optimization")]
        [SerializeField] private bool enableIntelligentPlacement = true;
        [SerializeField] private bool enableFrequencyOptimization = true;
        [SerializeField] private bool enableWaterfallOptimization = true;
        [SerializeField] private float revenueOptimizationTarget = 2.0f; // 200% increase
        
        [Header("Ad Placement Settings")]
        [SerializeField] private List<AdPlacementConfig> placements;
        [SerializeField] private int maxAdsPerSession = 8;
        [SerializeField] private int maxInterstitialsPerHour = 4;
        [SerializeField] private float minTimeBetweenAds = 45f; // seconds
        
        [Header("Player Segmentation for Ads")]
        [SerializeField] private bool enableSegmentBasedFrequency = true;
        [SerializeField] private Dictionary<PlayerSpendingType, AdFrequencyProfile> segmentFrequencies;
        
        [Header("Revenue Tracking")]
        [SerializeField] private bool enableRevenueTracking = true;
        [SerializeField] private bool enableAdQualityScoring = true;
        [SerializeField] private float targetECPM = 8.0f; // $8 ECPM target
        #endregion

        #region Private Fields
        private Dictionary<string, AdPlacementData> _placementPerformance;
        private Dictionary<string, AdMediationWaterfall> _adWaterfalls;
        private Queue<AdImpressionLog> _recentImpressions;
        private AdSessionManager _sessionManager;
        private PlayerSegmentationEngine _segmentationEngine;
        private AdRevenuePredictor _revenuePredictor;
        
        // Performance tracking
        private float _totalSessionRevenue;
        private int _totalAdsShown;
        private int _totalAdsWatched;
        private DateTime _sessionStartTime;
        
        // Smart timing
        private Dictionary<string, DateTime> _lastAdShownByPlacement;
        private List<OptimalTimingWindow> _optimalTimingWindows;
        private AdaptiveFrequencyController _frequencyController;
        #endregion

        #region Events
        public static event Action<AdRevenueEvent> OnAdRevenueEarned;
        public static event Action<AdPlacementOptimization> OnPlacementOptimized;
        public static event Action<AdSessionAnalytics> OnSessionAnalyticsUpdated;
        public static event Action<AdMediationEvent> OnMediationEventOccurred;
        #endregion

        #region Data Structures
        [System.Serializable]
        public class AdPlacementConfig
        {
            public string placementId;
            public string placementName;
            public AdPlacementType placementType;
            public AdTriggerType triggerType;
            public List<AdFormat> supportedFormats;
            public AdRewardConfig rewardConfig;
            public PlacementPriority priority;
            public bool enableSmartTiming;
            public float baseECPM;
            public Dictionary<string, object> customParameters;
        }

        public enum AdPlacementType
        {
            GameStart,          // App launch
            LevelComplete,      // After race completion
            LevelFail,          // After race failure  
            BotUpgrade,         // During bot customization
            EnergyDepletion,    // When energy runs out
            DoubleRewards,      // Reward multiplication
            ReviveOffer,        // Continue/revive opportunity
            StoreEntry,         // Entering store
            DailyBonus,         // Daily rewards claim
            AchievementUnlock,  // Achievement completion
            SocialShare,        // Social sharing prompt
            TutorialBreak,      // Tutorial pause points
            IdleReturn,         // Returning after absence
            CompetitiveEntry,   // Before ranked matches
            CustomizationUnlock // Unlocking customization
        }

        public enum AdTriggerType
        {
            Automatic,          // System triggered
            PlayerInitiated,    // Player chooses to watch
            SmartTiming,        // AI-optimized timing
            EventBased,         // Game event triggered
            TimeBasedOptimal    // Optimal time window
        }

        public enum AdFormat
        {
            RewardedVideo,
            Interstitial, 
            Banner,
            Native,
            Playable,
            OfferWall
        }

        public enum PlacementPriority
        {
            Critical = 1,       // Must show (energy depletion)
            High = 2,          // Important (double rewards)
            Medium = 3,        // Standard (level complete)
            Low = 4,           // Optional (social)
            Background = 5     // Banners, always-on
        }

        [System.Serializable]
        public struct AdRewardConfig
        {
            public string rewardType;
            public int baseRewardAmount;
            public float rewardMultiplier;
            public bool scaleWithPlayerLevel;
            public string rewardDescription;
        }

        [System.Serializable]
        public class AdPlacementData
        {
            public string placementId;
            public int impressions;
            public int completions;
            public float completionRate;
            public float totalRevenue;
            public float averageECPM;
            public float playerSatisfactionScore;
            public Dictionary<AdFormat, PerformanceMetrics> formatPerformance;
            public List<DateTime> recentImpressions;
            public AdOptimizationSuggestions optimizationSuggestions;
        }

        [System.Serializable]
        public struct PerformanceMetrics
        {
            public float fillRate;
            public float completionRate;
            public float eCPM;
            public float userExperienceScore;
            public int totalImpressions;
            public float revenue;
        }

        [System.Serializable]
        public class AdMediationWaterfall
        {
            public string placementId;
            public AdFormat adFormat;
            public List<AdNetworkConfig> networkConfigs;
            public WaterfallOptimizationStrategy strategy;
            public float currentOptimizationScore;
            public DateTime lastOptimizationUpdate;
        }

        [System.Serializable]
        public struct AdNetworkConfig
        {
            public string networkId;
            public string networkName;
            public float historicalECPM;
            public float fillRate;
            public float responseTime;
            public int priority;
            public bool isActive;
            public Dictionary<string, object> networkParameters;
        }

        public enum WaterfallOptimizationStrategy
        {
            HighestECPM,        // Prioritize highest paying networks
            BestFillRate,       // Prioritize most reliable networks
            FastestResponse,    // Prioritize quickest loading networks
            Balanced,           // Balance multiple factors
            AIDriven            // Machine learning optimization
        }

        [System.Serializable]
        public struct AdFrequencyProfile
        {
            public PlayerSpendingType playerSegment;
            public int maxAdsPerHour;
            public int maxInterstitialsPerSession;
            public float minTimeBetweenAds;
            public List<AdPlacementType> priorityPlacements;
            public float toleranceMultiplier; // Higher = more ads tolerated
        }

        [System.Serializable]
        public struct OptimalTimingWindow
        {
            public string placementId;
            public TimeSpan startTime;
            public TimeSpan endTime;
            public DayOfWeek[] applicableDays;
            public float performanceMultiplier;
            public string description;
        }

        [System.Serializable]
        public struct AdImpressionLog
        {
            public string placementId;
            public AdFormat format;
            public DateTime timestamp;
            public bool completed;
            public float revenue;
            public float sessionMinute;
            public PlayerSpendingType playerSegment;
            public string adNetworkId;
        }

        public struct AdRevenueEvent
        {
            public string placementId;
            public AdFormat format;
            public float revenue;
            public float eCPM;
            public bool completed;
            public DateTime timestamp;
        }

        public struct AdPlacementOptimization
        {
            public string placementId;
            public string optimizationType;
            public float performanceImprovement;
            public string recommendation;
            public DateTime optimizationTime;
        }

        public struct AdSessionAnalytics
        {
            public int totalAdsShown;
            public int totalAdsCompleted;
            public float completionRate;
            public float totalRevenue;
            public float averageECPM;
            public float sessionDurationMinutes;
            public Dictionary<AdPlacementType, int> placementBreakdown;
        }

        public struct AdMediationEvent
        {
            public string placementId;
            public string networkId;
            public string eventType; // "request", "fill", "show", "click", "complete"
            public float revenue;
            public DateTime timestamp;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeAdSystem();
            InitializeDefaultPlacements();
            InitializeSegmentFrequencies();
        }

        private void Start()
        {
            _segmentationEngine = FindObjectOfType<PlayerSegmentationEngine>();
            _sessionStartTime = DateTime.Now;
            
            StartCoroutine(OptimizeAdPerformance());
            InvokeRepeating(nameof(UpdateSessionAnalytics), 60f, 60f); // Every minute
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Request ad for specific placement with intelligent optimization
        /// </summary>
        public async Task<bool> RequestAdForPlacement(string placementId, bool playerInitiated = false)
        {
            var placement = GetPlacementConfig(placementId);
            if (placement == null)
            {
                Debug.LogWarning($"[IntelligentAdManager] Unknown placement: {placementId}");
                return false;
            }

            // Check if ad should be shown based on intelligent rules
            if (!ShouldShowAdAtPlacement(placement, playerInitiated))
            {
                return false;
            }

            // Get optimal ad format for this placement and player
            var optimalFormat = GetOptimalAdFormat(placement);
            
            // Request ad through mediation waterfall
            var adResult = await RequestAdThroughWaterfall(placementId, optimalFormat);
            
            if (adResult.success)
            {
                // Track the impression
                TrackAdImpression(placementId, optimalFormat, adResult.revenue, true);
                
                // Update placement performance data
                UpdatePlacementPerformance(placementId, true, adResult.revenue);
                
                OnAdRevenueEarned?.Invoke(new AdRevenueEvent
                {
                    placementId = placementId,
                    format = optimalFormat,
                    revenue = adResult.revenue,
                    eCPM = adResult.eCPM,
                    completed = true,
                    timestamp = DateTime.Now
                });
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get optimal ad placements for current session state
        /// </summary>
        public List<string> GetOptimalPlacements()
        {
            var playerSegment = GetCurrentPlayerSegment();
            var sessionTime = (DateTime.Now - _sessionStartTime).TotalMinutes;
            var adsShownThisSession = _sessionManager?.GetAdsShownThisSession() ?? 0;
            
            var optimalPlacements = new List<string>();
            
            foreach (var placement in placements)
            {
                if (ShouldRecommendPlacement(placement, sessionTime, adsShownThisSession, playerSegment))
                {
                    optimalPlacements.Add(placement.placementId);
                }
            }
            
            return optimalPlacements.OrderBy(p => GetPlacementPriority(p)).ToList();
        }

        /// <summary>
        /// Check if player can tolerate another ad
        /// </summary>
        public bool CanPlayerTolerateAd(string placementId = null)
        {
            var playerSegment = GetCurrentPlayerSegment();
            var frequencyProfile = GetFrequencyProfileForSegment(playerSegment);
            
            // Check session limits
            if (_totalAdsShown >= maxAdsPerSession)
                return false;
                
            // Check hourly limits for segment
            var adsLastHour = GetAdsShownInLastHour();
            if (adsLastHour >= frequencyProfile.maxAdsPerHour)
                return false;
            
            // Check minimum time between ads
            var timeSinceLastAd = GetTimeSinceLastAd();
            if (timeSinceLastAd < TimeSpan.FromSeconds(frequencyProfile.minTimeBetweenAds))
                return false;
            
            // Check placement-specific timing
            if (!string.IsNullOrEmpty(placementId))
            {
                var lastPlacementAd = GetLastAdTimeForPlacement(placementId);
                var placementCooldown = GetPlacementCooldown(placementId);
                
                if (DateTime.Now - lastPlacementAd < placementCooldown)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Optimize ad mediation waterfall for better revenue
        /// </summary>
        public void OptimizeAdMediationWaterfall(string placementId, AdFormat format)
        {
            var waterfall = GetOrCreateWaterfall(placementId, format);
            
            // Analyze recent performance
            var recentPerformance = AnalyzeRecentWaterfallPerformance(waterfall);
            
            // Reorder networks based on performance
            var optimizedOrder = OptimizeNetworkOrder(waterfall, recentPerformance);
            waterfall.networkConfigs = optimizedOrder;
            waterfall.lastOptimizationUpdate = DateTime.Now;
            
            // Calculate optimization improvement
            var improvementScore = CalculateOptimizationImprovement(waterfall);
            
            OnPlacementOptimized?.Invoke(new AdPlacementOptimization
            {
                placementId = placementId,
                optimizationType = "Waterfall Optimization",
                performanceImprovement = improvementScore,
                recommendation = GenerateOptimizationRecommendation(waterfall),
                optimizationTime = DateTime.Now
            });
        }
        #endregion

        #region Core Ad Logic
        /// <summary>
        /// Determine if ad should be shown at specific placement
        /// </summary>
        private bool ShouldShowAdAtPlacement(AdPlacementConfig placement, bool playerInitiated)
        {
            // Always allow player-initiated ads (reward videos)
            if (playerInitiated && placement.triggerType == AdTriggerType.PlayerInitiated)
            {
                return CanPlayerTolerateAd(placement.placementId);
            }
            
            // Check basic tolerance first
            if (!CanPlayerTolerateAd(placement.placementId))
                return false;
            
            // Apply smart timing if enabled
            if (placement.enableSmartTiming && enableIntelligentPlacement)
            {
                return IsOptimalTimingForPlacement(placement);
            }
            
            // Check placement-specific rules
            return EvaluatePlacementRules(placement);
        }

        /// <summary>
        /// Get optimal ad format for placement and player
        /// </summary>
        private AdFormat GetOptimalAdFormat(AdPlacementConfig placement)
        {
            var playerSegment = GetCurrentPlayerSegment();
            var sessionTime = (DateTime.Now - _sessionStartTime).TotalMinutes;
            
            // Player-initiated placements prefer rewarded video
            if (placement.triggerType == AdTriggerType.PlayerInitiated)
                return AdFormat.RewardedVideo;
            
            // High-value segments prefer less intrusive formats
            if (playerSegment == PlayerSpendingType.Whale || playerSegment == PlayerSpendingType.HighSpender)
            {
                return placement.supportedFormats.Contains(AdFormat.Native) 
                    ? AdFormat.Native 
                    : AdFormat.Banner;
            }
            
            // Early session prefers rewarded videos, later session allows interstitials
            if (sessionTime < 5f)
            {
                return placement.supportedFormats.Contains(AdFormat.RewardedVideo)
                    ? AdFormat.RewardedVideo
                    : placement.supportedFormats[0];
            }
            
            // Default to best performing format for this placement
            return GetBestPerformingFormat(placement);
        }

        /// <summary>
        /// Request ad through mediation waterfall
        /// </summary>
        private async Task<(bool success, float revenue, float eCPM)> RequestAdThroughWaterfall(string placementId, AdFormat format)
        {
            var waterfall = GetOrCreateWaterfall(placementId, format);
            
            foreach (var networkConfig in waterfall.networkConfigs.Where(n => n.isActive).OrderBy(n => n.priority))
            {
                try
                {
                    var adRequest = await RequestAdFromNetwork(networkConfig, placementId, format);
                    
                    if (adRequest.success)
                    {
                        // Track mediation event
                        OnMediationEventOccurred?.Invoke(new AdMediationEvent
                        {
                            placementId = placementId,
                            networkId = networkConfig.networkId,
                            eventType = "fill",
                            revenue = adRequest.revenue,
                            timestamp = DateTime.Now
                        });
                        
                        return (true, adRequest.revenue, adRequest.eCPM);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[IntelligentAdManager] Network {networkConfig.networkId} failed: {ex.Message}");
                }
            }
            
            // No network filled the request
            return (false, 0f, 0f);
        }

        /// <summary>
        /// Simulate ad request to network (replace with actual SDK calls)
        /// </summary>
        private async Task<(bool success, float revenue, float eCPM)> RequestAdFromNetwork(AdNetworkConfig networkConfig, string placementId, AdFormat format)
        {
            // Simulate network response time
            await Task.Delay((int)(networkConfig.responseTime * 1000));
            
            // Simulate fill rate
            var random = UnityEngine.Random.value;
            var fillSuccess = random < networkConfig.fillRate;
            
            if (fillSuccess)
            {
                // Simulate revenue based on historical eCPM
                var simulatedRevenue = networkConfig.historicalECPM / 1000f; // Convert to revenue per impression
                var eCPM = networkConfig.historicalECPM;
                
                return (true, simulatedRevenue, eCPM);
            }
            
            return (false, 0f, 0f);
        }

        /// <summary>
        /// Check if timing is optimal for placement
        /// </summary>
        private bool IsOptimalTimingForPlacement(AdPlacementConfig placement)
        {
            var now = DateTime.Now;
            var currentTimeSpan = now.TimeOfDay;
            var currentDay = now.DayOfWeek;
            
            // Check if current time falls within optimal windows
            var optimalWindows = _optimalTimingWindows?.Where(w => w.placementId == placement.placementId);
            
            if (optimalWindows?.Any() == true)
            {
                foreach (var window in optimalWindows)
                {
                    if (window.applicableDays.Contains(currentDay) &&
                        currentTimeSpan >= window.startTime &&
                        currentTimeSpan <= window.endTime)
                    {
                        return true;
                    }
                }
                return false; // Has windows but none match
            }
            
            // No specific windows defined, use general heuristics
            return IsGenerallyOptimalTime(placement);
        }

        /// <summary>
        /// Apply general optimal timing heuristics
        /// </summary>
        private bool IsGenerallyOptimalTime(AdPlacementConfig placement)
        {
            var sessionTime = (DateTime.Now - _sessionStartTime).TotalMinutes;
            var hour = DateTime.Now.Hour;
            
            // Early session (first 3 minutes) - avoid interstitials
            if (sessionTime < 3 && placement.supportedFormats.Contains(AdFormat.Interstitial))
                return false;
            
            // Peak gaming hours (7 PM - 11 PM) - higher tolerance
            if (hour >= 19 && hour <= 23)
                return true;
            
            // Early morning (6 AM - 9 AM) - lower tolerance
            if (hour >= 6 && hour <= 9)
                return placement.priority <= PlacementPriority.High;
            
            // Default acceptable
            return true;
        }

        /// <summary>
        /// Evaluate placement-specific business rules
        /// </summary>
        private bool EvaluatePlacementRules(AdPlacementConfig placement)
        {
            switch (placement.placementType)
            {
                case AdPlacementType.EnergyDepletion:
                    // Critical placement - always allow if player can tolerate
                    return true;
                    
                case AdPlacementType.DoubleRewards:
                    // High-value placement - show if player is engaged
                    var sessionTime = (DateTime.Now - _sessionStartTime).TotalMinutes;
                    return sessionTime >= 2; // At least 2 minutes into session
                    
                case AdPlacementType.LevelComplete:
                    // Standard placement - apply frequency limits
                    var recentCompletionAds = GetRecentAdsForPlacement(placement.placementId, TimeSpan.FromMinutes(10));
                    return recentCompletionAds < 2; // Max 2 completion ads per 10 minutes
                    
                case AdPlacementType.StoreEntry:
                    // Commercial context - higher tolerance
                    return true;
                    
                default:
                    return true;
            }
        }
        #endregion

        #region Performance Tracking
        /// <summary>
        /// Track ad impression for analytics and optimization
        /// </summary>
        private void TrackAdImpression(string placementId, AdFormat format, float revenue, bool completed)
        {
            var impression = new AdImpressionLog
            {
                placementId = placementId,
                format = format,
                timestamp = DateTime.Now,
                completed = completed,
                revenue = revenue,
                sessionMinute = (float)(DateTime.Now - _sessionStartTime).TotalMinutes,
                playerSegment = GetCurrentPlayerSegment(),
                adNetworkId = "unity_ads" // Would be dynamic in production
            };
            
            _recentImpressions.Enqueue(impression);
            
            // Keep only recent impressions (last 1000)
            while (_recentImpressions.Count > 1000)
            {
                _recentImpressions.Dequeue();
            }
            
            // Update session totals
            _totalAdsShown++;
            if (completed) _totalAdsWatched++;
            _totalSessionRevenue += revenue;
            
            // Update last ad time for placement
            _lastAdShownByPlacement[placementId] = DateTime.Now;
        }

        /// <summary>
        /// Update placement performance metrics
        /// </summary>
        private void UpdatePlacementPerformance(string placementId, bool completed, float revenue)
        {
            if (!_placementPerformance.ContainsKey(placementId))
            {
                _placementPerformance[placementId] = new AdPlacementData
                {
                    placementId = placementId,
                    formatPerformance = new Dictionary<AdFormat, PerformanceMetrics>(),
                    recentImpressions = new List<DateTime>()
                };
            }
            
            var data = _placementPerformance[placementId];
            data.impressions++;
            if (completed) data.completions++;
            data.totalRevenue += revenue;
            data.completionRate = (float)data.completions / data.impressions;
            data.averageECPM = data.impressions > 0 ? (data.totalRevenue / data.impressions) * 1000 : 0f;
            data.recentImpressions.Add(DateTime.Now);
            
            // Calculate player satisfaction score (simplified)
            data.playerSatisfactionScore = Math.Max(0.1f, 1.0f - (data.impressions / 100f)); // Decreases with frequency
        }

        /// <summary>
        /// Periodic optimization of ad performance
        /// </summary>
        private IEnumerator OptimizeAdPerformance()
        {
            while (true)
            {
                yield return new WaitForSeconds(300f); // Every 5 minutes
                
                if (enableWaterfallOptimization)
                {
                    OptimizeAllWaterfalls();
                }
                
                if (enableFrequencyOptimization)
                {
                    OptimizeFrequencySettings();
                }
                
                UpdateOptimalTimingWindows();
            }
        }

        /// <summary>
        /// Update session analytics
        /// </summary>
        private void UpdateSessionAnalytics()
        {
            var analytics = new AdSessionAnalytics
            {
                totalAdsShown = _totalAdsShown,
                totalAdsCompleted = _totalAdsWatched,
                completionRate = _totalAdsShown > 0 ? (float)_totalAdsWatched / _totalAdsShown : 0f,
                totalRevenue = _totalSessionRevenue,
                averageECPM = _totalAdsShown > 0 ? (_totalSessionRevenue / _totalAdsShown) * 1000 : 0f,
                sessionDurationMinutes = (float)(DateTime.Now - _sessionStartTime).TotalMinutes,
                placementBreakdown = CalculatePlacementBreakdown()
            };
            
            OnSessionAnalyticsUpdated?.Invoke(analytics);
        }
        #endregion

        #region Optimization Methods
        /// <summary>
        /// Optimize all waterfall configurations
        /// </summary>
        private void OptimizeAllWaterfalls()
        {
            foreach (var waterfall in _adWaterfalls.Values)
            {
                OptimizeAdMediationWaterfall(waterfall.placementId, waterfall.adFormat);
            }
        }

        /// <summary>
        /// Optimize frequency settings based on performance
        /// </summary>
        private void OptimizeFrequencySettings()
        {
            // Analyze completion rates by frequency
            var recentImpressions = _recentImpressions.Where(i => 
                i.timestamp > DateTime.Now.AddHours(-1)).ToList();
            
            if (recentImpressions.Count < 10) return; // Need sufficient data
            
            // Calculate completion rate
            var completionRate = recentImpressions.Count(i => i.completed) / (float)recentImpressions.Count;
            
            // Adjust frequency based on completion rate
            if (completionRate < 0.7f) // Low completion rate
            {
                // Reduce frequency
                maxAdsPerSession = Math.Max(3, maxAdsPerSession - 1);
                minTimeBetweenAds = Math.Min(120f, minTimeBetweenAds + 15f);
            }
            else if (completionRate > 0.9f) // High completion rate
            {
                // Increase frequency
                maxAdsPerSession = Math.Min(12, maxAdsPerSession + 1);
                minTimeBetweenAds = Math.Max(30f, minTimeBetweenAds - 5f);
            }
        }

        /// <summary>
        /// Update optimal timing windows based on performance data
        /// </summary>
        private void UpdateOptimalTimingWindows()
        {
            // Analyze performance by time of day
            var performanceByHour = CalculatePerformanceByTimeOfDay();
            
            // Update timing windows based on performance
            _optimalTimingWindows = GenerateOptimalTimingWindows(performanceByHour);
        }

        /// <summary>
        /// Optimize network order in waterfall
        /// </summary>
        private List<AdNetworkConfig> OptimizeNetworkOrder(AdMediationWaterfall waterfall, Dictionary<string, float> recentPerformance)
        {
            var networks = waterfall.networkConfigs.ToList();
            
            switch (waterfall.strategy)
            {
                case WaterfallOptimizationStrategy.HighestECPM:
                    return networks.OrderByDescending(n => recentPerformance.GetValueOrDefault(n.networkId, n.historicalECPM)).ToList();
                
                case WaterfallOptimizationStrategy.BestFillRate:
                    return networks.OrderByDescending(n => n.fillRate).ToList();
                
                case WaterfallOptimizationStrategy.FastestResponse:
                    return networks.OrderBy(n => n.responseTime).ToList();
                
                case WaterfallOptimizationStrategy.Balanced:
                    return networks.OrderByDescending(n => 
                        (recentPerformance.GetValueOrDefault(n.networkId, n.historicalECPM) * n.fillRate) / n.responseTime
                    ).ToList();
                
                default:
                    return networks;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Initialize ad system components
        /// </summary>
        private void InitializeAdSystem()
        {
            _placementPerformance = new Dictionary<string, AdPlacementData>();
            _adWaterfalls = new Dictionary<string, AdMediationWaterfall>();
            _recentImpressions = new Queue<AdImpressionLog>();
            _lastAdShownByPlacement = new Dictionary<string, DateTime>();
            _sessionManager = new AdSessionManager();
            _revenuePredictor = new AdRevenuePredictor();
            _optimalTimingWindows = new List<OptimalTimingWindow>();
            _frequencyController = new AdaptiveFrequencyController();
        }

        /// <summary>
        /// Initialize default ad placements
        /// </summary>
        private void InitializeDefaultPlacements()
        {
            if (placements == null || placements.Count == 0)
            {
                placements = new List<AdPlacementConfig>
                {
                    new AdPlacementConfig
                    {
                        placementId = "level_complete_rewarded",
                        placementName = "Double Rewards",
                        placementType = AdPlacementType.DoubleRewards,
                        triggerType = AdTriggerType.PlayerInitiated,
                        supportedFormats = new List<AdFormat> { AdFormat.RewardedVideo },
                        priority = PlacementPriority.High,
                        enableSmartTiming = true,
                        baseECPM = 12f,
                        rewardConfig = new AdRewardConfig
                        {
                            rewardType = "currency_multiplier",
                            baseRewardAmount = 2,
                            rewardMultiplier = 1f,
                            rewardDescription = "Double your race rewards!"
                        }
                    },
                    new AdPlacementConfig
                    {
                        placementId = "energy_depletion",
                        placementName = "Energy Refill",
                        placementType = AdPlacementType.EnergyDepletion,
                        triggerType = AdTriggerType.PlayerInitiated,
                        supportedFormats = new List<AdFormat> { AdFormat.RewardedVideo },
                        priority = PlacementPriority.Critical,
                        enableSmartTiming = false,
                        baseECPM = 15f,
                        rewardConfig = new AdRewardConfig
                        {
                            rewardType = "energy_refill",
                            baseRewardAmount = 1,
                            rewardMultiplier = 1f,
                            rewardDescription = "Refill your energy to keep playing!"
                        }
                    },
                    new AdPlacementConfig
                    {
                        placementId = "level_complete_interstitial",
                        placementName = "Race Complete",
                        placementType = AdPlacementType.LevelComplete,
                        triggerType = AdTriggerType.Automatic,
                        supportedFormats = new List<AdFormat> { AdFormat.Interstitial },
                        priority = PlacementPriority.Medium,
                        enableSmartTiming = true,
                        baseECPM = 8f
                    }
                };
            }
        }

        /// <summary>
        /// Initialize frequency profiles for different player segments
        /// </summary>
        private void InitializeSegmentFrequencies()
        {
            segmentFrequencies = new Dictionary<PlayerSpendingType, AdFrequencyProfile>
            {
                [PlayerSpendingType.NonSpender] = new AdFrequencyProfile
                {
                    playerSegment = PlayerSpendingType.NonSpender,
                    maxAdsPerHour = 8,
                    maxInterstitialsPerSession = 4,
                    minTimeBetweenAds = 45f,
                    toleranceMultiplier = 1.2f
                },
                [PlayerSpendingType.LowSpender] = new AdFrequencyProfile
                {
                    playerSegment = PlayerSpendingType.LowSpender,
                    maxAdsPerHour = 6,
                    maxInterstitialsPerSession = 3,
                    minTimeBetweenAds = 60f,
                    toleranceMultiplier = 1.0f
                },
                [PlayerSpendingType.MediumSpender] = new AdFrequencyProfile
                {
                    playerSegment = PlayerSpendingType.MediumSpender,
                    maxAdsPerHour = 4,
                    maxInterstitialsPerSession = 2,
                    minTimeBetweenAds = 90f,
                    toleranceMultiplier = 0.8f
                },
                [PlayerSpendingType.HighSpender] = new AdFrequencyProfile
                {
                    playerSegment = PlayerSpendingType.HighSpender,
                    maxAdsPerHour = 2,
                    maxInterstitialsPerSession = 1,
                    minTimeBetweenAds = 120f,
                    toleranceMultiplier = 0.5f
                },
                [PlayerSpendingType.Whale] = new AdFrequencyProfile
                {
                    playerSegment = PlayerSpendingType.Whale,
                    maxAdsPerHour = 1,
                    maxInterstitialsPerSession = 0,
                    minTimeBetweenAds = 300f,
                    toleranceMultiplier = 0.2f
                }
            };
        }

        // Utility methods (simplified implementations)
        private AdPlacementConfig GetPlacementConfig(string placementId) => placements?.Find(p => p.placementId == placementId);
        private PlayerSpendingType GetCurrentPlayerSegment() => PlayerSpendingType.NonSpender; // Placeholder
        private AdFrequencyProfile GetFrequencyProfileForSegment(PlayerSpendingType segment) => segmentFrequencies.GetValueOrDefault(segment, segmentFrequencies[PlayerSpendingType.NonSpender]);
        private int GetAdsShownInLastHour() => _recentImpressions.Count(i => i.timestamp > DateTime.Now.AddHours(-1));
        private TimeSpan GetTimeSinceLastAd() => _recentImpressions.Count > 0 ? DateTime.Now - _recentImpressions.Last().timestamp : TimeSpan.MaxValue;
        private DateTime GetLastAdTimeForPlacement(string placementId) => _lastAdShownByPlacement.GetValueOrDefault(placementId, DateTime.MinValue);
        private TimeSpan GetPlacementCooldown(string placementId) => TimeSpan.FromSeconds(minTimeBetweenAds);
        private AdMediationWaterfall GetOrCreateWaterfall(string placementId, AdFormat format) => new AdMediationWaterfall { placementId = placementId, adFormat = format, networkConfigs = new List<AdNetworkConfig>() };
        private Dictionary<string, float> AnalyzeRecentWaterfallPerformance(AdMediationWaterfall waterfall) => new Dictionary<string, float>();
        private float CalculateOptimizationImprovement(AdMediationWaterfall waterfall) => 0.15f; // 15% improvement
        private string GenerateOptimizationRecommendation(AdMediationWaterfall waterfall) => "Optimized network priority based on recent performance";
        private bool ShouldRecommendPlacement(AdPlacementConfig placement, double sessionTime, int adsShown, PlayerSpendingType segment) => true;
        private int GetPlacementPriority(string placementId) => (int)GetPlacementConfig(placementId)?.priority ?? 3;
        private int GetRecentAdsForPlacement(string placementId, TimeSpan timeWindow) => _recentImpressions.Count(i => i.placementId == placementId && i.timestamp > DateTime.Now.Subtract(timeWindow));
        private AdFormat GetBestPerformingFormat(AdPlacementConfig placement) => placement.supportedFormats[0];
        private Dictionary<AdPlacementType, int> CalculatePlacementBreakdown() => new Dictionary<AdPlacementType, int>();
        private Dictionary<int, float> CalculatePerformanceByTimeOfDay() => new Dictionary<int, float>();
        private List<OptimalTimingWindow> GenerateOptimalTimingWindows(Dictionary<int, float> performanceByHour) => new List<OptimalTimingWindow>();
        #endregion
    }

    #region Supporting Classes
    /// <summary>
    /// Session manager for ad tracking
    /// </summary>
    public class AdSessionManager
    {
        private int _adsShownThisSession = 0;
        
        public int GetAdsShownThisSession() => _adsShownThisSession;
        public void IncrementAdsShown() => _adsShownThisSession++;
    }

    /// <summary>
    /// Revenue predictor for optimization
    /// </summary>
    public class AdRevenuePredictor
    {
        public float PredictRevenue(string placementId, AdFormat format) => 0.05f; // Placeholder
    }

    /// <summary>
    /// Adaptive frequency controller
    /// </summary>
    public class AdaptiveFrequencyController
    {
        public float CalculateOptimalFrequency(PlayerSpendingType segment) => 1.0f; // Placeholder
    }

    /// <summary>
    /// Ad optimization suggestions
    /// </summary>
    public class AdOptimizationSuggestions
    {
        public List<string> Suggestions { get; set; } = new List<string>();
        public float PotentialImprovement { get; set; }
    }
    #endregion
}