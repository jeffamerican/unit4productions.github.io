/**
 * Example Deployment Script for Unit4Productions Gaming Platform
 * This demonstrates how to deploy a complete gaming website
 */

const Unit4GamingPlatform = require('./unit4-gaming-platform');
const SetupGuide = require('./setup-guide');

// Configuration - Replace with your actual values
const config = {
    // Required: GitHub Configuration
    githubToken: 'ghp_your_github_personal_access_token_here',
    owner: 'your-github-username',
    
    // Website Configuration
    siteName: 'Unit4Productions',
    siteUrl: 'https://unit4productions.com',
    description: 'Premium Gaming Experiences - Play Amazing Browser Games',
    
    // Branding
    primaryColor: '#FF6B35',
    secondaryColor: '#1A1A2E',
    accentColor: '#16213E',
    
    // Custom Domain (optional)
    customDomain: 'unit4productions.com',
    
    // Analytics (optional)
    analyticsId: 'G-XXXXXXXXXX',
    
    // Monetization (optional)
    adsenseId: 'ca-pub-xxxxxxxxxxxxxxxx',
    stripePublishableKey: 'pk_live_xxxxx',
    paypalClientId: 'your-paypal-client-id',
    
    // Cloudflare (optional for automatic DNS)
    cloudflareConfig: {
        apiToken: 'your-cloudflare-api-token'
    },
    
    // Monetization settings
    monetization: {
        premiumGamePrice: 4.99,
        subscriptionPrice: 9.99,
        adFreeTier: 2.99
    }
};

// Example games data
const exampleGames = [
    {
        gameData: {
            title: 'Space Adventure',
            slug: 'space-adventure',
            description: 'Embark on an epic journey through space in this thrilling adventure game.',
            type: 'html5',
            tags: ['action', 'space', 'adventure', 'sci-fi'],
            thumbnail: 'space-adventure-thumb.jpg',
            duration: '15-30 minutes',
            rating: 4.5,
            plays: 1250,
            premium: false,
            instructions: 'Use WASD keys to move your spaceship and spacebar to shoot.',
            createdAt: new Date().toISOString()
        },
        gameFiles: [
            {
                path: 'index.html',
                content: generateSampleGameHTML('Space Adventure')
            },
            {
                path: 'game.js',
                content: generateSampleGameJS('Space Adventure')
            },
            {
                path: 'style.css',
                content: generateSampleGameCSS()
            }
        ]
    },
    {
        gameData: {
            title: 'Puzzle Master',
            slug: 'puzzle-master',
            description: 'Challenge your mind with increasingly difficult puzzles.',
            type: 'html5',
            tags: ['puzzle', 'logic', 'brain', 'strategy'],
            thumbnail: 'puzzle-master-thumb.jpg',
            duration: '10-45 minutes',
            rating: 4.2,
            plays: 890,
            premium: true,
            price: 3.99,
            instructions: 'Click and drag pieces to solve the puzzle. Complete all levels to win!',
            createdAt: new Date().toISOString()
        },
        gameFiles: [
            {
                path: 'index.html',
                content: generateSampleGameHTML('Puzzle Master')
            },
            {
                path: 'game.js',
                content: generateSampleGameJS('Puzzle Master')
            },
            {
                path: 'style.css',
                content: generateSampleGameCSS()
            }
        ]
    }
];

/**
 * Main deployment function
 */
async function deployGamingEmpire() {
    console.log('🚀 Deploying Unit4Productions Gaming Empire...');
    console.log('==========================================');
    
    try {
        // Validate configuration
        validateConfiguration(config);
        
        // Initialize platform
        const platform = new Unit4GamingPlatform(config);
        
        // Deploy complete platform
        console.log('📦 Starting complete platform deployment...');
        const deployment = await platform.deployCompletePlatform({
            repoName: 'unit4productions-gaming',
            customDomain: config.customDomain,
            environment: 'production',
            initialGames: exampleGames
        });
        
        // Display results
        displayDeploymentResults(deployment);
        
        // Run validation if deployment succeeded
        if (deployment.status === 'COMPLETED') {
            console.log('🔍 Running validation tests...');
            const domain = config.customDomain || deployment.results.deployment.url.replace('https://', '');
            
            setTimeout(async () => {
                try {
                    const validation = await platform.validateDeployment(domain, 'unit4productions-gaming');
                    displayValidationResults(validation);
                } catch (error) {
                    console.warn('⚠️ Validation tests failed:', error.message);
                }
            }, 30000); // Wait 30 seconds for deployment to propagate
        }
        
        return deployment;
        
    } catch (error) {
        console.error('❌ Deployment failed:', error.message);
        console.error(error.stack);
        process.exit(1);
    }
}

/**
 * Deploy additional game after platform is set up
 */
async function deployAdditionalGame() {
    console.log('🎮 Deploying additional game...');
    
    const platform = new Unit4GamingPlatform(config);
    
    const newGame = {
        title: 'Racing Championship',
        slug: 'racing-championship',
        description: 'Fast-paced racing game with multiple tracks and vehicles.',
        type: 'html5',
        tags: ['racing', 'cars', 'sports', 'competition'],
        thumbnail: 'racing-thumb.jpg',
        duration: '5-20 minutes',
        rating: 4.7,
        plays: 2100,
        premium: false,
        instructions: 'Use arrow keys to steer and spacebar for nitro boost.',
        createdAt: new Date().toISOString()
    };
    
    const gameFiles = [
        {
            path: 'index.html',
            content: generateSampleGameHTML('Racing Championship')
        },
        {
            path: 'game.js',
            content: generateSampleGameJS('Racing Championship')
        },
        {
            path: 'style.css',
            content: generateSampleGameCSS()
        }
    ];
    
    const result = await platform.deployGame(newGame, gameFiles, 'unit4productions-gaming');
    console.log('✅ Game deployed successfully:', result.gameUrl);
    
    return result;
}

/**
 * Validate configuration before deployment
 */
function validateConfiguration(config) {
    const required = ['githubToken', 'owner'];
    const missing = required.filter(key => !config[key]);
    
    if (missing.length > 0) {
        throw new Error(`Missing required configuration: ${missing.join(', ')}`);
    }
    
    if (config.githubToken === 'ghp_your_github_personal_access_token_here') {
        throw new Error('Please replace the GitHub token with your actual token');
    }
    
    if (config.owner === 'your-github-username') {
        throw new Error('Please replace the owner with your actual GitHub username');
    }
    
    console.log('✅ Configuration validated');
}

/**
 * Display deployment results
 */
function displayDeploymentResults(deployment) {
    console.log('\n🎉 DEPLOYMENT COMPLETED!');
    console.log('========================');
    
    if (deployment.results.repository) {
        console.log('📂 Repository:', deployment.results.repository.html_url);
    }
    
    if (deployment.results.deployment) {
        console.log('🌐 Website:', deployment.results.deployment.url);
    }
    
    if (deployment.results.domain) {
        console.log('🔗 Custom Domain:', `https://${deployment.results.domain.domain}`);
        console.log('🔒 SSL Status:', deployment.results.domain.sslStatus.status);
    }
    
    if (deployment.results.games) {
        console.log('🎮 Games Deployed:', deployment.results.games.length);
    }
    
    if (deployment.results.validation) {
        console.log('📊 Validation Score:', `${deployment.results.validation.overallScore}/100`);
    }
    
    console.log('⏱️ Total Time:', calculateDeploymentTime(deployment));
    console.log('\n📋 Next Steps:');
    console.log('1. Configure DNS records (if using custom domain)');
    console.log('2. Wait for SSL certificate (up to 24 hours)');
    console.log('3. Set up Google Analytics and AdSense accounts');
    console.log('4. Upload more games using the deployment pipeline');
    console.log('5. Monitor performance and optimize based on analytics');
}

/**
 * Display validation results
 */
function displayValidationResults(validation) {
    console.log('\n🔍 VALIDATION RESULTS');
    console.log('=====================');
    console.log('Overall Score:', `${validation.overallScore}/100`);
    console.log('Status:', validation.status);
    
    console.log('\n📊 Category Scores:');
    for (const [category, result] of Object.entries(validation.tests)) {
        const percentage = Math.round((result.score / result.maxScore) * 100);
        console.log(`${category}: ${percentage}% (${result.score}/${result.maxScore})`);
    }
    
    // Show failed tests
    const failedTests = [];
    for (const [category, result] of Object.entries(validation.tests)) {
        const failed = result.tests.filter(test => test.status === 'FAIL');
        if (failed.length > 0) {
            failedTests.push({ category, tests: failed });
        }
    }
    
    if (failedTests.length > 0) {
        console.log('\n⚠️ Issues to Address:');
        failedTests.forEach(({ category, tests }) => {
            console.log(`\n${category}:`);
            tests.forEach(test => {
                console.log(`  - ${test.name}: ${test.error || test.details || 'Failed'}`);
            });
        });
    }
}

/**
 * Calculate deployment time
 */
function calculateDeploymentTime(deployment) {
    if (deployment.timestamp && deployment.completedAt) {
        const start = new Date(deployment.timestamp);
        const end = new Date(deployment.completedAt);
        const duration = Math.round((end - start) / 1000);
        return `${Math.floor(duration / 60)}m ${duration % 60}s`;
    }
    return 'Unknown';
}

/**
 * Generate sample game HTML
 */
function generateSampleGameHTML(gameTitle) {
    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${gameTitle} - Unit4Productions</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <div class="game-container">
        <header class="game-header">
            <h1>${gameTitle}</h1>
            <div class="game-controls">
                <button id="startBtn">Start Game</button>
                <button id="pauseBtn">Pause</button>
                <button id="resetBtn">Reset</button>
            </div>
        </header>
        
        <canvas id="gameCanvas" width="800" height="600"></canvas>
        
        <div class="game-info">
            <div class="score">Score: <span id="score">0</span></div>
            <div class="level">Level: <span id="level">1</span></div>
            <div class="lives">Lives: <span id="lives">3</span></div>
        </div>
        
        <div class="instructions">
            <h3>How to Play</h3>
            <p>Use WASD keys to move and spacebar to interact. Have fun!</p>
        </div>
    </div>
    
    <script src="game.js"></script>
</body>
</html>`;
}

/**
 * Generate sample game JavaScript
 */
function generateSampleGameJS(gameTitle) {
    return `// ${gameTitle} Game Logic
class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.score = 0;
        this.level = 1;
        this.lives = 3;
        this.gameRunning = false;
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.gameLoop();
        
        // Track game initialization
        if (typeof trackGameEvent !== 'undefined') {
            trackGameEvent('game_initialized', { game_name: '${gameTitle}' });
        }
    }

    setupEventListeners() {
        document.getElementById('startBtn').onclick = () => this.startGame();
        document.getElementById('pauseBtn').onclick = () => this.pauseGame();
        document.getElementById('resetBtn').onclick = () => this.resetGame();
        
        // Keyboard controls
        document.addEventListener('keydown', (e) => {
            if (!this.gameRunning) return;
            
            switch(e.code) {
                case 'KeyW': this.moveUp(); break;
                case 'KeyS': this.moveDown(); break;
                case 'KeyA': this.moveLeft(); break;
                case 'KeyD': this.moveRight(); break;
                case 'Space': this.interact(); break;
            }
        });
    }

    startGame() {
        this.gameRunning = true;
        document.getElementById('startBtn').textContent = 'Resume';
        
        // Track game start
        if (typeof trackGameStart !== 'undefined') {
            trackGameStart('${gameTitle}', 'html5');
        }
    }

    pauseGame() {
        this.gameRunning = !this.gameRunning;
        document.getElementById('pauseBtn').textContent = 
            this.gameRunning ? 'Pause' : 'Resume';
    }

    resetGame() {
        this.score = 0;
        this.level = 1;
        this.lives = 3;
        this.updateUI();
        this.gameRunning = false;
        document.getElementById('startBtn').textContent = 'Start Game';
        document.getElementById('pauseBtn').textContent = 'Pause';
    }

    updateUI() {
        document.getElementById('score').textContent = this.score;
        document.getElementById('level').textContent = this.level;
        document.getElementById('lives').textContent = this.lives;
    }

    gameLoop() {
        this.update();
        this.render();
        requestAnimationFrame(() => this.gameLoop());
    }

    update() {
        if (!this.gameRunning) return;
        
        // Game logic updates here
        // This is where you'd implement your actual game mechanics
    }

    render() {
        // Clear canvas
        this.ctx.fillStyle = '#000033';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Draw stars background
        this.ctx.fillStyle = '#FFFFFF';
        for (let i = 0; i < 50; i++) {
            const x = Math.random() * this.canvas.width;
            const y = Math.random() * this.canvas.height;
            this.ctx.fillRect(x, y, 1, 1);
        }
        
        // Draw game title
        this.ctx.fillStyle = '#FFAA00';
        this.ctx.font = '24px Arial';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('${gameTitle}', this.canvas.width / 2, 50);
        
        // Draw sample game element (player)
        this.ctx.fillStyle = '#00AA00';
        this.ctx.fillRect(this.canvas.width / 2 - 20, this.canvas.height / 2 - 20, 40, 40);
        
        if (!this.gameRunning) {
            this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
            this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
            
            this.ctx.fillStyle = '#FFFFFF';
            this.ctx.font = '32px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText('PAUSED', this.canvas.width / 2, this.canvas.height / 2);
        }
    }

    moveUp() { /* Implement movement */ }
    moveDown() { /* Implement movement */ }
    moveLeft() { /* Implement movement */ }
    moveRight() { /* Implement movement */ }
    interact() { /* Implement interaction */ }
}

// Initialize game when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    const game = new Game();
});`;
}

/**
 * Generate sample game CSS
 */
function generateSampleGameCSS() {
    return `/* Game Styles */
body {
    margin: 0;
    padding: 0;
    background: #0a0a0a;
    color: #ffffff;
    font-family: 'Arial', sans-serif;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
}

.game-container {
    background: #1a1a1a;
    border-radius: 10px;
    padding: 20px;
    box-shadow: 0 0 20px rgba(0, 255, 255, 0.3);
    text-align: center;
}

.game-header {
    margin-bottom: 20px;
}

.game-header h1 {
    color: #00aaff;
    margin: 0 0 10px 0;
    font-size: 2em;
    text-shadow: 0 0 10px rgba(0, 170, 255, 0.5);
}

.game-controls {
    display: flex;
    justify-content: center;
    gap: 10px;
    margin-bottom: 20px;
}

.game-controls button {
    background: linear-gradient(45deg, #ff6b35, #f7931e);
    border: none;
    color: white;
    padding: 10px 20px;
    border-radius: 5px;
    cursor: pointer;
    font-weight: bold;
    transition: all 0.3s;
}

.game-controls button:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(255, 107, 53, 0.3);
}

#gameCanvas {
    border: 2px solid #00aaff;
    border-radius: 5px;
    background: #000033;
    margin-bottom: 20px;
}

.game-info {
    display: flex;
    justify-content: space-around;
    background: #2a2a2a;
    padding: 10px;
    border-radius: 5px;
    margin-bottom: 20px;
}

.game-info > div {
    font-weight: bold;
}

.game-info span {
    color: #00aaff;
}

.instructions {
    background: #333;
    padding: 15px;
    border-radius: 5px;
    border-left: 4px solid #ff6b35;
}

.instructions h3 {
    color: #ff6b35;
    margin-top: 0;
}

/* Responsive design */
@media (max-width: 768px) {
    .game-container {
        margin: 10px;
        padding: 15px;
    }
    
    #gameCanvas {
        width: 100%;
        max-width: 400px;
        height: auto;
    }
    
    .game-controls {
        flex-direction: column;
        align-items: center;
    }
    
    .game-info {
        flex-direction: column;
        gap: 5px;
    }
}`;
}

// CLI interface
if (require.main === module) {
    const args = process.argv.slice(2);
    
    if (args.includes('--help') || args.includes('-h')) {
        console.log(`
Unit4Productions Gaming Platform Deployment

Usage:
  node example-deployment.js [options]

Options:
  --deploy          Deploy complete platform (default)
  --add-game        Deploy additional game to existing platform
  --validate        Run validation tests on existing deployment
  --guide           Generate setup guide documentation
  --help, -h        Show this help message

Examples:
  node example-deployment.js --deploy
  node example-deployment.js --add-game
  node example-deployment.js --validate
        `);
        process.exit(0);
    }
    
    if (args.includes('--add-game')) {
        deployAdditionalGame();
    } else if (args.includes('--validate')) {
        const platform = new Unit4GamingPlatform(config);
        const domain = config.customDomain || 'your-domain.com';
        platform.validateDeployment(domain, 'unit4productions-gaming')
            .then(displayValidationResults)
            .catch(console.error);
    } else if (args.includes('--guide')) {
        const guide = new SetupGuide();
        console.log(guide.generateDocumentation());
    } else {
        deployGamingEmpire();
    }
}

module.exports = {
    deployGamingEmpire,
    deployAdditionalGame,
    config,
    exampleGames
};`;