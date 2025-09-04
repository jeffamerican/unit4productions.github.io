using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRunners.Bot
{
    /// <summary>
    /// Bot archetype enumeration defining the 6 main bot types with distinct behavior patterns
    /// Each archetype has unique stat modifiers and AI behavior characteristics
    /// </summary>
    public enum BotArchetype
    {
        Balanced,   // No special modifiers, good all-around performance
        Speed,      // High speed, low health, aggressive AI
        Tank,       // High health, slow speed, defensive AI
        Jumper,     // Enhanced jumping, moderate speed, risk-taking AI
        Collector,  // Prioritizes collectibles, moderate stats, gathering-focused AI
        Lucky,      // Random bonuses, standard stats, risk-taking AI
        Hacker      // Enhanced sensors, good stats, strategic AI
    }

    /// <summary>
    /// AI decision types that the bot can make during runs
    /// These correspond to the available actions the bot can perform
    /// </summary>
    public enum AIDecisionType
    {
        Continue,   // Keep running forward (default action)
        Jump,       // Jump over obstacles or reach collectibles
        Slide,      // Slide under obstacles or through narrow spaces
        Dash        // Dash through obstacles or boost forward
    }

    /// <summary>
    /// Complete bot configuration data including archetype and equipped parts
    /// This is the primary data structure for saving/loading bot setups
    /// </summary>
    [System.Serializable]
    public class BotData
    {
        [Header("Bot Identity")]
        public string BotName = "Circuit Runner";
        public BotArchetype Archetype = BotArchetype.Balanced;
        public int BotLevel = 1;
        public int TotalXP = 0;

        [Header("Equipment")]
        public List<BotPart> EquippedParts = new List<BotPart>();
        
        [Header("Customization")]
        public Color PrimaryColor = Color.blue;
        public Color SecondaryColor = Color.white;
        public int VisualStyleID = 0;

        [Header("Performance Stats")]
        public int TotalRunsCompleted = 0;
        public float BestDistance = 0f;
        public float BestSurvivalTime = 0f;
        public int TotalScrapEarned = 0;

        /// <summary>
        /// Calculate total stat bonuses from all equipped parts
        /// </summary>
        public BotPartModifiers GetTotalModifiers()
        {
            BotPartModifiers total = new BotPartModifiers();
            
            foreach (var part in EquippedParts)
            {
                if (part != null)
                {
                    total.SpeedModifier += part.SpeedModifier;
                    total.JumpModifier += part.JumpModifier;
                    total.DashModifier += part.DashModifier;
                    total.HealthModifier += part.HealthModifier;
                    total.DamageReduction += part.DamageReduction;
                    total.RiskToleranceModifier += part.RiskToleranceModifier;
                    total.CollectiblePriorityModifier += part.CollectiblePriorityModifier;
                    total.DashCooldownReduction += part.DashCooldownReduction;
                }
            }
            
            return total;
        }

        /// <summary>
        /// Get display name with level information
        /// </summary>
        public string GetDisplayName()
        {
            return $"{BotName} (Lv.{BotLevel})";
        }

        /// <summary>
        /// Calculate overall bot rating based on stats and equipment
        /// </summary>
        public int CalculateOverallRating()
        {
            int baseRating = BotLevel * 10;
            int partRating = EquippedParts.Count * 5;
            int performanceRating = Mathf.FloorToInt(BestDistance / 10f);
            
            return baseRating + partRating + performanceRating;
        }
    }

    /// <summary>
    /// Individual bot part data structure for modular customization system
    /// Parts provide stat modifiers and special abilities
    /// </summary>
    [CreateAssetMenu(fileName = "New Bot Part", menuName = "Circuit Runners/Bot Part")]
    public class BotPart : ScriptableObject
    {
        [Header("Part Identity")]
        public string PartName = "Basic Part";
        public string Description = "A basic bot part";
        public BotPartType PartType = BotPartType.Chassis;
        public BotPartRarity Rarity = BotPartRarity.Common;
        
        [Header("Visual")]
        public Sprite PartIcon;
        public Sprite PartSprite;
        public Color PartColor = Color.white;
        
        [Header("Stat Modifiers")]
        public float SpeedModifier = 0f;
        public float JumpModifier = 0f;
        public float DashModifier = 0f;
        public int HealthModifier = 0;
        public int DamageReduction = 0;
        
        [Header("AI Behavior Modifiers")]
        public float RiskToleranceModifier = 0f;
        public float CollectiblePriorityModifier = 0f;
        
        [Header("Ability Modifiers")]
        public float DashCooldownReduction = 0f;
        public float SpeedBoostMultiplier = 1f;
        public bool HasActiveSpeedBoost = false;
        public float SpeedBoostDuration = 0f;
        
        [Header("Unlock Requirements")]
        public int RequiredLevel = 1;
        public int UnlockCost = 100;
        public List<BotPart> RequiredParts = new List<BotPart>();
        
        [Header("Monetization")]
        public bool IsPremiumPart = false;
        public int PremiumCurrencyCost = 0;

        /// <summary>
        /// Check if this part can be equipped on the given archetype
        /// </summary>
        public bool IsCompatibleWith(BotArchetype archetype)
        {
            // Some parts might be archetype-specific
            switch (PartType)
            {
                case BotPartType.SpeedBooster:
                    return archetype == BotArchetype.Speed || archetype == BotArchetype.Balanced;
                    
                case BotPartType.ArmorPlating:
                    return archetype == BotArchetype.Tank || archetype == BotArchetype.Balanced;
                    
                case BotPartType.JumpEnhancer:
                    return archetype == BotArchetype.Jumper || archetype == BotArchetype.Balanced;
                    
                default:
                    return true; // Most parts are compatible with all archetypes
            }
        }

        /// <summary>
        /// Get rarity-based color for UI display
        /// </summary>
        public Color GetRarityColor()
        {
            switch (Rarity)
            {
                case BotPartRarity.Common:
                    return Color.white;
                case BotPartRarity.Uncommon:
                    return Color.green;
                case BotPartRarity.Rare:
                    return Color.blue;
                case BotPartRarity.Epic:
                    return Color.magenta;
                case BotPartRarity.Legendary:
                    return Color.yellow;
                default:
                    return Color.gray;
            }
        }
    }

    /// <summary>
    /// Bot part type categories for equipment system
    /// </summary>
    public enum BotPartType
    {
        Chassis,        // Main body - affects health and base stats
        Processor,      // CPU - affects AI behavior and reaction time
        Sensors,        // Detection systems - affects scan range and accuracy
        Mobility,       // Movement systems - affects speed and agility
        SpeedBooster,   // Speed enhancement modules
        ArmorPlating,   // Defense systems - damage reduction
        JumpEnhancer,   // Jump improvement systems
        PowerCore,      // Energy systems - affects abilities
        Collectible,    // Collection optimization systems
        Special         // Unique parts with special abilities
    }

    /// <summary>
    /// Rarity tiers for bot parts affecting availability and power
    /// </summary>
    public enum BotPartRarity
    {
        Common,     // Easily obtainable, basic bonuses
        Uncommon,   // Moderate bonuses, some unlock requirements
        Rare,       // Good bonuses, harder to obtain
        Epic,       // Strong bonuses, rare drops or purchases
        Legendary   // Exceptional bonuses, premium or achievement rewards
    }

    /// <summary>
    /// Consolidated structure for part stat modifiers
    /// Makes it easier to calculate total bonuses from all parts
    /// </summary>
    [System.Serializable]
    public struct BotPartModifiers
    {
        public float SpeedModifier;
        public float JumpModifier;
        public float DashModifier;
        public int HealthModifier;
        public int DamageReduction;
        public float RiskToleranceModifier;
        public float CollectiblePriorityModifier;
        public float DashCooldownReduction;
    }

    /// <summary>
    /// AI decision data structure containing all information about a bot's decision
    /// Used by the AI system to plan and execute actions
    /// </summary>
    [System.Serializable]
    public class AIDecision
    {
        [Header("Decision Details")]
        public AIDecisionType DecisionType = AIDecisionType.Continue;
        public float ExecuteTime = 0f;
        public float Duration = 0.5f;
        public float Priority = 0.5f;
        public Transform TargetObject = null;
        public string Description = "";
        
        [Header("Execution State")]
        public bool IsExecuted = false;
        public float ExecutionStartTime = 0f;
        
        /// <summary>
        /// Whether this decision is currently being executed
        /// </summary>
        public bool IsActive => IsExecuted && !IsComplete;
        
        /// <summary>
        /// Whether this decision has finished executing
        /// </summary>
        public bool IsComplete => IsExecuted && (Time.time - ExecutionStartTime) >= Duration;
        
        /// <summary>
        /// Start executing this decision
        /// </summary>
        public void StartExecution()
        {
            IsExecuted = true;
            ExecutionStartTime = Time.time;
        }
    }

    /// <summary>
    /// ScriptableObject container for AI behavior configuration
    /// Allows designers to tweak AI behavior without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "AI Decision Data", menuName = "Circuit Runners/AI Decision Data")]
    public class AIDecisionData : ScriptableObject
    {
        [Header("Behavior Weights")]
        [Range(0f, 1f)] public float SafetyBias = 0.5f;
        [Range(0f, 1f)] public float AggressivenessBias = 0.5f;
        [Range(0f, 1f)] public float CollectibleBias = 0.7f;
        
        [Header("Decision Thresholds")]
        public float MinThreatDistance = 2f;
        public float MaxReactionTime = 1f;
        public float CollectibleDetectionRange = 15f;
        
        [Header("Action Preferences")]
        public float JumpPreference = 0.6f;
        public float SlidePreference = 0.4f;
        public float DashPreference = 0.3f;
    }

    /// <summary>
    /// Information about detected obstacles for AI decision-making
    /// </summary>
    [System.Serializable]
    public struct ObstacleInfo
    {
        public Transform Transform;
        public float Distance;
        public string ObstacleType;
        public AIDecisionType RequiredAction;
        public float ThreatLevel;
    }

    /// <summary>
    /// Information about detected collectibles for AI decision-making
    /// </summary>
    [System.Serializable]
    public struct CollectibleInfo
    {
        public Transform Transform;
        public float Distance;
        public string CollectibleType;
        public int Value;
        public float Priority;
    }

    /// <summary>
    /// Comprehensive statistics tracking for bot runs
    /// Used for progression, achievements, and analytics
    /// </summary>
    [System.Serializable]
    public class BotRunStatistics
    {
        [Header("Run Info")]
        public float StartTime = 0f;
        public float EndTime = 0f;
        public float SurvivalTime = 0f;
        public string EndReason = "";
        public BotArchetype BotArchetype = BotArchetype.Balanced;
        public List<BotPart> EquippedParts = new List<BotPart>();
        
        [Header("Performance Metrics")]
        public float DistanceTraveled = 0f;
        public float AverageSpeed = 0f;
        public float LastProgressTime = 0f;
        public bool HasCompletedCourse = false;
        public string DestructionReason = "";
        
        [Header("Action Statistics")]
        public int JumpsPerformed = 0;
        public int SlidesPerformed = 0;
        public int DashesPerformed = 0;
        public int ObstaclesHit = 0;
        public int ObstaclesAvoided = 0;
        public int SuccessfulLandings = 0;
        
        [Header("Collection Statistics")]
        public int CollectiblesGathered = 0;
        public int PowerUpsCollected = 0;
        public int ScrapCollected = 0;
        
        [Header("Damage Statistics")]
        public int DamagesTaken = 0;
        
        /// <summary>
        /// Calculate overall performance score for this run
        /// </summary>
        public int CalculatePerformanceScore()
        {
            int score = 0;
            
            // Distance bonus (1 point per meter)
            score += Mathf.FloorToInt(DistanceTraveled);
            
            // Survival time bonus (2 points per second)
            score += Mathf.FloorToInt(SurvivalTime * 2f);
            
            // Collection bonus (10 points per collectible)
            score += CollectiblesGathered * 10;
            
            // Completion bonus
            if (HasCompletedCourse)
            {
                score += 500;
            }
            
            // Perfect run bonus (no damage)
            if (DamagesTaken == 0 && DistanceTraveled > 50f)
            {
                score += 200;
            }
            
            // Skill bonus (successful actions)
            score += (JumpsPerformed + SlidesPerformed + DashesPerformed) * 5;
            
            return score;
        }
        
        /// <summary>
        /// Get a letter grade for this run's performance
        /// </summary>
        public string GetPerformanceGrade()
        {
            int score = CalculatePerformanceScore();
            
            if (score >= 1000) return "S";
            if (score >= 800) return "A";
            if (score >= 600) return "B";
            if (score >= 400) return "C";
            if (score >= 200) return "D";
            return "F";
        }
        
        /// <summary>
        /// Generate summary text for UI display
        /// </summary>
        public string GetSummaryText()
        {
            return $"Distance: {DistanceTraveled:F1}m | Time: {SurvivalTime:F1}s | " +
                   $"Collectibles: {CollectiblesGathered} | Score: {CalculatePerformanceScore()}";
        }
    }

    /// <summary>
    /// Event args for bot-related events
    /// Provides context information for UI updates and analytics
    /// </summary>
    public class BotEventArgs : EventArgs
    {
        public BotController Bot { get; set; }
        public string EventType { get; set; }
        public object EventData { get; set; }
        public float Timestamp { get; set; }
        
        public BotEventArgs(BotController bot, string eventType, object eventData = null)
        {
            Bot = bot;
            EventType = eventType;
            EventData = eventData;
            Timestamp = Time.time;
        }
    }
}