using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public Clan () {

	}

	public Clan (CellGroup group, Polity polity, Name name) : base (group, polity, name) {
	}

	public override void UpdateInternal () {
		
	}
}
