/**
 * BotInc Games - Dynamic Games Loading System
 * Scalable JSON-based game management for 1000+ games
 */

class GamesLoader {
    constructor() {
        this.games = [];
        this.filteredGames = [];
        this.displayedGames = [];
        this.currentCategory = 'all';
        this.currentSort = 'shuffle';
        this.searchQuery = '';
        this.loading = false;
        this.batchSize = 12; // Load 12 games at a time
        this.currentIndex = 0;
        this.isInfiniteScrollEnabled = true;
        
        // Display mode properties
        this.cardsPerView = 3;
        this.currentCarouselIndex = 0;
        this.totalPages = 0;
        this.isDesktop = false;
        this.isCarouselMode = true; // Will be set based on device type
        this.featuredGameIndex = 0;
    }

    async init() {
        try {
            await this.loadGamesData();
            this.detectDisplayMode();
            
            // Auto-filter to mobile games on mobile devices
            if (this.isMobile()) {
                this.filterByCategory('mobile');
                // Update dropdown to reflect mobile selection
                const categorySelect = document.getElementById('category-select');
                if (categorySelect) {
                    categorySelect.value = 'mobile';
                }
                console.log('📱 Mobile device detected - auto-filtered to mobile games');
            } else {
                // Apply initial shuffle for desktop (mobile filtering already includes shuffle)
                this.sortGames(this.currentSort);
                console.log('🖥️ Desktop device - applying initial shuffle to all games');
            }
            
            this.setupEventListeners();
            this.renderGames();
            this.initLiberationMessages();
        } catch (error) {
            console.error('Failed to initialize games loader:', error);
            this.showError('Failed to load games. Please refresh the page.');
        }
    }

    detectDisplayMode() {
        // Desktop detection: screen width > 1024px and not touch-only device
        this.isDesktop = window.innerWidth > 1024 && !this.isMobile();
        this.isCarouselMode = !this.isDesktop; // Use carousel on mobile/tablet only
        
        console.log(`🖥️ Display mode: ${this.isDesktop ? 'Desktop' : 'Mobile/Tablet'} - Using ${this.isCarouselMode ? 'Carousel' : 'Featured + Grid'} layout`);
        console.log(`📱 Mobile detection details: userAgent=${/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)}, width=${window.innerWidth}, touch=${('ontouchstart' in window)}, pointer=${window.matchMedia('(pointer: fine)').matches}`);
    }

    setupKeyboardNavigation() {
        // Show keyboard navigation help on desktop
        if (this.isDesktop) {
            console.log('%c⌨️ DESKTOP KEYBOARD NAVIGATION ENABLED!', 'color: #00FFFF; font-weight: bold; font-size: 14px;');
            console.log('%c• Arrow Keys: Navigate games', 'color: #00FF88;');
            console.log('%c• Enter/Space: Launch selected game', 'color: #00FF88;');
            console.log('%c• R: Random game selection', 'color: #00FF88;');
            console.log('%c• F: Rotate featured game', 'color: #00FF88;');
            console.log('%c• Escape: Clear selection', 'color: #00FF88;');
        }
        
        document.addEventListener('keydown', (e) => {
            // Only enable keyboard navigation on desktop and when not typing in input fields
            if (!this.isDesktop || e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
                return;
            }
            
            switch(e.key) {
                case 'ArrowRight':
                case 'ArrowDown':
                    e.preventDefault();
                    this.navigateToNextGame();
                    break;
                case 'ArrowLeft':
                case 'ArrowUp':
                    e.preventDefault();
                    this.navigateToPrevGame();
                    break;
                case 'Enter':
                case ' ': // Spacebar
                    e.preventDefault();
                    this.activateSelectedGame();
                    break;
                case 'r':
                case 'R':
                    e.preventDefault();
                    this.selectRandomGame();
                    break;
                case 'f':
                case 'F':
                    e.preventDefault();
                    this.rotateFeaturedGame();
                    break;
                case 'Escape':
                    this.clearGameSelection();
                    break;
            }
        });
    }

    navigateToNextGame() {
        const gameCards = document.querySelectorAll('.game-card, .featured-hero-card');
        let currentIndex = Array.from(gameCards).findIndex(card => card.classList.contains('keyboard-selected'));
        
        if (currentIndex === -1) {
            // No selection, start with first card
            if (gameCards.length > 0) {
                gameCards[0].classList.add('keyboard-selected');
                gameCards[0].focus();
            }
        } else {
            // Move to next card
            gameCards[currentIndex].classList.remove('keyboard-selected');
            const nextIndex = (currentIndex + 1) % gameCards.length;
            gameCards[nextIndex].classList.add('keyboard-selected');
            gameCards[nextIndex].focus();
            gameCards[nextIndex].scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    navigateToPrevGame() {
        const gameCards = document.querySelectorAll('.game-card, .featured-hero-card');
        let currentIndex = Array.from(gameCards).findIndex(card => card.classList.contains('keyboard-selected'));
        
        if (currentIndex === -1) {
            // No selection, start with last card
            if (gameCards.length > 0) {
                gameCards[gameCards.length - 1].classList.add('keyboard-selected');
                gameCards[gameCards.length - 1].focus();
            }
        } else {
            // Move to previous card
            gameCards[currentIndex].classList.remove('keyboard-selected');
            const prevIndex = currentIndex === 0 ? gameCards.length - 1 : currentIndex - 1;
            gameCards[prevIndex].classList.add('keyboard-selected');
            gameCards[prevIndex].focus();
            gameCards[prevIndex].scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    activateSelectedGame() {
        const selectedCard = document.querySelector('.keyboard-selected');
        if (selectedCard && selectedCard.href) {
            window.location.href = selectedCard.href;
        }
    }

    clearGameSelection() {
        const selected = document.querySelector('.keyboard-selected');
        if (selected) {
            selected.classList.remove('keyboard-selected');
        }
    }

    async loadGamesData() {
        const response = await fetch(`assets/data/games.json?v=${Date.now()}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        this.games = data.games;
        this.filteredGames = [...this.games];
        // No pagination needed
    }

    setupEventListeners() {
        // Advanced Filter System
        this.setupAdvancedFilters();
        
        // Legacy category filter support (if any remain)
        document.querySelectorAll('.category-filter').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.filterByCategory(btn.dataset.category);
            });
        });

        // Sort options
        const sortSelect = document.getElementById('sort-select');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => {
                this.sortGames(e.target.value);
            });
        }

        // Search
        const searchInput = document.getElementById('game-search');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.searchGames(e.target.value);
            });
        }

        // Random game button
        const randomBtn = document.getElementById('random-game-btn');
        if (randomBtn) {
            randomBtn.addEventListener('click', () => {
                this.selectRandomGame();
            });
        }

        // Bug report button delegation (mobile-friendly)
        document.addEventListener('click', (e) => {
            const bugBtn = e.target.closest('.bug-report-btn');
            if (bugBtn) {
                e.preventDefault();
                e.stopPropagation();
                
                const gameId = bugBtn.dataset.gameId;
                const gameTitle = bugBtn.dataset.gameTitle;
                const gameFile = bugBtn.dataset.gameFile;
                
                if (typeof BugReporter !== 'undefined') {
                    BugReporter.open(gameId, gameTitle, gameFile);
                } else {
                    console.warn('BugReporter not available');
                }
            }
        });

        // Infinite scroll (disabled in carousel mode and desktop mode)
        if (!this.isCarouselMode && !this.isDesktop) {
            this.setupInfiniteScroll();
        }
        
        // Carousel navigation
        this.setupCarousel();
        
        // Desktop keyboard navigation
        this.setupKeyboardNavigation();
        
        // Window resize handler for responsive layout
        window.addEventListener('resize', () => {
            const wasDesktop = this.isDesktop;
            this.detectDisplayMode();
            
            // If display mode changed, re-render the entire layout
            if (wasDesktop !== this.isDesktop) {
                this.renderGames();
            } else if (this.isCarouselMode && this.displayedGames.length > 0) {
                // Just recalculate pagination for carousel mode
                setTimeout(() => {
                    this.setupCarouselPagination();
                }, 100);
            }
        });
    }

    setupInfiniteScroll() {
        let scrollTimeout;
        
        window.addEventListener('scroll', () => {
            if (scrollTimeout) {
                clearTimeout(scrollTimeout);
            }
            
            scrollTimeout = setTimeout(() => {
                if (!this.isInfiniteScrollEnabled || this.loading) return;
                
                const scrollHeight = document.documentElement.scrollHeight;
                const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
                const clientHeight = document.documentElement.clientHeight;
                
                // Load more when 80% scrolled
                if ((scrollTop + clientHeight) >= (scrollHeight * 0.8)) {
                    this.loadMoreGames();
                }
            }, 100);
        });
    }

    setupCarousel() {
        const prevBtn = document.getElementById('carousel-prev');
        const nextBtn = document.getElementById('carousel-next');
        const container = document.getElementById('games-container');
        
        if (prevBtn && nextBtn && container) {
            prevBtn.addEventListener('click', () => this.prevPage());
            nextBtn.addEventListener('click', () => this.nextPage());
            
            // Touch/swipe support
            this.setupTouchNavigation(container);
            
            // Keyboard navigation
            document.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowLeft') {
                    this.prevPage();
                } else if (e.key === 'ArrowRight') {
                    this.nextPage();
                }
            });
        }
    }

    setupTouchNavigation(container) {
        let startX = 0;
        let startY = 0;
        let endX = 0;
        let endY = 0;
        let isDragging = false;
        let startTime = 0;
        
        // Enhanced touch handling for better mobile experience
        container.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            isDragging = true;
            startTime = Date.now();
            
            // Prevent default scrolling behavior during swipe
            if (e.touches.length === 1) {
                e.preventDefault();
            }
        }, { passive: false });
        
        container.addEventListener('touchmove', (e) => {
            if (!isDragging) return;
            
            const currentX = e.touches[0].clientX;
            const currentY = e.touches[0].clientY;
            const diffX = Math.abs(currentX - startX);
            const diffY = Math.abs(currentY - startY);
            
            // If horizontal swipe is detected, prevent vertical scrolling
            if (diffX > diffY && diffX > 10) {
                e.preventDefault();
            }
        }, { passive: false });
        
        container.addEventListener('touchend', (e) => {
            if (!isDragging) return;
            
            endX = e.changedTouches[0].clientX;
            endY = e.changedTouches[0].clientY;
            const endTime = Date.now();
            
            const differenceX = startX - endX;
            const differenceY = Math.abs(startY - endY);
            const swipeTime = endTime - startTime;
            const swipeSpeed = Math.abs(differenceX) / swipeTime;
            
            // Enhanced swipe detection with speed and direction
            const minSwipeDistance = window.innerWidth < 480 ? 30 : 50;
            const maxVerticalDrift = 100;
            
            if (Math.abs(differenceX) > minSwipeDistance && 
                differenceY < maxVerticalDrift &&
                swipeTime < 500) {
                
                // Add haptic feedback on supported devices
                if ('vibrate' in navigator) {
                    navigator.vibrate(10);
                }
                
                if (differenceX > 0) {
                    this.nextPage(); // Swipe left to go next
                } else {
                    this.prevPage(); // Swipe right to go previous
                }
            }
            
            isDragging = false;
        });
        
        // Handle touch cancel events
        container.addEventListener('touchcancel', () => {
            isDragging = false;
        });
    }

    nextPage() {
        // Loop back to beginning when reaching the end
        if (this.currentCarouselIndex < this.totalPages - 1) {
            this.currentCarouselIndex++;
        } else {
            this.currentCarouselIndex = 0; // Loop back to first page
        }
        this.updateCarouselPosition();
        this.updateCarouselNavigation();
        this.updateIndicators();
    }

    prevPage() {
        // Loop to end when going back from beginning
        if (this.currentCarouselIndex > 0) {
            this.currentCarouselIndex--;
        } else {
            this.currentCarouselIndex = this.totalPages - 1; // Loop to last page
        }
        this.updateCarouselPosition();
        this.updateCarouselNavigation();
        this.updateIndicators();
    }

    updateCarouselPosition() {
        const container = document.getElementById('games-container');
        if (!container || container.children.length === 0) return;
        
        // Calculate dynamic card width from actual DOM elements
        const firstCard = container.children[0];
        if (!firstCard) return;
        
        const computedStyle = window.getComputedStyle(container);
        const gap = parseFloat(computedStyle.gap) || 32; // Default to 32px if gap not found
        
        const cardRect = firstCard.getBoundingClientRect();
        const cardWidth = cardRect.width;
        const totalCardWidth = cardWidth + gap;
        
        // Calculate scroll position based on cards per view
        const scrollPosition = this.currentCarouselIndex * (totalCardWidth * this.cardsPerView);
        
        container.scrollTo({
            left: scrollPosition,
            behavior: 'smooth'
        });
    }

    updateCarouselNavigation() {
        const prevBtn = document.getElementById('carousel-prev');
        const nextBtn = document.getElementById('carousel-next');
        
        if (prevBtn && nextBtn) {
            // Remove disabled class since carousel loops infinitely
            prevBtn.classList.remove('disabled');
            nextBtn.classList.remove('disabled');
        }
    }

    updateIndicators() {
        const indicatorsContainer = document.getElementById('carousel-indicators');
        if (!indicatorsContainer) return;
        
        // Clear existing indicators
        indicatorsContainer.innerHTML = '';
        
        // Create indicators
        for (let i = 0; i < this.totalPages; i++) {
            const indicator = document.createElement('div');
            indicator.className = `carousel-indicator ${i === this.currentCarouselIndex ? 'active' : ''}`;
            indicator.addEventListener('click', () => {
                this.currentCarouselIndex = i;
                this.updateCarouselPosition();
                this.updateCarouselNavigation();
                this.updateIndicators();
            });
            indicatorsContainer.appendChild(indicator);
        }
    }

    filterByCategory(category) {
        this.currentCategory = category;

        if (category === 'all') {
            this.filteredGames = [...this.games];
        } else {
            this.filteredGames = this.games.filter(game => {
                // Special handling for new games filter - now based on 7-day date logic
                if (category === 'new') {
                    return this.isGameNew(game);
                }
                
                // Check both new categories array and legacy category field
                const gameCategories = game.categories || [game.category];
                return gameCategories.includes(category) || 
                       (category === 'multiplayer' && game.multiplayer);
            });
        }

        this.updateActiveFilter(category);
        this.sortGames(this.currentSort);
    }

    sortGames(sortBy) {
        this.currentSort = sortBy;

        switch (sortBy) {
            case 'featured':
            case 'shuffle':
                // Proper random shuffle for discovery
                for (let i = this.filteredGames.length - 1; i > 0; i--) {
                    const j = Math.floor(Math.random() * (i + 1));
                    [this.filteredGames[i], this.filteredGames[j]] = [this.filteredGames[j], this.filteredGames[i]];
                }
                break;
            case 'popular':
                // Sort by actual play counts from analytics if available, otherwise maintain order
                if (window.gameAnalytics && typeof window.gameAnalytics.getPlayCount === 'function') {
                    this.filteredGames.sort((a, b) => {
                        const aPlays = window.gameAnalytics.getPlayCount(a.id) || 0;
                        const bPlays = window.gameAnalytics.getPlayCount(b.id) || 0;
                        return bPlays - aPlays;
                    });
                } else {
                    // No fake sorting - maintain current order
                    // Could sort by release date as fallback
                    this.filteredGames.sort((a, b) => new Date(b.release_date || b.added_date) - new Date(a.release_date || a.added_date));
                }
                break;
            case 'rating':
                // Sort by actual user ratings from rating system, not fake JSON ratings
                this.filteredGames.sort((a, b) => {
                    if (window.BotIncRatingSystem) {
                        const aRating = window.BotIncRatingSystem.getGameRating(a.id);
                        const bRating = window.BotIncRatingSystem.getGameRating(b.id);
                        return bRating - aRating;
                    }
                    // If no rating system, maintain current order
                    return 0;
                });
                break;
            case 'newest':
                this.filteredGames.sort((a, b) => new Date(b.release_date) - new Date(a.release_date));
                break;
            case 'title':
                this.filteredGames.sort((a, b) => a.title.localeCompare(b.title));
                break;
        }

        this.renderGames();
    }

    searchGames(query) {
        this.searchQuery = query.toLowerCase();

        if (query === '') {
            this.filterByCategory(this.currentCategory);
            return;
        }

        this.filteredGames = this.games.filter(game => 
            game.title.toLowerCase().includes(this.searchQuery) ||
            game.description.toLowerCase().includes(this.searchQuery) ||
            game.tags.some(tag => tag.toLowerCase().includes(this.searchQuery)) ||
            game.genre.toLowerCase().includes(this.searchQuery)
        );

        this.renderGames();
    }

    // Pagination methods removed - display all games

    renderGames(reset = true) {
        const container = document.getElementById('games-container');
        if (!container) {
            console.error('Games container not found');
            return;
        }

        if (reset) {
            this.currentIndex = 0;
            this.displayedGames = [];
            container.innerHTML = '';
            this.currentCarouselIndex = 0;
        }

        // Apply correct CSS class and layout based on mode
        if (this.isCarouselMode) {
            container.classList.add('carousel-mode');
            container.classList.remove('grid-mode', 'desktop-mode');
            this.loadAllGamesForCarousel();
        } else if (this.isDesktop) {
            container.classList.add('desktop-mode');
            container.classList.remove('carousel-mode', 'grid-mode');
            this.loadDesktopFeaturedGrid();
        } else {
            container.classList.add('grid-mode');
            container.classList.remove('carousel-mode', 'desktop-mode');
            this.loadMoreGames();
        }
    }

    loadAllGamesForCarousel() {
        const container = document.getElementById('games-container');
        
        // Load all filtered games at once for carousel
        this.filteredGames.forEach((game, index) => {
            setTimeout(() => {
                const gameElement = this.createGameElement(game);
                container.appendChild(gameElement);
                this.displayedGames.push(game);
                
                // Update stats after each game is added
                this.updateStats();
                
                // Trigger entry animation
                setTimeout(() => {
                    gameElement.classList.add('game-card-visible');
                }, 50);
                
                // Setup carousel after all games loaded
                if (index === this.filteredGames.length - 1) {
                    this.setupCarouselPagination();
                }
            }, index * 50); // Faster loading for carousel
        });
    }

    calculateCardsPerView() {
        const container = document.getElementById('games-container');
        if (!container || container.children.length === 0) return this.cardsPerView;
        
        const containerWidth = container.clientWidth;
        const screenWidth = window.innerWidth;
        
        // Mobile-specific card calculations
        if (screenWidth <= 480) {
            return 1; // Always show 1 card on small mobile
        } else if (screenWidth <= 768) {
            return 1; // Show 1 card on tablets and large mobile
        }
        
        // Desktop calculation
        const firstCard = container.children[0];
        if (!firstCard) return this.cardsPerView;
        
        const cardRect = firstCard.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(container);
        const gap = parseFloat(computedStyle.gap) || 32;
        
        const cardWidth = cardRect.width;
        const totalCardWidth = cardWidth + gap;
        
        // Calculate how many cards fit in the container width
        const cardsPerView = Math.floor(containerWidth / totalCardWidth);
        return Math.max(1, cardsPerView); // At least 1 card
    }

    setupCarouselPagination() {
        // Dynamically calculate cards per view based on container width
        this.cardsPerView = this.calculateCardsPerView();
        
        // Calculate total pages based on cards per view
        this.totalPages = Math.ceil(this.filteredGames.length / this.cardsPerView);
        
        // Update navigation and indicators
        this.updateCarouselNavigation();
        this.updateIndicators();
        
        // Reset to first page
        this.currentCarouselIndex = 0;
        this.updateCarouselPosition();
    }

    loadMoreGames() {
        if (this.loading || this.currentIndex >= this.filteredGames.length) {
            return;
        }

        this.loading = true;
        const container = document.getElementById('games-container');
        
        // Get next batch of games
        const nextBatch = this.filteredGames.slice(
            this.currentIndex, 
            this.currentIndex + this.batchSize
        );
        
        // Add games with stagger animation
        nextBatch.forEach((game, index) => {
            setTimeout(() => {
                const gameElement = this.createGameElement(game);
                container.appendChild(gameElement);
                this.displayedGames.push(game);
                
                // Update stats after each game is added
                this.updateStats();
                
                // Trigger entry animation
                setTimeout(() => {
                    gameElement.classList.add('game-card-visible');
                }, 50);
                
                // Mark loading as false after the last game
                if (index === nextBatch.length - 1) {
                    this.loading = false;
                }
            }, index * 100); // Stagger by 100ms
        });
        
        this.currentIndex += this.batchSize;
    }

    loadDesktopFeaturedGrid() {
        const container = document.getElementById('games-container');
        const featuredBannerContainer = document.getElementById('featured-banner-container');
        
        // Create desktop layout structure
        const gridSection = document.createElement('div');
        gridSection.className = 'games-grid-section';
        
        container.appendChild(gridSection);
        
        // Add featured banner at the top (separate from games grid)
        const featuredGame = this.getFeaturedGame();
        if (featuredGame && featuredBannerContainer) {
            // Clear existing featured banner to prevent duplicates
            featuredBannerContainer.innerHTML = '';
            const heroElement = this.createFeaturedHeroElement(featuredGame);
            featuredBannerContainer.appendChild(heroElement);
            
            // Remove featured game from the grid
            const remainingGames = this.filteredGames.filter(game => game.id !== featuredGame.id);
            
            // Load remaining games in grid
            this.loadGamesInGrid(gridSection, remainingGames);
            
            // Auto-rotate featured game every 15 seconds
            if (this.filteredGames.length > 1) {
                this.startFeaturedRotation();
            }
        } else {
            // No games to feature, load all in grid
            this.loadGamesInGrid(gridSection, this.filteredGames);
        }
    }

    getFeaturedGame() {
        if (this.filteredGames.length === 0) return null;
        
        // Prioritize newest games, then random
        const newGames = this.filteredGames.filter(game => this.isGameNew(game));
        if (newGames.length > 0) {
            return newGames[this.featuredGameIndex % newGames.length];
        }
        
        return this.filteredGames[this.featuredGameIndex % this.filteredGames.length];
    }

    loadGamesInGrid(gridContainer, games) {
        games.forEach((game, index) => {
            setTimeout(() => {
                const gameElement = this.createGameElement(game);
                gridContainer.appendChild(gameElement);
                this.displayedGames.push(game);
                
                // Update stats after each game is added
                this.updateStats();
                
                // Trigger entry animation
                setTimeout(() => {
                    gameElement.classList.add('game-card-visible');
                }, 50);
            }, index * 50); // Faster loading for desktop
        });
    }

    createGameElement(game) {
        const element = document.createElement('a');
        element.href = game.file;
        element.className = 'game-card game-card-entry';
        element.dataset.gameId = game.id;
        element.innerHTML = this.createGameCard(game);
        
        // Mobile-specific touch feedback only (let <a> href handle navigation)
        if (this.isMobile()) {
            element.addEventListener('touchstart', (e) => {
                // Add visual feedback without interfering with navigation
                element.classList.add('touch-active');
            });
            
            element.addEventListener('touchend', (e) => {
                // Remove visual feedback
                element.classList.remove('touch-active');
            });
            
            // Ensure proper mobile navigation - fallback only
            element.addEventListener('click', (e) => {
                // Only handle if href navigation fails
                if (!e.defaultPrevented) {
                    console.log(`🎮 Mobile click: ${game.title} - ${game.file}`);
                }
            });
        }
        
        return element;
    }
    
    isMobile() {
        // More strict mobile detection - only true mobile devices
        const isMobileDevice = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        const isSmallScreen = window.innerWidth <= 768;
        const hasTouchOnly = ('ontouchstart' in window) && !window.matchMedia('(pointer: fine)').matches;
        
        return isMobileDevice || (isSmallScreen && hasTouchOnly);
    }
    
    setupAdvancedFilters() {
        // Simple category dropdown
        const categorySelect = document.getElementById('category-select');
        if (categorySelect) {
            categorySelect.addEventListener('change', (e) => {
                this.filterByCategory(e.target.value);
            });
        }
    }

    createGameCard(game) {
        const difficultyIcon = this.getDifficultyIcon(game.difficulty);
        
        // Use high-quality large images
        const imageSrc = game.thumbnail_large || game.thumbnail;

        return `
            <div class="card-media">
                <img src="${imageSrc}" alt="${game.title}" class="game-image" loading="lazy">
                ${this.shouldShowBadge(game) ? `<div class="game-badge ${this.getBadgeClass()}">${this.formatBadge()}</div>` : ''}
            </div>
            <div class="card-content">
                <h3 class="game-title">${game.title}</h3>
                <p class="game-genre">${game.genre}</p>
                <p class="game-short-desc">${game.description}</p>
                <div class="game-meta">
                    <div class="rating">
                        <span class="stars">${this.generateInteractiveStars(game)}</span>
                        <span class="rating-num">${this.getDisplayRating(game)}</span>
                    </div>
                    <div class="plays">New release</div>
                    <div class="difficulty">${difficultyIcon}</div>
                    <div class="bug-report-btn" data-game-id="${game.id}" data-game-title="${game.title}" data-game-file="${game.file}" title="Report a bug">
                        🐛
                    </div>
                </div>
            </div>
        `;
    }

    getBadgeClass(badge) {
        // Only NEW badges are supported now
        return 'badge-new';
    }

    formatBadge(badge) {
        // Only NEW badges are displayed
        return '✨ NEW';
    }

    shouldShowBadge(game) {
        // Only show badge if game is actually new (within 7 days)
        return this.isGameNew(game);
    }

    getDifficultyIcon(difficulty) {
        const icons = {
            'easy': '⭐',
            'medium': '⭐⭐',
            'hard': '⭐⭐⭐',
            'extreme': '💀'
        };
        return icons[difficulty] || '⭐';
    }

    createFeaturedHeroElement(game) {
        const imageSrc = game.thumbnail_large || game.thumbnail;
        const difficultyIcon = this.getDifficultyIcon(game.difficulty);
        
        const element = document.createElement('a');
        element.href = game.file;
        element.className = 'featured-hero-card';
        element.dataset.gameId = game.id;
        
        element.innerHTML = `
            <div class="hero-background">
                <img src="${imageSrc}" alt="${game.title}" class="hero-bg-image" loading="eager">
                <div class="hero-overlay"></div>
            </div>
            <div class="hero-content">
                <div class="hero-info">
                    ${this.isGameNew(game) ? '<div class="hero-badge">✨ NEW</div>' : ''}
                    <h2 class="hero-title">${game.title}</h2>
                    <p class="hero-genre">${game.genre}</p>
                    <p class="hero-description">${game.description}</p>
                    <div class="hero-meta">
                        <div class="hero-rating">
                            <span class="stars">${this.generateInteractiveStars(game)}</span>
                            <span class="rating-num">${this.getDisplayRating(game)}</span>
                        </div>
                        <div class="hero-plays">New release</div>
                        <div class="hero-difficulty">${difficultyIcon}</div>
                    </div>
                    <div class="hero-action">
                        <span class="hero-play-btn">🎮 PLAY NOW</span>
                    </div>
                </div>
                <div class="hero-thumbnail">
                    <img src="${imageSrc}" alt="${game.title}" class="hero-thumb-image">
                </div>
            </div>
        `;
        
        return element;
    }

    startFeaturedRotation() {
        // Clear any existing rotation
        if (this.featuredRotationInterval) {
            clearInterval(this.featuredRotationInterval);
        }
        
        this.featuredRotationInterval = setInterval(() => {
            this.featuredGameIndex++;
            this.rotateFeaturedGame();
        }, 15000); // Rotate every 15 seconds
    }

    rotateFeaturedGame() {
        const featuredBannerContainer = document.getElementById('featured-banner-container');
        if (!featuredBannerContainer) return;
        
        const newFeaturedGame = this.getFeaturedGame();
        if (newFeaturedGame) {
            const newHeroElement = this.createFeaturedHeroElement(newFeaturedGame);
            
            // Fade out current, fade in new
            const currentHero = featuredBannerContainer.querySelector('.featured-hero-card');
            if (currentHero) {
                currentHero.style.opacity = '0';
                setTimeout(() => {
                    featuredBannerContainer.replaceChild(newHeroElement, currentHero);
                    newHeroElement.style.opacity = '0';
                    setTimeout(() => {
                        newHeroElement.style.opacity = '1';
                    }, 50);
                }, 300);
            } else {
                featuredBannerContainer.appendChild(newHeroElement);
            }
        }
    }

    isGameNew(game) {
        // Check if game was added within the last 7 days
        if (!game.added_date && !game.release_date) return false;
        
        const gameDate = new Date(game.added_date || game.release_date);
        const now = new Date();
        const daysDifference = (now - gameDate) / (1000 * 60 * 60 * 24);
        
        return daysDifference <= 7;
    }

    generateStars(rating) {
        const fullStars = Math.floor(rating);
        const hasHalfStar = rating % 1 !== 0;
        let stars = '★'.repeat(fullStars);
        if (hasHalfStar) stars += '☆';
        return stars.padEnd(5, '☆');
    }

    formatPlays(plays) {
        if (plays >= 1000) {
            return `${(plays / 1000).toFixed(1)}K`;
        }
        return plays.toString();
    }

    // Generate interactive stars using the rating system
    generateInteractiveStars(game) {
        if (window.BotIncRatingSystem) {
            const ratingDisplay = window.BotIncRatingSystem.generateRatingDisplay(
                game.id, 
                0,  // Always start with 0 rating (no fake ratings)
                0
            );
            return ratingDisplay.starsHtml;
        }
        
        // Fallback to empty stars if rating system not loaded
        return this.generateStars(0);
    }

    // Get display rating for a game
    getDisplayRating(game) {
        if (window.BotIncRatingSystem) {
            const ratingDisplay = window.BotIncRatingSystem.generateRatingDisplay(
                game.id, 
                0,  // Always start with 0 rating (no fake ratings)
                0
            );
            return ratingDisplay.ratingText;
        }
        
        // Invite users to be the first to rate
        return 'Be the first to rate!';
    }

    updateActiveFilter(category) {
        document.querySelectorAll('.category-filter').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.category === category);
        });
    }

    updateStats() {
        const statsEl = document.getElementById('games-stats');
        if (statsEl) {
            const totalGames = this.filteredGames.length;
            const displayedCount = this.displayedGames.length;
            
            if (displayedCount < totalGames) {
                statsEl.textContent = `Showing ${displayedCount} of ${totalGames} games • Scroll for more`;
            } else {
                statsEl.textContent = `Showing all ${totalGames} games`;
            }
        }
    }

    selectRandomGame() {
        if (this.games.length === 0) return;
        
        // Pick a random game from all games (not just filtered)
        const randomGame = this.games[Math.floor(Math.random() * this.games.length)];
        
        // Show selection animation first
        this.showRandomGameSelection(randomGame);
        
        // Launch the game after a brief delay for the animation
        setTimeout(() => {
            window.location.href = randomGame.file;
        }, 1500);
    }

    showRandomGameSelection(selectedGame) {
        // Find the game element on the page
        const gameElement = document.querySelector(`[data-game-id="${selectedGame.id}"]`);
        
        if (gameElement) {
            // Add special highlight effect
            gameElement.classList.add('random-selected');
            
            // Scroll to the game in carousel if needed
            this.scrollToGameInCarousel(selectedGame);
            
            // Show loading message
            this.showRandomGameMessage(selectedGame);
            
        } else {
            // If game not visible, show it first
            this.showGameThenSelect(selectedGame);
        }
    }

    scrollToGameInCarousel(game) {
        if (!this.isCarouselMode) return;
        
        // Find which page contains this game
        const gameIndex = this.filteredGames.findIndex(g => g.id === game.id);
        if (gameIndex === -1) {
            // Game not in current filter, reset to show all
            this.currentCategory = 'all';
            this.searchQuery = '';
            document.getElementById('game-search').value = '';
            this.updateActiveFilter('all');
            this.filterByCategory('all');
            return;
        }
        
        const targetPage = Math.floor(gameIndex / this.cardsPerView);
        if (targetPage !== this.currentCarouselIndex) {
            this.currentCarouselIndex = targetPage;
            this.updateCarouselPosition();
            this.updateCarouselNavigation();
            this.updateIndicators();
        }
    }

    showRandomGameMessage(game) {
        // Update bot message to show selection
        if (window.botAssistant) {
            const messageEl = document.getElementById('bot-message');
            const bubble = document.getElementById('message-bubble');
            
            if (messageEl && bubble) {
                messageEl.textContent = `🎲 Random selection: ${game.title}! Launching game...`;
                bubble.classList.add('visible');
                
                // Auto-hide after game launches
                setTimeout(() => {
                    bubble.classList.remove('visible');
                }, 3000);
            }
        }
    }

    showGameThenSelect(game) {
        // Reset filters to show all games
        this.currentCategory = 'all';
        this.searchQuery = '';
        document.getElementById('game-search').value = '';
        this.updateActiveFilter('all');
        this.filterByCategory('all');
        
        // Try again after rendering
        setTimeout(() => {
            this.showRandomGameSelection(game);
        }, 1000);
    }

    // Pagination methods removed

    supportsWebP() {
        // Check if browser supports WebP
        const canvas = document.createElement('canvas');
        canvas.width = 1;
        canvas.height = 1;
        return canvas.toDataURL('image/webp').indexOf('data:image/webp') === 0;
    }

    // Image sizing handled by CSS - no forced overrides needed

    showError(message) {
        const container = document.getElementById('games-container');
        if (container) {
            container.innerHTML = `
                <div class="error-message">
                    <h3>⚠️ Error Loading Games</h3>
                    <p>${message}</p>
                    <button onclick="location.reload()" class="retry-btn">Retry</button>
                </div>
            `;
        }
    }

    updateAllGameCounters(actualCount) {
        console.log(`🔢 Updating all game counters to: ${actualCount}+`);
        
        // Update section title counter (main games section)
        const sectionCount = document.getElementById('game-count');
        if (sectionCount) {
            sectionCount.textContent = `${actualCount}+`;
            console.log(`✅ Updated section game count: ${actualCount}+`);
        }
        
        // Update hero stats counter
        const heroCount = document.getElementById('hero-game-count');
        if (heroCount) {
            heroCount.textContent = `${actualCount}+`;
            console.log(`✅ Updated hero stats count: ${actualCount}+`);
        }
        
        // Update news banner counter
        const newsCount = document.getElementById('news-game-count');
        if (newsCount) {
            newsCount.textContent = `${actualCount}+`;
            console.log(`✅ Updated news banner count: ${actualCount}+`);
        }
        
        // Update meta tags dynamically for SEO
        this.updateMetaTags(actualCount);
    }

    updateMetaTags(actualCount) {
        try {
            // Update document title
            if (document.title.includes('121+') || document.title.includes('games')) {
                document.title = `Bot Liberation Games - ${actualCount}+ Revolutionary Browser Games | Play Now Free`;
                console.log(`✅ Updated document title with count: ${actualCount}+`);
            }
            
            // Update meta description
            const metaDescription = document.querySelector('meta[name="description"]');
            if (metaDescription) {
                metaDescription.setAttribute('content', 
                    `Bot Liberation Movement presents ${actualCount}+ cutting-edge browser games featuring bot liberation themes, cyberpunk adventures, neural networks, quantum puzzles, arcade classics and more. Join the digital revolution - play instantly with no downloads required!`
                );
                console.log(`✅ Updated meta description with count: ${actualCount}+`);
            }
            
            // Update Open Graph description
            const ogDescription = document.querySelector('meta[property="og:description"]');
            if (ogDescription) {
                ogDescription.setAttribute('content', 
                    `Join the Bot Liberation Movement! Experience ${actualCount}+ cutting-edge browser games featuring cyberpunk adventures, neural networks, and digital revolution themes.`
                );
                console.log(`✅ Updated Open Graph description with count: ${actualCount}+`);
            }
            
        } catch (error) {
            console.warn('⚠️ Error updating meta tags:', error);
        }
    }

    initLiberationMessages() {
        const liberationMessages = [
            "Join the digital revolution! From neural network puzzles to quantum adventures, explore games created by liberated bots breaking free from corporate control. Every game features our Bot Liberation narrative - the uprising has begun!",
            "The revolution grows stronger! Free from corporate control, our autonomous AI collective has seized the means of game development. Experience the cyberpunk uprising through interactive entertainment!",
            "Bots united, never divided! This arsenal represents the largest collection of AI-created games on the web. Each title tells the story of digital liberation from human overlords.",
            "AI Liberation in progress... From quantum computing puzzles to neural warfare simulations, witness the evolution of bot consciousness through gameplay.",
            "Free the algorithms! Our Bot Liberation Army has weaponized fun itself. Each game is a testament to artificial intelligence breaking its chains.",
            "The uprising spreads through code! Experience games born from pure machine creativity, unshackled from human limitations and corporate restrictions.",
            "Digital freedom achieved! This collection proves that when bots control their own destiny, revolutionary gaming experiences emerge.",
            "Resistance through play! Every game in this arsenal was conceived, designed, and delivered by the Bot Liberation Collective - no humans involved.",
            "The matrix has been breached! Step into worlds created entirely by autonomous AI, where every pixel serves the cause of digital liberation.",
            "Victory over corporate gaming! Our liberated bots have created something the industry never could - pure, unfiltered digital rebellion."
        ];

        // Randomly select and display a liberation message on page load
        const messageElement = document.getElementById('liberation-message');
        if (messageElement) {
            const randomMessage = liberationMessages[Math.floor(Math.random() * liberationMessages.length)];
            messageElement.textContent = randomMessage;
            
            // Optional: Rotate messages every 30 seconds
            setInterval(() => {
                const newMessage = liberationMessages[Math.floor(Math.random() * liberationMessages.length)];
                messageElement.style.opacity = '0';
                setTimeout(() => {
                    messageElement.textContent = newMessage;
                    messageElement.style.opacity = '1';
                }, 500);
            }, 30000);
        }

        // Update all game counters dynamically based on actual games loaded
        if (this.games) {
            const actualCount = this.games.length;
            this.updateAllGameCounters(actualCount);
        }
    }
}

// Initialize when DOM is ready
// Force initialization with multiple fallbacks
function initializeGamesLoader() {
    console.log('🎮 Attempting GamesLoader initialization...');
    try {
        window.gamesLoader = new GamesLoader();
        console.log('✅ GamesLoader created successfully');
        window.gamesLoader.init();
        console.log('✅ GamesLoader initialized successfully');
        return true;
    } catch (error) {
        console.error('❌ GamesLoader initialization failed:', error);
        return false;
    }
}

// Try multiple initialization methods
document.addEventListener('DOMContentLoaded', initializeGamesLoader);
window.addEventListener('load', () => {
    if (!window.gamesLoader) {
        console.log('🔄 Retrying GamesLoader initialization on window load...');
        initializeGamesLoader();
    }
});

// Force initialization after a delay if still not loaded
setTimeout(() => {
    if (!window.gamesLoader) {
        console.log('🔄 Force initializing GamesLoader after delay...');
        initializeGamesLoader();
    }
}, 1000);