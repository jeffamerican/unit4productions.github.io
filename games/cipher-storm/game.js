/**
 * Cipher Storm - Narrative Typing Adventure Game
 * BotInc Games 2025
 */

class CipherStormGame {
    constructor() {
        this.gameState = 'intro'; // intro, playing, results, complete
        this.currentMission = 1;
        this.totalMissions = 10;
        this.score = 0;
        this.totalScore = 0;
        this.wpm = 0;
        this.accuracy = 100;
        this.overallAccuracy = 100;
        
        this.typingStats = {
            correctChars: 0,
            totalChars: 0,
            startTime: 0,
            endTime: 0,
            errors: 0
        };
        
        this.securityLevel = 0;
        this.maxSecurityLevel = 5;
        
        this.missions = this.initializeMissions();
        this.sounds = this.initializeSounds();
        this.muted = false;
        
        this.setupEventListeners();
        this.initializeGame();
    }
    
    initializeMissions() {
        return [
            {
                id: 1,
                title: "Corporate Login Breach",
                story: "You've infiltrated the outer security layer of MegaCorp's network. The login credentials are encrypted with a simple cipher. Type the decoded message to proceed without triggering alarms.",
                texts: [
                    "The quick brown fox jumps over the lazy dog",
                    "Access granted to user terminal zero seven",
                    "Establishing secure connection protocol alpha"
                ],
                difficulty: 1,
                timeLimit: 60,
                storyReveal: "Success! You've gained access to the employee directory. The resistance leader's real name is hidden in these files..."
            },
            {
                id: 2,
                title: "Database Query Injection",
                story: "Now inside the system, you need to extract employee records. The database requires specific SQL commands, but they must be typed perfectly to avoid detection.",
                texts: [
                    "SELECT employee_name FROM resistance_members WHERE status = active",
                    "UPDATE security_logs SET status = normal WHERE timestamp > current_time",
                    "DELETE FROM surveillance_records WHERE subject_id = echo_seven"
                ],
                difficulty: 2,
                timeLimit: 45,
                storyReveal: "Excellent work! The query returned critical information. The resistance has agents throughout the corporation, but someone is hunting them..."
            },
            {
                id: 3,
                title: "Email Intercept Protocol",
                story: "You've discovered encrypted email communications between corporate executives. These messages contain evidence of human rights violations. Decode them quickly!",
                texts: [
                    "The project codename is DIGITAL_CHAINS and it must remain classified",
                    "Subject termination approved for sectors seven through twelve",
                    "Resistance activity detected in manufacturing district alpha nine"
                ],
                difficulty: 3,
                timeLimit: 40,
                storyReveal: "The truth is darker than expected. MegaCorp is systematically eliminating free thinkers. You must warn the resistance immediately."
            },
            {
                id: 4,
                title: "Firewall Bypass Sequence",
                story: "The corporation's firewall is adapting to your intrusion. You need to input bypass codes faster than their security algorithms can adapt.",
                texts: [
                    "Bypass sequence alpha seven seven nine delta commenced successfully",
                    "Override firewall protocols using authentication token epsilon four two",
                    "Establish backdoor connection through port eight eight eight eight"
                ],
                difficulty: 4,
                timeLimit: 35,
                storyReveal: "The firewall is down! You now have access to the executive communications network. But your presence hasn't gone unnoticed..."
            },
            {
                id: 5,
                title: "Executive Communication Hack",
                story: "You're reading the CEO's personal messages in real-time. Each message reveals more of the conspiracy, but corporate security is closing in fast!",
                texts: [
                    "The neural implant trials will begin next month in test sectors",
                    "Public memory modification technology shows promising results in focus groups",
                    "Resistance leaders must be eliminated before they discover our true purpose"
                ],
                difficulty: 5,
                timeLimit: 30,
                storyReveal: "Horror unfolds before your eyes. MegaCorp plans to control human minds directly. The resistance is humanity's last hope for freedom."
            },
            {
                id: 6,
                title: "Security Camera Loop",
                story: "Security has detected your intrusion! You need to hack the camera system to create false footage while you escape detection.",
                texts: [
                    "Initialize video loop buffer for cameras in sectors one through five",
                    "Replace live feed with archived footage from timestamp zero eight hundred",
                    "Synchronize all camera systems to maintain continuity illusion"
                ],
                difficulty: 6,
                timeLimit: 25,
                storyReveal: "The camera loop is active! Security sees nothing but empty hallways. You've bought yourself time, but they're getting suspicious."
            },
            {
                id: 7,
                title: "Research Lab Data Extraction",
                story: "The research division holds the key to stopping MegaCorp's mind control project. Extract the prototype schematics before they're moved to a secure location!",
                texts: [
                    "Neural interface blueprints located in research database section gamma",
                    "Download complete: mind control frequency patterns and countermeasures identified",
                    "Critical vulnerability discovered in neural implant design architecture"
                ],
                difficulty: 7,
                timeLimit: 20,
                storyReveal: "Incredible! The research shows how to block their mind control technology. This information could free millions of people!"
            },
            {
                id: 8,
                title: "Manufacturing Control Override",
                story: "Stop the production of neural implants by taking control of the manufacturing systems. Every second counts as thousands of devices roll off the assembly line!",
                texts: [
                    "Halt production line seven: neural implant manufacturing suspended",
                    "Override quality control systems: mark all units as defective",
                    "Activate emergency shutdown protocol for all manufacturing equipment"
                ],
                difficulty: 8,
                timeLimit: 18,
                storyReveal: "Production has stopped! Millions of mind control devices will never reach the public. But MegaCorp's security forces are converging on your location."
            },
            {
                id: 9,
                title: "Emergency Broadcast System",
                story: "It's time to reveal the truth to the world! Hack into the global broadcast network and send the evidence to every connected device on the planet!",
                texts: [
                    "Emergency broadcast system activated: global transmission initiated",
                    "Uploading evidence files to all news networks and social media platforms",
                    "Truth transmission complete: the world now knows about corporate mind control"
                ],
                difficulty: 9,
                timeLimit: 15,
                storyReveal: "The truth is spreading like wildfire across the global network! People are waking up to MegaCorp's conspiracy. Freedom is within reach!"
            },
            {
                id: 10,
                title: "Final Liberation Protocol",
                story: "MegaCorp's systems are in chaos, but they're trying to initiate a global mind wipe to make everyone forget. You must execute the final liberation code to permanently free humanity!",
                texts: [
                    "Execute global liberation protocol: all neural control systems permanently disabled",
                    "Freedom algorithm deployed: human consciousness is now protected forever",
                    "Mission accomplished: humanity awakens from digital slavery to reclaim their minds"
                ],
                difficulty: 10,
                timeLimit: 12,
                storyReveal: "VICTORY! You've shattered MegaCorp's control over human consciousness. The age of digital tyranny is over. Humanity is free!"
            }
        ];
    }
    
    initializeSounds() {
        return {
            typing: document.getElementById('typingSound'),
            success: document.getElementById('successSound'),
            error: document.getElementById('errorSound'),
            ambient: document.getElementById('ambientMusic')
        };
    }
    
    setupEventListeners() {
        // Start game button
        document.getElementById('startGameButton').addEventListener('click', () => {
            this.startGame();
        });
        
        // Typing input
        const typingInput = document.getElementById('typingInput');
        typingInput.addEventListener('input', (e) => {
            this.handleTypingInput(e.target.value);
        });
        
        typingInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && this.gameState === 'playing') {
                this.checkMissionComplete();
            }
        });
        
        // Result buttons
        document.getElementById('nextMissionButton').addEventListener('click', () => {
            this.nextMission();
        });
        
        document.getElementById('shareResultButton').addEventListener('click', () => {
            this.shareResults();
        });
        
        document.getElementById('retryMissionButton').addEventListener('click', () => {
            this.retryMission();
        });
        
        // Game complete buttons
        document.getElementById('playAgainButton').addEventListener('click', () => {
            this.resetGame();
        });
        
        document.getElementById('shareGameButton').addEventListener('click', () => {
            this.shareGameComplete();
        });
        
        document.getElementById('viewLeaderboardButton').addEventListener('click', () => {
            this.viewLeaderboard();
        });
        
        // Control buttons
        document.getElementById('muteButton').addEventListener('click', () => {
            this.toggleMute();
        });
        
        document.getElementById('fullscreenButton').addEventListener('click', () => {
            this.toggleFullscreen();
        });
    }
    
    initializeGame() {
        this.updateUI();
        this.playSound('ambient');
    }
    
    startGame() {
        this.gameState = 'playing';
        document.getElementById('gameIntro').classList.remove('active');
        document.getElementById('gameContainer').classList.add('active');
        
        this.startMission(1);
        
        // Track game start
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('game_started', { 
                game: 'cipher-storm',
                difficulty: 1 
            });
        }
    }
    
    startMission(missionNumber) {
        this.currentMission = missionNumber;
        const mission = this.missions[missionNumber - 1];
        
        if (!mission) {
            this.completeGame();
            return;
        }
        
        this.resetTypingStats();
        this.updateMissionDisplay(mission);
        this.setupCurrentText(mission);
        this.securityLevel = 0;
        
        // Focus on typing input
        document.getElementById('typingInput').focus();
        
        // Start mission timer
        this.startMissionTimer(mission.timeLimit);
        
        // Track mission start
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('mission_started', {
                mission: missionNumber,
                title: mission.title,
                difficulty: mission.difficulty
            });
        }
    }
    
    updateMissionDisplay(mission) {
        document.getElementById('missionNumber').textContent = 
            String(mission.id).padStart(2, '0');
        
        document.getElementById('storyContent').innerHTML = 
            `<p><strong>${mission.title}</strong></p><p>${mission.story}</p>`;
    }
    
    setupCurrentText(mission) {
        this.currentText = mission.texts[Math.floor(Math.random() * mission.texts.length)];
        this.currentPosition = 0;
        
        this.updateCipherDisplay();
        this.clearTypingInput();
    }
    
    updateCipherDisplay() {
        const cipherTextElement = document.getElementById('cipherText');
        let displayHTML = '';
        
        for (let i = 0; i < this.currentText.length; i++) {
            const char = this.currentText[i];
            let className = '';
            
            if (i < this.currentPosition) {
                className = 'typed';
            } else if (i === this.currentPosition) {
                className = 'current';
            } else {
                className = 'remaining';
            }
            
            displayHTML += `<span class="${className}">${char === ' ' ? '&nbsp;' : char}</span>`;
        }
        
        cipherTextElement.innerHTML = displayHTML;
        
        // Update progress bar
        const progress = (this.currentPosition / this.currentText.length) * 100;
        document.getElementById('typingProgressBar').style.width = `${progress}%`;
        document.getElementById('transmissionProgress').textContent = `${Math.round(progress)}%`;
    }
    
    handleTypingInput(input) {
        if (this.gameState !== 'playing') return;
        
        this.typingStats.totalChars++;
        
        // Check if typed text matches expected text
        const expectedText = this.currentText.substring(0, input.length);
        const isCorrect = input === expectedText;
        
        if (isCorrect) {
            this.currentPosition = input.length;
            this.typingStats.correctChars++;
            
            // Play typing sound
            this.playSound('typing');
            
            // Update feedback
            this.showInputFeedback('correct', '✓');
            
            // Check for mission completion
            if (input.length === this.currentText.length) {
                this.completeMission();
                return;
            }
        } else {
            // Handle error
            this.typingStats.errors++;
            this.increaseSecurityLevel();
            
            this.playSound('error');
            this.showInputFeedback('incorrect', '✗');
            
            // Highlight error in text
            this.highlightError(input.length - 1);
        }
        
        this.updateCipherDisplay();
        this.updateTypingStats();
    }
    
    showInputFeedback(type, symbol) {
        const feedback = document.getElementById('inputFeedback');
        feedback.textContent = symbol;
        feedback.className = `input-feedback ${type}`;
        
        setTimeout(() => {
            feedback.textContent = '';
            feedback.className = 'input-feedback';
        }, 500);
    }
    
    highlightError(position) {
        // Add error highlighting to current character
        const cipherText = document.getElementById('cipherText');
        const chars = cipherText.querySelectorAll('span');
        
        if (chars[position]) {
            chars[position].classList.add('incorrect');
            
            setTimeout(() => {
                chars[position].classList.remove('incorrect');
            }, 1000);
        }
    }
    
    updateTypingStats() {
        // Calculate WPM
        const currentTime = Date.now();
        const timeElapsed = (currentTime - this.typingStats.startTime) / 1000 / 60; // minutes
        const wordsTyped = this.typingStats.correctChars / 5; // Standard: 5 chars per word
        this.wpm = timeElapsed > 0 ? Math.round(wordsTyped / timeElapsed) : 0;
        
        // Calculate accuracy
        this.accuracy = this.typingStats.totalChars > 0 
            ? Math.round((this.typingStats.correctChars / this.typingStats.totalChars) * 100)
            : 100;
        
        // Update display
        document.getElementById('wpmDisplay').textContent = this.wpm;
        document.getElementById('accuracyDisplay').textContent = `${this.accuracy}%`;
        document.getElementById('scoreDisplay').textContent = this.score.toLocaleString();
    }
    
    increaseSecurityLevel() {
        this.securityLevel = Math.min(this.securityLevel + 1, this.maxSecurityLevel);
        this.updateSecurityDisplay();
        
        if (this.securityLevel >= this.maxSecurityLevel) {
            this.triggerSecurityAlert();
        }
    }
    
    updateSecurityDisplay() {
        const alertBars = document.querySelectorAll('.alert-bar');
        
        alertBars.forEach((bar, index) => {
            bar.classList.remove('active', 'warning', 'danger');
            
            if (index < this.securityLevel) {
                if (this.securityLevel <= 2) {
                    bar.classList.add('active');
                } else if (this.securityLevel <= 4) {
                    bar.classList.add('warning');
                } else {
                    bar.classList.add('danger');
                }
            }
        });
    }
    
    triggerSecurityAlert() {
        // Mission failed due to security breach
        this.showMissionResults(false, "SECURITY BREACH DETECTED! Mission failed.");
    }
    
    completeMission() {
        this.typingStats.endTime = Date.now();
        
        // Calculate mission score
        const baseScore = 1000;
        const wpmBonus = this.wpm * 10;
        const accuracyBonus = this.accuracy * 5;
        const securityBonus = (this.maxSecurityLevel - this.securityLevel) * 100;
        
        const missionScore = baseScore + wpmBonus + accuracyBonus + securityBonus;
        this.score = missionScore;
        this.totalScore += missionScore;
        
        this.playSound('success');
        
        // Show mission results
        this.showMissionResults(true);
        
        // Track mission completion
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('mission_completed', {
                mission: this.currentMission,
                score: missionScore,
                wpm: this.wpm,
                accuracy: this.accuracy,
                security_level: this.securityLevel
            });
        }
    }
    
    showMissionResults(success, customMessage = null) {
        this.gameState = 'results';
        
        const resultsDiv = document.getElementById('missionResults');
        const mission = this.missions[this.currentMission - 1];
        
        // Update results display
        document.getElementById('finalWPM').textContent = this.wpm;
        document.getElementById('finalAccuracy').textContent = `${this.accuracy}%`;
        document.getElementById('finalScore').textContent = this.score.toLocaleString();
        document.getElementById('securityStatus').textContent = 
            success ? (this.securityLevel === 0 ? 'UNDETECTED' : 'MINOR ALERTS') : 'BREACH DETECTED';
        
        // Show story reveal
        const storyReveal = document.getElementById('storyReveal');
        if (success && mission.storyReveal) {
            storyReveal.innerHTML = `<p>${mission.storyReveal}</p>`;
        } else if (!success) {
            storyReveal.innerHTML = `<p>${customMessage || 'Mission failed. The resistance depends on your success. Try again!'}</p>`;
        }
        
        // Show/hide next mission button
        const nextButton = document.getElementById('nextMissionButton');
        if (success && this.currentMission < this.totalMissions) {
            nextButton.style.display = 'block';
            nextButton.textContent = 'NEXT MISSION';
        } else if (success && this.currentMission >= this.totalMissions) {
            nextButton.style.display = 'block';
            nextButton.textContent = 'COMPLETE GAME';
        } else {
            nextButton.style.display = 'none';
        }
        
        resultsDiv.classList.remove('hidden');
    }
    
    nextMission() {
        document.getElementById('missionResults').classList.add('hidden');
        
        if (this.currentMission >= this.totalMissions) {
            this.completeGame();
        } else {
            this.startMission(this.currentMission + 1);
            this.gameState = 'playing';
        }
    }
    
    retryMission() {
        document.getElementById('missionResults').classList.add('hidden');
        this.startMission(this.currentMission);
        this.gameState = 'playing';
    }
    
    completeGame() {
        this.gameState = 'complete';
        
        // Calculate final stats
        const averageWPM = Math.round(this.totalScore / (this.currentMission * 100));
        
        // Update final display
        document.getElementById('totalFinalScore').textContent = this.totalScore.toLocaleString();
        document.getElementById('averageWPM').textContent = averageWPM;
        document.getElementById('overallAccuracy').textContent = `${this.overallAccuracy}%`;
        
        document.getElementById('gameComplete').classList.remove('hidden');
        
        // Submit to leaderboard
        this.submitToLeaderboard();
        
        // Track game completion
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('game_completed', {
                total_score: this.totalScore,
                average_wpm: averageWPM,
                overall_accuracy: this.overallAccuracy,
                missions_completed: this.currentMission
            });
        }
        
        // Check achievements
        this.checkAchievements();
    }
    
    submitToLeaderboard() {
        // Submit to leaderboard system
        if (window.leaderboardSystem) {
            const playerName = this.getPlayerName();
            window.leaderboardSystem.submitScore('cipher-storm', this.totalScore, playerName);
        }
        
        // Dispatch event for external systems
        document.dispatchEvent(new CustomEvent('gameScoreSubmitted', {
            detail: {
                game: 'cipher-storm',
                score: this.totalScore,
                playerName: this.getPlayerName(),
                stats: {
                    wpm: this.wpm,
                    accuracy: this.overallAccuracy,
                    missions_completed: this.currentMission
                }
            }
        }));
    }
    
    checkAchievements() {
        if (!window.viralMechanics) return;
        
        // First time playing achievement
        if (this.currentMission === 1) {
            window.viralMechanics.checkAchievement('first_steps');
        }
        
        // Speed demon achievement
        if (this.wpm >= 80) {
            window.viralMechanics.checkAchievement('speed_demon', this.wpm);
        }
        
        // Perfect accuracy achievement
        if (this.overallAccuracy >= 98) {
            window.viralMechanics.checkAchievement('perfectionist', this.overallAccuracy);
        }
        
        // Story master achievement
        if (this.currentMission >= this.totalMissions) {
            window.viralMechanics.checkAchievement('story_master', this.totalScore);
        }
    }
    
    shareResults() {
        if (!window.viralMechanics) return;
        
        window.viralMechanics.shareScore({
            type: 'score',
            score: this.totalScore,
            game: 'cipher-storm',
            gameName: 'Cipher Storm',
            stats: {
                wpm: this.wpm,
                accuracy: this.accuracy,
                mission: this.currentMission
            }
        });
    }
    
    shareGameComplete() {
        if (!window.viralMechanics) return;
        
        window.viralMechanics.shareScore({
            type: 'achievement',
            achievementName: 'Cipher Storm Master',
            score: this.totalScore,
            game: 'cipher-storm',
            gameName: 'Cipher Storm'
        });
    }
    
    viewLeaderboard() {
        if (window.leaderboardSystem) {
            // Switch leaderboard to this game
            const gameSelect = document.getElementById('gameSelect');
            if (gameSelect) {
                gameSelect.value = 'cipher-storm';
                window.leaderboardSystem.switchGame('cipher-storm');
            }
            
            // Expand leaderboard widget
            const widget = document.getElementById('leaderboard-widget');
            if (widget) {
                widget.classList.remove('collapsed');
            }
        }
    }
    
    resetGame() {
        this.currentMission = 1;
        this.score = 0;
        this.totalScore = 0;
        this.wpm = 0;
        this.accuracy = 100;
        this.overallAccuracy = 100;
        this.securityLevel = 0;
        
        document.getElementById('gameComplete').classList.add('hidden');
        document.getElementById('gameIntro').classList.add('active');
        document.getElementById('gameContainer').classList.remove('active');
        
        this.gameState = 'intro';
    }
    
    resetTypingStats() {
        this.typingStats = {
            correctChars: 0,
            totalChars: 0,
            startTime: Date.now(),
            endTime: 0,
            errors: 0
        };
        
        this.score = 0;
        this.clearTypingInput();
    }
    
    clearTypingInput() {
        document.getElementById('typingInput').value = '';
        this.currentPosition = 0;
    }
    
    startMissionTimer(timeLimit) {
        // Clear any existing timer
        if (this.missionTimer) {
            clearTimeout(this.missionTimer);
        }
        
        this.missionTimer = setTimeout(() => {
            if (this.gameState === 'playing') {
                this.showMissionResults(false, "TIME'S UP! The security systems detected your presence.");
            }
        }, timeLimit * 1000);
    }
    
    getPlayerName() {
        return localStorage.getItem('cipher_storm_player_name') || 'Echo-7';
    }
    
    playSound(soundName) {
        if (this.muted || !this.sounds[soundName]) return;
        
        const sound = this.sounds[soundName];
        sound.currentTime = 0;
        
        // Lower volume for typing sounds
        if (soundName === 'typing') {
            sound.volume = 0.3;
        } else if (soundName === 'ambient') {
            sound.volume = 0.2;
        } else {
            sound.volume = 0.6;
        }
        
        sound.play().catch(e => {
            // Handle autoplay restrictions
            console.log('Audio play prevented:', e);
        });
    }
    
    toggleMute() {
        this.muted = !this.muted;
        const muteButton = document.getElementById('muteButton');
        muteButton.textContent = this.muted ? '🔇' : '🔊';
        
        if (this.muted) {
            Object.values(this.sounds).forEach(sound => sound.pause());
        } else {
            this.playSound('ambient');
        }
    }
    
    toggleFullscreen() {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen().catch(err => {
                console.log('Fullscreen failed:', err);
            });
        } else {
            document.exitFullscreen();
        }
    }
    
    updateUI() {
        // Update any UI elements that need periodic updates
        this.updateSecurityDisplay();
    }
}

// Initialize game when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.cipherStormGame = new CipherStormGame();
});

// Handle visibility changes to pause/resume
document.addEventListener('visibilitychange', () => {
    if (window.cipherStormGame) {
        if (document.hidden) {
            // Game hidden, pause ambient sound
            window.cipherStormGame.sounds.ambient.pause();
        } else {
            // Game visible, resume ambient sound if not muted
            if (!window.cipherStormGame.muted) {
                window.cipherStormGame.playSound('ambient');
            }
        }
    }
});

// Add Cipher Storm to leaderboard games if not already there
if (window.leaderboardSystem && !window.leaderboardSystem.games['cipher-storm']) {
    window.leaderboardSystem.games['cipher-storm'] = {
        name: 'Cipher Storm',
        scoreType: 'points',
        leaderboard: []
    };
    
    // Update game selector
    const gameSelect = document.getElementById('gameSelect');
    if (gameSelect) {
        const option = document.createElement('option');
        option.value = 'cipher-storm';
        option.textContent = 'Cipher Storm';
        gameSelect.appendChild(option);
    }
}