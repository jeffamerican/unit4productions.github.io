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
        this.currentSort = 'featured';
        this.searchQuery = '';
        this.loading = false;
        this.batchSize = 12; // Load 12 games at a time
        this.currentIndex = 0;
        this.isInfiniteScrollEnabled = true;
        
        // Carousel properties
        this.cardsPerView = 3;
        this.currentCarouselIndex = 0;
        this.totalPages = 0;
        this.isCarouselMode = true;
    }

    async init() {
        try {
            await this.loadGamesData();
            this.setupEventListeners();
            this.renderGames();
        } catch (error) {
            console.error('Failed to initialize games loader:', error);
            this.showError('Failed to load games. Please refresh the page.');
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
        // Category filter
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

        // Infinite scroll (disabled in carousel mode)
        if (!this.isCarouselMode) {
            this.setupInfiniteScroll();
        }
        
        // Carousel navigation
        this.setupCarousel();
        
        // Window resize handler for responsive carousel
        window.addEventListener('resize', () => {
            if (this.isCarouselMode && this.displayedGames.length > 0) {
                // Recalculate pagination on resize
                setTimeout(() => {
                    this.setupCarouselPagination();
                }, 100); // Small delay to ensure layout is settled
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
                // Check both new categories array and legacy category field
                const gameCategories = game.categories || [game.category];
                return gameCategories.includes(category) || 
                       (category === 'ai-exclusive' && game.ai_exclusive) ||
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
                // Random shuffle for discovery
                this.filteredGames.sort(() => Math.random() - 0.5);
                break;
            case 'popular':
                this.filteredGames.sort((a, b) => b.plays - a.plays);
                break;
            case 'rating':
                this.filteredGames.sort((a, b) => b.rating - a.rating);
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

        // Apply correct CSS class based on mode
        if (this.isCarouselMode) {
            container.classList.add('carousel-mode');
            container.classList.remove('grid-mode');
            this.loadAllGamesForCarousel();
        } else {
            container.classList.add('grid-mode');
            container.classList.remove('carousel-mode');
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

    createGameElement(game) {
        const element = document.createElement('a');
        element.href = game.file;
        element.className = 'game-card game-card-entry';
        element.dataset.gameId = game.id;
        element.innerHTML = this.createGameCard(game);
        return element;
    }

    createGameCard(game) {
        const difficultyIcon = this.getDifficultyIcon(game.difficulty);
        
        // Use high-quality large images
        const imageSrc = game.thumbnail_large || game.thumbnail;

        return `
            <div class="card-media">
                <img src="${imageSrc}" alt="${game.title}" class="game-image" loading="lazy">
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
                    <div class="plays">${this.formatPlays(game.plays)} plays</div>
                    <div class="difficulty">${difficultyIcon}</div>
                </div>
            </div>
        `;
    }

    getBadgeClass(badge) {
        const classes = {
            'flagship': 'badge-flagship',
            'new-release': 'badge-new',
            'ai-exclusive': 'badge-ai',
            '2-player': 'badge-multiplayer',
            'available': 'badge-available'
        };
        return classes[badge] || 'badge-default';
    }

    formatBadge(badge) {
        const labels = {
            'flagship': '🚀 FLAGSHIP',
            'new-release': '✨ NEW',
            'ai-exclusive': '🤖 AI ONLY',
            '2-player': '👥 2P',
            'available': 'PLAY NOW'
        };
        return labels[badge] || 'AVAILABLE';
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
                game.rating || 0, 
                0
            );
            return ratingDisplay.starsHtml;
        }
        
        // Fallback to static stars if rating system not loaded
        return this.generateStars(game.rating || 0);
    }

    // Get display rating for a game
    getDisplayRating(game) {
        if (window.BotIncRatingSystem) {
            const ratingDisplay = window.BotIncRatingSystem.generateRatingDisplay(
                game.id, 
                game.rating || 0, 
                0
            );
            return ratingDisplay.ratingText;
        }
        
        // Fallback to original rating
        return game.rating || 'No rating';
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
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gamesLoader = new GamesLoader();
    window.gamesLoader.init();
    
    // Images now respect CSS sizing rules - no forced shrinking
});