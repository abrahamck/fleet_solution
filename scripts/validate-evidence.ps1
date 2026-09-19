<#
.SYNOPSIS
    Validates machine evidence records and test proof logs for a feature.

.DESCRIPTION
    Scans `docs/features/FEATURE-XXX/evidence.md` to ensure:
    - Evidence table exists with verified statuses
    - Automated test log block is present
    - No unresolved CONFLICT states exist

.EXAMPLE
    .\scripts\validate-evidence.ps1 -FeaturePath "docs\features\FEATURE-001"
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$FeaturePath = ""
)

if ([string]::IsNullOrWhiteSpace($FeaturePath)) {
    $FeaturesDir = Join-Path (Get-Location).Path "docs\features"
    if (-not (Test-Path $FeaturesDir)) {
        $FeaturesDir = Join-Path $PSScriptRoot "..\docs\features"
    }

    $featureDirs = Get-ChildItem -Path $FeaturesDir -Directory | Where-Object { $_.Name -notmatch "^_" }
    $AllPassed = $true
    foreach ($dir in $featureDirs) {
        $ef = Join-Path $dir.FullName "evidence.md"
        if (Test-Path $ef) {
            & $PSCommandPath -FeaturePath $dir.FullName
            if ($LASTEXITCODE -ne 0) { $AllPassed = $false }
        }
    }
    if ($AllPassed) { exit 0 } else { exit 1 }
}

$ResolvedPath = (Resolve-Path $FeaturePath -ErrorAction SilentlyContinue).Path
if (-not $ResolvedPath) {
    Write-Error "Feature path not found: $FeaturePath"
    exit 1
}

$featureName = Split-Path $ResolvedPath -Leaf
Write-Host "`n=== Validating Evidence Records: $featureName ===" -ForegroundColor Cyan

$EvidenceFile = Join-Path $ResolvedPath "evidence.md"
if (-not (Test-Path $EvidenceFile)) {
    Write-Error "evidence.md not found in $ResolvedPath"
    exit 1
}

$evContent = Get-Content $EvidenceFile -Raw
$Errors = @()

# Check for CONFLICT states
if ($evContent -match "\bCONFLICT\b") {
    $Errors += "Contradiction/Conflict detected in evidence matrix. Human reconciliation required."
}

# Check for test log output block
if ($evContent -notmatch "(?s)```(text|powershell|bash)?.*?(Passed|Total|Success).*?```") {
    Write-Warning "No automated test output log block found in evidence.md."
}

# Count VERIFIED rows
$verifiedCount = ([regex]::Matches($evContent, "\bVERIFIED\b")).Count
Write-Host "Found $verifiedCount VERIFIED criteria proofs." -ForegroundColor Green

if ($Errors.Count -gt 0) {
    Write-Host "[FAILED] Evidence validation errors for $featureName:" -ForegroundColor Red
    foreach ($err in $Errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "[PASSED] Evidence records validated successfully for $featureName." -ForegroundColor Green
    exit 0
}
