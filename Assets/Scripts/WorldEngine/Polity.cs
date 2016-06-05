using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityInfluence {

	[XmlAttribute]
	public long PolityId;
	[XmlAttribute]
	public float Value;

	[XmlIgnore]
	public Polity Polity;

	public PolityInfluence () {

	}

	public PolityInfluence (Polity polity, float value) {
	
		PolityId = polity.Id;
		Polity = polity;
		Value = value;
	}
}

public abstract class Polity : Synchronizable {

	public const float MinPolityInfluence = 0.001f;

	[XmlAttribute]
	public long Id;

	[XmlAttribute]
	public long CoreGroupId;

	[XmlAttribute]
	public float TotalGroupInfluenceValue = 0;

	[XmlAttribute]
	public float TotalPopulation = 0;

	public List<long> InfluencedGroupIds;

	public Territory Territory = new Territory ();

	public PolityCulture Culture;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public HashSet<CellGroup> InfluencedGroups = new HashSet<CellGroup> ();

	public Polity () {
	
	}

	public Polity (CellGroup coreGroup, float coreGroupInfluenceValue) {

		World = coreGroup.World;

		Id = World.GeneratePolityId ();

		coreGroup.SetPolityInfluenceValue (this, coreGroupInfluenceValue);

		SetCoreGroup (coreGroup);

		Culture = new PolityCulture (this);
	}

	public void Destroy () {
		
		World.RemovePolity (this);

		foreach (CellGroup group in InfluencedGroups) {

			group.RemovePolityInfluence (this);
		}
	}

	public void SetCoreGroup (CellGroup group) {

		if (!InfluencedGroups.Contains (group))
			throw new System.Exception ("Group is not part of polity's influenced groups");

		CoreGroup = group;

		CoreGroupId = group.Id;
	}

	public void Update () {

		if (InfluencedGroups.Count <= 0) {
		
			World.AddPolityToRemove (this);

			return;
		}

		RunPopulationCensus ();
	
		Culture.Update ();
	}

	public void RunPopulationCensus () {

		TotalPopulation = 0;
	
		foreach (CellGroup group in InfluencedGroups) {

			TotalPopulation += group.Population * group.GetPolityInfluenceValue (this);
		}
	}

	public void AddInfluencedGroup (CellGroup group) {
	
		InfluencedGroups.Add (group);

		Territory.AddCell (group.Cell);
	}

	public void RemoveInfluencedGroup (CellGroup group) {

		InfluencedGroups.Remove (group);

		Territory.RemoveCell (group.Cell);
	}

	public virtual void Synchronize () {

		Culture.Synchronize ();

		InfluencedGroupIds = new List<long> (InfluencedGroups.Count);

		foreach (CellGroup g in InfluencedGroups) {

			InfluencedGroupIds.Add (g.Id);
		}
	}

	public virtual void FinalizeLoad () {

		CoreGroup = World.GetGroup (CoreGroupId);

		if (CoreGroup == null) {
			throw new System.Exception ("Missing Group with Id " + CoreGroupId);
		}

		foreach (int id in InfluencedGroupIds) {

			CellGroup group = World.GetGroup (id);

			if (group == null) {
				throw new System.Exception ("Missing Group with Id " + id);
			}

			InfluencedGroups.Add (group);
		}

		Culture.World = World;
		Culture.Polity = this;
		Culture.FinalizeLoad ();
	}

	public abstract float MigrationValue (TerrainCell targetCell, float sourceValue);
	public abstract void MergingEffects (CellGroup targetGroup, float sourceValue, float percentOfTarget);
	public abstract void UpdateEffects (CellGroup group, float influenceValue, int timeSpan);
}

public class Territory {

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	[XmlIgnore]
	public World World;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public Territory () {
	
	}

	public Territory (World world) {

		World = world;
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		CellPositions.Add (cell.Position);

		cell.AddEncompassingTerritory (this);

		return true;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		CellPositions.Remove (cell.Position);

		cell.RemoveEncompassingTerritory (this);

		return true;
	}

	public void FinalizeLoad () {

		foreach (WorldPosition position in CellPositions) {

			TerrainCell cell = World.GetCell (position);

			if (cell == null) {
				throw new System.Exception ("Cell missing at position " + position.Longitude + "," + position.Latitude);
			}
		
			_cells.Add (cell);

			cell.AddEncompassingTerritory (this);
		}
	}
}
