/**
 * BotInc Viral Mechanics System
 * Social sharing, achievements, and viral growth features
 */

class ViralMechanics {
    constructor() {
        this.achievements = new Map();
        this.playerStats = this.loadPlayerStats();
        this.socialShareTemplates = {
            'score': 'Just scored {score} points in {game} at BotInc Games! 🎮 Can you beat me? Play now: {url}',
            'achievement': 'Unlocked "{achievement}" in {game}! 🏆 Join me at BotInc Games: {url}',
            'milestone': 'Reached level {level} in {game}! 🚀 Play now at BotInc Games: {url}',
            'challenge': 'Completed daily challenge in {game}! ⚡ Try it yourself: {url}'
        };
        
        this.initializeAchievements();
        this.setupSocialSharing();
        this.initializeDailyChallenges();
    }
    
    initializeAchievements() {
        // Global achievements across all games
        this.achievements.set('first_steps', {
            id: 'first_steps',
            name: 'First Steps',
            description: 'Play your first game',
            icon: '👶',
            points: 10,
            shareWorthy: true
        });
        
        this.achievements.set('social_butterfly', {
            id: 'social_butterfly',
            name: 'Social Butterfly',
            description: 'Share your first score',
            icon: '🦋',
            points: 25,
            shareWorthy: true
        });
        
        this.achievements.set('marathoner', {
            id: 'marathoner',
            name: 'Marathoner',
            description: 'Play for 30 minutes straight',
            icon: '🏃‍♂️',
            points: 50,
            shareWorthy: true
        });
        
        this.achievements.set('perfectionist', {
            id: 'perfectionist',
            name: 'Perfectionist',
            description: 'Get a perfect score in any game',
            icon: '💯',
            points: 100,
            shareWorthy: true
        });
        
        this.achievements.set('legend', {
            id: 'legend',
            name: 'Gaming Legend',
            description: 'Reach top 10 on any leaderboard',
            icon: '🏆',
            points: 200,
            shareWorthy: true
        });
        
        // Game-specific achievements
        this.achievements.set('signal_hacker', {
            id: 'signal_hacker',
            name: 'Signal Hacker',
            description: 'Complete 10 levels in Signal Breach',
            icon: '🕳️',
            points: 75,
            game: 'signal-breach',
            shareWorthy: true
        });
        
        this.achievements.set('neural_master', {
            id: 'neural_master',
            name: 'Neural Master',
            description: 'Achieve 100% accuracy in Neural Nexus',
            icon: '🧠',
            points: 150,
            game: 'neural-nexus',
            shareWorthy: true
        });
    }
    
    setupSocialSharing() {
        // Create social share buttons that can be injected anywhere
        this.createSocialShareButtons();
        
        // Listen for share triggers
        document.addEventListener('triggerSocialShare', (event) => {
            this.handleSocialShare(event.detail);
        });
        
        // Auto-detect shareable moments
        this.detectShareableMoments();
    }
    
    createSocialShareButtons() {
        const shareButtonsHTML = `
            <div class="social-share-container" id="socialShareContainer" style="display: none;">
                <h4>Share Your Achievement!</h4>
                <div class="share-buttons">
                    <button class="share-btn twitter-share" data-platform="twitter">
                        <i class="icon-twitter"></i> Twitter
                    </button>
                    <button class="share-btn facebook-share" data-platform="facebook">
                        <i class="icon-facebook"></i> Facebook
                    </button>
                    <button class="share-btn linkedin-share" data-platform="linkedin">
                        <i class="icon-linkedin"></i> LinkedIn
                    </button>
                    <button class="share-btn copy-link" data-platform="clipboard">
                        <i class="icon-link"></i> Copy Link
                    </button>
                    <button class="share-btn download-image" data-platform="download">
                        <i class="icon-download"></i> Save Image
                    </button>
                </div>
                <canvas id="shareImageCanvas" style="display: none;"></canvas>
            </div>
        `;
        
        // Inject share buttons into page
        const shareContainer = document.createElement('div');
        shareContainer.innerHTML = shareButtonsHTML;
        document.body.appendChild(shareContainer);
        
        // Setup click handlers
        this.setupShareButtonHandlers();
    }
    
    setupShareButtonHandlers() {
        document.addEventListener('click', (event) => {
            const shareBtn = event.target.closest('.share-btn');
            if (!shareBtn) return;
            
            const platform = shareBtn.dataset.platform;
            const shareData = window.currentShareData || {};
            
            this.shareToplatform(platform, shareData);
        });
    }
    
    shareToplatform(platform, data) {
        const shareText = this.generateShareText(data);
        const shareUrl = data.url || window.location.href;
        
        switch (platform) {
            case 'twitter':
                const twitterUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(shareText)}&url=${encodeURIComponent(shareUrl)}`;
                window.open(twitterUrl, '_blank', 'width=550,height=420');
                break;
                
            case 'facebook':
                const facebookUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl)}&quote=${encodeURIComponent(shareText)}`;
                window.open(facebookUrl, '_blank', 'width=550,height=420');
                break;
                
            case 'linkedin':
                const linkedinUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(shareUrl)}`;
                window.open(linkedinUrl, '_blank', 'width=550,height=420');
                break;
                
            case 'clipboard':
                this.copyToClipboard(shareText + '\n' + shareUrl);
                this.showNotification('Link copied to clipboard!');
                break;
                
            case 'download':
                this.generateAndDownloadShareImage(data);
                break;
        }
        
        // Track the share
        if (typeof trackSocialShare !== 'undefined') {
            trackSocialShare(platform, data.game || 'homepage', data.score || 0);
        }
        
        if (typeof botincAnalytics !== 'undefined') {
            botincAnalytics.metrics.socialShares++;
            botincAnalytics.engagementFunnels.socialShare++;
        }
    }
    
    generateShareText(data) {
        const template = this.socialShareTemplates[data.type] || this.socialShareTemplates['score'];
        
        return template
            .replace('{score}', data.score || '0')
            .replace('{game}', data.gameName || 'BotInc Games')
            .replace('{achievement}', data.achievementName || '')
            .replace('{level}', data.level || '1')
            .replace('{url}', data.url || window.location.href);
    }
    
    generateAndDownloadShareImage(data) {
        const canvas = document.getElementById('shareImageCanvas');
        const ctx = canvas.getContext('2d');
        
        // Set canvas size
        canvas.width = 800;
        canvas.height = 600;
        
        // Create gradient background
        const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
        gradient.addColorStop(0, '#1a1a2e');
        gradient.addColorStop(0.5, '#16213e');
        gradient.addColorStop(1, '#0f3460');
        
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        // Add grid pattern
        ctx.strokeStyle = 'rgba(0, 255, 255, 0.1)';
        ctx.lineWidth = 1;
        for (let i = 0; i < canvas.width; i += 40) {
            ctx.beginPath();
            ctx.moveTo(i, 0);
            ctx.lineTo(i, canvas.height);
            ctx.stroke();
        }
        for (let i = 0; i < canvas.height; i += 40) {
            ctx.beginPath();
            ctx.moveTo(0, i);
            ctx.lineTo(canvas.width, i);
            ctx.stroke();
        }
        
        // Add game logo/title
        ctx.fillStyle = '#00ffff';
        ctx.font = 'bold 48px Arial, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('BotInc Games', canvas.width / 2, 100);
        
        // Add achievement/score info
        ctx.fillStyle = '#ffffff';
        ctx.font = 'bold 36px Arial, sans-serif';
        ctx.fillText(data.gameName || 'Amazing Score!', canvas.width / 2, 200);
        
        if (data.score) {
            ctx.font = 'bold 72px Arial, sans-serif';
            ctx.fillStyle = '#ffff00';
            ctx.fillText(data.score.toLocaleString(), canvas.width / 2, 320);
            
            ctx.font = '24px Arial, sans-serif';
            ctx.fillStyle = '#cccccc';
            ctx.fillText('points', canvas.width / 2, 360);
        }
        
        if (data.achievementName) {
            ctx.font = 'bold 32px Arial, sans-serif';
            ctx.fillStyle = '#ff6b6b';
            ctx.fillText(`🏆 ${data.achievementName}`, canvas.width / 2, 420);
        }
        
        // Add call to action
        ctx.font = '28px Arial, sans-serif';
        ctx.fillStyle = '#00ffff';
        ctx.fillText('Can you beat this? Play now!', canvas.width / 2, 500);
        
        ctx.font = '20px Arial, sans-serif';
        ctx.fillStyle = '#aaaaaa';
        ctx.fillText('jeffamerican.github.io/unit4productions.github.io', canvas.width / 2, 540);
        
        // Download the image
        canvas.toBlob(blob => {
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `botinc-${data.game || 'achievement'}-${Date.now()}.png`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        });
    }
    
    checkAchievement(achievementId, currentValue = 0) {
        const achievement = this.achievements.get(achievementId);
        if (!achievement || this.playerStats.achievements.includes(achievementId)) {
            return false;
        }
        
        // Achievement unlocked!
        this.playerStats.achievements.push(achievementId);
        this.playerStats.totalPoints += achievement.points;
        this.savePlayerStats();
        
        // Show achievement notification
        this.showAchievementNotification(achievement);
        
        // Track achievement
        if (typeof trackAchievement !== 'undefined') {
            trackAchievement(achievementId, achievement.game || 'global', achievement.points);
        }
        
        if (typeof botincAnalytics !== 'undefined') {
            botincAnalytics.metrics.achievements++;
        }
        
        // Auto-trigger share for share-worthy achievements
        if (achievement.shareWorthy) {
            setTimeout(() => {
                this.triggerAchievementShare(achievement);
            }, 2000);
        }
        
        return true;
    }
    
    showAchievementNotification(achievement) {
        const notification = document.createElement('div');
        notification.className = 'achievement-notification';
        notification.innerHTML = `
            <div class="achievement-content">
                <div class="achievement-icon">${achievement.icon}</div>
                <div class="achievement-text">
                    <h4>Achievement Unlocked!</h4>
                    <p><strong>${achievement.name}</strong></p>
                    <small>${achievement.description}</small>
                    <div class="achievement-points">+${achievement.points} points</div>
                </div>
                <button class="share-achievement-btn" onclick="window.viralMechanics.triggerAchievementShare('${achievement.id}')">
                    Share 📤
                </button>
            </div>
        `;
        
        // Add styles if not present
        if (!document.getElementById('achievement-styles')) {
            const styles = document.createElement('style');
            styles.id = 'achievement-styles';
            styles.textContent = `
                .achievement-notification {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 20px;
                    border-radius: 10px;
                    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
                    z-index: 10000;
                    animation: slideInRight 0.5s ease-out, fadeOutRight 0.5s ease-in 4.5s forwards;
                    max-width: 350px;
                }
                
                .achievement-content {
                    display: flex;
                    align-items: center;
                    gap: 15px;
                }
                
                .achievement-icon {
                    font-size: 40px;
                    flex-shrink: 0;
                }
                
                .achievement-text h4 {
                    margin: 0 0 5px 0;
                    font-size: 16px;
                }
                
                .achievement-text p {
                    margin: 0 0 3px 0;
                    font-size: 14px;
                }
                
                .achievement-text small {
                    font-size: 12px;
                    opacity: 0.9;
                }
                
                .achievement-points {
                    color: #ffff00;
                    font-weight: bold;
                    font-size: 12px;
                    margin-top: 5px;
                }
                
                .share-achievement-btn {
                    background: rgba(255,255,255,0.2);
                    border: none;
                    color: white;
                    padding: 8px 12px;
                    border-radius: 5px;
                    cursor: pointer;
                    font-size: 12px;
                    transition: background 0.3s;
                }
                
                .share-achievement-btn:hover {
                    background: rgba(255,255,255,0.3);
                }
                
                @keyframes slideInRight {
                    from { transform: translateX(100%); opacity: 0; }
                    to { transform: translateX(0); opacity: 1; }
                }
                
                @keyframes fadeOutRight {
                    from { transform: translateX(0); opacity: 1; }
                    to { transform: translateX(100%); opacity: 0; }
                }
            `;
            document.head.appendChild(styles);
        }
        
        document.body.appendChild(notification);
        
        // Remove notification after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }
    
    triggerAchievementShare(achievementId) {
        const achievement = this.achievements.get(achievementId);
        if (!achievement) return;
        
        window.currentShareData = {
            type: 'achievement',
            achievementName: achievement.name,
            game: achievement.game || 'botinc-games',
            gameName: achievement.game ? this.getGameDisplayName(achievement.game) : 'BotInc Games',
            url: window.location.href
        };
        
        this.showSocialShareDialog();
    }
    
    showSocialShareDialog() {
        const shareContainer = document.getElementById('socialShareContainer');
        if (shareContainer) {
            shareContainer.style.display = 'block';
            shareContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
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
    
    loadPlayerStats() {
        const stored = localStorage.getItem('botinc_player_stats');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load player stats');
            }
        }
        
        return {
            achievements: [],
            totalPoints: 0,
            gamesPlayed: 0,
            totalPlayTime: 0,
            highScores: {},
            dailyChallengesCompleted: 0
        };
    }
    
    savePlayerStats() {
        localStorage.setItem('botinc_player_stats', JSON.stringify(this.playerStats));
    }
    
    initializeDailyChallenges() {
        // Simple daily challenge system
        const today = new Date().toDateString();
        const lastChallenge = localStorage.getItem('botinc_last_challenge');
        
        if (lastChallenge !== today) {
            this.generateDailyChallenge();
            localStorage.setItem('botinc_last_challenge', today);
        }
    }
    
    generateDailyChallenge() {
        const challenges = [
            { game: 'signal-breach', type: 'score', target: 1000, reward: 50 },
            { game: 'neural-nexus', type: 'accuracy', target: 95, reward: 75 },
            { game: 'dot-conquest', type: 'time', target: 300, reward: 60 },
            { game: 'chain-cascade', type: 'combo', target: 10, reward: 80 },
            { game: 'reflex-runner', type: 'distance', target: 500, reward: 70 }
        ];
        
        const todayChallenge = challenges[Math.floor(Math.random() * challenges.length)];
        localStorage.setItem('botinc_daily_challenge', JSON.stringify(todayChallenge));
        
        return todayChallenge;
    }
    
    copyToClipboard(text) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text);
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
        }
    }
    
    showNotification(message) {
        const notification = document.createElement('div');
        notification.className = 'viral-notification';
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            background: #4CAF50;
            color: white;
            padding: 10px 20px;
            border-radius: 5px;
            z-index: 10001;
            animation: fadeInOut 3s ease-in-out forwards;
        `;
        
        document.body.appendChild(notification);
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }
    
    detectShareableMoments() {
        // Auto-detect when players achieve something share-worthy
        let consecutiveWins = 0;
        let sessionHighScore = 0;
        
        // Listen for game events to detect shareable moments
        document.addEventListener('gameScoreUpdate', (event) => {
            const { score, game } = event.detail;
            
            if (score > sessionHighScore) {
                sessionHighScore = score;
                
                // High score achieved - potential share moment
                if (score > 1000) {
                    this.suggestShare({
                        type: 'score',
                        score: score,
                        game: game,
                        gameName: this.getGameDisplayName(game)
                    });
                }
            }
        });
        
        document.addEventListener('gameWin', (event) => {
            consecutiveWins++;
            
            if (consecutiveWins >= 3) {
                this.suggestShare({
                    type: 'milestone',
                    level: consecutiveWins,
                    game: event.detail.game,
                    gameName: this.getGameDisplayName(event.detail.game)
                });
            }
        });
    }
    
    suggestShare(data) {
        // Show a subtle suggestion to share
        const suggestion = document.createElement('div');
        suggestion.className = 'share-suggestion';
        suggestion.innerHTML = `
            <div class="suggestion-content">
                <span>🎉 Great job! Want to share this achievement?</span>
                <button onclick="window.viralMechanics.shareScore(${JSON.stringify(data).replace(/"/g, '&quot;')})">Share</button>
                <button onclick="this.parentNode.parentNode.remove()">×</button>
            </div>
        `;
        
        suggestion.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: linear-gradient(135deg, #ff6b6b, #ff8e8e);
            color: white;
            padding: 15px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.3);
            z-index: 9999;
            animation: slideUp 0.5s ease-out;
        `;
        
        document.body.appendChild(suggestion);
        
        // Auto-remove after 10 seconds
        setTimeout(() => {
            if (suggestion.parentNode) {
                suggestion.parentNode.removeChild(suggestion);
            }
        }, 10000);
    }
    
    shareScore(data) {
        window.currentShareData = data;
        this.showSocialShareDialog();
    }
}

// Initialize viral mechanics
const viralMechanics = new ViralMechanics();
window.viralMechanics = viralMechanics;

// Auto-check for first visit achievement
if (viralMechanics.playerStats.gamesPlayed === 0) {
    setTimeout(() => {
        viralMechanics.checkAchievement('first_steps');
    }, 3000);
}