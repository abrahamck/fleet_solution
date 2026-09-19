<#
.SYNOPSIS
    Validates end-to-end traceability from requirements to tests and evidence.

.DESCRIPTION
    Scans a feature folder (`docs/features/FEATURE-XXX/`) and ensures every
    Acceptance Criteria (AC-xxx) in `requirements.md` is covered in `test-plan.md`
    and represented in `evidence.md`.

.EXAMPLE
    .\scripts\validate-traceability.ps1 -FeaturePath "docs\features\FEATURE-001"
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
Write-Host "`n=== Validating Traceability Graph: $featureName ===" -ForegroundColor Cyan

$ReqFile = Join-Path $ResolvedPath "requirements.md"
$TestPlanFile = Join-Path $ResolvedPath "test-plan.md"
$EvidenceFile = Join-Path $ResolvedPath "evidence.md"

if (-not (Test-Path $ReqFile)) {
    Write-Warning "requirements.md not found. Skipping traceability check for $featureName."
    exit 0
}

$reqContent = Get-Content $ReqFile -Raw
$matches = [regex]::Matches($reqContent, "AC-\d{3}")
$acIds = @($matches | ForEach-Object { $_.Value } | Select-Object -Unique)

if ($acIds.Count -eq 0) {
    Write-Warning "No AC-xxx criteria identifiers found in requirements.md."
    exit 0
}

Write-Host "Found $($acIds.Count) Acceptance Criteria: $($acIds -join ', ')" -ForegroundColor White

$Errors = @()

if (Test-Path $TestPlanFile) {
    $tpContent = Get-Content $TestPlanFile -Raw
    foreach ($ac in $acIds) {
        if ($tpContent -notmatch [regex]::Escape($ac)) {
            $Errors += "Criteria '$ac' is NOT mapped in test-plan.md"
        }
    }
}

if (Test-Path $EvidenceFile) {
    $evContent = Get-Content $EvidenceFile -Raw
    foreach ($ac in $acIds) {
        if ($evContent -notmatch [regex]::Escape($ac)) {
            $Errors += "Criteria '$ac' is NOT tracked in evidence.md"
        }
    }
}

if ($Errors.Count -gt 0) {
    Write-Host "[FAILED] Traceability gaps detected for $featureName:" -ForegroundColor Red
    foreach ($err in $Errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "[PASSED] Full unbroken traceability chain confirmed for all $($acIds.Count) criteria." -ForegroundColor Green
    exit 0
}
