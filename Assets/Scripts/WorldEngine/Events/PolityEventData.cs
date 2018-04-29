using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityEventData : WorldEventData {

	[XmlAttribute("OFacId")]
	public long OriginalDominantFactionId;

	public PolityEventData () {

	}

	public PolityEventData (PolityEvent e) : base (e) {

		OriginalDominantFactionId = e.OriginalDominantFactionId;
	}
}
