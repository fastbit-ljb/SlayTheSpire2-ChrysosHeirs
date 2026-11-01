using MegaCrit.Sts2.Core.Models;

namespace ChrysosHeirsMod;

/// <summary>
/// Definition of one Chrysos Heir (黄金裔): which acts their trial appears in,
/// the buffed-elite encounter their trial uses, and the resonance relic a
/// victory grants. The list index is the stable id persisted in TrialRecord.
/// </summary>
public sealed class HeirDef
{
    /// <summary>Stable id, also used in localization option keys.</summary>
    public required string Id { get; init; }

    /// <summary>0-based act indices whose trial fork offers this heir.</summary>
    public required int[] Acts { get; init; }

    public required Func<TrialEncounter> CreateEncounter { get; init; }

    public required Func<RelicModel> ResonanceRelic { get; init; }
}

public static class Heirs
{
    public static readonly IReadOnlyList<HeirDef> All =
    [
        new HeirDef
        {
            Id = "AGLAEA",
            Acts = [0],
            CreateEncounter = () => ModelDb.Encounter<AglaeaTrialEncounter>(),
            ResonanceRelic = () => ModelDb.Relic<RomanceResonance>(),
        },
        // TODO(phase 2): remaining 12 heirs.
    ];
}
