// ============================================
// Games Page Specific JavaScript
// Enhanced functionality for games collection
// ============================================

class GameInteractions {
    constructor() {
        this.init();
    }

    init() {
        this.setupAdvancedFiltering();
        this.setupSearchFunctionality();
        this.setupSortingOptions();
        this.setupWishlistManager();
        this.setupGamePreview();
        this.initializeGameCards();
    }

    // ============================================
    // Advanced Game Filtering
    // ============================================

    setupAdvancedFiltering() {
        const filterContainer = document.querySelector('.games-filter');
        if (!filterContainer) return;

        // Add search and sort controls
        const advancedControls = document.createElement('div');
        advancedControls.className = 'advanced-controls';
        advancedControls.innerHTML = `
            <div class="search-container">
                <input type="text" id="gameSearch" placeholder="Search games..." class="search-input">
                <button class="search-clear" id="searchClear">&times;</button>
            </div>
            <div class="sort-container">
                <label for="gameSort">Sort by:</label>
                <select id="gameSort" class="sort-select">
                    <option value="release">Release Date</option>
                    <option value="rating">Rating</option>
                    <option value="name">Name</option>
                    <option value="genre">Genre</option>
                </select>
            </div>
        `;

        filterContainer.appendChild(advancedControls);

        // Style the advanced controls
        const style = document.createElement('style');
        style.textContent = `
            .advanced-controls {
                display: flex;
                justify-content: center;
                gap: 2rem;
                margin-top: 1rem;
                flex-wrap: wrap;
            }
            
            .search-container {
                position: relative;
                min-width: 250px;
            }
            
            .search-input {
                width: 100%;
                padding: 0.75rem 2.5rem 0.75rem 1rem;
                background: rgba(0, 0, 0, 0.5);
                border: 2px solid rgba(0, 255, 136, 0.3);
                border-radius: 25px;
                color: white;
                font-size: 0.9rem;
            }
            
            .search-input:focus {
                outline: none;
                border-color: #00ff88;
                box-shadow: 0 0 10px rgba(0, 255, 136, 0.3);
            }
            
            .search-clear {
                position: absolute;
                right: 0.75rem;
                top: 50%;
                transform: translateY(-50%);
                background: none;
                border: none;
                color: rgba(255, 255, 255, 0.6);
                cursor: pointer;
                font-size: 1.2rem;
                display: none;
            }
            
            .search-clear.visible {
                display: block;
            }
            
            .sort-container {
                display: flex;
                align-items: center;
                gap: 0.5rem;
                color: rgba(255, 255, 255, 0.8);
            }
            
            .sort-select {
                padding: 0.5rem 1rem;
                background: rgba(0, 0, 0, 0.5);
                border: 2px solid rgba(0, 255, 136, 0.3);
                border-radius: 5px;
                color: white;
                font-size: 0.9rem;
            }
            
            .sort-select:focus {
                outline: none;
                border-color: #00ff88;
            }
            
            @media (max-width: 768px) {
                .advanced-controls {
                    flex-direction: column;
                    align-items: center;
                    gap: 1rem;
                }
                
                .search-container {
                    min-width: 200px;
                }
            }
        `;
        document.head.appendChild(style);
    }

    setupSearchFunctionality() {
        const searchInput = document.getElementById('gameSearch');
        const searchClear = document.getElementById('searchClear');
        
        if (!searchInput) return;

        searchInput.addEventListener('input', debounce((e) => {
            const query = e.target.value.toLowerCase();
            this.filterGames({ search: query });
            
            // Show/hide clear button
            if (query) {
                searchClear.classList.add('visible');
            } else {
                searchClear.classList.remove('visible');
            }
        }, 300));

        searchClear.addEventListener('click', () => {
            searchInput.value = '';
            searchClear.classList.remove('visible');
            this.filterGames({ search: '' });
        });
    }

    setupSortingOptions() {
        const sortSelect = document.getElementById('gameSort');
        if (!sortSelect) return;

        sortSelect.addEventListener('change', (e) => {
            this.sortGames(e.target.value);
        });
    }

    filterGames(filters = {}) {
        const gameCards = document.querySelectorAll('.game-card');
        const activeFilter = document.querySelector('.filter-tab.active')?.dataset.filter || 'all';
        
        gameCards.forEach(card => {
            let shouldShow = true;

            // Category filter
            if (activeFilter !== 'all' && !card.classList.contains(activeFilter)) {
                shouldShow = false;
            }

            // Search filter
            if (filters.search) {
                const title = card.querySelector('.game-title')?.textContent.toLowerCase() || '';
                const description = card.querySelector('.game-description')?.textContent.toLowerCase() || '';
                const genre = card.querySelector('.game-genre')?.textContent.toLowerCase() || '';
                
                if (!title.includes(filters.search) && 
                    !description.includes(filters.search) && 
                    !genre.includes(filters.search)) {
                    shouldShow = false;
                }
            }

            // Apply visibility
            if (shouldShow) {
                card.style.display = '';
                card.classList.remove('hiding');
                card.classList.add('showing');
            } else {
                card.classList.add('hiding');
                card.classList.remove('showing');
                setTimeout(() => {
                    if (card.classList.contains('hiding')) {
                        card.style.display = 'none';
                    }
                }, 300);
            }
        });

        this.updateResultsCount();
    }

    sortGames(sortBy) {
        const gamesGrid = document.getElementById('gamesGrid');
        const gameCards = Array.from(gamesGrid.querySelectorAll('.game-card'));
        
        gameCards.sort((a, b) => {
            switch (sortBy) {
                case 'name':
                    const nameA = a.querySelector('.game-title').textContent;
                    const nameB = b.querySelector('.game-title').textContent;
                    return nameA.localeCompare(nameB);
                
                case 'rating':
                    const ratingA = parseFloat(a.querySelector('.rating-value')?.textContent || 0);
                    const ratingB = parseFloat(b.querySelector('.rating-value')?.textContent || 0);
                    return ratingB - ratingA;
                
                case 'genre':
                    const genreA = a.querySelector('.game-genre').textContent;
                    const genreB = b.querySelector('.game-genre').textContent;
                    return genreA.localeCompare(genreB);
                
                case 'release':
                default:
                    // Sort by availability (available first, then upcoming)
                    const statusA = a.classList.contains('available') ? 0 : 1;
                    const statusB = b.classList.contains('available') ? 0 : 1;
                    return statusA - statusB;
            }
        });

        // Re-append sorted cards
        gameCards.forEach(card => {
            gamesGrid.appendChild(card);
        });
    }

    updateResultsCount() {
        const visibleCards = document.querySelectorAll('.game-card:not([style*="display: none"])').length;
        let countElement = document.querySelector('.results-count');
        
        if (!countElement) {
            countElement = document.createElement('div');
            countElement.className = 'results-count';
            countElement.style.cssText = `
                text-align: center;
                color: rgba(255, 255, 255, 0.7);
                margin: 1rem 0;
                font-size: 0.9rem;
            `;
            document.querySelector('.games-collection').prepend(countElement);
        }
        
        countElement.textContent = `Showing ${visibleCards} game${visibleCards !== 1 ? 's' : ''}`;
    }

    // ============================================
    // Wishlist Management
    // ============================================

    setupWishlistManager() {
        this.loadWishlistState();
        this.updateWishlistButtons();
    }

    loadWishlistState() {
        const wishlist = JSON.parse(localStorage.getItem('botinc_wishlist') || '[]');
        
        wishlist.forEach(gameId => {
            const gameCard = document.querySelector(`[data-game="${gameId}"]`);
            if (gameCard) {
                gameCard.classList.add('wishlisted');
                const button = gameCard.querySelector('.wishlist-button');
                if (button) {
                    button.innerHTML = '<i class="icon-heart-filled"></i><span>Wishlisted</span>';
                    button.classList.add('wishlisted');
                }
            }
        });
    }

    updateWishlistButtons() {
        document.querySelectorAll('.wishlist-button').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const gameCard = button.closest('.game-card');
                const gameId = gameCard.dataset.game;
                
                this.toggleWishlist(gameId, button);
            });
        });
    }

    toggleWishlist(gameId, button) {
        const wishlist = JSON.parse(localStorage.getItem('botinc_wishlist') || '[]');
        const gameCard = document.querySelector(`[data-game="${gameId}"]`);
        
        if (wishlist.includes(gameId)) {
            // Remove from wishlist
            const index = wishlist.indexOf(gameId);
            wishlist.splice(index, 1);
            localStorage.setItem('botinc_wishlist', JSON.stringify(wishlist));
            
            gameCard.classList.remove('wishlisted');
            button.innerHTML = '<i class="icon-heart"></i><span>Add to Wishlist</span>';
            button.classList.remove('wishlisted');
            
            this.showNotification(`Removed ${gameId} from wishlist`, 'info');
        } else {
            // Add to wishlist
            wishlist.push(gameId);
            localStorage.setItem('botinc_wishlist', JSON.stringify(wishlist));
            
            gameCard.classList.add('wishlisted');
            button.innerHTML = '<i class="icon-heart-filled"></i><span>Wishlisted</span>';
            button.classList.add('wishlisted');
            
            this.showNotification(`Added ${gameId} to wishlist!`, 'success');
        }
        
        // Track event
        window.BotIncWebsite.prototype.trackEvent('wishlist_toggle', {
            game: gameId,
            action: wishlist.includes(gameId) ? 'add' : 'remove'
        });
    }

    // ============================================
    // Game Preview System
    // ============================================

    setupGamePreview() {
        const gameCards = document.querySelectorAll('.game-card');
        
        gameCards.forEach(card => {
            const gameImage = card.querySelector('.game-image');
            
            if (gameImage) {
                gameImage.addEventListener('mouseenter', () => {
                    this.showGamePreview(card);
                });
                
                gameImage.addEventListener('mouseleave', () => {
                    this.hideGamePreview();
                });
            }
        });
    }

    showGamePreview(gameCard) {
        const gameTitle = gameCard.querySelector('.game-title').textContent;
        const gameDescription = gameCard.querySelector('.game-description').textContent;
        const gameGenre = gameCard.querySelector('.game-genre').textContent;
        
        // Create preview tooltip
        let preview = document.getElementById('gamePreview');
        if (!preview) {
            preview = document.createElement('div');
            preview.id = 'gamePreview';
            preview.style.cssText = `
                position: fixed;
                background: rgba(0, 0, 0, 0.95);
                border: 2px solid rgba(0, 255, 136, 0.5);
                border-radius: 10px;
                padding: 1rem;
                max-width: 300px;
                z-index: 1000;
                pointer-events: none;
                opacity: 0;
                transition: opacity 0.3s ease;
                backdrop-filter: blur(10px);
            `;
            document.body.appendChild(preview);
        }
        
        preview.innerHTML = `
            <h4 style="color: #00ff88; margin-bottom: 0.5rem;">${gameTitle}</h4>
            <p style="color: #00ffff; font-size: 0.8rem; margin-bottom: 0.5rem;">${gameGenre}</p>
            <p style="color: rgba(255, 255, 255, 0.8); font-size: 0.9rem; line-height: 1.4;">${gameDescription}</p>
        `;
        
        // Position and show
        document.addEventListener('mousemove', this.updatePreviewPosition);
        preview.style.opacity = '1';
    }

    hideGamePreview() {
        const preview = document.getElementById('gamePreview');
        if (preview) {
            preview.style.opacity = '0';
            document.removeEventListener('mousemove', this.updatePreviewPosition);
        }
    }

    updatePreviewPosition(e) {
        const preview = document.getElementById('gamePreview');
        if (preview) {
            const x = e.clientX + 15;
            const y = e.clientY + 15;
            
            // Keep preview within viewport
            const rect = preview.getBoundingClientRect();
            const maxX = window.innerWidth - rect.width - 10;
            const maxY = window.innerHeight - rect.height - 10;
            
            preview.style.left = Math.min(x, maxX) + 'px';
            preview.style.top = Math.min(y, maxY) + 'px';
        }
    }

    // ============================================
    // Game Card Animations
    // ============================================

    initializeGameCards() {
        const gameCards = document.querySelectorAll('.game-card');
        
        // Intersection Observer for entrance animations
        const cardObserver = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.classList.add('animate-in');
                    }, index * 100);
                }
            });
        }, { threshold: 0.1 });

        gameCards.forEach(card => {
            cardObserver.observe(card);
            
            // Add hover sound effect (if enabled)
            card.addEventListener('mouseenter', () => {
                this.playHoverSound();
            });
        });
    }

    playHoverSound() {
        // Simple hover sound using Web Audio API
        if (typeof AudioContext !== 'undefined') {
            const audioContext = new AudioContext();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);
            
            oscillator.frequency.value = 800;
            oscillator.type = 'square';
            
            gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);
            
            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.1);
        }
    }

    // ============================================
    // Utility Methods
    // ============================================

    showNotification(message, type = 'info') {
        // Use the main website's notification system
        if (window.BotIncWebsite) {
            const website = new window.BotIncWebsite();
            website.showNotification(message, type);
        } else {
            console.log(message);
        }
    }
}

// ============================================
// Enhanced Filter System
// ============================================

class GameFilter {
    constructor() {
        this.activeFilters = new Set();
        this.init();
    }

    init() {
        this.setupMultiSelectFilters();
        this.setupFilterTags();
    }

    setupMultiSelectFilters() {
        const filterTabs = document.querySelectorAll('.filter-tab');
        
        filterTabs.forEach(tab => {
            tab.addEventListener('click', (e) => {
                e.preventDefault();
                
                // Allow multiple selection with Ctrl/Cmd key
                if (e.ctrlKey || e.metaKey) {
                    if (this.activeFilters.has(tab.dataset.filter)) {
                        this.activeFilters.delete(tab.dataset.filter);
                        tab.classList.remove('active');
                    } else {
                        this.activeFilters.add(tab.dataset.filter);
                        tab.classList.add('active');
                    }
                } else {
                    // Single selection (default)
                    filterTabs.forEach(t => t.classList.remove('active'));
                    this.activeFilters.clear();
                    
                    if (tab.dataset.filter !== 'all') {
                        this.activeFilters.add(tab.dataset.filter);
                        tab.classList.add('active');
                    }
                }
                
                this.applyFilters();
            });
        });
    }

    setupFilterTags() {
        // Create filter tags container
        const filtersContainer = document.createElement('div');
        filtersContainer.className = 'active-filters';
        filtersContainer.style.cssText = `
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            margin: 1rem 0;
            justify-content: center;
        `;
        
        const gamesFilter = document.querySelector('.games-filter');
        if (gamesFilter) {
            gamesFilter.appendChild(filtersContainer);
        }
    }

    applyFilters() {
        const gameCards = document.querySelectorAll('.game-card');
        
        gameCards.forEach(card => {
            let shouldShow = true;
            
            if (this.activeFilters.size > 0) {
                shouldShow = Array.from(this.activeFilters).some(filter => 
                    card.classList.contains(filter)
                );
            }
            
            if (shouldShow) {
                card.style.display = '';
                card.classList.add('showing');
            } else {
                card.style.display = 'none';
                card.classList.remove('showing');
            }
        });
        
        this.updateFilterTags();
    }

    updateFilterTags() {
        const tagsContainer = document.querySelector('.active-filters');
        if (!tagsContainer) return;
        
        tagsContainer.innerHTML = '';
        
        this.activeFilters.forEach(filter => {
            const tag = document.createElement('span');
            tag.className = 'filter-tag';
            tag.style.cssText = `
                background: rgba(0, 255, 136, 0.2);
                border: 1px solid rgba(0, 255, 136, 0.5);
                color: #00ff88;
                padding: 0.25rem 0.75rem;
                border-radius: 15px;
                font-size: 0.8rem;
                cursor: pointer;
            `;
            tag.innerHTML = `${filter} &times;`;
            
            tag.addEventListener('click', () => {
                this.removeFilter(filter);
            });
            
            tagsContainer.appendChild(tag);
        });
    }

    removeFilter(filter) {
        this.activeFilters.delete(filter);
        
        // Update UI
        const tab = document.querySelector(`[data-filter="${filter}"]`);
        if (tab) {
            tab.classList.remove('active');
        }
        
        this.applyFilters();
    }
}

// ============================================
// Initialize Games Page
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.games-collection')) {
        new GameInteractions();
        new GameFilter();
    }
});

// Utility function for debouncing
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

// Export for use in other scripts
window.GameInteractions = GameInteractions;
window.GameFilter = GameFilter;