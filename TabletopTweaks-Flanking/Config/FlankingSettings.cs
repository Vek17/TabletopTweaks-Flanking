
using TabletopTweaks.Core.Config;

namespace TabletopTweaks.MythicReoworks.Config {
    public class FlankingSettings : IUpdatableSettings {
        public static int FlankingAngleDefault = 130;
        public static int FlankingAngleImprovedDefault = 80;
        public bool NewSettingsOffByDefault = false;
        public bool DisableAllFlankingMechanics = false;
        public int FlankingAngle = FlankingAngleDefault;
        public int FlankingAngleImproved = FlankingAngleImprovedDefault;
        public bool ApplyFlankingRulesToEnemies = false;
        public SettingGroup AddedFeats = new SettingGroup();

        public void Init() {
        }

        public void OverrideSettings(IUpdatableSettings userSettings) {
            var loadedSettings = userSettings as FlankingSettings;
            FlankingAngle = loadedSettings.FlankingAngle;
            FlankingAngleImproved = loadedSettings.FlankingAngleImproved;
            DisableAllFlankingMechanics = loadedSettings.DisableAllFlankingMechanics;
            NewSettingsOffByDefault = loadedSettings.NewSettingsOffByDefault;
            AddedFeats.LoadSettingGroup(loadedSettings.AddedFeats, NewSettingsOffByDefault);
        }
    }
}
