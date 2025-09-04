using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRunners.Data
{
    /// <summary>
    /// Types of currency in Circuit Runners
    /// </summary>
    public enum CurrencyType
    {
        Scrap,          // Primary free currency earned through gameplay
        DataCores,      // Premium free currency earned through achievements and events
        Premium         // Purchased currency (real money)
    }

    /// <summary>
    /// Comprehensive player statistics for progression tracking and analytics
    /// </summary>
    [System.Serializable]
    public class PlayerStatistics
    {
        [Header("Session Data")]
        public DateTime FirstPlayTime = DateTime.Now;
        public DateTime LastPlayTime = DateTime.Now;
        public TimeSpan TotalPlayTime = TimeSpan.Zero;
        public int TotalSessionCount = 0;
        
        [Header("Run Statistics")]
        public int TotalRunsStarted = 0;
        public int TotalRunsCompleted = 0;
        public int TotalRunsFailed = 0;
        public float BestDistance = 0f;
        public float BestSurvivalTime = 0f;
        public int BestScore = 0;
        public float TotalDistanceTraveled = 0f;
        public float TotalSurvivalTime = 0f;
        
        [Header("Currency Statistics")]
        public int TotalScrapEarned = 0;
        public int TotalScrapSpent = 0;
        public int TotalDataCoresEarned = 0;
        public int TotalDataCoresSpent = 0;
        public int TotalPremiumCurrencyPurchased = 0;
        public int TotalPremiumCurrencySpent = 0;
        
        [Header("Bot Statistics")]
        public int TotalBotsCustomized = 0;
        public int TotalPartsUnlocked = 0;
        public Dictionary<Bot.BotArchetype, int> RunsByArchetype = new Dictionary<Bot.BotArchetype, int>();
        public Dictionary<string, int> PartUsageCount = new Dictionary<string, int>();
        
        [Header("Course Statistics")]
        public Dictionary<Course.CourseTheme, float> DistanceByTheme = new Dictionary<Course.CourseTheme, float>();
        public Dictionary<Course.CourseDifficulty, int> RunsByDifficulty = new Dictionary<Course.CourseDifficulty, int>();
        public int TotalObstaclesHit = 0;
        public int TotalObstaclesAvoided = 0;
        public int TotalCollectiblesGathered = 0;
        public int TotalPowerUpsUsed = 0;
        
        [Header("Achievement Progress")]
        public int AchievementsUnlocked = 0;
        public DateTime LastAchievementUnlocked = default(DateTime);
        public Dictionary<string, int> AchievementProgress = new Dictionary<string, int>();
        
        [Header("Monetization Statistics")]
        public int TotalPurchasesMade = 0;
        public decimal TotalMoneySpent = 0m;
        public int RewardedAdsWatched = 0;
        public int InterstitialAdsWatched = 0;
        public int AdSkips = 0;
        
        /// <summary>
        /// Calculate success rate for runs
        /// </summary>
        public float GetRunSuccessRate()
        {
            if (TotalRunsStarted == 0) return 0f;
            return (float)TotalRunsCompleted / TotalRunsStarted;
        }
        
        /// <summary>
        /// Calculate average run distance
        /// </summary>
        public float GetAverageRunDistance()
        {
            if (TotalRunsStarted == 0) return 0f;
            return TotalDistanceTraveled / TotalRunsStarted;
        }
        
        /// <summary>
        /// Calculate average survival time
        /// </summary>
        public float GetAverageSurvivalTime()
        {
            if (TotalRunsStarted == 0) return 0f;
            return TotalSurvivalTime / TotalRunsStarted;
        }
        
        /// <summary>
        /// Get most used bot archetype
        /// </summary>
        public Bot.BotArchetype GetMostUsedArchetype()
        {
            Bot.BotArchetype mostUsed = Bot.BotArchetype.Balanced;
            int maxRuns = 0;
            
            foreach (var kvp in RunsByArchetype)
            {
                if (kvp.Value > maxRuns)
                {
                    maxRuns = kvp.Value;
                    mostUsed = kvp.Key;
                }
            }
            
            return mostUsed;
        }
        
        /// <summary>
        /// Update statistics after a run
        /// </summary>
        public void UpdateRunStatistics(Bot.BotRunStatistics runStats)
        {
            TotalRunsStarted++;
            
            if (runStats.HasCompletedCourse)
            {
                TotalRunsCompleted++;
            }
            else
            {
                TotalRunsFailed++;
            }
            
            // Update distance records
            TotalDistanceTraveled += runStats.DistanceTraveled;
            if (runStats.DistanceTraveled > BestDistance)
            {
                BestDistance = runStats.DistanceTraveled;
            }
            
            // Update survival time records
            TotalSurvivalTime += runStats.SurvivalTime;
            if (runStats.SurvivalTime > BestSurvivalTime)
            {
                BestSurvivalTime = runStats.SurvivalTime;
            }
            
            // Update score
            int runScore = runStats.CalculatePerformanceScore();
            if (runScore > BestScore)
            {
                BestScore = runScore;
            }
            
            // Update archetype usage
            if (!RunsByArchetype.ContainsKey(runStats.BotArchetype))
            {
                RunsByArchetype[runStats.BotArchetype] = 0;
            }
            RunsByArchetype[runStats.BotArchetype]++;
            
            // Update part usage
            foreach (var part in runStats.EquippedParts)
            {
                if (part != null)
                {
                    string partName = part.PartName;
                    if (!PartUsageCount.ContainsKey(partName))
                    {
                        PartUsageCount[partName] = 0;
                    }
                    PartUsageCount[partName]++;
                }
            }
            
            // Update obstacle/collectible stats
            TotalObstaclesHit += runStats.ObstaclesHit;
            TotalObstaclesAvoided += runStats.ObstaclesAvoided;
            TotalCollectiblesGathered += runStats.CollectiblesGathered;
            TotalPowerUpsUsed += runStats.PowerUpsCollected;
            
            LastPlayTime = DateTime.Now;
        }
        
        /// <summary>
        /// Get formatted play time string
        /// </summary>
        public string GetFormattedPlayTime()
        {
            if (TotalPlayTime.TotalHours >= 1)
            {
                return $"{TotalPlayTime.Hours}h {TotalPlayTime.Minutes}m";
            }
            else
            {
                return $"{TotalPlayTime.Minutes}m {TotalPlayTime.Seconds}s";
            }
        }
        
        /// <summary>
        /// Get player efficiency rating
        /// </summary>
        public float GetPlayerEfficiency()
        {
            if (TotalObstaclesHit + TotalObstaclesAvoided == 0) return 1f;
            return (float)TotalObstaclesAvoided / (TotalObstaclesHit + TotalObstaclesAvoided);
        }
    }

    /// <summary>
    /// Save data structure for persistent storage
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("Save Metadata")]
        public int SaveVersion = 1;
        public DateTime SaveTime = DateTime.Now;
        
        [Header("Currency Data")]
        public int CurrentScrap = 0;
        public int CurrentDataCores = 0;
        public int CurrentPremiumCurrency = 0;
        
        [Header("Progression Data")]
        public int PlayerLevel = 1;
        public int TotalXPEarned = 0;
        
        [Header("Energy Data")]
        public int CurrentEnergy = 5;
        public int MaxEnergy = 5;
        public DateTime LastEnergyRegenTime = DateTime.Now;
        
        [Header("Premium Data")]
        public bool HasPremiumAccount = false;
        public DateTime PremiumExpiryDate = default(DateTime);
        public bool HasAdRemoval = false;
        
        [Header("Player Data")]
        public PlayerStatistics PlayerStats = new PlayerStatistics();
        public List<string> UnlockedAchievements = new List<string>();
        public Dictionary<string, bool> UnlockedFeatures = new Dictionary<string, bool>();
        
        [Header("Bot Configuration Data")]
        public List<BotConfigurationSaveData> SavedBotConfigurations = new List<BotConfigurationSaveData>();
        public BotConfigurationSaveData CurrentBotConfiguration = null;
        
        /// <summary>
        /// Validate save data integrity
        /// </summary>
        public bool IsValid()
        {
            // Basic validation checks
            if (SaveVersion <= 0) return false;
            if (PlayerLevel <= 0) return false;
            if (CurrentEnergy < 0 || MaxEnergy <= 0) return false;
            if (CurrentScrap < 0 || CurrentDataCores < 0 || CurrentPremiumCurrency < 0) return false;
            
            return true;
        }
    }

    /// <summary>
    /// Serializable bot configuration data for save system
    /// </summary>
    [System.Serializable]
    public class BotConfigurationSaveData
    {
        public string BotName = "My Bot";
        public Bot.BotArchetype Archetype = Bot.BotArchetype.Balanced;
        public int BotLevel = 1;
        public int TotalXP = 0;
        public List<string> EquippedPartNames = new List<string>();
        public Color PrimaryColor = Color.blue;
        public Color SecondaryColor = Color.white;
        public int VisualStyleID = 0;
        public float BestDistance = 0f;
        public float BestSurvivalTime = 0f;
        public int TotalRunsCompleted = 0;
        public int TotalScrapEarned = 0;
        
        /// <summary>
        /// Convert to BotData for runtime use
        /// </summary>
        public Bot.BotData ToBotData()
        {
            var botData = new Bot.BotData
            {
                BotName = this.BotName,
                Archetype = this.Archetype,
                BotLevel = this.BotLevel,
                TotalXP = this.TotalXP,
                PrimaryColor = this.PrimaryColor,
                SecondaryColor = this.SecondaryColor,
                VisualStyleID = this.VisualStyleID,
                BestDistance = this.BestDistance,
                BestSurvivalTime = this.BestSurvivalTime,
                TotalRunsCompleted = this.TotalRunsCompleted,
                TotalScrapEarned = this.TotalScrapEarned,
                EquippedParts = new List<Bot.BotPart>()
            };
            
            // Load parts from names (would need part database lookup in real implementation)
            foreach (string partName in EquippedPartNames)
            {
                // botData.EquippedParts.Add(LoadPartByName(partName));
            }
            
            return botData;
        }
        
        /// <summary>
        /// Create from BotData
        /// </summary>
        public static BotConfigurationSaveData FromBotData(Bot.BotData botData)
        {
            var saveData = new BotConfigurationSaveData
            {
                BotName = botData.BotName,
                Archetype = botData.Archetype,
                BotLevel = botData.BotLevel,
                TotalXP = botData.TotalXP,
                PrimaryColor = botData.PrimaryColor,
                SecondaryColor = botData.SecondaryColor,
                VisualStyleID = botData.VisualStyleID,
                BestDistance = botData.BestDistance,
                BestSurvivalTime = botData.BestSurvivalTime,
                TotalRunsCompleted = botData.TotalRunsCompleted,
                TotalScrapEarned = botData.TotalScrapEarned,
                EquippedPartNames = new List<string>()
            };
            
            // Save part names
            foreach (var part in botData.EquippedParts)
            {
                if (part != null)
                {
                    saveData.EquippedPartNames.Add(part.PartName);
                }
            }
            
            return saveData;
        }
    }

    /// <summary>
    /// Player progress summary for UI display
    /// </summary>
    [System.Serializable]
    public class PlayerProgressSummary
    {
        public int PlayerLevel;
        public int CurrentXP;
        public float ProgressToNextLevel;
        public int TotalScrap;
        public int TotalDataCores;
        public int CurrentEnergy;
        public int MaxEnergy;
        public bool HasPremiumAccount;
        public int UnlockedAchievementCount;
        public TimeSpan TotalPlayTime;
        public int RunsCompleted;
        
        /// <summary>
        /// Get overall completion percentage
        /// </summary>
        public float GetOverallCompletion()
        {
            // This would be more sophisticated in a real implementation
            float levelCompletion = Mathf.Clamp01((float)PlayerLevel / 50f);
            float achievementCompletion = Mathf.Clamp01((float)UnlockedAchievementCount / 100f);
            
            return (levelCompletion + achievementCompletion) / 2f;
        }
        
        /// <summary>
        /// Get formatted summary text
        /// </summary>
        public string GetSummaryText()
        {
            return $"Level {PlayerLevel} • {RunsCompleted} Runs • {TotalScrap} Scrap";
        }
    }

    /// <summary>
    /// Achievement definition structure
    /// </summary>
    [System.Serializable]
    public class Achievement
    {
        [Header("Achievement Identity")]
        public string AchievementID;
        public string Title;
        public string Description;
        public Sprite Icon;
        
        [Header("Requirements")]
        public AchievementType Type;
        public int RequiredValue;
        public string RequiredData;
        
        [Header("Rewards")]
        public int ScrapReward;
        public int DataCoreReward;
        public int PremiumCurrencyReward;
        public bool UnlocksFeature;
        public string UnlockedFeatureID;
        
        [Header("Display")]
        public Color AchievementColor = Color.gold;
        public bool IsHidden;
        public int DisplayOrder;
        
        /// <summary>
        /// Check if achievement is completed based on player statistics
        /// </summary>
        public bool IsCompleted(PlayerStatistics playerStats)
        {
            switch (Type)
            {
                case AchievementType.RunsCompleted:
                    return playerStats.TotalRunsCompleted >= RequiredValue;
                    
                case AchievementType.DistanceTraveled:
                    return playerStats.TotalDistanceTraveled >= RequiredValue;
                    
                case AchievementType.CollectiblesGathered:
                    return playerStats.TotalCollectiblesGathered >= RequiredValue;
                    
                case AchievementType.ScrapEarned:
                    return playerStats.TotalScrapEarned >= RequiredValue;
                    
                case AchievementType.PerfectRuns:
                    // Would need additional tracking
                    return false;
                    
                case AchievementType.PlayTime:
                    return playerStats.TotalPlayTime.TotalHours >= RequiredValue;
                    
                case AchievementType.BotCustomization:
                    return playerStats.TotalBotsCustomized >= RequiredValue;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Get progress towards completion (0-1)
        /// </summary>
        public float GetProgress(PlayerStatistics playerStats)
        {
            float current = 0f;
            
            switch (Type)
            {
                case AchievementType.RunsCompleted:
                    current = playerStats.TotalRunsCompleted;
                    break;
                case AchievementType.DistanceTraveled:
                    current = playerStats.TotalDistanceTraveled;
                    break;
                case AchievementType.CollectiblesGathered:
                    current = playerStats.TotalCollectiblesGathered;
                    break;
                case AchievementType.ScrapEarned:
                    current = playerStats.TotalScrapEarned;
                    break;
                case AchievementType.PlayTime:
                    current = (float)playerStats.TotalPlayTime.TotalHours;
                    break;
                case AchievementType.BotCustomization:
                    current = playerStats.TotalBotsCustomized;
                    break;
            }
            
            return Mathf.Clamp01(current / RequiredValue);
        }
    }

    /// <summary>
    /// Types of achievements
    /// </summary>
    public enum AchievementType
    {
        RunsCompleted,      // Complete X runs
        DistanceTraveled,   // Travel X total distance
        CollectiblesGathered, // Collect X items
        ScrapEarned,        // Earn X total scrap
        PerfectRuns,        // Complete X runs without taking damage
        PlayTime,           // Play for X hours
        BotCustomization,   // Customize X bots
        SpecialEvent        // Event-specific achievements
    }

    /// <summary>
    /// Daily bonus system data
    /// </summary>
    [System.Serializable]
    public class DailyBonusData
    {
        public DateTime LastBonusClaimedDate = default(DateTime);
        public int ConsecutiveDaysLogged = 0;
        public bool HasClaimedTodaysBonus = false;
        public List<DailyReward> WeeklyRewards = new List<DailyReward>();
        
        /// <summary>
        /// Check if daily bonus is available
        /// </summary>
        public bool IsBonusAvailable()
        {
            DateTime today = DateTime.Today;
            return !HasClaimedTodaysBonus && 
                   (LastBonusClaimedDate == default(DateTime) || LastBonusClaimedDate.Date < today);
        }
        
        /// <summary>
        /// Get today's reward
        /// </summary>
        public DailyReward GetTodaysReward()
        {
            int dayIndex = ConsecutiveDaysLogged % 7;
            if (dayIndex < WeeklyRewards.Count)
            {
                return WeeklyRewards[dayIndex];
            }
            
            // Default reward if not configured
            return new DailyReward
            {
                ScrapAmount = 100 + (ConsecutiveDaysLogged * 10),
                DataCoreAmount = ConsecutiveDaysLogged >= 7 ? 1 : 0
            };
        }
        
        /// <summary>
        /// Claim daily bonus
        /// </summary>
        public DailyReward ClaimBonus()
        {
            DateTime today = DateTime.Today;
            DateTime yesterday = today.AddDays(-1);
            
            // Check if streak continues
            if (LastBonusClaimedDate.Date == yesterday)
            {
                ConsecutiveDaysLogged++;
            }
            else if (LastBonusClaimedDate.Date < yesterday)
            {
                ConsecutiveDaysLogged = 1; // Reset streak
            }
            
            LastBonusClaimedDate = today;
            HasClaimedTodaysBonus = true;
            
            return GetTodaysReward();
        }
        
        /// <summary>
        /// Reset daily flags (called at start of new day)
        /// </summary>
        public void ResetDailyFlags()
        {
            if (DateTime.Today > LastBonusClaimedDate.Date)
            {
                HasClaimedTodaysBonus = false;
            }
        }
    }

    /// <summary>
    /// Individual daily reward
    /// </summary>
    [System.Serializable]
    public class DailyReward
    {
        public int ScrapAmount = 100;
        public int DataCoreAmount = 0;
        public int PremiumCurrencyAmount = 0;
        public int EnergyRefillAmount = 0;
        public string BonusItem = "";
        public bool IsSpecialReward = false;
        
        /// <summary>
        /// Get reward description for UI
        /// </summary>
        public string GetDescription()
        {
            List<string> rewards = new List<string>();
            
            if (ScrapAmount > 0)
                rewards.Add($"{ScrapAmount} Scrap");
            if (DataCoreAmount > 0)
                rewards.Add($"{DataCoreAmount} Data Core" + (DataCoreAmount > 1 ? "s" : ""));
            if (PremiumCurrencyAmount > 0)
                rewards.Add($"{PremiumCurrencyAmount} Premium Currency");
            if (EnergyRefillAmount > 0)
                rewards.Add($"{EnergyRefillAmount} Energy");
            if (!string.IsNullOrEmpty(BonusItem))
                rewards.Add(BonusItem);
                
            return string.Join(", ", rewards);
        }
    }

    /// <summary>
    /// Event system for limited-time content
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        [Header("Event Identity")]
        public string EventID;
        public string EventName;
        public string Description;
        public Sprite EventIcon;
        
        [Header("Event Timing")]
        public DateTime StartTime;
        public DateTime EndTime;
        public bool IsActive;
        
        [Header("Event Modifiers")]
        public float ScrapMultiplier = 1f;
        public float XPMultiplier = 1f;
        public float EnergyRegenBoost = 1f;
        
        [Header("Special Rewards")]
        public List<EventReward> SpecialRewards = new List<EventReward>();
        public Course.CourseTheme EventTheme;
        public bool HasSpecialTheme;
        
        /// <summary>
        /// Check if event is currently active
        /// </summary>
        public bool IsCurrentlyActive()
        {
            DateTime now = DateTime.Now;
            return IsActive && now >= StartTime && now <= EndTime;
        }
        
        /// <summary>
        /// Get time remaining in event
        /// </summary>
        public TimeSpan TimeRemaining()
        {
            if (!IsCurrentlyActive()) return TimeSpan.Zero;
            return EndTime - DateTime.Now;
        }
    }

    /// <summary>
    /// Special event reward
    /// </summary>
    [System.Serializable]
    public class EventReward
    {
        public string RewardID;
        public string RewardName;
        public CurrencyType RewardType;
        public int Amount;
        public string RequiredAction; // "complete_5_runs", "travel_1000m", etc.
        public bool IsCompleted;
        
        /// <summary>
        /// Check if reward requirements are met
        /// </summary>
        public bool CheckCompletion(PlayerStatistics playerStats)
        {
            // This would parse RequiredAction and check against playerStats
            // For now, simplified implementation
            return false;
        }
    }
}