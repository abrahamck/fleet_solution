<#
.SYNOPSIS
    Validates ADR naming conventions, required markdown headers, and master index integrity.

.DESCRIPTION
    Scans `docs/ADR/` to ensure all ADRs comply with FleetNexus standards:
    - 3-digit numbering: ADR-###-*.md
    - Standard section headers: Status, Context, Decision, Consequences
    - Registered in docs/ADR/README.md index

.EXAMPLE
    .\scripts\validate-adr.ps1
#>

[CmdletBinding()]
param(
    [string]$AdrDir = ""
)

if ([string]::IsNullOrWhiteSpace($AdrDir)) {
    $AdrDir = Join-Path (Get-Location).Path "docs\ADR"
    if (-not (Test-Path $AdrDir)) {
        $AdrDir = Join-Path $PSScriptRoot "..\docs\ADR"
    }
}

$AdrPath = (Resolve-Path $AdrDir -ErrorAction SilentlyContinue).Path
if (-not $AdrPath) {
    Write-Error "ADR directory not found at: $AdrDir"
    exit 1
}

$ReadmePath = Join-Path $AdrPath "README.md"
if (-not (Test-Path $ReadmePath)) {
    Write-Error "Master ADR README.md index not found at: $ReadmePath"
    exit 1
}

$ReadmeContent = Get-Content $ReadmePath -Raw
$AdrFiles = Get-ChildItem -Path $AdrPath -Filter "ADR-*.md"

$Errors = @()
$ValidCount = 0

Write-Host "=== Validating Architecture Decision Records (docs/ADR/) ===" -ForegroundColor Cyan

foreach ($file in $AdrFiles) {
    $filename = $file.Name
    
    # 1. Check filename format
    if ($filename -notmatch "^ADR-\d{3}-[\w-]+\.md$") {
        $Errors += "File '$filename' does not match naming convention 'ADR-###-kebab-name.md'"
        continue
    }

    $content = Get-Content $file.FullName -Raw

    # 2. Check required sections
    $requiredKeywords = @("Status", "Context", "Decision", "Consequences")
    foreach ($kw in $requiredKeywords) {
        if ($content -notmatch "(?i)\b$kw\b") {
            $Errors += "File '$filename' is missing required section/header '$kw'"
        }
    }

    # 3. Check registration in README.md
    $adrNumber = ($filename -split "-")[0..1] -join "-"
    if ($ReadmeContent -notmatch [regex]::Escape($adrNumber)) {
        $Errors += "ADR '$filename' is not indexed in docs/ADR/README.md"
    }

    $ValidCount++
}

if ($Errors.Count -gt 0) {
    Write-Host "`n[FAILED] Found $($Errors.Count) ADR validation errors:" -ForegroundColor Red
    foreach ($err in $Errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "`n[PASSED] All $ValidCount ADRs are valid and fully indexed!" -ForegroundColor Green
    exit 0
}
