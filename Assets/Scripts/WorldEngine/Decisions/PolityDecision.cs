using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class PolityDecision : Decision {

	public Polity Polity;

	public PolityDecision (Polity polity) : base () {

		Polity = polity;
	}
}
