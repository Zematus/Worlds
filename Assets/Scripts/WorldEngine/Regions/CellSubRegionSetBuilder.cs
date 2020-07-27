using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public static class CellSubRegionSetBuilder
{
    public const int MaxSideLength = 25;
    public const float AltitudeFactor = 5;

    public static int DistanceComparison(TerrainCell a, TerrainCell b)
    {
        if (a.DistanceBuffer < b.DistanceBuffer) return -1;
        else if (a.DistanceBuffer > b.DistanceBuffer) return 1;

        return 0;
    }

    public static IEnumerable<CellRegion> TryGenerateSubRegions(
        GetRandomIntDelegate getRandomInt,
        CellSet startingSet,
        Language language)
    {
        List<TerrainCell> startCells = new List<TerrainCell>();

        // first subdivide the starting set and obtain a random starting point
        // from each subset
        foreach (CellSet subset in CellSet.SplitIntoSubsets(startingSet, MaxSideLength))
        {
            startCells.Add(subset.Cells.RandomSelect(getRandomInt));
        }

        HashSet<TerrainCell> addedCells = new HashSet<TerrainCell>();

        BinaryHeap<TerrainCell> distHeap =
            new BinaryHeap<TerrainCell>(DistanceComparison, startingSet.Cells.Count);

        foreach (TerrainCell startCell in startCells)
        {
            startCell.DistanceBuffer = 0;
            CellSet subset = new CellSet();

            distHeap.Insert(startCell);
            addedCells.Add(startCell);

            startCell.ObjectBuffer = subset;
        }

        // expand each starting point using Voronoi to form the subregions
        while (distHeap.Count > 0)
        {
            TerrainCell cell = distHeap.Extract(false);

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
            {
                TerrainCell nCell = pair.Value;

                if (addedCells.Contains(nCell)) continue;

                float accessibilityFactor = 1 / (0.001f + nCell.BaseAccessibility);

                float altitudeEffect = 1 + AltitudeFactor * Mathf.Abs(nCell.Altitude - cell.Altitude);
                float cellDistance =
                    nCell.NeighborDistances[pair.Key] * altitudeEffect * accessibilityFactor;

                nCell.DistanceBuffer = cell.DistanceBuffer + cellDistance;
                distHeap.Insert(nCell);
                addedCells.Add(nCell);

                CellSet cellSet = cell.ObjectBuffer as CellSet;
                cellSet.AddCell(nCell);

                nCell.ObjectBuffer = cellSet;
            }
        }

        // create sub regions
        foreach (TerrainCell startCell in startCells)
        {
            CellSet subset = startCell.ObjectBuffer as CellSet;

            CellRegion region = new CellRegion(startCell, language);

            region.AddCells(subset.Cells);
            region.EvaluateAttributes();
            region.Update();

            yield return region;
        }
    }
}
