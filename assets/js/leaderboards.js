/**
 * BotInc Game Promotion & Discovery System
 * Showcases top games and provides advertising opportunities - no user competition
 * 
 * DISABLED: Leaderboard system has been removed per user request
 */

// DISABLED - Leaderboard system removed
return;

class GamePromotionSystem {
    constructor() {
        this.gameStats = this.loadGameStats();
        this.promotedGames = this.loadPromotedGames();
        this.gameCategories = this.loadGameCategories();
        
        this.initializeGameShowcase();
        this.initializePromotions();
        this.setupGameDiscovery();
    }
    
    initializeLeaderboards() {
        this.games = {
            'signal-breach': {
                name: 'Signal Breach',
                scoreType: 'points',
                leaderboard: []
            },
            'neural-nexus': {
                name: 'Neural Nexus',
                scoreType: 'accuracy',
                leaderboard: []
            },
            'dot-conquest': {
                name: 'Dot Conquest',
                scoreType: 'territory',
                leaderboard: []
            },
            'chain-cascade': {
                name: 'Chain Cascade',
                scoreType: 'chains',
                leaderboard: []
            },
            'reflex-runner': {
                name: 'Reflex Runner',
                scoreType: 'distance',
                leaderboard: []
            }
        };
        
        // Initialize with fake data if empty
        if (Object.keys(this.leaderboards).length === 0) {
            this.seedLeaderboards();
        }
        
        this.createLeaderboardUI();
    }
    
    seedLeaderboards() {
        const fakeNames = [
            'CyberNinja', 'QuantumGamer', 'PixelMaster', 'CodeBreaker', 'NeonHacker',
            'DataPhoenix', 'SignalRunner', 'BinaryWolf', 'TechSamurai', 'NetRunner',
            'CyberStorm', 'QuantumLeap', 'PixelWizard', 'DigitalAce', 'VirtualHero'
        ];
        
        Object.keys(this.games).forEach(gameId => {
            this.leaderboards[gameId] = [];
            
            // Create 15 fake entries for each game
            for (let i = 0; i < 15; i++) {
                const name = fakeNames[Math.floor(Math.random() * fakeNames.length)] + Math.floor(Math.random() * 1000);
                const score = this.generateRealisticScore(gameId, i);
                
                this.leaderboards[gameId].push({
                    rank: i + 1,
                    playerName: name,
                    score: score,
                    timestamp: Date.now() - (i * 3600000), // Spread over hours
                    verified: Math.random() > 0.3 // 70% verified
                });
            }
        });
        
        this.saveLeaderboards();
    }
    
    generateRealisticScore(gameId, rank) {
        const baseScores = {
            'signal-breach': 5000 - (rank * 200),
            'neural-nexus': 98 - (rank * 2),
            'dot-conquest': 1000 - (rank * 50),
            'chain-cascade': 50 - (rank * 2),
            'reflex-runner': 800 - (rank * 30)
        };
        
        const base = baseScores[gameId] || 1000;
        const variation = Math.floor(Math.random() * 100) - 50;
        return Math.max(base + variation, 10);
    }
    
    createLeaderboardUI() {
        // Create floating leaderboard widget
        const widget = document.createElement('div');
        widget.id = 'leaderboard-widget';
        widget.className = 'leaderboard-widget collapsed';
        widget.innerHTML = `
            <div class="widget-header" onclick="this.parentNode.classList.toggle('collapsed')">
                <span class="widget-title">🏆 Leaderboards</span>
                <span class="widget-toggle">▼</span>
            </div>
            <div class="widget-content">
                <div class="game-selector">
                    <select id="gameSelect" onchange="window.leaderboardSystem.switchGame(this.value)">
                        ${Object.entries(this.games).map(([id, game]) => 
                            `<option value="${id}">${game.name}</option>`
                        ).join('')}
                    </select>
                </div>
                <div class="leaderboard-display" id="leaderboardDisplay"></div>
                <div class="player-rank" id="playerRank"></div>
                <div class="challenge-section" id="challengeSection"></div>
            </div>
        `;
        
        // Add widget styles
        if (!document.getElementById('leaderboard-styles')) {
            const styles = document.createElement('style');
            styles.id = 'leaderboard-styles';
            styles.textContent = `
                .leaderboard-widget {
                    position: fixed;
                    top: 50%;
                    right: -280px;
                    transform: translateY(-50%);
                    width: 300px;
                    background: rgba(20, 25, 35, 0.95);
                    border-radius: 15px 0 0 15px;
                    box-shadow: -5px 0 20px rgba(0,0,0,0.5);
                    color: white;
                    z-index: 9998;
                    transition: right 0.3s ease;
                    backdrop-filter: blur(10px);
                    border: 1px solid rgba(0, 255, 255, 0.3);
                }
                
                .leaderboard-widget:not(.collapsed) {
                    right: 0;
                }
                
                .widget-header {
                    padding: 15px 20px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    border-radius: 15px 0 0 0;
                    cursor: pointer;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    user-select: none;
                }
                
                .widget-title {
                    font-weight: bold;
                    font-size: 14px;
                }
                
                .widget-toggle {
                    transition: transform 0.3s;
                }
                
                .leaderboard-widget:not(.collapsed) .widget-toggle {
                    transform: rotate(180deg);
                }
                
                .widget-content {
                    padding: 20px;
                    max-height: 70vh;
                    overflow-y: auto;
                }
                
                .game-selector select {
                    width: 100%;
                    padding: 8px 12px;
                    background: rgba(255,255,255,0.1);
                    border: 1px solid rgba(0, 255, 255, 0.3);
                    border-radius: 6px;
                    color: white;
                    margin-bottom: 15px;
                }
                
                .leaderboard-display {
                    margin-bottom: 15px;
                }
                
                .leaderboard-entry {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 8px 0;
                    border-bottom: 1px solid rgba(255,255,255,0.1);
                    font-size: 12px;
                }
                
                .leaderboard-entry:last-child {
                    border-bottom: none;
                }
                
                .entry-rank {
                    width: 20px;
                    font-weight: bold;
                    color: #00ffff;
                }
                
                .entry-name {
                    flex: 1;
                    margin-left: 10px;
                    white-space: nowrap;
                    overflow: hidden;
                    text-overflow: ellipsis;
                }
                
                .entry-score {
                    font-weight: bold;
                    color: #ffff00;
                }
                
                .entry-verified {
                    color: #4CAF50;
                    margin-left: 5px;
                }
                
                .player-rank {
                    background: rgba(0, 255, 255, 0.1);
                    padding: 10px;
                    border-radius: 8px;
                    text-align: center;
                    margin-bottom: 15px;
                    border: 1px solid rgba(0, 255, 255, 0.3);
                }
                
                .challenge-section {
                    background: rgba(255, 165, 0, 0.1);
                    padding: 15px;
                    border-radius: 8px;
                    border: 1px solid rgba(255, 165, 0, 0.3);
                }
                
                .challenge-title {
                    font-weight: bold;
                    margin-bottom: 8px;
                    color: #FFA500;
                }
                
                .challenge-description {
                    font-size: 12px;
                    margin-bottom: 10px;
                    line-height: 1.4;
                }
                
                .challenge-progress {
                    background: rgba(255,255,255,0.1);
                    border-radius: 10px;
                    height: 8px;
                    overflow: hidden;
                    margin-bottom: 8px;
                }
                
                .challenge-progress-bar {
                    height: 100%;
                    background: linear-gradient(90deg, #FFA500, #FFD700);
                    transition: width 0.3s ease;
                }
                
                .challenge-reward {
                    font-size: 11px;
                    color: #FFD700;
                    text-align: center;
                }
                
                .top-three .entry-rank {
                    background: linear-gradient(135deg, #FFD700, #FFA500);
                    -webkit-background-clip: text;
                    -webkit-text-fill-color: transparent;
                    font-size: 14px;
                }
                
                @media (max-width: 768px) {
                    .leaderboard-widget {
                        width: 280px;
                        right: -260px;
                    }
                    
                    .leaderboard-widget:not(.collapsed) {
                        right: 0;
                    }
                }
            `;
            document.head.appendChild(styles);
        }
        
        document.body.appendChild(widget);
        
        // Initialize with first game
        this.switchGame(Object.keys(this.games)[0]);
        
        // Auto-expand after 3 seconds if user hasn't interacted
        setTimeout(() => {
            if (widget.classList.contains('collapsed')) {
                widget.classList.remove('collapsed');
                setTimeout(() => {
                    widget.classList.add('collapsed');
                }, 8000);
            }
        }, 3000);
    }
    
    switchGame(gameId) {
        if (!this.games[gameId]) return;
        
        const display = document.getElementById('leaderboardDisplay');
        const playerRank = document.getElementById('playerRank');
        const challengeSection = document.getElementById('challengeSection');
        
        if (!display) return;
        
        // Display top 10 scores
        const gameBoard = this.leaderboards[gameId] || [];
        const topScores = gameBoard.slice(0, 10);
        
        display.innerHTML = topScores.map((entry, index) => `
            <div class="leaderboard-entry ${index < 3 ? 'top-three' : ''}">
                <span class="entry-rank">#${entry.rank}</span>
                <span class="entry-name">${entry.playerName}</span>
                <span class="entry-score">${entry.score.toLocaleString()}</span>
                ${entry.verified ? '<span class="entry-verified">✓</span>' : ''}
            </div>
        `).join('');
        
        // Show player's current rank
        const playerScore = this.getPlayerScore(gameId);
        const playerPosition = this.calculatePlayerRank(gameId, playerScore);
        
        playerRank.innerHTML = `
            <div class="player-rank-info">
                <div><strong>Your Rank: #${playerPosition}</strong></div>
                <div>Score: ${playerScore.toLocaleString()}</div>
                <div class="rank-improvement">
                    ${this.getRankImprovementMessage(playerPosition)}
                </div>
            </div>
        `;
        
        // Show daily challenge for this game
        this.displayDailyChallengeForGame(gameId, challengeSection);
    }
    
    displayDailyChallengeForGame(gameId, container) {
        const challenge = this.getDailyChallengeForGame(gameId);
        if (!challenge || !container) return;
        
        const progress = this.getChallengeProgress(gameId, challenge);
        const progressPercent = Math.min((progress / challenge.target) * 100, 100);
        
        container.innerHTML = `
            <div class="challenge-title">⚡ Daily Challenge</div>
            <div class="challenge-description">${challenge.description}</div>
            <div class="challenge-progress">
                <div class="challenge-progress-bar" style="width: ${progressPercent}%"></div>
            </div>
            <div class="challenge-stats">
                Progress: ${progress}/${challenge.target}
            </div>
            <div class="challenge-reward">🏆 Reward: ${challenge.reward} points</div>
        `;
    }
    
    submitScore(gameId, score, playerName = 'Anonymous') {
        if (!this.games[gameId]) return false;
        
        // Check if this is a new high score for the player
        const currentBest = this.getPlayerScore(gameId);
        const isNewHighScore = score > currentBest;
        
        if (isNewHighScore) {
            // Update player's personal best
            this.updatePlayerScore(gameId, score);
            
            // Check if this score makes it to the leaderboard
            const leaderboard = this.leaderboards[gameId] || [];
            const lowestScore = leaderboard.length > 0 ? leaderboard[leaderboard.length - 1].score : 0;
            
            if (leaderboard.length < 100 || score > lowestScore) {
                this.addToLeaderboard(gameId, score, playerName);
                this.showLeaderboardAchievement(gameId, score);
            }
            
            // Check daily challenge progress
            this.updateChallengeProgress(gameId, score);
            
            // Trigger viral sharing for high scores
            if (this.isShareworthyScore(gameId, score)) {
                this.triggerScoreSharing(gameId, score);
            }
            
            return true;
        }
        
        return false;
    }
    
    addToLeaderboard(gameId, score, playerName) {
        const leaderboard = this.leaderboards[gameId] || [];
        
        const entry = {
            rank: 0, // Will be calculated after insertion
            playerName: playerName,
            score: score,
            timestamp: Date.now(),
            verified: true
        };
        
        leaderboard.push(entry);
        
        // Sort by score (descending) and update ranks
        leaderboard.sort((a, b) => b.score - a.score);
        leaderboard.forEach((entry, index) => {
            entry.rank = index + 1;
        });
        
        // Keep only top 100
        this.leaderboards[gameId] = leaderboard.slice(0, 100);
        
        this.saveLeaderboards();
        
        // Update UI if currently viewing this game
        const gameSelect = document.getElementById('gameSelect');
        if (gameSelect && gameSelect.value === gameId) {
            this.switchGame(gameId);
        }
    }
    
    showLeaderboardAchievement(gameId, score) {
        const rank = this.calculatePlayerRank(gameId, score);
        
        let achievementText = '';
        if (rank === 1) {
            achievementText = `🥇 NEW HIGH SCORE! You're #1 in ${this.games[gameId].name}!`;
        } else if (rank <= 3) {
            achievementText = `🥉 TOP 3! You're #${rank} in ${this.games[gameId].name}!`;
        } else if (rank <= 10) {
            achievementText = `🏆 TOP 10! You're #${rank} in ${this.games[gameId].name}!`;
        } else {
            achievementText = `📈 Leaderboard entry! You're #${rank} in ${this.games[gameId].name}!`;
        }
        
        const notification = document.createElement('div');
        notification.className = 'leaderboard-achievement';
        notification.innerHTML = `
            <div class="achievement-content">
                <div class="achievement-icon">${rank <= 3 ? '👑' : '🏆'}</div>
                <div class="achievement-text">
                    <h4>${achievementText}</h4>
                    <p>Score: ${score.toLocaleString()}</p>
                    <button onclick="window.viralMechanics.shareScore({
                        type: 'score',
                        score: ${score},
                        game: '${gameId}',
                        gameName: '${this.games[gameId].name}',
                        rank: ${rank}
                    })" class="share-score-btn">Share Achievement 🚀</button>
                </div>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(135deg, #FFD700, #FFA500);
            color: #333;
            padding: 25px;
            border-radius: 15px;
            box-shadow: 0 15px 35px rgba(0,0,0,0.3);
            z-index: 10004;
            text-align: center;
            animation: achievementPulse 0.6s ease-out;
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove after 8 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 8000);
        
        // Track achievement
        if (typeof trackAchievement !== 'undefined') {
            trackAchievement(`leaderboard_rank_${rank}`, gameId, score);
        }
    }
    
    initializeDailyChallenges() {
        const today = new Date().toDateString();
        const lastChallengeDate = localStorage.getItem('botinc_last_challenge_date');
        
        if (lastChallengeDate !== today) {
            this.generateDailyChallenges();
            localStorage.setItem('botinc_last_challenge_date', today);
        }
    }
    
    generateDailyChallenges() {
        const challengeTemplates = {
            'signal-breach': [
                { type: 'score', target: 2000, description: 'Score 2000+ points in Signal Breach', reward: 75 },
                { type: 'levels', target: 5, description: 'Complete 5 levels without detection', reward: 60 },
                { type: 'speed', target: 300, description: 'Complete a level in under 5 minutes', reward: 80 }
            ],
            'neural-nexus': [
                { type: 'accuracy', target: 95, description: 'Achieve 95%+ accuracy', reward: 70 },
                { type: 'streak', target: 20, description: 'Get 20 consecutive correct answers', reward: 85 },
                { type: 'speed', target: 100, description: 'Complete 100 questions in 10 minutes', reward: 90 }
            ],
            'dot-conquest': [
                { type: 'territory', target: 80, description: 'Control 80%+ of the map', reward: 65 },
                { type: 'speed', target: 180, description: 'Win a match in under 3 minutes', reward: 75 },
                { type: 'efficiency', target: 10, description: 'Win with fewer than 10 moves', reward: 85 }
            ],
            'chain-cascade': [
                { type: 'combo', target: 15, description: 'Create a 15+ chain combo', reward: 70 },
                { type: 'score', target: 5000, description: 'Score 5000+ points in one game', reward: 80 },
                { type: 'cascades', target: 50, description: 'Trigger 50 cascades in one session', reward: 75 }
            ],
            'reflex-runner': [
                { type: 'distance', target: 1000, description: 'Run 1000+ meters without crashing', reward: 65 },
                { type: 'powerups', target: 20, description: 'Collect 20 powerups in one run', reward: 70 },
                { type: 'obstacles', target: 100, description: 'Successfully avoid 100 obstacles', reward: 80 }
            ]
        };
        
        const todaysChallenges = {};
        
        Object.keys(challengeTemplates).forEach(gameId => {
            const templates = challengeTemplates[gameId];
            const randomChallenge = templates[Math.floor(Math.random() * templates.length)];
            
            todaysChallenges[gameId] = {
                ...randomChallenge,
                gameId: gameId,
                date: new Date().toDateString(),
                completed: false,
                progress: 0
            };
        });
        
        this.dailyChallenge = todaysChallenges;
        localStorage.setItem('botinc_daily_challenges', JSON.stringify(this.dailyChallenge));
    }
    
    getDailyChallengeForGame(gameId) {
        return this.dailyChallenge[gameId] || null;
    }
    
    getChallengeProgress(gameId, challenge) {
        if (!challenge) return 0;
        
        // Get progress from localStorage or player stats
        const progressKey = `challenge_progress_${gameId}_${challenge.type}`;
        return parseInt(localStorage.getItem(progressKey) || '0');
    }
    
    updateChallengeProgress(gameId, value) {
        const challenge = this.getDailyChallengeForGame(gameId);
        if (!challenge || challenge.completed) return;
        
        const progressKey = `challenge_progress_${gameId}_${challenge.type}`;
        const currentProgress = this.getChallengeProgress(gameId, challenge);
        const newProgress = Math.max(currentProgress, value);
        
        localStorage.setItem(progressKey, newProgress.toString());
        
        // Check if challenge is completed
        if (newProgress >= challenge.target && !challenge.completed) {
            this.completeDailyChallenge(gameId, challenge);
        }
        
        // Update UI if visible
        this.updateChallengeUI(gameId);
    }
    
    completeDailyChallenge(gameId, challenge) {
        challenge.completed = true;
        this.dailyChallenge[gameId] = challenge;
        localStorage.setItem('botinc_daily_challenges', JSON.stringify(this.dailyChallenge));
        
        // Award points
        if (window.viralMechanics) {
            window.viralMechanics.playerStats.totalPoints += challenge.reward;
            window.viralMechanics.savePlayerStats();
        }
        
        // Show completion notification
        this.showChallengeCompletedNotification(challenge);
        
        // Track completion
        if (typeof trackAchievement !== 'undefined') {
            trackAchievement('daily_challenge_completed', gameId, challenge.reward);
        }
    }
    
    showChallengeCompletedNotification(challenge) {
        const notification = document.createElement('div');
        notification.className = 'challenge-completed';
        notification.innerHTML = `
            <div class="completion-content">
                <div class="completion-icon">⚡</div>
                <div class="completion-text">
                    <h4>Daily Challenge Completed!</h4>
                    <p>${challenge.description}</p>
                    <div class="completion-reward">+${challenge.reward} points earned!</div>
                    <button onclick="window.viralMechanics.shareScore({
                        type: 'challenge',
                        game: '${challenge.gameId}',
                        gameName: '${this.games[challenge.gameId].name}',
                        challengeName: '${challenge.description}'
                    })" class="share-challenge-btn">Share Success 🎉</button>
                </div>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: linear-gradient(135deg, #4CAF50, #45a049);
            color: white;
            padding: 20px;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.3);
            z-index: 10005;
            animation: slideUp 0.5s ease-out;
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 7000);
    }
    
    updateChallengeUI(gameId) {
        const challengeSection = document.getElementById('challengeSection');
        const gameSelect = document.getElementById('gameSelect');
        
        if (challengeSection && gameSelect && gameSelect.value === gameId) {
            this.displayDailyChallengeForGame(gameId, challengeSection);
        }
    }
    
    isShareworthyScore(gameId, score) {
        const thresholds = {
            'signal-breach': 3000,
            'neural-nexus': 95,
            'dot-conquest': 800,
            'chain-cascade': 4000,
            'reflex-runner': 600
        };
        
        return score >= (thresholds[gameId] || 1000);
    }
    
    triggerScoreSharing(gameId, score) {
        if (window.viralMechanics) {
            setTimeout(() => {
                window.viralMechanics.suggestShare({
                    type: 'score',
                    score: score,
                    game: gameId,
                    gameName: this.games[gameId].name
                });
            }, 2000);
        }
    }
    
    getPlayerScore(gameId) {
        return this.playerScores[gameId] || 0;
    }
    
    updatePlayerScore(gameId, score) {
        this.playerScores[gameId] = Math.max(this.playerScores[gameId] || 0, score);
        this.savePlayerScores();
    }
    
    calculatePlayerRank(gameId, score) {
        const leaderboard = this.leaderboards[gameId] || [];
        
        // Find where this score would rank
        let rank = 1;
        for (const entry of leaderboard) {
            if (score <= entry.score) {
                rank++;
            } else {
                break;
            }
        }
        
        return rank;
    }
    
    getRankImprovementMessage(rank) {
        if (rank === 1) return '👑 You\'re the champion!';
        if (rank <= 3) return '🥉 Elite player!';
        if (rank <= 10) return '🏆 Top player!';
        if (rank <= 50) return '📈 Rising star!';
        return '🎯 Keep playing to climb higher!';
    }
    
    setupCompetitiveFeatures() {
        // Listen for score submissions from games
        document.addEventListener('gameScoreSubmitted', (event) => {
            const { game, score, playerName } = event.detail;
            this.submitScore(game, score, playerName || 'Anonymous Player');
        });
        
        // Weekly leaderboard reset (simulate)
        this.checkWeeklyReset();
    }
    
    checkWeeklyReset() {
        const now = new Date();
        const lastReset = new Date(localStorage.getItem('botinc_last_leaderboard_reset') || 0);
        const weeksSinceReset = Math.floor((now - lastReset) / (7 * 24 * 60 * 60 * 1000));
        
        if (weeksSinceReset >= 1) {
            this.performWeeklyReset();
            localStorage.setItem('botinc_last_leaderboard_reset', now.toISOString());
        }
    }
    
    performWeeklyReset() {
        // Archive current leaderboards and start fresh
        const archived = {
            week: new Date().toISOString(),
            leaderboards: { ...this.leaderboards }
        };
        
        localStorage.setItem('botinc_archived_leaderboards', JSON.stringify(archived));
        
        // Reset leaderboards but keep some top players for continuity
        Object.keys(this.leaderboards).forEach(gameId => {
            const currentBoard = this.leaderboards[gameId] || [];
            // Keep top 3 players with reduced scores for new season
            this.leaderboards[gameId] = currentBoard.slice(0, 3).map(entry => ({
                ...entry,
                score: Math.floor(entry.score * 0.7), // 30% score reduction
                timestamp: Date.now()
            }));
        });
        
        this.saveLeaderboards();
    }
    
    loadLeaderboards() {
        const stored = localStorage.getItem('botinc_leaderboards');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load leaderboards');
            }
        }
        return {};
    }
    
    saveLeaderboards() {
        localStorage.setItem('botinc_leaderboards', JSON.stringify(this.leaderboards));
    }
    
    loadDailyChallenge() {
        const stored = localStorage.getItem('botinc_daily_challenges');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load daily challenges');
            }
        }
        return {};
    }
    
    loadPlayerScores() {
        const stored = localStorage.getItem('botinc_player_scores');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to load player scores');
            }
        }
        return {};
    }
    
    savePlayerScores() {
        localStorage.setItem('botinc_player_scores', JSON.stringify(this.playerScores));
    }
    
    exportLeaderboardData() {
        return {
            leaderboards: this.leaderboards,
            dailyChallenges: this.dailyChallenge,
            playerScores: this.playerScores,
            timestamp: new Date().toISOString()
        };
    }
}

// Initialize leaderboard system - DISABLED
// const leaderboardSystem = new LeaderboardSystem();
// window.leaderboardSystem = leaderboardSystem;

// Add CSS animations - DISABLED
// if (!document.getElementById('leaderboard-animations')) {
//     const animations = document.createElement('style');
//     animations.id = 'leaderboard-animations';
    animations.textContent = `
        @keyframes achievementPulse {
            0% { transform: translate(-50%, -50%) scale(0.8); opacity: 0; }
            50% { transform: translate(-50%, -50%) scale(1.1); }
            100% { transform: translate(-50%, -50%) scale(1); opacity: 1; }
        }
        
        @keyframes slideUp {
            from { transform: translateY(100px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
        
        .share-score-btn, .share-challenge-btn {
            background: rgba(255,255,255,0.2);
            border: none;
            color: white;
            padding: 10px 15px;
            border-radius: 6px;
            cursor: pointer;
            margin-top: 10px;
            transition: all 0.3s;
        }
        
        .share-score-btn:hover, .share-challenge-btn:hover {
            background: rgba(255,255,255,0.3);
            transform: translateY(-2px);
        }
    `;
//     document.head.appendChild(animations);
// }