using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityEventData : WorldEventData
{
    #region OriginalDominantFactionId
    [XmlAttribute("ODFId")]
    public string OriginalDominantFactionIdStr
    {
        get { return OriginalDominantFactionId; }
        set { OriginalDominantFactionId = value; }
    }
    [XmlIgnore]
    public Identifier OriginalDominantFactionId;
    #endregion

    public PolityEventData()
    {

    }

    public PolityEventData(PolityEvent e) : base(e)
    {
        OriginalDominantFactionId = e.OriginalDominantFactionId;
    }
}
