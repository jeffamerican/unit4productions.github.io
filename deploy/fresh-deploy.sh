#!/bin/bash

# Fresh GitHub Pages Deployment - CEO Fix
echo "🔧 CEO Emergency Deployment Fix"
echo "================================"

# Create fresh deployment with minimal content first
FRESH_DIR="/tmp/unit4-fresh-deploy"
rm -rf "$FRESH_DIR"
mkdir -p "$FRESH_DIR"

echo "📝 Creating minimal test homepage..."

# Create simple test page to verify GitHub Pages works
cat > "$FRESH_DIR/index.html" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Unit4Productions Gaming - Test</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background: #000;
            color: #00ff00;
            padding: 50px;
            text-align: center;
        }
        h1 { font-size: 3rem; margin-bottom: 20px; }
        p { font-size: 1.5rem; margin-bottom: 30px; }
        .test { color: #ffff00; font-weight: bold; }
    </style>
</head>
<body>
    <h1>Unit4Productions Gaming</h1>
    <p class="test">🎮 DEPLOYMENT TEST - CEO MODE 🎮</p>
    <p>If you can see this, GitHub Pages is working!</p>
    <p>Full gaming website coming momentarily...</p>
    <script>
        console.log("Unit4Productions Gaming - CEO Emergency Deploy Test");
        document.body.innerHTML += '<p style="color:#ff00ff;">JavaScript working ✓</p>';
    </script>
</body>
</html>
EOF

echo "🔧 Initializing fresh git repository..."
cd "$FRESH_DIR"
git init
git branch -M main
git add .
git commit -m "CEO Emergency Deploy Test - Minimal site to verify GitHub Pages"

echo ""
echo "✅ Fresh test deployment ready!"
echo "📁 Location: $FRESH_DIR"
echo ""
echo "🚀 Next steps:"
echo "1. This creates a minimal test page"
echo "2. After this works, we'll add games back"
echo "3. Systematic CEO approach to fix the issue"
echo ""