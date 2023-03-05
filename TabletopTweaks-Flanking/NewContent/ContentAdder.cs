using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using TabletopTweaks.Flanking.NewContent.Feats;
using TabletopTweaks.Flanking.NewContent.Feats.TeamworkFeats;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.NewContent {
    class ContentAdder {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            [HarmonyPriority(799)]
            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                TTTContext.Logger.LogHeader("Loading New Content");
                GangUp.AddGangUp();
                ImprovedOutflank.AddImprovedOutflank();
                PackFlanking.AddPackFlanking();
            }
        }
    }
}
