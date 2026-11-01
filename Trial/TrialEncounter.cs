using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace ChrysosHeirsMod;

/// <summary>
/// A trial fight: vanilla monsters buffed "burning elite" style — more HP and
/// extra Strength, applied by TrialBuffPatch when CombatState.CreateCreature
/// spawns them. Victory grants the heir's resonance relic (TrialOffer.Resume);
/// defeat is a normal death.
/// </summary>
public abstract class TrialEncounter : EncounterModel
{
    /// <summary>Multiplier applied to every enemy's max HP at spawn.</summary>
    public abstract float HpMultiplier { get; }

    /// <summary>Strength applied to every enemy at spawn.</summary>
    public abstract int StrengthBonus { get; }

    public override RoomType RoomType => RoomType.Elite;
}

/// <summary>
/// Aglaea's trial: a single Terror Eel, hardened for the duel.
/// </summary>
public sealed class AglaeaTrialEncounter : TrialEncounter
{
    public override float HpMultiplier => 1.4f;

    public override int StrengthBonus => 3;

    public override IEnumerable<MonsterModel> AllPossibleMonsters
        => [ModelDb.Monster<TerrorEel>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return [(ModelDb.Monster<TerrorEel>().ToMutable(), null)];
    }
}
