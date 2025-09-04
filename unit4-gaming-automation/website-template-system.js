/**
 * Website Template System for Unit4Productions Gaming Website
 * Generates complete gaming website templates with responsive design
 */

class WebsiteTemplateSystem {
    constructor(config = {}) {
        this.siteName = config.siteName || 'Unit4Productions';
        this.siteUrl = config.siteUrl || 'https://unit4productions.com';
        this.description = config.description || 'Premium Gaming Experiences by Unit4Productions';
        this.primaryColor = config.primaryColor || '#FF6B35';
        this.secondaryColor = config.secondaryColor || '#1A1A2E';
        this.accentColor = config.accentColor || '#16213E';
        this.analyticsId = config.analyticsId || '';
        this.adsenseId = config.adsenseId || '';
    }

    /**
     * Generate the main index.html template
     */
    generateMainTemplate(games = []) {
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="${this.description}">
    <meta name="keywords" content="games, gaming, browser games, free games, online games, Unit4Productions">
    <meta name="author" content="Unit4Productions">
    
    <!-- Open Graph Meta Tags -->
    <meta property="og:title" content="${this.siteName}">
    <meta property="og:description" content="${this.description}">
    <meta property="og:url" content="${this.siteUrl}">
    <meta property="og:type" content="website">
    <meta property="og:image" content="${this.siteUrl}/assets/images/og-image.jpg">
    
    <!-- Twitter Card Meta Tags -->
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="${this.siteName}">
    <meta name="twitter:description" content="${this.description}">
    <meta name="twitter:image" content="${this.siteUrl}/assets/images/twitter-card.jpg">
    
    <title>${this.siteName} - Premium Gaming Experiences</title>
    <link rel="canonical" href="${this.siteUrl}">
    <link rel="icon" type="image/x-icon" href="/favicon.ico">
    <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png">
    
    <!-- Preconnect to external resources -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link rel="preconnect" href="https://www.google-analytics.com">
    
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Orbitron:wght@400;700;900&family=Exo+2:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <!-- Main Styles -->
    <link rel="stylesheet" href="/assets/css/main.css">
    
    ${this.generateAnalyticsCode()}
    ${this.generateAdsenseCode()}
</head>
<body>
    <!-- Navigation -->
    <nav class="navbar" id="navbar">
        <div class="nav-container">
            <div class="nav-logo">
                <a href="/" aria-label="Unit4Productions Home">
                    <img src="/assets/images/logo.svg" alt="Unit4Productions Logo" class="logo">
                    <span class="logo-text">${this.siteName}</span>
                </a>
            </div>
            
            <div class="nav-menu" id="nav-menu">
                <a href="#home" class="nav-link">Home</a>
                <a href="#games" class="nav-link">Games</a>
                <a href="#about" class="nav-link">About</a>
                <a href="#contact" class="nav-link">Contact</a>
            </div>
            
            <div class="nav-toggle" id="nav-toggle">
                <span class="bar"></span>
                <span class="bar"></span>
                <span class="bar"></span>
            </div>
        </div>
    </nav>

    <!-- Hero Section -->
    <section id="home" class="hero">
        <div class="hero-background">
            <div class="hero-overlay"></div>
            <video autoplay muted loop class="hero-video">
                <source src="/assets/videos/hero-background.mp4" type="video/mp4">
            </video>
        </div>
        
        <div class="hero-content">
            <h1 class="hero-title">
                <span class="title-main">UNIT4</span>
                <span class="title-sub">PRODUCTIONS</span>
            </h1>
            <p class="hero-description">Where Premium Gaming Experiences Come to Life</p>
            <div class="hero-actions">
                <a href="#games" class="btn btn-primary">
                    <i class="fas fa-gamepad"></i>
                    Play Now
                </a>
                <a href="#about" class="btn btn-secondary">
                    <i class="fas fa-info-circle"></i>
                    Learn More
                </a>
            </div>
        </div>
        
        <div class="scroll-indicator">
            <div class="scroll-arrow"></div>
        </div>
    </section>

    <!-- Games Section -->
    <section id="games" class="games-section">
        <div class="container">
            <div class="section-header">
                <h2 class="section-title">Our Games</h2>
                <p class="section-description">Discover our collection of premium gaming experiences</p>
            </div>
            
            <div class="games-grid" id="games-grid">
                ${this.generateGamesGrid(games)}
            </div>
            
            <!-- Load More Games -->
            <div class="load-more-container">
                <button class="btn btn-outline" id="load-more-games">
                    <i class="fas fa-plus"></i>
                    Load More Games
                </button>
            </div>
        </div>
    </section>

    <!-- Featured Game Section -->
    <section class="featured-game" id="featured-game">
        <div class="container">
            <div class="featured-content">
                <div class="featured-info">
                    <h2 class="featured-title">Featured Game</h2>
                    <h3 class="featured-name" id="featured-name">Loading...</h3>
                    <p class="featured-description" id="featured-description">Loading...</p>
                    <div class="featured-stats">
                        <div class="stat">
                            <i class="fas fa-play"></i>
                            <span id="featured-plays">0</span> Plays
                        </div>
                        <div class="stat">
                            <i class="fas fa-star"></i>
                            <span id="featured-rating">0</span> Rating
                        </div>
                    </div>
                    <a href="#" class="btn btn-primary" id="featured-play-btn">
                        <i class="fas fa-gamepad"></i>
                        Play Now
                    </a>
                </div>
                <div class="featured-preview">
                    <div class="preview-container">
                        <img src="/assets/images/placeholder-game.jpg" alt="Featured Game" id="featured-image">
                        <div class="play-overlay">
                            <i class="fas fa-play"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- About Section -->
    <section id="about" class="about-section">
        <div class="container">
            <div class="about-content">
                <div class="about-text">
                    <h2 class="section-title">About Unit4Productions</h2>
                    <p>We are passionate game developers dedicated to creating immersive, high-quality gaming experiences that captivate and entertain players worldwide.</p>
                    <p>Our team combines cutting-edge technology with creative storytelling to deliver games that push the boundaries of what's possible in browser-based gaming.</p>
                    
                    <div class="features-grid">
                        <div class="feature">
                            <i class="fas fa-rocket"></i>
                            <h3>Innovation First</h3>
                            <p>Pushing gaming boundaries with latest technologies</p>
                        </div>
                        <div class="feature">
                            <i class="fas fa-users"></i>
                            <h3>Community Focused</h3>
                            <p>Building games that bring people together</p>
                        </div>
                        <div class="feature">
                            <i class="fas fa-shield-alt"></i>
                            <h3>Quality Assured</h3>
                            <p>Every game tested for optimal performance</p>
                        </div>
                        <div class="feature">
                            <i class="fas fa-mobile-alt"></i>
                            <h3>Mobile Ready</h3>
                            <p>Play anywhere, anytime, on any device</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Newsletter Section -->
    <section class="newsletter-section">
        <div class="container">
            <div class="newsletter-content">
                <h2>Stay Updated</h2>
                <p>Get notified about new games, updates, and exclusive content</p>
                <form class="newsletter-form" id="newsletter-form">
                    <div class="form-group">
                        <input type="email" placeholder="Enter your email" required id="newsletter-email">
                        <button type="submit" class="btn btn-primary">
                            <i class="fas fa-paper-plane"></i>
                            Subscribe
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
        <div class="container">
            <div class="footer-content">
                <div class="footer-section">
                    <h3>Unit4Productions</h3>
                    <p>Creating premium gaming experiences that captivate and inspire.</p>
                    <div class="social-links">
                        <a href="#" aria-label="Twitter"><i class="fab fa-twitter"></i></a>
                        <a href="#" aria-label="Discord"><i class="fab fa-discord"></i></a>
                        <a href="#" aria-label="YouTube"><i class="fab fa-youtube"></i></a>
                        <a href="#" aria-label="GitHub"><i class="fab fa-github"></i></a>
                    </div>
                </div>
                
                <div class="footer-section">
                    <h4>Games</h4>
                    <ul>
                        <li><a href="/games">All Games</a></li>
                        <li><a href="/games/latest">Latest Releases</a></li>
                        <li><a href="/games/popular">Most Popular</a></li>
                        <li><a href="/games/categories">Categories</a></li>
                    </ul>
                </div>
                
                <div class="footer-section">
                    <h4>Company</h4>
                    <ul>
                        <li><a href="/about">About Us</a></li>
                        <li><a href="/careers">Careers</a></li>
                        <li><a href="/contact">Contact</a></li>
                        <li><a href="/press">Press Kit</a></li>
                    </ul>
                </div>
                
                <div class="footer-section">
                    <h4>Support</h4>
                    <ul>
                        <li><a href="/help">Help Center</a></li>
                        <li><a href="/privacy">Privacy Policy</a></li>
                        <li><a href="/terms">Terms of Service</a></li>
                        <li><a href="/dmca">DMCA</a></li>
                    </ul>
                </div>
            </div>
            
            <div class="footer-bottom">
                <p>&copy; 2024 Unit4Productions. All rights reserved.</p>
                <p>Made with <i class="fas fa-heart"></i> for gamers worldwide</p>
            </div>
        </div>
    </footer>

    <!-- Loading Screen -->
    <div class="loading-screen" id="loading-screen">
        <div class="loading-content">
            <div class="loading-logo">
                <img src="/assets/images/logo.svg" alt="Unit4Productions">
            </div>
            <div class="loading-progress">
                <div class="progress-bar" id="progress-bar"></div>
            </div>
            <p class="loading-text">Loading Gaming Experience...</p>
        </div>
    </div>

    <!-- Scripts -->
    <script src="/assets/js/main.js"></script>
    <script src="/assets/js/games.js"></script>
    <script src="/assets/js/analytics.js"></script>
    
    <!-- Service Worker Registration -->
    <script>
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => console.log('SW registered:', registration))
                .catch(error => console.log('SW registration failed:', error));
        }
    </script>
</body>
</html>`;
    }

    /**
     * Generate games grid HTML
     */
    generateGamesGrid(games) {
        if (!games || games.length === 0) {
            return `
                <div class="no-games">
                    <i class="fas fa-gamepad"></i>
                    <h3>Games Coming Soon</h3>
                    <p>We're working hard to bring you amazing gaming experiences. Check back soon!</p>
                </div>
            `;
        }

        return games.map(game => `
            <div class="game-card" data-game-id="${game.id}">
                <div class="game-image">
                    <img src="${game.thumbnail || '/assets/images/placeholder-game.jpg'}" 
                         alt="${game.title}" 
                         loading="lazy">
                    <div class="game-overlay">
                        <button class="play-btn" onclick="playGame('${game.id}')">
                            <i class="fas fa-play"></i>
                        </button>
                    </div>
                </div>
                
                <div class="game-info">
                    <h3 class="game-title">${game.title}</h3>
                    <p class="game-description">${game.description}</p>
                    
                    <div class="game-stats">
                        <span class="stat">
                            <i class="fas fa-play"></i>
                            ${game.plays || 0}
                        </span>
                        <span class="stat">
                            <i class="fas fa-star"></i>
                            ${game.rating || '0.0'}
                        </span>
                        <span class="stat">
                            <i class="fas fa-clock"></i>
                            ${game.duration || 'N/A'}
                        </span>
                    </div>
                    
                    <div class="game-tags">
                        ${(game.tags || []).map(tag => `<span class="tag">${tag}</span>`).join('')}
                    </div>
                    
                    <div class="game-actions">
                        <a href="/games/${game.slug}" class="btn btn-primary btn-small">
                            Play Now
                        </a>
                        <button class="btn btn-outline btn-small" onclick="favoriteGame('${game.id}')">
                            <i class="fas fa-heart"></i>
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    }

    /**
     * Generate individual game page template
     */
    generateGameTemplate(game) {
        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="${game.description}">
    <meta name="keywords" content="${game.tags ? game.tags.join(', ') : 'game, browser game, online game'}">
    
    <meta property="og:title" content="${game.title} - ${this.siteName}">
    <meta property="og:description" content="${game.description}">
    <meta property="og:url" content="${this.siteUrl}/games/${game.slug}">
    <meta property="og:type" content="website">
    <meta property="og:image" content="${game.thumbnail || this.siteUrl + '/assets/images/og-image.jpg'}">
    
    <title>${game.title} - Play Free Online | ${this.siteName}</title>
    <link rel="canonical" href="${this.siteUrl}/games/${game.slug}">
    
    <link rel="stylesheet" href="/assets/css/main.css">
    <link rel="stylesheet" href="/assets/css/game-page.css">
    
    ${this.generateAnalyticsCode()}
    ${this.generateAdsenseCode()}
</head>
<body class="game-page-body">
    <!-- Navigation -->
    <nav class="navbar minimal">
        <div class="nav-container">
            <div class="nav-logo">
                <a href="/">
                    <img src="/assets/images/logo.svg" alt="Unit4Productions Logo" class="logo">
                    <span class="logo-text">${this.siteName}</span>
                </a>
            </div>
            
            <div class="nav-actions">
                <button class="btn btn-outline btn-small" onclick="toggleFullscreen()">
                    <i class="fas fa-expand"></i>
                    Fullscreen
                </button>
                <a href="/" class="btn btn-secondary btn-small">
                    <i class="fas fa-home"></i>
                    Home
                </a>
            </div>
        </div>
    </nav>

    <!-- Game Container -->
    <main class="game-container">
        <div class="game-header">
            <div class="container">
                <div class="game-info-header">
                    <h1 class="game-title">${game.title}</h1>
                    <div class="game-meta">
                        <span class="meta-item">
                            <i class="fas fa-play"></i>
                            <span id="play-count">${game.plays || 0}</span> plays
                        </span>
                        <span class="meta-item">
                            <i class="fas fa-star"></i>
                            <span id="rating">${game.rating || '0.0'}</span> rating
                        </span>
                        <span class="meta-item">
                            <i class="fas fa-clock"></i>
                            ${game.duration || 'Variable'}
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <div class="game-content">
            <div class="game-player-container">
                <div class="game-player" id="game-player">
                    <div class="game-loading" id="game-loading">
                        <div class="loading-spinner"></div>
                        <p>Loading ${game.title}...</p>
                        <div class="loading-progress">
                            <div class="progress-bar" id="game-progress"></div>
                        </div>
                    </div>
                    
                    <div class="game-frame-container" id="game-frame-container" style="display: none;">
                        ${this.generateGameEmbed(game)}
                    </div>
                    
                    <div class="game-error" id="game-error" style="display: none;">
                        <i class="fas fa-exclamation-triangle"></i>
                        <h3>Game Loading Error</h3>
                        <p>Sorry, we're having trouble loading this game. Please try refreshing the page.</p>
                        <button class="btn btn-primary" onclick="reloadGame()">
                            <i class="fas fa-redo"></i>
                            Retry
                        </button>
                    </div>
                </div>

                <div class="game-controls">
                    <button class="control-btn" onclick="toggleMute()" id="mute-btn">
                        <i class="fas fa-volume-up"></i>
                    </button>
                    <button class="control-btn" onclick="toggleFullscreen()" id="fullscreen-btn">
                        <i class="fas fa-expand"></i>
                    </button>
                    <button class="control-btn" onclick="shareGame()" id="share-btn">
                        <i class="fas fa-share"></i>
                    </button>
                    <button class="control-btn" onclick="favoriteGame()" id="favorite-btn">
                        <i class="fas fa-heart"></i>
                    </button>
                </div>
            </div>

            <div class="game-sidebar">
                <div class="game-description-card">
                    <h3>About This Game</h3>
                    <p>${game.description}</p>
                    
                    ${game.instructions ? `
                    <h4>How to Play</h4>
                    <div class="game-instructions">
                        ${game.instructions}
                    </div>
                    ` : ''}
                    
                    ${game.tags && game.tags.length > 0 ? `
                    <div class="game-tags">
                        <h4>Tags</h4>
                        ${game.tags.map(tag => `<span class="tag">${tag}</span>`).join('')}
                    </div>
                    ` : ''}
                </div>

                <!-- Ad Space -->
                <div class="ad-container">
                    <div class="ad-placeholder">
                        <p>Advertisement</p>
                    </div>
                </div>

                <div class="related-games-card">
                    <h3>More Games</h3>
                    <div class="related-games" id="related-games">
                        <!-- Populated by JavaScript -->
                    </div>
                </div>
            </div>
        </div>
    </main>

    <!-- Game Rating Modal -->
    <div class="modal" id="rating-modal">
        <div class="modal-content">
            <h3>Rate This Game</h3>
            <div class="rating-stars">
                <span class="star" data-rating="1">★</span>
                <span class="star" data-rating="2">★</span>
                <span class="star" data-rating="3">★</span>
                <span class="star" data-rating="4">★</span>
                <span class="star" data-rating="5">★</span>
            </div>
            <div class="modal-actions">
                <button class="btn btn-secondary" onclick="closeModal('rating-modal')">Cancel</button>
                <button class="btn btn-primary" onclick="submitRating()">Submit</button>
            </div>
        </div>
    </div>

    <script src="/assets/js/main.js"></script>
    <script src="/assets/js/game-page.js"></script>
    <script>
        // Initialize game with metadata
        window.currentGame = ${JSON.stringify(game)};
        initializeGame();
    </script>
</body>
</html>`;
    }

    /**
     * Generate game embed code
     */
    generateGameEmbed(game) {
        switch (game.type) {
            case 'html5':
                return `<iframe 
                    src="/games/${game.slug}/index.html" 
                    width="100%" 
                    height="100%" 
                    frameborder="0" 
                    allowfullscreen
                    allow="autoplay; encrypted-media; gamepad">
                </iframe>`;
                
            case 'unity':
                return `<div id="unity-container" class="unity-desktop">
                    <canvas id="unity-canvas" width="800" height="600"></canvas>
                    <div id="unity-loading-bar">
                        <div id="unity-logo"></div>
                        <div id="unity-progress-bar-empty">
                            <div id="unity-progress-bar-full"></div>
                        </div>
                    </div>
                </div>
                <script src="/games/${game.slug}/Build/UnityLoader.js"></script>`;
                
            case 'flash':
                return `<div class="flash-warning">
                    <i class="fas fa-exclamation-triangle"></i>
                    <h4>Flash Game</h4>
                    <p>This game requires Flash Player. Please enable Flash or try our HTML5 games.</p>
                </div>`;
                
            default:
                return `<iframe 
                    src="/games/${game.slug}/index.html" 
                    width="100%" 
                    height="100%" 
                    frameborder="0" 
                    allowfullscreen>
                </iframe>`;
        }
    }

    /**
     * Generate analytics code
     */
    generateAnalyticsCode() {
        if (!this.analyticsId) return '';
        
        return `
        <!-- Google Analytics -->
        <script async src="https://www.googletagmanager.com/gtag/js?id=${this.analyticsId}"></script>
        <script>
            window.dataLayer = window.dataLayer || [];
            function gtag(){dataLayer.push(arguments);}
            gtag('js', new Date());
            gtag('config', '${this.analyticsId}');
        </script>`;
    }

    /**
     * Generate AdSense code
     */
    generateAdsenseCode() {
        if (!this.adsenseId) return '';
        
        return `
        <!-- Google AdSense -->
        <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${this.adsenseId}" crossorigin="anonymous"></script>`;
    }

    /**
     * Generate CSS styles
     */
    generateMainCSS() {
        return `/* Unit4Productions Gaming Website Styles */

/* CSS Custom Properties */
:root {
    --primary-color: ${this.primaryColor};
    --secondary-color: ${this.secondaryColor};
    --accent-color: ${this.accentColor};
    --text-light: #ffffff;
    --text-dark: #333333;
    --text-gray: #666666;
    --background-dark: #0f0f0f;
    --background-card: #1a1a1a;
    --border-color: #333333;
    --success-color: #00d084;
    --warning-color: #ff9500;
    --error-color: #ff3b30;
    
    --font-primary: 'Orbitron', 'Segoe UI', sans-serif;
    --font-secondary: 'Exo 2', 'Segoe UI', sans-serif;
    
    --border-radius: 8px;
    --border-radius-large: 16px;
    --box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    --box-shadow-hover: 0 8px 30px rgba(255, 107, 53, 0.3);
    
    --transition-fast: 0.2s ease;
    --transition-normal: 0.3s ease;
    --transition-slow: 0.5s ease;
}

/* Reset and Base Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

html {
    scroll-behavior: smooth;
    font-size: 16px;
}

body {
    font-family: var(--font-secondary);
    background: var(--background-dark);
    color: var(--text-light);
    line-height: 1.6;
    overflow-x: hidden;
}

/* Typography */
h1, h2, h3, h4, h5, h6 {
    font-family: var(--font-primary);
    font-weight: 700;
    line-height: 1.2;
    margin-bottom: 1rem;
}

h1 { font-size: clamp(2rem, 5vw, 3.5rem); }
h2 { font-size: clamp(1.75rem, 4vw, 2.5rem); }
h3 { font-size: clamp(1.25rem, 3vw, 1.75rem); }

p {
    margin-bottom: 1rem;
    color: var(--text-gray);
}

a {
    color: var(--primary-color);
    text-decoration: none;
    transition: var(--transition-fast);
}

a:hover {
    color: var(--text-light);
}

/* Layout Components */
.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 2rem;
}

.section-header {
    text-align: center;
    margin-bottom: 4rem;
}

.section-title {
    color: var(--text-light);
    position: relative;
    display: inline-block;
}

.section-title::after {
    content: '';
    position: absolute;
    bottom: -10px;
    left: 50%;
    transform: translateX(-50%);
    width: 60px;
    height: 3px;
    background: linear-gradient(90deg, var(--primary-color), var(--accent-color));
    border-radius: 2px;
}

.section-description {
    font-size: 1.1rem;
    color: var(--text-gray);
    max-width: 600px;
    margin: 0 auto;
}

/* Button Styles */
.btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.75rem 1.5rem;
    font-family: var(--font-secondary);
    font-weight: 600;
    text-decoration: none;
    border: none;
    border-radius: var(--border-radius);
    cursor: pointer;
    transition: var(--transition-normal);
    font-size: 1rem;
    text-align: center;
    position: relative;
    overflow: hidden;
}

.btn::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.1), transparent);
    transition: var(--transition-normal);
}

.btn:hover::before {
    left: 100%;
}

.btn-primary {
    background: linear-gradient(135deg, var(--primary-color), var(--accent-color));
    color: var(--text-light);
    box-shadow: var(--box-shadow);
}

.btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: var(--box-shadow-hover);
}

.btn-secondary {
    background: transparent;
    color: var(--text-light);
    border: 2px solid var(--primary-color);
}

.btn-secondary:hover {
    background: var(--primary-color);
    color: var(--text-light);
}

.btn-outline {
    background: transparent;
    color: var(--primary-color);
    border: 1px solid var(--primary-color);
}

.btn-outline:hover {
    background: var(--primary-color);
    color: var(--text-light);
}

.btn-small {
    padding: 0.5rem 1rem;
    font-size: 0.9rem;
}

/* Navigation */
.navbar {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    background: rgba(15, 15, 15, 0.95);
    backdrop-filter: blur(10px);
    border-bottom: 1px solid var(--border-color);
    z-index: 1000;
    transition: var(--transition-normal);
}

.navbar.scrolled {
    background: rgba(15, 15, 15, 0.98);
    box-shadow: var(--box-shadow);
}

.nav-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1rem 2rem;
    max-width: 1200px;
    margin: 0 auto;
}

.nav-logo {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.logo {
    height: 40px;
    width: auto;
}

.logo-text {
    font-family: var(--font-primary);
    font-size: 1.5rem;
    font-weight: 900;
    color: var(--primary-color);
}

.nav-menu {
    display: flex;
    gap: 2rem;
}

.nav-link {
    color: var(--text-light);
    font-weight: 500;
    position: relative;
    transition: var(--transition-fast);
}

.nav-link::after {
    content: '';
    position: absolute;
    bottom: -5px;
    left: 0;
    width: 0;
    height: 2px;
    background: var(--primary-color);
    transition: var(--transition-fast);
}

.nav-link:hover::after {
    width: 100%;
}

.nav-toggle {
    display: none;
    flex-direction: column;
    cursor: pointer;
}

.bar {
    width: 25px;
    height: 3px;
    background: var(--text-light);
    margin: 3px 0;
    transition: var(--transition-fast);
}

/* Hero Section */
.hero {
    position: relative;
    height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
}

.hero-background {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -2;
}

.hero-video {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.hero-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(135deg, rgba(255, 107, 53, 0.3), rgba(26, 26, 46, 0.7));
    z-index: -1;
}

.hero-content {
    text-align: center;
    max-width: 800px;
    padding: 0 2rem;
    z-index: 1;
}

.hero-title {
    margin-bottom: 1.5rem;
}

.title-main {
    display: block;
    font-size: clamp(3rem, 8vw, 6rem);
    font-weight: 900;
    letter-spacing: 0.1em;
    color: var(--primary-color);
    text-shadow: 0 4px 20px rgba(255, 107, 53, 0.5);
}

.title-sub {
    display: block;
    font-size: clamp(1.5rem, 4vw, 3rem);
    font-weight: 400;
    letter-spacing: 0.2em;
    color: var(--text-light);
    margin-top: -0.5rem;
}

.hero-description {
    font-size: 1.25rem;
    color: var(--text-light);
    margin-bottom: 2rem;
    text-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
}

.hero-actions {
    display: flex;
    gap: 1rem;
    justify-content: center;
    flex-wrap: wrap;
}

.scroll-indicator {
    position: absolute;
    bottom: 2rem;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    color: var(--text-light);
    cursor: pointer;
    animation: bounce 2s infinite;
}

.scroll-arrow {
    width: 20px;
    height: 20px;
    border: 2px solid var(--primary-color);
    border-top: none;
    border-left: none;
    transform: rotate(45deg);
}

@keyframes bounce {
    0%, 20%, 50%, 80%, 100% { transform: translateX(-50%) translateY(0); }
    40% { transform: translateX(-50%) translateY(-10px); }
    60% { transform: translateX(-50%) translateY(-5px); }
}

/* Games Section */
.games-section {
    padding: 6rem 0;
    background: linear-gradient(180deg, var(--background-dark), var(--background-card));
}

.games-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 2rem;
    margin-bottom: 3rem;
}

.game-card {
    background: var(--background-card);
    border-radius: var(--border-radius-large);
    overflow: hidden;
    transition: var(--transition-normal);
    border: 1px solid var(--border-color);
    position: relative;
}

.game-card:hover {
    transform: translateY(-5px);
    box-shadow: var(--box-shadow-hover);
}

.game-image {
    position: relative;
    width: 100%;
    height: 200px;
    overflow: hidden;
}

.game-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: var(--transition-normal);
}

.game-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: var(--transition-normal);
}

.game-card:hover .game-overlay {
    opacity: 1;
}

.play-btn {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    background: var(--primary-color);
    border: none;
    color: white;
    font-size: 1.5rem;
    cursor: pointer;
    transition: var(--transition-fast);
}

.play-btn:hover {
    transform: scale(1.1);
    background: var(--accent-color);
}

.game-info {
    padding: 1.5rem;
}

.game-title {
    color: var(--text-light);
    font-size: 1.25rem;
    margin-bottom: 0.5rem;
}

.game-description {
    color: var(--text-gray);
    font-size: 0.9rem;
    margin-bottom: 1rem;
    line-height: 1.5;
}

.game-stats {
    display: flex;
    gap: 1rem;
    margin-bottom: 1rem;
    font-size: 0.8rem;
}

.stat {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    color: var(--text-gray);
}

.stat i {
    color: var(--primary-color);
}

.game-tags {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
    margin-bottom: 1rem;
}

.tag {
    background: var(--accent-color);
    color: var(--text-light);
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.7rem;
    font-weight: 500;
}

.game-actions {
    display: flex;
    gap: 0.5rem;
    align-items: center;
}

.no-games {
    grid-column: 1 / -1;
    text-align: center;
    padding: 4rem 2rem;
    color: var(--text-gray);
}

.no-games i {
    font-size: 4rem;
    color: var(--primary-color);
    margin-bottom: 1rem;
}

/* Featured Game Section */
.featured-game {
    padding: 6rem 0;
    background: var(--background-card);
}

.featured-content {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 4rem;
    align-items: center;
}

.featured-info h2 {
    color: var(--primary-color);
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
}

.featured-name {
    font-size: 2.5rem;
    color: var(--text-light);
    margin-bottom: 1rem;
}

.featured-description {
    color: var(--text-gray);
    margin-bottom: 2rem;
    line-height: 1.6;
}

.featured-stats {
    display: flex;
    gap: 2rem;
    margin-bottom: 2rem;
}

.preview-container {
    position: relative;
    border-radius: var(--border-radius-large);
    overflow: hidden;
    cursor: pointer;
    transition: var(--transition-normal);
}

.preview-container:hover {
    transform: scale(1.02);
    box-shadow: var(--box-shadow-hover);
}

.preview-container img {
    width: 100%;
    height: auto;
    display: block;
}

.play-overlay {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 80px;
    height: 80px;
    background: rgba(255, 107, 53, 0.9);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2rem;
    color: white;
    transition: var(--transition-normal);
}

.preview-container:hover .play-overlay {
    background: var(--primary-color);
    transform: translate(-50%, -50%) scale(1.1);
}

/* About Section */
.about-section {
    padding: 6rem 0;
    background: var(--background-dark);
}

.about-content {
    max-width: 800px;
    margin: 0 auto;
    text-align: center;
}

.about-content p {
    font-size: 1.1rem;
    margin-bottom: 1.5rem;
    color: var(--text-gray);
}

.features-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 2rem;
    margin-top: 3rem;
}

.feature {
    text-align: center;
    padding: 2rem 1rem;
    background: var(--background-card);
    border-radius: var(--border-radius-large);
    transition: var(--transition-normal);
}

.feature:hover {
    transform: translateY(-5px);
    box-shadow: var(--box-shadow);
}

.feature i {
    font-size: 2.5rem;
    color: var(--primary-color);
    margin-bottom: 1rem;
}

.feature h3 {
    color: var(--text-light);
    margin-bottom: 0.5rem;
}

.feature p {
    color: var(--text-gray);
    font-size: 0.9rem;
}

/* Newsletter Section */
.newsletter-section {
    padding: 4rem 0;
    background: linear-gradient(135deg, var(--primary-color), var(--accent-color));
}

.newsletter-content {
    text-align: center;
    max-width: 600px;
    margin: 0 auto;
}

.newsletter-content h2 {
    color: var(--text-light);
    margin-bottom: 0.5rem;
}

.newsletter-content p {
    color: rgba(255, 255, 255, 0.9);
    margin-bottom: 2rem;
}

.newsletter-form {
    max-width: 400px;
    margin: 0 auto;
}

.form-group {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
}

.form-group input {
    flex: 1;
    min-width: 200px;
    padding: 0.75rem;
    border: none;
    border-radius: var(--border-radius);
    font-size: 1rem;
    background: rgba(255, 255, 255, 0.1);
    color: var(--text-light);
    backdrop-filter: blur(10px);
}

.form-group input::placeholder {
    color: rgba(255, 255, 255, 0.7);
}

.form-group input:focus {
    outline: none;
    background: rgba(255, 255, 255, 0.2);
}

/* Footer */
.footer {
    background: var(--background-card);
    padding: 3rem 0 1rem;
    border-top: 1px solid var(--border-color);
}

.footer-content {
    display: grid;
    grid-template-columns: 2fr 1fr 1fr 1fr;
    gap: 2rem;
    margin-bottom: 2rem;
}

.footer-section h3 {
    color: var(--primary-color);
    margin-bottom: 1rem;
}

.footer-section h4 {
    color: var(--text-light);
    margin-bottom: 1rem;
    font-size: 1rem;
}

.footer-section p {
    color: var(--text-gray);
    margin-bottom: 1rem;
}

.footer-section ul {
    list-style: none;
}

.footer-section ul li {
    margin-bottom: 0.5rem;
}

.footer-section ul li a {
    color: var(--text-gray);
    transition: var(--transition-fast);
}

.footer-section ul li a:hover {
    color: var(--primary-color);
}

.social-links {
    display: flex;
    gap: 1rem;
    margin-top: 1rem;
}

.social-links a {
    width: 40px;
    height: 40px;
    background: var(--primary-color);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    transition: var(--transition-fast);
}

.social-links a:hover {
    background: var(--accent-color);
    transform: translateY(-2px);
}

.footer-bottom {
    text-align: center;
    padding-top: 2rem;
    border-top: 1px solid var(--border-color);
    color: var(--text-gray);
}

.footer-bottom i {
    color: var(--error-color);
}

/* Loading Screen */
.loading-screen {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: var(--background-dark);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    transition: opacity var(--transition-slow), visibility var(--transition-slow);
}

.loading-screen.hidden {
    opacity: 0;
    visibility: hidden;
}

.loading-content {
    text-align: center;
    max-width: 400px;
}

.loading-logo img {
    width: 120px;
    height: auto;
    margin-bottom: 2rem;
    animation: pulse 2s infinite;
}

.loading-progress {
    width: 300px;
    height: 4px;
    background: var(--border-color);
    border-radius: 2px;
    margin: 2rem auto;
    overflow: hidden;
}

.progress-bar {
    width: 0;
    height: 100%;
    background: linear-gradient(90deg, var(--primary-color), var(--accent-color));
    border-radius: 2px;
    transition: width var(--transition-normal);
}

.loading-text {
    color: var(--text-gray);
    font-size: 1.1rem;
}

@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.7; }
}

/* Responsive Design */
@media (max-width: 768px) {
    .container {
        padding: 0 1rem;
    }
    
    .nav-menu {
        position: fixed;
        top: 70px;
        left: -100%;
        width: 100%;
        height: calc(100vh - 70px);
        background: var(--background-dark);
        flex-direction: column;
        justify-content: flex-start;
        align-items: center;
        gap: 2rem;
        padding: 2rem;
        transition: var(--transition-normal);
    }
    
    .nav-menu.active {
        left: 0;
    }
    
    .nav-toggle {
        display: flex;
    }
    
    .nav-toggle.active .bar:nth-child(1) {
        transform: rotate(-45deg) translate(-5px, 6px);
    }
    
    .nav-toggle.active .bar:nth-child(2) {
        opacity: 0;
    }
    
    .nav-toggle.active .bar:nth-child(3) {
        transform: rotate(45deg) translate(-5px, -6px);
    }
    
    .hero-actions {
        flex-direction: column;
        align-items: center;
    }
    
    .games-grid {
        grid-template-columns: 1fr;
    }
    
    .featured-content {
        grid-template-columns: 1fr;
        text-align: center;
    }
    
    .footer-content {
        grid-template-columns: 1fr;
        text-align: center;
    }
    
    .form-group {
        flex-direction: column;
    }
    
    .form-group input {
        min-width: auto;
    }
    
    .features-grid {
        grid-template-columns: 1fr;
    }
}

/* Utility Classes */
.text-center { text-align: center; }
.text-left { text-align: left; }
.text-right { text-align: right; }

.hidden { display: none !important; }
.visible { display: block !important; }

.fade-in {
    opacity: 0;
    animation: fadeIn var(--transition-slow) ease forwards;
}

@keyframes fadeIn {
    to { opacity: 1; }
}

.slide-up {
    transform: translateY(50px);
    opacity: 0;
    animation: slideUp var(--transition-slow) ease forwards;
}

@keyframes slideUp {
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

/* Print Styles */
@media print {
    .navbar, .footer, .loading-screen {
        display: none !important;
    }
    
    body {
        color: #000 !important;
        background: #fff !important;
    }
}`;
    }

    /**
     * Generate service worker for PWA functionality
     */
    generateServiceWorker() {
        return `// Service Worker for Unit4Productions Gaming Website
const CACHE_NAME = 'unit4productions-v1.0.0';
const STATIC_ASSETS = [
    '/',
    '/assets/css/main.css',
    '/assets/css/game-page.css',
    '/assets/js/main.js',
    '/assets/js/games.js',
    '/assets/js/analytics.js',
    '/assets/js/game-page.js',
    '/assets/images/logo.svg',
    '/assets/images/placeholder-game.jpg',
    '/favicon.ico',
    '/manifest.json'
];

// Install event - cache static assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_ASSETS))
            .then(() => self.skipWaiting())
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName !== CACHE_NAME)
                        .map(cacheName => caches.delete(cacheName))
                );
            })
            .then(() => self.clients.claim())
    );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // Return cached version or fetch from network
                return response || fetch(event.request);
            })
            .catch(() => {
                // Fallback for offline pages
                if (event.request.destination === 'document') {
                    return caches.match('/offline.html');
                }
            })
    );
});

// Background sync for analytics
self.addEventListener('sync', event => {
    if (event.tag === 'background-sync') {
        event.waitUntil(sendAnalytics());
    }
});

// Push notifications
self.addEventListener('push', event => {
    if (event.data) {
        const data = event.data.json();
        event.waitUntil(
            self.registration.showNotification(data.title, {
                body: data.body,
                icon: '/assets/images/icon-192.png',
                badge: '/assets/images/badge-72.png',
                tag: 'unit4-notification'
            })
        );
    }
});

async function sendAnalytics() {
    // Send cached analytics data when online
    try {
        const cache = await caches.open('analytics-cache');
        const requests = await cache.keys();
        
        for (const request of requests) {
            await fetch(request);
            await cache.delete(request);
        }
    } catch (error) {
        console.error('Failed to send analytics:', error);
    }
}`;
    }

    /**
     * Generate web app manifest
     */
    generateManifest() {
        return JSON.stringify({
            name: this.siteName,
            short_name: 'Unit4Games',
            description: this.description,
            start_url: '/',
            display: 'standalone',
            theme_color: this.primaryColor,
            background_color: this.secondaryColor,
            orientation: 'portrait-primary',
            icons: [
                {
                    src: '/assets/images/icon-72.png',
                    sizes: '72x72',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-96.png',
                    sizes: '96x96',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-128.png',
                    sizes: '128x128',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-144.png',
                    sizes: '144x144',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-152.png',
                    sizes: '152x152',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-192.png',
                    sizes: '192x192',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-384.png',
                    sizes: '384x384',
                    type: 'image/png'
                },
                {
                    src: '/assets/images/icon-512.png',
                    sizes: '512x512',
                    type: 'image/png'
                }
            ],
            categories: ['games', 'entertainment'],
            shortcuts: [
                {
                    name: 'Play Games',
                    short_name: 'Games',
                    description: 'Browse and play games',
                    url: '/games',
                    icons: [{ src: '/assets/images/games-icon.png', sizes: '96x96' }]
                },
                {
                    name: 'Latest Games',
                    short_name: 'Latest',
                    description: 'Check out our latest games',
                    url: '/games/latest',
                    icons: [{ src: '/assets/images/latest-icon.png', sizes: '96x96' }]
                }
            ]
        }, null, 2);
    }
}

module.exports = WebsiteTemplateSystem;