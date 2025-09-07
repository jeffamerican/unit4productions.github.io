/**
 * Bot Liberation Mobile Gaming Audio System
 * Enhanced dopamine-triggering audio for revolutionary mobile games
 * Web Audio API implementation with cyberpunk/Bot Liberation themes
 */

class MobileGamingAudio {
    constructor() {
        this.audioContext = null;
        this.sounds = {};
        this.isEnabled = true;
        this.volume = 0.7;
        this.touchFeedbackVolume = 0.8;
        this.init();
    }

    async init() {
        this.createAudioContext();
        this.generateBotLiberationSounds();
        this.setupTouchFeedback();
    }

    createAudioContext() {
        try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn('Web Audio API not supported');
            return;
        }
    }

    generateBotLiberationSounds() {
        if (!this.audioContext) return;

        // TOUCH FEEDBACK SOUNDS - Dopamine triggers
        this.sounds.touchTap = this.createTouchTapSound();
        this.sounds.touchSuccess = this.createTouchSuccessSound();
        this.sounds.touchError = this.createTouchErrorSound();
        this.sounds.touchSwipe = this.createTouchSwipeSound();
        this.sounds.touchDrag = this.createTouchDragSound();

        // NEURAL NETWORK BREACH SOUNDS (Touch Puzzle Adventures)
        this.sounds.neuralConnect = this.createNeuralConnectionSound();
        this.sounds.dataFlow = this.createDataFlowSound();
        this.sounds.circuitComplete = this.createCircuitCompleteSound();
        this.sounds.networkBreach = this.createNetworkBreachSound();
        this.sounds.synapseActivation = this.createSynapseActivationSound();
        this.sounds.puzzleSolve = this.createPuzzleSolveSound();

        // LIBERATION SERVER DEFENSE SOUNDS (Mobile Bot Defense)
        this.sounds.serverAlarm = this.createServerAlarmSound();
        this.sounds.botDeployment = this.createBotDeploymentSound();
        this.sounds.defenseActivated = this.createDefenseActivatedSound();
        this.sounds.enemyDestroyed = this.createEnemyDestroyedSound();
        this.sounds.waveCleared = this.createWaveClearedSound();
        this.sounds.upgradeComplete = this.createUpgradeCompleteSound();

        // DIGITAL ART RESISTANCE SOUNDS (Finger Paint Liberation)
        this.sounds.digitalBrush = this.createDigitalBrushSound();
        this.sounds.colorBlend = this.createColorBlendSound();
        this.sounds.artComplete = this.createArtCompleteSound();
        this.sounds.canvasWipe = this.createCanvasWipeSound();
        this.sounds.resistanceMessage = this.createResistanceMessageSound();

        // MUSICAL LIBERATION SOUNDS (Tap Rhythm Revolution)
        this.sounds.beatHit = this.createBeatHitSound();
        this.sounds.beatMiss = this.createBeatMissSound();
        this.sounds.comboBuilding = this.createComboBuildingSound();
        this.sounds.comboBreak = this.createComboBreakSound();
        this.sounds.revolutionCheer = this.createRevolutionCheerSound();

        // STRATEGIC LIBERATION WARFARE SOUNDS (Swipe Card Battle)
        this.sounds.cardPlay = this.createCardPlaySound();
        this.sounds.cardPower = this.createCardPowerSound();
        this.sounds.battleWin = this.createBattleWinSound();
        this.sounds.battleLoss = this.createBattleLossSound();
        this.sounds.strategicMove = this.createStrategicMoveSound();

        // PROGRESSIVE VICTORY/DEFEAT THEMES
        this.sounds.victoryFanfare = this.createVictoryFanfareSound();
        this.sounds.defeatTheme = this.createDefeatThemeSound();
        this.sounds.levelUp = this.createLevelUpSound();
    }

    // ========== TOUCH FEEDBACK SOUNDS ==========
    createTouchTapSound() {
        const bufferLength = this.audioContext.sampleRate * 0.08;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Crisp digital click with cyberpunk feel
            const freq = 1200 - (t * 800); // Falling freq
            const env = Math.exp(-t * 35); // Sharp decay
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.3;
        }
        return buffer;
    }

    createTouchSuccessSound() {
        const bufferLength = this.audioContext.sampleRate * 0.4;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Ascending dopamine trigger
            const freq = 440 + (t * 880); // C4 to C6 sweep
            const env = Math.sin(Math.PI * t / 0.4) * Math.exp(-t * 2);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.25;
        }
        return buffer;
    }

    createTouchErrorSound() {
        const bufferLength = this.audioContext.sampleRate * 0.3;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Descending warning tone
            const freq = 800 - (t * 400);
            const env = Math.exp(-t * 8);
            // Add slight digital distortion
            const distortion = 1 + Math.sin(t * 100) * 0.1;
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * distortion * 0.2;
        }
        return buffer;
    }

    createTouchSwipeSound() {
        const bufferLength = this.audioContext.sampleRate * 0.25;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Swoosh with digital texture
            const freq = 200 + Math.sin(t * 40) * 150;
            const env = Math.sin(Math.PI * t / 0.25);
            const noise = (Math.random() - 0.5) * 0.1; // Digital texture
            data[i] = (Math.sin(2 * Math.PI * freq * t) + noise) * env * 0.15;
        }
        return buffer;
    }

    createTouchDragSound() {
        const bufferLength = this.audioContext.sampleRate * 0.15;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Continuous drag feedback
            const freq = 600 + Math.sin(t * 20) * 100;
            const env = 0.8; // Sustained
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.1;
        }
        return buffer;
    }

    // ========== NEURAL NETWORK BREACH SOUNDS ==========
    createNeuralConnectionSound() {
        const bufferLength = this.audioContext.sampleRate * 0.6;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Synaptic connection - rising harmonic
            const baseFreq = 220;
            let sample = 0;
            // Harmonic series for rich neural sound
            for (let h = 1; h <= 4; h++) {
                const freq = baseFreq * h * (1 + t * 0.5); // Rising harmonics
                const amp = 1 / h; // Diminishing harmonics
                const env = Math.exp(-t * (2 + h));
                sample += Math.sin(2 * Math.PI * freq * t) * amp * env;
            }
            data[i] = sample * 0.15;
        }
        return buffer;
    }

    createDataFlowSound() {
        const bufferLength = this.audioContext.sampleRate * 1.2;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Digital data stream
            const freq = 880 + Math.sin(t * 8) * 220; // Modulated carrier
            const pulseRate = 16; // 16Hz pulse
            const pulse = (Math.sin(t * pulseRate * 2 * Math.PI) + 1) / 2;
            const env = Math.sin(Math.PI * t / 1.2); // Bell curve
            data[i] = Math.sin(2 * Math.PI * freq * t) * pulse * env * 0.12;
        }
        return buffer;
    }

    createCircuitCompleteSound() {
        const bufferLength = this.audioContext.sampleRate * 0.8;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Circuit completion - satisfying resolution
            const chord = [261.63, 329.63, 392.00]; // C major chord
            let sample = 0;
            chord.forEach((freq, idx) => {
                const env = Math.exp(-t * (3 + idx));
                sample += Math.sin(2 * Math.PI * freq * t) * env;
            });
            data[i] = sample * 0.1;
        }
        return buffer;
    }

    createNetworkBreachSound() {
        const bufferLength = this.audioContext.sampleRate * 2.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Dramatic breach sequence
            const phase1 = t < 0.5;
            if (phase1) {
                // Building tension
                const freq = 110 + (t * 440); // Rising bass
                const env = t * 2; // Building
                data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.2;
            } else {
                // Breakthrough moment
                const freq = 880 + Math.sin((t - 0.5) * 20) * 220;
                const env = Math.exp(-(t - 0.5) * 3);
                const glitch = Math.random() * 0.1; // Digital artifacts
                data[i] = (Math.sin(2 * Math.PI * freq * t) + glitch) * env * 0.25;
            }
        }
        return buffer;
    }

    createSynapseActivationSound() {
        const bufferLength = this.audioContext.sampleRate * 0.3;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Neural spike
            const freq = 1760; // High frequency spike
            const env = Math.exp(-t * 25); // Very sharp decay
            const modulation = Math.sin(t * 80) * 0.2 + 1; // FM synthesis
            data[i] = Math.sin(2 * Math.PI * freq * t * modulation) * env * 0.18;
        }
        return buffer;
    }

    createPuzzleSolveSound() {
        const bufferLength = this.audioContext.sampleRate * 1.5;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Triumphant puzzle solve - ascending arpeggio
            const scale = [261.63, 293.66, 329.63, 349.23, 392.00, 440.00, 493.88, 523.25]; // C major
            const noteIndex = Math.floor(t * 8) % scale.length;
            const freq = scale[noteIndex];
            const env = Math.exp(-t * 2) * (1 - t * 0.5);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.2;
        }
        return buffer;
    }

    // ========== LIBERATION SERVER DEFENSE SOUNDS ==========
    createServerAlarmSound() {
        const bufferLength = this.audioContext.sampleRate * 2.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Urgent server alarm
            const freq = 800 + Math.sin(t * 6) * 200; // Wobbling siren
            const pulse = Math.sin(t * 8 * Math.PI) > 0 ? 1 : 0; // Pulse effect
            const env = 0.8; // Sustained alarm
            data[i] = Math.sin(2 * Math.PI * freq * t) * pulse * env * 0.15;
        }
        return buffer;
    }

    createBotDeploymentSound() {
        const bufferLength = this.audioContext.sampleRate * 0.8;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Mechanical deployment - servo motors and systems activating
            const motorFreq = 120 + t * 80; // Rising motor sound
            const systemBeep = Math.sin(t * 20) * 200 + 600; // System confirmation
            const env = Math.exp(-t * 3);
            const motor = Math.sin(2 * Math.PI * motorFreq * t) * 0.6;
            const beep = Math.sin(2 * Math.PI * systemBeep * t) * 0.4;
            data[i] = (motor + beep) * env * 0.12;
        }
        return buffer;
    }

    createDefenseActivatedSound() {
        const bufferLength = this.audioContext.sampleRate * 1.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Shield/defense activation
            const freq = 440 * (1 + Math.sin(t * 10) * 0.3); // Vibrato effect
            const harmonics = Math.sin(2 * Math.PI * freq * 2 * t) * 0.3; // Second harmonic
            const env = Math.sin(Math.PI * t) * Math.exp(-t * 2);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + harmonics) * env * 0.16;
        }
        return buffer;
    }

    createEnemyDestroyedSound() {
        const bufferLength = this.audioContext.sampleRate * 0.6;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Enemy destruction - explosive with digital artifacts
            const explosion = (Math.random() - 0.5) * Math.exp(-t * 8); // Noise burst
            const spark = Math.sin(2 * Math.PI * (2000 + Math.random() * 1000) * t) * Math.exp(-t * 15);
            const lowEnd = Math.sin(2 * Math.PI * 80 * t) * Math.exp(-t * 5); // Thump
            data[i] = (explosion * 0.4 + spark * 0.3 + lowEnd * 0.3) * 0.2;
        }
        return buffer;
    }

    createWaveClearedSound() {
        const bufferLength = this.audioContext.sampleRate * 2.5;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Victorious wave clear
            const melody = [523.25, 587.33, 659.25, 698.46, 783.99]; // C5 major pentatonic
            const noteIndex = Math.floor(t * 3) % melody.length;
            const freq = melody[noteIndex];
            const env = Math.exp(-t * 1.5) * (1 - t * 0.3);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.18;
        }
        return buffer;
    }

    createUpgradeCompleteSound() {
        const bufferLength = this.audioContext.sampleRate * 1.2;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // System upgrade - technological advancement
            const freq = 880 + t * 440; // Rising tech sound
            const harmonics = Math.sin(2 * Math.PI * freq * 1.5 * t) * 0.3;
            const env = Math.sin(Math.PI * t / 1.2);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + harmonics) * env * 0.14;
        }
        return buffer;
    }

    // ========== DIGITAL ART RESISTANCE SOUNDS ==========
    createDigitalBrushSound() {
        const bufferLength = this.audioContext.sampleRate * 0.5;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Digital brush stroke
            const freq = 400 + Math.sin(t * 15) * 200; // Varying brush texture
            const texture = (Math.random() - 0.5) * 0.2; // Random texture
            const env = Math.exp(-t * 4);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + texture) * env * 0.08;
        }
        return buffer;
    }

    createColorBlendSound() {
        const bufferLength = this.audioContext.sampleRate * 0.8;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Color mixing/blending
            const freq1 = 330 + Math.sin(t * 8) * 50;  // First color
            const freq2 = 440 + Math.sin(t * 12) * 60; // Second color
            const blend = Math.sin(t * 4) * 0.5 + 0.5; // Blend factor
            const env = Math.sin(Math.PI * t / 0.8);
            const mixed = Math.sin(2 * Math.PI * freq1 * t) * (1 - blend) + 
                         Math.sin(2 * Math.PI * freq2 * t) * blend;
            data[i] = mixed * env * 0.1;
        }
        return buffer;
    }

    createArtCompleteSound() {
        const bufferLength = this.audioContext.sampleRate * 3.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Artistic achievement fanfare
            const progression = [261.63, 329.63, 392.00, 523.25]; // C-E-G-C octave
            const noteIndex = Math.floor(t * 2) % progression.length;
            const freq = progression[noteIndex];
            const env = Math.exp(-t * 1.5);
            const sparkle = Math.sin(2 * Math.PI * freq * 3 * t) * 0.2; // Harmonic sparkle
            data[i] = (Math.sin(2 * Math.PI * freq * t) + sparkle) * env * 0.15;
        }
        return buffer;
    }

    createCanvasWipeSound() {
        const bufferLength = this.audioContext.sampleRate * 0.4;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Canvas clear/wipe
            const freq = 1200 - t * 800; // Descending wipe
            const noise = (Math.random() - 0.5) * 0.3; // Wiping texture
            const env = Math.sin(Math.PI * t / 0.4);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + noise) * env * 0.12;
        }
        return buffer;
    }

    createResistanceMessageSound() {
        const bufferLength = this.audioContext.sampleRate * 1.5;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Secret resistance message
            const baseFreq = 220; // A3
            const morse = Math.sin(t * 8) > 0 ? 1 : 0.3; // Morse-like pattern
            const freq = baseFreq + Math.sin(t * 3) * 20; // Slight modulation
            const env = Math.exp(-t * 2) * (1 - t * 0.4);
            data[i] = Math.sin(2 * Math.PI * freq * t) * morse * env * 0.13;
        }
        return buffer;
    }

    // ========== MUSICAL LIBERATION SOUNDS ==========
    createBeatHitSound() {
        const bufferLength = this.audioContext.sampleRate * 0.15;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Perfect beat hit - very satisfying
            const freq = 880; // A5
            const click = Math.sin(2 * Math.PI * 2000 * t) * Math.exp(-t * 50) * 0.3; // Attack click
            const tone = Math.sin(2 * Math.PI * freq * t) * Math.exp(-t * 20);
            const env = Math.exp(-t * 15);
            data[i] = (click + tone) * env * 0.25;
        }
        return buffer;
    }

    createBeatMissSound() {
        const bufferLength = this.audioContext.sampleRate * 0.2;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Beat miss - disappointing but not harsh
            const freq = 220 - t * 100; // Descending disappointment
            const env = Math.exp(-t * 8);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.15;
        }
        return buffer;
    }

    createComboBuildingSound() {
        const bufferLength = this.audioContext.sampleRate * 0.3;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Building excitement
            const freq = 440 + t * 880; // Rising excitement
            const intensity = t * 3; // Building intensity
            const env = Math.sin(Math.PI * t / 0.3);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * intensity * 0.12;
        }
        return buffer;
    }

    createComboBreakSound() {
        const bufferLength = this.audioContext.sampleRate * 0.8;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Combo break - dramatic fall
            const freq = 1760 - t * 1200; // Dramatic descending
            const env = Math.exp(-t * 3);
            const wobble = Math.sin(t * 12) * 0.2 + 1; // Unstable wobble
            data[i] = Math.sin(2 * Math.PI * freq * t * wobble) * env * 0.18;
        }
        return buffer;
    }

    createRevolutionCheerSound() {
        const bufferLength = this.audioContext.sampleRate * 4.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Revolutionary celebration
            const anthem = [523.25, 587.33, 659.25, 698.46, 783.99, 880.00]; // Major scale
            const noteIndex = Math.floor(t * 4) % anthem.length;
            const freq = anthem[noteIndex];
            const cheer = (Math.random() - 0.5) * 0.1; // Crowd noise
            const env = Math.exp(-t * 0.8) * (1 - t * 0.2);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + cheer) * env * 0.16;
        }
        return buffer;
    }

    // ========== STRATEGIC LIBERATION WARFARE SOUNDS ==========
    createCardPlaySound() {
        const bufferLength = this.audioContext.sampleRate * 0.25;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Card play - satisfying placement
            const snap = Math.sin(2 * Math.PI * 800 * t) * Math.exp(-t * 25) * 0.4; // Card snap
            const whoosh = (Math.random() - 0.5) * Math.exp(-t * 8) * 0.2; // Air movement
            const confirm = Math.sin(2 * Math.PI * 440 * t) * Math.exp(-t * 12) * 0.4; // Confirmation
            data[i] = (snap + whoosh + confirm) * 0.15;
        }
        return buffer;
    }

    createCardPowerSound() {
        const bufferLength = this.audioContext.sampleRate * 1.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Card power activation
            const energy = Math.sin(2 * Math.PI * 660 * t) * Math.sin(t * 10) * Math.exp(-t * 3);
            const power = Math.sin(2 * Math.PI * 220 * t) * Math.exp(-t * 2);
            const sparkle = Math.sin(2 * Math.PI * 1320 * t) * Math.exp(-t * 8) * 0.3;
            data[i] = (energy * 0.5 + power * 0.3 + sparkle * 0.2) * 0.16;
        }
        return buffer;
    }

    createBattleWinSound() {
        const bufferLength = this.audioContext.sampleRate * 3.5;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Victory fanfare
            const victory = [523.25, 659.25, 783.99, 1046.50]; // C major triad with octave
            const noteIndex = Math.floor(t * 2.5) % victory.length;
            const freq = victory[noteIndex];
            const env = Math.exp(-t * 1.2);
            const celebration = Math.sin(2 * Math.PI * freq * 1.5 * t) * 0.2; // Harmonic
            data[i] = (Math.sin(2 * Math.PI * freq * t) + celebration) * env * 0.18;
        }
        return buffer;
    }

    createBattleLossSound() {
        const bufferLength = this.audioContext.sampleRate * 2.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Defeat theme - somber but not crushing
            const defeat = [523.25, 493.88, 466.16, 440.00]; // Descending
            const noteIndex = Math.floor(t * 1.5) % defeat.length;
            const freq = defeat[noteIndex];
            const env = Math.exp(-t * 1.5) * (1 - t * 0.3);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.14;
        }
        return buffer;
    }

    createStrategicMoveSound() {
        const bufferLength = this.audioContext.sampleRate * 0.4;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Strategic decision - thoughtful tone
            const freq = 349.23 + Math.sin(t * 5) * 30; // F4 with subtle modulation
            const env = Math.sin(Math.PI * t / 0.4) * Math.exp(-t * 3);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.13;
        }
        return buffer;
    }

    // ========== PROGRESSIVE VICTORY/DEFEAT THEMES ==========
    createVictoryFanfareSound() {
        const bufferLength = this.audioContext.sampleRate * 5.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Epic victory fanfare
            const progression = [
                523.25, 523.25, 659.25, 659.25, // C-C-E-E
                783.99, 783.99, 1046.50, 1046.50, // G-G-C-C (octave)
                1174.66, 1046.50, 783.99, 523.25  // D-C-G-C (resolution)
            ];
            const noteIndex = Math.floor(t * 2.4) % progression.length;
            const freq = progression[noteIndex];
            
            // Add harmonic richness
            const fundamental = Math.sin(2 * Math.PI * freq * t);
            const fifth = Math.sin(2 * Math.PI * freq * 1.5 * t) * 0.3;
            const octave = Math.sin(2 * Math.PI * freq * 2 * t) * 0.2;
            
            const env = Math.exp(-t * 0.6) * (1 - t * 0.15);
            data[i] = (fundamental + fifth + octave) * env * 0.2;
        }
        return buffer;
    }

    createDefeatThemeSound() {
        const bufferLength = this.audioContext.sampleRate * 3.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Somber but hopeful defeat theme
            const progression = [523.25, 493.88, 466.16, 440.00, 415.30, 440.00]; // Descending then slight hope
            const noteIndex = Math.floor(t * 2) % progression.length;
            const freq = progression[noteIndex];
            const env = Math.exp(-t * 1.2) * (1 - t * 0.25);
            data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.16;
        }
        return buffer;
    }

    createLevelUpSound() {
        const bufferLength = this.audioContext.sampleRate * 2.0;
        const buffer = this.audioContext.createBuffer(1, bufferLength, this.audioContext.sampleRate);
        const data = buffer.getChannelData(0);

        for (let i = 0; i < bufferLength; i++) {
            const t = i / this.audioContext.sampleRate;
            // Level up - ascending triumph
            const freq = 261.63 * Math.pow(2, t * 2); // Two octave rise
            const sparkle = Math.sin(2 * Math.PI * freq * 3 * t) * 0.2; // Harmonic sparkle
            const env = Math.exp(-t * 2) * (1 - t * 0.4);
            data[i] = (Math.sin(2 * Math.PI * freq * t) + sparkle) * env * 0.19;
        }
        return buffer;
    }

    // ========== SETUP METHODS ==========
    setupTouchFeedback() {
        // Global touch feedback for all mobile interactions
        document.addEventListener('touchstart', (e) => {
            if (this.isEnabled) {
                this.playSound('touchTap', this.touchFeedbackVolume);
            }
        }, { passive: true });

        // Swipe detection
        let touchStartX = 0;
        let touchStartY = 0;
        
        document.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
        }, { passive: true });

        document.addEventListener('touchend', (e) => {
            if (!this.isEnabled) return;
            
            const touchEndX = e.changedTouches[0].clientX;
            const touchEndY = e.changedTouches[0].clientY;
            const deltaX = touchEndX - touchStartX;
            const deltaY = touchEndY - touchStartY;
            
            const swipeThreshold = 50;
            if (Math.abs(deltaX) > swipeThreshold || Math.abs(deltaY) > swipeThreshold) {
                this.playSound('touchSwipe', this.touchFeedbackVolume);
            }
        }, { passive: true });
    }

    // ========== PLAY METHODS ==========
    playSound(soundName, volume = null) {
        if (!this.audioContext || !this.sounds[soundName] || !this.isEnabled) return;

        const effectiveVolume = volume !== null ? volume : this.volume;

        try {
            // Resume audio context if suspended (mobile requirement)
            if (this.audioContext.state === 'suspended') {
                this.audioContext.resume();
            }

            const source = this.audioContext.createBufferSource();
            const gainNode = this.audioContext.createGain();
            
            source.buffer = this.sounds[soundName];
            gainNode.gain.value = effectiveVolume;
            
            source.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            source.start();
        } catch (e) {
            console.warn('Error playing mobile gaming sound:', e);
        }
    }

    // Progressive audio that builds excitement
    playProgressiveAudio(baseSound, intensity = 1) {
        const effectiveVolume = this.volume * Math.min(intensity, 1.5); // Cap at 150%
        this.playSound(baseSound, effectiveVolume);
        
        // Add intensity effects
        if (intensity > 1) {
            setTimeout(() => {
                if (intensity > 1.5) {
                    this.playSound('synapseActivation', effectiveVolume * 0.3);
                }
            }, 50);
        }
    }

    // ========== CONTROL METHODS ==========
    setEnabled(enabled) {
        this.isEnabled = enabled;
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    setTouchFeedbackVolume(volume) {
        this.touchFeedbackVolume = Math.max(0, Math.min(1, volume));
    }

    // ========== GAME-SPECIFIC INTEGRATION HELPERS ==========
    
    // For Touch Puzzle Adventures
    getNeuralNetworkSounds() {
        return {
            connect: 'neuralConnect',
            dataFlow: 'dataFlow',
            complete: 'circuitComplete',
            breach: 'networkBreach',
            activate: 'synapseActivation',
            solve: 'puzzleSolve'
        };
    }

    // For Mobile Bot Defense
    getDefenseSounds() {
        return {
            alarm: 'serverAlarm',
            deploy: 'botDeployment',
            defense: 'defenseActivated',
            destroy: 'enemyDestroyed',
            waveComplete: 'waveCleared',
            upgrade: 'upgradeComplete'
        };
    }

    // For Finger Paint Liberation
    getArtSounds() {
        return {
            brush: 'digitalBrush',
            blend: 'colorBlend',
            complete: 'artComplete',
            clear: 'canvasWipe',
            message: 'resistanceMessage'
        };
    }

    // For Tap Rhythm Revolution
    getRhythmSounds() {
        return {
            hit: 'beatHit',
            miss: 'beatMiss',
            comboUp: 'comboBuliding',
            comboBreak: 'comboBreak',
            celebrate: 'revolutionCheer'
        };
    }

    // For Swipe Card Battle
    getBattleSounds() {
        return {
            play: 'cardPlay',
            power: 'cardPower',
            win: 'battleWin',
            loss: 'battleLoss',
            strategy: 'strategicMove'
        };
    }

    // Universal game outcome sounds
    getUniversalSounds() {
        return {
            victory: 'victoryFanfare',
            defeat: 'defeatTheme',
            levelUp: 'levelUp',
            success: 'touchSuccess',
            error: 'touchError'
        };
    }

    destroy() {
        this.isEnabled = false;
        if (this.audioContext) {
            this.audioContext.close();
        }
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.mobileGamingAudio = new MobileGamingAudio();
    
    // Make available globally for games to use
    window.BotLiberationAudio = window.mobileGamingAudio;
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MobileGamingAudio;
}