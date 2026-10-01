# Upload script for Mahyar Free VPN
# این اسکریپت فایل‌ها را مستقیماً از طریق GitHub API آپلود می‌کند
# نیاز به Git نصب ندارد!

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,
    
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [string]$RepoName = "mahyar-free",
    [string]$Branch = "main",
    [string]$SourcePath = "C:\Projects\mahyar-free-extracted"
)

$ErrorActionPreference = "Stop"

# Base64 URL encode function (GitHub uses this for content)
function ConvertTo-Base64Url {
    param([string]$Text)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $base64 = [Convert]::ToBase64String($bytes)
    return $base64.TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

# Convert file path to base64 for API
function Get-FileBase64 {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    return [Convert]::ToBase64String($bytes)
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Mahyar Free VPN - GitHub Uploader" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Username: $GitHubUsername" -ForegroundColor Yellow
Write-Host "Repository: $RepoName" -ForegroundColor Yellow
Write-Host "Source: $SourcePath" -ForegroundColor Yellow
Write-Host ""

# Check if source exists
if (-not (Test-Path $SourcePath)) {
    Write-Host "❌ Source path not found: $SourcePath" -ForegroundColor Red
    exit 1
}

# Get the default branch SHA (or create initial commit)
$apiBase = "https://api.github.com"
$headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept" = "application/vnd.github.v3+json"
    "User-Agent" = "MahyarFree-Uploader"
}

Write-Host "📡 Getting repository info..." -ForegroundColor Cyan
try {
    $repoInfo = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName" -Headers $headers -Method Get
    $defaultBranch = $repoInfo.default_branch
    Write-Host "✓ Repository found. Default branch: $defaultBranch" -ForegroundColor Green
} catch {
    Write-Host "❌ Cannot access repository: $_" -ForegroundColor Red
    Write-Host "Make sure the repository exists and your token has 'repo' scope." -ForegroundColor Yellow
    exit 1
}

# Get the latest commit SHA
Write-Host "📡 Getting latest commit..." -ForegroundColor Cyan
$ref = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/refs/heads/$Branch" -Headers $headers
$latestCommitSha = $ref.object.sha
Write-Host "✓ Latest commit: $latestCommitSha" -ForegroundColor Green

# Get the tree of latest commit
$commit = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/commits/$latestCommitSha" -Headers $headers
$baseTreeSha = $commit.tree.sha
Write-Host "✓ Base tree: $baseTreeSha" -ForegroundColor Green

# Collect all files
Write-Host "📁 Collecting files..." -ForegroundColor Cyan
$files = Get-ChildItem $SourcePath -Recurse -File | Where-Object { -not $_.FullName.Contains("\.git\") }
$treeItems = @()

foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($SourcePath.Length + 1) -replace '\\', '/'
    $content = Get-FileBase64 -Path $file.FullName
    
    $treeItems += @{
        path = $relativePath
        mode = "100644"
        type = "blob"
        content = [System.Text.Encoding]::UTF8.GetString([System.IO.File]::ReadAllBytes($file.FullName))
    }
    Write-Host "  + $relativePath" -ForegroundColor Gray
}

Write-Host "✓ Found $($files.Count) files" -ForegroundColor Green
Write-Host ""

# Create blobs
Write-Host "📦 Creating blobs..." -ForegroundColor Cyan
$blobShas = @{}
foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($SourcePath.Length + 1) -replace '\\', '/'
    $content = [System.Text.Encoding]::UTF8.GetString([System.IO.File]::ReadAllBytes($file.FullName))
    
    $body = @{
        content = $content
        encoding = "utf-8"
    } | ConvertTo-Json
    
    $blob = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/blobs" -Headers $headers -Method Post -Body $body
    $blobShas[$relativePath] = $blob.sha
}

Write-Host "✓ Created $($blobShas.Count) blobs" -ForegroundColor Green
Write-Host ""

# Create new tree
Write-Host "🌳 Creating tree..." -ForegroundColor Cyan
$treeObjects = @()
foreach ($key in $blobShas.Keys) {
    $treeObjects += @{
        path = $key
        mode = "100644"
        type = "blob"
        sha = $blobShas[$key]
    }
}

$treeBody = @{
    base_tree = $baseTreeSha
    tree = $treeObjects
} | ConvertTo-Json -Depth 10

$newTree = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/trees" -Headers $headers -Method Post -Body $treeBody
Write-Host "✓ Tree created: $($newTree.sha)" -ForegroundColor Green
Write-Host ""

# Create commit
Write-Host "💾 Creating commit..." -ForegroundColor Cyan
$commitBody = @{
    message = "Initial commit - Mahyar Free VPN v1.0

- Modern WPF UI with glassmorphism and glow effects
- Powered by sing-box v1.14
- Real TCP ping testing
- Auto-fetch configs from repository
- System tray support
- Multi-protocol: VMess, VLess, Trojan, Shadowsocks, Hysteria2
- Live traffic statistics
- Animated UI with pulse and rotation effects"
    tree = $newTree.sha
    parents = @($latestCommitSha)
} | ConvertTo-Json

$newCommit = Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/commits" -Headers $headers -Method Post -Body $commitBody
Write-Host "✓ Commit created: $($newCommit.sha)" -ForegroundColor Green
Write-Host ""

# Update reference
Write-Host "🔄 Updating branch reference..." -ForegroundColor Cyan
$refBody = @{
    sha = $newCommit.sha
} | ConvertTo-Json

Invoke-RestMethod -Uri "$apiBase/repos/$GitHubUsername/$RepoName/git/refs/heads/$Branch" -Headers $headers -Method Patch -Body $refBody | Out-Null
Write-Host "✓ Branch updated" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ Upload complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Repository: https://github.com/$GitHubUsername/$RepoName" -ForegroundColor Cyan
Write-Host "Actions: https://github.com/$GitHubUsername/$RepoName/actions" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Go to Actions tab" -ForegroundColor White
Write-Host "2. Wait for workflow to complete (~3-5 minutes)" -ForegroundColor White
Write-Host "3. Download the artifact from the workflow run" -ForegroundColor White
Write-Host ""
