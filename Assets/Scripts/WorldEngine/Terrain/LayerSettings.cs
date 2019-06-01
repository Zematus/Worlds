using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class LayerSettings
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute("F")]
    public float Frequency;
    [XmlAttribute("Ni")]
    public float SecondaryNoiseInfluence;

    public LayerSettings()
    {
    }

    public LayerSettings(Layer layer)
    {
        Id = layer.Id;
        Frequency = layer.Frequency;
        SecondaryNoiseInfluence = layer.SecondaryNoiseInfluence;
    }

    public void CopyValues(LayerSettings settings)
    {
        Frequency = settings.Frequency;
        SecondaryNoiseInfluence = settings.SecondaryNoiseInfluence;
    }
}
