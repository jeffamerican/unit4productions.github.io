/**
 * BotInc Viral Integration System
 * Comprehensive integration and orchestration of all viral growth systems
 */

class ViralIntegration {
    constructor() {
        this.systems = {
            analytics: null,
            viral: null,
            community: null,
            leaderboards: null,
            mobile: null,
            abTesting: null,
            influencer: null
        };
        
        this.integrationStatus = {
            initialized: false,
            systemsLoaded: 0,
            totalSystems: 7,
            errors: []
        };
        
        this.viralLoops = new Map();
        this.crossSystemEvents = new Map();
        
        this.initialize();
    }
    
    initialize() {
        this.waitForSystemsToLoad();
        this.setupCrossSystemIntegration();
        this.initializeViralLoops();
        this.setupEventOrchestration();
        this.monitorSystemHealth();
    }
    
    waitForSystemsToLoad() {
        const checkSystems = () => {
            // Check if all systems are loaded
            this.systems.analytics = window.botincAnalytics || null;
            this.systems.viral = window.viralMechanics || null;
            this.systems.community = window.communitySystem || null;
            // this.systems.leaderboards = window.leaderboardSystem || null; // Removed
            this.systems.mobile = window.mobileOptimization || null;
            this.systems.abTesting = window.abTesting || null;
            this.systems.influencer = window.influencerTools || null;
            
            this.integrationStatus.systemsLoaded = Object.values(this.systems).filter(s => s !== null).length;
            
            if (this.integrationStatus.systemsLoaded >= 5) { // Minimum viable systems
                this.finalizeIntegration();
            } else if (this.integrationStatus.systemsLoaded < 3) {
                // Retry after delay
                setTimeout(checkSystems, 1000);
            }
        };
        
        // Initial check
        setTimeout(checkSystems, 500);
        
        // Backup check
        setTimeout(() => {
            if (!this.integrationStatus.initialized) {
                this.finalizeIntegration(); // Proceed with whatever systems are available
            }
        }, 5000);
    }
    
    finalizeIntegration() {
        this.integrationStatus.initialized = true;
        
        this.connectSystems();
        this.initializeViralLoops();
        this.setupAutomation();
        this.startViralOptimization();
        
        console.log('🚀 Viral Integration System initialized:', {
            systemsLoaded: this.integrationStatus.systemsLoaded,
            totalSystems: this.integrationStatus.totalSystems,
            systems: Object.keys(this.systems).filter(k => this.systems[k] !== null)
        });
        
        this.trackIntegrationEvent('viral_integration_complete', {
            systemsLoaded: this.integrationStatus.systemsLoaded,
            loadTime: Date.now()
        });
    }
    
    connectSystems() {
        // Connect analytics to all other systems
        if (this.systems.analytics && this.systems.viral) {
            this.systems.viral.analytics = this.systems.analytics;
        }
        
        if (this.systems.analytics && this.systems.community) {
            this.systems.community.analytics = this.systems.analytics;
        }
        
        // Connect viral mechanics to leaderboards
        if (this.systems.viral && this.systems.leaderboards) {
            this.systems.viral.leaderboards = this.systems.leaderboards;
            this.systems.leaderboards.viral = this.systems.viral;
        }
        
        // Connect community system to viral mechanics
        if (this.systems.community && this.systems.viral) {
            this.systems.community.viral = this.systems.viral;
            this.systems.viral.community = this.systems.community;
        }
        
        // Connect A/B testing to all systems
        if (this.systems.abTesting) {
            Object.keys(this.systems).forEach(key => {
                if (this.systems[key] && key !== 'abTesting') {
                    this.systems[key].abTesting = this.systems.abTesting;
                }
            });
        }
    }
    
    initializeViralLoops() {
        // Define and activate viral growth loops
        
        // Social Sharing Loop
        this.createViralLoop('social_sharing', {
            trigger: 'high_score_achieved',
            actions: [
                'show_achievement_notification',
                'trigger_social_share_suggestion',
                'offer_bonus_points_for_sharing',
                'track_share_conversion'
            ],
            feedback: 'increase_share_incentives_if_low_conversion'
        });
        
        // Email Capture Loop
        this.createViralLoop('email_capture', {
            trigger: 'game_engagement_high',
            actions: [
                'show_email_capture_popup',
                'offer_exclusive_content',
                'track_capture_rate',
                'send_welcome_sequence'
            ],
            feedback: 'optimize_timing_based_on_conversion'
        });
        
        // Competitive Loop
        this.createViralLoop('competitive', {
            trigger: 'score_submitted',
            actions: [
                'update_leaderboards',
                'notify_rank_change',
                'suggest_challenge_friends',
                'promote_tournaments'
            ],
            feedback: 'adjust_difficulty_based_on_participation'
        });
        
        // Achievement Loop
        this.createViralLoop('achievement', {
            trigger: 'milestone_reached',
            actions: [
                'unlock_achievement',
                'show_progress_to_next',
                'encourage_sharing',
                'offer_next_challenge'
            ],
            feedback: 'balance_difficulty_for_retention'
        });
        
        // Referral Loop
        this.createViralLoop('referral', {
            trigger: 'player_satisfaction_high',
            actions: [
                'generate_referral_link',
                'offer_mutual_benefits',
                'track_referral_success',
                'reward_successful_referrers'
            ],
            feedback: 'optimize_referral_incentives'
        });
    }
    
    createViralLoop(name, config) {
        this.viralLoops.set(name, {
            ...config,
            active: true,
            metrics: {
                triggered: 0,
                completed: 0,
                conversionRate: 0,
                lastTriggered: null
            }
        });
        
        // Setup event listeners for this loop
        this.setupLoopEventListeners(name, config);
    }
    
    setupLoopEventListeners(loopName, config) {
        // Listen for trigger events
        document.addEventListener(config.trigger, (event) => {
            this.executeViralLoop(loopName, event.detail);
        });
        
        // Also listen for common game events that might trigger loops
        const commonTriggers = {
            'high_score_achieved': ['gameScoreSubmitted', 'achievementUnlocked'],
            'game_engagement_high': ['gameStart', 'levelCompleted'],
            'score_submitted': ['gameCompleted', 'scoreSubmitted'],
            'milestone_reached': ['levelUp', 'achievementUnlocked'],
            'player_satisfaction_high': ['gameCompleted', 'positiveRating']
        };
        
        if (commonTriggers[config.trigger]) {
            commonTriggers[config.trigger].forEach(eventName => {
                document.addEventListener(eventName, (event) => {
                    this.executeViralLoop(loopName, event.detail);
                });
            });
        }
    }
    
    executeViralLoop(loopName, eventData) {
        const loop = this.viralLoops.get(loopName);
        if (!loop || !loop.active) return;
        
        loop.metrics.triggered++;
        loop.metrics.lastTriggered = Date.now();
        
        // Execute actions in sequence with delays
        loop.actions.forEach((action, index) => {
            setTimeout(() => {
                this.executeLoopAction(action, eventData, loopName);
            }, index * 500); // Stagger actions
        });
        
        this.trackViralLoopExecution(loopName, eventData);
    }
    
    executeLoopAction(action, eventData, loopName) {
        switch (action) {
            case 'show_achievement_notification':
                if (this.systems.viral) {
                    this.systems.viral.showAchievementNotification(eventData);
                }
                break;
                
            case 'trigger_social_share_suggestion':
                if (this.systems.viral) {
                    setTimeout(() => {
                        this.systems.viral.suggestShare({
                            type: 'score',
                            score: eventData.score,
                            game: eventData.game
                        });
                    }, 2000);
                }
                break;
                
            case 'show_email_capture_popup':
                if (this.systems.community && !this.systems.community.emailCaptureActive) {
                    this.systems.community.showGameCompletionCapture(eventData.game, eventData.score);
                }
                break;
                
            case 'update_leaderboards':
                if (this.systems.leaderboards && eventData.score) {
                    this.systems.leaderboards.submitScore(eventData.game, eventData.score, 'Player');
                }
                break;
                
            case 'unlock_achievement':
                if (this.systems.viral && eventData.achievement) {
                    this.systems.viral.checkAchievement(eventData.achievement, eventData.value);
                }
                break;
                
            case 'generate_referral_link':
                if (this.systems.influencer) {
                    this.systems.influencer.generateReferralLink();
                }
                break;
                
            default:
                console.log(`Executing viral action: ${action}`, eventData);
        }
        
        // Update loop completion metrics
        const loop = this.viralLoops.get(loopName);
        if (loop) {
            loop.metrics.completed++;
            loop.metrics.conversionRate = (loop.metrics.completed / loop.metrics.triggered) * 100;
        }
    }
    
    setupAutomation() {
        // Automated optimization based on performance data
        setInterval(() => {
            this.optimizeViralLoops();
        }, 300000); // Every 5 minutes
        
        // Automated A/B test analysis
        if (this.systems.abTesting) {
            setInterval(() => {
                this.analyzeABTests();
            }, 600000); // Every 10 minutes
        }
        
        // Automated email capture optimization
        if (this.systems.community) {
            setInterval(() => {
                this.optimizeEmailCapture();
            }, 180000); // Every 3 minutes
        }
    }
    
    optimizeViralLoops() {
        this.viralLoops.forEach((loop, name) => {
            // Optimize based on conversion rates
            if (loop.metrics.triggered > 10) { // Minimum sample size
                if (loop.metrics.conversionRate < 5) {
                    // Low conversion rate - adjust strategy
                    this.adjustLoopStrategy(name, 'increase_incentives');
                } else if (loop.metrics.conversionRate > 20) {
                    // High conversion rate - amplify
                    this.adjustLoopStrategy(name, 'increase_frequency');
                }
            }
        });
    }
    
    adjustLoopStrategy(loopName, adjustment) {
        const loop = this.viralLoops.get(loopName);
        if (!loop) return;
        
        switch (adjustment) {
            case 'increase_incentives':
                // Increase rewards for social sharing, email signup, etc.
                this.increaseViralIncentives(loopName);
                break;
                
            case 'increase_frequency':
                // Trigger loop more frequently
                loop.triggerThreshold = (loop.triggerThreshold || 100) * 0.8;
                break;
                
            case 'decrease_frequency':
                // Trigger loop less frequently to avoid fatigue
                loop.triggerThreshold = (loop.triggerThreshold || 100) * 1.2;
                break;
        }
        
        this.trackViralOptimization(loopName, adjustment);
    }
    
    increaseViralIncentives(loopName) {
        if (loopName === 'social_sharing' && this.systems.viral) {
            // Increase social sharing rewards
            const currentReward = this.systems.viral.shareRewardPoints || 25;
            this.systems.viral.shareRewardPoints = Math.min(currentReward * 1.2, 100);
        }
        
        if (loopName === 'email_capture' && this.systems.community) {
            // Improve email capture incentives
            this.systems.community.emailIncentiveLevel = (this.systems.community.emailIncentiveLevel || 1) + 1;
        }
    }
    
    analyzeABTests() {
        if (!this.systems.abTesting) return;
        
        // Get results for all active tests
        this.systems.abTesting.activeTests.forEach((test, testId) => {
            const results = this.systems.abTesting.getTestResults(testId);
            
            if (results && results.participants >= this.systems.abTesting.config.minSampleSize) {
                // Find winning variant
                let winningVariant = null;
                let winningRate = 0;
                
                Object.entries(results.variants).forEach(([variant, data]) => {
                    if (data.conversionRate > winningRate && data.significant) {
                        winningVariant = variant;
                        winningRate = data.conversionRate;
                    }
                });
                
                if (winningVariant && winningVariant !== 'control') {
                    this.implementWinningVariant(testId, winningVariant, results);
                }
            }
        });
    }
    
    implementWinningVariant(testId, variant, results) {
        console.log(`🏆 A/B Test Winner: ${testId} - ${variant}`, results);
        
        // Auto-implement winning strategies
        switch (testId) {
            case 'email_capture_timing':
                if (this.systems.community) {
                    this.systems.community.emailCaptureOptimalTiming = variant;
                }
                break;
                
            case 'social_share_incentive':
                if (this.systems.viral) {
                    this.systems.viral.optimalShareIncentive = variant;
                }
                break;
        }
        
        this.trackABTestWinner(testId, variant, results);
    }
    
    optimizeEmailCapture() {
        if (!this.systems.community || !this.systems.analytics) return;
        
        const captureRate = this.systems.community.getSubscriptionSources();
        const totalCaptures = Object.values(captureRate).reduce((a, b) => a + b, 0);
        
        if (totalCaptures > 50) { // Minimum data threshold
            // Find best performing capture method
            const bestMethod = Object.entries(captureRate).reduce((a, b) => 
                captureRate[a[0]] > captureRate[b[0]] ? a : b
            )[0];
            
            // Optimize timing and messaging for best method
            this.optimizeCaptureMethod(bestMethod);
        }
    }
    
    optimizeCaptureMethod(method) {
        if (!this.systems.community) return;
        
        switch (method) {
            case 'exit_intent':
                this.systems.community.exitIntentDelay = Math.max(
                    (this.systems.community.exitIntentDelay || 0) - 500, 0
                );
                break;
                
            case 'scroll_trigger':
                this.systems.community.scrollTriggerThreshold = Math.max(
                    (this.systems.community.scrollTriggerThreshold || 70) - 5, 50
                );
                break;
                
            case 'game_completion':
                this.systems.community.gameCompletionDelay = Math.max(
                    (this.systems.community.gameCompletionDelay || 2000) - 250, 1000
                );
                break;
        }
    }
    
    startViralOptimization() {
        // Real-time viral coefficient optimization
        setInterval(() => {
            this.calculateAndOptimizeViralCoefficient();
        }, 60000); // Every minute
        
        // Cross-system event optimization
        this.optimizeCrossSystemEvents();
    }
    
    calculateAndOptimizeViralCoefficient() {
        if (!this.systems.analytics) return;
        
        const currentCoefficient = parseFloat(this.systems.analytics.metrics.viralCoefficient) || 0;
        const targetCoefficient = 1.2; // Goal: Each user brings in 1.2 new users
        
        if (currentCoefficient < targetCoefficient) {
            // Increase viral pressure
            this.increaseViralPressure();
        } else if (currentCoefficient > targetCoefficient * 1.5) {
            // Reduce viral fatigue
            this.reduceViralPressure();
        }
        
        this.trackViralCoefficient(currentCoefficient, targetCoefficient);
    }
    
    increaseViralPressure() {
        // Increase frequency and intensity of viral mechanisms
        
        if (this.systems.viral) {
            // Show more share prompts
            this.systems.viral.sharePromptFrequency = (this.systems.viral.sharePromptFrequency || 1) * 1.1;
        }
        
        if (this.systems.community) {
            // More aggressive email capture
            this.systems.community.captureAggressiveness = (this.systems.community.captureAggressiveness || 1) * 1.1;
        }
        
        // Leaderboard system removed
    }
    
    reduceViralPressure() {
        // Reduce frequency to prevent fatigue
        
        if (this.systems.viral) {
            this.systems.viral.sharePromptFrequency = Math.max(
                (this.systems.viral.sharePromptFrequency || 1) * 0.9, 0.5
            );
        }
        
        if (this.systems.community) {
            this.systems.community.captureAggressiveness = Math.max(
                (this.systems.community.captureAggressiveness || 1) * 0.9, 0.5
            );
        }
    }
    
    optimizeCrossSystemEvents() {
        // Setup enhanced cross-system event handling
        
        // When someone shares, also prompt for email if not captured
        this.setupCrossSystemEvent('social_share', 'email_capture_delay', (eventData) => {
            if (this.systems.community && !this.isEmailCaptured()) {
                setTimeout(() => {
                    this.systems.community.showScrollTriggeredCapture();
                }, 5000);
            }
        });
        
        // When someone joins leaderboard, prompt to share achievement
        this.setupCrossSystemEvent('leaderboard_entry', 'share_prompt', (eventData) => {
            if (this.systems.viral && eventData.rank <= 10) {
                setTimeout(() => {
                    this.systems.viral.suggestShare({
                        type: 'achievement',
                        achievementName: `Top ${eventData.rank} Player`,
                        score: eventData.score,
                        game: eventData.game
                    });
                }, 3000);
            }
        });
        
        // When email captured, encourage social follow
        this.setupCrossSystemEvent('email_captured', 'social_follow', (eventData) => {
            setTimeout(() => {
                this.showSocialFollowPrompt();
            }, 2000);
        });
    }
    
    setupCrossSystemEvent(triggerEvent, targetAction, handler) {
        this.crossSystemEvents.set(`${triggerEvent}_${targetAction}`, handler);
        
        document.addEventListener(triggerEvent, (event) => {
            handler(event.detail);
        });
    }
    
    isEmailCaptured() {
        return localStorage.getItem('botinc_email_captured') === 'true' ||
               (this.systems.community && this.systems.community.subscribers.length > 0);
    }
    
    showSocialFollowPrompt() {
        const prompt = document.createElement('div');
        prompt.className = 'social-follow-prompt';
        prompt.innerHTML = `
            <div class="prompt-content">
                <h4>🎉 Welcome to the Community!</h4>
                <p>Follow us on social media for daily gaming content and exclusive updates!</p>
                <div class="social-buttons">
                    <a href="https://twitter.com/botincgames" target="_blank" class="social-btn twitter">Twitter</a>
                    <a href="https://discord.gg/botincgames" target="_blank" class="social-btn discord">Discord</a>
                    <a href="https://youtube.com/@botincgames" target="_blank" class="social-btn youtube">YouTube</a>
                </div>
                <button onclick="this.parentNode.parentNode.remove()" class="close-btn">Continue Gaming</button>
            </div>
        `;
        
        prompt.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.8);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10003;
        `;
        
        document.body.appendChild(prompt);
        
        setTimeout(() => {
            if (prompt.parentNode) {
                prompt.parentNode.removeChild(prompt);
            }
        }, 15000);
    }
    
    monitorSystemHealth() {
        setInterval(() => {
            this.checkSystemHealth();
        }, 30000); // Every 30 seconds
    }
    
    checkSystemHealth() {
        const healthReport = {
            timestamp: Date.now(),
            systems: {},
            overall: 'healthy'
        };
        
        Object.entries(this.systems).forEach(([name, system]) => {
            healthReport.systems[name] = {
                loaded: system !== null,
                functional: this.isSystemFunctional(name, system),
                lastError: null
            };
            
            if (!healthReport.systems[name].functional) {
                healthReport.overall = 'degraded';
            }
        });
        
        // Log health issues
        if (healthReport.overall !== 'healthy') {
            console.warn('🚨 Viral Integration Health Issue:', healthReport);
        }
        
        this.trackSystemHealth(healthReport);
    }
    
    isSystemFunctional(name, system) {
        if (!system) return false;
        
        try {
            switch (name) {
                case 'analytics':
                    return system.metrics && typeof system.trackEngagementEvent === 'function';
                case 'viral':
                    return system.achievements && typeof system.checkAchievement === 'function';
                case 'community':
                    return system.subscribers && typeof system.addSubscriber === 'function';
                case 'leaderboards':
                    return system.games && typeof system.submitScore === 'function';
                default:
                    return true; // Assume functional if we can't test
            }
        } catch (e) {
            return false;
        }
    }
    
    // Analytics and tracking methods
    trackIntegrationEvent(eventName, data) {
        if (this.systems.analytics && this.systems.analytics.trackEngagementEvent) {
            this.systems.analytics.trackEngagementEvent(eventName, data);
        }
        
        console.log(`📊 Viral Integration Event: ${eventName}`, data);
    }
    
    trackViralLoopExecution(loopName, eventData) {
        this.trackIntegrationEvent('viral_loop_executed', {
            loop: loopName,
            eventData: eventData
        });
    }
    
    trackViralOptimization(loopName, adjustment) {
        this.trackIntegrationEvent('viral_optimization', {
            loop: loopName,
            adjustment: adjustment
        });
    }
    
    trackABTestWinner(testId, variant, results) {
        this.trackIntegrationEvent('ab_test_winner', {
            testId: testId,
            winningVariant: variant,
            conversionRate: results.variants[variant].conversionRate
        });
    }
    
    trackViralCoefficient(current, target) {
        this.trackIntegrationEvent('viral_coefficient_check', {
            current: current,
            target: target,
            performance: current >= target ? 'meeting_target' : 'below_target'
        });
    }
    
    trackSystemHealth(healthReport) {
        this.trackIntegrationEvent('system_health_check', healthReport);
    }
    
    // Public API methods for external systems
    getIntegrationStatus() {
        return {
            ...this.integrationStatus,
            viralLoops: Object.fromEntries(
                Array.from(this.viralLoops.entries()).map(([name, loop]) => [
                    name, {
                        active: loop.active,
                        metrics: loop.metrics
                    }
                ])
            )
        };
    }
    
    triggerViralMoment(type, data) {
        // External API to trigger viral moments
        const event = new CustomEvent(type, { detail: data });
        document.dispatchEvent(event);
    }
    
    optimizeForGoal(goal) {
        // External API to optimize for specific goals
        switch (goal) {
            case 'email_growth':
                this.increaseEmailCaptureAggressiveness();
                break;
            case 'social_sharing':
                this.increaseSocialSharingIncentives();
                break;
            case 'user_retention':
                this.optimizeForRetention();
                break;
        }
    }
    
    increaseEmailCaptureAggressiveness() {
        if (this.systems.community) {
            this.systems.community.emailCaptureActive = false; // Reset to allow new attempts
            this.systems.community.createScrollTriggeredCapture();
        }
    }
    
    increaseSocialSharingIncentives() {
        if (this.systems.viral) {
            this.systems.viral.shareRewardPoints = (this.systems.viral.shareRewardPoints || 25) * 1.5;
        }
    }
    
    optimizeForRetention() {
        // Focus on achievement systems and progression
        if (this.systems.viral) {
            // Lower achievement thresholds temporarily
            this.systems.viral.achievements.forEach(achievement => {
                if (achievement.points < 100) {
                    achievement.threshold = (achievement.threshold || 1000) * 0.8;
                }
            });
        }
    }
    
    setupCrossSystemIntegration() {
        // Set up integration between different systems
        this.setupAnalyticsIntegration();
        this.setupCommunityIntegration();
        this.setupViralMechanicsIntegration();
        this.setupCrossSystemEventHandling();
    }
    
    setupAnalyticsIntegration() {
        // Integrate with analytics system
        if (this.systems.analytics) {
            this.systems.analytics.on('viralEvent', (data) => {
                this.handleViralAnalyticsEvent(data);
            });
        }
    }
    
    setupCommunityIntegration() {
        // Integrate with community system
        if (this.systems.community) {
            this.systems.community.on('userEngagement', (data) => {
                this.handleCommunityEngagement(data);
            });
        }
    }
    
    setupViralMechanicsIntegration() {
        // Integrate with viral mechanics system
        if (this.systems.viral) {
            this.systems.viral.on('viralTrigger', (data) => {
                this.handleViralTrigger(data);
            });
        }
    }
    
    setupCrossSystemEventHandling() {
        // Handle events that span multiple systems
        document.addEventListener('crossSystemEvent', (event) => {
            this.processCrossSystemEvent(event.detail);
        });
    }
    
    handleViralAnalyticsEvent(data) {
        // Process viral-related analytics events
        this.trackViralEvent('analytics', data);
    }
    
    handleCommunityEngagement(data) {
        // Process community engagement for viral potential
        this.trackViralEvent('community', data);
    }
    
    handleViralTrigger(data) {
        // Process viral mechanics triggers
        this.trackViralEvent('mechanics', data);
    }
    
    processCrossSystemEvent(eventData) {
        // Process events that involve multiple systems
        const { systems, event, data } = eventData;
        
        // Coordinate response across systems
        systems.forEach(system => {
            if (this.systems[system]) {
                this.systems[system].handleCrossSystemEvent(event, data);
            }
        });
    }
    
    trackViralEvent(source, data) {
        // Track viral events from different sources
        const viralEvent = {
            source,
            data,
            timestamp: Date.now(),
            viralCoefficient: this.calculateViralCoefficient(data)
        };
        
        this.crossSystemEvents.set(`${source}_${Date.now()}`, viralEvent);
        this.emitViralEvent(viralEvent);
    }
    
    calculateViralCoefficient(data) {
        // Calculate viral coefficient based on event data
        let coefficient = 0;
        
        if (data.engagement) coefficient += 0.3;
        if (data.sharing) coefficient += 0.4;
        if (data.retention) coefficient += 0.3;
        
        return Math.min(coefficient, 1.0);
    }
    
    emitViralEvent(event) {
        // Emit viral event to other systems
        const customEvent = new CustomEvent('viralEvent', {
            detail: event
        });
        document.dispatchEvent(customEvent);
    }
}

// Initialize viral integration when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Small delay to allow other systems to initialize first
    setTimeout(() => {
        window.viralIntegration = new ViralIntegration();
    }, 1000);
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ViralIntegration;
}