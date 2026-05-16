#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates the template in N representative combinations, builds each and
    runs its test suite. Fails the script if any combination fails to build
    or has failing tests.

.PARAMETER OutputRoot
    Base folder where generated projects will be written. Each combination
    gets its own subfolder. Default: $env:TEMP/apitemplate-validate.

.PARAMETER KeepArtifacts
    Keep the generated projects on disk after the run. Without this flag,
    OutputRoot is wiped at the start of the script.

.EXAMPLE
    pwsh scripts/validate-template.ps1
    pwsh scripts/validate-template.ps1 -KeepArtifacts
#>

[CmdletBinding()]
param(
    [string]$OutputRoot = (Join-Path $env:TEMP 'apitemplate-validate'),
    [switch]$KeepArtifacts
)

$ErrorActionPreference = 'Stop'

# Combinations to exercise. Add or remove rows as you change the template.
$combinations = @(
    @{ Name = 'SqlCqrsJwtFull';        Args = @('--DatabaseProvider','sqlserver','--EnableJwt','true','--ArchitectureStyle','cqrs') },
    @{ Name = 'SqlUseCaseJwt';         Args = @('--DatabaseProvider','sqlserver','--EnableJwt','true','--ArchitectureStyle','usecase') },
    @{ Name = 'PgCqrsDomainNoJwt';     Args = @('--DatabaseProvider','postgres','--EnableJwt','false','--ArchitectureStyle','cqrs','--UseDomainProject','true') },
    @{ Name = 'MySqlUseCaseJwtDomain'; Args = @('--DatabaseProvider','mysql','--EnableJwt','true','--ArchitectureStyle','usecase','--UseDomainProject','true') },
    @{ Name = 'SqlNoValNoRl';          Args = @('--DatabaseProvider','sqlserver','--EnableJwt','true','--UseValidation','false','--EnableRateLimiting','false') },
    @{ Name = 'PgJwtNoPwSec';          Args = @('--DatabaseProvider','postgres','--EnableJwt','true','--EnablePasswordSecurity','false') },
    @{ Name = 'SqlCustomDbName';       Args = @('--DatabaseProvider','sqlserver','--EnableJwt','true','--DbContextName','Sample') }
)

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    Write-Host "Installing template from $repoRoot ..." -ForegroundColor Cyan
    dotnet new install . --force | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'dotnet new install failed.' }

    if (-not $KeepArtifacts -and (Test-Path $OutputRoot)) {
        Remove-Item -Recurse -Force $OutputRoot
    }
    New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

    $results = @()
    foreach ($combo in $combinations) {
        $name = $combo.Name
        $projectPath = Join-Path $OutputRoot $name

        Write-Host ''
        Write-Host "=== $name ===" -ForegroundColor Yellow
        Write-Host "args: $($combo.Args -join ' ')"

        if (Test-Path $projectPath) { Remove-Item -Recurse -Force $projectPath }

        $generateArgs = @('new','viana-api','-o',$projectPath) + $combo.Args
        & dotnet @generateArgs | Out-Null
        if ($LASTEXITCODE -ne 0) {
            $results += [PSCustomObject]@{ Name = $name; Step = 'generate'; Passed = 0; Failed = 0; Status = 'FAIL' }
            continue
        }

        $buildOutput = & dotnet build $projectPath --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host ($buildOutput | Select-String -Pattern 'error ' | Select-Object -First 5)
            $results += [PSCustomObject]@{ Name = $name; Step = 'build'; Passed = 0; Failed = 0; Status = 'FAIL' }
            continue
        }

        $testOutput = & dotnet test $projectPath --nologo --no-build 2>&1
        $summary = ($testOutput | Select-String -Pattern '(Passed!|Failed!)' | Select-Object -First 1).ToString()
        $passed = 0
        $failed = 0
        if ($summary -match 'Passed:\s+(\d+)')  { $passed = [int]$Matches[1] }
        if ($summary -match 'Failed:\s+(\d+)')  { $failed = [int]$Matches[1] }
        $status = if ($LASTEXITCODE -eq 0 -and $failed -eq 0) { 'PASS' } else { 'FAIL' }

        Write-Host "tests: passed=$passed failed=$failed" -ForegroundColor ($(if ($status -eq 'PASS') { 'Green' } else { 'Red' }))
        $results += [PSCustomObject]@{ Name = $name; Step = 'test'; Passed = $passed; Failed = $failed; Status = $status }
    }

    Write-Host ''
    Write-Host '=== Summary ===' -ForegroundColor Cyan
    $results | Format-Table -AutoSize | Out-Host

    $anyFail = $results | Where-Object { $_.Status -ne 'PASS' }
    if ($anyFail) {
        Write-Host 'Some combinations failed.' -ForegroundColor Red
        exit 1
    }

    Write-Host 'All combinations passed.' -ForegroundColor Green
    if (-not $KeepArtifacts) {
        Remove-Item -Recurse -Force $OutputRoot
    } else {
        Write-Host "Generated projects kept under $OutputRoot" -ForegroundColor DarkGray
    }
}
finally {
    Pop-Location
}
