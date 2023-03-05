using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using TabletopTweaks.Core.NewComponents;
using TabletopTweaks.Core.NewUnitParts;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.NewContent.Feats.TeamworkFeats {
    internal class PackFlanking {
        public static void AddPackFlanking() {
            var CombatExpertiseFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");
            var Icon = CombatExpertiseFeature.Icon;

            var PackFlanking = Helpers.CreateBlueprint<BlueprintFeature>(TTTContext, "PackFlanking", bp => {
                bp.SetName(TTTContext, "Pack Flanking");
                bp.SetDescription(TTTContext, "You and your companion creature are adept at fighting together against foes.\n" +
                    "Benefit: When you and your companion creature have this feat, " +
                    "and you both threaten the same opponent, you are both considered to be flanking that opponent, " +
                    "regardless of your actual positioning.\n" +
                    "Normal: You must be positioned opposite an ally to flank an opponent.");
                bp.m_Icon = Icon;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.TeamworkFeat, FeatureGroup.CombatFeat };
                bp.AddComponent<AddCustomMechanicsFeature>(c => {
                    c.Feature = UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.PackFlanking;
                });
                bp.AddComponent<PrerequisiteStatValue>(c => {
                    c.Stat = StatType.Intelligence;
                    c.Value = 13;
                });
                bp.AddPrerequisiteFeature(CombatExpertiseFeature);
                bp.AddPrerequisite<PrerequisitePet>(p => {
                    p.Group = Prerequisite.GroupType.Any;
                });
                bp.AddPrerequisite<PrerequisiteIsPet>(p => {
                    p.Group = Prerequisite.GroupType.Any;
                });
                bp.AddComponent<FeatureTagsComponent>(c => {
                    c.FeatureTags = FeatureTag.Attack | FeatureTag.Melee | FeatureTag.Teamwork;
                });
            });

            FeatTools.ConfigureAsTeamworkFeat(TTTContext, PackFlanking);
            if (TTTContext.Settings.DisableAllFlankingMechanics) { return; }
            if (TTTContext.Settings.AddedFeats.IsDisabled("PackFlanking")) { return; }
            FeatTools.AddAsFeat(PackFlanking);
        }
    }
}
