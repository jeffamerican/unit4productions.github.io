/**
 * UNIT4PRODUCTIONS - SOCIAL VIRAL GROWTH SYSTEM
 * Complete social sharing, referral, and viral mechanics for Signal Breach
 * Designed to maximize organic growth and player acquisition
 */

class SocialViralGrowthSystem {
    constructor() {
        this.baseUrl = 'https://unit4productions.com';
        this.shareCount = this.getShareCount();
        this.referralCode = this.generateReferralCode();
        this.socialPlatforms = ['twitter', 'facebook', 'instagram', 'tiktok', 'discord', 'reddit'];
        this.viralThresholds = {
            bronze: 5,   // 5 shares
            silver: 25,  // 25 shares
            gold: 100,   // 100 shares
            legend: 500  // 500 shares
        };
        
        this.init();
    }
    
    // =================== INITIALIZATION ===================
    
    init() {
        this.createSocialUI();
        this.setupEventListeners();
        this.checkViralStatus();
        this.trackReferralClicks();
        
        console.log('Social Viral Growth System Initialized');
        console.log('Referral Code:', this.referralCode);
    }
    
    // =================== SOCIAL SHARING CORE ===================
    
    shareScore(score, level, platform = 'auto') {
        const shareData = {
            score: score,
            level: level,
            playerName: this.getPlayerName(),
            timestamp: Date.now()
        };
        
        const shareContent = this.generateShareContent(shareData);
        
        if (platform === 'auto') {
            this.showShareMenu(shareContent, shareData);
        } else {
            this.shareToSpecificPlatform(platform, shareContent, shareData);
        }
        
        // Track sharing event
        this.trackShareEvent('score_share', platform, shareData);
    }
    
    generateShareContent(shareData) {
        const messages = [
            `🎮 Just scored ${shareData.score.toLocaleString()} on Signal Breach Level ${shareData.level}! Can you beat it?`,
            `⚡ BREAKTHROUGH! ${shareData.score.toLocaleString()} points on Signal Breach! Who's up for the challenge?`,
            `🚀 Level ${shareData.level} conquered with ${shareData.score.toLocaleString()} points! Your turn to shine!`,
            `💥 Signal Breach mastery: ${shareData.score.toLocaleString()} points! Think you can do better?`,
            `🏆 New personal best: ${shareData.score.toLocaleString()} on Signal Breach! Challenge accepted?`
        ];
        
        const baseMessage = messages[Math.floor(Math.random() * messages.length)];
        const gameUrl = `${this.baseUrl}/games/signal-breach?challenge=${shareData.score}&ref=${this.referralCode}`;
        const hashtags = '#SignalBreach #IndieGame #PuzzleGame #Gaming #Unit4Productions';
        
        return {
            message: baseMessage,
            url: gameUrl,
            hashtags: hashtags,
            image: this.generateScoreImage(shareData)
        };
    }
    
    showShareMenu(shareContent, shareData) {
        const shareMenu = document.createElement('div');
        shareMenu.className = 'viral-share-menu';
        shareMenu.innerHTML = `
            <div class="share-overlay">
                <div class="share-content">
                    <div class="share-header">
                        <h3>🎉 Share Your Achievement!</h3>
                        <button class="close-share" onclick="this.closest('.viral-share-menu').remove()">×</button>
                    </div>
                    
                    <div class="achievement-display">
                        <div class="score-highlight">${shareData.score.toLocaleString()}</div>
                        <div class="level-info">Level ${shareData.level} Complete</div>
                        <div class="achievement-badge">🏆 New Achievement Unlocked!</div>
                    </div>
                    
                    <div class="share-benefits">
                        <div class="benefit-item">🎁 Get +1 Life for sharing</div>
                        <div class="benefit-item">👥 Friends get special bonus when they play</div>
                        <div class="benefit-item">🏆 Unlock Viral Achievement badges</div>
                    </div>
                    
                    <div class="share-buttons">
                        ${this.generateShareButtons(shareContent, shareData)}
                    </div>
                    
                    <div class="referral-section">
                        <h4>📱 Invite Friends Directly</h4>
                        <div class="referral-link-container">
                            <input type="text" class="referral-link" value="${shareContent.url}" readonly>
                            <button class="copy-link-btn" onclick="this.copyReferralLink()">Copy Link</button>
                        </div>
                        <div class="referral-stats">
                            Referrals: <span class="referral-count">${this.getReferralCount()}</span> | 
                            Bonus Lives Earned: <span class="bonus-lives">${this.getBonusLives()}</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(shareMenu);
        this.attachShareEventListeners(shareMenu, shareContent, shareData);
    }
    
    generateShareButtons(shareContent, shareData) {
        const buttons = [
            {
                platform: 'twitter',
                icon: '🐦',
                name: 'Twitter',
                color: '#1da1f2',
                url: `https://twitter.com/intent/tweet?text=${encodeURIComponent(shareContent.message)}&url=${shareContent.url}&hashtags=${shareContent.hashtags.replace('#', '')}`
            },
            {
                platform: 'facebook',
                icon: '👥',
                name: 'Facebook',
                color: '#4267b2',
                url: `https://www.facebook.com/sharer/sharer.php?u=${shareContent.url}&quote=${encodeURIComponent(shareContent.message)}`
            },
            {
                platform: 'discord',
                icon: '💬',
                name: 'Discord',
                color: '#7289da',
                action: 'copy_discord'
            },
            {
                platform: 'reddit',
                icon: '🔴',
                name: 'Reddit',
                color: '#ff4500',
                url: `https://reddit.com/submit?url=${shareContent.url}&title=${encodeURIComponent(shareContent.message)}`
            },
            {
                platform: 'native',
                icon: '📱',
                name: 'More...',
                color: '#6c5ce7',
                action: 'native_share'
            }
        ];
        
        return buttons.map(btn => `
            <button class="share-btn ${btn.platform}-btn" 
                    style="background-color: ${btn.color}" 
                    data-platform="${btn.platform}"
                    data-url="${btn.url || ''}"
                    data-action="${btn.action || 'url'}">
                <span class="share-icon">${btn.icon}</span>
                <span class="share-name">${btn.name}</span>
            </button>
        `).join('');
    }
    
    // =================== PLATFORM-SPECIFIC SHARING ===================
    
    shareToSpecificPlatform(platform, shareContent, shareData) {
        const shareActions = {
            twitter: () => this.shareToTwitter(shareContent),
            facebook: () => this.shareToFacebook(shareContent),
            discord: () => this.shareToDiscord(shareContent),
            reddit: () => this.shareToReddit(shareContent),
            instagram: () => this.shareToInstagram(shareContent, shareData),
            tiktok: () => this.shareToTikTok(shareContent, shareData),
            native: () => this.shareNative(shareContent)
        };
        
        const action = shareActions[platform];
        if (action) {
            action();
            this.rewardSharing(platform, shareData);
        }
    }
    
    shareToTwitter(shareContent) {
        const tweetUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(shareContent.message)}&url=${shareContent.url}&hashtags=${shareContent.hashtags.replace(/# /g, '')}`;
        window.open(tweetUrl, '_blank', 'width=550,height=420');
    }
    
    shareToFacebook(shareContent) {
        const fbUrl = `https://www.facebook.com/sharer/sharer.php?u=${shareContent.url}&quote=${encodeURIComponent(shareContent.message)}`;
        window.open(fbUrl, '_blank', 'width=600,height=400');
    }
    
    shareToDiscord(shareContent) {
        const discordMessage = `${shareContent.message}\n${shareContent.url}\n${shareContent.hashtags}`;
        
        // Copy to clipboard for Discord
        navigator.clipboard.writeText(discordMessage).then(() => {
            this.showNotification('Message copied! Paste it in Discord 💬', 'success');
        }).catch(() => {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = discordMessage;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            this.showNotification('Message copied! Paste it in Discord 💬', 'success');
        });
    }
    
    shareToReddit(shareContent) {
        const redditUrl = `https://reddit.com/submit?url=${shareContent.url}&title=${encodeURIComponent(shareContent.message)}`;
        window.open(redditUrl, '_blank');
    }
    
    shareNative(shareContent) {
        if (navigator.share) {
            navigator.share({
                title: 'Signal Breach Achievement',
                text: shareContent.message,
                url: shareContent.url
            }).then(() => {
                this.showNotification('Thanks for sharing! 🎉', 'success');
            });
        } else {
            // Fallback: copy to clipboard
            navigator.clipboard.writeText(`${shareContent.message} ${shareContent.url}`);
            this.showNotification('Link copied to clipboard! 📋', 'success');
        }
    }
    
    // =================== VIRAL MECHANICS ===================
    
    rewardSharing(platform, shareData) {
        // Increment share count
        this.shareCount++;
        localStorage.setItem('total_shares', this.shareCount.toString());
        
        // Give immediate reward
        const rewards = this.calculateSharingRewards(platform);
        this.giveRewards(rewards);
        
        // Check for viral achievements
        this.checkViralAchievements();
        
        // Track the sharing event
        this.trackShareEvent('share_completed', platform, shareData);
        
        // Show reward notification
        this.showSharingReward(rewards);
    }
    
    calculateSharingRewards(platform) {
        const baseRewards = {
            lives: 1,
            experience: 10,
            viralPoints: 1
        };
        
        // Platform-specific bonuses
        const platformBonuses = {
            twitter: { viralPoints: 2, experience: 15 },
            facebook: { viralPoints: 1, experience: 12 },
            discord: { viralPoints: 3, experience: 20 },
            reddit: { viralPoints: 4, experience: 25 },
            instagram: { viralPoints: 2, experience: 18 },
            tiktok: { viralPoints: 5, experience: 30 }
        };
        
        const bonus = platformBonuses[platform] || {};
        
        return {
            ...baseRewards,
            ...bonus
        };
    }
    
    giveRewards(rewards) {
        // Give lives (if not premium)
        if (typeof signalBreachPremium !== 'undefined' && !signalBreachPremium.isPremium) {
            signalBreachPremium.lives = Math.min(5, signalBreachPremium.lives + rewards.lives);
            signalBreachPremium.saveGameState();
        }
        
        // Add experience points
        const currentXP = parseInt(localStorage.getItem('player_experience') || '0');
        localStorage.setItem('player_experience', (currentXP + rewards.experience).toString());
        
        // Add viral points
        const currentVP = parseInt(localStorage.getItem('viral_points') || '0');
        localStorage.setItem('viral_points', (currentVP + rewards.viralPoints).toString());
    }
    
    checkViralAchievements() {
        const currentShares = this.shareCount;
        const viralPoints = parseInt(localStorage.getItem('viral_points') || '0');
        
        // Check share-based achievements
        Object.keys(this.viralThresholds).forEach(tier => {
            const threshold = this.viralThresholds[tier];
            const achievementKey = `viral_${tier}_unlocked`;
            
            if (currentShares >= threshold && !localStorage.getItem(achievementKey)) {
                this.unlockViralAchievement(tier, threshold);
                localStorage.setItem(achievementKey, 'true');
            }
        });
        
        // Check viral point achievements
        if (viralPoints >= 100 && !localStorage.getItem('viral_master_unlocked')) {
            this.unlockViralAchievement('viral_master', 100);
            localStorage.setItem('viral_master_unlocked', 'true');
        }
    }
    
    unlockViralAchievement(tier, threshold) {
        const achievements = {
            bronze: {
                title: '🥉 Bronze Sharer',
                description: 'Shared 5 achievements',
                reward: 'Bronze badge + 2 bonus lives'
            },
            silver: {
                title: '🥈 Silver Influencer', 
                description: 'Shared 25 achievements',
                reward: 'Silver badge + exclusive theme'
            },
            gold: {
                title: '🥇 Gold Ambassador',
                description: 'Shared 100 achievements',
                reward: 'Gold badge + permanent 2x viral points'
            },
            legend: {
                title: '👑 Viral Legend',
                description: 'Shared 500 achievements',
                reward: 'Legend status + revenue sharing opportunity'
            },
            viral_master: {
                title: '🌟 Viral Master',
                description: 'Earned 100 viral points',
                reward: 'Special abilities + community recognition'
            }
        };
        
        const achievement = achievements[tier];
        if (achievement) {
            this.showAchievementUnlock(achievement);
            this.trackShareEvent('achievement_unlocked', tier, { threshold: threshold });
        }
    }
    
    // =================== REFERRAL SYSTEM ===================
    
    generateReferralCode() {
        let code = localStorage.getItem('referral_code');
        if (!code) {
            code = 'REF' + Math.random().toString(36).substring(2, 8).toUpperCase();
            localStorage.setItem('referral_code', code);
        }
        return code;
    }
    
    trackReferralClicks() {
        // Check if user came from a referral
        const urlParams = new URLSearchParams(window.location.search);
        const referralCode = urlParams.get('ref');
        
        if (referralCode && referralCode !== this.referralCode) {
            this.handleIncomingReferral(referralCode);
        }
    }
    
    handleIncomingReferral(referralCode) {
        // Store the referral source
        localStorage.setItem('referred_by', referralCode);
        
        // Give new user bonus
        this.giveNewUserBonus();
        
        // Track referral click
        this.trackShareEvent('referral_click', 'incoming', { referralCode: referralCode });
        
        // Show welcome message
        this.showReferralWelcome();
    }
    
    giveNewUserBonus() {
        // Give new user extra lives and content
        if (typeof signalBreachPremium !== 'undefined') {
            signalBreachPremium.lives = 5; // Start with full lives
            signalBreachPremium.saveGameState();
        }
        
        // Unlock bonus content
        localStorage.setItem('new_user_bonus_active', 'true');
        
        // Mark user as referred for future rewards
        localStorage.setItem('is_referred_user', 'true');
    }
    
    processReferralRewards() {
        // Reward the referrer when new user makes progress
        const referredBy = localStorage.getItem('referred_by');
        
        if (referredBy) {
            // Send reward notification to referrer (would be server-side in production)
            this.sendReferralReward(referredBy);
            
            // Track referral conversion
            this.trackShareEvent('referral_conversion', 'completed', { 
                referrer: referredBy,
                action: 'level_complete' 
            });
        }
    }
    
    sendReferralReward(referrerCode) {
        // In production, this would be a server API call
        const rewardData = {
            referrerCode: referrerCode,
            reward: 'bonus_lives',
            amount: 3,
            timestamp: Date.now()
        };
        
        console.log('Referral reward sent:', rewardData);
        
        // Store locally for demo purposes
        const referralRewards = JSON.parse(localStorage.getItem('referral_rewards') || '[]');
        referralRewards.push(rewardData);
        localStorage.setItem('referral_rewards', JSON.stringify(referralRewards));
    }
    
    // =================== SOCIAL CHALLENGES ===================
    
    createSocialChallenge(challengeType, challengeData) {
        const challenges = {
            score_challenge: {
                title: `Beat My Score: ${challengeData.score.toLocaleString()}`,
                description: `I scored ${challengeData.score.toLocaleString()} on Signal Breach Level ${challengeData.level}. Can you do better?`,
                reward: 'Winner gets bragging rights + bonus lives',
                duration: '7 days'
            },
            speed_challenge: {
                title: `Speed Run Challenge`,
                description: `Complete Level ${challengeData.level} in under ${challengeData.time} seconds!`,
                reward: 'Speed demon badge + exclusive theme',
                duration: '3 days'
            },
            streak_challenge: {
                title: `Win Streak Challenge`,
                description: `Achieve a ${challengeData.streak} game win streak!`,
                reward: 'Streak master title + premium preview',
                duration: '10 days'
            }
        };
        
        const challenge = challenges[challengeType];
        if (challenge) {
            this.launchSocialChallenge(challenge, challengeData);
        }
    }
    
    launchSocialChallenge(challenge, challengeData) {
        const challengeUrl = this.generateChallengeUrl(challenge, challengeData);
        const shareMessage = `🏆 CHALLENGE ISSUED! ${challenge.title}\n${challenge.description}\n${challenge.reward}\n\nAccept the challenge: ${challengeUrl}`;
        
        // Auto-share the challenge
        this.shareToSpecificPlatform('twitter', {
            message: shareMessage,
            url: challengeUrl,
            hashtags: '#SignalBreach #Gaming #Challenge'
        }, challengeData);
        
        // Track challenge creation
        this.trackShareEvent('challenge_created', 'social', challengeData);
    }
    
    generateChallengeUrl(challenge, challengeData) {
        const params = new URLSearchParams({
            challenge: 'active',
            type: challenge.type || 'score',
            target: challengeData.score || challengeData.time || challengeData.streak,
            level: challengeData.level,
            creator: this.referralCode
        });
        
        return `${this.baseUrl}/games/signal-breach?${params.toString()}`;
    }
    
    // =================== CONTENT CREATION TOOLS ===================
    
    generateScoreImage(shareData) {
        // Create a canvas element for score image
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        canvas.width = 800;
        canvas.height = 400;
        
        // Background gradient
        const gradient = ctx.createLinearGradient(0, 0, 800, 400);
        gradient.addColorStop(0, '#667eea');
        gradient.addColorStop(1, '#764ba2');
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, 800, 400);
        
        // Add game title
        ctx.fillStyle = 'white';
        ctx.font = 'bold 48px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('SIGNAL BREACH', 400, 80);
        
        // Add score
        ctx.font = 'bold 72px Arial';
        ctx.fillStyle = '#4ecdc4';
        ctx.fillText(shareData.score.toLocaleString(), 400, 180);
        
        // Add level info
        ctx.font = '32px Arial';
        ctx.fillStyle = 'white';
        ctx.fillText(`Level ${shareData.level} Complete`, 400, 230);
        
        // Add call to action
        ctx.font = '24px Arial';
        ctx.fillStyle = '#ffeb3b';
        ctx.fillText('Can you beat this score?', 400, 280);
        
        // Add branding
        ctx.font = '20px Arial';
        ctx.fillStyle = 'rgba(255,255,255,0.8)';
        ctx.fillText('Unit4Productions Gaming', 400, 360);
        
        return canvas.toDataURL('image/png');
    }
    
    // =================== UI COMPONENTS ===================
    
    createSocialUI() {
        // Add share button to game UI
        const shareButton = document.createElement('button');
        shareButton.id = 'social-share-btn';
        shareButton.className = 'social-share-button';
        shareButton.innerHTML = '📱 Share';
        shareButton.onclick = () => this.shareScore(0, 1); // Default values
        
        // Add to game interface
        const gameUI = document.querySelector('.game-ui') || document.body;
        gameUI.appendChild(shareButton);
        
        // Add viral status indicator
        this.createViralStatusIndicator();
    }
    
    createViralStatusIndicator() {
        const indicator = document.createElement('div');
        indicator.id = 'viral-status-indicator';
        indicator.className = 'viral-status';
        
        const viralLevel = this.getViralLevel();
        const viralPoints = parseInt(localStorage.getItem('viral_points') || '0');
        
        indicator.innerHTML = `
            <div class="viral-badge ${viralLevel.tier}">
                <span class="viral-icon">${viralLevel.icon}</span>
                <span class="viral-text">${viralLevel.name}</span>
            </div>
            <div class="viral-points">${viralPoints} VP</div>
        `;
        
        document.body.appendChild(indicator);
    }
    
    getViralLevel() {
        const shares = this.shareCount;
        
        if (shares >= 500) return { tier: 'legend', icon: '👑', name: 'Viral Legend' };
        if (shares >= 100) return { tier: 'gold', icon: '🥇', name: 'Gold Ambassador' };
        if (shares >= 25) return { tier: 'silver', icon: '🥈', name: 'Silver Influencer' };
        if (shares >= 5) return { tier: 'bronze', icon: '🥉', name: 'Bronze Sharer' };
        
        return { tier: 'newbie', icon: '🌱', name: 'Rising Star' };
    }
    
    // =================== EVENT HANDLERS ===================
    
    setupEventListeners() {
        // Listen for game events to trigger social opportunities
        document.addEventListener('gameComplete', (e) => {
            const { score, level, newRecord } = e.detail;
            
            if (newRecord || score > 1000) {
                // Auto-suggest sharing for high scores or records
                setTimeout(() => {
                    this.showSharingOpportunity(score, level, 'high_score');
                }, 2000);
            }
        });
        
        // Listen for achievement unlocks
        document.addEventListener('achievementUnlocked', (e) => {
            const { achievement } = e.detail;
            this.showSharingOpportunity(0, 0, 'achievement', achievement);
        });
        
        // Handle copy link functionality
        window.copyReferralLink = () => {
            const linkInput = document.querySelector('.referral-link');
            linkInput.select();
            document.execCommand('copy');
            this.showNotification('Referral link copied! 📋', 'success');
        };
    }
    
    attachShareEventListeners(shareMenu, shareContent, shareData) {
        const shareButtons = shareMenu.querySelectorAll('.share-btn');
        
        shareButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const platform = btn.dataset.platform;
                const url = btn.dataset.url;
                const action = btn.dataset.action;
                
                if (action === 'url' && url) {
                    window.open(url, '_blank', 'width=600,height=400');
                } else if (action === 'copy_discord') {
                    this.shareToDiscord(shareContent);
                } else if (action === 'native_share') {
                    this.shareNative(shareContent);
                }
                
                // Reward sharing and close menu
                this.rewardSharing(platform, shareData);
                shareMenu.remove();
            });
        });
    }
    
    // =================== NOTIFICATIONS ===================
    
    showSharingOpportunity(score, level, context, data = {}) {
        if (this.shouldShowSharingPrompt(context)) {
            setTimeout(() => {
                this.shareScore(score, level, 'auto');
            }, 1500);
        }
    }
    
    shouldShowSharingPrompt(context) {
        const lastPrompt = localStorage.getItem('last_sharing_prompt');
        const timeSinceLastPrompt = Date.now() - (parseInt(lastPrompt) || 0);
        
        // Don't show prompts more than once every 5 minutes
        if (timeSinceLastPrompt < 5 * 60 * 1000) {
            return false;
        }
        
        // Context-specific logic
        const promptChances = {
            high_score: 0.8,
            new_record: 0.9,
            achievement: 0.6,
            level_complete: 0.3
        };
        
        const shouldShow = Math.random() < (promptChances[context] || 0.5);
        
        if (shouldShow) {
            localStorage.setItem('last_sharing_prompt', Date.now().toString());
        }
        
        return shouldShow;
    }
    
    showSharingReward(rewards) {
        this.showNotification(
            `🎉 Thanks for sharing! +${rewards.lives} life, +${rewards.viralPoints} viral points!`,
            'reward'
        );
    }
    
    showAchievementUnlock(achievement) {
        const modal = document.createElement('div');
        modal.className = 'achievement-unlock-modal';
        modal.innerHTML = `
            <div class="achievement-content">
                <div class="achievement-animation">✨</div>
                <h2>Achievement Unlocked!</h2>
                <div class="achievement-badge">${achievement.title}</div>
                <p>${achievement.description}</p>
                <div class="achievement-reward">Reward: ${achievement.reward}</div>
                <button onclick="this.closest('.achievement-unlock-modal').remove()">
                    Awesome! 🎉
                </button>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Auto-remove after 10 seconds
        setTimeout(() => {
            if (modal.parentNode) {
                modal.remove();
            }
        }, 10000);
    }
    
    showReferralWelcome() {
        this.showNotification(
            '🎁 Welcome! You got bonus lives from your friend! Start playing now!',
            'welcome'
        );
    }
    
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `viral-notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }
    
    // =================== ANALYTICS & TRACKING ===================
    
    trackShareEvent(eventName, platform, data = {}) {
        // Track with Unit4Analytics if available
        if (typeof Unit4Track !== 'undefined') {
            Unit4Track.share(platform, eventName);
        }
        
        // Track with Google Analytics
        if (typeof gtag !== 'undefined') {
            gtag('event', eventName, {
                'event_category': 'Social_Viral',
                'platform': platform,
                'share_count': this.shareCount,
                'viral_points': localStorage.getItem('viral_points') || '0',
                'custom_data': JSON.stringify(data)
            });
        }
        
        console.log('Social Event Tracked:', eventName, platform, data);
    }
    
    // =================== UTILITY FUNCTIONS ===================
    
    getShareCount() {
        return parseInt(localStorage.getItem('total_shares') || '0');
    }
    
    getReferralCount() {
        return parseInt(localStorage.getItem('referral_count') || '0');
    }
    
    getBonusLives() {
        return parseInt(localStorage.getItem('bonus_lives_earned') || '0');
    }
    
    getPlayerName() {
        return localStorage.getItem('player_name') || 'Signal Breach Player';
    }
}

// =================== CSS STYLES ===================

const socialViralStyles = `
<style>
    .viral-share-menu {
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
    
    .share-content {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        border-radius: 20px;
        padding: 2rem;
        max-width: 600px;
        width: 90%;
        color: white;
        position: relative;
        animation: slideUp 0.3s ease;
    }
    
    .share-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1.5rem;
    }
    
    .close-share {
        background: none;
        border: none;
        color: white;
        font-size: 1.5rem;
        cursor: pointer;
        opacity: 0.8;
    }
    
    .achievement-display {
        text-align: center;
        margin-bottom: 2rem;
        padding: 1rem;
        background: rgba(255, 255, 255, 0.1);
        border-radius: 15px;
    }
    
    .score-highlight {
        font-size: 3rem;
        font-weight: bold;
        color: #4ecdc4;
        text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
    }
    
    .level-info {
        font-size: 1.2rem;
        margin: 0.5rem 0;
    }
    
    .achievement-badge {
        background: #ffd700;
        color: #000;
        padding: 0.5rem 1rem;
        border-radius: 20px;
        display: inline-block;
        margin-top: 0.5rem;
        font-weight: bold;
    }
    
    .share-benefits {
        margin-bottom: 2rem;
    }
    
    .benefit-item {
        margin: 0.5rem 0;
        padding: 0.5rem;
        background: rgba(255, 255, 255, 0.1);
        border-radius: 10px;
    }
    
    .share-buttons {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
        gap: 1rem;
        margin-bottom: 2rem;
    }
    
    .share-btn {
        padding: 1rem;
        border: none;
        border-radius: 10px;
        color: white;
        cursor: pointer;
        transition: all 0.3s ease;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        font-weight: bold;
    }
    
    .share-btn:hover {
        transform: translateY(-2px);
        box-shadow: 0 5px 15px rgba(0,0,0,0.3);
    }
    
    .share-icon {
        font-size: 1.5rem;
    }
    
    .referral-section {
        border-top: 1px solid rgba(255, 255, 255, 0.2);
        padding-top: 1.5rem;
    }
    
    .referral-link-container {
        display: flex;
        gap: 0.5rem;
        margin: 1rem 0;
    }
    
    .referral-link {
        flex: 1;
        padding: 0.5rem;
        border: 1px solid rgba(255, 255, 255, 0.3);
        border-radius: 5px;
        background: rgba(255, 255, 255, 0.1);
        color: white;
    }
    
    .copy-link-btn {
        padding: 0.5rem 1rem;
        background: #4ecdc4;
        color: white;
        border: none;
        border-radius: 5px;
        cursor: pointer;
        font-weight: bold;
    }
    
    .referral-stats {
        font-size: 0.9rem;
        opacity: 0.8;
        text-align: center;
    }
    
    .viral-status {
        position: fixed;
        top: 20px;
        left: 20px;
        background: rgba(0, 0, 0, 0.8);
        color: white;
        padding: 0.5rem 1rem;
        border-radius: 20px;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        z-index: 1000;
    }
    
    .viral-badge {
        display: flex;
        align-items: center;
        gap: 0.3rem;
    }
    
    .viral-badge.legend { color: #ffd700; }
    .viral-badge.gold { color: #ffeb3b; }
    .viral-badge.silver { color: #e0e0e0; }
    .viral-badge.bronze { color: #cd7f32; }
    .viral-badge.newbie { color: #4caf50; }
    
    .social-share-button {
        background: linear-gradient(45deg, #667eea, #764ba2);
        color: white;
        border: none;
        padding: 0.75rem 1.5rem;
        border-radius: 25px;
        cursor: pointer;
        font-weight: bold;
        margin: 1rem;
        transition: all 0.3s ease;
    }
    
    .social-share-button:hover {
        transform: scale(1.05);
        box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
    }
    
    .viral-notification {
        position: fixed;
        top: 20px;
        right: 20px;
        background: linear-gradient(45deg, #4ecdc4, #44a08d);
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 10px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.3);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        max-width: 350px;
    }
    
    .viral-notification.show {
        transform: translateX(0);
    }
    
    .viral-notification.reward {
        background: linear-gradient(45deg, #ffd700, #ffeb3b);
        color: #000;
    }
    
    .viral-notification.welcome {
        background: linear-gradient(45deg, #ff6b6b, #ee5a24);
    }
    
    .notification-content {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }
    
    .notification-close {
        background: none;
        border: none;
        color: inherit;
        font-size: 1.2rem;
        cursor: pointer;
        margin-left: 1rem;
    }
    
    .achievement-unlock-modal {
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
        animation: fadeIn 0.5s ease;
    }
    
    .achievement-content {
        background: linear-gradient(135deg, #ffd700 0%, #ffeb3b 100%);
        color: #000;
        padding: 3rem;
        border-radius: 20px;
        text-align: center;
        max-width: 400px;
        animation: bounceIn 0.5s ease;
    }
    
    .achievement-animation {
        font-size: 3rem;
        animation: sparkle 2s infinite;
    }
    
    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }
    
    @keyframes slideUp {
        from { transform: translateY(50px); opacity: 0; }
        to { transform: translateY(0); opacity: 1; }
    }
    
    @keyframes bounceIn {
        0% { transform: scale(0.3); opacity: 0; }
        50% { transform: scale(1.05); }
        70% { transform: scale(0.9); }
        100% { transform: scale(1); opacity: 1; }
    }
    
    @keyframes sparkle {
        0%, 100% { transform: scale(1); opacity: 1; }
        50% { transform: scale(1.2); opacity: 0.8; }
    }
    
    @media (max-width: 768px) {
        .share-content {
            padding: 1.5rem;
            width: 95%;
        }
        
        .share-buttons {
            grid-template-columns: repeat(2, 1fr);
        }
        
        .score-highlight {
            font-size: 2rem;
        }
    }
</style>
`;

// Add styles to document
document.head.insertAdjacentHTML('beforeend', socialViralStyles);

// Global instance
let socialViralGrowth;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    socialViralGrowth = new SocialViralGrowthSystem();
    
    // Make it globally accessible
    window.SocialViralGrowth = socialViralGrowth;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SocialViralGrowthSystem;
}