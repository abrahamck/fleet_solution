<#
.SYNOPSIS
    Git Worktree Automation Helper for FleetNexus V3 AI Feature Development Workflow.

.DESCRIPTION
    Automates creating, listing, and cleaning up isolated Git worktrees in the `.worktrees/` directory.

.EXAMPLE
    .\scripts\worktree-helper.ps1 create -FeatureId "FEATURE-001" -Branch "feature/FEATURE-001-signup"
    .\scripts\worktree-helper.ps1 list
    .\scripts\worktree-helper.ps1 remove -FeatureId "FEATURE-001"
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet("create", "list", "remove", "status")]
    [string]$Action,

    [Parameter(Mandatory = $false)]
    [string]$FeatureId,

    [Parameter(Mandatory = $false)]
    [string]$Branch
)

$RepoRoot = (Resolve-Path "$PSScriptRoot\..").Path
$WorktreesRoot = Join-Path $RepoRoot ".worktrees"

if (-not (Test-Path $WorktreesRoot)) {
    New-Item -ItemType Directory -Path $WorktreesRoot -Force | Out-Null
}

switch ($Action) {
    "list" {
        Write-Host "=== Active FleetNexus Git Worktrees ===" -ForegroundColor Cyan
        git -C $RepoRoot worktree list
    }

    "create" {
        if (-not $FeatureId) {
            Write-Error "Please specify -FeatureId (e.g. FEATURE-001)"
            return
        }

        $TargetDir = Join-Path $WorktreesRoot $FeatureId
        if (Test-Path $TargetDir) {
            Write-Warning "Worktree directory already exists at: $TargetDir"
            return
        }

        if (-not $Branch) {
            $Branch = "feature/$FeatureId"
        }

        Write-Host "Creating isolated worktree for $FeatureId on branch '$Branch'..." -ForegroundColor Green
        
        # Check if branch exists
        $branchExists = git -C $RepoRoot branch --list $Branch
        if ($branchExists) {
            git -C $RepoRoot worktree add "$TargetDir" "$Branch"
        } else {
            git -C $RepoRoot worktree add -b "$Branch" "$TargetDir" HEAD
        }

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully created worktree at: $TargetDir" -ForegroundColor Green
            Write-Host "Agent can operate safely inside: $TargetDir" -ForegroundColor Cyan
        } else {
            Write-Error "Failed to create worktree. Exit code: $LASTEXITCODE"
        }
    }

    "remove" {
        if (-not $FeatureId) {
            Write-Error "Please specify -FeatureId to remove (e.g. FEATURE-001)"
            return
        }

        $TargetDir = Join-Path $WorktreesRoot $FeatureId
        Write-Host "Removing worktree for $FeatureId at $TargetDir..." -ForegroundColor Yellow
        git -C $RepoRoot worktree remove "$TargetDir" --force
        git -C $RepoRoot worktree prune
        Write-Host "Worktree removed." -ForegroundColor Green
    }

    "status" {
        git -C $RepoRoot status
    }
}
