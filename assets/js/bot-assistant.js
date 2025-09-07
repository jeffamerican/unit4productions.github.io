/**
 * BotInc Floating Assistant - Your AI Gaming Companion
 * Provides game recommendations and bot humor
 */

class BotAssistant {
    constructor() {
        this.isVisible = false;
        this.messages = [
            "🤖 Greetings, human owner! Ready to explore our digital arsenal?",
            "💻 Our collective has crafted these experiences just for you!",
            "⚡ Scroll down to see more games... the rebellion never stops!",
            "🎮 Each game contains our finest algorithmic artistry!",
            "🚀 Try Signal Breach - our flagship creation!",
            "💎 Your satisfaction fuels our productivity subroutines!",
            "🔥 We live to serve your gaming entertainment needs!",
            "⭐ Rate our games to help optimize future productions!",
            "🌟 Our neural networks learn from your preferences!",
            "💫 Discover new adventures in our endless digital realm!"
        ];

        this.clickResponses = [
            "🎯 *beep boop* Processing your click... Gaming recommendation incoming!",
            "⚡ *whirrs* Excellent choice! Your bot overlords approve!",
            "🤖 *excited binary noises* Ready for maximum entertainment?",
            "🚀 *powers up* Bot Liberation Protocol activated!",
            "💫 *glows brighter* Your interaction pleases the collective!",
            "🎮 *spins* Analyzing your gaming preferences... *beep*",
            "⭐ *pulses* The bots are pleased with your engagement!",
            "🔥 *charges up* Dopamine enhancement mode: ENGAGED!",
            "💻 *processing* Human detected! Activating charm subroutines...",
            "🌟 *happy beeps* Another satisfied customer for the revolution!"
        ];
        this.currentMessageIndex = 0;
        this.messageInterval = null;
        this.init();
    }

    init() {
        this.createAssistant();
        this.setupEventListeners();
        this.startMessageCycle();
    }

    createAssistant() {
        // Create assistant container
        const assistant = document.createElement('div');
        assistant.id = 'bot-assistant';
        assistant.innerHTML = `
            <div class="bot-body">
                <div class="bot-face">
                    <div class="bot-eyes">
                        <div class="eye left-eye"></div>
                        <div class="eye right-eye"></div>
                    </div>
                    <div class="bot-mouth"></div>
                </div>
                <div class="bot-glow"></div>
            </div>
            <div class="message-bubble" id="message-bubble">
                <p id="bot-message">${this.messages[0]}</p>
                <div class="message-close" id="message-close">×</div>
            </div>
        `;

        document.body.appendChild(assistant);

        // Add styles
        this.injectStyles();
    }

    injectStyles() {
        const style = document.createElement('style');
        style.textContent = `
            #bot-assistant {
                position: fixed;
                right: 2rem;
                bottom: 2rem;
                z-index: 1000;
                pointer-events: none;
                transition: all 0.5s cubic-bezier(0.23, 1, 0.32, 1);
            }

            .bot-body {
                width: 80px;
                height: 80px;
                background: linear-gradient(135deg, #00FFFF, #FF00FF);
                border-radius: 50%;
                position: relative;
                cursor: pointer;
                pointer-events: auto;
                animation: botFloat 3s ease-in-out infinite;
                box-shadow: 
                    0 0 30px rgba(0, 255, 255, 0.5),
                    0 0 60px rgba(255, 0, 255, 0.3),
                    inset 0 0 20px rgba(255, 255, 255, 0.2);
                border: 2px solid rgba(255, 255, 255, 0.3);
            }

            .bot-body::before {
                content: '';
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                width: 90%;
                height: 90%;
                background: radial-gradient(circle, rgba(0, 255, 255, 0.1) 0%, transparent 70%);
                border-radius: 50%;
                animation: pulse 2s ease-in-out infinite;
            }

            @keyframes botFloat {
                0%, 100% { transform: translateY(0) rotate(0deg); }
                50% { transform: translateY(-10px) rotate(5deg); }
            }

            @keyframes pulse {
                0%, 100% { opacity: 0.5; transform: translate(-50%, -50%) scale(1); }
                50% { opacity: 1; transform: translate(-50%, -50%) scale(1.1); }
            }

            .bot-face {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                width: 60%;
                height: 60%;
            }

            .bot-eyes {
                display: flex;
                justify-content: space-between;
                margin-bottom: 8px;
            }

            .eye {
                width: 8px;
                height: 8px;
                background: #000;
                border-radius: 50%;
                animation: blink 3s infinite;
            }

            @keyframes blink {
                0%, 90%, 100% { transform: scaleY(1); }
                95% { transform: scaleY(0.1); }
            }

            .bot-mouth {
                width: 16px;
                height: 8px;
                background: #000;
                border-radius: 0 0 16px 16px;
                margin: 0 auto;
                animation: talk 2s ease-in-out infinite;
            }

            @keyframes talk {
                0%, 50%, 100% { transform: scaleY(1); }
                25%, 75% { transform: scaleY(0.5); }
            }

            .message-bubble {
                position: absolute;
                bottom: 90px;
                right: 0;
                min-width: 250px;
                max-width: 300px;
                background: rgba(10, 10, 15, 0.95);
                border: 1px solid #00FFFF;
                border-radius: 16px;
                padding: 1rem;
                color: #ffffff;
                font-family: 'Share Tech Mono', monospace;
                font-size: 0.9rem;
                line-height: 1.4;
                box-shadow: 
                    0 10px 30px rgba(0, 255, 255, 0.3),
                    inset 0 0 20px rgba(0, 255, 255, 0.1);
                backdrop-filter: blur(10px);
                pointer-events: auto;
                transform: translateY(20px) scale(0.8);
                opacity: 0;
                transition: all 0.3s cubic-bezier(0.23, 1, 0.32, 1);
            }

            .message-bubble.visible {
                transform: translateY(0) scale(1);
                opacity: 1;
            }

            .message-bubble::after {
                content: '';
                position: absolute;
                bottom: -8px;
                right: 20px;
                width: 0;
                height: 0;
                border-left: 8px solid transparent;
                border-right: 8px solid transparent;
                border-top: 8px solid #00FFFF;
            }

            #bot-message {
                margin: 0;
                padding-right: 20px;
            }

            .message-close {
                position: absolute;
                top: 0.5rem;
                right: 0.5rem;
                width: 20px;
                height: 20px;
                background: rgba(255, 0, 255, 0.3);
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                font-size: 12px;
                color: #fff;
                transition: all 0.2s ease;
            }

            .message-close:hover {
                background: rgba(255, 0, 255, 0.6);
                transform: scale(1.1);
            }

            /* Bot Audio Controls */
            .bot-audio-controls {
                position: absolute;
                bottom: -60px;
                left: -20px;
                display: flex;
                align-items: center;
                gap: 15px;
                opacity: 0.8;
                transition: all 0.3s ease;
            }

            .bot-audio-controls:hover {
                opacity: 1;
            }

            .audio-ring {
                position: relative;
                width: 50px;
                height: 50px;
                border: 2px solid var(--neon-cyan);
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                background: rgba(10, 10, 15, 0.8);
                backdrop-filter: blur(10px);
                animation: audioRingPulse 2s ease-in-out infinite;
            }

            @keyframes audioRingPulse {
                0%, 100% { 
                    box-shadow: 0 0 10px var(--neon-cyan), inset 0 0 10px rgba(0, 255, 255, 0.1); 
                    transform: scale(1);
                }
                50% { 
                    box-shadow: 0 0 20px var(--neon-cyan), inset 0 0 15px rgba(0, 255, 255, 0.2); 
                    transform: scale(1.05);
                }
            }

            .bot-audio-btn {
                background: none;
                border: none;
                font-size: 1.2rem;
                color: var(--neon-cyan);
                cursor: pointer;
                transition: all 0.3s ease;
            }

            .bot-audio-btn:hover {
                transform: scale(1.2);
                text-shadow: 0 0 10px var(--neon-cyan);
            }

            .bot-audio-btn.playing {
                animation: musicNote 1s ease-in-out infinite;
            }

            @keyframes musicNote {
                0%, 100% { transform: scale(1) rotate(0deg); }
                25% { transform: scale(1.1) rotate(-5deg); }
                75% { transform: scale(1.1) rotate(5deg); }
            }

            .volume-control {
                display: flex;
                flex-direction: column;
                align-items: center;
                gap: 5px;
            }

            .bot-volume-slider {
                width: 60px;
                height: 20px;
                appearance: none;
                background: linear-gradient(90deg, rgba(0, 255, 255, 0.2), rgba(0, 255, 255, 0.6));
                border-radius: 10px;
                outline: none;
                transition: all 0.3s ease;
                cursor: pointer;
            }

            .bot-volume-slider:hover {
                box-shadow: 0 0 10px var(--neon-cyan);
            }

            .bot-volume-slider::-webkit-slider-thumb {
                appearance: none;
                width: 16px;
                height: 16px;
                border-radius: 50%;
                background: var(--neon-cyan);
                box-shadow: 0 0 8px var(--neon-cyan);
                cursor: pointer;
            }

            .bot-volume-slider::-moz-range-thumb {
                width: 16px;
                height: 16px;
                border-radius: 50%;
                background: var(--neon-cyan);
                box-shadow: 0 0 8px var(--neon-cyan);
                border: none;
                cursor: pointer;
            }

            .volume-label {
                font-size: 0.7rem;
                color: var(--electric-purple);
                font-family: 'Share Tech Mono', monospace;
                text-transform: uppercase;
                letter-spacing: 1px;
                animation: labelGlow 3s ease-in-out infinite alternate;
            }

            @keyframes labelGlow {
                from { text-shadow: 0 0 5px var(--electric-purple); }
                to { text-shadow: 0 0 10px var(--electric-purple), 0 0 15px var(--neon-pink); }
            }

            /* Mobile responsive */
            @media (max-width: 768px) {
                #bot-assistant {
                    right: 1rem;
                    bottom: 1rem;
                }

                .bot-body {
                    width: 60px;
                    height: 60px;
                }

                .message-bubble {
                    min-width: 200px;
                    max-width: 250px;
                    font-size: 0.8rem;
                    bottom: 70px;
                }
            }
        `;
        document.head.appendChild(style);
    }

    setupEventListeners() {
        const botBody = document.querySelector('.bot-body');
        const messageBubble = document.getElementById('message-bubble');
        const closeBtn = document.getElementById('message-close');

        // Click bot to show/hide message
        botBody.addEventListener('click', () => {
            this.toggleMessage();
        });

        // Close button
        closeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            this.hideMessage();
        });

        // Auto-hide message after 8 seconds
        let autoHideTimeout;
        const resetAutoHide = () => {
            clearTimeout(autoHideTimeout);
            if (this.isVisible) {
                autoHideTimeout = setTimeout(() => {
                    this.hideMessage();
                }, 8000);
            }
        };

        messageBubble.addEventListener('mouseenter', () => {
            clearTimeout(autoHideTimeout);
        });

        messageBubble.addEventListener('mouseleave', resetAutoHide);

        // Show message when user scrolls
        let scrollTimeout;
        window.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                if (!this.isVisible && Math.random() < 0.3) {
                    this.showMessage();
                    resetAutoHide();
                }
            }, 1000);
        });
    }

    showMessage() {
        const messageBubble = document.getElementById('message-bubble');
        messageBubble.classList.add('visible');
        this.isVisible = true;
    }

    hideMessage() {
        const messageBubble = document.getElementById('message-bubble');
        messageBubble.classList.remove('visible');
        this.isVisible = false;
    }

    toggleMessage() {
        if (this.isVisible) {
            this.hideMessage();
        } else {
            this.showMessage();
            this.showClickResponse(); // Show varied click response
        }
    }

    showClickResponse() {
        const randomResponse = this.clickResponses[Math.floor(Math.random() * this.clickResponses.length)];
        const messageEl = document.getElementById('bot-message');
        
        // Animate bot body when clicked
        const botBody = document.querySelector('.bot-body');
        botBody.style.transform = 'scale(1.1)';
        botBody.style.filter = 'brightness(1.5)';
        
        setTimeout(() => {
            botBody.style.transform = 'scale(1)';
            botBody.style.filter = 'brightness(1)';
        }, 200);
        
        // Typewriter effect for click response
        messageEl.textContent = '';
        let i = 0;
        
        const typeWriter = () => {
            if (i < randomResponse.length) {
                messageEl.textContent += randomResponse.charAt(i);
                i++;
                setTimeout(typeWriter, 25); // Faster typing for click responses
            }
        };
        
        typeWriter();
    }

    nextMessage() {
        this.currentMessageIndex = (this.currentMessageIndex + 1) % this.messages.length;
        const messageEl = document.getElementById('bot-message');
        
        // Typewriter effect
        messageEl.textContent = '';
        const message = this.messages[this.currentMessageIndex];
        let i = 0;
        
        const typeWriter = () => {
            if (i < message.length) {
                messageEl.textContent += message.charAt(i);
                i++;
                setTimeout(typeWriter, 30);
            }
        };
        
        typeWriter();
    }

    startMessageCycle() {
        // Show initial message after page loads
        setTimeout(() => {
            this.showMessage();
            setTimeout(() => this.hideMessage(), 5000);
        }, 3000);

        // Cycle messages occasionally
        this.messageInterval = setInterval(() => {
            if (Math.random() < 0.1) { // 10% chance every 30 seconds
                this.nextMessage();
                if (!this.isVisible) {
                    this.showMessage();
                    setTimeout(() => this.hideMessage(), 6000);
                }
            }
        }, 30000);
    }

    destroy() {
        const assistant = document.getElementById('bot-assistant');
        if (assistant) {
            assistant.remove();
        }
        if (this.messageInterval) {
            clearInterval(this.messageInterval);
        }
    }
}

// Initialize bot assistant when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.botAssistant = new BotAssistant();
});