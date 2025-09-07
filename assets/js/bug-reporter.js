/**
 * Bug Reporter Module for BotInc Gaming Platform
 * Handles bug reporting modal and GitHub issue creation
 */

class BugReporter {
    constructor() {
        this.currentGame = null;
        this.isSubmitting = false;
        this.rateLimitStorage = 'bugReportCount';
        this.rateLimitWindow = 60 * 60 * 1000; // 1 hour in milliseconds
        this.maxReportsPerHour = 5;
    }

    /**
     * Open bug report modal for a specific game
     */
    static open(gameId, gameTitle, gameFile) {
        const reporter = new BugReporter();
        reporter.currentGame = {
            id: gameId,
            title: gameTitle,
            file: gameFile
        };
        reporter.showModal();
    }

    /**
     * Show the bug report modal
     */
    showModal() {
        // Check rate limit first
        if (!this.checkRateLimit()) {
            this.showRateLimitError();
            return;
        }

        // Create modal HTML
        const modalHTML = this.createModalHTML();
        
        // Add modal to DOM
        const existingModal = document.getElementById('bug-report-modal');
        if (existingModal) {
            existingModal.remove();
        }
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        
        // Store game data in the modal for access during submission
        const modal = document.getElementById('bug-report-modal');
        modal.dataset.gameId = this.currentGame.id;
        modal.dataset.gameTitle = this.currentGame.title;
        modal.dataset.gameFile = this.currentGame.file;
        
        // Setup event listeners
        this.setupModalEvents();
        
        // Focus on description field
        setTimeout(() => {
            const descField = document.getElementById('bug-description');
            if (descField) descField.focus();
        }, 300);
    }

    /**
     * Create the modal HTML structure
     */
    createModalHTML() {
        const mathChallenge = this.generateMathChallenge();
        return `
            <div id="bug-report-modal" class="promotion-overlay">
                <div class="promotion-popup bug-report-popup">
                    <div class="popup-header">
                        <h2>🐛 Report Bug: ${this.currentGame.title}</h2>
                        <button class="close-popup" onclick="BugReporter.close()">&times;</button>
                    </div>
                    <div class="bug-report-content">
                        <form id="bug-report-form">
                            <div class="form-group">
                                <label for="bug-type">Bug Type:</label>
                                <select id="bug-type" required>
                                    <option value="">Select bug type...</option>
                                    <option value="Gameplay">Gameplay Issue</option>
                                    <option value="Graphics">Graphics/Visual Bug</option>
                                    <option value="Audio">Audio Problem</option>
                                    <option value="Performance">Performance Issue</option>
                                    <option value="Controls">Controls/Input Issue</option>
                                    <option value="Loading">Loading/Connection Issue</option>
                                    <option value="Other">Other</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="bug-description">Description:</label>
                                <textarea id="bug-description" rows="4" placeholder="Please describe the bug you encountered..." required></textarea>
                            </div>
                            
                            <div class="form-group">
                                <label for="steps-to-reproduce">Steps to Reproduce (optional):</label>
                                <textarea id="steps-to-reproduce" rows="3" placeholder="1. Click on...&#10;2. Then...&#10;3. Bug occurs"></textarea>
                            </div>
                            
                            <div class="form-group">
                                <label for="user-email">Your Email (optional):</label>
                                <input type="email" id="user-email" placeholder="your.email@example.com">
                                <small>We may contact you for more details about the bug</small>
                            </div>
                            
                            <div class="anti-spam-challenge">
                                <label for="spam-check">Anti-spam: What is ${mathChallenge.question}?</label>
                                <input type="number" id="spam-check" required data-answer="${mathChallenge.answer}">
                            </div>
                        </form>
                    </div>
                    <div class="popup-footer">
                        <div class="popup-actions">
                            <button type="button" class="btn-secondary" onclick="BugReporter.close()">Cancel</button>
                            <button type="button" class="btn-primary" onclick="BugReporter.submit()">
                                <span class="btn-text">Submit Bug Report</span>
                                <span class="btn-loading" style="display: none;">Submitting...</span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Generate a simple math challenge for anti-spam
     */
    generateMathChallenge() {
        const a = Math.floor(Math.random() * 10) + 1;
        const b = Math.floor(Math.random() * 10) + 1;
        const operations = ['+', '-'];
        const op = operations[Math.floor(Math.random() * operations.length)];
        
        let answer;
        let question;
        
        if (op === '+') {
            answer = a + b;
            question = `${a} + ${b}`;
        } else {
            // Ensure positive result for subtraction
            const larger = Math.max(a, b);
            const smaller = Math.min(a, b);
            answer = larger - smaller;
            question = `${larger} - ${smaller}`;
        }
        
        return { question, answer };
    }

    /**
     * Setup modal event listeners
     */
    setupModalEvents() {
        // Close modal when clicking overlay
        const modal = document.getElementById('bug-report-modal');
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                BugReporter.close();
            }
        });

        // Keyboard navigation
        document.addEventListener('keydown', this.handleKeydown.bind(this));
    }

    /**
     * Handle keyboard events
     */
    handleKeydown(e) {
        if (e.key === 'Escape') {
            BugReporter.close();
        }
    }

    /**
     * Close the modal
     */
    static close() {
        const modal = document.getElementById('bug-report-modal');
        if (modal) {
            modal.style.animation = 'fadeOut 0.3s ease';
            setTimeout(() => {
                modal.remove();
            }, 300);
        }
        
        // Remove keyboard event listener
        document.removeEventListener('keydown', BugReporter.prototype.handleKeydown);
    }

    /**
     * Submit the bug report
     */
    static async submit() {
        // Get the current game data from the modal
        const modal = document.getElementById('bug-report-modal');
        if (!modal) {
            console.error('Bug report modal not found');
            return;
        }
        
        // Create reporter instance and set game data from stored dataset
        const reporter = new BugReporter();
        reporter.currentGame = {
            id: modal.dataset.gameId,
            title: modal.dataset.gameTitle,
            file: modal.dataset.gameFile
        };
        
        console.log('Modal dataset:', {
            gameId: modal.dataset.gameId,
            gameTitle: modal.dataset.gameTitle,
            gameFile: modal.dataset.gameFile
        });
        console.log('Reporter currentGame:', reporter.currentGame);
        
        await reporter.handleSubmit();
    }

    /**
     * Handle form submission
     */
    async handleSubmit() {
        console.log('Bug report submission started');
        
        if (this.isSubmitting) {
            console.log('Already submitting, returning');
            return;
        }

        const form = document.getElementById('bug-report-form');
        if (!form) {
            console.error('Bug report form not found');
            return;
        }
        
        console.log('Current game:', this.currentGame);
        
        // Check if currentGame is set
        if (!this.currentGame) {
            console.error('No current game data available');
            this.showError('Unable to identify the game. Please try again.');
            return;
        }
        
        // Validate form
        if (!this.validateForm()) {
            console.log('Form validation failed');
            return;
        }
        
        console.log('Form validation passed');

        this.isSubmitting = true;
        this.showLoading(true);

        try {
            // Prepare bug report data
            const bugData = {
                gameId: this.currentGame.id,
                gameTitle: this.currentGame.title,
                gameFile: this.currentGame.file,
                bugType: document.getElementById('bug-type').value,
                description: document.getElementById('bug-description').value,
                stepsToReproduce: document.getElementById('steps-to-reproduce').value,
                userEmail: document.getElementById('user-email').value,
                timestamp: new Date().toISOString(),
                userAgent: navigator.userAgent,
                url: window.location.href
            };

            // Submit to GitHub (via our integration)
            await GitHubIntegration.createIssue(bugData);
            
            // Update rate limit
            this.updateRateLimit();
            
            // Show success and close
            this.showSuccess();
            setTimeout(() => {
                BugReporter.close();
            }, 2000);

        } catch (error) {
            console.error('Bug report submission failed:', error);
            this.showError('Failed to submit bug report. Please try again later.');
        } finally {
            this.isSubmitting = false;
            this.showLoading(false);
        }
    }

    /**
     * Validate the form
     */
    validateForm() {
        const bugType = document.getElementById('bug-type').value;
        const description = document.getElementById('bug-description').value.trim();
        const spamAnswer = document.getElementById('spam-check').value;
        const expectedAnswer = document.getElementById('spam-check').dataset.answer;

        if (!bugType) {
            this.showError('Please select a bug type.');
            return false;
        }

        if (!description || description.length < 10) {
            this.showError('Please provide a description of at least 10 characters.');
            return false;
        }

        if (!spamAnswer || parseInt(spamAnswer) !== parseInt(expectedAnswer)) {
            this.showError('Please answer the anti-spam question correctly.');
            return false;
        }

        return true;
    }

    /**
     * Check rate limiting
     */
    checkRateLimit() {
        const now = Date.now();
        const stored = localStorage.getItem(this.rateLimitStorage);
        
        if (!stored) return true;
        
        const data = JSON.parse(stored);
        const windowStart = now - this.rateLimitWindow;
        
        // Filter reports within the current window
        const recentReports = data.reports.filter(time => time > windowStart);
        
        return recentReports.length < this.maxReportsPerHour;
    }

    /**
     * Update rate limit tracking
     */
    updateRateLimit() {
        const now = Date.now();
        const windowStart = now - this.rateLimitWindow;
        
        let data = { reports: [] };
        const stored = localStorage.getItem(this.rateLimitStorage);
        if (stored) {
            data = JSON.parse(stored);
            // Keep only reports within the window
            data.reports = data.reports.filter(time => time > windowStart);
        }
        
        // Add current report
        data.reports.push(now);
        
        localStorage.setItem(this.rateLimitStorage, JSON.stringify(data));
    }

    /**
     * Show rate limit error
     */
    showRateLimitError() {
        alert('You have reached the maximum number of bug reports per hour (5). Please try again later.');
    }

    /**
     * Show loading state
     */
    showLoading(show) {
        const btnText = document.querySelector('.btn-text');
        const btnLoading = document.querySelector('.btn-loading');
        const submitBtn = document.querySelector('.btn-primary');
        
        if (show) {
            btnText.style.display = 'none';
            btnLoading.style.display = 'inline';
            submitBtn.disabled = true;
        } else {
            btnText.style.display = 'inline';
            btnLoading.style.display = 'none';
            submitBtn.disabled = false;
        }
    }

    /**
     * Show success message
     */
    showSuccess() {
        const content = document.querySelector('.bug-report-content');
        content.innerHTML = `
            <div class="success-message">
                <h3>✅ Bug Report Submitted Successfully!</h3>
                <p>Thank you for helping improve ${this.currentGame.title}!</p>
                <p>Our bot team (@claude) has been notified and will investigate this issue.</p>
            </div>
        `;
    }

    /**
     * Show error message
     */
    showError(message) {
        // Remove existing error messages
        const existingError = document.querySelector('.error-message');
        if (existingError) {
            existingError.remove();
        }

        // Show new error
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.innerHTML = `<p>❌ ${message}</p>`;
        
        const content = document.querySelector('.bug-report-content');
        content.insertBefore(errorDiv, content.firstChild);

        // Auto-remove error after 5 seconds
        setTimeout(() => {
            errorDiv.remove();
        }, 5000);
    }
}

// Make BugReporter globally available
window.BugReporter = BugReporter;