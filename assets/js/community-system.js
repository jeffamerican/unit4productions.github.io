/**
 * BotInc Community System
 * Email capture, user accounts, and community features
 */

class CommunitySystem {
    constructor() {
        this.subscribers = this.loadSubscribers();
        this.userAccount = this.loadUserAccount();
        this.emailCaptureActive = false;
        
        this.initializeEmailCapture();
        this.initializeUserAccounts();
        this.setupCommunityFeatures();
    }
    
    initializeEmailCapture() {
        // Enhanced newsletter form handling
        this.setupNewsletterForm();
        this.createExitIntentCapture();
        this.createScrollTriggeredCapture();
        this.createGameCompletionCapture();
    }
    
    setupNewsletterForm() {
        const newsletterForm = document.getElementById('newsletterForm');
        if (!newsletterForm) return;
        
        newsletterForm.addEventListener('submit', (event) => {
            event.preventDefault();
            this.handleNewsletterSubmit(event.target);
        });
        
        // Add incentives to the existing form
        this.enhanceNewsletterForm();
    }
    
    enhanceNewsletterForm() {
        const newsletterSection = document.getElementById('newsletter');
        if (!newsletterSection) return;
        
        // Add incentives and social proof
        const incentivesHTML = `
            <div class="newsletter-incentives" style="margin-top: 20px;">
                <div class="incentive-list">
                    <div class="incentive-item">
                        <span class="incentive-icon">🎮</span>
                        <span>Exclusive beta access to new games</span>
                    </div>
                    <div class="incentive-item">
                        <span class="incentive-icon">🏆</span>
                        <span>Weekly high score competitions</span>
                    </div>
                    <div class="incentive-item">
                        <span class="incentive-icon">💎</span>
                        <span>Special achievements and rewards</span>
                    </div>
                    <div class="incentive-item">
                        <span class="incentive-icon">📱</span>
                        <span>Mobile game updates first</span>
                    </div>
                </div>
                <div class="social-proof">
                    <p><strong>${this.getSubscriberCount()}</strong> gamers already joined the community!</p>
                    <div class="recent-signups" id="recentSignups"></div>
                </div>
            </div>
        `;
        
        const formContainer = newsletterSection.querySelector('.newsletter-form');
        if (formContainer) {
            formContainer.insertAdjacentHTML('afterend', incentivesHTML);
            this.showRecentSignups();
        }
    }
    
    createExitIntentCapture() {
        let exitIntentShown = false;
        
        document.addEventListener('mouseleave', (event) => {
            if (event.clientY <= 0 && !exitIntentShown && !this.emailCaptureActive) {
                this.showExitIntentPopup();
                exitIntentShown = true;
            }
        });
    }
    
    createScrollTriggeredCapture() {
        let scrollCaptureShown = false;
        
        window.addEventListener('scroll', () => {
            const scrollPercent = (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100;
            
            if (scrollPercent > 70 && !scrollCaptureShown && !this.emailCaptureActive) {
                setTimeout(() => {
                    this.showScrollTriggeredCapture();
                    scrollCaptureShown = true;
                }, 2000);
            }
        });
    }
    
    createGameCompletionCapture() {
        // Listen for game completion events
        document.addEventListener('gameCompleted', (event) => {
            const { score, game } = event.detail;
            
            if (score > 500 && !this.emailCaptureActive) {
                setTimeout(() => {
                    this.showGameCompletionCapture(game, score);
                }, 1500);
            }
        });
    }
    
    showExitIntentPopup() {
        this.emailCaptureActive = true;
        
        const popup = this.createEmailCaptureModal({
            title: "Wait! Don't Miss Out! 🚀",
            subtitle: "Join 1000+ gamers who get exclusive access to new games",
            incentive: "Get immediate access to our upcoming Cyber Tournament!",
            buttonText: "Get Free Access",
            type: "exit-intent"
        });
        
        document.body.appendChild(popup);
    }
    
    showScrollTriggeredCapture() {
        this.emailCaptureActive = true;
        
        const banner = document.createElement('div');
        banner.className = 'scroll-capture-banner';
        banner.innerHTML = `
            <div class="capture-banner-content">
                <div class="banner-text">
                    <strong>🎯 You're clearly interested in gaming!</strong>
                    Join our community for exclusive tournaments and beta games.
                </div>
                <form class="banner-form" onsubmit="return window.communitySystem.handleInlineCapture(event)">
                    <input type="email" placeholder="Enter your email" required>
                    <button type="submit">Join Free</button>
                </form>
                <button class="banner-close" onclick="this.parentNode.parentNode.remove(); window.communitySystem.emailCaptureActive = false;">×</button>
            </div>
        `;
        
        banner.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px;
            z-index: 10000;
            animation: slideDown 0.5s ease-out;
            box-shadow: 0 2px 10px rgba(0,0,0,0.3);
        `;
        
        document.body.appendChild(banner);
        
        // Auto-remove after 30 seconds
        setTimeout(() => {
            if (banner.parentNode) {
                banner.parentNode.removeChild(banner);
                this.emailCaptureActive = false;
            }
        }, 30000);
    }
    
    showGameCompletionCapture(game, score) {
        this.emailCaptureActive = true;
        
        const popup = this.createEmailCaptureModal({
            title: `Awesome Score in ${this.getGameDisplayName(game)}! 🏆`,
            subtitle: `You scored ${score.toLocaleString()} points!`,
            incentive: "Want to compete in weekly tournaments with prizes?",
            buttonText: "Join Tournaments",
            type: "game-completion",
            gameData: { game, score }
        });
        
        document.body.appendChild(popup);
    }
    
    createEmailCaptureModal({ title, subtitle, incentive, buttonText, type, gameData = null }) {
        const modal = document.createElement('div');
        modal.className = 'email-capture-modal';
        modal.innerHTML = `
            <div class="modal-overlay" onclick="this.parentNode.remove(); window.communitySystem.emailCaptureActive = false;"></div>
            <div class="modal-content">
                <button class="modal-close" onclick="this.parentNode.parentNode.remove(); window.communitySystem.emailCaptureActive = false;">×</button>
                <div class="modal-header">
                    <h2>${title}</h2>
                    <p>${subtitle}</p>
                </div>
                <div class="modal-body">
                    <div class="incentive-highlight">
                        <p>${incentive}</p>
                    </div>
                    <form class="capture-form" onsubmit="return window.communitySystem.handleModalCapture(event, '${type}', ${JSON.stringify(gameData)})">
                        <div class="form-group">
                            <input type="email" placeholder="Enter your email address" required class="email-input">
                            <input type="text" placeholder="Your name (optional)" class="name-input">
                        </div>
                        <button type="submit" class="capture-submit">${buttonText}</button>
                    </form>
                    <div class="privacy-note">
                        <small>We respect your privacy. Unsubscribe anytime. No spam, just great gaming content.</small>
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="social-proof-mini">
                        ${this.generateMiniSocialProof()}
                    </div>
                </div>
            </div>
        `;
        
        // Add modal styles
        if (!document.getElementById('modal-styles')) {
            const styles = document.createElement('style');
            styles.id = 'modal-styles';
            styles.textContent = `
                .email-capture-modal {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    z-index: 10001;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    animation: modalFadeIn 0.3s ease-out;
                }
                
                .modal-overlay {
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0,0,0,0.8);
                    backdrop-filter: blur(5px);
                }
                
                .modal-content {
                    background: white;
                    border-radius: 15px;
                    padding: 30px;
                    max-width: 500px;
                    width: 90%;
                    position: relative;
                    animation: modalSlideUp 0.3s ease-out;
                    box-shadow: 0 20px 40px rgba(0,0,0,0.3);
                }
                
                .modal-close {
                    position: absolute;
                    top: 15px;
                    right: 15px;
                    background: none;
                    border: none;
                    font-size: 24px;
                    cursor: pointer;
                    color: #999;
                }
                
                .modal-header h2 {
                    margin: 0 0 10px 0;
                    color: #333;
                    font-size: 24px;
                }
                
                .modal-header p {
                    margin: 0 0 20px 0;
                    color: #666;
                    font-size: 16px;
                }
                
                .incentive-highlight {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 20px;
                    border-radius: 10px;
                    margin-bottom: 25px;
                    text-align: center;
                }
                
                .incentive-highlight p {
                    margin: 0;
                    font-weight: bold;
                    font-size: 18px;
                }
                
                .capture-form .form-group {
                    margin-bottom: 20px;
                }
                
                .email-input, .name-input {
                    width: 100%;
                    padding: 15px;
                    border: 2px solid #ddd;
                    border-radius: 8px;
                    font-size: 16px;
                    margin-bottom: 10px;
                    transition: border-color 0.3s;
                }
                
                .email-input:focus, .name-input:focus {
                    outline: none;
                    border-color: #667eea;
                }
                
                .capture-submit {
                    width: 100%;
                    padding: 15px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    border: none;
                    border-radius: 8px;
                    font-size: 18px;
                    font-weight: bold;
                    cursor: pointer;
                    transition: transform 0.3s;
                }
                
                .capture-submit:hover {
                    transform: translateY(-2px);
                }
                
                .privacy-note {
                    text-align: center;
                    margin-top: 15px;
                    color: #888;
                }
                
                .social-proof-mini {
                    text-align: center;
                    color: #666;
                    font-size: 14px;
                    margin-top: 15px;
                }
                
                @keyframes modalFadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                
                @keyframes modalSlideUp {
                    from { transform: translateY(50px); opacity: 0; }
                    to { transform: translateY(0); opacity: 1; }
                }
                
                @keyframes slideDown {
                    from { transform: translateY(-100%); }
                    to { transform: translateY(0); }
                }
            `;
            document.head.appendChild(styles);
        }
        
        return modal;
    }
    
    handleNewsletterSubmit(form) {
        const email = form.querySelector('input[type="email"]').value;
        const name = form.querySelector('input[name="name"]')?.value || '';
        
        this.addSubscriber(email, name, 'newsletter-form');
        this.showSuccessMessage('Thanks for joining! Check your email for exclusive beta access.');
        form.reset();
    }
    
    handleInlineCapture(event) {
        event.preventDefault();
        const email = event.target.querySelector('input[type="email"]').value;
        
        this.addSubscriber(email, '', 'scroll-trigger');
        this.showSuccessMessage('Welcome to the community!');
        event.target.closest('.scroll-capture-banner').remove();
        this.emailCaptureActive = false;
        
        return false;
    }
    
    handleModalCapture(event, type, gameData) {
        event.preventDefault();
        const email = event.target.querySelector('.email-input').value;
        const name = event.target.querySelector('.name-input').value;
        
        this.addSubscriber(email, name, type, gameData);
        
        // Show success and close modal
        event.target.closest('.email-capture-modal').remove();
        this.emailCaptureActive = false;
        
        this.showSuccessMessage('Welcome to the community! Check your email for your welcome package.');
        
        // Track conversion
        if (typeof trackEngagement !== 'undefined') {
            trackEngagement('email_capture', { source: type, email: email });
        }
        
        return false;
    }
    
    addSubscriber(email, name = '', source = 'unknown', gameData = null) {
        const subscriber = {
            email: email,
            name: name,
            source: source,
            timestamp: Date.now(),
            gameData: gameData,
            id: this.generateSubscriberId()
        };
        
        this.subscribers.push(subscriber);
        this.saveSubscribers();
        
        // Track analytics
        if (typeof botincAnalytics !== 'undefined') {
            botincAnalytics.trackEngagementEvent('email_signup', {
                source: source,
                email: email
            });
        }
        
        // Trigger welcome sequence
        this.triggerWelcomeSequence(subscriber);
    }
    
    triggerWelcomeSequence(subscriber) {
        // Since we can't send real emails, we'll simulate with localStorage
        const welcomeData = {
            subscriber: subscriber,
            welcomeItems: [
                'Beta access to Cipher Storm',
                'Exclusive tournament invitations',
                '10% bonus points in all games',
                'VIP Discord channel access'
            ],
            deliveryDate: Date.now()
        };
        
        localStorage.setItem(`botinc_welcome_${subscriber.id}`, JSON.stringify(welcomeData));
        
        // Show immediate value
        this.showWelcomeBonus(subscriber);
    }
    
    showWelcomeBonus(subscriber) {
        // Give immediate bonus points or achievement
        if (window.viralMechanics) {
            window.viralMechanics.checkAchievement('social_butterfly');
            window.viralMechanics.playerStats.totalPoints += 50;
            window.viralMechanics.savePlayerStats();
        }
        
        const bonus = document.createElement('div');
        bonus.className = 'welcome-bonus';
        bonus.innerHTML = `
            <div class="bonus-content">
                <h3>Welcome Bonus! 🎉</h3>
                <p>+50 Bonus Points</p>
                <p>Beta Access Granted</p>
                <small>Check your email for more exclusive content</small>
            </div>
        `;
        
        bonus.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(135deg, #4CAF50, #45a049);
            color: white;
            padding: 30px;
            border-radius: 15px;
            text-align: center;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            z-index: 10002;
            animation: bonusAppear 0.5s ease-out;
        `;
        
        document.body.appendChild(bonus);
        
        setTimeout(() => {
            if (bonus.parentNode) {
                bonus.parentNode.removeChild(bonus);
            }
        }, 4000);
    }
    
    getSubscriberCount() {
        return Math.max(1247 + this.subscribers.length, 1247); // Start with fake social proof
    }
    
    showRecentSignups() {
        const recentDiv = document.getElementById('recentSignups');
        if (!recentDiv) return;
        
        const fakeRecent = [
            'Alex from New York just joined!',
            'Maria from London joined 2 minutes ago',
            'Jake from Tokyo joined 5 minutes ago'
        ];
        
        let currentIndex = 0;
        setInterval(() => {
            recentDiv.textContent = fakeRecent[currentIndex];
            currentIndex = (currentIndex + 1) % fakeRecent.length;
        }, 3000);
    }
    
    generateMiniSocialProof() {
        const countries = ['🇺🇸 USA', '🇬🇧 UK', '🇨🇦 Canada', '🇦🇺 Australia', '🇩🇪 Germany'];
        const randomCountries = countries.slice(0, 3);
        
        return `Players from ${randomCountries.join(', ')} and more have joined!`;
    }
    
    generateSubscriberId() {
        return 'sub_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
    
    getGameDisplayName(gameId) {
        const gameNames = {
            'signal-breach': 'Signal Breach',
            'neural-nexus': 'Neural Nexus',
            'dot-conquest': 'Dot Conquest',
            'chain-cascade': 'Chain Cascade',
            'reflex-runner': 'Reflex Runner'
        };
        return gameNames[gameId] || gameId;
    }
    
    showSuccessMessage(message) {
        const notification = document.createElement('div');
        notification.className = 'success-notification';
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #4CAF50;
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.3);
            z-index: 10003;
            animation: slideInRight 0.5s ease-out, fadeOut 0.5s ease-in 4s forwards;
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 4500);
    }
    
    initializeUserAccounts() {
        // Simple user account system using localStorage
        this.createUserInterface();
    }
    
    createUserInterface() {
        // Add user account UI elements
        const nav = document.querySelector('.nav-menu');
        if (nav) {
            const userElement = document.createElement('li');
            userElement.innerHTML = `
                <div class="user-account-widget">
                    <span class="user-points">⭐ ${this.getUserPoints()}</span>
                    <span class="user-level">Lv.${this.getUserLevel()}</span>
                </div>
            `;
            nav.insertBefore(userElement, nav.lastElementChild);
        }
    }
    
    getUserPoints() {
        return window.viralMechanics?.playerStats.totalPoints || 0;
    }
    
    getUserLevel() {
        const points = this.getUserPoints();
        return Math.floor(points / 100) + 1;
    }
    
    setupCommunityFeatures() {
        // Add community features like recent activity, player counters, etc.
        this.addPlayerCounter();
        this.addRecentActivity();
    }
    
    addPlayerCounter() {
        const heroStats = document.querySelector('.hero-stats');
        if (heroStats) {
            const onlineCounter = document.createElement('div');
            onlineCounter.className = 'stat-item online-counter';
            onlineCounter.innerHTML = `
                <div class="stat-number" id="onlineCount">${this.generateOnlineCount()}</div>
                <div class="stat-label">Online Now</div>
            `;
            heroStats.appendChild(onlineCounter);
            
            // Update online count periodically
            setInterval(() => {
                document.getElementById('onlineCount').textContent = this.generateOnlineCount();
            }, 30000);
        }
    }
    
    generateOnlineCount() {
        // Generate realistic fluctuating online count
        const base = 247;
        const variation = Math.floor(Math.random() * 100) - 50;
        return Math.max(base + variation, 150);
    }
    
    addRecentActivity() {
        // Add a recent activity feed (could be expanded)
        const activityFeed = document.createElement('div');
        activityFeed.className = 'community-activity';
        activityFeed.innerHTML = `
            <div class="activity-header">
                <h4>Recent Community Activity</h4>
            </div>
            <div class="activity-list" id="activityList">
                <div class="activity-item">🏆 Player achieved high score in Signal Breach</div>
                <div class="activity-item">🎮 New player joined from Canada</div>
                <div class="activity-item">⚡ Daily challenge completed by 23 players</div>
            </div>
        `;
        
        // Could be added to sidebar or footer
    }
    
    loadSubscribers() {
        const stored = localStorage.getItem('botinc_subscribers');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load subscribers');
            }
        }
        return [];
    }
    
    saveSubscribers() {
        localStorage.setItem('botinc_subscribers', JSON.stringify(this.subscribers));
    }
    
    loadUserAccount() {
        const stored = localStorage.getItem('botinc_user_account');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load user account');
            }
        }
        return {
            id: this.generateUserId(),
            joinDate: Date.now(),
            preferences: {},
            progress: {}
        };
    }
    
    generateUserId() {
        return 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
    
    exportCommunityData() {
        return {
            subscribers: this.subscribers.length,
            subscriptionSources: this.getSubscriptionSources(),
            userAccount: this.userAccount,
            timestamp: new Date().toISOString()
        };
    }
    
    getSubscriptionSources() {
        const sources = {};
        this.subscribers.forEach(sub => {
            sources[sub.source] = (sources[sub.source] || 0) + 1;
        });
        return sources;
    }
}

// Initialize community system
const communitySystem = new CommunitySystem();
window.communitySystem = communitySystem;