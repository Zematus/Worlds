using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class HumanGroup {
	
	[XmlAttribute]
	public int Population;
	
	[XmlIgnore]
	public TerrainCell Cell;

	public HumanGroup () {
	

	}
}
