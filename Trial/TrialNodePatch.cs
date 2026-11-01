using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ChrysosHeirsMod;

/// <summary>
/// The trial fork's Unknown node must always roll an Event room hosting the
/// TrialOffer event, instead of the normal unknown-point room roll and event
/// queue draw. The map coordinate is captured by the EnterMapCoordInternal
/// prefix and checked against the fork geometry: column MapForkPatch.TrialCol
/// in the row just below the Boss.
/// </summary>
internal static class TrialNodeState
{
    internal static MapCoord? PendingCoord;

    internal static bool IsTrialCoord(IRunState runState)
    {
        if (runState?.Map == null || PendingCoord is not { } coord)
        {
            return false;
        }
        MapPoint? boss = runState.Map.BossMapPoint;
        if (boss == null || coord.col != MapForkPatch.TrialCol || coord.row != boss.coord.row - 1)
        {
            return false;
        }
        return runState.Map.GetPoint(coord)?.PointType == MapPointType.Unknown;
    }
}

[HarmonyPatch]
internal static class TrialNodePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunManager), "EnterMapCoordInternal")]
    private static void CaptureCoord(MapCoord coord)
    {
        TrialNodeState.PendingCoord = coord;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), "EnterMapCoordInternal")]
    private static void ClearCoord()
    {
        TrialNodeState.PendingCoord = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), "RollRoomTypeFor")]
    private static void ForceEventRoom(RunManager __instance, ref RoomType __result)
    {
        if (__result == RoomType.Event)
        {
            return;
        }
        RunState? state = (RunState?)AccessTools.Property(typeof(RunManager), "State")?.GetValue(__instance);
        if (state != null && TrialNodeState.IsTrialCoord(state))
        {
            __result = RoomType.Event;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEvent))]
    private static bool ServeTrialEvent(RunState runState, ref EventModel __result)
    {
        if (!TrialNodeState.IsTrialCoord(runState))
        {
            return true;
        }
        // Skip the normal queue draw (and its VisitedEventIds bookkeeping —
        // once-per-heir is tracked by TrialRecord instead).
        __result = ModelDb.Event<TrialOffer>();
        return false;
    }
}
