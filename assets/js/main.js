// ============================================
// BotInc Gaming Website - Main JavaScript
// Modern interactions and animations
// ============================================

class BotIncWebsite {
    constructor() {
        this.init();
    }

    init() {
        this.setupNavigation();
        this.setupHeroAnimations();
        this.setupScrollEffects();
        this.setupNewsletterForm();
        this.setupGameInteractions();
        this.setupProgressBars();
        this.setupLazyLoading();
        this.setupServiceWorker();
        this.initializeAnalytics();
    }

    // ============================================
    // Navigation System
    // ============================================
    
    setupNavigation() {
        const nav = document.getElementById('mainNav');
        const navToggle = document.getElementById('navToggle');
        const navMenu = document.getElementById('navMenu');
        
        // Mobile menu toggle
        if (navToggle && navMenu) {
            navToggle.addEventListener('click', () => {
                navToggle.classList.toggle('active');
                navMenu.classList.toggle('active');
                document.body.classList.toggle('nav-open');
            });
        }

        // Close mobile menu when clicking outside
        document.addEventListener('click', (e) => {
            if (navMenu && !nav.contains(e.target) && navMenu.classList.contains('active')) {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
                document.body.classList.remove('nav-open');
            }
        });

        // Smooth scrolling for navigation links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                const target = document.querySelector(anchor.getAttribute('href'));
                if (target) {
                    e.preventDefault();
                    const headerOffset = 80;
                    const elementPosition = target.getBoundingClientRect().top;
                    const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                    window.scrollTo({
                        top: offsetPosition,
                        behavior: 'smooth'
                    });

                    // Close mobile menu if open
                    if (navMenu && navMenu.classList.contains('active')) {
                        navToggle.classList.remove('active');
                        navMenu.classList.remove('active');
                        document.body.classList.remove('nav-open');
                    }

                    // Update active nav link
                    this.updateActiveNavLink(anchor.getAttribute('href'));
                }
            });
        });

        // Navigation scroll behavior
        let lastScrollY = window.scrollY;
        window.addEventListener('scroll', () => {
            const currentScrollY = window.scrollY;
            
            if (nav) {
                if (currentScrollY > 100) {
                    nav.classList.add('scrolled');
                } else {
                    nav.classList.remove('scrolled');
                }

                // Hide/show nav on scroll
                if (currentScrollY > lastScrollY && currentScrollY > 200) {
                    nav.style.transform = 'translateY(-100%)';
                } else {
                    nav.style.transform = 'translateY(0)';
                }
            }
            
            lastScrollY = currentScrollY;
            this.updateActiveNavLinkOnScroll();
        });
    }

    updateActiveNavLink(href) {
        document.querySelectorAll('.nav-link').forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('href') === href) {
                link.classList.add('active');
            }
        });
    }

    updateActiveNavLinkOnScroll() {
        const sections = document.querySelectorAll('section[id]');
        const scrollPos = window.scrollY + 150;

        sections.forEach(section => {
            const sectionTop = section.offsetTop;
            const sectionHeight = section.offsetHeight;
            const sectionId = section.getAttribute('id');

            if (scrollPos >= sectionTop && scrollPos < sectionTop + sectionHeight) {
                document.querySelectorAll('.nav-link').forEach(link => {
                    link.classList.remove('active');
                    if (link.getAttribute('href') === `#${sectionId}`) {
                        link.classList.add('active');
                    }
                });
            }
        });
    }

    // ============================================
    // Hero Section Animations
    // ============================================
    
    setupHeroAnimations() {
        const heroCanvas = document.getElementById('heroCanvas');
        if (heroCanvas) {
            this.initHeroCanvas(heroCanvas);
        }

        // Animate hero elements on load
        const heroElements = document.querySelectorAll('.hero-badge, .hero-title, .hero-description, .hero-actions, .hero-stats');
        heroElements.forEach((element, index) => {
            element.style.opacity = '0';
            element.style.transform = 'translateY(30px)';
            
            setTimeout(() => {
                element.style.transition = 'all 0.8s cubic-bezier(0.25, 0.46, 0.45, 0.94)';
                element.style.opacity = '1';
                element.style.transform = 'translateY(0)';
            }, index * 200);
        });
    }

    initHeroCanvas(canvas) {
        const ctx = canvas.getContext('2d');
        let animationId;
        let particles = [];

        const resizeCanvas = () => {
            canvas.width = canvas.offsetWidth;
            canvas.height = canvas.offsetHeight;
        };

        const createParticle = () => ({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            size: Math.random() * 2 + 1,
            speedX: (Math.random() - 0.5) * 0.5,
            speedY: (Math.random() - 0.5) * 0.5,
            opacity: Math.random() * 0.5 + 0.2,
            color: Math.random() > 0.5 ? '#00ff88' : '#00ffff'
        });

        const initParticles = () => {
            particles = [];
            for (let i = 0; i < 50; i++) {
                particles.push(createParticle());
            }
        };

        const animate = () => {
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            particles.forEach(particle => {
                // Update position
                particle.x += particle.speedX;
                particle.y += particle.speedY;

                // Wrap around edges
                if (particle.x < 0) particle.x = canvas.width;
                if (particle.x > canvas.width) particle.x = 0;
                if (particle.y < 0) particle.y = canvas.height;
                if (particle.y > canvas.height) particle.y = 0;

                // Draw particle
                ctx.beginPath();
                ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
                ctx.fillStyle = particle.color + Math.floor(particle.opacity * 255).toString(16).padStart(2, '0');
                ctx.fill();

                // Draw connections
                particles.forEach(otherParticle => {
                    const distance = Math.sqrt(
                        Math.pow(particle.x - otherParticle.x, 2) +
                        Math.pow(particle.y - otherParticle.y, 2)
                    );

                    if (distance < 100) {
                        ctx.beginPath();
                        ctx.moveTo(particle.x, particle.y);
                        ctx.lineTo(otherParticle.x, otherParticle.y);
                        ctx.strokeStyle = `rgba(0, 255, 136, ${(1 - distance / 100) * 0.1})`;
                        ctx.lineWidth = 0.5;
                        ctx.stroke();
                    }
                });
            });

            animationId = requestAnimationFrame(animate);
        };

        // Initialize
        window.addEventListener('resize', resizeCanvas);
        resizeCanvas();
        initParticles();
        animate();

        // Pause animation when not visible
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    if (!animationId) animate();
                } else {
                    if (animationId) {
                        cancelAnimationFrame(animationId);
                        animationId = null;
                    }
                }
            });
        });

        observer.observe(canvas);
    }

    // ============================================
    // Scroll Effects
    // ============================================
    
    setupScrollEffects() {
        // Reveal elements on scroll
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('active');
                }
            });
        }, observerOptions);

        // Observe elements for reveal animation
        document.querySelectorAll('.reveal, .game-card, .feature-item, .value-item').forEach(element => {
            element.classList.add('reveal');
            observer.observe(element);
        });

        // Parallax effect for hero background
        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const parallaxElements = document.querySelectorAll('.hero-background');
            
            parallaxElements.forEach(element => {
                const speed = 0.5;
                element.style.transform = `translateY(${scrolled * speed}px)`;
            });
        });
    }

    // ============================================
    // Progress Bars Animation
    // ============================================
    
    setupProgressBars() {
        const progressBars = document.querySelectorAll('.tech-progress, .progress-fill');
        
        const progressObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const progressBar = entry.target;
                    const targetWidth = progressBar.style.width || '0%';
                    
                    // Reset and animate
                    progressBar.style.width = '0%';
                    setTimeout(() => {
                        progressBar.style.width = targetWidth;
                    }, 200);
                }
            });
        }, { threshold: 0.5 });

        progressBars.forEach(bar => {
            progressObserver.observe(bar);
        });
    }

    // ============================================
    // Newsletter Form
    // ============================================
    
    setupNewsletterForm() {
        const newsletterForm = document.getElementById('newsletterForm');
        if (!newsletterForm) return;

        newsletterForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const formData = new FormData(newsletterForm);
            const email = formData.get('email');
            const submitButton = newsletterForm.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            
            // Validate email
            if (!this.validateEmail(email)) {
                this.showNotification('Please enter a valid email address.', 'error');
                return;
            }
            
            // Update button state
            submitButton.textContent = 'Subscribing...';
            submitButton.disabled = true;
            
            try {
                // Simulate API call (replace with actual endpoint)
                await this.simulateApiCall(1000);
                
                // Success
                this.showNotification('Successfully subscribed! Welcome to BotInc.', 'success');
                newsletterForm.reset();
                
                // Track subscription
                this.trackEvent('newsletter_subscribe', {
                    email: email,
                    source: 'homepage'
                });
                
            } catch (error) {
                console.error('Newsletter subscription error:', error);
                this.showNotification('Subscription failed. Please try again.', 'error');
            } finally {
                submitButton.textContent = originalText;
                submitButton.disabled = false;
            }
        });
    }

    validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    simulateApiCall(delay) {
        return new Promise((resolve) => {
            setTimeout(resolve, delay);
        });
    }

    // ============================================
    // Game Interactions
    // ============================================
    
    setupGameInteractions() {
        // Play game demo functionality
        window.playGameDemo = () => {
            window.open('/games/signal-breach/', '_blank');
            this.trackEvent('demo_played', { game: 'signal-breach' });
        };

        // Wishlist functionality
        window.addToWishlist = (gameId) => {
            const wishlist = JSON.parse(localStorage.getItem('botinc_wishlist') || '[]');
            
            if (!wishlist.includes(gameId)) {
                wishlist.push(gameId);
                localStorage.setItem('botinc_wishlist', JSON.stringify(wishlist));
                this.showNotification(`Added ${gameId} to your wishlist!`, 'success');
                
                this.trackEvent('wishlist_add', { game: gameId });
            } else {
                this.showNotification(`${gameId} is already in your wishlist.`, 'info');
            }
        };

        // Share game functionality
        window.shareGame = (gameId) => {
            const gameUrl = `${window.location.origin}/games/${gameId}/`;
            const shareText = `Check out ${gameId} by BotInc Games!`;
            
            if (navigator.share) {
                navigator.share({
                    title: `${gameId} - BotInc Games`,
                    text: shareText,
                    url: gameUrl
                }).then(() => {
                    this.trackEvent('game_shared', { game: gameId, method: 'native' });
                });
            } else {
                // Fallback to clipboard
                navigator.clipboard.writeText(`${shareText} ${gameUrl}`).then(() => {
                    this.showNotification('Game link copied to clipboard!', 'success');
                    this.trackEvent('game_shared', { game: gameId, method: 'clipboard' });
                });
            }
        };

        // Notify me functionality
        window.notifyMe = (gameId) => {
            const notifications = JSON.parse(localStorage.getItem('botinc_notifications') || '[]');
            
            if (!notifications.includes(gameId)) {
                notifications.push(gameId);
                localStorage.setItem('botinc_notifications', JSON.stringify(notifications));
                this.showNotification(`You'll be notified when ${gameId} is released!`, 'success');
                
                this.trackEvent('notification_signup', { game: gameId });
            } else {
                this.showNotification(`You're already signed up for ${gameId} notifications.`, 'info');
            }
        };
    }

    // ============================================
    // Lazy Loading
    // ============================================
    
    setupLazyLoading() {
        const lazyImages = document.querySelectorAll('img[data-src]');
        
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('loading');
                    img.classList.add('loaded');
                    observer.unobserve(img);
                }
            });
        });

        lazyImages.forEach(img => {
            img.classList.add('loading');
            imageObserver.observe(img);
        });
    }

    // ============================================
    // Notifications System
    // ============================================
    
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;

        // Style the notification
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${this.getNotificationColor(type)};
            color: white;
            padding: 1rem 1.5rem;
            border-radius: 5px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
            z-index: 10000;
            transform: translateX(400px);
            transition: transform 0.3s ease;
            max-width: 300px;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove after 5 seconds
        const autoRemove = setTimeout(() => {
            this.removeNotification(notification);
        }, 5000);

        // Close button functionality
        notification.querySelector('.notification-close').addEventListener('click', () => {
            clearTimeout(autoRemove);
            this.removeNotification(notification);
        });
    }

    getNotificationColor(type) {
        const colors = {
            success: 'linear-gradient(45deg, #00ff88, #00ffff)',
            error: 'linear-gradient(45deg, #ff0088, #ff4444)',
            info: 'linear-gradient(45deg, #0088ff, #00ffff)',
            warning: 'linear-gradient(45deg, #ff8800, #ffff00)'
        };
        return colors[type] || colors.info;
    }

    removeNotification(notification) {
        notification.style.transform = 'translateX(400px)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }

    // ============================================
    // Service Worker Registration
    // ============================================
    
    setupServiceWorker() {
        if ('serviceWorker' in navigator) {
            window.addEventListener('load', () => {
                navigator.serviceWorker.register('./sw.js')
                    .then(registration => {
                        console.log('SW registered: ', registration);
                    })
                    .catch(registrationError => {
                        console.log('SW registration failed: ', registrationError);
                    });
            });
        }
    }

    // ============================================
    // Analytics Integration
    // ============================================
    
    initializeAnalytics() {
        // Initialize analytics (Google Analytics, etc.)
        if (typeof gtag !== 'undefined') {
            gtag('config', 'GA_MEASUREMENT_ID', {
                page_title: document.title,
                page_location: window.location.href
            });
        }
    }

    trackEvent(eventName, parameters = {}) {
        // Track custom events
        if (typeof gtag !== 'undefined') {
            gtag('event', eventName, parameters);
        }
        
        // Console log for development
        console.log('Event tracked:', eventName, parameters);
    }

    // ============================================
    // Performance Monitoring
    // ============================================
    
    trackPerformance() {
        if ('performance' in window) {
            window.addEventListener('load', () => {
                setTimeout(() => {
                    const perfData = performance.getEntriesByType('navigation')[0];
                    
                    this.trackEvent('performance', {
                        load_time: Math.round(perfData.loadEventEnd - perfData.fetchStart),
                        dom_content_loaded: Math.round(perfData.domContentLoadedEventEnd - perfData.fetchStart),
                        first_contentful_paint: Math.round(performance.getEntriesByName('first-contentful-paint')[0]?.startTime || 0)
                    });
                }, 0);
            });
        }
    }
}

// ============================================
// Games Page Specific Functionality
// ============================================

class GamesPage {
    constructor() {
        if (document.querySelector('.games-collection')) {
            this.init();
        }
    }

    init() {
        this.setupGameFilters();
        this.setupGameCardAnimations();
    }

    setupGameFilters() {
        const filterTabs = document.querySelectorAll('.filter-tab');
        const gameCards = document.querySelectorAll('.game-card');

        filterTabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const filter = tab.dataset.filter;
                
                // Update active tab
                filterTabs.forEach(t => t.classList.remove('active'));
                tab.classList.add('active');
                
                // Filter games
                gameCards.forEach(card => {
                    if (filter === 'all' || card.classList.contains(filter)) {
                        card.classList.remove('hiding');
                        card.classList.add('showing');
                    } else {
                        card.classList.add('hiding');
                        card.classList.remove('showing');
                    }
                });
            });
        });
    }

    setupGameCardAnimations() {
        const gameCards = document.querySelectorAll('.game-card');
        
        gameCards.forEach((card, index) => {
            card.style.animationDelay = `${index * 0.1}s`;
        });
    }
}

// ============================================
// Initialize Website
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    new BotIncWebsite();
    new GamesPage();
});

// ============================================
// Utility Functions
// ============================================

// Debounce function for performance
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Throttle function for scroll events
function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    }
}

// Device detection
const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
const isTablet = /(ipad|tablet|(android(?!.*mobile))|(windows(?!.*phone)(.*touch))|kindle|playbook|silk|(puffin(?!.*(IP|AP|WP))))/i.test(navigator.userAgent);

// Add device classes to body
if (isMobile) document.body.classList.add('mobile');
if (isTablet) document.body.classList.add('tablet');

// Export for use in other scripts
window.BotIncWebsite = BotIncWebsite;
window.GamesPage = GamesPage;