using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellLayerData
{
    [XmlAttribute]
    public string Id;

    [XmlIgnore]
    public float Value;
    [XmlIgnore]
    public float BaseValue;

    [XmlAttribute("O")]
    public float Offset;

    public CellLayerData()
    {
    }

    public CellLayerData(CellLayerData source)
    {
        Id = source.Id;
        Value = source.Value;
        BaseValue = source.BaseValue;
        Offset = source.Offset;
    }
}
