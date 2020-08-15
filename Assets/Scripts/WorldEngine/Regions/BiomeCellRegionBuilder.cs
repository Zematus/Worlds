using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public static class BiomeCellRegionBuilder
{
    public const float BaseMaxAltitudeDifference = 1000;
    public const int AltitudeRoundnessTarget = 2000;

    public const float MaxClosedness = 0.5f;

    public const int MaxEnclosedRectArea = 125;
    public const int MinAreaSize = 8;

    private static TerrainCell _startCell;
    private static int _rngOffset;

    private static int _borderCount;
    private static List<Border> _borders;
    private static HashSet<TerrainCell> _exploredBorderCells;
    private static int _largestBorderRectArea;
    private static Border _largestBorder;

    private static HashSet<TerrainCell> _cellsThatCouldBeAdded;

    private static bool CanAddCellToRegion(TerrainCell cell, string biomeId)
    {
        if (cell.Region != null) return false;

        if (cell.IsLiquidSea) return false;

        return cell.GetLocalAndNeighborhoodMostPresentBiome(true) == biomeId;
    }

    private static Border CreateBorder(TerrainCell startCell)
    {
        Border b = new Border(_borderCount++, startCell);

        _borders.Add(b);

        return b;
    }

    private static void TryExploreBorder(
        TerrainCell startCell,
        HashSet<TerrainCell> borderCells)
    {
        if (_exploredBorderCells.Contains(startCell)) return;

        Border border = CreateBorder(startCell);

        Queue<TerrainCell> borderCellsToExplore = new Queue<TerrainCell>();

        borderCellsToExplore.Enqueue(startCell);
        _exploredBorderCells.Add(startCell);

        while (borderCellsToExplore.Count > 0)
        {
            TerrainCell cell = borderCellsToExplore.Dequeue();

            // first separate neighbor cells that are inside and outside border
            foreach (TerrainCell nCell in cell.NeighborList)
            {
                if (borderCells.Contains(nCell))
                {
                    if (_exploredBorderCells.Contains(nCell)) continue;

                    borderCellsToExplore.Enqueue(nCell);
                    _exploredBorderCells.Add(nCell);
                }
            }

            border.AddCell(cell);
        }

        border.Update();

        if (_largestBorderRectArea < border.RectArea)
        {
            _largestBorderRectArea = border.RectArea;
            _largestBorder = border;
        }
    }

    public static bool AddCellsWithinBiome(
        TerrainCell startCell,
        string biomeId,
        out CellSet addedCellSet,
        out Border outsideBorder,
        out List<CellSet> unincorporatedEnclosedAreas,
        int abortSize = -1)
    {
        outsideBorder = null;
        addedCellSet = new CellSet();
        unincorporatedEnclosedAreas = new List<CellSet>();

        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();
        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();

        HashSet<TerrainCell> borderCellsToExplore = new HashSet<TerrainCell>();

        int addedCount = 0;

        cellsToExplore.Enqueue(startCell);
        exploredCells.Add(startCell);

        _borderCount = 0;
        _borders = new List<Border>();
        _exploredBorderCells = new HashSet<TerrainCell>();
        _largestBorderRectArea = 0;
        _largestBorder = null;

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

            if ((abortSize > 0) && (addedCount >= abortSize)) return false;

            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.GetNonDiagonalNeighbors())
            {
                TerrainCell nCell = pair.Value;

                if (exploredCells.Contains(nCell)) continue;

                if (CanAddCellToRegion(nCell, biomeId))
                {
                    cellsToExplore.Enqueue(nCell);
                }
                else
                {
                    borderCellsToExplore.Add(nCell);
                }

                exploredCells.Add(nCell);
            }

            addedCellSet.AddCell(cell);
            addedCount++;
        }

        foreach (TerrainCell cell in borderCellsToExplore)
        {
            TryExploreBorder(cell, borderCellsToExplore);
        }

        outsideBorder = _largestBorder;

        foreach (Border border in _borders)
        {
            if (border == outsideBorder) continue;

            CellSet cellSet = border.GetEnclosedCellSet(addedCellSet.Cells);

            if (cellSet == null) continue;

            if (cellSet.Area <= MinAreaSize)
            {
                addedCellSet.Merge(cellSet);
            }
            else
            {
                unincorporatedEnclosedAreas.Add(cellSet);
            }
        }

        addedCellSet.Update();

        return true;
    }

    private static IEnumerable<Region> GenerateEnclosedRegions(
        HashSet<TerrainCell> enclosedArea,
        Language language,
        HashSet<TerrainCell> cellsToIgnore)
    {
        HashSet<TerrainCell> testedCells = new HashSet<TerrainCell>();

        foreach (TerrainCell cell in enclosedArea)
        {
            if (testedCells.Contains(cell)) continue;

            Region region = TryGenerateRegion(cell, language, cellsToIgnore);

            if (region == null)
            {
                testedCells.Add(cell);
                continue;
            }

            testedCells.UnionWith(region.GetCells());

            yield return region;
        }
    }

    public static Region TryGenerateRegion(
        TerrainCell startCell,
        Language language,
        HashSet<TerrainCell> cellsToIgnore = null)
    {
        //if ((startCell.Latitude == 141) && (startCell.Longitude == 318))
        //{
        //    Debug.Log("Debugging TryGenerateRegion...");
        //}

        if (startCell.WaterBiomePresence >= 1)
            return null;

        if (startCell.Region != null)
            return null;

#if DEBUG
        string debugRegionStr = "region";

        if (cellsToIgnore != null)
        {
            debugRegionStr = "enclosed region";
        }

        Debug.Log("Generating " + debugRegionStr + " from cell " + startCell.Position);
#endif

        string biomeId = startCell.GetLocalAndNeighborhoodMostPresentBiome(true);

        AddCellsWithinBiome(startCell, biomeId,
            out CellSet acceptedCellSet,
            out Border outsideBorder,
            out List<CellSet> enclosedAreas);

        HashSet<TerrainCell> cellsToSkip = new HashSet<TerrainCell>();

        if (cellsToIgnore != null)
        {
            cellsToSkip.UnionWith(cellsToIgnore);
        }

        int minAreaSizeToUse = MinAreaSize;
        int maxAttempts = 2;
        int attempt = 0;

        // Add neighboring areas that are too small to be regions of their own
        while (attempt < maxAttempts)
        {
            List<CellSet> areasToMerge = new List<CellSet>();
            List<Border> bordersToMerge = new List<Border>();

            bool hasAddedCells = false;

            foreach (TerrainCell borderCell in outsideBorder.Cells)
            {
#if DEBUG
                Manager.AddUpdatedCell(borderCell, CellUpdateType.Region, CellUpdateSubType.Membership);
#endif
                if (cellsToSkip.Contains(borderCell)) continue;
                if (borderCell.IsLiquidSea) continue;
                if (borderCell.Region != null) continue;

                string borderBiomeId = borderCell.GetLocalAndNeighborhoodMostPresentBiome(true);

                bool addedArea =
                    AddCellsWithinBiome(
                        borderCell,
                        borderBiomeId,
                        out CellSet newCellSet,
                        out Border newBorder,
                        out List<CellSet> extraEnclosedAreas,
                        minAreaSizeToUse);

                cellsToSkip.UnionWith(newCellSet.Cells);

                if (addedArea)
                {
                    areasToMerge.Add(newCellSet);
                    bordersToMerge.Add(newBorder);

                    enclosedAreas.AddRange(extraEnclosedAreas);

                    // reset min area to use
                    minAreaSizeToUse = MinAreaSize;
                }

                hasAddedCells |= addedArea;
            }

            bool bigEnough = acceptedCellSet.Area > MinAreaSize;

            if (!hasAddedCells)
            {
                // Stop if couldn't add any more cells to region and it is big enough,
                // or it is the second time it fails to add
                if (bigEnough) break;

                // next area to add can be any size. Don't abort.
                minAreaSizeToUse = -1;
                attempt++;

                cellsToSkip.Clear();

                if (cellsToIgnore != null)
                {
                    cellsToSkip.UnionWith(cellsToIgnore);
                }

                continue;
            }

            attempt = 0;

            foreach (CellSet area in areasToMerge)
            {
                acceptedCellSet.Merge(area);
            }

            foreach (Border border in bordersToMerge)
            {
                outsideBorder.Merge(border);
            }

            acceptedCellSet.Update();
            outsideBorder.Consolidate(acceptedCellSet.Cells);
        }

        Region region = CellSubRegionSetBuilder.GenerateRegionFromCellSet(
            startCell,
            GetRandomInt,
            acceptedCellSet,
            language);

        // Generate all enclosed regions
        List<Region> enclosedRegions = new List<Region>();

        foreach (CellSet enclosedArea in enclosedAreas)
        {
            foreach (Region enclosedRegion in
                GenerateEnclosedRegions(enclosedArea.Cells, language, acceptedCellSet.Cells))
            {
                enclosedRegions.Add(enclosedRegion);
            }
        }

        // create a super region if there are enclosed regions
        if (enclosedRegions.Count > 0)
        {
            SuperRegion superRegion = new SuperRegion(startCell, region, language);

            foreach (Region enclosedRegion in enclosedRegions)
            {
                superRegion.Add(enclosedRegion);
            }

#if DEBUG
            Debug.Log("Finished generating super " + debugRegionStr + " from cell " + startCell.Position);
#endif

            return superRegion;
        }

#if DEBUG
        Debug.Log("Finished generating " + debugRegionStr + " from cell " + startCell.Position);
#endif

        return region;
    }

    private static int GetRandomInt(int maxValue)
    {
        return _startCell.GetNextLocalRandomInt(_rngOffset++, maxValue);
    }
}
