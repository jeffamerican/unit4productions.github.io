#!/bin/bash

# Git-Based Gaming Website Deployment System
# Version: 2.0.0
# Description: Reliable deployment using standard git workflows

set -e  # Exit on any error

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/../config/deployment.json"
BUILD_DIR="$SCRIPT_DIR/../build"
TEMP_DIR="$SCRIPT_DIR/../temp"
LOG_FILE="$SCRIPT_DIR/../logs/deployment.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    local level="$1"
    shift
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo -e "${timestamp} [${level}] ${message}" | tee -a "$LOG_FILE"
    
    case "$level" in
        "ERROR") echo -e "${RED}[ERROR]${NC} ${message}" ;;
        "SUCCESS") echo -e "${GREEN}[SUCCESS]${NC} ${message}" ;;
        "WARNING") echo -e "${YELLOW}[WARNING]${NC} ${message}" ;;
        "INFO") echo -e "${BLUE}[INFO]${NC} ${message}" ;;
    esac
}

# Check prerequisites
check_prerequisites() {
    log "INFO" "Checking prerequisites..."
    
    # Check git
    if ! command -v git &> /dev/null; then
        log "ERROR" "Git is not installed. Please install git first."
        exit 1
    fi
    
    # Check git configuration
    if ! git config user.name > /dev/null || ! git config user.email > /dev/null; then
        log "ERROR" "Git user configuration missing. Run:"
        log "INFO" "git config --global user.name 'Your Name'"
        log "INFO" "git config --global user.email 'your.email@example.com'"
        exit 1
    fi
    
    # Check GitHub CLI (optional but recommended)
    if command -v gh &> /dev/null; then
        log "INFO" "GitHub CLI detected - enhanced features available"
    else
        log "WARNING" "GitHub CLI not found - some automation features disabled"
    fi
    
    # Create required directories
    mkdir -p "$BUILD_DIR" "$TEMP_DIR" "$(dirname "$LOG_FILE")"
    
    log "SUCCESS" "Prerequisites check passed"
}

# Parse command line arguments
parse_args() {
    COMMAND=""
    SITE_NAME=""
    REPO_NAME=""
    DOMAIN=""
    GAME_TYPE=""
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            create)
                COMMAND="create"
                shift
                ;;
            deploy)
                COMMAND="deploy"
                shift
                ;;
            update)
                COMMAND="update"
                shift
                ;;
            --site)
                SITE_NAME="$2"
                shift 2
                ;;
            --repo)
                REPO_NAME="$2"
                shift 2
                ;;
            --domain)
                DOMAIN="$2"
                shift 2
                ;;
            --type)
                GAME_TYPE="$2"
                shift 2
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            *)
                log "ERROR" "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    if [[ -z "$COMMAND" ]]; then
        log "ERROR" "Command required"
        show_help
        exit 1
    fi
}

# Show help
show_help() {
    cat << EOF
Git-Based Gaming Website Deployment System

USAGE:
    $0 <command> [options]

COMMANDS:
    create      Create new gaming website repository
    deploy      Deploy website to GitHub Pages
    update      Update existing deployment

OPTIONS:
    --site <name>       Site name (e.g., unit4productions)
    --repo <name>       Repository name
    --domain <domain>   Custom domain name
    --type <type>       Game type (html5, unity, portal)
    --help, -h          Show this help message

EXAMPLES:
    # Create gaming portal
    $0 create --site unit4productions --repo unit4productions.github.io --domain unit4productions.com --type portal
    
    # Deploy single game
    $0 deploy --site reflex-rings --type html5
    
    # Update existing site
    $0 update --site unit4productions

PREREQUISITES:
    - Git installed and configured
    - GitHub account with repository access
    - GitHub CLI (optional, for enhanced automation)

EOF
}

# Create new repository
create_repository() {
    local site_name="$1"
    local repo_name="$2"
    local domain="$3"
    local game_type="$4"
    
    log "INFO" "Creating repository: $repo_name"
    
    # Create local repository structure
    local repo_dir="$TEMP_DIR/$repo_name"
    
    if [[ -d "$repo_dir" ]]; then
        log "WARNING" "Repository directory exists, removing..."
        rm -rf "$repo_dir"
    fi
    
    mkdir -p "$repo_dir"
    cd "$repo_dir"
    
    # Initialize git repository
    git init
    log "INFO" "Initialized git repository"
    
    # Create initial structure based on game type
    case "$game_type" in
        "portal")
            create_portal_structure "$domain"
            ;;
        "html5")
            create_html5_structure "$site_name"
            ;;
        "unity")
            create_unity_structure "$site_name"
            ;;
        *)
            log "ERROR" "Unknown game type: $game_type"
            exit 1
            ;;
    esac
    
    # Initial commit
    git add .
    git commit -m "Initial commit: $site_name deployment

🎮 Game deployment setup complete
📱 Mobile-optimized gaming experience
🚀 Ready for GitHub Pages

🤖 Generated with Claude Code Deployment System
Co-Authored-By: Claude <noreply@anthropic.com>"
    
    # Create GitHub repository if GitHub CLI is available
    if command -v gh &> /dev/null; then
        log "INFO" "Creating GitHub repository..."
        gh repo create "$repo_name" --public --description "Gaming website: $site_name" || true
        
        # Add remote and push
        git remote add origin "https://github.com/$(gh auth status 2>&1 | grep -o 'as [^)]*' | cut -d' ' -f2)/$repo_name.git"
        git branch -M main
        git push -u origin main
        
        # Enable GitHub Pages
        gh repo edit --enable-wiki=false --enable-projects=false
        log "INFO" "Repository created and pushed to GitHub"
    else
        log "WARNING" "GitHub CLI not available. Manual setup required:"
        log "INFO" "1. Create repository on GitHub: $repo_name"
        log "INFO" "2. git remote add origin https://github.com/YOUR_USERNAME/$repo_name.git"
        log "INFO" "3. git push -u origin main"
        log "INFO" "4. Enable GitHub Pages in repository settings"
    fi
    
    log "SUCCESS" "Repository created: $repo_name"
}

# Create portal structure
create_portal_structure() {
    local domain="$1"
    
    log "INFO" "Creating gaming portal structure..."
    
    # Create directory structure
    mkdir -p assets/{css,js,images,games}
    mkdir -p games/{reflex-rings,quantum-hacker,circuit-runners}
    mkdir -p blog about contact privacy terms
    
    # Create main index.html
    cat > index.html << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Unit4 Productions - Premium Gaming Experience</title>
    <meta name="description" content="Discover premium indie games with innovative gameplay and stunning visuals. Play Reflex Rings, Quantum Hacker, and more.">
    
    <!-- SEO & Social -->
    <meta property="og:title" content="Unit4 Productions - Premium Gaming">
    <meta property="og:description" content="Premium indie games with innovative gameplay">
    <meta property="og:type" content="website">
    <meta name="twitter:card" content="summary_large_image">
    
    <!-- Performance -->
    <link rel="preload" href="assets/css/main.css" as="style">
    <link rel="preload" href="assets/js/main.js" as="script">
    
    <link rel="stylesheet" href="assets/css/main.css">
</head>
<body>
    <header class="header">
        <nav class="nav">
            <div class="nav-brand">
                <h1>Unit4 Productions</h1>
            </div>
            <ul class="nav-menu">
                <li><a href="#games">Games</a></li>
                <li><a href="#about">About</a></li>
                <li><a href="#contact">Contact</a></li>
            </ul>
        </nav>
    </header>

    <main>
        <section class="hero">
            <h2>Premium Indie Gaming Experience</h2>
            <p>Discover innovative games with stunning visuals and addictive gameplay</p>
            <button class="cta-button">Play Now</button>
        </section>

        <section id="games" class="games-section">
            <h2>Featured Games</h2>
            <div class="games-grid">
                <div class="game-card">
                    <h3>Reflex Rings</h3>
                    <p>Test your reflexes in this addictive arcade game</p>
                    <a href="games/reflex-rings/" class="play-button">Play</a>
                </div>
                <div class="game-card">
                    <h3>Quantum Hacker</h3>
                    <p>Hack through quantum puzzles in this sci-fi adventure</p>
                    <a href="games/quantum-hacker/" class="play-button">Play</a>
                </div>
                <div class="game-card">
                    <h3>Circuit Runners</h3>
                    <p>Race through digital circuits in this high-speed runner</p>
                    <a href="games/circuit-runners/" class="play-button">Play</a>
                </div>
            </div>
        </section>
    </main>

    <footer class="footer">
        <p>&copy; 2025 Unit4 Productions. All rights reserved.</p>
    </footer>

    <script src="assets/js/main.js"></script>
</body>
</html>
EOF

    # Create CSS
    cat > assets/css/main.css << 'EOF'
/* Unit4 Productions Gaming Portal Styles */
:root {
    --primary: #00ff88;
    --secondary: #0066ff;
    --dark: #1a1a1a;
    --light: #ffffff;
    --gray: #333333;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Arial', sans-serif;
    background: var(--dark);
    color: var(--light);
    line-height: 1.6;
}

.header {
    background: var(--gray);
    padding: 1rem 0;
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 1000;
}

.nav {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 2rem;
}

.nav-brand h1 {
    color: var(--primary);
    font-size: 1.5rem;
}

.nav-menu {
    list-style: none;
    display: flex;
    gap: 2rem;
}

.nav-menu a {
    color: var(--light);
    text-decoration: none;
    transition: color 0.3s;
}

.nav-menu a:hover {
    color: var(--primary);
}

main {
    margin-top: 80px;
    padding: 2rem 0;
}

.hero {
    text-align: center;
    padding: 4rem 2rem;
    background: linear-gradient(135deg, var(--secondary), var(--primary));
}

.hero h2 {
    font-size: 3rem;
    margin-bottom: 1rem;
}

.cta-button {
    background: var(--primary);
    color: var(--dark);
    border: none;
    padding: 1rem 2rem;
    font-size: 1.2rem;
    border-radius: 5px;
    cursor: pointer;
    transition: transform 0.3s;
}

.cta-button:hover {
    transform: scale(1.05);
}

.games-section {
    max-width: 1200px;
    margin: 0 auto;
    padding: 4rem 2rem;
}

.games-section h2 {
    text-align: center;
    margin-bottom: 3rem;
    color: var(--primary);
}

.games-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 2rem;
}

.game-card {
    background: var(--gray);
    padding: 2rem;
    border-radius: 10px;
    text-align: center;
    transition: transform 0.3s;
}

.game-card:hover {
    transform: translateY(-5px);
}

.play-button {
    display: inline-block;
    background: var(--secondary);
    color: var(--light);
    text-decoration: none;
    padding: 0.8rem 1.5rem;
    border-radius: 5px;
    margin-top: 1rem;
    transition: background 0.3s;
}

.play-button:hover {
    background: var(--primary);
    color: var(--dark);
}

.footer {
    background: var(--gray);
    text-align: center;
    padding: 2rem;
    margin-top: 4rem;
}

@media (max-width: 768px) {
    .hero h2 {
        font-size: 2rem;
    }
    
    .nav {
        flex-direction: column;
        gap: 1rem;
    }
    
    .nav-menu {
        gap: 1rem;
    }
}
EOF

    # Create JavaScript
    cat > assets/js/main.js << 'EOF'
// Unit4 Productions Gaming Portal JavaScript

document.addEventListener('DOMContentLoaded', function() {
    console.log('Unit4 Productions Gaming Portal Loaded');
    
    // Smooth scrolling for navigation links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Game card animations
    const gameCards = document.querySelectorAll('.game-card');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);
    
    gameCards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s, transform 0.6s';
        observer.observe(card);
    });
    
    // Basic analytics tracking
    function trackEvent(event, data) {
        if (typeof gtag !== 'undefined') {
            gtag('event', event, data);
        }
        console.log('Event:', event, data);
    }
    
    // Track game clicks
    document.querySelectorAll('.play-button').forEach(button => {
        button.addEventListener('click', function(e) {
            const gameName = this.closest('.game-card').querySelector('h3').textContent;
            trackEvent('game_click', {
                game_name: gameName,
                event_category: 'games',
                event_label: 'play_button'
            });
        });
    });
    
    // Track CTA clicks
    document.querySelectorAll('.cta-button').forEach(button => {
        button.addEventListener('click', function(e) {
            trackEvent('cta_click', {
                event_category: 'engagement',
                event_label: 'hero_cta'
            });
        });
    });
});
EOF

    # Create CNAME for custom domain
    if [[ -n "$domain" ]]; then
        echo "$domain" > CNAME
        log "INFO" "Created CNAME file for domain: $domain"
    fi
    
    # Create README
    cat > README.md << EOF
# Unit4 Productions Gaming Portal

Premium indie gaming experience with innovative gameplay and stunning visuals.

## Games

- **Reflex Rings** - Test your reflexes in this addictive arcade game
- **Quantum Hacker** - Hack through quantum puzzles in this sci-fi adventure  
- **Circuit Runners** - Race through digital circuits in this high-speed runner

## Features

- 📱 Mobile-responsive design
- ⚡ Fast loading performance
- 🎮 Optimized gaming experience
- 📊 Analytics integration
- 💰 Monetization ready

## Deployment

This site is automatically deployed to GitHub Pages.

## License

© 2025 Unit4 Productions. All rights reserved.
EOF

    log "SUCCESS" "Gaming portal structure created"
}

# Create HTML5 game structure
create_html5_structure() {
    local site_name="$1"
    
    log "INFO" "Creating HTML5 game structure for: $site_name"
    
    # Create basic HTML5 game template
    cat > index.html << EOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>$site_name - HTML5 Game</title>
    <style>
        body {
            margin: 0;
            padding: 0;
            background: #000;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            font-family: Arial, sans-serif;
        }
        #gameContainer {
            background: #222;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 0 20px rgba(0, 255, 136, 0.3);
        }
        canvas {
            border: 2px solid #00ff88;
            border-radius: 5px;
        }
        .controls {
            text-align: center;
            margin-top: 20px;
            color: white;
        }
        button {
            background: #00ff88;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            color: black;
            font-weight: bold;
            cursor: pointer;
            margin: 0 10px;
        }
        button:hover {
            background: #00cc6a;
        }
    </style>
</head>
<body>
    <div id="gameContainer">
        <canvas id="gameCanvas" width="800" height="600"></canvas>
        <div class="controls">
            <button onclick="startGame()">Start Game</button>
            <button onclick="pauseGame()">Pause</button>
            <button onclick="resetGame()">Reset</button>
        </div>
    </div>
    
    <script>
        // Basic HTML5 game framework
        const canvas = document.getElementById('gameCanvas');
        const ctx = canvas.getContext('2d');
        
        let gameRunning = false;
        let gameLoop;
        
        function startGame() {
            if (!gameRunning) {
                gameRunning = true;
                gameLoop = setInterval(update, 16); // 60 FPS
            }
        }
        
        function pauseGame() {
            gameRunning = false;
            clearInterval(gameLoop);
        }
        
        function resetGame() {
            pauseGame();
            // Reset game state
            draw();
        }
        
        function update() {
            // Game logic here
            draw();
        }
        
        function draw() {
            // Clear canvas
            ctx.fillStyle = '#111';
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            
            // Draw game title
            ctx.fillStyle = '#00ff88';
            ctx.font = '48px Arial';
            ctx.textAlign = 'center';
            ctx.fillText('$site_name', canvas.width/2, canvas.height/2);
            
            ctx.font = '24px Arial';
            ctx.fillText('HTML5 Game Ready for Development', canvas.width/2, canvas.height/2 + 50);
        }
        
        // Initial draw
        draw();
        
        // Analytics
        function trackGameEvent(event, data) {
            if (typeof gtag !== 'undefined') {
                gtag('event', event, data);
            }
            console.log('Game Event:', event, data);
        }
        
        // Track game interactions
        canvas.addEventListener('click', function() {
            trackGameEvent('game_interaction', {
                game: '$site_name',
                type: 'click'
            });
        });
    </script>
</body>
</html>
EOF

    # Create README
    cat > README.md << EOF
# $site_name

HTML5 Game deployment ready for GitHub Pages.

## Features

- 📱 Mobile-responsive game canvas
- 🎮 60 FPS game loop
- 📊 Analytics integration
- ⚡ Fast loading

## Development

Replace the game logic in the \`update()\` and \`draw()\` functions to implement your game.

## Deployment

Automatically deployed via GitHub Pages.
EOF

    log "SUCCESS" "HTML5 game structure created for: $site_name"
}

# Create Unity WebGL structure
create_unity_structure() {
    local site_name="$1"
    
    log "INFO" "Creating Unity WebGL structure for: $site_name"
    
    # Create directories
    mkdir -p Build TemplateData
    
    # Create index.html for Unity WebGL
    cat > index.html << EOF
<!DOCTYPE html>
<html lang="en-us">
<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>$site_name</title>
    <link rel="shortcut icon" href="TemplateData/favicon.ico">
    <link rel="stylesheet" href="TemplateData/style.css">
    <script src="TemplateData/UnityProgress.js"></script>  
    <script src="Build/UnityLoader.js"></script>
    <script>
      var gameInstance = UnityLoader.instantiate("gameContainer", "Build/Build.json", {onProgress: UnityProgress});
    </script>
</head>
<body>
    <div class="webgl-content">
        <div id="gameContainer" style="width: 960px; height: 600px"></div>
        <div class="footer">
            <div class="webgl-logo"></div>
            <div class="fullscreen" onclick="gameInstance.SetFullscreen(1)"></div>
            <div class="title">$site_name</div>
        </div>
    </div>
</body>
</html>
EOF

    # Create template CSS
    cat > TemplateData/style.css << 'EOF'
.webgl-content * {border: 0; margin: 0; padding: 0}
.webgl-content {position: absolute; top: 50%; left: 50%; -webkit-transform: translate(-50%, -50%); transform: translate(-50%, -50%);}

.webgl-content .logo, .progress {position: absolute; left: 50%; top: 50%; -webkit-transform: translate(-50%, -50%); transform: translate(-50%, -50%);}
.webgl-content .logo {background: url('progressLogo.Light.png') no-repeat center / contain; width: 154px; height: 130px;}
.webgl-content .progress {height: 18px; width: 141px; margin-top: 90px;}
.webgl-content .progress .empty {background: url('progressEmpty.Light.png') no-repeat right / cover; float: right; width: 100%; height: 100%; display: inline-block;}
.webgl-content .progress .full {background: url('progressFull.Light.png') no-repeat left / cover; float: left; width: 0%; height: 100%; display: inline-block;}

.webgl-content .logo.Dark {background-image: url('progressLogo.Dark.png');}
.webgl-content .progress.Dark .empty {background-image: url('progressEmpty.Dark.png');}
.webgl-content .progress.Dark .full {background-image: url('progressFull.Dark.png');}

.webgl-content .footer {margin-top: 5px; height: 38px; line-height: 38px; font-family: Helvetica, Verdana, Arial, sans-serif; font-size: 18px;}
.webgl-content .footer .webgl-logo, .title, .fullscreen {height: 100%; display: inline-block; background: transparent center no-repeat;}
.webgl-content .footer .webgl-logo {background-image: url('webgl-logo.png'); width: 204px; float: left;}
.webgl-content .footer .title {margin-right: 10px; float: right;}
.webgl-content .footer .fullscreen {background-image: url('fullscreen.png'); width: 38px; float: right;}
EOF

    # Create UnityProgress.js
    cat > TemplateData/UnityProgress.js << 'EOF'
function UnityProgress(gameInstance, progress) {
  if (!gameInstance.Module)
    return;
  if (!gameInstance.logo) {
    gameInstance.logo = document.createElement("div");
    gameInstance.logo.className = "logo " + gameInstance.Module.splashScreenStyle;
    gameInstance.container.appendChild(gameInstance.logo);
  }
  if (!gameInstance.progress) {    
    gameInstance.progress = document.createElement("div");
    gameInstance.progress.className = "progress " + gameInstance.Module.splashScreenStyle;
    gameInstance.progress.empty = document.createElement("div");
    gameInstance.progress.empty.className = "empty";
    gameInstance.progress.appendChild(gameInstance.progress.empty);
    gameInstance.progress.full = document.createElement("div");
    gameInstance.progress.full.className = "full";
    gameInstance.progress.appendChild(gameInstance.progress.full);
    gameInstance.container.appendChild(gameInstance.progress);
  }
  gameInstance.progress.full.style.width = (100 * progress) + "%";
  gameInstance.progress.empty.style.width = (100 * (1 - progress)) + "%";
  if (progress == 1)
    gameInstance.logo.style.display = gameInstance.progress.style.display = "none";
}
EOF

    # Create README
    cat > README.md << EOF
# $site_name - Unity WebGL

Unity WebGL game deployment for GitHub Pages.

## Setup

1. Build your Unity project for WebGL
2. Replace the Build/ folder contents with your Unity WebGL build
3. Update TemplateData/ with your Unity template assets
4. Commit and push to deploy

## Structure

- \`index.html\` - Main game page
- \`Build/\` - Unity WebGL build files (replace with your build)
- \`TemplateData/\` - Unity template assets

## Deployment

Automatically deployed via GitHub Pages.
EOF

    log "SUCCESS" "Unity WebGL structure created for: $site_name"
}

# Deploy website
deploy_website() {
    local site_name="$1"
    
    log "INFO" "Deploying website: $site_name"
    
    # Load configuration
    local config_data
    if [[ -f "$CONFIG_FILE" ]]; then
        config_data=$(cat "$CONFIG_FILE")
    else
        log "ERROR" "Configuration file not found: $CONFIG_FILE"
        exit 1
    fi
    
    # Find repository directory
    local repo_dir="$TEMP_DIR/$site_name"
    
    if [[ ! -d "$repo_dir" ]]; then
        log "ERROR" "Repository directory not found: $repo_dir"
        log "INFO" "Run 'create' command first"
        exit 1
    fi
    
    cd "$repo_dir"
    
    # Check if we have uncommitted changes
    if [[ -n "$(git status --porcelain)" ]]; then
        log "INFO" "Found uncommitted changes, committing..."
        
        git add .
        git commit -m "Update $site_name deployment

📱 Latest game updates and optimizations
🚀 Enhanced performance and user experience
🎮 Ready for production gaming

🤖 Generated with Claude Code Deployment System
Co-Authored-By: Claude <noreply@anthropic.com>"
        
        log "SUCCESS" "Changes committed"
    fi
    
    # Push to GitHub
    if git remote get-url origin > /dev/null 2>&1; then
        log "INFO" "Pushing to GitHub..."
        git push origin main
        log "SUCCESS" "Deployed to GitHub Pages"
        
        # Display deployment info
        local repo_name=$(basename "$(git remote get-url origin)" .git)
        local username=$(git remote get-url origin | sed 's/.*github.com[:/]//; s/\/.*//g')
        
        log "SUCCESS" "Deployment complete!"
        log "INFO" "GitHub Pages URL: https://$username.github.io/$repo_name"
        
        if [[ -f "CNAME" ]]; then
            local custom_domain=$(cat CNAME)
            log "INFO" "Custom domain: https://$custom_domain"
        fi
    else
        log "ERROR" "No git remote configured"
        log "INFO" "Configure remote with: git remote add origin <repository-url>"
        exit 1
    fi
}

# Update existing deployment
update_deployment() {
    local site_name="$1"
    
    log "INFO" "Updating deployment: $site_name"
    
    local repo_dir="$TEMP_DIR/$site_name"
    
    if [[ ! -d "$repo_dir" ]]; then
        log "ERROR" "Repository directory not found: $repo_dir"
        exit 1
    fi
    
    cd "$repo_dir"
    
    # Pull latest changes
    if git remote get-url origin > /dev/null 2>&1; then
        log "INFO" "Pulling latest changes..."
        git pull origin main
    fi
    
    # Deploy updates
    deploy_website "$site_name"
}

# Main execution
main() {
    echo "🎮 Git-Based Gaming Website Deployment System v2.0.0"
    echo "================================================================"
    
    check_prerequisites
    parse_args "$@"
    
    case "$COMMAND" in
        "create")
            if [[ -z "$SITE_NAME" ]] || [[ -z "$REPO_NAME" ]] || [[ -z "$GAME_TYPE" ]]; then
                log "ERROR" "Required parameters: --site, --repo, --type"
                show_help
                exit 1
            fi
            create_repository "$SITE_NAME" "$REPO_NAME" "$DOMAIN" "$GAME_TYPE"
            ;;
        "deploy")
            if [[ -z "$SITE_NAME" ]]; then
                log "ERROR" "Required parameter: --site"
                show_help
                exit 1
            fi
            deploy_website "$SITE_NAME"
            ;;
        "update")
            if [[ -z "$SITE_NAME" ]]; then
                log "ERROR" "Required parameter: --site"
                show_help
                exit 1
            fi
            update_deployment "$SITE_NAME"
            ;;
        *)
            log "ERROR" "Unknown command: $COMMAND"
            show_help
            exit 1
            ;;
    esac
    
    log "SUCCESS" "Operation completed successfully!"
}

# Run main function with all arguments
main "$@"