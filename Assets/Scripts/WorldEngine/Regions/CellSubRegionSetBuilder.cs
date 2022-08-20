using UnityEngine;
using System.Collections.Generic;

public static class CellSubRegionSetBuilder
{
    public const int MaxMajorLength = 30;
    public const int MinMajorLength = 20;
    public const float MaxScaleDiff = 1.618f;
    public const float MinRectAreaPercent = 0.6f;

    public const float HillinessEffect = 50;
    public const float AccessibilityPower = 4;

    public static int DistanceComparison(TerrainCell a, TerrainCell b)
    {
        if (a.DistanceBuffer < b.DistanceBuffer) return -1;
        else if (a.DistanceBuffer > b.DistanceBuffer) return 1;

        return 0;
    }

    private static IEnumerable<CellRegion> TryGenerateSubRegions(
        CellSet startingSet,
        Language language)
    {
        List<TerrainCell> startCells = new List<TerrainCell>();

        // initialize temp buffers
        foreach (TerrainCell cell in startingSet.Cells)
        {
            cell.DistanceBuffer = -1;
            cell.ObjectBuffer = null;
        }

        // first subdivide the starting set and obtain a random starting point
        // from each subset
        foreach (CellSet subset in CellSet.SplitIntoSubsets(
            startingSet, MaxMajorLength, MinMajorLength, MaxScaleDiff, MinRectAreaPercent))
        {
            startCells.Add(subset.GetMostCenteredCell());
        }

        HashSet<TerrainCell> addedCells = new HashSet<TerrainCell>();

        BinaryHeap<TerrainCell> distHeap =
            new BinaryHeap<TerrainCell>(DistanceComparison, startingSet.Cells.Count);

        foreach (TerrainCell startCell in startCells)
        {
            startCell.DistanceBuffer = 0;
            CellSet subset = new CellSet();

            distHeap.Insert(startCell);

            startCell.ObjectBuffer = subset;
        }

        // expand each starting point using Voronoi to form the subregions
        while (distHeap.Count > 0)
        {
            TerrainCell cell = distHeap.Extract(false);

            // skip cells that have already been added
            if (addedCells.Contains(cell)) continue;

            addedCells.Add(cell);

            CellSet cellSet = cell.ObjectBuffer as CellSet;
            cellSet.AddCell(cell);

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
            {
                TerrainCell nCell = pair.Value;

                // dont add cells that are outside of starting set
                if (!startingSet.Cells.Contains(nCell)) continue;

                // skip cells that have already been added
                if (addedCells.Contains(nCell)) continue;

                float accessibilityEffect = (cell.Accessibility + nCell.BaseAccessibility) / 2f;
                accessibilityEffect = Mathf.Pow(accessibilityEffect, AccessibilityPower);
                float accessibilityFactor = 1 / (0.001f + accessibilityEffect);

                float avgHilliness = (cell.Hilliness + nCell.Hilliness) / 2f;
                float hillinessFactor = 1 + HillinessEffect * avgHilliness;

                float nCellDistance = cell.DistanceBuffer + 
                    cell.NeighborDistances[pair.Key] * hillinessFactor * accessibilityFactor;

                if ((nCell.DistanceBuffer != -1) && (nCell.DistanceBuffer <= nCellDistance))
                    continue;

                nCell.DistanceBuffer = nCellDistance;
                distHeap.Insert(nCell);

                // set nCell to become part of of the set to which cell belongs
                nCell.ObjectBuffer = cell.ObjectBuffer;
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

    public static Region GenerateRegionFromCellSet(
        TerrainCell startCell,
        CellSet cellSet,
        Language language)
    {
        Region region;
        List<CellRegion> subRegions = new List<CellRegion>();

        // generate subregions
        subRegions.AddRange(TryGenerateSubRegions(cellSet, language));

        if (subRegions.Count < 0)
        {
            throw new System.Exception("CellSubRegionSetBuilder generated 0 subregions");
        }

        region = subRegions[0];

        // replace the region with a super region if there are more than one subregions
        if (subRegions.Count > 0)
        {
            SuperRegion superRegion = new SuperRegion(startCell, subRegions[0], language);

            for (int i = 1; i < subRegions.Count; i++)
            {
                superRegion.Add(subRegions[i]);
            }

            region = superRegion;
        }

        return region;
    }
}
