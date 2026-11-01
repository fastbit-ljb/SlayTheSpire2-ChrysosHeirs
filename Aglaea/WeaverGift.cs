using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace ChrysosHeirsMod;

/// <summary>
/// 织者的恩赐 (Weaver's Gift) — common: on pickup, add a Golden Thread Cut
/// (金丝裁衣) card to your deck.
/// </summary>
public sealed class WeaverGift : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained()
    {
        CardModel card = ModelDb.Card<GoldenThreadCut>().ToMutable();
        PileType.Deck.GetPile(Owner).AddInternal(card);
        return Task.CompletedTask;
    }
}
