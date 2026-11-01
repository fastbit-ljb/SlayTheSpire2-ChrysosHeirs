using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ChrysosHeirsMod;

/// <summary>
/// 金丝裁衣 (Golden Thread Cut) — ancient card granted by Weaver's Gift.
/// Deal 8 damage to one enemy and gain 2 Gold Thread.
/// </summary>
public sealed class GoldenThreadCut : CardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars
        => [new DamageVar(8m, ValueProp.Move)];

    public GoldenThreadCut()
        : base(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await PowerCmd.Apply<GoldThreadPower>(choiceContext, [base.Owner.Creature], 2m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
