using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CircuitRunners.Data
{
    /// <summary>
    /// Centralized resource and progression management system for Circuit Runners.
    /// Handles all currencies, player progression, save/load operations, and energy system.
    /// 
    /// Key Features:
    /// - Multi-currency system (Scrap, Data Cores, Premium currency)
    /// - Player progression with XP and levels
    /// - Energy system for run limiting
    /// - Persistent data storage with encryption
    /// - Achievement and unlock tracking
    /// - Daily bonus and event management
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region Currency System
        [Header("Currency")]
        [SerializeField] private int _currentScrap = 0;
        [SerializeField] private int _currentDataCores = 0;
        [SerializeField] private int _currentPremiumCurrency = 0;
        [SerializeField] private float _scrapMultiplier = 1f;
        [SerializeField] private float _xpMultiplier = 1f;
        
        /// <summary>
        /// Current scrap amount (primary free currency)
        /// </summary>
        public int CurrentScrap => _currentScrap;
        
        /// <summary>
        /// Current data cores (premium free currency earned through gameplay)
        /// </summary>
        public int CurrentDataCores => _currentDataCores;
        
        /// <summary>
        /// Current premium currency (purchased with real money)
        /// </summary>
        public int CurrentPremiumCurrency => _currentPremiumCurrency;
        
        /// <summary>
        /// Current scrap earning multiplier
        /// </summary>
        public float ScrapMultiplier => _scrapMultiplier;
        
        /// <summary>
        /// Current XP earning multiplier
        /// </summary>
        public float XPMultiplier => _xpMultiplier;
        #endregion

        #region Player Progression
        [Header("Player Progression")]
        [SerializeField] private int _playerLevel = 1;
        [SerializeField] private int _currentXP = 0;
        [SerializeField] private int _totalXPEarned = 0;
        [SerializeField] private int[] _levelXPRequirements;
        
        /// <summary>
        /// Current player level
        /// </summary>
        public int PlayerLevel => _playerLevel;
        
        /// <summary>
        /// Current XP in current level
        /// </summary>
        public int CurrentXP => _currentXP;
        
        /// <summary>
        /// Total XP earned across all time
        /// </summary>
        public int TotalXPEarned => _totalXPEarned;
        
        /// <summary>
        /// XP required for next level
        /// </summary>
        public int XPRequiredForNextLevel => GetXPRequirementForLevel(_playerLevel + 1) - _totalXPEarned;
        
        /// <summary>
        /// Progress to next level (0-1)
        /// </summary>
        public float ProgressToNextLevel
        {
            get
            {
                int currentLevelXP = GetXPRequirementForLevel(_playerLevel);
                int nextLevelXP = GetXPRequirementForLevel(_playerLevel + 1);
                return (float)(_totalXPEarned - currentLevelXP) / (nextLevelXP - currentLevelXP);
            }
        }
        #endregion

        #region Energy System
        [Header("Energy System")]
        [SerializeField] private int _currentEnergy = 5;
        [SerializeField] private int _maxEnergy = 5;
        [SerializeField] private float _energyRegenTime = 20f * 60f; // 20 minutes in seconds
        [SerializeField] private DateTime _lastEnergyRegenTime;
        [SerializeField] private bool _hasUnlimitedEnergy = false;
        
        /// <summary>
        /// Current energy amount
        /// </summary>
        public int CurrentEnergy => _hasUnlimitedEnergy ? _maxEnergy : _currentEnergy;
        
        /// <summary>
        /// Maximum energy capacity
        /// </summary>
        public int MaxEnergy => _maxEnergy;
        
        /// <summary>
        /// Whether player has unlimited energy (premium benefit)
        /// </summary>
        public bool HasUnlimitedEnergy => _hasUnlimitedEnergy;
        
        /// <summary>
        /// Time until next energy regeneration
        /// </summary>
        public float TimeUntilNextEnergy
        {
            get
            {
                if (_hasUnlimitedEnergy || _currentEnergy >= _maxEnergy) return 0f;
                
                float timePassed = (float)(DateTime.Now - _lastEnergyRegenTime).TotalSeconds;
                return Mathf.Max(0f, _energyRegenTime - timePassed);
            }
        }
        #endregion

        #region Premium Features
        [Header("Premium Features")]
        [SerializeField] private bool _hasPremiumAccount = false;
        [SerializeField] private DateTime _premiumExpiryDate;
        [SerializeField] private bool _hasAdRemoval = false;
        [SerializeField] private bool _hasDailyBonus = false;
        [SerializeField] private float _currentEventMultiplier = 1f;
        
        /// <summary>
        /// Whether player has an active premium account
        /// </summary>
        public bool HasPremiumAccount => _hasPremiumAccount && DateTime.Now < _premiumExpiryDate;
        
        /// <summary>
        /// Whether player has purchased ad removal
        /// </summary>
        public bool HasAdRemoval => _hasAdRemoval;
        
        /// <summary>
        /// Whether daily bonus is active
        /// </summary>
        public bool HasDailyBonus => _hasDailyBonus;
        #endregion

        #region Statistics & Achievements
        [Header("Statistics")]
        [SerializeField] private PlayerStatistics _playerStats;
        [SerializeField] private List<string> _unlockedAchievements = new List<string>();
        [SerializeField] private Dictionary<string, bool> _unlockedFeatures = new Dictionary<string, bool>();
        
        /// <summary>
        /// Player statistics for achievements and analytics
        /// </summary>
        public PlayerStatistics PlayerStats => _playerStats;
        
        /// <summary>
        /// List of unlocked achievement IDs
        /// </summary>
        public List<string> UnlockedAchievements => _unlockedAchievements;
        #endregion

        #region Save System
        [Header("Save System")]
        [SerializeField] private string _saveFileName = "circuit_runners_save.dat";
        [SerializeField] private bool _enableEncryption = true;
        [SerializeField] private bool _enableCloudSync = false;
        [SerializeField] private float _autoSaveInterval = 30f;
        
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, _saveFileName);
        private float _lastSaveTime = 0f;
        #endregion

        #region Events
        /// <summary>
        /// Fired when any currency changes
        /// </summary>
        public event Action<CurrencyType, int, int> OnCurrencyChanged; // type, old, new
        
        /// <summary>
        /// Fired when player levels up
        /// </summary>
        public event Action<int, int> OnPlayerLevelUp; // old level, new level
        
        /// <summary>
        /// Fired when XP is gained
        /// </summary>
        public event Action<int, int> OnXPGained; // amount gained, new total
        
        /// <summary>
        /// Fired when energy changes
        /// </summary>
        public event Action<int, int> OnEnergyChanged; // old energy, new energy
        
        /// <summary>
        /// Fired when an achievement is unlocked
        /// </summary>
        public event Action<string> OnAchievementUnlocked; // achievement ID
        
        /// <summary>
        /// Fired when premium status changes
        /// </summary>
        public event Action<bool> OnPremiumStatusChanged; // has premium
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Initialize default values
            InitializeDefaults();
            
            // Load saved data
            LoadGameData();
            
            // Start energy regeneration
            StartEnergyRegeneration();
        }

        private void Start()
        {
            // Process any offline time
            ProcessOfflineTime();
            
            // Start auto-save coroutine
            InvokeRepeating(nameof(AutoSave), _autoSaveInterval, _autoSaveInterval);
            
            Debug.Log("[ResourceManager] Resource manager initialized");
            Debug.Log($"[ResourceManager] Player Level: {_playerLevel}, Scrap: {_currentScrap}, Energy: {_currentEnergy}/{_maxEnergy}");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGameData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveGameData();
            }
        }

        private void OnDestroy()
        {
            SaveGameData();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize default values for new players
        /// </summary>
        private void InitializeDefaults()
        {
            if (_levelXPRequirements == null || _levelXPRequirements.Length == 0)
            {
                GenerateXPRequirements();
            }
            
            if (_playerStats == null)
            {
                _playerStats = new PlayerStatistics();
            }
            
            // Set initial energy regeneration time
            if (_lastEnergyRegenTime == default(DateTime))
            {
                _lastEnergyRegenTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Generate XP requirements for each level
        /// </summary>
        private void GenerateXPRequirements()
        {
            const int maxLevel = 100;
            _levelXPRequirements = new int[maxLevel + 1];
            
            _levelXPRequirements[0] = 0;
            _levelXPRequirements[1] = 0;
            
            for (int level = 2; level <= maxLevel; level++)
            {
                // Exponential XP curve: base cost increases with level
                int baseCost = 100;
                float multiplier = 1.15f + (level * 0.01f); // Slightly increasing difficulty
                int levelCost = Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, level - 2));
                
                _levelXPRequirements[level] = _levelXPRequirements[level - 1] + levelCost;
            }
        }
        #endregion

        #region Currency Management
        /// <summary>
        /// Add scrap with multipliers and events
        /// </summary>
        /// <param name="amount">Base amount to add</param>
        public void AddScrap(int amount)
        {
            int finalAmount = Mathf.RoundToInt(amount * _scrapMultiplier);
            int oldScrap = _currentScrap;
            _currentScrap += finalAmount;
            
            // Update statistics
            _playerStats.TotalScrapEarned += finalAmount;
            _playerStats.LastPlayTime = DateTime.Now;
            
            OnCurrencyChanged?.Invoke(CurrencyType.Scrap, oldScrap, _currentScrap);
            
            Debug.Log($"[ResourceManager] Added {finalAmount} scrap (base: {amount}, multiplier: {_scrapMultiplier:F2})");
        }

        /// <summary>
        /// Spend scrap if available
        /// </summary>
        /// <param name="amount">Amount to spend</param>
        /// <returns>True if transaction successful</returns>
        public bool SpendScrap(int amount)
        {
            if (_currentScrap < amount)
            {
                Debug.LogWarning($"[ResourceManager] Insufficient scrap: need {amount}, have {_currentScrap}");
                return false;
            }
            
            int oldScrap = _currentScrap;
            _currentScrap -= amount;
            _playerStats.TotalScrapSpent += amount;
            
            OnCurrencyChanged?.Invoke(CurrencyType.Scrap, oldScrap, _currentScrap);
            
            Debug.Log($"[ResourceManager] Spent {amount} scrap, remaining: {_currentScrap}");
            return true;
        }

        /// <summary>
        /// Add data cores (premium free currency)
        /// </summary>
        public void AddDataCores(int amount)
        {
            int oldCores = _currentDataCores;
            _currentDataCores += amount;
            _playerStats.TotalDataCoresEarned += amount;
            
            OnCurrencyChanged?.Invoke(CurrencyType.DataCores, oldCores, _currentDataCores);
            
            Debug.Log($"[ResourceManager] Added {amount} data cores, total: {_currentDataCores}");
        }

        /// <summary>
        /// Spend data cores if available
        /// </summary>
        public bool SpendDataCores(int amount)
        {
            if (_currentDataCores < amount)
            {
                Debug.LogWarning($"[ResourceManager] Insufficient data cores: need {amount}, have {_currentDataCores}");
                return false;
            }
            
            int oldCores = _currentDataCores;
            _currentDataCores -= amount;
            _playerStats.TotalDataCoresSpent += amount;
            
            OnCurrencyChanged?.Invoke(CurrencyType.DataCores, oldCores, _currentDataCores);
            return true;
        }

        /// <summary>
        /// Add premium currency (purchased with real money)
        /// </summary>
        public void AddPremiumCurrency(int amount)
        {
            int oldCurrency = _currentPremiumCurrency;
            _currentPremiumCurrency += amount;
            _playerStats.TotalPremiumCurrencyPurchased += amount;
            
            OnCurrencyChanged?.Invoke(CurrencyType.Premium, oldCurrency, _currentPremiumCurrency);
            
            Debug.Log($"[ResourceManager] Added {amount} premium currency, total: {_currentPremiumCurrency}");
        }

        /// <summary>
        /// Spend premium currency if available
        /// </summary>
        public bool SpendPremiumCurrency(int amount)
        {
            if (_currentPremiumCurrency < amount)
            {
                Debug.LogWarning($"[ResourceManager] Insufficient premium currency: need {amount}, have {_currentPremiumCurrency}");
                return false;
            }
            
            int oldCurrency = _currentPremiumCurrency;
            _currentPremiumCurrency -= amount;
            _playerStats.TotalPremiumCurrencySpent += amount;
            
            OnCurrencyChanged?.Invoke(CurrencyType.Premium, oldCurrency, _currentPremiumCurrency);
            return true;
        }

        /// <summary>
        /// Check if player has enough of a specific currency
        /// </summary>
        public bool HasEnoughCurrency(CurrencyType currencyType, int amount)
        {
            switch (currencyType)
            {
                case CurrencyType.Scrap:
                    return _currentScrap >= amount;
                case CurrencyType.DataCores:
                    return _currentDataCores >= amount;
                case CurrencyType.Premium:
                    return _currentPremiumCurrency >= amount;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get current amount of specific currency
        /// </summary>
        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Scrap:
                    return _currentScrap;
                case CurrencyType.DataCores:
                    return _currentDataCores;
                case CurrencyType.Premium:
                    return _currentPremiumCurrency;
                default:
                    return 0;
            }
        }
        #endregion

        #region XP and Leveling
        /// <summary>
        /// Add XP with multipliers and check for level up
        /// </summary>
        public void AddXP(int amount)
        {
            int finalAmount = Mathf.RoundToInt(amount * _xpMultiplier);
            int oldXP = _totalXPEarned;
            int oldLevel = _playerLevel;
            
            _totalXPEarned += finalAmount;
            _playerStats.TotalXPEarned += finalAmount;
            
            // Check for level up
            while (_totalXPEarned >= GetXPRequirementForLevel(_playerLevel + 1) && _playerLevel < _levelXPRequirements.Length - 1)
            {
                _playerLevel++;
                OnLevelUp(oldLevel, _playerLevel);
                oldLevel = _playerLevel;
            }
            
            // Update current level XP
            _currentXP = _totalXPEarned - GetXPRequirementForLevel(_playerLevel);
            
            OnXPGained?.Invoke(finalAmount, _totalXPEarned);
            
            Debug.Log($"[ResourceManager] Added {finalAmount} XP (base: {amount}, multiplier: {_xpMultiplier:F2})");
        }

        /// <summary>
        /// Handle level up rewards and notifications
        /// </summary>
        private void OnLevelUp(int oldLevel, int newLevel)
        {
            Debug.Log($"[ResourceManager] Level up! {oldLevel} -> {newLevel}");
            
            // Level up rewards
            int scrapReward = newLevel * 50;
            int coreReward = newLevel % 5 == 0 ? 1 : 0; // Data core every 5 levels
            
            _currentScrap += scrapReward;
            _currentDataCores += coreReward;
            
            // Increase max energy every 10 levels
            if (newLevel % 10 == 0 && _maxEnergy < 10)
            {
                _maxEnergy++;
                _currentEnergy = _maxEnergy; // Full refill on energy increase
            }
            
            // Check for feature unlocks
            CheckFeatureUnlocks(newLevel);
            
            // Fire event
            OnPlayerLevelUp?.Invoke(oldLevel, newLevel);
            
            Debug.Log($"[ResourceManager] Level {newLevel} rewards: {scrapReward} scrap, {coreReward} data cores");
        }

        /// <summary>
        /// Get XP requirement for specific level
        /// </summary>
        private int GetXPRequirementForLevel(int level)
        {
            if (level <= 0 || level >= _levelXPRequirements.Length)
                return int.MaxValue;
                
            return _levelXPRequirements[level];
        }

        /// <summary>
        /// Check for feature unlocks at new level
        /// </summary>
        private void CheckFeatureUnlocks(int level)
        {
            // Example feature unlocks
            if (level == 5)
            {
                UnlockFeature("BotCustomization");
            }
            
            if (level == 10)
            {
                UnlockFeature("DailyBonuses");
            }
            
            if (level == 15)
            {
                UnlockFeature("CourseThemes");
            }
        }

        /// <summary>
        /// Unlock a game feature
        /// </summary>
        private void UnlockFeature(string featureId)
        {
            if (!_unlockedFeatures.ContainsKey(featureId))
            {
                _unlockedFeatures[featureId] = true;
                Debug.Log($"[ResourceManager] Feature unlocked: {featureId}");
            }
        }

        /// <summary>
        /// Check if a feature is unlocked
        /// </summary>
        public bool IsFeatureUnlocked(string featureId)
        {
            return _unlockedFeatures.ContainsKey(featureId) && _unlockedFeatures[featureId];
        }
        #endregion

        #region Energy System
        /// <summary>
        /// Start the energy regeneration system
        /// </summary>
        private void StartEnergyRegeneration()
        {
            InvokeRepeating(nameof(RegenerateEnergy), 1f, 1f); // Check every second
        }

        /// <summary>
        /// Regenerate energy over time
        /// </summary>
        private void RegenerateEnergy()
        {
            if (_hasUnlimitedEnergy || _currentEnergy >= _maxEnergy) return;
            
            float timePassed = (float)(DateTime.Now - _lastEnergyRegenTime).TotalSeconds;
            
            if (timePassed >= _energyRegenTime)
            {
                int oldEnergy = _currentEnergy;
                _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + 1);
                _lastEnergyRegenTime = DateTime.Now;
                
                OnEnergyChanged?.Invoke(oldEnergy, _currentEnergy);
                
                Debug.Log($"[ResourceManager] Energy regenerated: {_currentEnergy}/{_maxEnergy}");
            }
        }

        /// <summary>
        /// Consume energy for a run
        /// </summary>
        public bool ConsumeEnergy(int amount = 1)
        {
            if (_hasUnlimitedEnergy) return true;
            
            if (_currentEnergy < amount)
            {
                Debug.LogWarning($"[ResourceManager] Insufficient energy: need {amount}, have {_currentEnergy}");
                return false;
            }
            
            int oldEnergy = _currentEnergy;
            _currentEnergy -= amount;
            
            OnEnergyChanged?.Invoke(oldEnergy, _currentEnergy);
            
            Debug.Log($"[ResourceManager] Energy consumed: {amount}, remaining: {_currentEnergy}");
            return true;
        }

        /// <summary>
        /// Check if player has enough energy
        /// </summary>
        public bool HasEnoughEnergy(int amount = 1)
        {
            return _hasUnlimitedEnergy || _currentEnergy >= amount;
        }

        /// <summary>
        /// Refill energy to full (from purchase or ad)
        /// </summary>
        public void RefillEnergy()
        {
            int oldEnergy = _currentEnergy;
            _currentEnergy = _maxEnergy;
            _lastEnergyRegenTime = DateTime.Now;
            
            OnEnergyChanged?.Invoke(oldEnergy, _currentEnergy);
            
            Debug.Log("[ResourceManager] Energy refilled to full");
        }

        /// <summary>
        /// Process offline time for energy regeneration
        /// </summary>
        public void ProcessOfflineTime()
        {
            if (_hasUnlimitedEnergy) return;
            
            TimeSpan offlineTime = DateTime.Now - _lastEnergyRegenTime;
            int energyToRegenerate = Mathf.FloorToInt((float)offlineTime.TotalSeconds / _energyRegenTime);
            
            if (energyToRegenerate > 0)
            {
                int oldEnergy = _currentEnergy;
                _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + energyToRegenerate);
                _lastEnergyRegenTime = DateTime.Now.AddSeconds(-(offlineTime.TotalSeconds % _energyRegenTime));
                
                OnEnergyChanged?.Invoke(oldEnergy, _currentEnergy);
                
                Debug.Log($"[ResourceManager] Offline energy regeneration: +{energyToRegenerate} energy");
            }
        }
        #endregion

        #region Premium Features
        /// <summary>
        /// Grant premium account for specified duration
        /// </summary>
        public void GrantPremiumAccount(TimeSpan duration)
        {
            DateTime newExpiry = DateTime.Now.Add(duration);
            
            if (!_hasPremiumAccount || newExpiry > _premiumExpiryDate)
            {
                _hasPremiumAccount = true;
                _premiumExpiryDate = newExpiry;
                
                // Apply premium benefits
                _scrapMultiplier = 2f;
                _xpMultiplier = 1.5f;
                _hasUnlimitedEnergy = true;
                
                OnPremiumStatusChanged?.Invoke(true);
                
                Debug.Log($"[ResourceManager] Premium account granted until {_premiumExpiryDate}");
            }
        }

        /// <summary>
        /// Grant permanent ad removal
        /// </summary>
        public void GrantAdRemoval()
        {
            _hasAdRemoval = true;
            Debug.Log("[ResourceManager] Ad removal granted");
        }

        /// <summary>
        /// Get current event multiplier for reward type
        /// </summary>
        public float GetCurrentEventMultiplier(string rewardType)
        {
            // This would be expanded to handle different event types
            return _currentEventMultiplier;
        }

        /// <summary>
        /// Set event multiplier (for limited-time events)
        /// </summary>
        public void SetEventMultiplier(float multiplier, TimeSpan duration)
        {
            _currentEventMultiplier = multiplier;
            
            // Schedule multiplier reset
            Invoke(nameof(ResetEventMultiplier), (float)duration.TotalSeconds);
            
            Debug.Log($"[ResourceManager] Event multiplier set to {multiplier:F2}x for {duration.TotalHours:F1} hours");
        }

        private void ResetEventMultiplier()
        {
            _currentEventMultiplier = 1f;
            Debug.Log("[ResourceManager] Event multiplier reset to 1.0x");
        }
        #endregion

        #region Bot Configuration Management
        /// <summary>
        /// Save a bot configuration
        /// </summary>
        public void SaveBotConfiguration(Bot.BotData botConfig)
        {
            // In a real implementation, this would save to persistent storage
            // For now, just update the statistics
            if (botConfig != null)
            {
                _playerStats.LastPlayTime = DateTime.Now;
                Debug.Log($"[ResourceManager] Bot configuration saved: {botConfig.GetDisplayName()}");
            }
        }

        /// <summary>
        /// Load the player's current bot configuration
        /// </summary>
        public Bot.BotData LoadBotConfiguration()
        {
            // In a real implementation, this would load from persistent storage
            // For now, return a default configuration
            return null;
        }
        #endregion

        #region Achievement System
        /// <summary>
        /// Unlock an achievement
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!_unlockedAchievements.Contains(achievementId))
            {
                _unlockedAchievements.Add(achievementId);
                OnAchievementUnlocked?.Invoke(achievementId);
                
                // Award achievement rewards
                AwardAchievementRewards(achievementId);
                
                Debug.Log($"[ResourceManager] Achievement unlocked: {achievementId}");
            }
        }

        /// <summary>
        /// Check if achievement is unlocked
        /// </summary>
        public bool IsAchievementUnlocked(string achievementId)
        {
            return _unlockedAchievements.Contains(achievementId);
        }

        /// <summary>
        /// Award rewards for achievement completion
        /// </summary>
        private void AwardAchievementRewards(string achievementId)
        {
            // Example achievement rewards
            switch (achievementId.ToLower())
            {
                case "first_run":
                    AddScrap(100);
                    break;
                case "distance_runner":
                    AddDataCores(1);
                    break;
                case "collector":
                    AddScrap(500);
                    break;
                case "perfect_run":
                    AddDataCores(2);
                    AddPremiumCurrency(10);
                    break;
            }
        }
        #endregion

        #region Save/Load System
        /// <summary>
        /// Auto-save game data
        /// </summary>
        private void AutoSave()
        {
            if (Time.time - _lastSaveTime >= _autoSaveInterval)
            {
                SaveGameData();
            }
        }

        /// <summary>
        /// Save all game data to persistent storage with comprehensive error handling
        /// </summary>
        public void SaveGameData()
        {
            const int maxRetryAttempts = 3;
            int retryCount = 0;
            
            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    // Validate save directory exists
                    string saveDirectory = Path.GetDirectoryName(SaveFilePath);
                    if (!Directory.Exists(saveDirectory))
                    {
                        Debug.LogWarning($"[ResourceManager] Save directory doesn't exist, creating: {saveDirectory}");
                        Directory.CreateDirectory(saveDirectory);
                    }
                    
                    // Create save data with null-safety
                    var saveData = CreateSaveData();
                    
                    if (saveData == null)
                    {
                        throw new Exception("Failed to create save data object");
                    }

                    // Serialize to JSON with validation
                    string json = JsonUtility.ToJson(saveData, true);
                    
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception("Serialization resulted in empty JSON");
                    }
                    
                    // Apply encryption if enabled
                    if (_enableEncryption)
                    {
                        json = EncryptSaveData(json);
                        
                        if (string.IsNullOrEmpty(json))
                        {
                            throw new Exception("Encryption failed - resulted in empty data");
                        }
                    }

                    // Create backup of existing save file
                    CreateSaveBackup();
                    
                    // Write to temporary file first, then rename (atomic operation)
                    string tempFilePath = SaveFilePath + ".tmp";
                    File.WriteAllText(tempFilePath, json);
                    
                    // Verify the temporary file was written correctly
                    if (!File.Exists(tempFilePath) || new FileInfo(tempFilePath).Length == 0)
                    {
                        throw new Exception("Temporary save file is missing or empty");
                    }
                    
                    // Atomic rename to final save file
                    if (File.Exists(SaveFilePath))
                    {
                        File.Delete(SaveFilePath);
                    }
                    File.Move(tempFilePath, SaveFilePath);
                    
                    _lastSaveTime = Time.time;

                    Debug.Log($"[ResourceManager] Game data saved successfully to {SaveFilePath} (Size: {new FileInfo(SaveFilePath).Length} bytes)");
                    return; // Success - exit retry loop
                }
                catch (Exception e)
                {
                    retryCount++;
                    Debug.LogError($"[ResourceManager] Save attempt {retryCount} failed: {e.Message}");
                    
                    if (retryCount < maxRetryAttempts)
                    {
                        Debug.LogWarning($"[ResourceManager] Retrying save operation... ({retryCount}/{maxRetryAttempts})");
                        
                        // Wait briefly before retry
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        Debug.LogError($"[ResourceManager] All save attempts failed. Data may be lost: {e.Message}\n{e.StackTrace}");
                        
                        // Try to restore from backup if available
                        if (TryRestoreFromBackup())
                        {
                            Debug.LogWarning("[ResourceManager] Restored from backup after save failure");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Create save data object with null-safety validation
        /// </summary>
        private SaveData CreateSaveData()
        {
            try
            {
                return new SaveData
                {
                    // Currency (with validation)
                    CurrentScrap = Mathf.Max(0, _currentScrap),
                    CurrentDataCores = Mathf.Max(0, _currentDataCores),
                    CurrentPremiumCurrency = Mathf.Max(0, _currentPremiumCurrency),
                    
                    // Progression (with validation)
                    PlayerLevel = Mathf.Max(1, _playerLevel),
                    TotalXPEarned = Mathf.Max(0, _totalXPEarned),
                    
                    // Energy (with validation)
                    CurrentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy),
                    MaxEnergy = Mathf.Max(1, _maxEnergy),
                    LastEnergyRegenTime = _lastEnergyRegenTime != default(DateTime) ? _lastEnergyRegenTime : DateTime.Now,
                    
                    // Premium
                    HasPremiumAccount = _hasPremiumAccount,
                    PremiumExpiryDate = _premiumExpiryDate,
                    HasAdRemoval = _hasAdRemoval,
                    
                    // Statistics (with null-safety)
                    PlayerStats = _playerStats ?? new PlayerStatistics(),
                    UnlockedAchievements = _unlockedAchievements ?? new List<string>(),
                    UnlockedFeatures = _unlockedFeatures ?? new Dictionary<string, bool>(),
                    
                    // Metadata
                    SaveVersion = 1,
                    SaveTime = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Failed to create save data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Create backup of existing save file
        /// </summary>
        private void CreateSaveBackup()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string backupPath = SaveFilePath + ".bak";
                    File.Copy(SaveFilePath, backupPath, true);
                    Debug.Log($"[ResourceManager] Save backup created: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ResourceManager] Failed to create save backup: {ex.Message}");
                // Don't fail the save operation for backup issues
            }
        }
        
        /// <summary>
        /// Try to restore from backup file if main save is corrupted
        /// </summary>
        private bool TryRestoreFromBackup()
        {
            try
            {
                string backupPath = SaveFilePath + ".bak";
                
                if (File.Exists(backupPath))
                {
                    Debug.LogWarning("[ResourceManager] Attempting to restore from backup...");
                    
                    // Validate backup file
                    if (new FileInfo(backupPath).Length > 0)
                    {
                        File.Copy(backupPath, SaveFilePath, true);
                        Debug.Log("[ResourceManager] Successfully restored from backup");
                        return true;
                    }
                    else
                    {
                        Debug.LogError("[ResourceManager] Backup file is empty");
                    }
                }
                else
                {
                    Debug.LogWarning("[ResourceManager] No backup file available");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Failed to restore from backup: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Load all game data from persistent storage with comprehensive error handling
        /// </summary>
        public void LoadGameData()
        {
            try
            {
                // Check if save file exists
                if (!File.Exists(SaveFilePath))
                {
                    Debug.Log("[ResourceManager] No save file found, checking for backup...");
                    
                    // Try to load from backup if available
                    if (TryLoadFromBackup())
                    {
                        return;
                    }
                    
                    Debug.Log("[ResourceManager] No save data found, using defaults");
                    InitializeDefaultData();
                    return;
                }
                
                // Validate save file
                FileInfo saveFileInfo = new FileInfo(SaveFilePath);
                if (saveFileInfo.Length == 0)
                {
                    Debug.LogWarning("[ResourceManager] Save file is empty, trying backup...");
                    
                    if (TryLoadFromBackup())
                    {
                        return;
                    }
                    
                    Debug.LogError("[ResourceManager] Save file and backup are both empty, using defaults");
                    InitializeDefaultData();
                    return;
                }

                // Read save file content
                string json = File.ReadAllText(SaveFilePath);
                
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("Save file content is empty");
                }
                
                // Decrypt if encryption is enabled
                if (_enableEncryption)
                {
                    json = DecryptSaveData(json);
                    
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception("Decryption resulted in empty data");
                    }
                }

                // Deserialize JSON data
                SaveData saveData = null;
                try
                {
                    saveData = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception jsonEx)
                {
                    Debug.LogError($"[ResourceManager] JSON deserialization failed: {jsonEx.Message}");
                    throw new Exception($"Save data corruption detected: {jsonEx.Message}");
                }
                
                if (saveData == null)
                {
                    throw new Exception("Deserialized save data is null");
                }
                
                // Validate save data integrity
                if (!ValidateSaveData(saveData))
                {
                    throw new Exception("Save data validation failed");
                }
                
                // Apply loaded data
                ApplySaveData(saveData);
                
                Debug.Log($"[ResourceManager] Game data loaded successfully (Version: {saveData.SaveVersion}, Size: {saveFileInfo.Length} bytes)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceManager] Failed to load game data: {e.Message}");
                
                // Try to recover from backup
                if (TryLoadFromBackup())
                {
                    Debug.LogWarning("[ResourceManager] Recovered game data from backup");
                    return;
                }
                
                // Fallback to default values
                Debug.LogWarning("[ResourceManager] Using default values due to load failure");
                InitializeDefaultData();
                
                // Save current defaults to prevent future load issues
                try
                {
                    SaveGameData();
                    Debug.Log("[ResourceManager] Default data saved successfully");
                }
                catch (Exception saveEx)
                {
                    Debug.LogError($"[ResourceManager] Failed to save default data: {saveEx.Message}");
                }
            }
        }
        
        /// <summary>
        /// Try to load game data from backup file
        /// </summary>
        private bool TryLoadFromBackup()
        {
            try
            {
                string backupPath = SaveFilePath + ".bak";
                
                if (!File.Exists(backupPath))
                {
                    Debug.Log("[ResourceManager] No backup file available");
                    return false;
                }
                
                FileInfo backupInfo = new FileInfo(backupPath);
                if (backupInfo.Length == 0)
                {
                    Debug.LogWarning("[ResourceManager] Backup file is empty");
                    return false;
                }
                
                Debug.LogWarning("[ResourceManager] Loading from backup file...");
                
                string json = File.ReadAllText(backupPath);
                
                if (_enableEncryption)
                {
                    json = DecryptSaveData(json);
                }
                
                var saveData = JsonUtility.FromJson<SaveData>(json);
                
                if (saveData != null && ValidateSaveData(saveData))
                {
                    ApplySaveData(saveData);
                    
                    // Copy backup to main save file
                    File.Copy(backupPath, SaveFilePath, true);
                    
                    Debug.Log("[ResourceManager] Successfully loaded from backup and restored main save file");
                    return true;
                }
                else
                {
                    Debug.LogError("[ResourceManager] Backup file is also corrupted");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Failed to load from backup: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate save data integrity and structure
        /// </summary>
        private bool ValidateSaveData(SaveData saveData)
        {
            try
            {
                // Check for basic data integrity
                if (saveData.PlayerLevel < 1 || saveData.PlayerLevel > 1000)
                {
                    Debug.LogError($"[ResourceManager] Invalid player level in save data: {saveData.PlayerLevel}");
                    return false;
                }
                
                if (saveData.CurrentScrap < 0 || saveData.CurrentDataCores < 0 || saveData.CurrentPremiumCurrency < 0)
                {
                    Debug.LogError("[ResourceManager] Invalid currency values in save data");
                    return false;
                }
                
                if (saveData.CurrentEnergy < 0 || saveData.MaxEnergy < 1)
                {
                    Debug.LogError("[ResourceManager] Invalid energy values in save data");
                    return false;
                }
                
                if (saveData.SaveVersion < 1)
                {
                    Debug.LogError($"[ResourceManager] Invalid save version: {saveData.SaveVersion}");
                    return false;
                }
                
                // Check for reasonable timestamp
                if (saveData.SaveTime > DateTime.Now.AddDays(1))
                {
                    Debug.LogWarning("[ResourceManager] Save timestamp is in the future - possible clock issue");
                    // Don't fail validation for this, just warn
                }
                
                Debug.Log("[ResourceManager] Save data validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Save data validation error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Initialize default data for new players or corrupted saves
        /// </summary>
        private void InitializeDefaultData()
        {
            try
            {
                Debug.Log("[ResourceManager] Initializing default player data...");
                
                // Reset all values to safe defaults
                _currentScrap = 100; // Give new players some starting currency
                _currentDataCores = 0;
                _currentPremiumCurrency = 0;
                _playerLevel = 1;
                _totalXPEarned = 0;
                _currentXP = 0;
                _currentEnergy = _maxEnergy;
                _hasPremiumAccount = false;
                _hasAdRemoval = false;
                _hasDailyBonus = false;
                _lastEnergyRegenTime = DateTime.Now;
                
                // Initialize collections with null-safety
                _playerStats = new PlayerStatistics();
                _unlockedAchievements = new List<string>();
                _unlockedFeatures = new Dictionary<string, bool>();
                
                // Generate default XP requirements if needed
                if (_levelXPRequirements == null || _levelXPRequirements.Length == 0)
                {
                    GenerateXPRequirements();
                }
                
                Debug.Log("[ResourceManager] Default data initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Failed to initialize default data: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply loaded save data to current state
        /// </summary>
        private void ApplySaveData(SaveData saveData)
        {
            // Currency
            _currentScrap = saveData.CurrentScrap;
            _currentDataCores = saveData.CurrentDataCores;
            _currentPremiumCurrency = saveData.CurrentPremiumCurrency;
            
            // Progression
            _playerLevel = saveData.PlayerLevel;
            _totalXPEarned = saveData.TotalXPEarned;
            _currentXP = _totalXPEarned - GetXPRequirementForLevel(_playerLevel);
            
            // Energy
            _currentEnergy = saveData.CurrentEnergy;
            _maxEnergy = saveData.MaxEnergy;
            _lastEnergyRegenTime = saveData.LastEnergyRegenTime;
            
            // Premium
            _hasPremiumAccount = saveData.HasPremiumAccount;
            _premiumExpiryDate = saveData.PremiumExpiryDate;
            _hasAdRemoval = saveData.HasAdRemoval;
            
            // Check premium status and apply benefits
            if (HasPremiumAccount)
            {
                _scrapMultiplier = 2f;
                _xpMultiplier = 1.5f;
                _hasUnlimitedEnergy = true;
            }
            
            // Statistics and unlocks
            _playerStats = saveData.PlayerStats ?? new PlayerStatistics();
            _unlockedAchievements = saveData.UnlockedAchievements ?? new List<string>();
            _unlockedFeatures = saveData.UnlockedFeatures ?? new Dictionary<string, bool>();
        }

        /// <summary>
        /// Enhanced encryption for save data with multiple layers of security
        /// Provides better protection against save file tampering and cheating
        /// </summary>
        private string EncryptSaveData(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    throw new ArgumentException("Data cannot be null or empty");
                }
                
                // Layer 1: Add integrity hash
                string dataWithHash = AddIntegrityHash(data);
                
                // Layer 2: XOR encryption with dynamic key
                string encryptedData = XOREncrypt(dataWithHash);
                
                // Layer 3: Base64 encoding
                byte[] encryptedBytes = System.Text.Encoding.UTF8.GetBytes(encryptedData);
                string base64Data = Convert.ToBase64String(encryptedBytes);
                
                // Layer 4: Add checksum for additional validation
                string finalData = AddChecksum(base64Data);
                
                Debug.Log($"[ResourceManager] Save data encrypted successfully (Original: {data.Length} bytes, Encrypted: {finalData.Length} bytes)");
                return finalData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Encryption failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// XOR encryption with dynamic key based on device and game data
        /// </summary>
        private string XOREncrypt(string data)
        {
            // Generate dynamic key based on device identifier and game constants
            string deviceKey = SystemInfo.deviceUniqueIdentifier ?? "DefaultDevice";
            string gameKey = "CircuitRunnersKey2024";
            string combinedKey = $"{gameKey}_{deviceKey}_{Application.version}";
            
            // Create SHA256 hash of combined key for consistent encryption
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(combinedKey);
                byte[] hashBytes = sha256.ComputeHash(keyBytes);
                string hashKey = Convert.ToBase64String(hashBytes);
                
                char[] chars = data.ToCharArray();
                
                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i] = (char)(chars[i] ^ hashKey[i % hashKey.Length]);
                }
                
                return new string(chars);
            }
        }
        
        /// <summary>
        /// Add integrity hash to detect data tampering
        /// </summary>
        private string AddIntegrityHash(string data)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = sha256.ComputeHash(dataBytes);
                string hash = Convert.ToBase64String(hashBytes);
                
                // Embed hash at the beginning of data
                return $"{hash}|{data}";
            }
        }
        
        /// <summary>
        /// Add simple checksum for basic validation
        /// </summary>
        private string AddChecksum(string data)
        {
            int checksum = 0;
            foreach (char c in data)
            {
                checksum += c;
            }
            
            return $"{data}#{checksum:X8}";
        }

        /// <summary>
        /// Enhanced decryption for save data with integrity validation
        /// </summary>
        private string DecryptSaveData(string encryptedData)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData))
                {
                    throw new ArgumentException("Encrypted data cannot be null or empty");
                }
                
                // Layer 1: Verify and remove checksum
                string dataWithoutChecksum = VerifyAndRemoveChecksum(encryptedData);
                
                // Layer 2: Base64 decode
                byte[] base64Bytes = Convert.FromBase64String(dataWithoutChecksum);
                string base64Data = System.Text.Encoding.UTF8.GetString(base64Bytes);
                
                // Layer 3: XOR decrypt
                string decryptedData = XORDecrypt(base64Data);
                
                // Layer 4: Verify and remove integrity hash
                string finalData = VerifyAndRemoveIntegrityHash(decryptedData);
                
                Debug.Log($"[ResourceManager] Save data decrypted successfully (Encrypted: {encryptedData.Length} bytes, Decrypted: {finalData.Length} bytes)");
                return finalData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResourceManager] Decryption failed: {ex.Message}");
                throw new Exception($"Save data decryption failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// XOR decryption (same as encryption due to XOR properties)
        /// </summary>
        private string XORDecrypt(string data)
        {
            return XOREncrypt(data); // XOR is symmetric
        }
        
        /// <summary>
        /// Verify and remove integrity hash
        /// </summary>
        private string VerifyAndRemoveIntegrityHash(string dataWithHash)
        {
            string[] parts = dataWithHash.Split('|');
            
            if (parts.Length != 2)
            {
                throw new Exception("Invalid hash format in save data");
            }
            
            string expectedHash = parts[0];
            string actualData = parts[1];
            
            // Verify hash
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(actualData);
                byte[] hashBytes = sha256.ComputeHash(dataBytes);
                string actualHash = Convert.ToBase64String(hashBytes);
                
                if (expectedHash != actualHash)
                {
                    throw new Exception("Save data integrity check failed - data may be corrupted or tampered");
                }
            }
            
            return actualData;
        }
        
        /// <summary>
        /// Verify and remove checksum
        /// </summary>
        private string VerifyAndRemoveChecksum(string dataWithChecksum)
        {
            int lastHashIndex = dataWithChecksum.LastIndexOf('#');
            
            if (lastHashIndex == -1 || lastHashIndex == dataWithChecksum.Length - 1)
            {
                throw new Exception("Invalid checksum format in save data");
            }
            
            string data = dataWithChecksum.Substring(0, lastHashIndex);
            string checksumStr = dataWithChecksum.Substring(lastHashIndex + 1);
            
            // Verify checksum
            int expectedChecksum = 0;
            foreach (char c in data)
            {
                expectedChecksum += c;
            }
            
            if (!int.TryParse(checksumStr, System.Globalization.NumberStyles.HexNumber, null, out int actualChecksum))
            {
                throw new Exception("Invalid checksum format");
            }
            
            if (expectedChecksum != actualChecksum)
            {
                throw new Exception("Save data checksum validation failed");
            }
            
            return data;
        }
        #endregion

        #region Public Utilities
        /// <summary>
        /// Get comprehensive player progress summary
        /// </summary>
        public PlayerProgressSummary GetProgressSummary()
        {
            return new PlayerProgressSummary
            {
                PlayerLevel = _playerLevel,
                CurrentXP = _currentXP,
                ProgressToNextLevel = ProgressToNextLevel,
                TotalScrap = _currentScrap,
                TotalDataCores = _currentDataCores,
                CurrentEnergy = CurrentEnergy,
                MaxEnergy = _maxEnergy,
                HasPremiumAccount = HasPremiumAccount,
                UnlockedAchievementCount = _unlockedAchievements.Count,
                TotalPlayTime = _playerStats.TotalPlayTime,
                RunsCompleted = _playerStats.TotalRunsCompleted
            };
        }

        /// <summary>
        /// Reset all progress (for testing or player request)
        /// </summary>
        public void ResetAllProgress()
        {
            _currentScrap = 0;
            _currentDataCores = 0;
            _currentPremiumCurrency = 0;
            _playerLevel = 1;
            _totalXPEarned = 0;
            _currentXP = 0;
            _currentEnergy = _maxEnergy;
            _playerStats = new PlayerStatistics();
            _unlockedAchievements.Clear();
            _unlockedFeatures.Clear();
            
            SaveGameData();
            
            Debug.Log("[ResourceManager] All progress reset");
        }
        #endregion
    }
}