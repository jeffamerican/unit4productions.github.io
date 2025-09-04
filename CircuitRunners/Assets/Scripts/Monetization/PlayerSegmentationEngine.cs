using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CircuitRunners.Monetization;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Advanced player segmentation engine for precise monetization targeting
    /// Uses behavioral analytics, spending patterns, and engagement metrics for dynamic segmentation
    /// Target: Increase ARPU by 40% through personalized monetization strategies
    /// </summary>
    public class PlayerSegmentationEngine : MonoBehaviour
    {
        #region Configuration
        [Header("Segmentation Settings")]
        [SerializeField] private bool enableRealTimeSegmentation = true;
        [SerializeField] private bool enablePredictiveSegmentation = true;
        [SerializeField] private bool enableBehavioralTracking = true;
        [SerializeField] private float segmentUpdateFrequencyHours = 6f;
        
        [Header("Whale Detection")]
        [SerializeField] private float whaleSpendingThreshold = 200f;
        [SerializeField] private float dolphinSpendingThreshold = 50f;
        [SerializeField] private int whaleActivityThreshold = 20; // sessions per week
        
        [Header("Engagement Thresholds")]
        [SerializeField] private float highEngagementMinutes = 30f;
        [SerializeField] private int retentionBenchmarkDays = 7;
        [SerializeField] private float churnWarningThreshold = 0.7f;
        #endregion

        #region Private Fields
        private Dictionary<string, PlayerSegmentProfile> _playerSegments;
        private Dictionary<string, MonetizationStrategy> _segmentStrategies;
        private List<SegmentationRule> _segmentationRules;
        private PredictiveModel _churnPredictionModel;
        private PredictiveModel _spendingPredictionModel;
        private DateTime _lastSegmentUpdate;
        #endregion

        #region Events
        public static event Action<PlayerSegmentProfile> OnPlayerSegmentChanged;
        public static event Action<WhaleDetectionEvent> OnWhaleDetected;
        public static event Action<ChurnRiskEvent> OnChurnRiskIdentified;
        public static event Action<SegmentationMetrics> OnSegmentationMetricsUpdated;
        #endregion

        #region Data Structures
        [System.Serializable]
        public class PlayerSegmentProfile
        {
            public string playerId;
            public PlayerSegmentType primarySegment;
            public List<PlayerSegmentType> secondarySegments;
            public float segmentConfidence;
            public DateTime lastUpdated;
            
            [Header("Behavioral Data")]
            public BehavioralMetrics behavior;
            public SpendingMetrics spending;
            public EngagementMetrics engagement;
            public SocialMetrics social;
            public CompetitiveMetrics competitive;
            
            [Header("Predictive Scores")]
            public float churnRisk; // 0-1, higher = more likely to churn
            public float spendingPotential; // 0-1, higher = more likely to spend
            public float viralityScore; // 0-1, higher = more likely to share/refer
            public float competitivenessScore; // 0-1, higher = more competitive
            
            [Header("Monetization Preferences")]
            public List<MonetizationChannel> preferredChannels;
            public List<string> respondsToOffers;
            public Dictionary<string, float> productAffinities;
            public float pricesensitivity; // 0-1, higher = more price sensitive
        }

        public enum PlayerSegmentType
        {
            // Spending-based segments
            Whale,              // High spender, high engagement
            Dolphin,            // Medium spender, consistent engagement
            Minnow,             // Low spender, occasional purchases
            FreeRider,          // No spending, high engagement
            
            // Behavior-based segments
            Competitor,         // Focused on competitive gameplay
            Collector,          // Wants to collect all bots/parts
            Casual,             // Plays occasionally, low commitment
            Achiever,           // Goal-oriented, achievement focused
            Social,             // Social features are primary motivation
            
            // Lifecycle-based segments
            NewPlayer,          // Recently installed, learning
            GrowingPlayer,      // Engaged, increasing activity
            EstablishedPlayer,  // Long-term stable player
            ChurningPlayer,     // Declining engagement, at risk
            
            // Special segments
            Influencer,         // High social reach/viral potential
            Comeback,           // Returned after period of absence
            VIP,                // Top-tier treatment required
            Unknown             // Insufficient data for classification
        }

        public enum MonetizationChannel
        {
            DirectPurchase,     // Store IAP
            RewardedAds,        // Ad-based rewards
            BattlePass,         // Subscription-style progression
            LimitedOffers,      // Time-limited deals
            SocialOffers,       // Friend referrals, etc.
            CompetitiveRewards, // Tournament entry fees
            Customization,      // Cosmetic purchases
            Convenience        // Time-saving purchases
        }

        [System.Serializable]
        public struct BehavioralMetrics
        {
            public int totalSessions;
            public float averageSessionLength;
            public int daysPlayed;
            public int consecutiveDays;
            public DateTime lastActive;
            public List<string> favoriteFeatures;
            public Dictionary<string, int> featureUsage;
            public float sessionConsistency; // 0-1, higher = more consistent play times
        }

        [System.Serializable]
        public struct SpendingMetrics
        {
            public float lifetimeValue;
            public float averageOrderValue;
            public int totalPurchases;
            public DateTime firstPurchaseDate;
            public DateTime lastPurchaseDate;
            public List<string> purchasedProducts;
            public float spendingVelocity; // $ per day since first purchase
            public Dictionary<string, float> categorySpending;
        }

        [System.Serializable]
        public struct EngagementMetrics
        {
            public float day1Retention;
            public float day7Retention;
            public float day30Retention;
            public int totalGameplayHours;
            public int levelsCompleted;
            public int achievementsUnlocked;
            public float engagementTrend; // -1 to 1, negative = declining
            public List<DateTime> sessionStartTimes;
        }

        [System.Serializable]
        public struct SocialMetrics
        {
            public int friendsInvited;
            public int friendsAccepted;
            public int socialShares;
            public float socialEngagementRate;
            public bool hasReviewed;
            public int forumPosts;
            public float influenceScore;
        }

        [System.Serializable]
        public struct CompetitiveMetrics
        {
            public int rankedGamesPlayed;
            public float currentRating;
            public float peakRating;
            public int tournamentsEntered;
            public int wins;
            public int losses;
            public float competitiveWinRate;
            public bool prefersPvP;
        }

        [System.Serializable]
        public class MonetizationStrategy
        {
            public PlayerSegmentType targetSegment;
            public string strategyName;
            public List<MonetizationTactic> tactics;
            public Dictionary<string, float> productRecommendations;
            public float targetConversionRate;
            public float expectedARPU;
            public string communicationStyle;
            public List<string> preferredOfferTypes;
        }

        [System.Serializable]
        public struct MonetizationTactic
        {
            public string tacticId;
            public string tacticName;
            public MonetizationChannel channel;
            public float successRate;
            public Dictionary<string, object> parameters;
        }

        [System.Serializable]
        public class SegmentationRule
        {
            public string ruleId;
            public string ruleName;
            public PlayerSegmentType targetSegment;
            public List<SegmentationCriteria> criteria;
            public float minConfidenceScore;
            public bool isExclusiveSegment;
        }

        [System.Serializable]
        public struct SegmentationCriteria
        {
            public string metricName;
            public ComparisonOperator operation;
            public float threshold;
            public float weight; // Importance of this criteria (0-1)
        }

        public enum ComparisonOperator
        {
            GreaterThan,
            LessThan,
            Equal,
            Between,
            Contains
        }

        public struct WhaleDetectionEvent
        {
            public string playerId;
            public float lifetimeValue;
            public float spendingVelocity;
            public PlayerSegmentType previousSegment;
            public DateTime detectionTime;
        }

        public struct ChurnRiskEvent
        {
            public string playerId;
            public float churnProbability;
            public List<string> riskFactors;
            public PlayerSegmentType currentSegment;
            public DateTime identificationTime;
        }

        public struct SegmentationMetrics
        {
            public Dictionary<PlayerSegmentType, int> segmentDistribution;
            public Dictionary<PlayerSegmentType, float> segmentARPU;
            public Dictionary<PlayerSegmentType, float> segmentRetention;
            public float overallSegmentationAccuracy;
            public DateTime lastUpdate;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _playerSegments = new Dictionary<string, PlayerSegmentProfile>();
            _segmentStrategies = new Dictionary<string, MonetizationStrategy>();
            _segmentationRules = new List<SegmentationRule>();
            
            InitializeSegmentationRules();
            InitializeMonetizationStrategies();
            InitializePredictiveModels();
        }

        private void Start()
        {
            if (enableRealTimeSegmentation)
            {
                InvokeRepeating(nameof(UpdatePlayerSegments), 0f, segmentUpdateFrequencyHours * 3600f);
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get current player segment with full profile
        /// </summary>
        public PlayerSegmentProfile GetPlayerSegment(string playerId)
        {
            if (_playerSegments.ContainsKey(playerId))
            {
                var profile = _playerSegments[playerId];
                
                // Update if data is stale
                if (DateTime.Now - profile.lastUpdated > TimeSpan.FromHours(segmentUpdateFrequencyHours))
                {
                    profile = UpdatePlayerSegment(playerId);
                }
                
                return profile;
            }
            
            // Create new profile for unknown player
            return CreateNewPlayerProfile(playerId);
        }

        /// <summary>
        /// Get monetization strategy for player segment
        /// </summary>
        public MonetizationStrategy GetSegmentStrategy(PlayerSegmentType segment)
        {
            var strategyKey = segment.ToString();
            return _segmentStrategies.ContainsKey(strategyKey) ? _segmentStrategies[strategyKey] : GetDefaultStrategy();
        }

        /// <summary>
        /// Update player behavioral data
        /// </summary>
        public void UpdatePlayerBehavior(string playerId, BehavioralMetrics behavior)
        {
            var profile = GetPlayerSegment(playerId);
            profile.behavior = behavior;
            
            // Recalculate segment if significant behavior change
            var newSegment = CalculatePlayerSegment(profile);
            if (newSegment != profile.primarySegment)
            {
                var previousSegment = profile.primarySegment;
                profile.primarySegment = newSegment;
                profile.lastUpdated = DateTime.Now;
                
                _playerSegments[playerId] = profile;
                OnPlayerSegmentChanged?.Invoke(profile);
                
                Debug.Log($"[PlayerSegmentationEngine] Player {playerId} moved from {previousSegment} to {newSegment}");
            }
        }

        /// <summary>
        /// Update player spending data
        /// </summary>
        public void UpdatePlayerSpending(string playerId, SpendingMetrics spending)
        {
            var profile = GetPlayerSegment(playerId);
            var previousLTV = profile.spending.lifetimeValue;
            profile.spending = spending;
            
            // Check for whale detection
            if (spending.lifetimeValue >= whaleSpendingThreshold && previousLTV < whaleSpendingThreshold)
            {
                OnWhaleDetected?.Invoke(new WhaleDetectionEvent
                {
                    playerId = playerId,
                    lifetimeValue = spending.lifetimeValue,
                    spendingVelocity = spending.spendingVelocity,
                    previousSegment = profile.primarySegment,
                    detectionTime = DateTime.Now
                });
            }
            
            // Recalculate segment
            var newSegment = CalculatePlayerSegment(profile);
            if (newSegment != profile.primarySegment)
            {
                profile.primarySegment = newSegment;
                profile.lastUpdated = DateTime.Now;
                _playerSegments[playerId] = profile;
                OnPlayerSegmentChanged?.Invoke(profile);
            }
        }

        /// <summary>
        /// Predict churn risk for player
        /// </summary>
        public float PredictChurnRisk(string playerId)
        {
            var profile = GetPlayerSegment(playerId);
            
            if (enablePredictiveSegmentation && _churnPredictionModel != null)
            {
                profile.churnRisk = _churnPredictionModel.Predict(profile);
                
                if (profile.churnRisk > churnWarningThreshold)
                {
                    OnChurnRiskIdentified?.Invoke(new ChurnRiskEvent
                    {
                        playerId = playerId,
                        churnProbability = profile.churnRisk,
                        riskFactors = GetChurnRiskFactors(profile),
                        currentSegment = profile.primarySegment,
                        identificationTime = DateTime.Now
                    });
                }
            }
            
            return profile.churnRisk;
        }

        /// <summary>
        /// Get personalized monetization recommendations
        /// </summary>
        public List<MonetizationRecommendation> GetMonetizationRecommendations(string playerId)
        {
            var profile = GetPlayerSegment(playerId);
            var strategy = GetSegmentStrategy(profile.primarySegment);
            var recommendations = new List<MonetizationRecommendation>();
            
            foreach (var tactic in strategy.tactics)
            {
                var recommendation = new MonetizationRecommendation
                {
                    tacticId = tactic.tacticId,
                    channel = tactic.channel,
                    priority = CalculateRecommendationPriority(profile, tactic),
                    expectedConversionRate = tactic.successRate * profile.segmentConfidence,
                    personalizedMessage = GeneratePersonalizedMessage(profile, tactic),
                    products = GetRecommendedProducts(profile, strategy)
                };
                
                recommendations.Add(recommendation);
            }
            
            return recommendations.OrderByDescending(r => r.priority).ToList();
        }
        #endregion

        #region Core Segmentation Logic
        /// <summary>
        /// Calculate player segment based on all available metrics
        /// </summary>
        private PlayerSegmentType CalculatePlayerSegment(PlayerSegmentProfile profile)
        {
            var scores = new Dictionary<PlayerSegmentType, float>();
            
            foreach (var rule in _segmentationRules)
            {
                var confidence = EvaluateSegmentationRule(profile, rule);
                if (confidence >= rule.minConfidenceScore)
                {
                    scores[rule.targetSegment] = Math.Max(scores.GetValueOrDefault(rule.targetSegment, 0f), confidence);
                }
            }
            
            // Return segment with highest confidence
            if (scores.Count > 0)
            {
                var bestSegment = scores.OrderByDescending(kvp => kvp.Value).First();
                profile.segmentConfidence = bestSegment.Value;
                return bestSegment.Key;
            }
            
            return PlayerSegmentType.Unknown;
        }

        /// <summary>
        /// Evaluate segmentation rule against player profile
        /// </summary>
        private float EvaluateSegmentationRule(PlayerSegmentProfile profile, SegmentationRule rule)
        {
            float totalWeight = 0f;
            float scoreSum = 0f;
            
            foreach (var criteria in rule.criteria)
            {
                var score = EvaluateCriteria(profile, criteria);
                scoreSum += score * criteria.weight;
                totalWeight += criteria.weight;
            }
            
            return totalWeight > 0 ? scoreSum / totalWeight : 0f;
        }

        /// <summary>
        /// Evaluate individual segmentation criteria
        /// </summary>
        private float EvaluateCriteria(PlayerSegmentProfile profile, SegmentationCriteria criteria)
        {
            float value = GetMetricValue(profile, criteria.metricName);
            
            switch (criteria.operation)
            {
                case ComparisonOperator.GreaterThan:
                    return value > criteria.threshold ? 1f : 0f;
                    
                case ComparisonOperator.LessThan:
                    return value < criteria.threshold ? 1f : 0f;
                    
                case ComparisonOperator.Equal:
                    return Math.Abs(value - criteria.threshold) < 0.01f ? 1f : 0f;
                    
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Get metric value from player profile by name
        /// </summary>
        private float GetMetricValue(PlayerSegmentProfile profile, string metricName)
        {
            switch (metricName.ToLower())
            {
                // Spending metrics
                case "lifetimevalue": return profile.spending.lifetimeValue;
                case "averageordervalue": return profile.spending.averageOrderValue;
                case "totalpurchases": return profile.spending.totalPurchases;
                case "spendingvelocity": return profile.spending.spendingVelocity;
                
                // Engagement metrics
                case "totalsessions": return profile.behavior.totalSessions;
                case "averagesessionlength": return profile.behavior.averageSessionLength;
                case "daysplayed": return profile.behavior.daysPlayed;
                case "day7retention": return profile.engagement.day7Retention;
                case "day30retention": return profile.engagement.day30Retention;
                
                // Competitive metrics
                case "rankedgamesplayed": return profile.competitive.rankedGamesPlayed;
                case "currentrating": return profile.competitive.currentRating;
                case "competitivewinrate": return profile.competitive.competitiveWinRate;
                
                // Social metrics
                case "friendsinvited": return profile.social.friendsInvited;
                case "socialshares": return profile.social.socialShares;
                case "influencescore": return profile.social.influenceScore;
                
                // Predictive scores
                case "churnrisk": return profile.churnRisk;
                case "spendingpotential": return profile.spendingPotential;
                case "viralityscore": return profile.viralityScore;
                
                default:
                    Debug.LogWarning($"Unknown metric: {metricName}");
                    return 0f;
            }
        }

        /// <summary>
        /// Update player segment with latest data
        /// </summary>
        private PlayerSegmentProfile UpdatePlayerSegment(string playerId)
        {
            var profile = _playerSegments.GetValueOrDefault(playerId, CreateNewPlayerProfile(playerId));
            
            // Update predictive scores
            if (enablePredictiveSegmentation)
            {
                profile.churnRisk = _churnPredictionModel?.Predict(profile) ?? 0f;
                profile.spendingPotential = _spendingPredictionModel?.Predict(profile) ?? 0f;
                profile.viralityScore = CalculateViralityScore(profile);
                profile.competitivenessScore = CalculateCompetitivenessScore(profile);
            }
            
            // Recalculate primary segment
            profile.primarySegment = CalculatePlayerSegment(profile);
            profile.lastUpdated = DateTime.Now;
            
            _playerSegments[playerId] = profile;
            return profile;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize segmentation rules for different player types
        /// </summary>
        private void InitializeSegmentationRules()
        {
            _segmentationRules = new List<SegmentationRule>
            {
                // Whale segment
                new SegmentationRule
                {
                    ruleId = "whale_detection",
                    ruleName = "High-Value Spender",
                    targetSegment = PlayerSegmentType.Whale,
                    criteria = new List<SegmentationCriteria>
                    {
                        new SegmentationCriteria { metricName = "lifetimevalue", operation = ComparisonOperator.GreaterThan, threshold = whaleSpendingThreshold, weight = 0.6f },
                        new SegmentationCriteria { metricName = "spendingvelocity", operation = ComparisonOperator.GreaterThan, threshold = 10f, weight = 0.4f }
                    },
                    minConfidenceScore = 0.8f,
                    isExclusiveSegment = true
                },
                
                // Dolphin segment
                new SegmentationRule
                {
                    ruleId = "dolphin_detection",
                    ruleName = "Medium Spender",
                    targetSegment = PlayerSegmentType.Dolphin,
                    criteria = new List<SegmentationCriteria>
                    {
                        new SegmentationCriteria { metricName = "lifetimevalue", operation = ComparisonOperator.GreaterThan, threshold = dolphinSpendingThreshold, weight = 0.5f },
                        new SegmentationCriteria { metricName = "totalpurchases", operation = ComparisonOperator.GreaterThan, threshold = 3, weight = 0.3f },
                        new SegmentationCriteria { metricName = "day7retention", operation = ComparisonOperator.GreaterThan, threshold = 0.5f, weight = 0.2f }
                    },
                    minConfidenceScore = 0.7f,
                    isExclusiveSegment = false
                },
                
                // Competitor segment
                new SegmentationRule
                {
                    ruleId = "competitor_detection",
                    ruleName = "Competitive Player",
                    targetSegment = PlayerSegmentType.Competitor,
                    criteria = new List<SegmentationCriteria>
                    {
                        new SegmentationCriteria { metricName = "rankedgamesplayed", operation = ComparisonOperator.GreaterThan, threshold = 20, weight = 0.4f },
                        new SegmentationCriteria { metricName = "competitivewinrate", operation = ComparisonOperator.GreaterThan, threshold = 0.5f, weight = 0.3f },
                        new SegmentationCriteria { metricName = "currentrating", operation = ComparisonOperator.GreaterThan, threshold = 1200, weight = 0.3f }
                    },
                    minConfidenceScore = 0.6f,
                    isExclusiveSegment = false
                },
                
                // Social segment
                new SegmentationRule
                {
                    ruleId = "social_detection",
                    ruleName = "Social Player",
                    targetSegment = PlayerSegmentType.Social,
                    criteria = new List<SegmentationCriteria>
                    {
                        new SegmentationCriteria { metricName = "friendsinvited", operation = ComparisonOperator.GreaterThan, threshold = 5, weight = 0.4f },
                        new SegmentationCriteria { metricName = "socialshares", operation = ComparisonOperator.GreaterThan, threshold = 3, weight = 0.3f },
                        new SegmentationCriteria { metricName = "influencescore", operation = ComparisonOperator.GreaterThan, threshold = 0.6f, weight = 0.3f }
                    },
                    minConfidenceScore = 0.6f,
                    isExclusiveSegment = false
                },
                
                // Churning player
                new SegmentationRule
                {
                    ruleId = "churn_risk_detection",
                    ruleName = "At-Risk Player",
                    targetSegment = PlayerSegmentType.ChurningPlayer,
                    criteria = new List<SegmentationCriteria>
                    {
                        new SegmentationCriteria { metricName = "churnrisk", operation = ComparisonOperator.GreaterThan, threshold = churnWarningThreshold, weight = 0.8f },
                        new SegmentationCriteria { metricName = "day7retention", operation = ComparisonOperator.LessThan, threshold = 0.3f, weight = 0.2f }
                    },
                    minConfidenceScore = 0.7f,
                    isExclusiveSegment = false
                }
            };
        }

        /// <summary>
        /// Initialize monetization strategies for each segment
        /// </summary>
        private void InitializeMonetizationStrategies()
        {
            _segmentStrategies = new Dictionary<string, MonetizationStrategy>();
            
            // Whale strategy - Premium experiences
            _segmentStrategies[PlayerSegmentType.Whale.ToString()] = new MonetizationStrategy
            {
                targetSegment = PlayerSegmentType.Whale,
                strategyName = "VIP Experience",
                tactics = new List<MonetizationTactic>
                {
                    new MonetizationTactic
                    {
                        tacticId = "whale_exclusive_offers",
                        tacticName = "Exclusive Premium Offers",
                        channel = MonetizationChannel.DirectPurchase,
                        successRate = 0.8f,
                        parameters = new Dictionary<string, object> { ["minPrice"] = 50f, ["exclusiveContent"] = true }
                    },
                    new MonetizationTactic
                    {
                        tacticId = "whale_early_access",
                        tacticName = "Early Access Content",
                        channel = MonetizationChannel.DirectPurchase,
                        successRate = 0.7f,
                        parameters = new Dictionary<string, object> { ["earlyAccessDays"] = 7 }
                    }
                },
                targetConversionRate = 0.75f,
                expectedARPU = 150f,
                communicationStyle = "Premium, Exclusive",
                preferredOfferTypes = new List<string> { "exclusive", "premium", "early_access" }
            };
            
            // Competitor strategy - Performance focused
            _segmentStrategies[PlayerSegmentType.Competitor.ToString()] = new MonetizationStrategy
            {
                targetSegment = PlayerSegmentType.Competitor,
                strategyName = "Competitive Edge",
                tactics = new List<MonetizationTactic>
                {
                    new MonetizationTactic
                    {
                        tacticId = "competitive_advantages",
                        tacticName = "Performance Upgrades",
                        channel = MonetizationChannel.DirectPurchase,
                        successRate = 0.6f,
                        parameters = new Dictionary<string, object> { ["focusOnStats"] = true, ["competitiveBoosts"] = true }
                    },
                    new MonetizationTactic
                    {
                        tacticId = "tournament_entries",
                        tacticName = "Tournament Entry Fees",
                        channel = MonetizationChannel.CompetitiveRewards,
                        successRate = 0.5f,
                        parameters = new Dictionary<string, object> { ["rewardMultiplier"] = 2f }
                    }
                },
                targetConversionRate = 0.4f,
                expectedARPU = 25f,
                communicationStyle = "Achievement, Performance",
                preferredOfferTypes = new List<string> { "competitive", "performance", "achievement" }
            };
            
            // Free rider strategy - Ad monetization focus
            _segmentStrategies[PlayerSegmentType.FreeRider.ToString()] = new MonetizationStrategy
            {
                targetSegment = PlayerSegmentType.FreeRider,
                strategyName = "Engagement Monetization",
                tactics = new List<MonetizationTactic>
                {
                    new MonetizationTactic
                    {
                        tacticId = "rewarded_ads_focus",
                        tacticName = "Value-Driven Ad Rewards",
                        channel = MonetizationChannel.RewardedAds,
                        successRate = 0.8f,
                        parameters = new Dictionary<string, object> { ["highValueRewards"] = true, ["frequentOffers"] = true }
                    },
                    new MonetizationTactic
                    {
                        tacticId = "starter_pack_conversion",
                        tacticName = "First Purchase Incentive",
                        channel = MonetizationChannel.LimitedOffers,
                        successRate = 0.1f,
                        parameters = new Dictionary<string, object> { ["maxPrice"] = 4.99f, ["highDiscount"] = true }
                    }
                },
                targetConversionRate = 0.05f,
                expectedARPU = 2f,
                communicationStyle = "Value, Casual",
                preferredOfferTypes = new List<string> { "starter", "value", "limited_time" }
            };
        }

        /// <summary>
        /// Initialize predictive models for churn and spending
        /// </summary>
        private void InitializePredictiveModels()
        {
            // Simplified predictive models - in production, use ML libraries
            _churnPredictionModel = new PredictiveModel("churn");
            _spendingPredictionModel = new PredictiveModel("spending");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Create new player profile with default values
        /// </summary>
        private PlayerSegmentProfile CreateNewPlayerProfile(string playerId)
        {
            return new PlayerSegmentProfile
            {
                playerId = playerId,
                primarySegment = PlayerSegmentType.NewPlayer,
                secondarySegments = new List<PlayerSegmentType>(),
                segmentConfidence = 0.5f,
                lastUpdated = DateTime.Now,
                behavior = new BehavioralMetrics(),
                spending = new SpendingMetrics(),
                engagement = new EngagementMetrics(),
                social = new SocialMetrics(),
                competitive = new CompetitiveMetrics(),
                preferredChannels = new List<MonetizationChannel>(),
                respondsToOffers = new List<string>(),
                productAffinities = new Dictionary<string, float>(),
                pricesensitivity = 0.5f
            };
        }

        /// <summary>
        /// Calculate virality score based on social metrics
        /// </summary>
        private float CalculateViralityScore(PlayerSegmentProfile profile)
        {
            var social = profile.social;
            var baseScore = 0f;
            
            // Friend invitations (0-0.4)
            baseScore += Math.Min(social.friendsInvited / 10f, 0.4f);
            
            // Social shares (0-0.3)
            baseScore += Math.Min(social.socialShares / 10f, 0.3f);
            
            // Influence score (0-0.3)
            baseScore += social.influenceScore * 0.3f;
            
            return Mathf.Clamp01(baseScore);
        }

        /// <summary>
        /// Calculate competitiveness score
        /// </summary>
        private float CalculateCompetitivenessScore(PlayerSegmentProfile profile)
        {
            var competitive = profile.competitive;
            var baseScore = 0f;
            
            // Games played (0-0.3)
            baseScore += Math.Min(competitive.rankedGamesPlayed / 100f, 0.3f);
            
            // Win rate (0-0.4)
            baseScore += competitive.competitiveWinRate * 0.4f;
            
            // Rating (0-0.3)
            baseScore += Math.Min(competitive.currentRating / 2000f, 0.3f);
            
            return Mathf.Clamp01(baseScore);
        }

        /// <summary>
        /// Get churn risk factors for a player
        /// </summary>
        private List<string> GetChurnRiskFactors(PlayerSegmentProfile profile)
        {
            var riskFactors = new List<string>();
            
            if (profile.engagement.engagementTrend < -0.3f)
                riskFactors.Add("Declining engagement trend");
            
            if (profile.behavior.averageSessionLength < 5f)
                riskFactors.Add("Very short sessions");
            
            if (profile.engagement.day7Retention < 0.3f)
                riskFactors.Add("Poor 7-day retention");
            
            if ((DateTime.Now - profile.behavior.lastActive).TotalDays > 3)
                riskFactors.Add("Inactive for multiple days");
            
            if (profile.spending.totalPurchases == 0 && profile.behavior.totalSessions > 10)
                riskFactors.Add("No purchases despite engagement");
            
            return riskFactors;
        }

        /// <summary>
        /// Calculate recommendation priority for a tactic
        /// </summary>
        private float CalculateRecommendationPriority(PlayerSegmentProfile profile, MonetizationTactic tactic)
        {
            float priority = tactic.successRate * profile.segmentConfidence;
            
            // Boost priority based on preferred channels
            if (profile.preferredChannels.Contains(tactic.channel))
            {
                priority *= 1.5f;
            }
            
            // Adjust for price sensitivity
            if (tactic.parameters.ContainsKey("minPrice"))
            {
                var minPrice = (float)tactic.parameters["minPrice"];
                var priceAdjustment = 1f - (profile.pricesensitivity * minPrice / 100f);
                priority *= Mathf.Clamp(priceAdjustment, 0.5f, 1.5f);
            }
            
            return Mathf.Clamp01(priority);
        }

        /// <summary>
        /// Generate personalized message for tactic
        /// </summary>
        private string GeneratePersonalizedMessage(PlayerSegmentProfile profile, MonetizationTactic tactic)
        {
            var strategy = GetSegmentStrategy(profile.primarySegment);
            
            switch (profile.primarySegment)
            {
                case PlayerSegmentType.Whale:
                    return "Exclusive premium content just for our VIP players like you!";
                
                case PlayerSegmentType.Competitor:
                    return $"Dominate the competition with these performance upgrades! Current rating: {profile.competitive.currentRating}";
                
                case PlayerSegmentType.Social:
                    return "Share the fun with friends and earn exclusive rewards!";
                
                case PlayerSegmentType.Collector:
                    return "Complete your collection with these rare and exclusive items!";
                
                default:
                    return "Special offer tailored just for you!";
            }
        }

        /// <summary>
        /// Get recommended products for segment
        /// </summary>
        private List<string> GetRecommendedProducts(PlayerSegmentProfile profile, MonetizationStrategy strategy)
        {
            var products = new List<string>();
            
            switch (profile.primarySegment)
            {
                case PlayerSegmentType.Whale:
                    products.AddRange(new[] { "premium_mega_pack", "exclusive_collection", "vip_membership" });
                    break;
                
                case PlayerSegmentType.Competitor:
                    products.AddRange(new[] { "performance_parts", "tournament_entry", "ranking_boost" });
                    break;
                
                case PlayerSegmentType.FreeRider:
                    products.AddRange(new[] { "starter_pack", "energy_refill", "small_currency_pack" });
                    break;
                
                default:
                    products.AddRange(new[] { "standard_pack", "premium_currency", "customization_bundle" });
                    break;
            }
            
            return products;
        }

        /// <summary>
        /// Update all player segments periodically
        /// </summary>
        private void UpdatePlayerSegments()
        {
            var updatedCount = 0;
            
            foreach (var playerId in _playerSegments.Keys.ToList())
            {
                UpdatePlayerSegment(playerId);
                updatedCount++;
            }
            
            _lastSegmentUpdate = DateTime.Now;
            Debug.Log($"[PlayerSegmentationEngine] Updated {updatedCount} player segments");
            
            // Update and broadcast segmentation metrics
            UpdateSegmentationMetrics();
        }

        /// <summary>
        /// Update and broadcast segmentation metrics
        /// </summary>
        private void UpdateSegmentationMetrics()
        {
            var metrics = new SegmentationMetrics
            {
                segmentDistribution = CalculateSegmentDistribution(),
                segmentARPU = CalculateSegmentARPU(),
                segmentRetention = CalculateSegmentRetention(),
                overallSegmentationAccuracy = CalculateSegmentationAccuracy(),
                lastUpdate = DateTime.Now
            };
            
            OnSegmentationMetricsUpdated?.Invoke(metrics);
        }

        /// <summary>
        /// Get default monetization strategy
        /// </summary>
        private MonetizationStrategy GetDefaultStrategy()
        {
            return new MonetizationStrategy
            {
                targetSegment = PlayerSegmentType.Unknown,
                strategyName = "Default Strategy",
                tactics = new List<MonetizationTactic>(),
                targetConversionRate = 0.02f,
                expectedARPU = 1f,
                communicationStyle = "Generic",
                preferredOfferTypes = new List<string> { "standard" }
            };
        }

        // Placeholder calculation methods
        private Dictionary<PlayerSegmentType, int> CalculateSegmentDistribution() => new Dictionary<PlayerSegmentType, int>();
        private Dictionary<PlayerSegmentType, float> CalculateSegmentARPU() => new Dictionary<PlayerSegmentType, float>();
        private Dictionary<PlayerSegmentType, float> CalculateSegmentRetention() => new Dictionary<PlayerSegmentType, float>();
        private float CalculateSegmentationAccuracy() => 0.85f;
        #endregion
    }

    #region Supporting Data Structures
    /// <summary>
    /// Monetization recommendation for specific player
    /// </summary>
    public struct MonetizationRecommendation
    {
        public string tacticId;
        public MonetizationChannel channel;
        public float priority;
        public float expectedConversionRate;
        public string personalizedMessage;
        public List<string> products;
    }

    /// <summary>
    /// Simple predictive model for player behavior
    /// </summary>
    public class PredictiveModel
    {
        private string _modelType;
        
        public PredictiveModel(string modelType)
        {
            _modelType = modelType;
        }
        
        public float Predict(PlayerSegmentationEngine.PlayerSegmentProfile profile)
        {
            // Simplified prediction logic - replace with actual ML model
            switch (_modelType)
            {
                case "churn":
                    return PredictChurn(profile);
                case "spending":
                    return PredictSpending(profile);
                default:
                    return 0.5f;
            }
        }
        
        private float PredictChurn(PlayerSegmentationEngine.PlayerSegmentProfile profile)
        {
            float churnScore = 0f;
            
            // Engagement factors
            if (profile.engagement.day7Retention < 0.3f) churnScore += 0.3f;
            if (profile.behavior.averageSessionLength < 5f) churnScore += 0.2f;
            if ((DateTime.Now - profile.behavior.lastActive).TotalDays > 3) churnScore += 0.3f;
            if (profile.engagement.engagementTrend < -0.2f) churnScore += 0.2f;
            
            return Mathf.Clamp01(churnScore);
        }
        
        private float PredictSpending(PlayerSegmentationEngine.PlayerSegmentProfile profile)
        {
            float spendingScore = 0f;
            
            // Historical spending
            if (profile.spending.totalPurchases > 0) spendingScore += 0.4f;
            if (profile.spending.spendingVelocity > 1f) spendingScore += 0.3f;
            
            // Engagement indicators
            if (profile.behavior.averageSessionLength > 20f) spendingScore += 0.2f;
            if (profile.engagement.day7Retention > 0.7f) spendingScore += 0.1f;
            
            return Mathf.Clamp01(spendingScore);
        }
    }
    #endregion
}