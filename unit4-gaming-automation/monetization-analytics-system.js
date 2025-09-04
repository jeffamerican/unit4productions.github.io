/**
 * Monetization and Analytics System for Unit4Productions
 * Handles Google Analytics, AdSense, payments, and revenue optimization
 */

class MonetizationAnalyticsSystem {
    constructor(config = {}) {
        this.config = {
            googleAnalytics: {
                measurementId: config.analyticsId || '',
                apiSecret: config.analyticsApiSecret || ''
            },
            googleAdSense: {
                publisherId: config.adsenseId || '',
                adUnits: config.adUnits || []
            },
            stripe: {
                publishableKey: config.stripePublishableKey || '',
                webhookSecret: config.stripeWebhookSecret || ''
            },
            paypal: {
                clientId: config.paypalClientId || ''
            },
            monetization: {
                premiumGamePrice: config.premiumPrice || 4.99,
                subscriptionPrice: config.subscriptionPrice || 9.99,
                adFreeTier: config.adFreeTier || 2.99
            }
        };
    }

    /**
     * Generate comprehensive analytics tracking code
     */
    generateAnalyticsCode() {
        if (!this.config.googleAnalytics.measurementId) {
            return '';
        }

        return `
<!-- Google Analytics 4 -->
<script async src="https://www.googletagmanager.com/gtag/js?id=${this.config.googleAnalytics.measurementId}"></script>
<script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    
    gtag('config', '${this.config.googleAnalytics.measurementId}', {
        page_title: document.title,
        page_location: window.location.href,
        custom_map: {
            'custom_parameter_1': 'game_type',
            'custom_parameter_2': 'game_difficulty'
        }
    });

    // Enhanced E-commerce tracking
    gtag('config', '${this.config.googleAnalytics.measurementId}', {
        debug_mode: ${process.env.NODE_ENV === 'development'},
        send_page_view: true
    });

    // Custom events for gaming
    window.trackGameEvent = function(eventName, parameters = {}) {
        gtag('event', eventName, {
            event_category: 'game',
            event_label: parameters.game_name || 'unknown',
            value: parameters.value || 1,
            game_type: parameters.game_type,
            game_difficulty: parameters.difficulty,
            user_level: parameters.user_level,
            session_duration: parameters.session_duration,
            custom_parameter_1: parameters.game_type,
            custom_parameter_2: parameters.difficulty
        });
    };

    // Track game-specific metrics
    window.trackGameStart = function(gameName, gameType) {
        trackGameEvent('game_start', {
            game_name: gameName,
            game_type: gameType,
            timestamp: Date.now()
        });
    };

    window.trackGameComplete = function(gameName, duration, score) {
        trackGameEvent('game_complete', {
            game_name: gameName,
            session_duration: duration,
            value: score,
            timestamp: Date.now()
        });
    };

    window.trackGameFailure = function(gameName, level, reason) {
        trackGameEvent('game_failure', {
            game_name: gameName,
            user_level: level,
            failure_reason: reason,
            timestamp: Date.now()
        });
    };

    // Track monetization events
    window.trackPurchase = function(itemName, price, currency = 'USD') {
        gtag('event', 'purchase', {
            transaction_id: 'txn_' + Date.now(),
            value: price,
            currency: currency,
            items: [{
                item_id: itemName.toLowerCase().replace(/\\s+/g, '_'),
                item_name: itemName,
                category: 'game',
                quantity: 1,
                price: price
            }]
        });
    };

    // Track ad interactions
    window.trackAdClick = function(adUnit, position) {
        trackGameEvent('ad_click', {
            ad_unit: adUnit,
            ad_position: position,
            value: 0.01 // Estimated value per ad click
        });
    };

    // Track user engagement
    window.trackUserEngagement = function(action, element) {
        gtag('event', action, {
            event_category: 'engagement',
            event_label: element,
            engagement_time_msec: Date.now()
        });
    };
</script>

<!-- Facebook Pixel (optional) -->
<script>
!function(f,b,e,v,n,t,s)
{if(f.fbq)return;n=f.fbq=function(){n.callMethod?
n.callMethod.apply(n,arguments):n.queue.push(arguments)};
if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
n.queue=[];t=b.createElement(e);t.async=!0;
t.src=v;s=b.getElementsByTagName(e)[0];
s.parentNode.insertBefore(t,s)}(window, document,'script',
'https://connect.facebook.net/en_US/fbevents.js');

fbq('init', 'YOUR_FACEBOOK_PIXEL_ID');
fbq('track', 'PageView');
</script>
`;
    }

    /**
     * Generate AdSense integration code
     */
    generateAdSenseCode() {
        if (!this.config.googleAdSense.publisherId) {
            return '';
        }

        return `
<!-- Google AdSense -->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${this.config.googleAdSense.publisherId}" crossorigin="anonymous"></script>

<script>
    // AdSense configuration
    window.adsbygoogle = window.adsbygoogle || [];
    
    // Auto ads (optional)
    (adsbygoogle = window.adsbygoogle || []).push({
        google_ad_client: "${this.config.googleAdSense.publisherId}",
        enable_page_level_ads: true
    });

    // Ad loading helper
    window.loadAd = function(containerId, adSlot) {
        try {
            (adsbygoogle = window.adsbygoogle || []).push({});
        } catch (e) {
            console.warn('Ad loading failed:', e);
        }
    };

    // Track ad performance
    window.trackAdPerformance = function(adUnit, revenue) {
        if (typeof gtag !== 'undefined') {
            gtag('event', 'ad_impression', {
                event_category: 'monetization',
                event_label: adUnit,
                value: revenue || 0
            });
        }
    };
</script>`;
    }

    /**
     * Generate payment integration code (Stripe + PayPal)
     */
    generatePaymentIntegration() {
        return `
<!-- Payment Integration Scripts -->
<script src="https://js.stripe.com/v3/"></script>
<script src="https://www.paypal.com/sdk/js?client-id=${this.config.paypal.clientId}&currency=USD"></script>

<script>
    // Stripe Configuration
    const stripe = Stripe('${this.config.stripe.publishableKey}');
    
    // Payment processing
    window.ProcessPayment = {
        // Stripe payment
        processStripePayment: async function(amount, productName, successCallback, errorCallback) {
            try {
                // Create payment intent on your backend
                const response = await fetch('/api/create-payment-intent', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        amount: amount * 100, // Convert to cents
                        product: productName,
                        currency: 'usd'
                    })
                });

                const { clientSecret } = await response.json();

                // Confirm payment
                const result = await stripe.confirmCardPayment(clientSecret);

                if (result.error) {
                    errorCallback(result.error);
                } else {
                    // Track purchase
                    trackPurchase(productName, amount);
                    successCallback(result.paymentIntent);
                }
            } catch (error) {
                errorCallback(error);
            }
        },

        // PayPal payment
        initPayPal: function(containerId, amount, productName, successCallback, errorCallback) {
            paypal.Buttons({
                createOrder: function(data, actions) {
                    return actions.order.create({
                        purchase_units: [{
                            amount: {
                                value: amount.toString(),
                                currency_code: 'USD'
                            },
                            description: productName
                        }]
                    });
                },
                onApprove: function(data, actions) {
                    return actions.order.capture().then(function(details) {
                        // Track purchase
                        trackPurchase(productName, amount);
                        successCallback(details);
                    });
                },
                onError: function(err) {
                    errorCallback(err);
                }
            }).render('#' + containerId);
        },

        // Subscription management
        createSubscription: function(planId, successCallback, errorCallback) {
            // Implementation for subscription creation
            paypal.Buttons({
                createSubscription: function(data, actions) {
                    return actions.subscription.create({
                        'plan_id': planId
                    });
                },
                onApprove: function(data, actions) {
                    trackGameEvent('subscription_start', {
                        subscription_id: data.subscriptionID,
                        value: ${this.config.monetization.subscriptionPrice}
                    });
                    successCallback(data);
                },
                onError: function(err) {
                    errorCallback(err);
                }
            }).render('#subscription-paypal-button');
        }
    };

    // Premium game unlock
    window.unlockPremiumGame = function(gameSlug, gameTitle) {
        const modal = document.createElement('div');
        modal.className = 'payment-modal';
        modal.innerHTML = \`
            <div class="payment-modal-content">
                <h3>Unlock Premium Game</h3>
                <p>Get unlimited access to <strong>\${gameTitle}</strong></p>
                <div class="pricing">
                    <span class="price">$${this.config.monetization.premiumGamePrice}</span>
                    <span class="description">One-time purchase</span>
                </div>
                
                <div class="payment-methods">
                    <button id="stripe-pay-btn" class="btn btn-primary">
                        <i class="fas fa-credit-card"></i>
                        Pay with Card
                    </button>
                    <div id="paypal-button-container"></div>
                </div>
                
                <button class="modal-close" onclick="closePaymentModal()">×</button>
            </div>
        \`;
        
        document.body.appendChild(modal);

        // Initialize PayPal
        ProcessPayment.initPayPal(
            'paypal-button-container',
            ${this.config.monetization.premiumGamePrice},
            gameTitle,
            function(details) {
                // Success
                localStorage.setItem('premium_' + gameSlug, 'true');
                location.reload();
            },
            function(error) {
                alert('Payment failed. Please try again.');
            }
        );

        // Stripe payment button
        document.getElementById('stripe-pay-btn').onclick = function() {
            ProcessPayment.processStripePayment(
                ${this.config.monetization.premiumGamePrice},
                gameTitle,
                function(paymentIntent) {
                    localStorage.setItem('premium_' + gameSlug, 'true');
                    location.reload();
                },
                function(error) {
                    alert('Payment failed. Please try again.');
                }
            );
        };
    };

    // Check premium status
    window.isPremiumUnlocked = function(gameSlug) {
        return localStorage.getItem('premium_' + gameSlug) === 'true' ||
               localStorage.getItem('subscription_active') === 'true';
    };

    // Close payment modal
    window.closePaymentModal = function() {
        const modal = document.querySelector('.payment-modal');
        if (modal) modal.remove();
    };
</script>`;
    }

    /**
     * Generate ad placement HTML
     */
    generateAdPlacements() {
        const placements = [
            {
                id: 'header-banner',
                type: 'display',
                size: '728x90',
                position: 'header'
            },
            {
                id: 'sidebar-rectangle',
                type: 'display',
                size: '300x250',
                position: 'sidebar'
            },
            {
                id: 'footer-banner',
                type: 'display',
                size: '728x90',
                position: 'footer'
            },
            {
                id: 'mobile-banner',
                type: 'display',
                size: '320x50',
                position: 'mobile'
            },
            {
                id: 'interstitial',
                type: 'interstitial',
                size: 'responsive',
                position: 'between-games'
            }
        ];

        return placements.map(ad => `
<!-- Ad Placement: ${ad.id} -->
<div class="ad-container ad-${ad.position}" id="${ad.id}">
    <ins class="adsbygoogle"
         style="display:inline-block;width:${ad.size === 'responsive' ? '100%' : ad.size.split('x')[0] + 'px'};height:${ad.size === 'responsive' ? 'auto' : ad.size.split('x')[1] + 'px'}"
         data-ad-client="${this.config.googleAdSense.publisherId}"
         data-ad-slot="YOUR_AD_SLOT_ID_${ad.id.toUpperCase()}"
         ${ad.size === 'responsive' ? 'data-ad-format="auto" data-full-width-responsive="true"' : ''}></ins>
    <script>
         (adsbygoogle = window.adsbygoogle || []).push({});
         trackAdPerformance('${ad.id}', 0.05); // Estimated revenue per impression
    </script>
</div>`).join('\n');
    }

    /**
     * Generate subscription management interface
     */
    generateSubscriptionInterface() {
        return `
<!-- Subscription Interface -->
<div class="subscription-section" id="subscription-section" style="display: none;">
    <div class="subscription-card">
        <h3>Premium Gaming Subscription</h3>
        <div class="benefits">
            <ul>
                <li><i class="fas fa-check"></i> Ad-free gaming experience</li>
                <li><i class="fas fa-check"></i> Access to all premium games</li>
                <li><i class="fas fa-check"></i> Early access to new releases</li>
                <li><i class="fas fa-check"></i> Exclusive content and features</li>
                <li><i class="fas fa-check"></i> Cloud save synchronization</li>
            </ul>
        </div>
        
        <div class="pricing-table">
            <div class="plan">
                <h4>Monthly</h4>
                <div class="price">$${this.config.monetization.subscriptionPrice}<span>/month</span></div>
                <div id="subscription-paypal-button"></div>
            </div>
        </div>
    </div>
</div>

<script>
    // Initialize subscription interface
    document.addEventListener('DOMContentLoaded', function() {
        // Check if user is already subscribed
        const isSubscribed = localStorage.getItem('subscription_active') === 'true';
        
        if (!isSubscribed) {
            document.getElementById('subscription-section').style.display = 'block';
            
            // Initialize subscription PayPal button
            ProcessPayment.createSubscription(
                'YOUR_PAYPAL_PLAN_ID',
                function(data) {
                    localStorage.setItem('subscription_active', 'true');
                    localStorage.setItem('subscription_id', data.subscriptionID);
                    document.getElementById('subscription-section').style.display = 'none';
                    location.reload();
                },
                function(error) {
                    console.error('Subscription failed:', error);
                }
            );
        }
    });

    // Show subscription modal for premium features
    window.showSubscriptionModal = function() {
        const modal = document.createElement('div');
        modal.className = 'subscription-modal';
        modal.innerHTML = \`
            <div class="modal-content">
                <h3>Upgrade to Premium</h3>
                <p>Unlock all premium features and enjoy ad-free gaming!</p>
                <div class="subscription-benefits">
                    <div class="benefit">
                        <i class="fas fa-gamepad"></i>
                        <span>Access to ${this.config.monetization.premiumGameCount || 50}+ premium games</span>
                    </div>
                    <div class="benefit">
                        <i class="fas fa-ad"></i>
                        <span>100% ad-free experience</span>
                    </div>
                    <div class="benefit">
                        <i class="fas fa-cloud"></i>
                        <span>Cloud saves & cross-device sync</span>
                    </div>
                </div>
                <div class="modal-actions">
                    <button class="btn btn-primary" onclick="ProcessPayment.createSubscription('PLAN_ID', function(){location.reload();}, console.error)">
                        Subscribe Now - $${this.config.monetization.subscriptionPrice}/month
                    </button>
                    <button class="btn btn-secondary" onclick="closeSubscriptionModal()">Maybe Later</button>
                </div>
            </div>
        \`;
        
        document.body.appendChild(modal);
    };

    window.closeSubscriptionModal = function() {
        const modal = document.querySelector('.subscription-modal');
        if (modal) modal.remove();
    };
</script>`;
    }

    /**
     * Generate revenue analytics dashboard
     */
    generateRevenueAnalytics() {
        return `
<script>
    // Revenue Analytics Dashboard
    window.RevenueAnalytics = {
        // Track revenue events
        trackRevenue: function(source, amount, currency = 'USD') {
            const revenueData = {
                source: source, // 'ads', 'premium', 'subscription'
                amount: parseFloat(amount),
                currency: currency,
                timestamp: Date.now(),
                date: new Date().toISOString().split('T')[0]
            };

            // Store locally for dashboard
            const dailyRevenue = JSON.parse(localStorage.getItem('daily_revenue') || '[]');
            dailyRevenue.push(revenueData);
            localStorage.setItem('daily_revenue', JSON.stringify(dailyRevenue));

            // Send to analytics
            if (typeof gtag !== 'undefined') {
                gtag('event', 'purchase', {
                    transaction_id: 'rev_' + Date.now(),
                    value: amount,
                    currency: currency,
                    event_category: 'revenue',
                    event_label: source
                });
            }
        },

        // Get revenue summary
        getRevenueSummary: function(days = 30) {
            const dailyRevenue = JSON.parse(localStorage.getItem('daily_revenue') || '[]');
            const cutoffDate = Date.now() - (days * 24 * 60 * 60 * 1000);
            
            const recentRevenue = dailyRevenue.filter(r => r.timestamp > cutoffDate);
            
            const summary = {
                total: 0,
                ads: 0,
                premium: 0,
                subscription: 0,
                transactions: recentRevenue.length
            };

            recentRevenue.forEach(r => {
                summary.total += r.amount;
                summary[r.source] = (summary[r.source] || 0) + r.amount;
            });

            return summary;
        },

        // Display revenue dashboard
        showDashboard: function() {
            const summary = this.getRevenueSummary();
            
            const dashboard = document.createElement('div');
            dashboard.className = 'revenue-dashboard';
            dashboard.innerHTML = \`
                <div class="dashboard-header">
                    <h3>Revenue Dashboard (Last 30 Days)</h3>
                    <button onclick="this.parentElement.parentElement.remove()">×</button>
                </div>
                <div class="revenue-stats">
                    <div class="stat">
                        <h4>Total Revenue</h4>
                        <span class="amount">$\${summary.total.toFixed(2)}</span>
                    </div>
                    <div class="stat">
                        <h4>Ad Revenue</h4>
                        <span class="amount">$\${summary.ads.toFixed(2)}</span>
                    </div>
                    <div class="stat">
                        <h4>Premium Sales</h4>
                        <span class="amount">$\${summary.premium.toFixed(2)}</span>
                    </div>
                    <div class="stat">
                        <h4>Subscriptions</h4>
                        <span class="amount">$\${summary.subscription.toFixed(2)}</span>
                    </div>
                </div>
                <div class="chart-placeholder">
                    <p>Revenue chart would be displayed here</p>
                </div>
            \`;
            
            document.body.appendChild(dashboard);
        }
    };

    // Automatically track ad revenue (estimated)
    const originalPush = window.adsbygoogle.push;
    window.adsbygoogle.push = function() {
        const result = originalPush.apply(this, arguments);
        // Estimate ad revenue (actual revenue would come from AdSense API)
        RevenueAnalytics.trackRevenue('ads', 0.05); // $0.05 per ad impression estimate
        return result;
    };
</script>`;
    }

    /**
     * Generate complete monetization integration
     */
    generateCompleteIntegration() {
        return {
            analytics: this.generateAnalyticsCode(),
            adsense: this.generateAdSenseCode(),
            payments: this.generatePaymentIntegration(),
            subscriptions: this.generateSubscriptionInterface(),
            adPlacements: this.generateAdPlacements(),
            revenueAnalytics: this.generateRevenueAnalytics(),
            styles: this.generateMonetizationStyles()
        };
    }

    /**
     * Generate CSS styles for monetization elements
     */
    generateMonetizationStyles() {
        return `
/* Monetization Styles */
.ad-container {
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 1rem 0;
    min-height: 50px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 8px;
    position: relative;
}

.ad-container::before {
    content: 'Advertisement';
    position: absolute;
    top: -20px;
    left: 0;
    font-size: 0.7rem;
    color: #666;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.ad-header {
    margin: 2rem 0;
}

.ad-sidebar {
    position: sticky;
    top: 100px;
}

.ad-footer {
    margin: 2rem 0;
}

.ad-mobile {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 999;
}

@media (max-width: 768px) {
    .ad-header,
    .ad-sidebar,
    .ad-footer {
        display: none;
    }
}

/* Payment Modal Styles */
.payment-modal,
.subscription-modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10000;
}

.payment-modal-content,
.modal-content {
    background: #1a1a1a;
    padding: 2rem;
    border-radius: 16px;
    max-width: 400px;
    width: 90%;
    position: relative;
}

.modal-close {
    position: absolute;
    top: 1rem;
    right: 1rem;
    background: none;
    border: none;
    color: #fff;
    font-size: 1.5rem;
    cursor: pointer;
}

.pricing {
    text-align: center;
    margin: 1.5rem 0;
}

.price {
    font-size: 2rem;
    font-weight: bold;
    color: var(--primary-color);
}

.description {
    color: #666;
    margin-left: 0.5rem;
}

.payment-methods {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.subscription-benefits {
    margin: 1.5rem 0;
}

.benefit {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    margin: 0.75rem 0;
    color: #ccc;
}

.benefit i {
    color: var(--primary-color);
    width: 20px;
}

/* Revenue Dashboard Styles */
.revenue-dashboard {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: #1a1a1a;
    border: 1px solid #333;
    border-radius: 16px;
    padding: 2rem;
    max-width: 600px;
    width: 90%;
    z-index: 10000;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
}

.dashboard-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #333;
}

.revenue-stats {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
    gap: 1rem;
    margin-bottom: 2rem;
}

.stat {
    text-align: center;
    padding: 1rem;
    background: rgba(255, 107, 53, 0.1);
    border-radius: 8px;
}

.stat h4 {
    font-size: 0.8rem;
    color: #666;
    margin-bottom: 0.5rem;
    text-transform: uppercase;
}

.stat .amount {
    font-size: 1.25rem;
    font-weight: bold;
    color: var(--primary-color);
}

.chart-placeholder {
    height: 200px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #666;
}

/* Premium unlock button */
.premium-unlock-btn {
    background: linear-gradient(135deg, gold, orange);
    color: #000;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 20px;
    font-weight: bold;
    cursor: pointer;
    transition: transform 0.2s ease;
}

.premium-unlock-btn:hover {
    transform: scale(1.05);
}

/* Subscription card */
.subscription-card {
    background: linear-gradient(135deg, var(--primary-color), var(--accent-color));
    padding: 2rem;
    border-radius: 16px;
    color: white;
    margin: 2rem 0;
}

.subscription-card h3 {
    text-align: center;
    margin-bottom: 1rem;
}

.benefits ul {
    list-style: none;
    padding: 0;
}

.benefits li {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0.5rem 0;
}

.benefits i {
    color: #00d084;
}

.pricing-table {
    text-align: center;
    margin-top: 2rem;
}

.plan h4 {
    margin-bottom: 0.5rem;
}

.plan .price {
    font-size: 2rem;
    font-weight: bold;
    margin-bottom: 1rem;
}

.plan .price span {
    font-size: 1rem;
    font-weight: normal;
}`;
    }
}

module.exports = MonetizationAnalyticsSystem;