using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.MythicReoworks.Config;
using static UnityModManagerNet.UnityModManager;

namespace TabletopTweaks.Flanking.ModLogic {
    internal class ModContextTTTFlanking : ModContextBase {
        public FlankingSettings Settings;

        public ModContextTTTFlanking(ModEntry ModEntry) : base(ModEntry) {
            LoadAllSettings();
#if DEBUG
            Debug = true;
#endif
        }
        public override void LoadAllSettings() {
            LoadSettings("Settings.json", "TabletopTweaks.Flanking.Config", ref Settings);
            LoadBlueprints("TabletopTweaks.Flanking.Config", Main.TTTContext);
            LoadLocalization("TabletopTweaks.Flanking.Localization");
        }
        public override void AfterBlueprintCachePatches() {
            base.AfterBlueprintCachePatches();
            if (Debug) {
                Blueprints.RemoveUnused();
                SaveSettings(BlueprintsFile, Blueprints);
                ModLocalizationPack.RemoveUnused();
                SaveLocalization(ModLocalizationPack);
            }
        }
        public override void SaveAllSettings() {
            base.SaveAllSettings();
            SaveSettings("Settings.json", Settings);
        }
    }
}
