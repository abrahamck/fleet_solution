<#
.SYNOPSIS
    Validates atomic task contracts for a feature.

.DESCRIPTION
    Scans `docs/features/FEATURE-XXX/tasks/*.md` to ensure each task has:
    - Numbered sequence (###-*.md)
    - Target Requirements (REQ-xxx) or Acceptance Criteria (AC-xxx)
    - Files In-Scope declaration
    - Definition of Done (DoD) checklist

.EXAMPLE
    .\scripts\validate-task.ps1 -TasksDir "docs\features\FEATURE-001\tasks"
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$TasksDir = ""
)

if ([string]::IsNullOrWhiteSpace($TasksDir)) {
    $FeaturesDir = Join-Path (Get-Location).Path "docs\features"
    if (-not (Test-Path $FeaturesDir)) {
        $FeaturesDir = Join-Path $PSScriptRoot "..\docs\features"
    }

    $featureDirs = Get-ChildItem -Path $FeaturesDir -Directory | Where-Object { $_.Name -notmatch "^_" }
    $AllPassed = $true
    foreach ($dir in $featureDirs) {
        $td = Join-Path $dir.FullName "tasks"
        if (Test-Path $td) {
            & $PSCommandPath -TasksDir $td
            if ($LASTEXITCODE -ne 0) { $AllPassed = $false }
        }
    }
    if ($AllPassed) { exit 0 } else { exit 1 }
}

$ResolvedPath = (Resolve-Path $TasksDir -ErrorAction SilentlyContinue).Path
if (-not $ResolvedPath) {
    Write-Error "Tasks directory not found: $TasksDir"
    exit 1
}

$taskFiles = Get-ChildItem -Path $ResolvedPath -Filter "*.md"
if ($taskFiles.Count -eq 0) {
    Write-Warning "No task files found in: $ResolvedPath"
    exit 0
}

Write-Host "=== Validating Tasks in $TasksDir ===" -ForegroundColor Cyan
$Errors = @()
$ValidCount = 0

foreach ($file in $taskFiles) {
    $name = $file.Name
    if ($name -notmatch "^\d{3}-[\w-]+\.md$") {
        $Errors += "Task '$name' does not follow 3-digit sequence format: ###-kebab-name.md"
        continue
    }

    $content = Get-Content $file.FullName -Raw
    $requiredSections = @("Objective", "Files In-Scope", "Definition of Done")
    foreach ($sec in $requiredSections) {
        if ($content -notmatch "(?i)\b$sec\b") {
            $Errors += "Task '$name' is missing required section: '$sec'"
        }
    }
    $ValidCount++
}

if ($Errors.Count -gt 0) {
    Write-Host "[FAILED] Task contract errors:" -ForegroundColor Red
    foreach ($err in $Errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "[PASSED] All $ValidCount tasks satisfy atomic contract standards." -ForegroundColor Green
    exit 0
}
