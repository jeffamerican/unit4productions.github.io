/**
 * BotInc Mobile Optimization System
 * Enhanced mobile experience with touch gestures, performance optimizations, and mobile-specific features
 */

class MobileOptimization {
    constructor() {
        this.isMobile = this.detectMobile();
        this.isTablet = this.detectTablet();
        this.touchDevice = 'ontouchstart' in window;
        this.orientation = this.getOrientation();
        
        this.init();
    }
    
    detectMobile() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
               window.innerWidth <= 768;
    }
    
    detectTablet() {
        return /iPad|Android/i.test(navigator.userAgent) && 
               window.innerWidth >= 768 && window.innerWidth <= 1024;
    }
    
    getOrientation() {
        return window.innerHeight > window.innerWidth ? 'portrait' : 'landscape';
    }
    
    init() {
        if (this.isMobile || this.touchDevice) {
            this.setupMobileOptimizations();
            this.setupTouchGestures();
            this.setupOrientationHandling();
            this.optimizePerformance();
            this.setupMobileUI();
        }
        
        this.setupResponsiveImages();
        this.setupViewportOptimization();
    }
    
    setupMobileOptimizations() {
        // Add mobile-specific CSS class
        document.body.classList.add('mobile-device');
        
        if (this.isTablet) {
            document.body.classList.add('tablet-device');
        }
        
        // Disable hover effects on mobile
        const style = document.createElement('style');
        style.textContent = `
            @media (hover: none) and (pointer: coarse) {
                .hover-effect:hover,
                .btn:hover,
                .nav-link:hover,
                .game-card:hover {
                    transform: none !important;
                    box-shadow: initial !important;
                }
            }
        `;
        document.head.appendChild(style);
        
        // Prevent zoom on form inputs
        this.preventFormZoom();
        
        // Optimize touch targets
        this.optimizeTouchTargets();
    }
    
    setupTouchGestures() {
        let startX, startY, endX, endY;
        
        document.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });
        
        document.addEventListener('touchmove', (e) => {
            // Prevent overscroll on body
            if (e.target === document.body || e.target === document.documentElement) {
                e.preventDefault();
            }
        }, { passive: false });
        
        document.addEventListener('touchend', (e) => {
            if (!e.changedTouches[0]) return;
            
            endX = e.changedTouches[0].clientX;
            endY = e.changedTouches[0].clientY;
            
            this.handleSwipeGesture(startX, startY, endX, endY);
        }, { passive: true });
        
        // Add haptic feedback for touch interactions
        this.setupHapticFeedback();
    }
    
    handleSwipeGesture(startX, startY, endX, endY) {
        const deltaX = endX - startX;
        const deltaY = endY - startY;
        const minSwipeDistance = 50;
        
        if (Math.abs(deltaX) < minSwipeDistance && Math.abs(deltaY) < minSwipeDistance) {
            return; // Not a swipe
        }
        
        if (Math.abs(deltaX) > Math.abs(deltaY)) {
            // Horizontal swipe
            if (deltaX > 0) {
                this.handleSwipeRight();
            } else {
                this.handleSwipeLeft();
            }
        } else {
            // Vertical swipe
            if (deltaY > 0) {
                this.handleSwipeDown();
            } else {
                this.handleSwipeUp();
            }
        }
    }
    
    handleSwipeRight() {
        // Navigate back in games or open side menu
        if (window.location.pathname.includes('/games/')) {
            this.showNavigationHint('Swipe left to continue');
        }
    }
    
    handleSwipeLeft() {
        // Next action in games or close menus
        if (window.location.pathname.includes('/games/')) {
            this.showNavigationHint('Swipe right to go back');
        }
    }
    
    handleSwipeDown() {
        // Pull to refresh or show more content
        if (window.scrollY === 0) {
            this.triggerPullToRefresh();
        }
    }
    
    handleSwipeUp() {
        // Quick access to leaderboards or social sharing
        const leaderboardWidget = document.getElementById('leaderboard-widget');
        if (leaderboardWidget) {
            leaderboardWidget.classList.toggle('collapsed');
        }
    }
    
    setupHapticFeedback() {
        // Add subtle vibration feedback for interactions
        const addHapticFeedback = (element, intensity = 'light') => {
            element.addEventListener('touchstart', () => {
                if (navigator.vibrate) {
                    const vibrationPattern = {
                        light: [10],
                        medium: [20],
                        heavy: [30, 10, 20]
                    };
                    navigator.vibrate(vibrationPattern[intensity] || vibrationPattern.light);
                }
            }, { passive: true });
        };
        
        // Add haptic feedback to buttons
        document.querySelectorAll('.btn, .game-card, .achievement-notification').forEach(element => {
            addHapticFeedback(element, 'light');
        });
        
        // Add stronger feedback for achievements
        document.addEventListener('achievementUnlocked', () => {
            if (navigator.vibrate) {
                navigator.vibrate([50, 30, 50]);
            }
        });
    }
    
    setupOrientationHandling() {
        const handleOrientationChange = () => {
            const newOrientation = this.getOrientation();
            
            if (newOrientation !== this.orientation) {
                this.orientation = newOrientation;
                document.body.classList.remove('portrait', 'landscape');
                document.body.classList.add(newOrientation);
                
                // Recalculate layouts
                setTimeout(() => {
                    this.recalculateLayouts();
                }, 100);
                
                // Show orientation-specific hints
                this.showOrientationHints();
            }
        };
        
        window.addEventListener('orientationchange', handleOrientationChange);
        window.addEventListener('resize', handleOrientationChange);
        
        // Set initial orientation
        document.body.classList.add(this.orientation);
    }
    
    showOrientationHints() {
        if (window.location.pathname.includes('/games/')) {
            if (this.orientation === 'landscape') {
                this.showNotification('Landscape mode active - Enhanced gaming experience!', 'success');
            } else {
                this.showNotification('Portrait mode - Rotate for better gaming!', 'info');
            }
        }
    }
    
    optimizePerformance() {
        // Reduce motion for better performance on low-end devices
        if (this.isLowEndDevice()) {
            document.body.classList.add('reduced-motion');
            
            const style = document.createElement('style');
            style.textContent = `
                .reduced-motion * {
                    animation-duration: 0.1s !important;
                    animation-iteration-count: 1 !important;
                    transition-duration: 0.1s !important;
                }
            `;
            document.head.appendChild(style);
        }
        
        // Lazy load images
        this.setupLazyLoading();
        
        // Optimize reflows and repaints
        this.optimizeRendering();
        
        // Preload critical resources
        this.preloadCriticalResources();
    }
    
    isLowEndDevice() {
        return navigator.hardwareConcurrency <= 2 || 
               navigator.deviceMemory <= 2 ||
               /Android.*[Vv]ersion\/[89]\./i.test(navigator.userAgent);
    }
    
    setupLazyLoading() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        imageObserver.unobserve(img);
                    }
                });
            });
            
            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
    }
    
    optimizeRendering() {
        // Use passive event listeners where possible
        const passiveEvents = ['scroll', 'touchstart', 'touchmove', 'wheel'];
        
        passiveEvents.forEach(event => {
            document.addEventListener(event, () => {}, { passive: true });
        });
        
        // Debounce resize events
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                this.recalculateLayouts();
            }, 250);
        });
    }
    
    setupMobileUI() {
        // Create mobile navigation if needed
        this.createMobileNavigation();
        
        // Add mobile-specific buttons
        this.addMobileControlButtons();
        
        // Optimize form inputs for mobile
        this.optimizeFormsForMobile();
        
        // Add pull-to-refresh indicator
        this.setupPullToRefresh();
    }
    
    createMobileNavigation() {
        const nav = document.querySelector('.main-nav');
        if (nav && !nav.querySelector('.mobile-menu-toggle')) {
            const menuToggle = document.createElement('button');
            menuToggle.className = 'mobile-menu-toggle';
            menuToggle.innerHTML = '☰';
            menuToggle.addEventListener('click', () => {
                nav.classList.toggle('mobile-menu-open');
            });
            
            nav.appendChild(menuToggle);
            
            // Add mobile menu styles
            const style = document.createElement('style');
            style.textContent = `
                @media (max-width: 768px) {
                    .mobile-menu-toggle {
                        display: block;
                        background: none;
                        border: 2px solid var(--primary-color, #00ffff);
                        color: var(--text-primary, #ffffff);
                        padding: 10px;
                        border-radius: 4px;
                        cursor: pointer;
                        font-size: 18px;
                        min-width: 44px;
                        min-height: 44px;
                    }
                    
                    .nav-menu {
                        display: none;
                        position: fixed;
                        top: 70px;
                        left: 0;
                        right: 0;
                        background: rgba(26, 26, 46, 0.95);
                        backdrop-filter: blur(10px);
                        padding: 20px;
                        z-index: 1000;
                    }
                    
                    .mobile-menu-open .nav-menu {
                        display: flex;
                        flex-direction: column;
                        gap: 15px;
                    }
                    
                    .nav-link {
                        padding: 15px;
                        border: 1px solid var(--primary-color, #00ffff);
                        border-radius: 6px;
                        text-align: center;
                        min-height: 44px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }
                }
            `;
            document.head.appendChild(style);
        }
    }
    
    addMobileControlButtons() {
        if (window.location.pathname.includes('/games/')) {
            const controlsContainer = document.createElement('div');
            controlsContainer.className = 'mobile-game-controls';
            controlsContainer.innerHTML = `
                <button class="mobile-control-btn" id="mobileShareBtn">📤</button>
                <button class="mobile-control-btn" id="mobileLeaderboardBtn">🏆</button>
                <button class="mobile-control-btn" id="mobilePauseBtn">⏸️</button>
                <button class="mobile-control-btn" id="mobileFullscreenBtn">⛶</button>
            `;
            
            document.body.appendChild(controlsContainer);
            
            // Add mobile controls styles
            const style = document.createElement('style');
            style.textContent = `
                .mobile-game-controls {
                    position: fixed;
                    bottom: 20px;
                    right: 20px;
                    display: flex;
                    flex-direction: column;
                    gap: 10px;
                    z-index: 1001;
                }
                
                @media (min-width: 769px) {
                    .mobile-game-controls {
                        display: none;
                    }
                }
                
                .mobile-control-btn {
                    width: 50px;
                    height: 50px;
                    border-radius: 50%;
                    background: rgba(0, 255, 255, 0.9);
                    border: none;
                    color: #000;
                    font-size: 18px;
                    cursor: pointer;
                    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
                    transition: all 0.3s ease;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                
                .mobile-control-btn:active {
                    transform: scale(0.95);
                    background: rgba(0, 255, 255, 1);
                }
            `;
            document.head.appendChild(style);
            
            // Setup mobile control handlers
            this.setupMobileControlHandlers();
        }
    }
    
    setupMobileControlHandlers() {
        const shareBtn = document.getElementById('mobileShareBtn');
        const leaderboardBtn = document.getElementById('mobileLeaderboardBtn');
        const pauseBtn = document.getElementById('mobilePauseBtn');
        const fullscreenBtn = document.getElementById('mobileFullscreenBtn');
        
        if (shareBtn) {
            shareBtn.addEventListener('click', () => {
                if (window.viralMechanics) {
                    window.viralMechanics.showSocialShareDialog();
                }
            });
        }
        
        if (leaderboardBtn) {
            leaderboardBtn.addEventListener('click', () => {
                const widget = document.getElementById('leaderboard-widget');
                if (widget) {
                    widget.classList.remove('collapsed');
                }
            });
        }
        
        if (pauseBtn) {
            pauseBtn.addEventListener('click', () => {
                // Trigger pause in current game
                if (window.game && typeof window.game.pause === 'function') {
                    window.game.pause();
                }
            });
        }
        
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', () => {
                if (!document.fullscreenElement) {
                    document.documentElement.requestFullscreen();
                } else {
                    document.exitFullscreen();
                }
            });
        }
    }
    
    optimizeFormsForMobile() {
        document.querySelectorAll('input, textarea, select').forEach(input => {
            // Prevent zoom on focus
            if (input.type !== 'range' && input.type !== 'checkbox' && input.type !== 'radio') {
                if (parseFloat(getComputedStyle(input).fontSize) < 16) {
                    input.style.fontSize = '16px';
                }
            }
            
            // Add proper input modes
            if (input.type === 'email') {
                input.setAttribute('inputmode', 'email');
            } else if (input.type === 'tel') {
                input.setAttribute('inputmode', 'tel');
            } else if (input.type === 'number') {
                input.setAttribute('inputmode', 'numeric');
            }
            
            // Improve touch targets
            if (getComputedStyle(input).minHeight < '44px') {
                input.style.minHeight = '44px';
            }
        });
    }
    
    setupPullToRefresh() {
        let startY = 0;
        let pullDistance = 0;
        let isPulling = false;
        let refreshThreshold = 80;
        
        const createPullIndicator = () => {
            const indicator = document.createElement('div');
            indicator.id = 'pull-to-refresh-indicator';
            indicator.innerHTML = '↓ Pull to refresh';
            indicator.style.cssText = `
                position: fixed;
                top: -60px;
                left: 0;
                right: 0;
                height: 60px;
                background: var(--primary-color, #00ffff);
                color: var(--background-dark, #000);
                display: flex;
                align-items: center;
                justify-content: center;
                font-weight: bold;
                transition: transform 0.3s ease;
                z-index: 1000;
            `;
            document.body.appendChild(indicator);
            return indicator;
        };
        
        const indicator = createPullIndicator();
        
        document.addEventListener('touchstart', (e) => {
            if (window.scrollY === 0) {
                startY = e.touches[0].clientY;
                isPulling = true;
            }
        }, { passive: true });
        
        document.addEventListener('touchmove', (e) => {
            if (!isPulling) return;
            
            pullDistance = Math.max(0, e.touches[0].clientY - startY);
            
            if (pullDistance > 0) {
                const progress = Math.min(pullDistance / refreshThreshold, 1);
                indicator.style.transform = `translateY(${Math.min(pullDistance * 0.8, 60)}px)`;
                
                if (progress >= 1) {
                    indicator.innerHTML = '↑ Release to refresh';
                } else {
                    indicator.innerHTML = '↓ Pull to refresh';
                }
            }
        }, { passive: true });
        
        document.addEventListener('touchend', () => {
            if (isPulling && pullDistance >= refreshThreshold) {
                this.triggerPullToRefresh();
            }
            
            isPulling = false;
            pullDistance = 0;
            indicator.style.transform = 'translateY(-60px)';
            indicator.innerHTML = '↓ Pull to refresh';
        }, { passive: true });
    }
    
    triggerPullToRefresh() {
        // Refresh current page data
        if (window.botincAnalytics) {
            window.botincAnalytics.updateDashboardDisplay();
        }
        
        if (window.leaderboardSystem) {
            const gameSelect = document.getElementById('gameSelect');
            if (gameSelect) {
                window.leaderboardSystem.switchGame(gameSelect.value);
            }
        }
        
        this.showNotification('Content refreshed!', 'success');
    }
    
    preventFormZoom() {
        const viewportMeta = document.querySelector('meta[name="viewport"]');
        if (viewportMeta) {
            viewportMeta.setAttribute('content', 
                viewportMeta.getAttribute('content') + ', maximum-scale=1.0, user-scalable=no'
            );
        }
    }
    
    optimizeTouchTargets() {
        document.querySelectorAll('a, button, input, select, textarea').forEach(element => {
            const computedStyle = getComputedStyle(element);
            const minSize = 44; // Apple/Google recommended minimum touch target size
            
            if (parseInt(computedStyle.height) < minSize || parseInt(computedStyle.width) < minSize) {
                element.style.minHeight = minSize + 'px';
                element.style.minWidth = minSize + 'px';
                element.style.padding = Math.max(parseInt(computedStyle.padding) || 0, 8) + 'px';
            }
        });
    }
    
    setupResponsiveImages() {
        // Create responsive image system
        const images = document.querySelectorAll('img[data-mobile-src]');
        images.forEach(img => {
            if (this.isMobile && img.dataset.mobileSrc) {
                img.src = img.dataset.mobileSrc;
            }
        });
        
        // Optimize image loading for mobile
        if (this.isMobile) {
            document.querySelectorAll('img').forEach(img => {
                img.loading = 'lazy';
            });
        }
    }
    
    setupViewportOptimization() {
        // Dynamic viewport height for mobile browsers
        const setViewportHeight = () => {
            const vh = window.innerHeight * 0.01;
            document.documentElement.style.setProperty('--vh', `${vh}px`);
        };
        
        setViewportHeight();
        window.addEventListener('resize', setViewportHeight);
        window.addEventListener('orientationchange', () => {
            setTimeout(setViewportHeight, 100);
        });
        
        // Add CSS custom property support
        const style = document.createElement('style');
        style.textContent = `
            .full-height-mobile {
                height: 100vh;
                height: calc(var(--vh, 1vh) * 100);
            }
        `;
        document.head.appendChild(style);
    }
    
    recalculateLayouts() {
        // Force recalculation of game canvases and layouts
        const canvases = document.querySelectorAll('canvas');
        canvases.forEach(canvas => {
            const rect = canvas.getBoundingClientRect();
            if (rect.width > 0 && rect.height > 0) {
                canvas.width = rect.width;
                canvas.height = rect.height;
            }
        });
        
        // Trigger resize events for games
        if (window.game && typeof window.game.handleResize === 'function') {
            window.game.handleResize();
        }
        
        // Update leaderboard widget position
        const widget = document.getElementById('leaderboard-widget');
        if (widget && this.isMobile) {
            widget.style.position = 'fixed';
            widget.style.bottom = '0';
            widget.style.right = '0';
            widget.style.left = '0';
            widget.style.borderRadius = '15px 15px 0 0';
        }
    }
    
    showNavigationHint(message) {
        this.showNotification(message, 'info', 2000);
    }
    
    showNotification(message, type = 'info', duration = 3000) {
        const notification = document.createElement('div');
        notification.className = `mobile-notification mobile-notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            left: 20px;
            right: 20px;
            padding: 15px;
            border-radius: 8px;
            color: white;
            font-weight: bold;
            text-align: center;
            z-index: 10000;
            opacity: 0;
            transform: translateY(-20px);
            transition: all 0.3s ease;
            ${type === 'success' ? 'background: #4CAF50;' : ''}
            ${type === 'error' ? 'background: #f44336;' : ''}
            ${type === 'warning' ? 'background: #ff9800;' : ''}
            ${type === 'info' ? 'background: #2196F3;' : ''}
        `;
        
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 10);
        
        // Animate out and remove
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, duration);
    }
    
    preloadCriticalResources() {
        // Preload critical game assets for better performance
        const criticalAssets = [
            '/assets/js/viral-mechanics.js',
            '/assets/js/leaderboards.js',
            '/assets/css/main.css'
        ];
        
        criticalAssets.forEach(asset => {
            const link = document.createElement('link');
            link.rel = 'preload';
            
            if (asset.endsWith('.js')) {
                link.as = 'script';
            } else if (asset.endsWith('.css')) {
                link.as = 'style';
            }
            
            link.href = asset;
            document.head.appendChild(link);
        });
    }
}

// Initialize mobile optimization
document.addEventListener('DOMContentLoaded', () => {
    window.mobileOptimization = new MobileOptimization();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MobileOptimization;
}