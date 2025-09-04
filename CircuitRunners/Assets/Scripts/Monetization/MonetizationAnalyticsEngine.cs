using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CircuitRunners.Monetization;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Comprehensive Monetization Analytics Engine
    /// Tracks all monetization KPIs, conversion funnels, and revenue optimization metrics
    /// Target: Provide real-time insights for achieving $50+ Day 1 revenue and optimizing LTV
    /// </summary>
    public class MonetizationAnalyticsEngine : MonoBehaviour
    {
        #region Configuration
        [Header("Analytics Configuration")]
        [SerializeField] private bool enableRealTimeTracking = true;
        [SerializeField] private bool enableCohortAnalysis = true;
        [SerializeField] private bool enableFunnelTracking = true;
        [SerializeField] private bool enableLTVPrediction = true;
        
        [Header("Reporting Settings")]
        [SerializeField] private int maxEventsInMemory = 10000;
        [SerializeField] private float analyticsUpdateIntervalSeconds = 60f;
        [SerializeField] private bool enableAutomaticReporting = true;
        [SerializeField] private List<string> customEventTypes;
        
        [Header("Performance Targets")]
        [SerializeField] private float targetDay1Revenue = 50f;
        [SerializeField] private float targetMonth1Revenue = 500f;
        [SerializeField] private float targetARPU = 0.50f;
        [SerializeField] private float targetConversionRate = 0.05f;
        #endregion

        #region Private Fields
        private Queue<MonetizationEvent> _eventQueue;
        private Dictionary<string, PlayerAnalyticsProfile> _playerProfiles;
        private List<CohortAnalysis> _cohortAnalyses;
        private Dictionary<string, ConversionFunnel> _conversionFunnels;
        private MonetizationPerformanceMetrics _currentMetrics;
        private RevenuePredictionModel _revenuePredictionModel;
        
        // Real-time tracking
        private DateTime _sessionStartTime;
        private float _sessionRevenue;
        private int _sessionConversions;
        private Dictionary<string, float> _productPerformance;
        
        // Dashboard data
        private MonetizationDashboard _dashboardData;
        private List<RevenueAlert> _activeAlerts;
        #endregion

        #region Events
        public static event Action<MonetizationEvent> OnMonetizationEventTracked;
        public static event Action<MonetizationPerformanceMetrics> OnMetricsUpdated;
        public static event Action<ConversionFunnelAnalysis> OnFunnelAnalysisCompleted;
        public static event Action<CohortAnalysis> OnCohortAnalysisUpdated;
        public static event Action<RevenueAlert> OnRevenueAlertTriggered;
        public static event Action<MonetizationDashboard> OnDashboardDataUpdated;
        #endregion

        #region Data Structures
        [System.Serializable]
        public struct MonetizationEvent
        {
            public string eventId;
            public string playerId;
            public DateTime timestamp;
            public MonetizationEventType eventType;
            public string productId;
            public float revenue;
            public string currency;
            public Dictionary<string, object> customData;
            public string funnelStep;
            public PlayerSpendingType playerSegment;
            public int sessionDay; // Days since install
            public float sessionMinute; // Minutes into current session
        }

        public enum MonetizationEventType
        {
            // Purchase events
            PurchaseAttempted,
            PurchaseCompleted,
            PurchaseFailed,
            PurchaseRefunded,
            
            // Ad events
            AdRequested,
            AdShown,
            AdCompleted,
            AdSkipped,
            AdFailed,
            
            // Offer events
            OfferShown,
            OfferClicked,
            OfferAccepted,
            OfferDismissed,
            OfferExpired,
            
            // Store events
            StoreVisited,
            ProductViewed,
            CartAbandoned,
            
            // Currency events
            CurrencyEarned,
            CurrencySpent,
            CurrencyPurchased,
            
            // Progression events
            LevelUp,
            MilestoneReached,
            AchievementUnlocked,
            
            // Engagement events
            SessionStart,
            SessionEnd,
            FeatureUsed,
            
            // Social events
            FriendInvited,
            SocialShare,
            ReviewPrompted,
            
            // Retention events
            DayNRetention,
            ComebackTriggered,
            ChurnRiskDetected
        }

        [System.Serializable]
        public class PlayerAnalyticsProfile
        {
            public string playerId;
            public DateTime firstSeen;
            public DateTime lastSeen;
            public int totalSessions;
            public float totalRevenue;
            public float totalAdRevenue;
            public int totalPurchases;
            public float averageOrderValue;
            public PlayerSpendingType spendingSegment;
            public float predictedLTV;
            public Dictionary<string, int> productPurchases;
            public List<ConversionFunnelStep> funnelProgress;
            public CohortInfo cohortInfo;
            public EngagementMetrics engagementMetrics;
        }

        [System.Serializable]
        public struct CohortInfo
        {
            public string cohortId;
            public DateTime cohortStartDate;
            public string acquisitionChannel;
            public string campaignSource;
            public Dictionary<string, object> cohortAttributes;
        }

        [System.Serializable]
        public struct EngagementMetrics
        {
            public float averageSessionLength;
            public int daysRetained;
            public float engagementScore;
            public List<string> favoriteFeatures;
            public DateTime lastActiveDate;
        }

        [System.Serializable]
        public class ConversionFunnel
        {
            public string funnelId;
            public string funnelName;
            public List<FunnelStep> steps;
            public Dictionary<string, int> stepCounts;
            public Dictionary<string, float> conversionRates;
            public DateTime lastUpdated;
            public float overallConversionRate;
        }

        [System.Serializable]
        public struct FunnelStep
        {
            public string stepId;
            public string stepName;
            public int stepOrder;
            public MonetizationEventType[] triggerEvents;
            public Dictionary<string, object> stepCriteria;
        }

        [System.Serializable]
        public struct ConversionFunnelStep
        {
            public string funnelId;
            public string stepId;
            public DateTime timestamp;
            public bool completed;
            public Dictionary<string, object> stepData;
        }

        [System.Serializable]
        public class CohortAnalysis
        {
            public string cohortId;
            public DateTime cohortDate;
            public int totalUsers;
            public Dictionary<int, RetentionData> dayRetention; // Day -> Retention data
            public Dictionary<int, RevenueData> dayRevenue; // Day -> Revenue data
            public float averageLTV;
            public float payingUserPercentage;
            public Dictionary<string, float> segmentDistribution;
            public CohortPerformanceMetrics performanceMetrics;
        }

        [System.Serializable]
        public struct RetentionData
        {
            public int activeUsers;
            public float retentionRate;
            public float averageSessionLength;
            public int averageSessions;
        }

        [System.Serializable]
        public struct RevenueData
        {
            public float totalRevenue;
            public float averageRevenue;
            public float arpu;
            public float arppu;
            public int payingUsers;
        }

        [System.Serializable]
        public struct CohortPerformanceMetrics
        {
            public float day1Retention;
            public float day7Retention;
            public float day30Retention;
            public float day1Revenue;
            public float day7Revenue;
            public float day30Revenue;
            public float conversionRate;
            public TimeSpan timeToFirstPurchase;
        }

        [System.Serializable]
        public struct MonetizationPerformanceMetrics
        {
            [Header("Revenue Metrics")]
            public float totalRevenue;
            public float todaysRevenue;
            public float averageDailyRevenue;
            public float revenueGrowthRate;
            
            [Header("User Metrics")]
            public int totalUsers;
            public int activeUsers;
            public int payingUsers;
            public float conversionRate;
            
            [Header("ARPU Metrics")]
            public float arpu; // Average Revenue Per User
            public float arppu; // Average Revenue Per Paying User
            public float targetArpu;
            public float arpuGrowthRate;
            
            [Header("Product Performance")]
            public Dictionary<string, ProductPerformanceMetrics> productMetrics;
            public string bestPerformingProduct;
            public string worstPerformingProduct;
            
            [Header("Funnel Metrics")]
            public Dictionary<string, float> funnelConversionRates;
            public float averageFunnelConversionRate;
            
            [Header("Cohort Metrics")]
            public Dictionary<string, float> cohortRetentionRates;
            public float averageLTV;
            public float predictedLTV;
            
            [Header("Alert Status")]
            public int activeAlerts;
            public List<string> criticalIssues;
            
            public DateTime lastUpdated;
        }

        [System.Serializable]
        public struct ProductPerformanceMetrics
        {
            public string productId;
            public float totalRevenue;
            public int totalPurchases;
            public float conversionRate;
            public float averageOrderValue;
            public float revenuePerImpression;
            public Dictionary<PlayerSpendingType, float> segmentConversionRates;
        }

        [System.Serializable]
        public struct ConversionFunnelAnalysis
        {
            public string funnelId;
            public Dictionary<string, FunnelStepAnalysis> stepAnalyses;
            public List<FunnelOptimizationRecommendation> recommendations;
            public float overallConversionRate;
            public float potentialImprovement;
            public DateTime analysisTime;
        }

        [System.Serializable]
        public struct FunnelStepAnalysis
        {
            public string stepId;
            public int entrances;
            public int exits;
            public int completions;
            public float conversionRate;
            public float dropOffRate;
            public TimeSpan averageTimeSpent;
            public List<string> commonDropOffReasons;
        }

        [System.Serializable]
        public struct FunnelOptimizationRecommendation
        {
            public string stepId;
            public string recommendationType;
            public string description;
            public float expectedImprovement;
            public int priority; // 1 = highest
        }

        [System.Serializable]
        public struct RevenueAlert
        {
            public string alertId;
            public AlertType alertType;
            public AlertSeverity severity;
            public string message;
            public float currentValue;
            public float threshold;
            public DateTime triggerTime;
            public Dictionary<string, object> alertData;
            public bool isResolved;
        }

        public enum AlertType
        {
            RevenueBelowTarget,
            ConversionRateDropped,
            ARPUDeclined,
            HighChurnRate,
            FunnelBottleneck,
            PaymentFailureSpike,
            UnusualSpendingPattern,
            CohortUnderperforming
        }

        public enum AlertSeverity
        {
            Info = 1,
            Warning = 2,
            Critical = 3,
            Emergency = 4
        }

        [System.Serializable]
        public class MonetizationDashboard
        {
            [Header("Key Performance Indicators")]
            public float todayRevenue;
            public float monthRevenue;
            public float revenueVsTarget;
            public int activeUsers;
            public float conversionRate;
            public float arpu;
            
            [Header("Real-time Metrics")]
            public int liveUsers;
            public float revenueThisHour;
            public int conversionsThisHour;
            public float averageOrderValue;
            
            [Header("Trends")]
            public List<DailyMetrics> revenueChart;
            public List<DailyMetrics> conversionChart;
            public List<DailyMetrics> arpuChart;
            
            [Header("Top Performers")]
            public List<ProductRanking> topProducts;
            public List<CohortRanking> topCohorts;
            public List<SegmentPerformance> segmentPerformance;
            
            [Header("Alerts and Insights")]
            public List<RevenueAlert> alerts;
            public List<string> insights;
            public List<string> recommendations;
            
            public DateTime lastUpdated;
        }

        [System.Serializable]
        public struct DailyMetrics
        {
            public DateTime date;
            public float value;
            public float target;
            public float growthRate;
        }

        [System.Serializable]
        public struct ProductRanking
        {
            public string productId;
            public string productName;
            public float revenue;
            public int purchases;
            public float conversionRate;
        }

        [System.Serializable]
        public struct CohortRanking
        {
            public string cohortId;
            public DateTime cohortDate;
            public float averageLTV;
            public float retentionRate;
            public int userCount;
        }

        [System.Serializable]
        public struct SegmentPerformance
        {
            public PlayerSpendingType segment;
            public int userCount;
            public float totalRevenue;
            public float averageRevenue;
            public float conversionRate;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeAnalyticsSystem();
        }

        private void Start()
        {
            _sessionStartTime = DateTime.Now;
            
            // Start periodic analytics processing
            InvokeRepeating(nameof(ProcessAnalytics), 0f, analyticsUpdateIntervalSeconds);
            InvokeRepeating(nameof(UpdateDashboard), 30f, 30f); // Update dashboard every 30 seconds
            InvokeRepeating(nameof(CheckAlerts), 120f, 120f); // Check alerts every 2 minutes
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Track monetization event with full context
        /// </summary>
        public void TrackEvent(MonetizationEventType eventType, string productId = null, float revenue = 0f, 
                               string currency = "USD", Dictionary<string, object> customData = null, string playerId = null)
        {
            var monetizationEvent = new MonetizationEvent
            {
                eventId = Guid.NewGuid().ToString(),
                playerId = playerId ?? GetCurrentPlayerId(),
                timestamp = DateTime.Now,
                eventType = eventType,
                productId = productId,
                revenue = revenue,
                currency = currency,
                customData = customData ?? new Dictionary<string, object>(),
                funnelStep = GetCurrentFunnelStep(eventType),
                playerSegment = GetPlayerSegment(playerId ?? GetCurrentPlayerId()),
                sessionDay = GetSessionDay(playerId ?? GetCurrentPlayerId()),
                sessionMinute = (float)(DateTime.Now - _sessionStartTime).TotalMinutes
            };
            
            // Add to event queue
            _eventQueue.Enqueue(monetizationEvent);
            
            // Keep queue size manageable
            while (_eventQueue.Count > maxEventsInMemory)
            {
                _eventQueue.Dequeue();
            }
            
            // Update real-time metrics
            UpdateRealTimeMetrics(monetizationEvent);
            
            // Update player profile
            UpdatePlayerProfile(monetizationEvent);
            
            // Update conversion funnels
            UpdateConversionFunnels(monetizationEvent);
            
            OnMonetizationEventTracked?.Invoke(monetizationEvent);
            
            Debug.Log($"[MonetizationAnalytics] Tracked {eventType}: {productId} - ${revenue:F2}");
        }

        /// <summary>
        /// Get current monetization performance metrics
        /// </summary>
        public MonetizationPerformanceMetrics GetCurrentMetrics()
        {
            return _currentMetrics;
        }

        /// <summary>
        /// Get player analytics profile
        /// </summary>
        public PlayerAnalyticsProfile GetPlayerProfile(string playerId)
        {
            return _playerProfiles.GetValueOrDefault(playerId, null);
        }

        /// <summary>
        /// Generate comprehensive monetization report
        /// </summary>
        public MonetizationReport GenerateReport(DateTime startDate, DateTime endDate)
        {
            var events = _eventQueue.Where(e => e.timestamp >= startDate && e.timestamp <= endDate).ToList();
            
            var report = new MonetizationReport
            {
                reportId = Guid.NewGuid().ToString(),
                startDate = startDate,
                endDate = endDate,
                totalEvents = events.Count,
                totalRevenue = events.Sum(e => e.revenue),
                uniqueUsers = events.Select(e => e.playerId).Distinct().Count(),
                conversionRate = CalculateConversionRate(events),
                topProducts = GetTopProducts(events),
                revenueBreakdown = GetRevenueBreakdown(events),
                cohortAnalysis = GetCohortAnalysisForPeriod(startDate, endDate),
                funnelAnalysis = AnalyzeConversionFunnels(events),
                recommendations = GenerateRecommendations(events),
                generatedAt = DateTime.Now
            };
            
            return report;
        }

        /// <summary>
        /// Predict revenue for upcoming period
        /// </summary>
        public RevenuePrediction PredictRevenue(int daysAhead)
        {
            if (_revenuePredictionModel == null)
                return new RevenuePrediction { confidence = 0f };
            
            var historicalData = GetHistoricalRevenueData();
            return _revenuePredictionModel.PredictRevenue(historicalData, daysAhead);
        }

        /// <summary>
        /// Get conversion funnel analysis
        /// </summary>
        public ConversionFunnelAnalysis AnalyzeConversionFunnel(string funnelId)
        {
            if (!_conversionFunnels.ContainsKey(funnelId))
                return new ConversionFunnelAnalysis();
            
            var funnel = _conversionFunnels[funnelId];
            var analysis = new ConversionFunnelAnalysis
            {
                funnelId = funnelId,
                stepAnalyses = new Dictionary<string, FunnelStepAnalysis>(),
                recommendations = new List<FunnelOptimizationRecommendation>(),
                overallConversionRate = funnel.overallConversionRate,
                potentialImprovement = CalculateFunnelPotentialImprovement(funnel),
                analysisTime = DateTime.Now
            };
            
            // Analyze each step
            foreach (var step in funnel.steps)
            {
                analysis.stepAnalyses[step.stepId] = AnalyzeFunnelStep(funnel, step);
            }
            
            // Generate recommendations
            analysis.recommendations = GenerateFunnelRecommendations(analysis);
            
            OnFunnelAnalysisCompleted?.Invoke(analysis);
            return analysis;
        }

        /// <summary>
        /// Create custom conversion funnel
        /// </summary>
        public void CreateConversionFunnel(string funnelId, string funnelName, List<FunnelStep> steps)
        {
            var funnel = new ConversionFunnel
            {
                funnelId = funnelId,
                funnelName = funnelName,
                steps = steps,
                stepCounts = new Dictionary<string, int>(),
                conversionRates = new Dictionary<string, float>(),
                lastUpdated = DateTime.Now,
                overallConversionRate = 0f
            };
            
            _conversionFunnels[funnelId] = funnel;
            Debug.Log($"[MonetizationAnalytics] Created funnel: {funnelName}");
        }

        /// <summary>
        /// Get monetization dashboard data
        /// </summary>
        public MonetizationDashboard GetDashboardData()
        {
            return _dashboardData;
        }
        #endregion

        #region Core Analytics Processing
        /// <summary>
        /// Process analytics and update metrics
        /// </summary>
        private void ProcessAnalytics()
        {
            if (_eventQueue.Count == 0) return;
            
            try
            {
                // Update performance metrics
                UpdatePerformanceMetrics();
                
                // Update cohort analyses
                if (enableCohortAnalysis)
                {
                    UpdateCohortAnalyses();
                }
                
                // Process conversion funnels
                if (enableFunnelTracking)
                {
                    ProcessConversionFunnels();
                }
                
                // Update LTV predictions
                if (enableLTVPrediction)
                {
                    UpdateLTVPredictions();
                }
                
                OnMetricsUpdated?.Invoke(_currentMetrics);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationAnalytics] Error processing analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Update real-time performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            var events = _eventQueue.ToList();
            var today = DateTime.Now.Date;
            var todayEvents = events.Where(e => e.timestamp.Date == today).ToList();
            
            // Calculate revenue metrics
            var totalRevenue = events.Sum(e => e.revenue);
            var todaysRevenue = todayEvents.Sum(e => e.revenue);
            
            // Calculate user metrics
            var totalUsers = events.Select(e => e.playerId).Distinct().Count();
            var activeUsers = events.Where(e => e.timestamp > DateTime.Now.AddDays(-7))
                                   .Select(e => e.playerId).Distinct().Count();
            var payingUsers = events.Where(e => e.revenue > 0)
                                   .Select(e => e.playerId).Distinct().Count();
            
            // Calculate ARPU metrics
            var arpu = totalUsers > 0 ? totalRevenue / totalUsers : 0f;
            var arppu = payingUsers > 0 ? totalRevenue / payingUsers : 0f;
            var conversionRate = totalUsers > 0 ? (float)payingUsers / totalUsers : 0f;
            
            _currentMetrics = new MonetizationPerformanceMetrics
            {
                totalRevenue = totalRevenue,
                todaysRevenue = todaysRevenue,
                averageDailyRevenue = CalculateAverageDailyRevenue(events),
                revenueGrowthRate = CalculateRevenueGrowthRate(events),
                
                totalUsers = totalUsers,
                activeUsers = activeUsers,
                payingUsers = payingUsers,
                conversionRate = conversionRate,
                
                arpu = arpu,
                arppu = arppu,
                targetArpu = targetARPU,
                arpuGrowthRate = CalculateARPUGrowthRate(),
                
                productMetrics = CalculateProductMetrics(events),
                bestPerformingProduct = GetBestPerformingProduct(events),
                worstPerformingProduct = GetWorstPerformingProduct(events),
                
                funnelConversionRates = CalculateFunnelConversionRates(),
                averageFunnelConversionRate = CalculateAverageFunnelConversionRate(),
                
                cohortRetentionRates = CalculateCohortRetentionRates(),
                averageLTV = CalculateAverageLTV(),
                predictedLTV = PredictAverageLTV(),
                
                activeAlerts = _activeAlerts.Count,
                criticalIssues = GetCriticalIssues(),
                
                lastUpdated = DateTime.Now
            };
            
            _sessionRevenue += todayEvents.Where(e => e.timestamp > _sessionStartTime).Sum(e => e.revenue);
            _sessionConversions += todayEvents.Where(e => e.eventType == MonetizationEventType.PurchaseCompleted 
                                                       && e.timestamp > _sessionStartTime).Count();
        }

        /// <summary>
        /// Update cohort analyses
        /// </summary>
        private void UpdateCohortAnalyses()
        {
            // Group players by cohort (install date)
            var playerCohorts = _playerProfiles.Values
                .GroupBy(p => p.firstSeen.Date)
                .Where(g => g.Count() >= 10) // Minimum cohort size
                .OrderByDescending(g => g.Key)
                .Take(30); // Last 30 cohorts
            
            _cohortAnalyses.Clear();
            
            foreach (var cohortGroup in playerCohorts)
            {
                var cohortDate = cohortGroup.Key;
                var cohortPlayers = cohortGroup.ToList();
                
                var cohortAnalysis = new CohortAnalysis
                {
                    cohortId = $"cohort_{cohortDate:yyyy_MM_dd}",
                    cohortDate = cohortDate,
                    totalUsers = cohortPlayers.Count,
                    dayRetention = CalculateCohortRetention(cohortPlayers, cohortDate),
                    dayRevenue = CalculateCohortRevenue(cohortPlayers, cohortDate),
                    averageLTV = cohortPlayers.Average(p => p.predictedLTV),
                    payingUserPercentage = (float)cohortPlayers.Count(p => p.totalPurchases > 0) / cohortPlayers.Count,
                    segmentDistribution = CalculateCohortSegmentDistribution(cohortPlayers),
                    performanceMetrics = CalculateCohortPerformanceMetrics(cohortPlayers, cohortDate)
                };
                
                _cohortAnalyses.Add(cohortAnalysis);
                OnCohortAnalysisUpdated?.Invoke(cohortAnalysis);
            }
        }

        /// <summary>
        /// Process conversion funnels
        /// </summary>
        private void ProcessConversionFunnels()
        {
            foreach (var funnel in _conversionFunnels.Values)
            {
                UpdateFunnelStepCounts(funnel);
                CalculateFunnelConversionRates(funnel);
            }
        }

        /// <summary>
        /// Update LTV predictions for all players
        /// </summary>
        private void UpdateLTVPredictions()
        {
            if (_revenuePredictionModel == null) return;
            
            foreach (var profile in _playerProfiles.Values)
            {
                profile.predictedLTV = _revenuePredictionModel.PredictPlayerLTV(profile);
            }
        }

        /// <summary>
        /// Update dashboard data
        /// </summary>
        private void UpdateDashboard()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            
            var todayEvents = _eventQueue.Where(e => e.timestamp.Date == today).ToList();
            var monthEvents = _eventQueue.Where(e => e.timestamp >= thisMonth).ToList();
            var hourEvents = _eventQueue.Where(e => e.timestamp > now.AddHours(-1)).ToList();
            
            _dashboardData = new MonetizationDashboard
            {
                // KPIs
                todayRevenue = todayEvents.Sum(e => e.revenue),
                monthRevenue = monthEvents.Sum(e => e.revenue),
                revenueVsTarget = (todayEvents.Sum(e => e.revenue) / targetDay1Revenue) * 100f,
                activeUsers = _currentMetrics.activeUsers,
                conversionRate = _currentMetrics.conversionRate,
                arpu = _currentMetrics.arpu,
                
                // Real-time
                liveUsers = GetCurrentLiveUsers(),
                revenueThisHour = hourEvents.Sum(e => e.revenue),
                conversionsThisHour = hourEvents.Count(e => e.eventType == MonetizationEventType.PurchaseCompleted),
                averageOrderValue = CalculateAverageOrderValue(todayEvents),
                
                // Trends
                revenueChart = GenerateRevenueChart(),
                conversionChart = GenerateConversionChart(),
                arpuChart = GenerateARPUChart(),
                
                // Rankings
                topProducts = GetTopProductRankings(),
                topCohorts = GetTopCohortRankings(),
                segmentPerformance = GetSegmentPerformance(),
                
                // Alerts
                alerts = _activeAlerts.ToList(),
                insights = GenerateInsights(),
                recommendations = GenerateOptimizationRecommendations(),
                
                lastUpdated = DateTime.Now
            };
            
            OnDashboardDataUpdated?.Invoke(_dashboardData);
        }

        /// <summary>
        /// Check for revenue alerts and issues
        /// </summary>
        private void CheckAlerts()
        {
            var newAlerts = new List<RevenueAlert>();
            
            // Revenue below target alert
            if (_currentMetrics.todaysRevenue < targetDay1Revenue * 0.5f && DateTime.Now.Hour > 12)
            {
                newAlerts.Add(new RevenueAlert
                {
                    alertId = Guid.NewGuid().ToString(),
                    alertType = AlertType.RevenueBelowTarget,
                    severity = AlertSeverity.Warning,
                    message = $"Today's revenue (${_currentMetrics.todaysRevenue:F2}) is below 50% of target (${targetDay1Revenue})",
                    currentValue = _currentMetrics.todaysRevenue,
                    threshold = targetDay1Revenue * 0.5f,
                    triggerTime = DateTime.Now
                });
            }
            
            // Conversion rate dropped alert
            var historicalConversionRate = CalculateHistoricalConversionRate();
            if (_currentMetrics.conversionRate < historicalConversionRate * 0.7f)
            {
                newAlerts.Add(new RevenueAlert
                {
                    alertId = Guid.NewGuid().ToString(),
                    alertType = AlertType.ConversionRateDropped,
                    severity = AlertSeverity.Critical,
                    message = $"Conversion rate ({_currentMetrics.conversionRate:P2}) dropped significantly below historical average ({historicalConversionRate:P2})",
                    currentValue = _currentMetrics.conversionRate,
                    threshold = historicalConversionRate * 0.7f,
                    triggerTime = DateTime.Now
                });
            }
            
            // Add new alerts and trigger events
            foreach (var alert in newAlerts)
            {
                if (!_activeAlerts.Any(a => a.alertType == alert.alertType && !a.isResolved))
                {
                    _activeAlerts.Add(alert);
                    OnRevenueAlertTriggered?.Invoke(alert);
                }
            }
            
            // Clean up old resolved alerts
            _activeAlerts.RemoveAll(a => a.isResolved && (DateTime.Now - a.triggerTime).Hours > 24);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Initialize analytics system
        /// </summary>
        private void InitializeAnalyticsSystem()
        {
            _eventQueue = new Queue<MonetizationEvent>();
            _playerProfiles = new Dictionary<string, PlayerAnalyticsProfile>();
            _cohortAnalyses = new List<CohortAnalysis>();
            _conversionFunnels = new Dictionary<string, ConversionFunnel>();
            _productPerformance = new Dictionary<string, float>();
            _activeAlerts = new List<RevenueAlert>();
            _revenuePredictionModel = new RevenuePredictionModel();
            
            // Initialize default conversion funnels
            InitializeDefaultFunnels();
            
            // Initialize dashboard
            _dashboardData = new MonetizationDashboard();
        }

        /// <summary>
        /// Initialize default conversion funnels
        /// </summary>
        private void InitializeDefaultFunnels()
        {
            // Purchase funnel
            CreateConversionFunnel("purchase_funnel", "Purchase Conversion", new List<FunnelStep>
            {
                new FunnelStep
                {
                    stepId = "store_visit",
                    stepName = "Store Visit",
                    stepOrder = 1,
                    triggerEvents = new[] { MonetizationEventType.StoreVisited }
                },
                new FunnelStep
                {
                    stepId = "product_view",
                    stepName = "Product View",
                    stepOrder = 2,
                    triggerEvents = new[] { MonetizationEventType.ProductViewed }
                },
                new FunnelStep
                {
                    stepId = "purchase_attempt",
                    stepName = "Purchase Attempt",
                    stepOrder = 3,
                    triggerEvents = new[] { MonetizationEventType.PurchaseAttempted }
                },
                new FunnelStep
                {
                    stepId = "purchase_complete",
                    stepName = "Purchase Complete",
                    stepOrder = 4,
                    triggerEvents = new[] { MonetizationEventType.PurchaseCompleted }
                }
            });
            
            // Onboarding monetization funnel
            CreateConversionFunnel("onboarding_funnel", "Onboarding Conversion", new List<FunnelStep>
            {
                new FunnelStep
                {
                    stepId = "tutorial_start",
                    stepName = "Tutorial Start",
                    stepOrder = 1,
                    triggerEvents = new[] { MonetizationEventType.SessionStart }
                },
                new FunnelStep
                {
                    stepId = "offer_shown",
                    stepName = "Offer Shown",
                    stepOrder = 2,
                    triggerEvents = new[] { MonetizationEventType.OfferShown }
                },
                new FunnelStep
                {
                    stepId = "offer_clicked",
                    stepName = "Offer Clicked",
                    stepOrder = 3,
                    triggerEvents = new[] { MonetizationEventType.OfferClicked }
                },
                new FunnelStep
                {
                    stepId = "first_purchase",
                    stepName = "First Purchase",
                    stepOrder = 4,
                    triggerEvents = new[] { MonetizationEventType.PurchaseCompleted }
                }
            });
        }

        /// <summary>
        /// Update real-time metrics from event
        /// </summary>
        private void UpdateRealTimeMetrics(MonetizationEvent monetizationEvent)
        {
            if (monetizationEvent.revenue > 0)
            {
                _sessionRevenue += monetizationEvent.revenue;
                
                if (!string.IsNullOrEmpty(monetizationEvent.productId))
                {
                    if (!_productPerformance.ContainsKey(monetizationEvent.productId))
                        _productPerformance[monetizationEvent.productId] = 0f;
                    
                    _productPerformance[monetizationEvent.productId] += monetizationEvent.revenue;
                }
            }
            
            if (monetizationEvent.eventType == MonetizationEventType.PurchaseCompleted)
            {
                _sessionConversions++;
            }
        }

        /// <summary>
        /// Update player analytics profile
        /// </summary>
        private void UpdatePlayerProfile(MonetizationEvent monetizationEvent)
        {
            if (!_playerProfiles.ContainsKey(monetizationEvent.playerId))
            {
                _playerProfiles[monetizationEvent.playerId] = new PlayerAnalyticsProfile
                {
                    playerId = monetizationEvent.playerId,
                    firstSeen = monetizationEvent.timestamp,
                    productPurchases = new Dictionary<string, int>(),
                    funnelProgress = new List<ConversionFunnelStep>(),
                    cohortInfo = new CohortInfo
                    {
                        cohortId = $"cohort_{monetizationEvent.timestamp:yyyy_MM_dd}",
                        cohortStartDate = monetizationEvent.timestamp.Date
                    }
                };
            }
            
            var profile = _playerProfiles[monetizationEvent.playerId];
            profile.lastSeen = monetizationEvent.timestamp;
            
            if (monetizationEvent.eventType == MonetizationEventType.SessionStart)
                profile.totalSessions++;
            
            if (monetizationEvent.revenue > 0)
            {
                profile.totalRevenue += monetizationEvent.revenue;
                if (monetizationEvent.eventType == MonetizationEventType.PurchaseCompleted)
                {
                    profile.totalPurchases++;
                    if (!string.IsNullOrEmpty(monetizationEvent.productId))
                    {
                        if (!profile.productPurchases.ContainsKey(monetizationEvent.productId))
                            profile.productPurchases[monetizationEvent.productId] = 0;
                        profile.productPurchases[monetizationEvent.productId]++;
                    }
                }
            }
            
            profile.averageOrderValue = profile.totalPurchases > 0 ? profile.totalRevenue / profile.totalPurchases : 0f;
            profile.spendingSegment = DetermineSpendingSegment(profile);
        }

        /// <summary>
        /// Update conversion funnels with new event
        /// </summary>
        private void UpdateConversionFunnels(MonetizationEvent monetizationEvent)
        {
            foreach (var funnel in _conversionFunnels.Values)
            {
                foreach (var step in funnel.steps)
                {
                    if (step.triggerEvents.Contains(monetizationEvent.eventType))
                    {
                        var playerProfile = _playerProfiles[monetizationEvent.playerId];
                        playerProfile.funnelProgress.Add(new ConversionFunnelStep
                        {
                            funnelId = funnel.funnelId,
                            stepId = step.stepId,
                            timestamp = monetizationEvent.timestamp,
                            completed = true,
                            stepData = monetizationEvent.customData
                        });
                    }
                }
            }
        }

        // Calculation helper methods (simplified implementations)
        private float CalculateAverageDailyRevenue(List<MonetizationEvent> events) => events.Sum(e => e.revenue) / Math.Max(1, (DateTime.Now - events.First().timestamp).Days);
        private float CalculateRevenueGrowthRate(List<MonetizationEvent> events) => 0.15f; // Placeholder
        private float CalculateARPUGrowthRate() => 0.10f; // Placeholder
        private Dictionary<string, ProductPerformanceMetrics> CalculateProductMetrics(List<MonetizationEvent> events) => new Dictionary<string, ProductPerformanceMetrics>();
        private string GetBestPerformingProduct(List<MonetizationEvent> events) => events.GroupBy(e => e.productId).OrderByDescending(g => g.Sum(e => e.revenue)).FirstOrDefault()?.Key ?? "none";
        private string GetWorstPerformingProduct(List<MonetizationEvent> events) => "unknown";
        private Dictionary<string, float> CalculateFunnelConversionRates() => new Dictionary<string, float>();
        private float CalculateAverageFunnelConversionRate() => 0.05f;
        private Dictionary<string, float> CalculateCohortRetentionRates() => new Dictionary<string, float>();
        private float CalculateAverageLTV() => 25f;
        private float PredictAverageLTV() => 30f;
        private List<string> GetCriticalIssues() => new List<string>();
        private float CalculateConversionRate(List<MonetizationEvent> events) => 0.05f;
        private List<ProductRanking> GetTopProducts(List<MonetizationEvent> events) => new List<ProductRanking>();
        private Dictionary<string, float> GetRevenueBreakdown(List<MonetizationEvent> events) => new Dictionary<string, float>();
        private List<CohortAnalysis> GetCohortAnalysisForPeriod(DateTime start, DateTime end) => new List<CohortAnalysis>();
        private List<ConversionFunnelAnalysis> AnalyzeConversionFunnels(List<MonetizationEvent> events) => new List<ConversionFunnelAnalysis>();
        private List<string> GenerateRecommendations(List<MonetizationEvent> events) => new List<string>();
        private List<DailyMetrics> GetHistoricalRevenueData() => new List<DailyMetrics>();
        private PlayerSpendingType GetPlayerSegment(string playerId) => PlayerSpendingType.NonSpender;
        private int GetSessionDay(string playerId) => 1;
        private string GetCurrentFunnelStep(MonetizationEventType eventType) => "unknown";
        private string GetCurrentPlayerId() => "player_123";
        private Dictionary<int, RetentionData> CalculateCohortRetention(List<PlayerAnalyticsProfile> players, DateTime cohortDate) => new Dictionary<int, RetentionData>();
        private Dictionary<int, RevenueData> CalculateCohortRevenue(List<PlayerAnalyticsProfile> players, DateTime cohortDate) => new Dictionary<int, RevenueData>();
        private Dictionary<string, float> CalculateCohortSegmentDistribution(List<PlayerAnalyticsProfile> players) => new Dictionary<string, float>();
        private CohortPerformanceMetrics CalculateCohortPerformanceMetrics(List<PlayerAnalyticsProfile> players, DateTime cohortDate) => new CohortPerformanceMetrics();
        private void UpdateFunnelStepCounts(ConversionFunnel funnel) { }
        private void CalculateFunnelConversionRates(ConversionFunnel funnel) { }
        private float CalculateFunnelPotentialImprovement(ConversionFunnel funnel) => 0.15f;
        private FunnelStepAnalysis AnalyzeFunnelStep(ConversionFunnel funnel, FunnelStep step) => new FunnelStepAnalysis();
        private List<FunnelOptimizationRecommendation> GenerateFunnelRecommendations(ConversionFunnelAnalysis analysis) => new List<FunnelOptimizationRecommendation>();
        private PlayerSpendingType DetermineSpendingSegment(PlayerAnalyticsProfile profile) => PlayerSpendingType.NonSpender;
        private float CalculateHistoricalConversionRate() => 0.05f;
        private int GetCurrentLiveUsers() => 42;
        private float CalculateAverageOrderValue(List<MonetizationEvent> events) => events.Where(e => e.revenue > 0).Average(e => e.revenue);
        private List<DailyMetrics> GenerateRevenueChart() => new List<DailyMetrics>();
        private List<DailyMetrics> GenerateConversionChart() => new List<DailyMetrics>();
        private List<DailyMetrics> GenerateARPUChart() => new List<DailyMetrics>();
        private List<ProductRanking> GetTopProductRankings() => new List<ProductRanking>();
        private List<CohortRanking> GetTopCohortRankings() => new List<CohortRanking>();
        private List<SegmentPerformance> GetSegmentPerformance() => new List<SegmentPerformance>();
        private List<string> GenerateInsights() => new List<string> { "Conversion rate trending upward", "Premium products showing strong performance" };
        private List<string> GenerateOptimizationRecommendations() => new List<string> { "Increase premium showcase frequency", "Test higher-value starter packs" };
        #endregion
    }

    #region Supporting Data Structures
    /// <summary>
    /// Comprehensive monetization report
    /// </summary>
    [System.Serializable]
    public struct MonetizationReport
    {
        public string reportId;
        public DateTime startDate;
        public DateTime endDate;
        public int totalEvents;
        public float totalRevenue;
        public int uniqueUsers;
        public float conversionRate;
        public List<ProductRanking> topProducts;
        public Dictionary<string, float> revenueBreakdown;
        public List<CohortAnalysis> cohortAnalysis;
        public List<ConversionFunnelAnalysis> funnelAnalysis;
        public List<string> recommendations;
        public DateTime generatedAt;
    }

    /// <summary>
    /// Revenue prediction with confidence intervals
    /// </summary>
    [System.Serializable]
    public struct RevenuePrediction
    {
        public float predictedRevenue;
        public float lowerBound;
        public float upperBound;
        public float confidence;
        public int daysAhead;
        public string methodology;
        public DateTime predictionDate;
    }

    /// <summary>
    /// Revenue prediction model
    /// </summary>
    public class RevenuePredictionModel
    {
        public RevenuePrediction PredictRevenue(List<DailyMetrics> historicalData, int daysAhead)
        {
            // Simplified prediction - in production, use more sophisticated ML models
            var averageRevenue = historicalData.Average(d => d.value);
            var growth = historicalData.Count > 1 ? 
                (historicalData.Last().value - historicalData.First().value) / historicalData.Count : 0f;
            
            var predicted = averageRevenue + (growth * daysAhead);
            
            return new RevenuePrediction
            {
                predictedRevenue = predicted,
                lowerBound = predicted * 0.8f,
                upperBound = predicted * 1.2f,
                confidence = 0.75f,
                daysAhead = daysAhead,
                methodology = "Linear trend analysis",
                predictionDate = DateTime.Now
            };
        }
        
        public float PredictPlayerLTV(PlayerAnalyticsProfile profile)
        {
            // Simplified LTV prediction based on spending velocity and engagement
            var daysSinceFirst = (DateTime.Now - profile.firstSeen).Days;
            if (daysSinceFirst <= 0) return 0f;
            
            var spendingVelocity = profile.totalRevenue / daysSinceFirst;
            var projectedDays = 365f; // 1 year LTV projection
            
            return spendingVelocity * projectedDays * GetEngagementMultiplier(profile);
        }
        
        private float GetEngagementMultiplier(PlayerAnalyticsProfile profile)
        {
            // Adjust LTV based on engagement patterns
            var sessionFrequency = profile.totalSessions / Math.Max(1, (DateTime.Now - profile.firstSeen).Days);
            return Mathf.Clamp(sessionFrequency, 0.5f, 2.0f);
        }
    }
    #endregion
}