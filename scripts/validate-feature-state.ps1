<#
.SYNOPSIS
    Validates state machine transitions and preconditions for a FleetNexus feature directory.

.DESCRIPTION
    Ensures that a feature directory (`docs/features/FEATURE-XXX/`) satisfies all preconditions
    required for its declared state in `status.md`.

.EXAMPLE
    .\scripts\validate-feature-state.ps1 -FeaturePath "docs\features\FEATURE-001"
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$FeaturePath = ""
)

if ([string]::IsNullOrWhiteSpace($FeaturePath)) {
    # Scan all feature directories if not specified
    $FeaturesDir = Join-Path (Get-Location).Path "docs\features"
    if (-not (Test-Path $FeaturesDir)) {
        $FeaturesDir = Join-Path $PSScriptRoot "..\docs\features"
    }

    $featureDirs = Get-ChildItem -Path $FeaturesDir -Directory | Where-Object { $_.Name -notmatch "^_" }
    if ($featureDirs.Count -eq 0) {
        Write-Host "No feature directories found to validate in $FeaturesDir." -ForegroundColor Yellow
        exit 0
    }

    $AllPassed = $true
    foreach ($dir in $featureDirs) {
        & $PSCommandPath -FeaturePath $dir.FullName
        if ($LASTEXITCODE -ne 0) { $AllPassed = $false }
    }
    if ($AllPassed) { exit 0 } else { exit 1 }
}

$ResolvedPath = (Resolve-Path $FeaturePath -ErrorAction SilentlyContinue).Path
if (-not $ResolvedPath) {
    Write-Error "Feature path not found: $FeaturePath"
    exit 1
}

$featureName = Split-Path $ResolvedPath -Leaf
Write-Host "`n=== Validating Feature State: $featureName ===" -ForegroundColor Cyan

$StatusFile = Join-Path $ResolvedPath "status.md"
if (-not (Test-Path $StatusFile)) {
    Write-Error "Missing status.md in feature directory: $ResolvedPath"
    exit 1
}

$statusContent = Get-Content $StatusFile -Raw

# Determine current state
$match = [regex]::Match($statusContent, "(?i)\*\*Current State\*\*:\s*`?([A-Z_]+)`?")
if (-not $match.Success) {
    Write-Error "Could not parse **Current State** in $StatusFile"
    exit 1
}
$currentState = $match.Groups[1].Value
Write-Host "Current State: $currentState" -ForegroundColor White

$ReqFile = Join-Path $ResolvedPath "requirements.md"
$DesignFile = Join-Path $ResolvedPath "design.md"
$PlanFile = Join-Path $ResolvedPath "implementation-plan.md"
$TestPlanFile = Join-Path $ResolvedPath "test-plan.md"
$EvidenceFile = Join-Path $ResolvedPath "evidence.md"
$TasksDir = Join-Path $ResolvedPath "tasks"

$Errors = @()

switch ($currentState) {
    "INTAKE" {
        # Minimal checks
    }
    "DISCOVERY" {
        if (-not (Test-Path $ReqFile)) { $Errors += "State DISCOVERY requires requirements.md to exist." }
    }
    "REQUIREMENTS_REVIEW" {
        if (-not (Test-Path $ReqFile)) { $Errors += "State REQUIREMENTS_REVIEW requires requirements.md." }
    }
    "ARCHITECTURE_REVIEW" {
        if (-not (Test-Path $ReqFile)) { $Errors += "requirements.md is required." }
        else {
            $reqContent = Get-Content $ReqFile -Raw
            if ($reqContent -notmatch "(?i)\*\*Status\*\*:\s*`?APPROVED`?") {
                $Errors += "Gate 1 Violation: requirements.md must be marked APPROVED before ARCHITECTURE_REVIEW."
            }
        }
        if (-not (Test-Path $DesignFile)) { $Errors += "design.md must exist." }
    }
    "PLANNING" {
        if (-not (Test-Path $DesignFile)) { $Errors += "design.md must exist." }
        else {
            $designContent = Get-Content $DesignFile -Raw
            if ($designContent -notmatch "(?i)\*\*Status\*\*:\s*`?APPROVED`?") {
                $Errors += "Gate 2 Violation: design.md must be marked APPROVED before PLANNING."
            }
        }
        if (-not (Test-Path $PlanFile)) { $Errors += "implementation-plan.md must exist." }
    }
    "TEST_DESIGN" {
        if (-not (Test-Path $ReqFile)) { $Errors += "requirements.md is required." }
        if (-not (Test-Path $TestPlanFile)) { $Errors += "test-plan.md must exist." }
    }
    "READY_FOR_IMPLEMENTATION" {
        if (-not (Test-Path $ReqFile)) { $Errors += "requirements.md is required." }
        if (-not (Test-Path $DesignFile)) { $Errors += "design.md is required." }
        if (-not (Test-Path $PlanFile)) { $Errors += "implementation-plan.md is required." }
        if (-not (Test-Path $TestPlanFile)) { $Errors += "test-plan.md is required." }
        if (-not (Test-Path $TasksDir) -or (Get-ChildItem $TasksDir -Filter "*.md").Count -eq 0) {
            $Errors += "tasks/ directory must contain at least one task contract."
        }
    }
    "IMPLEMENTATION" {
        if (-not (Test-Path $TasksDir)) { $Errors += "tasks/ directory is required." }
        if (-not (Test-Path $EvidenceFile)) { $Errors += "evidence.md must exist to record verification evidence." }
    }
    "VERIFICATION" {
        if (-not (Test-Path $EvidenceFile)) { $Errors += "evidence.md is required." }
    }
    "REVIEW" {
        if (-not (Test-Path $EvidenceFile)) { $Errors += "evidence.md is required." }
    }
    "APPROVED" {
        if (-not (Test-Path $EvidenceFile)) { $Errors += "evidence.md is required." }
    }
    "MERGED" {
        # Terminal state
    }
    Default {
        $Errors += "Unknown state: $currentState"
    }
}

if ($Errors.Count -gt 0) {
    Write-Host "[FAILED] State validation errors for $featureName:" -ForegroundColor Red
    foreach ($err in $Errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "[PASSED] Feature $featureName satisfies all preconditions for state $currentState." -ForegroundColor Green
    exit 0
}
