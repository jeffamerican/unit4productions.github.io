/**
 * Netlify Function: GitHub Bug Report Handler
 * Securely creates GitHub issues from bug reports with proper authentication
 */

exports.handler = async (event, context) => {
    // Set CORS headers for cross-origin requests
    const headers = {
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Headers': 'Content-Type',
        'Access-Control-Allow-Methods': 'POST, OPTIONS',
        'Content-Type': 'application/json'
    };

    // Handle CORS preflight request
    if (event.httpMethod === 'OPTIONS') {
        return {
            statusCode: 200,
            headers,
            body: ''
        };
    }

    // Only allow POST requests
    if (event.httpMethod !== 'POST') {
        return {
            statusCode: 405,
            headers,
            body: JSON.stringify({ error: 'Method not allowed' })
        };
    }

    try {
        // Parse incoming bug report data
        const payload = JSON.parse(event.body);
        
        if (!payload.client_payload) {
            throw new Error('Invalid payload structure');
        }

        const { title, body, labels, game_id } = payload.client_payload;

        // Validate required fields
        if (!title || !body) {
            throw new Error('Title and body are required');
        }

        // Get GitHub token from environment variables
        const githubToken = process.env.G_TOKEN;
        
        if (!githubToken) {
            console.error('G_TOKEN environment variable not set');
            return {
                statusCode: 500,
                headers,
                body: JSON.stringify({ 
                    error: 'Server configuration error',
                    message: 'G_TOKEN not configured in environment'
                })
            };
        }

        // Repository information
        const repoOwner = 'jeffamerican';
        const repoName = 'unit4productions.github.io';

        // Create GitHub issue directly
        const issueResponse = await fetch(`https://api.github.com/repos/${repoOwner}/${repoName}/issues`, {
            method: 'POST',
            headers: {
                'Authorization': `token ${githubToken}`,
                'Content-Type': 'application/json',
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'BotInc-Bug-Reporter-Function/1.0'
            },
            body: JSON.stringify({
                title: title,
                body: body,
                labels: [...(labels || []), `game:${game_id}`],
                assignees: [] // Could add specific assignees
            })
        });

        if (!issueResponse.ok) {
            const errorData = await issueResponse.text();
            console.error('GitHub API error:', issueResponse.status, errorData);
            
            throw new Error(`GitHub API failed: ${issueResponse.status} - ${issueResponse.statusText}`);
        }

        const issueData = await issueResponse.json();

        // Add comment mentioning @claude
        await fetch(`https://api.github.com/repos/${repoOwner}/${repoName}/issues/${issueData.number}/comments`, {
            method: 'POST',
            headers: {
                'Authorization': `token ${githubToken}`,
                'Content-Type': 'application/json',
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'BotInc-Bug-Reporter-Function/1.0'
            },
            body: JSON.stringify({
                body: '@claude This bug report needs your attention! 🤖'
            })
        });

        console.log(`✅ GitHub issue created successfully: ${issueData.html_url}`);

        // Return success response
        return {
            statusCode: 200,
            headers,
            body: JSON.stringify({
                success: true,
                issue_url: issueData.html_url,
                issue_number: issueData.number,
                message: 'Bug report submitted successfully!'
            })
        };

    } catch (error) {
        console.error('Function error:', error.message);
        
        return {
            statusCode: 500,
            headers,
            body: JSON.stringify({
                error: 'Failed to create GitHub issue',
                message: error.message
            })
        };
    }
};