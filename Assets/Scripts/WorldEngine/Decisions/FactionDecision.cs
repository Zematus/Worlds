using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionDecision : Decision {

	public Faction Faction;

	public FactionDecision (Faction faction) : base () {

		Faction = faction;
	}
}
