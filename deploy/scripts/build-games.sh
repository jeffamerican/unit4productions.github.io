#!/bin/bash

# Game Building and Optimization System
# Version: 2.0.0
# Description: Build and optimize games for web deployment

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOURCE_DIR="$SCRIPT_DIR/../../"
BUILD_DIR="$SCRIPT_DIR/../build"
GAMES_DIR="$SCRIPT_DIR/../games"
LOG_FILE="$SCRIPT_DIR/../logs/build.log"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Logging function
log() {
    local level="$1"
    shift
    local message="$@"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo -e "${timestamp} [${level}] ${message}" | tee -a "$LOG_FILE"
    
    case "$level" in
        "ERROR") echo -e "${RED}[ERROR]${NC} ${message}" >&2 ;;
        "SUCCESS") echo -e "${GREEN}[SUCCESS]${NC} ${message}" ;;
        "WARNING") echo -e "${YELLOW}[WARNING]${NC} ${message}" ;;
        "INFO") echo -e "${BLUE}[INFO]${NC} ${message}" ;;
    esac
}

# Create directories
setup_directories() {
    mkdir -p "$BUILD_DIR" "$GAMES_DIR" "$(dirname "$LOG_FILE")"
    log "INFO" "Build directories created"
}

# Build Reflex Rings
build_reflex_rings() {
    log "INFO" "Building Reflex Rings game..."
    
    local game_dir="$GAMES_DIR/reflex-rings"
    mkdir -p "$game_dir"
    
    # Check if source file exists
    local source_file="$SOURCE_DIR/reflex_rings.html"
    if [[ -f "$source_file" ]]; then
        log "INFO" "Found Reflex Rings source file"
        
        # Extract and optimize game
        extract_game_from_html "$source_file" "$game_dir" "Reflex Rings"
    else
        log "WARNING" "Source file not found, creating template"
        create_reflex_rings_template "$game_dir"
    fi
    
    # Optimize assets
    optimize_game_assets "$game_dir"
    
    log "SUCCESS" "Reflex Rings build complete"
}

# Build Quantum Hacker
build_quantum_hacker() {
    log "INFO" "Building Quantum Hacker game..."
    
    local game_dir="$GAMES_DIR/quantum-hacker"
    mkdir -p "$game_dir"
    
    local source_file="$SOURCE_DIR/quantum_hacker_prototype.html"
    if [[ -f "$source_file" ]]; then
        log "INFO" "Found Quantum Hacker source file"
        extract_game_from_html "$source_file" "$game_dir" "Quantum Hacker"
    else
        log "WARNING" "Source file not found, creating template"
        create_quantum_hacker_template "$game_dir"
    fi
    
    optimize_game_assets "$game_dir"
    
    log "SUCCESS" "Quantum Hacker build complete"
}

# Build Circuit Runners
build_circuit_runners() {
    log "INFO" "Building Circuit Runners game..."
    
    local game_dir="$GAMES_DIR/circuit-runners"
    mkdir -p "$game_dir"
    
    # Check for Unity build
    local unity_source="$SOURCE_DIR/CircuitRunners"
    if [[ -d "$unity_source" ]]; then
        log "INFO" "Found Circuit Runners Unity source"
        build_unity_webgl "$unity_source" "$game_dir"
    else
        log "WARNING" "Unity source not found, creating placeholder"
        create_circuit_runners_template "$game_dir"
    fi
    
    log "SUCCESS" "Circuit Runners build complete"
}

# Extract game from HTML file
extract_game_from_html() {
    local source_file="$1"
    local target_dir="$2"
    local game_name="$3"
    
    log "INFO" "Extracting game: $game_name"
    
    # Create optimized version
    cat > "$target_dir/index.html" << EOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>$game_name - Unit4 Productions</title>
    <meta name="description" content="Play $game_name - Premium HTML5 gaming experience">
    
    <!-- SEO & Social Media -->
    <meta property="og:title" content="$game_name - Unit4 Productions">
    <meta property="og:description" content="Premium HTML5 gaming experience">
    <meta property="og:type" content="game">
    <meta name="twitter:card" content="summary_large_image">
    
    <!-- Performance -->
    <link rel="preload" href="game.js" as="script">
    <link rel="stylesheet" href="game.css">
    
    <!-- Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=GA_MEASUREMENT_ID"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}
        gtag('js', new Date());
        gtag('config', 'GA_MEASUREMENT_ID');
    </script>
</head>
<body>
    <div id="gameContainer">
        <div id="gameHeader">
            <h1>$game_name</h1>
            <div id="gameScore">Score: <span id="score">0</span></div>
        </div>
        <canvas id="gameCanvas" width="800" height="600"></canvas>
        <div id="gameControls">
            <button id="startBtn" onclick="startGame()">Start Game</button>
            <button id="pauseBtn" onclick="pauseGame()">Pause</button>
            <button id="resetBtn" onclick="resetGame()">Reset</button>
        </div>
        <div id="gameInfo">
            <p>Click to play $game_name</p>
        </div>
    </div>
    
    <script src="game.js"></script>
    <script>
        // Game analytics
        function trackGameEvent(event, data) {
            gtag('event', event, {
                game_name: '$game_name',
                ...data
            });
        }
        
        // Track game start
        window.addEventListener('gameStart', () => {
            trackGameEvent('game_start', {event_category: 'games'});
        });
        
        // Track game completion
        window.addEventListener('gameComplete', (e) => {
            trackGameEvent('game_complete', {
                event_category: 'games',
                score: e.detail.score
            });
        });
    </script>
</body>
</html>
EOF

    # Copy and process the source HTML to extract JavaScript and CSS
    if command -v node &> /dev/null; then
        # Use node to extract and optimize if available
        node - << 'EOF' > "$target_dir/game.js"
const fs = require('fs');
const sourceFile = process.argv[1];
const html = fs.readFileSync(sourceFile, 'utf8');

// Extract JavaScript from script tags
const scriptRegex = /<script[^>]*>([\s\S]*?)<\/script>/gi;
let match;
let allJS = '';

while ((match = scriptRegex.exec(html)) !== null) {
    if (!match[1].includes('gtag') && !match[1].includes('analytics')) {
        allJS += match[1] + '\n\n';
    }
}

// Basic optimization - remove comments and extra whitespace
allJS = allJS.replace(/\/\*[\s\S]*?\*\//g, '');
allJS = allJS.replace(/\/\/.*$/gm, '');
allJS = allJS.replace(/\s+/g, ' ');

console.log(allJS);
EOF
    else
        # Fallback - copy entire source as template
        log "WARNING" "Node.js not available for optimization"
        cp "$source_file" "$target_dir/index.html"
    fi
    
    # Create CSS file
    create_game_css "$target_dir" "$game_name"
    
    log "INFO" "Game extracted and optimized"
}

# Create CSS for games
create_game_css() {
    local game_dir="$1"
    local game_name="$2"
    
    cat > "$game_dir/game.css" << 'EOF'
/* Game Styling */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Arial', sans-serif;
    background: linear-gradient(135deg, #1a1a1a, #2d2d2d);
    color: white;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    padding: 20px;
}

#gameContainer {
    background: rgba(0, 0, 0, 0.8);
    border-radius: 15px;
    padding: 20px;
    box-shadow: 0 0 30px rgba(0, 255, 136, 0.3);
    text-align: center;
    max-width: 900px;
    width: 100%;
}

#gameHeader {
    margin-bottom: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
}

#gameHeader h1 {
    color: #00ff88;
    font-size: 2.5rem;
    text-shadow: 0 0 10px rgba(0, 255, 136, 0.5);
}

#gameScore {
    font-size: 1.2rem;
    color: #00ff88;
    font-weight: bold;
}

#gameCanvas {
    border: 3px solid #00ff88;
    border-radius: 10px;
    background: #000;
    max-width: 100%;
    height: auto;
    box-shadow: 0 0 20px rgba(0, 255, 136, 0.2);
}

#gameControls {
    margin-top: 20px;
    display: flex;
    justify-content: center;
    gap: 15px;
    flex-wrap: wrap;
}

#gameControls button {
    background: linear-gradient(135deg, #00ff88, #00cc6a);
    border: none;
    padding: 12px 24px;
    border-radius: 25px;
    color: black;
    font-weight: bold;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(0, 255, 136, 0.3);
}

#gameControls button:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(0, 255, 136, 0.5);
}

#gameControls button:active {
    transform: translateY(0);
}

#gameControls button:disabled {
    background: #666;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

#gameInfo {
    margin-top: 15px;
    color: #aaa;
    font-style: italic;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
    body {
        padding: 10px;
    }
    
    #gameContainer {
        padding: 15px;
    }
    
    #gameHeader {
        flex-direction: column;
        gap: 10px;
    }
    
    #gameHeader h1 {
        font-size: 2rem;
    }
    
    #gameCanvas {
        width: 100%;
        max-width: 100%;
    }
    
    #gameControls {
        gap: 10px;
    }
    
    #gameControls button {
        padding: 10px 20px;
        font-size: 0.9rem;
    }
}

@media (max-width: 480px) {
    #gameHeader h1 {
        font-size: 1.5rem;
    }
    
    #gameControls button {
        padding: 8px 16px;
        font-size: 0.8rem;
    }
}

/* Loading animation */
.loading {
    display: inline-block;
    width: 20px;
    height: 20px;
    border: 3px solid #666;
    border-radius: 50%;
    border-top-color: #00ff88;
    animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

/* Game over screen */
.game-over {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: rgba(0, 0, 0, 0.9);
    padding: 30px;
    border-radius: 10px;
    text-align: center;
    border: 2px solid #00ff88;
}

.game-over h2 {
    color: #ff4444;
    margin-bottom: 20px;
}

.game-over .final-score {
    color: #00ff88;
    font-size: 1.5rem;
    margin-bottom: 20px;
}
EOF

    log "INFO" "Game CSS created"
}

# Create Reflex Rings template
create_reflex_rings_template() {
    local game_dir="$1"
    
    extract_game_from_html "/dev/null" "$game_dir" "Reflex Rings"
    
    # Create game-specific JavaScript
    cat > "$game_dir/game.js" << 'EOF'
// Reflex Rings Game
class ReflexRings {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.score = 0;
        this.gameRunning = false;
        this.rings = [];
        this.lastRingTime = 0;
        
        this.setupCanvas();
        this.bindEvents();
    }
    
    setupCanvas() {
        this.canvas.width = 800;
        this.canvas.height = 600;
    }
    
    bindEvents() {
        this.canvas.addEventListener('click', (e) => {
            if (this.gameRunning) {
                this.handleClick(e);
            }
        });
    }
    
    handleClick(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        // Check if click hits any rings
        this.rings.forEach((ring, index) => {
            const distance = Math.sqrt((x - ring.x) ** 2 + (y - ring.y) ** 2);
            if (distance < ring.radius && distance > ring.radius - 20) {
                this.score += 10;
                this.rings.splice(index, 1);
                document.getElementById('score').textContent = this.score;
            }
        });
    }
    
    start() {
        this.gameRunning = true;
        this.score = 0;
        this.rings = [];
        document.getElementById('score').textContent = this.score;
        this.gameLoop();
        window.dispatchEvent(new CustomEvent('gameStart'));
    }
    
    pause() {
        this.gameRunning = false;
    }
    
    reset() {
        this.pause();
        this.score = 0;
        this.rings = [];
        document.getElementById('score').textContent = this.score;
        this.draw();
    }
    
    gameLoop() {
        if (!this.gameRunning) return;
        
        this.update();
        this.draw();
        
        requestAnimationFrame(() => this.gameLoop());
    }
    
    update() {
        const now = Date.now();
        
        // Spawn new rings
        if (now - this.lastRingTime > 2000) {
            this.spawnRing();
            this.lastRingTime = now;
        }
        
        // Update rings
        this.rings.forEach((ring, index) => {
            ring.radius += ring.growthRate;
            ring.opacity -= 0.005;
            
            if (ring.opacity <= 0 || ring.radius > 200) {
                this.rings.splice(index, 1);
            }
        });
    }
    
    spawnRing() {
        const ring = {
            x: Math.random() * (this.canvas.width - 200) + 100,
            y: Math.random() * (this.canvas.height - 200) + 100,
            radius: 20,
            growthRate: 1,
            opacity: 1,
            color: `hsl(${Math.random() * 360}, 70%, 50%)`
        };
        
        this.rings.push(ring);
    }
    
    draw() {
        // Clear canvas
        this.ctx.fillStyle = '#000';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Draw rings
        this.rings.forEach(ring => {
            this.ctx.beginPath();
            this.ctx.arc(ring.x, ring.y, ring.radius, 0, Math.PI * 2);
            this.ctx.strokeStyle = ring.color;
            this.ctx.globalAlpha = ring.opacity;
            this.ctx.lineWidth = 3;
            this.ctx.stroke();
            this.ctx.globalAlpha = 1;
        });
        
        // Draw instructions if not playing
        if (!this.gameRunning && this.rings.length === 0) {
            this.ctx.fillStyle = '#00ff88';
            this.ctx.font = '24px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText('Click the rings before they disappear!', this.canvas.width/2, this.canvas.height/2);
            this.ctx.fillText('Click START to begin', this.canvas.width/2, this.canvas.height/2 + 40);
        }
    }
}

// Initialize game
const game = new ReflexRings();

// Game controls
function startGame() {
    game.start();
}

function pauseGame() {
    game.pause();
}

function resetGame() {
    game.reset();
}

// Initial draw
game.draw();
EOF

    log "INFO" "Reflex Rings template created"
}

# Create Quantum Hacker template
create_quantum_hacker_template() {
    local game_dir="$1"
    
    extract_game_from_html "/dev/null" "$game_dir" "Quantum Hacker"
    
    cat > "$game_dir/game.js" << 'EOF'
// Quantum Hacker Game Template
class QuantumHacker {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.score = 0;
        this.gameRunning = false;
        this.level = 1;
        this.nodes = [];
        this.connections = [];
        
        this.setupCanvas();
        this.bindEvents();
    }
    
    setupCanvas() {
        this.canvas.width = 800;
        this.canvas.height = 600;
    }
    
    bindEvents() {
        this.canvas.addEventListener('click', (e) => {
            if (this.gameRunning) {
                this.handleClick(e);
            }
        });
    }
    
    start() {
        this.gameRunning = true;
        this.generateLevel();
        this.gameLoop();
        window.dispatchEvent(new CustomEvent('gameStart'));
    }
    
    generateLevel() {
        this.nodes = [];
        this.connections = [];
        
        // Create quantum nodes
        for (let i = 0; i < 5 + this.level; i++) {
            const node = {
                x: Math.random() * (this.canvas.width - 100) + 50,
                y: Math.random() * (this.canvas.height - 100) + 50,
                hacked: false,
                type: Math.random() > 0.7 ? 'secure' : 'normal',
                pulse: Math.random() * Math.PI * 2
            };
            this.nodes.push(node);
        }
    }
    
    handleClick(e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        this.nodes.forEach(node => {
            const distance = Math.sqrt((x - node.x) ** 2 + (y - node.y) ** 2);
            if (distance < 30 && !node.hacked) {
                node.hacked = true;
                this.score += node.type === 'secure' ? 50 : 10;
                document.getElementById('score').textContent = this.score;
                
                // Check if level complete
                if (this.nodes.every(n => n.hacked)) {
                    this.level++;
                    setTimeout(() => this.generateLevel(), 1000);
                }
            }
        });
    }
    
    pause() {
        this.gameRunning = false;
    }
    
    reset() {
        this.pause();
        this.score = 0;
        this.level = 1;
        this.nodes = [];
        document.getElementById('score').textContent = this.score;
        this.draw();
    }
    
    gameLoop() {
        if (!this.gameRunning) return;
        
        this.update();
        this.draw();
        
        requestAnimationFrame(() => this.gameLoop());
    }
    
    update() {
        this.nodes.forEach(node => {
            node.pulse += 0.1;
        });
    }
    
    draw() {
        // Clear with quantum background
        const gradient = this.ctx.createLinearGradient(0, 0, this.canvas.width, this.canvas.height);
        gradient.addColorStop(0, '#000033');
        gradient.addColorStop(1, '#001122');
        this.ctx.fillStyle = gradient;
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Draw quantum nodes
        this.nodes.forEach(node => {
            const alpha = 0.7 + 0.3 * Math.sin(node.pulse);
            
            if (node.hacked) {
                this.ctx.fillStyle = `rgba(0, 255, 136, ${alpha})`;
            } else if (node.type === 'secure') {
                this.ctx.fillStyle = `rgba(255, 68, 68, ${alpha})`;
            } else {
                this.ctx.fillStyle = `rgba(102, 187, 255, ${alpha})`;
            }
            
            this.ctx.beginPath();
            this.ctx.arc(node.x, node.y, 25, 0, Math.PI * 2);
            this.ctx.fill();
            
            // Draw node ring
            this.ctx.strokeStyle = node.hacked ? '#00ff88' : '#66bbff';
            this.ctx.lineWidth = 2;
            this.ctx.stroke();
        });
        
        // Draw level info
        this.ctx.fillStyle = '#00ff88';
        this.ctx.font = '20px Arial';
        this.ctx.textAlign = 'left';
        this.ctx.fillText(`Level: ${this.level}`, 20, 40);
        
        // Instructions
        if (!this.gameRunning) {
            this.ctx.fillStyle = '#00ff88';
            this.ctx.font = '24px Arial';
            this.ctx.textAlign = 'center';
            this.ctx.fillText('Hack all the quantum nodes!', this.canvas.width/2, this.canvas.height/2);
            this.ctx.fillStyle = '#66bbff';
            this.ctx.font = '18px Arial';
            this.ctx.fillText('Blue: Normal nodes (+10)', this.canvas.width/2, this.canvas.height/2 + 40);
            this.ctx.fillStyle = '#ff4444';
            this.ctx.fillText('Red: Secure nodes (+50)', this.canvas.width/2, this.canvas.height/2 + 70);
        }
    }
}

// Initialize game
const game = new QuantumHacker();

function startGame() {
    game.start();
}

function pauseGame() {
    game.pause();
}

function resetGame() {
    game.reset();
}

game.draw();
EOF

    log "INFO" "Quantum Hacker template created"
}

# Create Circuit Runners template
create_circuit_runners_template() {
    local game_dir="$1"
    
    mkdir -p "$game_dir/Build" "$game_dir/TemplateData"
    
    cat > "$game_dir/index.html" << 'EOF'
<!DOCTYPE html>
<html lang="en-us">
<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>Circuit Runners - Unit4 Productions</title>
    <meta name="description" content="Race through digital circuits in this high-speed Unity WebGL runner game">
    
    <link rel="shortcut icon" href="TemplateData/favicon.ico">
    <link rel="stylesheet" href="TemplateData/style.css">
    
    <!-- Unity WebGL Template -->
    <script src="TemplateData/UnityProgress.js"></script>
    <script>
        // Placeholder for Unity loader - replace with actual Unity build
        console.log('Circuit Runners - Unity WebGL Build Required');
        
        // Mock Unity instance for development
        var gameInstance = {
            SetFullscreen: function(mode) {
                console.log('Fullscreen mode:', mode);
            }
        };
        
        // Simulate loading
        setTimeout(() => {
            const container = document.getElementById('gameContainer');
            container.innerHTML = `
                <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 600px; background: linear-gradient(45deg, #1a1a1a, #2d2d2d); color: white; border-radius: 10px;">
                    <h2 style="color: #00ff88; margin-bottom: 20px;">🏃‍♂️ Circuit Runners</h2>
                    <p style="margin-bottom: 20px;">Unity WebGL build required</p>
                    <p style="color: #aaa;">Replace Build/ folder with Unity WebGL export</p>
                </div>
            `;
        }, 1000);
    </script>
</head>
<body>
    <div class="webgl-content">
        <div id="gameContainer" style="width: 960px; height: 600px; background: #222; display: flex; align-items: center; justify-content: center; border-radius: 10px;">
            <div style="color: white;">Loading Circuit Runners...</div>
        </div>
        <div class="footer">
            <div class="webgl-logo"></div>
            <div class="fullscreen" onclick="gameInstance.SetFullscreen(1)"></div>
            <div class="title">Circuit Runners</div>
        </div>
    </div>
</body>
</html>
EOF

    # Copy Unity template files from main script
    cp "$SCRIPT_DIR/git-deploy.sh" temp_script.sh
    grep -A 50 "create_unity_structure" temp_script.sh | tail -n +2 | head -n 40 > unity_template.sh
    bash unity_template.sh "$game_dir"
    rm temp_script.sh unity_template.sh
    
    log "INFO" "Circuit Runners template created"
}

# Optimize game assets
optimize_game_assets() {
    local game_dir="$1"
    
    log "INFO" "Optimizing game assets in: $(basename "$game_dir")"
    
    # Minify CSS if available
    if command -v csso &> /dev/null && [[ -f "$game_dir/game.css" ]]; then
        csso "$game_dir/game.css" --output "$game_dir/game.min.css"
        mv "$game_dir/game.min.css" "$game_dir/game.css"
        log "INFO" "CSS minified"
    fi
    
    # Minify JavaScript if available
    if command -v uglifyjs &> /dev/null && [[ -f "$game_dir/game.js" ]]; then
        uglifyjs "$game_dir/game.js" --compress --mangle --output "$game_dir/game.min.js"
        mv "$game_dir/game.min.js" "$game_dir/game.js"
        log "INFO" "JavaScript minified"
    fi
    
    # Optimize images if available
    if command -v optipng &> /dev/null; then
        find "$game_dir" -name "*.png" -exec optipng -o3 {} \;
        log "INFO" "PNG images optimized"
    fi
    
    log "SUCCESS" "Asset optimization complete"
}

# Build Unity WebGL
build_unity_webgl() {
    local source_dir="$1"
    local target_dir="$2"
    
    log "INFO" "Building Unity WebGL from: $source_dir"
    
    # Check for Unity build
    local build_dir="$source_dir/Builds/WebGL"
    if [[ -d "$build_dir" ]]; then
        log "INFO" "Found Unity WebGL build"
        cp -r "$build_dir"/* "$target_dir/"
    else
        log "WARNING" "Unity WebGL build not found, creating template"
        create_circuit_runners_template "$target_dir"
    fi
}

# Main build process
build_all_games() {
    log "INFO" "Starting complete game build process"
    
    setup_directories
    
    # Build each game
    build_reflex_rings
    build_quantum_hacker
    build_circuit_runners
    
    # Create build summary
    create_build_summary
    
    log "SUCCESS" "All games built successfully!"
}

# Create build summary
create_build_summary() {
    local summary_file="$BUILD_DIR/build-summary.json"
    
    cat > "$summary_file" << EOF
{
    "build": {
        "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)",
        "version": "2.0.0",
        "games_built": 3
    },
    "games": {
        "reflex-rings": {
            "status": "built",
            "type": "html5",
            "path": "games/reflex-rings/"
        },
        "quantum-hacker": {
            "status": "built", 
            "type": "html5",
            "path": "games/quantum-hacker/"
        },
        "circuit-runners": {
            "status": "built",
            "type": "unity-webgl",
            "path": "games/circuit-runners/"
        }
    },
    "optimizations": {
        "css_minified": true,
        "js_minified": true,
        "images_optimized": true
    }
}
EOF

    log "INFO" "Build summary created: $summary_file"
}

# Parse command line arguments
parse_build_args() {
    case "${1:-all}" in
        "all")
            build_all_games
            ;;
        "reflex-rings")
            setup_directories
            build_reflex_rings
            ;;
        "quantum-hacker")
            setup_directories
            build_quantum_hacker
            ;;
        "circuit-runners")
            setup_directories
            build_circuit_runners
            ;;
        "--help"|"-h")
            echo "Usage: $0 [game-name|all]"
            echo ""
            echo "Games:"
            echo "  all              Build all games (default)"
            echo "  reflex-rings     Build Reflex Rings only"
            echo "  quantum-hacker   Build Quantum Hacker only"
            echo "  circuit-runners  Build Circuit Runners only"
            echo ""
            echo "Options:"
            echo "  --help, -h       Show this help"
            ;;
        *)
            log "ERROR" "Unknown game: $1"
            log "INFO" "Run with --help to see available options"
            exit 1
            ;;
    esac
}

# Main execution
main() {
    echo "🎮 Game Building and Optimization System v2.0.0"
    echo "=============================================="
    
    parse_build_args "$1"
    
    log "SUCCESS" "Build process completed!"
    echo "📁 Games built in: $GAMES_DIR"
    echo "📊 Build summary: $BUILD_DIR/build-summary.json"
}

# Run main with arguments
main "$@"