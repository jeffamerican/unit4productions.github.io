/**
 * Vercel serverless function to handle bug reports
 * Securely creates GitHub issues using repository dispatch
 */

export default async function handler(req, res) {
    // Only allow POST requests
    if (req.method !== 'POST') {
        return res.status(405).json({ error: 'Method not allowed' });
    }

    try {
        // Parse the bug report payload
        const payload = req.body;
        
        // Validate payload structure
        if (!payload.event_type || payload.event_type !== 'bug_report' || !payload.client_payload) {
            return res.status(400).json({ error: 'Invalid payload structure' });
        }

        // GitHub repository details
        const REPO_OWNER = 'Unit4Productions';
        const REPO_NAME = 'NEXUS';
        const GITHUB_TOKEN = process.env.G_TOKEN;

        if (!GITHUB_TOKEN) {
            console.error('G_TOKEN environment variable not set');
            return res.status(500).json({ error: 'Server configuration error' });
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
            return res.status(response.status).json({ 
                error: 'Failed to create GitHub issue',
                details: errorText 
            });
        }

        // Success
        res.setHeader('Access-Control-Allow-Origin', '*');
        res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
        
        return res.status(200).json({ 
            success: true, 
            message: 'Bug report submitted successfully' 
        });

    } catch (error) {
        console.error('Function error:', error);
        return res.status(500).json({ 
            error: 'Internal server error',
            message: error.message 
        });
    }
}