# Bug Report Webhook Setup Guide

## Overview
The bug report system uses a serverless function to securely handle GitHub API calls with the `G_TOKEN` secret. This prevents exposing sensitive tokens in the frontend code.

## Deployment Options

### Option 1: Netlify (Recommended)
1. Deploy your site to Netlify
2. Add the `G_TOKEN` environment variable in Netlify dashboard
3. The function will be automatically available at: `https://your-site.netlify.app/.netlify/functions/bug-report`

### Option 2: Vercel
1. Deploy your site to Vercel
2. Add the `G_TOKEN` environment variable in Vercel dashboard  
3. The function will be automatically available at: `https://your-site.vercel.app/api/bug-report`

### Option 3: Custom Server
Deploy the serverless function code to any platform that supports Node.js functions.

## Configuration

### Environment Variables
Set the following environment variable in your deployment platform:
- `G_TOKEN`: Your GitHub Personal Access Token with `public_repo` scope

### Frontend Configuration
Update the `getWebhookEndpoint()` function in `assets/js/github-integration.js`:

```javascript
getWebhookEndpoint() {
    // Replace with your actual webhook URL
    return 'https://your-site.netlify.app/.netlify/functions/bug-report';
}
```

## Testing
1. Deploy the serverless function with the `G_TOKEN` environment variable
2. Update the webhook URL in the frontend
3. Test bug report submission
4. Check GitHub repository for created issues

## Fallback Behavior
If the webhook is not available, the system will:
1. Try direct GitHub API (will fail due to CORS)
2. Fall back to local simulation mode
3. Store reports in localStorage for manual processing

This ensures the bug report system always works, even during setup.