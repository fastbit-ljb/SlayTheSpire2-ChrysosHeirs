using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ChrysosHeirsMod;

/// <summary>
/// Mod entry point. RelicModel / PowerModel / EventModel / CardModel subclasses
/// in this assembly are discovered and registered automatically by the game's
/// ModelDb.Init(); do NOT touch ModelDb here (it is not initialized yet).
/// Harmony patches only: map fork generation, trial event injection, relic
/// pool injection.
/// </summary>
[ModInitializer(initializerMethod: nameof(Initialize))]
public static class ModInit
{
    private static void Initialize()
    {
        Harmony harmony = new Harmony("fastbit-ljb.ChrysosHeirs");
        harmony.PatchAll(typeof(ModInit).Assembly);
    }
}
