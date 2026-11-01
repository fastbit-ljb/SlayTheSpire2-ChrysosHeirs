using System;
using System.Linq;
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
///
/// Patched via TargetMethod() picking the constructor by parameter count:
/// MethodType.Constructor without argument types only matches a
/// parameterless constructor, and every map constructor has optional
/// parameters instead.
/// </summary>
[HarmonyPatch]
internal static class MapForkPatch
{
    /// <summary>Column of the trial (?) node. Never 3: the Boss owns (3, rows).</summary>
    internal const int TrialCol = 2;

    /// <summary>Column of the supply campfire node.</summary>
    internal const int SupplyCol = 4;

    [HarmonyPatch(typeof(StandardActMap))]
    internal static class StandardActMapCtorPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredConstructors(typeof(StandardActMap))
                .First(c => c.GetParameters().Length == 7);
        }

        [HarmonyPostfix]
        private static void Postfix(ActMap __instance)
        {
            InsertFork(__instance);
        }
    }

    [HarmonyPatch(typeof(GoldenPathActMap))]
    internal static class GoldenPathActMapCtorPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredConstructors(typeof(GoldenPathActMap))
                .First(c => c.GetParameters().Length == 1);
        }

        [HarmonyPostfix]
        private static void Postfix(ActMap __instance)
        {
            InsertFork(__instance);
        }
    }

    [HarmonyPatch(typeof(SpoilsActMap))]
    internal static class SpoilsActMapCtorPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredConstructors(typeof(SpoilsActMap))
                .OrderBy(c => c.GetParameters().Length)
                .First();
        }

        [HarmonyPostfix]
        private static void Postfix(ActMap __instance)
        {
            InsertFork(__instance);
        }
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
        Godot.GD.Print($"[ChrysosHeirs] InsertFork: type={map.GetType().Name} cols={cols} rows={rows} bossRow={bossRow} secondBoss={(map.SecondBossMapPoint != null)}");
        Validate("pre-insert", map, grid);
        if (rows != bossRow)
        {
            // Already forked (idempotency) or an unexpected map layout.
            Godot.GD.Print("[ChrysosHeirs] InsertFork skipped: rows != bossRow");
            return;
        }

        var newGrid = new MapPoint?[cols, rows + 1];
        // NOTE: Array.Copy would scramble positions here — multidimensional
        // arrays are linearized with a per-dimension stride, and the row
        // stride differs after growing the grid by one row.
        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                newGrid[c, r] = grid[c, r];
            }
        }

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
        Validate("post-insert", map, newGrid);
    }

    /// <summary>
    /// Debug validation: every child of every known point must live in the
    /// grid or be one of the special points. Dumps dangling edges to the log.
    /// </summary>
    private static void Validate(string label, ActMap map, MapPoint?[,] grid)
    {
        int dangling = 0;
        var special = new HashSet<MapPoint>();
        if (map.BossMapPoint != null) special.Add(map.BossMapPoint);
        if (map.StartingMapPoint != null) special.Add(map.StartingMapPoint);
        if (map.SecondBossMapPoint != null) special.Add(map.SecondBossMapPoint);

        void Check(MapPoint p)
        {
            foreach (MapPoint child in p.Children)
            {
                bool inGrid = child.coord.col >= 0 && child.coord.col < grid.GetLength(0)
                    && child.coord.row >= 0 && child.coord.row < grid.GetLength(1)
                    && grid[child.coord.col, child.coord.row] == child;
                if (!inGrid && !special.Contains(child))
                {
                    Godot.GD.Print($"[ChrysosHeirs] {label} DANGLING: point{p} ({p.PointType}) -> child{child} ({child.PointType})");
                    dangling++;
                }
            }
        }

        foreach (MapPoint p in grid)
        {
            if (p != null)
            {
                Check(p);
            }
        }
        foreach (MapPoint p in special)
        {
            Check(p);
        }
        if (dangling > 0)
        {
            Godot.GD.Print($"[ChrysosHeirs] {label}: {dangling} dangling edges");
        }
    }
}
