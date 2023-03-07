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
    internal class ImprovedOutflank {
        public static void AddImprovedOutflank() {
            var Outflank = BlueprintTools.GetBlueprint<BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11");
            var Icon_ImprovedOutflank = AssetLoader.LoadInternal(TTTContext, folder: "Feats", file: "Icon_ImprovedOutflank.png");

            var ImprovedOutflank = Helpers.CreateBlueprint<BlueprintFeature>(TTTContext, "ImprovedOutflank", bp => {
                bp.SetName(TTTContext, "Improved Outflank");
                bp.SetDescription(TTTContext, "You can easily find openings in your enemies’ defenses.\n" +
                    "Benefit: Whenever you and an ally who also has this feat are threatening the same foe, " +
                    "you are considered to be flanking if you are at least perpendicular with your ally.\n" +
                    "Normal: You must be positioned opposite an ally to flank an opponent.");
                bp.m_Icon = Icon_ImprovedOutflank;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.TeamworkFeat, FeatureGroup.CombatFeat };
                bp.AddComponent<AddCustomMechanicsFeature>(c => {
                    c.Feature = UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.ImprovedOutflank;
                });
                bp.AddComponent<PrerequisiteStatValue>(c => {
                    c.Stat = StatType.BaseAttackBonus;
                    c.Value = 6;
                });
                bp.AddPrerequisiteFeature(Outflank);
                bp.AddComponent<FeatureTagsComponent>(c => {
                    c.FeatureTags = FeatureTag.Attack | FeatureTag.Melee | FeatureTag.Teamwork;
                });
            });

            FeatTools.ConfigureAsTeamworkFeat(TTTContext, ImprovedOutflank);
            if (TTTContext.Settings.DisableAllFlankingMechanics) { return; }
            if (TTTContext.Settings.AddedFeats.IsDisabled("ImprovedOutflank")) { return; }
            FeatTools.AddAsFeat(ImprovedOutflank);
        }
    }
}
