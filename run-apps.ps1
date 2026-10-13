param(
    [ValidateSet('Api', 'Frontend')]
    [string]$App
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = $PSScriptRoot

if ($App) {
    switch ($App) {
        'Api' {
            Set-Location -LiteralPath $repositoryRoot
            & dotnet run --project (Join-Path $repositoryRoot 'backend\Pokemon.Api') -- --urls 'http://localhost:5000'
            exit $LASTEXITCODE
        }
        'Frontend' {
            Set-Location -LiteralPath (Join-Path $repositoryRoot 'frontend')
            & npm.cmd start
            exit $LASTEXITCODE
        }
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET SDK is required. Install .NET 8 and try again.'
}

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw 'Node.js and npm are required. Install Node.js and try again.'
}

$frontendDirectory = Join-Path $repositoryRoot 'frontend'
if (-not (Test-Path (Join-Path $frontendDirectory 'node_modules'))) {
    Write-Host 'Installing frontend dependencies...'
    Push-Location $frontendDirectory
    try {
        & npm.cmd ci
        if ($LASTEXITCODE -ne 0) {
            throw "npm ci failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

$powershellPath = (Get-Process -Id $PID).Path
foreach ($appName in @('Api', 'Frontend')) {
    $childCommand = "& '$($PSCommandPath.Replace("'", "''"))' -App $appName"
    $encodedCommand = [Convert]::ToBase64String(
        [Text.Encoding]::Unicode.GetBytes($childCommand)
    )

    Start-Process -FilePath $powershellPath `
        -ArgumentList @('-NoExit', '-ExecutionPolicy', 'Bypass', '-EncodedCommand', $encodedCommand) `
        -WorkingDirectory $repositoryRoot
}

Write-Host 'API:      http://localhost:5000/api/pokemon'
Write-Host 'Frontend: http://localhost:4200'
Write-Host 'Close each app window to stop that app.'
