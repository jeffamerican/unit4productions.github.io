/**
 * BotInc Analytics Dashboard
 * Real-time analytics for viral growth tracking
 */

class AnalyticsDashboard {
    constructor() {
        this.metrics = {
            sessions: 0,
            gameStarts: 0,
            achievements: 0,
            socialShares: 0,
            averageSessionTime: 0,
            viralCoefficient: 0
        };
        
        this.gameMetrics = {
            'signal-breach': { sessions: 0, completions: 0, avgScore: 0, shares: 0 },
            'neural-nexus': { sessions: 0, completions: 0, avgScore: 0, shares: 0 },
            'dot-conquest': { sessions: 0, completions: 0, avgScore: 0, shares: 0 },
            'chain-cascade': { sessions: 0, completions: 0, avgScore: 0, shares: 0 },
            'reflex-runner': { sessions: 0, completions: 0, avgScore: 0, shares: 0 }
        };
        
        this.engagementFunnels = {
            landingPage: 0,
            gameClick: 0,
            gameStart: 0,
            firstAchievement: 0,
            socialShare: 0,
            return: 0
        };
        
        this.initializeTracking();
        this.setupRealTimeUpdates();
    }
    
    initializeTracking() {
        // Track page visits
        this.trackPageVisit();
        
        // Track scroll depth for engagement
        this.trackScrollDepth();
        
        // Track time on site
        this.startSessionTimer();
        
        // Track game interactions
        this.trackGameClicks();
        
        // Track social interactions
        this.trackSocialInteractions();
    }
    
    trackPageVisit() {
        this.metrics.sessions++;
        this.engagementFunnels.landingPage++;
        
        if (typeof gtag !== 'undefined') {
            gtag('event', 'page_view', {
                'event_category': 'engagement',
                'event_label': window.location.pathname
            });
        }
        
        this.updateLocalStorage();
    }
    
    trackScrollDepth() {
        let maxScroll = 0;
        let milestones = [25, 50, 75, 100];
        
        window.addEventListener('scroll', () => {
            const scrollPercent = Math.round((window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100);
            
            if (scrollPercent > maxScroll) {
                maxScroll = scrollPercent;
                
                milestones.forEach(milestone => {
                    if (scrollPercent >= milestone && maxScroll < milestone) {
                        this.trackEngagementEvent('scroll_depth', { depth: milestone });
                    }
                });
            }
        });
    }
    
    startSessionTimer() {
        this.sessionStart = Date.now();
        
        // Track session length every 30 seconds
        setInterval(() => {
            const sessionLength = Math.floor((Date.now() - this.sessionStart) / 1000);
            this.metrics.averageSessionTime = sessionLength;
            
            if (sessionLength > 0 && sessionLength % 30 === 0) {
                this.trackEngagementEvent('session_milestone', { time: sessionLength });
            }
        }, 30000);
        
        // Track session end
        window.addEventListener('beforeunload', () => {
            const sessionLength = Math.floor((Date.now() - this.sessionStart) / 1000);
            this.trackEngagementEvent('session_end', { 
                totalTime: sessionLength,
                bounceRate: sessionLength < 30 ? 1 : 0
            });
        });
    }
    
    trackGameClicks() {
        document.addEventListener('click', (event) => {
            const gameLink = event.target.closest('a[href*="/games/"]');
            if (gameLink) {
                const gameName = this.extractGameName(gameLink.href);
                this.engagementFunnels.gameClick++;
                
                this.trackEngagementEvent('game_click', { 
                    game: gameName,
                    source: 'homepage'
                });
                
                if (this.gameMetrics[gameName]) {
                    this.gameMetrics[gameName].sessions++;
                }
            }
        });
    }
    
    trackSocialInteractions() {
        // Track social share buttons when they're clicked
        document.addEventListener('click', (event) => {
            if (event.target.closest('.social-share-btn')) {
                const platform = event.target.dataset.platform;
                this.metrics.socialShares++;
                this.engagementFunnels.socialShare++;
                
                this.trackEngagementEvent('social_share', { 
                    platform: platform,
                    content: 'homepage'
                });
            }
        });
    }
    
    trackEngagementEvent(eventType, data = {}) {
        if (typeof trackEngagement !== 'undefined') {
            trackEngagement(eventType, data);
        }
        
        this.updateViralCoefficient();
        this.updateLocalStorage();
    }
    
    updateViralCoefficient() {
        // Simple viral coefficient calculation
        // (Social Shares + Game Completions) / Total Sessions
        const totalPositiveActions = this.metrics.socialShares + this.metrics.achievements;
        this.metrics.viralCoefficient = this.metrics.sessions > 0 ? 
            (totalPositiveActions / this.metrics.sessions).toFixed(3) : 0;
    }
    
    extractGameName(url) {
        const match = url.match(/\/games\/([^\/]+)\//);
        return match ? match[1] : 'unknown';
    }
    
    updateLocalStorage() {
        localStorage.setItem('botinc_analytics', JSON.stringify({
            metrics: this.metrics,
            gameMetrics: this.gameMetrics,
            engagementFunnels: this.engagementFunnels,
            lastUpdate: Date.now()
        }));
    }
    
    loadFromLocalStorage() {
        const stored = localStorage.getItem('botinc_analytics');
        if (stored) {
            try {
                const data = JSON.parse(stored);
                this.metrics = { ...this.metrics, ...data.metrics };
                this.gameMetrics = { ...this.gameMetrics, ...data.gameMetrics };
                this.engagementFunnels = { ...this.engagementFunnels, ...data.engagementFunnels };
            } catch (e) {
                console.warn('Failed to load analytics data from localStorage');
            }
        }
    }
    
    setupRealTimeUpdates() {
        // Update dashboard every 5 seconds if dashboard exists
        setInterval(() => {
            this.updateDashboardDisplay();
        }, 5000);
    }
    
    updateDashboardDisplay() {
        const dashboard = document.getElementById('analytics-dashboard');
        if (!dashboard) return;
        
        // Update metrics display
        const metricsHTML = `
            <div class="analytics-grid">
                <div class="metric-card">
                    <h3>Sessions</h3>
                    <span class="metric-value">${this.metrics.sessions}</span>
                </div>
                <div class="metric-card">
                    <h3>Game Starts</h3>
                    <span class="metric-value">${this.metrics.gameStarts}</span>
                </div>
                <div class="metric-card">
                    <h3>Social Shares</h3>
                    <span class="metric-value">${this.metrics.socialShares}</span>
                </div>
                <div class="metric-card">
                    <h3>Viral Coefficient</h3>
                    <span class="metric-value">${this.metrics.viralCoefficient}</span>
                </div>
                <div class="metric-card">
                    <h3>Avg Session Time</h3>
                    <span class="metric-value">${Math.floor(this.metrics.averageSessionTime / 60)}m ${this.metrics.averageSessionTime % 60}s</span>
                </div>
                <div class="metric-card">
                    <h3>Achievements</h3>
                    <span class="metric-value">${this.metrics.achievements}</span>
                </div>
            </div>
            
            <div class="funnel-analysis">
                <h4>Engagement Funnel</h4>
                <div class="funnel-step">
                    <span>Landing Page:</span> <strong>${this.engagementFunnels.landingPage}</strong>
                </div>
                <div class="funnel-step">
                    <span>Game Click:</span> <strong>${this.engagementFunnels.gameClick}</strong>
                    <small>(${((this.engagementFunnels.gameClick / this.engagementFunnels.landingPage) * 100).toFixed(1)}%)</small>
                </div>
                <div class="funnel-step">
                    <span>Game Start:</span> <strong>${this.engagementFunnels.gameStart}</strong>
                    <small>(${((this.engagementFunnels.gameStart / this.engagementFunnels.gameClick) * 100).toFixed(1)}%)</small>
                </div>
                <div class="funnel-step">
                    <span>Social Share:</span> <strong>${this.engagementFunnels.socialShare}</strong>
                    <small>(${((this.engagementFunnels.socialShare / this.engagementFunnels.gameStart) * 100).toFixed(1)}%)</small>
                </div>
            </div>
        `;
        
        dashboard.innerHTML = metricsHTML;
    }
    
    exportData() {
        return {
            metrics: this.metrics,
            gameMetrics: this.gameMetrics,
            engagementFunnels: this.engagementFunnels,
            timestamp: new Date().toISOString()
        };
    }
}

// Initialize analytics dashboard
const botincAnalytics = new AnalyticsDashboard();
botincAnalytics.loadFromLocalStorage();

// Export for global access
window.botincAnalytics = botincAnalytics;