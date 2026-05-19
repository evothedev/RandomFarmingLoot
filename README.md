# Random Farming Loot

A high-performance Rust server plugin that transforms the standard resource gathering experience by introducing an engaging, random loot reward system.

Players harvesting wood, mining ore nodes, or gathering ground resources have a configurable chance to uncover random bonus items from the server's dynamic item database.

## 🚀 Key Features

-   **Organic Interaction:** Integrates seamlessly with Rust’s gathering hooks (`OnDispenserGather`, `OnCollectiblePickup`, and `OnGrowableGathered`).
-   **Hardcore Mode (Optional):** Can be configured to _replace_ default resource gathering (wood/stone/ore) with random item rewards, providing a unique survival challenge.
-   **Smart Stacking Engine:** Intelligently handles stack limits, ensuring that non-stackable items (like weapons or attire) drop in appropriate quantities.
-   **Player Autonomy:** Players can use `/farmitems` to toggle the farming reward system and associated chat notifications for themselves.
-   **High-Fidelity Admin Dashboard:** An in-game administrative control panel (`/farmconfig`) to manage drop rates, system status, and notifications in real-time.

## ⚙️ Configuration (`RandomFarmingLoot.json`)

```
{
  "EnabledGlobally": true,
  "EnableChatMessages": true,
  "ReplaceDefaultGathering": true,
  "DropChancePercentage": 2.0
}
```

-   **`EnabledGlobally`**: Master toggle for the farming reward system.
-   **`EnableChatMessages`**: Toggles whether players see chat notifications for found loot.
-   **`ReplaceDefaultGathering`**: If `true`, stops the gathering of standard resources (wood/stone/ore) and replaces them with random loot.
-   **`DropChancePercentage`**: The chance (0-100) that a harvest action triggers a loot drop.

## 🛠️ Installation & Permissions

### Installation

1.  Place `RandomFarmingLoot.cs` into your server's `carbon/plugins/` (or `oxide/plugins/`) directory.
2.  The plugin will automatically generate the configuration file on first launch.

### Admin Setup (Carbon Console)

To open the admin dashboard, grant the necessary permission:

```
c.grant user <username_or_steamid> randomfarmingloot.admin
```

## 💬 Commands

| Command | Type | Target | Description |
| --- | --- | --- | --- |
| `/farmitems` | Chat | Player | Toggles random farming rewards ON/OFF. |
| `/farmconfig` | Chat | Admin | Opens the farming admin control panel. |
