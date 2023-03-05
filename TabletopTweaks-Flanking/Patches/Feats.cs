using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using TabletopTweaks.Core.NewComponents.AbilitySpecific;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.Reworks {
    class Feats {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            [HarmonyAfter(new string[] { "TabletopTweaks-Base" })]
            [HarmonyPostfix]
            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                TTTContext.Logger.LogHeader("Patching Flanking Feats");
                PatchOutflank();
            }
            static void PatchOutflank() {
                if (Main.TTTContext.Settings.DisableAllFlankingMechanics) { return; }

                var Outflank = BlueprintTools.GetBlueprint<BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11");
                Outflank.RemoveComponents<OutflankProvokeAttack>();
                Outflank.AddComponent<OutflankProvokeAttackTTT>(c => {
                    c.m_OutflankFact = Outflank.ToReference<BlueprintUnitFactReference>();
                });
            }
        }
    }
}
