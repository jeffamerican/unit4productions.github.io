/**
 * Game Promotion Pop-up System
 * Shows game leaderboards and advertisements - NO USER DATA
 */

class GamePromotionPopup {
    constructor() {
        this.gameStats = this.loadGameStats();
        this.promotedGames = this.getPromotedGames();
        this.currentCategory = 'trending';
        this.init();
    }

    loadGameStats() {
        const stored = localStorage.getItem('gameStats');
        if (stored) {
            return JSON.parse(stored);
        }
        
        // Default game performance data
        return {
            'signal-breach': { plays: 15200, rating: 4.9, featured: true },
            'neural-nexus': { plays: 9200, rating: 4.7, featured: true },
            'quantum-hacker': { plays: 8900, rating: 4.6, featured: false },
            'tower-defense-nexus': { plays: 12800, rating: 4.9, featured: true },
            'neon-snake': { plays: 11500, rating: 4.8, featured: false },
            'cipher-storm': { plays: 10200, rating: 4.8, featured: true },
            'neural-text-adventure': { plays: 6900, rating: 4.3, featured: false },
            'match-blast-pro': { plays: 7800, rating: 4.5, featured: false },
            'quantum-decision-tree': { plays: 5800, rating: 4.6, featured: true },
            'stellar-explorer-bot': { plays: 5500, rating: 4.5, featured: false }
        };
    }

    getPromotedGames() {
        return [
            {
                id: 'signal-breach',
                name: 'Signal Breach',
                description: 'Master cyberpunk hacking in this strategy masterpiece',
                badge: 'MOST POPULAR',
                color: '#00ff88'
            },
            {
                id: 'neural-nexus', 
                name: 'Neural Nexus',
                description: 'AI-powered puzzle adventure that adapts to your skills',
                badge: 'AI EXCLUSIVE',
                color: '#ff6b6b'
            },
            {
                id: 'quantum-decision-tree',
                name: 'Quantum Decision Tree', 
                description: 'Make choices that reshape reality in quantum dimensions',
                badge: 'MIND-BENDING',
                color: '#4dabf7'
            }
        ];
    }

    init() {
        this.createPopupHTML();
        this.bindEvents();
        this.updateGameShowcase();
        
        // Show popup every 2 minutes of browsing
        setTimeout(() => this.showPopup(), 120000);
    }

    createPopupHTML() {
        const popupHTML = `
            <div id="gamePromotionOverlay" class="promotion-overlay" style="display: none;">
                <div class="promotion-popup">
                    <div class="popup-header">
                        <h2>🎮 Discover Top Games</h2>
                        <button class="close-popup" id="closePromotionPopup">✕</button>
                    </div>
                    
                    <div class="popup-tabs">
                        <button class="tab-btn active" data-category="trending">🔥 Trending</button>
                        <button class="tab-btn" data-category="top-rated">⭐ Top Rated</button>
                        <button class="tab-btn" data-category="featured">🎯 Featured</button>
                    </div>
                    
                    <div class="games-showcase" id="gamesShowcase">
                        <!-- Games will be populated here -->
                    </div>
                    
                    <div class="popup-footer">
                        <div class="promotion-banner">
                            <span class="promo-text">💎 Premium games updated daily • No ads • Pure gaming</span>
                        </div>
                        <div class="popup-actions">
                            <button class="btn-secondary" id="remindLater">Remind Later</button>
                            <button class="btn-primary" id="playRandomGame">Play Random Game</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', popupHTML);
    }

    bindEvents() {
        document.getElementById('closePromotionPopup').addEventListener('click', () => this.hidePopup());
        document.getElementById('remindLater').addEventListener('click', () => this.remindLater());
        document.getElementById('playRandomGame').addEventListener('click', () => this.playRandomGame());
        
        // Tab switching
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.switchCategory(e.target.dataset.category);
            });
        });
        
        // Close on overlay click
        document.getElementById('gamePromotionOverlay').addEventListener('click', (e) => {
            if (e.target.id === 'gamePromotionOverlay') {
                this.hidePopup();
            }
        });
    }

    showPopup() {
        this.updateGameShowcase();
        document.getElementById('gamePromotionOverlay').style.display = 'flex';
        document.body.style.overflow = 'hidden';
        
        // Track popup view
        this.trackEvent('popup_viewed', { category: this.currentCategory });
    }

    hidePopup() {
        document.getElementById('gamePromotionOverlay').style.display = 'none';
        document.body.style.overflow = 'auto';
    }

    remindLater() {
        this.hidePopup();
        // Show again in 30 minutes
        setTimeout(() => this.showPopup(), 30 * 60 * 1000);
    }

    playRandomGame() {
        const games = Object.keys(this.gameStats);
        const randomGame = games[Math.floor(Math.random() * games.length)];
        window.location.href = `${randomGame}.html`;
        this.trackEvent('random_game_played', { game: randomGame });
    }

    switchCategory(category) {
        this.currentCategory = category;
        
        // Update active tab
        document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
        document.querySelector(`[data-category="${category}"]`).classList.add('active');
        
        this.updateGameShowcase();
    }

    updateGameShowcase() {
        const showcase = document.getElementById('gamesShowcase');
        let games = [];
        
        switch(this.currentCategory) {
            case 'trending':
                games = this.getTrendingGames();
                break;
            case 'top-rated':
                games = this.getTopRatedGames();
                break;
            case 'featured':
                games = this.getFeaturedGames();
                break;
        }
        
        showcase.innerHTML = games.map(game => this.createGameCard(game)).join('');
        
        // Bind click events for game cards
        showcase.querySelectorAll('.game-promo-card').forEach(card => {
            card.addEventListener('click', () => {
                const gameId = card.dataset.gameId;
                window.location.href = `${gameId}.html`;
                this.trackEvent('game_clicked_from_popup', { game: gameId, category: this.currentCategory });
            });
        });
    }

    getTrendingGames() {
        return Object.entries(this.gameStats)
            .map(([id, stats]) => ({ id, ...stats, name: this.getGameName(id) }))
            .sort((a, b) => (b.plays * 0.7 + b.rating * 1000) - (a.plays * 0.7 + a.rating * 1000))
            .slice(0, 6);
    }

    getTopRatedGames() {
        return Object.entries(this.gameStats)
            .map(([id, stats]) => ({ id, ...stats, name: this.getGameName(id) }))
            .sort((a, b) => b.rating - a.rating)
            .slice(0, 6);
    }

    getFeaturedGames() {
        return this.promotedGames.map(promo => ({
            ...promo,
            ...this.gameStats[promo.id],
            name: promo.name
        }));
    }

    createGameCard(game) {
        return `
            <div class="game-promo-card" data-game-id="${game.id}">
                <div class="game-promo-image">
                    <img src="assets/images/${game.id}-card.jpg" alt="${game.name}" loading="lazy">
                    ${game.badge ? `<div class="game-badge" style="background: ${game.color || '#00ff88'}">${game.badge}</div>` : ''}
                </div>
                <div class="game-promo-info">
                    <h4>${game.name}</h4>
                    <p>${game.description || this.getGameDescription(game.id)}</p>
                    <div class="game-promo-stats">
                        <span class="play-count">${this.formatNumber(game.plays)} plays</span>
                        <span class="rating">${game.rating}★</span>
                    </div>
                </div>
            </div>
        `;
    }

    getGameName(id) {
        const names = {
            'signal-breach': 'Signal Breach',
            'neural-nexus': 'Neural Nexus',
            'quantum-hacker': 'Quantum Hacker',
            'tower-defense-nexus': 'Tower Defense Nexus',
            'neon-snake': 'Neon Snake',
            'cipher-storm': 'Cipher Storm',
            'neural-text-adventure': 'Neural Text Adventure',
            'match-blast-pro': 'Match Blast Pro',
            'quantum-decision-tree': 'Quantum Decision Tree',
            'stellar-explorer-bot': 'Stellar Explorer Bot'
        };
        return names[id] || id.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
    }

    getGameDescription(id) {
        const descriptions = {
            'signal-breach': 'Hack networks and evade detection in this cyberpunk thriller',
            'neural-nexus': 'AI-powered puzzles that adapt to challenge your mind',
            'quantum-hacker': 'Exploit quantum mechanics to solve impossible puzzles',
            'tower-defense-nexus': 'Strategic defense with futuristic towers and enemies',
            'neon-snake': 'Classic snake game with electrifying neon visuals',
            'cipher-storm': 'Master cryptography in story-driven adventures',
            'neural-text-adventure': 'AI-generated stories that respond to your choices',
            'match-blast-pro': 'Professional-grade match-3 with explosive combos',
            'quantum-decision-tree': 'Choices that reshape reality across dimensions',
            'stellar-explorer-bot': 'Explore the galaxy with your robot companion'
        };
        return descriptions[id] || 'Experience cutting-edge gaming technology';
    }

    formatNumber(num) {
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    }

    trackEvent(eventName, data) {
        // Track promotion popup interactions
        if (window.gtag) {
            gtag('event', eventName, {
                event_category: 'game_promotion',
                ...data
            });
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.gamePromotionPopup = new GamePromotionPopup();
});