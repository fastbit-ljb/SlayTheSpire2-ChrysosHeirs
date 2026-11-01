using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;

namespace ChrysosHeirsMod;

/// <summary>
/// Inserts the trial fork into every generated act map: behind the last row
/// of campfires, an Unknown node (the Ancient Trial) and one extra RestSite
/// (supply) replace the direct campfire -> Boss edge, so the player picks:
/// challenge the trial, or take a rest instead.
///
/// The new points MUST live inside the Grid array: saving, loading, the map
/// screen and travel all derive from Grid, and any child referenced from
/// outside it crashes NMapScreen.DrawPaths. Grid is a getter-only auto
/// property, so the backing field is swapped via reflection.
/// </summary>
[HarmonyPatch]
internal static class MapForkPatch
{
    /// <summary>Column of the trial (?) node. Never 3: the Boss owns (3, rows).</summary>
    internal const int TrialCol = 2;

    /// <summary>Column of the supply campfire node.</summary>
    private const int SupplyCol = 4;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StandardActMap), MethodType.Constructor)]
    private static void StandardActMapPostfix(ActMap __instance)
    {
        InsertFork(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GoldenPathActMap), MethodType.Constructor)]
    private static void GoldenPathActMapPostfix(ActMap __instance)
    {
        InsertFork(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpoilsActMap), MethodType.Constructor)]
    private static void SpoilsActMapPostfix(ActMap __instance)
    {
        InsertFork(__instance);
    }

    internal static void InsertFork(ActMap map)
    {
        if (map?.BossMapPoint == null)
        {
            return;
        }
        FieldInfo? gridField = AccessTools.Field(map.GetType(), "<Grid>k__BackingField");
        if (gridField == null)
        {
            return;
        }
        var grid = (MapPoint?[,])gridField.GetValue(map);
        int cols = grid.GetLength(0);
        int rows = grid.GetLength(1);
        int bossRow = map.BossMapPoint.coord.row;
        if (rows != bossRow)
        {
            // Already forked (idempotency) or an unexpected map layout.
            return;
        }

        var newGrid = new MapPoint?[cols, rows + 1];
        Array.Copy(grid, newGrid, grid.Length);

        var trial = new MapPoint(TrialCol, rows)
        {
            PointType = MapPointType.Unknown,
            CanBeModified = false
        };
        var supply = new MapPoint(SupplyCol, rows)
        {
            PointType = MapPointType.RestSite,
            CanBeModified = false
        };
        newGrid[TrialCol, rows] = trial;
        newGrid[SupplyCol, rows] = supply;

        // Reroute every last-row campfire: campfire -> {trial, supply} -> Boss.
        for (int c = 0; c < cols; c++)
        {
            MapPoint? rest = newGrid[c, rows - 1];
            if (rest == null)
            {
                continue;
            }
            rest.RemoveChildPoint(map.BossMapPoint);
            rest.AddChildPoint(trial);
            rest.AddChildPoint(supply);
        }
        trial.AddChildPoint(map.BossMapPoint);
        supply.AddChildPoint(map.BossMapPoint);

        gridField.SetValue(map, newGrid);
    }
}
