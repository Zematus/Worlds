using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Event : WorldEvent
{
    public const string FactionTargetType = "faction";
    public const string GroupTargetType = "group";

    public override void Trigger()
    {
        throw new System.NotImplementedException();
    }
}
