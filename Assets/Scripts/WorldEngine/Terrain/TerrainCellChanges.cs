using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TerrainCellChanges
{
    [XmlAttribute("Lon")]
    public int Longitude;
    [XmlAttribute("Lat")]
    public int Latitude;

    [XmlAttribute("BA")]
    public float BaseAltitudeValue;
    [XmlAttribute("BT")]
    public float BaseTemperatureValue;
    [XmlAttribute("BR")]
    public float BaseRainfallValue;

    [XmlAttribute("OT")]
    public float BaseTemperatureOffset;
    [XmlAttribute("OR")]
    public float BaseRainfallOffset;

    [XmlAttribute("A")]
    public float Altitude;
    [XmlAttribute("T")]
    public float Temperature;
    [XmlAttribute("R")]
    public float Rainfall;

    [XmlAttribute("Fp")]
    public float FarmlandPercentage = 0;

    public List<string> Flags = new List<string>();

    public TerrainCellChanges()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    public TerrainCellChanges(TerrainCell cell)
    {
        Longitude = cell.Longitude;
        Latitude = cell.Latitude;

        BaseAltitudeValue = cell.BaseAltitudeValue;
        BaseTemperatureValue = cell.BaseTemperatureValue;
        BaseRainfallValue = cell.BaseRainfallValue;

        BaseTemperatureOffset = cell.BaseTemperatureOffset;
        BaseRainfallOffset = cell.BaseRainfallOffset;

        Altitude = cell.Altitude;
        Temperature = cell.Temperature;
        Rainfall = cell.Rainfall;
        
        FarmlandPercentage = cell.FarmlandPercentage;
    }
}
