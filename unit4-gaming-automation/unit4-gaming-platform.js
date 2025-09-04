/**
 * Unit4Productions Gaming Platform Automation System
 * Main orchestrator for complete GitHub Pages gaming website deployment
 */

const GitHubAPIManager = require('./github-api-manager');
const WebsiteTemplateSystem = require('./website-template-system');
const GameDeploymentPipeline = require('./game-deployment-pipeline');
const DomainConfigurationSystem = require('./domain-configuration-system');
const MonetizationAnalyticsSystem = require('./monetization-analytics-system');
const TestingValidationSystem = require('./testing-validation-system');

class Unit4GamingPlatform {
    constructor(config) {
        // Validate required configuration
        this.validateConfig(config);
        
        // Initialize all subsystems
        this.github = new GitHubAPIManager(config.githubToken, config.owner);
        this.templates = new WebsiteTemplateSystem({
            siteName: config.siteName || 'Unit4Productions',
            siteUrl: config.siteUrl || 'https://unit4productions.com',
            description: config.description,
            primaryColor: config.primaryColor,
            secondaryColor: config.secondaryColor,
            analyticsId: config.analyticsId,
            adsenseId: config.adsenseId
        });
        
        this.gamesPipeline = new GameDeploymentPipeline(this.github, this.templates);
        this.domains = new DomainConfigurationSystem(this.github, config.cloudflareConfig);
        this.monetization = new MonetizationAnalyticsSystem(config);
        this.testing = new TestingValidationSystem(this.github, this.domains);
        
        this.config = config;
        this.deploymentLog = [];
    }

    /**
     * Validate configuration requirements
     */
    validateConfig(config) {
        const required = ['githubToken', 'owner'];
        const missing = required.filter(key => !config[key]);
        
        if (missing.length > 0) {
            throw new Error(`Missing required configuration: ${missing.join(', ')}`);
        }
    }

    /**
     * Deploy complete gaming platform from scratch
     */
    async deployCompletePlatform(options = {}) {
        const deployment = {
            id: `deployment-${Date.now()}`,
            timestamp: new Date().toISOString(),
            status: 'STARTED',
            steps: [],
            results: {}
        };

        try {
            console.log('🚀 Starting complete Unit4Productions gaming platform deployment...');
            
            // Step 1: Create GitHub Repository
            deployment.steps.push('Creating GitHub Repository');
            const repo = await this.github.createRepository(
                options.repoName || 'unit4productions-gaming',
                'Unit4Productions Gaming Website - Automated Deployment'
            );
            deployment.results.repository = repo;
            console.log('✅ Repository created successfully');

            // Step 2: Enable GitHub Pages
            deployment.steps.push('Enabling GitHub Pages');
            const pages = await this.github.enableGitHubPages(
                repo.name,
                options.customDomain || 'unit4productions.com'
            );
            deployment.results.githubPages = pages;
            console.log('✅ GitHub Pages enabled');

            // Step 3: Generate and deploy website structure
            deployment.steps.push('Generating Website Templates');
            await this.deployWebsiteStructure(repo.name, options.initialGames || []);
            console.log('✅ Website structure deployed');

            // Step 4: Setup custom domain
            if (options.customDomain) {
                deployment.steps.push('Configuring Custom Domain');
                const domainResult = await this.domains.setupCustomDomain(
                    repo.name,
                    options.customDomain,
                    options.environment || 'production'
                );
                deployment.results.domain = domainResult;
                console.log('✅ Custom domain configured');
            }

            // Step 5: Deploy initial games (if provided)
            if (options.initialGames && options.initialGames.length > 0) {
                deployment.steps.push('Deploying Initial Games');
                const gameResults = await this.gamesPipeline.deployMultipleGames(
                    options.initialGames,
                    repo.name
                );
                deployment.results.games = gameResults;
                console.log(`✅ ${options.initialGames.length} games deployed`);
            }

            // Step 6: Setup monetization
            deployment.steps.push('Integrating Monetization Systems');
            await this.integrateMonetization(repo.name);
            console.log('✅ Monetization systems integrated');

            // Step 7: Wait for deployment and validate
            deployment.steps.push('Monitoring Deployment');
            const deploymentStatus = await this.github.monitorDeployment(repo.name);
            deployment.results.deployment = deploymentStatus;
            
            if (deploymentStatus.success) {
                console.log('✅ Deployment completed successfully');
                
                // Step 8: Run validation tests
                deployment.steps.push('Running Validation Tests');
                const validationResults = await this.testing.runCompleteValidation(
                    options.customDomain || deploymentStatus.url.replace('https://', ''),
                    repo.name
                );
                deployment.results.validation = validationResults;
                console.log(`✅ Validation completed - Score: ${validationResults.overallScore}/100`);
            }

            deployment.status = 'COMPLETED';
            deployment.completedAt = new Date().toISOString();
            
            // Generate deployment report
            const report = this.generateDeploymentReport(deployment);
            await this.saveDeploymentReport(repo.name, report);

            console.log('🎉 Complete gaming platform deployment finished!');
            console.log(`🌐 Website URL: ${deploymentStatus.url}`);
            if (options.customDomain) {
                console.log(`🔗 Custom Domain: https://${options.customDomain}`);
            }

            return deployment;

        } catch (error) {
            deployment.status = 'FAILED';
            deployment.error = error.message;
            deployment.failedAt = new Date().toISOString();
            
            console.error('❌ Deployment failed:', error);
            throw error;
        }
    }

    /**
     * Deploy complete website structure and templates
     */
    async deployWebsiteStructure(repoName, initialGames = []) {
        // Generate all necessary files
        const files = [
            // Main HTML templates
            {
                path: 'index.html',
                content: this.templates.generateMainTemplate(initialGames)
            },
            // CSS styles
            {
                path: 'assets/css/main.css',
                content: this.templates.generateMainCSS()
            },
            // Service Worker for PWA
            {
                path: 'sw.js',
                content: this.templates.generateServiceWorker()
            },
            // Web App Manifest
            {
                path: 'manifest.json',
                content: this.templates.generateManifest()
            },
            // Main JavaScript functionality
            {
                path: 'assets/js/main.js',
                content: this.generateMainJavaScript()
            },
            // Game-specific JavaScript
            {
                path: 'assets/js/games.js',
                content: this.generateGamesJavaScript()
            },
            // Game page template JavaScript
            {
                path: 'assets/js/game-page.js',
                content: this.generateGamePageJavaScript()
            },
            // Analytics JavaScript
            {
                path: 'assets/js/analytics.js',
                content: this.monetization.generateRevenueAnalytics()
            },
            // Game page CSS
            {
                path: 'assets/css/game-page.css',
                content: this.generateGamePageCSS()
            },
            // Games index
            {
                path: 'games/index.json',
                content: JSON.stringify(initialGames, null, 2)
            },
            // 404 page
            {
                path: '404.html',
                content: this.generate404Page()
            },
            // Offline page for PWA
            {
                path: 'offline.html',
                content: this.generateOfflinePage()
            },
            // Robots.txt
            {
                path: 'robots.txt',
                content: this.generateRobotsTxt()
            }
        ];

        // Upload all files
        const uploadResults = await this.github.uploadMultipleFiles(
            repoName,
            files,
            'Deploy complete website structure'
        );

        console.log(`Uploaded ${files.length} website files`);
        return uploadResults;
    }

    /**
     * Generate main JavaScript functionality
     */
    generateMainJavaScript() {
        return `
// Main JavaScript for Unit4Productions Gaming Website
class Unit4GamingWebsite {
    constructor() {
        this.init();
    }

    init() {
        this.setupNavigation();
        this.setupScrollEffects();
        this.setupLoadingScreen();
        this.setupNewsletterForm();
        this.setupLazyLoading();
        this.trackPageView();
    }

    setupNavigation() {
        const navToggle = document.getElementById('nav-toggle');
        const navMenu = document.getElementById('nav-menu');
        
        if (navToggle) {
            navToggle.addEventListener('click', () => {
                navMenu.classList.toggle('active');
                navToggle.classList.toggle('active');
            });
        }

        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth' });
                    navMenu.classList.remove('active');
                    navToggle.classList.remove('active');
                }
            });
        });

        // Navbar scroll effect
        window.addEventListener('scroll', () => {
            const navbar = document.getElementById('navbar');
            if (window.scrollY > 100) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
    }

    setupScrollEffects() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        }, observerOptions);

        document.querySelectorAll('.game-card, .feature, .section-header').forEach(el => {
            observer.observe(el);
        });
    }

    setupLoadingScreen() {
        const loadingScreen = document.getElementById('loading-screen');
        const progressBar = document.getElementById('progress-bar');
        
        if (loadingScreen && progressBar) {
            let progress = 0;
            const interval = setInterval(() => {
                progress += Math.random() * 30;
                progressBar.style.width = Math.min(progress, 100) + '%';
                
                if (progress >= 100) {
                    clearInterval(interval);
                    setTimeout(() => {
                        loadingScreen.classList.add('hidden');
                    }, 500);
                }
            }, 200);
        }
    }

    setupNewsletterForm() {
        const form = document.getElementById('newsletter-form');
        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const email = document.getElementById('newsletter-email').value;
                
                // Track newsletter signup
                if (typeof trackUserEngagement !== 'undefined') {
                    trackUserEngagement('newsletter_signup', email);
                }
                
                // Here you would integrate with your email service
                alert('Thank you for subscribing! We\\'ll keep you updated on new games.');
                form.reset();
            });
        }
    }

    setupLazyLoading() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        observer.unobserve(img);
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
    }

    trackPageView() {
        if (typeof gtag !== 'undefined') {
            gtag('config', 'GA_MEASUREMENT_ID', {
                page_title: document.title,
                page_location: window.location.href
            });
        }
    }
}

// Global utility functions
window.playGame = function(gameId) {
    const gameUrl = \`/games/\${gameId}\`;
    
    // Track game start
    if (typeof trackGameStart !== 'undefined') {
        trackGameStart(gameId, 'browser');
    }
    
    window.location.href = gameUrl;
};

window.favoriteGame = function(gameId) {
    const favorites = JSON.parse(localStorage.getItem('favoriteGames') || '[]');
    
    if (favorites.includes(gameId)) {
        favorites.splice(favorites.indexOf(gameId), 1);
    } else {
        favorites.push(gameId);
    }
    
    localStorage.setItem('favoriteGames', JSON.stringify(favorites));
    
    // Update UI
    const btn = document.querySelector(\`[onclick="favoriteGame('\${gameId}')"]\`);
    if (btn) {
        const icon = btn.querySelector('i');
        if (favorites.includes(gameId)) {
            icon.className = 'fas fa-heart';
            btn.classList.add('favorited');
        } else {
            icon.className = 'far fa-heart';
            btn.classList.remove('favorited');
        }
    }
    
    // Track event
    if (typeof trackUserEngagement !== 'undefined') {
        trackUserEngagement('favorite_game', gameId);
    }
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new Unit4GamingWebsite();
});
`;
    }

    /**
     * Generate games-specific JavaScript
     */
    generateGamesJavaScript() {
        return `
// Games functionality for Unit4Productions
class GamesManager {
    constructor() {
        this.games = [];
        this.currentPage = 1;
        this.gamesPerPage = 12;
        this.init();
    }

    async init() {
        await this.loadGames();
        this.setupFiltering();
        this.setupLoadMore();
        this.setupFeaturedGame();
    }

    async loadGames() {
        try {
            const response = await fetch('/games/index.json');
            this.games = await response.json();
            this.renderGames();
            this.updateFeaturedGame();
        } catch (error) {
            console.error('Failed to load games:', error);
        }
    }

    renderGames() {
        const container = document.getElementById('games-grid');
        if (!container) return;

        const startIndex = 0;
        const endIndex = this.currentPage * this.gamesPerPage;
        const gamesToShow = this.games.slice(startIndex, endIndex);

        container.innerHTML = gamesToShow.map(game => this.createGameCard(game)).join('');
        
        // Update load more button
        const loadMoreBtn = document.getElementById('load-more-games');
        if (loadMoreBtn) {
            loadMoreBtn.style.display = endIndex >= this.games.length ? 'none' : 'block';
        }
    }

    createGameCard(game) {
        const isFavorite = this.isFavoriteGame(game.slug);
        return \`
            <div class="game-card" data-game-id="\${game.slug}">
                <div class="game-image">
                    <img src="\${game.thumbnail || '/assets/images/placeholder-game.jpg'}" 
                         alt="\${game.title}" 
                         loading="lazy">
                    <div class="game-overlay">
                        <button class="play-btn" onclick="playGame('\${game.slug}')">
                            <i class="fas fa-play"></i>
                        </button>
                    </div>
                </div>
                
                <div class="game-info">
                    <h3 class="game-title">\${game.title}</h3>
                    <p class="game-description">\${game.description}</p>
                    
                    <div class="game-stats">
                        <span class="stat">
                            <i class="fas fa-play"></i>
                            \${this.formatNumber(game.plays || 0)}
                        </span>
                        <span class="stat">
                            <i class="fas fa-star"></i>
                            \${game.rating || '0.0'}
                        </span>
                        <span class="stat">
                            <i class="fas fa-clock"></i>
                            \${game.duration || 'N/A'}
                        </span>
                    </div>
                    
                    <div class="game-tags">
                        \${(game.tags || []).map(tag => \`<span class="tag">\${tag}</span>\`).join('')}
                    </div>
                    
                    <div class="game-actions">
                        <a href="/games/\${game.slug}" class="btn btn-primary btn-small">
                            Play Now
                        </a>
                        <button class="btn btn-outline btn-small \${isFavorite ? 'favorited' : ''}" 
                                onclick="favoriteGame('\${game.slug}')">
                            <i class="fas fa-heart"></i>
                        </button>
                    </div>
                </div>
            </div>
        \`;
    }

    setupFiltering() {
        // Tag filtering
        const tagButtons = document.querySelectorAll('.tag-filter');
        tagButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const tag = btn.dataset.tag;
                this.filterByTag(tag);
            });
        });

        // Search functionality
        const searchInput = document.getElementById('games-search');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.searchGames(e.target.value);
            });
        }
    }

    filterByTag(tag) {
        if (tag === 'all') {
            this.renderGames();
            return;
        }

        const filteredGames = this.games.filter(game => 
            game.tags && game.tags.includes(tag)
        );
        
        const container = document.getElementById('games-grid');
        if (container) {
            container.innerHTML = filteredGames.map(game => this.createGameCard(game)).join('');
        }
    }

    searchGames(query) {
        if (!query.trim()) {
            this.renderGames();
            return;
        }

        const filteredGames = this.games.filter(game =>
            game.title.toLowerCase().includes(query.toLowerCase()) ||
            game.description.toLowerCase().includes(query.toLowerCase()) ||
            (game.tags && game.tags.some(tag => tag.toLowerCase().includes(query.toLowerCase())))
        );

        const container = document.getElementById('games-grid');
        if (container) {
            container.innerHTML = filteredGames.map(game => this.createGameCard(game)).join('');
        }
    }

    setupLoadMore() {
        const loadMoreBtn = document.getElementById('load-more-games');
        if (loadMoreBtn) {
            loadMoreBtn.addEventListener('click', () => {
                this.currentPage++;
                this.renderGames();
            });
        }
    }

    setupFeaturedGame() {
        if (this.games.length > 0) {
            // Pick a random featured game or the latest one
            const featured = this.games[0]; // or this.games[Math.floor(Math.random() * this.games.length)];
            this.updateFeaturedGame(featured);
        }
    }

    updateFeaturedGame(game) {
        if (!game) return;

        const nameEl = document.getElementById('featured-name');
        const descEl = document.getElementById('featured-description');
        const playsEl = document.getElementById('featured-plays');
        const ratingEl = document.getElementById('featured-rating');
        const imageEl = document.getElementById('featured-image');
        const playBtn = document.getElementById('featured-play-btn');

        if (nameEl) nameEl.textContent = game.title;
        if (descEl) descEl.textContent = game.description;
        if (playsEl) playsEl.textContent = this.formatNumber(game.plays || 0);
        if (ratingEl) ratingEl.textContent = game.rating || '0.0';
        if (imageEl) imageEl.src = game.thumbnail || '/assets/images/placeholder-game.jpg';
        if (playBtn) playBtn.href = \`/games/\${game.slug}\`;
    }

    formatNumber(num) {
        if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
        if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
        return num.toString();
    }

    isFavoriteGame(gameId) {
        const favorites = JSON.parse(localStorage.getItem('favoriteGames') || '[]');
        return favorites.includes(gameId);
    }
}

// Initialize games manager
document.addEventListener('DOMContentLoaded', () => {
    new GamesManager();
});
`;
    }

    /**
     * Generate game page JavaScript
     */
    generateGamePageJavaScript() {
        return `
// Game Page functionality
class GamePageManager {
    constructor() {
        this.game = window.currentGame || {};
        this.init();
    }

    init() {
        this.setupGameControls();
        this.setupGameLoading();
        this.loadRelatedGames();
        this.trackGameView();
        this.checkPremiumStatus();
    }

    setupGameControls() {
        // Fullscreen toggle
        const fullscreenBtn = document.getElementById('fullscreen-btn');
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', () => this.toggleFullscreen());
        }

        // Mute toggle
        const muteBtn = document.getElementById('mute-btn');
        if (muteBtn) {
            muteBtn.addEventListener('click', () => this.toggleMute());
        }

        // Share button
        const shareBtn = document.getElementById('share-btn');
        if (shareBtn) {
            shareBtn.addEventListener('click', () => this.shareGame());
        }

        // Favorite button
        const favoriteBtn = document.getElementById('favorite-btn');
        if (favoriteBtn) {
            favoriteBtn.addEventListener('click', () => this.toggleFavorite());
        }
    }

    setupGameLoading() {
        const gameFrame = document.getElementById('game-frame-container');
        const loadingDiv = document.getElementById('game-loading');
        const progressBar = document.getElementById('game-progress');

        if (gameFrame && loadingDiv) {
            // Simulate loading progress
            let progress = 0;
            const interval = setInterval(() => {
                progress += Math.random() * 20;
                if (progressBar) {
                    progressBar.style.width = Math.min(progress, 100) + '%';
                }

                if (progress >= 100) {
                    clearInterval(interval);
                    setTimeout(() => {
                        loadingDiv.style.display = 'none';
                        gameFrame.style.display = 'block';
                        this.trackGameStart();
                    }, 500);
                }
            }, 300);
        }
    }

    toggleFullscreen() {
        const gamePlayer = document.getElementById('game-player');
        const fullscreenBtn = document.getElementById('fullscreen-btn');
        
        if (!document.fullscreenElement) {
            gamePlayer.requestFullscreen().then(() => {
                fullscreenBtn.innerHTML = '<i class="fas fa-compress"></i>';
            });
        } else {
            document.exitFullscreen().then(() => {
                fullscreenBtn.innerHTML = '<i class="fas fa-expand"></i>';
            });
        }
    }

    toggleMute() {
        const muteBtn = document.getElementById('mute-btn');
        const iframe = document.querySelector('#game-frame-container iframe');
        
        // This would depend on the game's implementation
        // For now, just toggle the button state
        const icon = muteBtn.querySelector('i');
        if (icon.classList.contains('fa-volume-up')) {
            icon.className = 'fas fa-volume-mute';
        } else {
            icon.className = 'fas fa-volume-up';
        }
    }

    shareGame() {
        if (navigator.share) {
            navigator.share({
                title: this.game.title,
                text: this.game.description,
                url: window.location.href
            });
        } else {
            // Fallback: copy to clipboard
            navigator.clipboard.writeText(window.location.href).then(() => {
                alert('Game URL copied to clipboard!');
            });
        }

        // Track share event
        if (typeof trackUserEngagement !== 'undefined') {
            trackUserEngagement('share_game', this.game.slug);
        }
    }

    toggleFavorite() {
        const favoriteBtn = document.getElementById('favorite-btn');
        favoriteGame(this.game.slug);
        
        // Update button state
        const icon = favoriteBtn.querySelector('i');
        const favorites = JSON.parse(localStorage.getItem('favoriteGames') || '[]');
        
        if (favorites.includes(this.game.slug)) {
            favoriteBtn.classList.add('favorited');
        } else {
            favoriteBtn.classList.remove('favorited');
        }
    }

    async loadRelatedGames() {
        try {
            const response = await fetch('/games/index.json');
            const allGames = await response.json();
            
            // Filter related games by tags or category
            const relatedGames = allGames
                .filter(g => g.slug !== this.game.slug)
                .filter(g => this.game.tags && g.tags && 
                    g.tags.some(tag => this.game.tags.includes(tag)))
                .slice(0, 4);

            const relatedContainer = document.getElementById('related-games');
            if (relatedContainer && relatedGames.length > 0) {
                relatedContainer.innerHTML = relatedGames.map(game => \`
                    <div class="related-game">
                        <a href="/games/\${game.slug}">
                            <img src="\${game.thumbnail || '/assets/images/placeholder-game.jpg'}" 
                                 alt="\${game.title}">
                            <h4>\${game.title}</h4>
                        </a>
                    </div>
                \`).join('');
            }
        } catch (error) {
            console.error('Failed to load related games:', error);
        }
    }

    trackGameView() {
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('game_view', {
                game_name: this.game.title,
                game_type: this.game.type
            });
        }
    }

    trackGameStart() {
        if (typeof trackGameStart !== 'undefined') {
            trackGameStart(this.game.title, this.game.type);
        }
    }

    checkPremiumStatus() {
        if (this.game.premium && !isPremiumUnlocked(this.game.slug)) {
            this.showPremiumGate();
        }
    }

    showPremiumGate() {
        const gameFrame = document.getElementById('game-frame-container');
        if (gameFrame) {
            gameFrame.innerHTML = \`
                <div class="premium-gate">
                    <div class="premium-content">
                        <i class="fas fa-crown"></i>
                        <h3>Premium Game</h3>
                        <p>Unlock "\${this.game.title}" to play this premium gaming experience.</p>
                        <button class="btn btn-primary" onclick="unlockPremiumGame('\${this.game.slug}', '\${this.game.title}')">
                            <i class="fas fa-unlock"></i>
                            Unlock for $\${this.game.price || '4.99'}
                        </button>
                        <p class="premium-note">One-time purchase • Play forever</p>
                    </div>
                </div>
            \`;
        }
    }
}

// Utility functions
window.reloadGame = function() {
    location.reload();
};

window.initializeGame = function() {
    new GamePageManager();
};

// Rating system
window.showRatingModal = function() {
    const modal = document.getElementById('rating-modal');
    if (modal) {
        modal.style.display = 'flex';
    }
};

window.closeModal = function(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
};

window.submitRating = function() {
    const selectedRating = document.querySelector('.star.selected')?.dataset.rating;
    if (selectedRating) {
        // Here you would send the rating to your backend
        console.log(\`Rating submitted: \${selectedRating}\`);
        
        // Track rating event
        if (typeof trackUserEngagement !== 'undefined') {
            trackUserEngagement('rate_game', selectedRating);
        }
        
        closeModal('rating-modal');
        alert('Thank you for rating this game!');
    }
};

// Star rating interaction
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.star').forEach(star => {
        star.addEventListener('click', () => {
            const rating = star.dataset.rating;
            document.querySelectorAll('.star').forEach((s, index) => {
                if (index < rating) {
                    s.classList.add('selected');
                } else {
                    s.classList.remove('selected');
                }
            });
        });
    });
});
`;
    }

    /**
     * Generate game page CSS
     */
    generateGamePageCSS() {
        return `
/* Game Page Specific Styles */
.game-page-body {
    background: var(--background-dark);
    margin: 0;
    padding: 0;
    overflow-x: hidden;
}

.navbar.minimal {
    background: rgba(15, 15, 15, 0.95);
    padding: 0.75rem 0;
}

.nav-actions {
    display: flex;
    gap: 1rem;
    align-items: center;
}

.game-container {
    padding-top: 80px;
    min-height: 100vh;
}

.game-header {
    background: var(--background-card);
    border-bottom: 1px solid var(--border-color);
    padding: 2rem 0;
}

.game-info-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 1rem;
}

.game-title {
    color: var(--text-light);
    font-size: 2rem;
    font-weight: 700;
    margin: 0;
}

.game-meta {
    display: flex;
    gap: 2rem;
    color: var(--text-gray);
    font-size: 0.9rem;
}

.meta-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.meta-item i {
    color: var(--primary-color);
}

.game-content {
    display: grid;
    grid-template-columns: 1fr 300px;
    gap: 2rem;
    padding: 2rem;
    max-width: 1400px;
    margin: 0 auto;
}

.game-player-container {
    display: flex;
    flex-direction: column;
}

.game-player {
    background: #000;
    border-radius: var(--border-radius-large);
    overflow: hidden;
    position: relative;
    aspect-ratio: 16/9;
    min-height: 400px;
}

.game-loading {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: #000;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: var(--text-light);
    gap: 1rem;
}

.loading-spinner {
    width: 50px;
    height: 50px;
    border: 4px solid var(--border-color);
    border-top: 4px solid var(--primary-color);
    border-radius: 50%;
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

.loading-progress {
    width: 200px;
    height: 4px;
    background: var(--border-color);
    border-radius: 2px;
    overflow: hidden;
}

.progress-bar {
    height: 100%;
    background: linear-gradient(90deg, var(--primary-color), var(--accent-color));
    width: 0;
    transition: width 0.3s ease;
}

.game-frame-container {
    width: 100%;
    height: 100%;
}

.game-frame-container iframe {
    width: 100%;
    height: 100%;
    border: none;
}

.game-error {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: var(--background-card);
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: var(--text-light);
    text-align: center;
    gap: 1rem;
}

.game-error i {
    font-size: 3rem;
    color: var(--error-color);
}

.game-controls {
    display: flex;
    gap: 1rem;
    justify-content: center;
    margin-top: 1rem;
}

.control-btn {
    background: var(--background-card);
    border: 1px solid var(--border-color);
    color: var(--text-light);
    padding: 0.75rem;
    border-radius: 50%;
    cursor: pointer;
    transition: var(--transition-fast);
    width: 50px;
    height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.control-btn:hover {
    background: var(--primary-color);
    border-color: var(--primary-color);
}

.control-btn.favorited {
    background: var(--error-color);
    border-color: var(--error-color);
}

.game-sidebar {
    display: flex;
    flex-direction: column;
    gap: 2rem;
}

.game-description-card,
.related-games-card {
    background: var(--background-card);
    border: 1px solid var(--border-color);
    border-radius: var(--border-radius-large);
    padding: 2rem;
}

.game-description-card h3,
.related-games-card h3 {
    color: var(--text-light);
    margin-bottom: 1rem;
}

.game-description-card h4 {
    color: var(--primary-color);
    margin: 1.5rem 0 0.5rem 0;
    font-size: 1rem;
}

.game-instructions {
    background: rgba(255, 107, 53, 0.1);
    padding: 1rem;
    border-radius: var(--border-radius);
    border-left: 4px solid var(--primary-color);
    margin: 0.5rem 0;
}

.game-tags {
    margin-top: 1rem;
}

.game-tags h4 {
    margin-bottom: 0.5rem;
}

.game-tags .tag {
    margin-right: 0.5rem;
    margin-bottom: 0.5rem;
}

.ad-container {
    background: rgba(255, 255, 255, 0.05);
    border-radius: var(--border-radius);
    padding: 1rem;
    text-align: center;
    color: var(--text-gray);
    margin: 1rem 0;
}

.related-games {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.related-game {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 0.75rem;
    background: rgba(255, 255, 255, 0.05);
    border-radius: var(--border-radius);
    transition: var(--transition-fast);
}

.related-game:hover {
    background: rgba(255, 107, 53, 0.1);
}

.related-game img {
    width: 60px;
    height: 45px;
    object-fit: cover;
    border-radius: 4px;
}

.related-game h4 {
    color: var(--text-light);
    font-size: 0.9rem;
    margin: 0;
}

.related-game a {
    display: flex;
    align-items: center;
    gap: 1rem;
    text-decoration: none;
    color: inherit;
    width: 100%;
}

/* Premium Game Gate */
.premium-gate {
    width: 100%;
    height: 100%;
    background: linear-gradient(135deg, var(--accent-color), var(--background-dark));
    display: flex;
    align-items: center;
    justify-content: center;
}

.premium-content {
    text-align: center;
    color: var(--text-light);
    max-width: 400px;
    padding: 2rem;
}

.premium-content i {
    font-size: 4rem;
    color: gold;
    margin-bottom: 1rem;
}

.premium-content h3 {
    font-size: 2rem;
    margin-bottom: 1rem;
    color: gold;
}

.premium-content p {
    margin-bottom: 2rem;
    color: var(--text-gray);
}

.premium-note {
    font-size: 0.8rem;
    color: var(--text-gray);
    margin-top: 1rem;
}

/* Rating Modal */
.modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: none;
    align-items: center;
    justify-content: center;
    z-index: 10000;
}

.modal-content {
    background: var(--background-card);
    padding: 2rem;
    border-radius: var(--border-radius-large);
    text-align: center;
    max-width: 400px;
    width: 90%;
}

.rating-stars {
    display: flex;
    justify-content: center;
    gap: 0.5rem;
    margin: 2rem 0;
}

.star {
    font-size: 2rem;
    color: var(--border-color);
    cursor: pointer;
    transition: var(--transition-fast);
}

.star:hover,
.star.selected {
    color: gold;
}

.modal-actions {
    display: flex;
    gap: 1rem;
    justify-content: center;
    margin-top: 2rem;
}

/* Responsive Design */
@media (max-width: 968px) {
    .game-content {
        grid-template-columns: 1fr;
        gap: 1rem;
        padding: 1rem;
    }
    
    .game-sidebar {
        order: -1;
    }
    
    .game-info-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }
    
    .game-meta {
        flex-wrap: wrap;
        gap: 1rem;
    }
    
    .game-title {
        font-size: 1.5rem;
    }
    
    .nav-actions {
        gap: 0.5rem;
    }
    
    .nav-actions .btn-small {
        font-size: 0.8rem;
        padding: 0.5rem 0.75rem;
    }
}

@media (max-width: 480px) {
    .game-controls {
        gap: 0.5rem;
        flex-wrap: wrap;
    }
    
    .control-btn {
        width: 45px;
        height: 45px;
    }
    
    .premium-content {
        padding: 1rem;
    }
    
    .premium-content i {
        font-size: 3rem;
    }
    
    .premium-content h3 {
        font-size: 1.5rem;
    }
}
`;
    }

    /**
     * Integrate monetization systems
     */
    async integrateMonetization(repoName) {
        const monetizationIntegration = this.monetization.generateCompleteIntegration();
        
        // Update main template with monetization code
        const updatedMainTemplate = this.templates.generateMainTemplate([]);
        await this.github.uploadFile(
            repoName,
            'index.html',
            updatedMainTemplate,
            'Integrate monetization systems'
        );

        // Upload monetization-specific files
        const monetizationFiles = [
            {
                path: 'assets/js/monetization.js',
                content: monetizationIntegration.payments + '\n\n' + monetizationIntegration.subscriptions
            },
            {
                path: 'assets/css/monetization.css',
                content: monetizationIntegration.styles
            }
        ];

        await this.github.uploadMultipleFiles(
            repoName,
            monetizationFiles,
            'Add monetization integration files'
        );

        console.log('Monetization systems integrated');
    }

    /**
     * Generate 404 page
     */
    generate404Page() {
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Page Not Found - Unit4Productions</title>
    <link rel="stylesheet" href="/assets/css/main.css">
</head>
<body>
    <div class="error-page">
        <div class="error-content">
            <h1 class="error-code">404</h1>
            <h2 class="error-title">Game Over!</h2>
            <p class="error-message">The page you're looking for couldn't be found.</p>
            <div class="error-actions">
                <a href="/" class="btn btn-primary">
                    <i class="fas fa-home"></i>
                    Return Home
                </a>
                <a href="/games" class="btn btn-secondary">
                    <i class="fas fa-gamepad"></i>
                    Browse Games
                </a>
            </div>
        </div>
    </div>
    
    <style>
        .error-page {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, var(--background-dark), var(--accent-color));
            text-align: center;
            padding: 2rem;
        }
        
        .error-content {
            max-width: 500px;
        }
        
        .error-code {
            font-size: 8rem;
            font-family: var(--font-primary);
            color: var(--primary-color);
            text-shadow: 0 4px 20px rgba(255, 107, 53, 0.5);
            margin-bottom: 0;
        }
        
        .error-title {
            font-size: 2rem;
            color: var(--text-light);
            margin-bottom: 1rem;
        }
        
        .error-message {
            color: var(--text-gray);
            margin-bottom: 2rem;
            font-size: 1.1rem;
        }
        
        .error-actions {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }
    </style>
</body>
</html>`;
    }

    /**
     * Generate offline page for PWA
     */
    generateOfflinePage() {
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Offline - Unit4Productions</title>
    <link rel="stylesheet" href="/assets/css/main.css">
</head>
<body>
    <div class="offline-page">
        <div class="offline-content">
            <i class="fas fa-wifi-slash offline-icon"></i>
            <h1 class="offline-title">You're Offline</h1>
            <p class="offline-message">It looks like you're not connected to the internet. Check your connection and try again.</p>
            <button class="btn btn-primary" onclick="location.reload()">
                <i class="fas fa-redo"></i>
                Try Again
            </button>
        </div>
    </div>
    
    <style>
        .offline-page {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: var(--background-dark);
            text-align: center;
            padding: 2rem;
        }
        
        .offline-content {
            max-width: 400px;
        }
        
        .offline-icon {
            font-size: 4rem;
            color: var(--text-gray);
            margin-bottom: 2rem;
        }
        
        .offline-title {
            color: var(--text-light);
            margin-bottom: 1rem;
        }
        
        .offline-message {
            color: var(--text-gray);
            margin-bottom: 2rem;
        }
    </style>
</body>
</html>`;
    }

    /**
     * Generate robots.txt
     */
    generateRobotsTxt() {
        return `User-agent: *
Allow: /

# Game files
Allow: /games/
Allow: /assets/

# Important pages
Allow: /sitemap.xml
Allow: /manifest.json

# Disallow admin/private areas
Disallow: /admin/
Disallow: /.git/
Disallow: /reports/

# Sitemaps
Sitemap: ${this.config.siteUrl}/sitemap.xml

# Crawl-delay for respectful crawling
Crawl-delay: 1`;
    }

    /**
     * Deploy a new game to the platform
     */
    async deployGame(gameData, gameFiles, repoName) {
        return this.gamesPipeline.deployGame(gameData, gameFiles, repoName);
    }

    /**
     * Run validation tests on deployed website
     */
    async validateDeployment(domain, repoName) {
        return this.testing.runCompleteValidation(domain, repoName);
    }

    /**
     * Generate deployment report
     */
    generateDeploymentReport(deployment) {
        return {
            deployment: deployment,
            summary: {
                repositoryUrl: deployment.results.repository?.html_url,
                websiteUrl: deployment.results.deployment?.url,
                customDomain: deployment.results.domain?.domain,
                gamesDeployed: deployment.results.games?.length || 0,
                overallScore: deployment.results.validation?.overallScore || 'N/A',
                status: deployment.status
            },
            nextSteps: this.generateNextSteps(deployment),
            troubleshooting: this.generateTroubleshootingGuide()
        };
    }

    /**
     * Generate next steps for after deployment
     */
    generateNextSteps(deployment) {
        const steps = [
            'Configure DNS records with your domain provider',
            'Wait for SSL certificate to be issued (up to 24 hours)',
            'Upload game files using the deployment pipeline',
            'Configure Google Analytics and AdSense accounts',
            'Test website functionality across different devices'
        ];

        if (deployment.results.validation?.overallScore < 80) {
            steps.push('Review validation report and fix identified issues');
        }

        return steps;
    }

    /**
     * Generate troubleshooting guide
     */
    generateTroubleshootingGuide() {
        return {
            commonIssues: [
                {
                    issue: 'Custom domain not working',
                    solution: 'Check DNS records and wait up to 48 hours for propagation'
                },
                {
                    issue: 'SSL certificate pending',
                    solution: 'Wait up to 24 hours for GitHub to issue certificate'
                },
                {
                    issue: 'Games not loading',
                    solution: 'Check game file paths and ensure proper HTML5 structure'
                },
                {
                    issue: 'Analytics not tracking',
                    solution: 'Verify Google Analytics ID is correctly configured'
                }
            ],
            supportResources: [
                'GitHub Pages Documentation: https://docs.github.com/en/pages',
                'Google Analytics Setup: https://analytics.google.com',
                'AdSense Help Center: https://support.google.com/adsense'
            ]
        };
    }

    /**
     * Save deployment report to repository
     */
    async saveDeploymentReport(repoName, report) {
        const reportContent = JSON.stringify(report, null, 2);
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        
        await this.github.uploadFile(
            repoName,
            `reports/deployment-report-${timestamp}.json`,
            reportContent,
            'Add deployment report'
        );

        console.log('Deployment report saved to repository');
    }
}

module.exports = Unit4GamingPlatform;