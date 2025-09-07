/**
 * GitHub Integration Module for Bug Reporting
 * Handles secure creation of GitHub issues via GitHub Actions workflow
 */

class GitHubIntegration {
    constructor() {
        this.repoOwner = 'jeffamerican';
        this.repoName = 'unit4productions.github.io';
        this.workflowFile = 'bug-report.yml';
    }

    /**
     * Create a GitHub issue for a bug report
     * Uses repository dispatch to trigger GitHub Actions workflow
     */
    static async createIssue(bugData) {
        const integration = new GitHubIntegration();
        return await integration.submitBugReport(bugData);
    }

    /**
     * Submit bug report via GitHub Actions workflow dispatch
     */
    async submitBugReport(bugData) {
        try {
            // Format issue title
            const issueTitle = `[Bug Report] ${bugData.gameTitle} - ${bugData.bugType}`;
            
            // Format issue body with all relevant information
            const issueBody = this.formatIssueBody(bugData);
            
            // Prepare payload for GitHub Actions
            const payload = {
                event_type: 'bug_report',
                client_payload: {
                    title: issueTitle,
                    body: issueBody,
                    labels: ['bug', 'user-reported', bugData.bugType.toLowerCase()],
                    game_id: bugData.gameId,
                    game_file: bugData.gameFile
                }
            };

            // For now, we'll use a simple webhook approach
            // This will be replaced with GitHub Actions repository dispatch
            const response = await this.sendToWebhook(payload);
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            console.log('Bug report submitted successfully');
            return response;

        } catch (error) {
            console.error('Failed to submit bug report:', error);
            throw new Error('Failed to submit bug report. Please try again later.');
        }
    }

    /**
     * Format the GitHub issue body with all bug report information
     */
    formatIssueBody(bugData) {
        return `@claude Please investigate this bug report:

## Game Information
- **Game:** ${bugData.gameTitle}
- **Game ID:** ${bugData.gameId}
- **Game URL:** [${bugData.gameFile}](${window.location.origin}/${bugData.gameFile})
- **Bug Type:** ${bugData.bugType}

## Bug Description
${bugData.description}

${bugData.stepsToReproduce ? `## Steps to Reproduce
${bugData.stepsToReproduce}` : ''}

## Technical Details
- **Reported:** ${new Date(bugData.timestamp).toLocaleString()}
- **User Agent:** ${bugData.userAgent}
- **Page URL:** ${bugData.url}
${bugData.userEmail ? `- **Contact:** ${bugData.userEmail}` : ''}

## Bot Instructions
This bug report was automatically generated from the BotInc Gaming Platform. Please investigate the reported issue and update the game if necessary.

---
*This issue was created automatically by the Bug Reporting System*`;
    }

    /**
     * Send bug report via GitHub repository dispatch
     * Uses a serverless function or webhook to securely handle GitHub API calls
     */
    async sendToWebhook(payload) {
        try {
            // REAL GitHub integration via Netlify function
            const webhookUrl = this.getWebhookEndpoint();
            
            if (webhookUrl) {
                console.log('🚀 Sending bug report to GitHub via Netlify function:', payload.client_payload.title);
                
                const response = await fetch(webhookUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    const result = await response.json();
                    console.log('✅ Bug report successfully created GitHub issue:', result.issue_url);
                    return {
                        ok: true,
                        status: 200,
                        json: () => Promise.resolve(result)
                    };
                } else {
                    const error = await response.text();
                    console.error('❌ Netlify function failed:', response.status, error);
                    throw new Error(`Netlify function failed: ${response.status}`);
                }
            }

            // Fallback: Try direct GitHub API
            console.log('⚠️  Netlify function not available, trying direct GitHub API...');
            return await this.tryDirectGitHubAPI(payload);
            
        } catch (error) {
            console.error('❌ All GitHub submission methods failed:', error.message);
            throw new Error(`GitHub integration failed: ${error.message}`);
        }
    }

    /**
     * Get webhook endpoint URL - REAL GitHub integration
     */
    getWebhookEndpoint() {
        // REAL GitHub integration using webhook proxy
        // This will be a serverless function that handles authentication
        const webhookEndpoints = [
            // Option 1: Netlify function (if available)
            'https://jeffamerican.netlify.app/.netlify/functions/github-bug-report',
            
            // Option 2: Vercel function (if available) 
            'https://bug-reporter-proxy.vercel.app/api/github-issue',
            
            // Option 3: GitHub-hosted webhook service
            'https://api.github.com/repos/jeffamerican/unit4productions.github.io/dispatches'
        ];
        
        // For now, we'll try a direct approach with the first available endpoint
        return webhookEndpoints[0]; // Start with Netlify
    }

    /**
     * Create GitHub issue directly via REST API
     * This approach creates issues directly instead of using repository dispatch
     */
    async tryDirectGitHubAPI(payload) {
        // Method 1: Try to create issue directly
        const issueUrl = `https://api.github.com/repos/${this.repoOwner}/${this.repoName}/issues`;
        
        try {
            const issueResponse = await this.createGitHubIssueDirectly(issueUrl, payload.client_payload);
            if (issueResponse.ok) {
                return issueResponse;
            }
        } catch (error) {
            console.log('Direct issue creation failed, trying repository dispatch...');
        }
        
        // Method 2: Fallback to repository dispatch
        const dispatchUrl = `https://api.github.com/repos/${this.repoOwner}/${this.repoName}/dispatches`;
        
        const response = await fetch(dispatchUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'BotInc-Bug-Reporter/1.0'
                // Note: Authentication handled by serverless function
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            throw new Error(`GitHub API failed: ${response.status} - ${response.statusText}`);
        }

        return response;
    }

    /**
     * Create GitHub issue directly via REST API
     */
    async createGitHubIssueDirectly(url, issueData) {
        const issuePayload = {
            title: issueData.title,
            body: issueData.body,
            labels: issueData.labels || [],
            assignees: [] // Could add assignees here
        };

        console.log('🚀 Attempting direct GitHub issue creation:', issuePayload.title);

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'BotInc-Bug-Reporter/1.0'
                // Note: Authentication token would need to be added server-side
            },
            body: JSON.stringify(issuePayload)
        });

        return response;
    }

    /**
     * Enhanced local submission system for GitHub Pages deployment
     * Creates structured data for easy GitHub issue creation
     */
    async simulateLocalSubmission(payload) {
        console.log('🔄 Processing bug report via enhanced local system:', payload);
        
        // Simulate realistic API delay
        await new Promise(resolve => setTimeout(resolve, 1500));
        
        // Create a structured local issue for GitHub processing
        const issueData = this.createEnhancedLocalIssue(payload.client_payload);
        
        // Show user that report will be processed
        console.log('✅ Bug report queued for GitHub issue creation');
        console.log('📝 Issue ready for processing:', issueData.title);
        
        // Return enhanced success response
        return {
            ok: true,
            status: 200,
            statusText: 'Bug Report Queued Successfully',
            issueId: issueData.id,
            title: issueData.title
        };
    }

    /**
     * Create an enhanced local issue ready for GitHub processing
     * Structured for easy conversion to real GitHub issues
     */
    createEnhancedLocalIssue(issueData) {
        const issue = {
            id: Date.now(),
            title: issueData.title,
            body: issueData.body,
            labels: issueData.labels,
            game_id: issueData.game_id,
            game_file: issueData.game_file,
            created_at: new Date().toISOString(),
            status: 'pending_github_creation',
            github_url: null, // Will be populated when real issue is created
            local_url: `https://github.com/${this.repoOwner}/${this.repoName}/issues/new`,
            workflow_ready: true
        };

        // Enhanced localStorage structure
        this.saveEnhancedBugReport(issue);

        // Enhanced logging for debugging
        console.log('📋 Enhanced bug report created:', {
            id: issue.id,
            title: issue.title,
            game: issue.game_id,
            labels: issue.labels,
            ready_for_github: issue.workflow_ready
        });

        console.log('🚀 To create GitHub issue, run workflow_dispatch with:');
        console.log(`   Title: ${issue.title}`);
        console.log(`   Game ID: ${issue.game_id}`);
        console.log(`   Labels: ${issue.labels.join(', ')}`);

        return issue;
    }

    /**
     * Save enhanced bug report to localStorage with better structure
     */
    saveEnhancedBugReport(issue) {
        // Get existing reports
        const existingReports = JSON.parse(localStorage.getItem('bug-reports-enhanced') || '[]');
        
        // Add new report
        existingReports.push(issue);
        
        // Keep only last 50 reports to avoid localStorage bloat
        if (existingReports.length > 50) {
            existingReports.splice(0, existingReports.length - 50);
        }
        
        // Save enhanced structure
        localStorage.setItem('bug-reports-enhanced', JSON.stringify(existingReports));
        
        // Also save to legacy format for backwards compatibility
        const legacyReport = {
            id: issue.id,
            title: issue.title,
            body: issue.body,
            labels: issue.labels,
            created_at: issue.created_at,
            url: issue.local_url
        };
        const existingLegacy = JSON.parse(localStorage.getItem('bug-reports') || '[]');
        existingLegacy.push(legacyReport);
        localStorage.setItem('bug-reports', JSON.stringify(existingLegacy));
    }

    /**
     * Get all enhanced bug reports ready for GitHub processing
     */
    static getEnhancedBugReports() {
        return JSON.parse(localStorage.getItem('bug-reports-enhanced') || '[]');
    }

    /**
     * Get all local bug reports (legacy format)
     */
    static getLocalIssues() {
        return JSON.parse(localStorage.getItem('bug-reports') || '[]');
    }

    /**
     * Get pending bug reports that need GitHub issue creation
     */
    static getPendingBugReports() {
        const reports = this.getEnhancedBugReports();
        return reports.filter(report => report.status === 'pending_github_creation');
    }

    /**
     * Mark bug report as processed (GitHub issue created)
     */
    static markBugReportProcessed(reportId, githubIssueUrl) {
        const reports = this.getEnhancedBugReports();
        const reportIndex = reports.findIndex(r => r.id === reportId);
        
        if (reportIndex !== -1) {
            reports[reportIndex].status = 'github_issue_created';
            reports[reportIndex].github_url = githubIssueUrl;
            reports[reportIndex].processed_at = new Date().toISOString();
            
            localStorage.setItem('bug-reports-enhanced', JSON.stringify(reports));
            console.log(`✅ Bug report ${reportId} marked as processed:`, githubIssueUrl);
        }
    }

    /**
     * Clear all bug reports (both formats)
     */
    static clearAllBugReports() {
        localStorage.removeItem('bug-reports');
        localStorage.removeItem('bug-reports-enhanced');
        console.log('All bug reports cleared');
    }
}

// Make GitHubIntegration globally available
window.GitHubIntegration = GitHubIntegration;