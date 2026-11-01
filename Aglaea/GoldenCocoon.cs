using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ChrysosHeirsMod;

/// <summary>
/// 黄金之茧 (Golden Cocoon) — rare, mixed: at the start of each combat gain
/// 2 Strength and 3 Block, but lose 3 HP at the end of each of your turns.
/// </summary>
public sealed class GoldenCocoon : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), new[] { Owner.Creature }, 2m, Owner.Creature, null);
        await CreatureCmd.GainBlock(Owner.Creature, 3m, default, null);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
        {
            return;
        }
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 3m, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
}
