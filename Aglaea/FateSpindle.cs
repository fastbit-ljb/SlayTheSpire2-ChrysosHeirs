using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ChrysosHeirsMod;

/// <summary>
/// 命运纺锤 (Fate Spindle) — common: whenever you play a card, 25% chance the
/// next card you play costs 0.
/// </summary>
public sealed class FateSpindle : RelicModel
{
    private bool _charged;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_charged && Owner.RunState.Rng.Niche.NextFloat() < 0.25f)
        {
            _charged = true;
            Flash();
        }
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (!_charged || originalCost <= 0m)
        {
            modifiedCost = originalCost;
            return false;
        }
        modifiedCost = 0m;
        _charged = false;
        return true;
    }
}
