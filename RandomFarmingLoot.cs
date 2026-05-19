using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Random Farming Loot", "Evo", "1.3.0")]
    [Description("Replaces farming gathering with random loot.")]
    public class RandomFarmingLoot : RustPlugin
    {
        private Configuration config;
        private HashSet<ulong> optedOutPlayers = new HashSet<ulong>();
        private List<ItemDefinition> validItemDefinitions = new List<ItemDefinition>();
        private int totalItemsSalvaged = 0;

        private class Configuration
        {
            public bool EnabledGlobally = true;
            public bool EnableChatMessages = true;
            public bool ReplaceDefaultGathering = true;
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<Configuration>() ?? new Configuration();
            SaveConfig();
        }
        protected override void SaveConfig() => Config.WriteObject(config);

        private void OnServerInitialized()
        {
            CompileValidItemsPool();
            permission.RegisterPermission("randomfarmingloot.admin", this);
        }

        private void CompileValidItemsPool()
        {
            validItemDefinitions.Clear();
            
            // Aggressive blacklist
            string[] resourceBlacklist = { 
                "wood", "stones", "metal.ore", "sulfur.ore", "metal.fragments", "sulfur", "charcoal", 
                "lowgradefuel", "crudeoil", "cloth", "leather", "bones", "meat.raw", "fat.animal", 
                "mushroom", "cactusflesh", "apple", "fish", "meat.cooked"
            };

            var allItems = ItemManager.GetItemDefinitions();
            foreach (var item in allItems)
            {
                if (item == null || string.IsNullOrEmpty(item.shortname)) continue;
                if (resourceBlacklist.Contains(item.shortname)) continue;
                if (item.category == ItemCategory.Resources) continue; 
                
                if (item.category == ItemCategory.Weapon ||
                    item.category == ItemCategory.Medical ||
                    item.category == ItemCategory.Attire ||
                    item.category == ItemCategory.Tool ||
                    item.category == ItemCategory.Component ||
                    item.category == ItemCategory.Ammunition ||
                    item.category == ItemCategory.Items)
                {
                    validItemDefinitions.Add(item);
                }
            }
            Puts($"[Random Farming Loot] Compiled {validItemDefinitions.Count} items.");
        }

        private object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            BasePlayer player = entity as BasePlayer;
            if (player == null || !config.EnabledGlobally || optedOutPlayers.Contains(player.userID)) return null;

            GiveFarmingReward(player);

            return config.ReplaceDefaultGathering ? false : null;
        }

        private void OnCollectiblePickup(CollectibleEntity collectible, BasePlayer player)
        {
            if (player == null || !config.EnabledGlobally || optedOutPlayers.Contains(player.userID)) return;
            
            GiveFarmingReward(player);

            if (config.ReplaceDefaultGathering)
            {
                collectible.Kill();
            }
        }

        private void GiveFarmingReward(BasePlayer player)
        {
            if (validItemDefinitions.Count == 0) return;
            var randomDefinition = validItemDefinitions[UnityEngine.Random.Range(0, validItemDefinitions.Count)];
            
            Item item = ItemManager.Create(randomDefinition, 1, 0UL);
            if (item != null)
            {
                player.GiveItem(item);
                totalItemsSalvaged++;
                if (config.EnableChatMessages) 
                    player.ChatMessage($"<color=#32CD32>[Lucky Farmer]</color> You salvaged a bonus {randomDefinition.displayName.english}!");
            }
        }

        [ChatCommand("farmitems")]
        private void CmdToggleFarmItems(BasePlayer player, string command, string[] args)
        {
            if (optedOutPlayers.Contains(player.userID)) { optedOutPlayers.Remove(player.userID); player.ChatMessage("Bonus drops enabled."); }
            else { optedOutPlayers.Add(player.userID); player.ChatMessage("Bonus drops disabled."); }
        }

        [ChatCommand("farmconfig")]
        private void CmdFarmConfig(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "randomfarmingloot.admin")) return;
            OpenAdminGui(player);
        }

        private void OpenAdminGui(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "FarmAdminGui");
            var elements = new CuiElementContainer();

            elements.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95" },
                RectTransform = { AnchorMin = "0.35 0.3", AnchorMax = "0.65 0.7" },
                CursorEnabled = true
            }, "Overlay", "FarmAdminGui");

            elements.Add(new CuiLabel { Text = { Text = "FARMING LOOT CONFIG", FontSize = 18, Align = TextAnchor.MiddleCenter }, RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 1" } }, "FarmAdminGui");

            // System Status
            string status = config.EnabledGlobally ? "ENABLED" : "DISABLED";
            elements.Add(new CuiButton { Button = { Command = "farm_toggle_global", Color = "0.2 0.4 0.2 1" }, RectTransform = { AnchorMin = "0.1 0.7", AnchorMax = "0.9 0.8" }, Text = { Text = $"System: {status}", Align = TextAnchor.MiddleCenter } }, "FarmAdminGui");

            // Mode Toggle
            string mode = config.ReplaceDefaultGathering ? "REPLACE ALL" : "BONUS ONLY";
            elements.Add(new CuiButton { Button = { Command = "farm_toggle_mode", Color = "0.2 0.2 0.4 1" }, RectTransform = { AnchorMin = "0.1 0.55", AnchorMax = "0.9 0.65" }, Text = { Text = $"Mode: {mode}", Align = TextAnchor.MiddleCenter } }, "FarmAdminGui");

            // Chat Toggle (NEW)
            string chatStatus = config.EnableChatMessages ? "CHAT: ON" : "CHAT: OFF";
            elements.Add(new CuiButton { Button = { Command = "farm_toggle_chat", Color = "0.4 0.4 0.2 1" }, RectTransform = { AnchorMin = "0.1 0.4", AnchorMax = "0.9 0.5" }, Text = { Text = chatStatus, Align = TextAnchor.MiddleCenter } }, "FarmAdminGui");

            elements.Add(new CuiLabel { Text = { Text = $"Total Salvaged: {totalItemsSalvaged}", FontSize = 14, Align = TextAnchor.MiddleCenter }, RectTransform = { AnchorMin = "0 0.25", AnchorMax = "1 0.35" } }, "FarmAdminGui");

            elements.Add(new CuiButton { Button = { Command = "farm_close_gui", Color = "0.6 0.2 0.2 1" }, RectTransform = { AnchorMin = "0.4 0.1", AnchorMax = "0.6 0.2" }, Text = { Text = "Close", Align = TextAnchor.MiddleCenter } }, "FarmAdminGui");

            CuiHelper.AddUi(player, elements);
        }

        [ConsoleCommand("farm_toggle_global")]
        private void CcmdToggleGlobal(ConsoleSystem.Arg arg) { config.EnabledGlobally = !config.EnabledGlobally; SaveConfig(); OpenAdminGui(arg.Player()); }

        [ConsoleCommand("farm_toggle_mode")]
        private void CcmdToggleMode(ConsoleSystem.Arg arg) { config.ReplaceDefaultGathering = !config.ReplaceDefaultGathering; SaveConfig(); OpenAdminGui(arg.Player()); }

        [ConsoleCommand("farm_toggle_chat")]
        private void CcmdToggleChat(ConsoleSystem.Arg arg) { config.EnableChatMessages = !config.EnableChatMessages; SaveConfig(); OpenAdminGui(arg.Player()); }

        [ConsoleCommand("farm_close_gui")]
        private void CcmdCloseGui(ConsoleSystem.Arg arg) { CuiHelper.DestroyUi(arg.Player(), "FarmAdminGui"); }
    }
}