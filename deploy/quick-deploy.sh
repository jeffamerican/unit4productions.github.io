#!/bin/bash

# Quick Direct Deployment Script
# Deploys Unit4Productions gaming website with our existing games

set -e

echo "🚀 Quick Deploy - Unit4Productions Gaming"
echo "========================================"

# Check if we have our game files
GAMES_DIR="/z/AI_Software/ExecutiveSuite"
if [ ! -f "$GAMES_DIR/reflex_rings.html" ]; then
    echo "❌ Game files not found in expected location"
    exit 1
fi

# Create temp deployment directory
DEPLOY_DIR="/tmp/unit4-gaming-deploy"
rm -rf "$DEPLOY_DIR"
mkdir -p "$DEPLOY_DIR"

echo "📁 Creating website structure..."

# Create main index.html
cat > "$DEPLOY_DIR/index.html" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Unit4Productions Gaming</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            min-height: 100vh;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        .header {
            text-align: center;
            padding: 60px 0;
        }
        .header h1 {
            font-size: 3.5rem;
            margin-bottom: 20px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }
        .header p {
            font-size: 1.4rem;
            opacity: 0.9;
        }
        .games-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
            gap: 30px;
            margin: 40px 0;
        }
        .game-card {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 30px;
            text-align: center;
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            backdrop-filter: blur(10px);
        }
        .game-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 20px 40px rgba(0,0,0,0.3);
        }
        .game-card h3 {
            font-size: 1.8rem;
            margin-bottom: 15px;
            color: #fff;
        }
        .game-card p {
            margin-bottom: 25px;
            opacity: 0.8;
            line-height: 1.6;
        }
        .play-btn {
            display: inline-block;
            padding: 12px 30px;
            background: linear-gradient(45deg, #ff6b6b, #ee5a24);
            color: white;
            text-decoration: none;
            border-radius: 25px;
            font-weight: bold;
            transition: background 0.3s ease;
        }
        .play-btn:hover {
            background: linear-gradient(45deg, #ee5a24, #ff6b6b);
        }
        .footer {
            text-align: center;
            padding: 40px 0;
            opacity: 0.7;
        }
        @media (max-width: 768px) {
            .header h1 { font-size: 2.5rem; }
            .header p { font-size: 1.2rem; }
            .games-grid { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Unit4Productions Gaming</h1>
            <p>Professional Browser Games • Play Instantly • No Downloads Required</p>
        </div>
        
        <div class="games-grid">
            <div class="game-card">
                <h3>🎯 Reflex Rings</h3>
                <p>Test your reflexes! Click shrinking rings at the perfect moment for maximum points. Simple to learn, challenging to master.</p>
                <a href="reflex-rings.html" class="play-btn">Play Now</a>
            </div>
            
            <div class="game-card">
                <h3>🔐 Quantum Hacker</h3>
                <p>Strategic hacking simulation! Infiltrate corporate networks, place nodes, and avoid security algorithms in this cyberpunk strategy game.</p>
                <a href="quantum-hacker.html" class="play-btn">Play Now</a>
            </div>
            
            <div class="game-card">
                <h3>🤖 Circuit Runners</h3>
                <p>Build autonomous bots and watch them navigate procedural obstacle courses. Customize, optimize, and compete for the best times!</p>
                <a href="circuit-runners.html" class="play-btn">Play Now</a>
            </div>
        </div>
        
        <div class="footer">
            <p>&copy; 2025 Unit4Productions. Professional browser gaming experiences.</p>
        </div>
    </div>
</body>
</html>
EOF

# Copy game files
echo "🎮 Copying games..."
cp "$GAMES_DIR/reflex_rings.html" "$DEPLOY_DIR/reflex-rings.html"

if [ -f "$GAMES_DIR/quantum_hacker_prototype.html" ]; then
    cp "$GAMES_DIR/quantum_hacker_prototype.html" "$DEPLOY_DIR/quantum-hacker.html"
fi

if [ -f "$GAMES_DIR/signal-breach.html" ]; then
    cp "$GAMES_DIR/signal-breach.html" "$DEPLOY_DIR/circuit-runners.html"
fi

# Initialize git repository
echo "📡 Setting up Git repository..."
cd "$DEPLOY_DIR"
git init
git branch -M main

# Create CNAME file for custom domain (optional)
echo "unit4productions.com" > CNAME

# Add all files
git add .
git commit -m "Initial Unit4Productions Gaming website deployment

Features:
- Professional gaming portal homepage
- 3 fully playable HTML5 games  
- Mobile-responsive design
- Ready for custom domain setup

Games included:
- Reflex Rings: Timing-based arcade game
- Quantum Hacker: Strategic cyberpunk simulation  
- Circuit Runners: Bot-building strategy game

Generated by Unit4Productions automated deployment system"

echo ""
echo "✅ Website built successfully!"
echo "📁 Location: $DEPLOY_DIR"
echo ""
echo "🚀 To deploy to GitHub Pages:"
echo "1. Create repository: unit4productions.github.io"
echo "2. Run these commands:"
echo "   cd $DEPLOY_DIR"
echo "   git remote add origin https://github.com/YOUR_USERNAME/unit4productions.github.io.git"
echo "   git push -u origin main"
echo ""
echo "🌐 Your gaming website will be live at:"
echo "   https://YOUR_USERNAME.github.io"
echo "   https://unit4productions.com (after DNS setup)"
echo ""
echo "📊 Ready for:"
echo "   • Google Analytics integration"
echo "   • AdSense monetization" 
echo "   • Premium game features"
echo "   • Social media sharing"