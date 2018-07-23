using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class WorldEventData {

	[XmlAttribute("TId")]
	public long TypeId;

    [XmlAttribute("SDate")]
    public long SpawnDate;

    [XmlAttribute("TDate")]
	public long TriggerDate;

	public WorldEventData () {
	
	}

	public WorldEventData (WorldEvent e) {

		TypeId = e.TypeId;
        SpawnDate = e.SpawnDate;
		TriggerDate = e.TriggerDate;
	}
}
