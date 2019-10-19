using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class TerrainCellAlteration
{
    [ProtoMember(1)]
    public int Longitude;
    [ProtoMember(2)]
    public int Latitude;

    [ProtoMember(3)]
    public float BaseAltitudeValue;
    [ProtoMember(4)]
    public float BaseTemperatureValue;
    [ProtoMember(5)]
    public float BaseRainfallValue;

    [ProtoMember(6)]
    public float BaseTemperatureOffset;
    [ProtoMember(7)]
    public float BaseRainfallOffset;

    [ProtoMember(8)]
    public float Altitude;
    [ProtoMember(9)]
    public float OriginalAltitude;
    [ProtoMember(10)]
    public float Temperature;
    [ProtoMember(11)]
    public float OriginalTemperature;
    [ProtoMember(12)]
    public float Rainfall;
    [ProtoMember(13)]
    public float WaterAccumulation;

    [ProtoMember(14)]
    public float FarmlandPercentage = 0;
    [ProtoMember(15)]
    public float Arability = 0;
    [ProtoMember(16)]
    public float Accessibility = 0;

    [ProtoMember(17)]
    public bool Modified;

    [ProtoMember(18)]
    private List<CellLayerData> _LayerData;
    public List<CellLayerData> LayerData
    {
        get => _LayerData ?? (_LayerData = new List<CellLayerData>());
        set => _LayerData = value;
    }

    public WorldPosition Position;

    public TerrainCellAlteration()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    public TerrainCellAlteration(TerrainCell cell, bool addLayerData = true)
    {
        Longitude = cell.Longitude;
        Latitude = cell.Latitude;

        Position = cell.Position;

        BaseAltitudeValue = cell.BaseAltitudeValue;
        BaseTemperatureValue = cell.BaseTemperatureValue;
        BaseRainfallValue = cell.BaseRainfallValue;

        BaseTemperatureOffset = cell.BaseTemperatureOffset;
        BaseRainfallOffset = cell.BaseRainfallOffset;

        OriginalAltitude = cell.OriginalAltitude;
        Altitude = cell.Altitude;
        Temperature = cell.Temperature;
        OriginalTemperature = cell.OriginalTemperature;
        Rainfall = cell.Rainfall;
        WaterAccumulation = cell.WaterAccumulation;
        
        FarmlandPercentage = cell.FarmlandPercentage;
        Accessibility = cell.Accessibility;
        Arability = cell.Arability;

        if (addLayerData)
        {
            foreach (CellLayerData data in cell.LayerData)
            {
                if (data.Offset == 0) continue;

                LayerData.Add(new CellLayerData(data));
            }
        }

        Modified = cell.Modified;
    }
}
