
using TabletopTweaks.Core.Config;

namespace TabletopTweaks.MythicReoworks.Config {
    public class FlankingSettings : IUpdatableSettings {
        public static readonly int FlankingAngleDefault = 130;
        public static readonly int FlankingAngleImprovedDefault = 80;

        public bool DisableAllFlankingMechanics = false;
        public bool ApplyFlankingRulesToEnemies = true;
        public bool GiveEnemiesGangUp = true;
        public int FlankingAngle = FlankingAngleDefault;
        public int FlankingAngleImproved = FlankingAngleImprovedDefault;
        public bool NewSettingsOffByDefault = false;
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
