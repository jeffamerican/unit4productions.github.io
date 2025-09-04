using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CircuitRunners.Core
{
    /// <summary>
    /// Central game manager that orchestrates all core game systems and manages game state.
    /// This singleton handles session management, game flow, and coordinates between all major systems.
    /// 
    /// Key Responsibilities:
    /// - Game state management (menu, building, running, results)
    /// - Session flow control and transitions
    /// - System initialization and shutdown
    /// - Performance monitoring and optimization triggers
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Implementation
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Game State Management
        /// <summary>
        /// Defines the primary game states for the Circuit Runners flow
        /// </summary>
        public enum GameState
        {
            MainMenu,       // Player in main menu, can access bot builder
            BotBuilding,    // Player customizing their bot
            PreRun,         // Final preparations before run starts
            Running,        // Bot is actively running the course
            PostRun,        // Results screen, resource collection
            Paused,         // Game is paused (can happen during running)
            Loading         // Loading between states or scenes
        }

        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        [SerializeField] private GameState _previousState = GameState.MainMenu;
        
        /// <summary>
        /// Current game state - triggers state change events when modified
        /// </summary>
        public GameState CurrentState 
        { 
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _previousState = _currentState;
                    _currentState = value;
                    OnGameStateChanged?.Invoke(_previousState, _currentState);
                }
            }
        }

        /// <summary>
        /// Event fired when game state changes - used by UI and other systems to respond
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        #endregion

        #region System References
        [Header("Core Systems")]
        [SerializeField] private Bot.BotController _activeBotController;
        [SerializeField] private Course.CourseGenerator _courseGenerator;
        [SerializeField] private Data.ResourceManager _resourceManager;
        [SerializeField] private Bot.BotBuilder _botBuilder;
        [SerializeField] private Monetization.MonetizationManager _monetizationManager;

        // System properties for easy access by other classes
        public Bot.BotController ActiveBot => _activeBotController;
        public Course.CourseGenerator CourseGenerator => _courseGenerator;
        public Data.ResourceManager Resources => _resourceManager;
        public Bot.BotBuilder BotBuilder => _botBuilder;
        public Monetization.MonetizationManager Monetization => _monetizationManager;
        #endregion

        #region Session Management
        [Header("Session Settings")]
        [SerializeField] private float _runTimeoutSeconds = 300f; // 5 minute max run time
        [SerializeField] private int _maxConsecutiveRuns = 5; // Energy system integration
        
        private int _currentRunCount = 0;
        private float _sessionStartTime;
        private Coroutine _currentRunCoroutine;

        /// <summary>
        /// Current run number in this session (resets when energy depleted)
        /// </summary>
        public int CurrentRunCount => _currentRunCount;
        
        /// <summary>
        /// Whether the player can start another run (energy/cooldown check)
        /// </summary>
        public bool CanStartRun => _currentRunCount < _maxConsecutiveRuns && 
                                   Resources.HasEnoughEnergy(1);
        #endregion

        #region Performance Monitoring
        [Header("Performance")]
        [SerializeField] private float _targetFrameRate = 60f;
        [SerializeField] private float _lowFrameRateThreshold = 30f;
        [SerializeField] private bool _enablePerformanceOptimizations = true;
        
        private float[] _frameTimeHistory = new float[30]; // Track last 30 frames
        private int _frameTimeIndex = 0;
        private bool _performanceOptimizationsActive = false;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Ensure singleton pattern and persistent across scenes
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize core settings
            InitializeGameSettings();
        }

        private void Start()
        {
            _sessionStartTime = Time.time;
            InitializeSystems();
            
            // Start in main menu state
            TransitionToState(GameState.MainMenu);
        }

        private void Update()
        {
            // Optimized performance monitoring (reduced frequency)
            if (_enablePerformanceOptimizations)
            {
                MonitorPerformance();
            }

            // Update active systems based on current state
            UpdateCurrentState();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                HandleApplicationPause();
            }
            else
            {
                HandleApplicationResume();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && CurrentState == GameState.Running)
            {
                // Auto-pause during runs when app loses focus
                TransitionToState(GameState.Paused);
            }
        }
        #endregion

        #region System Initialization
        /// <summary>
        /// Initialize core game settings and Unity configurations
        /// </summary>
        private void InitializeGameSettings()
        {
            // Set target frame rate for mobile optimization
            Application.targetFrameRate = (int)_targetFrameRate;
            
            // Prevent screen from sleeping during gameplay
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Set quality settings for mobile optimization
            QualitySettings.vSyncCount = 0; // Disable VSync for better mobile performance
            
            Debug.Log("[GameManager] Core game settings initialized");
        }

        /// <summary>
        /// Initialize all game systems in proper dependency order
        /// </summary>
        private void InitializeSystems()
        {
            try
            {
                // Initialize systems in dependency order
                // Resource Manager first (others depend on it)
                if (_resourceManager == null)
                    _resourceManager = FindObjectOfType<Data.ResourceManager>();
                
                // Monetization system (depends on resources)
                if (_monetizationManager == null)
                    _monetizationManager = FindObjectOfType<Monetization.MonetizationManager>();
                
                // Course generator (independent system)
                if (_courseGenerator == null)
                    _courseGenerator = FindObjectOfType<Course.CourseGenerator>();
                
                // Bot builder (depends on resources)
                if (_botBuilder == null)
                    _botBuilder = FindObjectOfType<Bot.BotBuilder>();
                
                // Bot controller initialized when needed during runs
                
                Debug.Log("[GameManager] All systems initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] System initialization failed: {e.Message}");
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Transition to a new game state with proper validation and cleanup
        /// </summary>
        /// <param name="newState">Target game state</param>
        public void TransitionToState(GameState newState)
        {
            // Validate state transition
            if (!IsValidStateTransition(CurrentState, newState))
            {
                Debug.LogWarning($"[GameManager] Invalid state transition from {CurrentState} to {newState}");
                return;
            }

            Debug.Log($"[GameManager] State transition: {CurrentState} -> {newState}");
            
            // Cleanup current state
            ExitCurrentState();
            
            // Change state (triggers events)
            CurrentState = newState;
            
            // Initialize new state
            EnterNewState();
        }

        /// <summary>
        /// Validates whether a state transition is allowed
        /// </summary>
        private bool IsValidStateTransition(GameState from, GameState to)
        {
            // Define valid state transitions
            switch (from)
            {
                case GameState.MainMenu:
                    return to == GameState.BotBuilding || to == GameState.Loading;
                    
                case GameState.BotBuilding:
                    return to == GameState.PreRun || to == GameState.MainMenu || to == GameState.Loading;
                    
                case GameState.PreRun:
                    return to == GameState.Running || to == GameState.BotBuilding || to == GameState.Loading;
                    
                case GameState.Running:
                    return to == GameState.PostRun || to == GameState.Paused || to == GameState.Loading;
                    
                case GameState.Paused:
                    return to == GameState.Running || to == GameState.MainMenu || to == GameState.Loading;
                    
                case GameState.PostRun:
                    return to == GameState.MainMenu || to == GameState.BotBuilding || to == GameState.Loading;
                    
                case GameState.Loading:
                    return true; // Loading can transition to any state
                    
                default:
                    return false;
            }
        }

        /// <summary>
        /// Cleanup logic when exiting the current state
        /// </summary>
        private void ExitCurrentState()
        {
            switch (CurrentState)
            {
                case GameState.Running:
                    // Stop any running coroutines
                    if (_currentRunCoroutine != null)
                    {
                        StopCoroutine(_currentRunCoroutine);
                        _currentRunCoroutine = null;
                    }
                    break;
                    
                case GameState.BotBuilding:
                    // Save current bot configuration
                    if (_botBuilder != null)
                        _botBuilder.SaveCurrentBotConfiguration();
                    break;
            }
        }

        /// <summary>
        /// Initialization logic when entering a new state
        /// </summary>
        private void EnterNewState()
        {
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    // Reset run count when returning to main menu
                    _currentRunCount = 0;
                    break;
                    
                case GameState.BotBuilding:
                    // Initialize bot builder if needed
                    if (_botBuilder != null)
                        _botBuilder.InitializeBuildingSession();
                    break;
                    
                case GameState.PreRun:
                    // Validate bot configuration and course readiness
                    PrepareForRun();
                    break;
                    
                case GameState.Running:
                    // Start the actual run
                    StartBotRun();
                    break;
                    
                case GameState.PostRun:
                    // Calculate and display results
                    ProcessRunResults();
                    break;
            }
        }

        /// <summary>
        /// Update logic that runs every frame based on current state
        /// </summary>
        private void UpdateCurrentState()
        {
            switch (CurrentState)
            {
                case GameState.Running:
                    // Monitor bot progress and check for run completion
                    if (_activeBotController != null)
                    {
                        CheckRunCompletion();
                    }
                    break;
                    
                case GameState.Paused:
                    // Handle pause input (resume/quit)
                    HandlePauseInput();
                    break;
            }
        }
        #endregion

        #region Run Management
        /// <summary>
        /// Prepare all systems for an upcoming bot run
        /// </summary>
        private void PrepareForRun()
        {
            try
            {
                // Validate energy availability
                if (!CanStartRun)
                {
                    Debug.LogWarning("[GameManager] Cannot start run - insufficient energy or at run limit");
                    // Could trigger monetization prompt for energy purchase
                    _monetizationManager?.ShowEnergyPurchasePrompt();
                    TransitionToState(GameState.MainMenu);
                    return;
                }

                // Generate new course
                if (_courseGenerator != null)
                {
                    _courseGenerator.GenerateNewCourse();
                }

                // Prepare bot with current configuration
                if (_botBuilder != null && _activeBotController != null)
                {
                    _botBuilder.ApplyConfigurationToBot(_activeBotController);
                }

                Debug.Log("[GameManager] Run preparation completed successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Run preparation failed: {e.Message}");
                TransitionToState(GameState.BotBuilding);
            }
        }

        /// <summary>
        /// Start the actual bot run with timeout and monitoring
        /// </summary>
        private void StartBotRun()
        {
            if (_activeBotController == null)
            {
                Debug.LogError("[GameManager] No active bot controller for run");
                TransitionToState(GameState.BotBuilding);
                return;
            }

            // Consume energy for this run
            if (!_resourceManager.ConsumeEnergy(1))
            {
                Debug.LogWarning("[GameManager] Failed to consume energy for run");
                TransitionToState(GameState.MainMenu);
                return;
            }

            // Start run with timeout
            _currentRunCoroutine = StartCoroutine(RunWithTimeout());
            _currentRunCount++;

            // Start the bot
            _activeBotController.StartRun();

            Debug.Log($"[GameManager] Bot run started (Run #{_currentRunCount})");
        }

        /// <summary>
        /// Coroutine to handle run timeout and ensure runs don't go on indefinitely
        /// </summary>
        private IEnumerator RunWithTimeout()
        {
            float startTime = Time.time;
            
            while (Time.time - startTime < _runTimeoutSeconds)
            {
                // Check if run completed naturally
                if (_activeBotController != null && _activeBotController.IsRunComplete)
                {
                    yield break; // Exit coroutine, run completed normally
                }
                
                yield return null; // Wait one frame
            }

            // If we reach here, the run timed out
            Debug.LogWarning("[GameManager] Bot run timed out");
            
            if (_activeBotController != null)
            {
                _activeBotController.ForceEndRun("Timeout");
            }
            
            TransitionToState(GameState.PostRun);
        }

        /// <summary>
        /// Check if the current run should end and handle completion
        /// </summary>
        private void CheckRunCompletion()
        {
            if (_activeBotController == null) return;

            // Check various end conditions
            bool runComplete = _activeBotController.IsRunComplete;
            bool botDestroyed = _activeBotController.IsBotDestroyed;
            bool courseComplete = _activeBotController.HasCompletedCourse;

            if (runComplete || botDestroyed || courseComplete)
            {
                // End the run and transition to results
                TransitionToState(GameState.PostRun);
            }
        }

        /// <summary>
        /// Process run results, calculate rewards, and update progression
        /// </summary>
        private void ProcessRunResults()
        {
            if (_activeBotController == null) return;

            try
            {
                // Calculate run performance metrics
                var runStats = _activeBotController.GetRunStatistics();
                
                // Calculate base rewards based on performance
                int baseScrap = CalculateScrapReward(runStats);
                int baseXP = CalculateXPReward(runStats);
                
                // Apply any multipliers or bonuses
                int finalScrap = ApplyRewardMultipliers(baseScrap, "scrap");
                int finalXP = ApplyRewardMultipliers(baseXP, "xp");
                
                // Award resources
                _resourceManager.AddScrap(finalScrap);
                _resourceManager.AddXP(finalXP);
                
                // Check for achievements or milestones
                CheckAchievements(runStats);
                
                // Show rewarded ad opportunity if applicable
                if (_monetizationManager != null && ShouldShowRewardedAd())
                {
                    _monetizationManager.ShowRewardedAdPrompt(finalScrap, finalXP);
                }

                Debug.Log($"[GameManager] Run results processed - Scrap: {finalScrap}, XP: {finalXP}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Failed to process run results: {e.Message}");
            }
        }
        #endregion

        #region Reward Calculation
        /// <summary>
        /// Calculate base scrap reward based on run performance
        /// </summary>
        private int CalculateScrapReward(Bot.BotRunStatistics stats)
        {
            int baseReward = 10; // Minimum reward
            
            // Distance bonus
            baseReward += Mathf.FloorToInt(stats.DistanceTraveled * 0.5f);
            
            // Collectible bonus
            baseReward += stats.CollectiblesGathered * 5;
            
            // Survival time bonus
            baseReward += Mathf.FloorToInt(stats.SurvivalTime * 2f);
            
            // Perfect run bonus
            if (stats.HasCompletedCourse && stats.DamagesTaken == 0)
            {
                baseReward += 50;
            }

            return baseReward;
        }

        /// <summary>
        /// Calculate base XP reward based on run performance
        /// </summary>
        private int CalculateXPReward(Bot.BotRunStatistics stats)
        {
            int baseXP = 5; // Minimum XP
            
            // Performance-based XP
            baseXP += Mathf.FloorToInt(stats.DistanceTraveled * 0.2f);
            baseXP += stats.ObstaclesAvoided * 2;
            
            // Completion bonus
            if (stats.HasCompletedCourse)
            {
                baseXP += 25;
            }

            return baseXP;
        }

        /// <summary>
        /// Apply reward multipliers from premium purchases, achievements, etc.
        /// </summary>
        private int ApplyRewardMultipliers(int baseAmount, string rewardType)
        {
            float multiplier = 1.0f;
            
            // Premium account multiplier
            if (_resourceManager.HasPremiumAccount)
            {
                multiplier += 0.5f;
            }
            
            // Daily bonus multiplier
            if (_resourceManager.HasDailyBonus)
            {
                multiplier += 0.25f;
            }
            
            // Event multiplier (weekends, special events)
            multiplier += _resourceManager.GetCurrentEventMultiplier(rewardType);

            return Mathf.RoundToInt(baseAmount * multiplier);
        }

        /// <summary>
        /// Check if player qualifies for achievements based on run performance
        /// </summary>
        private void CheckAchievements(Bot.BotRunStatistics stats)
        {
            // This would integrate with an achievement system
            // For now, just log potential achievements
            
            if (stats.DistanceTraveled > 1000f)
            {
                Debug.Log("[GameManager] Achievement unlocked: Long Distance Runner");
            }
            
            if (stats.CollectiblesGathered >= 50)
            {
                Debug.Log("[GameManager] Achievement unlocked: Collector Bot");
            }
            
            if (stats.HasCompletedCourse && stats.DamagesTaken == 0)
            {
                Debug.Log("[GameManager] Achievement unlocked: Perfect Run");
            }
        }

        /// <summary>
        /// Determine if player should be offered a rewarded ad opportunity
        /// </summary>
        private bool ShouldShowRewardedAd()
        {
            // Show ads every 3-4 runs, but not too frequently
            return _currentRunCount % 3 == 0 && UnityEngine.Random.value < 0.7f;
        }
        #endregion

        #region Performance Monitoring
        private float _lastPerformanceCheck = 0f;
        private const float PERFORMANCE_CHECK_INTERVAL = 1f; // Check only once per second
        private float _performanceSampleAccumulator = 0f;
        private int _performanceSampleCount = 0;
        
        /// <summary>
        /// Optimized performance monitoring that doesn't run every frame
        /// Uses sampling approach to reduce CPU overhead while maintaining accuracy
        /// </summary>
        private void MonitorPerformance()
        {
            // Sample frame times without heavy processing every frame
            _performanceSampleAccumulator += Time.unscaledDeltaTime;
            _performanceSampleCount++;
            
            // Only perform expensive calculations periodically
            if (Time.unscaledTime - _lastPerformanceCheck >= PERFORMANCE_CHECK_INTERVAL)
            {
                _lastPerformanceCheck = Time.unscaledTime;
                
                // Calculate average FPS from accumulated samples
                if (_performanceSampleCount > 0)
                {
                    float averageFrameTime = _performanceSampleAccumulator / _performanceSampleCount;
                    float currentFPS = 1f / averageFrameTime;
                    
                    // Store sample in history for trend analysis
                    _frameTimeHistory[_frameTimeIndex] = averageFrameTime;
                    _frameTimeIndex = (_frameTimeIndex + 1) % _frameTimeHistory.Length;
                    
                    // Check if we need to adjust performance settings
                    CheckPerformanceThresholds(currentFPS);
                    
                    // Reset accumulators
                    _performanceSampleAccumulator = 0f;
                    _performanceSampleCount = 0;
                }
            }
        }
        
        /// <summary>
        /// Check performance thresholds and trigger optimizations with hysteresis
        /// </summary>
        private void CheckPerformanceThresholds(float currentFPS)
        {
            // Use hysteresis to prevent rapid toggling of optimizations
            const float HYSTERESIS_MARGIN = 5f;
            
            if (!_performanceOptimizationsActive)
            {
                // Enable optimizations if FPS drops below threshold
                if (currentFPS < _lowFrameRateThreshold)
                {
                    // Check if this is a consistent trend, not a spike
                    if (IsConsistentLowPerformance())
                    {
                        EnablePerformanceOptimizations();
                    }
                }
            }
            else
            {
                // Disable optimizations if FPS is consistently good
                float disableThreshold = _targetFrameRate * 0.9f;
                if (currentFPS > disableThreshold + HYSTERESIS_MARGIN)
                {
                    if (IsConsistentGoodPerformance())
                    {
                        DisablePerformanceOptimizations();
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if low performance is consistent over recent samples
        /// </summary>
        private bool IsConsistentLowPerformance()
        {
            int lowFrameCount = 0;
            int totalSamples = 0;
            
            for (int i = 0; i < _frameTimeHistory.Length; i++)
            {
                if (_frameTimeHistory[i] > 0f) // Valid sample
                {
                    totalSamples++;
                    float fps = 1f / _frameTimeHistory[i];
                    if (fps < _lowFrameRateThreshold)
                    {
                        lowFrameCount++;
                    }
                }
            }
            
            // Require at least 70% of recent samples to be low performance
            return totalSamples > 0 && (float)lowFrameCount / totalSamples >= 0.7f;
        }
        
        /// <summary>
        /// Check if good performance is consistent over recent samples
        /// </summary>
        private bool IsConsistentGoodPerformance()
        {
            int goodFrameCount = 0;
            int totalSamples = 0;
            float goodThreshold = _targetFrameRate * 0.8f;
            
            for (int i = 0; i < _frameTimeHistory.Length; i++)
            {
                if (_frameTimeHistory[i] > 0f) // Valid sample
                {
                    totalSamples++;
                    float fps = 1f / _frameTimeHistory[i];
                    if (fps > goodThreshold)
                    {
                        goodFrameCount++;
                    }
                }
            }
            
            // Require at least 80% of recent samples to be good performance
            return totalSamples > 0 && (float)goodFrameCount / totalSamples >= 0.8f;
        }

        /// <summary>
        /// Enable performance optimizations when frame rate drops
        /// </summary>
        private void EnablePerformanceOptimizations()
        {
            _performanceOptimizationsActive = true;
            
            // Reduce visual quality
            QualitySettings.SetQualityLevel(0); // Fastest quality setting
            
            // Reduce particle effects
            if (_courseGenerator != null)
                _courseGenerator.ReduceParticleEffects();
                
            // Reduce bot visual complexity
            if (_activeBotController != null)
                _activeBotController.EnablePerformanceMode();
            
            Debug.Log("[GameManager] Performance optimizations enabled");
        }

        /// <summary>
        /// Disable performance optimizations when frame rate recovers
        /// </summary>
        private void DisablePerformanceOptimizations()
        {
            _performanceOptimizationsActive = false;
            
            // Restore visual quality
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1);
            
            // Restore visual effects
            if (_courseGenerator != null)
                _courseGenerator.RestoreParticleEffects();
                
            if (_activeBotController != null)
                _activeBotController.DisablePerformanceMode();
            
            Debug.Log("[GameManager] Performance optimizations disabled");
        }
        #endregion

        #region Pause/Resume Handling
        /// <summary>
        /// Handle application pause (minimize/background)
        /// </summary>
        private void HandleApplicationPause()
        {
            if (CurrentState == GameState.Running)
            {
                TransitionToState(GameState.Paused);
            }
            
            // Save current progress
            _resourceManager?.SaveGameData();
            
            Debug.Log("[GameManager] Application paused, game state saved");
        }

        /// <summary>
        /// Handle application resume (restore from background)
        /// </summary>
        private void HandleApplicationResume()
        {
            // Check if significant time has passed (could regenerate energy)
            if (_resourceManager != null)
            {
                _resourceManager.ProcessOfflineTime();
            }
            
            Debug.Log("[GameManager] Application resumed");
        }

        /// <summary>
        /// Handle input during pause state
        /// </summary>
        private void HandlePauseInput()
        {
            // This would typically be handled by UI system
            // For now, just check for common resume conditions
            
            if (Input.touchCount > 0 || Input.anyKeyDown)
            {
                // Resume game if there was user input
                TransitionToState(GameState.Running);
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Start a new bot run (called from UI)
        /// </summary>
        public void StartNewRun()
        {
            if (CanStartRun)
            {
                TransitionToState(GameState.PreRun);
            }
            else
            {
                Debug.LogWarning("[GameManager] Cannot start new run - energy depleted or at run limit");
                // Show energy purchase or wait message
            }
        }

        /// <summary>
        /// Return to main menu (called from UI)
        /// </summary>
        public void ReturnToMainMenu()
        {
            TransitionToState(GameState.MainMenu);
        }

        /// <summary>
        /// Open bot builder (called from UI)
        /// </summary>
        public void OpenBotBuilder()
        {
            TransitionToState(GameState.BotBuilding);
        }

        /// <summary>
        /// Pause current run (called from UI)
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Running)
            {
                TransitionToState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume paused run (called from UI)
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                TransitionToState(GameState.Running);
            }
        }

        /// <summary>
        /// Get current session statistics for UI display
        /// </summary>
        public SessionStatistics GetSessionStatistics()
        {
            return new SessionStatistics
            {
                SessionDuration = Time.time - _sessionStartTime,
                RunsCompleted = _currentRunCount,
                CurrentState = CurrentState.ToString(),
                CanStartRun = CanStartRun,
                PerformanceOptimizationsActive = _performanceOptimizationsActive
            };
        }
        #endregion

        #region Data Structures
        /// <summary>
        /// Session statistics for UI and analytics
        /// </summary>
        [System.Serializable]
        public class SessionStatistics
        {
            public float SessionDuration;
            public int RunsCompleted;
            public string CurrentState;
            public bool CanStartRun;
            public bool PerformanceOptimizationsActive;
        }
        #endregion
    }
}