using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CircuitRunners.Course
{
    /// <summary>
    /// Procedural course generator that creates endless running tracks with obstacles,
    /// collectibles, and environmental elements. Uses configurable generation rules
    /// to create varied and challenging courses that scale with player progression.
    /// 
    /// Key Features:
    /// - Infinite scrolling course generation
    /// - Difficulty scaling based on distance and player level
    /// - Balanced obstacle and collectible placement
    /// - Environmental storytelling through visual themes
    /// - Performance optimization with object pooling
    /// </summary>
    public class CourseGenerator : MonoBehaviour
    {
        #region Generation Configuration
        [Header("Course Settings")]
        [SerializeField] private CourseConfiguration _courseConfig;
        [SerializeField] private float _sectionLength = 50f;
        [SerializeField] private int _sectionsAhead = 3;
        [SerializeField] private int _sectionsBehind = 1;
        [SerializeField] private float _difficultyScale = 1f;
        
        /// <summary>
        /// Current course configuration settings
        /// </summary>
        public CourseConfiguration CourseConfig => _courseConfig;
        
        /// <summary>
        /// Length of each course section in world units
        /// </summary>
        public float SectionLength => _sectionLength;
        #endregion

        #region Course State
        [Header("Current Course State")]
        [SerializeField] private int _currentSectionIndex = 0;
        [SerializeField] private float _currentDistance = 0f;
        [SerializeField] private CourseTheme _currentTheme = CourseTheme.Industrial;
        [SerializeField] private CourseDifficulty _currentDifficulty = CourseDifficulty.Easy;
        
        // Active course sections
        private List<CourseSection> _activeSections = new List<CourseSection>();
        private Queue<CourseSection> _upcomingSections = new Queue<CourseSection>();
        
        /// <summary>
        /// Current distance traveled in the course
        /// </summary>
        public float CurrentDistance => _currentDistance;
        
        /// <summary>
        /// Current difficulty level
        /// </summary>
        public CourseDifficulty CurrentDifficulty => _currentDifficulty;
        #endregion

        #region Object Pooling
        [Header("Object Pools")]
        [SerializeField] private ObjectPool<ObstacleData> _obstaclePool;
        [SerializeField] private ObjectPool<CollectibleData> _collectiblePool;
        [SerializeField] private ObjectPool<EnvironmentProp> _environmentPool;
        [SerializeField] private ObjectPool<PowerUpData> _powerUpPool;
        
        // Pool containers
        private Dictionary<string, Queue<GameObject>> _pooledObjects = new Dictionary<string, Queue<GameObject>>();
        private Transform _poolContainer;
        
        /// <summary>
        /// Maximum pooled objects per type to prevent memory issues
        /// </summary>
        private const int MAX_POOL_SIZE = 100;
        #endregion

        #region Generation Rules
        [Header("Generation Rules")]
        [SerializeField] private GenerationRules _generationRules;
        [SerializeField] private float _obstacleSpacing = 8f;
        [SerializeField] private float _collectibleDensity = 0.3f;
        [SerializeField] private float _powerUpFrequency = 0.1f;
        
        // Generation state tracking
        private float _lastObstaclePosition = 0f;
        private float _lastCollectibleCluster = 0f;
        private int _consecutiveHazards = 0;
        private CoursePattern _currentPattern = CoursePattern.Standard;
        #endregion

        #region Performance Optimization
        [Header("Performance")]
        [SerializeField] private bool _enableLOD = true;
        [SerializeField] private float _lodDistance = 100f;
        [SerializeField] private int _maxActiveObjects = 200;
        [SerializeField] private bool _useObjectCulling = true;
        
        private bool _performanceModeActive = false;
        private int _currentActiveObjects = 0;
        #endregion

        #region Events
        /// <summary>
        /// Fired when a new course section is generated
        /// </summary>
        public event Action<CourseSection> OnSectionGenerated;
        
        /// <summary>
        /// Fired when course difficulty changes
        /// </summary>
        public event Action<CourseDifficulty> OnDifficultyChanged;
        
        /// <summary>
        /// Fired when course theme changes
        /// </summary>
        public event Action<CourseTheme> OnThemeChanged;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Create pool container
            _poolContainer = new GameObject("Course Object Pools").transform;
            _poolContainer.SetParent(transform);
            
            // Initialize object pools
            InitializeObjectPools();
            
            // Load default configuration if none assigned
            if (_courseConfig == null)
            {
                _courseConfig = CreateDefaultConfiguration();
            }
        }

        private void Start()
        {
            // Initialize generation rules
            if (_generationRules == null)
            {
                _generationRules = CreateDefaultGenerationRules();
            }
            
            // Generate initial course sections
            GenerateInitialCourse();
            
            Debug.Log("[CourseGenerator] Course generator initialized");
        }

        private float _lastGenerationUpdate = 0f;
        private float _lastCullingUpdate = 0f;
        private const float GENERATION_UPDATE_INTERVAL = 0.1f; // 10 times per second
        private const float CULLING_UPDATE_INTERVAL = 0.2f; // 5 times per second
        
        private void Update()
        {
            float currentTime = Time.unscaledTime;
            
            // Optimized course generation updates (reduced frequency)
            if (currentTime - _lastGenerationUpdate >= GENERATION_UPDATE_INTERVAL)
            {
                _lastGenerationUpdate = currentTime;
                UpdateCourseGeneration();
            }
            
            // Optimized object culling updates (even less frequent)
            if (_useObjectCulling && currentTime - _lastCullingUpdate >= CULLING_UPDATE_INTERVAL)
            {
                _lastCullingUpdate = currentTime;
                UpdateObjectCulling();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize object pools for all course elements
        /// </summary>
        private void InitializeObjectPools()
        {
            // Initialize pools for different object types
            InitializePool("Obstacles", 50);
            InitializePool("Collectibles", 100);
            InitializePool("Environment", 30);
            InitializePool("PowerUps", 20);
            InitializePool("Hazards", 25);
            
            Debug.Log("[CourseGenerator] Object pools initialized");
        }

        /// <summary>
        /// Initialize a specific object pool with proper resource loading and fallbacks
        /// </summary>
        private void InitializePool(string poolName, int initialSize)
        {
            try
            {
                var pool = new Queue<GameObject>();
                GameObject prefab = LoadPoolPrefabSafely(poolName);
                
                if (prefab == null)
                {
                    Debug.LogWarning($"[CourseGenerator] No prefab found for {poolName}, creating basic objects");
                }
                
                for (int i = 0; i < initialSize; i++)
                {
                    GameObject obj = null;
                    
                    if (prefab != null)
                    {
                        // Use actual prefab if available
                        obj = Instantiate(prefab);
                        obj.name = $"{poolName}_Pooled_{i}";
                    }
                    else
                    {
                        // Create basic placeholder objects
                        obj = CreateBasicPoolObject(poolName, i);
                    }
                    
                    if (obj != null)
                    {
                        obj.transform.SetParent(_poolContainer);
                        obj.SetActive(false);
                        pool.Enqueue(obj);
                    }
                }
                
                _pooledObjects[poolName] = pool;
                Debug.Log($"[CourseGenerator] Initialized {poolName} pool with {pool.Count} objects");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Failed to initialize {poolName} pool: {ex.Message}");
                // Create empty pool to prevent null reference errors
                _pooledObjects[poolName] = new Queue<GameObject>();
            }
        }
        
        /// <summary>
        /// Safely load prefab for object pool with fallback mechanisms
        /// </summary>
        private GameObject LoadPoolPrefabSafely(string poolName)
        {
            try
            {
                // Try different potential paths for prefabs
                string[] prefabPaths = {
                    $"Course/{poolName}Prefab",
                    $"Prefabs/{poolName}",
                    $"Course/Elements/{poolName}",
                    $"GameObjects/{poolName}"
                };
                
                foreach (string path in prefabPaths)
                {
                    GameObject prefab = Resources.Load<GameObject>(path);
                    if (prefab != null)
                    {
                        Debug.Log($"[CourseGenerator] Loaded prefab for {poolName} from: {path}");
                        return prefab;
                    }
                }
                
                Debug.LogWarning($"[CourseGenerator] No prefab found for {poolName} in any standard path");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Error loading prefab for {poolName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Create basic pool object when prefab is not available
        /// </summary>
        private GameObject CreateBasicPoolObject(string poolName, int index)
        {
            try
            {
                GameObject obj = new GameObject($"{poolName}_Pooled_{index}");
                
                // Add appropriate components based on pool type
                switch (poolName.ToLower())
                {
                    case "obstacles":
                        obj.AddComponent<BoxCollider>();
                        obj.AddComponent<MeshRenderer>();
                        obj.tag = "Obstacle";
                        break;
                        
                    case "collectibles":
                        obj.AddComponent<SphereCollider>().isTrigger = true;
                        obj.AddComponent<MeshRenderer>();
                        obj.tag = "Collectible";
                        break;
                        
                    case "powerups":
                        obj.AddComponent<CapsuleCollider>().isTrigger = true;
                        obj.AddComponent<MeshRenderer>();
                        obj.tag = "PowerUp";
                        break;
                        
                    case "environment":
                        obj.AddComponent<MeshRenderer>();
                        obj.tag = "Environment";
                        break;
                        
                    case "hazards":
                        obj.AddComponent<BoxCollider>().isTrigger = true;
                        obj.AddComponent<MeshRenderer>();
                        obj.tag = "Hazard";
                        break;
                        
                    default:
                        obj.AddComponent<MeshRenderer>();
                        break;
                }
                
                return obj;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Failed to create basic pool object for {poolName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create default course configuration
        /// </summary>
        private CourseConfiguration CreateDefaultConfiguration()
        {
            var config = ScriptableObject.CreateInstance<CourseConfiguration>();
            config.MaxDifficulty = CourseDifficulty.Extreme;
            config.ThemeProgression = new CourseTheme[] { CourseTheme.Industrial, CourseTheme.Neon, CourseTheme.Wasteland };
            config.DifficultyRampRate = 0.01f;
            config.SectionVariationCount = 5;
            
            return config;
        }

        /// <summary>
        /// Create default generation rules
        /// </summary>
        private GenerationRules CreateDefaultGenerationRules()
        {
            var rules = ScriptableObject.CreateInstance<GenerationRules>();
            rules.MinObstacleSpacing = 5f;
            rules.MaxObstacleSpacing = 15f;
            rules.CollectibleClusterSize = 3;
            rules.HazardSpacing = 20f;
            rules.PowerUpSpacing = 30f;
            rules.MaxConsecutiveHazards = 2;
            
            return rules;
        }

        /// <summary>
        /// Generate the initial course sections for game start
        /// </summary>
        private void GenerateInitialCourse()
        {
            // Generate starting sections
            for (int i = 0; i < _sectionsAhead; i++)
            {
                var section = GenerateSection(i * _sectionLength);
                _activeSections.Add(section);
            }
            
            // Pre-generate upcoming sections
            for (int i = 0; i < 2; i++)
            {
                var section = GenerateSection((_sectionsAhead + i) * _sectionLength);
                _upcomingSections.Enqueue(section);
            }
            
            Debug.Log($"[CourseGenerator] Initial course generated with {_activeSections.Count} active sections");
        }
        #endregion

        #region Course Generation
        /// <summary>
        /// Generate a new course for a complete run
        /// </summary>
        public void GenerateNewCourse()
        {
            // Clear existing course
            ClearActiveCourse();
            
            // Reset generation state
            _currentSectionIndex = 0;
            _currentDistance = 0f;
            _lastObstaclePosition = 0f;
            _lastCollectibleCluster = 0f;
            _consecutiveHazards = 0;
            
            // Determine course theme and difficulty
            DetermineCourseParameters();
            
            // Generate initial sections
            GenerateInitialCourse();
            
            Debug.Log($"[CourseGenerator] New course generated - Theme: {_currentTheme}, Difficulty: {_currentDifficulty}");
        }

        /// <summary>
        /// Update course generation based on player progress
        /// </summary>
        private void UpdateCourseGeneration()
        {
            // Get player position from game manager
            var gameManager = Core.GameManager.Instance;
            if (gameManager?.ActiveBot == null) return;
            
            float playerPosition = gameManager.ActiveBot.transform.position.x;
            _currentDistance = playerPosition;
            
            // Check if we need to generate new sections
            float sectionBoundary = (_currentSectionIndex + _sectionsAhead) * _sectionLength;
            
            if (playerPosition > sectionBoundary - _sectionLength)
            {
                GenerateNextSection();
                CleanupOldSections(playerPosition);
            }
            
            // Update difficulty based on distance
            UpdateDifficulty();
        }

        /// <summary>
        /// Generate the next course section
        /// </summary>
        private void GenerateNextSection()
        {
            _currentSectionIndex++;
            float sectionStartX = _currentSectionIndex * _sectionLength;
            
            // Use pre-generated section if available
            CourseSection newSection;
            if (_upcomingSections.Count > 0)
            {
                newSection = _upcomingSections.Dequeue();
                newSection.StartPosition = sectionStartX;
            }
            else
            {
                newSection = GenerateSection(sectionStartX);
            }
            
            _activeSections.Add(newSection);
            
            // Pre-generate next section
            var futureSection = GenerateSection(sectionStartX + _sectionLength);
            _upcomingSections.Enqueue(futureSection);
            
            OnSectionGenerated?.Invoke(newSection);
            
            Debug.Log($"[CourseGenerator] Generated section {_currentSectionIndex} at position {sectionStartX}");
        }

        /// <summary>
        /// Generate a course section at the specified position
        /// </summary>
        private CourseSection GenerateSection(float startX)
        {
            var section = new CourseSection
            {
                SectionIndex = Mathf.FloorToInt(startX / _sectionLength),
                StartPosition = startX,
                EndPosition = startX + _sectionLength,
                Theme = _currentTheme,
                Difficulty = _currentDifficulty,
                Pattern = SelectSectionPattern(),
                Elements = new List<CourseElement>()
            };
            
            // Generate section content based on pattern
            GenerateSectionContent(section);
            
            return section;
        }

        /// <summary>
        /// Generate content for a course section
        /// </summary>
        private void GenerateSectionContent(CourseSection section)
        {
            float sectionProgress = 0f;
            float currentX = section.StartPosition;
            
            while (sectionProgress < 1f)
            {
                // Determine what to place at this position
                var elementType = DetermineNextElement(currentX, section);
                
                if (elementType != CourseElementType.Empty)
                {
                    var element = CreateCourseElement(elementType, currentX, section.Difficulty);
                    if (element != null)
                    {
                        section.Elements.Add(element);
                        InstantiateCourseElement(element);
                    }
                }
                
                // Advance position
                float spacing = CalculateElementSpacing(elementType, section.Difficulty);
                currentX += spacing;
                sectionProgress = (currentX - section.StartPosition) / _sectionLength;
            }
            
            // Add environmental props for visual variety
            AddEnvironmentalElements(section);
        }

        /// <summary>
        /// Determine what type of element to place at a position
        /// </summary>
        private CourseElementType DetermineNextElement(float position, CourseSection section)
        {
            // Check spacing constraints
            float sinceLastObstacle = position - _lastObstaclePosition;
            float sinceLastCollectible = position - _lastCollectibleCluster;
            
            // Apply pattern-specific logic
            switch (section.Pattern)
            {
                case CoursePattern.ObstacleHeavy:
                    if (sinceLastObstacle >= _generationRules.MinObstacleSpacing)
                    {
                        if (UnityEngine.Random.value < 0.7f)
                        {
                            _lastObstaclePosition = position;
                            return UnityEngine.Random.value < 0.8f ? CourseElementType.Obstacle : CourseElementType.Hazard;
                        }
                    }
                    break;
                    
                case CoursePattern.CollectibleRich:
                    if (sinceLastCollectible >= 10f)
                    {
                        if (UnityEngine.Random.value < 0.8f)
                        {
                            _lastCollectibleCluster = position;
                            return CourseElementType.Collectible;
                        }
                    }
                    break;
                    
                case CoursePattern.Mixed:
                    if (sinceLastObstacle >= _obstacleSpacing)
                    {
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            _lastObstaclePosition = position;
                            return CourseElementType.Obstacle;
                        }
                    }
                    if (sinceLastCollectible >= 15f)
                    {
                        if (UnityEngine.Random.value < _collectibleDensity)
                        {
                            _lastCollectibleCluster = position;
                            return CourseElementType.Collectible;
                        }
                    }
                    break;
                    
                case CoursePattern.Standard:
                default:
                    // Standard generation logic
                    if (sinceLastObstacle >= _obstacleSpacing)
                    {
                        float obstacleChance = CalculateObstacleChance(section.Difficulty);
                        if (UnityEngine.Random.value < obstacleChance)
                        {
                            _lastObstaclePosition = position;
                            return CourseElementType.Obstacle;
                        }
                    }
                    
                    if (sinceLastCollectible >= 12f)
                    {
                        if (UnityEngine.Random.value < _collectibleDensity)
                        {
                            _lastCollectibleCluster = position;
                            return CourseElementType.Collectible;
                        }
                    }
                    
                    // Power-up chance
                    if (UnityEngine.Random.value < _powerUpFrequency)
                    {
                        return CourseElementType.PowerUp;
                    }
                    break;
            }
            
            return CourseElementType.Empty;
        }

        /// <summary>
        /// Create a course element of the specified type
        /// </summary>
        private CourseElement CreateCourseElement(CourseElementType elementType, float xPosition, CourseDifficulty difficulty)
        {
            var element = new CourseElement
            {
                ElementType = elementType,
                Position = new Vector3(xPosition, CalculateElementHeight(elementType, xPosition), 0f),
                Difficulty = difficulty,
                Theme = _currentTheme
            };
            
            // Set specific properties based on element type
            switch (elementType)
            {
                case CourseElementType.Obstacle:
                    element.ObstacleType = SelectObstacleType(difficulty);
                    element.RequiredAction = DetermineRequiredAction(element.ObstacleType);
                    break;
                    
                case CourseElementType.Collectible:
                    element.CollectibleType = SelectCollectibleType();
                    element.Value = CalculateCollectibleValue(element.CollectibleType, difficulty);
                    break;
                    
                case CourseElementType.PowerUp:
                    element.PowerUpType = SelectPowerUpType();
                    break;
                    
                case CourseElementType.Hazard:
                    element.HazardType = SelectHazardType(difficulty);
                    break;
            }
            
            return element;
        }

        /// <summary>
        /// Instantiate a course element in the world
        /// </summary>
        private GameObject InstantiateCourseElement(CourseElement element)
        {
            GameObject obj = GetPooledObject(element.ElementType.ToString());
            if (obj == null)
            {
                obj = CreateNewElement(element);
            }
            
            if (obj != null)
            {
                obj.transform.position = element.Position;
                obj.SetActive(true);
                
                // Configure the element's component
                ConfigureElementComponent(obj, element);
                
                _currentActiveObjects++;
            }
            
            return obj;
        }

        /// <summary>
        /// Configure an element's component with the generated data
        /// </summary>
        private void ConfigureElementComponent(GameObject obj, CourseElement element)
        {
            switch (element.ElementType)
            {
                case CourseElementType.Obstacle:
                    var obstacleData = obj.GetComponent<ObstacleData>() ?? obj.AddComponent<ObstacleData>();
                    obstacleData.ObstacleType = element.ObstacleType;
                    obstacleData.RequiredAction = element.RequiredAction;
                    obstacleData.BaseDamage = CalculateObstacleDamage(element.Difficulty);
                    break;
                    
                case CourseElementType.Collectible:
                    var collectibleData = obj.GetComponent<CollectibleData>() ?? obj.AddComponent<CollectibleData>();
                    collectibleData.CollectibleType = element.CollectibleType;
                    collectibleData.ScrapValue = element.Value;
                    break;
                    
                case CourseElementType.PowerUp:
                    var powerUpData = obj.GetComponent<PowerUpData>() ?? obj.AddComponent<PowerUpData>();
                    powerUpData.PowerUpType = element.PowerUpType;
                    break;
                    
                case CourseElementType.Hazard:
                    var hazardData = obj.GetComponent<HazardData>() ?? obj.AddComponent<HazardData>();
                    hazardData.HazardType = element.HazardType;
                    break;
            }
        }
        #endregion

        #region Element Generation Helpers
        /// <summary>
        /// Calculate the height position for an element
        /// </summary>
        private float CalculateElementHeight(CourseElementType elementType, float xPosition)
        {
            float baseHeight = 0f;
            
            switch (elementType)
            {
                case CourseElementType.Obstacle:
                    // Obstacles can be at ground level or elevated
                    baseHeight = UnityEngine.Random.value < 0.3f ? 3f : 0f;
                    break;
                    
                case CourseElementType.Collectible:
                    // Collectibles can be at various heights to encourage jumping/sliding
                    baseHeight = UnityEngine.Random.Range(-1f, 4f);
                    break;
                    
                case CourseElementType.PowerUp:
                    // Power-ups typically at medium height
                    baseHeight = UnityEngine.Random.Range(1f, 3f);
                    break;
                    
                case CourseElementType.Hazard:
                    // Hazards usually at ground level
                    baseHeight = 0f;
                    break;
            }
            
            return baseHeight;
        }

        /// <summary>
        /// Select section pattern based on current conditions
        /// </summary>
        private CoursePattern SelectSectionPattern()
        {
            // Base pattern selection on distance and difficulty
            float patternRandom = UnityEngine.Random.value;
            
            switch (_currentDifficulty)
            {
                case CourseDifficulty.Easy:
                    return patternRandom < 0.8f ? CoursePattern.Standard : CoursePattern.CollectibleRich;
                    
                case CourseDifficulty.Medium:
                    if (patternRandom < 0.4f) return CoursePattern.Standard;
                    if (patternRandom < 0.7f) return CoursePattern.Mixed;
                    return CoursePattern.ObstacleHeavy;
                    
                case CourseDifficulty.Hard:
                    if (patternRandom < 0.3f) return CoursePattern.Mixed;
                    return CoursePattern.ObstacleHeavy;
                    
                case CourseDifficulty.Extreme:
                    return UnityEngine.Random.value < 0.7f ? CoursePattern.ObstacleHeavy : CoursePattern.Gauntlet;
                    
                default:
                    return CoursePattern.Standard;
            }
        }

        /// <summary>
        /// Calculate spacing between elements
        /// </summary>
        private float CalculateElementSpacing(CourseElementType elementType, CourseDifficulty difficulty)
        {
            float baseSpacing = 8f;
            float difficultyModifier = 1f - ((int)difficulty * 0.15f);
            
            switch (elementType)
            {
                case CourseElementType.Obstacle:
                    baseSpacing = _obstacleSpacing;
                    break;
                    
                case CourseElementType.Collectible:
                    baseSpacing = 6f;
                    break;
                    
                case CourseElementType.PowerUp:
                    baseSpacing = 25f;
                    break;
                    
                case CourseElementType.Hazard:
                    baseSpacing = 15f;
                    break;
            }
            
            return baseSpacing * difficultyModifier;
        }

        /// <summary>
        /// Calculate obstacle appearance chance based on difficulty
        /// </summary>
        private float CalculateObstacleChance(CourseDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CourseDifficulty.Easy: return 0.3f;
                case CourseDifficulty.Medium: return 0.5f;
                case CourseDifficulty.Hard: return 0.7f;
                case CourseDifficulty.Extreme: return 0.9f;
                default: return 0.4f;
            }
        }

        /// <summary>
        /// Select appropriate obstacle type for difficulty
        /// </summary>
        private string SelectObstacleType(CourseDifficulty difficulty)
        {
            string[] easyObstacles = { "Low Barrier", "Simple Block" };
            string[] mediumObstacles = { "High Barrier", "Spinning Blade", "Moving Platform" };
            string[] hardObstacles = { "Laser Grid", "Crushing Block", "Electro Barrier" };
            string[] extremeObstacles = { "Multi-Stage Trap", "Plasma Wall", "Quantum Barrier" };
            
            switch (difficulty)
            {
                case CourseDifficulty.Easy:
                    return easyObstacles[UnityEngine.Random.Range(0, easyObstacles.Length)];
                case CourseDifficulty.Medium:
                    return mediumObstacles[UnityEngine.Random.Range(0, mediumObstacles.Length)];
                case CourseDifficulty.Hard:
                    return hardObstacles[UnityEngine.Random.Range(0, hardObstacles.Length)];
                case CourseDifficulty.Extreme:
                    return extremeObstacles[UnityEngine.Random.Range(0, extremeObstacles.Length)];
                default:
                    return "Simple Block";
            }
        }

        /// <summary>
        /// Determine required action to overcome obstacle
        /// </summary>
        private Bot.AIDecisionType DetermineRequiredAction(string obstacleType)
        {
            switch (obstacleType.ToLower())
            {
                case "low barrier":
                case "simple block":
                    return Bot.AIDecisionType.Jump;
                    
                case "high barrier":
                case "laser grid":
                    return UnityEngine.Random.value < 0.5f ? Bot.AIDecisionType.Slide : Bot.AIDecisionType.Dash;
                    
                case "spinning blade":
                case "electro barrier":
                    return Bot.AIDecisionType.Slide;
                    
                case "moving platform":
                case "crushing block":
                    return Bot.AIDecisionType.Dash;
                    
                default:
                    return Bot.AIDecisionType.Jump;
            }
        }

        /// <summary>
        /// Calculate damage value for obstacles
        /// </summary>
        private int CalculateObstacleDamage(CourseDifficulty difficulty)
        {
            int baseDamage = 10;
            int difficultyMultiplier = (int)difficulty + 1;
            
            return baseDamage * difficultyMultiplier;
        }

        /// <summary>
        /// Select collectible type
        /// </summary>
        private string SelectCollectibleType()
        {
            string[] collectibleTypes = { "Scrap", "Data Core", "Circuit Fragment", "Energy Cell", "Rare Metal" };
            return collectibleTypes[UnityEngine.Random.Range(0, collectibleTypes.Length)];
        }

        /// <summary>
        /// Calculate collectible value
        /// </summary>
        private int CalculateCollectibleValue(string collectibleType, CourseDifficulty difficulty)
        {
            int baseValue = 1;
            
            switch (collectibleType.ToLower())
            {
                case "scrap": baseValue = 1; break;
                case "data core": baseValue = 5; break;
                case "circuit fragment": baseValue = 3; break;
                case "energy cell": baseValue = 2; break;
                case "rare metal": baseValue = 10; break;
            }
            
            // Increase value with difficulty
            baseValue += (int)difficulty;
            
            return baseValue;
        }

        /// <summary>
        /// Select power-up type
        /// </summary>
        private string SelectPowerUpType()
        {
            string[] powerUpTypes = { "Speed Boost", "Shield", "Magnet", "Double Points", "Invincibility" };
            return powerUpTypes[UnityEngine.Random.Range(0, powerUpTypes.Length)];
        }

        /// <summary>
        /// Select hazard type
        /// </summary>
        private string SelectHazardType(CourseDifficulty difficulty)
        {
            string[] hazardTypes = { "Spike Pit", "Lava Pool", "Acid Bath", "Void Zone", "Plasma Field" };
            int maxIndex = Mathf.Min(hazardTypes.Length, (int)difficulty + 2);
            return hazardTypes[UnityEngine.Random.Range(0, maxIndex)];
        }

        /// <summary>
        /// Add environmental elements for visual variety
        /// </summary>
        private void AddEnvironmentalElements(CourseSection section)
        {
            int environmentCount = UnityEngine.Random.Range(2, 6);
            
            for (int i = 0; i < environmentCount; i++)
            {
                float xPos = UnityEngine.Random.Range(section.StartPosition, section.EndPosition);
                float yPos = UnityEngine.Random.Range(-2f, 5f);
                
                var environmentElement = new CourseElement
                {
                    ElementType = CourseElementType.Environment,
                    Position = new Vector3(xPos, yPos, UnityEngine.Random.Range(1f, 3f)), // Z for background/foreground
                    Theme = section.Theme
                };
                
                section.Elements.Add(environmentElement);
                
                // Instantiate environment object
                GameObject envObj = GetPooledObject("Environment");
                if (envObj != null)
                {
                    envObj.transform.position = environmentElement.Position;
                    envObj.SetActive(true);
                    
                    // Configure environment visual based on theme
                    ConfigureEnvironmentVisual(envObj, section.Theme);
                }
            }
        }

        /// <summary>
        /// Configure environment visuals based on theme
        /// </summary>
        private void ConfigureEnvironmentVisual(GameObject envObj, CourseTheme theme)
        {
            var renderer = envObj.GetComponent<Renderer>();
            if (renderer == null) return;
            
            switch (theme)
            {
                case CourseTheme.Industrial:
                    renderer.material.color = Color.gray;
                    break;
                case CourseTheme.Neon:
                    renderer.material.color = Color.cyan;
                    break;
                case CourseTheme.Wasteland:
                    renderer.material.color = Color.yellow;
                    break;
                case CourseTheme.Digital:
                    renderer.material.color = Color.green;
                    break;
                case CourseTheme.Arctic:
                    renderer.material.color = Color.white;
                    break;
            }
        }
        #endregion

        #region Course Management
        /// <summary>
        /// Determine course parameters for the current run
        /// </summary>
        private void DetermineCourseParameters()
        {
            // Get player progression data
            var resourceManager = Core.GameManager.Instance?.Resources;
            int playerLevel = resourceManager?.PlayerLevel ?? 1;
            
            // Set difficulty based on player level and distance
            _currentDifficulty = CalculateCourseDifficulty(playerLevel, _currentDistance);
            
            // Set theme based on progression
            _currentTheme = SelectCourseTheme(playerLevel);
            
            // Apply difficulty scaling
            _difficultyScale = 1f + (playerLevel * 0.1f);
            
            Debug.Log($"[CourseGenerator] Course parameters - Difficulty: {_currentDifficulty}, Theme: {_currentTheme}");
        }

        /// <summary>
        /// Calculate appropriate course difficulty
        /// </summary>
        private CourseDifficulty CalculateCourseDifficulty(int playerLevel, float distance)
        {
            // Base difficulty on player level
            CourseDifficulty baseDifficulty = (CourseDifficulty)Mathf.Min(playerLevel / 5, (int)CourseDifficulty.Extreme);
            
            // Increase difficulty during the run based on distance
            float distanceBonus = distance / 1000f; // Every 1000 units increases difficulty
            int totalDifficulty = (int)baseDifficulty + Mathf.FloorToInt(distanceBonus);
            
            return (CourseDifficulty)Mathf.Clamp(totalDifficulty, 0, (int)CourseDifficulty.Extreme);
        }

        /// <summary>
        /// Select course theme based on player progression
        /// </summary>
        private CourseTheme SelectCourseTheme(int playerLevel)
        {
            if (_courseConfig.ThemeProgression == null || _courseConfig.ThemeProgression.Length == 0)
            {
                return CourseTheme.Industrial; // Default theme
            }
            
            // Unlock themes based on player level
            int themeIndex = Mathf.Clamp(playerLevel / 3, 0, _courseConfig.ThemeProgression.Length - 1);
            
            // Add some randomness for variety
            if (UnityEngine.Random.value < 0.2f && themeIndex > 0)
            {
                themeIndex = UnityEngine.Random.Range(0, themeIndex + 1);
            }
            
            return _courseConfig.ThemeProgression[themeIndex];
        }

        /// <summary>
        /// Update difficulty during the run
        /// </summary>
        private void UpdateDifficulty()
        {
            var newDifficulty = CalculateCourseDifficulty(
                Core.GameManager.Instance?.Resources?.PlayerLevel ?? 1, 
                _currentDistance);
            
            if (newDifficulty != _currentDifficulty)
            {
                _currentDifficulty = newDifficulty;
                OnDifficultyChanged?.Invoke(_currentDifficulty);
            }
        }

        /// <summary>
        /// Clear the active course
        /// </summary>
        private void ClearActiveCourse()
        {
            // Return all active objects to pools
            foreach (var section in _activeSections)
            {
                foreach (var element in section.Elements)
                {
                    ReturnElementToPool(element);
                }
            }
            
            _activeSections.Clear();
            _upcomingSections.Clear();
            _currentActiveObjects = 0;
        }

        /// <summary>
        /// Cleanup old sections that are behind the player
        /// </summary>
        private void CleanupOldSections(float playerPosition)
        {
            float cleanupBoundary = playerPosition - (_sectionsBehind * _sectionLength);
            
            for (int i = _activeSections.Count - 1; i >= 0; i--)
            {
                if (_activeSections[i].EndPosition < cleanupBoundary)
                {
                    var section = _activeSections[i];
                    
                    // Return section elements to pools
                    foreach (var element in section.Elements)
                    {
                        ReturnElementToPool(element);
                    }
                    
                    _activeSections.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Return a course element to its object pool
        /// </summary>
        private void ReturnElementToPool(CourseElement element)
        {
            // Find the GameObject associated with this element and return to pool
            // This would need proper tracking in a real implementation
            _currentActiveObjects--;
        }
        #endregion

        #region Object Pooling
        /// <summary>
        /// Get a pooled object of the specified type
        /// </summary>
        private GameObject GetPooledObject(string poolType)
        {
            if (_pooledObjects.ContainsKey(poolType) && _pooledObjects[poolType].Count > 0)
            {
                return _pooledObjects[poolType].Dequeue();
            }
            
            return null;
        }

        /// <summary>
        /// Return an object to its pool
        /// </summary>
        private void ReturnToPool(string poolType, GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            obj.transform.SetParent(_poolContainer);
            
            if (!_pooledObjects.ContainsKey(poolType))
            {
                _pooledObjects[poolType] = new Queue<GameObject>();
            }
            
            if (_pooledObjects[poolType].Count < MAX_POOL_SIZE)
            {
                _pooledObjects[poolType].Enqueue(obj);
            }
            else
            {
                // Destroy excess objects to prevent memory leaks
                DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Create a new element when pool is empty
        /// </summary>
        private GameObject CreateNewElement(CourseElement element)
        {
            // This would normally load from prefab resources
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"{element.ElementType}_{element.GetHashCode()}";
            
            // Add appropriate component
            switch (element.ElementType)
            {
                case CourseElementType.Obstacle:
                    obj.AddComponent<ObstacleData>();
                    obj.tag = "Obstacle";
                    break;
                case CourseElementType.Collectible:
                    obj.AddComponent<CollectibleData>();
                    obj.tag = "Collectible";
                    break;
                case CourseElementType.PowerUp:
                    obj.AddComponent<PowerUpData>();
                    obj.tag = "PowerUp";
                    break;
                case CourseElementType.Hazard:
                    obj.AddComponent<HazardData>();
                    obj.tag = "Hazard";
                    break;
            }
            
            return obj;
        }
        #endregion

        #region Performance Optimization
        /// <summary>
        /// Update object culling for performance
        /// </summary>
        private void UpdateObjectCulling()
        {
            if (_currentActiveObjects <= _maxActiveObjects) return;
            
            // Implement distance-based culling
            // This would disable distant objects to improve performance
        }

        /// <summary>
        /// Reduce particle effects for performance mode
        /// </summary>
        public void ReduceParticleEffects()
        {
            _performanceModeActive = true;
            // Disable or reduce particle systems
        }

        /// <summary>
        /// Restore particle effects when performance improves
        /// </summary>
        public void RestoreParticleEffects()
        {
            _performanceModeActive = false;
            // Re-enable particle systems
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get the current active course sections
        /// </summary>
        public List<CourseSection> GetActiveSections()
        {
            return new List<CourseSection>(_activeSections);
        }

        /// <summary>
        /// Get course statistics for the current run
        /// </summary>
        public CourseStatistics GetCourseStatistics()
        {
            return new CourseStatistics
            {
                TotalDistance = _currentDistance,
                CurrentDifficulty = _currentDifficulty,
                CurrentTheme = _currentTheme,
                SectionsGenerated = _currentSectionIndex,
                ActiveObjects = _currentActiveObjects,
                PerformanceModeActive = _performanceModeActive
            };
        }

        /// <summary>
        /// Force a theme change (for special events or purchases)
        /// </summary>
        public void ForceThemeChange(CourseTheme newTheme)
        {
            _currentTheme = newTheme;
            OnThemeChanged?.Invoke(_currentTheme);
        }
        
        /// <summary>
        /// Get object from pool with automatic expansion if needed
        /// </summary>
        private GameObject GetPooledObject(string poolName)
        {
            try
            {
                if (!_pooledObjects.ContainsKey(poolName))
                {
                    Debug.LogWarning($"[CourseGenerator] Pool {poolName} not found, creating new pool");
                    InitializePool(poolName, 10);
                }
                
                var pool = _pooledObjects[poolName];
                
                // If pool is empty, expand it
                if (pool.Count == 0)
                {
                    if (_currentActiveObjects < _maxActiveObjects)
                    {
                        ExpandPool(poolName, 5);
                    }
                    else
                    {
                        Debug.LogWarning($"[CourseGenerator] Max active objects reached ({_maxActiveObjects}), cannot expand {poolName} pool");
                        return null;
                    }
                }
                
                if (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        _currentActiveObjects++;
                        return obj;
                    }
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Error getting pooled object for {poolName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Return object to pool for reuse
        /// </summary>
        private void ReturnToPool(GameObject obj, string poolName)
        {
            try
            {
                if (obj == null || !_pooledObjects.ContainsKey(poolName))
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                    return;
                }
                
                obj.SetActive(false);
                obj.transform.SetParent(_poolContainer);
                _pooledObjects[poolName].Enqueue(obj);
                _currentActiveObjects--;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Error returning object to {poolName} pool: {ex.Message}");
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        
        /// <summary>
        /// Expand existing pool when more objects are needed
        /// </summary>
        private void ExpandPool(string poolName, int expandAmount)
        {
            try
            {
                if (!_pooledObjects.ContainsKey(poolName))
                {
                    return;
                }
                
                var pool = _pooledObjects[poolName];
                GameObject prefab = LoadPoolPrefabSafely(poolName);
                
                for (int i = 0; i < expandAmount; i++)
                {
                    // Check total pool size limit
                    if (GetTotalPoolSize() >= MAX_POOL_SIZE)
                    {
                        Debug.LogWarning($"[CourseGenerator] Maximum total pool size reached ({MAX_POOL_SIZE})");
                        break;
                    }
                    
                    GameObject obj = null;
                    
                    if (prefab != null)
                    {
                        obj = Instantiate(prefab);
                        obj.name = $"{poolName}_Expanded_{i}";
                    }
                    else
                    {
                        obj = CreateBasicPoolObject(poolName, i);
                    }
                    
                    if (obj != null)
                    {
                        obj.transform.SetParent(_poolContainer);
                        obj.SetActive(false);
                        pool.Enqueue(obj);
                    }
                }
                
                Debug.Log($"[CourseGenerator] Expanded {poolName} pool by {expandAmount} objects");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CourseGenerator] Failed to expand {poolName} pool: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get total number of objects across all pools
        /// </summary>
        private int GetTotalPoolSize()
        {
            int total = 0;
            foreach (var pool in _pooledObjects.Values)
            {
                total += pool.Count;
            }
            return total + _currentActiveObjects;
        }
        
        /// <summary>
        /// Reduce particle effects during performance mode
        /// </summary>
        public void ReduceParticleEffects()
        {
            _performanceModeActive = true;
            // Implementation would reduce or disable particle effects
            Debug.Log("[CourseGenerator] Reduced particle effects for performance mode");
        }
        
        /// <summary>
        /// Restore full particle effects when performance improves
        /// </summary>
        public void RestoreParticleEffects()
        {
            _performanceModeActive = false;
            // Implementation would restore full particle effects
            Debug.Log("[CourseGenerator] Restored full particle effects");
        }
        #endregion
    }
}