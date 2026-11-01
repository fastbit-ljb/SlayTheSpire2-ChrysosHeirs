using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace ChrysosHeirsMod;

/// <summary>
/// The Ancient Trial event at the map fork behind the last campfire: lists
/// this act's un-challenged Chrysos Heirs; challenging one starts their buffed
/// elite fight. Victory grants the heir's resonance relic (rewards offered in
/// Resume, BattlewornDummy style); defeat is a normal game-ending death.
/// Once per heir per run, tracked by the TrialRecord relic.
/// </summary>
public sealed class TrialOffer : EventModel
{
    private int _pendingHeirIndex;

    public override bool IsShared => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = [];
        if (Owner != null)
        {
            TrialRecord? record = Owner.Relics.OfType<TrialRecord>().FirstOrDefault();
            int act = Owner.RunState.CurrentActIndex;
            for (int i = 0; i < Heirs.All.Count; i++)
            {
                HeirDef heir = Heirs.All[i];
                if (!heir.Acts.Contains(act) || (record?.HasChallenged(i) ?? false))
                {
                    continue;
                }
                options.Add(new EventOption(this, () => Confirm(i), $"CHRYSOS_TRIAL_OFFER.pages.INITIAL.options.{heir.Id}"));
            }
        }
        options.Add(new EventOption(this, Leave, "CHRYSOS_TRIAL_OFFER.pages.INITIAL.options.LEAVE"));
        return options;
    }

    private Task Confirm(int heirIndex)
    {
        _pendingHeirIndex = heirIndex;
        SetEventState(L10NLookup("CHRYSOS_TRIAL_OFFER.pages.CONFIRM.description"), new EventOption[]
        {
            new(this, () => BeginChallenge(heirIndex), "CHRYSOS_TRIAL_OFFER.pages.CONFIRM.options.FIGHT"),
            new(this, BackToInitial, "CHRYSOS_TRIAL_OFFER.pages.CONFIRM.options.BACK"),
        });
        return Task.CompletedTask;
    }

    private async Task BeginChallenge(int heirIndex)
    {
        if (Owner == null)
        {
            return;
        }
        TrialRecord? record = Owner.Relics.OfType<TrialRecord>().FirstOrDefault();
        if (record == null)
        {
            record = (TrialRecord)ModelDb.Relic<TrialRecord>().ToMutable();
            record.Owner = Owner;
            await RelicCmd.Obtain(record, Owner);
        }
        record.MarkChallenged(heirIndex);
        EnterCombatWithoutExitingEvent(Heirs.All[heirIndex].CreateEncounter(), Array.Empty<Reward>(), shouldResumeAfterCombat: true);
    }

    private Task BackToInitial()
    {
        SetEventState(L10NLookup("CHRYSOS_TRIAL_OFFER.pages.INITIAL.description"), GenerateInitialOptions());
        return Task.CompletedTask;
    }

    private Task Leave()
    {
        SetEventFinished(L10NLookup("CHRYSOS_TRIAL_OFFER.pages.LEAVE.description"));
        return Task.CompletedTask;
    }

    public override async Task Resume(AbstractRoom exitedRoom)
    {
        if (Owner != null && exitedRoom is CombatRoom { Encounter: TrialEncounter })
        {
            RelicModel resonance = Heirs.All[_pendingHeirIndex].ResonanceRelic().ToMutable();
            List<Reward> rewards = [new RelicReward(resonance, Owner)];
            await RewardsCmd.OfferCustom(Owner, rewards);
            SetEventFinished(L10NLookup("CHRYSOS_TRIAL_OFFER.pages.VICTORY.description"));
            return;
        }
        Leave();
    }
}
