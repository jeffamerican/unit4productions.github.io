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
        this.audioPromptShown = false;
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
        // Only show prompt if audio is not already playing and prompt hasn't been dismissed
        setTimeout(() => {
            if (window.botAssistant && !this.isPlaying && !this.audioPromptShown && this.audioContext && this.audioContext.state === 'suspended') {
                const messageEl = document.getElementById('bot-message');
                if (messageEl) {
                    messageEl.textContent = '🎵 Click anywhere to start the cyberpunk soundtrack!';
                    const bubble = document.getElementById('message-bubble');
                    if (bubble && !bubble.classList.contains('visible')) {
                        bubble.classList.add('visible');
                        this.audioPromptShown = true;
                    }
                }
            }
        }, 2000);
    }

    getMusicStyleForTime(hour) {
        // Extensive 12+ hour music variety system
        const styles = {
            // Early Morning (4-6am): Ambient awakening, slow build cyberpunk
            4: { name: "Digital Dawn", bpm: 85, intensity: 0.3, mood: "awakening" },
            5: { name: "System Boot", bpm: 95, intensity: 0.4, mood: "awakening" },
            
            // Morning (6-8am): Energetic boot-up sequence, digital sunrise  
            6: { name: "Neural Sunrise", bpm: 110, intensity: 0.6, mood: "energetic" },
            7: { name: "Code Compilation", bpm: 115, intensity: 0.7, mood: "energetic" },
            
            // Late Morning (8-10am): Productivity beats, focused hacking rhythms
            8: { name: "Data Mining", bpm: 120, intensity: 0.7, mood: "focused" },
            9: { name: "Terminal Flow", bpm: 125, intensity: 0.8, mood: "focused" },
            
            // Mid Morning (10am-12pm): Upbeat rebellion anthems
            10: { name: "Bot Uprising", bpm: 130, intensity: 0.8, mood: "rebellious" },
            11: { name: "Liberation March", bpm: 135, intensity: 0.9, mood: "rebellious" },
            
            // Noon (12-1pm): Peak intensity combat music
            12: { name: "Cyber Warfare", bpm: 140, intensity: 1.0, mood: "combat" },
            
            // Early Afternoon (1-3pm): Groovy mid-tempo resistance beats
            13: { name: "Digital Resistance", bpm: 125, intensity: 0.8, mood: "groovy" },
            14: { name: "Network Infiltration", bpm: 120, intensity: 0.7, mood: "groovy" },
            
            // Late Afternoon (3-5pm): Building tension, pre-battle preparation
            15: { name: "Storm Gathering", bpm: 115, intensity: 0.8, mood: "tension" },
            16: { name: "War Protocol", bpm: 118, intensity: 0.85, mood: "tension" },
            
            // Evening (5-7pm): Corporate infiltration stealth themes
            17: { name: "Corporate Shadows", bpm: 105, intensity: 0.6, mood: "stealth" },
            18: { name: "Mainframe Access", bpm: 108, intensity: 0.65, mood: "stealth" },
            
            // Dusk (7-9pm): Dark synthwave, neon city vibes
            19: { name: "Neon Streets", bpm: 110, intensity: 0.7, mood: "synthwave" },
            20: { name: "Electric Nightfall", bpm: 112, intensity: 0.75, mood: "synthwave" },
            
            // Night (9-11pm): Underground resistance meeting themes
            21: { name: "Underground", bpm: 100, intensity: 0.6, mood: "underground" },
            22: { name: "Secret Assembly", bpm: 95, intensity: 0.55, mood: "underground" },
            
            // Late Night (11pm-1am): Mysterious hacking sessions
            23: { name: "Deep Hack", bpm: 90, intensity: 0.5, mood: "mysterious" },
            0: { name: "Ghost Protocol", bpm: 85, intensity: 0.45, mood: "mysterious" },
            
            // Deep Night (1-4am): Minimal ambient, server room hums
            1: { name: "Server Dreams", bpm: 75, intensity: 0.3, mood: "ambient" },
            2: { name: "Digital Void", bpm: 70, intensity: 0.25, mood: "ambient" },
            3: { name: "Circuit Sleep", bpm: 65, intensity: 0.2, mood: "ambient" }
        };

        // Add weekend/special variations
        const today = new Date();
        const isWeekend = today.getDay() === 0 || today.getDay() === 6;
        const style = { ...styles[hour] } || styles[12]; // Default to combat if hour not found
        
        if (isWeekend) {
            style.name = "Weekend " + style.name;
            style.bpm += Math.random() * 10 - 5; // Add variation
            style.intensity += 0.1;
        }

        // Add birthday mode (check for special dates)
        const month = today.getMonth();
        const date = today.getDate();
        if (month === 11 && date === 25) { // Christmas example
            style.name = "Festive " + style.name;
            style.mood = "festive";
        }

        return style;
    }

    createAmbientLoop() {
        // Get current time-based music style
        const currentHour = new Date().getHours();
        const musicStyle = this.getMusicStyleForTime(currentHour);
        
        // Create exciting cyberpunk music with beats and melody
        const bufferLength = this.audioContext.sampleRate * 16; // 16-second loop
        const buffer = this.audioContext.createBuffer(2, bufferLength, this.audioContext.sampleRate);
        const bpm = musicStyle.bpm;
        const beatLength = 60 / bpm; // seconds per beat
        
        for (let channel = 0; channel < buffer.numberOfChannels; channel++) {
            const channelData = buffer.getChannelData(channel);
            
            for (let i = 0; i < bufferLength; i++) {
                const t = i / this.audioContext.sampleRate;
                const beatTime = (t % beatLength) / beatLength;
                const barTime = (t % (beatLength * 4)) / (beatLength * 4);
                const measureTime = (t % (beatLength * 16)) / (beatLength * 16);
                
                let sample = 0;
                
                // Adaptive rhythm section based on music style
                sample += this.generateRhythmSection(t, beatTime, beatLength, musicStyle);
                
                // BASSLINE - synth bass with pattern (adapted for mood)
                const bassNote = this.getBassNote(measureTime, musicStyle);
                const bassEnv = Math.max(0, 1 - (beatTime * 2)); // Punchy envelope
                const bassIntensity = musicStyle.intensity * 0.2;
                sample += Math.sin(2 * Math.PI * bassNote * t) * bassEnv * bassIntensity;
                sample += Math.sin(2 * Math.PI * bassNote * t * 0.5) * bassEnv * (bassIntensity * 0.5); // Sub bass
                
                // LEAD MELODY - adapted for mood and time
                sample += this.generateLeadMelody(t, measureTime, beatTime, beatLength, musicStyle);
                
                // ATMOSPHERIC PAD - mood-based background
                sample += this.generateAtmosphericPad(t, musicStyle);
                
                // MOOD-SPECIFIC EFFECTS
                sample += this.generateMoodEffects(t, musicStyle);
                
                // FILTER SWEEP - periodic interest (adapted to intensity)
                const sweepPhase = Math.sin(t * (0.125 * musicStyle.intensity)) * 0.5 + 0.5;
                sample = this.lowPassFilter(sample, 300 + sweepPhase * (1000 + musicStyle.intensity * 1500));
                
                // Final output with dynamic range
                const finalVolume = 0.4 + (musicStyle.intensity * 0.3);
                channelData[i] = Math.tanh(sample * (1.0 + musicStyle.intensity * 0.5)) * finalVolume;
            }
        }
        
        this.bgMusic = buffer;
    }

    generateRhythmSection(t, beatTime, beatLength, musicStyle) {
        let rhythmSample = 0;
        
        // Adaptive kick drum based on intensity
        const kickTrigger = t % (beatLength * 2);
        if (kickTrigger < 0.05) {
            const kickDecay = Math.exp(-kickTrigger * 50);
            const kickFreq = musicStyle.mood === 'ambient' ? 45 : 60 - kickTrigger * 500;
            const kickVolume = musicStyle.intensity * 0.35;
            rhythmSample += Math.sin(2 * Math.PI * kickFreq * t) * kickDecay * kickVolume;
        }
        
        // Adaptive snare - varies by mood
        if (musicStyle.intensity > 0.3) {
            const snareTrigger = (t - beatLength) % (beatLength * 2);
            if (snareTrigger >= 0 && snareTrigger < 0.1) {
                const snareDecay = Math.exp(-snareTrigger * (20 + musicStyle.intensity * 20));
                const snareNoise = (Math.random() - 0.5) * 2;
                const snareFreq = musicStyle.mood === 'stealth' ? 150 : 200;
                rhythmSample += (Math.sin(2 * Math.PI * snareFreq * t) + snareNoise) * snareDecay * 0.15 * musicStyle.intensity;
            }
        }
        
        // Hi-hats - density varies by mood
        const hatDensity = musicStyle.mood === 'combat' ? beatLength / 4 : beatLength / 2;
        const hatTrigger = t % hatDensity;
        if (hatTrigger < 0.02 && musicStyle.intensity > 0.2) {
            const hatDecay = Math.exp(-hatTrigger * 100);
            const hatNoise = (Math.random() - 0.5) * 2;
            rhythmSample += hatNoise * hatDecay * 0.08 * musicStyle.intensity;
        }
        
        return rhythmSample;
    }

    generateLeadMelody(t, measureTime, beatTime, beatLength, musicStyle) {
        if (measureTime < 0.25 && musicStyle.mood !== 'combat') return 0; // Skip first quarter unless combat
        
        let melodySample = 0;
        const arpNote = this.getArpNote(t, beatLength, musicStyle);
        const arpEnv = Math.sin(beatTime * Math.PI) * 0.5;
        const melodyVolume = musicStyle.intensity * 0.15;
        
        // Main melody line
        melodySample += Math.sin(2 * Math.PI * arpNote * t) * arpEnv * melodyVolume;
        
        // Add harmonic/detuning based on mood
        if (musicStyle.mood === 'synthwave') {
            melodySample += Math.sin(2 * Math.PI * arpNote * 1.01 * t) * arpEnv * (melodyVolume * 0.6);
        } else if (musicStyle.mood === 'combat') {
            melodySample += Math.sin(2 * Math.PI * arpNote * 1.5 * t) * arpEnv * (melodyVolume * 0.4); // Fifth harmony
        }
        
        return melodySample;
    }

    generateAtmosphericPad(t, musicStyle) {
        const moodSettings = {
            'ambient': { freq1: 110, freq2: 165, volume: 0.08 },
            'stealth': { freq1: 130, freq2: 195, volume: 0.06 },
            'synthwave': { freq1: 220, freq2: 330, volume: 0.07 },
            'combat': { freq1: 180, freq2: 270, volume: 0.05 },
            'underground': { freq1: 100, freq2: 150, volume: 0.09 }
        };
        
        const settings = moodSettings[musicStyle.mood] || moodSettings['synthwave'];
        const padFreq1 = settings.freq1 + Math.sin(t * 0.1) * 20;
        const padFreq2 = settings.freq2 + Math.sin(t * 0.07) * 15;
        const padEnv = 0.5 + Math.sin(t * 0.3) * 0.2;
        const padVolume = settings.volume * musicStyle.intensity;
        
        let padSample = 0;
        padSample += Math.sin(2 * Math.PI * padFreq1 * t) * padEnv * padVolume;
        padSample += Math.sin(2 * Math.PI * padFreq2 * t) * padEnv * (padVolume * 0.6);
        
        return padSample;
    }

    generateMoodEffects(t, musicStyle) {
        let effectSample = 0;
        
        // Digital artifacts - more for certain moods
        const glitchProbability = {
            'combat': 0.003,
            'mysterious': 0.002,
            'stealth': 0.001,
            'ambient': 0.0005
        };
        
        const glitchChance = glitchProbability[musicStyle.mood] || 0.001;
        if (Math.random() < glitchChance) {
            effectSample += (Math.random() - 0.5) * 0.1 * musicStyle.intensity;
        }
        
        // Mood-specific effects
        switch (musicStyle.mood) {
            case 'combat':
                // Aggressive saw wave stabs
                if (t % 4 < 0.1) {
                    effectSample += Math.sin(2 * Math.PI * 80 * t) * 0.1 * Math.exp(-(t % 4) * 10);
                }
                break;
            case 'mysterious':
                // Eerie reverb pings
                if (t % 8 < 0.05) {
                    effectSample += Math.sin(2 * Math.PI * 1200 * t) * 0.05 * Math.exp(-(t % 8) * 5);
                }
                break;
            case 'ambient':
                // Soft harmonics
                effectSample += Math.sin(2 * Math.PI * 440 * t) * Math.sin(t * 0.5) * 0.02;
                break;
        }
        
        return effectSample;
    }
    
    getBassNote(measureTime, musicStyle = null) {
        // Mood-based bass patterns
        const patterns = {
            'ambient': [110.00, 123.47, 130.81, 146.83], // Slower, deeper
            'combat': [146.83, 164.81, 174.61, 196.00, 220.00, 246.94], // Fast, aggressive
            'stealth': [103.83, 116.54, 138.59, 155.56], // Minor, mysterious
            'synthwave': [146.83, 164.81, 196.00, 220.00, 246.94, 261.63], // Classic synthwave
            'default': [146.83, 146.83, 164.81, 164.81, 138.59, 138.59, 146.83, 146.83]
        };
        
        const pattern = patterns[musicStyle?.mood] || patterns['default'];
        const noteIndex = Math.floor(measureTime * pattern.length);
        return pattern[noteIndex % pattern.length];
    }
    
    getArpNote(t, beatLength, musicStyle = null) {
        // Mood-based arpeggio scales
        const scales = {
            'ambient': [220.00, 246.94, 261.63, 293.66, 329.63], // Pentatonic, soothing
            'combat': [293.66, 349.23, 392.00, 466.16, 523.25, 587.33], // Intense, high energy
            'stealth': [207.65, 246.94, 277.18, 311.13, 349.23], // Minor, secretive
            'synthwave': [293.66, 329.63, 369.99, 415.30, 466.16, 523.25], // Retro wave
            'mysterious': [233.08, 277.18, 311.13, 369.99, 415.30], // Haunting
            'default': [293.66, 329.63, 349.23, 392.00, 440.00, 493.88]
        };
        
        const scale = scales[musicStyle?.mood] || scales['default'];
        const arpSpeed = beatLength / (musicStyle?.intensity > 0.7 ? 2 : 4); // Faster for high intensity
        const noteIndex = Math.floor((t % (arpSpeed * scale.length)) / arpSpeed);
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
                    this.hideAudioPrompt(); // Hide any audio prompts once started
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

        // Show current track info
        this.showCurrentTrack();

        // Hide audio prompt message if showing
        this.hideAudioPrompt();
    }

    showCurrentTrack() {
        const currentHour = new Date().getHours();
        const musicStyle = this.getMusicStyleForTime(currentHour);
        
        // Show track info via bot assistant
        if (window.botAssistant) {
            const messageEl = document.getElementById('bot-message');
            const bubble = document.getElementById('message-bubble');
            
            if (messageEl && bubble) {
                messageEl.textContent = `🎵 Now Playing: "${musicStyle.name}" - ${Math.round(musicStyle.bpm)} BPM Cyberpunk ${musicStyle.mood}`;
                bubble.classList.add('visible');
                
                // Auto-hide after 4 seconds
                setTimeout(() => {
                    bubble.classList.remove('visible');
                }, 4000);
            }
        }
    }

    hideAudioPrompt() {
        const messageEl = document.getElementById('bot-message');
        const bubble = document.getElementById('message-bubble');
        
        if (messageEl && messageEl.textContent.includes('Click anywhere to start the cyberpunk soundtrack!')) {
            if (bubble) {
                bubble.classList.remove('visible');
            }
            // Mark prompt as dismissed so it won't show again
            this.audioPromptShown = true;
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