using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public static class BiomeCellRegionBuilder
{
    // Notes:
    // - find borders
    // - after adding all possible cells to region check areas enclosed by borders
    // --- if area smaller than a percentage of region, add to region
    // ------ if the area contained a region, merge that region into larger region
    // - neighbors are associated to borders
    // - mark regions that are too small to be added to bigger bordering regions
    // - jump over one cell wide areas
    // - do not add to region one cell wide areas unless surrounded by ocean
    // - create regions for areas adjacent to cell groups even if unhabited
    // - a region size should be limited by the discovering polity exploration range
    // - if a polity expands beyond a region border into unexplored ares then the new
    //   areas should be added to the region if they are similar

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
            foreach (TerrainCell nCell in cell.Neighbors.Values)
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

        border.CalcRectangle();

        if (_largestBorderRectArea < border.RectArea)
        {
            _largestBorderRectArea = border.RectArea;
            _largestBorder = border;
        }
    }

    public static bool AddCellsWithinBiome(
        TerrainCell startCell,
        string biomeId,
        out HashSet<TerrainCell> addedCells,
        out Border outsideBorder,
        out List<HashSet<TerrainCell>> unincorporatedEnclosedAreas,
        int abortSize = -1)
    {
        outsideBorder = null;
        addedCells = new HashSet<TerrainCell>();
        unincorporatedEnclosedAreas = new List<HashSet<TerrainCell>>();

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

            addedCells.Add(cell);
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

            border.GetEnclosedCellSet(
                addedCells,
                out HashSet<TerrainCell> cellSet,
                out int area);

            if (area <= MinAreaSize)
            {
                addedCells.UnionWith(cellSet);
            }
            else
            {
                unincorporatedEnclosedAreas.Add(cellSet);
            }
        }

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

    //private static List<BorderedArea> SplitArea(
    //    HashSet<TerrainCell> cells,
    //    Border border,
    //    int maxAllowedLength)
    //{
    //    List<BorderedArea> borderedAreas = new List<BorderedArea>();

    //    int maxLength = Mathf.Max(border.RectHeight, border.RectWidth);

    //    if (maxLength <= maxAllowedLength)
    //    {
    //        borderedAreas.Add(new BorderedArea()
    //        {
    //            EnclosedCells = cells,
    //            EnclosingBorder = border
    //        });

    //        return borderedAreas;
    //    }

    //    if (maxLength == border.RectHeight)
    //    {
    //        int middleHeight = (border.Top.Latitude + border.Bottom.Latitude) / 2;

    //        HashSet<TerrainCell> topCells = new HashSet<TerrainCell>();
    //        HashSet<TerrainCell> bottomCells = new HashSet<TerrainCell>();

    //        foreach (TerrainCell cell in cells)
    //        {
    //            if (cell.Latitude > middleHeight)   bottomCells.Add(cell);
    //            else                                topCells.Add(cell);
    //        }
    //    }

    //    return borderedAreas;
    //}

    public static Region TryGenerateRegion(
        TerrainCell startCell,
        Language language,
        HashSet<TerrainCell> cellsToIgnore = null)
    {
#if DEBUG
        Debug.Log("Generating region from cell " + startCell.Position);
#endif

        //if ((startCell.Latitude == 141) && (startCell.Longitude == 318))
        //{
        //    Debug.Log("Debugging TryGenerateRegion...");
        //}

        if (startCell.WaterBiomePresence >= 1)
            return null;

        if (startCell.Region != null)
            return null;

        string biomeId = startCell.GetLocalAndNeighborhoodMostPresentBiome(true);

        AddCellsWithinBiome(startCell, biomeId,
            out HashSet<TerrainCell> acceptedCells,
            out Border outsideBorder,
            out List<HashSet<TerrainCell>> enclosedAreas);

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
            List<HashSet<TerrainCell>> areasToMerge = new List<HashSet<TerrainCell>>();
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
                        out HashSet<TerrainCell> newCells,
                        out Border newBorder,
                        out List<HashSet<TerrainCell>> extraEnclosedAreas,
                        minAreaSizeToUse);

                cellsToSkip.UnionWith(newCells);

                if (addedArea)
                {
                    areasToMerge.Add(newCells);
                    bordersToMerge.Add(newBorder);

                    enclosedAreas.AddRange(extraEnclosedAreas);

                    // reset min area to use
                    minAreaSizeToUse = MinAreaSize;
                }

                hasAddedCells |= addedArea;
            }

            bool bigEnough = acceptedCells.Count > MinAreaSize;

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

            foreach (HashSet<TerrainCell> area in areasToMerge)
            {
                acceptedCells.UnionWith(area);
            }

            foreach (Border border in bordersToMerge)
            {
                outsideBorder.Merge(border);
            }

            outsideBorder.Consolidate(acceptedCells);
        }

        // Generate all inner regions
        List<Region> innerRegions = new List<Region>();

        foreach (HashSet<TerrainCell> enclosedArea in enclosedAreas)
        {
            foreach (Region innerRegion in
                GenerateEnclosedRegions(enclosedArea, language, acceptedCells))
            {
                innerRegions.Add(innerRegion);
            }
        }

        // Create main region
        CellRegion region = new CellRegion(startCell, language);

        region.AddCells(acceptedCells);
        region.EvaluateAttributes();
        region.Update();

        // create a super region instead if there are inner regions
        if (innerRegions.Count > 0)
        {
            SuperRegion superRegion = new SuperRegion(startCell, region, language);

            foreach (Region innerRegion in innerRegions)
            {
                superRegion.Add(innerRegion);
            }

#if DEBUG
            Debug.Log("Finished generating super region from cell " + startCell.Position);
#endif

            return superRegion;
        }

#if DEBUG
        Debug.Log("Finished generating region from cell " + startCell.Position);
#endif

        return region;
    }

    private static int GetRandomInt(int maxValue)
    {
        return _startCell.GetNextLocalRandomInt(_rngOffset++, maxValue);
    }

    // older versions of Generate Region (TODO: remove them)
    [System.Obsolete]
    public static Region TryGenerateRegion_reduced(
        TerrainCell startCell, Language establishmentLanguage, string biomeId)
    {
        int regionSize = 1;

        HashSet<CellRegion> borderingRegions = new HashSet<CellRegion>();

        HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> rejectedCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();

        acceptedCells.Add(startCell);
        exploredCells.Add(startCell);

        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();

        foreach (TerrainCell cell in startCell.Neighbors.Values)
        {
            cellsToExplore.Enqueue(cell);
            exploredCells.Add(cell);
        }

        int borderCells = 0;

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

            bool accepted = false;

            string cellBiomeId = cell.BiomeWithMostPresence;

            if (cell.Region != null) // if cell belongs to another region, reject, but add region to neighbors
            {
                borderingRegions.Add(cell.Region as CellRegion);
            }
            else if (cellBiomeId == biomeId) // if cell has target biome, accept
            {
                accepted = true;
            }

            if (accepted)
            {
                acceptedCells.Add(cell);
                regionSize++;

                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    if (rejectedCells.Contains(nCell))
                    {
                        // give another chance;
                        rejectedCells.Remove(nCell);
                        borderCells--;
                    }
                    else if (exploredCells.Contains(nCell))
                    {
                        continue;
                    }

                    cellsToExplore.Enqueue(nCell);
                    exploredCells.Add(nCell);
                }
            }

            if (!accepted)
            {
                rejectedCells.Add(cell);
                borderCells++;
            }
        }

        CellRegion region = null;

        int minRegionSize = 20;

        if ((regionSize <= minRegionSize) && (borderingRegions.Count > 0))
        {
            _rngOffset = RngOffsets.REGION_SELECT_BORDER_REGION_TO_REPLACE_WITH;
            _startCell = startCell;

            region = borderingRegions.RandomSelect(GetRandomInt);

            region.ResetInfo();
        }
        else
        {
            region = new CellRegion(startCell, establishmentLanguage);
        }

        region.AddCells(acceptedCells);

        region.EvaluateAttributes();

        region.Update();

        return region;
    }

    [System.Obsolete]
    public static Region TryGenerateRegion_original(TerrainCell startCell, Language establishmentLanguage, string biomeId)
    {
        int regionSize = 1;

        HashSet<CellRegion> borderingRegions = new HashSet<CellRegion>();

        // round the base altitude to a multiple of AltitudeRoundnessTarget
        float baseAltitude = AltitudeRoundnessTarget * Mathf.Round(startCell.Altitude / AltitudeRoundnessTarget);

        HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> rejectedCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();

        acceptedCells.Add(startCell);
        exploredCells.Add(startCell);

        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();

        foreach (TerrainCell cell in startCell.Neighbors.Values)
        {
            cellsToExplore.Enqueue(cell);
            exploredCells.Add(cell);
        }

        int borderCells = 0;

        while (cellsToExplore.Count > 0)
        {
            int toExploreCount = cellsToExplore.Count;

            float closedness = 1 - (toExploreCount / (float)(toExploreCount + borderCells));

            TerrainCell cell = cellsToExplore.Dequeue();

            float closednessFactor = 1;
            float maxClosednessFactor = MaxClosedness / 2f;

            if (MaxClosedness < 1)
            {
                closednessFactor =
                    ((1 + maxClosednessFactor) *
                    (1 - closedness) / (1 - MaxClosedness)) -
                    maxClosednessFactor;
            }

            float maxAltitudeDifference = BaseMaxAltitudeDifference * closednessFactor;

            bool accepted = false;

            string cellBiomeId = cell.BiomeWithMostPresence;

            if (cell.Region != null) // if cell belongs to another region, reject, but add region to neighbors
            {
                borderingRegions.Add(cell.Region as CellRegion);
            }
            else if (cellBiomeId == biomeId) // if cell has target biome, accept
            {
                accepted = true;
            }
            else // if cell is surrounded by a majority of cells with target biome, accept
            {
                int nSurroundCount = 0;
                int minNSurroundCount = 3;

                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    if ((nCell.BiomeWithMostPresence == biomeId) || acceptedCells.Contains(nCell))
                    {
                        nSurroundCount++;
                    }
                    else
                    {
                        nSurroundCount = 0;
                    }
                }

                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    if ((nCell.BiomeWithMostPresence == biomeId) || acceptedCells.Contains(nCell))
                    {
                        nSurroundCount++;
                    }
                    else
                    {
                        nSurroundCount = 0;
                    }
                }

                int secondRepeatCount = 1;
                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    // repeat until minNSurroundCount
                    if (secondRepeatCount >= minNSurroundCount)
                        break;

                    if (nCell.BiomeWithMostPresence == biomeId)
                    {
                        nSurroundCount++;
                    }
                    else
                    {
                        nSurroundCount = 0;
                    }

                    secondRepeatCount++;
                }

                if (nSurroundCount >= minNSurroundCount)
                {
                    accepted = true;
                }
            }

            if (accepted)
            {
                if (Mathf.Abs(cell.Altitude - baseAltitude) < maxAltitudeDifference)
                {
                    acceptedCells.Add(cell);
                    regionSize++;

                    foreach (TerrainCell nCell in cell.Neighbors.Values)
                    {
                        if (rejectedCells.Contains(nCell))
                        {
                            // give another chance;
                            rejectedCells.Remove(nCell);
                            borderCells--;
                        }
                        else if (exploredCells.Contains(nCell))
                        {
                            continue;
                        }

                        cellsToExplore.Enqueue(nCell);
                        exploredCells.Add(nCell);
                    }
                }
                else
                {
                    accepted = false;
                }
            }

            if (!accepted)
            {
                rejectedCells.Add(cell);
                borderCells++;
            }
        }

        CellRegion region = null;

        int minRegionSize = 20;

        if ((regionSize <= minRegionSize) && (borderingRegions.Count > 0))
        {
            _rngOffset = RngOffsets.REGION_SELECT_BORDER_REGION_TO_REPLACE_WITH;
            _startCell = startCell;

            region = borderingRegions.RandomSelect(GetRandomInt);

            region.ResetInfo();
        }
        else
        {
            region = new CellRegion(startCell, establishmentLanguage);
        }

        region.AddCells(acceptedCells);

        region.EvaluateAttributes();

        region.Update();

        return region;
    }
}
