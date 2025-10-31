# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Editor Icons

> Quick overview: Search, preview, copy, and export Unity’s built‑in editor icon textures. Includes a helper API to fetch icons programmatically by name or enum.

A tiny tool window to browse Unity’s internal editor icons. Filter by name, switch between small/big sets, click to inspect, copy the icon reference, or export a single icon (or all) as PNGs. Also exposes a simple utility to get `GUIContent`/`Texture2D` for an icon in your own editor scripts.

![screenshot](Documentation/Screenshot.png)

## Features
- Fast icon browser
  - Filter by name (string contains)
  - Toggle Small/Big icon sets for faster scanning
  - Grid layout with responsive sizing
- Inspector footer for the selected icon
  - Preview at best‑fit size
  - Info: dimensions, total icon count, indicates “Pro skin” variant (name starts with `d_`)
  - Copy ready‑to‑paste call: `EditorGUIUtility.IconContent("…")`
  - Buttons: Copy to clipboard, Save icon to file…
- Export tools
  - Save single icon as PNG
  - Save all known icons to a chosen folder (skips existing files)
- Programmatic utilities
  - `EditorIconUtilities.GetContent(EditorIconNames name)` → `GUIContent`
  - `EditorIconUtilities.GetTexture(EditorIconNames name)` → `Texture2D`
  - `EditorIconUtilities.GetReferenceByName(EditorIconNames name)` → string reference for IconContent

## Requirements
- Unity Editor 6000.0+ (Editor‑only; no runtime code)
- Optional: UnityEssentials Editor Window Drawer module (the window uses `EditorWindowDrawer` for layout)

## Usage

Open the browser window:
- Menu: Tools → Editor Icons
- Shortcut: Ctrl/Cmd + E

In the window:
- Use the toolbar search to filter by icon name
- Toggle Small/Big to switch the dataset
- Click an icon to select; the footer shows details and actions
- Use “Save all icons to folder…” to export every known icon once

### Programmatic use in editor scripts
Fetch a `GUIContent` you can draw directly:

```csharp
using UnityEditor;
using UnityEngine;
using UnityEssentials;

public class IconUsageExample
{
    [MenuItem("Tools/Icon Usage Example")] private static void Demo()
    {
        // From enum (preferred when available)
        var gc1 = EditorIconUtilities.GetContent(EditorIconNames.AssetStore_Icon /* example value */);

        // From string reference (as shown in the window footer)
        var gc2 = EditorGUIUtility.IconContent("Folder Icon");

        // Get Texture2D only
        Texture2D tex = EditorIconUtilities.GetTexture(EditorIconNames.Folder_Icon /* example value */);

        // Draw something quickly
        var rect = new Rect(10, 10, 32, 32);
        EditorGUI.DrawPreviewTexture(rect, tex);
    }
}
```

Notes
- The enum `EditorIconNames` maps into a large internal reference array; entries may vary by Unity version/skin
- Pro skin icons typically start with `d_` (e.g., `d_Folder Icon`)

## Notes and Limitations
- Editor‑only: APIs return null in player
- Icon availability varies across Unity versions and skins
- The “all icons” list is curated in this package; it may not include every possible internal icon in every version
- Exported PNGs are raw copies of the editor textures (sRGB); licensing is subject to Unity’s terms

## Files in This Package
- `Editor/EditorIconSearch.cs` – Core logic (collection, filtering, export single/all)
- `Editor/EditorIconSearchEditor.cs` – Tool window UI (menu, toolbar, grid, footer)
- `Runtime/EditorIconUtilities.cs` – Helper API (get reference string, GUIContent, Texture2D)
- `Runtime/UnityEssentials.EditorIcons.asmdef` – Assembly definition
- `package.json` – Package manifest metadata

## Tags
unity, unity-editor, icons, editor icons, gui, guicontent, texture2d, search, export, pro-skin, editor-tool
