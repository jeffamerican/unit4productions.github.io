/**
 * Bot Liberation Games - Interactive Rating System
 * Allows users to rate games with a persistent 5-star system
 */

class RatingSystem {
    constructor() {
        this.ratings = this.loadRatings();
        this.userRatings = this.loadUserRatings();
    }

    // Load all ratings from localStorage
    loadRatings() {
        const stored = localStorage.getItem('botinc-game-ratings');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to parse stored ratings:', e);
            }
        }
        return {};
    }

    // Load user's personal ratings from localStorage
    loadUserRatings() {
        const stored = localStorage.getItem('botinc-user-ratings');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                console.warn('Failed to parse user ratings:', e);
            }
        }
        return {};
    }

    // Save all ratings to localStorage
    saveRatings() {
        try {
            localStorage.setItem('botinc-game-ratings', JSON.stringify(this.ratings));
        } catch (e) {
            console.error('Failed to save ratings:', e);
        }
    }

    // Save user's personal ratings to localStorage
    saveUserRatings() {
        try {
            localStorage.setItem('botinc-user-ratings', JSON.stringify(this.userRatings));
        } catch (e) {
            console.error('Failed to save user ratings:', e);
        }
    }

    // Submit a rating for a game
    rateGame(gameId, rating) {
        if (rating < 1 || rating > 5 || !Number.isInteger(rating)) {
            console.error('Invalid rating:', rating);
            return false;
        }

        // Initialize game rating data if it doesn't exist
        if (!this.ratings[gameId]) {
            this.ratings[gameId] = {
                total: 0,
                count: 0,
                ratings: { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 }
            };
        }

        const gameData = this.ratings[gameId];
        const previousUserRating = this.userRatings[gameId];

        // If user already rated this game, subtract their old rating
        if (previousUserRating) {
            gameData.total -= previousUserRating;
            gameData.count -= 1;
            gameData.ratings[previousUserRating] -= 1;
        }

        // Add the new rating
        gameData.total += rating;
        gameData.count += 1;
        gameData.ratings[rating] += 1;

        // Store user's rating
        this.userRatings[gameId] = rating;

        // Save to localStorage
        this.saveRatings();
        this.saveUserRatings();

        // Trigger update event
        this.triggerRatingUpdate(gameId);

        return true;
    }

    // Get average rating for a game
    getAverageRating(gameId) {
        if (!this.ratings[gameId] || this.ratings[gameId].count === 0) {
            return 0;
        }
        return this.ratings[gameId].total / this.ratings[gameId].count;
    }

    // Get rating count for a game
    getRatingCount(gameId) {
        if (!this.ratings[gameId]) {
            return 0;
        }
        return this.ratings[gameId].count;
    }

    // Get user's rating for a game
    getUserRating(gameId) {
        return this.userRatings[gameId] || 0;
    }

    // Generate interactive star HTML
    generateInteractiveStars(gameId, currentRating = 0, userRating = 0) {
        const stars = [];
        
        for (let i = 1; i <= 5; i++) {
            let starClass = 'star';
            
            // Show user rating if they've rated it
            if (userRating > 0) {
                starClass += i <= userRating ? ' star-filled user-rated' : ' star-empty';
            } else {
                // Show average rating
                starClass += i <= Math.round(currentRating) ? ' star-filled' : ' star-empty';
            }
            
            stars.push(`<span class="${starClass}" data-rating="${i}" data-game-id="${gameId}">★</span>`);
        }
        
        return stars.join('');
    }

    // Generate rating display with count
    generateRatingDisplay(gameId, fallbackRating = 0, fallbackCount = 0) {
        const avgRating = this.getAverageRating(gameId);
        const count = this.getRatingCount(gameId);
        const userRating = this.getUserRating(gameId);
        
        // Use real data if available, otherwise fall back to original data
        const displayRating = count > 0 ? avgRating : fallbackRating;
        const displayCount = count > 0 ? count : fallbackCount;
        
        const starsHtml = this.generateInteractiveStars(gameId, displayRating, userRating);
        const ratingText = displayRating > 0 ? displayRating.toFixed(1) : 'No ratings';
        const countText = displayCount > 0 ? this.formatCount(displayCount) : '';
        
        return {
            starsHtml,
            ratingText,
            countText,
            hasUserRating: userRating > 0
        };
    }

    // Format count for display
    formatCount(count) {
        if (count >= 1000) {
            return (count / 1000).toFixed(1) + 'K';
        }
        return count.toString();
    }

    // Set up event listeners for star interactions
    setupEventListeners() {
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('star')) {
                e.preventDefault();
                e.stopPropagation();
                
                const rating = parseInt(e.target.dataset.rating);
                const gameId = e.target.dataset.gameId;
                
                if (this.rateGame(gameId, rating)) {
                    // Show feedback
                    this.showRatingFeedback(e.target, rating);
                }
            }
        });

        document.addEventListener('mouseover', (e) => {
            if (e.target.classList.contains('star')) {
                this.previewRating(e.target);
            }
        });

        document.addEventListener('mouseout', (e) => {
            if (e.target.classList.contains('star')) {
                this.clearPreview(e.target);
            }
        });
    }

    // Preview rating on hover
    previewRating(starElement) {
        const rating = parseInt(starElement.dataset.rating);
        const gameId = starElement.dataset.gameId;
        const starsContainer = starElement.parentElement;
        const stars = starsContainer.querySelectorAll('.star');
        
        stars.forEach((star, index) => {
            const starRating = index + 1;
            if (starRating <= rating) {
                star.classList.add('star-preview');
            } else {
                star.classList.remove('star-preview');
            }
        });
    }

    // Clear rating preview
    clearPreview(starElement) {
        const starsContainer = starElement.parentElement;
        const stars = starsContainer.querySelectorAll('.star');
        
        stars.forEach(star => {
            star.classList.remove('star-preview');
        });
    }

    // Show rating feedback
    showRatingFeedback(starElement, rating) {
        const gameCard = starElement.closest('.game-card');
        if (!gameCard) return;

        // Create feedback element
        const feedback = document.createElement('div');
        feedback.className = 'rating-feedback';
        feedback.textContent = `Rated ${rating} star${rating !== 1 ? 's' : ''}!`;
        
        gameCard.style.position = 'relative';
        gameCard.appendChild(feedback);

        // Remove feedback after 2 seconds
        setTimeout(() => {
            if (feedback.parentElement) {
                feedback.remove();
            }
        }, 2000);
    }

    // Trigger rating update event
    triggerRatingUpdate(gameId) {
        const event = new CustomEvent('ratingUpdated', {
            detail: { gameId, rating: this.getAverageRating(gameId), count: this.getRatingCount(gameId) }
        });
        document.dispatchEvent(event);
    }

    // Initialize the rating system
    init() {
        this.setupEventListeners();
        console.log('BotInc Rating System initialized');
    }
}

// Create global rating system instance
window.BotIncRatingSystem = new RatingSystem();

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.BotIncRatingSystem.init();
    });
} else {
    window.BotIncRatingSystem.init();
}