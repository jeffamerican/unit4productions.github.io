# ⚡ Quick Start - Gaming Deployment in 5 Minutes

**Deploy your gaming website faster than making coffee.**

## 🚀 Prerequisites (30 seconds)

```bash
# Check if you have git (required)
git --version

# If missing, install git first:
# Windows: Download from git-scm.com
# Mac: brew install git  
# Linux: sudo apt install git
```

## ⚙️ Setup (2 minutes)

```bash
# 1. Configure git (replace with your info)
git config --global user.name "Your Name"
git config --global user.email "your@email.com"

# 2. Optional: Setup GitHub CLI for automation
gh auth login

# 3. Navigate to deployment scripts
cd deploy/scripts

# 4. Make scripts executable
chmod +x *.sh
```

## 🎮 Deploy (2 minutes)

```bash
# Deploy everything automatically
./auto-deploy.sh
```

**That's it!** The script will:
- ✅ Build all games
- ✅ Create GitHub repositories
- ✅ Deploy to GitHub Pages
- ✅ Optimize for mobile
- ✅ Setup analytics

## 🔗 Your Live Sites

Replace `[USERNAME]` with your GitHub username:

- **Main Portal:** `https://[USERNAME].github.io/unit4productions.github.io`
- **Reflex Rings:** `https://[USERNAME].github.io/reflex-rings-game`
- **Quantum Hacker:** `https://[USERNAME].github.io/quantum-hacker-game`
- **Circuit Runners:** `https://[USERNAME].github.io/circuit-runners-game`

## 🔧 Quick Commands

```bash
# Rebuild all games
./build-games.sh

# Update existing deployment  
./git-deploy.sh update --site unit4productions

# Validate everything works
./validate-deployment.sh quick

# Deploy single game
./git-deploy.sh create --site reflex-rings --type html5
```

## 🆘 Quick Fixes

**Permission denied?**
```bash
chmod +x deploy/scripts/*.sh
```

**Git config missing?**
```bash
git config --global user.name "Your Name"
git config --global user.email "your@email.com"
```

**GitHub authentication failed?**
```bash
gh auth login
```

**Need help?** Check the full [SETUP_GUIDE.md](./SETUP_GUIDE.md)

---

**🎉 Congratulations! Your gaming websites are now live!** 

*Share your games, track analytics, and start making money!*