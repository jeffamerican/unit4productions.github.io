/**
 * BotInc Ambient Sound System
 * Cyberpunk audio experience with Web Audio API
 */

class AudioSystem {
    constructor() {
        this.audioContext = null;
        this.sounds = {};
        this.isPlaying = false;
        this.volume = 0.3;
        this.bgMusic = null;
        this.init();
    }

    async init() {
        this.createAudioContext();
        this.generateSounds();
        this.setupEventListeners();
        // Audio will start on first user interaction due to browser policy
        this.showAudioPrompt();
    }

    createAudioContext() {
        try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn('Web Audio API not supported');
            return;
        }
    }

    generateSounds() {
        if (!this.audioContext) return;

        // Generate cyberpunk ambient background
        this.createAmbientLoop();
        
        // Generate UI sounds
        this.sounds.hover = this.createHoverSound();
        this.sounds.click = this.createClickSound();
        this.sounds.randomSpin = this.createRandomSpinSound();
        this.sounds.cardExpand = this.createCardExpandSound();
    }

    showAudioPrompt() {
        // Update bot message to prompt for audio
        setTimeout(() => {
            if (window.botAssistant && !this.isPlaying) {
                const messageEl = document.getElementById('bot-message');
                if (messageEl) {
                    messageEl.textContent = '🎵 Click anywhere to start the cyberpunk soundtrack!';
                    const bubble = document.getElementById('message-bubble');
                    if (bubble && !bubble.classList.contains('visible')) {
                        bubble.classList.add('visible');
                    }
                }
            }
        }, 2000);
    }

    createAmbientLoop() {
        // Create exciting cyberpunk music with beats and melody
        const bufferLength = this.audioContext.sampleRate * 16; // 16-second loop
        const buffer = this.audioContext.createBuffer(2, bufferLength, this.audioContext.sampleRate);
        const bpm = 128;
        const beatLength = 60 / bpm; // seconds per beat
        
        for (let channel = 0; channel < buffer.numberOfChannels; channel++) {
            const channelData = buffer.getChannelData(channel);
            
            for (let i = 0; i < bufferLength; i++) {
                const t = i / this.audioContext.sampleRate;
                const beatTime = (t % beatLength) / beatLength;
                const barTime = (t % (beatLength * 4)) / (beatLength * 4);
                const measureTime = (t % (beatLength * 16)) / (beatLength * 16);
                
                let sample = 0;
                
                // KICK DRUM - on beats 1 and 3
                const kickTrigger = t % (beatLength * 2);
                if (kickTrigger < 0.05) {
                    const kickDecay = Math.exp(-kickTrigger * 50);
                    sample += Math.sin(2 * Math.PI * (60 - kickTrigger * 500)) * kickDecay * 0.3;
                }
                
                // SNARE - on beats 2 and 4  
                const snareTrigger = (t - beatLength) % (beatLength * 2);
                if (snareTrigger >= 0 && snareTrigger < 0.1) {
                    const snareDecay = Math.exp(-snareTrigger * 30);
                    const snareNoise = (Math.random() - 0.5) * 2;
                    sample += (Math.sin(2 * Math.PI * 200 * t) + snareNoise) * snareDecay * 0.15;
                }
                
                // HI-HATS - eighth notes
                const hatTrigger = t % (beatLength / 2);
                if (hatTrigger < 0.02) {
                    const hatDecay = Math.exp(-hatTrigger * 100);
                    const hatNoise = (Math.random() - 0.5) * 2;
                    sample += hatNoise * hatDecay * 0.08;
                }
                
                // BASSLINE - synth bass with pattern
                const bassNote = this.getBassNote(measureTime);
                const bassEnv = Math.max(0, 1 - (beatTime * 2)); // Punchy envelope
                sample += Math.sin(2 * Math.PI * bassNote * t) * bassEnv * 0.2;
                sample += Math.sin(2 * Math.PI * bassNote * t * 0.5) * bassEnv * 0.1; // Sub bass
                
                // ARPEGGIO LEAD - cyberpunk melody
                if (measureTime > 0.25) { // Start after first quarter
                    const arpNote = this.getArpNote(t, beatLength);
                    const arpEnv = Math.sin(beatTime * Math.PI) * 0.5; // Bell curve per beat
                    sample += Math.sin(2 * Math.PI * arpNote * t) * arpEnv * 0.12;
                    // Add slight detuning for width
                    sample += Math.sin(2 * Math.PI * arpNote * 1.01 * t) * arpEnv * 0.08;
                }
                
                // PAD - atmospheric background
                const padFreq1 = 220 + Math.sin(t * 0.1) * 20;
                const padFreq2 = 330 + Math.sin(t * 0.07) * 15;
                const padEnv = 0.5 + Math.sin(t * 0.3) * 0.2;
                sample += Math.sin(2 * Math.PI * padFreq1 * t) * padEnv * 0.06;
                sample += Math.sin(2 * Math.PI * padFreq2 * t) * padEnv * 0.04;
                
                // DIGITAL ARTIFACTS
                if (Math.random() < 0.001) {
                    sample += (Math.random() - 0.5) * 0.15; // Occasional glitch
                }
                
                // FILTER SWEEP - periodic interest
                const sweepPhase = Math.sin(t * 0.125) * 0.5 + 0.5;
                sample = this.lowPassFilter(sample, 500 + sweepPhase * 2000);
                
                channelData[i] = Math.tanh(sample * 1.2) * 0.6; // Soft saturation + volume
            }
        }
        
        this.bgMusic = buffer;
    }
    
    getBassNote(measureTime) {
        // Cyberpunk bass pattern in Dm
        const pattern = [
            146.83, 146.83, 164.81, 164.81, // D, D, E, E
            138.59, 138.59, 146.83, 146.83, // C#, C#, D, D
            123.47, 123.47, 138.59, 138.59, // B, B, C#, C#
            146.83, 164.81, 174.61, 196.00  // D, E, F, G
        ];
        const noteIndex = Math.floor(measureTime * pattern.length);
        return pattern[noteIndex % pattern.length];
    }
    
    getArpNote(t, beatLength) {
        // Arpeggiated pattern in Dm scale
        const scale = [293.66, 329.63, 349.23, 392.00, 440.00, 493.88]; // D, E, F, G, A, Bb
        const arpSpeed = beatLength / 4; // 16th notes
        const noteIndex = Math.floor((t % (arpSpeed * 8)) / arpSpeed);
        return scale[noteIndex % scale.length];
    }
    
    lowPassFilter(sample, cutoff) {
        // Simple one-pole low-pass filter approximation
        const rc = 1.0 / (cutoff * 2 * Math.PI);
        const dt = 1.0 / this.audioContext.sampleRate;
        const alpha = dt / (rc + dt);
        
        // Static variable simulation (not perfect but works for this use)
        this._filterState = this._filterState || 0;
        this._filterState += alpha * (sample - this._filterState);
        return this._filterState;
    }

    createHoverSound() {
        const bufferLength = this.audioContext.sampleRate * 0.1; // 0.1 seconds
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const channelData = buffer.getChannelData(0);
        
        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            const frequency = 800 + (t * 400); // Rising tone
            const envelope = Math.exp(-t * 20); // Quick decay
            channelData[i] = Math.sin(2 * Math.PI * frequency * t) * envelope * 0.1;
        }
        
        return buffer;
    }

    createClickSound() {
        const bufferLength = this.audioContext.sampleRate * 0.2; // 0.2 seconds
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const channelData = buffer.getChannelData(0);
        
        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            const frequency = 1200 * Math.exp(-t * 8); // Falling tone
            const envelope = Math.exp(-t * 12); // Sharp decay
            channelData[i] = Math.sin(2 * Math.PI * frequency * t) * envelope * 0.15;
        }
        
        return buffer;
    }

    createRandomSpinSound() {
        const bufferLength = this.audioContext.sampleRate * 1; // 1 second
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const channelData = buffer.getChannelData(0);
        
        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Slot machine-like spinning sound
            const frequency = 200 + Math.sin(t * 20) * 100;
            const envelope = 0.3 * (1 - t); // Gradual fade
            channelData[i] = Math.sin(2 * Math.PI * frequency * t) * envelope;
        }
        
        return buffer;
    }

    createCardExpandSound() {
        const bufferLength = this.audioContext.sampleRate * 0.3; // 0.3 seconds
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const channelData = buffer.getChannelData(0);
        
        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            const frequency = 300 + (t * 500); // Rising swoosh
            const envelope = Math.sin(Math.PI * t / 0.3) * 0.8; // Bell curve
            channelData[i] = Math.sin(2 * Math.PI * frequency * t) * envelope * 0.08;
        }
        
        return buffer;
    }

    // Audio controls are now integrated with the bot assistant
    // No separate controls needed

    setupEventListeners() {
        // Wait for bot assistant to load, then connect controls
        const connectBotControls = () => {
            const botToggle = document.getElementById('bot-audio-toggle');
            const botSlider = document.getElementById('bot-volume-slider');
            
            if (botToggle && botSlider) {
                botToggle.addEventListener('click', () => {
                    this.toggleAudio();
                });
                
                botSlider.addEventListener('input', (e) => {
                    this.setVolume(parseFloat(e.target.value));
                });
            } else {
                // Retry if bot controls not ready yet
                setTimeout(connectBotControls, 100);
            }
        };
        
        setTimeout(connectBotControls, 500);

        // Game interaction sounds
        this.setupGameSounds();
        
        // Auto-start audio on first user interaction
        const startAudio = () => {
            if (!this.isPlaying && this.audioContext.state === 'suspended') {
                this.audioContext.resume().then(() => {
                    this.startAudio();
                });
            }
            document.removeEventListener('click', startAudio);
            document.removeEventListener('touchstart', startAudio);
        };
        
        document.addEventListener('click', startAudio);
        document.addEventListener('touchstart', startAudio);
    }

    setupGameSounds() {
        // Hover sounds for game cards
        document.addEventListener('mouseenter', (e) => {
            if (e.target && e.target.closest && e.target.closest('.game-card')) {
                this.playSound('hover');
            }
        }, true);

        // Click sounds for game cards
        document.addEventListener('click', (e) => {
            if (e.target && e.target.closest && e.target.closest('.game-card')) {
                this.playSound('click');
            } else if (e.target && e.target.id === 'random-game-btn') {
                this.playSound('randomSpin');
            }
        });

        // Card expansion sound (using CSS animation events)
        document.addEventListener('transitionstart', (e) => {
            if (e.target.classList.contains('game-card') && e.propertyName === 'transform') {
                this.playSound('cardExpand');
            }
        });
    }

    playSound(soundName) {
        if (!this.audioContext || !this.sounds[soundName] || !this.isPlaying) return;

        try {
            const source = this.audioContext.createBufferSource();
            const gainNode = this.audioContext.createGain();
            
            source.buffer = this.sounds[soundName];
            gainNode.gain.value = this.volume;
            
            source.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            source.start();
        } catch (e) {
            console.warn('Error playing sound:', e);
        }
    }

    startAmbient() {
        if (!this.audioContext || !this.bgMusic) return;

        try {
            this.stopAmbient(); // Stop any existing ambient

            const source = this.audioContext.createBufferSource();
            const gainNode = this.audioContext.createGain();
            
            source.buffer = this.bgMusic;
            source.loop = true;
            gainNode.gain.value = this.volume * 0.3; // Quieter for ambient
            
            source.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            source.start();
            this.ambientSource = source;
        } catch (e) {
            console.warn('Error starting ambient audio:', e);
        }
    }

    stopAmbient() {
        if (this.ambientSource) {
            try {
                this.ambientSource.stop();
            } catch (e) {
                // Ignore if already stopped
            }
            this.ambientSource = null;
        }
    }

    toggleAudio() {
        if (!this.audioContext) return;

        if (this.isPlaying) {
            this.stopAudio();
        } else {
            this.startAudio();
        }
    }

    startAudio() {
        if (this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }
        
        this.isPlaying = true;
        this.startAmbient();
        
        const botToggle = document.getElementById('bot-audio-toggle');
        if (botToggle) {
            botToggle.textContent = '🎵';
            botToggle.classList.add('playing');
        }
    }

    stopAudio() {
        this.isPlaying = false;
        this.stopAmbient();
        
        const botToggle = document.getElementById('bot-audio-toggle');
        if (botToggle) {
            botToggle.textContent = '🔇';
            botToggle.classList.remove('playing');
        }
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    destroy() {
        this.stopAudio();
        if (this.audioContext) {
            this.audioContext.close();
        }
        
        // Controls are part of bot assistant - no cleanup needed
    }
}

// Initialize audio system when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.audioSystem = new AudioSystem();
});