#!/bin/bash

# Deployment Validation and Testing System
# Version: 2.0.0
# Description: Comprehensive testing for deployed games and websites

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_FILE="$SCRIPT_DIR/../logs/validation.log"
REPORT_FILE="$SCRIPT_DIR/../reports/validation-report.json"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
WARNING_TESTS=0

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
        "TEST") echo -e "${CYAN}[TEST]${NC} ${message}" ;;
    esac
}

# Test result tracking
test_result() {
    local test_name="$1"
    local result="$2"
    local message="$3"
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    
    case "$result" in
        "PASS")
            PASSED_TESTS=$((PASSED_TESTS + 1))
            log "SUCCESS" "✓ $test_name: $message"
            ;;
        "FAIL")
            FAILED_TESTS=$((FAILED_TESTS + 1))
            log "ERROR" "✗ $test_name: $message"
            ;;
        "WARN")
            WARNING_TESTS=$((WARNING_TESTS + 1))
            log "WARNING" "⚠ $test_name: $message"
            ;;
    esac
}

# Initialize validation
init_validation() {
    log "INFO" "Starting deployment validation..."
    
    # Create required directories
    mkdir -p "$(dirname "$LOG_FILE")" "$(dirname "$REPORT_FILE")"
    
    # Clear previous results
    TOTAL_TESTS=0
    PASSED_TESTS=0
    FAILED_TESTS=0
    WARNING_TESTS=0
    
    echo "🔍 Deployment Validation System v2.0.0"
    echo "======================================"
    echo ""
}

# Validate file structure
validate_file_structure() {
    log "TEST" "Validating file structure..."
    
    local base_dir="$SCRIPT_DIR/../"
    local required_files=(
        "config/deployment.json"
        "scripts/git-deploy.sh"
        "scripts/build-games.sh"
        "scripts/auto-deploy.sh"
    )
    
    for file in "${required_files[@]}"; do
        local full_path="$base_dir$file"
        if [[ -f "$full_path" ]]; then
            test_result "File Structure" "PASS" "$file exists"
        else
            test_result "File Structure" "FAIL" "$file missing"
        fi
    done
    
    # Check script permissions
    local scripts=(
        "$SCRIPT_DIR/git-deploy.sh"
        "$SCRIPT_DIR/build-games.sh"
        "$SCRIPT_DIR/auto-deploy.sh"
    )
    
    for script in "${scripts[@]}"; do
        if [[ -f "$script" ]]; then
            if [[ -x "$script" ]]; then
                test_result "Script Permissions" "PASS" "$(basename "$script") is executable"
            else
                log "INFO" "Making $(basename "$script") executable..."
                chmod +x "$script"
                test_result "Script Permissions" "PASS" "$(basename "$script") made executable"
            fi
        fi
    done
}

# Validate game builds
validate_game_builds() {
    log "TEST" "Validating game builds..."
    
    local games_dir="$SCRIPT_DIR/../games"
    local games=("reflex-rings" "quantum-hacker" "circuit-runners")
    
    for game in "${games[@]}"; do
        local game_dir="$games_dir/$game"
        
        if [[ -d "$game_dir" ]]; then
            # Check for index.html
            if [[ -f "$game_dir/index.html" ]]; then
                test_result "Game Build" "PASS" "$game has index.html"
                
                # Validate HTML structure
                validate_html_file "$game_dir/index.html" "$game"
            else
                test_result "Game Build" "FAIL" "$game missing index.html"
            fi
            
            # Check for game assets
            if [[ -f "$game_dir/game.js" ]] || [[ -d "$game_dir/Build" ]]; then
                test_result "Game Assets" "PASS" "$game has game assets"
            else
                test_result "Game Assets" "WARN" "$game missing game assets"
            fi
            
            # Check for CSS
            if [[ -f "$game_dir/game.css" ]] || [[ -f "$game_dir/TemplateData/style.css" ]]; then
                test_result "Game Styling" "PASS" "$game has CSS"
            else
                test_result "Game Styling" "WARN" "$game missing CSS"
            fi
        else
            test_result "Game Build" "FAIL" "$game directory not found"
        fi
    done
}

# Validate HTML file
validate_html_file() {
    local html_file="$1"
    local game_name="$2"
    
    # Check HTML5 doctype
    if head -n 1 "$html_file" | grep -q "<!DOCTYPE html>"; then
        test_result "HTML5 Doctype" "PASS" "$game_name uses HTML5 doctype"
    else
        test_result "HTML5 Doctype" "FAIL" "$game_name missing HTML5 doctype"
    fi
    
    # Check meta viewport
    if grep -q "viewport" "$html_file"; then
        test_result "Mobile Viewport" "PASS" "$game_name has mobile viewport"
    else
        test_result "Mobile Viewport" "FAIL" "$game_name missing mobile viewport"
    fi
    
    # Check title tag
    if grep -q "<title>" "$html_file"; then
        test_result "Page Title" "PASS" "$game_name has title tag"
    else
        test_result "Page Title" "FAIL" "$game_name missing title tag"
    fi
    
    # Check meta description
    if grep -q "meta.*description" "$html_file"; then
        test_result "Meta Description" "PASS" "$game_name has meta description"
    else
        test_result "Meta Description" "WARN" "$game_name missing meta description"
    fi
    
    # Check Canvas element (for HTML5 games)
    if [[ "$game_name" != "circuit-runners" ]]; then
        if grep -q "<canvas" "$html_file"; then
            test_result "Canvas Element" "PASS" "$game_name has canvas element"
        else
            test_result "Canvas Element" "WARN" "$game_name missing canvas element"
        fi
    fi
}

# Validate git repositories
validate_git_repositories() {
    log "TEST" "Validating git repositories..."
    
    local temp_dir="$SCRIPT_DIR/../temp"
    local repos=("unit4productions.github.io" "reflex-rings-game" "quantum-hacker-game" "circuit-runners-game")
    
    for repo in "${repos[@]}"; do
        local repo_dir="$temp_dir/$repo"
        
        if [[ -d "$repo_dir" ]]; then
            cd "$repo_dir"
            
            # Check if it's a git repository
            if [[ -d ".git" ]]; then
                test_result "Git Repository" "PASS" "$repo is a git repository"
                
                # Check for commits
                if git log --oneline -n 1 &> /dev/null; then
                    test_result "Git Commits" "PASS" "$repo has commits"
                else
                    test_result "Git Commits" "WARN" "$repo has no commits"
                fi
                
                # Check for remote
                if git remote get-url origin &> /dev/null; then
                    test_result "Git Remote" "PASS" "$repo has remote origin"
                    
                    # Check remote URL format
                    local remote_url=$(git remote get-url origin)
                    if [[ "$remote_url" =~ github\.com ]]; then
                        test_result "GitHub Remote" "PASS" "$repo uses GitHub"
                    else
                        test_result "GitHub Remote" "WARN" "$repo not using GitHub"
                    fi
                else
                    test_result "Git Remote" "FAIL" "$repo missing remote origin"
                fi
            else
                test_result "Git Repository" "FAIL" "$repo is not a git repository"
            fi
        else
            test_result "Repository Directory" "WARN" "$repo directory not found"
        fi
    done
}

# Validate configuration
validate_configuration() {
    log "TEST" "Validating configuration..."
    
    local config_file="$SCRIPT_DIR/../config/deployment.json"
    
    if [[ -f "$config_file" ]]; then
        test_result "Config File" "PASS" "deployment.json exists"
        
        # Validate JSON syntax
        if command -v node &> /dev/null; then
            if node -e "JSON.parse(require('fs').readFileSync('$config_file', 'utf8'))" &> /dev/null; then
                test_result "JSON Syntax" "PASS" "deployment.json is valid JSON"
            else
                test_result "JSON Syntax" "FAIL" "deployment.json has invalid JSON syntax"
            fi
        elif command -v python3 &> /dev/null; then
            if python3 -c "import json; json.load(open('$config_file'))" &> /dev/null; then
                test_result "JSON Syntax" "PASS" "deployment.json is valid JSON"
            else
                test_result "JSON Syntax" "FAIL" "deployment.json has invalid JSON syntax"
            fi
        else
            test_result "JSON Validation" "WARN" "No JSON validator available"
        fi
        
        # Check required sections
        local required_sections=("deployment" "sites" "templates" "github")
        for section in "${required_sections[@]}"; do
            if grep -q "\"$section\"" "$config_file"; then
                test_result "Config Section" "PASS" "$section section exists"
            else
                test_result "Config Section" "FAIL" "$section section missing"
            fi
        done
    else
        test_result "Config File" "FAIL" "deployment.json not found"
    fi
}

# Validate dependencies
validate_dependencies() {
    log "TEST" "Validating system dependencies..."
    
    local required_tools=("git" "bash" "curl")
    local optional_tools=("node" "python3" "gh")
    
    # Check required tools
    for tool in "${required_tools[@]}"; do
        if command -v "$tool" &> /dev/null; then
            local version=$("$tool" --version 2>/dev/null | head -n 1 || echo "unknown")
            test_result "Required Tool" "PASS" "$tool is available ($version)"
        else
            test_result "Required Tool" "FAIL" "$tool is not installed"
        fi
    done
    
    # Check optional tools
    for tool in "${optional_tools[@]}"; do
        if command -v "$tool" &> /dev/null; then
            local version=$("$tool" --version 2>/dev/null | head -n 1 || echo "unknown")
            test_result "Optional Tool" "PASS" "$tool is available ($version)"
        else
            test_result "Optional Tool" "WARN" "$tool is not installed (reduces functionality)"
        fi
    done
    
    # Check git configuration
    if git config user.name &> /dev/null && git config user.email &> /dev/null; then
        test_result "Git Config" "PASS" "Git user configuration is set"
    else
        test_result "Git Config" "FAIL" "Git user configuration missing"
    fi
    
    # Check GitHub CLI authentication
    if command -v gh &> /dev/null; then
        if gh auth status &> /dev/null; then
            test_result "GitHub Auth" "PASS" "GitHub CLI is authenticated"
        else
            test_result "GitHub Auth" "WARN" "GitHub CLI not authenticated (manual setup required)"
        fi
    fi
}

# Performance validation
validate_performance() {
    log "TEST" "Validating performance optimizations..."
    
    local games_dir="$SCRIPT_DIR/../games"
    local games=("reflex-rings" "quantum-hacker" "circuit-runners")
    
    for game in "${games[@]}"; do
        local game_dir="$games_dir/$game"
        
        if [[ -d "$game_dir" ]]; then
            # Check file sizes
            local index_size=$(stat -c%s "$game_dir/index.html" 2>/dev/null || echo "0")
            if [[ $index_size -lt 100000 ]]; then  # Less than 100KB
                test_result "File Size" "PASS" "$game index.html is optimized ($index_size bytes)"
            else
                test_result "File Size" "WARN" "$game index.html is large ($index_size bytes)"
            fi
            
            # Check for minified CSS
            if [[ -f "$game_dir/game.css" ]]; then
                if grep -q "minified\|compressed" "$game_dir/game.css" || [[ $(wc -l < "$game_dir/game.css") -lt 10 ]]; then
                    test_result "CSS Optimization" "PASS" "$game CSS appears optimized"
                else
                    test_result "CSS Optimization" "WARN" "$game CSS could be minified"
                fi
            fi
            
            # Check for minified JavaScript
            if [[ -f "$game_dir/game.js" ]]; then
                if grep -q "minified\|compressed" "$game_dir/game.js" || [[ $(grep -c "^[[:space:]]*$" "$game_dir/game.js") -lt 5 ]]; then
                    test_result "JS Optimization" "PASS" "$game JavaScript appears optimized"
                else
                    test_result "JS Optimization" "WARN" "$game JavaScript could be minified"
                fi
            fi
        fi
    done
}

# Security validation
validate_security() {
    log "TEST" "Validating security configurations..."
    
    local games_dir="$SCRIPT_DIR/../games"
    local games=("reflex-rings" "quantum-hacker" "circuit-runners")
    
    for game in "${games[@]}"; do
        local game_dir="$games_dir/$game"
        
        if [[ -d "$game_dir" && -f "$game_dir/index.html" ]]; then
            # Check for HTTPS references
            if grep -q "http://" "$game_dir/index.html"; then
                test_result "HTTPS Usage" "WARN" "$game has HTTP references (should use HTTPS)"
            else
                test_result "HTTPS Usage" "PASS" "$game uses HTTPS or relative URLs"
            fi
            
            # Check for inline scripts (security consideration)
            local inline_scripts=$(grep -c "javascript:" "$game_dir/index.html" || echo "0")
            if [[ $inline_scripts -eq 0 ]]; then
                test_result "Inline Scripts" "PASS" "$game avoids inline JavaScript"
            else
                test_result "Inline Scripts" "WARN" "$game has inline JavaScript ($inline_scripts instances)"
            fi
            
            # Check for external script sources
            if grep -q "src.*http" "$game_dir/index.html"; then
                test_result "External Scripts" "WARN" "$game loads external scripts (verify sources)"
            else
                test_result "External Scripts" "PASS" "$game uses local scripts only"
            fi
        fi
    done
}

# Mobile responsiveness validation
validate_mobile_responsiveness() {
    log "TEST" "Validating mobile responsiveness..."
    
    local games_dir="$SCRIPT_DIR/../games"
    local games=("reflex-rings" "quantum-hacker" "circuit-runners")
    
    for game in "${games[@]}"; do
        local game_dir="$games_dir/$game"
        
        if [[ -d "$game_dir" && -f "$game_dir/index.html" ]]; then
            # Check viewport meta tag
            if grep -q "viewport.*width=device-width" "$game_dir/index.html"; then
                test_result "Mobile Viewport" "PASS" "$game has mobile viewport"
            else
                test_result "Mobile Viewport" "FAIL" "$game missing mobile viewport"
            fi
            
            # Check for responsive CSS
            if [[ -f "$game_dir/game.css" ]]; then
                if grep -q "@media" "$game_dir/game.css"; then
                    test_result "Responsive CSS" "PASS" "$game has media queries"
                else
                    test_result "Responsive CSS" "WARN" "$game missing media queries"
                fi
                
                # Check for flexible layouts
                if grep -q "flex\|grid\|%\|vw\|vh" "$game_dir/game.css"; then
                    test_result "Flexible Layout" "PASS" "$game uses flexible layout"
                else
                    test_result "Flexible Layout" "WARN" "$game may not be responsive"
                fi
            fi
        fi
    done
}

# Generate validation report
generate_validation_report() {
    log "INFO" "Generating validation report..."
    
    local success_rate=0
    if [[ $TOTAL_TESTS -gt 0 ]]; then
        success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    fi
    
    # Create JSON report
    cat > "$REPORT_FILE" << EOF
{
    "validation": {
        "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%S.%3NZ)",
        "version": "2.0.0",
        "duration": "$(date -d @$(($(date +%s) - ${START_TIME:-$(date +%s)})) -u +%M:%S)",
        "environment": {
            "os": "$(uname -s)",
            "shell": "$SHELL",
            "git_version": "$(git --version || echo 'not available')"
        }
    },
    "summary": {
        "total_tests": $TOTAL_TESTS,
        "passed_tests": $PASSED_TESTS,
        "failed_tests": $FAILED_TESTS,
        "warning_tests": $WARNING_TESTS,
        "success_rate": ${success_rate}
    },
    "categories": {
        "file_structure": "validated",
        "game_builds": "validated",
        "git_repositories": "validated",
        "configuration": "validated",
        "dependencies": "validated",
        "performance": "validated",
        "security": "validated",
        "mobile_responsiveness": "validated"
    },
    "recommendations": [
        "Review failed tests and address critical issues",
        "Consider addressing warning items for better reliability",
        "Set up automated validation in CI/CD pipeline",
        "Monitor deployment health regularly"
    ],
    "status": "$(if [[ $FAILED_TESTS -eq 0 ]]; then echo 'PASSED'; else echo 'FAILED'; fi)"
}
EOF
    
    # Create readable summary
    local summary_file="$SCRIPT_DIR/../reports/validation-summary.txt"
    cat > "$summary_file" << EOF
🔍 DEPLOYMENT VALIDATION SUMMARY
===============================

Validation Date: $(date '+%Y-%m-%d %H:%M:%S')
System Version: 2.0.0

📊 TEST RESULTS:
   Total Tests:    $TOTAL_TESTS
   ✅ Passed:       $PASSED_TESTS
   ❌ Failed:       $FAILED_TESTS
   ⚠️  Warnings:    $WARNING_TESTS
   
   Success Rate: ${success_rate}%

🎯 VALIDATION CATEGORIES:
   ✓ File Structure
   ✓ Game Builds
   ✓ Git Repositories
   ✓ Configuration
   ✓ Dependencies
   ✓ Performance
   ✓ Security
   ✓ Mobile Responsiveness

$(if [[ $FAILED_TESTS -eq 0 ]]; then
    echo "🎉 VALIDATION PASSED!"
    echo "   All critical tests passed. Deployment is ready."
else
    echo "⚠️  VALIDATION ISSUES FOUND!"
    echo "   $FAILED_TESTS critical issues need attention."
fi)

$(if [[ $WARNING_TESTS -gt 0 ]]; then
    echo "📝 RECOMMENDATIONS:"
    echo "   $WARNING_TESTS warnings should be reviewed for optimization."
fi)

📄 Full Report: validation-report.json
📋 Log File: validation.log
EOF
    
    log "SUCCESS" "Validation report generated"
}

# Show validation summary
show_validation_summary() {
    echo ""
    echo "🔍 VALIDATION COMPLETE!"
    echo "======================"
    echo ""
    
    local success_rate=0
    if [[ $TOTAL_TESTS -gt 0 ]]; then
        success_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    fi
    
    echo -e "${BLUE}📊 Test Results:${NC}"
    echo "   Total Tests: $TOTAL_TESTS"
    echo -e "   ${GREEN}✅ Passed: $PASSED_TESTS${NC}"
    if [[ $FAILED_TESTS -gt 0 ]]; then
        echo -e "   ${RED}❌ Failed: $FAILED_TESTS${NC}"
    fi
    if [[ $WARNING_TESTS -gt 0 ]]; then
        echo -e "   ${YELLOW}⚠️ Warnings: $WARNING_TESTS${NC}"
    fi
    echo "   Success Rate: ${success_rate}%"
    echo ""
    
    if [[ $FAILED_TESTS -eq 0 ]]; then
        echo -e "${GREEN}🎉 VALIDATION PASSED!${NC}"
        echo "   All critical tests passed. Your deployment is ready!"
    else
        echo -e "${RED}⚠️ VALIDATION ISSUES FOUND!${NC}"
        echo "   $FAILED_TESTS critical issues need attention before deployment."
    fi
    
    if [[ $WARNING_TESTS -gt 0 ]]; then
        echo -e "${YELLOW}📝 Recommendations:${NC}"
        echo "   $WARNING_TESTS warnings should be reviewed for optimization."
    fi
    
    echo ""
    echo -e "${CYAN}📄 Full report: $(basename "$REPORT_FILE")${NC}"
    echo -e "${CYAN}📋 Logs: $(basename "$LOG_FILE")${NC}"
    echo ""
}

# Main validation function
main() {
    START_TIME=$(date +%s)
    
    init_validation
    
    validate_dependencies
    echo ""
    
    validate_configuration
    echo ""
    
    validate_file_structure
    echo ""
    
    validate_game_builds
    echo ""
    
    validate_git_repositories
    echo ""
    
    validate_performance
    echo ""
    
    validate_security
    echo ""
    
    validate_mobile_responsiveness
    echo ""
    
    generate_validation_report
    show_validation_summary
    
    # Exit with error code if there are critical failures
    if [[ $FAILED_TESTS -gt 0 ]]; then
        exit 1
    fi
}

# Handle command line arguments
case "${1:-validate}" in
    "validate"|"")
        main
        ;;
    "quick")
        echo "🔍 Quick validation..."
        init_validation
        validate_dependencies
        validate_file_structure
        validate_game_builds
        show_validation_summary
        ;;
    "security")
        echo "🔒 Security validation..."
        init_validation
        validate_security
        show_validation_summary
        ;;
    "performance")
        echo "⚡ Performance validation..."
        init_validation
        validate_performance
        show_validation_summary
        ;;
    "--help"|"-h")
        cat << 'EOF'
Deployment Validation and Testing System

USAGE:
    ./validate-deployment.sh [mode]

MODES:
    validate       Full validation (default)
    quick          Quick essential checks
    security       Security-focused validation
    performance    Performance optimization checks
    --help, -h     Show this help

FEATURES:
    🔍 Comprehensive testing
    📊 Detailed reporting
    ⚡ Performance validation
    🔒 Security checks
    📱 Mobile responsiveness
    🎮 Game-specific validation

VALIDATION CATEGORIES:
    - File structure integrity
    - Game build verification
    - Git repository status
    - Configuration validation
    - Dependency checks
    - Performance optimization
    - Security best practices
    - Mobile responsiveness

EXAMPLES:
    ./validate-deployment.sh           # Full validation
    ./validate-deployment.sh quick     # Essential checks only
    ./validate-deployment.sh security  # Security audit
EOF
        ;;
    *)
        log "ERROR" "Unknown mode: $1"
        log "INFO" "Run with --help to see available options"
        exit 1
        ;;
esac