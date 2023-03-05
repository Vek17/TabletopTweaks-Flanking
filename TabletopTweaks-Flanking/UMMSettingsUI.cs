using TabletopTweaks.Core.UMMTools;
using TabletopTweaks.MythicReoworks.Config;
using UnityModManagerNet;

namespace TabletopTweaks.Flanking {
    internal static class UMMSettingsUI {
        private static int selectedTab;
        public static void OnGUI(UnityModManager.ModEntry modEntry) {
            UI.AutoWidth();
            UI.TabBar(ref selectedTab,
                    () => UI.Label("SOME SETTINGS WILL NOT BE UPDATED UNTIL YOU RESTART YOUR GAME.".yellow().bold()),
                    new NamedAction("Flanking Settings", () => SettingsTabs.Settings()),
                    new NamedAction("Added Feats", () => SettingsTabs.AddedFeats())
            );
        }
    }

    static class SettingsTabs {
        public static void Settings() {
            var Settings = Main.TTTContext.Settings;
            UI.Div(0, 15);
            using (UI.VerticalScope()) {
                UI.Toggle("Disable All Flanking Mechanics".bold(), ref Settings.DisableAllFlankingMechanics);
                UI.Toggle("Apply New Flanking to Enemies".bold(), ref Settings.ApplyFlankingRulesToEnemies);
                UI.Label("Flanking Angle".green().bold());
                UI.Slider(ref Settings.FlankingAngle, 0, 180, FlankingSettings.FlankingAngleDefault);
                UI.Label("Improved Outflank Flanking Angle".green().bold());
                UI.Slider(ref Settings.FlankingAngleImproved, 0, Settings.FlankingAngle, FlankingSettings.FlankingAngleImprovedDefault);
            }
        }
        public static void AddedFeats() {
            var TabLevel = SetttingUI.TabLevel.Zero;
            var Settings = Main.TTTContext.Settings;
            UI.Div(0, 15);
            using (UI.VerticalScope()) {
                UI.Toggle("New Settings Off By Default".bold(), ref Settings.NewSettingsOffByDefault);
                UI.Space(25);
                SetttingUI.SettingGroup("Added Feats", TabLevel, Settings.AddedFeats);
            }
        }
    }
}
