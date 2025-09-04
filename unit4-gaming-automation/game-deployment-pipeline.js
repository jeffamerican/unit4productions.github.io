/**
 * Game Deployment Pipeline for Unit4Productions
 * Handles automated game processing, optimization, and deployment
 */

const fs = require('fs').promises;
const path = require('path');
const JSZip = require('jszip');
const sharp = require('sharp');

class GameDeploymentPipeline {
    constructor(githubManager, templateSystem) {
        this.githubManager = githubManager;
        this.templateSystem = templateSystem;
        this.supportedFormats = ['html5', 'unity', 'construct', 'flash'];
        this.imageFormats = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
        this.audioFormats = ['.mp3', '.wav', '.ogg', '.m4a'];
        this.videoFormats = ['.mp4', '.webm', '.ogv'];
    }

    /**
     * Deploy a complete game to GitHub Pages
     */
    async deployGame(gameData, gameFiles, repoName) {
        try {
            console.log(`Starting deployment for game: ${gameData.title}`);

            // Step 1: Validate game data and files
            await this.validateGameData(gameData);
            await this.validateGameFiles(gameFiles);

            // Step 2: Process and optimize game files
            const processedFiles = await this.processGameFiles(gameFiles, gameData);

            // Step 3: Generate game page
            const gamePageContent = this.templateSystem.generateGameTemplate(gameData);

            // Step 4: Generate thumbnails and optimize images
            const optimizedMedia = await this.optimizeMediaFiles(processedFiles.media, gameData.slug);

            // Step 5: Create file upload batch
            const filesToUpload = [
                ...processedFiles.game,
                ...optimizedMedia,
                {
                    path: `games/${gameData.slug}/index.html`,
                    content: gamePageContent
                },
                {
                    path: `games/${gameData.slug}/game-config.json`,
                    content: JSON.stringify(gameData, null, 2)
                }
            ];

            // Step 6: Upload files to GitHub
            const uploadResults = await this.githubManager.uploadMultipleFiles(
                repoName,
                filesToUpload,
                `Deploy game: ${gameData.title}`
            );

            // Step 7: Update main games index
            await this.updateGamesIndex(repoName, gameData);

            // Step 8: Generate sitemap entry
            await this.updateSitemap(repoName, gameData);

            // Step 9: Monitor deployment
            const deploymentStatus = await this.githubManager.monitorDeployment(repoName);

            console.log(`Game deployed successfully: ${deploymentStatus.url}/games/${gameData.slug}`);

            return {
                success: true,
                gameUrl: `${deploymentStatus.url}/games/${gameData.slug}`,
                deploymentResults: uploadResults,
                gameData: gameData
            };

        } catch (error) {
            console.error('Game deployment failed:', error);
            throw error;
        }
    }

    /**
     * Validate game data structure
     */
    async validateGameData(gameData) {
        const required = ['title', 'slug', 'description', 'type'];
        const missing = required.filter(field => !gameData[field]);

        if (missing.length > 0) {
            throw new Error(`Missing required game data fields: ${missing.join(', ')}`);
        }

        if (!this.supportedFormats.includes(gameData.type)) {
            throw new Error(`Unsupported game type: ${gameData.type}`);
        }

        // Validate slug format
        const slugRegex = /^[a-z0-9-]+$/;
        if (!slugRegex.test(gameData.slug)) {
            throw new Error('Game slug must contain only lowercase letters, numbers, and hyphens');
        }

        // Set defaults
        gameData.plays = gameData.plays || 0;
        gameData.rating = gameData.rating || 0;
        gameData.tags = gameData.tags || [];
        gameData.createdAt = gameData.createdAt || new Date().toISOString();
        gameData.updatedAt = new Date().toISOString();

        return true;
    }

    /**
     * Validate game files structure
     */
    async validateGameFiles(gameFiles) {
        if (!gameFiles || gameFiles.length === 0) {
            throw new Error('No game files provided');
        }

        // Check for main game file
        const hasMainFile = gameFiles.some(file => 
            file.path === 'index.html' || 
            file.path.endsWith('.unity3d') ||
            file.path.endsWith('.swf')
        );

        if (!hasMainFile) {
            throw new Error('No main game file found (index.html, .unity3d, or .swf)');
        }

        return true;
    }

    /**
     * Process and optimize game files
     */
    async processGameFiles(gameFiles, gameData) {
        const processedFiles = {
            game: [],
            media: [],
            config: []
        };

        for (const file of gameFiles) {
            const ext = path.extname(file.path).toLowerCase();

            // Process based on file type
            if (ext === '.html') {
                const processedHtml = await this.processHTMLFile(file.content, gameData);
                processedFiles.game.push({
                    path: `games/${gameData.slug}/${file.path}`,
                    content: processedHtml
                });
            }
            else if (ext === '.js') {
                const processedJS = await this.processJavaScriptFile(file.content, gameData);
                processedFiles.game.push({
                    path: `games/${gameData.slug}/${file.path}`,
                    content: processedJS
                });
            }
            else if (ext === '.css') {
                const processedCSS = await this.processCSSFile(file.content, gameData);
                processedFiles.game.push({
                    path: `games/${gameData.slug}/${file.path}`,
                    content: processedCSS
                });
            }
            else if (this.imageFormats.includes(ext)) {
                processedFiles.media.push(file);
            }
            else if (this.audioFormats.includes(ext) || this.videoFormats.includes(ext)) {
                processedFiles.media.push(file);
            }
            else {
                // Other files (JSON, XML, etc.)
                processedFiles.game.push({
                    path: `games/${gameData.slug}/${file.path}`,
                    content: file.content
                });
            }
        }

        return processedFiles;
    }

    /**
     * Process HTML files for optimization and analytics injection
     */
    async processHTMLFile(htmlContent, gameData) {
        let processedHTML = htmlContent;

        // Inject meta tags for SEO
        const metaTags = `
    <meta name="description" content="${gameData.description}">
    <meta name="keywords" content="${(gameData.tags || []).join(', ')}">
    <meta property="og:title" content="${gameData.title}">
    <meta property="og:description" content="${gameData.description}">
    <meta property="og:type" content="game">
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="${gameData.title}">
    <meta name="twitter:description" content="${gameData.description}">`;

        // Inject analytics code
        const analyticsCode = `
    <script>
        // Game Analytics
        function trackGameEvent(action, value) {
            if (typeof gtag !== 'undefined') {
                gtag('event', action, {
                    'event_category': 'game',
                    'event_label': '${gameData.title}',
                    'value': value || 1
                });
            }
        }
        
        // Track game start
        trackGameEvent('game_start');
        
        // Track game loading time
        window.addEventListener('load', () => {
            trackGameEvent('game_loaded');
        });
    </script>`;

        // Insert meta tags in head
        if (processedHTML.includes('<head>')) {
            processedHTML = processedHTML.replace('<head>', `<head>${metaTags}`);
        }

        // Insert analytics before closing body tag
        if (processedHTML.includes('</body>')) {
            processedHTML = processedHTML.replace('</body>', `${analyticsCode}</body>`);
        }

        // Optimize for mobile
        if (!processedHTML.includes('viewport')) {
            const viewportTag = '<meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">';
            processedHTML = processedHTML.replace('<head>', `<head>${viewportTag}`);
        }

        // Add game wrapper for responsive design
        const gameWrapperCSS = `
    <style>
        .game-wrapper {
            width: 100%;
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #000;
            overflow: hidden;
        }
        
        .game-container {
            max-width: 100%;
            max-height: 100%;
            position: relative;
        }
        
        @media (max-width: 768px) {
            .game-wrapper {
                height: 100vh;
                height: -webkit-fill-available;
            }
        }
    </style>`;

        processedHTML = processedHTML.replace('</head>', `${gameWrapperCSS}</head>`);

        return processedHTML;
    }

    /**
     * Process JavaScript files for optimization
     */
    async processJavaScriptFile(jsContent, gameData) {
        let processedJS = jsContent;

        // Add game configuration
        const gameConfig = `
// Game Configuration
window.GAME_CONFIG = ${JSON.stringify({
    title: gameData.title,
    slug: gameData.slug,
    version: gameData.version || '1.0.0',
    analytics: true
})};

// Analytics helper functions
window.trackGameEvent = function(action, value) {
    if (typeof gtag !== 'undefined') {
        gtag('event', action, {
            'event_category': 'game',
            'event_label': '${gameData.title}',
            'value': value || 1
        });
    }
};

`;

        processedJS = gameConfig + processedJS;

        return processedJS;
    }

    /**
     * Process CSS files for optimization
     */
    async processCSSFile(cssContent, gameData) {
        let processedCSS = cssContent;

        // Add responsive design improvements
        const responsiveCSS = `
/* Responsive Game Styles */
@media (max-width: 768px) {
    body {
        margin: 0;
        padding: 0;
        overflow: hidden;
    }
    
    canvas, iframe {
        width: 100vw !important;
        height: 100vh !important;
        max-width: none !important;
        max-height: none !important;
    }
}

/* Loading styles */
.game-loading {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: #000;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    color: #fff;
    z-index: 1000;
}

.game-loading.hidden {
    display: none;
}

`;

        processedCSS = responsiveCSS + processedCSS;

        return processedCSS;
    }

    /**
     * Optimize media files (images, audio, video)
     */
    async optimizeMediaFiles(mediaFiles, gameSlug) {
        const optimizedFiles = [];

        for (const file of mediaFiles) {
            const ext = path.extname(file.path).toLowerCase();

            if (this.imageFormats.includes(ext)) {
                const optimizedImages = await this.optimizeImage(file, gameSlug);
                optimizedFiles.push(...optimizedImages);
            } else {
                // For audio/video files, just copy them
                optimizedFiles.push({
                    path: `games/${gameSlug}/${file.path}`,
                    content: file.content
                });
            }
        }

        return optimizedFiles;
    }

    /**
     * Optimize individual image file
     */
    async optimizeImage(imageFile, gameSlug) {
        const optimizedFiles = [];
        const ext = path.extname(imageFile.path);
        const baseName = path.basename(imageFile.path, ext);

        try {
            // Create multiple sizes for responsive design
            const sizes = [
                { suffix: '', width: null, height: null }, // Original
                { suffix: '-large', width: 1200, height: null },
                { suffix: '-medium', width: 800, height: null },
                { suffix: '-small', width: 400, height: null },
                { suffix: '-thumb', width: 200, height: 200 }
            ];

            for (const size of sizes) {
                let sharpInstance = sharp(Buffer.from(imageFile.content, 'binary'));

                if (size.width || size.height) {
                    sharpInstance = sharpInstance.resize(size.width, size.height, {
                        fit: 'inside',
                        withoutEnlargement: true
                    });
                }

                // Convert to WebP for better compression
                const webpBuffer = await sharpInstance.webp({ quality: 80 }).toBuffer();
                
                optimizedFiles.push({
                    path: `games/${gameSlug}/assets/images/${baseName}${size.suffix}.webp`,
                    content: webpBuffer.toString('base64')
                });

                // Keep original format as fallback
                const originalBuffer = await sharpInstance.toBuffer();
                optimizedFiles.push({
                    path: `games/${gameSlug}/assets/images/${baseName}${size.suffix}${ext}`,
                    content: originalBuffer.toString('base64')
                });
            }

        } catch (error) {
            console.warn(`Failed to optimize image ${imageFile.path}:`, error);
            // Fallback: use original file
            optimizedFiles.push({
                path: `games/${gameSlug}/${imageFile.path}`,
                content: imageFile.content
            });
        }

        return optimizedFiles;
    }

    /**
     * Update the main games index with new game
     */
    async updateGamesIndex(repoName, gameData) {
        try {
            // Get current games index
            let gamesIndex = [];
            try {
                const indexFile = await this.githubManager.getFile(repoName, 'games/index.json');
                gamesIndex = JSON.parse(Buffer.from(indexFile.content, 'base64').toString());
            } catch (error) {
                // File doesn't exist, start with empty array
            }

            // Add or update game in index
            const existingIndex = gamesIndex.findIndex(game => game.slug === gameData.slug);
            if (existingIndex >= 0) {
                gamesIndex[existingIndex] = gameData;
            } else {
                gamesIndex.push(gameData);
            }

            // Sort by creation date (newest first)
            gamesIndex.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

            // Upload updated index
            await this.githubManager.uploadFile(
                repoName,
                'games/index.json',
                JSON.stringify(gamesIndex, null, 2),
                `Update games index for ${gameData.title}`
            );

            // Update main page with games list
            const mainPageContent = this.templateSystem.generateMainTemplate(gamesIndex);
            await this.githubManager.uploadFile(
                repoName,
                'index.html',
                mainPageContent,
                `Update main page with ${gameData.title}`
            );

            console.log('Games index updated successfully');

        } catch (error) {
            console.error('Failed to update games index:', error);
            throw error;
        }
    }

    /**
     * Update sitemap with new game
     */
    async updateSitemap(repoName, gameData) {
        try {
            let sitemap = [];
            
            try {
                const sitemapFile = await this.githubManager.getFile(repoName, 'sitemap.json');
                sitemap = JSON.parse(Buffer.from(sitemapFile.content, 'base64').toString());
            } catch (error) {
                // Initialize sitemap
                sitemap = [{
                    url: '/',
                    lastmod: new Date().toISOString(),
                    changefreq: 'daily',
                    priority: '1.0'
                }];
            }

            // Add game to sitemap
            const gameUrl = `/games/${gameData.slug}`;
            const existingEntry = sitemap.find(entry => entry.url === gameUrl);
            
            if (existingEntry) {
                existingEntry.lastmod = new Date().toISOString();
            } else {
                sitemap.push({
                    url: gameUrl,
                    lastmod: new Date().toISOString(),
                    changefreq: 'weekly',
                    priority: '0.8'
                });
            }

            // Generate XML sitemap
            const xmlSitemap = this.generateXMLSitemap(sitemap);

            // Upload both JSON and XML versions
            await Promise.all([
                this.githubManager.uploadFile(
                    repoName,
                    'sitemap.json',
                    JSON.stringify(sitemap, null, 2),
                    'Update sitemap'
                ),
                this.githubManager.uploadFile(
                    repoName,
                    'sitemap.xml',
                    xmlSitemap,
                    'Update XML sitemap'
                )
            ]);

            console.log('Sitemap updated successfully');

        } catch (error) {
            console.error('Failed to update sitemap:', error);
        }
    }

    /**
     * Generate XML sitemap
     */
    generateXMLSitemap(sitemapData) {
        const baseUrl = this.templateSystem.siteUrl;
        
        let xml = '<?xml version="1.0" encoding="UTF-8"?>\n';
        xml += '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n';

        for (const entry of sitemapData) {
            xml += '  <url>\n';
            xml += `    <loc>${baseUrl}${entry.url}</loc>\n`;
            xml += `    <lastmod>${entry.lastmod}</lastmod>\n`;
            xml += `    <changefreq>${entry.changefreq}</changefreq>\n`;
            xml += `    <priority>${entry.priority}</priority>\n`;
            xml += '  </url>\n';
        }

        xml += '</urlset>';
        return xml;
    }

    /**
     * Deploy multiple games in batch
     */
    async deployMultipleGames(gamesData, repoName) {
        const results = [];

        for (const { gameData, gameFiles } of gamesData) {
            try {
                const result = await this.deployGame(gameData, gameFiles, repoName);
                results.push({ success: true, game: gameData.title, result });
                
                // Wait between deployments to avoid rate limits
                await new Promise(resolve => setTimeout(resolve, 2000));
                
            } catch (error) {
                results.push({ 
                    success: false, 
                    game: gameData.title, 
                    error: error.message 
                });
            }
        }

        return results;
    }

    /**
     * Update existing game
     */
    async updateGame(gameData, gameFiles, repoName) {
        // Same as deploy but with update messaging
        return this.deployGame(gameData, gameFiles, repoName);
    }

    /**
     * Remove game from deployment
     */
    async removeGame(gameSlug, repoName) {
        try {
            // Get current games index
            const indexFile = await this.githubManager.getFile(repoName, 'games/index.json');
            let gamesIndex = JSON.parse(Buffer.from(indexFile.content, 'base64').toString());

            // Remove game from index
            gamesIndex = gamesIndex.filter(game => game.slug !== gameSlug);

            // Update games index
            await this.githubManager.uploadFile(
                repoName,
                'games/index.json',
                JSON.stringify(gamesIndex, null, 2),
                `Remove game: ${gameSlug}`
            );

            // Update main page
            const mainPageContent = this.templateSystem.generateMainTemplate(gamesIndex);
            await this.githubManager.uploadFile(
                repoName,
                'index.html',
                mainPageContent,
                `Update main page after removing ${gameSlug}`
            );

            console.log(`Game ${gameSlug} removed successfully`);

            return { success: true, removedGame: gameSlug };

        } catch (error) {
            console.error(`Failed to remove game ${gameSlug}:`, error);
            throw error;
        }
    }

    /**
     * Generate game package from uploaded files
     */
    async processGamePackage(packageBuffer, gameData) {
        const zip = new JSZip();
        const zipContent = await zip.loadAsync(packageBuffer);
        const gameFiles = [];

        // Extract all files from zip
        for (const [path, file] of Object.entries(zipContent.files)) {
            if (!file.dir) {
                const content = await file.async('string');
                gameFiles.push({ path, content });
            }
        }

        return gameFiles;
    }
}

module.exports = GameDeploymentPipeline;