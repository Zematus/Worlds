using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Route : ISynchronizable
{
    //public List<WorldPosition> CellPositions = new List<WorldPosition>();

    [XmlAttribute("C")]
    public bool Consolidated = false;

    [XmlAttribute("CD")]
    public long CreationDate;

    [XmlAttribute("SLo")]
    public int StartLongitude;

    [XmlAttribute("SLa")]
    public int StartLatitude;

    [XmlIgnore]
    public float Length = 0;

    [XmlIgnore]
    public int MigrationDirectionInt = -1;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public TerrainCell FirstCell;

    [XmlIgnore]
    public TerrainCell LastCell;

    [XmlIgnore]
    public List<TerrainCell> Cells = new List<TerrainCell>();

//#if DEBUG
//    [XmlIgnore]
//    public List<string> DebugLogs = new List<string>();
//#endif

    public Direction MigrationDirection
    {
        get { return (Direction)MigrationDirectionInt; }
    }

    private const float CoastPreferenceIncrement = 400;

    private Direction _traverseDirection;

    private bool _isTraversingSea;
    private float _currentDirectionOffset;

    private float _currentCoastPreference;
    private float _currentEndRoutePreference;

    public Route()
    {

    }

    public Route(TerrainCell startCell)
    {
        World = startCell.World;

        FirstCell = startCell;
        StartLongitude = FirstCell.Longitude;
        StartLatitude = FirstCell.Latitude;

        Build();
    }

    public void Destroy()
    {
        if (!Consolidated)
            return;

        foreach (TerrainCell cell in Cells)
        {
            cell.RemoveCrossingRoute(this);
            Manager.AddUpdatedCell(cell, CellUpdateType.Route, CellUpdateSubType.All);
        }
    }

    public void Reset()
    {
        if (!Consolidated)
            return;

        foreach (TerrainCell cell in Cells)
        {
            cell.RemoveCrossingRoute(this);
            Manager.AddUpdatedCell(cell, CellUpdateType.Route, CellUpdateSubType.All);
        }

        Consolidated = false;
    }

    private void BuildInternal()
    {
//#if DEBUG
//        DebugLogs.Clear();
//#endif

        _isTraversingSea = false;
        _currentEndRoutePreference = 0;
        _currentDirectionOffset = 0;
        _currentCoastPreference = CoastPreferenceIncrement;
        Length = 0;

        //CellPositions.Clear();
        Cells.Clear();

        AddCell(FirstCell);
        LastCell = FirstCell;

        TerrainCell nextCell = FirstCell;
        Direction nextDirection;

        int rngOffset = 0;

        while (true)
        {
            //#if DEBUG
            //            TerrainCell prevCell = nextCell;
            //#endif

            nextCell = ChooseNextSeaCell(nextCell, rngOffset++, out nextDirection);

            //#if DEBUG
            //            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //            {
            //                if ((FirstCell.Longitude == Manager.TracingData.Longitude) && (FirstCell.Latitude == Manager.TracingData.Latitude))
            //                {

            //                    string cellPos = "Position: Long:" + FirstCell.Longitude + "|Lat:" + FirstCell.Latitude;
            //                    string prevCellDesc = "Position: Long:" + prevCell.Longitude + "|Lat:" + prevCell.Latitude;

            //                    string nextCellDesc = "Null";

            //                    if (nextCell != null)
            //                    {
            //                        nextCellDesc = "Position: Long:" + nextCell.Longitude + "|Lat:" + nextCell.Latitude;
            //                    }

            //                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //                        "ChooseNextSeaCell - FirstCell:" + cellPos,
            //                        "CurrentDate: " + World.CurrentDate +
            //                        ", prevCell: " + prevCellDesc +
            //                        ", nextCell: " + nextCellDesc +
            //                        "");

            //                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                }
            //            }
            //#endif

            if (nextCell == null)
            {
                LastCell = nextCell;
                break;
            }

            Length += LastCell.NeighborDistances[nextDirection];

            AddCell(nextCell);

            if (nextCell.GetBiomePresence(Biome.Ocean) <= 0)
                break;

            LastCell = nextCell;
        }

        if (nextCell != null)
        {
            MigrationDirectionInt = (int)LastCell.GetDirection(nextCell);
        }

        LastCell = nextCell;
    }

    public void Build()
    {
        CreationDate = World.CurrentDate;

        BuildInternal();
    }

    public void Consolidate()
    {
        if (Consolidated)
            return;

        foreach (TerrainCell cell in Cells)
        {
            cell.AddCrossingRoute(this);
            Manager.AddUpdatedCell(cell, CellUpdateType.Route, CellUpdateSubType.All);
        }

        Consolidated = true;
    }

    public void AddCell(TerrainCell cell)
    {
        Cells.Add(cell);
        //CellPositions.Add(cell.Position);
    }

    public TerrainCell ChooseNextSeaCell(TerrainCell currentCell, int rngOffset, out Direction direction)
    {
        if (_isTraversingSea)
            return ChooseNextDepthSeaCell(currentCell, rngOffset, out direction);
        else
            return ChooseNextCoastalCell(currentCell, rngOffset, out direction);
    }

    public TerrainCell ChooseNextDepthSeaCell(TerrainCell currentCell, int rngOffset, out Direction direction)
    {
        Direction newDirection = _traverseDirection;
        float newOffset = _currentDirectionOffset;

        float rngOutput = FirstCell.GetLocalRandomFloat(CreationDate, RngOffsets.ROUTE_CHOOSE_NEXT_DEPTH_SEA_CELL + rngOffset);

//#if DEBUG
//        string debugLog = "ChooseNextDepthSeaCell: rng=" + rngOutput +
//            ", _traverseDirection=" + _traverseDirection +
//            ", _currentDirectionOffset=" + _currentDirectionOffset;
//#endif

        float deviation = 2 * rngOutput - 1;
        deviation = (deviation * deviation + 1f) / 2f;
        deviation = newOffset - deviation;

        if (deviation >= 0.5f)
        {
            newDirection = (Direction)(((int)_traverseDirection + 1) % 8);
        }
        else if (deviation < -0.5f)
        {
            newDirection = (Direction)(((int)_traverseDirection + 6) % 8);
        }
        else if (deviation < 0)
        {
            newDirection = (Direction)(((int)_traverseDirection + 7) % 8);
        }

        TerrainCell nextCell = currentCell.GetNeighborCell(newDirection);
        direction = newDirection;

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((FirstCell.Longitude == Manager.TracingData.Longitude) && (FirstCell.Latitude == Manager.TracingData.Latitude))
        //            {

        //                string cellPos = "Position: Long:" + FirstCell.Longitude + "|Lat:" + FirstCell.Latitude;

        //                string nextCellDesc = "Null";

        //                if (nextCell != null)
        //                {
        //                    nextCellDesc = "Position: Long:" + nextCell.Longitude + "|Lat:" + nextCell.Latitude;
        //                }

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ChooseNextDepthSeaCell - FirstCell:" + cellPos,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", deviation: " + deviation +
        //                    ", nextCell: " + nextCellDesc +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (nextCell == null)
        {
//#if DEBUG
//            DebugLogs.Add(debugLog);
//#endif
            return null;
        }

        if (Cells.Contains(nextCell))
        {
//#if DEBUG
//            DebugLogs.Add(debugLog);
//#endif
            return null;
        }

        if (nextCell.IsPartOfCoastline)
        {
            _currentCoastPreference += CoastPreferenceIncrement;
            _currentEndRoutePreference += 0.1f;

            _isTraversingSea = false;
        }

//#if DEBUG
//        debugLog += ", _currentCoastPreference=" + _currentCoastPreference + 
//            ", _currentEndRoutePreference=" + _currentEndRoutePreference;

//        DebugLogs.Add(debugLog);
//#endif

        return nextCell;
    }

    private class CoastalCellValue : CollectionUtility.ElementWeightPair<KeyValuePair<Direction, TerrainCell>>
    {
        public CoastalCellValue(KeyValuePair<Direction, TerrainCell> pair, float weight) : base(pair, weight)
        {

        }
    }

    public TerrainCell ChooseNextCoastalCell(TerrainCell currentCell, int rngOffset, out Direction direction)
    {
        float totalWeight = 0;

        List<CoastalCellValue> coastalCellWeights = new List<CoastalCellValue>(currentCell.Neighbors.Count);

        //#if DEBUG
        //        string cellWeightsStr = "";
        //#endif

        foreach (KeyValuePair<Direction, TerrainCell> nPair in currentCell.Neighbors)
        {
            TerrainCell nCell = nPair.Value;

            if (Cells.Contains(nCell))
                continue;

            float oceanPresence = nCell.GetBiomePresence(Biome.Ocean);

            float weight = oceanPresence;

            if (nCell.IsPartOfCoastline)
                weight *= _currentCoastPreference;

            weight += (1f - oceanPresence) * _currentEndRoutePreference;

            coastalCellWeights.Add(new CoastalCellValue(nPair, weight));

            //#if DEBUG
            //            cellWeightsStr += "\n\tnCell Direction: " + nPair.Key + " - Position: " + nCell.Position + " - Weight: " + weight;
            //#endif

            totalWeight += weight;
        }

        //#if DEBUG
        //        cellWeightsStr += "\n";
        //#endif

//#if DEBUG
//        string debugLog = "ChooseNextCoastalCell: Cells.Count=" + Cells.Count +
//            ", coastalCellWeights.Count=" + coastalCellWeights.Count +
//            ", totalWeight=" + totalWeight;
//#endif

        if (coastalCellWeights.Count == 0)
        {
            direction = Direction.South;

//#if DEBUG
//            DebugLogs.Add(debugLog);
//#endif

            return null;
        }

        if (totalWeight <= 0)
        {
            direction = Direction.South;

//#if DEBUG
//            DebugLogs.Add(debugLog);
//#endif

            return null;
        }

        float rngOutput = FirstCell.GetLocalRandomFloat(CreationDate, RngOffsets.ROUTE_CHOOSE_NEXT_COASTAL_CELL + rngOffset);

//#if DEBUG
//        debugLog += ", rng1=" + rngOutput;
//#endif

        KeyValuePair<Direction, TerrainCell> targetPair =
            CollectionUtility.WeightedSelection(coastalCellWeights.ToArray(), totalWeight, rngOutput);

        TerrainCell targetCell = targetPair.Value;
        direction = targetPair.Key;

        if (targetCell == null)
        {
            throw new System.Exception("targetCell is null");
        }

        if (!targetCell.IsPartOfCoastline)
        {
            _isTraversingSea = true;
            _traverseDirection = direction;

            rngOutput = FirstCell.GetLocalRandomFloat(CreationDate, RngOffsets.ROUTE_CHOOSE_NEXT_COASTAL_CELL_2 + rngOffset);

//#if DEBUG
//            debugLog += ", rng2=" + rngOutput + ", _traverseDirection=" + _traverseDirection;
//#endif

            _currentDirectionOffset = rngOutput;
        }

        _currentEndRoutePreference += 0.1f;

//#if DEBUG
//        debugLog += ", _currentEndRoutePreference=" + _currentEndRoutePreference;

//        DebugLogs.Add(debugLog);
//#endif

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((FirstCell.Longitude == Manager.TracingData.Longitude) && (FirstCell.Latitude == Manager.TracingData.Latitude))
        //            {
        //                string cellPos = "Position: Long:" + FirstCell.Longitude + "|Lat:" + FirstCell.Latitude;
        //                string currentCellDesc = "Position: Long:" + currentCell.Longitude + "|Lat:" + currentCell.Latitude;

        //                string targetCellDesc = "Null";

        //                if (targetCell != null)
        //                {
        //                    targetCellDesc = "Position: Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
        //                }

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ChooseNextCoastalCell - FirstCell:" + cellPos,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", currentCell: " + currentCellDesc +
        //                    ", targetCell: " + targetCellDesc +
        //                    ", _currentEndRoutePreference: " + _currentEndRoutePreference +
        //                    //					", nCells: " + cellWeightsStr + 
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        return targetCell;
    }

    public bool ContainsCell(TerrainCell cell)
    {
        return Cells.Contains(cell);
    }

    public void Synchronize()
    {

    }

    public void FinalizeLoad()
    {
        if (!Consolidated)
        {
            throw new System.Exception("Can't finalize unconsolidated route");
        }

        //TerrainCell currentCell = null;

        //bool first = true;

        //if (CellPositions.Count == 0)
        //{
        //    throw new System.Exception("CellPositions is empty");
        //}

        //foreach (WorldPosition p in CellPositions)
        //{
        //    currentCell = World.GetCell(p);

        //    if (currentCell == null)
        //    {
        //        Debug.LogError("Unable to find terrain cell at [" + currentCell.Longitude + "," + currentCell.Latitude + "]");
        //    }

        //    if (first)
        //    {
        //        FirstCell = currentCell;
        //        first = false;
        //    }

        //    Cells.Add(currentCell);
        //}

        FirstCell = World.GetCell(StartLongitude, StartLatitude);

        BuildInternal();

        foreach (TerrainCell cell in Cells)
        {
            cell.AddCrossingRoute(this);
        }

        //LastCell = currentCell;
    }
}
