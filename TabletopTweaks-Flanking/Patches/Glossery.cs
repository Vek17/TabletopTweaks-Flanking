using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem;
using System.Linq;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.Patches {
    internal class Glossery {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            [HarmonyAfter(new string[] { "TabletopTweaks-Base" })]
            [HarmonyPostfix]
            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                TTTContext.Logger.LogHeader("Patching Glossery");
                PatchFlanking();
            }
            static void PatchFlanking() {
                if (Main.TTTContext.Settings.DisableAllFlankingMechanics) { return; }

                var Flanking = BlueprintTools.GetBlueprint<BlueprintEncyclopediaPage>("ccb75777c98c8204da1a4485234dd003");
                var newDescription = Helpers.CreateString(TTTContext, "Flanking.encyclopedia", "When making a melee attack, a character gets a +2 flanking bonus if an opponent is threatened by an ally on the opposite side of their target.", shouldProcess: true);
                Flanking.TemporaryContext(bp => {
                    bp.Blocks.OfType<BlueprintEncyclopediaBlockText>().First().Text = newDescription;
                });

                Helpers.CreateGlosseryEntry(
                modContext: TTTContext,
                   key: $"Flanking",
                   name: "Flanking",
                   description: newDescription,
                   EncyclopediaPage: Flanking.ToReference<BlueprintScriptableObjectReference>()
               );
                TTTContext.Logger.LogPatch(Flanking);
            }
        }
    }
}
