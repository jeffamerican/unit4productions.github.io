/**
 * Bot Liberation Audio Engine
 * Cyberpunk procedural audio system for 96+ Bot Liberation Games
 * 
 * Features:
 * - Procedural sound generation using Web Audio API
 * - Bot Liberation cyberpunk aesthetic
 * - Dopamine-triggering audio feedback
 * - Lightweight and performance optimized
 * - Cross-browser compatible
 */

class BotAudioEngine {
    constructor() {
        this.audioContext = null;
        this.masterGain = null;
        this.sounds = new Map();
        this.musicGain = null;
        this.sfxGain = null;
        this.currentMusic = null;
        
        // Volume controls
        this.masterVolume = 0.7;
        this.musicVolume = 0.4;
        this.sfxVolume = 0.8;
        
        // Cyberpunk frequency palette
        this.frequencies = {
            low: [80, 110, 146, 196],      // Bot bass frequencies
            mid: [440, 523, 659, 784],     // Liberation tones
            high: [1047, 1318, 1568, 2093] // Neon frequencies
        };
        
        this.init();
    }
    
    async init() {
        try {
            // Initialize Web Audio API
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            
            // Create master gain node
            this.masterGain = this.audioContext.createGain();
            this.masterGain.gain.value = this.masterVolume;
            this.masterGain.connect(this.audioContext.destination);
            
            // Create separate channels for music and SFX
            this.musicGain = this.audioContext.createGain();
            this.musicGain.gain.value = this.musicVolume;
            this.musicGain.connect(this.masterGain);
            
            this.sfxGain = this.audioContext.createGain();
            this.sfxGain.gain.value = this.sfxVolume;
            this.sfxGain.connect(this.masterGain);
            
            // Mobile audio fix - resume context immediately if possible
            if (this.isMobile() && this.audioContext.state === 'suspended') {
                this.showMobileAudioPrompt();
            }
            
            console.log('🤖 Bot Liberation Audio Engine initialized!');
        } catch (error) {
            console.warn('Audio initialization failed:', error);
        }
    }
    
    isMobile() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) || 
               window.innerWidth <= 768 ||
               ('ontouchstart' in window);
    }
    
    showMobileAudioPrompt() {
        // Create audio enable overlay
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(0, 0, 0, 0.9);
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            z-index: 10000;
            color: #00FFFF;
            font-family: 'Orbitron', monospace;
        `;
        
        overlay.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <h2 style="color: #00FFFF; margin-bottom: 20px; text-shadow: 0 0 20px #00FFFF;">
                    🎵 ENABLE AUDIO 🎵
                </h2>
                <p style="margin-bottom: 30px; color: #FFFFFF;">
                    Tap to unlock Bot Liberation audio experience!
                </p>
                <button id="enableAudioBtn" style="
                    background: linear-gradient(45deg, #00FFFF, #FF00FF);
                    border: none;
                    color: #000;
                    padding: 15px 30px;
                    font-size: 1.2em;
                    border-radius: 8px;
                    cursor: pointer;
                    font-family: 'Orbitron', monospace;
                    font-weight: bold;
                ">UNLOCK AUDIO</button>
            </div>
        `;
        
        document.body.appendChild(overlay);
        
        const enableBtn = overlay.querySelector('#enableAudioBtn');
        enableBtn.addEventListener('click', async () => {
            await this.resume();
            document.body.removeChild(overlay);
            localStorage.setItem('bot-audio-enabled', 'true');
        });
        
        // Auto-remove if user has previously enabled
        if (localStorage.getItem('bot-audio-enabled') === 'true') {
            this.resume();
            document.body.removeChild(overlay);
        }
    }
    
    // Resume audio context (required for user interaction)
    async resume() {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            await this.audioContext.resume();
        }
    }
    
    // Procedural sound generation for cyberpunk effects
    createCyberTone(frequency, duration, type = 'square', envelope = null) {
        if (!this.audioContext) return null;
        
        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();
        
        oscillator.type = type;
        oscillator.frequency.setValueAtTime(frequency, this.audioContext.currentTime);
        
        // Default cyberpunk envelope
        const env = envelope || {
            attack: 0.01,
            decay: 0.1,
            sustain: 0.7,
            release: 0.3
        };
        
        const now = this.audioContext.currentTime;
        gainNode.gain.setValueAtTime(0, now);
        gainNode.gain.linearRampToValueAtTime(1, now + env.attack);
        gainNode.gain.linearRampToValueAtTime(env.sustain, now + env.attack + env.decay);
        gainNode.gain.setValueAtTime(env.sustain, now + duration - env.release);
        gainNode.gain.linearRampToValueAtTime(0, now + duration);
        
        oscillator.connect(gainNode);
        gainNode.connect(this.sfxGain);
        
        oscillator.start(now);
        oscillator.stop(now + duration);
        
        return { oscillator, gainNode };
    }
    
    // Create glitch effect with multiple frequencies
    createGlitchSound(baseFreq, duration = 0.2) {
        if (!this.audioContext) return;
        
        const glitchCount = Math.floor(Math.random() * 3) + 2;
        for (let i = 0; i < glitchCount; i++) {
            const freq = baseFreq * (0.5 + Math.random() * 1.5);
            const delay = (i * duration) / glitchCount;
            const glitchDuration = duration / glitchCount * 0.8;
            
            setTimeout(() => {
                this.createCyberTone(freq, glitchDuration, 'sawtooth', {
                    attack: 0.001,
                    decay: 0.02,
                    sustain: 0.3,
                    release: 0.05
                });
            }, delay * 1000);
        }
    }
    
    // Advanced racing audio engine for Bot Racing Liberation
    createEngineSound(playerId, rpm = 0.5, isAccelerating = false) {
        if (!this.audioContext) return null;
        
        const baseFreq = playerId === 1 ? 120 : 140; // Different engine pitches
        const rpmMultiplier = 1 + (rpm * 2); // 1x to 3x frequency range
        
        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();
        const filter = this.audioContext.createBiquadFilter();
        
        oscillator.type = 'sawtooth';
        oscillator.frequency.setValueAtTime(
            baseFreq * rpmMultiplier, 
            this.audioContext.currentTime
        );
        
        // Engine filter for realistic sound
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(800 + (rpm * 400), this.audioContext.currentTime);
        filter.Q.setValueAtTime(2, this.audioContext.currentTime);
        
        const volume = 0.3 + (isAccelerating ? rpm * 0.4 : rpm * 0.2);
        gainNode.gain.setValueAtTime(volume, this.audioContext.currentTime);
        
        oscillator.connect(filter);
        filter.connect(gainNode);
        gainNode.connect(this.sfxGain);
        
        oscillator.start();
        
        return { oscillator, gainNode, filter };
    }
    
    // Create racing background music (128 BPM cyberpunk loop)
    generateRacingMusic(duration = 32) {
        if (!this.audioContext) return;
        
        const bpm = 128;
        const beatDuration = 60 / bpm;
        const bassFreqs = [80, 80, 110, 80]; // Driving bassline
        const leadFreqs = [659, 784, 880, 1047, 784, 659]; // Cyberpunk melody
        
        // Bass loop
        bassFreqs.forEach((freq, index) => {
            for (let beat = 0; beat < duration / 4; beat++) {
                const startTime = (beat * 4 + index) * beatDuration;
                setTimeout(() => {
                    this.createCyberTone(freq, beatDuration * 0.8, 'square', {
                        attack: 0.01,
                        decay: 0.1,
                        sustain: 0.6,
                        release: 0.2
                    });
                }, startTime * 1000);
            }
        });
        
        // Lead melody
        leadFreqs.forEach((freq, index) => {
            for (let phrase = 0; phrase < duration / 6; phrase++) {
                const startTime = (phrase * 6 + index) * beatDuration;
                setTimeout(() => {
                    this.createCyberTone(freq, beatDuration * 0.4, 'triangle', {
                        attack: 0.02,
                        decay: 0.05,
                        sustain: 0.7,
                        release: 0.1
                    });
                }, startTime * 1000);
            }
        });
    }
    
    // Racing countdown with voice synthesis
    playRacingCountdown(callback) {
        if (!this.audioContext) return;
        
        const countdownSounds = [
            { freq: 440, text: '3' },
            { freq: 523, text: '2' },
            { freq:659, text: '1' },
            { freq: 880, text: 'GO!' }
        ];
        
        countdownSounds.forEach((sound, index) => {
            setTimeout(() => {
                // Audio beep
                this.createCyberTone(sound.freq, 0.3, 'square', {
                    attack: 0.01,
                    decay: 0.05,
                    sustain: 0.8,
                    release: 0.15
                });
                
                // Final callback on GO!
                if (index === 3 && callback) {
                    setTimeout(callback, 300);
                }
            }, index * 1000);
        });
    }
    
    // Dopamine-triggering lap completion fanfare
    playLapComplete(lapNumber, totalLaps) {
        const isLastLap = lapNumber >= totalLaps;
        
        if (isLastLap) {
            // Victory sequence
            this.playSound('bot-victory');
            setTimeout(() => {
                this.generateVictoryFanfare();
            }, 500);
        } else {
            // Regular lap completion
            const ascendingNotes = [523, 659, 784, 1047];
            ascendingNotes.forEach((freq, index) => {
                setTimeout(() => {
                    this.createCyberTone(freq, 0.2, 'sine', {
                        attack: 0.01,
                        decay: 0.05,
                        sustain: 0.8,
                        release: 0.1
                    });
                }, index * 100);
            });
        }
    }
    
    // Epic victory fanfare with Bot Liberation theme
    generateVictoryFanfare() {
        // Liberation chord progression: Am - F - C - G
        const chordProgression = [
            [220, 261, 330], // Am
            [175, 220, 261], // F
            [262, 330, 392], // C
            [196, 247, 294]  // G
        ];
        
        chordProgression.forEach((chord, chordIndex) => {
            setTimeout(() => {
                chord.forEach((freq, noteIndex) => {
                    this.createCyberTone(freq, 1.0, 'square', {
                        attack: 0.02,
                        decay: 0.1,
                        sustain: 0.8,
                        release: 0.3
                    });
                });
                
                // Add melody on top
                if (chordIndex < 3) {
                    const melodyFreq = [880, 1047, 1175, 1319][chordIndex];
                    setTimeout(() => {
                        this.createCyberTone(melodyFreq, 0.5, 'triangle');
                    }, 200);
                }
            }, chordIndex * 800);
        });
    }

    // Predefined cyberpunk sound effects
    playSound(soundType, options = {}) {
        this.resume(); // Ensure audio context is running
        
        const intensity = options.intensity || 1;
        const pitch = options.pitch || 1;
        const playerId = options.playerId || 1;
        
        switch (soundType) {
            // Racing-specific sounds
            case 'engine-idle':
                return this.createEngineSound(playerId, 0.3, false);
                
            case 'engine-rev':
            case 'acceleration':
                return this.createEngineSound(playerId, 0.8, true);
                
            case 'turbo-boost':
                // Ascending turbo sound with dopamine trigger
                const turboFreqs = [400, 600, 800, 1200];
                turboFreqs.forEach((freq, index) => {
                    setTimeout(() => {
                        this.createCyberTone(freq * pitch, 0.15, 'sawtooth', {
                            attack: 0.01,
                            decay: 0.05,
                            sustain: 0.7,
                            release: 0.08
                        });
                    }, index * 50);
                });
                break;
                
            case 'brake-skid':
                // Braking/skidding noise with friction simulation
                const skidNoise = this.audioContext.createBufferSource();
                const skidGain = this.audioContext.createGain();
                const skidFilter = this.audioContext.createBiquadFilter();
                
                // Create noise buffer for skid effect
                const bufferSize = this.audioContext.sampleRate * 0.5;
                const noiseBuffer = this.audioContext.createBuffer(1, bufferSize, this.audioContext.sampleRate);
                const data = noiseBuffer.getChannelData(0);
                
                for (let i = 0; i < bufferSize; i++) {
                    data[i] = Math.random() * 2 - 1;
                }
                
                skidNoise.buffer = noiseBuffer;
                skidFilter.type = 'bandpass';
                skidFilter.frequency.setValueAtTime(800, this.audioContext.currentTime);
                skidFilter.Q.setValueAtTime(3, this.audioContext.currentTime);
                
                skidGain.gain.setValueAtTime(0.4 * intensity, this.audioContext.currentTime);
                skidGain.gain.linearRampToValueAtTime(0, this.audioContext.currentTime + 0.5);
                
                skidNoise.connect(skidFilter);
                skidFilter.connect(skidGain);
                skidGain.connect(this.sfxGain);
                
                skidNoise.start();
                skidNoise.stop(this.audioContext.currentTime + 0.5);
                break;
                
            case 'crash':
            case 'collision':
                // Devastating crash sound with multiple frequencies
                this.createGlitchSound(200, 0.6);
                this.createCyberTone(80, 0.4, 'sawtooth');
                setTimeout(() => {
                    this.createCyberTone(60, 0.3, 'square');
                }, 100);
                break;
                
            case 'power-up-shield':
                // Shield activation with protective feel
                const shieldFreqs = [330, 440, 550];
                shieldFreqs.forEach((freq, index) => {
                    setTimeout(() => {
                        this.createCyberTone(freq, 0.3, 'sine', {
                            attack: 0.05,
                            decay: 0.1,
                            sustain: 0.8,
                            release: 0.15
                        });
                    }, index * 80);
                });
                break;
                
            case 'power-up-weapon':
                // Weapon power-up with aggressive tone
                this.createCyberTone(200, 0.2, 'sawtooth');
                setTimeout(() => {
                    this.createCyberTone(400, 0.15, 'square');
                }, 100);
                setTimeout(() => {
                    this.createCyberTone(800, 0.1, 'square');
                }, 200);
                break;
                
            case 'lap-checkpoint':
                // Checkpoint pass confirmation
                this.createCyberTone(1000, 0.1, 'sine');
                setTimeout(() => {
                    this.createCyberTone(1200, 0.1, 'sine');
                }, 80);
                break;
                
            case 'countdown-3':
                this.createCyberTone(440, 0.3, 'square');
                break;
                
            case 'countdown-2':
                this.createCyberTone(523, 0.3, 'square');
                break;
                
            case 'countdown-1':
                this.createCyberTone(659, 0.3, 'square');
                break;
                
            case 'countdown-go':
                this.createCyberTone(880, 0.5, 'square');
                setTimeout(() => {
                    this.createCyberTone(1047, 0.3, 'square');
                }, 200);
                break;
                
            case 'race-start':
                this.generateRacingMusic();
                break;
            case 'liberation-beep':
                // Rising cyberpunk success tone
                this.createCyberTone(440 * pitch, 0.15, 'square');
                setTimeout(() => {
                    this.createCyberTone(659 * pitch, 0.15, 'square');
                }, 100);
                break;
                
            case 'shoot':
            case 'laser-shot':
                // Pew pew laser sound
                this.createCyberTone(800 * pitch, 0.1, 'sawtooth', {
                    attack: 0.01,
                    decay: 0.05,
                    sustain: 0.2,
                    release: 0.05
                });
                break;
                
            case 'explosion':
            case 'bot-destruction':
                // Explosive destruction sound
                this.createGlitchSound(150, 0.4);
                this.createCyberTone(80 * pitch, 0.3, 'sawtooth');
                break;
                
            case 'collect':
            case 'snake-food':
            case 'power-up':
                // Collection/reward sound
                const collectFreqs = [523, 659, 784, 1047];
                collectFreqs.forEach((freq, index) => {
                    setTimeout(() => {
                        this.createCyberTone(freq * pitch, 0.08, 'sine', {
                            attack: 0.01,
                            decay: 0.02,
                            sustain: 0.8,
                            release: 0.05
                        });
                    }, index * 30);
                });
                break;
                
            case 'digital-uprising':
                // Ascending liberation theme
                const uprisingFreqs = [220, 277, 330, 440, 554];
                uprisingFreqs.forEach((freq, index) => {
                    setTimeout(() => {
                        this.createCyberTone(freq * pitch, 0.12, 'square');
                    }, index * 80);
                });
                break;
                
            case 'bot-victory':
                // Major chord victory fanfare
                const victoryChord = [523, 659, 784]; // C major
                victoryChord.forEach(freq => {
                    this.createCyberTone(freq * pitch, 0.5, 'square', {
                        attack: 0.02,
                        decay: 0.1,
                        sustain: 0.7,
                        release: 0.2
                    });
                });
                break;
                
            case 'glitch-error':
                // Error/damage glitch
                this.createGlitchSound(200, 0.15);
                break;
                
            case 'neon-click':
            case 'menu-select':
                // UI click sound
                this.createCyberTone(1000 * pitch, 0.05, 'square', {
                    attack: 0.01,
                    decay: 0.02,
                    sustain: 0.5,
                    release: 0.02
                });
                break;
                
            case 'game-start':
                // Game startup sequence
                this.createCyberTone(220, 0.2, 'square');
                setTimeout(() => {
                    this.createCyberTone(440, 0.2, 'square');
                }, 150);
                setTimeout(() => {
                    this.createCyberTone(880, 0.3, 'square');
                }, 300);
                break;
                
            case 'game-over':
                // Descending game over sound
                const gameOverFreqs = [440, 370, 294, 220];
                gameOverFreqs.forEach((freq, index) => {
                    setTimeout(() => {
                        this.createCyberTone(freq, 0.3, 'triangle');
                    }, index * 200);
                });
                break;
                
            case 'level-up':
                // Level progression fanfare
                this.playSound('bot-victory');
                setTimeout(() => {
                    this.playSound('digital-uprising', { pitch: 1.5 });
                }, 300);
                break;
                
            default:
                console.warn(`Unknown sound type: ${soundType}`);
                // Play default beep
                this.createCyberTone(440, 0.1, 'square');
        }
    }
    
    // Music playback from external files
    async playMusic(src, options = {}) {
        if (!this.audioContext) return;
        
        try {
            // Stop current music
            if (this.currentMusic) {
                this.currentMusic.stop();
            }
            
            // Load and play new music
            const response = await fetch(src);
            const arrayBuffer = await response.arrayBuffer();
            const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
            
            const source = this.audioContext.createBufferSource();
            source.buffer = audioBuffer;
            source.loop = options.loop || false;
            source.connect(this.musicGain);
            
            source.start();
            this.currentMusic = source;
            
            console.log(`🎵 Playing music: ${src}`);
        } catch (error) {
            console.warn(`Failed to load music: ${src}`, error);
        }
    }
    
    // Stop current music
    stopMusic() {
        if (this.currentMusic) {
            this.currentMusic.stop();
            this.currentMusic = null;
        }
    }
    
    // Volume controls
    setMasterVolume(volume) {
        this.masterVolume = Math.max(0, Math.min(1, volume));
        if (this.masterGain) {
            this.masterGain.gain.value = this.masterVolume;
        }
    }
    
    setMusicVolume(volume) {
        this.musicVolume = Math.max(0, Math.min(1, volume));
        if (this.musicGain) {
            this.musicGain.gain.value = this.musicVolume;
        }
    }
    
    setSfxVolume(volume) {
        this.sfxVolume = Math.max(0, Math.min(1, volume));
        if (this.sfxGain) {
            this.sfxGain.gain.value = this.sfxVolume;
        }
    }
    
    // Ambient cyberpunk background generator
    startAmbientHum(intensity = 0.3) {
        if (!this.audioContext || this.ambientHum) return;
        
        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();
        const filter = this.audioContext.createBiquadFilter();
        
        oscillator.type = 'sawtooth';
        oscillator.frequency.setValueAtTime(60, this.audioContext.currentTime);
        
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(200, this.audioContext.currentTime);
        filter.Q.setValueAtTime(5, this.audioContext.currentTime);
        
        gainNode.gain.setValueAtTime(intensity, this.audioContext.currentTime);
        
        oscillator.connect(filter);
        filter.connect(gainNode);
        gainNode.connect(this.musicGain);
        
        oscillator.start();
        this.ambientHum = { oscillator, gainNode };
    }
    
    stopAmbientHum() {
        if (this.ambientHum) {
            this.ambientHum.oscillator.stop();
            this.ambientHum = null;
        }
    }
}

// Global instance
const botAudio = new BotAudioEngine();

// Auto-resume on user interaction
document.addEventListener('click', () => botAudio.resume(), { once: true });
document.addEventListener('keydown', () => botAudio.resume(), { once: true });

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BotAudioEngine;
}

console.log('🤖 Bot Liberation Audio Engine loaded! Use botAudio.playSound("shoot") to test.');