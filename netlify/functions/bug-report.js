/**
 * Netlify serverless function to handle bug reports
 * Securely creates GitHub issues using repository dispatch
 */

exports.handler = async (event, context) => {
    // Only allow POST requests
    if (event.httpMethod !== 'POST') {
        return {
            statusCode: 405,
            body: JSON.stringify({ error: 'Method not allowed' })
        };
    }

    try {
        // Parse the bug report payload
        const payload = JSON.parse(event.body);
        
        // Validate payload structure
        if (!payload.event_type || payload.event_type !== 'bug_report' || !payload.client_payload) {
            return {
                statusCode: 400,
                body: JSON.stringify({ error: 'Invalid payload structure' })
            };
        }

        // GitHub repository details
        const REPO_OWNER = 'Unit4Productions';
        const REPO_NAME = 'NEXUS';
        const GITHUB_TOKEN = process.env.G_TOKEN;

        if (!GITHUB_TOKEN) {
            console.error('G_TOKEN environment variable not set');
            return {
                statusCode: 500,
                body: JSON.stringify({ error: 'Server configuration error' })
            };
        }

        // Call GitHub repository dispatch API
        const response = await fetch(`https://api.github.com/repos/${REPO_OWNER}/${REPO_NAME}/dispatches`, {
            method: 'POST',
            headers: {
                'Authorization': `token ${GITHUB_TOKEN}`,
                'Content-Type': 'application/json',
                'Accept': 'application/vnd.github.v3+json',
                'User-Agent': 'BotInc-Bug-Reporter/1.0'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('GitHub API error:', response.status, errorText);
            return {
                statusCode: response.status,
                body: JSON.stringify({ 
                    error: 'Failed to create GitHub issue',
                    details: errorText 
                })
            };
        }

        // Success
        return {
            statusCode: 200,
            headers: {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Headers': 'Content-Type',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                success: true, 
                message: 'Bug report submitted successfully' 
            })
        };

    } catch (error) {
        console.error('Function error:', error);
        return {
            statusCode: 500,
            body: JSON.stringify({ 
                error: 'Internal server error',
                message: error.message 
            })
        };
    }
};