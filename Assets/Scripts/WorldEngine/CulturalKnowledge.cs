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

public abstract class CulturalKnowledge : CulturalKnowledgeInfo, Synchronizable {

	public const float MinValue = 0.001f;
	
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

	public CulturalKnowledge (CellGroup group, string id, string name, float value, float asymptote) : base (id, name) {

		Group = group;
		Value = value;
		Asymptote = asymptote;
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

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {

	}
	
	public void UpdateProgressLevel () {

		ProgressLevel = 0;

		if (Asymptote > 0)
			ProgressLevel = Value / Asymptote;
	}
	
	public void RecalculateAsymptote () {

		Asymptote = CalculateBaseAsymptote ();

		Group.Culture.Discoveries.ForEach (d => Asymptote = Mathf.Max (CalculateAsymptoteInternal (d), Asymptote));

		UpdateProgressLevel ();

		SetHighestAsymptote (Asymptote);
	}

	public void CalculateAsymptote (CulturalDiscovery discovery) {

		float newAsymptote = CalculateAsymptoteInternal (discovery);

		if (newAsymptote > Asymptote) {

			Asymptote = newAsymptote;

			UpdateProgressLevel ();

			SetHighestAsymptote (Asymptote);
		}
	}

	public void Update (int timeSpan) {

		UpdateInternal (timeSpan);
		
		UpdateProgressLevel ();
	}

	protected void UpdateValue (int timeSpan, float timeEffectFactor, float specificModifier) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
		randomModifier *= randomModifier;
		float randomFactor = specificModifier - randomModifier;
		randomFactor = Mathf.Clamp (randomFactor, -1, 1);

		float maxTargetValue = Asymptote;
		float minTargetValue = -0.2f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = Value + (maxTargetValue - Value) * randomFactor;
		} else {
			targetValue = Value - (minTargetValue - Value) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		Value = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
	}

	public abstract float CalculateExpectedProgressLevel ();
	public abstract float CalculateTransferFactor ();

	public abstract bool WillBeLost ();
	public abstract void LossConsequences ();

	protected abstract void UpdateInternal (int timeSpan);
	protected abstract float CalculateAsymptoteInternal (CulturalDiscovery discovery);
	protected abstract float CalculateBaseAsymptote ();
}

public class ShipbuildingKnowledge : CulturalKnowledge {

	public const string ShipbuildingKnowledgeId = "ShipbuildingKnowledge";
	public const string ShipbuildingKnowledgeName = "Shipbuilding";

	public const float MinKnowledgeValueForSailing = 3;
	public const float OptimalKnowledgeValueForSailing = 10;

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;
	public const float NeighborhoodOceanPresenceModifier = 1.5f;

	public static float HighestAsymptote = 0;

	private float _neighborhoodOceanPresence;
	
	public ShipbuildingKnowledge () {

		if (Asymptote > HighestAsymptote) {
			
			HighestAsymptote = Asymptote;
		}
	}

	public ShipbuildingKnowledge (CellGroup group, float value = 1f) : base (group, ShipbuildingKnowledgeId, ShipbuildingKnowledgeName, value) {
		
		CalculateNeighborhoodOceanPresence ();
	}

	public ShipbuildingKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.Value, baseKnowledge.Asymptote) {
		
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

		UpdateValue (timeSpan, TimeEffectConstant, _neighborhoodOceanPresence * NeighborhoodOceanPresenceModifier);

		TryGenerateSailingDiscoveryEvent ();
	}

	private void TryGenerateSailingDiscoveryEvent () {

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

	public override float CalculateExpectedProgressLevel ()
	{
		if (_neighborhoodOceanPresence <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / _neighborhoodOceanPresence, MinValue, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_neighborhoodOceanPresence * 0.9f) + 0.1f;
	}

	public override bool WillBeLost ()
	{
		if (Value < 0) {

			return true;
		}

		if ((Value < MinValue) && (_neighborhoodOceanPresence <= 0)) {

			return true;
		}

		return false;
	}

	public override void LossConsequences ()
	{
		if (BoatMakingDiscoveryEvent.CanSpawnIn (Group)) {

			int triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate (Group);

			Group.World.InsertEventToHappen (new BoatMakingDiscoveryEvent (Group, triggerDate));
		}
	}

	protected override float CalculateBaseAsymptote ()
	{
		return 0;
	}
}

public class AgricultureKnowledge : CulturalKnowledge {

	public const string AgricultureKnowledgeId = "AgricultureKnowledge";
	public const string AgricultureKnowledgeName = "Agriculture";

	public const float TimeEffectConstant = CellGroup.GenerationTime * 2000;
	public const float TerrainFactorModifier = 1.5f;
	public const float MinAccesibility = 0.2f;

	public static float HighestAsymptote = 0;

	private float _terrainFactor;

	public AgricultureKnowledge () {

		if (Asymptote > HighestAsymptote) {

			HighestAsymptote = Asymptote;
		}
	}

	public AgricultureKnowledge (CellGroup group, float value = 1f) : base (group, AgricultureKnowledgeId, AgricultureKnowledgeName, value) {

		CalculateTerrainFactor ();
	}

	public AgricultureKnowledge (CellGroup group, AgricultureKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.Value, baseKnowledge.Asymptote) {

		CalculateTerrainFactor ();
	}

	public AgricultureKnowledge (CellGroup group, AgricultureKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

		CalculateTerrainFactor ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateTerrainFactor ();
	}

	public void CalculateTerrainFactor () {

		_terrainFactor = CalculateTerrainFactorIn (Group.Cell);
	}

	public static float CalculateTerrainFactorIn (TerrainCell cell) {

		float accesibilityFactor = (cell.Accessibility - MinAccesibility) / (1f - MinAccesibility);

		return Mathf.Clamp01 (cell.Arability * cell.Accessibility * accesibilityFactor);
	}

	protected override void UpdateInternal (int timeSpan) {

		UpdateValue (timeSpan, TimeEffectConstant, _terrainFactor * TerrainFactorModifier);
	}

	protected override float CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case PlantCultivationDiscovery.PlantCultivationDiscoveryId:
			return 10;
		}

		return 0;
	}

	public override float CalculateExpectedProgressLevel ()
	{
		if (_terrainFactor <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / _terrainFactor, MinValue, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_terrainFactor * 0.9f) + 0.1f;
	}

	public override bool WillBeLost ()
	{
		if (Value < 0) {
		
			return true;
		}

		if ((Value < MinValue) && (_terrainFactor <= 0)) {

			return true;
		}

		return false;
	}

	public override void LossConsequences ()
	{
		Group.Culture.RemoveActivity (CulturalActivity.FarmingActivityId);

		if (PlantCultivationDiscoveryEvent.CanSpawnIn (Group)) {

			int triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate (Group);

			Group.World.InsertEventToHappen (new PlantCultivationDiscoveryEvent (Group, triggerDate));
		}

		if (FarmDegradationEvent.CanSpawnIn (Group.Cell)) {

			int triggerDate = FarmDegradationEvent.CalculateTriggerDate (Group.Cell);

			Group.World.InsertEventToHappen (new FarmDegradationEvent (Group.Cell, triggerDate));
		}
	}

	protected override float CalculateBaseAsymptote ()
	{
		return 0;
	}
}

public class SocialOrganizationKnowledge : CulturalKnowledge {

	public const string SocialOrganizationKnowledgeId = "SocialOrganizationKnowledge";
	public const string SocialOrganizationKnowledgeName = "Social Organization";

	public const float MinKnowledgeValueForTribalism = 4;
	public const float OptimalKnowledgeValueForTribalism = 10;

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;
	public const float PopulationDensityModifier = 10000f;

	public static float HighestAsymptote = 0;

	public SocialOrganizationKnowledge () {

		if (Asymptote > HighestAsymptote) {

			HighestAsymptote = Asymptote;
		}
	}

	public SocialOrganizationKnowledge (CellGroup group, float value = 1f) : base (group, SocialOrganizationKnowledgeId, SocialOrganizationKnowledgeName, value) {

	}

	public SocialOrganizationKnowledge (CellGroup group, SocialOrganizationKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.Value, baseKnowledge.Asymptote) {

	}

	public SocialOrganizationKnowledge (CellGroup group, SocialOrganizationKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();
	}

	protected float CalculatePopulationFactor () {

		float population = Group.Population;

		float popFactor = population / (population + (PopulationDensityModifier * Asymptote));
		popFactor = 0.1f + popFactor * 0.9f;

		return popFactor;
	}

	protected override void UpdateInternal (int timeSpan) {

		float populationFactor = CalculatePopulationFactor ();

		UpdateValue (timeSpan, TimeEffectConstant, populationFactor);

		TryGenerateTribalismDiscoveryEvent ();
	}

	private void TryGenerateTribalismDiscoveryEvent () {

		if (Value < TribalismDiscoveryEvent.MinSocialOrganizationKnowledgeSpawnEventValue)
			return;

		if (Value > TribalismDiscoveryEvent.OptimalSocialOrganizationKnowledgeValue)
			return;

		if (TribalismDiscoveryEvent.CanSpawnIn (Group)) {

			int triggerDate = TribalismDiscoveryEvent.CalculateTriggerDate (Group);

			Group.World.InsertEventToHappen (new TribalismDiscoveryEvent (Group, triggerDate));
		}
	}

	protected override float CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case TribalismDiscovery.TribalismDiscoveryId:
			return 30;
		}

		return 0;
	}

	public override float CalculateExpectedProgressLevel ()
	{
		float populationFactor = CalculatePopulationFactor ();

		if (populationFactor <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / populationFactor, MinValue, 1);
	}

	public override float CalculateTransferFactor ()
	{
		float populationFactor = CalculatePopulationFactor ();

		return (populationFactor * 0.9f) + 0.1f;
	}

	public override bool WillBeLost ()
	{
		return false;
	}

	public override void LossConsequences ()
	{
	}

	protected override float CalculateBaseAsymptote ()
	{
		return 10;
	}
}
