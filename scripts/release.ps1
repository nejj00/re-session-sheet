<#
.SYNOPSIS
    Builds, exports, zips, and publishes a GitHub Release for RE Session Sheet.

.DESCRIPTION
    Run this manually whenever you want to ship a new prototype build. It does NOT commit
    or stage any of your changes for you - commit whatever you want included before running this.
    It DOES push your current branch (so the release's tag points at something that's really on
    GitHub) and create the release itself via the GitHub CLI.

.PARAMETER Version
    The release tag, e.g. "v0.2.0-prototype". Used as the git tag and in the zip filename.
    If the tag doesn't exist yet, gh creates it automatically pointing at your current commit.

.PARAMETER Notes
    Release notes shown on the GitHub release page (e.g. a short changelog).

.PARAMETER Stable
    Pass this switch to publish as a full release. Without it, releases are marked
    "pre-release" on GitHub, which fits a prototype build.

.PARAMETER GodotExe
    Path to the Godot Mono editor executable, if it's not on PATH as "godot".

.EXAMPLE
    .\scripts\release.ps1 -Version "v0.2.0-prototype" -Notes "Reworked XP costs, added session log deletion."
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Notes = "Automated prototype build.",

    [switch]$Stable,

    [string]$GodotExe = "godot"
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$PresetName = "Resident Evil Session Sheet Prototype"
$ExportDir = Join-Path $RepoRoot "builds\windows"
$ExportExe = Join-Path $ExportDir "RESessionSheet.exe"
$ZipPath = Join-Path $RepoRoot "builds\RESessionSheet-$Version-windows.zip"

function Assert-CommandExists {
    param([string]$Name, [string]$InstallHint)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Write-Error "'$Name' was not found on PATH. $InstallHint"
        exit 1
    }
}

Write-Host "== Checking prerequisites ==" -ForegroundColor Cyan
Assert-CommandExists -Name "dotnet" -InstallHint "Install the .NET SDK from https://dotnet.microsoft.com/download."
Assert-CommandExists -Name $GodotExe -InstallHint "Install Godot 4.7 (Mono), or pass -GodotExe <path-to-exe>. If you just installed it, open a NEW terminal window first - PATH changes don't apply to already-open shells."
Assert-CommandExists -Name "gh" -InstallHint "Install with: winget install GitHub.cli (then open a new terminal window)."

gh auth status *>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "GitHub CLI isn't authenticated yet. Run 'gh auth login' once, then re-run this script."
    exit 1
}

Write-Host "== Checking git working tree ==" -ForegroundColor Cyan
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Warning "You have uncommitted changes. The build will include them, but the git tag won't capture them unless you commit first."
    $confirm = Read-Host "Continue anyway? (y/N)"
    if ($confirm -notmatch '^[Yy]') {
        Write-Host "Aborted. Commit/push your changes and re-run."
        exit 1
    }
}

Write-Host "== Building ==" -ForegroundColor Cyan
dotnet build RESessionSheet.csproj
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed - fix compile errors before exporting." }

Write-Host "== Cleaning previous export ==" -ForegroundColor Cyan
Remove-Item -Path "$ExportDir\RESessionSheet.exe*" -ErrorAction SilentlyContinue
Remove-Item -Path "$ExportDir\data_*" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "== Exporting release build via Godot ==" -ForegroundColor Cyan
& $GodotExe --headless --path $RepoRoot --export-release $PresetName $ExportExe
if ($LASTEXITCODE -ne 0) { throw "Godot export failed - see output above." }
if (-not (Test-Path $ExportExe)) { throw "Export did not produce $ExportExe - check the Godot output above." }

Write-Host "== Zipping build ==" -ForegroundColor Cyan
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path "$ExportDir\*" -DestinationPath $ZipPath -CompressionLevel Optimal

Write-Host "== Pushing current branch ==" -ForegroundColor Cyan
git push
if ($LASTEXITCODE -ne 0) { throw "git push failed - resolve manually, then re-run." }

Write-Host "== Creating GitHub Release $Version ==" -ForegroundColor Cyan
$releaseArgs = @($Version, $ZipPath, "--title", $Version, "--notes", $Notes)
if (-not $Stable) { $releaseArgs += "--prerelease" }

gh release create @releaseArgs
if ($LASTEXITCODE -ne 0) { throw "gh release create failed - see output above." }

$repoUrl = gh repo view --json url -q .url
Write-Host ""
Write-Host "Done! Release published: $repoUrl/releases/tag/$Version" -ForegroundColor Green
