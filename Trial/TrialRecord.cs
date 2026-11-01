using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ChrysosHeirsMod;

/// <summary>
/// Granted the first time a player starts a trial. Records which heirs were
/// challenged this run ("each heir's trial can only be attempted once").
/// The list persists via [SavedProperty] alongside the run.
/// </summary>
public sealed class TrialRecord : RelicModel
{
    private int[] _challengedHeirs = [];

    [SavedProperty]
    public int[] ChallengedHeirs
    {
        get => _challengedHeirs;
        set
        {
            AssertMutable();
            _challengedHeirs = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => ChallengedHeirs.Length > 0;

    public override int DisplayAmount => ChallengedHeirs.Length;

    public bool HasChallenged(int heirIndex) => ChallengedHeirs.Contains(heirIndex);

    public void MarkChallenged(int heirIndex)
    {
        if (HasChallenged(heirIndex))
        {
            return;
        }
        ChallengedHeirs = [.. ChallengedHeirs, heirIndex];
    }
}
