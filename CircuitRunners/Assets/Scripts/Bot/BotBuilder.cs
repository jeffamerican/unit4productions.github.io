using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitRunners.Bot
{
    /// <summary>
    /// Bot builder system for modular bot customization and configuration.
    /// Handles the drag-and-drop interface, part compatibility validation,
    /// and visual preview of bot configurations.
    /// 
    /// Key Features:
    /// - Modular part system with compatibility checking
    /// - Real-time stat calculation and preview
    /// - Save/load bot configurations
    /// - Visual customization with color and style options
    /// - Mobile-optimized touch interface
    /// </summary>
    public class BotBuilder : MonoBehaviour
    {
        #region Builder Configuration
        [Header("Builder Settings")]
        [SerializeField] private BotData _currentBotConfiguration;
        [SerializeField] private List<BotPart> _availableParts = new List<BotPart>();
        [SerializeField] private List<BotData> _savedConfigurations = new List<BotData>();
        [SerializeField] private int _maxSavedConfigurations = 10;
        
        /// <summary>
        /// Current bot configuration being edited
        /// </summary>
        public BotData CurrentConfiguration => _currentBotConfiguration;
        
        /// <summary>
        /// All parts available to the player (unlocked parts only)
        /// </summary>
        public List<BotPart> AvailableParts => _availableParts;
        #endregion

        #region Part Slots System
        [Header("Part Slots")]
        [SerializeField] private BotPartSlot[] _partSlots = new BotPartSlot[6];
        [SerializeField] private int _maxPartsPerBot = 6;
        
        // Part slot types - each bot can have one of each type
        private readonly BotPartType[] _slotTypes = {
            BotPartType.Chassis,
            BotPartType.Processor,
            BotPartType.Sensors,
            BotPartType.Mobility,
            BotPartType.PowerCore,
            BotPartType.Special
        };
        
        /// <summary>
        /// Get equipped part in specific slot
        /// </summary>
        public BotPart GetEquippedPart(BotPartType slotType)
        {
            return _currentBotConfiguration.EquippedParts
                .FirstOrDefault(p => p != null && p.PartType == slotType);
        }
        
        /// <summary>
        /// Check if a slot is empty
        /// </summary>
        public bool IsSlotEmpty(BotPartType slotType)
        {
            return GetEquippedPart(slotType) == null;
        }
        #endregion

        #region UI References
        [Header("UI Components")]
        [SerializeField] private Transform _partsInventoryPanel;
        [SerializeField] private Transform _botPreviewPanel;
        [SerializeField] private Button _saveConfigurationButton;
        [SerializeField] private Button _loadConfigurationButton;
        [SerializeField] private Button _resetConfigurationButton;
        [SerializeField] private Button _randomizeButton;
        
        [Header("Stats Display")]
        [SerializeField] private Slider _speedStatBar;
        [SerializeField] private Slider _jumpStatBar;
        [SerializeField] private Slider _healthStatBar;
        [SerializeField] private Slider _defenseStatBar;
        [SerializeField] private Text _overallRatingText;
        
        [Header("Visual Customization")]
        [SerializeField] private ColorPicker _primaryColorPicker;
        [SerializeField] private ColorPicker _secondaryColorPicker;
        [SerializeField] private Dropdown _visualStyleDropdown;
        
        // Dynamic UI elements
        private List<BotPartUI> _partInventoryItems = new List<BotPartUI>();
        private BotPartUI _draggedPart = null;
        private Camera _uiCamera;
        #endregion

        #region Preview System
        [Header("Bot Preview")]
        [SerializeField] private Transform _botPreviewModel;
        [SerializeField] private Animator _previewAnimator;
        [SerializeField] private SpriteRenderer[] _previewSprites;
        [SerializeField] private float _previewRotationSpeed = 30f;
        
        private bool _previewRotating = true;
        private BotPartModifiers _currentTotalModifiers;
        #endregion

        #region Events
        /// <summary>
        /// Fired when bot configuration changes
        /// </summary>
        public event Action<BotData> OnConfigurationChanged;
        
        /// <summary>
        /// Fired when a part is equipped or unequipped
        /// </summary>
        public event Action<BotPart, bool> OnPartEquipped;
        
        /// <summary>
        /// Fired when bot stats are recalculated
        /// </summary>
        public event Action<BotPartModifiers> OnStatsUpdated;
        
        /// <summary>
        /// Fired when visual customization changes
        /// </summary>
        public event Action<Color, Color, int> OnVisualsChanged;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _uiCamera = Camera.main; // Will be replaced with UI camera reference
            
            // Initialize with default bot configuration if none exists
            if (_currentBotConfiguration == null)
            {
                CreateDefaultConfiguration();
            }
            
            // Initialize part slots
            InitializePartSlots();
        }

        private void Start()
        {
            // Setup UI event handlers
            SetupUIEventHandlers();
            
            // Load available parts from resources or data manager
            LoadAvailableParts();
            
            // Initialize inventory display
            RefreshPartsInventory();
            
            // Update preview and stats
            UpdateBotPreview();
            UpdateStatsDisplay();
            
            Debug.Log("[BotBuilder] Bot builder initialized with default configuration");
        }

        private void Update()
        {
            // Handle part dragging input
            HandlePartDragging();
            
            // Rotate preview model
            if (_previewRotating && _botPreviewModel != null)
            {
                _botPreviewModel.Rotate(Vector3.up, _previewRotationSpeed * Time.deltaTime);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize bot building session with current or default configuration
        /// </summary>
        public void InitializeBuildingSession()
        {
            // Load player's current bot configuration
            LoadPlayerConfiguration();
            
            // Refresh UI to match loaded configuration
            RefreshAllUI();
            
            Debug.Log("[BotBuilder] Building session initialized");
        }

        /// <summary>
        /// Create a default bot configuration for new players
        /// </summary>
        private void CreateDefaultConfiguration()
        {
            _currentBotConfiguration = new BotData
            {
                BotName = "My First Bot",
                Archetype = BotArchetype.Balanced,
                BotLevel = 1,
                TotalXP = 0,
                PrimaryColor = Color.blue,
                SecondaryColor = Color.white,
                VisualStyleID = 0,
                EquippedParts = new List<BotPart>()
            };
        }

        /// <summary>
        /// Initialize part slot system
        /// </summary>
        private void InitializePartSlots()
        {
            for (int i = 0; i < _partSlots.Length && i < _slotTypes.Length; i++)
            {
                if (_partSlots[i] == null)
                {
                    // Create slot GameObject if not assigned
                    GameObject slotObject = new GameObject($"{_slotTypes[i]}Slot");
                    slotObject.transform.SetParent(_botPreviewPanel);
                    _partSlots[i] = slotObject.AddComponent<BotPartSlot>();
                }
                
                _partSlots[i].Initialize(_slotTypes[i], this);
            }
        }

        /// <summary>
        /// Setup UI event handlers for buttons and controls
        /// </summary>
        private void SetupUIEventHandlers()
        {
            if (_saveConfigurationButton != null)
                _saveConfigurationButton.onClick.AddListener(SaveCurrentConfiguration);
                
            if (_loadConfigurationButton != null)
                _loadConfigurationButton.onClick.AddListener(ShowLoadConfigurationDialog);
                
            if (_resetConfigurationButton != null)
                _resetConfigurationButton.onClick.AddListener(ResetConfiguration);
                
            if (_randomizeButton != null)
                _randomizeButton.onClick.AddListener(RandomizeConfiguration);
            
            // Color picker events
            if (_primaryColorPicker != null)
                _primaryColorPicker.OnColorChanged += OnPrimaryColorChanged;
                
            if (_secondaryColorPicker != null)
                _secondaryColorPicker.OnColorChanged += OnSecondaryColorChanged;
                
            if (_visualStyleDropdown != null)
                _visualStyleDropdown.onValueChanged.AddListener(OnVisualStyleChanged);
        }

        /// <summary>
        /// Load all parts available to the player
        /// </summary>
        private void LoadAvailableParts()
        {
            _availableParts.Clear();
            
            // Load from Resources folder (in a real game, this would come from save data)
            BotPart[] allParts = Resources.LoadAll<BotPart>("BotParts");
            
            foreach (var part in allParts)
            {
                // Check if player has unlocked this part
                if (IsPartUnlocked(part))
                {
                    _availableParts.Add(part);
                }
            }
            
            // Sort by rarity and type for organized display
            _availableParts.Sort((a, b) => 
            {
                int rarityComparison = a.Rarity.CompareTo(b.Rarity);
                return rarityComparison != 0 ? rarityComparison : a.PartType.CompareTo(b.PartType);
            });
            
            Debug.Log($"[BotBuilder] Loaded {_availableParts.Count} available parts");
        }

        /// <summary>
        /// Check if a part is unlocked for the player
        /// </summary>
        private bool IsPartUnlocked(BotPart part)
        {
            // Get resource manager to check unlock status
            var resourceManager = Core.GameManager.Instance?.Resources;
            if (resourceManager == null) return true; // Default to unlocked if no resource manager
            
            // Check level requirement
            if (resourceManager.PlayerLevel < part.RequiredLevel)
                return false;
            
            // Check if player has purchased premium parts
            if (part.IsPremiumPart && !resourceManager.HasPremiumAccount)
                return false;
            
            // Check required parts
            foreach (var requiredPart in part.RequiredParts)
            {
                if (!_availableParts.Contains(requiredPart))
                    return false;
            }
            
            return true;
        }
        #endregion

        #region Configuration Management
        /// <summary>
        /// Apply a bot configuration to the current builder
        /// </summary>
        /// <param name="botController">Bot controller to apply configuration to</param>
        public void ApplyConfigurationToBot(BotController botController)
        {
            if (botController == null)
            {
                Debug.LogError("[BotBuilder] Cannot apply configuration to null bot controller");
                return;
            }
            
            botController.ApplyBotConfiguration(_currentBotConfiguration);
            Debug.Log($"[BotBuilder] Applied configuration to bot: {_currentBotConfiguration.GetDisplayName()}");
        }

        /// <summary>
        /// Save current bot configuration to persistent storage
        /// </summary>
        public void SaveCurrentBotConfiguration()
        {
            var resourceManager = Core.GameManager.Instance?.Resources;
            resourceManager?.SaveBotConfiguration(_currentBotConfiguration);
            
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log($"[BotBuilder] Saved bot configuration: {_currentBotConfiguration.GetDisplayName()}");
        }

        /// <summary>
        /// Load player's current bot configuration
        /// </summary>
        private void LoadPlayerConfiguration()
        {
            var resourceManager = Core.GameManager.Instance?.Resources;
            if (resourceManager != null)
            {
                var savedConfig = resourceManager.LoadBotConfiguration();
                if (savedConfig != null)
                {
                    _currentBotConfiguration = savedConfig;
                }
            }
        }

        /// <summary>
        /// Save current configuration to saved configurations list
        /// </summary>
        public void SaveCurrentConfiguration()
        {
            if (_savedConfigurations.Count >= _maxSavedConfigurations)
            {
                _savedConfigurations.RemoveAt(0); // Remove oldest
            }
            
            // Create a deep copy of current configuration
            var configCopy = CreateConfigurationCopy(_currentBotConfiguration);
            configCopy.BotName = $"Config {DateTime.Now:HH:mm}";
            
            _savedConfigurations.Add(configCopy);
            
            Debug.Log($"[BotBuilder] Configuration saved to slot {_savedConfigurations.Count}");
        }

        /// <summary>
        /// Load a saved configuration
        /// </summary>
        public void LoadConfiguration(int configIndex)
        {
            if (configIndex < 0 || configIndex >= _savedConfigurations.Count)
            {
                Debug.LogError($"[BotBuilder] Invalid configuration index: {configIndex}");
                return;
            }
            
            _currentBotConfiguration = CreateConfigurationCopy(_savedConfigurations[configIndex]);
            RefreshAllUI();
            
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log($"[BotBuilder] Loaded configuration: {_currentBotConfiguration.GetDisplayName()}");
        }

        /// <summary>
        /// Reset bot configuration to default state
        /// </summary>
        public void ResetConfiguration()
        {
            CreateDefaultConfiguration();
            RefreshAllUI();
            
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log("[BotBuilder] Configuration reset to default");
        }

        /// <summary>
        /// Generate a random bot configuration from available parts
        /// </summary>
        public void RandomizeConfiguration()
        {
            // Clear current parts
            _currentBotConfiguration.EquippedParts.Clear();
            
            // Randomly select parts for each slot
            foreach (var slotType in _slotTypes)
            {
                var compatibleParts = _availableParts
                    .Where(p => p.PartType == slotType && p.IsCompatibleWith(_currentBotConfiguration.Archetype))
                    .ToList();
                
                if (compatibleParts.Count > 0)
                {
                    var randomPart = compatibleParts[UnityEngine.Random.Range(0, compatibleParts.Count)];
                    _currentBotConfiguration.EquippedParts.Add(randomPart);
                }
            }
            
            // Randomize colors
            _currentBotConfiguration.PrimaryColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            _currentBotConfiguration.SecondaryColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            
            // Randomize visual style
            _currentBotConfiguration.VisualStyleID = UnityEngine.Random.Range(0, 5);
            
            RefreshAllUI();
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log("[BotBuilder] Configuration randomized");
        }

        /// <summary>
        /// Create a deep copy of a bot configuration
        /// </summary>
        private BotData CreateConfigurationCopy(BotData original)
        {
            return new BotData
            {
                BotName = original.BotName,
                Archetype = original.Archetype,
                BotLevel = original.BotLevel,
                TotalXP = original.TotalXP,
                EquippedParts = new List<BotPart>(original.EquippedParts),
                PrimaryColor = original.PrimaryColor,
                SecondaryColor = original.SecondaryColor,
                VisualStyleID = original.VisualStyleID,
                TotalRunsCompleted = original.TotalRunsCompleted,
                BestDistance = original.BestDistance,
                BestSurvivalTime = original.BestSurvivalTime,
                TotalScrapEarned = original.TotalScrapEarned
            };
        }
        #endregion

        #region Part Management
        /// <summary>
        /// Equip a part to the bot in the appropriate slot
        /// </summary>
        /// <param name="part">Part to equip</param>
        /// <returns>True if successfully equipped</returns>
        public bool EquipPart(BotPart part)
        {
            if (part == null)
            {
                Debug.LogError("[BotBuilder] Cannot equip null part");
                return false;
            }
            
            // Check if part is available
            if (!_availableParts.Contains(part))
            {
                Debug.LogError($"[BotBuilder] Part not available: {part.PartName}");
                return false;
            }
            
            // Check compatibility
            if (!part.IsCompatibleWith(_currentBotConfiguration.Archetype))
            {
                Debug.LogWarning($"[BotBuilder] Part {part.PartName} not compatible with {_currentBotConfiguration.Archetype} archetype");
                return false;
            }
            
            // Remove existing part of same type if any
            UnequipPart(part.PartType);
            
            // Add new part
            _currentBotConfiguration.EquippedParts.Add(part);
            
            // Update UI and stats
            UpdateStatsDisplay();
            UpdateBotPreview();
            
            OnPartEquipped?.Invoke(part, true);
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log($"[BotBuilder] Equipped part: {part.PartName}");
            return true;
        }

        /// <summary>
        /// Unequip a part from the specified slot
        /// </summary>
        /// <param name="partType">Type of part to unequip</param>
        /// <returns>The unequipped part, or null if slot was empty</returns>
        public BotPart UnequipPart(BotPartType partType)
        {
            var existingPart = _currentBotConfiguration.EquippedParts
                .FirstOrDefault(p => p != null && p.PartType == partType);
            
            if (existingPart != null)
            {
                _currentBotConfiguration.EquippedParts.Remove(existingPart);
                
                // Update UI and stats
                UpdateStatsDisplay();
                UpdateBotPreview();
                
                OnPartEquipped?.Invoke(existingPart, false);
                OnConfigurationChanged?.Invoke(_currentBotConfiguration);
                
                Debug.Log($"[BotBuilder] Unequipped part: {existingPart.PartName}");
            }
            
            return existingPart;
        }

        /// <summary>
        /// Check if a specific part can be equipped
        /// </summary>
        public bool CanEquipPart(BotPart part)
        {
            if (part == null) return false;
            if (!_availableParts.Contains(part)) return false;
            if (!part.IsCompatibleWith(_currentBotConfiguration.Archetype)) return false;
            
            return true;
        }

        /// <summary>
        /// Get all parts of a specific type that can be equipped
        /// </summary>
        public List<BotPart> GetCompatibleParts(BotPartType partType)
        {
            return _availableParts
                .Where(p => p.PartType == partType && p.IsCompatibleWith(_currentBotConfiguration.Archetype))
                .ToList();
        }
        #endregion

        #region Visual Customization
        /// <summary>
        /// Change primary color of the bot
        /// </summary>
        public void ChangePrimaryColor(Color newColor)
        {
            _currentBotConfiguration.PrimaryColor = newColor;
            UpdateBotPreview();
            OnVisualsChanged?.Invoke(newColor, _currentBotConfiguration.SecondaryColor, _currentBotConfiguration.VisualStyleID);
        }

        /// <summary>
        /// Change secondary color of the bot
        /// </summary>
        public void ChangeSecondaryColor(Color newColor)
        {
            _currentBotConfiguration.SecondaryColor = newColor;
            UpdateBotPreview();
            OnVisualsChanged?.Invoke(_currentBotConfiguration.PrimaryColor, newColor, _currentBotConfiguration.VisualStyleID);
        }

        /// <summary>
        /// Change visual style of the bot
        /// </summary>
        public void ChangeVisualStyle(int styleID)
        {
            _currentBotConfiguration.VisualStyleID = styleID;
            UpdateBotPreview();
            OnVisualsChanged?.Invoke(_currentBotConfiguration.PrimaryColor, _currentBotConfiguration.SecondaryColor, styleID);
        }

        // UI Event handlers
        private void OnPrimaryColorChanged(Color color) => ChangePrimaryColor(color);
        private void OnSecondaryColorChanged(Color color) => ChangeSecondaryColor(color);
        private void OnVisualStyleChanged(int styleIndex) => ChangeVisualStyle(styleIndex);
        #endregion

        #region UI Updates
        /// <summary>
        /// Refresh all UI elements to match current configuration
        /// </summary>
        private void RefreshAllUI()
        {
            RefreshPartsInventory();
            UpdateStatsDisplay();
            UpdateBotPreview();
            UpdateColorPickers();
            UpdateStyleDropdown();
        }

        /// <summary>
        /// Refresh the parts inventory display
        /// </summary>
        private void RefreshPartsInventory()
        {
            // Clear existing inventory items
            foreach (var item in _partInventoryItems)
            {
                if (item != null && item.gameObject != null)
                    DestroyImmediate(item.gameObject);
            }
            _partInventoryItems.Clear();
            
            // Create UI items for each available part
            foreach (var part in _availableParts)
            {
                GameObject itemObject = CreatePartInventoryItem(part);
                if (itemObject != null)
                {
                    BotPartUI partUI = itemObject.GetComponent<BotPartUI>();
                    if (partUI != null)
                    {
                        _partInventoryItems.Add(partUI);
                    }
                }
            }
            
            Debug.Log($"[BotBuilder] Refreshed inventory with {_partInventoryItems.Count} items");
        }

        /// <summary>
        /// Create a UI item for a bot part in the inventory
        /// </summary>
        private GameObject CreatePartInventoryItem(BotPart part)
        {
            if (_partsInventoryPanel == null) return null;
            
            // Create basic UI structure (would be replaced with proper prefab)
            GameObject itemObject = new GameObject($"Part_{part.PartName}");
            itemObject.transform.SetParent(_partsInventoryPanel);
            
            // Add UI components
            var partUI = itemObject.AddComponent<BotPartUI>();
            partUI.Initialize(part, this);
            
            return itemObject;
        }

        /// <summary>
        /// Update stats display bars and text
        /// </summary>
        private void UpdateStatsDisplay()
        {
            // Calculate current stats
            _currentTotalModifiers = _currentBotConfiguration.GetTotalModifiers();
            
            // Base stats for the archetype
            float baseSpeed = 8f;
            float baseJump = 12f;
            float baseHealth = 100f;
            float baseDefense = 0f;
            
            // Apply archetype modifiers (simplified version)
            switch (_currentBotConfiguration.Archetype)
            {
                case BotArchetype.Speed:
                    baseSpeed *= 1.3f;
                    baseHealth *= 0.8f;
                    break;
                case BotArchetype.Tank:
                    baseSpeed *= 0.8f;
                    baseHealth *= 1.5f;
                    baseDefense += 10f;
                    break;
                case BotArchetype.Jumper:
                    baseJump *= 1.5f;
                    break;
            }
            
            // Apply part modifiers
            float finalSpeed = baseSpeed + _currentTotalModifiers.SpeedModifier;
            float finalJump = baseJump + _currentTotalModifiers.JumpModifier;
            float finalHealth = baseHealth + _currentTotalModifiers.HealthModifier;
            float finalDefense = baseDefense + _currentTotalModifiers.DamageReduction;
            
            // Update UI sliders (normalize to 0-1 range)
            if (_speedStatBar != null)
                _speedStatBar.value = Mathf.Clamp01(finalSpeed / 20f);
                
            if (_jumpStatBar != null)
                _jumpStatBar.value = Mathf.Clamp01(finalJump / 25f);
                
            if (_healthStatBar != null)
                _healthStatBar.value = Mathf.Clamp01(finalHealth / 200f);
                
            if (_defenseStatBar != null)
                _defenseStatBar.value = Mathf.Clamp01(finalDefense / 50f);
            
            // Update overall rating
            if (_overallRatingText != null)
            {
                int rating = _currentBotConfiguration.CalculateOverallRating();
                _overallRatingText.text = $"Rating: {rating}";
            }
            
            OnStatsUpdated?.Invoke(_currentTotalModifiers);
        }

        /// <summary>
        /// Update bot preview model with current configuration
        /// </summary>
        private void UpdateBotPreview()
        {
            if (_botPreviewModel == null) return;
            
            // Update colors
            foreach (var sprite in _previewSprites)
            {
                if (sprite != null)
                {
                    sprite.color = _currentBotConfiguration.PrimaryColor;
                }
            }
            
            // Update animator parameters
            if (_previewAnimator != null)
            {
                _previewAnimator.SetInteger("Archetype", (int)_currentBotConfiguration.Archetype);
                _previewAnimator.SetInteger("StyleID", _currentBotConfiguration.VisualStyleID);
                _previewAnimator.SetInteger("PartCount", _currentBotConfiguration.EquippedParts.Count);
            }
            
            // Update equipped parts visualization
            UpdateEquippedPartsDisplay();
        }

        /// <summary>
        /// Update the display of equipped parts in slots
        /// </summary>
        private void UpdateEquippedPartsDisplay()
        {
            for (int i = 0; i < _partSlots.Length && i < _slotTypes.Length; i++)
            {
                var slotType = _slotTypes[i];
                var equippedPart = GetEquippedPart(slotType);
                
                if (_partSlots[i] != null)
                {
                    _partSlots[i].UpdateDisplay(equippedPart);
                }
            }
        }

        /// <summary>
        /// Update color picker UI elements
        /// </summary>
        private void UpdateColorPickers()
        {
            if (_primaryColorPicker != null)
                _primaryColorPicker.SetCurrentColor(_currentBotConfiguration.PrimaryColor);
                
            if (_secondaryColorPicker != null)
                _secondaryColorPicker.SetCurrentColor(_currentBotConfiguration.SecondaryColor);
        }

        /// <summary>
        /// Update visual style dropdown
        /// </summary>
        private void UpdateStyleDropdown()
        {
            if (_visualStyleDropdown != null)
                _visualStyleDropdown.value = _currentBotConfiguration.VisualStyleID;
        }
        #endregion

        #region Part Dragging System
        /// <summary>
        /// Handle part dragging input for mobile and desktop
        /// </summary>
        private void HandlePartDragging()
        {
            // Handle touch input for mobile
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                HandleDragInput(touch.position, touch.phase == TouchPhase.Began, 
                              touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled);
            }
            // Handle mouse input for desktop
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                Vector2 mousePos = Input.mousePosition;
                HandleDragInput(mousePos, Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0));
            }
        }

        /// <summary>
        /// Process drag input from touch or mouse
        /// </summary>
        private void HandleDragInput(Vector2 screenPosition, bool inputBegan, bool inputEnded)
        {
            if (inputBegan)
            {
                StartPartDrag(screenPosition);
            }
            else if (_draggedPart != null)
            {
                if (inputEnded)
                {
                    EndPartDrag(screenPosition);
                }
                else
                {
                    UpdatePartDrag(screenPosition);
                }
            }
        }

        /// <summary>
        /// Start dragging a part from inventory
        /// </summary>
        private void StartPartDrag(Vector2 screenPosition)
        {
            // Raycast to find which part was clicked
            var hitPart = GetPartUIAtPosition(screenPosition);
            if (hitPart != null && CanEquipPart(hitPart.Part))
            {
                _draggedPart = hitPart;
                _draggedPart.StartDragging();
            }
        }

        /// <summary>
        /// Update part position during drag
        /// </summary>
        private void UpdatePartDrag(Vector2 screenPosition)
        {
            if (_draggedPart != null)
            {
                _draggedPart.UpdateDragPosition(screenPosition);
                
                // Highlight valid drop targets
                HighlightDropTargets(screenPosition);
            }
        }

        /// <summary>
        /// End part dragging and attempt to drop
        /// </summary>
        private void EndPartDrag(Vector2 screenPosition)
        {
            if (_draggedPart != null)
            {
                // Check if dropped on valid slot
                var targetSlot = GetSlotAtPosition(screenPosition);
                if (targetSlot != null && targetSlot.SlotType == _draggedPart.Part.PartType)
                {
                    // Equip the part
                    EquipPart(_draggedPart.Part);
                }
                
                // Clean up drag state
                _draggedPart.EndDragging();
                _draggedPart = null;
                
                // Clear drop target highlights
                ClearDropTargetHighlights();
            }
        }

        /// <summary>
        /// Get part UI at screen position
        /// </summary>
        private BotPartUI GetPartUIAtPosition(Vector2 screenPosition)
        {
            // This would use proper UI raycasting in a real implementation
            foreach (var partUI in _partInventoryItems)
            {
                if (partUI != null && partUI.IsAtScreenPosition(screenPosition))
                {
                    return partUI;
                }
            }
            return null;
        }

        /// <summary>
        /// Get part slot at screen position
        /// </summary>
        private BotPartSlot GetSlotAtPosition(Vector2 screenPosition)
        {
            foreach (var slot in _partSlots)
            {
                if (slot != null && slot.IsAtScreenPosition(screenPosition))
                {
                    return slot;
                }
            }
            return null;
        }

        /// <summary>
        /// Highlight valid drop targets for current dragged part
        /// </summary>
        private void HighlightDropTargets(Vector2 screenPosition)
        {
            if (_draggedPart == null) return;
            
            foreach (var slot in _partSlots)
            {
                if (slot != null)
                {
                    bool isValidTarget = slot.SlotType == _draggedPart.Part.PartType;
                    slot.SetHighlighted(isValidTarget);
                }
            }
        }

        /// <summary>
        /// Clear all drop target highlights
        /// </summary>
        private void ClearDropTargetHighlights()
        {
            foreach (var slot in _partSlots)
            {
                if (slot != null)
                {
                    slot.SetHighlighted(false);
                }
            }
        }
        #endregion

        #region Dialog Systems
        /// <summary>
        /// Show load configuration dialog
        /// </summary>
        private void ShowLoadConfigurationDialog()
        {
            // This would open a dialog with saved configurations
            // For now, just load the first saved configuration if available
            if (_savedConfigurations.Count > 0)
            {
                LoadConfiguration(0);
            }
            else
            {
                Debug.Log("[BotBuilder] No saved configurations available");
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Get current bot stats for display or comparison
        /// </summary>
        public BotPartModifiers GetCurrentStats()
        {
            return _currentTotalModifiers;
        }

        /// <summary>
        /// Check if current configuration is valid for play
        /// </summary>
        public bool IsConfigurationValid()
        {
            // Basic validation - could be expanded
            return _currentBotConfiguration != null && 
                   _currentBotConfiguration.EquippedParts != null;
        }

        /// <summary>
        /// Get configuration completion percentage (how many slots are filled)
        /// </summary>
        public float GetConfigurationCompleteness()
        {
            int filledSlots = 0;
            foreach (var slotType in _slotTypes)
            {
                if (!IsSlotEmpty(slotType))
                {
                    filledSlots++;
                }
            }
            
            return (float)filledSlots / _slotTypes.Length;
        }

        /// <summary>
        /// Set bot archetype and update compatibility
        /// </summary>
        public void SetBotArchetype(BotArchetype newArchetype)
        {
            _currentBotConfiguration.Archetype = newArchetype;
            
            // Remove incompatible parts
            var incompatibleParts = _currentBotConfiguration.EquippedParts
                .Where(p => p != null && !p.IsCompatibleWith(newArchetype))
                .ToList();
                
            foreach (var part in incompatibleParts)
            {
                UnequipPart(part.PartType);
            }
            
            // Update AI behavior and stats
            UpdateStatsDisplay();
            UpdateBotPreview();
            
            OnConfigurationChanged?.Invoke(_currentBotConfiguration);
            
            Debug.Log($"[BotBuilder] Bot archetype changed to {newArchetype}");
        }
        #endregion
    }
}