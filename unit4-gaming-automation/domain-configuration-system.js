/**
 * Custom Domain Configuration System for Unit4Productions
 * Handles automatic domain setup, DNS management, and SSL configuration
 */

const dns = require('dns').promises;

class DomainConfigurationSystem {
    constructor(githubManager, cloudflareConfig = null) {
        this.githubManager = githubManager;
        this.cloudflareConfig = cloudflareConfig;
        this.domains = {
            primary: 'unit4productions.com',
            staging: 'staging.unit4productions.com',
            dev: 'dev.unit4productions.com'
        };
    }

    /**
     * Setup complete domain configuration for GitHub Pages
     */
    async setupCustomDomain(repoName, domain, environment = 'production') {
        try {
            console.log(`Setting up custom domain: ${domain}`);

            // Step 1: Validate domain
            await this.validateDomain(domain);

            // Step 2: Setup GitHub Pages custom domain
            await this.githubManager.setupCustomDomain(repoName, domain);

            // Step 3: Generate DNS configuration instructions
            const dnsConfig = await this.generateDNSConfiguration(domain, repoName);

            // Step 4: Setup Cloudflare if configured
            if (this.cloudflareConfig) {
                await this.setupCloudflare(domain, dnsConfig);
            }

            // Step 5: Generate domain verification files
            await this.generateDomainVerification(repoName, domain);

            // Step 6: Setup redirects and aliases
            await this.setupDomainRedirects(repoName, domain);

            // Step 7: Monitor SSL certificate status
            const sslStatus = await this.monitorSSLCertificate(domain);

            console.log(`Domain setup completed for: ${domain}`);

            return {
                success: true,
                domain: domain,
                dnsConfiguration: dnsConfig,
                sslStatus: sslStatus,
                verificationFiles: ['domain-verification.html', 'CNAME']
            };

        } catch (error) {
            console.error(`Domain setup failed for ${domain}:`, error);
            throw error;
        }
    }

    /**
     * Validate domain configuration
     */
    async validateDomain(domain) {
        // Basic domain format validation
        const domainRegex = /^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9](?:\.[a-zA-Z]{2,})+$/;
        if (!domainRegex.test(domain)) {
            throw new Error(`Invalid domain format: ${domain}`);
        }

        // Check if domain exists
        try {
            await dns.resolve(domain);
            console.log(`Domain ${domain} is valid and resolvable`);
        } catch (error) {
            console.warn(`Domain ${domain} may not be configured yet: ${error.message}`);
        }

        return true;
    }

    /**
     * Generate DNS configuration instructions
     */
    async generateDNSConfiguration(domain, repoName) {
        const githubPages = [
            '185.199.108.153',
            '185.199.109.153',
            '185.199.110.153',
            '185.199.111.153'
        ];

        const config = {
            domain: domain,
            records: [
                // A records for apex domain
                {
                    type: 'A',
                    name: '@',
                    value: githubPages[0],
                    ttl: 300
                },
                {
                    type: 'A',
                    name: '@',
                    value: githubPages[1],
                    ttl: 300
                },
                {
                    type: 'A',
                    name: '@',
                    value: githubPages[2],
                    ttl: 300
                },
                {
                    type: 'A',
                    name: '@',
                    value: githubPages[3],
                    ttl: 300
                },
                // CNAME for www
                {
                    type: 'CNAME',
                    name: 'www',
                    value: `${this.githubManager.owner}.github.io`,
                    ttl: 300
                }
            ],
            instructions: this.generateDNSInstructions(domain)
        };

        return config;
    }

    /**
     * Generate human-readable DNS setup instructions
     */
    generateDNSInstructions(domain) {
        return {
            title: `DNS Configuration for ${domain}`,
            steps: [
                {
                    step: 1,
                    title: 'Login to your domain registrar',
                    description: 'Access your domain management panel (GoDaddy, Namecheap, etc.)'
                },
                {
                    step: 2,
                    title: 'Navigate to DNS Management',
                    description: 'Find the DNS or Domain settings section'
                },
                {
                    step: 3,
                    title: 'Add A Records',
                    description: 'Add the following A records for your root domain:',
                    records: [
                        'A @ 185.199.108.153',
                        'A @ 185.199.109.153',
                        'A @ 185.199.110.153',
                        'A @ 185.199.111.153'
                    ]
                },
                {
                    step: 4,
                    title: 'Add CNAME Record',
                    description: 'Add CNAME record for www subdomain:',
                    records: [
                        `CNAME www ${this.githubManager.owner}.github.io`
                    ]
                },
                {
                    step: 5,
                    title: 'Save Changes',
                    description: 'Save your DNS settings and wait for propagation (up to 48 hours)'
                }
            ],
            commonProviders: {
                'GoDaddy': {
                    url: 'https://dcc.godaddy.com/manage/dns',
                    notes: 'Use "Host" field for name, "Points to" for value'
                },
                'Namecheap': {
                    url: 'https://ap.www.namecheap.com/domains/domaincontrolpanel',
                    notes: 'Use "Advanced DNS" tab'
                },
                'Cloudflare': {
                    url: 'https://dash.cloudflare.com',
                    notes: 'DNS records section in domain overview'
                },
                'AWS Route 53': {
                    url: 'https://console.aws.amazon.com/route53',
                    notes: 'Create hosted zone and record sets'
                }
            }
        };
    }

    /**
     * Setup Cloudflare DNS automation
     */
    async setupCloudflare(domain, dnsConfig) {
        if (!this.cloudflareConfig || !this.cloudflareConfig.apiToken) {
            console.log('Cloudflare not configured, skipping automated DNS setup');
            return;
        }

        const headers = {
            'Authorization': `Bearer ${this.cloudflareConfig.apiToken}`,
            'Content-Type': 'application/json'
        };

        try {
            // Get zone ID
            const zoneResponse = await fetch(
                `https://api.cloudflare.com/client/v4/zones?name=${domain}`,
                { method: 'GET', headers }
            );

            if (!zoneResponse.ok) {
                throw new Error('Failed to get Cloudflare zone information');
            }

            const zoneData = await zoneResponse.json();
            if (zoneData.result.length === 0) {
                throw new Error(`Domain ${domain} not found in Cloudflare`);
            }

            const zoneId = zoneData.result[0].id;

            // Add DNS records
            for (const record of dnsConfig.records) {
                const recordData = {
                    type: record.type,
                    name: record.name === '@' ? domain : record.name,
                    content: record.value,
                    ttl: record.ttl || 300
                };

                const response = await fetch(
                    `https://api.cloudflare.com/client/v4/zones/${zoneId}/dns_records`,
                    {
                        method: 'POST',
                        headers,
                        body: JSON.stringify(recordData)
                    }
                );

                if (response.ok) {
                    console.log(`Created ${record.type} record: ${record.name} -> ${record.value}`);
                } else {
                    const error = await response.json();
                    console.warn(`Failed to create DNS record: ${error.errors?.[0]?.message}`);
                }
            }

            // Enable security features
            await this.setupCloudflareSettings(zoneId);

        } catch (error) {
            console.error('Cloudflare setup failed:', error);
        }
    }

    /**
     * Setup Cloudflare security and performance settings
     */
    async setupCloudflareSettings(zoneId) {
        const headers = {
            'Authorization': `Bearer ${this.cloudflareConfig.apiToken}`,
            'Content-Type': 'application/json'
        };

        const settings = [
            { id: 'ssl', value: 'flexible' },
            { id: 'always_use_https', value: 'on' },
            { id: 'security_level', value: 'medium' },
            { id: 'cache_level', value: 'aggressive' },
            { id: 'browser_cache_ttl', value: 14400 },
            { id: 'minify', value: { css: 'on', html: 'on', js: 'on' } }
        ];

        for (const setting of settings) {
            try {
                const response = await fetch(
                    `https://api.cloudflare.com/client/v4/zones/${zoneId}/settings/${setting.id}`,
                    {
                        method: 'PATCH',
                        headers,
                        body: JSON.stringify({ value: setting.value })
                    }
                );

                if (response.ok) {
                    console.log(`Updated Cloudflare setting: ${setting.id}`);
                }
            } catch (error) {
                console.warn(`Failed to update setting ${setting.id}:`, error);
            }
        }
    }

    /**
     * Generate domain verification files
     */
    async generateDomainVerification(repoName, domain) {
        const verificationContent = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Domain Verification - ${domain}</title>
</head>
<body>
    <h1>Domain Verification</h1>
    <p>This file verifies ownership of ${domain} for Unit4Productions.</p>
    <p>Generated on: ${new Date().toISOString()}</p>
    <p>Repository: ${repoName}</p>
</body>
</html>`;

        // Upload domain verification file
        await this.githubManager.uploadFile(
            repoName,
            'domain-verification.html',
            verificationContent,
            'Add domain verification file'
        );

        // Create or update CNAME file
        await this.githubManager.uploadFile(
            repoName,
            'CNAME',
            domain,
            'Setup custom domain CNAME'
        );

        console.log('Domain verification files created');
    }

    /**
     * Setup domain redirects and aliases
     */
    async setupDomainRedirects(repoName, primaryDomain) {
        const redirectsConfig = {
            redirects: [
                // Redirect www to non-www (or vice versa based on preference)
                {
                    from: `www.${primaryDomain}`,
                    to: primaryDomain,
                    status: 301
                },
                // Redirect old paths to new structure
                {
                    from: '/game/*',
                    to: '/games/:splat',
                    status: 301
                }
            ],
            headers: [
                {
                    source: '/games/*',
                    headers: [
                        { key: 'Cache-Control', value: 'public, max-age=86400' },
                        { key: 'X-Frame-Options', value: 'SAMEORIGIN' }
                    ]
                }
            ]
        };

        // Create _redirects file for Netlify-style redirects (if using other platforms)
        const redirectsContent = redirectsConfig.redirects
            .map(redirect => `${redirect.from} ${redirect.to} ${redirect.status}`)
            .join('\n');

        await this.githubManager.uploadFile(
            repoName,
            '_redirects',
            redirectsContent,
            'Setup domain redirects'
        );

        // Create robots.txt
        const robotsContent = `User-agent: *
Allow: /

# Game files
Allow: /games/

# Assets
Allow: /assets/

# Sitemaps
Sitemap: https://${primaryDomain}/sitemap.xml

# Disallow admin areas (if any)
Disallow: /admin/
Disallow: /.git/`;

        await this.githubManager.uploadFile(
            repoName,
            'robots.txt',
            robotsContent,
            'Add robots.txt'
        );

        console.log('Domain redirects and SEO files configured');
    }

    /**
     * Monitor SSL certificate status
     */
    async monitorSSLCertificate(domain, timeout = 300000) {
        const startTime = Date.now();
        
        while (Date.now() - startTime < timeout) {
            try {
                const response = await fetch(`https://${domain}`, {
                    method: 'HEAD',
                    timeout: 10000
                });

                if (response.ok) {
                    return {
                        status: 'active',
                        domain: domain,
                        certificate: 'valid',
                        checkedAt: new Date().toISOString()
                    };
                }
            } catch (error) {
                console.log(`SSL certificate not ready for ${domain}, checking again in 30 seconds...`);
            }

            await new Promise(resolve => setTimeout(resolve, 30000)); // Wait 30 seconds
        }

        return {
            status: 'pending',
            domain: domain,
            certificate: 'pending',
            message: 'SSL certificate may still be provisioning',
            checkedAt: new Date().toISOString()
        };
    }

    /**
     * Setup subdomain for staging/development
     */
    async setupSubdomain(repoName, subdomain, environment = 'staging') {
        const fullDomain = `${subdomain}.${this.domains.primary}`;
        
        try {
            await this.setupCustomDomain(repoName, fullDomain, environment);
            
            // Create environment-specific configuration
            const envConfig = {
                environment: environment,
                domain: fullDomain,
                analytics: environment === 'production',
                debug: environment !== 'production',
                cacheEnabled: environment === 'production'
            };

            await this.githubManager.uploadFile(
                repoName,
                'config.json',
                JSON.stringify(envConfig, null, 2),
                `Setup ${environment} environment configuration`
            );

            return {
                success: true,
                subdomain: fullDomain,
                environment: environment
            };

        } catch (error) {
            console.error(`Subdomain setup failed for ${fullDomain}:`, error);
            throw error;
        }
    }

    /**
     * Check domain health and configuration
     */
    async checkDomainHealth(domain) {
        const healthCheck = {
            domain: domain,
            timestamp: new Date().toISOString(),
            checks: {}
        };

        try {
            // DNS Resolution
            try {
                const addresses = await dns.resolve(domain);
                healthCheck.checks.dns = {
                    status: 'pass',
                    addresses: addresses
                };
            } catch (error) {
                healthCheck.checks.dns = {
                    status: 'fail',
                    error: error.message
                };
            }

            // HTTPS Check
            try {
                const response = await fetch(`https://${domain}`, {
                    method: 'HEAD',
                    timeout: 10000
                });
                
                healthCheck.checks.https = {
                    status: response.ok ? 'pass' : 'fail',
                    statusCode: response.status,
                    headers: Object.fromEntries(response.headers)
                };
            } catch (error) {
                healthCheck.checks.https = {
                    status: 'fail',
                    error: error.message
                };
            }

            // WWW Redirect Check
            try {
                const response = await fetch(`https://www.${domain}`, {
                    method: 'HEAD',
                    timeout: 10000,
                    redirect: 'manual'
                });
                
                healthCheck.checks.wwwRedirect = {
                    status: response.status >= 300 && response.status < 400 ? 'pass' : 'fail',
                    statusCode: response.status,
                    location: response.headers.get('location')
                };
            } catch (error) {
                healthCheck.checks.wwwRedirect = {
                    status: 'fail',
                    error: error.message
                };
            }

        } catch (error) {
            console.error('Domain health check failed:', error);
        }

        return healthCheck;
    }

    /**
     * Generate domain setup report
     */
    generateDomainReport(domain, dnsConfig, sslStatus) {
        return {
            domain: domain,
            setupDate: new Date().toISOString(),
            status: sslStatus.status,
            dnsConfiguration: dnsConfig,
            sslCertificate: sslStatus,
            verificationSteps: [
                {
                    step: 'DNS Configuration',
                    status: 'manual',
                    description: 'Configure DNS records with your domain provider'
                },
                {
                    step: 'Domain Verification',
                    status: 'completed',
                    description: 'Domain verification files uploaded'
                },
                {
                    step: 'SSL Certificate',
                    status: sslStatus.certificate === 'valid' ? 'completed' : 'pending',
                    description: 'GitHub Pages SSL certificate'
                },
                {
                    step: 'Redirects',
                    status: 'completed',
                    description: 'Domain redirects configured'
                }
            ],
            nextSteps: [
                'Configure DNS records with your domain registrar',
                'Wait for DNS propagation (up to 48 hours)',
                'Verify SSL certificate is active',
                'Test all redirects and subdomain configurations'
            ]
        };
    }
}

module.exports = DomainConfigurationSystem;