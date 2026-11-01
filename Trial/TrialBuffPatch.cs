using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ChrysosHeirsMod;

/// <summary>
/// Applies the trial buff when a TrialEncounter spawns its enemies: scale max
/// HP and add Strength, mirroring how the game itself scales monsters
/// (Creature.ScaleMonsterHpForMultiplayer) — direct internal setters, before
/// the first turn begins.
/// </summary>
[HarmonyPatch(typeof(CombatState), nameof(CombatState.CreateCreature))]
internal static class TrialBuffPatch
{
    private static void Postfix(CombatState __instance, Creature __result)
    {
        if (__result.Side != CombatSide.Enemy || __instance.Encounter is not TrialEncounter trial)
        {
            return;
        }
        int newMax = Math.Max(1, (int)Math.Round(__result.MaxHp * trial.HpMultiplier));
        __result.SetMaxHpInternal(newMax);
        __result.SetCurrentHpInternal(newMax);
        if (trial.StrengthBonus > 0)
        {
            PowerModel strength = ModelDb.Power<StrengthPower>().ToMutable();
            strength.ApplyInternal(__result, trial.StrengthBonus, silent: true);
        }
    }
}
