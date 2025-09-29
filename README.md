The original idea (might be missing some things that we already have or contain ideas that have already been discarded)
https://docs.google.com/document/d/1uLigNKKskPH23ATBY_9Pc3H4F7HDMB9xZB0QT89EGkQ/edit?tab=t.0

The excel calulator (this is up to date)
https://docs.google.com/spreadsheets/d/1p5RMnwihZ-BZRN9nikhw7DapNMyggghfLIrUcNZh3oc/edit?gid=0#gid=0

Test steps (this is up to date)
https://docs.google.com/document/d/16_NsLGFQD7lW6_dboALFrIQPr8tDoUhsiHXrmrZEHsg/edit?tab=t.0

# Linting
## How to ensure correct formatting of my code?
1. Run the following command in the root of the project: `dotnet format ClickForLife.sln --verify-no-changes`
2. Look through the warning messages and fix all issues.

However, if you already have the C# plugin installed, all linting issues should already be highlighted in accordance with the .editorconfig file.

# Save/Load
Encrypted save file location on Windows: `C:\Users\[Username]\AppData\LocalLow\DefaultCompany\ClickForLife\savefile.bin`

The save can be deleted manually or through the context menu option found on the SaveDataContainer SO that is at `Assets/ScriptableObjects/Variables/SaveDataContainer`

For debugging purposes there is also a toggle button in-game to save to savefile.json instead in unencrypted json.

# Running WebGL build locally
- Use Unity to create the WebGL build in a folder.
- Go to that folder and run `python <ProjectRoot>\local-server.py`
