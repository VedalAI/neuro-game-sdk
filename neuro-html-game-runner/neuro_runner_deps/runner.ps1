# This script uses https://github.com/static-web-server/ to serve webfiles on localhost.
# Please choose the correct distribution for your OS and architecture.

param(
    [string]$port = "8787"
)

# [Environment]::SetEnvironmentVariable("NEURO_SDK_WS_URL", "localhost:8000", 'Process')

$exePath = "neuro_runner_deps\static-web-server.exe"

# Function to display menu and handle selection
function Show-Menu {
    param (
        [string]$Title,
        [array]$Options
    )
    
    $selectedIndex = 0
    $pressed = $null
    $maxLength = ($Options | Measure-Object -Property Length -Maximum).Maximum
    
    do {
        Clear-Host
        Write-Host $Title -ForegroundColor Cyan
        Write-Host

        for ($i = 0; $i -lt $Options.Count; $i++) {
            if ($i -eq $selectedIndex) {
                Write-Host ("> " + $Options[$i].PadRight($maxLength)) -ForegroundColor Green
            } else {
                Write-Host ("  " + $Options[$i].PadRight($maxLength)) -ForegroundColor Gray
            }
        }

        $pressed = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

        switch ($pressed.VirtualKeyCode) {
            38 { # Up arrow
                if ($selectedIndex -gt 0) {
                    $selectedIndex--
                }
                break
            }
            40 { # Down arrow
                if ($selectedIndex -lt ($Options.Count - 1)) {
                    $selectedIndex++
                }
                break
            }
        }
    } while ($pressed.VirtualKeyCode -ne 13) # Enter key

    return $selectedIndex
}


# Scan for HTML game folders
# Write-Host "Scanning for HTML games..." -ForegroundColor Yellow
$folders = Get-ChildItem -Directory | Where-Object {
    Test-Path (Join-Path $_.FullName "*.html")
}

if ($folders.Count -eq 0) {
    Write-Host "`nNo HTML games found in neighboring directories." -ForegroundColor Red
    Write-Host "Make sure game folders contain .html files."
    Read-Host "Press Enter to exit"
    exit
}

# Build menu options for folders
$folderOptions = @()
foreach ($folder in $folders) {
    $htmlFiles = Get-ChildItem -Path $folder.FullName -Filter "*.html"
    if ($htmlFiles.Count -eq 1) {
        $folderOptions += "$($folder.Name) ($($htmlFiles[0].Name))"
    } else {
        $folderOptions += "$($folder.Name)"
    }
}

# Show folder selection menu
Write-Host
$title = "These folders look like they might contain HTML games. Which do you want to play?"
$folderIndex = Show-Menu -Title $title -Options $folderOptions

$selectedFolder = $folders[$folderIndex]
$htmlFiles = Get-ChildItem -Path $selectedFolder.FullName -Filter "*.html"

# If multiple HTML files, show second menu
if ($htmlFiles.Count -gt 1) {
    Write-Host
    $title = "The folder $($selectedFolder.Name) has multiple HTML files, which one is the game located at?"
    $htmlOptions = $htmlFiles | ForEach-Object { $_.Name }
    $fileIndex = Show-Menu -Title $title -Options $htmlOptions
    $selectedFile = $htmlFiles[$fileIndex].Name
} else {
    $selectedFile = $htmlFiles[0].Name
}

Write-Host
# Write the neuro sdk variable if present
if (Test-Path env:NEURO_SDK_WS_URL) {
    Write-Host "NEURO_SDK_WS_URL = $env:NEURO_SDK_WS_URL"
    Write-Host "Writing to game directory..."
    New-Item -ItemType Directory -Force -Path "$selectedFolder\`$env\"
    $env:NEURO_SDK_WS_URL | Out-File -FilePath "$selectedFolder\`$env\NEURO_SDK_WS_URL"
} else {
   Write-Host "NEURO_SDK_WS_URL environment variable is not set."
}

$config = @"
[advanced]
[[advanced.headers]]
source = "**/*"
headers.Cross-Origin-Opener-Policy = "same-origin"
headers.Access-Control-Allow-Methods = "GET, POST, OPTIONS"
headers.Access-Control-Allow-Headers = "Content-Type"
headers.Cache-Control = "no-cache, no-store, must-revalidate"
headers.Pragma = "no-cache"
headers.Expires = "0"
[[advanced.headers]]
source = "**/*.{gz}"
headers.Content-Encoding = "gzip"
[[advanced.headers]]
source = "**/*.{br}"
headers.Content-Encoding = "br"
"@

Set-Content -Path "neuro_runner_deps\server_config.toml" -Value $config

# Start the server
Write-Host
Write-Host "[PLACEHOLDER] Starting local server..." -ForegroundColor Yellow
Write-Host "Game will be running at http://localhost:$port/$selectedFile (Ctrl+Click to open)" -ForegroundColor Cyan

# Attempt to open the browser automatically
Start-Process "http://localhost:$port/$selectedFile"

& $exePath "--port", $port, "--root", $selectedFolder, "-w", "neuro_runner_deps\server_config.toml"


