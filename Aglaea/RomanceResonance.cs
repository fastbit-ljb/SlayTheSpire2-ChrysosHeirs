using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ChrysosHeirsMod;

/// <summary>
/// 浪漫共鸣 (Romance Resonance) — Aglaea's resonance relic, the reward for
/// winning her trial. At the start of each combat gain 1 extra Energy; each
/// Skill card played grants 1 Gold Thread (next attack +3 damage per stack).
/// Trial-only: never enters relic pools.
/// </summary>
public sealed class RomanceResonance : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override Task BeforeCombatStart()
    {
        return PlayerCmd.GainEnergy(1m, Owner);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card?.Type != CardType.Skill || cardPlay.Card.Owner != Owner)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<GoldThreadPower>(choiceContext, [Owner.Creature], 1m, Owner.Creature, null);
    }
}
