using HarmonyLib;
using TabletopTweaks.Core.MechanicsChanges;
using TabletopTweaks.Core.Utilities;
using TabletopTweaks.Flanking.MechanicsChanges;
using TabletopTweaks.Flanking.ModLogic;
using UnityModManagerNet;

namespace TabletopTweaks.Flanking {
    static class Main {
        public static bool Enabled;
        public static ModContextTTTFlanking TTTContext;
        static bool Load(UnityModManager.ModEntry modEntry) {
            var harmony = new Harmony(modEntry.Info.Id);
            TTTContext = new ModContextTTTFlanking(modEntry);
            TTTContext.LoadAllSettings();
            TTTContext.ModEntry.OnSaveGUI = OnSaveGUI;
            TTTContext.ModEntry.OnGUI = UMMSettingsUI.OnGUI;
            harmony.PatchAll();
            PostPatchInitializer.Initialize(TTTContext);
            if (!TTTContext.Settings.DisableAllFlankingMechanics) {
                FlankingMechanics.SetFlankingProvider(TTTContext, new TTTFlankingProvider());
            }
            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            TTTContext.SaveAllSettings();
        }
    }
}
