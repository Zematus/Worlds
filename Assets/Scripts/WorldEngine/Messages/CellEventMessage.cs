using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class CellEventMessage : WorldEventMessage
{
    public WorldPosition Position;

    public CellEventMessage()
    {

    }

    public CellEventMessage(TerrainCell cell, long id, long date) : base(cell.World, id, date)
    {
        Position = cell.Position;
    }
}
