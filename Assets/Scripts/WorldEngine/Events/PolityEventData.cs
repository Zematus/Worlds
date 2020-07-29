using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityEventData : WorldEventData
{
    public Identifier OriginalDominantFactionId;

    public PolityEventData()
    {

    }

    public PolityEventData(PolityEvent e) : base(e)
    {
        OriginalDominantFactionId = e.OriginalDominantFactionId;
    }
}
