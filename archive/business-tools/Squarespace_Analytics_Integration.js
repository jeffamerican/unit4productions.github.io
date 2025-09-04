/**
 * UNIT4PRODUCTIONS - SQUARESPACE ANALYTICS & MONETIZATION INTEGRATION
 * Complete tracking and revenue optimization system for Signal Breach
 * 
 * DEPLOYMENT: Add this code to Squarespace Settings > Advanced > Code Injection > Header
 */

// =================== GOOGLE ANALYTICS 4 SETUP ===================

// Replace 'GA_MEASUREMENT_ID' with your actual Google Analytics 4 Measurement ID
const GA4_MEASUREMENT_ID = 'GA_MEASUREMENT_ID'; // Format: G-XXXXXXXXXX

// Initialize Google Analytics 4
(function() {
  // Load Google Analytics script
  const script = document.createElement('script');
  script.async = true;
  script.src = `https://www.googletagmanager.com/gtag/js?id=${GA4_MEASUREMENT_ID}`;
  document.head.appendChild(script);
  
  // Initialize dataLayer and gtag
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  window.gtag = gtag; // Make gtag globally available
  
  gtag('js', new Date());
  gtag('config', GA4_MEASUREMENT_ID, {
    // Enhanced ecommerce tracking
    send_page_view: true,
    allow_google_signals: true,
    allow_ad_personalization_signals: true,
    
    // Custom parameters for gaming site
    custom_map: {
      'custom_game_type': 'game_type',
      'custom_level': 'level',
      'custom_score': 'score',
      'custom_session_length': 'session_length'
    }
  });
})();

// =================== GAME ANALYTICS TRACKER ===================

const Unit4Analytics = {
  // Initialize tracking when page loads
  init: function() {
    this.trackPageView();
    this.setupSessionTracking();
    this.trackUserEngagement();
  },
  
  // Track basic page views with game context
  trackPageView: function() {
    const currentPage = window.location.pathname;
    const gameType = this.detectGameType(currentPage);
    
    gtag('event', 'page_view', {
      'page_title': document.title,
      'page_location': window.location.href,
      'game_type': gameType,
      'user_type': this.getUserType()
    });
  },
  
  // Detect which game is being played
  detectGameType: function(path) {
    if (path.includes('signal-breach')) return 'Signal_Breach';
    if (path.includes('chain-cascade')) return 'Chain_Cascade';
    if (path.includes('dot-conquest')) return 'Dot_Conquest';
    if (path.includes('reflex-runner')) return 'Reflex_Runner';
    if (path.includes('games')) return 'Games_Hub';
    return 'Website';
  },
  
  // Determine if user is premium or free
  getUserType: function() {
    const premiumKey = localStorage.getItem('premium_key');
    const hasSupporter = localStorage.getItem('supporter_status');
    
    if (premiumKey) return 'Premium';
    if (hasSupporter) return 'Supporter';
    return 'Free';
  },
  
  // Session tracking for engagement metrics
  setupSessionTracking: function() {
    const sessionStart = Date.now();
    let lastActivity = sessionStart;
    
    // Update last activity on user interaction
    ['click', 'scroll', 'keypress'].forEach(event => {
      document.addEventListener(event, () => {
        lastActivity = Date.now();
      });
    });
    
    // Track session end when user leaves
    window.addEventListener('beforeunload', () => {
      const sessionLength = Math.round((lastActivity - sessionStart) / 1000);
      this.trackSessionEnd(sessionLength);
    });
    
    // Periodic engagement tracking
    setInterval(() => {
      this.trackEngagementPing();
    }, 30000); // Every 30 seconds
  },
  
  // Track user engagement patterns
  trackUserEngagement: function() {
    // Scroll depth tracking
    let maxScroll = 0;
    window.addEventListener('scroll', () => {
      const scrollPercent = Math.round(
        (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100
      );
      if (scrollPercent > maxScroll) {
        maxScroll = scrollPercent;
        if (maxScroll % 25 === 0) { // Track at 25%, 50%, 75%, 100%
          gtag('event', 'scroll_depth', {
            'event_category': 'Engagement',
            'scroll_depth': maxScroll
          });
        }
      }
    });
    
    // Click heatmap tracking
    document.addEventListener('click', (e) => {
      const element = e.target;
      const elementInfo = {
        tag: element.tagName,
        class: element.className,
        id: element.id,
        text: element.textContent.substring(0, 50)
      };
      
      gtag('event', 'click_tracking', {
        'event_category': 'UI_Interaction',
        'element_info': JSON.stringify(elementInfo),
        'page_location': window.location.pathname
      });
    });
  },
  
  // Track session end
  trackSessionEnd: function(sessionLength) {
    gtag('event', 'session_end', {
      'event_category': 'Engagement',
      'session_length': sessionLength,
      'user_type': this.getUserType(),
      'page_type': this.detectGameType(window.location.pathname)
    });
  },
  
  // Engagement ping for active sessions
  trackEngagementPing: function() {
    gtag('event', 'engagement_ping', {
      'event_category': 'Engagement',
      'user_type': this.getUserType(),
      'timestamp': Date.now()
    });
  },
  
  // =================== GAME-SPECIFIC EVENTS ===================
  
  // Game start tracking
  trackGameStart: function(gameType, level = 1) {
    gtag('event', 'game_start', {
      'event_category': 'Game_Events',
      'game_type': gameType,
      'level': level,
      'user_type': this.getUserType()
    });
  },
  
  // Level completion tracking
  trackLevelComplete: function(gameType, level, score, timeSpent) {
    gtag('event', 'level_complete', {
      'event_category': 'Game_Events',
      'game_type': gameType,
      'level': level,
      'score': score,
      'time_spent': timeSpent,
      'user_type': this.getUserType()
    });
  },
  
  // Game over tracking
  trackGameOver: function(gameType, finalScore, levelsCompleted, totalTime) {
    gtag('event', 'game_over', {
      'event_category': 'Game_Events',
      'game_type': gameType,
      'final_score': finalScore,
      'levels_completed': levelsCompleted,
      'total_time': totalTime,
      'user_type': this.getUserType()
    });
  },
  
  // High score achievements
  trackHighScore: function(gameType, score, previousBest) {
    gtag('event', 'high_score', {
      'event_category': 'Achievements',
      'game_type': gameType,
      'new_score': score,
      'previous_best': previousBest,
      'improvement': score - previousBest
    });
  },
  
  // =================== MONETIZATION TRACKING ===================
  
  // Track premium feature interest
  trackPremiumInterest: function(feature, context) {
    gtag('event', 'premium_interest', {
      'event_category': 'Monetization',
      'feature_interested': feature,
      'context': context, // 'lives_depleted', 'theme_locked', etc.
      'user_type': this.getUserType()
    });
  },
  
  // Track purchase intent (when user clicks buy)
  trackPurchaseIntent: function(product, price) {
    gtag('event', 'begin_checkout', {
      'event_category': 'Ecommerce',
      'currency': 'USD',
      'value': price,
      'items': [{
        'item_id': product,
        'item_name': product,
        'category': 'Game_Premium',
        'price': price,
        'quantity': 1
      }]
    });
  },
  
  // Track successful purchases
  trackPurchase: function(product, price, transactionId) {
    gtag('event', 'purchase', {
      'event_category': 'Ecommerce',
      'transaction_id': transactionId,
      'currency': 'USD',
      'value': price,
      'items': [{
        'item_id': product,
        'item_name': product,
        'category': 'Game_Premium',
        'price': price,
        'quantity': 1
      }]
    });
    
    // Update user type in local storage
    localStorage.setItem('premium_key', 'PREMIUM_' + Date.now());
    localStorage.setItem('purchase_date', new Date().toISOString());
  },
  
  // Track donations (Ko-fi, PayPal)
  trackDonation: function(amount, platform) {
    gtag('event', 'donation', {
      'event_category': 'Support',
      'platform': platform, // 'kofi', 'paypal'
      'amount': amount,
      'currency': 'USD'
    });
  },
  
  // =================== SOCIAL SHARING TRACKING ===================
  
  // Track social shares
  trackSocialShare: function(platform, content) {
    gtag('event', 'share', {
      'event_category': 'Social',
      'method': platform, // 'twitter', 'facebook', 'native'
      'content_type': content, // 'high_score', 'achievement', 'game'
      'user_type': this.getUserType()
    });
  },
  
  // Track referrals
  trackReferral: function(referralCode) {
    gtag('event', 'referral_click', {
      'event_category': 'Marketing',
      'referral_code': referralCode
    });
    
    localStorage.setItem('referral_source', referralCode);
  },
  
  // =================== ERROR TRACKING ===================
  
  // Track JavaScript errors
  trackError: function(error, context) {
    gtag('event', 'exception', {
      'description': error.message,
      'fatal': false,
      'context': context
    });
  },
  
  // =================== A/B TESTING FRAMEWORK ===================
  
  // Initialize A/B test variant
  initializeABTest: function(testName, variants) {
    let userVariant = localStorage.getItem(`ab_test_${testName}`);
    
    if (!userVariant) {
      userVariant = variants[Math.floor(Math.random() * variants.length)];
      localStorage.setItem(`ab_test_${testName}`, userVariant);
    }
    
    gtag('event', 'ab_test_assignment', {
      'event_category': 'Experiment',
      'test_name': testName,
      'variant': userVariant
    });
    
    return userVariant;
  },
  
  // Track A/B test conversions
  trackABConversion: function(testName, variant, conversionType) {
    gtag('event', 'ab_test_conversion', {
      'event_category': 'Experiment',
      'test_name': testName,
      'variant': variant,
      'conversion_type': conversionType
    });
  }
};

// =================== GOOGLE ADSENSE INTEGRATION ===================

const AdSenseManager = {
  // Initialize AdSense (add your publisher ID)
  publisherId: 'ca-pub-XXXXXXXXXXXXXXXXX', // Replace with your AdSense publisher ID
  
  init: function() {
    if (this.publisherId === 'ca-pub-XXXXXXXXXXXXXXXXX') {
      console.log('AdSense: Publisher ID not configured');
      return;
    }
    
    // Load AdSense script
    const script = document.createElement('script');
    script.async = true;
    script.src = `https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${this.publisherId}`;
    script.crossOrigin = 'anonymous';
    document.head.appendChild(script);
    
    // Initialize auto ads
    (window.adsbygoogle = window.adsbygoogle || []).push({});
  },
  
  // Show ads only for free users
  showAdsForFreeUsers: function() {
    const userType = Unit4Analytics.getUserType();
    if (userType === 'Premium') {
      this.hideAllAds();
    }
  },
  
  // Hide ads for premium users
  hideAllAds: function() {
    const ads = document.querySelectorAll('.adsbygoogle');
    ads.forEach(ad => {
      ad.style.display = 'none';
    });
  }
};

// =================== REVENUE DASHBOARD ===================

const RevenueDashboard = {
  // Update dashboard with current metrics
  updateDashboard: function() {
    // This would integrate with your backend API
    // For now, we'll use local storage data
    
    const stats = {
      dailyVisitors: this.getDailyVisitors(),
      premiumUsers: this.getPremiumUsers(),
      estimatedRevenue: this.getEstimatedRevenue(),
      conversionRate: this.getConversionRate()
    };
    
    // Send to analytics dashboard
    gtag('event', 'dashboard_update', {
      'event_category': 'Analytics',
      'daily_visitors': stats.dailyVisitors,
      'premium_users': stats.premiumUsers,
      'estimated_revenue': stats.estimatedRevenue,
      'conversion_rate': stats.conversionRate
    });
    
    return stats;
  },
  
  getDailyVisitors: function() {
    // Implement logic to count daily visitors
    const today = new Date().toDateString();
    const visitors = JSON.parse(localStorage.getItem('daily_visitors') || '{}');
    return visitors[today] || 0;
  },
  
  getPremiumUsers: function() {
    // Count premium users (this would be server-side in production)
    return localStorage.getItem('premium_key') ? 1 : 0;
  },
  
  getEstimatedRevenue: function() {
    // Calculate estimated revenue
    const premiumUsers = this.getPremiumUsers();
    const donations = JSON.parse(localStorage.getItem('total_donations') || '0');
    return (premiumUsers * 4.99) + donations;
  },
  
  getConversionRate: function() {
    // Calculate conversion rate
    const totalVisitors = this.getDailyVisitors();
    const premiumUsers = this.getPremiumUsers();
    return totalVisitors > 0 ? (premiumUsers / totalVisitors * 100).toFixed(2) : 0;
  }
};

// =================== INITIALIZATION ===================

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
  // Initialize analytics
  Unit4Analytics.init();
  
  // Initialize AdSense
  AdSenseManager.init();
  AdSenseManager.showAdsForFreeUsers();
  
  // Set up error tracking
  window.addEventListener('error', function(e) {
    Unit4Analytics.trackError(e.error, 'global_error_handler');
  });
  
  // Initialize A/B tests
  const pricingVariant = Unit4Analytics.initializeABTest('pricing_v1', ['2.99', '4.99', '7.99']);
  const themeVariant = Unit4Analytics.initializeABTest('themes_v1', ['10_themes', '15_themes', '20_themes']);
  
  // Update revenue dashboard every 5 minutes
  setInterval(() => {
    RevenueDashboard.updateDashboard();
  }, 5 * 60 * 1000);
  
  console.log('Unit4Productions Analytics & Monetization System Initialized');
  console.log('Pricing Variant:', pricingVariant);
  console.log('Theme Variant:', themeVariant);
});

// =================== UTILITY FUNCTIONS ===================

// Make tracking functions globally available
window.Unit4Track = {
  gameStart: (game, level) => Unit4Analytics.trackGameStart(game, level),
  levelComplete: (game, level, score, time) => Unit4Analytics.trackLevelComplete(game, level, score, time),
  gameOver: (game, score, levels, time) => Unit4Analytics.trackGameOver(game, score, levels, time),
  premiumInterest: (feature, context) => Unit4Analytics.trackPremiumInterest(feature, context),
  purchase: (product, price, id) => Unit4Analytics.trackPurchase(product, price, id),
  share: (platform, content) => Unit4Analytics.trackSocialShare(platform, content),
  donation: (amount, platform) => Unit4Analytics.trackDonation(amount, platform)
};

// Export for use in games
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { Unit4Analytics, AdSenseManager, RevenueDashboard };
}