/**
 * SIGNAL BREACH - PREMIUM FEATURES INTEGRATION
 * Complete freemium monetization system for browser-based Signal Breach
 * Integrates with Unit4Productions analytics and payment systems
 */

class SignalBreachPremium {
    constructor() {
        this.isPremium = this.checkPremiumStatus();
        this.lives = this.isPremium ? -1 : 5; // -1 = unlimited
        this.lastLifeRegen = Date.now();
        this.themes = this.loadAvailableThemes();
        this.currentTheme = this.loadCurrentTheme();
        this.premiumLevels = [11, 12, 13, 14, 15, 16, 17, 18, 19, 20]; // Premium levels
        this.statistics = this.loadStatistics();
        
        this.init();
    }
    
    // =================== PREMIUM STATUS MANAGEMENT ===================
    
    checkPremiumStatus() {
        const premiumKey = localStorage.getItem('premium_key');
        const purchaseDate = localStorage.getItem('purchase_date');
        
        if (!premiumKey || !purchaseDate) return false;
        
        // Check if purchase is still valid (1 year)
        const daysSincePurchase = (Date.now() - new Date(purchaseDate)) / (1000 * 60 * 60 * 24);
        return daysSincePurchase < 365;
    }
    
    // =================== LIVES SYSTEM ===================
    
    useLive() {
        if (this.isPremium) {
            this.trackGameEvent('premium_unlimited_life_used');
            return true;
        }
        
        if (this.lives > 0) {
            this.lives--;
            this.saveGameState();
            this.updateUI();
            this.trackGameEvent('free_life_used', { lives_remaining: this.lives });
            return true;
        }
        
        // No lives left - show premium upgrade prompt
        this.showPremiumPrompt('lives_depleted');
        this.trackGameEvent('lives_depleted_upgrade_prompt');
        return false;
    }
    
    regenerateLives() {
        if (this.isPremium || this.lives >= 5) return;
        
        const timeSinceRegen = Date.now() - this.lastLifeRegen;
        const livesToAdd = Math.floor(timeSinceRegen / (30 * 60 * 1000)); // 30 minutes per life
        
        if (livesToAdd > 0) {
            this.lives = Math.min(5, this.lives + livesToAdd);
            this.lastLifeRegen = Date.now();
            this.saveGameState();
            this.updateUI();
            this.trackGameEvent('lives_regenerated', { lives_added: livesToAdd });
        }
    }
    
    // =================== PREMIUM THEMES SYSTEM ===================
    
    loadAvailableThemes() {
        const freeThemes = [
            {
                id: 'classic',
                name: 'Classic Cyan',
                primary: '#00ff88',
                secondary: '#008844',
                background: '#001122',
                free: true
            },
            {
                id: 'red',
                name: 'Alert Red',
                primary: '#ff4444',
                secondary: '#aa2222',
                background: '#220011',
                free: true
            },
            {
                id: 'blue',
                name: 'Ocean Blue',
                primary: '#4488ff',
                secondary: '#2244aa',
                background: '#001122',
                free: true
            }
        ];
        
        const premiumThemes = [
            {
                id: 'neon_purple',
                name: 'Neon Purple',
                primary: '#bb44ff',
                secondary: '#7722aa',
                background: '#110022',
                free: false
            },
            {
                id: 'gold_rush',
                name: 'Gold Rush',
                primary: '#ffdd44',
                secondary: '#aa8822',
                background: '#221100',
                free: false
            },
            {
                id: 'emerald',
                name: 'Emerald Matrix',
                primary: '#44ffaa',
                secondary: '#22aa66',
                background: '#002211',
                free: false
            },
            {
                id: 'sunset',
                name: 'Sunset Glow',
                primary: '#ff8844',
                secondary: '#aa4422',
                background: '#221100',
                free: false
            },
            {
                id: 'ice_blue',
                name: 'Ice Blue',
                primary: '#88ddff',
                secondary: '#4499aa',
                background: '#001122',
                free: false
            },
            {
                id: 'toxic_green',
                name: 'Toxic Green',
                primary: '#88ff44',
                secondary: '#44aa22',
                background: '#112200',
                free: false
            },
            {
                id: 'magma',
                name: 'Magma Core',
                primary: '#ff6644',
                secondary: '#aa3322',
                background: '#220000',
                free: false
            },
            {
                id: 'deep_space',
                name: 'Deep Space',
                primary: '#6644ff',
                secondary: '#3322aa',
                background: '#000022',
                free: false
            },
            {
                id: 'cyber_pink',
                name: 'Cyber Pink',
                primary: '#ff44bb',
                secondary: '#aa2266',
                background: '#220011',
                free: false
            },
            {
                id: 'void_black',
                name: 'Void Black',
                primary: '#888888',
                secondary: '#444444',
                background: '#000000',
                free: false
            }
        ];
        
        return [...freeThemes, ...premiumThemes];
    }
    
    loadCurrentTheme() {
        const savedTheme = localStorage.getItem('selected_theme') || 'classic';
        const theme = this.themes.find(t => t.id === savedTheme);
        
        // If theme is premium and user isn't premium, revert to classic
        if (theme && !theme.free && !this.isPremium) {
            return this.themes.find(t => t.id === 'classic');
        }
        
        return theme || this.themes[0];
    }
    
    changeTheme(themeId) {
        const theme = this.themes.find(t => t.id === themeId);
        
        if (!theme) return false;
        
        if (!theme.free && !this.isPremium) {
            this.showPremiumPrompt('theme_locked');
            this.trackGameEvent('premium_theme_interest', { theme: themeId });
            return false;
        }
        
        this.currentTheme = theme;
        localStorage.setItem('selected_theme', themeId);
        this.applyTheme();
        this.trackGameEvent('theme_changed', { theme: themeId });
        return true;
    }
    
    applyTheme() {
        const root = document.documentElement;
        root.style.setProperty('--primary-color', this.currentTheme.primary);
        root.style.setProperty('--secondary-color', this.currentTheme.secondary);
        root.style.setProperty('--background-color', this.currentTheme.background);
        
        // Update game UI elements
        this.updateGameColors();
    }
    
    // =================== PREMIUM LEVELS SYSTEM ===================
    
    isLevelLocked(level) {
        return this.premiumLevels.includes(level) && !this.isPremium;
    }
    
    canPlayLevel(level) {
        if (this.isLevelLocked(level)) {
            this.showPremiumPrompt('level_locked');
            this.trackGameEvent('premium_level_interest', { level: level });
            return false;
        }
        return true;
    }
    
    // =================== STATISTICS SYSTEM ===================
    
    loadStatistics() {
        const defaultStats = {
            totalGamesPlayed: 0,
            totalScore: 0,
            bestScore: 0,
            totalTimePlayedMinutes: 0,
            levelsCompleted: [],
            averageScore: 0,
            gamesPerDay: {},
            achievements: []
        };
        
        const saved = localStorage.getItem('game_statistics');
        return saved ? {...defaultStats, ...JSON.parse(saved)} : defaultStats;
    }
    
    updateStatistics(gameData) {
        this.statistics.totalGamesPlayed++;
        this.statistics.totalScore += gameData.score;
        this.statistics.bestScore = Math.max(this.statistics.bestScore, gameData.score);
        this.statistics.totalTimePlayedMinutes += Math.round(gameData.timePlayed / 60);
        this.statistics.averageScore = Math.round(this.statistics.totalScore / this.statistics.totalGamesPlayed);
        
        // Daily games tracking
        const today = new Date().toDateString();
        this.statistics.gamesPerDay[today] = (this.statistics.gamesPerDay[today] || 0) + 1;
        
        // Level completion tracking
        if (gameData.levelCompleted && !this.statistics.levelsCompleted.includes(gameData.level)) {
            this.statistics.levelsCompleted.push(gameData.level);
        }
        
        this.saveStatistics();
        
        if (this.isPremium) {
            this.updatePremiumDashboard();
        }
    }
    
    saveStatistics() {
        localStorage.setItem('game_statistics', JSON.stringify(this.statistics));
    }
    
    // =================== PREMIUM DASHBOARD ===================
    
    updatePremiumDashboard() {
        if (!this.isPremium) return;
        
        const dashboard = document.getElementById('premium-dashboard');
        if (!dashboard) return;
        
        dashboard.innerHTML = `
            <div class="dashboard-header">
                <h3>📊 Your Gaming Statistics</h3>
                <div class="premium-badge">Premium Member</div>
            </div>
            <div class="stats-grid">
                <div class="stat-card">
                    <div class="stat-number">${this.statistics.totalGamesPlayed}</div>
                    <div class="stat-label">Games Played</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${this.statistics.bestScore.toLocaleString()}</div>
                    <div class="stat-label">Best Score</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${this.statistics.averageScore.toLocaleString()}</div>
                    <div class="stat-label">Average Score</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${this.statistics.totalTimePlayedMinutes}</div>
                    <div class="stat-label">Minutes Played</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${this.statistics.levelsCompleted.length}</div>
                    <div class="stat-label">Levels Completed</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">${Object.keys(this.statistics.gamesPerDay).length}</div>
                    <div class="stat-label">Days Active</div>
                </div>
            </div>
        `;
    }
    
    // =================== PREMIUM PROMPTS ===================
    
    showPremiumPrompt(context) {
        // Remove existing prompts
        const existingPrompt = document.getElementById('premium-prompt');
        if (existingPrompt) existingPrompt.remove();
        
        const prompt = document.createElement('div');
        prompt.id = 'premium-prompt';
        prompt.className = 'premium-prompt-overlay';
        
        let title, message, benefits;
        
        switch (context) {
            case 'lives_depleted':
                title = '🔋 Out of Lives!';
                message = 'Your lives will regenerate in 30 minutes, or upgrade to Premium for unlimited lives!';
                benefits = ['Unlimited Lives', 'Never Wait Again', 'Exclusive Levels', 'Custom Themes'];
                break;
                
            case 'theme_locked':
                title = '🎨 Premium Theme';
                message = 'This beautiful theme is available with Premium membership!';
                benefits = ['15 Custom Themes', 'Unlimited Lives', 'Exclusive Levels', 'Ad-Free Experience'];
                break;
                
            case 'level_locked':
                title = '🎮 Premium Level';
                message = 'This level is part of our Premium content collection!';
                benefits = ['10 Exclusive Levels', 'Unique Challenges', 'Premium Themes', 'Statistics Dashboard'];
                break;
                
            default:
                title = '⭐ Upgrade to Premium';
                message = 'Unlock the full Signal Breach experience!';
                benefits = ['Unlimited Lives', '10 Exclusive Levels', '15 Custom Themes', 'Statistics Dashboard'];
        }
        
        prompt.innerHTML = `
            <div class="premium-prompt-content">
                <button class="close-btn" onclick="this.closest('.premium-prompt-overlay').remove()">×</button>
                <div class="premium-icon">${title.split(' ')[0]}</div>
                <h2>${title.substring(2)}</h2>
                <p class="premium-message">${message}</p>
                
                <div class="premium-benefits">
                    <h3>Premium Benefits:</h3>
                    <div class="benefits-list">
                        ${benefits.map(benefit => `<div class="benefit-item">✓ ${benefit}</div>`).join('')}
                    </div>
                </div>
                
                <div class="premium-pricing">
                    <div class="price-tag">
                        <span class="price">$4.99</span>
                        <span class="price-label">One-time purchase</span>
                    </div>
                </div>
                
                <div class="premium-actions">
                    <button class="upgrade-btn" onclick="this.upgrade()">
                        🚀 Upgrade Now
                    </button>
                    <button class="maybe-later-btn" onclick="this.closest('.premium-prompt-overlay').remove()">
                        Maybe Later
                    </button>
                </div>
                
                <div class="premium-guarantee">
                    <small>30-day money-back guarantee • Instant activation</small>
                </div>
            </div>
        `;
        
        // Add upgrade functionality
        prompt.querySelector('.upgrade-btn').onclick = () => {
            this.trackGameEvent('premium_upgrade_clicked', { context: context });
            window.open('/premium-signal-breach', '_blank');
            prompt.remove();
        };
        
        document.body.appendChild(prompt);
        
        // Track prompt display
        this.trackGameEvent('premium_prompt_shown', { context: context });
    }
    
    // =================== SOCIAL SHARING ===================
    
    shareScore(score, level) {
        const message = `Just scored ${score.toLocaleString()} on Signal Breach Level ${level}! 🎮 Can you beat it?`;
        const url = 'https://unit4productions.com/games/signal-breach';
        const hashtags = 'SignalBreach,IndieGame,PuzzleGame,Unit4Productions';
        
        if (navigator.share) {
            // Native sharing (mobile)
            navigator.share({
                title: 'Signal Breach High Score',
                text: message,
                url: url
            }).then(() => {
                this.trackGameEvent('score_shared', { platform: 'native', score: score });
                this.showShareReward();
            });
        } else {
            // Fallback social sharing
            const shareMenu = document.createElement('div');
            shareMenu.className = 'share-menu-overlay';
            shareMenu.innerHTML = `
                <div class="share-menu-content">
                    <h3>Share Your Score!</h3>
                    <div class="share-buttons">
                        <a href="https://twitter.com/intent/tweet?text=${encodeURIComponent(message)}&url=${url}&hashtags=${hashtags}" 
                           target="_blank" class="share-btn twitter-btn" onclick="this.trackShare('twitter')">
                            Share on Twitter
                        </a>
                        <a href="https://www.facebook.com/sharer/sharer.php?u=${url}&quote=${encodeURIComponent(message)}" 
                           target="_blank" class="share-btn facebook-btn" onclick="this.trackShare('facebook')">
                            Share on Facebook
                        </a>
                        <button class="share-btn copy-btn" onclick="this.copyToClipboard('${message} ${url}')">
                            Copy Link
                        </button>
                    </div>
                    <button class="close-share-btn" onclick="this.closest('.share-menu-overlay').remove()">Close</button>
                </div>
            `;
            
            document.body.appendChild(shareMenu);
        }
    }
    
    showShareReward() {
        // Give small bonus for sharing
        if (!this.isPremium && this.lives < 5) {
            this.lives++;
            this.saveGameState();
            this.updateUI();
            
            this.showNotification('Thanks for sharing! +1 Life bonus! 🎉');
        }
    }
    
    // =================== NOTIFICATIONS SYSTEM ===================
    
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `game-notification ${type}`;
        notification.textContent = message;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);
        
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
    
    // =================== GAME STATE MANAGEMENT ===================
    
    saveGameState() {
        const gameState = {
            lives: this.lives,
            lastLifeRegen: this.lastLifeRegen,
            isPremium: this.isPremium,
            currentTheme: this.currentTheme.id
        };
        localStorage.setItem('signal_breach_state', JSON.stringify(gameState));
    }
    
    loadGameState() {
        const saved = localStorage.getItem('signal_breach_state');
        if (saved) {
            const state = JSON.parse(saved);
            this.lives = state.lives || 5;
            this.lastLifeRegen = state.lastLifeRegen || Date.now();
        }
    }
    
    // =================== UI UPDATES ===================
    
    updateUI() {
        // Update lives display
        const livesDisplay = document.getElementById('lives-display');
        if (livesDisplay) {
            if (this.isPremium) {
                livesDisplay.innerHTML = '🔋 <span class="unlimited">UNLIMITED</span>';
                livesDisplay.classList.add('premium');
            } else {
                livesDisplay.innerHTML = `🔋 <span class="lives-count">${this.lives}</span>/5`;
                livesDisplay.classList.remove('premium');
            }
        }
        
        // Update premium indicator
        const premiumIndicator = document.getElementById('premium-indicator');
        if (premiumIndicator) {
            premiumIndicator.style.display = this.isPremium ? 'block' : 'none';
        }
        
        // Update theme selector
        this.updateThemeSelector();
        
        // Update level selector
        this.updateLevelSelector();
    }
    
    updateThemeSelector() {
        const themeSelector = document.getElementById('theme-selector');
        if (!themeSelector) return;
        
        themeSelector.innerHTML = this.themes.map(theme => `
            <div class="theme-option ${theme.id === this.currentTheme.id ? 'selected' : ''} ${!theme.free && !this.isPremium ? 'locked' : ''}"
                 onclick="signalBreachPremium.changeTheme('${theme.id}')"
                 data-theme="${theme.id}">
                <div class="theme-preview" style="background: ${theme.primary}; border-color: ${theme.secondary};">
                    ${!theme.free && !this.isPremium ? '🔒' : ''}
                </div>
                <div class="theme-name">${theme.name}</div>
                ${!theme.free ? '<div class="premium-tag">Premium</div>' : ''}
            </div>
        `).join('');
    }
    
    updateLevelSelector() {
        const levelSelector = document.getElementById('level-selector');
        if (!levelSelector) return;
        
        for (let i = 1; i <= 20; i++) {
            const levelBtn = levelSelector.querySelector(`[data-level="${i}"]`);
            if (levelBtn) {
                if (this.isLevelLocked(i)) {
                    levelBtn.classList.add('locked');
                    levelBtn.innerHTML = `${i} 🔒`;
                } else {
                    levelBtn.classList.remove('locked');
                    levelBtn.innerHTML = `${i}`;
                }
            }
        }
    }
    
    updateGameColors() {
        // Apply theme to game elements
        const gameElements = document.querySelectorAll('.game-element, .signal, .grid-line');
        gameElements.forEach(element => {
            element.style.color = this.currentTheme.primary;
            element.style.borderColor = this.currentTheme.secondary;
        });
    }
    
    // =================== ANALYTICS INTEGRATION ===================
    
    trackGameEvent(eventName, parameters = {}) {
        if (typeof Unit4Track !== 'undefined') {
            Unit4Track.gameStart('Signal_Breach', parameters.level || 1);
        }
        
        // Custom game analytics
        if (typeof gtag !== 'undefined') {
            gtag('event', eventName, {
                'event_category': 'Signal_Breach_Game',
                'user_type': this.isPremium ? 'Premium' : 'Free',
                'lives_remaining': this.lives,
                'current_theme': this.currentTheme.id,
                ...parameters
            });
        }
    }
    
    // =================== INITIALIZATION ===================
    
    init() {
        this.loadGameState();
        this.applyTheme();
        this.updateUI();
        
        // Set up life regeneration timer
        setInterval(() => {
            this.regenerateLives();
        }, 60000); // Check every minute
        
        // Track initialization
        this.trackGameEvent('game_initialized');
        
        console.log('Signal Breach Premium System Initialized');
        console.log('Premium Status:', this.isPremium);
        console.log('Lives:', this.lives);
        console.log('Current Theme:', this.currentTheme.name);
    }
}

// =================== PREMIUM PROMPT STYLES ===================

const premiumStyles = `
    <style>
        .premium-prompt-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.9);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 10000;
            animation: fadeIn 0.3s ease;
        }
        
        .premium-prompt-content {
            background: linear-gradient(135deg, #1a0033 0%, #000000 100%);
            border: 2px solid var(--primary-color, #00ff88);
            border-radius: 20px;
            padding: 2rem;
            max-width: 500px;
            width: 90%;
            color: var(--primary-color, #00ff88);
            text-align: center;
            position: relative;
            animation: slideUp 0.3s ease;
        }
        
        .close-btn {
            position: absolute;
            top: 10px;
            right: 15px;
            background: none;
            border: none;
            color: var(--primary-color, #00ff88);
            font-size: 1.5rem;
            cursor: pointer;
            opacity: 0.7;
        }
        
        .close-btn:hover {
            opacity: 1;
        }
        
        .premium-icon {
            font-size: 3rem;
            margin-bottom: 1rem;
        }
        
        .premium-benefits {
            margin: 1.5rem 0;
            text-align: left;
        }
        
        .benefits-list {
            display: grid;
            gap: 0.5rem;
            margin-top: 1rem;
        }
        
        .benefit-item {
            display: flex;
            align-items: center;
            color: #4ecdc4;
        }
        
        .premium-pricing {
            margin: 1.5rem 0;
        }
        
        .price-tag {
            display: inline-block;
            background: var(--primary-color, #00ff88);
            color: #000;
            padding: 1rem 2rem;
            border-radius: 15px;
            font-weight: bold;
        }
        
        .price {
            font-size: 2rem;
            display: block;
        }
        
        .price-label {
            font-size: 0.9rem;
            opacity: 0.8;
        }
        
        .premium-actions {
            display: flex;
            gap: 1rem;
            justify-content: center;
            margin: 1.5rem 0;
        }
        
        .upgrade-btn {
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
            color: white;
            border: none;
            padding: 1rem 2rem;
            border-radius: 25px;
            font-weight: bold;
            cursor: pointer;
            font-size: 1.1rem;
            transition: all 0.3s ease;
        }
        
        .upgrade-btn:hover {
            transform: scale(1.05);
            box-shadow: 0 5px 15px rgba(255, 107, 107, 0.4);
        }
        
        .maybe-later-btn {
            background: transparent;
            color: var(--primary-color, #00ff88);
            border: 1px solid var(--primary-color, #00ff88);
            padding: 1rem 2rem;
            border-radius: 25px;
            cursor: pointer;
            transition: all 0.3s ease;
        }
        
        .maybe-later-btn:hover {
            background: var(--primary-color, #00ff88);
            color: #000;
        }
        
        .premium-guarantee {
            margin-top: 1rem;
            opacity: 0.7;
        }
        
        .game-notification {
            position: fixed;
            top: 20px;
            right: 20px;
            background: var(--primary-color, #00ff88);
            color: #000;
            padding: 1rem 1.5rem;
            border-radius: 10px;
            font-weight: bold;
            z-index: 9999;
            transform: translateX(100%);
            transition: transform 0.3s ease;
        }
        
        .game-notification.show {
            transform: translateX(0);
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        @keyframes slideUp {
            from { transform: translateY(50px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
        
        .lives-display {
            font-size: 1.2rem;
            margin-bottom: 1rem;
        }
        
        .lives-display.premium .unlimited {
            color: #ffd700;
            font-weight: bold;
        }
        
        .theme-option.locked {
            opacity: 0.5;
            cursor: not-allowed;
        }
        
        .theme-option.locked:hover {
            opacity: 0.7;
        }
        
        .premium-tag {
            background: #ffd700;
            color: #000;
            padding: 2px 6px;
            border-radius: 10px;
            font-size: 0.7rem;
            font-weight: bold;
        }
    </style>
`;

// Add styles to document
document.head.insertAdjacentHTML('beforeend', premiumStyles);

// Global instance
let signalBreachPremium;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    signalBreachPremium = new SignalBreachPremium();
    
    // Make it globally accessible
    window.SignalBreachPremium = signalBreachPremium;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SignalBreachPremium;
}