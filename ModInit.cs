using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ChrysosHeirsMod;

/// <summary>
/// Mod entry point. ModelDb discovers model subclasses in the assembly
/// automatically; do NOT touch ModelDb in the initializer itself (it is not
/// initialized yet). Here we only: register Harmony patches, add the normal
/// character relics to the shared relic pool, and (hot-reload fallback)
/// re-inject all models after ModelDb.Init.
/// Trial-only content (TrialOffer, TrialRecord, TrialEncounters, resonance
/// relics) is deliberately NOT added to any pool.
/// </summary>
[ModInitializer(initializerMethod: nameof(Initialize))]
public static class ModInit
{
    private static void Initialize()
    {
        ModHelper.AddModelToPool<SharedRelicPool, GoldenWeb>();
        ModHelper.AddModelToPool<SharedRelicPool, GoldenCocoon>();
        ModHelper.AddModelToPool<SharedRelicPool, WeaverGift>();
        ModHelper.AddModelToPool<SharedRelicPool, FateSpindle>();
        ModHelper.AddModelToPool<SharedRelicPool, GoldenRoseFinger>();

        Harmony harmony = new Harmony("fastbit-ljb.ChrysosHeirs");
        harmony.PatchAll(typeof(ModInit).Assembly);
    }
}

/// <summary>
/// The mod-list hot reloader can place Workshop DLLs in a collectible load
/// context that the game's normal mod-type scan does not enumerate. Re-register
/// our models after the base database pass as a compatibility fallback.
/// ModelDb.Inject is idempotent, so normal/default-context loading is unchanged.
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
internal static class ModelDbRegistrationPatch
{
    [HarmonyPostfix]
    private static void RegisterModels()
    {
        ModelDb.Inject(typeof(TrialOffer));
        ModelDb.Inject(typeof(TrialRecord));
        ModelDb.Inject(typeof(AglaeaTrialEncounter));
        ModelDb.Inject(typeof(RomanceResonance));
        ModelDb.Inject(typeof(GoldThreadPower));
        ModelDb.Inject(typeof(GoldenWeb));
        ModelDb.Inject(typeof(GoldenCocoon));
        ModelDb.Inject(typeof(WeaverGift));
        ModelDb.Inject(typeof(FateSpindle));
        ModelDb.Inject(typeof(GoldenRoseFinger));
        ModelDb.Inject(typeof(GoldenThreadCut));
    }
}
