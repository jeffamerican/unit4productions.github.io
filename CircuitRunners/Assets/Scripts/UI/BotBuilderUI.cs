using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CircuitRunners.UI
{
    /// <summary>
    /// UI component representing a draggable bot part in the inventory
    /// Handles visual representation, drag interactions, and state management
    /// </summary>
    public class BotPartUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image _partIcon;
        [SerializeField] private Text _partNameText;
        [SerializeField] private Text _partStatsText;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private Button _partButton;
        
        [Header("Drag Settings")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _dragAlpha = 0.7f;
        
        private Bot.BotPart _part;
        private Bot.BotBuilder _botBuilder;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private Vector2 _originalPosition;
        private bool _isDragging = false;

        /// <summary>
        /// The bot part represented by this UI element
        /// </summary>
        public Bot.BotPart Part => _part;

        /// <summary>
        /// Initialize the UI element with part data
        /// </summary>
        public void Initialize(Bot.BotPart part, Bot.BotBuilder builder)
        {
            _part = part;
            _botBuilder = builder;
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            
            UpdateVisualDisplay();
            SetupEventHandlers();
        }

        /// <summary>
        /// Update the visual display of the part
        /// </summary>
        private void UpdateVisualDisplay()
        {
            if (_part == null) return;

            // Set part icon
            if (_partIcon != null && _part.PartIcon != null)
                _partIcon.sprite = _part.PartIcon;

            // Set part name
            if (_partNameText != null)
                _partNameText.text = _part.PartName;

            // Set stats text
            if (_partStatsText != null)
                _partStatsText.text = GenerateStatsText();

            // Set rarity background color
            if (_rarityBackground != null)
                _rarityBackground.color = _part.GetRarityColor();
        }

        /// <summary>
        /// Generate stats text for display
        /// </summary>
        private string GenerateStatsText()
        {
            string statsText = "";
            
            if (_part.SpeedModifier != 0)
                statsText += $"Speed: {_part.SpeedModifier:+0;-0}\n";
                
            if (_part.JumpModifier != 0)
                statsText += $"Jump: {_part.JumpModifier:+0;-0}\n";
                
            if (_part.HealthModifier != 0)
                statsText += $"Health: {_part.HealthModifier:+0;-0}\n";
                
            if (_part.DamageReduction > 0)
                statsText += $"Defense: +{_part.DamageReduction}\n";

            return statsText.TrimEnd('\n');
        }

        /// <summary>
        /// Setup event handlers for interactions
        /// </summary>
        private void SetupEventHandlers()
        {
            if (_partButton != null)
            {
                _partButton.onClick.AddListener(OnPartClicked);
            }
        }

        /// <summary>
        /// Handle part click for quick equip
        /// </summary>
        private void OnPartClicked()
        {
            if (_botBuilder != null && _botBuilder.CanEquipPart(_part))
            {
                _botBuilder.EquipPart(_part);
            }
        }

        /// <summary>
        /// Check if this UI element is at the specified screen position
        /// </summary>
        public bool IsAtScreenPosition(Vector2 screenPosition)
        {
            if (_rectTransform == null) return false;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, screenPosition, _canvas.worldCamera, out localPoint);
            
            return _rectTransform.rect.Contains(localPoint);
        }

        /// <summary>
        /// Start dragging state
        /// </summary>
        public void StartDragging()
        {
            _isDragging = true;
            _originalPosition = _rectTransform.anchoredPosition;
            
            if (_canvasGroup != null)
                _canvasGroup.alpha = _dragAlpha;
                
            // Move to front for proper layering
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Update drag position
        /// </summary>
        public void UpdateDragPosition(Vector2 screenPosition)
        {
            if (!_isDragging || _rectTransform == null) return;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform, screenPosition, _canvas.worldCamera, out localPoint);
            
            _rectTransform.anchoredPosition = localPoint;
        }

        /// <summary>
        /// End dragging state
        /// </summary>
        public void EndDragging()
        {
            _isDragging = false;
            
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
            
            // Return to original position if not equipped
            _rectTransform.anchoredPosition = _originalPosition;
        }

        #region IDragHandler Implementation
        public void OnBeginDrag(PointerEventData eventData)
        {
            StartDragging();
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateDragPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Check for drop on valid slot
            var slot = GetSlotUnderPointer(eventData);
            if (slot != null && slot.SlotType == _part.PartType)
            {
                _botBuilder?.EquipPart(_part);
            }
            
            EndDragging();
        }

        /// <summary>
        /// Get the bot part slot under the pointer
        /// </summary>
        private BotPartSlot GetSlotUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<BotPartSlot>();
                if (slot != null)
                {
                    return slot;
                }
            }
            
            return null;
        }
        #endregion
    }

    /// <summary>
    /// UI component representing a bot part slot in the builder
    /// Handles drop zones, visual feedback, and equipped part display
    /// </summary>
    public class BotPartSlot : MonoBehaviour, IDropHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image _slotBackground;
        [SerializeField] private Image _equippedPartIcon;
        [SerializeField] private Text _slotTypeText;
        [SerializeField] private Button _removeButton;
        
        [Header("Visual States")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _emptyColor = Color.gray;
        
        private Bot.BotPartType _slotType;
        private Bot.BotBuilder _botBuilder;
        private Bot.BotPart _equippedPart;
        private bool _isHighlighted = false;

        /// <summary>
        /// The type of part this slot accepts
        /// </summary>
        public Bot.BotPartType SlotType => _slotType;

        /// <summary>
        /// The currently equipped part in this slot
        /// </summary>
        public Bot.BotPart EquippedPart => _equippedPart;

        /// <summary>
        /// Initialize the slot with type and builder reference
        /// </summary>
        public void Initialize(Bot.BotPartType slotType, Bot.BotBuilder builder)
        {
            _slotType = slotType;
            _botBuilder = builder;
            
            if (_slotTypeText != null)
                _slotTypeText.text = slotType.ToString();
            
            if (_removeButton != null)
            {
                _removeButton.onClick.AddListener(RemoveEquippedPart);
                _removeButton.gameObject.SetActive(false);
            }
            
            UpdateDisplay(null);
        }

        /// <summary>
        /// Update the visual display of the slot
        /// </summary>
        public void UpdateDisplay(Bot.BotPart equippedPart)
        {
            _equippedPart = equippedPart;
            
            if (_equippedPart != null)
            {
                // Show equipped part
                if (_equippedPartIcon != null)
                {
                    _equippedPartIcon.sprite = _equippedPart.PartIcon;
                    _equippedPartIcon.color = Color.white;
                }
                
                if (_slotBackground != null)
                    _slotBackground.color = _equippedPart.GetRarityColor();
                
                if (_removeButton != null)
                    _removeButton.gameObject.SetActive(true);
            }
            else
            {
                // Show empty slot
                if (_equippedPartIcon != null)
                {
                    _equippedPartIcon.sprite = null;
                    _equippedPartIcon.color = Color.clear;
                }
                
                if (_slotBackground != null)
                    _slotBackground.color = _emptyColor;
                
                if (_removeButton != null)
                    _removeButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set highlight state for drag feedback
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            _isHighlighted = highlighted;
            
            if (_slotBackground != null)
            {
                Color targetColor = _isHighlighted ? _highlightColor : 
                                   (_equippedPart != null ? _equippedPart.GetRarityColor() : _emptyColor);
                _slotBackground.color = targetColor;
            }
        }

        /// <summary>
        /// Remove the currently equipped part
        /// </summary>
        public void RemoveEquippedPart()
        {
            if (_botBuilder != null && _equippedPart != null)
            {
                _botBuilder.UnequipPart(_slotType);
            }
        }

        /// <summary>
        /// Check if this slot is at the specified screen position
        /// </summary>
        public bool IsAtScreenPosition(Vector2 screenPosition)
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return false;
            
            var canvas = GetComponentInParent<Canvas>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPosition, canvas.worldCamera, out localPoint);
            
            return rectTransform.rect.Contains(localPoint);
        }

        #region IDropHandler Implementation
        public void OnDrop(PointerEventData eventData)
        {
            var draggedPartUI = eventData.pointerDrag?.GetComponent<BotPartUI>();
            if (draggedPartUI != null && draggedPartUI.Part.PartType == _slotType)
            {
                _botBuilder?.EquipPart(draggedPartUI.Part);
            }
        }
        #endregion
    }

    /// <summary>
    /// Simple color picker UI component for bot customization
    /// </summary>
    public class ColorPicker : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _colorDisplay;
        [SerializeField] private Slider _redSlider;
        [SerializeField] private Slider _greenSlider;
        [SerializeField] private Slider _blueSlider;
        [SerializeField] private Button[] _presetColorButtons;
        
        [Header("Preset Colors")]
        [SerializeField] private Color[] _presetColors = {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.magenta, Color.cyan, Color.white, Color.black
        };
        
        private Color _currentColor = Color.white;
        
        /// <summary>
        /// Event fired when color changes
        /// </summary>
        public System.Action<Color> OnColorChanged;

        private void Start()
        {
            SetupSliders();
            SetupPresetButtons();
            UpdateColorDisplay();
        }

        /// <summary>
        /// Setup RGB sliders
        /// </summary>
        private void SetupSliders()
        {
            if (_redSlider != null)
            {
                _redSlider.onValueChanged.AddListener(OnRedChanged);
                _redSlider.value = _currentColor.r;
            }
            
            if (_greenSlider != null)
            {
                _greenSlider.onValueChanged.AddListener(OnGreenChanged);
                _greenSlider.value = _currentColor.g;
            }
            
            if (_blueSlider != null)
            {
                _blueSlider.onValueChanged.AddListener(OnBlueChanged);
                _blueSlider.value = _currentColor.b;
            }
        }

        /// <summary>
        /// Setup preset color buttons
        /// </summary>
        private void SetupPresetButtons()
        {
            for (int i = 0; i < _presetColorButtons.Length && i < _presetColors.Length; i++)
            {
                int colorIndex = i; // Capture for closure
                var button = _presetColorButtons[i];
                var color = _presetColors[i];
                
                button.image.color = color;
                button.onClick.AddListener(() => SetCurrentColor(color));
            }
        }

        /// <summary>
        /// Set the current color and update UI
        /// </summary>
        public void SetCurrentColor(Color color)
        {
            _currentColor = color;
            UpdateSliders();
            UpdateColorDisplay();
            OnColorChanged?.Invoke(_currentColor);
        }

        /// <summary>
        /// Update slider values to match current color
        /// </summary>
        private void UpdateSliders()
        {
            if (_redSlider != null)
                _redSlider.value = _currentColor.r;
            if (_greenSlider != null)
                _greenSlider.value = _currentColor.g;
            if (_blueSlider != null)
                _blueSlider.value = _currentColor.b;
        }

        /// <summary>
        /// Update the color display
        /// </summary>
        private void UpdateColorDisplay()
        {
            if (_colorDisplay != null)
                _colorDisplay.color = _currentColor;
        }

        // Slider event handlers
        private void OnRedChanged(float value)
        {
            _currentColor.r = value;
            UpdateColorDisplay();
            OnColorChanged?.Invoke(_currentColor);
        }

        private void OnGreenChanged(float value)
        {
            _currentColor.g = value;
            UpdateColorDisplay();
            OnColorChanged?.Invoke(_currentColor);
        }

        private void OnBlueChanged(float value)
        {
            _currentColor.b = value;
            UpdateColorDisplay();
            OnColorChanged?.Invoke(_currentColor);
        }
    }
}