# MH Achievement Manager

A specialized desktop management tool designed for **MHServerEmu** (Marvel Heroes Server Emulator) developers and content creators to inspect, edit, and localize game achievement data.

---

## 🚀 Key Features

* **Achievement Management & Editing:** 
  * Inspect, modify, and create new server achievements from scratch.
  * Export changes into standard patch files (`AchievementInfoMap*.json`).

* **Localization & Multi-Language Support:**
  * Full control over localized text strings (`AchievementStringMap*.json`).
  * Extends support for up to 8 additional languages simultaneously (including `ru_ru`, `fr_fr`, `de_de`, and others).

* **Client Data Integration:**
  * Displays icon asset names and resolves prototype definitions extracted directly from client archives.

---

## 📦 Client Assets Setup

To enable icon name resolution and full prototype inspection, link your local client data archive:

1. Launch **MH Achievement Manager**.
2. Navigate to `File` -> `Open PakFile...`
3. Locate and select the **`Calligraphy.sip`** archive file (typically located in `Marvel Heroes\Data\Game\`).

---

## 🛠 Tech Stack

* **Framework:** .NET 8.0 (Windows Forms)
* **Architecture:** Handcrafted C# UI components (Clean, Designer-less code)
* **Target:** Windows x64

---

## 📜 Credits & License

* **Author:** AlexBond
* **Ecosystem:** [MHServerEmu](https://github.com/MHServerEmu)
* Released under the **MIT License**.
