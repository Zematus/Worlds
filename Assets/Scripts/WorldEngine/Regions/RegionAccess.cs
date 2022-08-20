using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class RegionAccess
{
    #region RegionId
    [XmlAttribute("RId")]
    public string RegionIdStr
    {
        get { return RegionId; }
        set { RegionId = value; }
    }
    [XmlIgnore]
    public Identifier RegionId;
    #endregion

    [XmlAttribute("C")]
    public int Count;

    [XmlIgnore]
    public Region Region;
}
