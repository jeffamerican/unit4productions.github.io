using UnityEngine;
using CircuitRunners.Course;

/// <summary>
/// Example course configurations to demonstrate the procedural generation system.
/// These would normally be created as individual ScriptableObject assets in Unity,
/// but are provided here as code examples for the MVP implementation.
/// 
/// To use these in Unity:
/// 1. Right-click in Project -> Create -> Circuit Runners -> Course Configuration
/// 2. Configure the values as shown in these examples
/// 3. Save the asset in Assets/ScriptableObjects/Courses/
/// </summary>
namespace CircuitRunners.ScriptableObjects.Courses
{
    /// <summary>
    /// Course configurations for different difficulty levels and themes
    /// </summary>
    public static class CourseConfigurations
    {
        /// <summary>
        /// Beginner-friendly course configuration
        /// </summary>
        public static CourseConfiguration CreateBeginnerConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            
            // Basic settings
            config.name = "Beginner Course Config";
            config.MaxDifficulty = CourseDifficulty.Medium;
            config.DifficultyRampRate = 0.005f; // Slow difficulty increase
            config.DifficultyDecayRate = 0.01f; // Fast difficulty decrease after failure
            
            // Theme progression
            config.ThemeProgression = new CourseTheme[] 
            { 
                CourseTheme.Industrial, 
                CourseTheme.Neon 
            };
            config.ThemeUnlockLevels = new int[] { 1, 10 };
            
            // Section configuration
            config.BaseSectionLength = 60f; // Longer sections for beginners
            config.SectionVariationCount = 3;
            config.MinSectionSpacing = 8f; // More generous spacing
            
            // Element density (lower for beginners)
            config.ObstacleDensity = 0.25f;
            config.CollectibleDensity = 0.4f; // More collectibles
            config.PowerUpDensity = 0.15f; // More power-ups
            config.HazardDensity = 0.05f; // Minimal hazards
            
            // Create difficulty curve (starts easy, ramps slowly)
            config.DifficultyOverDistance = CreateEasyDifficultyCurve();
            config.RewardOverDifficulty = CreateGenerousRewardCurve();
            
            return config;
        }
        
        /// <summary>
        /// Standard course configuration for intermediate players
        /// </summary>
        public static CourseConfiguration CreateStandardConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            
            config.name = "Standard Course Config";
            config.MaxDifficulty = CourseDifficulty.Hard;
            config.DifficultyRampRate = 0.01f; // Moderate difficulty increase
            config.DifficultyDecayRate = 0.005f; // Moderate difficulty decrease
            
            // Extended theme progression
            config.ThemeProgression = new CourseTheme[] 
            { 
                CourseTheme.Industrial, 
                CourseTheme.Neon, 
                CourseTheme.Wasteland, 
                CourseTheme.Digital 
            };
            config.ThemeUnlockLevels = new int[] { 1, 5, 10, 15 };
            
            // Balanced section configuration
            config.BaseSectionLength = 50f;
            config.SectionVariationCount = 5;
            config.MinSectionSpacing = 5f;
            
            // Balanced element density
            config.ObstacleDensity = 0.4f;
            config.CollectibleDensity = 0.3f;
            config.PowerUpDensity = 0.1f;
            config.HazardDensity = 0.15f;
            
            config.DifficultyOverDistance = CreateBalancedDifficultyCurve();
            config.RewardOverDifficulty = CreateBalancedRewardCurve();
            
            return config;
        }
        
        /// <summary>
        /// Challenging course configuration for experienced players
        /// </summary>
        public static CourseConfiguration CreateExpertConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            
            config.name = "Expert Course Config";
            config.MaxDifficulty = CourseDifficulty.Extreme;
            config.DifficultyRampRate = 0.015f; // Fast difficulty increase
            config.DifficultyDecayRate = 0.002f; // Slow difficulty decrease
            
            // Full theme progression
            config.ThemeProgression = new CourseTheme[] 
            { 
                CourseTheme.Industrial, 
                CourseTheme.Neon, 
                CourseTheme.Wasteland, 
                CourseTheme.Digital,
                CourseTheme.Arctic,
                CourseTheme.Volcanic,
                CourseTheme.Space
            };
            config.ThemeUnlockLevels = new int[] { 1, 3, 6, 10, 15, 20, 25 };
            
            // Tight section configuration
            config.BaseSectionLength = 40f; // Shorter sections
            config.SectionVariationCount = 8; // More variety
            config.MinSectionSpacing = 3f; // Tighter spacing
            
            // High-intensity element density
            config.ObstacleDensity = 0.6f;
            config.CollectibleDensity = 0.2f; // Fewer collectibles, more skill-based
            config.PowerUpDensity = 0.08f; // Rare power-ups
            config.HazardDensity = 0.25f; // Significant hazard presence
            
            config.DifficultyOverDistance = CreateChallengingDifficultyCurve();
            config.RewardOverDifficulty = CreateSkillBasedRewardCurve();
            
            return config;
        }
        
        /// <summary>
        /// Event-specific course configuration with special modifiers
        /// </summary>
        public static CourseConfiguration CreateEventConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            
            config.name = "Special Event Config";
            config.MaxDifficulty = CourseDifficulty.Hard;
            config.DifficultyRampRate = 0.008f;
            config.DifficultyDecayRate = 0.008f;
            
            // Event theme focus
            config.ThemeProgression = new CourseTheme[] { CourseTheme.Space, CourseTheme.Digital };
            config.ThemeUnlockLevels = new int[] { 1, 1 }; // Both available immediately
            
            config.BaseSectionLength = 45f;
            config.SectionVariationCount = 6;
            config.MinSectionSpacing = 4f;
            
            // Event-specific density (more collectibles for rewards)
            config.ObstacleDensity = 0.35f;
            config.CollectibleDensity = 0.5f; // High collectible density
            config.PowerUpDensity = 0.2f; // Frequent power-ups
            config.HazardDensity = 0.1f; // Reduced hazards for fun factor
            
            config.DifficultyOverDistance = CreateEventDifficultyCurve();
            config.RewardOverDifficulty = CreateEventRewardCurve();
            
            return config;
        }
        
        /// <summary>
        /// Speed run focused configuration
        /// </summary>
        public static CourseConfiguration CreateSpeedRunConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            
            config.name = "Speed Run Config";
            config.MaxDifficulty = CourseDifficulty.Extreme;
            config.DifficultyRampRate = 0.02f; // Very fast ramp
            config.DifficultyDecayRate = 0.001f; // Minimal decay
            
            // Fast-paced themes
            config.ThemeProgression = new CourseTheme[] 
            { 
                CourseTheme.Digital, 
                CourseTheme.Space 
            };
            config.ThemeUnlockLevels = new int[] { 1, 1 };
            
            // Optimized for speed
            config.BaseSectionLength = 35f; // Short sections
            config.SectionVariationCount = 10; // High variety
            config.MinSectionSpacing = 2f; // Very tight spacing
            
            // Speed-focused density
            config.ObstacleDensity = 0.7f; // High obstacle density
            config.CollectibleDensity = 0.15f; // Minimal collectibles
            config.PowerUpDensity = 0.05f; // Rare power-ups
            config.HazardDensity = 0.3f; // High stakes
            
            config.DifficultyOverDistance = CreateSpeedRunDifficultyCurve();
            config.RewardOverDifficulty = CreateSpeedRunRewardCurve();
            
            return config;
        }
        
        #region Difficulty Curve Creation
        /// <summary>
        /// Create easy difficulty curve for beginners
        /// </summary>
        private static AnimationCurve CreateEasyDifficultyCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);      // Start at 0 difficulty
            curve.AddKey(0.2f, 0.1f);  // Very slow initial ramp
            curve.AddKey(0.5f, 0.3f);  // Gentle middle section
            curve.AddKey(1f, 0.6f);    // Max at 60% difficulty
            
            // Set tangents for smooth curve
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create balanced difficulty curve for standard play
        /// </summary>
        private static AnimationCurve CreateBalancedDifficultyCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);      // Start at 0 difficulty
            curve.AddKey(0.1f, 0.15f); // Quick initial ramp
            curve.AddKey(0.3f, 0.4f);  // Steady progression
            curve.AddKey(0.6f, 0.7f);  // Challenging middle
            curve.AddKey(1f, 1f);      // Full difficulty at end
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create challenging difficulty curve for experts
        /// </summary>
        private static AnimationCurve CreateChallengingDifficultyCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0.2f);    // Start at higher baseline
            curve.AddKey(0.1f, 0.4f);  // Rapid initial ramp
            curve.AddKey(0.3f, 0.7f);  // Quick progression
            curve.AddKey(0.5f, 0.9f);  // Near max difficulty early
            curve.AddKey(1f, 1f);      // Maximum difficulty
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create event-specific difficulty curve
        /// </summary>
        private static AnimationCurve CreateEventDifficultyCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);      // Easy start
            curve.AddKey(0.3f, 0.3f);  // Gradual build
            curve.AddKey(0.7f, 0.5f);  // Plateau for enjoyment
            curve.AddKey(1f, 0.8f);    // Final challenge
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create speed run difficulty curve (aggressive ramp)
        /// </summary>
        private static AnimationCurve CreateSpeedRunDifficultyCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0.3f);    // Start challenging
            curve.AddKey(0.2f, 0.6f);  // Rapid increase
            curve.AddKey(0.5f, 0.9f);  // Near maximum early
            curve.AddKey(1f, 1f);      // Full intensity
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        #endregion
        
        #region Reward Curve Creation
        /// <summary>
        /// Create generous reward curve for beginners
        /// </summary>
        private static AnimationCurve CreateGenerousRewardCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 1f);      // High base rewards
            curve.AddKey(0.5f, 1.5f);  // Generous scaling
            curve.AddKey(1f, 2f);      // Double rewards at high difficulty
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create balanced reward curve for standard play
        /// </summary>
        private static AnimationCurve CreateBalancedRewardCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 1f);      // Standard base rewards
            curve.AddKey(0.5f, 1.3f);  // Moderate scaling
            curve.AddKey(1f, 1.8f);    // Good rewards for high difficulty
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create skill-based reward curve for experts
        /// </summary>
        private static AnimationCurve CreateSkillBasedRewardCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0.8f);    // Lower base rewards
            curve.AddKey(0.3f, 1.2f);  // Reward skill progression
            curve.AddKey(0.7f, 2f);    // High rewards for high skill
            curve.AddKey(1f, 3f);      // Exceptional rewards for mastery
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create event reward curve (bonus focused)
        /// </summary>
        private static AnimationCurve CreateEventRewardCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 1.5f);    // Bonus base rewards
            curve.AddKey(0.5f, 2f);    // High scaling
            curve.AddKey(1f, 3f);      // Maximum event bonuses
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Create speed run reward curve (time-based bonuses)
        /// </summary>
        private static AnimationCurve CreateSpeedRunRewardCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0.5f);    // Low base (speed focus, not collection)
            curve.AddKey(0.5f, 1f);    // Standard middle rewards
            curve.AddKey(1f, 4f);      // Massive bonuses for extreme difficulty
            
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            return curve;
        }
        #endregion
    }
    
    /// <summary>
    /// Generation rules configurations for different play styles
    /// </summary>
    public static class GenerationRulesConfigurations
    {
        /// <summary>
        /// Beginner-friendly generation rules
        /// </summary>
        public static GenerationRules CreateBeginnerRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.name = "Beginner Generation Rules";
            
            // Obstacle rules (generous spacing)
            rules.MinObstacleSpacing = 8f;
            rules.MaxObstacleSpacing = 20f;
            rules.MaxConsecutiveObstacles = 2;
            rules.ObstacleHeightVariation = 1.5f;
            
            // Collectible rules (frequent and clustered)
            rules.CollectibleClusterSize = 4;
            rules.CollectibleSpacing = 1.5f;
            rules.CollectibleHeightRange = 3f;
            
            // Hazard rules (minimal and well-warned)
            rules.HazardSpacing = 40f;
            rules.MaxConsecutiveHazards = 1;
            rules.HazardWarningDistance = 15f;
            
            // Power-up rules (frequent)
            rules.PowerUpSpacing = 25f;
            rules.PowerUpLifetime = 12f;
            
            // Pattern rules (simple)
            rules.PatternLength = 120f;
            rules.MaxPatternComplexity = 3;
            
            return rules;
        }
        
        /// <summary>
        /// Standard generation rules for balanced gameplay
        /// </summary>
        public static GenerationRules CreateStandardRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.name = "Standard Generation Rules";
            
            // Obstacle rules (balanced)
            rules.MinObstacleSpacing = 5f;
            rules.MaxObstacleSpacing = 15f;
            rules.MaxConsecutiveObstacles = 3;
            rules.ObstacleHeightVariation = 2f;
            
            // Collectible rules (moderate)
            rules.CollectibleClusterSize = 3;
            rules.CollectibleSpacing = 2f;
            rules.CollectibleHeightRange = 4f;
            
            // Hazard rules (standard)
            rules.HazardSpacing = 25f;
            rules.MaxConsecutiveHazards = 2;
            rules.HazardWarningDistance = 10f;
            
            // Power-up rules (standard)
            rules.PowerUpSpacing = 35f;
            rules.PowerUpLifetime = 10f;
            
            // Pattern rules (moderate complexity)
            rules.PatternLength = 100f;
            rules.MaxPatternComplexity = 5;
            
            return rules;
        }
        
        /// <summary>
        /// Expert generation rules for challenging gameplay
        /// </summary>
        public static GenerationRules CreateExpertRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.name = "Expert Generation Rules";
            
            // Obstacle rules (tight spacing)
            rules.MinObstacleSpacing = 3f;
            rules.MaxObstacleSpacing = 10f;
            rules.MaxConsecutiveObstacles = 5;
            rules.ObstacleHeightVariation = 3f;
            
            // Collectible rules (sparse and skill-based)
            rules.CollectibleClusterSize = 2;
            rules.CollectibleSpacing = 3f;
            rules.CollectibleHeightRange = 5f;
            
            // Hazard rules (frequent and dangerous)
            rules.HazardSpacing = 15f;
            rules.MaxConsecutiveHazards = 3;
            rules.HazardWarningDistance = 5f;
            
            // Power-up rules (rare)
            rules.PowerUpSpacing = 50f;
            rules.PowerUpLifetime = 8f;
            
            // Pattern rules (high complexity)
            rules.PatternLength = 80f;
            rules.MaxPatternComplexity = 8;
            
            return rules;
        }
        
        /// <summary>
        /// Speed run focused generation rules
        /// </summary>
        public static GenerationRules CreateSpeedRunRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.name = "Speed Run Generation Rules";
            
            // Obstacle rules (very tight for speed)
            rules.MinObstacleSpacing = 2f;
            rules.MaxObstacleSpacing = 8f;
            rules.MaxConsecutiveObstacles = 4;
            rules.ObstacleHeightVariation = 2.5f;
            
            // Collectible rules (minimal)
            rules.CollectibleClusterSize = 1;
            rules.CollectibleSpacing = 4f;
            rules.CollectibleHeightRange = 6f;
            
            // Hazard rules (high stakes)
            rules.HazardSpacing = 12f;
            rules.MaxConsecutiveHazards = 2;
            rules.HazardWarningDistance = 3f;
            
            // Power-up rules (very rare)
            rules.PowerUpSpacing = 80f;
            rules.PowerUpLifetime = 6f;
            
            // Pattern rules (maximum complexity)
            rules.PatternLength = 60f;
            rules.MaxPatternComplexity = 10;
            
            return rules;
        }
        
        /// <summary>
        /// Collect-a-thon generation rules (collectible focused)
        /// </summary>
        public static GenerationRules CreateCollectorRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.name = "Collector Generation Rules";
            
            // Obstacle rules (moderate to allow collection focus)
            rules.MinObstacleSpacing = 6f;
            rules.MaxObstacleSpacing = 18f;
            rules.MaxConsecutiveObstacles = 2;
            rules.ObstacleHeightVariation = 2f;
            
            // Collectible rules (abundant and varied)
            rules.CollectibleClusterSize = 5;
            rules.CollectibleSpacing = 1f;
            rules.CollectibleHeightRange = 6f;
            
            // Hazard rules (minimal to protect collections)
            rules.HazardSpacing = 50f;
            rules.MaxConsecutiveHazards = 1;
            rules.HazardWarningDistance = 20f;
            
            // Power-up rules (frequent magnet/collection bonuses)
            rules.PowerUpSpacing = 20f;
            rules.PowerUpLifetime = 15f;
            
            // Pattern rules (collection-optimized)
            rules.PatternLength = 150f;
            rules.MaxPatternComplexity = 4;
            
            return rules;
        }
    }
    
    /// <summary>
    /// Utility class for managing course configurations
    /// </summary>
    public static class CourseConfigurationManager
    {
        /// <summary>
        /// Get configuration appropriate for player level
        /// </summary>
        public static CourseConfiguration GetConfigurationForPlayer(int playerLevel, int runsCompleted)
        {
            // Beginner players (levels 1-10 or first 20 runs)
            if (playerLevel <= 10 || runsCompleted < 20)
            {
                return CourseConfigurations.CreateBeginnerConfiguration();
            }
            // Expert players (level 25+ with many runs)
            else if (playerLevel >= 25 && runsCompleted > 100)
            {
                return CourseConfigurations.CreateExpertConfiguration();
            }
            // Standard for everyone else
            else
            {
                return CourseConfigurations.CreateStandardConfiguration();
            }
        }
        
        /// <summary>
        /// Get generation rules appropriate for player skill
        /// </summary>
        public static GenerationRules GetRulesForPlayer(int playerLevel, float successRate)
        {
            // High success rate = needs more challenge
            if (successRate > 0.8f && playerLevel > 15)
            {
                return GenerationRulesConfigurations.CreateExpertRules();
            }
            // Low success rate = needs easier rules
            else if (successRate < 0.3f || playerLevel < 5)
            {
                return GenerationRulesConfigurations.CreateBeginnerRules();
            }
            // Standard for moderate players
            else
            {
                return GenerationRulesConfigurations.CreateStandardRules();
            }
        }
        
        /// <summary>
        /// Get special event configuration
        /// </summary>
        public static CourseConfiguration GetEventConfiguration(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "speed_challenge":
                    return CourseConfigurations.CreateSpeedRunConfiguration();
                case "collector_event":
                    // Would create collector-focused config
                    return CourseConfigurations.CreateEventConfiguration();
                default:
                    return CourseConfigurations.CreateEventConfiguration();
            }
        }
        
        /// <summary>
        /// Create a custom configuration by blending existing ones
        /// </summary>
        public static CourseConfiguration CreateBlendedConfiguration(
            CourseConfiguration config1, 
            CourseConfiguration config2, 
            float blendFactor)
        {
            var blended = ScriptableObject.CreateInstance<CourseConfiguration>();
            blended.name = $"Blended Config ({config1.name} + {config2.name})";
            
            // Blend numeric values
            blended.DifficultyRampRate = Mathf.Lerp(config1.DifficultyRampRate, config2.DifficultyRampRate, blendFactor);
            blended.BaseSectionLength = Mathf.Lerp(config1.BaseSectionLength, config2.BaseSectionLength, blendFactor);
            blended.ObstacleDensity = Mathf.Lerp(config1.ObstacleDensity, config2.ObstacleDensity, blendFactor);
            blended.CollectibleDensity = Mathf.Lerp(config1.CollectibleDensity, config2.CollectibleDensity, blendFactor);
            blended.PowerUpDensity = Mathf.Lerp(config1.PowerUpDensity, config2.PowerUpDensity, blendFactor);
            blended.HazardDensity = Mathf.Lerp(config1.HazardDensity, config2.HazardDensity, blendFactor);
            
            // Use config1 for non-blendable properties
            blended.MaxDifficulty = config1.MaxDifficulty;
            blended.ThemeProgression = config1.ThemeProgression;
            blended.DifficultyOverDistance = config1.DifficultyOverDistance;
            
            return blended;
        }
        
        /// <summary>
        /// Validate a course configuration for gameplay balance
        /// </summary>
        public static bool ValidateConfiguration(CourseConfiguration config)
        {
            // Check for reasonable values
            if (config.BaseSectionLength <= 0) return false;
            if (config.ObstacleDensity < 0 || config.ObstacleDensity > 1) return false;
            if (config.CollectibleDensity < 0 || config.CollectibleDensity > 1) return false;
            if (config.DifficultyRampRate < 0) return false;
            
            // Check that obstacle + hazard density isn't overwhelming
            if (config.ObstacleDensity + config.HazardDensity > 0.8f) return false;
            
            // Ensure minimum collectibles for progression
            if (config.CollectibleDensity < 0.1f) return false;
            
            return true;
        }
    }
}