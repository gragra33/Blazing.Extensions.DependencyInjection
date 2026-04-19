<#
.SYNOPSIS
    Local CI/CD test runner for Blazing.Extensions.DependencyInjection.

.DESCRIPTION
    Validates and executes GitHub Actions workflows locally using actionlint (static analysis)
    and act (Docker-based execution). Mirrors GitHub Actions behavior for net8.0/net9.0/net10.0.

.PARAMETER Mode
    dry      - Validate workflow graph via act dry-run only (requires Docker + act)
    lint     - actionlint static analysis only
    ci       - Full workflow execution via act
    all      - lint + ci (default)

.PARAMETER Workflow
    ci       - Run only ci.yml (default)
    release  - Run only release.yml
    both     - Run both workflows

.PARAMETER Job
    Optionally run one job by name (for Mode=ci).

.EXAMPLE
    .\ci-cd-test-run.ps1
    .\ci-cd-test-run.ps1 -Mode lint
    .\ci-cd-test-run.ps1 -Mode dry
    .\ci-cd-test-run.ps1 -Mode ci -Workflow ci -Job build-and-test
#>

[CmdletBinding()]
param(
    [ValidateSet('dry', 'lint', 'ci', 'all')]
    [string]$Mode = 'all',

    [ValidateSet('ci', 'release', 'both')]
    [string]$Workflow = 'ci',

    [string]$Job = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Header { param([string]$msg) Write-Host "`n━━━ $msg ━━━" -ForegroundColor Cyan }
function Write-Pass { param([string]$msg) Write-Host "  ✅ $msg" -ForegroundColor Green }
function Write-Fail { param([string]$msg) Write-Host "  ❌ $msg" -ForegroundColor Red }
function Write-Warn { param([string]$msg) Write-Host "  ⚠  $msg" -ForegroundColor Yellow }
function Write-Section { param([string]$msg) Write-Host "`n  ▶ $msg" -ForegroundColor White }

$Script:Errors = [System.Collections.Generic.List[string]]::new()
$Script:Warnings = [System.Collections.Generic.List[string]]::new()

function Add-Error { param([string]$msg) $Script:Errors.Add($msg); Write-Fail $msg }
function Add-Warning { param([string]$msg) $Script:Warnings.Add($msg); Write-Warn $msg }

$RepoRoot = $PSScriptRoot
$WorkflowDir = Join-Path $RepoRoot '.github' 'workflows'
$CiYaml = Join-Path $WorkflowDir 'ci.yml'
$ReleaseYaml = Join-Path $WorkflowDir 'release.yml'
$CiSlnx = Join-Path $RepoRoot 'Blazing.Extensions.DependencyInjection.CI.slnx'

function Test-Tool {
    param([string]$Name)
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

Write-Header 'Prerequisite Check'

if (-not (Test-Tool 'dotnet')) {
    Add-Error "Tool 'dotnet' not found. Install: https://dotnet.microsoft.com/download"
} else {
    Write-Pass "dotnet $(dotnet --version)"
}

if (-not (Test-Path $CiYaml)) { Add-Error "Missing workflow file: $CiYaml" }
if (-not (Test-Path $ReleaseYaml)) { Add-Error "Missing workflow file: $ReleaseYaml" }
if (-not (Test-Path $CiSlnx)) { Add-Error "Missing CI solution file: $CiSlnx" }

$needsActionlint = $Mode -in @('lint', 'all')
$needsAct = $Mode -in @('dry', 'ci', 'all')

$hasActionlint = $false
if ($needsActionlint) {
    $hasActionlint = Test-Tool 'actionlint'
    if (-not $hasActionlint) {
        Add-Error "Tool 'actionlint' not found. Install: winget install rhysd.actionlint"
    }
}

$hasAct = $false
$dockerAvailable = $false
if ($needsAct) {
    $hasAct = Test-Tool 'act'
    if (-not $hasAct) {
        Add-Error "Tool 'act' not found. Install: winget install nektos.act"
    }

    try {
        $null = docker info 2>$null
        $dockerAvailable = $true
        Write-Pass 'Docker daemon reachable'
    } catch {
        Add-Error 'Docker not reachable — act dry/ci modes require Docker'
    }
}

if ($Mode -in @('lint', 'all') -and $hasActionlint) {
    Write-Header 'YAML Static Analysis (actionlint)'

    $yamlFiles = @()
    if ($Workflow -in @('ci', 'both')) { $yamlFiles += $CiYaml }
    if ($Workflow -in @('release', 'both')) { $yamlFiles += $ReleaseYaml }

    foreach ($yaml in $yamlFiles) {
        $name = Split-Path $yaml -Leaf
        Write-Section "Linting $name"
        $out = actionlint $yaml 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Pass "$name — no issues"
        } else {
            $out | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
            Add-Error "$name has actionlint violations (see above)"
        }
    }
}

if ($Mode -in @('dry', 'all') -and $hasAct -and $dockerAvailable) {
    Write-Header 'Workflow Dry-Run (act -n)'

    $dryWorkflows = @()
    if ($Workflow -in @('ci', 'both')) { $dryWorkflows += @{ Name = 'CI'; File = $CiYaml } }
    if ($Workflow -in @('release', 'both')) { $dryWorkflows += @{ Name = 'Release'; File = $ReleaseYaml } }

    foreach ($wf in $dryWorkflows) {
        Write-Section "Dry-run $($wf.Name) workflow"
        Push-Location $RepoRoot
        try {
            $out = act push --workflows $wf.File -n 2>&1
            $failed = @($out | Where-Object {
                $_ -match '(FAIL|error)' -and
                $_ -notmatch 'DRYRUN' -and
                $_ -notmatch 'upload-artifact' -and
                $_ -notmatch '\.cache\\act\\actions-upload-artifact' -and
                $_ -notmatch 'The system cannot find the file specified'
            })

            if ($LASTEXITCODE -eq 0 -or $failed.Count -eq 0) {
                Write-Pass "$($wf.Name) dry-run succeeded"
            } else {
                $out | Where-Object { $_ -match '(FAIL|error|warn)' } | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
                Add-Error "$($wf.Name) dry-run reported issues"
            }
        } finally {
            Pop-Location
        }
    }
}

if ($Mode -in @('ci', 'all') -and $hasAct -and $dockerAvailable) {
    Write-Header 'Full CI Execution (act)'

    $actWorkflows = @()
    if ($Workflow -in @('ci', 'both')) { $actWorkflows += @{ Name = 'CI'; File = $CiYaml; Event = 'push' } }
    if ($Workflow -in @('release', 'both')) { $actWorkflows += @{ Name = 'Release'; File = $ReleaseYaml; Event = 'push' } }

    foreach ($wf in $actWorkflows) {
        Write-Section "Running $($wf.Name) workflow via act"
        Push-Location $RepoRoot
        try {
            $actArgs = @($wf.Event, '--workflows', $wf.File)
            if ($Job) { $actArgs += @('-j', $Job) }

            $outLines = [System.Collections.Generic.List[string]]::new()
            & act @actArgs 2>&1 | ForEach-Object {
                $outLines.Add($_)
                if ($_ -match '(✅|❌|🏁|PASS|FAIL|Error|error:|warning:)') {
                    Write-Host "    $_"
                }
            }

            $jobSucceeded = @($outLines | Where-Object { $_ -match '🏁.*Job succeeded' })
            $jobFailed = @($outLines | Where-Object { $_ -match '🏁.*Job failed' })
            $testPassed = @($outLines | Where-Object { $_ -match 'Passed!.*Failed:\s+0' })
            $testFailed = @($outLines | Where-Object { $_ -match 'Failed!.*Failed:\s+[^0]' })

            if ($testPassed.Count -gt 0) {
                $testPassed | ForEach-Object { Write-Pass ($_ -replace '^\|\s*', '') }
            }
            if ($testFailed.Count -gt 0) {
                $testFailed | ForEach-Object { Add-Error ($_ -replace '^\|\s*', '') }
            }

            $realFailures = @($jobFailed | Where-Object { $_ -notmatch 'Upload test results' })

            if ($LASTEXITCODE -eq 0 -or ($jobSucceeded.Count -gt 0 -and $realFailures.Count -eq 0)) {
                Write-Pass "$($wf.Name) workflow — all jobs succeeded"
            } else {
                Add-Error "$($wf.Name) workflow had job failures (see above)"
            }
        } finally {
            Pop-Location
        }
    }
}

Write-Header 'Summary'

if ($Script:Warnings.Count -gt 0) {
    Write-Host "`n  Warnings:" -ForegroundColor Yellow
    $Script:Warnings | ForEach-Object { Write-Host "    ⚠  $_" -ForegroundColor Yellow }
}

if ($Script:Errors.Count -eq 0) {
    Write-Host "`n  ✅ All checks passed!`n" -ForegroundColor Green
    exit 0
}

Write-Host "`n  ❌ $($Script:Errors.Count) error(s) found:" -ForegroundColor Red
$Script:Errors | ForEach-Object { Write-Host "    • $_" -ForegroundColor Red }
Write-Host ''
exit 1
