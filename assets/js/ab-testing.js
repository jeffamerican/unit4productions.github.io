/**
 * BotInc A/B Testing Framework
 * Advanced experimentation system for optimizing viral growth and user engagement
 */

class ABTesting {
    constructor() {
        this.userId = this.getOrCreateUserId();
        this.activeTests = new Map();
        this.results = this.loadResults();
        this.config = {
            trackingEnabled: true,
            debugMode: false,
            defaultTrafficAllocation: 0.5, // 50/50 split by default
            minSampleSize: 100,
            confidenceLevel: 0.95
        };
        
        this.initialize();
    }
    
    getOrCreateUserId() {
        let userId = localStorage.getItem('ab_testing_user_id');
        if (!userId) {
            userId = 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            localStorage.setItem('ab_testing_user_id', userId);
        }
        return userId;
    }
    
    initialize() {
        this.setupTests();
        this.loadActiveTests();
        
        // Setup automatic event tracking
        this.setupEventTracking();
        
        // Initialize with current page context
        this.trackPageView();
    }
    
    setupTests() {
        // Define A/B tests for different aspects of the site
        this.defineTest('homepage_hero_cta', {
            name: 'Homepage Hero CTA Button Text',
            variants: {
                control: { text: 'Play Signal Breach', style: 'btn-primary' },
                variant_a: { text: 'Start Hacking Now', style: 'btn-primary' },
                variant_b: { text: 'Begin Your Mission', style: 'btn-primary pulse-animation' },
                variant_c: { text: 'Play Now - FREE', style: 'btn-primary highlight-free' }
            },
            trafficAllocation: { control: 0.25, variant_a: 0.25, variant_b: 0.25, variant_c: 0.25 },
            goals: ['cta_click', 'game_start', 'session_length_60s'],
            active: true
        });
        
        this.defineTest('email_capture_timing', {
            name: 'Email Capture Popup Timing',
            variants: {
                control: { trigger: 'exit_intent', delay: 0 },
                variant_a: { trigger: 'time_delay', delay: 30000 }, // 30 seconds
                variant_b: { trigger: 'scroll_depth', delay: 0.75 }, // 75% scroll
                variant_c: { trigger: 'game_completion', delay: 0 }
            },
            trafficAllocation: { control: 0.25, variant_a: 0.25, variant_b: 0.25, variant_c: 0.25 },
            goals: ['email_signup', 'popup_shown', 'popup_dismissed'],
            active: true
        });
        
        this.defineTest('social_share_incentive', {
            name: 'Social Share Incentive Message',
            variants: {
                control: { message: 'Share your achievement!', reward: null },
                variant_a: { message: 'Share and get +50 bonus points!', reward: 50 },
                variant_b: { message: 'Share for exclusive achievement!', reward: 'exclusive_badge' },
                variant_c: { message: 'Challenge your friends!', reward: null }
            },
            trafficAllocation: { control: 0.25, variant_a: 0.25, variant_b: 0.25, variant_c: 0.25 },
            goals: ['social_share', 'share_click', 'viral_referral'],
            active: true
        });
        
        this.defineTest('game_completion_flow', {
            name: 'Game Completion Call-to-Action',
            variants: {
                control: { 
                    flow: ['show_score', 'share_button', 'play_again'], 
                    timing: [0, 2000, 4000] 
                },
                variant_a: { 
                    flow: ['show_score', 'achievement_unlock', 'email_capture', 'share_button'], 
                    timing: [0, 1000, 3000, 5000] 
                },
                variant_b: { 
                    flow: ['celebration_animation', 'show_score', 'leaderboard_position', 'share_button'], 
                    timing: [0, 1500, 3000, 4500] 
                }
            },
            trafficAllocation: { control: 0.33, variant_a: 0.33, variant_b: 0.34 },
            goals: ['email_signup', 'social_share', 'game_restart', 'session_extension'],
            active: true
        });
        
        this.defineTest('leaderboard_visibility', {
            name: 'Leaderboard Widget Visibility',
            variants: {
                control: { position: 'right_collapsed', autoShow: false, prominence: 'normal' },
                variant_a: { position: 'right_expanded', autoShow: true, prominence: 'highlighted' },
                variant_b: { position: 'bottom_sticky', autoShow: false, prominence: 'normal' },
                variant_c: { position: 'right_collapsed', autoShow: true, prominence: 'pulsing' }
            },
            trafficAllocation: { control: 0.25, variant_a: 0.25, variant_b: 0.25, variant_c: 0.25 },
            goals: ['leaderboard_view', 'competitive_game_start', 'score_improvement'],
            active: true
        });
    }
    
    defineTest(testId, config) {
        this.activeTests.set(testId, {
            id: testId,
            ...config,
            startDate: Date.now(),
            participants: new Map(),
            results: new Map()
        });
    }
    
    loadActiveTests() {
        const storedTests = localStorage.getItem('ab_testing_active');
        if (storedTests) {
            try {
                const tests = JSON.parse(storedTests);
                Object.entries(tests).forEach(([testId, assignment]) => {
                    if (this.activeTests.has(testId)) {
                        this.assignUserToVariant(testId, assignment.variant);
                    }
                });
            } catch (e) {
                console.warn('Failed to load active A/B tests');
            }
        }
        
        // Assign user to active tests they haven't been assigned to
        this.activeTests.forEach((test, testId) => {
            if (test.active && !this.isUserAssigned(testId)) {
                this.assignUserToTest(testId);
            }
        });
    }
    
    assignUserToTest(testId) {
        const test = this.activeTests.get(testId);
        if (!test || !test.active) return null;
        
        // Use deterministic assignment based on user ID for consistency
        const hash = this.hashString(this.userId + testId);
        const normalized = (hash % 10000) / 10000;
        
        let cumulativeWeight = 0;
        for (const [variant, weight] of Object.entries(test.trafficAllocation)) {
            cumulativeWeight += weight;
            if (normalized <= cumulativeWeight) {
                this.assignUserToVariant(testId, variant);
                return variant;
            }
        }
        
        // Fallback to control
        this.assignUserToVariant(testId, 'control');
        return 'control';
    }
    
    assignUserToVariant(testId, variant) {
        const test = this.activeTests.get(testId);
        if (!test) return;
        
        test.participants.set(this.userId, {
            variant: variant,
            assignedAt: Date.now(),
            events: []
        });
        
        // Store assignment
        this.saveAssignment(testId, variant);
        
        // Apply variant
        this.applyVariant(testId, variant);
        
        // Track assignment
        this.trackEvent('ab_test_assigned', {
            testId: testId,
            variant: variant
        });
    }
    
    applyVariant(testId, variant) {
        const test = this.activeTests.get(testId);
        if (!test) return;
        
        const variantConfig = test.variants[variant];
        if (!variantConfig) return;
        
        switch (testId) {
            case 'homepage_hero_cta':
                this.applyHeroCTAVariant(variantConfig);
                break;
                
            case 'email_capture_timing':
                this.applyEmailCaptureVariant(variantConfig);
                break;
                
            case 'social_share_incentive':
                this.applySocialShareVariant(variantConfig);
                break;
                
            case 'game_completion_flow':
                this.applyGameCompletionVariant(variantConfig);
                break;
                
            case 'leaderboard_visibility':
                this.applyLeaderboardVariant(variantConfig);
                break;
        }
    }
    
    applyHeroCTAVariant(config) {
        const ctaButton = document.querySelector('.hero-actions .btn-primary');
        if (ctaButton) {
            ctaButton.innerHTML = `<span>${config.text}</span><i class="icon-play"></i>`;
            ctaButton.className = `btn ${config.style}`;
            
            // Add additional styles for variants
            if (config.style.includes('pulse-animation')) {
                ctaButton.style.animation = 'pulse 2s infinite';
            }
            if (config.style.includes('highlight-free')) {
                ctaButton.style.background = 'linear-gradient(135deg, #4CAF50, #45a049)';
            }
        }
    }
    
    applyEmailCaptureVariant(config) {
        if (!window.communitySystem) return;
        
        // Modify community system behavior based on variant
        window.communitySystem.emailCaptureConfig = config;
        
        switch (config.trigger) {
            case 'time_delay':
                setTimeout(() => {
                    if (!window.communitySystem.emailCaptureActive) {
                        window.communitySystem.showScrollTriggeredCapture();
                    }
                }, config.delay);
                break;
                
            case 'scroll_depth':
                // Override scroll trigger threshold
                window.communitySystem.scrollTriggerThreshold = config.delay * 100;
                break;
        }
    }
    
    applySocialShareVariant(config) {
        if (!window.viralMechanics) return;
        
        // Store variant config for use in share dialogs
        window.viralMechanics.shareIncentiveConfig = config;
        
        // Override share templates if needed
        if (config.reward) {
            window.viralMechanics.socialShareTemplates.score = 
                config.message + ' ' + window.viralMechanics.socialShareTemplates.score;
        }
    }
    
    applyGameCompletionVariant(config) {
        // Store completion flow config for games to use
        window.abTestingCompletionFlow = config;
    }
    
    applyLeaderboardVariant(config) {
        const widget = document.getElementById('leaderboard-widget');
        if (!widget) return;
        
        switch (config.position) {
            case 'right_expanded':
                widget.classList.remove('collapsed');
                break;
                
            case 'bottom_sticky':
                widget.style.position = 'fixed';
                widget.style.bottom = '0';
                widget.style.right = '0';
                widget.style.left = '0';
                widget.style.top = 'auto';
                break;
        }
        
        if (config.prominence === 'highlighted') {
            widget.style.border = '2px solid #ffff00';
            widget.style.boxShadow = '0 0 20px rgba(255, 255, 0, 0.3)';
        }
        
        if (config.prominence === 'pulsing') {
            widget.style.animation = 'pulse 3s infinite';
        }
        
        if (config.autoShow) {
            setTimeout(() => {
                widget.classList.remove('collapsed');
            }, 3000);
        }
    }
    
    trackGoal(goalId, data = {}) {
        // Track goal completion for all active tests
        this.activeTests.forEach((test, testId) => {
            if (test.goals.includes(goalId) && this.isUserAssigned(testId)) {
                const participant = test.participants.get(this.userId);
                if (participant) {
                    participant.events.push({
                        goal: goalId,
                        timestamp: Date.now(),
                        data: data
                    });
                    
                    // Update test results
                    this.updateTestResults(testId, participant.variant, goalId, data);
                }
            }
        });
        
        // Track in analytics
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('ab_test_goal', {
                goal: goalId,
                data: data
            });
        }
    }
    
    updateTestResults(testId, variant, goalId, data) {
        const test = this.activeTests.get(testId);
        if (!test) return;
        
        if (!test.results.has(variant)) {
            test.results.set(variant, {
                participants: 0,
                goals: new Map(),
                conversionRate: 0
            });
        }
        
        const variantResults = test.results.get(variant);
        
        if (!variantResults.goals.has(goalId)) {
            variantResults.goals.set(goalId, 0);
        }
        
        variantResults.goals.set(goalId, variantResults.goals.get(goalId) + 1);
        variantResults.participants = test.participants.size;
        
        // Calculate conversion rate for primary goal (first in goals array)
        const primaryGoal = test.goals[0];
        if (variantResults.goals.has(primaryGoal)) {
            variantResults.conversionRate = (variantResults.goals.get(primaryGoal) / variantResults.participants) * 100;
        }
        
        this.saveResults();
    }
    
    setupEventTracking() {
        // Track common events that might be test goals
        
        // CTA clicks
        document.addEventListener('click', (event) => {
            const button = event.target.closest('a, button');
            if (button) {
                if (button.classList.contains('btn-primary')) {
                    this.trackGoal('cta_click', { element: button.textContent });
                }
                
                if (button.textContent.includes('Share') || button.classList.contains('share-btn')) {
                    this.trackGoal('share_click', { type: button.dataset.platform });
                }
                
                if (button.textContent.includes('Play') || button.href && button.href.includes('/games/')) {
                    this.trackGoal('game_click', { game: this.extractGameFromUrl(button.href) });
                }
            }
        });
        
        // Form submissions
        document.addEventListener('submit', (event) => {
            if (event.target.querySelector('input[type="email"]')) {
                this.trackGoal('email_signup', { form: event.target.id });
            }
        });
        
        // Game events
        document.addEventListener('gameStart', (event) => {
            this.trackGoal('game_start', { game: event.detail.game });
        });
        
        document.addEventListener('gameCompleted', (event) => {
            this.trackGoal('game_completion', event.detail);
        });
        
        document.addEventListener('gameScoreSubmitted', (event) => {
            this.trackGoal('score_submission', event.detail);
        });
        
        // Social sharing
        document.addEventListener('socialShare', (event) => {
            this.trackGoal('social_share', event.detail);
        });
        
        // Session length tracking
        setTimeout(() => {
            this.trackGoal('session_length_60s', { duration: 60 });
        }, 60000);
        
        setTimeout(() => {
            this.trackGoal('session_length_300s', { duration: 300 });
        }, 300000);
    }
    
    trackPageView() {
        this.trackGoal('page_view', { 
            url: window.location.pathname,
            referrer: document.referrer 
        });
    }
    
    isUserAssigned(testId) {
        const test = this.activeTests.get(testId);
        return test && test.participants.has(this.userId);
    }
    
    getUserVariant(testId) {
        const test = this.activeTests.get(testId);
        if (!test || !test.participants.has(this.userId)) {
            return null;
        }
        return test.participants.get(this.userId).variant;
    }
    
    getTestResults(testId) {
        const test = this.activeTests.get(testId);
        if (!test) return null;
        
        const results = {
            testId: testId,
            name: test.name,
            active: test.active,
            participants: test.participants.size,
            variants: {}
        };
        
        test.results.forEach((variantData, variant) => {
            results.variants[variant] = {
                participants: variantData.participants,
                conversionRate: variantData.conversionRate,
                goals: Object.fromEntries(variantData.goals),
                significant: this.isStatisticallySignificant(test, variant)
            };
        });
        
        return results;
    }
    
    isStatisticallySignificant(test, variant) {
        const variantResults = test.results.get(variant);
        const controlResults = test.results.get('control');
        
        if (!variantResults || !controlResults || 
            variantResults.participants < this.config.minSampleSize ||
            controlResults.participants < this.config.minSampleSize) {
            return false;
        }
        
        // Simple significance test (in production, use proper statistical testing)
        const variantRate = variantResults.conversionRate / 100;
        const controlRate = controlResults.conversionRate / 100;
        const improvementThreshold = 0.05; // 5% minimum improvement
        
        return Math.abs(variantRate - controlRate) > improvementThreshold;
    }
    
    exportResults() {
        const allResults = {};
        this.activeTests.forEach((test, testId) => {
            allResults[testId] = this.getTestResults(testId);
        });
        
        return {
            userId: this.userId,
            exportDate: new Date().toISOString(),
            tests: allResults,
            summary: this.generateSummary()
        };
    }
    
    generateSummary() {
        let totalParticipants = 0;
        let totalTests = 0;
        let significantTests = 0;
        
        this.activeTests.forEach((test) => {
            if (test.active) {
                totalTests++;
                totalParticipants += test.participants.size;
                
                if (Array.from(test.results.keys()).some(variant => 
                    this.isStatisticallySignificant(test, variant))) {
                    significantTests++;
                }
            }
        });
        
        return {
            totalTests,
            totalParticipants,
            significantTests,
            significanceRate: totalTests > 0 ? (significantTests / totalTests) * 100 : 0
        };
    }
    
    // Utility methods
    hashString(str) {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            const char = str.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash; // Convert to 32bit integer
        }
        return Math.abs(hash);
    }
    
    extractGameFromUrl(url) {
        const match = url.match(/\/games\/([^\/]+)\//);
        return match ? match[1] : 'unknown';
    }
    
    saveAssignment(testId, variant) {
        const assignments = JSON.parse(localStorage.getItem('ab_testing_active') || '{}');
        assignments[testId] = { variant, assignedAt: Date.now() };
        localStorage.setItem('ab_testing_active', JSON.stringify(assignments));
    }
    
    saveResults() {
        const resultsData = {};
        this.activeTests.forEach((test, testId) => {
            resultsData[testId] = {
                participants: test.participants.size,
                results: Object.fromEntries(
                    Array.from(test.results.entries()).map(([variant, data]) => [
                        variant,
                        {
                            participants: data.participants,
                            conversionRate: data.conversionRate,
                            goals: Object.fromEntries(data.goals)
                        }
                    ])
                )
            };
        });
        
        localStorage.setItem('ab_testing_results', JSON.stringify(resultsData));
    }
    
    loadResults() {
        const stored = localStorage.getItem('ab_testing_results');
        return stored ? JSON.parse(stored) : {};
    }
    
    trackEvent(eventName, data) {
        if (this.config.debugMode) {
            console.log(`A/B Test Event: ${eventName}`, data);
        }
        
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent(eventName, data);
        }
    }
}

// Initialize A/B testing system
document.addEventListener('DOMContentLoaded', () => {
    window.abTesting = new ABTesting();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ABTesting;
}