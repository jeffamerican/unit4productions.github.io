using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRunners.Course
{
    /// <summary>
    /// Course themes that affect visual style and atmosphere
    /// Each theme has distinct visual elements and may affect gameplay
    /// </summary>
    public enum CourseTheme
    {
        Industrial,     // Metal, steam, gears - starting theme
        Neon,          // Cyberpunk, bright colors, electronic elements
        Wasteland,     // Post-apocalyptic, rusty, dusty environment
        Digital,       // Matrix-like, green code, virtual reality
        Arctic,        // Ice, snow, cold blue tones
        Volcanic,      // Lava, fire, red/orange color scheme
        Space,         // Stars, nebulae, zero gravity effects
        Underwater     // Ocean, bubbles, aquatic life
    }

    /// <summary>
    /// Difficulty levels that affect obstacle density and complexity
    /// </summary>
    public enum CourseDifficulty
    {
        Easy,       // Few obstacles, generous spacing, simple patterns
        Medium,     // Moderate obstacles, balanced spacing
        Hard,       // More obstacles, tighter spacing, complex patterns
        Extreme     // Maximum obstacles, minimal spacing, expert patterns
    }

    /// <summary>
    /// Course generation patterns that define section characteristics
    /// </summary>
    public enum CoursePattern
    {
        Standard,       // Balanced mix of obstacles and collectibles
        ObstacleHeavy,  // High density of obstacles, fewer collectibles
        CollectibleRich, // Many collectibles, fewer obstacles
        Mixed,          // Random mix of different elements
        Gauntlet,       // Continuous challenges, high difficulty
        Breather        // Low difficulty section for recovery
    }

    /// <summary>
    /// Types of course elements that can be generated
    /// </summary>
    public enum CourseElementType
    {
        Empty,          // No element at this position
        Obstacle,       // Standard obstacle requiring player action
        Collectible,    // Items that provide rewards
        PowerUp,        // Temporary ability enhancements
        Hazard,         // Dangerous elements that destroy the bot
        Environment,    // Visual elements for atmosphere
        Checkpoint      // Save/progress points (for longer courses)
    }

    /// <summary>
    /// Configuration data for course generation parameters
    /// </summary>
    [CreateAssetMenu(fileName = "Course Configuration", menuName = "Circuit Runners/Course Configuration")]
    public class CourseConfiguration : ScriptableObject
    {
        [Header("Difficulty Settings")]
        public CourseDifficulty MaxDifficulty = CourseDifficulty.Extreme;
        public float DifficultyRampRate = 0.01f; // How quickly difficulty increases
        public float DifficultyDecayRate = 0.005f; // How difficulty decreases after failure
        
        [Header("Theme Progression")]
        public CourseTheme[] ThemeProgression = { CourseTheme.Industrial, CourseTheme.Neon, CourseTheme.Wasteland };
        public int[] ThemeUnlockLevels = { 1, 5, 10 };
        
        [Header("Section Configuration")]
        public float BaseSectionLength = 50f;
        public int SectionVariationCount = 5;
        public float MinSectionSpacing = 5f;
        
        [Header("Element Density")]
        [Range(0f, 1f)] public float ObstacleDensity = 0.4f;
        [Range(0f, 1f)] public float CollectibleDensity = 0.3f;
        [Range(0f, 1f)] public float PowerUpDensity = 0.1f;
        [Range(0f, 1f)] public float HazardDensity = 0.2f;
        
        [Header("Balancing")]
        public AnimationCurve DifficultyOverDistance;
        public AnimationCurve RewardOverDifficulty;
        
        /// <summary>
        /// Get theme unlock level
        /// </summary>
        public int GetThemeUnlockLevel(CourseTheme theme)
        {
            for (int i = 0; i < ThemeProgression.Length; i++)
            {
                if (ThemeProgression[i] == theme)
                {
                    return i < ThemeUnlockLevels.Length ? ThemeUnlockLevels[i] : 1;
                }
            }
            return 999; // Theme not found, very high unlock level
        }

        /// <summary>
        /// Check if theme is unlocked for player level
        /// </summary>
        public bool IsThemeUnlocked(CourseTheme theme, int playerLevel)
        {
            return playerLevel >= GetThemeUnlockLevel(theme);
        }
    }

    /// <summary>
    /// Generation rules for course element placement and spacing
    /// </summary>
    [CreateAssetMenu(fileName = "Generation Rules", menuName = "Circuit Runners/Generation Rules")]
    public class GenerationRules : ScriptableObject
    {
        [Header("Obstacle Rules")]
        public float MinObstacleSpacing = 5f;
        public float MaxObstacleSpacing = 15f;
        public int MaxConsecutiveObstacles = 3;
        public float ObstacleHeightVariation = 2f;
        
        [Header("Collectible Rules")]
        public int CollectibleClusterSize = 3;
        public float CollectibleSpacing = 2f;
        public float CollectibleHeightRange = 4f;
        
        [Header("Hazard Rules")]
        public float HazardSpacing = 20f;
        public int MaxConsecutiveHazards = 2;
        public float HazardWarningDistance = 10f;
        
        [Header("Power-up Rules")]
        public float PowerUpSpacing = 30f;
        public float PowerUpLifetime = 10f;
        
        [Header("Pattern Rules")]
        public float PatternLength = 100f;
        public int MaxPatternComplexity = 5;
        
        /// <summary>
        /// Validate that the rules create playable courses
        /// </summary>
        public bool ValidateRules()
        {
            if (MinObstacleSpacing <= 0) return false;
            if (MaxObstacleSpacing < MinObstacleSpacing) return false;
            if (CollectibleClusterSize <= 0) return false;
            if (HazardSpacing < MinObstacleSpacing * 2) return false;
            
            return true;
        }
    }

    /// <summary>
    /// Represents a generated course section with all its elements
    /// </summary>
    [System.Serializable]
    public class CourseSection
    {
        [Header("Section Identity")]
        public int SectionIndex;
        public float StartPosition;
        public float EndPosition;
        
        [Header("Section Properties")]
        public CourseTheme Theme;
        public CourseDifficulty Difficulty;
        public CoursePattern Pattern;
        
        [Header("Section Content")]
        public List<CourseElement> Elements = new List<CourseElement>();
        
        [Header("Section Metadata")]
        public float GenerationTime;
        public int EstimatedScore;
        public bool HasCheckpoint;
        
        /// <summary>
        /// Get section length
        /// </summary>
        public float Length => EndPosition - StartPosition;
        
        /// <summary>
        /// Get elements of a specific type
        /// </summary>
        public List<CourseElement> GetElementsOfType(CourseElementType elementType)
        {
            return Elements.FindAll(e => e.ElementType == elementType);
        }
        
        /// <summary>
        /// Count elements by type
        /// </summary>
        public int CountElementsOfType(CourseElementType elementType)
        {
            return Elements.Count(e => e.ElementType == elementType);
        }
        
        /// <summary>
        /// Calculate section difficulty score
        /// </summary>
        public float CalculateDifficultyScore()
        {
            float score = 0f;
            
            foreach (var element in Elements)
            {
                switch (element.ElementType)
                {
                    case CourseElementType.Obstacle:
                        score += 2f;
                        break;
                    case CourseElementType.Hazard:
                        score += 5f;
                        break;
                    case CourseElementType.Collectible:
                        score -= 0.5f; // Collectibles make it slightly easier
                        break;
                    case CourseElementType.PowerUp:
                        score -= 1f;
                        break;
                }
            }
            
            return score / Length; // Normalize by section length
        }
        
        /// <summary>
        /// Get section summary for debugging
        /// </summary>
        public string GetSummary()
        {
            return $"Section {SectionIndex}: {Theme} theme, {Difficulty} difficulty, " +
                   $"{Elements.Count} elements ({CountElementsOfType(CourseElementType.Obstacle)} obstacles, " +
                   $"{CountElementsOfType(CourseElementType.Collectible)} collectibles)";
        }
    }

    /// <summary>
    /// Individual course element with position and properties
    /// </summary>
    [System.Serializable]
    public class CourseElement
    {
        [Header("Element Identity")]
        public CourseElementType ElementType;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        
        [Header("Element Properties")]
        public CourseTheme Theme;
        public CourseDifficulty Difficulty;
        
        [Header("Type-Specific Data")]
        public string ObstacleType;
        public Bot.AIDecisionType RequiredAction;
        public string CollectibleType;
        public int Value;
        public string PowerUpType;
        public string HazardType;
        
        [Header("Visual Properties")]
        public Color ElementColor = Color.white;
        public Vector3 Scale = Vector3.one;
        public float AnimationSpeed = 1f;
        
        [Header("Gameplay Properties")]
        public bool IsActive = true;
        public float LifeTime = 0f; // 0 = permanent
        public bool RequiresPlayerAction = false;
        
        /// <summary>
        /// Check if element is still valid/active
        /// </summary>
        public bool IsValid()
        {
            if (!IsActive) return false;
            if (LifeTime > 0f && Time.time > LifeTime) return false;
            
            return true;
        }
        
        /// <summary>
        /// Get element difficulty modifier
        /// </summary>
        public float GetDifficultyModifier()
        {
            float modifier = 1f + ((int)Difficulty * 0.25f);
            
            // Adjust based on element type
            switch (ElementType)
            {
                case CourseElementType.Obstacle:
                    modifier *= 1.2f;
                    break;
                case CourseElementType.Hazard:
                    modifier *= 2f;
                    break;
                case CourseElementType.Collectible:
                    modifier *= 0.8f;
                    break;
            }
            
            return modifier;
        }
        
        /// <summary>
        /// Get reward value for this element
        /// </summary>
        public int GetRewardValue()
        {
            if (ElementType != CourseElementType.Collectible) return 0;
            
            int baseValue = Value;
            float difficultyBonus = GetDifficultyModifier();
            
            return Mathf.RoundToInt(baseValue * difficultyBonus);
        }
    }

    /// <summary>
    /// Component for obstacle game objects
    /// </summary>
    public class ObstacleData : MonoBehaviour
    {
        [Header("Obstacle Properties")]
        public string ObstacleType = "Basic Obstacle";
        public Bot.AIDecisionType RequiredAction = Bot.AIDecisionType.Jump;
        public int BaseDamage = 10;
        
        [Header("Visual Feedback")]
        public Color WarningColor = Color.red;
        public float WarningFlashSpeed = 2f;
        public bool ShowWarningEffect = true;
        
        [Header("Animation")]
        public bool IsAnimated = false;
        public float AnimationSpeed = 1f;
        public Vector3 MovementPattern = Vector3.zero;
        
        private Renderer _renderer;
        private Color _originalColor;
        private Vector3 _originalPosition;
        private float _animationTime = 0f;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
            
            _originalPosition = transform.position;
        }

        private void Update()
        {
            if (IsAnimated)
            {
                UpdateAnimation();
            }
            
            if (ShowWarningEffect)
            {
                UpdateWarningEffect();
            }
        }

        private void UpdateAnimation()
        {
            _animationTime += Time.deltaTime * AnimationSpeed;
            
            // Apply movement pattern
            Vector3 offset = new Vector3(
                Mathf.Sin(_animationTime) * MovementPattern.x,
                Mathf.Sin(_animationTime * 1.5f) * MovementPattern.y,
                Mathf.Sin(_animationTime * 0.8f) * MovementPattern.z
            );
            
            transform.position = _originalPosition + offset;
        }

        private void UpdateWarningEffect()
        {
            if (_renderer == null) return;
            
            float flash = Mathf.Sin(Time.time * WarningFlashSpeed);
            Color currentColor = Color.Lerp(_originalColor, WarningColor, flash * 0.5f + 0.5f);
            _renderer.material.color = currentColor;
        }

        /// <summary>
        /// Get obstacle threat level for AI decision making
        /// </summary>
        public float GetThreatLevel()
        {
            return BaseDamage / 10f; // Normalize to 0-1 range typically
        }
    }

    /// <summary>
    /// Component for collectible game objects
    /// </summary>
    public class CollectibleData : MonoBehaviour
    {
        [Header("Collectible Properties")]
        public string CollectibleType = "Scrap";
        public int ScrapValue = 1;
        public int XPValue = 0;
        
        [Header("Visual Effects")]
        public bool HasGlowEffect = true;
        public Color GlowColor = Color.yellow;
        public float GlowIntensity = 1f;
        public bool RotatesContinuously = true;
        public float RotationSpeed = 90f;
        
        [Header("Collection Effects")]
        public GameObject CollectionParticle;
        public AudioClip CollectionSound;
        public float CollectionRange = 1f;
        
        private Renderer _renderer;
        private AudioSource _audioSource;
        private bool _isCollected = false;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _audioSource = GetComponent<AudioSource>();
            
            if (HasGlowEffect && _renderer != null)
            {
                ApplyGlowEffect();
            }
        }

        private void Update()
        {
            if (_isCollected) return;
            
            if (RotatesContinuously)
            {
                transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
            }
            
            if (HasGlowEffect)
            {
                UpdateGlowEffect();
            }
        }

        private void ApplyGlowEffect()
        {
            if (_renderer?.material != null)
            {
                _renderer.material.SetColor("_EmissionColor", GlowColor * GlowIntensity);
            }
        }

        private void UpdateGlowEffect()
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 0.7f;
            Color currentGlow = GlowColor * (GlowIntensity * pulse);
            
            if (_renderer?.material != null)
            {
                _renderer.material.SetColor("_EmissionColor", currentGlow);
            }
        }

        /// <summary>
        /// Collect this item
        /// </summary>
        public void Collect()
        {
            if (_isCollected) return;
            
            _isCollected = true;
            
            // Play collection effects
            if (CollectionParticle != null)
            {
                Instantiate(CollectionParticle, transform.position, Quaternion.identity);
            }
            
            if (CollectionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(CollectionSound);
            }
            
            // Hide visual (but keep for audio)
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
            
            // Destroy after audio finishes
            float destroyDelay = CollectionSound != null ? CollectionSound.length : 0.1f;
            Destroy(gameObject, destroyDelay);
        }

        /// <summary>
        /// Get total reward value including bonuses
        /// </summary>
        public int GetTotalValue()
        {
            return ScrapValue; // Could include bonuses, multipliers, etc.
        }
    }

    /// <summary>
    /// Component for power-up game objects
    /// </summary>
    public class PowerUpData : MonoBehaviour
    {
        [Header("Power-up Properties")]
        public string PowerUpType = "Speed Boost";
        public float Duration = 5f;
        public float EffectStrength = 1.5f;
        
        [Header("Visual Effects")]
        public Color PowerUpColor = Color.magenta;
        public bool HasEnergyEffect = true;
        public GameObject ActivationParticle;
        
        private void Start()
        {
            // Setup visual effects
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = PowerUpColor;
            }
        }

        /// <summary>
        /// Activate power-up effect
        /// </summary>
        public void Activate()
        {
            if (ActivationParticle != null)
            {
                Instantiate(ActivationParticle, transform.position, Quaternion.identity);
            }
            
            // The actual power-up effect would be applied by the bot controller
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Component for hazard game objects
    /// </summary>
    public class HazardData : MonoBehaviour
    {
        [Header("Hazard Properties")]
        public string HazardType = "Spike Pit";
        public bool IsInstantKill = true;
        public int Damage = 999;
        
        [Header("Warning System")]
        public float WarningDistance = 5f;
        public GameObject WarningEffect;
        public Color DangerColor = Color.red;
        
        private bool _warningActive = false;

        private void Update()
        {
            CheckForWarning();
        }

        private void CheckForWarning()
        {
            // Check if bot is approaching
            var bot = Core.GameManager.Instance?.ActiveBot;
            if (bot == null) return;
            
            float distance = Vector3.Distance(transform.position, bot.transform.position);
            
            if (distance <= WarningDistance && !_warningActive)
            {
                ActivateWarning();
            }
            else if (distance > WarningDistance && _warningActive)
            {
                DeactivateWarning();
            }
        }

        private void ActivateWarning()
        {
            _warningActive = true;
            
            if (WarningEffect != null)
            {
                WarningEffect.SetActive(true);
            }
        }

        private void DeactivateWarning()
        {
            _warningActive = false;
            
            if (WarningEffect != null)
            {
                WarningEffect.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Environment prop component for visual elements
    /// </summary>
    public class EnvironmentProp : MonoBehaviour
    {
        [Header("Environment Properties")]
        public CourseTheme Theme;
        public bool IsBackground = true;
        public float ParallaxMultiplier = 0.5f;
        
        [Header("Animation")]
        public bool HasIdleAnimation = false;
        public float IdleAnimationSpeed = 1f;
        
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (HasIdleAnimation)
            {
                // Simple bobbing animation
                float bob = Mathf.Sin(Time.time * IdleAnimationSpeed) * 0.1f;
                transform.position = _startPosition + Vector3.up * bob;
            }
        }

        /// <summary>
        /// Update parallax position based on camera movement
        /// </summary>
        public void UpdateParallax(Vector3 cameraMovement)
        {
            if (IsBackground)
            {
                transform.position += cameraMovement * ParallaxMultiplier;
            }
        }
    }

    /// <summary>
    /// Statistics about generated course
    /// </summary>
    [System.Serializable]
    public class CourseStatistics
    {
        public float TotalDistance;
        public CourseDifficulty CurrentDifficulty;
        public CourseTheme CurrentTheme;
        public int SectionsGenerated;
        public int TotalObstacles;
        public int TotalCollectibles;
        public int TotalPowerUps;
        public int TotalHazards;
        public int ActiveObjects;
        public bool PerformanceModeActive;
        
        /// <summary>
        /// Get statistics summary
        /// </summary>
        public string GetSummary()
        {
            return $"Distance: {TotalDistance:F0}m, Difficulty: {CurrentDifficulty}, " +
                   $"Sections: {SectionsGenerated}, Objects: {ActiveObjects}";
        }
    }

    /// <summary>
    /// Generic object pool class for course elements
    /// </summary>
    [System.Serializable]
    public class ObjectPool<T> where T : Component
    {
        [SerializeField] private T _prefab;
        [SerializeField] private int _initialSize = 10;
        [SerializeField] private int _maxSize = 50;
        
        private Queue<T> _pool = new Queue<T>();
        private Transform _poolParent;

        public void Initialize(Transform parent)
        {
            _poolParent = parent;
            
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        public T Get()
        {
            if (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            
            return CreateNewObject();
        }

        public void Return(T obj)
        {
            if (obj == null) return;
            
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolParent);
            
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
            }
            else
            {
                // Destroy excess objects
                UnityEngine.Object.DestroyImmediate(obj.gameObject);
            }
        }

        private T CreateNewObject()
        {
            if (_prefab == null) return null;
            
            var obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
            obj.gameObject.SetActive(false);
            return obj;
        }
    }
}