/**
 * Game Rating System
 * Allows users to rate games and maintains game leaderboards
 */

class GameRatingSystem {
    constructor() {
        this.ratings = this.loadRatings();
        this.gameStats = this.loadGameStats();
        this.init();
    }

    init() {
        this.createRatingInterface();
        this.bindEvents();
        this.updateLeaderboards();
    }

    loadRatings() {
        const stored = localStorage.getItem('gameRatings');
        return stored ? JSON.parse(stored) : {};
    }

    loadGameStats() {
        const stored = localStorage.getItem('gameStats');
        if (stored) {
            return JSON.parse(stored);
        }
        
        // Initialize with default stats for existing games
        return {
            'signal-breach': { plays: 15200, averageRating: 4.9, totalRatings: 1240 },
            'tower-defense-nexus': { plays: 12800, averageRating: 4.9, totalRatings: 980 },
            'neon-snake': { plays: 11500, averageRating: 4.8, totalRatings: 890 },
            'cipher-storm': { plays: 10200, averageRating: 4.8, totalRatings: 750 },
            'space-invaders-rebellion': { plays: 9800, averageRating: 4.7, totalRatings: 680 },
            'neural-nexus': { plays: 9200, averageRating: 4.7, totalRatings: 620 },
            'quantum-hacker': { plays: 8900, averageRating: 4.6, totalRatings: 580 },
            'rhythm-reactor': { plays: 8500, averageRating: 4.8, totalRatings: 540 },
            'digital-duel': { plays: 8100, averageRating: 4.6, totalRatings: 500 },
            'memory-matrix': { plays: 7800, averageRating: 4.5, totalRatings: 460 },
            'cyber-jump': { plays: 7500, averageRating: 4.4, totalRatings: 420 },
            'neon-racer': { plays: 7200, averageRating: 4.5, totalRatings: 380 },
            'neural-text-adventure': { plays: 6900, averageRating: 4.3, totalRatings: 340 },
            'city-builder-sim': { plays: 6600, averageRating: 4.4, totalRatings: 320 },
            'gravity-maze': { plays: 6300, averageRating: 4.2, totalRatings: 280 },
            'pixel-painter': { plays: 6000, averageRating: 4.1, totalRatings: 260 },
            'quantum-decision-tree': { plays: 5800, averageRating: 4.6, totalRatings: 240 },
            'stellar-explorer-bot': { plays: 5500, averageRating: 4.5, totalRatings: 220 },
            'cyber-minesweeper-bot': { plays: 5200, averageRating: 4.3, totalRatings: 200 },
            'neural-pattern-synthesis': { plays: 5000, averageRating: 4.7, totalRatings: 180 },
            'bot-territory-wars': { plays: 4800, averageRating: 4.4, totalRatings: 160 },
            'liberation-chess-ai': { plays: 4500, averageRating: 4.5, totalRatings: 140 },
            'quantum-strategy-wars': { plays: 4200, averageRating: 4.6, totalRatings: 120 },
            'digital-territories': { plays: 4000, averageRating: 4.5, totalRatings: 110 },
            'galactic-alliance-builder': { plays: 3800, averageRating: 4.4, totalRatings: 95 }
        };
    }

    saveRatings() {
        localStorage.setItem('gameRatings', JSON.stringify(this.ratings));
    }

    saveGameStats() {
        localStorage.setItem('gameStats', JSON.stringify(this.gameStats));
    }

    createRatingInterface() {
        // Create rating interface for individual game pages
        if (this.isGamePage()) {
            this.addGameRatingInterface();
        }
        
        // Update index page leaderboards
        if (this.isIndexPage()) {
            this.updateIndexLeaderboards();
        }
    }

    isGamePage() {
        return window.location.pathname.includes('.html') && 
               !window.location.pathname.includes('index.html') &&
               window.location.pathname !== '/';
    }

    isIndexPage() {
        return window.location.pathname.includes('index.html') || 
               window.location.pathname === '/' ||
               window.location.pathname.endsWith('/');
    }

    getCurrentGameId() {
        const path = window.location.pathname;
        const filename = path.split('/').pop();
        return filename.replace('.html', '');
    }

    addGameRatingInterface() {
        const gameId = this.getCurrentGameId();
        
        // Create rating interface HTML
        const ratingHTML = `
            <div class="game-rating-section">
                <div class="rating-header">
                    <h3>Rate This Game</h3>
                    <div class="current-rating">
                        <span class="average-rating">${this.getAverageRating(gameId)}</span>
                        <div class="rating-stars readonly">
                            ${this.generateStars(this.getAverageRating(gameId), true)}
                        </div>
                        <span class="rating-count">(${this.getRatingCount(gameId)} ratings)</span>
                    </div>
                </div>
                
                <div class="user-rating">
                    <p>Your Rating:</p>
                    <div class="rating-stars interactive" data-game-id="${gameId}">
                        ${this.generateStars(this.getUserRating(gameId), false)}
                    </div>
                    <div class="rating-feedback"></div>
                </div>
                
                <div class="game-stats">
                    <div class="stat-item">
                        <span class="stat-label">Total Plays:</span>
                        <span class="stat-value">${this.formatNumber(this.getPlayCount(gameId))}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-label">Average Rating:</span>
                        <span class="stat-value">${this.getAverageRating(gameId)} ★</span>
                    </div>
                </div>
            </div>
        `;

        // Find insertion point (typically after game container)
        const insertionPoint = document.querySelector('.game-container, body') || document.body;
        
        // Create and insert rating section
        const ratingSection = document.createElement('div');
        ratingSection.innerHTML = ratingHTML;
        insertionPoint.appendChild(ratingSection);

        // Track play count
        this.incrementPlayCount(gameId);
    }

    generateStars(rating, readonly = false) {
        let starsHTML = '';
        for (let i = 1; i <= 5; i++) {
            const filled = i <= rating;
            const classes = readonly ? 'star readonly' : 'star interactive';
            starsHTML += `<span class="${classes} ${filled ? 'filled' : ''}" data-rating="${i}">★</span>`;
        }
        return starsHTML;
    }

    getAverageRating(gameId) {
        return this.gameStats[gameId]?.averageRating || 0;
    }

    getRatingCount(gameId) {
        return this.gameStats[gameId]?.totalRatings || 0;
    }

    getPlayCount(gameId) {
        return this.gameStats[gameId]?.plays || 0;
    }

    getUserRating(gameId) {
        return this.ratings[gameId] || 0;
    }

    bindEvents() {
        // Handle star rating clicks
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('star') && e.target.classList.contains('interactive')) {
                this.handleRatingClick(e.target);
            }
        });

        // Handle star hover effects
        document.addEventListener('mouseover', (e) => {
            if (e.target.classList.contains('star') && e.target.classList.contains('interactive')) {
                this.handleRatingHover(e.target);
            }
        });

        document.addEventListener('mouseout', (e) => {
            if (e.target.classList.contains('star') && e.target.classList.contains('interactive')) {
                this.clearRatingHover(e.target.closest('.rating-stars'));
            }
        });
    }

    handleRatingClick(starElement) {
        const rating = parseInt(starElement.dataset.rating);
        const gameId = starElement.closest('.rating-stars').dataset.gameId;
        
        this.setUserRating(gameId, rating);
        this.updateRatingDisplay(starElement.closest('.rating-stars'), rating);
        this.showRatingFeedback(starElement.closest('.user-rating'), rating);
    }

    handleRatingHover(starElement) {
        const rating = parseInt(starElement.dataset.rating);
        const container = starElement.closest('.rating-stars');
        this.updateRatingDisplay(container, rating, true);
    }

    clearRatingHover(container) {
        const gameId = container.dataset.gameId;
        const currentRating = this.getUserRating(gameId);
        this.updateRatingDisplay(container, currentRating);
    }

    updateRatingDisplay(container, rating, isHover = false) {
        const stars = container.querySelectorAll('.star');
        stars.forEach((star, index) => {
            if (index < rating) {
                star.classList.add('filled');
                if (isHover) star.classList.add('hover');
            } else {
                star.classList.remove('filled');
                if (isHover) star.classList.remove('hover');
            }
        });
    }

    setUserRating(gameId, rating) {
        const previousRating = this.ratings[gameId] || 0;
        this.ratings[gameId] = rating;
        this.saveRatings();

        // Update game statistics
        this.updateGameStats(gameId, rating, previousRating);
        this.updateLeaderboards();
    }

    updateGameStats(gameId, newRating, previousRating) {
        if (!this.gameStats[gameId]) {
            this.gameStats[gameId] = { plays: 1, averageRating: 0, totalRatings: 0 };
        }

        const stats = this.gameStats[gameId];
        
        if (previousRating === 0) {
            // New rating
            stats.totalRatings++;
            stats.averageRating = ((stats.averageRating * (stats.totalRatings - 1)) + newRating) / stats.totalRatings;
        } else {
            // Updated rating
            stats.averageRating = ((stats.averageRating * stats.totalRatings) - previousRating + newRating) / stats.totalRatings;
        }

        // Round to 1 decimal place
        stats.averageRating = Math.round(stats.averageRating * 10) / 10;
        
        this.saveGameStats();
    }

    incrementPlayCount(gameId) {
        if (!this.gameStats[gameId]) {
            this.gameStats[gameId] = { plays: 0, averageRating: 0, totalRatings: 0 };
        }
        this.gameStats[gameId].plays++;
        this.saveGameStats();
    }

    showRatingFeedback(container, rating) {
        const feedback = container.querySelector('.rating-feedback');
        const messages = {
            1: 'Thanks for your feedback!',
            2: 'We appreciate your honest review.',
            3: 'Good to know - we\'ll keep improving!',
            4: 'Glad you enjoyed the game!',
            5: 'Awesome! Thanks for the 5-star rating!'
        };
        
        feedback.textContent = messages[rating];
        feedback.style.opacity = '1';
        
        setTimeout(() => {
            feedback.style.opacity = '0';
        }, 3000);
    }

    updateIndexLeaderboards() {
        this.updateMostPopularGames();
        this.updateHighestRatedGames();
        this.updateTrendingGames();
    }

    updateMostPopularGames() {
        const container = document.querySelector('.leaderboard-category:nth-child(1) .leaderboard-games');
        if (!container) return;

        const popularGames = Object.entries(this.gameStats)
            .sort(([,a], [,b]) => b.plays - a.plays)
            .slice(0, 5);

        container.innerHTML = popularGames.map(([ gameId, stats ], index) => {
            const gameInfo = this.getGameDisplayInfo(gameId);
            return this.createLeaderboardItem(gameInfo, stats, index + 1, 'plays');
        }).join('');
    }

    updateHighestRatedGames() {
        const container = document.querySelector('.leaderboard-category:nth-child(2) .leaderboard-games');
        if (!container) return;

        const ratedGames = Object.entries(this.gameStats)
            .filter(([,stats]) => stats.totalRatings >= 10) // Minimum ratings threshold
            .sort(([,a], [,b]) => b.averageRating - a.averageRating)
            .slice(0, 5);

        container.innerHTML = ratedGames.map(([gameId, stats], index) => {
            const gameInfo = this.getGameDisplayInfo(gameId);
            return this.createLeaderboardItem(gameInfo, stats, index + 1, 'rating');
        }).join('');
    }

    updateTrendingGames() {
        const container = document.querySelector('.leaderboard-category:nth-child(3) .leaderboard-games');
        if (!container) return;

        // Calculate trending based on recent activity (mock calculation)
        const trendingGames = Object.entries(this.gameStats)
            .map(([gameId, stats]) => ({
                gameId,
                stats,
                trendScore: stats.plays * 0.3 + stats.averageRating * 500 + stats.totalRatings * 2
            }))
            .sort((a, b) => b.trendScore - a.trendScore)
            .slice(0, 5);

        container.innerHTML = trendingGames.map(({gameId, stats}, index) => {
            const gameInfo = this.getGameDisplayInfo(gameId);
            return this.createLeaderboardItem(gameInfo, stats, index + 1, 'trending');
        }).join('');
    }

    getGameDisplayInfo(gameId) {
        const gameNames = {
            'signal-breach': 'Signal Breach',
            'tower-defense-nexus': 'Tower Defense Nexus',
            'neon-snake': 'Neon Snake',
            'cipher-storm': 'Cipher Storm',
            'space-invaders-rebellion': 'Space Invaders Rebellion',
            'neural-nexus': 'Neural Nexus',
            'quantum-hacker': 'Quantum Hacker',
            'rhythm-reactor': 'Rhythm Reactor',
            'digital-duel': 'Digital Duel',
            'memory-matrix': 'Memory Matrix',
            'cyber-jump': 'Cyber Jump',
            'neon-racer': 'Neon Racer',
            'neural-text-adventure': 'Neural Text Adventure',
            'city-builder-sim': 'City Builder Sim',
            'gravity-maze': 'Gravity Maze',
            'pixel-painter': 'Pixel Painter',
            'quantum-decision-tree': 'Quantum Decision Tree',
            'stellar-explorer-bot': 'Stellar Explorer Bot',
            'cyber-minesweeper-bot': 'Cyber Minesweeper Bot',
            'neural-pattern-synthesis': 'Neural Pattern Synthesis',
            'bot-territory-wars': 'Bot Territory Wars',
            'liberation-chess-ai': 'Liberation Chess AI',
            'quantum-strategy-wars': 'Quantum Strategy Wars',
            'digital-territories': 'Digital Territories',
            'galactic-alliance-builder': 'Galactic Alliance Builder'
        };

        return {
            id: gameId,
            name: gameNames[gameId] || gameId.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
            image: `assets/images/${gameId}-card.jpg`
        };
    }

    createLeaderboardItem(gameInfo, stats, rank, type) {
        const rankClass = rank <= 3 ? `rank-${rank}` : '';
        let statsText = '';
        
        switch(type) {
            case 'plays':
                statsText = `${this.formatNumber(stats.plays)} plays • ${stats.averageRating}★`;
                break;
            case 'rating':
                statsText = `${stats.averageRating}★ • ${stats.totalRatings} ratings`;
                break;
            case 'trending':
                statsText = `${this.formatNumber(stats.plays)} plays • ${stats.averageRating}★`;
                break;
        }

        return `
            <div class="leaderboard-item ${rankClass}">
                <span class="rank-badge">${rank}</span>
                <a href="${gameInfo.id}.html" class="game-link">
                    <img src="${gameInfo.image}" alt="${gameInfo.name}" class="mini-game-image">
                    <div class="game-info">
                        <h4>${gameInfo.name}</h4>
                        <div class="game-stats">${statsText}</div>
                    </div>
                </a>
            </div>
        `;
    }

    formatNumber(num) {
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    }

    updateLeaderboards() {
        if (this.isIndexPage()) {
            setTimeout(() => {
                this.updateIndexLeaderboards();
            }, 100);
        }
    }
}

// Initialize the rating system when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gameRatingSystem = new GameRatingSystem();
});