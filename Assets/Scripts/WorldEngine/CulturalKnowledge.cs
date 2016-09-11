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

public class CulturalKnowledge : CulturalKnowledgeInfo {

	// TODO: knowledge.Value should be an int, not a float.
	[XmlAttribute]
	public float Value;

	public CulturalKnowledge () {
	}

	public CulturalKnowledge (string id, string name, float value) : base (id, name) {

		Value = value;
	}

	public CulturalKnowledge (CulturalKnowledge baseKnowledge) : base (baseKnowledge) {

		Value = baseKnowledge.Value;
	}
}

public abstract class CellCulturalKnowledge : CulturalKnowledge, ISynchronizable {

	public const float MinValue = 0.001f;
	
	[XmlAttribute]
	public float ProgressLevel;
	
	[XmlAttribute]
	public float Asymptote;

	[XmlIgnore]
	public CellGroup Group;
	
	public CellCulturalKnowledge () {

	}

	public CellCulturalKnowledge (CellGroup group, string id, string name, float value) : base (id, name, value) {

		Group = group;
	}

	public CellCulturalKnowledge (CellGroup group, string id, string name, float value, float asymptote) : base (id, name, value) {

		Group = group;
		Asymptote = asymptote;
	}

	public static CellCulturalKnowledge CreateCellInstance (CellGroup group, CulturalKnowledge baseKnowledge, float initialValue) {

		if (ShipbuildingKnowledge.IsShipbuildingKnowledge (baseKnowledge)) {

			return new ShipbuildingKnowledge (group, baseKnowledge, initialValue);
		}

		if (AgricultureKnowledge.IsAgricultureKnowledge (baseKnowledge)) {

			return new AgricultureKnowledge (group, baseKnowledge, initialValue);
		}

		if (SocialOrganizationKnowledge.IsSocialOrganizationKnowledge (baseKnowledge)) {

			return new SocialOrganizationKnowledge (group, baseKnowledge, initialValue);
		}

		throw new System.Exception ("Unexpected CulturalKnowledge type: " + baseKnowledge.Id);
	}
	
	public CellCulturalKnowledge GenerateCopy (CellGroup targetGroup) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = knowledgeType.GetConstructor (new System.Type[] {typeof(CellGroup), knowledgeType});
		
		return cInfo.Invoke (new object[] {targetGroup, this}) as CellCulturalKnowledge;
	}
	
	public CellCulturalKnowledge GenerateCopy (CellGroup targetGroup, float initialValue) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = knowledgeType.GetConstructor (new System.Type[] {typeof(CellGroup), knowledgeType, typeof(float)});
		
		return cInfo.Invoke (new object[] {targetGroup, this, initialValue}) as CellCulturalKnowledge;
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

	public void Merge (CellCulturalKnowledge knowledge, float percentage) {
	
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

	public void CalculateAsymptote (CellCulturalDiscovery discovery) {

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

	protected void UpdateValueInternal (int timeSpan, float timeEffectFactor, float specificModifier) {

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

	public abstract void PolityCulturalInfluence (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan);

	protected void PolityCulturalInfluenceInternal (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan, float timeEffectFactor) {

		float targetValue = polityKnowledge.Value;
		float influenceEffect = polityInfluence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat ();

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		float change = (targetValue - Value) * influenceEffect * timeEffect * randomEffect;

		Value += change;
	}

	public abstract float CalculateExpectedProgressLevel ();
	public abstract float CalculateTransferFactor ();

	public abstract bool WillBeLost ();
	public abstract void LossConsequences ();

	protected abstract void UpdateInternal (int timeSpan);
	protected abstract float CalculateAsymptoteInternal (CulturalDiscovery discovery);
	protected abstract float CalculateBaseAsymptote ();
}

public class ShipbuildingKnowledge : CellCulturalKnowledge {

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

	public ShipbuildingKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

		CalculateNeighborhoodOceanPresence ();
	}

	public static bool IsShipbuildingKnowledge (CulturalKnowledge knowledge) {

		return knowledge.Id.Contains (ShipbuildingKnowledgeId);
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

		UpdateValueInternal (timeSpan, TimeEffectConstant, _neighborhoodOceanPresence * NeighborhoodOceanPresenceModifier);

		TryGenerateSailingDiscoveryEvent ();
	}

	public override void PolityCulturalInfluence (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan) {

		PolityCulturalInfluenceInternal (polityKnowledge, polityInfluence, timeSpan, TimeEffectConstant);

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

public class AgricultureKnowledge : CellCulturalKnowledge {

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

	public AgricultureKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

		CalculateTerrainFactor ();
	}

	public static bool IsAgricultureKnowledge (CulturalKnowledge knowledge) {

		return knowledge.Id.Contains (AgricultureKnowledgeId);
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

		UpdateValueInternal (timeSpan, TimeEffectConstant, _terrainFactor * TerrainFactorModifier);
	}

	public override void PolityCulturalInfluence (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan) {

		PolityCulturalInfluenceInternal (polityKnowledge, polityInfluence, timeSpan, TimeEffectConstant);
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
		Group.Culture.RemoveActivity (CellCulturalActivity.FarmingActivityId);

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

public class SocialOrganizationKnowledge : CellCulturalKnowledge {

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

	public SocialOrganizationKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, float initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, initialValue) {

	}

	public static bool IsSocialOrganizationKnowledge (CulturalKnowledge knowledge) {

		return knowledge.Id.Contains (SocialOrganizationKnowledgeId);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();
	}

	private float CalculatePopulationFactor () {

		float areaFactor = Group.Cell.Area / TerrainCell.MaxArea;

		float population = Group.Population;
		float popFactor = population * areaFactor;

		float densityFactor = PopulationDensityModifier * Asymptote * areaFactor;

		float finalPopFactor = popFactor / (popFactor + densityFactor);
		finalPopFactor = 0.1f + finalPopFactor * 0.9f;

		return finalPopFactor;
	}

	private float CalculatePolityInfluenceFactor () {

		float totalInfluence = Group.TotalPolityInfluenceValue * 0.4f;

		return totalInfluence;
	}

	protected override void UpdateInternal (int timeSpan) {

		float populationFactor = CalculatePopulationFactor ();

		float influenceFactor = CalculatePolityInfluenceFactor ();

		float totalFactor = populationFactor + influenceFactor * (1 - populationFactor);

		UpdateValueInternal (timeSpan, TimeEffectConstant, totalFactor);

		TryGenerateTribalismDiscoveryEvent ();
	}

	public override void PolityCulturalInfluence (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan) {

		PolityCulturalInfluenceInternal (polityKnowledge, polityInfluence, timeSpan, TimeEffectConstant);

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
