using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEventMessage
{
    public const long FactionSplitEventMessageId = 7;

    [XmlAttribute]
    public long Id;

    [XmlAttribute]
    public long Date;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public string Message => GenerateMessage();

    public WorldEventMessage()
    {

    }

    public WorldEventMessage(World world, long id, long date)
    {
        World = world;
        Id = id;
        Date = date;
    }

    protected abstract string GenerateMessage();
}
