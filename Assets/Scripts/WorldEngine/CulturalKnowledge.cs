using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalKnowledgeInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;
	
	public CulturalKnowledgeInfo () {
	}
	
	public CulturalKnowledgeInfo (string id, string name) {
		
		Id = id;
		
		Name = name;
	}
	
	public CulturalKnowledgeInfo (CulturalKnowledgeInfo baseInfo) {
		
		Id = baseInfo.Id;
		
		Name = baseInfo.Name;
	}
}

public abstract class CulturalKnowledge : CulturalKnowledgeInfo {
	
	[XmlAttribute]
	public float Value;
	
	[XmlAttribute]
	public float ProgressLevel;
	
	[XmlAttribute]
	public float Asymptote;

	[XmlIgnore]
	public CellGroup Group;
	
	public CulturalKnowledge () {

	}

	public CulturalKnowledge (CellGroup group, string id, string name, float value) : base (id, name) {

		Group = group;
		Value = value;
	}
	
	public CulturalKnowledge GenerateCopy (CellGroup targetGroup) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = knowledgeType.GetConstructor (new System.Type[] {typeof(CellGroup), knowledgeType});
		
		return cInfo.Invoke (new object[] {targetGroup, this}) as CulturalKnowledge;
	}
	
	public CulturalKnowledge GenerateCopy (CellGroup targetGroup, float initialValue) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = knowledgeType.GetConstructor (new System.Type[] {typeof(CellGroup), knowledgeType, typeof(float)});
		
		return cInfo.Invoke (new object[] {targetGroup, this, initialValue}) as CulturalKnowledge;
	}
	
	public float GetHighestAsymptote () {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.FieldInfo fInfo = knowledgeType.GetField ("HighestAsymptote");
		
		return (float)fInfo.GetValue (this);
	}
	
	public void SetHighestAsymptote (float value) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.FieldInfo fInfo = knowledgeType.GetField ("HighestAsymptote");
		
		float currentValue = (float)fInfo.GetValue (this);
		fInfo.SetValue (this, Mathf.Max (value, currentValue));
	}

	public void Merge (CulturalKnowledge knowledge, float percentage) {
	
		Value = Value * (1f - percentage) + knowledge.Value * percentage;
	}
	
	public void IncreaseValue (float targetValue, float percentage) {

		if (targetValue > Value) {

			Value += (targetValue - Value) * percentage;
		}
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public virtual void FinalizeLoad () {

	}
	
	public void UpdateProgressLevel () {

		if (Asymptote <= 0)
			throw new System.Exception ("Asymptote is equal or less than 0");

		ProgressLevel = Value / Asymptote;
	}
	
	public void RecalculateAsymptote () {

		Asymptote = 0;
		
		Group.Culture.Discoveries.ForEach (d => Asymptote = Mathf.Max (CalculateAsymptoteInternal (d), Asymptote));

		UpdateProgressLevel ();

		SetHighestAsymptote (Asymptote);
	}
	
	public void CalculateAndSetAsymptote (CulturalDiscovery discovery) {

		Asymptote = Mathf.Max (CalculateAsymptoteInternal (discovery), Asymptote);

		UpdateProgressLevel ();
		
		SetHighestAsymptote (Asymptote);
	}

	public void Update (int timeSpan) {

		UpdateInternal (timeSpan);
		
		UpdateProgressLevel ();
	}

	protected abstract void UpdateInternal (int timeSpan);
	public abstract float CalculateModifiedProgressLevel ();
	public abstract float CalculateTransferFactor ();
	
	protected abstract float CalculateAsymptoteInternal (CulturalDiscovery discovery);
}

public class ShipbuildingKnowledge : CulturalKnowledge {

	public const string ShipbuildingKnowledgeId = "ShipbuildingKnowledge";
	public const string ShipbuildingKnowledgeName = "Shipbuilding";

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public static float HighestAsymptote = 0;

	private float _neighborhoodOceanPresence;
	
	public ShipbuildingKnowledge () {

		if (Asymptote > HighestAsymptote) {
			
			HighestAsymptote = Asymptote;
		}
	}

	public ShipbuildingKnowledge (CellGroup group, float value = 0f) : base (group, ShipbuildingKnowledgeId, ShipbuildingKnowledgeName, value) {
		
		CalculateNeighborhoodOceanPresence ();
	}

	public ShipbuildingKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.Value) {
		
		CalculateNeighborhoodOceanPresence ();
	}
	
	public ShipbuildingKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {
		
		CalculateNeighborhoodOceanPresence ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodOceanPresence ();
	}
	
	public void CalculateNeighborhoodOceanPresence () {
		
		_neighborhoodOceanPresence = CalculateNeighborhoodOceanPresenceIn (Group);
	}
	
	public static float CalculateNeighborhoodOceanPresenceIn (CellGroup group) {

		float neighborhoodPresence;
		
		int groupCellBonus = 1;
		int cellCount = groupCellBonus;
		
		TerrainCell groupCell = group.Cell;
		
		float totalPresence = groupCell.GetBiomePresence ("Ocean") * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {
			
			totalPresence += c.GetBiomePresence ("Ocean");
			cellCount++;
		}
		
		neighborhoodPresence = totalPresence / cellCount;
		
		if ((neighborhoodPresence < 0) || (neighborhoodPresence > 1)) {
			
			throw new System.Exception ("Neighborhood Ocean Presence outside range: " + neighborhoodPresence);
		}

		return neighborhoodPresence;
	}

	protected override void UpdateInternal (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifierFactor1 = 0.75f;
		float randomModifierFactor2 = 1f;
		float randomModifier = randomModifierFactor1 * groupCell.GetNextLocalRandomFloat ();
		randomModifier = randomModifierFactor2 * (_neighborhoodOceanPresence - randomModifier);
		randomModifier = Mathf.Clamp (randomModifier, -1, 1);

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (Asymptote - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}
		
		targetValue = Mathf.Clamp (targetValue, 0, Asymptote);
		
		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * _neighborhoodOceanPresence;
		
		Value = (Value * (1 - factor)) + (targetValue * factor);

		if (Value < SailingDiscoveryEvent.MinShipBuildingKnowledgeSpawnEventValue)
			return;
		
		if (Value > SailingDiscoveryEvent.OptimalShipBuildingKnowledgeValue)
			return;

		if (SailingDiscoveryEvent.CanSpawnIn (Group)) {
			
			int triggerDate = SailingDiscoveryEvent.CalculateTriggerDate (Group);
			
			Group.World.InsertEventToHappen (new SailingDiscoveryEvent (Group, triggerDate));
		}
	}

	protected override float CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case BoatMakingDiscovery.BoatMakingDiscoveryId:
			return 10;
		case SailingDiscovery.SailingDiscoveryId:
			return 30;
		}

		return 0;
	}

	public override float CalculateModifiedProgressLevel ()
	{
		float oceanPresenceFactor = (_neighborhoodOceanPresence * 0.9f) + 0.1f;

		return Mathf.Min (ProgressLevel / oceanPresenceFactor, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_neighborhoodOceanPresence * 0.9f) + 0.1f;
	}
}

public class AgricultureKnowledge : CulturalKnowledge {

	public const string AgricultureKnowledgeId = "AgricultureKnowledge";
	public const string AgricultureKnowledgeName = "Agriculture";

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public static float HighestAsymptote = 0;

	private float _neighborhoodOceanPresence;

	public AgricultureKnowledge () {

		if (Asymptote > HighestAsymptote) {

			HighestAsymptote = Asymptote;
		}
	}

	public AgricultureKnowledge (CellGroup group, float value = 0f) : base (group, AgricultureKnowledgeId, AgricultureKnowledgeName, value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public AgricultureKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.Value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public AgricultureKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

		CalculateNeighborhoodOceanPresence ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodOceanPresence ();
	}

	public void CalculateNeighborhoodOceanPresence () {

		_neighborhoodOceanPresence = CalculateNeighborhoodOceanPresenceIn (Group);
	}

	public static float CalculateNeighborhoodOceanPresenceIn (CellGroup group) {

		float neighborhoodPresence;

		int groupCellBonus = 1;
		int cellCount = groupCellBonus;

		TerrainCell groupCell = group.Cell;

		float totalPresence = groupCell.GetBiomePresence ("Ocean") * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {

			totalPresence += c.GetBiomePresence ("Ocean");
			cellCount++;
		}

		neighborhoodPresence = totalPresence / cellCount;

		if ((neighborhoodPresence < 0) || (neighborhoodPresence > 1)) {

			throw new System.Exception ("Neighborhood Ocean Presence outside range: " + neighborhoodPresence);
		}

		return neighborhoodPresence;
	}

	protected override void UpdateInternal (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifierFactor1 = 0.75f;
		float randomModifierFactor2 = 1f;
		float randomModifier = randomModifierFactor1 * groupCell.GetNextLocalRandomFloat ();
		randomModifier = randomModifierFactor2 * (_neighborhoodOceanPresence - randomModifier);
		randomModifier = Mathf.Clamp (randomModifier, -1, 1);

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (Asymptote - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		targetValue = Mathf.Clamp (targetValue, 0, Asymptote);

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * _neighborhoodOceanPresence;

		Value = (Value * (1 - factor)) + (targetValue * factor);

		if (Value < SailingDiscoveryEvent.MinShipBuildingKnowledgeSpawnEventValue)
			return;

		if (Value > SailingDiscoveryEvent.OptimalShipBuildingKnowledgeValue)
			return;

		if (SailingDiscoveryEvent.CanSpawnIn (Group)) {

			int triggerDate = SailingDiscoveryEvent.CalculateTriggerDate (Group);

			Group.World.InsertEventToHappen (new SailingDiscoveryEvent (Group, triggerDate));
		}
	}

	protected override float CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case BoatMakingDiscovery.BoatMakingDiscoveryId:
			return 10;
		case SailingDiscovery.SailingDiscoveryId:
			return 30;
		}

		return 0;
	}

	public override float CalculateModifiedProgressLevel ()
	{
		float oceanPresenceFactor = (_neighborhoodOceanPresence * 0.9f) + 0.1f;

		return Mathf.Min (ProgressLevel / oceanPresenceFactor, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_neighborhoodOceanPresence * 0.9f) + 0.1f;
	}
}
