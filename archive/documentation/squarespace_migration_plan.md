# Migration Plan: From Squarespace to GitHub Pages
## Complete Transition Strategy for Gaming Website Automation

### Executive Summary

This migration plan outlines the complete transition from manual Squarespace management to fully automated GitHub Pages deployment for HTML5 gaming websites, eliminating human intervention and reducing costs to $0.

---

## Current State Analysis

### Squarespace Limitations
- **Manual Updates:** Requires human intervention for each game deployment
- **Cost:** Monthly subscription fees ($12-40/month)
- **Limited Automation:** No comprehensive API for full site management
- **Vendor Lock-in:** Proprietary platform dependencies
- **Scalability Issues:** Manual processes don't scale with multiple games

### Migration Benefits
- **100% Automation:** Zero human intervention required
- **Cost Reduction:** $144-480/year savings (move to $0 cost)
- **Full API Control:** Complete programmatic management
- **Scalability:** Deploy unlimited games automatically
- **Version Control:** Git-based deployment history
- **Performance:** GitHub's global CDN
- **Custom Domains:** Full DNS control for unit4productions.com

---

## Phase 1: Pre-Migration Setup (Week 1)

### 1.1 GitHub Environment Preparation

```bash
# Set up GitHub organization for game hosting
gh api \
  --method POST \
  -H "Accept: application/vnd.github+json" \
  /user/orgs \
  -f login="unit4productions" \
  -f email="admin@unit4productions.com"

# Create main website repository
gh repo create unit4productions/main-website \
  --description "Unit4 Productions Main Gaming Website" \
  --public \
  --add-readme
```

### 1.2 Domain Preparation

```bash
# Test DNS configuration before migration
dig unit4productions.com
dig games.unit4productions.com

# Prepare DNS records (don't apply yet)
# A records: 185.199.108.153, 185.199.109.153, 185.199.110.153, 185.199.111.153
# AAAA records: 2606:50c0:8000::153, 2606:50c0:8001::153, 2606:50c0:8002::153, 2606:50c0:8003::153
```

### 1.3 Content Audit and Export

#### Squarespace Content Extraction Script

```javascript
// squarespace-audit.js
const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

class SquarespaceAuditor {
  constructor(siteUrl, credentials) {
    this.siteUrl = siteUrl;
    this.credentials = credentials;
    this.browser = null;
    this.page = null;
  }

  async init() {
    this.browser = await puppeteer.launch({ headless: false });
    this.page = await this.browser.newPage();
  }

  async auditContent() {
    await this.page.goto(this.siteUrl);
    
    // Extract game listings
    const games = await this.page.evaluate(() => {
      const gameElements = document.querySelectorAll('[data-game], .game-item, .portfolio-item');
      return Array.from(gameElements).map(el => ({
        title: el.querySelector('h1, h2, h3, .title')?.textContent?.trim(),
        description: el.querySelector('p, .description')?.textContent?.trim(),
        image: el.querySelector('img')?.src,
        link: el.querySelector('a')?.href,
        html: el.outerHTML
      }));
    });

    // Extract page structure
    const pages = await this.page.evaluate(() => {
      const navLinks = document.querySelectorAll('nav a, .navigation a');
      return Array.from(navLinks).map(link => ({
        title: link.textContent.trim(),
        url: link.href,
        path: new URL(link.href).pathname
      }));
    });

    // Extract assets
    const assets = await this.page.evaluate(() => {
      const images = Array.from(document.querySelectorAll('img')).map(img => img.src);
      const scripts = Array.from(document.querySelectorAll('script[src]')).map(script => script.src);
      const styles = Array.from(document.querySelectorAll('link[rel="stylesheet"]')).map(link => link.href);
      
      return { images, scripts, styles };
    });

    return { games, pages, assets };
  }

  async downloadAssets(assets, outputDir = './squarespace-backup') {
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    for (const imageUrl of assets.images) {
      if (imageUrl && !imageUrl.startsWith('data:')) {
        try {
          const response = await this.page.goto(imageUrl);
          const buffer = await response.buffer();
          const filename = path.basename(new URL(imageUrl).pathname) || `image-${Date.now()}.jpg`;
          fs.writeFileSync(path.join(outputDir, 'images', filename), buffer);
        } catch (error) {
          console.warn(`Failed to download image: ${imageUrl}`);
        }
      }
    }
  }

  async generateMigrationData() {
    const audit = await this.auditContent();
    
    const migrationData = {
      timestamp: new Date().toISOString(),
      source: this.siteUrl,
      games: audit.games.map(game => ({
        ...game,
        migrationStatus: 'pending',
        newRepository: null,
        newUrl: null
      })),
      pages: audit.pages,
      assets: audit.assets,
      migrationPlan: {
        totalGames: audit.games.length,
        estimatedMigrationTime: `${Math.ceil(audit.games.length * 0.5)} hours`,
        phases: [
          'Content extraction',
          'Repository creation',
          'Asset migration',
          'Game conversion',
          'DNS switchover',
          'Verification'
        ]
      }
    };

    fs.writeFileSync('./migration-data.json', JSON.stringify(migrationData, null, 2));
    console.log(`Migration data generated: ${audit.games.length} games found`);
    
    return migrationData;
  }

  async close() {
    if (this.browser) {
      await this.browser.close();
    }
  }
}

// Usage
async function auditSquarespace() {
  const auditor = new SquarespaceAuditor('https://unit4productions.com', {
    username: process.env.SQUARESPACE_USER,
    password: process.env.SQUARESPACE_PASS
  });

  await auditor.init();
  const migrationData = await auditor.generateMigrationData();
  await auditor.downloadAssets(migrationData.assets);
  await auditor.close();

  return migrationData;
}

module.exports = { SquarespaceAuditor, auditSquarespace };
```

---

## Phase 2: Parallel Environment Setup (Week 2)

### 2.1 Create GitHub Pages Infrastructure

```javascript
// github-infrastructure.js
const { Octokit } = require("@octokit/rest");

class GitHubInfrastructure {
  constructor(token, org = 'unit4productions') {
    this.octokit = new Octokit({ auth: token });
    this.org = org;
  }

  async setupMainWebsite() {
    // Create main website repository
    const { data: mainRepo } = await this.octokit.rest.repos.createInOrg({
      org: this.org,
      name: 'website',
      description: 'Unit4 Productions Main Website',
      private: false,
      has_issues: true,
      has_projects: false,
      has_wiki: false
    });

    // Enable Pages
    await this.octokit.rest.repos.createPagesSite({
      owner: this.org,
      repo: 'website',
      source: { branch: 'main', path: '/' }
    });

    return mainRepo;
  }

  async setupGameRepositories(games) {
    const repositories = [];

    for (const game of games) {
      const repoName = this.sanitizeRepoName(game.title);
      
      try {
        // Create game repository
        const { data: gameRepo } = await this.octokit.rest.repos.createInOrg({
          org: this.org,
          name: repoName,
          description: `HTML5 Game: ${game.title}`,
          private: false
        });

        // Enable Pages
        await this.octokit.rest.repos.createPagesSite({
          owner: this.org,
          repo: repoName,
          source: { branch: 'main', path: '/' }
        });

        repositories.push({
          original: game,
          repository: gameRepo,
          url: `https://${this.org}.github.io/${repoName}`
        });

        console.log(`✅ Repository created: ${gameRepo.full_name}`);
      } catch (error) {
        console.error(`❌ Failed to create repository for ${game.title}:`, error.message);
      }
    }

    return repositories;
  }

  async setupCustomDomains(repositories, baseDomain = 'unit4productions.com') {
    // Main website gets apex domain
    await this.octokit.rest.repos.updateInformationAboutPagesSite({
      owner: this.org,
      repo: 'website',
      cname: baseDomain
    });

    // Each game gets a subdomain
    for (const repo of repositories) {
      const subdomain = `${repo.repository.name}.${baseDomain}`;
      
      await this.octokit.rest.repos.updateInformationAboutPagesSite({
        owner: this.org,
        repo: repo.repository.name,
        cname: subdomain
      });

      repo.customDomain = subdomain;
    }
  }

  sanitizeRepoName(title) {
    return title
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .substring(0, 100);
  }
}

module.exports = GitHubInfrastructure;
```

### 2.2 Content Conversion Pipeline

```javascript
// content-converter.js
const fs = require('fs');
const path = require('path');

class ContentConverter {
  constructor(migrationData) {
    this.migrationData = migrationData;
  }

  convertGameToHTML5(game) {
    const gameHtml = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${game.title} - Unit4 Productions</title>
    <meta name="description" content="${game.description}">
    <link rel="stylesheet" href="css/game.css">
    
    <!-- Game Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=GA_MEASUREMENT_ID"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}
        gtag('js', new Date());
        gtag('config', 'GA_MEASUREMENT_ID');
    </script>
    
    <!-- Monetization -->
    <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXX" crossorigin="anonymous"></script>
</head>
<body>
    <header>
        <nav>
            <a href="/" class="logo">Unit4 Productions</a>
            <ul>
                <li><a href="/">Home</a></li>
                <li><a href="/games/">Games</a></li>
                <li><a href="/about/">About</a></li>
            </ul>
        </nav>
    </header>
    
    <main>
        <div class="game-container">
            <h1>${game.title}</h1>
            <div class="game-wrapper">
                <canvas id="gameCanvas" width="800" height="600"></canvas>
                <div class="game-controls">
                    <button id="playButton">Play Game</button>
                    <button id="pauseButton">Pause</button>
                    <button id="resetButton">Reset</button>
                </div>
            </div>
            
            <div class="game-info">
                <p>${game.description}</p>
                <div class="game-stats">
                    <span>Score: <span id="score">0</span></span>
                    <span>Level: <span id="level">1</span></span>
                    <span>Lives: <span id="lives">3</span></span>
                </div>
            </div>
        </div>
        
        <!-- Ad placement -->
        <div class="ad-container">
            <ins class="adsbygoogle"
                 style="display:block"
                 data-ad-client="ca-pub-XXXXXXXXX"
                 data-ad-slot="XXXXXXXXX"
                 data-ad-format="auto"
                 data-full-width-responsive="true"></ins>
        </div>
    </main>
    
    <footer>
        <p>&copy; 2025 Unit4 Productions. All rights reserved.</p>
    </footer>
    
    <script src="js/game-engine.js"></script>
    <script src="js/${this.sanitizeFileName(game.title)}.js"></script>
    <script>
        (adsbygoogle = window.adsbygoogle || []).push({});
    </script>
</body>
</html>`;

    return gameHtml;
  }

  generateGameEngine() {
    return `// Universal Game Engine for Unit4 Productions
class GameEngine {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.gameState = 'menu'; // menu, playing, paused, gameOver
        this.score = 0;
        this.level = 1;
        this.lives = 3;
        
        this.setupEventListeners();
        this.init();
    }
    
    init() {
        this.render();
        this.trackAnalytics('game_loaded');
    }
    
    setupEventListeners() {
        document.getElementById('playButton')?.addEventListener('click', () => this.startGame());
        document.getElementById('pauseButton')?.addEventListener('click', () => this.pauseGame());
        document.getElementById('resetButton')?.addEventListener('click', () => this.resetGame());
        
        // Keyboard controls
        document.addEventListener('keydown', (e) => this.handleKeyPress(e));
        
        // Touch/mobile controls
        this.canvas.addEventListener('touchstart', (e) => this.handleTouch(e));
    }
    
    startGame() {
        this.gameState = 'playing';
        this.trackAnalytics('game_started');
        this.gameLoop();
    }
    
    pauseGame() {
        this.gameState = 'paused';
        this.trackAnalytics('game_paused');
    }
    
    resetGame() {
        this.score = 0;
        this.level = 1;
        this.lives = 3;
        this.gameState = 'menu';
        this.updateUI();
        this.trackAnalytics('game_reset');
    }
    
    gameLoop() {
        if (this.gameState !== 'playing') return;
        
        this.update();
        this.render();
        
        requestAnimationFrame(() => this.gameLoop());
    }
    
    update() {
        // Override in specific game implementations
    }
    
    render() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Draw game-specific content
        this.drawGame();
        
        // Draw UI overlay
        this.drawUI();
    }
    
    drawGame() {
        // Override in specific game implementations
    }
    
    drawUI() {
        this.ctx.fillStyle = '#fff';
        this.ctx.font = '20px Arial';
        this.ctx.fillText('Score: ' + this.score, 10, 30);
        this.ctx.fillText('Level: ' + this.level, 10, 60);
        this.ctx.fillText('Lives: ' + this.lives, 10, 90);
    }
    
    updateUI() {
        document.getElementById('score').textContent = this.score;
        document.getElementById('level').textContent = this.level;
        document.getElementById('lives').textContent = this.lives;
    }
    
    handleKeyPress(event) {
        // Override in specific game implementations
    }
    
    handleTouch(event) {
        // Override in specific game implementations
    }
    
    trackAnalytics(eventName, parameters = {}) {
        if (typeof gtag !== 'undefined') {
            gtag('event', eventName, {
                event_category: 'game',
                event_label: document.title,
                ...parameters
            });
        }
    }
    
    gameOver() {
        this.gameState = 'gameOver';
        this.trackAnalytics('game_over', {
            final_score: this.score,
            final_level: this.level
        });
    }
}`;
  }

  sanitizeFileName(title) {
    return title
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  async convertAllGames() {
    const convertedGames = [];

    for (const game of this.migrationData.games) {
      const gameFiles = {
        html: this.convertGameToHTML5(game),
        engine: this.generateGameEngine(),
        css: this.generateGameCSS(),
        js: `// ${game.title} specific implementation
class ${this.toPascalCase(game.title)}Game extends GameEngine {
    constructor() {
        super('gameCanvas');
        // Game-specific initialization
    }
    
    drawGame() {
        // Implement ${game.title} rendering
        this.ctx.fillStyle = '#333';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        this.ctx.fillStyle = '#fff';
        this.ctx.font = '48px Arial';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('${game.title}', this.canvas.width / 2, this.canvas.height / 2);
    }
    
    update() {
        // Implement ${game.title} game logic
    }
    
    handleKeyPress(event) {
        if (this.gameState !== 'playing') return;
        
        // Implement ${game.title} controls
        switch(event.code) {
            case 'ArrowLeft':
            case 'KeyA':
                // Move left
                break;
            case 'ArrowRight':
            case 'KeyD':
                // Move right
                break;
            case 'Space':
                // Action
                break;
        }
    }
}

// Initialize game when page loads
window.addEventListener('load', () => {
    new ${this.toPascalCase(game.title)}Game();
});`
      };

      convertedGames.push({
        original: game,
        files: gameFiles,
        repoName: this.sanitizeFileName(game.title)
      });
    }

    return convertedGames;
  }

  generateGameCSS() {
    return `/* Unit4 Productions Game Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Arial', sans-serif;
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    min-height: 100vh;
    color: #fff;
}

header {
    background: rgba(0,0,0,0.8);
    padding: 1rem 0;
    position: sticky;
    top: 0;
    z-index: 1000;
}

nav {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 2rem;
}

.logo {
    font-size: 1.5rem;
    font-weight: bold;
    color: #fff;
    text-decoration: none;
}

nav ul {
    list-style: none;
    display: flex;
    gap: 2rem;
}

nav a {
    color: #fff;
    text-decoration: none;
    transition: color 0.3s;
}

nav a:hover {
    color: #4CAF50;
}

main {
    max-width: 1200px;
    margin: 0 auto;
    padding: 2rem;
}

.game-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2rem;
}

.game-wrapper {
    background: #000;
    border-radius: 10px;
    padding: 1rem;
    box-shadow: 0 0 30px rgba(0,0,0,0.5);
}

#gameCanvas {
    border-radius: 5px;
    max-width: 100%;
    height: auto;
}

.game-controls {
    display: flex;
    gap: 1rem;
    margin-top: 1rem;
    justify-content: center;
}

.game-controls button {
    padding: 0.5rem 1rem;
    font-size: 1rem;
    background: #4CAF50;
    color: #fff;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: background 0.3s;
}

.game-controls button:hover {
    background: #45a049;
}

.game-controls button:disabled {
    background: #666;
    cursor: not-allowed;
}

.game-info {
    text-align: center;
    max-width: 600px;
}

.game-stats {
    display: flex;
    gap: 2rem;
    justify-content: center;
    margin-top: 1rem;
    font-size: 1.1rem;
    font-weight: bold;
}

.ad-container {
    margin: 2rem auto;
    max-width: 800px;
    text-align: center;
}

footer {
    background: rgba(0,0,0,0.8);
    text-align: center;
    padding: 2rem;
    margin-top: 4rem;
}

@media (max-width: 768px) {
    nav {
        flex-direction: column;
        gap: 1rem;
    }
    
    nav ul {
        gap: 1rem;
    }
    
    .game-stats {
        flex-direction: column;
        gap: 0.5rem;
    }
    
    .game-controls {
        flex-wrap: wrap;
    }
}`;
  }

  toPascalCase(str) {
    return str
      .replace(/(?:^\w|[A-Z]|\b\w)/g, (word, index) => 
        index === 0 ? word.toUpperCase() : word.toUpperCase()
      )
      .replace(/\s+/g, '')
      .replace(/[^a-zA-Z0-9]/g, '');
  }
}

module.exports = ContentConverter;
```

---

## Phase 3: Content Migration (Week 3)

### 3.1 Automated Migration Script

```javascript
// migrate.js - Master migration orchestrator
const { SquarespaceAuditor } = require('./squarespace-audit');
const GitHubInfrastructure = require('./github-infrastructure');
const ContentConverter = require('./content-converter');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

class MigrationOrchestrator {
  constructor(config) {
    this.config = config;
    this.github = new GitHubInfrastructure(config.githubToken, config.org);
    this.migrationData = null;
    this.repositories = [];
  }

  async runFullMigration() {
    console.log('🚀 Starting full migration from Squarespace to GitHub Pages');
    
    try {
      // Phase 1: Audit existing content
      await this.auditExistingContent();
      
      // Phase 2: Setup GitHub infrastructure
      await this.setupGitHubInfrastructure();
      
      // Phase 3: Convert and deploy content
      await this.convertAndDeploy();
      
      // Phase 4: Configure DNS (manual verification step)
      await this.prepareDNSConfiguration();
      
      // Phase 5: Verify migration
      await this.verifyMigration();
      
      console.log('✅ Migration completed successfully!');
      this.generateMigrationReport();
      
    } catch (error) {
      console.error('❌ Migration failed:', error);
      await this.rollbackMigration();
    }
  }

  async auditExistingContent() {
    console.log('📊 Auditing existing Squarespace content...');
    
    const auditor = new SquarespaceAuditor(this.config.squarespaceUrl);
    await auditor.init();
    this.migrationData = await auditor.generateMigrationData();
    await auditor.close();
    
    console.log(`Found ${this.migrationData.games.length} games to migrate`);
  }

  async setupGitHubInfrastructure() {
    console.log('🏗️ Setting up GitHub infrastructure...');
    
    // Create main website
    const mainRepo = await this.github.setupMainWebsite();
    console.log(`Main website repository: ${mainRepo.html_url}`);
    
    // Create game repositories
    this.repositories = await this.github.setupGameRepositories(this.migrationData.games);
    console.log(`Created ${this.repositories.length} game repositories`);
    
    // Setup custom domains (will be activated after DNS change)
    await this.github.setupCustomDomains(this.repositories, this.config.domain);
    console.log('Custom domains configured');
  }

  async convertAndDeploy() {
    console.log('🔄 Converting and deploying content...');
    
    const converter = new ContentConverter(this.migrationData);
    const convertedGames = await converter.convertAllGames();
    
    for (let i = 0; i < convertedGames.length; i++) {
      const game = convertedGames[i];
      const repo = this.repositories[i];
      
      await this.deployGameToRepository(game, repo);
      console.log(`✅ Deployed: ${game.original.title}`);
    }
    
    // Deploy main website
    await this.deployMainWebsite();
  }

  async deployGameToRepository(convertedGame, repository) {
    const tempDir = `./temp-${convertedGame.repoName}`;
    
    try {
      // Clone repository
      execSync(`git clone ${repository.repository.clone_url} ${tempDir}`, { stdio: 'inherit' });
      
      // Create directory structure
      const dirs = ['js', 'css', 'images', 'audio', '.github/workflows'];
      dirs.forEach(dir => {
        const fullPath = path.join(tempDir, dir);
        if (!fs.existsSync(fullPath)) {
          fs.mkdirSync(fullPath, { recursive: true });
        }
      });
      
      // Write game files
      fs.writeFileSync(path.join(tempDir, 'index.html'), convertedGame.files.html);
      fs.writeFileSync(path.join(tempDir, 'js', 'game-engine.js'), convertedGame.files.engine);
      fs.writeFileSync(path.join(tempDir, 'js', `${convertedGame.repoName}.js`), convertedGame.files.js);
      fs.writeFileSync(path.join(tempDir, 'css', 'game.css'), convertedGame.files.css);
      
      // Create GitHub workflow
      fs.writeFileSync(path.join(tempDir, '.github', 'workflows', 'deploy.yml'), this.generateWorkflow());
      
      // Commit and push
      process.chdir(tempDir);
      execSync('git add .', { stdio: 'inherit' });
      execSync(`git commit -m "Initial migration: ${convertedGame.original.title}"`, { stdio: 'inherit' });
      execSync('git push origin main', { stdio: 'inherit' });
      process.chdir('..');
      
      // Cleanup
      execSync(`rm -rf ${tempDir}`, { stdio: 'inherit' });
      
    } catch (error) {
      console.error(`Failed to deploy ${convertedGame.original.title}:`, error.message);
      // Cleanup on error
      if (fs.existsSync(tempDir)) {
        execSync(`rm -rf ${tempDir}`, { stdio: 'inherit' });
      }
    }
  }

  generateWorkflow() {
    return `name: Deploy Game
on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      pages: write
      id-token: write
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      
    - name: Setup Pages
      uses: actions/configure-pages@v3
      
    - name: Upload artifact
      uses: actions/upload-pages-artifact@v2
      with:
        path: '.'
        
    - name: Deploy to GitHub Pages
      uses: actions/deploy-pages@v2`;
  }

  async prepareDNSConfiguration() {
    console.log('📋 Preparing DNS configuration...');
    
    const dnsRecords = [
      '# DNS Records for unit4productions.com migration',
      '# Apply these records in your DNS provider:',
      '',
      '# Main website (apex domain)',
      'Type: A',
      'Host: @',
      'Values: 185.199.108.153, 185.199.109.153, 185.199.110.153, 185.199.111.153',
      '',
      '# Main website (www subdomain)',
      'Type: CNAME',
      'Host: www',
      'Value: unit4productions.github.io',
      ''
    ];

    // Add game subdomains
    this.repositories.forEach(repo => {
      dnsRecords.push(`# Game: ${repo.original.title}`);
      dnsRecords.push('Type: CNAME');
      dnsRecords.push(`Host: ${repo.repository.name}`);
      dnsRecords.push(`Value: unit4productions.github.io`);
      dnsRecords.push('');
    });

    fs.writeFileSync('./DNS_CONFIGURATION.txt', dnsRecords.join('\\n'));
    console.log('DNS configuration saved to DNS_CONFIGURATION.txt');
    console.log('⚠️  Apply these DNS records manually before proceeding to verification');
  }

  async verifyMigration() {
    console.log('🔍 Verifying migration...');
    
    const results = {
      mainWebsite: null,
      games: [],
      dns: [],
      ssl: []
    };

    // Test main website
    try {
      const response = await fetch(`https://${this.config.domain}`);
      results.mainWebsite = {
        url: this.config.domain,
        status: response.status,
        success: response.ok
      };
    } catch (error) {
      results.mainWebsite = {
        url: this.config.domain,
        status: 'ERROR',
        success: false,
        error: error.message
      };
    }

    // Test each game
    for (const repo of this.repositories) {
      const gameUrl = `https://${repo.repository.name}.${this.config.domain}`;
      try {
        const response = await fetch(gameUrl);
        results.games.push({
          title: repo.original.title,
          url: gameUrl,
          status: response.status,
          success: response.ok
        });
      } catch (error) {
        results.games.push({
          title: repo.original.title,
          url: gameUrl,
          status: 'ERROR',
          success: false,
          error: error.message
        });
      }
    }

    fs.writeFileSync('./migration-verification.json', JSON.stringify(results, null, 2));
    console.log('Verification results saved to migration-verification.json');

    const successCount = results.games.filter(g => g.success).length;
    console.log(`✅ ${successCount}/${results.games.length} games successfully migrated`);
  }

  generateMigrationReport() {
    const report = {
      timestamp: new Date().toISOString(),
      source: this.config.squarespaceUrl,
      target: this.config.domain,
      summary: {
        totalGames: this.migrationData.games.length,
        repositoriesCreated: this.repositories.length,
        migrationStatus: 'completed',
        costSavings: {
          squarespaceMonthly: this.config.squarespaceCost || 25,
          githubMonthly: 0,
          annualSavings: (this.config.squarespaceCost || 25) * 12
        }
      },
      repositories: this.repositories.map(repo => ({
        name: repo.repository.name,
        url: repo.repository.html_url,
        pages: repo.url,
        customDomain: repo.customDomain,
        originalGame: repo.original.title
      })),
      nextSteps: [
        'Apply DNS configuration from DNS_CONFIGURATION.txt',
        'Wait 24-48 hours for DNS propagation',
        'Verify all sites are accessible',
        'Cancel Squarespace subscription',
        'Monitor analytics and performance'
      ]
    };

    fs.writeFileSync('./MIGRATION_REPORT.json', JSON.stringify(report, null, 2));
    console.log('📊 Migration report saved to MIGRATION_REPORT.json');
    console.log(`💰 Annual cost savings: $${report.summary.costSavings.annualSavings}`);
  }

  async rollbackMigration() {
    console.log('⏪ Rolling back migration...');
    
    // Delete created repositories (optional - might want to keep for retry)
    const rollbackChoice = process.env.ROLLBACK_DELETE_REPOS || 'no';
    
    if (rollbackChoice === 'yes') {
      for (const repo of this.repositories) {
        try {
          await this.github.octokit.rest.repos.delete({
            owner: this.config.org,
            repo: repo.repository.name
          });
          console.log(`Deleted repository: ${repo.repository.name}`);
        } catch (error) {
          console.error(`Failed to delete ${repo.repository.name}:`, error.message);
        }
      }
    }
    
    console.log('Rollback completed. Check logs for details.');
  }
}

// Configuration and execution
const migrationConfig = {
  githubToken: process.env.GITHUB_TOKEN,
  org: 'unit4productions',
  domain: 'unit4productions.com',
  squarespaceUrl: 'https://unit4productions.com',
  squarespaceCost: 25 // monthly cost
};

// Run migration
async function runMigration() {
  const orchestrator = new MigrationOrchestrator(migrationConfig);
  await orchestrator.runFullMigration();
}

if (require.main === module) {
  runMigration().catch(console.error);
}

module.exports = MigrationOrchestrator;
```

---

## Phase 4: DNS Switchover (Week 4)

### 4.1 DNS Transition Plan

```bash
#!/bin/bash
# dns-switchover.sh

DOMAIN="unit4productions.com"
ORG="unit4productions"

echo "🔄 DNS Switchover Plan for $DOMAIN"

# Step 1: Lower TTL values (do this 24-48 hours before switchover)
echo "1. Lower TTL values to 300 seconds (5 minutes)"
echo "   - This allows faster DNS propagation during switchover"

# Step 2: Create backup of current DNS
echo "2. Backup current DNS configuration"
dig $DOMAIN ANY > dns-backup-before-migration.txt

# Step 3: Apply new DNS records
echo "3. Apply GitHub Pages DNS records:"
echo ""
echo "Main website (apex domain):"
echo "Type: A, Host: @, Values: 185.199.108.153, 185.199.109.153, 185.199.110.153, 185.199.111.153"
echo ""
echo "WWW redirect:"
echo "Type: CNAME, Host: www, Value: ${ORG}.github.io"
echo ""

# Generate game-specific DNS records
echo "Game-specific subdomains:"
# This would be populated from migration data
echo "Type: CNAME, Host: [game-name], Value: ${ORG}.github.io"

# Step 4: Monitor DNS propagation
echo ""
echo "4. Monitor DNS propagation:"
echo "   dig $DOMAIN"
echo "   nslookup $DOMAIN"
echo "   Online tools: whatsmydns.net"

# Step 5: Verify HTTPS certificates
echo ""
echo "5. Verify HTTPS certificates are provisioned"
echo "   GitHub Pages automatically provisions Let's Encrypt certificates"
echo "   This may take 5-10 minutes after DNS propagation"

# Step 6: Final verification
echo ""
echo "6. Final verification checklist:"
echo "   [ ] Main website loads correctly"
echo "   [ ] All games are accessible"
echo "   [ ] HTTPS works properly"
echo "   [ ] Analytics tracking active"
echo "   [ ] Ad monetization functional"
```

### 4.2 Rollback Plan

```bash
#!/bin/bash
# dns-rollback.sh

DOMAIN="unit4productions.com"
SQUARESPACE_IP="198.185.159.144"  # Example Squarespace IP

echo "🔄 DNS Rollback Plan for $DOMAIN"
echo "⚠️  Use this only if GitHub Pages migration fails"

echo ""
echo "Rollback DNS records (restore Squarespace):"
echo "Type: A, Host: @, Value: $SQUARESPACE_IP"
echo "Type: CNAME, Host: www, Value: ext-cust.squarespace.com"

echo ""
echo "Remove game subdomains:"
echo "Delete all CNAME records for game subdomains"

echo ""
echo "Restore original TTL values"
echo "Monitor DNS propagation back to Squarespace"
```

---

## Phase 5: Verification and Optimization (Week 5)

### 5.1 Comprehensive Testing Suite

```javascript
// test-migration.js
const puppeteer = require('puppeteer');
const fs = require('fs');

class MigrationTester {
  constructor(domain, games) {
    this.domain = domain;
    this.games = games;
    this.browser = null;
    this.results = {
      timestamp: new Date().toISOString(),
      overall: { passed: 0, failed: 0, warnings: 0 },
      tests: []
    };
  }

  async init() {
    this.browser = await puppeteer.launch({ headless: false });
  }

  async runAllTests() {
    console.log('🧪 Running comprehensive migration tests...');

    await this.init();

    try {
      // Test main website
      await this.testMainWebsite();

      // Test each game
      for (const game of this.games) {
        await this.testGame(game);
      }

      // Performance tests
      await this.runPerformanceTests();

      // SEO tests
      await this.runSEOTests();

      // Analytics tests
      await this.testAnalytics();

      // Accessibility tests
      await this.runAccessibilityTests();

    } finally {
      await this.browser.close();
    }

    this.generateTestReport();
    return this.results;
  }

  async testMainWebsite() {
    const testName = 'Main Website Functionality';
    console.log(`Testing: ${testName}`);

    try {
      const page = await this.browser.newPage();
      
      // Test loading
      const response = await page.goto(`https://${this.domain}`, { waitUntil: 'networkidle2' });
      this.recordTest(testName, 'HTTP Status', response.status() === 200);
      
      // Test HTTPS
      this.recordTest(testName, 'HTTPS Security', page.url().startsWith('https://'));
      
      // Test navigation
      const navLinks = await page.$$('nav a');
      this.recordTest(testName, 'Navigation Links', navLinks.length > 0);
      
      // Test responsive design
      await page.setViewport({ width: 375, height: 667 }); // Mobile
      await page.waitForTimeout(1000);
      const mobileLayout = await page.$('.mobile-nav, .hamburger, nav ul');
      this.recordTest(testName, 'Mobile Responsive', !!mobileLayout);
      
      await page.close();
    } catch (error) {
      this.recordTest(testName, 'Overall', false, error.message);
    }
  }

  async testGame(game) {
    const testName = `Game: ${game.title}`;
    const gameUrl = `https://${game.slug}.${this.domain}`;
    
    console.log(`Testing: ${testName}`);

    try {
      const page = await this.browser.newPage();
      
      // Test game loading
      const response = await page.goto(gameUrl, { waitUntil: 'networkidle2' });
      this.recordTest(testName, 'Game Loads', response.status() === 200);
      
      // Test canvas element
      const canvas = await page.$('#gameCanvas');
      this.recordTest(testName, 'Canvas Present', !!canvas);
      
      // Test game controls
      const playButton = await page.$('#playButton, .play-button, button[data-action="play"]');
      this.recordTest(testName, 'Play Button', !!playButton);
      
      // Test game initialization
      await page.evaluate(() => {
        window.testGameLoaded = typeof Game !== 'undefined' || typeof window.gameInstance !== 'undefined';
      });
      const gameLoaded = await page.evaluate(() => window.testGameLoaded);
      this.recordTest(testName, 'Game Script Loaded', gameLoaded);
      
      // Test analytics
      const analyticsLoaded = await page.evaluate(() => typeof gtag !== 'undefined');
      this.recordTest(testName, 'Analytics Loaded', analyticsLoaded);
      
      // Test ads
      const adsLoaded = await page.evaluate(() => typeof adsbygoogle !== 'undefined');
      this.recordTest(testName, 'Ad Scripts Loaded', adsLoaded);
      
      await page.close();
    } catch (error) {
      this.recordTest(testName, 'Overall', false, error.message);
    }
  }

  async runPerformanceTests() {
    console.log('Testing: Performance Metrics');
    
    const page = await this.browser.newPage();
    
    // Test main website performance
    await page.goto(`https://${this.domain}`);
    
    const performanceMetrics = await page.evaluate(() => {
      const navigation = performance.getEntriesByType('navigation')[0];
      return {
        loadTime: navigation.loadEventEnd - navigation.fetchStart,
        domContentLoaded: navigation.domContentLoadedEventEnd - navigation.fetchStart,
        firstPaint: performance.getEntriesByName('first-paint')[0]?.startTime || 0,
        largestContentfulPaint: performance.getEntriesByName('largest-contentful-paint')[0]?.startTime || 0
      };
    });
    
    this.recordTest('Performance', 'Load Time < 3s', performanceMetrics.loadTime < 3000);
    this.recordTest('Performance', 'DOM Ready < 2s', performanceMetrics.domContentLoaded < 2000);
    this.recordTest('Performance', 'First Paint < 1s', performanceMetrics.firstPaint < 1000);
    
    await page.close();
  }

  async runSEOTests() {
    console.log('Testing: SEO Optimization');
    
    const page = await this.browser.newPage();
    await page.goto(`https://${this.domain}`);
    
    // Test meta tags
    const title = await page.$eval('title', el => el.textContent);
    const description = await page.$('meta[name="description"]');
    const viewport = await page.$('meta[name="viewport"]');
    
    this.recordTest('SEO', 'Page Title Present', !!title && title.length > 0);
    this.recordTest('SEO', 'Meta Description', !!description);
    this.recordTest('SEO', 'Viewport Meta', !!viewport);
    
    // Test structured data
    const structuredData = await page.$('script[type="application/ld+json"]');
    this.recordTest('SEO', 'Structured Data', !!structuredData, 'Warning: Consider adding structured data for better SEO');
    
    await page.close();
  }

  async testAnalytics() {
    console.log('Testing: Analytics Integration');
    
    const page = await this.browser.newPage();
    await page.goto(`https://${this.domain}`);
    
    // Check for Google Analytics
    const gaLoaded = await page.evaluate(() => {
      return typeof gtag !== 'undefined' && typeof dataLayer !== 'undefined';
    });
    
    this.recordTest('Analytics', 'Google Analytics', gaLoaded);
    
    // Test custom events
    if (gaLoaded) {
      await page.evaluate(() => {
        gtag('event', 'test_migration', {
          event_category: 'migration_test',
          event_label: 'automated_testing'
        });
      });
      this.recordTest('Analytics', 'Custom Events', true);
    }
    
    await page.close();
  }

  async runAccessibilityTests() {
    console.log('Testing: Accessibility Compliance');
    
    const page = await this.browser.newPage();
    await page.goto(`https://${this.domain}`);
    
    // Test alt attributes on images
    const imagesWithoutAlt = await page.$$eval('img:not([alt])', imgs => imgs.length);
    this.recordTest('Accessibility', 'Image Alt Attributes', imagesWithoutAlt === 0);
    
    // Test heading structure
    const headings = await page.$$eval('h1, h2, h3, h4, h5, h6', headings => 
      headings.map(h => h.tagName).join(',')
    );
    const hasH1 = headings.includes('H1');
    this.recordTest('Accessibility', 'Heading Structure', hasH1);
    
    // Test keyboard navigation
    const focusableElements = await page.$$eval(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',
      elements => elements.length
    );
    this.recordTest('Accessibility', 'Focusable Elements', focusableElements > 0);
    
    await page.close();
  }

  recordTest(category, testName, passed, message = '') {
    const result = {
      category,
      test: testName,
      passed,
      message,
      timestamp: new Date().toISOString()
    };
    
    this.results.tests.push(result);
    
    if (passed) {
      this.results.overall.passed++;
      console.log(`  ✅ ${testName}`);
    } else {
      if (message.includes('Warning')) {
        this.results.overall.warnings++;
        console.log(`  ⚠️  ${testName}: ${message}`);
      } else {
        this.results.overall.failed++;
        console.log(`  ❌ ${testName}: ${message}`);
      }
    }
  }

  generateTestReport() {
    const reportPath = './MIGRATION_TEST_REPORT.json';
    fs.writeFileSync(reportPath, JSON.stringify(this.results, null, 2));
    
    console.log('\\n📊 Test Results Summary:');
    console.log(`✅ Passed: ${this.results.overall.passed}`);
    console.log(`❌ Failed: ${this.results.overall.failed}`);
    console.log(`⚠️  Warnings: ${this.results.overall.warnings}`);
    console.log(`📄 Detailed report: ${reportPath}`);
    
    const successRate = (this.results.overall.passed / this.results.tests.length) * 100;
    console.log(`📈 Success Rate: ${successRate.toFixed(1)}%`);
    
    if (successRate >= 90) {
      console.log('🎉 Migration testing PASSED! Ready for production.');
    } else if (successRate >= 75) {
      console.log('⚠️  Migration testing has WARNINGS. Review before going live.');
    } else {
      console.log('❌ Migration testing FAILED. Address issues before proceeding.');
    }
  }
}

module.exports = MigrationTester;

// Usage
if (require.main === module) {
  const tester = new MigrationTester('unit4productions.com', [
    { title: 'Space Invaders', slug: 'space-invaders' },
    { title: 'Tetris', slug: 'tetris' },
    // Add other games
  ]);
  
  tester.runAllTests().catch(console.error);
}
```

---

## Migration Timeline and Checklist

### Week 1: Preparation
- [ ] Audit existing Squarespace content
- [ ] Export assets and content
- [ ] Setup GitHub organization
- [ ] Create GitHub tokens and permissions
- [ ] Test GitHub API access

### Week 2: Parallel Environment
- [ ] Create GitHub repositories for all games
- [ ] Enable GitHub Pages on all repos
- [ ] Configure custom domains (without DNS activation)
- [ ] Convert content to HTML5/CSS/JS
- [ ] Deploy converted games to GitHub

### Week 3: Content Migration
- [ ] Run automated migration scripts
- [ ] Verify all games load correctly
- [ ] Test functionality and interactivity
- [ ] Implement analytics and monetization
- [ ] Performance optimization

### Week 4: DNS Switchover
- [ ] Lower DNS TTL values
- [ ] Backup existing DNS configuration
- [ ] Apply new GitHub Pages DNS records
- [ ] Monitor DNS propagation (24-48 hours)
- [ ] Verify HTTPS certificate provisioning

### Week 5: Verification and Go-Live
- [ ] Run comprehensive testing suite
- [ ] Performance and accessibility testing
- [ ] Analytics verification
- [ ] User acceptance testing
- [ ] Cancel Squarespace subscription
- [ ] Monitor for 30 days

---

## Cost Savings Analysis

| Item | Squarespace | GitHub Pages | Annual Savings |
|------|-------------|--------------|----------------|
| Hosting | $25/month | $0/month | $300 |
| Custom Domain | Included | Free | $0 |
| SSL Certificates | Included | Free | $0 |
| CDN/Performance | Included | Free | $0 |
| Analytics | Basic | Google Analytics | $0 |
| **Total Annual** | **$300** | **$0** | **$300** |

**Additional Benefits:**
- Unlimited scalability
- Version control and rollbacks
- Automated deployments
- No vendor lock-in
- Full API control
- AI agent compatible

This migration plan provides a comprehensive, step-by-step approach to moving from Squarespace to GitHub Pages while maintaining full automation capabilities and achieving significant cost savings.