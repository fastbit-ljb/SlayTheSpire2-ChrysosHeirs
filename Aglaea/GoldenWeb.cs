using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ChrysosHeirsMod;

/// <summary>
/// 金丝织网 (Golden Web) — rare, mixed: whenever the player gains Energy,
/// draw 1 card; hand size is reduced by 1. The energy trigger replaces
/// PlayerCmd.GainEnergy (prefix swap) because the game exposes no
/// "after energy gained" hook; the original logic is replicated with the
/// draw appended.
/// </summary>
public sealed class GoldenWeb : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task GainEnergyAndDraw(decimal amount, Player player)
    {
        if (amount <= 0m || CombatManager.Instance.IsEnding)
        {
            return;
        }
        ICombatState combatState = player.Creature.CombatState;
        decimal finalAmount = Hook.ModifyEnergyGain(combatState, player, amount, out IEnumerable<AbstractModel> modifiers);
        await Hook.AfterModifyingEnergyGain(combatState, modifiers);
        if (finalAmount > 0m)
        {
            SfxCmd.Play("event:/sfx/ui/gain_energy");
            player.PlayerCombatState.GainEnergy(finalAmount);
            Flash();
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1m, player);
        }
    }
}

[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainEnergy))]
internal static class GoldenWebEnergyPatch
{
    [HarmonyPrefix]
    private static bool ReplaceWithDrawVersion(ref Task __result, decimal amount, Player player)
    {
        if (player.Relics.OfType<GoldenWeb>().FirstOrDefault() is not { } relic)
        {
            return true;
        }
        __result = relic.GainEnergyAndDraw(amount, player);
        return false;
    }
}

/// <summary>
/// Hand size -1 while any player owns a GoldenWeb. MaxCardsInHand is a static
/// property, hence the "any player" check (fine for single player, which is
/// this mod's scope).
/// </summary>
[HarmonyPatch(typeof(CardPile), "MaxCardsInHand", MethodType.Getter)]
internal static class GoldenWebHandLimitPatch
{
    [HarmonyPostfix]
    private static void ReduceHandLimit(ref int __result)
    {
        RunState? state = (RunState?)AccessTools.Property(typeof(RunManager), "State")?.GetValue(RunManager.Instance);
        if (state?.Players.Any(p => p.Relics.OfType<GoldenWeb>().Any()) == true)
        {
            __result -= 1;
        }
    }
}
