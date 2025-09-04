using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRunners.Bot
{
    /// <summary>
    /// Controls bot AI behavior during runs, including movement decisions, obstacle avoidance,
    /// and collectible gathering. This is the core system that brings bots to life with
    /// intelligent behavior based on their configured archetype and parts.
    /// 
    /// Key Features:
    /// - AI decision-making system with configurable behavior patterns
    /// - Physics-based movement with mobile-optimized controls
    /// - Performance tracking and statistics collection
    /// - Modular ability system tied to bot configuration
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BotController : MonoBehaviour
    {
        #region Bot Configuration
        [Header("Bot Configuration")]
        [SerializeField] private BotData _botConfiguration;
        [SerializeField] private BotArchetype _currentArchetype = BotArchetype.Balanced;
        [SerializeField] private List<BotPart> _equippedParts = new List<BotPart>();

        /// <summary>
        /// Current bot archetype determines base behavior patterns
        /// </summary>
        public BotArchetype Archetype => _currentArchetype;
        
        /// <summary>
        /// All parts currently equipped on this bot
        /// </summary>
        public List<BotPart> EquippedParts => _equippedParts;
        #endregion

        #region Movement & Physics
        [Header("Movement Settings")]
        [SerializeField] private float _baseRunSpeed = 8f;
        [SerializeField] private float _jumpForce = 12f;
        [SerializeField] private float _slideDistance = 3f;
        [SerializeField] private float _dashForce = 15f;
        [SerializeField] private float _maxFallSpeed = 20f;

        [Header("AI Decision Making")]
        [SerializeField] private float _decisionDistance = 10f; // How far ahead the bot looks
        [SerializeField] private float _reactionTime = 0.2f; // AI reaction delay for realism
        [SerializeField] private LayerMask _obstacleLayerMask = -1;
        [SerializeField] private LayerMask _collectibleLayerMask = -1;

        // Physics components
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private Animator _animator;

        // Movement state
        private bool _isGrounded = false;
        private bool _isSliding = false;
        private bool _canDash = true;
        private float _currentSpeed;
        private Vector2 _moveDirection = Vector2.right;

        // Ground detection
        [Header("Ground Detection")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask _groundLayerMask = -1;
        #endregion

        #region AI System
        [Header("AI Behavior")]
        [SerializeField] private AIDecisionData _aiDecisionData;
        [SerializeField] private float _riskTolerance = 0.5f; // 0 = very safe, 1 = very risky
        [SerializeField] private float _collectiblePriority = 0.7f; // How much bot prioritizes collectibles

        // AI decision tracking
        private Queue<AIDecision> _pendingDecisions = new Queue<AIDecision>();
        private AIDecision _currentDecision = null;
        private float _lastDecisionTime = 0f;
        private List<ObstacleInfo> _detectedObstacles = new List<ObstacleInfo>();
        private List<CollectibleInfo> _detectedCollectibles = new List<CollectibleInfo>();
        #endregion

        #region Run State & Statistics
        [Header("Run State")]
        [SerializeField] private bool _isRunActive = false;
        [SerializeField] private bool _isRunComplete = false;
        [SerializeField] private bool _isBotDestroyed = false;
        [SerializeField] private bool _hasCompletedCourse = false;
        [SerializeField] private bool _performanceModeEnabled = false;

        // Run statistics tracking
        private BotRunStatistics _currentRunStats;
        private float _runStartTime;
        private Vector3 _startPosition;
        private float _furthestDistance = 0f;

        /// <summary>
        /// Whether the bot is currently in an active run
        /// </summary>
        public bool IsRunActive => _isRunActive;
        
        /// <summary>
        /// Whether the current run has completed (success or failure)
        /// </summary>
        public bool IsRunComplete => _isRunComplete;
        
        /// <summary>
        /// Whether the bot was destroyed during the run
        /// </summary>
        public bool IsBotDestroyed => _isBotDestroyed;
        
        /// <summary>
        /// Whether the bot successfully completed the entire course
        /// </summary>
        public bool HasCompletedCourse => _hasCompletedCourse;
        #endregion

        #region Events
        /// <summary>
        /// Fired when the bot makes a significant decision (jump, slide, dash)
        /// </summary>
        public event Action<AIDecision> OnBotDecision;
        
        /// <summary>
        /// Fired when the bot collects an item
        /// </summary>
        public event Action<CollectibleInfo> OnCollectibleGathered;
        
        /// <summary>
        /// Fired when the bot takes damage or hits an obstacle
        /// </summary>
        public event Action<ObstacleInfo> OnObstacleHit;
        
        /// <summary>
        /// Fired when the run ends for any reason
        /// </summary>
        public event Action<BotRunStatistics> OnRunEnded;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Get required components
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _animator = GetComponent<Animator>();

            // Initialize run statistics
            _currentRunStats = new BotRunStatistics();
            
            // Set up ground check point if not assigned
            if (_groundCheckPoint == null)
            {
                GameObject groundCheck = new GameObject("GroundCheck");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0, -_collider.bounds.extents.y - 0.1f, 0);
                _groundCheckPoint = groundCheck.transform;
            }
        }

        private void Start()
        {
            // Initialize AI system
            InitializeAI();
            
            // Apply initial bot configuration
            if (_botConfiguration != null)
            {
                ApplyBotConfiguration(_botConfiguration);
            }
        }

        private void Update()
        {
            if (!_isRunActive) return;

            // Update AI decision making
            UpdateAI();
            
            // Update movement and physics
            UpdateMovement();
            
            // Update run statistics
            UpdateRunStatistics();
            
            // Check for run completion conditions
            CheckRunCompletion();
        }

        private void FixedUpdate()
        {
            if (!_isRunActive) return;

            // Apply physics-based movement
            ApplyMovement();
            
            // Check ground contact
            CheckGrounded();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.collider);
        }
        #endregion

        #region Bot Configuration
        /// <summary>
        /// Apply a complete bot configuration including archetype and parts
        /// </summary>
        /// <param name="config">Bot configuration data</param>
        public void ApplyBotConfiguration(BotData config)
        {
            _botConfiguration = config;
            _currentArchetype = config.Archetype;
            _equippedParts.Clear();
            _equippedParts.AddRange(config.EquippedParts);

            // Apply archetype modifiers
            ApplyArchetypeModifiers();
            
            // Apply part modifiers
            ApplyPartModifiers();
            
            // Update AI behavior based on configuration
            UpdateAIBehavior();
            
            // Update visual representation
            UpdateBotVisuals();

            Debug.Log($"[BotController] Configuration applied: {_currentArchetype} archetype with {_equippedParts.Count} parts");
        }

        /// <summary>
        /// Apply base stat modifiers based on bot archetype
        /// </summary>
        private void ApplyArchetypeModifiers()
        {
            switch (_currentArchetype)
            {
                case BotArchetype.Speed:
                    _baseRunSpeed *= 1.3f;
                    _reactionTime *= 0.8f;
                    _jumpForce *= 0.9f;
                    _riskTolerance = 0.7f;
                    break;
                    
                case BotArchetype.Tank:
                    _baseRunSpeed *= 0.8f;
                    _jumpForce *= 1.2f;
                    _reactionTime *= 1.1f;
                    _riskTolerance = 0.3f;
                    break;
                    
                case BotArchetype.Jumper:
                    _jumpForce *= 1.5f;
                    _baseRunSpeed *= 0.95f;
                    _riskTolerance = 0.6f;
                    break;
                    
                case BotArchetype.Collector:
                    _collectiblePriority = 0.9f;
                    _decisionDistance *= 1.2f;
                    _baseRunSpeed *= 0.9f;
                    _riskTolerance = 0.4f;
                    break;
                    
                case BotArchetype.Lucky:
                    _riskTolerance = 0.8f;
                    _reactionTime *= 0.9f;
                    break;
                    
                case BotArchetype.Hacker:
                    _decisionDistance *= 1.3f;
                    _reactionTime *= 0.7f;
                    _collectiblePriority = 0.8f;
                    break;
                    
                case BotArchetype.Balanced:
                default:
                    // No modifiers for balanced archetype
                    break;
            }
        }

        /// <summary>
        /// Apply stat modifiers from equipped parts
        /// </summary>
        private void ApplyPartModifiers()
        {
            foreach (var part in _equippedParts)
            {
                if (part == null) continue;

                // Apply part-specific modifiers
                _baseRunSpeed += part.SpeedModifier;
                _jumpForce += part.JumpModifier;
                _dashForce += part.DashModifier;
                _riskTolerance += part.RiskToleranceModifier;
                _collectiblePriority += part.CollectiblePriorityModifier;

                // Clamp values to reasonable ranges
                _riskTolerance = Mathf.Clamp01(_riskTolerance);
                _collectiblePriority = Mathf.Clamp01(_collectiblePriority);
            }
        }

        /// <summary>
        /// Update AI behavior parameters based on bot configuration
        /// </summary>
        private void UpdateAIBehavior()
        {
            if (_aiDecisionData == null)
            {
                _aiDecisionData = ScriptableObject.CreateInstance<AIDecisionData>();
            }

            // Customize AI behavior based on archetype
            switch (_currentArchetype)
            {
                case BotArchetype.Speed:
                    _aiDecisionData.AggressivenessBias = 0.7f;
                    _aiDecisionData.SafetyBias = 0.3f;
                    break;
                    
                case BotArchetype.Tank:
                    _aiDecisionData.AggressivenessBias = 0.2f;
                    _aiDecisionData.SafetyBias = 0.8f;
                    break;
                    
                case BotArchetype.Collector:
                    _aiDecisionData.CollectibleBias = 0.9f;
                    _aiDecisionData.SafetyBias = 0.6f;
                    break;
                    
                default:
                    _aiDecisionData.AggressivenessBias = 0.5f;
                    _aiDecisionData.SafetyBias = 0.5f;
                    _aiDecisionData.CollectibleBias = 0.7f;
                    break;
            }
        }

        /// <summary>
        /// Update bot visual representation based on equipped parts
        /// </summary>
        private void UpdateBotVisuals()
        {
            // This would integrate with a visual customization system
            // For now, just update animator parameters
            if (_animator != null)
            {
                _animator.SetInteger("Archetype", (int)_currentArchetype);
                _animator.SetInteger("PartCount", _equippedParts.Count);
            }
        }
        #endregion

        #region AI System
        /// <summary>
        /// Initialize the AI decision-making system
        /// </summary>
        private void InitializeAI()
        {
            _detectedObstacles.Clear();
            _detectedCollectibles.Clear();
            _pendingDecisions.Clear();
            _currentDecision = null;
            _lastDecisionTime = 0f;

            Debug.Log("[BotController] AI system initialized");
        }

        /// <summary>
        /// Main AI update loop - scans environment and makes decisions
        /// </summary>
        private void UpdateAI()
        {
            // Scan environment for obstacles and collectibles
            ScanEnvironment();
            
            // Process pending decisions
            ProcessDecisionQueue();
            
            // Make new decisions if needed
            if (ShouldMakeNewDecision())
            {
                MakeDecision();
            }
            
            // Execute current decision
            ExecuteCurrentDecision();
        }

        /// <summary>
        /// Scan ahead for obstacles and collectibles
        /// </summary>
        private void ScanEnvironment()
        {
            _detectedObstacles.Clear();
            _detectedCollectibles.Clear();

            Vector2 scanOrigin = transform.position;
            Vector2 scanDirection = _moveDirection;
            float scanDistance = _decisionDistance;

            // Raycast for obstacles
            RaycastHit2D[] obstacleHits = Physics2D.RaycastAll(
                scanOrigin, scanDirection, scanDistance, _obstacleLayerMask);

            foreach (var hit in obstacleHits)
            {
                if (hit.collider != null && hit.collider != _collider)
                {
                    _detectedObstacles.Add(new ObstacleInfo
                    {
                        Transform = hit.transform,
                        Distance = hit.distance,
                        ObstacleType = DetermineObstacleType(hit.collider),
                        RequiredAction = DetermineRequiredAction(hit.collider),
                        ThreatLevel = CalculateThreatLevel(hit.collider, hit.distance)
                    });
                }
            }

            // Raycast for collectibles
            RaycastHit2D[] collectibleHits = Physics2D.RaycastAll(
                scanOrigin, scanDirection, scanDistance, _collectibleLayerMask);

            foreach (var hit in collectibleHits)
            {
                if (hit.collider != null)
                {
                    _detectedCollectibles.Add(new CollectibleInfo
                    {
                        Transform = hit.transform,
                        Distance = hit.distance,
                        CollectibleType = DetermineCollectibleType(hit.collider),
                        Value = GetCollectibleValue(hit.collider),
                        Priority = CalculateCollectiblePriority(hit.collider, hit.distance)
                    });
                }
            }

            // Sort by distance for decision making
            _detectedObstacles.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            _detectedCollectibles.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Determine if the AI should make a new decision
        /// </summary>
        private bool ShouldMakeNewDecision()
        {
            // Always make decisions if there's an immediate threat
            if (_detectedObstacles.Count > 0 && _detectedObstacles[0].Distance < 2f)
                return true;

            // Make decisions at regular intervals
            if (Time.time - _lastDecisionTime > _reactionTime)
                return true;

            // Make decisions when current one is complete
            return _currentDecision == null || _currentDecision.IsComplete;
        }

        /// <summary>
        /// Core AI decision-making logic
        /// </summary>
        private void MakeDecision()
        {
            _lastDecisionTime = Time.time;

            // Prioritize immediate threats
            if (_detectedObstacles.Count > 0)
            {
                var nearestObstacle = _detectedObstacles[0];
                
                // Calculate urgency based on distance and bot speed
                float timeToImpact = nearestObstacle.Distance / _currentSpeed;
                
                if (timeToImpact < 1.5f) // Critical threat
                {
                    var decision = CreateAvoidanceDecision(nearestObstacle);
                    QueueDecision(decision);
                    return;
                }
            }

            // Consider collectibles if no immediate threats
            if (_detectedCollectibles.Count > 0)
            {
                var bestCollectible = _detectedCollectibles[0];
                
                // Only pursue if priority is high enough and it's reasonably safe
                if (bestCollectible.Priority > _collectiblePriority && 
                    IsCollectibleSafe(bestCollectible))
                {
                    var decision = CreateCollectionDecision(bestCollectible);
                    QueueDecision(decision);
                    return;
                }
            }

            // Default behavior: continue running
            var continueDecision = new AIDecision
            {
                DecisionType = AIDecisionType.Continue,
                ExecuteTime = Time.time,
                Duration = _reactionTime,
                Priority = 0.1f,
                Description = "Continue running forward"
            };
            
            QueueDecision(continueDecision);
        }

        /// <summary>
        /// Create a decision to avoid an obstacle
        /// </summary>
        private AIDecision CreateAvoidanceDecision(ObstacleInfo obstacle)
        {
            AIDecisionType bestAction = obstacle.RequiredAction;
            float priority = 1.0f; // High priority for avoidance
            
            // Modify action based on bot capabilities and situation
            if (bestAction == AIDecisionType.Jump && !_isGrounded)
            {
                // Can't jump if not grounded, try sliding instead
                bestAction = AIDecisionType.Slide;
                priority *= 0.8f;
            }
            else if (bestAction == AIDecisionType.Dash && !_canDash)
            {
                // Can't dash if on cooldown, try jumping
                bestAction = AIDecisionType.Jump;
                priority *= 0.7f;
            }

            return new AIDecision
            {
                DecisionType = bestAction,
                ExecuteTime = Time.time + Mathf.Max(0, obstacle.Distance / _currentSpeed - 0.5f),
                Duration = GetActionDuration(bestAction),
                Priority = priority,
                TargetObject = obstacle.Transform,
                Description = $"Avoid {obstacle.ObstacleType} obstacle"
            };
        }

        /// <summary>
        /// Create a decision to collect an item
        /// </summary>
        private AIDecision CreateCollectionDecision(CollectibleInfo collectible)
        {
            // Determine if we need to adjust path to collect
            float verticalDistance = collectible.Transform.position.y - transform.position.y;
            AIDecisionType action = AIDecisionType.Continue;
            
            if (verticalDistance > 1.5f && _isGrounded)
            {
                action = AIDecisionType.Jump;
            }
            else if (verticalDistance < -1.5f)
            {
                action = AIDecisionType.Slide;
            }

            return new AIDecision
            {
                DecisionType = action,
                ExecuteTime = Time.time + Mathf.Max(0, collectible.Distance / _currentSpeed - 0.3f),
                Duration = GetActionDuration(action),
                Priority = collectible.Priority,
                TargetObject = collectible.Transform,
                Description = $"Collect {collectible.CollectibleType} item"
            };
        }

        /// <summary>
        /// Add a decision to the execution queue
        /// </summary>
        private void QueueDecision(AIDecision decision)
        {
            _pendingDecisions.Enqueue(decision);
            OnBotDecision?.Invoke(decision);
        }

        /// <summary>
        /// Process queued decisions and set current decision
        /// </summary>
        private void ProcessDecisionQueue()
        {
            while (_pendingDecisions.Count > 0)
            {
                var nextDecision = _pendingDecisions.Peek();
                
                // Check if it's time to execute this decision
                if (Time.time >= nextDecision.ExecuteTime)
                {
                    _currentDecision = _pendingDecisions.Dequeue();
                    _currentDecision.StartExecution();
                    break;
                }
                else
                {
                    break; // Wait for execution time
                }
            }
        }

        /// <summary>
        /// Execute the current AI decision
        /// </summary>
        private void ExecuteCurrentDecision()
        {
            if (_currentDecision == null || !_currentDecision.IsActive)
                return;

            switch (_currentDecision.DecisionType)
            {
                case AIDecisionType.Jump:
                    ExecuteJump();
                    break;
                    
                case AIDecisionType.Slide:
                    ExecuteSlide();
                    break;
                    
                case AIDecisionType.Dash:
                    ExecuteDash();
                    break;
                    
                case AIDecisionType.Continue:
                default:
                    // Just keep running - no special action needed
                    break;
            }

            // Check if decision is complete
            if (_currentDecision.IsComplete)
            {
                _currentDecision = null;
            }
        }
        #endregion

        #region Movement Actions
        /// <summary>
        /// Execute jump action with physics and animation
        /// </summary>
        private void ExecuteJump()
        {
            if (_isGrounded && !_isSliding)
            {
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _animator?.SetTrigger("Jump");
                
                // Update statistics
                _currentRunStats.JumpsPerformed++;
                
                Debug.Log("[BotController] Executed jump");
            }
        }

        /// <summary>
        /// Execute slide action with physics and animation
        /// </summary>
        private void ExecuteSlide()
        {
            if (_isGrounded && !_isSliding)
            {
                StartCoroutine(SlideCoroutine());
                _animator?.SetTrigger("Slide");
                
                // Update statistics
                _currentRunStats.SlidesPerformed++;
                
                Debug.Log("[BotController] Executed slide");
            }
        }

        /// <summary>
        /// Execute dash action with physics and cooldown
        /// </summary>
        private void ExecuteDash()
        {
            if (_canDash)
            {
                _rigidbody.AddForce(_moveDirection * _dashForce, ForceMode2D.Impulse);
                _animator?.SetTrigger("Dash");
                
                // Start dash cooldown
                StartCoroutine(DashCooldownCoroutine());
                
                // Update statistics
                _currentRunStats.DashesPerformed++;
                
                Debug.Log("[BotController] Executed dash");
            }
        }

        /// <summary>
        /// Coroutine to handle slide duration and collision changes
        /// </summary>
        private IEnumerator SlideCoroutine()
        {
            _isSliding = true;
            
            // Modify collider for sliding (make shorter)
            Vector2 originalSize = _collider.bounds.size;
            Vector2 slideSize = new Vector2(originalSize.x, originalSize.y * 0.5f);
            
            if (_collider is BoxCollider2D boxCollider)
            {
                boxCollider.size = slideSize;
                boxCollider.offset = new Vector2(0, -originalSize.y * 0.25f);
            }
            
            // Slide duration based on distance
            float slideDuration = _slideDistance / _currentSpeed;
            yield return new WaitForSeconds(slideDuration);
            
            // Restore collider
            if (_collider is BoxCollider2D box)
            {
                box.size = originalSize;
                box.offset = Vector2.zero;
            }
            
            _isSliding = false;
        }

        /// <summary>
        /// Coroutine to handle dash cooldown
        /// </summary>
        private IEnumerator DashCooldownCoroutine()
        {
            _canDash = false;
            
            // Cooldown duration based on bot configuration
            float cooldownDuration = 2f;
            foreach (var part in _equippedParts)
            {
                cooldownDuration -= part.DashCooldownReduction;
            }
            cooldownDuration = Mathf.Max(0.5f, cooldownDuration);
            
            yield return new WaitForSeconds(cooldownDuration);
            _canDash = true;
        }
        #endregion

        #region Movement & Physics
        /// <summary>
        /// Update movement parameters and current speed
        /// </summary>
        private void UpdateMovement()
        {
            // Calculate current speed with any active modifiers
            _currentSpeed = _baseRunSpeed;
            
            // Apply temporary speed modifiers from power-ups or abilities
            foreach (var part in _equippedParts)
            {
                if (part.HasActiveSpeedBoost)
                {
                    _currentSpeed *= part.SpeedBoostMultiplier;
                }
            }
            
            // Clamp to reasonable limits
            _currentSpeed = Mathf.Clamp(_currentSpeed, 1f, 25f);
            
            // Update animator speed parameter
            _animator?.SetFloat("Speed", _currentSpeed / _baseRunSpeed);
        }

        /// <summary>
        /// Apply physics-based movement
        /// </summary>
        private void ApplyMovement()
        {
            // Horizontal movement
            Vector2 targetVelocity = new Vector2(_currentSpeed, _rigidbody.velocity.y);
            
            // Clamp fall speed
            if (targetVelocity.y < -_maxFallSpeed)
            {
                targetVelocity.y = -_maxFallSpeed;
            }
            
            _rigidbody.velocity = targetVelocity;
        }

        /// <summary>
        /// Check if bot is grounded using physics raycast
        /// </summary>
        private void CheckGrounded()
        {
            bool wasGrounded = _isGrounded;
            
            _isGrounded = Physics2D.OverlapCircle(
                _groundCheckPoint.position, _groundCheckRadius, _groundLayerMask);
            
            // Update animator
            _animator?.SetBool("IsGrounded", _isGrounded);
            
            // Landing detection for statistics
            if (!wasGrounded && _isGrounded)
            {
                _currentRunStats.SuccessfulLandings++;
            }
        }
        #endregion

        #region Collision Handling
        /// <summary>
        /// Handle collision with obstacles, collectibles, and course elements
        /// </summary>
        private void HandleCollision(Collider2D other)
        {
            if (!_isRunActive) return;

            string tag = other.tag;
            
            switch (tag)
            {
                case "Obstacle":
                    HandleObstacleCollision(other);
                    break;
                    
                case "Collectible":
                    HandleCollectibleCollection(other);
                    break;
                    
                case "Hazard":
                    HandleHazardCollision(other);
                    break;
                    
                case "PowerUp":
                    HandlePowerUpCollection(other);
                    break;
                    
                case "CourseEnd":
                    HandleCourseCompletion();
                    break;
                    
                case "DeathZone":
                    HandleBotDestruction("Fell into death zone");
                    break;
            }
        }

        /// <summary>
        /// Handle collision with obstacles (damage, slow down, etc.)
        /// </summary>
        private void HandleObstacleCollision(Collider2D obstacle)
        {
            var obstacleComponent = obstacle.GetComponent<Course.ObstacleData>();
            if (obstacleComponent == null) return;

            // Calculate damage based on obstacle type and bot resistance
            int damage = obstacleComponent.BaseDamage;
            
            // Apply bot resistances from parts
            foreach (var part in _equippedParts)
            {
                damage -= part.DamageReduction;
            }
            damage = Mathf.Max(1, damage); // Minimum 1 damage

            // Apply damage and effects
            _currentRunStats.DamagesTaken += damage;
            _currentRunStats.ObstaclesHit++;

            // Create obstacle info for event
            var obstacleInfo = new ObstacleInfo
            {
                Transform = obstacle.transform,
                Distance = 0f,
                ObstacleType = obstacleComponent.ObstacleType,
                ThreatLevel = damage / 10f
            };

            OnObstacleHit?.Invoke(obstacleInfo);

            // Check if bot is destroyed
            if (_currentRunStats.DamagesTaken >= GetMaxHealth())
            {
                HandleBotDestruction("Took too much damage");
            }
            else
            {
                // Apply temporary speed reduction or other effects
                StartCoroutine(ApplyObstacleEffect(obstacleComponent));
            }

            Debug.Log($"[BotController] Hit obstacle: {damage} damage, total: {_currentRunStats.DamagesTaken}");
        }

        /// <summary>
        /// Handle collection of valuable items
        /// </summary>
        private void HandleCollectibleCollection(Collider2D collectible)
        {
            var collectibleComponent = collectible.GetComponent<Course.CollectibleData>();
            if (collectibleComponent == null) return;

            // Add to statistics
            _currentRunStats.CollectiblesGathered++;
            _currentRunStats.ScrapCollected += collectibleComponent.ScrapValue;

            // Create collectible info for event
            var collectibleInfo = new CollectibleInfo
            {
                Transform = collectible.transform,
                Distance = 0f,
                CollectibleType = collectibleComponent.CollectibleType,
                Value = collectibleComponent.ScrapValue,
                Priority = 1f
            };

            OnCollectibleGathered?.Invoke(collectibleInfo);

            // Destroy or disable the collectible
            collectible.gameObject.SetActive(false);

            Debug.Log($"[BotController] Collected {collectibleComponent.CollectibleType}: +{collectibleComponent.ScrapValue} scrap");
        }

        /// <summary>
        /// Handle collision with hazards (usually instant destruction)
        /// </summary>
        private void HandleHazardCollision(Collider2D hazard)
        {
            var hazardComponent = hazard.GetComponent<Course.HazardData>();
            string hazardType = hazardComponent != null ? hazardComponent.HazardType : "Unknown hazard";
            
            HandleBotDestruction($"Hit {hazardType}");
        }

        /// <summary>
        /// Handle collection of power-ups and temporary boosts
        /// </summary>
        private void HandlePowerUpCollection(Collider2D powerUp)
        {
            var powerUpComponent = powerUp.GetComponent<Course.PowerUpData>();
            if (powerUpComponent == null) return;

            // Apply power-up effects
            StartCoroutine(ApplyPowerUpEffect(powerUpComponent));
            
            // Update statistics
            _currentRunStats.PowerUpsCollected++;

            // Disable power-up
            powerUp.gameObject.SetActive(false);

            Debug.Log($"[BotController] Collected power-up: {powerUpComponent.PowerUpType}");
        }

        /// <summary>
        /// Handle successful course completion
        /// </summary>
        private void HandleCourseCompletion()
        {
            _hasCompletedCourse = true;
            _currentRunStats.HasCompletedCourse = true;
            
            EndRun("Course completed successfully");
        }

        /// <summary>
        /// Handle bot destruction and run failure
        /// </summary>
        private void HandleBotDestruction(string reason)
        {
            _isBotDestroyed = true;
            _currentRunStats.DestructionReason = reason;
            
            EndRun(reason);
        }
        #endregion

        #region Run Management
        /// <summary>
        /// Start a new run with the current bot configuration
        /// </summary>
        public void StartRun()
        {
            if (_isRunActive)
            {
                Debug.LogWarning("[BotController] Cannot start run - run already active");
                return;
            }

            // Initialize run state
            _isRunActive = true;
            _isRunComplete = false;
            _isBotDestroyed = false;
            _hasCompletedCourse = false;
            _runStartTime = Time.time;
            _startPosition = transform.position;
            _furthestDistance = 0f;

            // Reset statistics
            _currentRunStats = new BotRunStatistics
            {
                StartTime = _runStartTime,
                BotArchetype = _currentArchetype,
                EquippedParts = new List<BotPart>(_equippedParts)
            };

            // Reset AI state
            InitializeAI();

            // Reset movement state
            _canDash = true;
            _isSliding = false;
            _currentSpeed = _baseRunSpeed;

            // Update animator
            _animator?.SetBool("IsRunning", true);

            Debug.Log($"[BotController] Run started with {_currentArchetype} bot");
        }

        /// <summary>
        /// Force end the current run (timeout, user action, etc.)
        /// </summary>
        /// <param name="reason">Reason for ending the run</param>
        public void ForceEndRun(string reason)
        {
            if (!_isRunActive) return;

            EndRun($"Run ended: {reason}");
        }

        /// <summary>
        /// Internal method to end the run and finalize statistics
        /// </summary>
        private void EndRun(string reason)
        {
            if (!_isRunActive) return;

            _isRunActive = false;
            _isRunComplete = true;

            // Finalize statistics
            _currentRunStats.EndTime = Time.time;
            _currentRunStats.SurvivalTime = _currentRunStats.EndTime - _currentRunStats.StartTime;
            _currentRunStats.DistanceTraveled = _furthestDistance;
            _currentRunStats.EndReason = reason;

            // Calculate derived statistics
            _currentRunStats.AverageSpeed = _currentRunStats.DistanceTraveled / _currentRunStats.SurvivalTime;
            _currentRunStats.ObstaclesAvoided = CalculateObstaclesAvoided();

            // Stop movement
            _rigidbody.velocity = Vector2.zero;
            _animator?.SetBool("IsRunning", false);

            // Fire completion event
            OnRunEnded?.Invoke(_currentRunStats);

            Debug.Log($"[BotController] Run ended: {reason}. Distance: {_currentRunStats.DistanceTraveled:F1}m, Time: {_currentRunStats.SurvivalTime:F1}s");
        }

        /// <summary>
        /// Check various run completion conditions
        /// </summary>
        private void CheckRunCompletion()
        {
            if (!_isRunActive) return;

            // Check if bot fell too far below the course
            if (transform.position.y < -50f)
            {
                HandleBotDestruction("Fell off the course");
                return;
            }

            // Check if bot has been stuck for too long
            float currentDistance = transform.position.x - _startPosition.x;
            if (currentDistance > _furthestDistance)
            {
                _furthestDistance = currentDistance;
                _currentRunStats.LastProgressTime = Time.time;
            }
            else if (Time.time - _currentRunStats.LastProgressTime > 10f)
            {
                HandleBotDestruction("No forward progress for 10 seconds");
                return;
            }

            // Check for other completion conditions (could be extended)
        }
        #endregion

        #region Statistics & Helpers
        /// <summary>
        /// Update run statistics every frame
        /// </summary>
        private void UpdateRunStatistics()
        {
            if (!_isRunActive) return;

            // Update distance
            float currentDistance = transform.position.x - _startPosition.x;
            if (currentDistance > _furthestDistance)
            {
                _furthestDistance = currentDistance;
                _currentRunStats.DistanceTraveled = _furthestDistance;
                _currentRunStats.LastProgressTime = Time.time;
            }

            // Update survival time
            _currentRunStats.SurvivalTime = Time.time - _runStartTime;
        }

        /// <summary>
        /// Get current run statistics
        /// </summary>
        /// <returns>Current run statistics object</returns>
        public BotRunStatistics GetRunStatistics()
        {
            return _currentRunStats;
        }

        /// <summary>
        /// Calculate obstacles avoided (obstacles detected but not hit)
        /// </summary>
        private int CalculateObstaclesAvoided()
        {
            // This would need to track detected vs hit obstacles
            // For now, estimate based on distance and typical obstacle density
            int estimatedObstacles = Mathf.FloorToInt(_currentRunStats.DistanceTraveled / 20f);
            return Mathf.Max(0, estimatedObstacles - _currentRunStats.ObstaclesHit);
        }

        /// <summary>
        /// Get maximum health based on bot configuration
        /// </summary>
        private int GetMaxHealth()
        {
            int baseHealth = 100;
            
            // Archetype modifiers
            switch (_currentArchetype)
            {
                case BotArchetype.Tank:
                    baseHealth += 50;
                    break;
                case BotArchetype.Speed:
                    baseHealth -= 25;
                    break;
            }
            
            // Part modifiers
            foreach (var part in _equippedParts)
            {
                baseHealth += part.HealthModifier;
            }
            
            return Mathf.Max(25, baseHealth);
        }
        #endregion

        #region Performance Optimization
        /// <summary>
        /// Enable performance mode (reduced visual effects, simplified AI)
        /// </summary>
        public void EnablePerformanceMode()
        {
            _performanceModeEnabled = true;
            
            // Reduce AI decision frequency
            _reactionTime *= 1.5f;
            _decisionDistance *= 0.8f;
            
            // Reduce visual effects
            if (_animator != null)
            {
                _animator.enabled = false;
            }
            
            Debug.Log("[BotController] Performance mode enabled");
        }

        /// <summary>
        /// Disable performance mode (restore full quality)
        /// </summary>
        public void DisablePerformanceMode()
        {
            _performanceModeEnabled = false;
            
            // Restore AI parameters
            ApplyArchetypeModifiers(); // This resets reaction time and decision distance
            
            // Restore visual effects
            if (_animator != null)
            {
                _animator.enabled = true;
            }
            
            Debug.Log("[BotController] Performance mode disabled");
        }
        #endregion

        #region Helper Methods for AI
        private string DetermineObstacleType(Collider2D obstacle)
        {
            var obstacleData = obstacle.GetComponent<Course.ObstacleData>();
            return obstacleData != null ? obstacleData.ObstacleType : "Unknown";
        }

        private AIDecisionType DetermineRequiredAction(Collider2D obstacle)
        {
            var obstacleData = obstacle.GetComponent<Course.ObstacleData>();
            if (obstacleData == null) return AIDecisionType.Jump;

            return obstacleData.RequiredAction;
        }

        private float CalculateThreatLevel(Collider2D obstacle, float distance)
        {
            var obstacleData = obstacle.GetComponent<Course.ObstacleData>();
            float baseThreat = obstacleData != null ? obstacleData.BaseDamage / 10f : 0.5f;
            
            // Closer obstacles are more threatening
            float distanceModifier = Mathf.Lerp(2f, 0.1f, distance / _decisionDistance);
            
            return baseThreat * distanceModifier;
        }

        private string DetermineCollectibleType(Collider2D collectible)
        {
            var collectibleData = collectible.GetComponent<Course.CollectibleData>();
            return collectibleData != null ? collectibleData.CollectibleType : "Scrap";
        }

        private int GetCollectibleValue(Collider2D collectible)
        {
            var collectibleData = collectible.GetComponent<Course.CollectibleData>();
            return collectibleData != null ? collectibleData.ScrapValue : 1;
        }

        private float CalculateCollectiblePriority(Collider2D collectible, float distance)
        {
            int value = GetCollectibleValue(collectible);
            float basePriority = value / 10f;
            
            // Closer collectibles have higher priority
            float distanceModifier = Mathf.Lerp(1f, 0.2f, distance / _decisionDistance);
            
            return basePriority * distanceModifier * _collectiblePriority;
        }

        private bool IsCollectibleSafe(CollectibleInfo collectible)
        {
            // Check if pursuing this collectible would put bot in danger
            foreach (var obstacle in _detectedObstacles)
            {
                if (obstacle.Distance < collectible.Distance + 2f)
                {
                    return false; // Too risky
                }
            }
            return true;
        }

        private float GetActionDuration(AIDecisionType actionType)
        {
            switch (actionType)
            {
                case AIDecisionType.Jump:
                    return 1.0f;
                case AIDecisionType.Slide:
                    return _slideDistance / _currentSpeed;
                case AIDecisionType.Dash:
                    return 0.5f;
                default:
                    return 0.5f;
            }
        }

        private IEnumerator ApplyObstacleEffect(Course.ObstacleData obstacle)
        {
            // Apply temporary effects based on obstacle type
            float originalSpeed = _currentSpeed;
            
            switch (obstacle.ObstacleType.ToLower())
            {
                case "slowdown":
                    _currentSpeed *= 0.5f;
                    yield return new WaitForSeconds(2f);
                    break;
                    
                case "stun":
                    _currentSpeed = 0f;
                    yield return new WaitForSeconds(1f);
                    break;
            }
            
            _currentSpeed = originalSpeed;
        }

        private IEnumerator ApplyPowerUpEffect(Course.PowerUpData powerUp)
        {
            // Apply temporary power-up effects
            switch (powerUp.PowerUpType.ToLower())
            {
                case "speed":
                    _currentSpeed *= 1.5f;
                    yield return new WaitForSeconds(5f);
                    ApplyArchetypeModifiers(); // Reset to base speed
                    break;
                    
                case "shield":
                    // Temporary invulnerability would be handled here
                    yield return new WaitForSeconds(3f);
                    break;
                    
                case "magnet":
                    _collectiblePriority = 1.0f;
                    _decisionDistance *= 1.5f;
                    yield return new WaitForSeconds(8f);
                    UpdateAIBehavior(); // Reset to base values
                    break;
            }
        }
        #endregion

        #region Debug Visualization
        private void OnDrawGizmosSelected()
        {
            // Draw AI scan range
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector2.right * _decisionDistance);
            
            // Draw ground check
            if (_groundCheckPoint != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
            }
            
            // Draw detected obstacles
            Gizmos.color = Color.red;
            foreach (var obstacle in _detectedObstacles)
            {
                if (obstacle.Transform != null)
                {
                    Gizmos.DrawWireCube(obstacle.Transform.position, Vector3.one);
                }
            }
            
            // Draw detected collectibles
            Gizmos.color = Color.blue;
            foreach (var collectible in _detectedCollectibles)
            {
                if (collectible.Transform != null)
                {
                    Gizmos.DrawWireSphere(collectible.Transform.position, 0.5f);
                }
            }
        }
        #endregion
    }
}