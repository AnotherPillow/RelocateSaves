using System;
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

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Program), nameof(StardewValley.Program.GetSavesFolder)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Program_GetSavesFolder_Prefix))
            );
        }
        private static bool Program_GetSavesFolder_Prefix(ref string __result)
        {
            if (Config.NewSavePath is null) return true;
            Directory.CreateDirectory(Config.NewSavePath);
            __result = Config.NewSavePath;
            return false;
        }
    }
}