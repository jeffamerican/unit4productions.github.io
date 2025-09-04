/**
 * GitHub API Manager for Unit4Productions Gaming Website Automation
 * Handles all GitHub API interactions for automated deployment
 */

class GitHubAPIManager {
    constructor(token, owner = 'unit4productions') {
        this.token = token;
        this.owner = owner;
        this.baseURL = 'https://api.github.com';
        this.headers = {
            'Authorization': `Bearer ${token}`,
            'Accept': 'application/vnd.github.v3+json',
            'User-Agent': 'Unit4Productions-Automation'
        };
    }

    /**
     * Create a new repository for the gaming website
     */
    async createRepository(repoName, description = 'Unit4Productions Gaming Website') {
        const url = `${this.baseURL}/user/repos`;
        const data = {
            name: repoName,
            description: description,
            homepage: 'https://unit4productions.com',
            private: false,
            has_issues: true,
            has_projects: true,
            has_wiki: false,
            auto_init: true,
            gitignore_template: 'Node',
            license_template: 'mit'
        };

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: this.headers,
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`Failed to create repository: ${response.statusText}`);
            }

            const repo = await response.json();
            console.log(`Repository created: ${repo.html_url}`);
            return repo;
        } catch (error) {
            console.error('Error creating repository:', error);
            throw error;
        }
    }

    /**
     * Enable GitHub Pages for the repository
     */
    async enableGitHubPages(repoName, customDomain = null) {
        const url = `${this.baseURL}/repos/${this.owner}/${repoName}/pages`;
        const data = {
            source: {
                branch: 'main',
                path: '/'
            }
        };

        if (customDomain) {
            data.cname = customDomain;
        }

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: this.headers,
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`Failed to enable GitHub Pages: ${response.statusText}`);
            }

            const pages = await response.json();
            console.log(`GitHub Pages enabled: ${pages.html_url}`);
            return pages;
        } catch (error) {
            console.error('Error enabling GitHub Pages:', error);
            throw error;
        }
    }

    /**
     * Upload or update a file in the repository
     */
    async uploadFile(repoName, filePath, content, message = 'Update file via automation') {
        // First, try to get the current file to get its SHA (for updates)
        let sha = null;
        try {
            const existingFile = await this.getFile(repoName, filePath);
            sha = existingFile.sha;
        } catch (error) {
            // File doesn't exist, that's okay for new files
        }

        const url = `${this.baseURL}/repos/${this.owner}/${repoName}/contents/${filePath}`;
        const data = {
            message: message,
            content: Buffer.from(content, 'utf8').toString('base64')
        };

        if (sha) {
            data.sha = sha;
        }

        try {
            const response = await fetch(url, {
                method: 'PUT',
                headers: this.headers,
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`Failed to upload file ${filePath}: ${response.statusText}`);
            }

            const result = await response.json();
            console.log(`File uploaded: ${filePath}`);
            return result;
        } catch (error) {
            console.error(`Error uploading file ${filePath}:`, error);
            throw error;
        }
    }

    /**
     * Get file contents from repository
     */
    async getFile(repoName, filePath) {
        const url = `${this.baseURL}/repos/${this.owner}/${repoName}/contents/${filePath}`;
        
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: this.headers
            });

            if (!response.ok) {
                throw new Error(`File not found: ${filePath}`);
            }

            return await response.json();
        } catch (error) {
            throw error;
        }
    }

    /**
     * Upload multiple files in batch
     */
    async uploadMultipleFiles(repoName, files, commitMessage = 'Batch upload via automation') {
        const results = [];
        
        for (const file of files) {
            try {
                const result = await this.uploadFile(
                    repoName,
                    file.path,
                    file.content,
                    commitMessage
                );
                results.push({ success: true, path: file.path, result });
            } catch (error) {
                results.push({ success: false, path: file.path, error: error.message });
            }
        }

        return results;
    }

    /**
     * Set up custom domain for GitHub Pages
     */
    async setupCustomDomain(repoName, domain) {
        // Create CNAME file
        await this.uploadFile(repoName, 'CNAME', domain, 'Add custom domain configuration');
        
        // Update repository settings
        const url = `${this.baseURL}/repos/${this.owner}/${repoName}/pages`;
        const data = {
            cname: domain,
            https_enforced: true
        };

        try {
            const response = await fetch(url, {
                method: 'PUT',
                headers: this.headers,
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`Failed to set custom domain: ${response.statusText}`);
            }

            const result = await response.json();
            console.log(`Custom domain configured: ${domain}`);
            return result;
        } catch (error) {
            console.error('Error setting custom domain:', error);
            throw error;
        }
    }

    /**
     * Get deployment status
     */
    async getDeploymentStatus(repoName) {
        const url = `${this.baseURL}/repos/${this.owner}/${repoName}/pages`;
        
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: this.headers
            });

            if (!response.ok) {
                throw new Error(`Failed to get deployment status: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error getting deployment status:', error);
            throw error;
        }
    }

    /**
     * Create a new branch
     */
    async createBranch(repoName, branchName, fromBranch = 'main') {
        // Get the SHA of the source branch
        const refUrl = `${this.baseURL}/repos/${this.owner}/${repoName}/git/refs/heads/${fromBranch}`;
        const refResponse = await fetch(refUrl, {
            method: 'GET',
            headers: this.headers
        });
        
        if (!refResponse.ok) {
            throw new Error(`Failed to get ${fromBranch} branch reference`);
        }
        
        const refData = await refResponse.json();
        const sha = refData.object.sha;

        // Create new branch
        const createUrl = `${this.baseURL}/repos/${this.owner}/${repoName}/git/refs`;
        const data = {
            ref: `refs/heads/${branchName}`,
            sha: sha
        };

        try {
            const response = await fetch(createUrl, {
                method: 'POST',
                headers: this.headers,
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                throw new Error(`Failed to create branch: ${response.statusText}`);
            }

            const result = await response.json();
            console.log(`Branch created: ${branchName}`);
            return result;
        } catch (error) {
            console.error('Error creating branch:', error);
            throw error;
        }
    }

    /**
     * Monitor deployment progress
     */
    async monitorDeployment(repoName, timeout = 300000) { // 5 minutes timeout
        const startTime = Date.now();
        
        while (Date.now() - startTime < timeout) {
            try {
                const status = await this.getDeploymentStatus(repoName);
                
                if (status.status === 'built') {
                    console.log('Deployment completed successfully');
                    return { success: true, url: status.html_url };
                }
                
                if (status.status === 'errored') {
                    throw new Error('Deployment failed');
                }
                
                console.log(`Deployment status: ${status.status}`);
                await new Promise(resolve => setTimeout(resolve, 10000)); // Wait 10 seconds
                
            } catch (error) {
                console.error('Error monitoring deployment:', error);
                await new Promise(resolve => setTimeout(resolve, 10000));
            }
        }
        
        throw new Error('Deployment monitoring timeout');
    }

    /**
     * Get repository statistics
     */
    async getRepositoryStats(repoName) {
        const url = `${this.baseURL}/repos/${this.owner}/${repoName}`;
        
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: this.headers
            });

            if (!response.ok) {
                throw new Error(`Failed to get repository stats: ${response.statusText}`);
            }

            const repo = await response.json();
            return {
                name: repo.name,
                description: repo.description,
                url: repo.html_url,
                homepage: repo.homepage,
                stars: repo.stargazers_count,
                forks: repo.forks_count,
                size: repo.size,
                language: repo.language,
                created_at: repo.created_at,
                updated_at: repo.updated_at,
                pushed_at: repo.pushed_at
            };
        } catch (error) {
            console.error('Error getting repository stats:', error);
            throw error;
        }
    }
}

module.exports = GitHubAPIManager;