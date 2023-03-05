using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using TabletopTweaks.Core.NewComponents;
using TabletopTweaks.Core.NewUnitParts;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.NewContent.Feats {
    internal class GangUp {
        public static void AddGangUp() {
            var CombatExpertiseFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("4c44724ffa8844f4d9bedb5bb27d144a");

            var GangUp = Helpers.CreateBlueprint<BlueprintFeature>(TTTContext, "GangUp", bp => {
                bp.SetName(TTTContext, "Gang Up");
                bp.SetDescription(TTTContext, "You are adept at using greater numbers against foes.\n" +
                    "Benefit: You are considered to be flanking an opponent if at least two of your allies are threatening that opponent, " +
                    "regardless of your actual positioning.\n" +
                    "Normal: You must be positioned opposite an ally to flank an opponent.");
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.Groups = new FeatureGroup[] { FeatureGroup.Feat };
                bp.AddComponent<AddCustomMechanicsFeature>(c => {
                    c.Feature = UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.GangUp;
                });
                bp.AddComponent<PrerequisiteStatValue>(c => {
                    c.Stat = StatType.Intelligence;
                    c.Value = 13;
                });
                bp.AddPrerequisiteFeature(CombatExpertiseFeature);
                bp.AddComponent<FeatureTagsComponent>(c => {
                    c.FeatureTags = FeatureTag.Attack;
                });
            });
            if (TTTContext.Settings.DisableAllFlankingMechanics) { return; }
            if (TTTContext.Settings.AddedFeats.IsDisabled("GangUp")) { return; }
            FeatTools.AddAsFeat(GangUp);
        }
    }
}
