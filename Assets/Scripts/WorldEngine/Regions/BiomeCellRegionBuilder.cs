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
    public const int MaxEnclosedArea = 25;

    private static TerrainCell _startCell;
    private static int _rngOffset;

    private static int _borderCount;
    private static List<Border> _borders;
    private static HashSet<TerrainCell> _borderCells;
    private static int _largestBorderRectArea;

    public static bool CanAddCellToBiomeRegion(TerrainCell cell, string biomeId)
    {
        if (cell.Region != null)
        {
            return false;
        }

        string cellBiomeId = cell.BiomeWithMostPresence;

        return cellBiomeId == biomeId;
    }

    public class Border
    {
        public int Id;

        public HashSet<TerrainCell> Cells;

        public TerrainCell Top;
        public TerrainCell Bottom;
        public TerrainCell Left;
        public TerrainCell Right;

        public int RectArea = 1;
        public int RectWidth = 1;
        public int RectHeight = 1;

        public Border(int id, TerrainCell startCell)
        {
            Id = id;
            Cells = new HashSet<TerrainCell>();

            Cells.Add(startCell);

            Top = startCell;
            Bottom = startCell;
            Left = startCell;
            Right = startCell;
        }

        public void AddCell(TerrainCell cell)
        {
            Cells.Add(cell);

            if (((cell.Longitude - Left.Longitude) == -1) ||
                ((cell.Longitude - Left.Longitude - Manager.WorldWidth) == -1))
            {
                Left = cell;
            }

            if (((cell.Longitude - Right.Longitude) == 1) ||
                ((cell.Longitude - Right.Longitude + Manager.WorldWidth) == 1))
            {
                Right = cell;
            }

            if ((cell.Latitude - Top.Latitude) == -1)
            {
                Top = cell;
            }

            if ((cell.Latitude - Bottom.Latitude) == 1)
            {
                Bottom = cell;
            }
        }

        public void CalcRectangle()
        {
            int top = Top.Latitude;
            int bottom = Bottom.Latitude;
            int left = Left.Longitude;
            int right = Right.Longitude;

            // adjust for world wrap
            if (right < left) right += Manager.WorldWidth;

            RectHeight = bottom - top + 1;
            RectWidth = right - left + 1;

            RectArea = RectWidth * RectHeight;
        }

        public bool IsCellEnclosed(TerrainCell cell)
        {
            int top = Top.Latitude;
            int bottom = Bottom.Latitude;
            int left = Left.Longitude;
            int right = Right.Longitude;

            // adjust for world wrap
            if (right < left) right += Manager.WorldWidth;

            if (cell.Latitude < top) return false;

            if (cell.Latitude > bottom) return false;

            int longitude = cell.Longitude;

            if (longitude.IsInsideRange(left, right)) return true;

            longitude += Manager.WorldWidth;

            if (longitude.IsInsideRange(left, right)) return true;

            return false;
        }

        public void GetEnclosedCellSet(
            HashSet<TerrainCell> outsideSet,
            out HashSet<TerrainCell> set,
            out int area)
        {
            set = new HashSet<TerrainCell>();
            area = 0;

            HashSet<TerrainCell> exploredSet = new HashSet<TerrainCell>();
            exploredSet.UnionWith(outsideSet);

            Queue<TerrainCell> toAdd = new Queue<TerrainCell>();

            toAdd.Enqueue(Top);

            while (toAdd.Count > 0)
            {
                TerrainCell cell = toAdd.Dequeue();

                if (!cell.IsBelowSeaLevel)
                {
                    set.Add(cell);
                    area++;
                }

                foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
                {
                    TerrainCell nCell = pair.Value;

                    if (exploredSet.Contains(nCell)) continue;

                    if (TerrainCell.IsDiagonalDirection(pair.Key)) continue;

                    if (!IsCellEnclosed(nCell)) continue;

                    toAdd.Enqueue(nCell);
                    exploredSet.Add(nCell);
                }
            }
        }
    }

    private static Border CreateBorder(TerrainCell startCell)
    {
        Border b = new Border(_borderCount++, startCell);

        _borders.Add(b);

        return b;
    }

    private static void TryExploreBorder(
        TerrainCell startCell,
        string biomeId)
    {
        if (_borderCells.Contains(startCell)) return;

        Border border = CreateBorder(startCell);

        HashSet<TerrainCell> borderExploredCells = new HashSet<TerrainCell>();

        Queue<TerrainCell> borderCellsToExplore = new Queue<TerrainCell>();

        HashSet<TerrainCell> inBorderCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> outBorderCells = new HashSet<TerrainCell>();

        borderCellsToExplore.Enqueue(startCell);
        borderExploredCells.Add(startCell);

        while (borderCellsToExplore.Count > 0)
        {
            TerrainCell cell = borderCellsToExplore.Dequeue();

            inBorderCells.Clear();
            outBorderCells.Clear();

            // first separate neighbor cells that are inside and outside border
            foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
            {
                Direction d = pair.Key;
                TerrainCell nCell = pair.Value;

                if (CanAddCellToBiomeRegion(nCell, biomeId))
                {
                    inBorderCells.Add(nCell);
                }
                else
                {
                    // ignore diagonal directions
                    if (!TerrainCell.IsDiagonalDirection(d))
                    {
                        outBorderCells.Add(nCell);
                    }
                }
            }

            // now find which neighbor cells are exactly in the border
            foreach (TerrainCell cellIn in outBorderCells)
            {
                bool isBorder = false;

                // find if any of the neighbor to the neighbor is an cell outside
                foreach (TerrainCell nc in cellIn.Neighbors.Values)
                {
                    if (inBorderCells.Contains(nc))
                    {
                        isBorder = true;
                        break;
                    }
                }

                if (isBorder)
                {
                    if (borderExploredCells.Contains(cellIn))
                    {
                        continue;
                    }

                    borderCellsToExplore.Enqueue(cellIn);
                    borderExploredCells.Add(cellIn);
                }
            }

            border.AddCell(cell);
            _borderCells.Add(cell);
        }

        border.CalcRectangle();

        if (_largestBorderRectArea < border.RectArea)
        {
            _largestBorderRectArea = border.RectArea;
        }
    }

    public static Region TryGenerateRegion(
        TerrainCell startCell,
        Language language,
        string biomeId)
    {
        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();
        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell>();

        HashSet<TerrainCell> borderCellsToExplore = new HashSet<TerrainCell>();

        cellsToExplore.Enqueue(startCell);
        exploredCells.Add(startCell);

        _borderCount = 0;
        _borders = new List<Border>();
        _borderCells = new HashSet<TerrainCell>();
        _largestBorderRectArea = 0;

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

            foreach (TerrainCell nCell in cell.Neighbors.Values)
            {
                if (exploredCells.Contains(nCell))
                {
                    continue;
                }

                if (CanAddCellToBiomeRegion(nCell, biomeId))
                {
                    cellsToExplore.Enqueue(nCell);
                }
                else
                {
                    borderCellsToExplore.Add(nCell);
                }

                exploredCells.Add(nCell);
            }

            acceptedCells.Add(cell);
        }

        foreach (TerrainCell cell in borderCellsToExplore)
        {
            TryExploreBorder(cell, biomeId);
        }

        foreach (Border border in _borders)
        {
            if (border.RectArea < _largestBorderRectArea)
            {
                if (border.RectArea <= MaxEnclosedRectArea)
                {
                    border.GetEnclosedCellSet(
                        acceptedCells,
                        out HashSet<TerrainCell> cellSet,
                        out int area);

                    if (area <= MaxEnclosedArea)
                    {
                        acceptedCells.UnionWith(cellSet);
                    }
                }
            }
        }

        CellRegion region = new CellRegion(startCell, language);

        region.AddCells(acceptedCells);

        region.EvaluateAttributes();

        region.Update();

        return region;
    }

    private static int GetRandomInt(int maxValue)
    {
        return _startCell.GetNextLocalRandomInt(_rngOffset++, maxValue);
    }

    // older versions of

    public static Region TryGenerateRegion_reduced(TerrainCell startCell, Language establishmentLanguage, string biomeId)
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
