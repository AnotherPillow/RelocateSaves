using System;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace RelocateSaves
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModEntry.Config = this.Helper.ReadConfig<ModConfig>();
            var harmony = new Harmony(this.ModManifest.UniqueID);
            
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Program), nameof(StardewValley.Program.GetSavesFolder)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Program_GetSavesFolder_Prefix))
            );
        }
        private static bool Program_GetSavesFolder_Prefix(ref string __result)
        {
            if (String.IsNullOrEmpty(Config.NewSavePath)) return true;
            
            Directory.CreateDirectory(Config.NewSavePath);
            __result = Config.NewSavePath;
            
            return false;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => ModEntry.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(ModEntry.Config)
            );

            // add some config options
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "New Save File Path",
                getValue: () => ModEntry.Config.NewSavePath ?? "",
                setValue: value => ModEntry.Config.NewSavePath = value
            );
        }
    }
}