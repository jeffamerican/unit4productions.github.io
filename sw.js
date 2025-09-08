// Service Worker for BotInc Gaming Platform
// Version: 5.0.0 - Mobile Full-Screen Game Preview Modal Implementation

const CACHE_NAME = 'botinc-v5.0.0';
const STATIC_CACHE = 'botinc-static-v5.0.0';
const DYNAMIC_CACHE = 'botinc-dynamic-v5.0.0';

// Files to cache for offline functionality
const STATIC_FILES = [
    '/',
    '/index.html',
    '/assets/css/main.css',
    '/assets/js/main.js',
    '/assets/js/games.js',
    '/assets/js/analytics-dashboard.js',
    '/assets/js/viral-integration.js',
    '/assets/js/influencer-tools.js',
    '/assets/js/community-system.js',
    '/assets/js/viral-mechanics.js',
    '/assets/js/mobile-optimization.js',
    '/assets/js/ab-testing.js',
    '/assets/images/botinc-logo.svg'
];

// Install event - cache static files
self.addEventListener('install', (event) => {
    console.log('Service Worker: Installing...');
    
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => {
                console.log('Service Worker: Caching static files');
                return cache.addAll(STATIC_FILES);
            })
            .then(() => {
                console.log('Service Worker: Static files cached successfully');
                return self.skipWaiting();
            })
            .catch((error) => {
                console.error('Service Worker: Failed to cache static files', error);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('Service Worker: Activating...');
    
    event.waitUntil(
        caches.keys()
            .then((cacheNames) => {
                return Promise.all(
                    cacheNames.map((cacheName) => {
                        if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                            console.log('Service Worker: Deleting old cache', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service Worker: Activated successfully');
                return self.clients.claim();
            })
    );
});

// Fetch event - serve cached content when offline
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);
    
    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }
    
    // Skip external requests (analytics, ads, etc.)
    if (url.origin !== location.origin) {
        return;
    }
    
    event.respondWith(
        caches.match(request)
            .then((cachedResponse) => {
                // Return cached version if available
                if (cachedResponse) {
                    console.log('Service Worker: Serving from cache', request.url);
                    return cachedResponse;
                }
                
                // Otherwise, fetch from network
                return fetch(request)
                    .then((response) => {
                        // Don't cache non-successful responses
                        if (!response || response.status !== 200 || response.type !== 'basic') {
                            return response;
                        }
                        
                        // Clone the response
                        const responseToCache = response.clone();
                        
                        // Cache dynamic content
                        caches.open(DYNAMIC_CACHE)
                            .then((cache) => {
                                cache.put(request, responseToCache);
                            });
                        
                        return response;
                    })
                    .catch((error) => {
                        console.log('Service Worker: Network request failed', request.url, error);
                        
                        // Return offline page for navigation requests
                        if (request.destination === 'document') {
                            return caches.match('/index.html');
                        }
                        
                        // Return placeholder for images
                        if (request.destination === 'image') {
                            return new Response(
                                '<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200"><rect width="200" height="200" fill="#f0f0f0"/><text x="100" y="100" text-anchor="middle" fill="#999">Image Offline</text></svg>',
                                { headers: { 'Content-Type': 'image/svg+xml' } }
                            );
                        }
                        
                        throw error;
                    });
            })
    );
});

// Background sync for analytics
self.addEventListener('sync', (event) => {
    if (event.tag === 'analytics-sync') {
        event.waitUntil(syncAnalytics());
    }
});

// Push notifications
self.addEventListener('push', (event) => {
    if (event.data) {
        const data = event.data.json();
        const options = {
            body: data.body,
            icon: '/assets/images/botinc-logo.svg',
            badge: '/assets/images/botinc-logo.svg',
            vibrate: [100, 50, 100],
            data: {
                dateOfArrival: Date.now(),
                primaryKey: data.primaryKey
            },
            actions: [
                {
                    action: 'explore',
                    title: 'Explore Games',
                    icon: '/assets/images/botinc-logo.svg'
                },
                {
                    action: 'close',
                    title: 'Close',
                    icon: '/assets/images/botinc-logo.svg'
                }
            ]
        };
        
        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    }
});

// Notification click handling
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    
    if (event.action === 'explore') {
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

// Helper functions
async function syncAnalytics() {
    try {
        // Get pending analytics data from IndexedDB
        const pendingData = await getPendingAnalytics();
        
        if (pendingData.length > 0) {
            // Send to analytics endpoint
            const response = await fetch('/api/analytics/batch', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(pendingData)
            });
            
            if (response.ok) {
                // Clear pending data
                await clearPendingAnalytics();
                console.log('Service Worker: Analytics synced successfully');
            }
        }
    } catch (error) {
        console.error('Service Worker: Failed to sync analytics', error);
    }
}

async function getPendingAnalytics() {
    // Placeholder for IndexedDB operations
    return [];
}

async function clearPendingAnalytics() {
    // Placeholder for IndexedDB operations
    return Promise.resolve();
}

// Message handling for communication with main thread
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'GET_VERSION') {
        event.ports[0].postMessage({ version: CACHE_NAME });
    }
});

console.log('Service Worker: Loaded successfully');
