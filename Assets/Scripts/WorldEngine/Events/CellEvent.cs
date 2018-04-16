using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class CellEvent : WorldEvent {

	[XmlAttribute]
	public int CellLongitude;
	[XmlAttribute]
	public int CellLatitude;

	[XmlIgnore]
	public TerrainCell Cell;

	public CellEvent () {

	}

	public CellEvent (TerrainCell cell, long triggerDate, long eventTypeId) : base (cell.World, triggerDate, cell.GenerateUniqueIdentifier (triggerDate, 1000L, eventTypeId), eventTypeId) {

		Cell = cell;
		CellLongitude = cell.Longitude;
		CellLatitude = cell.Latitude;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			string cellLoc = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("CellEvent - Cell: " + cellLoc, "TriggerDate: " + TriggerDate);
//
//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//		}
//		#endif
	}
}
