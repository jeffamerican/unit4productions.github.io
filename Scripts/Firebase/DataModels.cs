using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

namespace CircuitRunners.Firebase.DataModels
{
    /// <summary>
    /// Core player profile data structure optimized for Firestore efficiency
    /// Uses denormalization for read performance and batched updates for write efficiency
    /// </summary>
    [FirestoreData]
    public class PlayerProfile
    {
        // Primary identification and metadata
        [FirestoreProperty] public string PlayerId { get; set; }
        [FirestoreProperty] public string DisplayName { get; set; }
        [FirestoreProperty] public string AvatarUrl { get; set; }
        [FirestoreProperty] public Timestamp CreatedAt { get; set; }
        [FirestoreProperty] public Timestamp LastActiveAt { get; set; }
        
        // Game progression data (frequently accessed together)
        [FirestoreProperty] public int Level { get; set; }
        [FirestoreProperty] public long Experience { get; set; }
        [FirestoreProperty] public int PrestigeLevel { get; set; }
        
        // Currency and resources (grouped for atomic updates)
        [FirestoreProperty] public CurrencyData Currencies { get; set; }
        
        // Game statistics (denormalized for leaderboard efficiency)
        [FirestoreProperty] public PlayerStats Stats { get; set; }
        
        // Social and engagement data
        [FirestoreProperty] public List<string> FriendIds { get; set; }
        [FirestoreProperty] public int DailyLoginStreak { get; set; }
        [FirestoreProperty] public Timestamp LastDailyReward { get; set; }
        
        // Monetization tracking (essential for free-to-play optimization)
        [FirestoreProperty] public MonetizationData Monetization { get; set; }
        
        // Settings and preferences
        [FirestoreProperty] public PlayerSettings Settings { get; set; }
        
        public PlayerProfile()
        {
            // Initialize collections to prevent null reference exceptions
            FriendIds = new List<string>();
            Currencies = new CurrencyData();
            Stats = new PlayerStats();
            Monetization = new MonetizationData();
            Settings = new PlayerSettings();
        }
    }

    /// <summary>
    /// Player currency data structure with validation and transaction tracking
    /// Supports multiple currency types with fraud prevention measures
    /// </summary>
    [FirestoreData]
    public class CurrencyData
    {
        [FirestoreProperty] public long Coins { get; set; }           // Primary earned currency
        [FirestoreProperty] public int Gems { get; set; }            // Premium currency
        [FirestoreProperty] public int Energy { get; set; }          // Time-gated resource
        [FirestoreProperty] public int MaxEnergy { get; set; }       // Energy cap (upgradeable)
        [FirestoreProperty] public Timestamp LastEnergyRefill { get; set; }
        
        // Anti-cheat validation fields
        [FirestoreProperty] public string LastTransactionHash { get; set; }
        [FirestoreProperty] public Timestamp LastValidation { get; set; }
        
        public CurrencyData()
        {
            Coins = 1000;          // Starting currency
            Gems = 50;             // Welcome bonus
            Energy = 100;          // Full energy start
            MaxEnergy = 100;       // Base energy cap
        }
    }

    /// <summary>
    /// Comprehensive player statistics for leaderboards and analytics
    /// Optimized for efficient querying and real-time updates
    /// </summary>
    [FirestoreData]
    public class PlayerStats
    {
        // Core gameplay metrics
        [FirestoreProperty] public long TotalScore { get; set; }
        [FirestoreProperty] public long BestRun { get; set; }
        [FirestoreProperty] public int TotalRuns { get; set; }
        [FirestoreProperty] public int WinStreak { get; set; }
        [FirestoreProperty] public int BestWinStreak { get; set; }
        
        // Time-based statistics
        [FirestoreProperty] public int TotalPlayTime { get; set; }    // Seconds
        [FirestoreProperty] public int SessionsPlayed { get; set; }
        [FirestoreProperty] public float AverageSessionLength { get; set; }
        
        // Bot and collection stats
        [FirestoreProperty] public int BotsUnlocked { get; set; }
        [FirestoreProperty] public int BotsMaxLevel { get; set; }
        [FirestoreProperty] public string FavoriteBot { get; set; }
        
        // Achievement and progression
        [FirestoreProperty] public int AchievementsUnlocked { get; set; }
        [FirestoreProperty] public List<string> CompletedChallenges { get; set; }
        
        // Social engagement metrics
        [FirestoreProperty] public int TournamentWins { get; set; }
        [FirestoreProperty] public int TournamentParticipations { get; set; }
        [FirestoreProperty] public int BotsShared { get; set; }
        
        public PlayerStats()
        {
            CompletedChallenges = new List<string>();
            FavoriteBot = "starter_bot";
        }
    }

    /// <summary>
    /// Bot configuration and customization data
    /// Supports sharing, versioning, and community features
    /// </summary>
    [FirestoreData]
    public class BotData
    {
        [FirestoreProperty] public string BotId { get; set; }
        [FirestoreProperty] public string OwnerId { get; set; }
        [FirestoreProperty] public string BotName { get; set; }
        [FirestoreProperty] public string BotType { get; set; }       // "speed", "power", "balanced", "custom"
        [FirestoreProperty] public int Level { get; set; }
        [FirestoreProperty] public Timestamp CreatedAt { get; set; }
        [FirestoreProperty] public Timestamp LastModified { get; set; }
        
        // Bot statistics and performance
        [FirestoreProperty] public BotStats Performance { get; set; }
        
        // Customization and appearance
        [FirestoreProperty] public BotCustomization Customization { get; set; }
        
        // Upgrade and progression system
        [FirestoreProperty] public BotUpgrades Upgrades { get; set; }
        
        // Social and sharing features
        [FirestoreProperty] public bool IsPublic { get; set; }
        [FirestoreProperty] public int Downloads { get; set; }
        [FirestoreProperty] public float CommunityRating { get; set; }
        [FirestoreProperty] public List<string> Tags { get; set; }
        
        public BotData()
        {
            Performance = new BotStats();
            Customization = new BotCustomization();
            Upgrades = new BotUpgrades();
            Tags = new List<string>();
            IsPublic = false;
        }
    }

    /// <summary>
    /// Bot performance statistics for optimization and balancing
    /// </summary>
    [FirestoreData]
    public class BotStats
    {
        [FirestoreProperty] public int Speed { get; set; }           // 1-100 scale
        [FirestoreProperty] public int Power { get; set; }          // 1-100 scale  
        [FirestoreProperty] public int Durability { get; set; }     // 1-100 scale
        [FirestoreProperty] public int Special { get; set; }        // 1-100 scale
        [FirestoreProperty] public float WinRate { get; set; }      // Performance metric
        [FirestoreProperty] public int BattlesWon { get; set; }
        [FirestoreProperty] public int BattlesFought { get; set; }
        [FirestoreProperty] public long BestScore { get; set; }
        
        public int GetOverallRating()
        {
            return (Speed + Power + Durability + Special) / 4;
        }
    }

    /// <summary>
    /// Bot visual customization options
    /// </summary>
    [FirestoreData]
    public class BotCustomization
    {
        [FirestoreProperty] public string PrimaryColor { get; set; }    // Hex color
        [FirestoreProperty] public string SecondaryColor { get; set; }  // Hex color
        [FirestoreProperty] public string PatternType { get; set; }     // Visual pattern
        [FirestoreProperty] public List<string> Accessories { get; set; } // Unlocked accessories
        [FirestoreProperty] public string EmoteSet { get; set; }        // Celebration animations
        
        public BotCustomization()
        {
            Accessories = new List<string>();
            PrimaryColor = "#FF6B35";      // Circuit runner orange
            SecondaryColor = "#2E86AB";    // Circuit runner blue
            PatternType = "solid";
        }
    }

    /// <summary>
    /// Bot upgrade system with progression tracking
    /// </summary>
    [FirestoreData]
    public class BotUpgrades
    {
        [FirestoreProperty] public int SpeedLevel { get; set; }
        [FirestoreProperty] public int PowerLevel { get; set; }
        [FirestoreProperty] public int DurabilityLevel { get; set; }
        [FirestoreProperty] public int SpecialLevel { get; set; }
        [FirestoreProperty] public long TotalUpgradeCost { get; set; } // For analytics
        [FirestoreProperty] public List<string> UnlockedAbilities { get; set; }
        
        public BotUpgrades()
        {
            UnlockedAbilities = new List<string>();
            // All upgrades start at level 1
            SpeedLevel = 1;
            PowerLevel = 1;
            DurabilityLevel = 1;
            SpecialLevel = 1;
        }
        
        public int GetTotalUpgradeLevel()
        {
            return SpeedLevel + PowerLevel + DurabilityLevel + SpecialLevel;
        }
    }

    /// <summary>
    /// Leaderboard entry structure optimized for real-time updates and queries
    /// Supports multiple leaderboard types with efficient indexing
    /// </summary>
    [FirestoreData]
    public class LeaderboardEntry
    {
        [FirestoreProperty] public string PlayerId { get; set; }
        [FirestoreProperty] public string DisplayName { get; set; }
        [FirestoreProperty] public string AvatarUrl { get; set; }
        [FirestoreProperty] public long Score { get; set; }
        [FirestoreProperty] public int Rank { get; set; }
        [FirestoreProperty] public Timestamp AchievedAt { get; set; }
        [FirestoreProperty] public string BotUsed { get; set; }      // For strategy analysis
        [FirestoreProperty] public string LeaderboardType { get; set; } // "global", "weekly", "friends"
        [FirestoreProperty] public Dictionary<string, object> MetaData { get; set; }
        
        public LeaderboardEntry()
        {
            MetaData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Tournament and event data structure
    /// Supports recurring tournaments with different rule sets
    /// </summary>
    [FirestoreData]
    public class Tournament
    {
        [FirestoreProperty] public string TournamentId { get; set; }
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public string Description { get; set; }
        [FirestoreProperty] public Timestamp StartTime { get; set; }
        [FirestoreProperty] public Timestamp EndTime { get; set; }
        [FirestoreProperty] public bool IsActive { get; set; }
        [FirestoreProperty] public string TournamentType { get; set; } // "weekly", "special", "seasonal"
        
        // Tournament rules and restrictions
        [FirestoreProperty] public TournamentRules Rules { get; set; }
        
        // Prizes and rewards
        [FirestoreProperty] public List<TournamentReward> Rewards { get; set; }
        
        // Participation tracking
        [FirestoreProperty] public int MaxParticipants { get; set; }
        [FirestoreProperty] public int CurrentParticipants { get; set; }
        [FirestoreProperty] public List<string> ParticipantIds { get; set; }
        
        public Tournament()
        {
            Rules = new TournamentRules();
            Rewards = new List<TournamentReward>();
            ParticipantIds = new List<string>();
        }
    }

    /// <summary>
    /// Tournament rules and restrictions for fair competition
    /// </summary>
    [FirestoreData]
    public class TournamentRules
    {
        [FirestoreProperty] public List<string> AllowedBotTypes { get; set; }
        [FirestoreProperty] public int MaxBotLevel { get; set; }
        [FirestoreProperty] public int MinPlayerLevel { get; set; }
        [FirestoreProperty] public bool AllowUpgrades { get; set; }
        [FirestoreProperty] public int MaxAttempts { get; set; }
        
        public TournamentRules()
        {
            AllowedBotTypes = new List<string>();
            MaxBotLevel = 50;
            MinPlayerLevel = 1;
            AllowUpgrades = true;
            MaxAttempts = 3;
        }
    }

    /// <summary>
    /// Tournament reward structure with tier-based prizes
    /// </summary>
    [FirestoreData]
    public class TournamentReward
    {
        [FirestoreProperty] public int MinRank { get; set; }
        [FirestoreProperty] public int MaxRank { get; set; }
        [FirestoreProperty] public Dictionary<string, int> Rewards { get; set; } // "coins", "gems", "items"
        [FirestoreProperty] public string SpecialReward { get; set; }            // Unique items/bots
        
        public TournamentReward()
        {
            Rewards = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Monetization tracking data for analytics and optimization
    /// Critical for understanding player spending patterns and LTV
    /// </summary>
    [FirestoreData]
    public class MonetizationData
    {
        [FirestoreProperty] public float TotalSpent { get; set; }              // Lifetime revenue
        [FirestoreProperty] public int PurchaseCount { get; set; }             // Total purchases
        [FirestoreProperty] public Timestamp FirstPurchase { get; set; }       // Conversion timing
        [FirestoreProperty] public Timestamp LastPurchase { get; set; }        // Recency
        [FirestoreProperty] public List<string> PurchasedItems { get; set; }   // Purchase history
        [FirestoreProperty] public int AdsWatched { get; set; }                // Ad engagement
        [FirestoreProperty] public float AdRevenue { get; set; }               // Estimated ad revenue
        [FirestoreProperty] public string PlayerSegment { get; set; }          // "whale", "dolphin", "f2p"
        [FirestoreProperty] public bool HasPremiumPass { get; set; }           // Premium subscription
        [FirestoreProperty] public Timestamp PremiumExpiry { get; set; }       // Subscription end date
        
        public MonetizationData()
        {
            PurchasedItems = new List<string>();
            PlayerSegment = "f2p";  // Free-to-play by default
        }
        
        /// <summary>
        /// Calculate estimated Lifetime Value based on spending patterns
        /// </summary>
        public float EstimateLifetimeValue()
        {
            if (PurchaseCount == 0) return AdRevenue;
            
            var daysSinceFirst = (DateTime.UtcNow - FirstPurchase.ToDateTime()).Days;
            if (daysSinceFirst <= 0) return TotalSpent + AdRevenue;
            
            var spendingRate = TotalSpent / daysSinceFirst;
            var projectedLifetime = 365f; // 1 year projection
            
            return (spendingRate * projectedLifetime) + AdRevenue;
        }
    }

    /// <summary>
    /// Player settings and preferences for personalized experience
    /// </summary>
    [FirestoreData]
    public class PlayerSettings
    {
        [FirestoreProperty] public bool SoundEnabled { get; set; }
        [FirestoreProperty] public bool MusicEnabled { get; set; }
        [FirestoreProperty] public bool NotificationsEnabled { get; set; }
        [FirestoreProperty] public bool SocialFeaturesEnabled { get; set; }
        [FirestoreProperty] public string Language { get; set; }
        [FirestoreProperty] public int GraphicsQuality { get; set; }    // 0=Low, 1=Medium, 2=High
        [FirestoreProperty] public bool AnalyticsOptOut { get; set; }   // GDPR compliance
        [FirestoreProperty] public string TimeZone { get; set; }
        
        public PlayerSettings()
        {
            SoundEnabled = true;
            MusicEnabled = true;
            NotificationsEnabled = true;
            SocialFeaturesEnabled = true;
            Language = "en";
            GraphicsQuality = 1; // Medium by default
            AnalyticsOptOut = false;
            TimeZone = "UTC";
        }
    }

    /// <summary>
    /// Analytics event data structure for comprehensive tracking
    /// Optimized for Firebase Analytics custom events
    /// </summary>
    [System.Serializable]
    public class AnalyticsEventData
    {
        public string eventName;
        public string playerId;
        public Dictionary<string, object> parameters;
        public DateTime timestamp;
        public string sessionId;
        public string buildVersion;
        public string platform;
        
        public AnalyticsEventData(string eventName)
        {
            this.eventName = eventName;
            this.parameters = new Dictionary<string, object>();
            this.timestamp = DateTime.UtcNow;
            this.buildVersion = Application.version;
            this.platform = Application.platform.ToString();
        }
        
        // Helper methods for common event types
        public static AnalyticsEventData CreateGameplayEvent(string action, int score, string botUsed)
        {
            var eventData = new AnalyticsEventData("gameplay_action");
            eventData.parameters["action"] = action;
            eventData.parameters["score"] = score;
            eventData.parameters["bot_used"] = botUsed;
            return eventData;
        }
        
        public static AnalyticsEventData CreateMonetizationEvent(string action, string item, float value)
        {
            var eventData = new AnalyticsEventData("monetization_action");
            eventData.parameters["action"] = action;
            eventData.parameters["item"] = item;
            eventData.parameters["value"] = value;
            return eventData;
        }
        
        public static AnalyticsEventData CreateProgressionEvent(string milestone, int level, int experience)
        {
            var eventData = new AnalyticsEventData("player_progression");
            eventData.parameters["milestone"] = milestone;
            eventData.parameters["level"] = level;
            eventData.parameters["experience"] = experience;
            return eventData;
        }
    }
}

/// <summary>
/// Data model validation and utility class
/// Provides helper methods for data integrity and optimization
/// </summary>
namespace CircuitRunners.Firebase.Utils
{
    public static class DataModelValidator
    {
        /// <summary>
        /// Validates player profile data before Firestore write operations
        /// Prevents invalid data from being stored and ensures consistency
        /// </summary>
        public static bool ValidatePlayerProfile(DataModels.PlayerProfile profile)
        {
            if (string.IsNullOrEmpty(profile.PlayerId)) return false;
            if (string.IsNullOrEmpty(profile.DisplayName)) return false;
            if (profile.Level < 1) return false;
            if (profile.Currencies.Coins < 0) return false;
            if (profile.Currencies.Gems < 0) return false;
            if (profile.Currencies.Energy < 0) return false;
            
            return true;
        }
        
        /// <summary>
        /// Validates bot data for consistency and security
        /// </summary>
        public static bool ValidateBotData(DataModels.BotData bot)
        {
            if (string.IsNullOrEmpty(bot.BotId)) return false;
            if (string.IsNullOrEmpty(bot.OwnerId)) return false;
            if (string.IsNullOrEmpty(bot.BotName)) return false;
            if (bot.Level < 1 || bot.Level > 100) return false;
            
            // Validate stat ranges
            var stats = bot.Performance;
            if (stats.Speed < 1 || stats.Speed > 100) return false;
            if (stats.Power < 1 || stats.Power > 100) return false;
            if (stats.Durability < 1 || stats.Durability > 100) return false;
            if (stats.Special < 1 || stats.Special > 100) return false;
            
            return true;
        }
        
        /// <summary>
        /// Calculates estimated Firestore operation costs for budget monitoring
        /// </summary>
        public static int EstimateFirestoreOperations(string operationType, int dataSize)
        {
            switch (operationType.ToLower())
            {
                case "player_create":
                    return 1; // Single document write
                case "player_update":
                    return 1; // Single document update
                case "leaderboard_read":
                    return Math.Min(dataSize, 50); // Limit leaderboard queries
                case "bot_collection_read":
                    return Math.Min(dataSize, 20); // Limit bot queries
                default:
                    return 1;
            }
        }
    }
}