using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionEventData : WorldEventData
{
    #region OriginalPolityId
    [XmlAttribute("OPId")]
    public string OriginalPolityIdStr
    {
        get { return OriginalPolityId; }
        set { OriginalPolityId = value; }
    }
    [XmlIgnore]
    public Identifier OriginalPolityId;
    #endregion

    public FactionEventData()
    {

    }

    public FactionEventData(FactionEvent e) : base(e)
    {
        OriginalPolityId = e.OriginalPolityId;
    }
}
