using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionEventData : WorldEventData
{
    public Identifier OriginalPolityId;

    public FactionEventData()
    {

    }

    public FactionEventData(FactionEvent e) : base(e)
    {
        OriginalPolityId = e.OriginalPolityId;
    }
}
