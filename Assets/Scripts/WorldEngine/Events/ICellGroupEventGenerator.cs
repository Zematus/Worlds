using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface ICellGroupEventGenerator
{
    bool CanAssignEventTypeToGroup(CellGroup group);
    CellGroupEvent GenerateAndAddEvent(CellGroup group);
}
