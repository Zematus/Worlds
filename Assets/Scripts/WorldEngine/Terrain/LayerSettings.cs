using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using ProtoBuf;

[ProtoContract]
public class LayerSettings
{
    [ProtoMember(1)]
    public string Id;

    [ProtoMember(2)]
    public float Frequency;
    [ProtoMember(3)]
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
