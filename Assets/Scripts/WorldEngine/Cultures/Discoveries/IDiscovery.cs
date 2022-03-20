using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface IDiscovery
{
    string Id { get; }
    string Name { get; }

    int UId { get; }

    void OnGain(CellGroup group);

    void OnLoss(CellGroup group);
}
