/**
 * BotInc Games - Dynamic Games Loading System
 * Scalable JSON-based game management for 1000+ games
 */

class GamesLoader {
    constructor() {
        this.games = [];
        this.filteredGames = [];
        this.currentPage = 1;
        this.gamesPerPage = 60; // 12x5 grid for maximum density
        this.totalPages = 1;
        this.currentCategory = 'all';
        this.currentSort = 'featured';
        this.searchQuery = '';
        this.loading = false;
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
        const response = await fetch('assets/data/games.json');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        this.games = data.games;
        this.filteredGames = [...this.games];
        this.updatePagination();
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

        // Pagination
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('page-btn')) {
                e.preventDefault();
                this.goToPage(parseInt(e.target.dataset.page));
            }
        });

        // Load more button
        const loadMoreBtn = document.getElementById('load-more');
        if (loadMoreBtn) {
            loadMoreBtn.addEventListener('click', () => {
                this.loadMore();
            });
        }
    }

    filterByCategory(category) {
        this.currentCategory = category;
        this.currentPage = 1;

        if (category === 'all') {
            this.filteredGames = [...this.games];
        } else {
            this.filteredGames = this.games.filter(game => 
                game.category === category || 
                (category === 'ai-exclusive' && game.ai_exclusive) ||
                (category === 'multiplayer' && game.multiplayer)
            );
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

        this.updatePagination();
        this.renderGames();
    }

    searchGames(query) {
        this.searchQuery = query.toLowerCase();
        this.currentPage = 1;

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

        this.updatePagination();
        this.renderGames();
    }

    updatePagination() {
        this.totalPages = Math.ceil(this.filteredGames.length / this.gamesPerPage);
        this.renderPagination();
    }

    goToPage(page) {
        if (page < 1 || page > this.totalPages) return;
        this.currentPage = page;
        this.renderGames();
        this.scrollToGames();
    }

    loadMore() {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.renderGames(true); // append mode
        }
    }

    renderGames(append = false) {
        const container = document.getElementById('games-container');
        if (!container) {
            console.error('Games container not found');
            return;
        }

        const startIndex = (this.currentPage - 1) * this.gamesPerPage;
        const endIndex = startIndex + this.gamesPerPage;
        const pagegames = this.filteredGames.slice(startIndex, endIndex);

        const gamesHTML = pagegames.map(game => this.createGameCard(game)).join('');

        if (append) {
            container.innerHTML += gamesHTML;
        } else {
            container.innerHTML = gamesHTML;
        }

        // Force image sizes after rendering
        this.forceImageSizes();
        this.updateStats();
    }

    createGameCard(game) {
        const badgeClass = this.getBadgeClass(game.badge);
        const difficultyIcon = this.getDifficultyIcon(game.difficulty);
        
        // Use high-quality large images
        const imageSrc = game.thumbnail_large || game.thumbnail;

        return `
            <a href="${game.file}" class="game-card" data-game-id="${game.id}">
                <div class="card-media">
                    <img src="${imageSrc}" alt="${game.title}" class="game-image" loading="lazy">
                    <div class="game-badge ${badgeClass}">${this.formatBadge(game.badge)}</div>
                    ${game.ai_exclusive ? '<div class="ai-exclusive-icon">🤖</div>' : ''}
                    ${game.multiplayer ? '<div class="multiplayer-icon">👥</div>' : ''}
                </div>
                <div class="card-content">
                    <h3 class="game-title">${game.title}</h3>
                    <p class="game-genre">${game.genre}</p>
                    <p class="game-short-desc">${game.description}</p>
                    <div class="game-meta">
                        <div class="rating">
                            <span class="stars">${this.generateStars(game.rating)}</span>
                            <span class="rating-num">${game.rating}</span>
                        </div>
                        <div class="plays">${this.formatPlays(game.plays)}</div>
                        <div class="difficulty">${difficultyIcon}</div>
                    </div>
                </div>
            </a>
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

    updateActiveFilter(category) {
        document.querySelectorAll('.category-filter').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.category === category);
        });
    }

    updateStats() {
        const statsEl = document.getElementById('games-stats');
        if (statsEl) {
            const showing = Math.min(this.currentPage * this.gamesPerPage, this.filteredGames.length);
            statsEl.textContent = `Showing ${showing} of ${this.filteredGames.length} games`;
        }
    }

    renderPagination() {
        const paginationEl = document.getElementById('pagination');
        if (!paginationEl || this.totalPages <= 1) return;

        let paginationHTML = '';
        
        // Previous button
        if (this.currentPage > 1) {
            paginationHTML += `<button class="page-btn" data-page="${this.currentPage - 1}">← Previous</button>`;
        }

        // Page numbers
        for (let i = 1; i <= this.totalPages; i++) {
            if (i === this.currentPage) {
                paginationHTML += `<button class="page-btn active" data-page="${i}">${i}</button>`;
            } else if (i === 1 || i === this.totalPages || Math.abs(i - this.currentPage) <= 2) {
                paginationHTML += `<button class="page-btn" data-page="${i}">${i}</button>`;
            } else if (Math.abs(i - this.currentPage) === 3) {
                paginationHTML += '<span class="pagination-dots">...</span>';
            }
        }

        // Next button
        if (this.currentPage < this.totalPages) {
            paginationHTML += `<button class="page-btn" data-page="${this.currentPage + 1}">Next →</button>`;
        }

        paginationEl.innerHTML = paginationHTML;
    }

    scrollToGames() {
        const gamesSection = document.getElementById('games-section');
        if (gamesSection) {
            gamesSection.scrollIntoView({ behavior: 'smooth' });
        }
    }

    supportsWebP() {
        // Check if browser supports WebP
        const canvas = document.createElement('canvas');
        canvas.width = 1;
        canvas.height = 1;
        return canvas.toDataURL('image/webp').indexOf('data:image/webp') === 0;
    }

    forceImageSizes() {
        // Force all game images to be ultra-compact thumbnails for maximum density
        const images = document.querySelectorAll('.game-image, .card-media img, .games-grid img, #games-container img');
        images.forEach(img => {
            img.style.maxWidth = '65px';
            img.style.maxHeight = '48.75px';
            img.style.width = '65px';
            img.style.height = '48.75px';
            img.style.objectFit = 'cover';
            img.style.display = 'block';
        });
    }

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
    
    // Global image size enforcer - ultra compact for maximum density
    setInterval(() => {
        const images = document.querySelectorAll('img:not(.nav-logo):not(.footer-logo)');
        images.forEach(img => {
            if (img.offsetWidth > 65 || img.offsetHeight > 48.75) {
                img.style.maxWidth = '65px';
                img.style.maxHeight = '48.75px';
                img.style.width = '65px';
                img.style.height = '48.75px';
                img.style.objectFit = 'cover';
                img.style.display = 'block';
            }
        });
    }, 1000);
});