using Kingmaker.EntitySystem.Entities;
using System.Linq;
using TabletopTweaks.Core.MechanicsChanges;
using TabletopTweaks.Core.NewUnitParts;
using UnityEngine;
using static TabletopTweaks.Flanking.Main;

namespace TabletopTweaks.Flanking.MechanicsChanges {
    public class TTTFlankingProvider : FlankingMechanics.IFlankingProvider {

        public float FlankingAngle => TTTContext.Settings.FlankingAngle;
        public float FlankingAngleImproved => TTTContext.Settings.FlankingAngleImproved;
        public bool ApplyToEnemies => TTTContext.Settings.ApplyFlankingRulesToEnemies;

        public bool CheckFlankedBy(UnitEntityData target, UnitEntityData initiator) {
            if (target == null || initiator == null) { return false; }
            if (!initiator.CombatState.IsEngage(target)) { return false; }
            if (target.Descriptor.State.Features.CannotBeFlanked) { return false; }

            var combatState = target.CombatState;
            var origin = target.Position;
            var position1 = initiator.Position;

            if (!ApplyToEnemies && !initiator.IsPlayerFaction) {
                return combatState.EngagedBy.Count > 1;
            }
            if (TTTContext.Settings.GiveEnemiesGangUp && !initiator.IsPlayerFaction) {
                return combatState.EngagedBy.Count > 2;
            }
            if (initiator.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.GangUp)) {
                var GangUp = combatState.EngagedBy.Count > 2;
                if (GangUp) { return true; }
            }
            if (initiator.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.PackFlanking)) {
                var PackFlanking = false;
                if (initiator.IsPet) {
                    PackFlanking = initiator.Master.CombatState.IsEngage(target) && initiator.Master.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.PackFlanking);
                } else {
                    PackFlanking = initiator.Pets.Any(p => p.Entity.CombatState.IsEngage(target) && p.Entity.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.PackFlanking));
                }
                if (PackFlanking) { return true; }
            }
            var HasImprovedOutflank = initiator.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.ImprovedOutflank);
            foreach (var unit2 in combatState.EngagedBy) {
                if (initiator == unit2) { continue; }

                var position2 = unit2.Position;
                var angle = Vector3.Angle(position1 - origin, position2 - origin);
                if (HasImprovedOutflank) {
                    bool useImprovedAngle = initiator.State.Features.SoloTactics || unit2.CustomMechanicsFeature(UnitPartCustomMechanicsFeatures.CustomMechanicsFeature.ImprovedOutflank);
                    if (useImprovedAngle && angle >= FlankingAngleImproved) {
                        return true;
                    };
                }
                if (angle >= FlankingAngle) {
                    return true;
                };
            }
            return false;
        }

        public static TTTFlankingProvider Instance = new TTTFlankingProvider();
    }
}
