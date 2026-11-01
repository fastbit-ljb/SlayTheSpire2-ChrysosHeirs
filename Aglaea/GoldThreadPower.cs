using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ChrysosHeirsMod;

/// <summary>
/// 金丝 (Gold Thread): each stack makes the next attack card deal +3 damage.
/// All stacks are consumed by the next attack card's damage. Carried by the
/// player (Romance Resonance gains a stack per skill card played).
/// </summary>
public sealed class GoldThreadPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (Amount <= 0 || cardSource == null || cardSource.Type != CardType.Attack || dealer != Owner)
        {
            return amount;
        }
        int stacks = Amount;
        SetAmount(0);
        return amount + 3 * stacks;
    }
}
