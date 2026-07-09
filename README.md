# RE Session Sheet

A desktop character sheet for a custom Resident-Evil-inspired tabletop game, built with Godot 4 (Mono/C#).

## Project layout

- `src/Domain/` — pure C# game rules and data model (no Godot dependencies; testable with plain `dotnet test`).
  - `Model/` — character data (attributes, skills, health, inventory, etc.)
  - `Rules/` — XP costs, wound track math, herb recipes
  - `Serialization/` — JSON save format
- `src/App/` — the `GameState` autoload (current character, save/load, cross-panel signals)
- `src/UI/` — Godot scenes and C# UI code (roster screen, character sheet panels, reusable components, theme)
- `tests/RESessionSheet.Tests/` — xUnit tests for the Domain layer
- `reference/` — the original rulebook spreadsheet, an old HTML character sheet, and prior character/session notes used as design source material (not part of the app)
- `data/catalogs/` — reserved for future structured content (weapons, merits/flaws, talents)

## Requirements

- [Godot 4.7 (Mono/.NET build)](https://godotengine.org/download) — must be the **.NET** edition, not the standard build, since this project uses C#.
- .NET SDK 8.0 or later (for building/testing outside the editor).

## Running in the editor

Open `project.godot` in Godot 4.7 (Mono) and press Play. The entry scene is `src/UI/Main.tscn`.

## Building and running from the command line

```sh
dotnet build RESessionSheet.csproj
```

## Running tests

```sh
dotnet test tests/RESessionSheet.Tests/RESessionSheet.Tests.csproj
```

## Exporting a standalone Windows build

1. Install the matching export templates in the Godot editor (**Editor → Manage Export Templates**), or download them directly:
   `https://github.com/godotengine/godot/releases/download/4.7-stable/Godot_v4.7-stable_mono_export_templates.tpz`
   and extract into `%APPDATA%\Godot\export_templates\4.7.stable.mono\`.
2. From the command line:
   ```sh
   godot --headless --path . --export-release "Windows Desktop" "builds/windows/RESessionSheet.exe"
   ```
   (use `--export-debug` instead for a debug build with more diagnostics)
3. The `builds/windows/` folder will contain `RESessionSheet.exe` plus a `data_RESessionSheet_windows_x86_64/` folder — both are required to run the app; copy the whole folder to distribute it.

The export preset itself lives in `export_presets.cfg` (tracked in git); only `export_credentials.cfg`, if one is ever created for code signing, should stay untracked.

## Save data location

Characters are saved as individual JSON files under Godot's `user://characters/` directory, which on Windows resolves to:

```
%APPDATA%\Godot\app_userdata\RE Session Sheet\characters\
```

## Status

V1 covers the core play loop: bio/backstory/archetype tags, attributes and skills (XP-gated dot ratings with refund-on-lower), virtues, XP tracking, the 7-tier/14-box wound track with strain and herb-combo healing, action points, a structured inventory list, a session log, and freeform merits/flaws/traits/backgrounds.

Not yet built (by design, later phases): the drag-and-drop grid inventory, structured weapon/merit/talent catalogs with auto-applied mechanical effects, and a legacy importer for the old HTML sheet's JSON export.
