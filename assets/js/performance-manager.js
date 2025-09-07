/**
 * Bot Liberation Performance Manager
 * Adaptive performance system for optimal experience across all devices
 * From potato mode to ultra-high-fi with maximum dopamine
 */

class PerformanceManager {
    constructor() {
        this.currentTier = 3; // Default to standard mode
        this.capabilities = {};
        this.benchmarks = {};
        this.preferences = this.loadPreferences();
        this.monitor = new PerformanceMonitor();
        this.features = {};
        
        // Performance thresholds
        this.tierThresholds = {
            1: { fps: 15, memory: 2, cores: 2, gpu: 'basic' },      // Potato
            2: { fps: 30, memory: 4, cores: 4, gpu: 'integrated' }, // Basic
            3: { fps: 45, memory: 8, cores: 4, gpu: 'decent' },     // Standard
            4: { fps: 60, memory: 16, cores: 8, gpu: 'dedicated' }, // Enhanced
            5: { fps: 120, memory: 32, cores: 16, gpu: 'gaming' }   // Ultra
        };
        
        this.init();
    }

    async init() {
        console.log('🎯 Bot Liberation Performance Manager initializing...');
        
        // Skip detection if user has manual preference
        if (this.preferences.manualTier) {
            this.currentTier = this.preferences.manualTier;
            this.applyTier();
            return;
        }
        
        try {
            await this.detectCapabilities();
            await this.runBenchmarks();
            this.calculateOptimalTier();
            this.applyTier();
            this.startMonitoring();
            
            console.log(`🚀 Performance tier ${this.currentTier} activated!`);
        } catch (error) {
            console.warn('Performance detection failed, using standard mode:', error);
            this.currentTier = 3;
            this.applyTier();
        }
    }

    async detectCapabilities() {
        this.capabilities = {
            // Hardware detection
            cores: navigator.hardwareConcurrency || 4,
            memory: navigator.deviceMemory || 4,
            platform: navigator.platform,
            userAgent: navigator.userAgent,
            
            // GPU detection
            gpu: this.detectGPU(),
            webgl: this.testWebGLSupport(),
            
            // Network
            connection: this.getConnectionInfo(),
            
            // Screen
            screen: {
                width: window.screen.width,
                height: window.screen.height,
                pixelRatio: window.devicePixelRatio || 1,
                refreshRate: await this.detectRefreshRate()
            },
            
            // Mobile detection
            mobile: this.isMobile(),
            
            // Battery (if available)
            battery: await this.getBatteryInfo()
        };
    }

    detectGPU() {
        const canvas = document.createElement('canvas');
        const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
        
        if (!gl) return 'none';
        
        const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
        if (!debugInfo) return 'unknown';
        
        const renderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);
        
        // Classify GPU types
        if (renderer.toLowerCase().includes('nvidia') || renderer.toLowerCase().includes('geforce')) {
            if (renderer.includes('RTX') || renderer.includes('GTX 1080') || renderer.includes('GTX 1070')) {
                return 'gaming';
            } else if (renderer.includes('GTX') || renderer.includes('GT ')) {
                return 'dedicated';
            }
        } else if (renderer.toLowerCase().includes('amd') || renderer.toLowerCase().includes('radeon')) {
            if (renderer.includes('RX ') || renderer.includes('Vega')) {
                return 'gaming';
            } else if (renderer.includes('R9') || renderer.includes('R7')) {
                return 'dedicated';
            }
        } else if (renderer.toLowerCase().includes('intel')) {
            return 'integrated';
        }
        
        return 'decent';
    }

    testWebGLSupport() {
        try {
            const canvas = document.createElement('canvas');
            return !!(window.WebGLRenderingContext && 
                     canvas.getContext('webgl') || canvas.getContext('experimental-webgl'));
        } catch (e) {
            return false;
        }
    }

    getConnectionInfo() {
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        if (!connection) return { type: 'unknown', speed: 'unknown' };
        
        return {
            type: connection.effectiveType || 'unknown',
            downlink: connection.downlink || 0,
            rtt: connection.rtt || 0
        };
    }

    async detectRefreshRate() {
        return new Promise((resolve) => {
            let start = performance.now();
            let frameCount = 0;
            
            function countFrames() {
                frameCount++;
                const elapsed = performance.now() - start;
                
                if (elapsed >= 1000) { // 1 second sample
                    resolve(Math.round(frameCount * 1000 / elapsed));
                } else {
                    requestAnimationFrame(countFrames);
                }
            }
            
            requestAnimationFrame(countFrames);
        });
    }

    isMobile() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) || 
               window.innerWidth <= 768 ||
               ('ontouchstart' in window);
    }

    async getBatteryInfo() {
        if ('getBattery' in navigator) {
            try {
                const battery = await navigator.getBattery();
                return {
                    charging: battery.charging,
                    level: battery.level
                };
            } catch (e) {
                return { charging: true, level: 1 };
            }
        }
        return { charging: true, level: 1 };
    }

    async runBenchmarks() {
        console.log('🔥 Running performance benchmarks...');
        
        this.benchmarks = {
            fps: await this.benchmarkFPS(),
            cssAnimation: await this.benchmarkCSSAnimation(),
            canvasRendering: await this.benchmarkCanvasRendering(),
            memoryUsage: this.measureMemoryUsage()
        };
    }

    async benchmarkFPS() {
        return new Promise((resolve) => {
            let frames = 0;
            let start = performance.now();
            
            function frame() {
                frames++;
                const elapsed = performance.now() - start;
                
                if (elapsed >= 1000) { // 1 second test
                    resolve(Math.round(frames * 1000 / elapsed));
                } else {
                    requestAnimationFrame(frame);
                }
            }
            
            requestAnimationFrame(frame);
        });
    }

    async benchmarkCSSAnimation() {
        return new Promise((resolve) => {
            const testElement = document.createElement('div');
            testElement.style.cssText = `
                position: fixed;
                top: -100px;
                left: -100px;
                width: 50px;
                height: 50px;
                background: red;
                transition: transform 0.5s ease;
            `;
            document.body.appendChild(testElement);
            
            const start = performance.now();
            testElement.style.transform = 'translateX(100px)';
            
            const checkAnimation = () => {
                const computed = getComputedStyle(testElement);
                const transform = computed.transform;
                
                if (transform !== 'none' && transform.includes('matrix')) {
                    const duration = performance.now() - start;
                    document.body.removeChild(testElement);
                    resolve(duration < 100 ? 'fast' : duration < 300 ? 'medium' : 'slow');
                } else {
                    requestAnimationFrame(checkAnimation);
                }
            };
            
            requestAnimationFrame(checkAnimation);
        });
    }

    async benchmarkCanvasRendering() {
        return new Promise((resolve) => {
            const canvas = document.createElement('canvas');
            canvas.width = 500;
            canvas.height = 500;
            const ctx = canvas.getContext('2d');
            
            const start = performance.now();
            
            // Draw complex shapes
            for (let i = 0; i < 1000; i++) {
                ctx.beginPath();
                ctx.arc(Math.random() * 500, Math.random() * 500, Math.random() * 10, 0, Math.PI * 2);
                ctx.fillStyle = `hsl(${Math.random() * 360}, 50%, 50%)`;
                ctx.fill();
            }
            
            const duration = performance.now() - start;
            resolve(duration < 50 ? 'fast' : duration < 150 ? 'medium' : 'slow');
        });
    }

    measureMemoryUsage() {
        if ('memory' in performance) {
            return {
                used: performance.memory.usedJSHeapSize / 1048576, // MB
                total: performance.memory.totalJSHeapSize / 1048576, // MB
                limit: performance.memory.jsHeapSizeLimit / 1048576 // MB
            };
        }
        return { used: 0, total: 0, limit: 0 };
    }

    calculateOptimalTier() {
        let score = 0;
        
        // CPU score (0-5)
        if (this.capabilities.cores >= 16) score += 5;
        else if (this.capabilities.cores >= 8) score += 4;
        else if (this.capabilities.cores >= 4) score += 3;
        else if (this.capabilities.cores >= 2) score += 2;
        else score += 1;
        
        // Memory score (0-5)
        if (this.capabilities.memory >= 32) score += 5;
        else if (this.capabilities.memory >= 16) score += 4;
        else if (this.capabilities.memory >= 8) score += 3;
        else if (this.capabilities.memory >= 4) score += 2;
        else score += 1;
        
        // GPU score (0-5)
        switch (this.capabilities.gpu) {
            case 'gaming': score += 5; break;
            case 'dedicated': score += 4; break;
            case 'decent': score += 3; break;
            case 'integrated': score += 2; break;
            default: score += 1;
        }
        
        // FPS score (0-5)
        if (this.benchmarks.fps >= 120) score += 5;
        else if (this.benchmarks.fps >= 60) score += 4;
        else if (this.benchmarks.fps >= 45) score += 3;
        else if (this.benchmarks.fps >= 30) score += 2;
        else score += 1;
        
        // Mobile penalty
        if (this.capabilities.mobile) score -= 3;
        
        // Battery penalty
        if (this.capabilities.battery && !this.capabilities.battery.charging && this.capabilities.battery.level < 0.3) {
            score -= 2;
        }
        
        // Convert score to tier (4-20 points possible)
        if (score >= 18) this.currentTier = 5;      // Ultra
        else if (score >= 14) this.currentTier = 4; // Enhanced  
        else if (score >= 10) this.currentTier = 3; // Standard
        else if (score >= 6) this.currentTier = 2;  // Basic
        else this.currentTier = 1;                  // Potato
        
        console.log(`Performance Score: ${score}/20 → Tier ${this.currentTier}`);
    }

    applyTier() {
        // Remove existing tier classes
        document.body.classList.remove('perf-tier-1', 'perf-tier-2', 'perf-tier-3', 'perf-tier-4', 'perf-tier-5');
        
        // Apply new tier class
        document.body.classList.add(`perf-tier-${this.currentTier}`);
        
        // Set CSS custom properties
        document.documentElement.style.setProperty('--perf-tier', this.currentTier);
        
        // Configure features based on tier
        this.configureFeatures();
        
        // Dispatch tier change event
        window.dispatchEvent(new CustomEvent('performanceTierChanged', { 
            detail: { tier: this.currentTier, capabilities: this.capabilities } 
        }));
    }

    configureFeatures() {
        this.features = {
            particles: this.currentTier >= 3,
            complexAnimations: this.currentTier >= 3,
            shadows: this.currentTier >= 2,
            gradients: this.currentTier >= 2,
            canvasEffects: this.currentTier >= 4,
            easterEggs: this.currentTier >= 4,
            interactiveEffects: this.currentTier >= 4,
            ultraEffects: this.currentTier >= 5,
            audioProcessing: this.currentTier >= 3,
            hapticFeedback: this.currentTier >= 3
        };
        
        // Store features globally
        window.perfFeatures = this.features;
    }

    startMonitoring() {
        this.monitor.start();
        
        // Auto-adjust based on performance
        setInterval(() => {
            const currentFPS = this.monitor.getAverageFPS();
            
            // Downgrade if struggling
            if (currentFPS < 25 && this.currentTier > 1) {
                console.log('📉 Performance struggling, downgrading tier');
                this.currentTier--;
                this.applyTier();
            }
            
            // Upgrade if performing well (be conservative)
            else if (currentFPS > 55 && this.currentTier < 5 && this.currentTier < this.getMaxPossibleTier()) {
                console.log('📈 Performance excellent, considering upgrade');
                // Only upgrade after sustained good performance
                setTimeout(() => {
                    if (this.monitor.getAverageFPS() > 55) {
                        this.currentTier++;
                        this.applyTier();
                    }
                }, 5000);
            }
        }, 10000); // Check every 10 seconds
    }

    getMaxPossibleTier() {
        // Calculate max tier based on hardware, not current performance
        let maxScore = 0;
        if (this.capabilities.cores >= 8) maxScore += 4;
        else if (this.capabilities.cores >= 4) maxScore += 3;
        if (this.capabilities.memory >= 8) maxScore += 3;
        if (this.capabilities.gpu === 'gaming' || this.capabilities.gpu === 'dedicated') maxScore += 4;
        
        if (maxScore >= 10) return 5;
        if (maxScore >= 8) return 4;
        if (maxScore >= 6) return 3;
        if (maxScore >= 4) return 2;
        return 1;
    }

    // User preference methods
    setManualTier(tier) {
        this.preferences.manualTier = tier;
        this.currentTier = tier;
        this.savePreferences();
        this.applyTier();
    }

    resetToAuto() {
        this.preferences.manualTier = null;
        this.savePreferences();
        this.init();
    }

    loadPreferences() {
        try {
            const stored = localStorage.getItem('bot-perf-preferences');
            return stored ? JSON.parse(stored) : {};
        } catch (e) {
            return {};
        }
    }

    savePreferences() {
        localStorage.setItem('bot-perf-preferences', JSON.stringify(this.preferences));
    }

    // Getters
    getCurrentTier() {
        return this.currentTier;
    }

    getCapabilities() {
        return this.capabilities;
    }

    getBenchmarks() {
        return this.benchmarks;
    }

    getFeatures() {
        return this.features;
    }
}

class PerformanceMonitor {
    constructor() {
        this.fpsHistory = [];
        this.frameCount = 0;
        this.lastTime = performance.now();
        this.isRunning = false;
    }

    start() {
        this.isRunning = true;
        this.measureFPS();
    }

    stop() {
        this.isRunning = false;
    }

    measureFPS() {
        if (!this.isRunning) return;
        
        const currentTime = performance.now();
        this.frameCount++;
        
        // Calculate FPS every second
        if (currentTime - this.lastTime >= 1000) {
            const fps = Math.round((this.frameCount * 1000) / (currentTime - this.lastTime));
            this.fpsHistory.push(fps);
            
            // Keep only last 30 seconds
            if (this.fpsHistory.length > 30) {
                this.fpsHistory.shift();
            }
            
            this.frameCount = 0;
            this.lastTime = currentTime;
        }
        
        requestAnimationFrame(() => this.measureFPS());
    }

    getAverageFPS() {
        if (this.fpsHistory.length === 0) return 60; // Assume good performance initially
        return this.fpsHistory.reduce((a, b) => a + b, 0) / this.fpsHistory.length;
    }

    getCurrentFPS() {
        return this.fpsHistory[this.fpsHistory.length - 1] || 60;
    }
}

// Initialize performance manager
window.perfManager = new PerformanceManager();