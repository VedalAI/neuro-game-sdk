# Neuro HTML Game Runner

## Usage
- Execute neuro_html_game_runner.bat next to folders containing games with .html entrypoints. Only the single script is required.
- You can select what game to play.

## What it does
- First, it downloads static-web-server if it are not already present.
- It writes the environment variable NEURO_SDK_WS_URL, if it exists, to a file `/$env/NEURO_SDK_WS_URL` relative to the game directory.
- It uses static-web-server to serve the files for the selected game on localhost:8787.
- The game can then be played safely from localhost:8787 in the browser.

## Why?
- HTML games are safe to run due to the browser sandbox. However, they require a local server to run in order serve game files.
- Bundling a local server with the game is dangerous, as it requires executing untrusted code, subverting the security model of the browser.
- This script can be used with any HTML game to run it locally safely.

## Dependencies
- https://github.com/static-web-server