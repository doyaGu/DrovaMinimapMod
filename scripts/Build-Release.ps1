[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$GameRoot,

    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if ([string]::IsNullOrWhiteSpace($GameRoot)) {
    $GameRoot = Split-Path -Parent $projectRoot
}

$resolvedGameRoot = (Resolve-Path $GameRoot).Path
$projectPath = Join-Path $projectRoot 'DrovaMinimapMod.csproj'
$readmePath = Join-Path $projectRoot 'README.md'
$changelogPath = Join-Path $projectRoot 'CHANGELOG.md'
$licensePath = Join-Path $projectRoot 'LICENSE'

if (-not (Test-Path -LiteralPath $licensePath)) {
    throw "Release packaging requires a LICENSE file: $licensePath"
}

[xml]$projectXml = Get-Content -Raw $projectPath
$version = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'The project Version property is required for release packaging.'
}

$buildArguments = @(
    'build',
    $projectPath,
    '-c', 'Release',
    '-nologo',
    "-p:GameRoot=$resolvedGameRoot",
    "-p:Deploy=$($Deploy.IsPresent.ToString().ToLowerInvariant())"
)

& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$dllPath = Join-Path $projectRoot 'bin\Release\net6.0\DrovaMinimapMod.dll'
if (-not (Test-Path $dllPath)) {
    throw "Release DLL was not produced: $dllPath"
}

$distPath = Join-Path $projectRoot 'dist'
New-Item -ItemType Directory -Path $distPath -Force | Out-Null
$archivePath = Join-Path $distPath "DrovaMinimapMod-$version.zip"

Compress-Archive -LiteralPath @($dllPath, $readmePath, $changelogPath, $licensePath) -DestinationPath $archivePath -Force
Write-Output "Created $archivePath"
