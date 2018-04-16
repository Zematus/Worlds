using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class WorldEventSnapshot {

	public System.Type EventType;

	public long TriggerDate;
	public long SpawnDate;
	public long Id;

	public WorldEventSnapshot (WorldEvent e) {

		EventType = e.GetType ();
	
		TriggerDate = e.TriggerDate;
		SpawnDate = e.SpawnDate;
		Id = e.Id;
	}
}
