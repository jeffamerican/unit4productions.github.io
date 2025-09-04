/**
 * BotInc Influencer Partnership Tools
 * Tools for streamers, content creators, and influencers to showcase games
 */

class InfluencerTools {
    constructor() {
        this.partnerId = this.generatePartnerId();
        this.streamingMode = false;
        this.recordingMode = false;
        this.customOverlays = new Map();
        this.analyticsData = {
            impressions: 0,
            clicks: 0,
            conversions: 0,
            revenue: 0
        };
        
        this.initialize();
    }
    
    generatePartnerId() {
        // Check for existing partner ID or create new one
        let partnerId = localStorage.getItem('botinc_partner_id');
        if (!partnerId) {
            partnerId = 'partner_' + Date.now() + '_' + Math.random().toString(36).substr(2, 8);
            localStorage.setItem('botinc_partner_id', partnerId);
        }
        return partnerId;
    }
    
    initialize() {
        this.setupStreamingTools();
        this.setupContentCreationTools();
        this.setupInfluencerDashboard();
        this.setupCustomBranding();
        this.detectStreamingEnvironment();
    }
    
    detectStreamingEnvironment() {
        // Detect if running in OBS browser source or similar
        const isOBS = window.location.search.includes('obs=1') || 
                     window.obsstudio !== undefined ||
                     navigator.userAgent.includes('OBS');
                     
        const isStreamLabs = window.location.search.includes('streamlabs=1');
        
        if (isOBS || isStreamLabs) {
            this.enableStreamingMode();
        }
        
        // Check for stream-specific URL parameters
        this.parseStreamingParams();
    }
    
    parseStreamingParams() {
        const urlParams = new URLSearchParams(window.location.search);
        
        if (urlParams.get('partner')) {
            this.partnerId = urlParams.get('partner');
        }
        
        if (urlParams.get('overlay')) {
            this.enableOverlay(urlParams.get('overlay'));
        }
        
        if (urlParams.get('brand')) {
            this.applyCustomBranding(urlParams.get('brand'));
        }
        
        if (urlParams.get('tournament')) {
            this.enableTournamentMode(urlParams.get('tournament'));
        }
    }
    
    setupStreamingTools() {
        // Create streaming control panel
        if (this.shouldShowStreamingControls()) {
            this.createStreamingControlPanel();
        }
        
        // Setup stream-friendly UI modifications
        this.setupStreamOptimizations();
        
        // Create custom overlays for different streaming scenarios
        this.setupStreamOverlays();
    }
    
    shouldShowStreamingControls() {
        return window.location.search.includes('streaming=1') || 
               this.streamingMode ||
               localStorage.getItem('botinc_streaming_mode') === 'true';
    }
    
    createStreamingControlPanel() {
        const panel = document.createElement('div');
        panel.id = 'streaming-control-panel';
        panel.className = 'streaming-panel';
        panel.innerHTML = `
            <div class="panel-header">
                <h3>🎥 Streaming Tools</h3>
                <button class="panel-toggle" onclick="this.parentNode.parentNode.classList.toggle('minimized')">−</button>
            </div>
            <div class="panel-content">
                <div class="control-group">
                    <label>Overlay Mode:</label>
                    <select id="overlayMode" onchange="window.influencerTools.changeOverlay(this.value)">
                        <option value="none">None</option>
                        <option value="minimal">Minimal</option>
                        <option value="full">Full Branding</option>
                        <option value="tournament">Tournament</option>
                        <option value="custom">Custom</option>
                    </select>
                </div>
                
                <div class="control-group">
                    <label>Stream Info:</label>
                    <input type="text" id="streamerName" placeholder="Your channel name" 
                           value="${localStorage.getItem('streamer_name') || ''}"
                           onchange="window.influencerTools.updateStreamerInfo(this.value)">
                </div>
                
                <div class="control-group">
                    <button onclick="window.influencerTools.generateTournament()" class="control-btn">
                        🏆 Create Tournament
                    </button>
                    <button onclick="window.influencerTools.enableCompetitiveMode()" class="control-btn">
                        ⚔️ Competitive Mode
                    </button>
                </div>
                
                <div class="control-group">
                    <label>Viewer Interaction:</label>
                    <button onclick="window.influencerTools.enableViewerChallenges()" class="control-btn">
                        🎯 Viewer Challenges
                    </button>
                    <button onclick="window.influencerTools.showChat Integration()" class="control-btn">
                        💬 Chat Commands
                    </button>
                </div>
                
                <div class="analytics-mini">
                    <h4>Stream Analytics</h4>
                    <div class="mini-stats">
                        <div class="mini-stat">
                            <span class="stat-value" id="streamImpressions">${this.analyticsData.impressions}</span>
                            <span class="stat-label">Views</span>
                        </div>
                        <div class="mini-stat">
                            <span class="stat-value" id="streamClicks">${this.analyticsData.clicks}</span>
                            <span class="stat-label">Clicks</span>
                        </div>
                        <div class="mini-stat">
                            <span class="stat-value" id="streamConversions">${this.analyticsData.conversions}</span>
                            <span class="stat-label">Players</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        // Add streaming panel styles
        const styles = document.createElement('style');
        styles.id = 'streaming-tools-styles';
        styles.textContent = `
            .streaming-panel {
                position: fixed;
                top: 10px;
                right: 10px;
                width: 280px;
                background: rgba(0, 0, 0, 0.9);
                border: 2px solid #00ffff;
                border-radius: 8px;
                font-family: 'Courier New', monospace;
                color: white;
                z-index: 10000;
                backdrop-filter: blur(10px);
                transition: all 0.3s ease;
            }
            
            .streaming-panel.minimized .panel-content {
                display: none;
            }
            
            .panel-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 10px 15px;
                background: rgba(0, 255, 255, 0.1);
                border-bottom: 1px solid rgba(0, 255, 255, 0.3);
            }
            
            .panel-header h3 {
                margin: 0;
                font-size: 14px;
                color: #00ffff;
            }
            
            .panel-toggle {
                background: none;
                border: none;
                color: #00ffff;
                cursor: pointer;
                font-size: 16px;
                width: 24px;
                height: 24px;
            }
            
            .panel-content {
                padding: 15px;
            }
            
            .control-group {
                margin-bottom: 15px;
            }
            
            .control-group label {
                display: block;
                font-size: 12px;
                color: #cccccc;
                margin-bottom: 5px;
            }
            
            .control-group select,
            .control-group input {
                width: 100%;
                padding: 8px;
                background: rgba(255, 255, 255, 0.1);
                border: 1px solid rgba(0, 255, 255, 0.3);
                border-radius: 4px;
                color: white;
                font-size: 12px;
            }
            
            .control-btn {
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                border: none;
                color: white;
                padding: 8px 12px;
                border-radius: 4px;
                cursor: pointer;
                font-size: 11px;
                margin: 2px;
                transition: all 0.3s ease;
            }
            
            .control-btn:hover {
                transform: translateY(-1px);
                box-shadow: 0 3px 10px rgba(0, 0, 0, 0.3);
            }
            
            .analytics-mini {
                background: rgba(0, 255, 255, 0.05);
                border: 1px solid rgba(0, 255, 255, 0.2);
                border-radius: 4px;
                padding: 10px;
                margin-top: 15px;
            }
            
            .analytics-mini h4 {
                margin: 0 0 10px 0;
                font-size: 12px;
                color: #00ffff;
            }
            
            .mini-stats {
                display: flex;
                justify-content: space-between;
            }
            
            .mini-stat {
                text-align: center;
            }
            
            .stat-value {
                display: block;
                font-size: 16px;
                font-weight: bold;
                color: #00ffff;
            }
            
            .stat-label {
                font-size: 10px;
                color: #999;
            }
            
            @media (max-width: 768px) {
                .streaming-panel {
                    width: 240px;
                    right: 5px;
                    top: 5px;
                }
            }
        `;
        document.head.appendChild(styles);
        
        document.body.appendChild(panel);
    }
    
    setupStreamOptimizations() {
        if (this.streamingMode) {
            // Add stream-friendly CSS class
            document.body.classList.add('streaming-mode');
            
            // Optimize for streaming
            const streamStyles = document.createElement('style');
            streamStyles.textContent = `
                .streaming-mode {
                    --font-scale: 1.2;
                }
                
                .streaming-mode .game-score,
                .streaming-mode .game-stats,
                .streaming-mode .leaderboard-entry {
                    font-size: calc(1em * var(--font-scale));
                    font-weight: bold;
                }
                
                .streaming-mode .achievement-notification {
                    transform: scale(1.3);
                    animation-duration: 3s;
                }
                
                .streaming-mode .social-share-container {
                    display: none !important;
                }
            `;
            document.head.appendChild(streamStyles);
        }
    }
    
    setupStreamOverlays() {
        // Create different overlay templates
        this.createOverlayTemplate('minimal', {
            logo: true,
            score: true,
            achievements: false,
            social: false,
            transparency: 0.8
        });
        
        this.createOverlayTemplate('full', {
            logo: true,
            score: true,
            achievements: true,
            social: true,
            partnerInfo: true,
            transparency: 0.9
        });
        
        this.createOverlayTemplate('tournament', {
            logo: true,
            score: true,
            leaderboard: true,
            timer: true,
            participants: true,
            transparency: 0.95
        });
    }
    
    createOverlayTemplate(name, config) {
        this.customOverlays.set(name, config);
    }
    
    changeOverlay(overlayName) {
        // Remove existing overlay
        const existingOverlay = document.getElementById('stream-overlay');
        if (existingOverlay) {
            existingOverlay.remove();
        }
        
        if (overlayName === 'none') return;
        
        const config = this.customOverlays.get(overlayName);
        if (!config) return;
        
        // Create new overlay
        const overlay = document.createElement('div');
        overlay.id = 'stream-overlay';
        overlay.className = `stream-overlay overlay-${overlayName}`;
        
        let overlayHTML = '';
        
        if (config.logo) {
            overlayHTML += `
                <div class="overlay-logo">
                    <img src="/assets/images/botinc-logo.png" alt="BotInc Games">
                    <span>BotInc Games</span>
                </div>
            `;
        }
        
        if (config.score) {
            overlayHTML += `
                <div class="overlay-score" id="overlayScore">
                    <div class="score-label">SCORE</div>
                    <div class="score-value">0</div>
                </div>
            `;
        }
        
        if (config.partnerInfo) {
            overlayHTML += `
                <div class="overlay-partner">
                    <div class="partner-label">STREAMED BY</div>
                    <div class="partner-name">${localStorage.getItem('streamer_name') || 'Streamer'}</div>
                </div>
            `;
        }
        
        if (config.leaderboard) {
            overlayHTML += `
                <div class="overlay-leaderboard" id="overlayLeaderboard">
                    <h4>🏆 TOP PLAYERS</h4>
                    <div class="overlay-leaderboard-list"></div>
                </div>
            `;
        }
        
        overlay.innerHTML = overlayHTML;
        
        // Add overlay styles
        const overlayStyles = document.createElement('style');
        overlayStyles.textContent = `
            .stream-overlay {
                position: fixed;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                pointer-events: none;
                z-index: 9999;
                opacity: ${config.transparency};
            }
            
            .overlay-logo {
                position: absolute;
                top: 20px;
                left: 20px;
                display: flex;
                align-items: center;
                gap: 10px;
                background: rgba(0, 0, 0, 0.8);
                padding: 10px 15px;
                border-radius: 25px;
                border: 2px solid #00ffff;
            }
            
            .overlay-logo img {
                width: 24px;
                height: 24px;
            }
            
            .overlay-logo span {
                color: #00ffff;
                font-weight: bold;
                font-size: 14px;
            }
            
            .overlay-score {
                position: absolute;
                top: 20px;
                right: 20px;
                background: rgba(0, 0, 0, 0.9);
                padding: 15px 20px;
                border-radius: 10px;
                border: 2px solid #ffff00;
                text-align: center;
                min-width: 120px;
            }
            
            .score-label {
                font-size: 12px;
                color: #ffff00;
                margin-bottom: 5px;
            }
            
            .score-value {
                font-size: 24px;
                font-weight: bold;
                color: white;
            }
            
            .overlay-partner {
                position: absolute;
                bottom: 20px;
                left: 20px;
                background: rgba(0, 0, 0, 0.8);
                padding: 10px 15px;
                border-radius: 8px;
                border: 2px solid #ff6b6b;
            }
            
            .partner-label {
                font-size: 10px;
                color: #ff6b6b;
                margin-bottom: 3px;
            }
            
            .partner-name {
                font-size: 14px;
                font-weight: bold;
                color: white;
            }
            
            .overlay-leaderboard {
                position: absolute;
                top: 100px;
                right: 20px;
                background: rgba(0, 0, 0, 0.9);
                padding: 15px;
                border-radius: 8px;
                border: 2px solid #00ffff;
                min-width: 200px;
            }
            
            .overlay-leaderboard h4 {
                margin: 0 0 10px 0;
                color: #00ffff;
                font-size: 12px;
                text-align: center;
            }
        `;
        document.head.appendChild(overlayStyles);
        
        document.body.appendChild(overlay);
        
        // Setup overlay data binding
        this.bindOverlayData();
    }
    
    bindOverlayData() {
        // Update overlay with real-time game data
        const updateOverlay = () => {
            const scoreElement = document.getElementById('overlayScore');
            if (scoreElement) {
                // Get current game score
                let currentScore = 0;
                if (window.game && window.game.gameState && window.game.gameState.score) {
                    currentScore = window.game.gameState.score;
                } else if (window.gameState && window.gameState.score) {
                    currentScore = window.gameState.score;
                }
                
                const scoreValue = scoreElement.querySelector('.score-value');
                if (scoreValue) {
                    scoreValue.textContent = currentScore.toLocaleString();
                }
            }
            
            // Update leaderboard
            const leaderboardElement = document.getElementById('overlayLeaderboard');
            if (leaderboardElement && window.leaderboardSystem) {
                this.updateOverlayLeaderboard();
            }
        };
        
        setInterval(updateOverlay, 1000);
    }
    
    updateOverlayLeaderboard() {
        const leaderboardList = document.querySelector('.overlay-leaderboard-list');
        if (!leaderboardList) return;
        
        // Get current game leaderboard
        const currentGame = this.getCurrentGame();
        const leaderboard = window.leaderboardSystem.leaderboards[currentGame] || [];
        
        const topPlayers = leaderboard.slice(0, 5);
        leaderboardList.innerHTML = topPlayers.map((entry, index) => `
            <div class="overlay-leaderboard-entry">
                <span class="entry-rank">#${index + 1}</span>
                <span class="entry-name">${entry.playerName}</span>
                <span class="entry-score">${entry.score.toLocaleString()}</span>
            </div>
        `).join('');
    }
    
    setupContentCreationTools() {
        // Tools for content creators to generate engaging content
        
        // Screenshot and video capture enhancements
        this.setupCaptureTools();
        
        // Auto-generated content hooks
        this.setupContentHooks();
        
        // Highlight moments detection
        this.setupHighlightDetection();
    }
    
    setupCaptureTools() {
        // Enhanced screenshot function for content creation
        window.captureGameplayScreenshot = (includeBranding = true) => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            
            // Capture current viewport
            html2canvas(document.body, {
                canvas: canvas,
                backgroundColor: null,
                scale: 2 // High quality
            }).then(capturedCanvas => {
                if (includeBranding) {
                    this.addBrandingToCapture(ctx, canvas);
                }
                
                // Download the image
                const link = document.createElement('a');
                link.download = `botinc-gameplay-${Date.now()}.png`;
                link.href = canvas.toDataURL();
                link.click();
                
                // Track capture event
                this.trackInfluencerEvent('screenshot_captured', {
                    includeBranding: includeBranding,
                    timestamp: Date.now()
                });
            });
        };
    }
    
    addBrandingToCapture(ctx, canvas) {
        // Add BotInc branding to captured images
        ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
        ctx.fillRect(0, canvas.height - 60, canvas.width, 60);
        
        ctx.fillStyle = '#00ffff';
        ctx.font = 'bold 24px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('BotInc Games - jeffamerican.github.io/unit4productions.github.io', 
                     canvas.width / 2, canvas.height - 20);
    }
    
    setupContentHooks() {
        // Auto-generate content ideas based on gameplay
        this.contentSuggestions = [
            { trigger: 'high_score', content: 'New personal best! Check out this epic score run!' },
            { trigger: 'achievement_unlock', content: 'Achievement unlocked! This was harder than it looks...' },
            { trigger: 'close_call', content: 'That was WAY too close! Heart attack moment 😅' },
            { trigger: 'perfect_run', content: 'Flawless victory! Sometimes skill meets luck perfectly!' },
            { trigger: 'funny_fail', content: 'Epic fail compilation material right here! 😂' }
        ];
        
        // Listen for events that could generate content
        document.addEventListener('gameEvent', (event) => {
            this.suggestContent(event.detail);
        });
    }
    
    suggestContent(eventData) {
        const suggestion = this.contentSuggestions.find(s => s.trigger === eventData.type);
        if (suggestion) {
            this.showContentSuggestion(suggestion.content, eventData);
        }
    }
    
    showContentSuggestion(content, eventData) {
        // Show content suggestion notification
        const notification = document.createElement('div');
        notification.className = 'content-suggestion';
        notification.innerHTML = `
            <div class="suggestion-header">💡 Content Idea</div>
            <div class="suggestion-text">${content}</div>
            <div class="suggestion-actions">
                <button onclick="window.influencerTools.saveContentIdea('${content}', ${JSON.stringify(eventData)})">
                    Save Idea
                </button>
                <button onclick="this.parentNode.parentNode.remove()">
                    Dismiss
                </button>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            bottom: 80px;
            right: 20px;
            width: 300px;
            background: rgba(255, 107, 107, 0.95);
            color: white;
            padding: 15px;
            border-radius: 8px;
            border: 2px solid #ff6b6b;
            z-index: 10001;
            animation: slideIn 0.5s ease-out;
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove after 10 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 10000);
    }
    
    saveContentIdea(content, eventData) {
        const ideas = JSON.parse(localStorage.getItem('botinc_content_ideas') || '[]');
        ideas.push({
            content: content,
            eventData: eventData,
            timestamp: Date.now(),
            used: false
        });
        localStorage.setItem('botinc_content_ideas', JSON.stringify(ideas));
        
        this.showNotification('Content idea saved!', 'success');
    }
    
    setupInfluencerDashboard() {
        // Create dashboard for influencer metrics and tools
        if (this.shouldShowInfluencerDashboard()) {
            this.createInfluencerDashboard();
        }
    }
    
    shouldShowInfluencerDashboard() {
        return localStorage.getItem('botinc_influencer_mode') === 'true' ||
               window.location.search.includes('influencer=1');
    }
    
    createInfluencerDashboard() {
        // Dashboard will be created as a separate page/modal
        // For now, add quick access tools
        const quickTools = document.createElement('div');
        quickTools.id = 'influencer-quick-tools';
        quickTools.innerHTML = `
            <div class="quick-tools">
                <button onclick="window.influencerTools.generateReferralLink()" title="Generate Referral Link">
                    🔗
                </button>
                <button onclick="window.influencerTools.openAnalytics()" title="View Analytics">
                    📊
                </button>
                <button onclick="window.influencerTools.captureHighlight()" title="Capture Highlight">
                    🎬
                </button>
                <button onclick="window.influencerTools.startCompetition()" title="Start Competition">
                    🏁
                </button>
            </div>
        `;
        
        quickTools.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 20px;
            z-index: 9999;
        `;
        
        const toolStyles = document.createElement('style');
        toolStyles.textContent = `
            .quick-tools {
                display: flex;
                gap: 10px;
            }
            
            .quick-tools button {
                width: 50px;
                height: 50px;
                border-radius: 50%;
                background: rgba(0, 255, 255, 0.9);
                border: none;
                font-size: 20px;
                cursor: pointer;
                transition: all 0.3s ease;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            }
            
            .quick-tools button:hover {
                transform: translateY(-2px);
                box-shadow: 0 6px 16px rgba(0, 0, 0, 0.4);
            }
        `;
        document.head.appendChild(toolStyles);
        
        document.body.appendChild(quickTools);
    }
    
    setupCustomBranding() {
        // Allow influencers to customize branding elements
        this.brandingConfig = {
            colors: {
                primary: '#00ffff',
                secondary: '#ff6b6b',
                accent: '#ffff00'
            },
            logo: null,
            watermark: null,
            customCSS: null
        };
        
        this.loadCustomBranding();
    }
    
    loadCustomBranding() {
        const stored = localStorage.getItem('botinc_custom_branding');
        if (stored) {
            try {
                this.brandingConfig = { ...this.brandingConfig, ...JSON.parse(stored) };
                this.applyCustomBranding();
            } catch (e) {
                console.warn('Failed to load custom branding');
            }
        }
    }
    
    applyCustomBranding(brandId = null) {
        if (brandId) {
            // Apply specific brand preset
            const brandPresets = {
                'cyberpunk': {
                    colors: { primary: '#ff0080', secondary: '#00ff80', accent: '#8000ff' }
                },
                'neon': {
                    colors: { primary: '#ff6b6b', secondary: '#4ecdc4', accent: '#45b7d1' }
                },
                'matrix': {
                    colors: { primary: '#00ff00', secondary: '#003300', accent: '#00aa00' }
                }
            };
            
            if (brandPresets[brandId]) {
                this.brandingConfig = { ...this.brandingConfig, ...brandPresets[brandId] };
            }
        }
        
        // Apply custom CSS variables
        document.documentElement.style.setProperty('--influencer-primary', this.brandingConfig.colors.primary);
        document.documentElement.style.setProperty('--influencer-secondary', this.brandingConfig.colors.secondary);
        document.documentElement.style.setProperty('--influencer-accent', this.brandingConfig.colors.accent);
        
        // Apply custom CSS if provided
        if (this.brandingConfig.customCSS) {
            const customStyles = document.createElement('style');
            customStyles.id = 'influencer-custom-styles';
            customStyles.textContent = this.brandingConfig.customCSS;
            document.head.appendChild(customStyles);
        }
    }
    
    // Public methods for influencer tools
    generateReferralLink() {
        const baseUrl = window.location.origin + window.location.pathname;
        const referralLink = `${baseUrl}?ref=${this.partnerId}&utm_source=influencer&utm_medium=social&utm_campaign=${Date.now()}`;
        
        this.copyToClipboard(referralLink);
        this.showNotification('Referral link copied!', 'success');
        
        this.trackInfluencerEvent('referral_link_generated', {
            partnerId: this.partnerId
        });
    }
    
    openAnalytics() {
        // Open analytics in new window/modal
        window.open('/dashboard/', '_blank');
    }
    
    captureHighlight() {
        window.captureGameplayScreenshot(true);
    }
    
    startCompetition() {
        // Generate competition parameters
        const competition = {
            id: 'comp_' + Date.now(),
            creator: this.partnerId,
            game: this.getCurrentGame(),
            duration: 300, // 5 minutes
            goal: 'high_score',
            prize: 'Shoutout + Custom Achievement'
        };
        
        this.showNotification('Competition started! Share the link with your audience.', 'success');
        
        // Generate competition link
        const competitionLink = `${window.location.href}?competition=${btoa(JSON.stringify(competition))}`;
        this.copyToClipboard(competitionLink);
    }
    
    enableStreamingMode() {
        this.streamingMode = true;
        document.body.classList.add('streaming-mode');
        localStorage.setItem('botinc_streaming_mode', 'true');
        
        this.createStreamingControlPanel();
    }
    
    enableViewerChallenges() {
        // Enable viewer interaction features
        this.showNotification('Viewer challenges enabled! Use chat commands: !challenge, !score, !compete', 'info');
    }
    
    generateTournament() {
        const tournament = {
            id: 'tournament_' + Date.now(),
            creator: this.partnerId,
            name: `${localStorage.getItem('streamer_name') || 'Streamer'}'s Tournament`,
            game: this.getCurrentGame(),
            format: 'bracket',
            maxParticipants: 32,
            entryCode: this.generateEntryCode()
        };
        
        this.showNotification(`Tournament created! Entry code: ${tournament.entryCode}`, 'success');
    }
    
    generateEntryCode() {
        return Math.random().toString(36).substr(2, 6).toUpperCase();
    }
    
    updateStreamerInfo(name) {
        localStorage.setItem('streamer_name', name);
        
        // Update any displayed streamer info
        document.querySelectorAll('.partner-name').forEach(element => {
            element.textContent = name;
        });
    }
    
    // Utility methods
    getCurrentGame() {
        const path = window.location.pathname;
        const match = path.match(/\/games\/([^\/]+)\//);
        return match ? match[1] : 'homepage';
    }
    
    trackInfluencerEvent(eventName, data) {
        this.analyticsData.impressions++;
        
        if (eventName.includes('click')) {
            this.analyticsData.clicks++;
        }
        
        if (eventName.includes('conversion') || eventName.includes('signup')) {
            this.analyticsData.conversions++;
        }
        
        // Update UI if visible
        const impressionsEl = document.getElementById('streamImpressions');
        const clicksEl = document.getElementById('streamClicks');
        const conversionsEl = document.getElementById('streamConversions');
        
        if (impressionsEl) impressionsEl.textContent = this.analyticsData.impressions;
        if (clicksEl) clicksEl.textContent = this.analyticsData.clicks;
        if (conversionsEl) conversionsEl.textContent = this.analyticsData.conversions;
        
        // Track in main analytics
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('influencer_' + eventName, {
                partnerId: this.partnerId,
                ...data
            });
        }
    }
    
    copyToClipboard(text) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text);
        } else {
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
        }
    }
    
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `influencer-notification ${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: ${type === 'success' ? '#4CAF50' : type === 'error' ? '#f44336' : '#2196F3'};
            color: white;
            padding: 15px 25px;
            border-radius: 8px;
            z-index: 10002;
            font-weight: bold;
            animation: fadeInOut 3s ease-in-out forwards;
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }
}

// Initialize influencer tools
document.addEventListener('DOMContentLoaded', () => {
    window.influencerTools = new InfluencerTools();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = InfluencerTools;
}