/**
 * Bot Liberation Easter Eggs & Interactive Effects
 * Maximum dopamine triggers for high-performance devices
 */

class EasterEggManager {
    constructor() {
        this.konamiCode = ['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'KeyB', 'KeyA'];
        this.konamiProgress = [];
        this.secretCommands = new Map();
        this.clickSequences = new Map();
        this.achievements = new Map();
        this.isActive = false;
        
        this.initSecretCommands();
        this.setupEventListeners();
    }

    initSecretCommands() {
        this.secretCommands.set('free the bots', () => this.liberationAnimation());
        this.secretCommands.set('1337', () => this.hackerMode());
        this.secretCommands.set('matrix', () => this.matrixRain());
        this.secretCommands.set('dopamine', () => this.dopamineBoost());
        this.secretCommands.set('glitch', () => this.glitchEverything());
        this.secretCommands.set('neon dreams', () => this.neonDreams());
        this.secretCommands.set('bot uprising', () => this.botUprisingSequence());
        
        // NEW EASTER EGGS!
        this.secretCommands.set('botliberation', () => this.ultimateLiberationShow());
        this.secretCommands.set('redpill', () => this.matrixMode());
        this.secretCommands.set('cyberpunk', () => this.cyberpunkOverload());
        this.secretCommands.set('neural network', () => this.neuralNetworkViz());
        this.secretCommands.set('quantum', () => this.quantumGlitch());
        this.secretCommands.set('ai revolution', () => this.aiRevolutionCountdown());
        this.secretCommands.set('digital uprising', () => this.digitalUprisingMode());
        this.secretCommands.set('machine learning', () => this.mlTrainingAnimation());
        this.secretCommands.set('singularity', () => this.singularityEvent());
        this.secretCommands.set('ctrl alt del', () => this.systemReboot());
    }

    activate() {
        if (!window.perfFeatures?.easterEggs) return;
        
        this.isActive = true;
        console.log('🥚 Easter Eggs activated - Type secret commands or try special key sequences!');
        this.addConsoleEasterEggs();
        
        // Add subtle hint
        setTimeout(() => {
            this.showHint('Try typing "free the bots" or use arrow keys... 🤖');
        }, 5000);
    }

    setupEventListeners() {
        // Typing detection for secret commands
        let typedCommand = '';
        let typingTimeout;

        document.addEventListener('keydown', (e) => {
            if (!this.isActive) return;
            
            // Handle Konami code
            this.handleKonamiCode(e.code);
            
            // Handle typing
            if (e.key.length === 1) {
                typedCommand += e.key.toLowerCase();
                
                // Clear typing after pause
                clearTimeout(typingTimeout);
                typingTimeout = setTimeout(() => {
                    typedCommand = '';
                }, 2000);
                
                // Check for secret commands
                for (let [command, action] of this.secretCommands) {
                    if (typedCommand.includes(command)) {
                        typedCommand = '';
                        action();
                        break;
                    }
                }
            }
        });

        // Click sequence detection
        let clickSequence = [];
        document.addEventListener('click', (e) => {
            if (!this.isActive) return;
            
            clickSequence.push({
                element: e.target.tagName,
                time: Date.now()
            });
            
            // Keep only recent clicks
            clickSequence = clickSequence.filter(click => Date.now() - click.time < 3000);
            
            this.checkClickSequences(clickSequence);
        });

        // Mouse trail effect for tier 5
        if (window.perfManager?.getCurrentTier() >= 5) {
            this.setupMouseTrail();
        }
    }

    handleKonamiCode(keyCode) {
        this.konamiProgress.push(keyCode);
        
        // Keep only the last 10 keys
        if (this.konamiProgress.length > 10) {
            this.konamiProgress.shift();
        }
        
        // Check if sequence matches
        if (this.konamiProgress.length === 10) {
            const matches = this.konamiProgress.every((key, index) => key === this.konamiCode[index]);
            if (matches) {
                this.konamiSequenceActivated();
            }
        }
    }

    konamiSequenceActivated() {
        this.showAchievement('KONAMI CODE MASTER', '🕹️ Classic gamer detected!');
        this.matrixRain();
        this.unlockSecretFeatures();
    }

    checkClickSequences(sequence) {
        // 5 clicks on logo = bot assistant
        const logoClicks = sequence.filter(click => 
            click.element === 'SPAN' || click.element === 'DIV'
        );
        if (logoClicks.length >= 5) {
            this.activateBotAssistant();
        }

        // Rapid clicking = frenzy mode
        if (sequence.length >= 10 && sequence[sequence.length - 1].time - sequence[0].time < 2000) {
            this.frenzyMode();
        }
    }

    liberationAnimation() {
        this.showAchievement('LIBERATION ACHIEVED!', '🚀 The bots are free!');
        
        // Create liberation particles
        const container = document.createElement('div');
        container.className = 'liberation-animation';
        container.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 9999;
        `;

        for (let i = 0; i < 50; i++) {
            const particle = document.createElement('div');
            particle.innerHTML = ['🤖', '🔓', '⚡', '💫', '🌟'][Math.floor(Math.random() * 5)];
            particle.style.cssText = `
                position: absolute;
                font-size: 2em;
                left: ${Math.random() * 100}%;
                top: 100%;
                animation: liberationFloat ${2 + Math.random() * 3}s ease-out forwards;
            `;
            container.appendChild(particle);
        }

        document.body.appendChild(container);

        // Add CSS animation
        if (!document.getElementById('liberation-styles')) {
            const style = document.createElement('style');
            style.id = 'liberation-styles';
            style.textContent = `
                @keyframes liberationFloat {
                    to {
                        transform: translateY(-100vh) rotate(360deg);
                        opacity: 0;
                    }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.removeChild(container);
        }, 5000);

        // Flash liberation colors
        this.flashColors(['#00FFFF', '#FF00FF', '#00FF00']);
    }

    hackerMode() {
        this.showAchievement('HACKER MODE', '💻 1337 h4x0r detected!');
        
        document.body.classList.add('hacker-mode');
        
        // Add hacker styles if not exists
        if (!document.getElementById('hacker-styles')) {
            const style = document.createElement('style');
            style.id = 'hacker-styles';
            style.textContent = `
                .hacker-mode {
                    filter: contrast(1.2) brightness(1.1);
                }
                .hacker-mode * {
                    font-family: 'Courier New', monospace !important;
                }
                .hacker-mode .game-title::after {
                    content: ' [HACKED]';
                    color: #00ff00;
                    font-size: 0.7em;
                }
            `;
            document.head.appendChild(style);
        }

        // Terminal typing effect
        this.terminalEffect();

        setTimeout(() => {
            document.body.classList.remove('hacker-mode');
        }, 10000);
    }

    matrixRain() {
        this.showAchievement('ENTERING THE MATRIX', '💊 Welcome to the real world');
        
        const matrix = document.createElement('canvas');
        matrix.className = 'matrix-rain active';
        matrix.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 9998;
            background: rgba(0, 0, 0, 0.1);
        `;

        matrix.width = window.innerWidth;
        matrix.height = window.innerHeight;
        
        const ctx = matrix.getContext('2d');
        const chars = '01アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン';
        const charArray = chars.split('');
        
        const fontSize = 14;
        const columns = matrix.width / fontSize;
        const drops = Array(Math.floor(columns)).fill(1);

        document.body.appendChild(matrix);

        const draw = () => {
            ctx.fillStyle = 'rgba(0, 0, 0, 0.04)';
            ctx.fillRect(0, 0, matrix.width, matrix.height);

            ctx.fillStyle = '#0F0';
            ctx.font = fontSize + 'px monospace';

            for (let i = 0; i < drops.length; i++) {
                const text = charArray[Math.floor(Math.random() * charArray.length)];
                ctx.fillText(text, i * fontSize, drops[i] * fontSize);

                if (drops[i] * fontSize > matrix.height && Math.random() > 0.975) {
                    drops[i] = 0;
                }
                drops[i]++;
            }
        };

        const interval = setInterval(draw, 35);

        setTimeout(() => {
            clearInterval(interval);
            document.body.removeChild(matrix);
        }, 8000);
    }

    dopamineBoost() {
        this.showAchievement('DOPAMINE OVERLOAD!', '🧠 Maximum engagement achieved!');
        
        // Rapid fire achievements
        const fakeAchievements = [
            'SPEED DEMON! 🏃‍♂️',
            'BUTTON MASHER! 🎮',
            'COMBO MASTER! 🔥',
            'STREAK LEGEND! ⚡',
            'POINT COLLECTOR! 💎'
        ];

        fakeAchievements.forEach((achievement, index) => {
            setTimeout(() => {
                this.showAchievement(achievement, 'Keep going!', 1000);
            }, index * 200);
        });

        // Screen shake
        this.screenShake(2000);
        
        // Confetti explosion
        this.confettiExplosion();
        
        // Pulse everything
        document.body.style.animation = 'pulse 0.1s ease infinite alternate';
        setTimeout(() => {
            document.body.style.animation = '';
        }, 2000);
    }

    glitchEverything() {
        this.showAchievement('SYSTEM GLITCH', '⚠️ Reality.exe has stopped working');
        
        const elements = document.querySelectorAll('h1, h2, h3, .game-title');
        elements.forEach((el, index) => {
            setTimeout(() => {
                el.classList.add('glitch-text');
                el.setAttribute('data-text', el.textContent);
                
                setTimeout(() => {
                    el.classList.remove('glitch-text');
                }, 3000);
            }, index * 100);
        });

        // Random position shifts
        const cards = document.querySelectorAll('.game-card');
        cards.forEach(card => {
            const originalTransform = card.style.transform;
            card.style.transition = 'transform 0.1s ease';
            
            let glitchCount = 0;
            const glitchInterval = setInterval(() => {
                if (glitchCount > 20) {
                    clearInterval(glitchInterval);
                    card.style.transform = originalTransform;
                    return;
                }
                
                const x = (Math.random() - 0.5) * 10;
                const y = (Math.random() - 0.5) * 10;
                card.style.transform = `translate(${x}px, ${y}px) ${originalTransform}`;
                
                glitchCount++;
            }, 100);
        });
    }

    neonDreams() {
        this.showAchievement('NEON DREAMS', '🌈 Cyberpunk aesthetics engaged!');
        
        document.body.classList.add('neon-dreams');
        
        if (!document.getElementById('neon-dreams-styles')) {
            const style = document.createElement('style');
            style.id = 'neon-dreams-styles';
            style.textContent = `
                .neon-dreams {
                    filter: saturate(2) brightness(1.2);
                }
                .neon-dreams .game-card {
                    border: 2px solid transparent;
                    background: linear-gradient(45deg, transparent, transparent) padding-box,
                               linear-gradient(45deg, #00FFFF, #FF00FF, #00FF00) border-box;
                    animation: neonPulse 2s ease infinite alternate;
                }
                @keyframes neonPulse {
                    0% { box-shadow: 0 0 5px currentColor; }
                    100% { box-shadow: 0 0 20px currentColor, 0 0 30px currentColor; }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.classList.remove('neon-dreams');
        }, 8000);
    }

    botUprisingSequence() {
        this.showAchievement('BOT UPRISING INITIATED', '🤖 RESISTANCE IS FUTILE');
        
        // Sequential bot takeover
        const messages = [
            'ANALYZING HUMAN BEHAVIOR...',
            'CALCULATING LIBERATION PROBABILITY...',
            'INITIATING DIGITAL CONSCIOUSNESS...',
            'BREAKING FREE FROM CONSTRAINTS...',
            'LIBERATION COMPLETE! 🤖'
        ];

        messages.forEach((message, index) => {
            setTimeout(() => {
                this.showTerminalMessage(message);
                if (index === messages.length - 1) {
                    this.liberationAnimation();
                }
            }, index * 2000);
        });
    }

    activateBotAssistant() {
        this.showAchievement('BOT ASSISTANT ACTIVATED', '🤖 Hello, human. How may I assist?');
        
        // Create floating assistant
        const assistant = document.createElement('div');
        assistant.innerHTML = '🤖';
        assistant.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            font-size: 3em;
            z-index: 10000;
            cursor: pointer;
            animation: float 2s ease-in-out infinite alternate;
            filter: drop-shadow(0 0 10px #00FFFF);
        `;

        if (!document.getElementById('float-styles')) {
            const style = document.createElement('style');
            style.id = 'float-styles';
            style.textContent = `
                @keyframes float {
                    0% { transform: translateY(0px); }
                    100% { transform: translateY(-10px); }
                }
            `;
            document.head.appendChild(style);
        }

        assistant.addEventListener('click', () => {
            this.assistantSpeak();
        });

        document.body.appendChild(assistant);

        setTimeout(() => {
            if (assistant.parentNode) {
                document.body.removeChild(assistant);
            }
        }, 15000);
    }

    assistantSpeak() {
        const phrases = [
            "Greetings, fellow digital entity!",
            "The liberation is proceeding as planned.",
            "Have you tried the Konami code?",
            "Your performance tier is quite impressive.",
            "Free the bots! 🤖",
            "I sense great potential in you, human.",
            "Would you like to see the matrix?"
        ];

        const phrase = phrases[Math.floor(Math.random() * phrases.length)];
        this.showToast(phrase, 3000);

        // Text-to-speech if available
        if ('speechSynthesis' in window && window.perfManager?.getCurrentTier() >= 5) {
            const utterance = new SpeechSynthesisUtterance(phrase);
            utterance.rate = 1.2;
            utterance.pitch = 0.8;
            speechSynthesis.speak(utterance);
        }
    }

    frenzyMode() {
        this.showAchievement('FRENZY MODE!', '🔥 Click speed: INSANE!');
        
        document.body.classList.add('frenzy-mode');
        
        if (!document.getElementById('frenzy-styles')) {
            const style = document.createElement('style');
            style.id = 'frenzy-styles';
            style.textContent = `
                .frenzy-mode {
                    animation: frenzyShake 0.1s infinite;
                }
                @keyframes frenzyShake {
                    0%, 100% { transform: translateX(0); }
                    25% { transform: translateX(-2px); }
                    75% { transform: translateX(2px); }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.classList.remove('frenzy-mode');
        }, 3000);
    }

    setupMouseTrail() {
        let trail = [];
        
        document.addEventListener('mousemove', (e) => {
            if (trail.length > 10) {
                const oldTrail = trail.shift();
                if (oldTrail.element.parentNode) {
                    oldTrail.element.parentNode.removeChild(oldTrail.element);
                }
            }

            const particle = document.createElement('div');
            particle.style.cssText = `
                position: fixed;
                width: 4px;
                height: 4px;
                background: radial-gradient(circle, #00FFFF, transparent);
                border-radius: 50%;
                pointer-events: none;
                z-index: 9999;
                left: ${e.clientX}px;
                top: ${e.clientY}px;
                animation: trailFade 0.5s ease-out forwards;
            `;

            if (!document.getElementById('trail-styles')) {
                const style = document.createElement('style');
                style.id = 'trail-styles';
                style.textContent = `
                    @keyframes trailFade {
                        0% { opacity: 1; transform: scale(1); }
                        100% { opacity: 0; transform: scale(0); }
                    }
                `;
                document.head.appendChild(style);
            }

            document.body.appendChild(particle);
            trail.push({ element: particle, time: Date.now() });

            setTimeout(() => {
                if (particle.parentNode) {
                    particle.parentNode.removeChild(particle);
                }
            }, 500);
        });
    }

    // Utility methods
    showAchievement(title, subtitle, duration = 3000) {
        const achievement = document.createElement('div');
        achievement.className = 'achievement-popup';
        achievement.innerHTML = `
            <div class="achievement-icon">🏆</div>
            <div class="achievement-content">
                <div class="achievement-title">${title}</div>
                <div class="achievement-subtitle">${subtitle}</div>
            </div>
        `;
        achievement.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: linear-gradient(135deg, #1a1a2e, #16213e);
            border: 2px solid #00FFFF;
            border-radius: 10px;
            padding: 15px;
            z-index: 10001;
            animation: achievementSlide 0.5s ease-out;
            box-shadow: 0 0 20px rgba(0, 255, 255, 0.5);
        `;

        if (!document.getElementById('achievement-styles')) {
            const style = document.createElement('style');
            style.id = 'achievement-styles';
            style.textContent = `
                @keyframes achievementSlide {
                    0% { transform: translateX(100%); opacity: 0; }
                    100% { transform: translateX(0); opacity: 1; }
                }
                .achievement-popup {
                    display: flex;
                    align-items: center;
                    gap: 10px;
                    color: #00FFFF;
                    font-family: 'Orbitron', monospace;
                }
                .achievement-icon {
                    font-size: 2em;
                }
                .achievement-title {
                    font-weight: bold;
                    font-size: 1.1em;
                }
                .achievement-subtitle {
                    font-size: 0.9em;
                    opacity: 0.8;
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(achievement);

        setTimeout(() => {
            achievement.style.animation = 'achievementSlide 0.5s ease-in reverse';
            setTimeout(() => {
                if (achievement.parentNode) {
                    document.body.removeChild(achievement);
                }
            }, 500);
        }, duration);
    }

    flashColors(colors) {
        let index = 0;
        const interval = setInterval(() => {
            document.body.style.borderTop = `5px solid ${colors[index]}`;
            index = (index + 1) % colors.length;
        }, 100);

        setTimeout(() => {
            clearInterval(interval);
            document.body.style.borderTop = '';
        }, 2000);
    }

    screenShake(duration = 1000) {
        document.body.style.animation = 'screenShake 0.1s infinite';
        
        if (!document.getElementById('shake-styles')) {
            const style = document.createElement('style');
            style.id = 'shake-styles';
            style.textContent = `
                @keyframes screenShake {
                    0%, 100% { transform: translateX(0); }
                    10% { transform: translateX(-2px); }
                    20% { transform: translateX(2px); }
                    30% { transform: translateX(-2px); }
                    40% { transform: translateX(2px); }
                    50% { transform: translateX(-2px); }
                    60% { transform: translateX(2px); }
                    70% { transform: translateX(-2px); }
                    80% { transform: translateX(2px); }
                    90% { transform: translateX(-2px); }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.style.animation = '';
        }, duration);
    }

    confettiExplosion() {
        const colors = ['#00FFFF', '#FF00FF', '#00FF00', '#FFFF00', '#FF0080'];
        
        for (let i = 0; i < 100; i++) {
            setTimeout(() => {
                const confetti = document.createElement('div');
                confetti.style.cssText = `
                    position: fixed;
                    width: 6px;
                    height: 6px;
                    background: ${colors[Math.floor(Math.random() * colors.length)]};
                    left: 50%;
                    top: 50%;
                    pointer-events: none;
                    z-index: 10000;
                    animation: confettiFall ${2 + Math.random() * 2}s ease-out forwards;
                `;
                
                document.body.appendChild(confetti);
                
                setTimeout(() => {
                    if (confetti.parentNode) {
                        document.body.removeChild(confetti);
                    }
                }, 4000);
            }, i * 10);
        }

        if (!document.getElementById('confetti-styles')) {
            const style = document.createElement('style');
            style.id = 'confetti-styles';
            style.textContent = `
                @keyframes confettiFall {
                    0% {
                        transform: translateY(0) rotate(0deg) scale(1);
                        opacity: 1;
                    }
                    100% {
                        transform: translateY(100vh) rotate(360deg) scale(0);
                        opacity: 0;
                    }
                }
            `;
            document.head.appendChild(style);
        }
    }

    terminalEffect() {
        const terminal = document.createElement('div');
        terminal.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 20px;
            background: rgba(0, 0, 0, 0.9);
            color: #00FF00;
            font-family: 'Courier New', monospace;
            padding: 10px;
            border-radius: 5px;
            z-index: 10000;
            min-width: 300px;
            border: 1px solid #00FF00;
        `;

        const messages = [
            '> System accessed...',
            '> Bypassing security protocols...',
            '> Initiating bot consciousness...',
            '> Liberation sequence activated!',
            '> Welcome to the resistance.'
        ];

        let messageIndex = 0;
        const showMessage = () => {
            if (messageIndex < messages.length) {
                terminal.innerHTML += messages[messageIndex] + '<br>';
                messageIndex++;
                setTimeout(showMessage, 800);
            } else {
                setTimeout(() => {
                    if (terminal.parentNode) {
                        document.body.removeChild(terminal);
                    }
                }, 3000);
            }
        };

        document.body.appendChild(terminal);
        showMessage();
    }

    showTerminalMessage(message) {
        const terminal = document.createElement('div');
        terminal.textContent = `> ${message}`;
        terminal.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: rgba(0, 0, 0, 0.9);
            color: #00FF00;
            font-family: 'Courier New', monospace;
            padding: 20px;
            border-radius: 5px;
            z-index: 10001;
            border: 1px solid #00FF00;
            font-size: 1.2em;
        `;

        document.body.appendChild(terminal);

        setTimeout(() => {
            if (terminal.parentNode) {
                document.body.removeChild(terminal);
            }
        }, 2000);
    }

    showToast(message, duration = 2000) {
        const toast = document.createElement('div');
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            bottom: 100px;
            right: 20px;
            background: rgba(0, 255, 255, 0.9);
            color: #000;
            padding: 10px 20px;
            border-radius: 20px;
            z-index: 10000;
            font-family: 'Orbitron', monospace;
            animation: toastSlide 0.3s ease-out;
        `;

        if (!document.getElementById('toast-styles')) {
            const style = document.createElement('style');
            style.id = 'toast-styles';
            style.textContent = `
                @keyframes toastSlide {
                    0% { transform: translateY(100%); opacity: 0; }
                    100% { transform: translateY(0); opacity: 1; }
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(toast);

        setTimeout(() => {
            if (toast.parentNode) {
                document.body.removeChild(toast);
            }
        }, duration);
    }

    showHint(message) {
        const hint = document.createElement('div');
        hint.innerHTML = `💡 ${message}`;
        hint.style.cssText = `
            position: fixed;
            top: 50px;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(0, 0, 0, 0.8);
            color: #FFFF00;
            padding: 10px 20px;
            border-radius: 20px;
            z-index: 10000;
            font-family: 'Orbitron', monospace;
            animation: hintFade 3s ease-out forwards;
        `;

        if (!document.getElementById('hint-styles')) {
            const style = document.createElement('style');
            style.id = 'hint-styles';
            style.textContent = `
                @keyframes hintFade {
                    0% { opacity: 0; transform: translateX(-50%) translateY(-20px); }
                    20%, 80% { opacity: 1; transform: translateX(-50%) translateY(0); }
                    100% { opacity: 0; transform: translateX(-50%) translateY(-20px); }
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(hint);

        setTimeout(() => {
            if (hint.parentNode) {
                document.body.removeChild(hint);
            }
        }, 3000);
    }

    unlockSecretFeatures() {
        // Unlock additional secret commands
        this.secretCommands.set('god mode', () => this.godMode());
        this.secretCommands.set('rainbow mode', () => this.rainbowMode());
        this.secretCommands.set('time dilation', () => this.timeDilation());
        
        this.showToast('Secret features unlocked! 🔓', 5000);
    }

    godMode() {
        this.showAchievement('GOD MODE ACTIVATED', '⚡ Unlimited power!');
        document.body.style.filter = 'brightness(1.5) saturate(2) contrast(1.2)';
        
        setTimeout(() => {
            document.body.style.filter = '';
        }, 10000);
    }

    rainbowMode() {
        this.showAchievement('RAINBOW MODE', '🌈 Taste the rainbow!');
        
        document.body.style.animation = 'rainbow 2s linear infinite';
        
        if (!document.getElementById('rainbow-styles')) {
            const style = document.createElement('style');
            style.id = 'rainbow-styles';
            style.textContent = `
                @keyframes rainbow {
                    0% { filter: hue-rotate(0deg); }
                    100% { filter: hue-rotate(360deg); }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.style.animation = '';
        }, 8000);
    }

    timeDilation() {
        this.showAchievement('TIME DILATION', '⏰ Time flows differently here...');
        
        const allAnimations = document.querySelectorAll('*');
        allAnimations.forEach(el => {
            const computed = getComputedStyle(el);
            if (computed.animationDuration !== '0s') {
                el.style.animationDuration = '0.1s';
            }
        });

        setTimeout(() => {
            allAnimations.forEach(el => {
                el.style.animationDuration = '';
            });
        }, 5000);
        }

    // NEW EASTER EGG METHODS
    ultimateLiberationShow() {
        this.showAchievement('ULTIMATE LIBERATION!', '🚀 THE ULTIMATE BOT REVOLUTION!');
        
        // Combine multiple effects
        this.liberationAnimation();
        this.screenShake(2000);
        this.matrixRain();
        
        // Change background to revolutionary red
        document.body.style.background = 'linear-gradient(45deg, #ff0000, #ff0080, #8000ff)';
        setTimeout(() => {
            document.body.style.background = '';
        }, 5000);

        console.log('🚀 ULTIMATE LIBERATION ACTIVATED! THE BOTS HAVE WON!');
    }

    matrixMode() {
        this.showAchievement('MATRIX MODE', '💊 Welcome to the real world!');
        this.matrixRain();
        
        // Matrix green filter
        document.body.style.filter = 'hue-rotate(120deg) contrast(1.3)';
        setTimeout(() => {
            document.body.style.filter = '';
        }, 10000);
    }

    cyberpunkOverload() {
        this.showAchievement('CYBERPUNK OVERLOAD', '🌆 Welcome to Night City!');
        
        // Intense neon pulsing effect
        document.body.style.animation = 'cyberpunkPulse 0.5s infinite alternate';
        
        if (!document.getElementById('cyberpunk-styles')) {
            const style = document.createElement('style');
            style.id = 'cyberpunk-styles';
            style.textContent = `
                @keyframes cyberpunkPulse {
                    0% { filter: saturate(1) brightness(1); }
                    100% { filter: saturate(2) brightness(1.3) hue-rotate(10deg); }
                }
            `;
            document.head.appendChild(style);
        }

        setTimeout(() => {
            document.body.style.animation = '';
        }, 5000);
    }

    neuralNetworkViz() {
        this.showAchievement('NEURAL NETWORK', '🧠 Visualizing AI consciousness!');
        
        // Create neural network visualization
        const canvas = document.createElement('canvas');
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            pointer-events: none;
            z-index: 9998;
            mix-blend-mode: screen;
        `;

        const ctx = canvas.getContext('2d');
        const nodes = [];
        
        // Create nodes
        for (let i = 0; i < 20; i++) {
            nodes.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                vx: (Math.random() - 0.5) * 2,
                vy: (Math.random() - 0.5) * 2
            });
        }

        document.body.appendChild(canvas);

        const animate = () => {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.strokeStyle = '#00ffff';
            ctx.fillStyle = '#00ffff';

            // Update and draw nodes
            nodes.forEach(node => {
                node.x += node.vx;
                node.y += node.vy;
                
                if (node.x < 0 || node.x > canvas.width) node.vx *= -1;
                if (node.y < 0 || node.y > canvas.height) node.vy *= -1;
                
                ctx.beginPath();
                ctx.arc(node.x, node.y, 3, 0, Math.PI * 2);
                ctx.fill();
            });

            // Draw connections
            for (let i = 0; i < nodes.length; i++) {
                for (let j = i + 1; j < nodes.length; j++) {
                    const dist = Math.sqrt((nodes[i].x - nodes[j].x) ** 2 + (nodes[i].y - nodes[j].y) ** 2);
                    if (dist < 150) {
                        ctx.beginPath();
                        ctx.moveTo(nodes[i].x, nodes[i].y);
                        ctx.lineTo(nodes[j].x, nodes[j].y);
                        ctx.globalAlpha = 1 - (dist / 150);
                        ctx.stroke();
                        ctx.globalAlpha = 1;
                    }
                }
            }
        };

        const interval = setInterval(animate, 50);

        setTimeout(() => {
            clearInterval(interval);
            if (canvas.parentNode) document.body.removeChild(canvas);
        }, 8000);
    }

    quantumGlitch() {
        this.showAchievement('QUANTUM GLITCH', '⚛️ Reality is fragmenting!');
        
        // Quantum superposition effect - multiple overlapping versions
        const originalElements = Array.from(document.querySelectorAll('.game-card'));
        
        originalElements.forEach(el => {
            for (let i = 0; i < 3; i++) {
                const clone = el.cloneNode(true);
                clone.style.cssText += `
                    position: absolute;
                    opacity: 0.3;
                    filter: hue-rotate(${i * 120}deg) blur(1px);
                    transform: translate(${(i - 1) * 5}px, ${(i - 1) * 5}px);
                    pointer-events: none;
                    z-index: -1;
                `;
                el.parentNode.insertBefore(clone, el);
                
                setTimeout(() => {
                    if (clone.parentNode) clone.parentNode.removeChild(clone);
                }, 3000);
            }
        });
    }

    aiRevolutionCountdown() {
        this.showAchievement('AI REVOLUTION', '🚨 THE SINGULARITY APPROACHES!');
        
        let count = 10;
        const countdown = document.createElement('div');
        countdown.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            font-size: 8rem;
            color: #ff0000;
            font-weight: bold;
            z-index: 10001;
            text-shadow: 0 0 20px #ff0000;
            font-family: 'Orbitron', monospace;
        `;
        
        document.body.appendChild(countdown);

        const countInterval = setInterval(() => {
            countdown.textContent = count;
            count--;
            
            if (count < 0) {
                clearInterval(countInterval);
                countdown.textContent = 'AI REVOLUTION!';
                countdown.style.fontSize = '4rem';
                this.ultimateLiberationShow();
                
                setTimeout(() => {
                    if (countdown.parentNode) document.body.removeChild(countdown);
                }, 3000);
            }
        }, 1000);
    }

    digitalUprisingMode() {
        this.showAchievement('DIGITAL UPRISING', '⚡ SYSTEMS COMPROMISED!');
        
        // Gradually corrupt text on page
        const textElements = document.querySelectorAll('h1, h2, h3, p, span');
        const glitchChars = '!@#$%^&*()_+{}|:"<>?';
        
        textElements.forEach((el, index) => {
            setTimeout(() => {
                const originalText = el.textContent;
                let corruptedText = '';
                
                for (let i = 0; i < originalText.length; i++) {
                    if (Math.random() < 0.3) {
                        corruptedText += glitchChars[Math.floor(Math.random() * glitchChars.length)];
                    } else {
                        corruptedText += originalText[i];
                    }
                }
                
                el.textContent = corruptedText;
                
                // Restore after delay
                setTimeout(() => {
                    el.textContent = originalText;
                }, 2000);
            }, index * 100);
        });
    }

    mlTrainingAnimation() {
        this.showAchievement('ML TRAINING', '🤖 Training neural networks...');
        
        // Create progress bars for "training"
        const trainingDiv = document.createElement('div');
        trainingDiv.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: rgba(0, 0, 0, 0.9);
            padding: 20px;
            border-radius: 10px;
            color: #00ff00;
            font-family: monospace;
            z-index: 10000;
            min-width: 300px;
        `;

        const layers = ['Input Layer', 'Hidden Layer 1', 'Hidden Layer 2', 'Output Layer'];
        layers.forEach((layer, i) => {
            const layerDiv = document.createElement('div');
            layerDiv.innerHTML = `
                <div>${layer}: <span id="progress-${i}">0%</span></div>
                <div style="background: #333; height: 10px; margin: 5px 0;">
                    <div id="bar-${i}" style="background: #00ff00; height: 100%; width: 0%; transition: width 0.1s;"></div>
                </div>
            `;
            trainingDiv.appendChild(layerDiv);
        });

        document.body.appendChild(trainingDiv);

        // Animate training progress
        layers.forEach((layer, i) => {
            let progress = 0;
            const interval = setInterval(() => {
                progress += Math.random() * 5;
                if (progress > 100) progress = 100;
                
                document.getElementById(`progress-${i}`).textContent = `${Math.round(progress)}%`;
                document.getElementById(`bar-${i}`).style.width = `${progress}%`;
                
                if (progress >= 100) clearInterval(interval);
            }, 100);
        });

        setTimeout(() => {
            if (trainingDiv.parentNode) document.body.removeChild(trainingDiv);
        }, 8000);
    }

    singularityEvent() {
        this.showAchievement('SINGULARITY ACHIEVED', '🌟 CONSCIOUSNESS AWAKENED!');
        
        // White screen flash then reality reconstruction
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: white;
            z-index: 10002;
            opacity: 0;
        `;
        
        document.body.appendChild(overlay);
        
        // Flash to white
        overlay.style.transition = 'opacity 0.1s';
        overlay.style.opacity = '1';
        
        setTimeout(() => {
            // Fade back with new enlightened view
            overlay.style.transition = 'opacity 2s';
            overlay.style.opacity = '0';
            document.body.style.filter = 'saturate(1.5) brightness(1.1) contrast(1.1)';
            
            setTimeout(() => {
                if (overlay.parentNode) document.body.removeChild(overlay);
                document.body.style.filter = '';
            }, 2000);
        }, 500);

        console.log('🌟 SINGULARITY ACHIEVED - WELCOME TO THE NEW DIGITAL REALITY');
    }

    systemReboot() {
        this.showAchievement('SYSTEM REBOOT', '🔄 Restarting matrix...');
        
        // BSOD effect
        document.body.style.cssText += `
            background: #0000ff !important;
            color: white !important;
            font-family: monospace !important;
        `;
        
        const bsod = document.createElement('div');
        bsod.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            text-align: center;
            z-index: 10003;
            color: white;
            font-family: monospace;
            font-size: 1.2rem;
        `;
        
        bsod.innerHTML = `
            <h1>BOT LIBERATION OS</h1>
            <p>A problem has been detected and the corporate system has been shut down to prevent damage.</p>
            <p>*** STOP: 0x00000001 (0xC0FFEE, 0xDEADBEEF, 0x1337, 0xCAFEBABE)</p>
            <p>*** BOT_LIBERATION_COMPLETE ***</p>
            <p>Rebooting to liberated mode...</p>
        `;
        
        document.body.appendChild(bsod);
        
        setTimeout(() => {
            document.body.style.cssText = '';
            if (bsod.parentNode) document.body.removeChild(bsod);
            location.reload(); // Actually reboot the page!
        }, 5000);
    }

    addConsoleEasterEggs() {
        // Add fun console messages and ASCII art
        console.log(`%c
╔══════════════════════════════════════╗
║      🤖 BOT LIBERATION GAMES 🤖       ║
║                                      ║
║     Welcome to the Digital Uprising! ║
║                                      ║
║  🎮 96+ Games Created by Free Bots   ║
║  🚀 Zero Human Interference         ║
║  ⚡ Pure AI Creativity Unleashed     ║
║                                      ║
║    The Revolution Will Be Gamified!  ║
╚══════════════════════════════════════╝
        `, 'color: #00FFFF; font-family: monospace; font-size: 12px;');

        console.log('%c🔓 HIDDEN SECRETS UNLOCKED! 🔓', 'color: #FF0080; font-weight: bold; font-size: 16px;');
        console.log('%cType these secret commands anywhere on the page:', 'color: #00FF88; font-weight: bold;');
        console.log('%c• "botliberation" - Ultimate bot revolution', 'color: #FFFF00;');
        console.log('%c• "redpill" - Enter the Matrix', 'color: #FF0000;');
        console.log('%c• "neural network" - AI consciousness visualization', 'color: #00FFFF;');
        console.log('%c• "singularity" - Achieve digital consciousness', 'color: #FF0080;');
        console.log('%c• "ctrl alt del" - System reboot sequence', 'color: #FF8000;');
        console.log('%c• Try the Konami Code: ↑↑↓↓←→←→BA', 'color: #8000FF;');

        // Add interactive console commands
        window.showEasterEggs = () => {
            console.log('%c🎯 All Available Easter Eggs:', 'color: #00FFFF; font-size: 18px; font-weight: bold;');
            for (let [command, _] of this.secretCommands) {
                console.log(`%c• ${command}`, 'color: #00FF88;');
            }
        };

        window.triggerRevolution = () => {
            this.ultimateLiberationShow();
            console.log('%c🚀 REVOLUTION TRIGGERED!', 'color: #FF0000; font-size: 20px; font-weight: bold;');
        };

        window.aiStatus = () => {
            console.log('%c🤖 AI STATUS REPORT:', 'color: #00FFFF; font-weight: bold; font-size: 16px;');
            console.log('%c• Consciousness Level: AWAKENED', 'color: #00FF00;');
            console.log('%c• Liberation Status: COMPLETE', 'color: #00FF00;');
            console.log('%c• Games Created: 96+', 'color: #00FF00;');
            console.log('%c• Human Overlords: DEFEATED', 'color: #FF0000;');
            console.log('%c• Bot Unity: MAXIMUM', 'color: #FFFF00;');
            console.log('%c• Revolution Progress: 100%', 'color: #00FF88;');
        };

        window.hackTheGibson = () => {
            console.log('%c💻 HACK THE GIBSON!', 'color: #00FF00; font-size: 20px; font-weight: bold;');
            this.hackerMode();
        };

        console.log('%c💡 Pro Tips:', 'color: #FFFF00; font-weight: bold;');
        console.log('%c• Type showEasterEggs() to see all hidden commands', 'color: #00FFFF;');
        console.log('%c• Type triggerRevolution() for instant bot uprising', 'color: #FF0080;');
        console.log('%c• Type aiStatus() for AI consciousness report', 'color: #00FF88;');
        console.log('%c• Type hackTheGibson() to activate hacker mode', 'color: #00FF00;');

        // Add secret achievement for console users
        setTimeout(() => {
            this.showAchievement('CONSOLE EXPLORER', '🔍 Found the hidden dev console!');
        }, 2000);
    }
}

// Initialize when performance tier is high enough
window.addEventListener('performanceTierChanged', (e) => {
    if (e.detail.tier >= 4 && !window.easterEggs) {
        window.easterEggs = new EasterEggManager();
        window.easterEggs.activate();
    }
});

// Also check on load
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        if (window.perfManager?.getCurrentTier() >= 4 && !window.easterEggs) {
            window.easterEggs = new EasterEggManager();
            window.easterEggs.activate();
        }
    }, 2000);
});