using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroupEventSnapshot : WorldEventSnapshot
{
    public Identifier GroupId;
    public CellGroupSnapshot GroupSnapshot;

    public CellGroupEventSnapshot(CellGroupEvent e) : base(e)
    {

        GroupId = e.GroupId;
        GroupSnapshot = e.Group.GetSnapshot();
    }
}
