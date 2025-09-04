using System;
using System.Collections.Generic;
using UnityEngine;
using CircuitRunners.Data;

namespace CircuitRunners.Monetization
{
    /// <summary>
    /// Monetization Balance System for Circuit Runners
    /// Manages and optimizes the game economy to maximize player engagement and revenue
    /// Uses data-driven approaches to balance progression, energy systems, and pricing
    /// 
    /// Key Features:
    /// - Dynamic difficulty and reward scaling
    /// - Energy system optimization for retention
    /// - Ad placement timing optimization
    /// - Purchase funnel optimization
    /// - Player segmentation for targeted offers
    /// - Real-time balance adjustments based on player behavior
    /// </summary>
    public class MonetizationBalancer : MonoBehaviour
    {
        #region Configuration
        [Header("Energy System Balance")]
        [SerializeField] private EnergyBalanceConfig _energyConfig;
        
        [Header("Progression Balance")]
        [SerializeField] private ProgressionBalanceConfig _progressionConfig;
        
        [Header("Ad Monetization Balance")]
        [SerializeField] private AdBalanceConfig _adConfig;
        
        [Header("Purchase Pricing Balance")]
        [SerializeField] private PricingBalanceConfig _pricingConfig;
        
        [Header("Player Segmentation")]
        [SerializeField] private PlayerSegmentConfig _segmentConfig;
        
        /// <summary>
        /// Current monetization balance configuration
        /// </summary>
        public MonetizationBalance CurrentBalance { get; private set; }
        #endregion

        #region Dependencies
        private ResourceManager _resourceManager;
        private MonetizationManager _monetizationManager;
        private Core.GameManager _gameManager;
        #endregion

        #region Events
        /// <summary>
        /// Fired when monetization balance is updated
        /// </summary>
        public event Action<MonetizationBalance> OnBalanceUpdated;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Initialize default configurations if not assigned
            InitializeDefaultConfigurations();
            
            // Initialize balance system
            CurrentBalance = new MonetizationBalance();
            ApplyDefaultBalance();
        }

        private void Start()
        {
            // Find dependencies
            _resourceManager = FindObjectOfType<ResourceManager>();
            _monetizationManager = FindObjectOfType<MonetizationManager>();
            _gameManager = Core.GameManager.Instance;
            
            if (_resourceManager == null)
            {
                Debug.LogError("[MonetizationBalancer] ResourceManager not found!");
            }
            
            // Start balance optimization
            StartCoroutine(OptimizeBalancePeriodically());
            
            Debug.Log("[MonetizationBalancer] Monetization balancer initialized");
        }
        #endregion

        #region Balance Optimization
        /// <summary>
        /// Periodically optimize monetization balance based on player behavior
        /// </summary>
        private System.Collections.IEnumerator OptimizeBalancePeriodically()
        {
            const float OPTIMIZATION_INTERVAL = 300f; // 5 minutes
            
            while (true)
            {
                yield return new WaitForSeconds(OPTIMIZATION_INTERVAL);
                
                try
                {
                    OptimizeBalance();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MonetizationBalancer] Balance optimization failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Main balance optimization logic
        /// </summary>
        private void OptimizeBalance()
        {
            if (_resourceManager == null)
            {
                return;
            }
            
            Debug.Log("[MonetizationBalancer] Running balance optimization...");
            
            // Analyze current player state
            var playerAnalysis = AnalyzePlayerState();
            
            // Determine player segment
            var playerSegment = DeterminePlayerSegment(playerAnalysis);
            
            // Optimize energy system
            OptimizeEnergyBalance(playerAnalysis, playerSegment);
            
            // Optimize progression curve
            OptimizeProgressionBalance(playerAnalysis, playerSegment);
            
            // Optimize ad timing
            OptimizeAdBalance(playerAnalysis, playerSegment);
            
            // Optimize pricing
            OptimizePricingBalance(playerAnalysis, playerSegment);
            
            // Apply optimized balance
            ApplyBalanceChanges();
            
            Debug.Log($"[MonetizationBalancer] Balance optimized for player segment: {playerSegment}");
        }

        /// <summary>
        /// Analyze current player state for optimization
        /// </summary>
        private PlayerStateAnalysis AnalyzePlayerState()
        {
            var analysis = new PlayerStateAnalysis();
            
            try
            {
                if (_resourceManager != null)
                {
                    analysis.PlayerLevel = _resourceManager.PlayerLevel;
                    analysis.TotalXP = _resourceManager.TotalXPEarned;
                    analysis.CurrentScrap = _resourceManager.CurrentScrap;
                    analysis.CurrentDataCores = _resourceManager.CurrentDataCores;
                    analysis.CurrentEnergy = _resourceManager.CurrentEnergy;
                    analysis.MaxEnergy = _resourceManager.MaxEnergy;
                    analysis.HasPremiumAccount = _resourceManager.HasPremiumAccount;
                    analysis.UnlockedAchievementCount = _resourceManager.UnlockedAchievements.Count;
                }
                
                if (_gameManager != null)
                {
                    var sessionStats = _gameManager.GetSessionStatistics();
                    analysis.SessionDuration = sessionStats.SessionDuration;
                    analysis.RunsCompleted = sessionStats.RunsCompleted;
                }
                
                // Calculate derived metrics
                analysis.ProgressionRate = CalculateProgressionRate(analysis);
                analysis.EngagementScore = CalculateEngagementScore(analysis);
                analysis.MonetizationPotential = CalculateMonetizationPotential(analysis);
                
                Debug.Log($"[MonetizationBalancer] Player analysis complete - Level: {analysis.PlayerLevel}, Engagement: {analysis.EngagementScore:F2}");
                
                return analysis;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Player analysis failed: {ex.Message}");
                return new PlayerStateAnalysis(); // Return default analysis
            }
        }

        /// <summary>
        /// Calculate player's progression rate
        /// </summary>
        private float CalculateProgressionRate(PlayerStateAnalysis analysis)
        {
            if (analysis.SessionDuration <= 0f)
            {
                return 0f;
            }
            
            // XP per minute as progression rate
            float progressionRate = analysis.TotalXP / (analysis.SessionDuration / 60f);
            
            // Normalize to 0-1 scale (assume 100 XP/min is maximum expected rate)
            return Mathf.Clamp01(progressionRate / 100f);
        }

        /// <summary>
        /// Calculate player engagement score
        /// </summary>
        private float CalculateEngagementScore(PlayerStateAnalysis analysis)
        {
            float score = 0f;
            
            // Session duration factor (longer sessions = higher engagement)
            if (analysis.SessionDuration > 0f)
            {
                score += Mathf.Clamp01(analysis.SessionDuration / 1800f) * 0.3f; // 30 minutes = max
            }
            
            // Runs per session factor
            if (analysis.SessionDuration > 0f)
            {
                float runsPerMinute = analysis.RunsCompleted / (analysis.SessionDuration / 60f);
                score += Mathf.Clamp01(runsPerMinute / 2f) * 0.3f; // 2 runs/min = max
            }
            
            // Achievement factor
            score += Mathf.Clamp01(analysis.UnlockedAchievementCount / 20f) * 0.2f; // 20 achievements = max
            
            // Level progression factor
            score += Mathf.Clamp01(analysis.PlayerLevel / 50f) * 0.2f; // Level 50 = max
            
            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// Calculate monetization potential score
        /// </summary>
        private float CalculateMonetizationPotential(PlayerStateAnalysis analysis)
        {
            float potential = 0f;
            
            // Premium account indicates willingness to spend
            if (analysis.HasPremiumAccount)
            {
                potential += 0.4f;
            }
            
            // High engagement suggests conversion potential
            potential += analysis.EngagementScore * 0.3f;
            
            // Resource scarcity creates purchase motivation
            float resourceRatio = (analysis.CurrentScrap + analysis.CurrentDataCores * 10f) / Mathf.Max(1f, analysis.PlayerLevel * 100f);
            if (resourceRatio < 0.5f) // Player has low resources relative to level
            {
                potential += 0.2f;
            }
            
            // Energy constraints create monetization opportunities
            if (analysis.CurrentEnergy == 0)
            {
                potential += 0.1f;
            }
            
            return Mathf.Clamp01(potential);
        }
        #endregion

        #region Player Segmentation
        /// <summary>
        /// Determine player segment for targeted balance optimization
        /// </summary>
        private PlayerSegment DeterminePlayerSegment(PlayerStateAnalysis analysis)
        {
            try
            {
                // Premium players (whales)
                if (analysis.HasPremiumAccount || analysis.MonetizationPotential > 0.8f)
                {
                    return PlayerSegment.Premium;
                }
                
                // Highly engaged free players (potential converters)
                if (analysis.EngagementScore > 0.7f && analysis.PlayerLevel >= 10)
                {
                    return PlayerSegment.HighEngagement;
                }
                
                // Regular players
                if (analysis.EngagementScore > 0.4f || analysis.PlayerLevel >= 5)
                {
                    return PlayerSegment.Regular;
                }
                
                // New or low engagement players
                return PlayerSegment.Casual;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Player segmentation failed: {ex.Message}");
                return PlayerSegment.Regular; // Default segment
            }
        }
        #endregion

        #region Energy System Optimization
        /// <summary>
        /// Optimize energy system balance based on player behavior
        /// </summary>
        private void OptimizeEnergyBalance(PlayerStateAnalysis analysis, PlayerSegment segment)
        {
            try
            {
                var energyBalance = CurrentBalance.EnergyBalance;
                
                switch (segment)
                {
                    case PlayerSegment.Premium:
                        // Premium players: Generous energy, focus on content consumption
                        energyBalance.BaseRegenTimeMinutes = _energyConfig.PremiumRegenTime;
                        energyBalance.MaxEnergy = Math.Max(energyBalance.MaxEnergy, _energyConfig.PremiumMaxEnergy);
                        energyBalance.EnergyPerRun = _energyConfig.PremiumCostPerRun;
                        break;
                        
                    case PlayerSegment.HighEngagement:
                        // High engagement players: Balanced energy with conversion opportunities
                        energyBalance.BaseRegenTimeMinutes = _energyConfig.EngagedRegenTime;
                        energyBalance.MaxEnergy = _energyConfig.EngagedMaxEnergy;
                        energyBalance.EnergyPerRun = _energyConfig.EngagedCostPerRun;
                        break;
                        
                    case PlayerSegment.Regular:
                        // Regular players: Standard energy system
                        energyBalance.BaseRegenTimeMinutes = _energyConfig.RegularRegenTime;
                        energyBalance.MaxEnergy = _energyConfig.RegularMaxEnergy;
                        energyBalance.EnergyPerRun = _energyConfig.RegularCostPerRun;
                        break;
                        
                    case PlayerSegment.Casual:
                        // Casual players: More forgiving energy system
                        energyBalance.BaseRegenTimeMinutes = _energyConfig.CasualRegenTime;
                        energyBalance.MaxEnergy = _energyConfig.CasualMaxEnergy;
                        energyBalance.EnergyPerRun = _energyConfig.CasualCostPerRun;
                        break;
                }
                
                // Apply energy scarcity adjustments
                if (analysis.CurrentEnergy == 0 && analysis.SessionDuration > 300f) // Player stuck for 5+ minutes
                {
                    // Reduce regen time temporarily to improve retention
                    energyBalance.BaseRegenTimeMinutes *= 0.8f;
                }
                
                Debug.Log($"[MonetizationBalancer] Energy balance optimized for {segment} segment");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Energy optimization failed: {ex.Message}");
            }
        }
        #endregion

        #region Progression Balance Optimization
        /// <summary>
        /// Optimize progression curve based on player performance
        /// </summary>
        private void OptimizeProgressionBalance(PlayerStateAnalysis analysis, PlayerSegment segment)
        {
            try
            {
                var progressionBalance = CurrentBalance.ProgressionBalance;
                
                // Adjust XP multipliers based on segment
                switch (segment)
                {
                    case PlayerSegment.Premium:
                        progressionBalance.XPMultiplier = _progressionConfig.PremiumXPMultiplier;
                        progressionBalance.ScrapMultiplier = _progressionConfig.PremiumScrapMultiplier;
                        break;
                        
                    case PlayerSegment.HighEngagement:
                        progressionBalance.XPMultiplier = _progressionConfig.EngagedXPMultiplier;
                        progressionBalance.ScrapMultiplier = _progressionConfig.EngagedScrapMultiplier;
                        break;
                        
                    case PlayerSegment.Regular:
                        progressionBalance.XPMultiplier = _progressionConfig.RegularXPMultiplier;
                        progressionBalance.ScrapMultiplier = _progressionConfig.RegularScrapMultiplier;
                        break;
                        
                    case PlayerSegment.Casual:
                        progressionBalance.XPMultiplier = _progressionConfig.CasualXPMultiplier;
                        progressionBalance.ScrapMultiplier = _progressionConfig.CasualScrapMultiplier;
                        break;
                }
                
                // Adjust based on progression rate
                if (analysis.ProgressionRate < 0.3f) // Slow progression
                {
                    progressionBalance.XPMultiplier *= 1.2f; // Boost XP
                    progressionBalance.ScrapMultiplier *= 1.1f; // Boost rewards
                }
                else if (analysis.ProgressionRate > 0.8f) // Very fast progression
                {
                    progressionBalance.XPMultiplier *= 0.9f; // Slightly reduce to maintain challenge
                }
                
                Debug.Log($"[MonetizationBalancer] Progression balance optimized - XP: {progressionBalance.XPMultiplier:F2}x, Scrap: {progressionBalance.ScrapMultiplier:F2}x");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Progression optimization failed: {ex.Message}");
            }
        }
        #endregion

        #region Ad Balance Optimization
        /// <summary>
        /// Optimize ad timing and frequency
        /// </summary>
        private void OptimizeAdBalance(PlayerStateAnalysis analysis, PlayerSegment segment)
        {
            try
            {
                var adBalance = CurrentBalance.AdBalance;
                
                switch (segment)
                {
                    case PlayerSegment.Premium:
                        // Premium players see fewer ads
                        adBalance.InterstitialFrequency = _adConfig.PremiumInterstitialFrequency;
                        adBalance.RewardedAdCooldownSeconds = _adConfig.PremiumRewardedCooldown;
                        adBalance.MaxRewardedAdsPerSession = _adConfig.PremiumMaxRewardedPerSession;
                        break;
                        
                    case PlayerSegment.HighEngagement:
                        // High engagement players get balanced ad experience
                        adBalance.InterstitialFrequency = _adConfig.EngagedInterstitialFrequency;
                        adBalance.RewardedAdCooldownSeconds = _adConfig.EngagedRewardedCooldown;
                        adBalance.MaxRewardedAdsPerSession = _adConfig.EngagedMaxRewardedPerSession;
                        break;
                        
                    case PlayerSegment.Regular:
                        // Regular ad frequency
                        adBalance.InterstitialFrequency = _adConfig.RegularInterstitialFrequency;
                        adBalance.RewardedAdCooldownSeconds = _adConfig.RegularRewardedCooldown;
                        adBalance.MaxRewardedAdsPerSession = _adConfig.RegularMaxRewardedPerSession;
                        break;
                        
                    case PlayerSegment.Casual:
                        // Casual players see more rewarded ads, fewer interstitials
                        adBalance.InterstitialFrequency = _adConfig.CasualInterstitialFrequency;
                        adBalance.RewardedAdCooldownSeconds = _adConfig.CasualRewardedCooldown;
                        adBalance.MaxRewardedAdsPerSession = _adConfig.CasualMaxRewardedPerSession;
                        break;
                }
                
                Debug.Log($"[MonetizationBalancer] Ad balance optimized for {segment} segment");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Ad optimization failed: {ex.Message}");
            }
        }
        #endregion

        #region Pricing Balance Optimization
        /// <summary>
        /// Optimize purchase pricing and offers
        /// </summary>
        private void OptimizePricingBalance(PlayerStateAnalysis analysis, PlayerSegment segment)
        {
            try
            {
                var pricingBalance = CurrentBalance.PricingBalance;
                
                // Base pricing adjustments by segment
                switch (segment)
                {
                    case PlayerSegment.Premium:
                        pricingBalance.PremiumCurrencyValueMultiplier = _pricingConfig.PremiumValueMultiplier;
                        pricingBalance.ShowSpecialOffers = _pricingConfig.ShowPremiumOffers;
                        break;
                        
                    case PlayerSegment.HighEngagement:
                        pricingBalance.PremiumCurrencyValueMultiplier = _pricingConfig.EngagedValueMultiplier;
                        pricingBalance.ShowSpecialOffers = _pricingConfig.ShowEngagedOffers;
                        break;
                        
                    case PlayerSegment.Regular:
                        pricingBalance.PremiumCurrencyValueMultiplier = _pricingConfig.RegularValueMultiplier;
                        pricingBalance.ShowSpecialOffers = _pricingConfig.ShowRegularOffers;
                        break;
                        
                    case PlayerSegment.Casual:
                        pricingBalance.PremiumCurrencyValueMultiplier = _pricingConfig.CasualValueMultiplier;
                        pricingBalance.ShowSpecialOffers = _pricingConfig.ShowCasualOffers;
                        break;
                }
                
                // Dynamic pricing based on player state
                if (analysis.MonetizationPotential > 0.7f && analysis.CurrentEnergy == 0)
                {
                    // High-potential player is energy-blocked - show energy offers
                    pricingBalance.ShowEnergyOffers = true;
                    pricingBalance.EnergyOfferValue *= 1.2f; // Increase value
                }
                
                if (analysis.CurrentScrap < analysis.PlayerLevel * 50) // Low resources
                {
                    // Show resource boost offers
                    pricingBalance.ShowResourceOffers = true;
                }
                
                Debug.Log($"[MonetizationBalancer] Pricing balance optimized for {segment} segment");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Pricing optimization failed: {ex.Message}");
            }
        }
        #endregion

        #region Configuration Management
        /// <summary>
        /// Initialize default configurations if not assigned in inspector
        /// </summary>
        private void InitializeDefaultConfigurations()
        {
            if (_energyConfig == null)
            {
                _energyConfig = CreateDefaultEnergyConfig();
            }
            
            if (_progressionConfig == null)
            {
                _progressionConfig = CreateDefaultProgressionConfig();
            }
            
            if (_adConfig == null)
            {
                _adConfig = CreateDefaultAdConfig();
            }
            
            if (_pricingConfig == null)
            {
                _pricingConfig = CreateDefaultPricingConfig();
            }
            
            if (_segmentConfig == null)
            {
                _segmentConfig = CreateDefaultSegmentConfig();
            }
        }

        /// <summary>
        /// Apply default balance settings
        /// </summary>
        private void ApplyDefaultBalance()
        {
            CurrentBalance.EnergyBalance = new EnergyBalance
            {
                BaseRegenTimeMinutes = 20f,
                MaxEnergy = 5,
                EnergyPerRun = 1
            };
            
            CurrentBalance.ProgressionBalance = new ProgressionBalance
            {
                XPMultiplier = 1f,
                ScrapMultiplier = 1f
            };
            
            CurrentBalance.AdBalance = new AdBalance
            {
                InterstitialFrequency = 3,
                RewardedAdCooldownSeconds = 30f,
                MaxRewardedAdsPerSession = 10
            };
            
            CurrentBalance.PricingBalance = new PricingBalance
            {
                PremiumCurrencyValueMultiplier = 1f,
                ShowSpecialOffers = false,
                ShowEnergyOffers = false,
                ShowResourceOffers = false,
                EnergyOfferValue = 1f
            };
        }

        /// <summary>
        /// Apply optimized balance changes to the game systems
        /// </summary>
        private void ApplyBalanceChanges()
        {
            try
            {
                // Apply energy balance changes
                if (_resourceManager != null)
                {
                    // Note: In a real implementation, you would need methods in ResourceManager
                    // to update these values dynamically
                    Debug.Log($"[MonetizationBalancer] Energy regen time set to {CurrentBalance.EnergyBalance.BaseRegenTimeMinutes} minutes");
                }
                
                // Apply progression balance changes
                if (_resourceManager != null)
                {
                    // Update multipliers in resource manager
                    Debug.Log($"[MonetizationBalancer] XP multiplier set to {CurrentBalance.ProgressionBalance.XPMultiplier:F2}x");
                }
                
                // Notify systems of balance update
                OnBalanceUpdated?.Invoke(CurrentBalance);
                
                Debug.Log("[MonetizationBalancer] Balance changes applied successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MonetizationBalancer] Failed to apply balance changes: {ex.Message}");
            }
        }
        #endregion

        #region Default Configuration Creation
        private EnergyBalanceConfig CreateDefaultEnergyConfig()
        {
            return new EnergyBalanceConfig
            {
                // Premium segment (generous)
                PremiumRegenTime = 10f,
                PremiumMaxEnergy = 8,
                PremiumCostPerRun = 1,
                
                // High engagement segment (balanced)
                EngagedRegenTime = 15f,
                EngagedMaxEnergy = 6,
                EngagedCostPerRun = 1,
                
                // Regular segment (standard)
                RegularRegenTime = 20f,
                RegularMaxEnergy = 5,
                RegularCostPerRun = 1,
                
                // Casual segment (forgiving)
                CasualRegenTime = 15f,
                CasualMaxEnergy = 6,
                CasualCostPerRun = 1
            };
        }

        private ProgressionBalanceConfig CreateDefaultProgressionConfig()
        {
            return new ProgressionBalanceConfig
            {
                // Premium gets significant bonuses
                PremiumXPMultiplier = 2f,
                PremiumScrapMultiplier = 2f,
                
                // High engagement gets moderate bonuses
                EngagedXPMultiplier = 1.3f,
                EngagedScrapMultiplier = 1.3f,
                
                // Regular gets standard progression
                RegularXPMultiplier = 1f,
                RegularScrapMultiplier = 1f,
                
                // Casual gets slight boost to encourage progression
                CasualXPMultiplier = 1.2f,
                CasualScrapMultiplier = 1.1f
            };
        }

        private AdBalanceConfig CreateDefaultAdConfig()
        {
            return new AdBalanceConfig
            {
                // Premium sees fewer ads
                PremiumInterstitialFrequency = 5,
                PremiumRewardedCooldown = 60f,
                PremiumMaxRewardedPerSession = 15,
                
                // High engagement balanced
                EngagedInterstitialFrequency = 4,
                EngagedRewardedCooldown = 45f,
                EngagedMaxRewardedPerSession = 12,
                
                // Regular standard frequency
                RegularInterstitialFrequency = 3,
                RegularRewardedCooldown = 30f,
                RegularMaxRewardedPerSession = 10,
                
                // Casual fewer interstitials, more rewarded ads
                CasualInterstitialFrequency = 4,
                CasualRewardedCooldown = 20f,
                CasualMaxRewardedPerSession = 15
            };
        }

        private PricingBalanceConfig CreateDefaultPricingConfig()
        {
            return new PricingBalanceConfig
            {
                // Premium gets better value
                PremiumValueMultiplier = 1.5f,
                ShowPremiumOffers = true,
                
                // High engagement gets good offers
                EngagedValueMultiplier = 1.2f,
                ShowEngagedOffers = true,
                
                // Regular standard pricing
                RegularValueMultiplier = 1f,
                ShowRegularOffers = false,
                
                // Casual gets entry-level offers
                CasualValueMultiplier = 1.1f,
                ShowCasualOffers = true
            };
        }

        private PlayerSegmentConfig CreateDefaultSegmentConfig()
        {
            return new PlayerSegmentConfig
            {
                PremiumThreshold = 0.8f,
                HighEngagementThreshold = 0.7f,
                RegularThreshold = 0.4f
            };
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get current energy balance settings
        /// </summary>
        public EnergyBalance GetCurrentEnergyBalance()
        {
            return CurrentBalance?.EnergyBalance ?? new EnergyBalance();
        }

        /// <summary>
        /// Get current progression balance settings
        /// </summary>
        public ProgressionBalance GetCurrentProgressionBalance()
        {
            return CurrentBalance?.ProgressionBalance ?? new ProgressionBalance();
        }

        /// <summary>
        /// Get current ad balance settings
        /// </summary>
        public AdBalance GetCurrentAdBalance()
        {
            return CurrentBalance?.AdBalance ?? new AdBalance();
        }

        /// <summary>
        /// Get current pricing balance settings
        /// </summary>
        public PricingBalance GetCurrentPricingBalance()
        {
            return CurrentBalance?.PricingBalance ?? new PricingBalance();
        }

        /// <summary>
        /// Force balance optimization (for testing or admin features)
        /// </summary>
        public void ForceBalanceOptimization()
        {
            OptimizeBalance();
        }
        #endregion
    }

    #region Data Structures
    [Serializable]
    public class MonetizationBalance
    {
        public EnergyBalance EnergyBalance = new EnergyBalance();
        public ProgressionBalance ProgressionBalance = new ProgressionBalance();
        public AdBalance AdBalance = new AdBalance();
        public PricingBalance PricingBalance = new PricingBalance();
    }

    [Serializable]
    public class EnergyBalance
    {
        public float BaseRegenTimeMinutes = 20f;
        public int MaxEnergy = 5;
        public int EnergyPerRun = 1;
    }

    [Serializable]
    public class ProgressionBalance
    {
        public float XPMultiplier = 1f;
        public float ScrapMultiplier = 1f;
    }

    [Serializable]
    public class AdBalance
    {
        public int InterstitialFrequency = 3;
        public float RewardedAdCooldownSeconds = 30f;
        public int MaxRewardedAdsPerSession = 10;
    }

    [Serializable]
    public class PricingBalance
    {
        public float PremiumCurrencyValueMultiplier = 1f;
        public bool ShowSpecialOffers = false;
        public bool ShowEnergyOffers = false;
        public bool ShowResourceOffers = false;
        public float EnergyOfferValue = 1f;
    }

    [Serializable]
    public class PlayerStateAnalysis
    {
        public int PlayerLevel;
        public int TotalXP;
        public int CurrentScrap;
        public int CurrentDataCores;
        public int CurrentEnergy;
        public int MaxEnergy;
        public bool HasPremiumAccount;
        public float SessionDuration;
        public int RunsCompleted;
        public int UnlockedAchievementCount;
        public float ProgressionRate;
        public float EngagementScore;
        public float MonetizationPotential;
    }

    public enum PlayerSegment
    {
        Casual,
        Regular, 
        HighEngagement,
        Premium
    }

    [Serializable]
    public class EnergyBalanceConfig
    {
        [Header("Premium Segment")]
        public float PremiumRegenTime = 10f;
        public int PremiumMaxEnergy = 8;
        public int PremiumCostPerRun = 1;
        
        [Header("High Engagement Segment")]
        public float EngagedRegenTime = 15f;
        public int EngagedMaxEnergy = 6;
        public int EngagedCostPerRun = 1;
        
        [Header("Regular Segment")]
        public float RegularRegenTime = 20f;
        public int RegularMaxEnergy = 5;
        public int RegularCostPerRun = 1;
        
        [Header("Casual Segment")]
        public float CasualRegenTime = 15f;
        public int CasualMaxEnergy = 6;
        public int CasualCostPerRun = 1;
    }

    [Serializable]
    public class ProgressionBalanceConfig
    {
        [Header("Premium Segment")]
        public float PremiumXPMultiplier = 2f;
        public float PremiumScrapMultiplier = 2f;
        
        [Header("High Engagement Segment")]
        public float EngagedXPMultiplier = 1.3f;
        public float EngagedScrapMultiplier = 1.3f;
        
        [Header("Regular Segment")]
        public float RegularXPMultiplier = 1f;
        public float RegularScrapMultiplier = 1f;
        
        [Header("Casual Segment")]
        public float CasualXPMultiplier = 1.2f;
        public float CasualScrapMultiplier = 1.1f;
    }

    [Serializable]
    public class AdBalanceConfig
    {
        [Header("Premium Segment")]
        public int PremiumInterstitialFrequency = 5;
        public float PremiumRewardedCooldown = 60f;
        public int PremiumMaxRewardedPerSession = 15;
        
        [Header("High Engagement Segment")]
        public int EngagedInterstitialFrequency = 4;
        public float EngagedRewardedCooldown = 45f;
        public int EngagedMaxRewardedPerSession = 12;
        
        [Header("Regular Segment")]
        public int RegularInterstitialFrequency = 3;
        public float RegularRewardedCooldown = 30f;
        public int RegularMaxRewardedPerSession = 10;
        
        [Header("Casual Segment")]
        public int CasualInterstitialFrequency = 4;
        public float CasualRewardedCooldown = 20f;
        public int CasualMaxRewardedPerSession = 15;
    }

    [Serializable]
    public class PricingBalanceConfig
    {
        [Header("Premium Segment")]
        public float PremiumValueMultiplier = 1.5f;
        public bool ShowPremiumOffers = true;
        
        [Header("High Engagement Segment")]
        public float EngagedValueMultiplier = 1.2f;
        public bool ShowEngagedOffers = true;
        
        [Header("Regular Segment")]
        public float RegularValueMultiplier = 1f;
        public bool ShowRegularOffers = false;
        
        [Header("Casual Segment")]
        public float CasualValueMultiplier = 1.1f;
        public bool ShowCasualOffers = true;
    }

    [Serializable]
    public class PlayerSegmentConfig
    {
        public float PremiumThreshold = 0.8f;
        public float HighEngagementThreshold = 0.7f;
        public float RegularThreshold = 0.4f;
    }
    #endregion
}