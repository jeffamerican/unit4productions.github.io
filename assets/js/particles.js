/**
 * BotInc Advanced Particle System
 * Multi-layered cyberpunk particle effects
 */

class ParticleSystem {
    constructor() {
        this.particles = {
            binary: [],
            orbs: [],
            matrix: [],
            geometric: [],
            streaks: []
        };
        this.canvas = null;
        this.ctx = null;
        this.animationId = null;
        this.init();
    }

    init() {
        this.createCanvas();
        this.initParticles();
        this.animate();
        this.setupResize();
    }

    createCanvas() {
        this.canvas = document.createElement('canvas');
        this.canvas.id = 'particle-canvas';
        this.canvas.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 1;
            opacity: 0.9;
        `;
        
        document.body.prepend(this.canvas);
        this.ctx = this.canvas.getContext('2d');
        this.resize();
    }

    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    setupResize() {
        window.addEventListener('resize', () => {
            this.resize();
        });
    }

    initParticles() {
        // Adjust particle counts based on performance tier
        const tier = window.perfManager ? window.perfManager.getCurrentTier() : 3;
        const particleMultiplier = {
            1: 0,    // Potato - no particles 
            2: 0,    // Basic - no particles
            3: 0.5,  // Standard - half particles
            4: 1.0,  // Enhanced - full particles
            5: 1.5   // Ultra - more particles
        }[tier] || 0.5;

        // Binary code particles
        const binaryCount = Math.floor(30 * particleMultiplier);
        for (let i = 0; i < binaryCount; i++) {
            this.particles.binary.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height,
                vx: (Math.random() - 0.5) * 0.5,
                vy: -Math.random() * 0.8 - 0.2,
                text: Math.random() > 0.5 ? '1' : '0',
                opacity: Math.random() * 0.7 + 0.3,
                size: Math.random() * 12 + 8,
                life: Math.random() * 200 + 100
            });
        }

        // Glowing orbs
        const orbCount = Math.floor(15 * particleMultiplier);
        for (let i = 0; i < orbCount; i++) {
            this.particles.orbs.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height,
                vx: (Math.random() - 0.5) * 0.3,
                vy: (Math.random() - 0.5) * 0.3,
                radius: Math.random() * 4 + 2,
                opacity: Math.random() * 0.6 + 0.4,
                color: this.getRandomColor(),
                pulsePhase: Math.random() * Math.PI * 2,
                pulseSpeed: Math.random() * 0.02 + 0.01
            });
        }

        // Matrix characters
        const matrixCount = Math.floor(20 * particleMultiplier);
        for (let i = 0; i < matrixCount; i++) {
            this.particles.matrix.push({
                x: Math.random() * this.canvas.width,
                y: -50,
                vy: Math.random() * 2 + 1,
                char: this.getRandomMatrixChar(),
                opacity: Math.random() * 0.8 + 0.2,
                size: Math.random() * 14 + 10,
                trail: []
            });
        }

        // Geometric shapes
        const geometricCount = Math.floor(10 * particleMultiplier);
        for (let i = 0; i < geometricCount; i++) {
            this.particles.geometric.push({
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height,
                vx: (Math.random() - 0.5) * 0.4,
                vy: (Math.random() - 0.5) * 0.4,
                rotation: Math.random() * Math.PI * 2,
                rotationSpeed: (Math.random() - 0.5) * 0.02,
                size: Math.random() * 20 + 10,
                shape: Math.random() > 0.5 ? 'triangle' : 'square',
                opacity: Math.random() * 0.4 + 0.2,
                color: this.getRandomColor()
            });
        }

        // Light streaks
        const streakCount = Math.floor(5 * particleMultiplier);
        for (let i = 0; i < streakCount; i++) {
            this.createStreak();
        }
    }

    getRandomColor() {
        const colors = ['#00FFFF', '#FF00FF', '#00FF00', '#FF0080'];
        return colors[Math.floor(Math.random() * colors.length)];
    }

    getRandomMatrixChar() {
        const chars = 'アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン';
        return chars[Math.floor(Math.random() * chars.length)];
    }

    createStreak() {
        this.particles.streaks.push({
            x: Math.random() * this.canvas.width,
            y: Math.random() * this.canvas.height,
            vx: (Math.random() - 0.5) * 8,
            vy: (Math.random() - 0.5) * 8,
            length: Math.random() * 100 + 50,
            opacity: 1,
            color: this.getRandomColor(),
            life: Math.random() * 60 + 30
        });
    }

    updateBinary() {
        this.particles.binary.forEach((particle, index) => {
            particle.x += particle.vx;
            particle.y += particle.vy;
            particle.life--;

            if (particle.life <= 0 || particle.y < -50) {
                // Respawn at bottom
                particle.x = Math.random() * this.canvas.width;
                particle.y = this.canvas.height + 50;
                particle.life = Math.random() * 200 + 100;
                particle.text = Math.random() > 0.5 ? '1' : '0';
                particle.opacity = Math.random() * 0.7 + 0.3;
            }

            // Fade out near edges
            if (particle.y < 100) {
                particle.opacity *= (particle.y / 100);
            }
        });
    }

    updateOrbs() {
        this.particles.orbs.forEach(orb => {
            orb.x += orb.vx;
            orb.y += orb.vy;
            orb.pulsePhase += orb.pulseSpeed;

            // Bounce off edges
            if (orb.x <= orb.radius || orb.x >= this.canvas.width - orb.radius) {
                orb.vx *= -1;
            }
            if (orb.y <= orb.radius || orb.y >= this.canvas.height - orb.radius) {
                orb.vy *= -1;
            }

            // Keep in bounds
            orb.x = Math.max(orb.radius, Math.min(this.canvas.width - orb.radius, orb.x));
            orb.y = Math.max(orb.radius, Math.min(this.canvas.height - orb.radius, orb.y));
        });
    }

    updateMatrix() {
        this.particles.matrix.forEach(particle => {
            particle.y += particle.vy;

            // Add to trail
            particle.trail.push({ x: particle.x, y: particle.y, opacity: particle.opacity });
            if (particle.trail.length > 10) {
                particle.trail.shift();
            }

            if (particle.y > this.canvas.height + 50) {
                particle.x = Math.random() * this.canvas.width;
                particle.y = -50;
                particle.char = this.getRandomMatrixChar();
                particle.trail = [];
            }
        });
    }

    updateGeometric() {
        this.particles.geometric.forEach(shape => {
            shape.x += shape.vx;
            shape.y += shape.vy;
            shape.rotation += shape.rotationSpeed;

            // Wrap around edges
            if (shape.x > this.canvas.width + shape.size) shape.x = -shape.size;
            if (shape.x < -shape.size) shape.x = this.canvas.width + shape.size;
            if (shape.y > this.canvas.height + shape.size) shape.y = -shape.size;
            if (shape.y < -shape.size) shape.y = this.canvas.height + shape.size;
        });
    }

    updateStreaks() {
        this.particles.streaks.forEach((streak, index) => {
            streak.x += streak.vx;
            streak.y += streak.vy;
            streak.life--;
            streak.opacity = streak.life / 60;

            if (streak.life <= 0) {
                this.particles.streaks.splice(index, 1);
                // Create new streak occasionally
                if (Math.random() < 0.1) {
                    this.createStreak();
                }
            }
        });
    }

    drawBinary() {
        this.ctx.font = `${12}px 'Share Tech Mono', monospace`;
        this.ctx.textAlign = 'center';
        
        this.particles.binary.forEach(particle => {
            this.ctx.fillStyle = `rgba(0, 255, 255, ${particle.opacity})`;
            this.ctx.fillText(particle.text, particle.x, particle.y);
        });
    }

    drawOrbs() {
        this.particles.orbs.forEach(orb => {
            const pulseFactor = Math.sin(orb.pulsePhase) * 0.3 + 0.7;
            const radius = orb.radius * pulseFactor;
            
            // Glow effect
            const gradient = this.ctx.createRadialGradient(orb.x, orb.y, 0, orb.x, orb.y, radius * 2);
            gradient.addColorStop(0, `${orb.color}${Math.floor(orb.opacity * 255).toString(16).padStart(2, '0')}`);
            gradient.addColorStop(1, `${orb.color}00`);
            
            this.ctx.fillStyle = gradient;
            this.ctx.beginPath();
            this.ctx.arc(orb.x, orb.y, radius * 2, 0, Math.PI * 2);
            this.ctx.fill();
            
            // Core
            this.ctx.fillStyle = orb.color;
            this.ctx.beginPath();
            this.ctx.arc(orb.x, orb.y, radius * 0.3, 0, Math.PI * 2);
            this.ctx.fill();
        });
    }

    drawMatrix() {
        this.ctx.font = `${14}px 'Share Tech Mono', monospace`;
        this.ctx.textAlign = 'center';
        
        this.particles.matrix.forEach(particle => {
            // Draw trail
            particle.trail.forEach((point, index) => {
                const trailOpacity = (index / particle.trail.length) * particle.opacity * 0.5;
                this.ctx.fillStyle = `rgba(0, 255, 0, ${trailOpacity})`;
                this.ctx.fillText(particle.char, point.x, point.y);
            });
            
            // Draw main character
            this.ctx.fillStyle = `rgba(0, 255, 0, ${particle.opacity})`;
            this.ctx.fillText(particle.char, particle.x, particle.y);
        });
    }

    drawGeometric() {
        this.particles.geometric.forEach(shape => {
            this.ctx.save();
            this.ctx.translate(shape.x, shape.y);
            this.ctx.rotate(shape.rotation);
            this.ctx.strokeStyle = `${shape.color}${Math.floor(shape.opacity * 255).toString(16).padStart(2, '0')}`;
            this.ctx.lineWidth = 2;
            
            this.ctx.beginPath();
            if (shape.shape === 'triangle') {
                this.ctx.moveTo(0, -shape.size/2);
                this.ctx.lineTo(-shape.size/2, shape.size/2);
                this.ctx.lineTo(shape.size/2, shape.size/2);
                this.ctx.closePath();
            } else {
                this.ctx.rect(-shape.size/2, -shape.size/2, shape.size, shape.size);
            }
            this.ctx.stroke();
            this.ctx.restore();
        });
    }

    drawStreaks() {
        this.particles.streaks.forEach(streak => {
            this.ctx.strokeStyle = `${streak.color}${Math.floor(streak.opacity * 255).toString(16).padStart(2, '0')}`;
            this.ctx.lineWidth = 2;
            this.ctx.lineCap = 'round';
            
            this.ctx.beginPath();
            this.ctx.moveTo(streak.x, streak.y);
            this.ctx.lineTo(streak.x - streak.vx * 10, streak.y - streak.vy * 10);
            this.ctx.stroke();
        });
    }

    animate() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // Update particles
        this.updateBinary();
        this.updateOrbs();
        this.updateMatrix();
        this.updateGeometric();
        this.updateStreaks();

        // Draw particles
        this.drawBinary();
        this.drawOrbs();
        this.drawMatrix();
        this.drawGeometric();
        this.drawStreaks();

        this.animationId = requestAnimationFrame(() => this.animate());
    }

    destroy() {
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }
        if (this.canvas) {
            this.canvas.remove();
        }
    }
}

// Initialize particle system when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Wait for performance manager to initialize
    setTimeout(() => {
        if (window.perfFeatures && window.perfFeatures.particles) {
            window.particleSystem = new ParticleSystem();
        }
    }, 100);
});

// Listen for performance tier changes
window.addEventListener('performanceTierChanged', (event) => {
    const { tier, capabilities } = event.detail;
    
    if (tier >= 3 && !window.particleSystem) {
        // Enable particles for tier 3+
        window.particleSystem = new ParticleSystem();
    } else if (tier < 3 && window.particleSystem) {
        // Disable particles for low-tier devices
        window.particleSystem.destroy();
        window.particleSystem = null;
    }
});