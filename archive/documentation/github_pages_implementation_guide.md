# GitHub Pages Implementation Guide
## Complete API Setup for Automated Gaming Website Deployment

### Overview

This guide provides step-by-step instructions for setting up GitHub Pages with complete API automation for your HTML5 gaming website, including custom domain integration for unit4productions.com.

---

## Prerequisites

1. **GitHub Account** - Free account sufficient
2. **Domain Control** - Access to unit4productions.com DNS settings
3. **Personal Access Token** - GitHub API authentication
4. **Git CLI** or **GitHub CLI** - For automation scripts

---

## Step 1: GitHub API Authentication

### Create Personal Access Token

```bash
# Using GitHub CLI (recommended)
gh auth login

# Or create token manually at:
# https://github.com/settings/tokens/new
# Required scopes: repo, pages, workflow
```

### Store Token Securely
```bash
# Environment variable method
export GITHUB_TOKEN="ghp_your_token_here"

# Or use GitHub CLI auth
gh auth status
```

---

## Step 2: Repository Setup via API

### Create Repository Programmatically

```javascript
// Node.js example for repository creation
const { Octokit } = require("@octokit/rest");

const octokit = new Octokit({
  auth: process.env.GITHUB_TOKEN,
});

async function createGameRepository(gameName) {
  try {
    // Create repository
    const { data: repo } = await octokit.rest.repos.createForAuthenticatedUser({
      name: `${gameName}-game`,
      description: `HTML5 Game: ${gameName}`,
      private: false, // Public for free GitHub Pages
      has_issues: true,
      has_projects: false,
      has_wiki: false,
      auto_init: true,
      gitignore_template: "Node"
    });
    
    console.log(`Repository created: ${repo.html_url}`);
    return repo;
  } catch (error) {
    console.error("Repository creation failed:", error.message);
  }
}

// Usage
createGameRepository("space-invaders");
```

### Bash Script Alternative

```bash
#!/bin/bash
# create_game_repo.sh

GAME_NAME=$1
REPO_NAME="${GAME_NAME}-game"

# Create repository
gh repo create $REPO_NAME \
  --description "HTML5 Game: $GAME_NAME" \
  --public \
  --add-readme \
  --gitignore Node

# Clone repository
gh repo clone $REPO_NAME
cd $REPO_NAME

echo "Repository $REPO_NAME created and cloned successfully"
```

---

## Step 3: Enable GitHub Pages via API

### Configure Pages Settings

```javascript
// Enable GitHub Pages
async function enableGitHubPages(owner, repo, branch = "main") {
  try {
    const response = await octokit.rest.repos.createPagesSite({
      owner: owner,
      repo: repo,
      source: {
        branch: branch,
        path: "/"
      }
    });
    
    console.log(`Pages enabled: ${response.data.html_url}`);
    return response.data;
  } catch (error) {
    console.error("Pages setup failed:", error.message);
  }
}

// Configure custom domain
async function setCustomDomain(owner, repo, domain) {
  try {
    await octokit.rest.repos.updateInformationAboutPagesSite({
      owner: owner,
      repo: repo,
      cname: domain
    });
    
    console.log(`Custom domain set: ${domain}`);
  } catch (error) {
    console.error("Custom domain setup failed:", error.message);
  }
}

// Usage
await enableGitHubPages("yourusername", "space-invaders-game");
await setCustomDomain("yourusername", "space-invaders-game", "games.unit4productions.com");
```

### CLI Method

```bash
# Enable Pages via GitHub CLI
gh api \
  --method POST \
  -H "Accept: application/vnd.github+json" \
  /repos/OWNER/REPO/pages \
  -f source[branch]=main \
  -f source[path]=/

# Set custom domain
gh api \
  --method PUT \
  -H "Accept: application/vnd.github+json" \
  /repos/OWNER/REPO/pages \
  -f cname=games.unit4productions.com
```

---

## Step 4: DNS Configuration for unit4productions.com

### DNS Records Setup

```txt
# Add these DNS records to unit4productions.com
Type: CNAME
Host: games
Value: yourusername.github.io
TTL: 3600

# For apex domain (optional)
Type: A
Host: @
Value: 185.199.108.153
Value: 185.199.109.153
Value: 185.199.110.153
Value: 185.199.111.153
TTL: 3600

# AAAA records for IPv6 (optional)
Type: AAAA
Host: @
Value: 2606:50c0:8000::153
Value: 2606:50c0:8001::153
Value: 2606:50c0:8002::153
Value: 2606:50c0:8003::153
TTL: 3600
```

### Verify DNS Configuration

```bash
# Check DNS propagation
dig games.unit4productions.com CNAME
nslookup games.unit4productions.com

# Verify with GitHub's API
gh api /repos/OWNER/REPO/pages
```

---

## Step 5: GitHub Actions Workflow Setup

### Create Automated Deployment Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy HTML5 Game

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      pages: write
      id-token: write
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
        
    - name: Install dependencies
      run: |
        if [ -f package.json ]; then
          npm ci
        fi
        
    - name: Build game
      run: |
        if [ -f package.json ]; then
          npm run build --if-present
        fi
        
    - name: Optimize assets
      run: |
        # Minify HTML, CSS, JS
        if command -v html-minifier &> /dev/null; then
          find . -name "*.html" -exec html-minifier --collapse-whitespace --remove-comments {} \;
        fi
        
    - name: Setup Pages
      uses: actions/configure-pages@v3
      
    - name: Upload artifact
      uses: actions/upload-pages-artifact@v2
      with:
        path: '.'
        
    - name: Deploy to GitHub Pages
      id: deployment
      uses: actions/deploy-pages@v2
      
    - name: Notify deployment success
      run: |
        echo "Game deployed successfully to: ${{ steps.deployment.outputs.page_url }}"
```

### Auto-Generated Game Structure

```javascript
// scripts/generate-game.js
const fs = require('fs');
const path = require('path');

function generateGameStructure(gameName, gameType = 'canvas') {
  const gameDir = `./${gameName}`;
  
  // Create directory structure
  if (!fs.existsSync(gameDir)) {
    fs.mkdirSync(gameDir, { recursive: true });
  }
  
  const directories = ['assets', 'js', 'css', 'audio', 'images'];
  directories.forEach(dir => {
    fs.mkdirSync(path.join(gameDir, dir), { recursive: true });
  });
  
  // Generate index.html
  const indexHtml = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${gameName}</title>
    <link rel="stylesheet" href="css/style.css">
    <script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
</head>
<body>
    <div id="gameContainer">
        <canvas id="gameCanvas" width="800" height="600"></canvas>
    </div>
    <div id="gameUI">
        <div id="score">Score: <span id="scoreValue">0</span></div>
        <button id="startButton">Start Game</button>
    </div>
    
    <!-- Ad container -->
    <div id="adContainer">
        <ins class="adsbygoogle"
             style="display:block"
             data-ad-client="ca-pub-XXXXXXXXX"
             data-ad-slot="XXXXXXXXX"
             data-ad-format="auto"></ins>
    </div>
    
    <script src="js/game.js"></script>
    <script>
        (adsbygoogle = window.adsbygoogle || []).push({});
    </script>
</body>
</html>`;
  
  // Generate game.js
  const gameJs = `// ${gameName} - HTML5 Game
class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.score = 0;
        this.gameRunning = false;
        
        this.setupEventListeners();
        this.init();
    }
    
    init() {
        console.log('Game initialized: ${gameName}');
        this.render();
    }
    
    setupEventListeners() {
        document.getElementById('startButton').addEventListener('click', () => {
            this.startGame();
        });
        
        // Keyboard controls
        document.addEventListener('keydown', (e) => {
            this.handleInput(e.keyCode);
        });
    }
    
    startGame() {
        this.gameRunning = true;
        this.score = 0;
        this.gameLoop();
    }
    
    gameLoop() {
        if (!this.gameRunning) return;
        
        this.update();
        this.render();
        
        requestAnimationFrame(() => this.gameLoop());
    }
    
    update() {
        // Game logic here
        this.updateScore();
    }
    
    render() {
        // Clear canvas
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Draw game elements
        this.ctx.fillStyle = '#333';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        this.ctx.fillStyle = '#fff';
        this.ctx.font = '24px Arial';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('${gameName}', this.canvas.width / 2, 50);
    }
    
    handleInput(keyCode) {
        if (!this.gameRunning) return;
        
        switch(keyCode) {
            case 37: // Left arrow
                break;
            case 39: // Right arrow
                break;
            case 32: // Spacebar
                break;
        }
    }
    
    updateScore() {
        document.getElementById('scoreValue').textContent = this.score;
    }
}

// Initialize game when page loads
window.addEventListener('load', () => {
    new Game();
});`;
  
  // Generate CSS
  const styleCSS = `/* ${gameName} Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: Arial, sans-serif;
    background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
    display: flex;
    flex-direction: column;
    align-items: center;
    min-height: 100vh;
    padding: 20px;
}

#gameContainer {
    background: #000;
    border: 3px solid #333;
    border-radius: 10px;
    margin-bottom: 20px;
    box-shadow: 0 0 20px rgba(0,0,0,0.5);
}

#gameCanvas {
    display: block;
    border-radius: 7px;
}

#gameUI {
    display: flex;
    align-items: center;
    gap: 20px;
    margin-bottom: 20px;
}

#score {
    color: white;
    font-size: 18px;
    font-weight: bold;
}

#startButton {
    padding: 10px 20px;
    font-size: 16px;
    background: #4CAF50;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: background 0.3s;
}

#startButton:hover {
    background: #45a049;
}

#adContainer {
    width: 100%;
    max-width: 800px;
    margin-top: 20px;
}

@media (max-width: 868px) {
    #gameCanvas {
        width: 100%;
        height: auto;
        max-width: 800px;
    }
    
    #gameContainer {
        width: 100%;
    }
}`;
  
  // Write files
  fs.writeFileSync(path.join(gameDir, 'index.html'), indexHtml);
  fs.writeFileSync(path.join(gameDir, 'js', 'game.js'), gameJs);
  fs.writeFileSync(path.join(gameDir, 'css', 'style.css'), styleCSS);
  
  console.log(`Game structure generated for ${gameName}`);
}

// Export for use in automation scripts
module.exports = { generateGameStructure };

// Command line usage
if (require.main === module) {
  const gameName = process.argv[2] || 'my-game';
  generateGameStructure(gameName);
}`;

// Create the generator script
fs.writeFileSync('./scripts/generate-game.js', generateGameStructure.toString());
```

---

## Step 6: Complete Automation Script

### Master Deployment Script

```javascript
// deploy-game.js - Complete automation script
const { Octokit } = require("@octokit/rest");
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

class GameDeployer {
  constructor(token, username) {
    this.octokit = new Octokit({ auth: token });
    this.username = username;
  }
  
  async deployNewGame(gameName, customDomain = null) {
    const repoName = `${gameName}-game`;
    
    try {
      // Step 1: Create repository
      console.log(`Creating repository: ${repoName}`);
      const repo = await this.createRepository(repoName, gameName);
      
      // Step 2: Enable GitHub Pages
      console.log('Enabling GitHub Pages...');
      await this.enablePages(this.username, repoName);
      
      // Step 3: Set custom domain if provided
      if (customDomain) {
        console.log(`Setting custom domain: ${customDomain}`);
        await this.setCustomDomain(this.username, repoName, customDomain);
      }
      
      // Step 4: Generate game files
      console.log('Generating game structure...');
      this.generateGameFiles(gameName);
      
      // Step 5: Commit and push
      console.log('Committing and pushing files...');
      await this.commitAndPush(repoName, gameName);
      
      console.log(`\\n✅ Game deployed successfully!`);
      console.log(`Repository: https://github.com/${this.username}/${repoName}`);
      console.log(`Live URL: ${customDomain ? `https://${customDomain}` : `https://${this.username}.github.io/${repoName}`}`);
      
      return {
        repository: `https://github.com/${this.username}/${repoName}`,
        liveUrl: customDomain ? `https://${customDomain}` : `https://${this.username}.github.io/${repoName}`
      };
      
    } catch (error) {
      console.error(`❌ Deployment failed:`, error.message);
      throw error;
    }
  }
  
  async createRepository(name, description) {
    const { data } = await this.octokit.rest.repos.createForAuthenticatedUser({
      name: name,
      description: `HTML5 Game: ${description}`,
      private: false,
      has_issues: true,
      has_projects: false,
      has_wiki: false,
      auto_init: false
    });
    return data;
  }
  
  async enablePages(owner, repo) {
    await this.octokit.rest.repos.createPagesSite({
      owner: owner,
      repo: repo,
      source: { branch: "main", path: "/" }
    });
  }
  
  async setCustomDomain(owner, repo, domain) {
    await this.octokit.rest.repos.updateInformationAboutPagesSite({
      owner: owner,
      repo: repo,
      cname: domain
    });
  }
  
  generateGameFiles(gameName) {
    // Create directory structure
    const dirs = ['js', 'css', 'assets', 'images', 'audio', '.github/workflows'];
    dirs.forEach(dir => {
      if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
      }
    });
    
    // Generate files (HTML, CSS, JS, workflow)
    // ... (file generation code from previous section)
  }
  
  async commitAndPush(repoName, gameName) {
    const commands = [
      'git init',
      `git remote add origin https://github.com/${this.username}/${repoName}.git`,
      'git add .',
      `git commit -m "Initial commit: ${gameName} game deployment"`,
      'git branch -M main',
      'git push -u origin main'
    ];
    
    commands.forEach(cmd => {
      execSync(cmd, { stdio: 'inherit' });
    });
  }
}

// Usage
async function main() {
  const deployer = new GameDeployer(
    process.env.GITHUB_TOKEN,
    'yourusername'
  );
  
  await deployer.deployNewGame('tetris', 'games.unit4productions.com');
}

if (require.main === module) {
  main().catch(console.error);
}

module.exports = GameDeployer;
```

### Package.json for Dependencies

```json
{
  "name": "github-pages-game-deployer",
  "version": "1.0.0",
  "description": "Automated HTML5 game deployment to GitHub Pages",
  "main": "deploy-game.js",
  "scripts": {
    "deploy": "node deploy-game.js",
    "generate": "node scripts/generate-game.js"
  },
  "dependencies": {
    "@octokit/rest": "^20.0.2"
  },
  "keywords": ["github-pages", "html5-games", "deployment", "automation"],
  "author": "Your Name",
  "license": "MIT"
}
```

---

## Step 7: Testing and Verification

### Automated Testing Script

```bash
#!/bin/bash
# test-deployment.sh

DOMAIN=$1
GAME_NAME=$2

if [ -z "$DOMAIN" ]; then
    echo "Usage: ./test-deployment.sh <domain> [game-name]"
    exit 1
fi

echo "🧪 Testing deployment for $DOMAIN"

# Test DNS resolution
echo "📡 Testing DNS..."
if dig +short $DOMAIN | grep -q "github.io"; then
    echo "✅ DNS configured correctly"
else
    echo "❌ DNS configuration issue"
    dig $DOMAIN
fi

# Test HTTPS
echo "🔒 Testing HTTPS..."
if curl -s -I https://$DOMAIN | head -n 1 | grep -q "200 OK"; then
    echo "✅ HTTPS working"
else
    echo "❌ HTTPS issue"
    curl -I https://$DOMAIN
fi

# Test game loading
echo "🎮 Testing game loading..."
if curl -s https://$DOMAIN | grep -q "canvas"; then
    echo "✅ Game HTML loaded successfully"
else
    echo "❌ Game loading issue"
fi

# Performance test
echo "⚡ Running performance test..."
curl -w "Connect: %{time_connect}s\\nTTFB: %{time_starttransfer}s\\nTotal: %{time_total}s\\nSize: %{size_download} bytes\\n" \
     -o /dev/null -s https://$DOMAIN

echo "🏁 Test completed for $DOMAIN"
```

---

## Summary

This implementation guide provides:

1. **Complete API Control** - Every step automated via GitHub API
2. **Zero Cost Solution** - Free GitHub Pages hosting
3. **Custom Domain Support** - Full DNS integration for unit4productions.com
4. **Automated Workflows** - GitHub Actions for CI/CD
5. **Game Generation** - Automated HTML5 game structure creation
6. **Testing Scripts** - Verification of deployments
7. **Scalable Architecture** - Can deploy unlimited games

**Total Implementation Time:** 30-60 minutes for initial setup, 2-3 minutes per game deployment thereafter.

The system requires zero human intervention after initial setup and can be controlled entirely by AI agents or automated scripts.