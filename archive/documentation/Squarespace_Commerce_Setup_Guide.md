# SQUARESPACE COMMERCE SETUP FOR GAMING MONETIZATION
## Complete Configuration Guide for Unit4Productions

### OVERVIEW
This guide configures Squarespace Commerce to handle premium game purchases, digital product delivery, and subscription management for Signal Breach and future games.

---

## PHASE 1: SQUARESPACE COMMERCE ACTIVATION

### 1.1 UPGRADE TO COMMERCE PLAN
1. **Navigate to:** Settings > Billing & Account
2. **Upgrade to:** Commerce Plan ($18/month)
3. **Benefits:**
   - 3% transaction fee + 30¢ per transaction
   - Unlimited products and storage
   - Professional checkout experience
   - Inventory management
   - Customer accounts
   - Email marketing integration

### 1.2 PAYMENT PROCESSOR SETUP
1. **Go to:** Commerce > Payments
2. **Enable Stripe Integration:**
   - Stripe: 2.9% + 30¢ per transaction
   - PayPal: Available as secondary option
   - Apple Pay & Google Pay: Automatic activation

3. **Configure Payment Methods:**
   ```
   Primary: Stripe (Credit/Debit Cards)
   Secondary: PayPal
   Mobile: Apple Pay, Google Pay
   ```

---

## PHASE 2: PRODUCT CATALOG SETUP

### 2.1 PREMIUM GAME PRODUCTS

#### Product 1: Signal Breach Premium
```
Product Name: Signal Breach Premium
SKU: SB_PREMIUM_2024
Price: $4.99
Type: Digital Product
Category: Game Premium Features

Description:
Transform your Signal Breach experience with premium features:
• Unlimited Lives - Never wait to play again
• 10 Exclusive Levels - Unique challenges and mechanics
• 15 Custom Themes - Personalize your gaming experience
• Ad-Free Gameplay - Pure, uninterrupted gaming
• Priority Support - Direct access to our development team
• Advanced Statistics - Track your progress and achievements

Digital Delivery: Automatic via email
Customer Support: premium@unit4productions.com
```

#### Product 2: Gaming Supporter Pack
```
Product Name: Gaming Supporter Pack  
SKU: GAMING_SUPPORTER_2024
Price: $9.99
Type: Digital Subscription (Annual)
Category: Supporter Membership

Description:
Support indie game development and get exclusive benefits:
• All Signal Breach Premium Features
• Early Access to New Games (24-48 hours)
• Exclusive Developer Discord Channel Access
• Monthly Developer Updates and Behind-the-Scenes
• Vote on New Game Features and Direction
• Special Supporter Badge Across All Games
• 20% Discount on Future Premium Purchases

Digital Delivery: Account activation + Discord invite
Billing Cycle: Annual subscription
```

#### Product 3: Ultimate Gamer Package
```
Product Name: Ultimate Gamer Package
SKU: ULTIMATE_GAMER_2024  
Price: $19.99
Type: Digital Product + Services
Category: VIP Membership

Description:
The complete Unit4Productions gaming experience:
• All Previous Benefits Included
• Monthly Exclusive Game Release (Yours to Keep)
• 1-on-1 Developer Feedback Sessions (Quarterly)
• Beta Testing Privileges for All New Games
• Revenue Sharing on Community-Created Content
• Lifetime 20% Discount on All Future Products
• Personal Thanks in Game Credits
• Direct Input on Development Roadmap

Digital Delivery: VIP account setup + calendar scheduling
Customer Support: VIP priority queue
```

### 2.2 DONATION PRODUCTS

#### Coffee Fund
```
Product Name: Developer Coffee Fund
SKU: COFFEE_FUND
Price: $3.00
Type: Digital Donation
Description: Keep the developers caffeinated! ☕
```

#### Pizza Fund  
```
Product Name: Pizza Development Fund
SKU: PIZZA_FUND
Price: $10.00
Type: Digital Donation
Description: Fuel late-night coding sessions! 🍕
```

---

## PHASE 3: DIGITAL DELIVERY SYSTEM

### 3.1 AUTOMATED DELIVERY SETUP

#### Email Templates Configuration:
1. **Go to:** Commerce > Customer Notifications
2. **Configure Order Confirmation Email:**

```html
Subject: 🎮 Your Unit4Productions Premium Access is Ready!

Dear {customerName},

Welcome to the Unit4Productions Premium Gaming Experience! 🚀

ORDER DETAILS:
Order #: {orderNumber}
Product: {productName}  
Amount: ${orderTotal}
Date: {orderDate}

YOUR PREMIUM ACCESS:
Premium Key: {customField:premiumKey}
Activation Link: https://unit4productions.com/activate?key={customField:premiumKey}

NEXT STEPS:
1. Click the activation link above
2. Your premium features will be instantly unlocked
3. Refresh your game page to see new content
4. Join our Discord community: https://discord.gg/unit4productions

WHAT'S INCLUDED:
✓ Unlimited Lives - Never wait to play
✓ 10 Exclusive Levels - Unique challenges  
✓ 15 Custom Themes - Personalize your experience
✓ Ad-Free Gaming - Pure gameplay
✓ Priority Support - Direct developer contact
✓ Advanced Statistics - Track your progress

Need help? Reply to this email or contact premium@unit4productions.com

Thank you for supporting indie game development!

The Unit4Productions Team
https://unit4productions.com
```

### 3.2 PREMIUM KEY GENERATION

#### Automated Key Generation Script:
```javascript
// Add to Squarespace Code Injection (Footer)
// Webhook handler for new orders
function handleNewOrder(orderData) {
    // Generate unique premium key
    const premiumKey = generatePremiumKey(orderData.id, orderData.customerEmail);
    
    // Store in database/local storage
    storePremiumKey(premiumKey, orderData);
    
    // Send to customer via email template variable
    updateCustomerRecord(orderData.customerEmail, {
        premiumKey: premiumKey,
        purchaseDate: new Date().toISOString(),
        product: orderData.items[0].sku
    });
}

function generatePremiumKey(orderId, email) {
    const timestamp = Date.now();
    const hash = btoa(orderId + email + timestamp).replace(/[^a-zA-Z0-9]/g, '').substring(0, 16);
    return `PREMIUM_${hash}_${timestamp.toString(36).toUpperCase()}`;
}

function storePremiumKey(key, orderData) {
    // In production, this would go to a secure database
    // For MVP, we'll use a simple validation system
    const keyData = {
        key: key,
        email: orderData.customerEmail,
        product: orderData.items[0].sku,
        purchaseDate: new Date().toISOString(),
        active: true
    };
    
    // Store in secure key registry
    fetch('/api/premium-keys', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(keyData)
    });
}
```

---

## PHASE 4: CUSTOMER ACCOUNT MANAGEMENT

### 4.1 CUSTOMER ACCOUNTS SETUP
1. **Enable Customer Accounts:** Commerce > Customer Accounts
2. **Configure Account Pages:**
   - Account Dashboard
   - Order History  
   - Premium Status Display
   - Download Center

### 4.2 PREMIUM STATUS INTEGRATION
```html
<!-- Customer Account Premium Status Widget -->
<div id="premium-status-widget">
    <div class="premium-status-header">
        <h3>🎮 Gaming Status</h3>
    </div>
    <div class="premium-status-content">
        <div class="status-badge premium">
            👑 Premium Member
        </div>
        <div class="premium-details">
            <div class="detail-item">
                <label>Premium Since:</label>
                <span id="premium-since">March 15, 2024</span>
            </div>
            <div class="detail-item">
                <label>Premium Key:</label>
                <span id="premium-key">PREMIUM_ABC123_XYZ</span>
                <button onclick="copyPremiumKey()">Copy</button>
            </div>
            <div class="detail-item">
                <label>Games Unlocked:</label>
                <span id="games-unlocked">Signal Breach Premium</span>
            </div>
        </div>
        <div class="premium-actions">
            <a href="/games/signal-breach?premium=true" class="play-premium-btn">
                🎮 Play Premium Games
            </a>
            <a href="/discord" class="discord-btn">
                💬 Join Discord Community
            </a>
        </div>
    </div>
</div>
```

---

## PHASE 5: INVENTORY & ORDER MANAGEMENT

### 5.1 DIGITAL INVENTORY SETUP
1. **Product Inventory Settings:**
   - Track Quantity: Off (Digital products)
   - Allow Backorders: N/A
   - SKU Management: On
   - Variant Management: Off

### 5.2 ORDER FULFILLMENT WORKFLOW
```
New Order Received
    ↓
Auto-Generate Premium Key
    ↓
Send Confirmation Email with Key
    ↓
Update Customer Account Status
    ↓
Track Analytics Event
    ↓
Add to Premium User Database
    ↓
Send Welcome to Discord (if applicable)
```

---

## PHASE 6: ANALYTICS INTEGRATION

### 6.1 E-COMMERCE TRACKING
```javascript
// Enhanced Squarespace Commerce Analytics
function trackEcommerceEvent(eventName, orderData) {
    // Google Analytics 4 Enhanced Ecommerce
    gtag('event', eventName, {
        'transaction_id': orderData.id,
        'value': orderData.grandTotal,
        'currency': 'USD',
        'items': orderData.items.map(item => ({
            'item_id': item.sku,
            'item_name': item.productName,
            'category': 'Gaming_Premium',
            'quantity': item.quantity,
            'price': item.basePrice
        }))
    });
    
    // Custom Unit4Productions tracking
    if (typeof Unit4Track !== 'undefined') {
        Unit4Track.purchase(orderData.items[0].sku, orderData.grandTotal, orderData.id);
    }
}

// Track key commerce events
function setupCommerceTracking() {
    // Order completion
    window.addEventListener('squarespace:order-complete', function(event) {
        trackEcommerceEvent('purchase', event.detail);
        
        // Set customer as premium in local analytics
        localStorage.setItem('customer_status', 'premium');
        localStorage.setItem('purchase_value', event.detail.grandTotal);
    });
    
    // Cart events
    window.addEventListener('squarespace:cart-add', function(event) {
        gtag('event', 'add_to_cart', {
            'currency': 'USD',
            'value': event.detail.item.basePrice,
            'items': [{
                'item_id': event.detail.item.sku,
                'item_name': event.detail.item.productName,
                'category': 'Gaming_Premium',
                'quantity': 1,
                'price': event.detail.item.basePrice
            }]
        });
    });
}
```

### 6.2 REVENUE DASHBOARD INTEGRATION
```javascript
// Revenue tracking and dashboard updates
function updateRevenueDashboard() {
    // Fetch commerce data from Squarespace
    fetch('/api/commerce/orders', {
        method: 'GET',
        headers: { 'Authorization': 'Bearer YOUR_API_KEY' }
    })
    .then(response => response.json())
    .then(data => {
        const revenueStats = calculateRevenueStats(data.orders);
        
        // Update dashboard display
        document.getElementById('daily-revenue').textContent = `$${revenueStats.daily}`;
        document.getElementById('monthly-revenue').textContent = `$${revenueStats.monthly}`;
        document.getElementById('premium-users').textContent = revenueStats.premiumUsers;
        document.getElementById('conversion-rate').textContent = `${revenueStats.conversionRate}%`;
        
        // Track in analytics
        gtag('event', 'dashboard_update', {
            'daily_revenue': revenueStats.daily,
            'monthly_revenue': revenueStats.monthly,
            'premium_users': revenueStats.premiumUsers
        });
    });
}

function calculateRevenueStats(orders) {
    const today = new Date();
    const thisMonth = today.getMonth();
    const thisYear = today.getFullYear();
    
    let dailyRevenue = 0;
    let monthlyRevenue = 0;
    let premiumUsers = 0;
    
    orders.forEach(order => {
        const orderDate = new Date(order.createdAt);
        
        // Daily revenue
        if (orderDate.toDateString() === today.toDateString()) {
            dailyRevenue += order.grandTotal;
        }
        
        // Monthly revenue
        if (orderDate.getMonth() === thisMonth && orderDate.getFullYear() === thisYear) {
            monthlyRevenue += order.grandTotal;
            premiumUsers++;
        }
    });
    
    return {
        daily: dailyRevenue.toFixed(2),
        monthly: monthlyRevenue.toFixed(2),
        premiumUsers: premiumUsers,
        conversionRate: ((premiumUsers / (orders.length || 1)) * 100).toFixed(2)
    };
}
```

---

## PHASE 7: CUSTOMER SUPPORT INTEGRATION

### 7.1 PREMIUM SUPPORT SETUP
1. **Create Support Email:** premium@unit4productions.com
2. **Set Up Auto-Responders:**

```
Subject: Premium Support - We've Got You Covered! 🎮

Hi there!

Thanks for reaching out to Unit4Productions Premium Support.

As a Premium member, you get priority support with:
✓ 24-hour response guarantee
✓ Direct developer access
✓ Advanced troubleshooting
✓ Feature request priority

We'll get back to you within 24 hours (usually much sooner!).

In the meantime, check out our Premium FAQ: https://unit4productions.com/premium-faq

Best regards,
Unit4Productions Support Team
```

### 7.2 DISCORD INTEGRATION
1. **Set Up Discord Server** with channels:
   - #premium-members
   - #developer-chat  
   - #beta-testing
   - #feature-requests

2. **Automated Discord Invites:**
```javascript
function sendDiscordInvite(customerEmail, premiumTier) {
    const inviteData = {
        email: customerEmail,
        tier: premiumTier,
        roles: getTierRoles(premiumTier)
    };
    
    // Send to Discord bot API
    fetch('/api/discord/invite', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(inviteData)
    });
}

function getTierRoles(tier) {
    const roleMap = {
        'SB_PREMIUM_2024': ['Premium Member'],
        'GAMING_SUPPORTER_2024': ['Premium Member', 'Supporter'],
        'ULTIMATE_GAMER_2024': ['Premium Member', 'Supporter', 'VIP']
    };
    return roleMap[tier] || ['Member'];
}
```

---

## PHASE 8: TESTING & VALIDATION

### 8.1 TEST PURCHASE FLOW
1. **Test Environment Setup:**
   - Use Stripe test mode
   - Create test products
   - Verify email delivery
   - Test key generation

2. **Purchase Flow Validation:**
   ```
   1. Add premium product to cart ✓
   2. Complete checkout process ✓
   3. Receive confirmation email ✓
   4. Premium key generated ✓
   5. Key activates premium features ✓
   6. Analytics event tracked ✓
   7. Customer account updated ✓
   ```

### 8.2 PREMIUM FEATURE VALIDATION
```javascript
// Test premium activation
function testPremiumActivation(testKey) {
    localStorage.setItem('premium_key', testKey);
    localStorage.setItem('purchase_date', new Date().toISOString());
    
    // Reload game to test premium features
    location.reload();
    
    console.log('Testing Premium Features:');
    console.log('- Unlimited Lives:', signalBreachPremium.lives === -1);
    console.log('- Premium Themes Available:', signalBreachPremium.themes.filter(t => !t.free).length);
    console.log('- Ad-Free Mode:', document.querySelectorAll('.adsbygoogle').length === 0);
}
```

---

## IMPLEMENTATION CHECKLIST

### Week 1: Basic Setup
- [ ] Upgrade to Squarespace Commerce
- [ ] Configure Stripe payment processing  
- [ ] Create premium product listings
- [ ] Set up email templates
- [ ] Test purchase flow

### Week 2: Advanced Features
- [ ] Implement premium key generation
- [ ] Set up customer accounts
- [ ] Configure Discord integration
- [ ] Create support workflows
- [ ] Integrate analytics tracking

### Week 3: Testing & Launch  
- [ ] Complete end-to-end testing
- [ ] Launch premium features
- [ ] Monitor conversion rates
- [ ] Optimize based on user feedback
- [ ] Scale successful elements

### Success Metrics:
- **Target Conversion Rate:** 3-5% (Free → Premium)
- **Average Order Value:** $7.50+ 
- **Customer Support Response Time:** <24 hours
- **Premium Feature Satisfaction:** 4.5+ stars
- **Monthly Recurring Revenue:** $500+ by Month 2

This Squarespace Commerce configuration creates a professional, scalable monetization system that can grow with Unit4Productions while maintaining excellent user experience and automated operations.