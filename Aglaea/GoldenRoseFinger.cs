using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ChrysosHeirsMod;

/// <summary>
/// 金玫之指 (Golden Rose Finger) — rare: upgraded attack cards deal 5 more
/// damage; upgraded skill cards grant 3 more Block. On pickup, upgrade a
/// random card in your deck.
/// </summary>
public sealed class GoldenRoseFinger : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.IsUpgraded && cardSource.Type == CardType.Attack && dealer == Owner.Creature)
        {
            return amount + 5m;
        }
        return amount;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.IsUpgraded && cardSource.Type == CardType.Skill && target == Owner.Creature)
        {
            return block + 3m;
        }
        return block;
    }

    public override Task AfterObtained()
    {
        List<CardModel> upgradable = PileType.Deck.GetPile(Owner).Cards.Where(c => c.IsUpgradable).ToList();
        CardModel? pick = Owner.RunState.Rng.Niche.NextItem(upgradable);
        if (pick != null)
        {
            CardCmd.Upgrade(pick);
        }
        return Task.CompletedTask;
    }
}
