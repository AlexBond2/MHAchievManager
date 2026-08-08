# MH Achievement Manager

A Windows desktop utility for inspecting, editing, and localizing achievement data for the [MHServerEmu](https://github.com/Crypto137/MHServerEmu) project.

## Features

* **Achievement Data Management:** Browse, edit, and create achievement definitions, exporting patch files (`AchievementInfoMap*.json`).
* **Localization Support:** Manage text mappings (`AchievementStringMap*.json`) with support for up to 8 languages (including `ru_ru`, `fr_fr`, `de_de`, etc.).
* **Client Archive Integration:** Resolve icon assets and inspect prototype definitions directly from game archives.

## Setup & Client Assets

To enable icon preview and prototype resolution:

1. Open **MH Achievement Manager**.
2. Go to **File -> Open PakFile...**
3. Select `Calligraphy.sip` (located in your game installation under `Marvel Heroes\Data\Game\`).

## Requirements

* Windows (x64)
* .NET 8.0 Runtime

## Credits & License

* **Author:** AlexBond
* **Dependencies & Integrations:** 
  * Designed for use with [MHServerEmu](https://github.com/Crypto137/MHServerEmu).
  * Uses **OpenCalligraphy.Core** from [Crypto137/OpenCalligraphy](https://github.com/Crypto137/OpenCalligraphy) for client archive handling.

This project is licensed under the [MIT License](LICENSE).
