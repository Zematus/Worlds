using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryEventMessage : CellEventMessage {

	public const string SailingDiscoveryMessagePrefix = "Sailing discovered";
	public const string TribalismDiscoveryMessagePrefix = "Tribalism discovered";
	public const string BoatMakingDiscoveryMessagePrefix = "Boat making discovered";
	public const string PlantCultivationDiscoveryMessagePrefix = "Plant cultivation discovered";

	[XmlAttribute]
	public string DiscoveryId;

	public DiscoveryEventMessage () {

	}

	public DiscoveryEventMessage (string discoveryId, TerrainCell cell, long id, long date) : base (cell, id, date) {

		DiscoveryId = discoveryId;
	}

	protected override string GenerateMessage ()
	{
		string prefix = null;

		if (DiscoveryId == SailingDiscovery.DiscoveryId) {
			prefix = SailingDiscoveryMessagePrefix;
		} else if (DiscoveryId == TribalismDiscovery.DiscoveryId) {
			prefix = TribalismDiscoveryMessagePrefix;
		} else if (DiscoveryId == BoatMakingDiscovery.DiscoveryId) {
			prefix = BoatMakingDiscoveryMessagePrefix;
		} else if (DiscoveryId == PlantCultivationDiscovery.DiscoveryId) {
			prefix = PlantCultivationDiscoveryMessagePrefix;
		} 

		if (prefix == null) {
			Debug.LogError ("Unhandled DiscoveryId: " + DiscoveryId);
		}

		Territory territory = World.GetCell (Position).EncompassingTerritory;

		if (territory != null) {
			return prefix + " in " + territory.Polity.Name.BoldText + " at " + Position;
		}

		return prefix + " at " + Position;
	}
}
