using UnityEngine;
using CircuitRunners.Bot;

/// <summary>
/// Example bot part configurations to demonstrate the modular system.
/// These would normally be created as individual ScriptableObject assets in Unity,
/// but are provided here as code examples for the MVP implementation.
/// 
/// To use these in Unity:
/// 1. Right-click in Project -> Create -> Circuit Runners -> Bot Part
/// 2. Configure the values as shown in these examples
/// 3. Save the asset in Assets/ScriptableObjects/BotParts/
/// </summary>
namespace CircuitRunners.ScriptableObjects.BotParts
{
    /// <summary>
    /// Chassis parts - affect core stats and bot appearance
    /// </summary>
    public static class ChassisParts
    {
        /// <summary>
        /// Basic chassis for new players - balanced stats
        /// </summary>
        public static BotPart CreateBasicChassis()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Basic Chassis";
            part.Description = "Standard bot chassis with balanced performance";
            part.PartType = BotPartType.Chassis;
            part.Rarity = BotPartRarity.Common;
            
            // No modifiers for basic chassis
            part.SpeedModifier = 0f;
            part.JumpModifier = 0f;
            part.HealthModifier = 0;
            part.DamageReduction = 0;
            
            part.RequiredLevel = 1;
            part.UnlockCost = 0; // Free starting part
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Speed-focused lightweight chassis
        /// </summary>
        public static BotPart CreateSpeedChassis()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Velocity Frame";
            part.Description = "Lightweight chassis designed for maximum speed";
            part.PartType = BotPartType.Chassis;
            part.Rarity = BotPartRarity.Uncommon;
            
            // Speed bonuses with health trade-off
            part.SpeedModifier = 2f;
            part.JumpModifier = 0.5f;
            part.HealthModifier = -20;
            part.DamageReduction = 0;
            
            // AI behavior modifiers
            part.RiskToleranceModifier = 0.2f; // More aggressive
            
            part.RequiredLevel = 5;
            part.UnlockCost = 500;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Tank-focused armored chassis
        /// </summary>
        public static BotPart CreateArmoredChassis()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Fortress Frame";
            part.Description = "Heavy-duty chassis with maximum protection";
            part.PartType = BotPartType.Chassis;
            part.Rarity = BotPartRarity.Uncommon;
            
            // Defense bonuses with speed trade-off
            part.SpeedModifier = -1f;
            part.JumpModifier = -0.5f;
            part.HealthModifier = 50;
            part.DamageReduction = 5;
            
            // AI behavior modifiers
            part.RiskToleranceModifier = -0.2f; // More cautious
            
            part.RequiredLevel = 3;
            part.UnlockCost = 300;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Premium high-performance chassis
        /// </summary>
        public static BotPart CreateEliteChassis()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Elite Composite Frame";
            part.Description = "Advanced chassis with superior performance across all metrics";
            part.PartType = BotPartType.Chassis;
            part.Rarity = BotPartRarity.Legendary;
            
            // Balanced high-tier bonuses
            part.SpeedModifier = 1.5f;
            part.JumpModifier = 1.5f;
            part.HealthModifier = 25;
            part.DamageReduction = 3;
            
            part.RequiredLevel = 20;
            part.UnlockCost = 0; // Premium only
            part.IsPremiumPart = true;
            part.PremiumCurrencyCost = 100;
            
            return part;
        }
    }
    
    /// <summary>
    /// Processor parts - affect AI behavior and reaction time
    /// </summary>
    public static class ProcessorParts
    {
        /// <summary>
        /// Basic processor for standard AI performance
        /// </summary>
        public static BotPart CreateBasicProcessor()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Standard CPU";
            part.Description = "Reliable processor for consistent bot behavior";
            part.PartType = BotPartType.Processor;
            part.Rarity = BotPartRarity.Common;
            
            // No modifiers for basic processor
            part.RequiredLevel = 1;
            part.UnlockCost = 0;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// High-speed processor for faster reactions
        /// </summary>
        public static BotPart CreateQuantumProcessor()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Quantum Processing Unit";
            part.Description = "Advanced processor enabling faster decision-making";
            part.PartType = BotPartType.Processor;
            part.Rarity = BotPartRarity.Rare;
            
            // Improves reaction time and risk assessment
            part.RiskToleranceModifier = 0.1f;
            part.CollectiblePriorityModifier = 0.1f;
            
            part.RequiredLevel = 8;
            part.UnlockCost = 800;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// AI-focused processor for better collectible detection
        /// </summary>
        public static BotPart CreateCollectorProcessor()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Harvest AI Core";
            part.Description = "Specialized processor optimized for resource collection";
            part.PartType = BotPartType.Processor;
            part.Rarity = BotPartRarity.Uncommon;
            
            // Significantly improves collectible prioritization
            part.CollectiblePriorityModifier = 0.4f;
            part.RiskToleranceModifier = -0.1f; // More cautious to preserve resources
            
            part.RequiredLevel = 6;
            part.UnlockCost = 600;
            part.IsPremiumPart = false;
            
            return part;
        }
    }
    
    /// <summary>
    /// Sensor parts - affect detection range and accuracy
    /// </summary>
    public static class SensorParts
    {
        /// <summary>
        /// Basic sensors for standard detection
        /// </summary>
        public static BotPart CreateBasicSensors()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Standard Sensors";
            part.Description = "Basic sensor array for obstacle and collectible detection";
            part.PartType = BotPartType.Sensors;
            part.Rarity = BotPartRarity.Common;
            
            part.RequiredLevel = 1;
            part.UnlockCost = 0;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Advanced sensors with longer range
        /// </summary>
        public static BotPart CreateLongRangeSensors()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Long-Range Radar Array";
            part.Description = "Extended detection range for better planning";
            part.PartType = BotPartType.Sensors;
            part.Rarity = BotPartRarity.Uncommon;
            
            // Would affect bot's decision distance (handled in BotController)
            part.RiskToleranceModifier = -0.1f; // Better planning = more cautious
            
            part.RequiredLevel = 4;
            part.UnlockCost = 400;
            part.IsPremiumPart = false;
            
            return part;
        }
    }
    
    /// <summary>
    /// Mobility parts - affect movement capabilities
    /// </summary>
    public static class MobilityParts
    {
        /// <summary>
        /// Basic mobility system
        /// </summary>
        public static BotPart CreateBasicMobility()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Standard Actuators";
            part.Description = "Basic movement systems for reliable locomotion";
            part.PartType = BotPartType.Mobility;
            part.Rarity = BotPartRarity.Common;
            
            part.RequiredLevel = 1;
            part.UnlockCost = 0;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Jump-enhanced mobility system
        /// </summary>
        public static BotPart CreateJumpBoosters()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Hydraulic Jump Boosters";
            part.Description = "Enhanced jumping capability for vertical obstacles";
            part.PartType = BotPartType.Mobility;
            part.Rarity = BotPartRarity.Uncommon;
            
            part.JumpModifier = 3f;
            part.SpeedModifier = -0.5f; // Weight trade-off
            
            part.RequiredLevel = 7;
            part.UnlockCost = 700;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Speed-focused mobility system
        /// </summary>
        public static BotPart CreateTurboThrusters()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Turbo Thrusters";
            part.Description = "High-speed propulsion for maximum velocity";
            part.PartType = BotPartType.Mobility;
            part.Rarity = BotPartRarity.Rare;
            
            part.SpeedModifier = 2.5f;
            part.DashModifier = 2f;
            part.DashCooldownReduction = 0.5f;
            
            part.RequiredLevel = 10;
            part.UnlockCost = 1000;
            part.IsPremiumPart = false;
            
            return part;
        }
    }
    
    /// <summary>
    /// Power core parts - affect ability cooldowns and energy
    /// </summary>
    public static class PowerCoreParts
    {
        /// <summary>
        /// Basic power core
        /// </summary>
        public static BotPart CreateBasicPowerCore()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Standard Power Cell";
            part.Description = "Reliable energy source for basic operations";
            part.PartType = BotPartType.PowerCore;
            part.Rarity = BotPartRarity.Common;
            
            part.RequiredLevel = 1;
            part.UnlockCost = 0;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// High-capacity power core
        /// </summary>
        public static BotPart CreateHighCapacityCore()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Arc Reactor Core";
            part.Description = "High-capacity power source for extended abilities";
            part.PartType = BotPartType.PowerCore;
            part.Rarity = BotPartRarity.Rare;
            
            part.DashCooldownReduction = 1f;
            part.SpeedBoostMultiplier = 1.3f;
            part.SpeedBoostDuration = 3f;
            
            part.RequiredLevel = 12;
            part.UnlockCost = 1200;
            part.IsPremiumPart = false;
            
            return part;
        }
    }
    
    /// <summary>
    /// Special parts - unique abilities and effects
    /// </summary>
    public static class SpecialParts
    {
        /// <summary>
        /// Magnet system for attracting collectibles
        /// </summary>
        public static BotPart CreateMagnetSystem()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Electromagnetic Attractor";
            part.Description = "Attracts nearby collectibles automatically";
            part.PartType = BotPartType.Special;
            part.Rarity = BotPartRarity.Epic;
            
            part.CollectiblePriorityModifier = 0.5f;
            // Would also have magnetic attraction effect (handled in game logic)
            
            part.RequiredLevel = 15;
            part.UnlockCost = 1500;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Shield generator for damage protection
        /// </summary>
        public static BotPart CreateShieldGenerator()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Energy Shield Generator";
            part.Description = "Provides temporary invulnerability after taking damage";
            part.PartType = BotPartType.Special;
            part.Rarity = BotPartRarity.Epic;
            
            part.DamageReduction = 3;
            part.HealthModifier = 10;
            // Would also have shield regeneration effect
            
            part.RequiredLevel = 18;
            part.UnlockCost = 1800;
            part.IsPremiumPart = false;
            
            return part;
        }
        
        /// <summary>
        /// Premium luck modifier part
        /// </summary>
        public static BotPart CreateLuckAmplifier()
        {
            var part = ScriptableObject.CreateInstance<BotPart>();
            part.PartName = "Quantum Luck Field";
            part.Description = "Increases chance of rare collectibles and better rewards";
            part.PartType = BotPartType.Special;
            part.Rarity = BotPartRarity.Legendary;
            
            part.CollectiblePriorityModifier = 0.3f;
            // Would also increase rare item drop rates
            
            part.RequiredLevel = 25;
            part.UnlockCost = 0; // Premium only
            part.IsPremiumPart = true;
            part.PremiumCurrencyCost = 150;
            
            return part;
        }
    }
    
    /// <summary>
    /// Utility class for creating part databases and testing
    /// </summary>
    public static class BotPartDatabase
    {
        /// <summary>
        /// Create all example parts for testing and development
        /// </summary>
        public static BotPart[] CreateAllExampleParts()
        {
            return new BotPart[]
            {
                // Chassis parts
                ChassisParts.CreateBasicChassis(),
                ChassisParts.CreateSpeedChassis(),
                ChassisParts.CreateArmoredChassis(),
                ChassisParts.CreateEliteChassis(),
                
                // Processor parts
                ProcessorParts.CreateBasicProcessor(),
                ProcessorParts.CreateQuantumProcessor(),
                ProcessorParts.CreateCollectorProcessor(),
                
                // Sensor parts
                SensorParts.CreateBasicSensors(),
                SensorParts.CreateLongRangeSensors(),
                
                // Mobility parts
                MobilityParts.CreateBasicMobility(),
                MobilityParts.CreateJumpBoosters(),
                MobilityParts.CreateTurboThrusters(),
                
                // Power core parts
                PowerCoreParts.CreateBasicPowerCore(),
                PowerCoreParts.CreateHighCapacityCore(),
                
                // Special parts
                SpecialParts.CreateMagnetSystem(),
                SpecialParts.CreateShieldGenerator(),
                SpecialParts.CreateLuckAmplifier()
            };
        }
        
        /// <summary>
        /// Create starter parts for new players
        /// </summary>
        public static BotPart[] CreateStarterParts()
        {
            return new BotPart[]
            {
                ChassisParts.CreateBasicChassis(),
                ProcessorParts.CreateBasicProcessor(),
                SensorParts.CreateBasicSensors(),
                MobilityParts.CreateBasicMobility(),
                PowerCoreParts.CreateBasicPowerCore()
            };
        }
        
        /// <summary>
        /// Create parts for specific archetype focus
        /// </summary>
        public static BotPart[] CreateArchetypeParts(BotArchetype archetype)
        {
            switch (archetype)
            {
                case BotArchetype.Speed:
                    return new BotPart[]
                    {
                        ChassisParts.CreateSpeedChassis(),
                        MobilityParts.CreateTurboThrusters(),
                        PowerCoreParts.CreateHighCapacityCore()
                    };
                    
                case BotArchetype.Tank:
                    return new BotPart[]
                    {
                        ChassisParts.CreateArmoredChassis(),
                        SpecialParts.CreateShieldGenerator()
                    };
                    
                case BotArchetype.Jumper:
                    return new BotPart[]
                    {
                        MobilityParts.CreateJumpBoosters()
                    };
                    
                case BotArchetype.Collector:
                    return new BotPart[]
                    {
                        ProcessorParts.CreateCollectorProcessor(),
                        SpecialParts.CreateMagnetSystem()
                    };
                    
                case BotArchetype.Lucky:
                    return new BotPart[]
                    {
                        SpecialParts.CreateLuckAmplifier()
                    };
                    
                case BotArchetype.Hacker:
                    return new BotPart[]
                    {
                        ProcessorParts.CreateQuantumProcessor(),
                        SensorParts.CreateLongRangeSensors()
                    };
                    
                default:
                    return CreateStarterParts();
            }
        }
        
        /// <summary>
        /// Get parts by rarity for progression gating
        /// </summary>
        public static BotPart[] GetPartsByRarity(BotPartRarity rarity)
        {
            var allParts = CreateAllExampleParts();
            return System.Array.FindAll(allParts, part => part.Rarity == rarity);
        }
        
        /// <summary>
        /// Get parts by type for UI organization
        /// </summary>
        public static BotPart[] GetPartsByType(BotPartType partType)
        {
            var allParts = CreateAllExampleParts();
            return System.Array.FindAll(allParts, part => part.PartType == partType);
        }
        
        /// <summary>
        /// Get parts available at specific level
        /// </summary>
        public static BotPart[] GetPartsForLevel(int playerLevel)
        {
            var allParts = CreateAllExampleParts();
            return System.Array.FindAll(allParts, part => part.RequiredLevel <= playerLevel);
        }
    }
}