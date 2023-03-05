using HarmonyLib;
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TabletopTweaks.Core.MechanicsChanges;
using TurnBased.Controllers;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.MechanicsChanges {
    internal class FlankingPatches {

        internal class Prefixes {

            [HarmonyPatch(typeof(UnitCombatState), "IsFlanked", MethodType.Getter)]
            static class UnitCombatState_IsFlanked_Patch {
                static bool Prefix(UnitCombatState __instance, ref bool __result) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }
                    if (!FlankingMechanics.CustomProviderLoaded) { return true; }
                    if (CombatController.IsInTurnBasedCombat() && __instance.Unit.IsInCombat) {
                        __result = !__instance.Unit.Descriptor.State.Features.CannotBeFlanked
                            && __instance.EngagedBy.Any(unit => __instance.Unit.IsFlankedBy(unit));
                        return false;
                    }
                    if (TacticalCombatHelper.IsActive) {
                        return true;
                    }
                    __result = !__instance.Unit.Descriptor.State.Features.CannotBeFlanked
                        && __instance.EngagedBy.Any(unit => __instance.Unit.IsFlankedBy(unit));
                    return false;
                }
            }

            [HarmonyPatch(typeof(ModifyD20), nameof(ModifyD20.CheckTandemTrip))]
            static class ModifyD20_CheckTandemTrip_Patch {
                static bool Prefix(ModifyD20 __instance, RuleCombatManeuver evt, ref bool __result) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (!(evt.Target.CombatState.EngagedBy.Count < 2) || (evt.Type != CombatManeuver.Trip && evt.Type != CombatManeuver.AwesomeBlow)) {
                        __result = false;
                        return false;
                    }
                    bool triggerTandamTrip = __instance.Owner.State.Features.SoloTactics;
                    if (!triggerTandamTrip) {
                        foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy) {
                            triggerTandamTrip = unitEntityData != __instance.Owner && unitEntityData.Descriptor.HasFact(__instance.TandemTripFeature);
                            if (triggerTandamTrip) {
                                break;
                            }
                        }
                    }
                    __result = triggerTandamTrip;
                    return false;
                }
            }

            [HarmonyPatch(typeof(OutflankAttackBonus), nameof(OutflankAttackBonus.OnEventAboutToTrigger))]
            static class OutflankAttackBonus_OnEventDidTrigger_Patch {
                static bool Prefix(OutflankAttackBonus __instance, RuleCalculateAttackBonus evt) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (!evt.Weapon.Blueprint.IsMelee || !evt.Target.IsFlankedBy(evt.Initiator)) {
                        return false;
                    }
                    bool ApplyAttackBonus = __instance.Owner.State.Features.SoloTactics;
                    if (!ApplyAttackBonus) {
                        foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy.Where(initator => evt.Target.IsFlankedBy(initator))) {
                            ApplyAttackBonus = unitEntityData != __instance.Owner && unitEntityData.Descriptor.HasFact(__instance.OutflankFact);
                            if (ApplyAttackBonus) {
                                break;
                            }
                        }
                    }
                    if (ApplyAttackBonus) {
                        evt.AddModifier(__instance.AttackBonus * __instance.Fact.GetRank(), __instance.Fact, __instance.Descriptor);
                    }
                    return false;
                }
            }

            [HarmonyPatch(typeof(OutflankDamageBonus), nameof(OutflankDamageBonus.OnEventDidTrigger))]
            static class OutflankDamageBonus_OnEventDidTrigger_Patch {
                static bool Prefix(OutflankDamageBonus __instance, RuleCalculateDamage evt) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (!evt.Target.IsFlankedBy(evt.Initiator)) {
                        return false;
                    }
                    int bonus = __instance.DamageBonus;
                    if (__instance.Owner.State.Features.SoloTactics
                        || (!__instance.m_OutflankFact.IsEmpty() && evt.Target.CombatState.EngagedBy
                            .Where(initator => evt.Target.IsFlankedBy(initator))
                            .Any(initator => initator.HasFact(__instance.OutflankFact)))) {
                        bonus += __instance.IncreasedDamageBonus;
                    }
                    BaseDamage first = evt.DamageBundle.First;
                    if (first == null) {
                        return false;
                    }
                    first.AddModifierTargetRelated(bonus, __instance.Fact, ModifierDescriptor.UntypedStackable);
                    return false;
                }
            }

            [HarmonyPatch(typeof(OutflankProvokeAttack), nameof(OutflankProvokeAttack.OnEventDidTrigger))]
            static class OutflankProvokeAttack_OnEventDidTrigger_Patch {
                static bool Prefix(OutflankProvokeAttack __instance, RuleAttackRoll evt) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (evt.IsFake
                    || !evt.IsHit
                    || !evt.IsCriticalConfirmed
                    || evt.FortificationNegatesCriticalHit
                    || (!evt.Target.IsFlankedBy(evt.Initiator) && !evt.Weapon.Blueprint.IsMelee)) {
                        return false;
                    }
                    foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy.Where(initator => evt.Target.IsFlankedBy(initator))) {
                        if (unitEntityData.Descriptor.HasFact(__instance.OutflankFact) && unitEntityData != __instance.Owner) {
                            Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(unitEntityData, evt.Target, false);
                        }
                    }
                    return false;
                }
            }

            [HarmonyPatch(typeof(PreciseStrike), nameof(PreciseStrike.OnEventAboutToTrigger))]
            static class PreciseStrike_OnEventAboutToTrigger_Patch {
                static bool Prefix(PreciseStrike __instance, RulePrepareDamage evt) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (!evt.Target.IsFlankedBy(__instance.Owner) || evt.DamageBundle.Weapon == null || !evt.DamageBundle.Weapon.Blueprint.IsMelee) {
                        return false;
                    }
                    bool triggerPreciseStrike = __instance.Owner.State.Features.SoloTactics;
                    if (!triggerPreciseStrike) {
                        foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy.Where(u => u.Descriptor.HasFact(__instance.PreciseStrikeFact))) {
                            triggerPreciseStrike = evt.Target.IsFlankedBy(unitEntityData) && unitEntityData != __instance.Owner;
                            if (triggerPreciseStrike) {
                                break;
                            }
                        }
                    }
                    if (triggerPreciseStrike) {
                        BaseDamage damage = __instance.Damage.CreateDamage();
                        evt.Add(damage);
                    }
                    return false;
                }
            }

            [HarmonyPatch(typeof(MadDogPackTactics), nameof(MadDogPackTactics.OnEventAboutToTrigger))]
            static class MadDogPackTactics_OnEventAboutToTrigger_Patch {
                static bool Prefix(MadDogPackTactics __instance, RuleCalculateAttackBonus evt) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return true; }

                    if (!evt.Target.IsFlankedBy(evt.Initiator) || !evt.Weapon.Blueprint.IsMelee) {
                        return false;
                    }
                    bool applyPackTackticks = false;
                    foreach (UnitEntityData unitEntityData in evt.Target.CombatState.EngagedBy.Where(initiator => evt.Target.IsFlankedBy(initiator))) {
                        applyPackTackticks = (unitEntityData.IsPet && unitEntityData.Master == __instance.Owner) || (__instance.Owner.IsPet && unitEntityData == __instance.Owner.Master);
                        if (applyPackTackticks) {
                            break;
                        }
                    }
                    if (applyPackTackticks) {
                        evt.AddModifier(2, __instance.Fact, ModifierDescriptor.UntypedStackable);
                    }
                    return false;
                }
            }
        }

        internal class Transpilers {

            static readonly MethodInfo FlankingMechanics_IsFlankedBy = AccessTools.Method(
                typeof(FlankingMechanics),
                "IsFlankedBy",
                parameters: new Type[] { typeof(UnitEntityData), typeof(UnitEntityData) }
            );
            static readonly MethodInfo UnitCombatState_IsFlanked_Get = AccessTools.PropertyGetter(
                typeof(UnitCombatState),
                "IsFlanked"
            );
            static readonly FieldInfo RulebookEvent_Initiator_Get = AccessTools.Field(
                typeof(RulebookEvent),
                "Initiator"
            );

            static void ReplaceFlankCheck(List<CodeInstruction> codes, int index, string patchTarget) {
                TTTContext.Logger.Log($"{patchTarget} - Replacing flanking at index: {index}");
                codes[index] = new CodeInstruction(OpCodes.Nop);
                codes[index - 1] = new CodeInstruction(OpCodes.Nop);
                codes.InsertRange(index - 1, new CodeInstruction[] {
                    codes[index - 3].Clone(), //Load rule
                    new CodeInstruction(OpCodes.Ldfld, RulebookEvent_Initiator_Get),
                    new CodeInstruction(OpCodes.Call, FlankingMechanics_IsFlankedBy),
                });
            }

            [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
            static class RuleAttackRoll_OnTrigger_Patch {
                // ------------before------------
                // (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked);
                // ------------after-------------
                // ruleAttackWithWeapon.FirstAttack = true;
                // (this.IsTargetFlatFooted || this.Target.IsFlankedBy(this.Initiator)));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            ReplaceFlankCheck(codes, i, "RuleAttackRoll_OnTrigger");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(RuleCalculateAttackBonus), nameof(RuleCalculateAttackBonus.OnTrigger))]
            static class RuleCalculateAttackBonus_OnTrigger_Patch {
                // ------------before------------
                // (this.Target.CombatState.IsFlanked || this.TargetIsFlanked);
                // ------------after-------------
                // (this.Target.CombatState.IsFlanked || this.Target.IsFlankedBy(this.Initiator));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            ReplaceFlankCheck(codes, i, "RuleCalculateAttackBonus_OnTrigger");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(RulePrepareDamage), nameof(RulePrepareDamage.OnTrigger))]
            static class RulePrepareDamage_OnTrigger_Patch {
                // ------------before------------
                // (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked);
                // ------------after-------------
                // ruleAttackWithWeapon.FirstAttack = true;
                // (this.IsTargetFlatFooted || this.Target.IsFlankedBy(this.Initiator)));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            ReplaceFlankCheck(codes, i, "RulePrepareDamage_OnTrigger");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(AutoConfirmCritAgainstFlanked), nameof(AutoConfirmCritAgainstFlanked.OnEventAboutToTrigger))]
            static class AutoConfirmCritAgainstFlanked_OnEventAboutToTrigger_Patch {
                // ------------before------------
                // (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked);
                // ------------after-------------
                // ruleAttackWithWeapon.FirstAttack = true;
                // (this.IsTargetFlatFooted || this.Target.IsFlankedBy(this.Initiator)));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            ReplaceFlankCheck(codes, i, "AutoConfirmCritAgainstFlanked_OnEventAboutToTrigger");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(FlankedAttackBonus), nameof(FlankedAttackBonus.OnEventAboutToTrigger))]
            static class FlankedAttackBonus_OnEventAboutToTrigger_Patch {
                // ------------before------------
                // (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked);
                // ------------after-------------
                // ruleAttackWithWeapon.FirstAttack = true;
                // (this.IsTargetFlatFooted || this.Target.IsFlankedBy(this.Initiator)));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            ReplaceFlankCheck(codes, i, "FlankedAttackBonus_OnEventAboutToTrigger");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(BackToBack), nameof(BackToBack.OnEventAboutToTrigger))]
            static class BackToBack_OnEventAboutToTrigger_Patch {
                // ------------before------------
                // (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked);
                // ------------after-------------
                // ruleAttackWithWeapon.FirstAttack = true;
                // (this.IsTargetFlatFooted || this.Target.IsFlankedBy(this.Initiator)));
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    if (TTTContext.Settings.DisableAllFlankingMechanics) { return instructions; }
                    var codes = new List<CodeInstruction>(instructions);
                    //if (Main.TTTContext.Fixes.Feats.IsDisabled("VitalStrike")) { return instructions; }
                    //ILUtils.LogIL(TTTContext, codes);
                    FindInsertionTargetAndReplaceFlankChecks(codes);
                    //ILUtils.LogIL(TTTContext, codes);
                    return codes.AsEnumerable();
                }
                private static void FindInsertionTargetAndReplaceFlankChecks(List<CodeInstruction> codes) {
                    //Looking for the arguments that define the object creation because searching for the object creation itself is hard
                    for (int i = 0; i < codes.Count; i++) {
                        if (codes[i].Calls(UnitCombatState_IsFlanked_Get)) {
                            TTTContext.Logger.Log($"BackToBack_OnEventAboutToTrigger - Replacing flanking at index: {i}");
                            codes[i] = new CodeInstruction(OpCodes.Nop);
                            codes[i - 1] = new CodeInstruction(OpCodes.Nop);
                            codes.InsertRange(i - 1, new CodeInstruction[] {
                                new CodeInstruction(OpCodes.Ldarg_1),
                                new CodeInstruction(OpCodes.Ldfld, RulebookEvent_Initiator_Get),
                                new CodeInstruction(OpCodes.Call, FlankingMechanics_IsFlankedBy),
                            });
                        }
                    }
                }
            }
        }
    }
}
