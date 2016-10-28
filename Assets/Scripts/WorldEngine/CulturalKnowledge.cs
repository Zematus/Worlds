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

	[XmlAttribute("RO")]
	public int RngOffset;
	
	public CulturalKnowledgeInfo () {
	}
	
	public CulturalKnowledgeInfo (string id, string name, int rngOffset) {
		
		Id = id;
		Name = name;
		RngOffset = rngOffset;
	}
	
	public CulturalKnowledgeInfo (CulturalKnowledgeInfo baseInfo) {
		
		Id = baseInfo.Id;
		Name = baseInfo.Name;
		RngOffset = baseInfo.RngOffset;
	}
}

public class CulturalKnowledge : CulturalKnowledgeInfo {

	public const float ValueScaleFactor = 0.01f;

	[XmlAttribute]
	public int Value;

	public CulturalKnowledge () {
	}

	public CulturalKnowledge (string id, string name, int rngOffset, int value) : base (id, name, rngOffset) {

		Value = value;
	}

	public CulturalKnowledge (CulturalKnowledge baseKnowledge) : base (baseKnowledge) {

		Value = baseKnowledge.Value;
	}

	public float ScaledValue {
		get { return Value * ValueScaleFactor; }
	}
}

public class PolityCulturalKnowledge : CulturalKnowledge {

	[XmlIgnore]
	public float AggregateValue;

	public PolityCulturalKnowledge () {
	}

	public PolityCulturalKnowledge (string id, string name, int rngOffset, int value) : base (id, name, rngOffset, value) {
	}

	public PolityCulturalKnowledge (CulturalKnowledge baseKnowledge) : base (baseKnowledge) {
	}
}

public abstract class CellCulturalKnowledge : CulturalKnowledge, ISynchronizable {

	public const float MinProgressLevel = 0.001f;
	
	[XmlAttribute]
	public float ProgressLevel;
	
	[XmlAttribute]
	public int Asymptote;

	[XmlIgnore]
	public CellGroup Group;

	public float ScaledAsymptote {
		get { return Asymptote * ValueScaleFactor; }
	}
	
	public CellCulturalKnowledge () {

	}

	public CellCulturalKnowledge (CellGroup group, string id, string name, int rngOffset, int value) : base (id, name, rngOffset, value) {

		Group = group;
	}

	public CellCulturalKnowledge (CellGroup group, string id, string name, int rngOffset, int value, int asymptote) : base (id, name, rngOffset, value) {

		Group = group;
		Asymptote = asymptote;
	}

	public static CellCulturalKnowledge CreateCellInstance (CellGroup group, CulturalKnowledge baseKnowledge, int initialValue) {

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
	
	public CellCulturalKnowledge GenerateCopy (CellGroup targetGroup, int initialValue) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = knowledgeType.GetConstructor (new System.Type[] {typeof(CellGroup), knowledgeType, typeof(float)});
		
		return cInfo.Invoke (new object[] {targetGroup, this, initialValue}) as CellCulturalKnowledge;
	}
	
	public int GetHighestAsymptote () {
		
		System.Type knowledgeType = this.GetType ();

		System.Reflection.FieldInfo fInfo = knowledgeType.GetField ("HighestAsymptote");
		
		return (int)fInfo.GetValue (this);
	}
	
	public void SetHighestAsymptote (int value) {
		
		System.Type knowledgeType = this.GetType ();
		
		System.Reflection.FieldInfo fInfo = knowledgeType.GetField ("HighestAsymptote");

		int currentValue = (int)fInfo.GetValue (this);
		fInfo.SetValue (this, Mathf.Max (value, currentValue));
	}

	public void Merge (CellCulturalKnowledge knowledge, float percentage) {

		float d;
		int mergedValue = (int)MathUtility.MergeAndGetDecimals (Value, knowledge.Value, percentage, out d);

		if (d > Group.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_MERGE + RngOffset + (int)knowledge.Group.Id))
			mergedValue++;
	
		Value = mergedValue;
	}
	
//	public void IncreaseValue (int targetValue, float percentage) {
//
//		if (targetValue > Value) {
//
//			float d;
//			int valueIncrease = (int)MathUtility.MultiplyAndGetDecimals (targetValue - Value, percentage, out d);
//
//			if (d > Group.GetNextLocalRandomFloat ())
//				valueIncrease++;
//
//			Value += valueIncrease;
//
//			#if DEBUG
//			if ((Asymptote > 1) && (Value > Asymptote)) {
//				Debug.LogError ("IncreaseValue: new value " + Value + " above Asymptote " + Asymptote);
//			}
//			#endif
//		}
//	}
	
	public void ModifyValue (float percentage) {

		float d;
		int modifiedValue = (int)MathUtility.MultiplyAndGetDecimals (Value, percentage, out d);

		if (d > Group.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_MODIFY_VALUE + RngOffset))
			modifiedValue++;
		
		Value = modifiedValue;
	}

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {

	}
	
	public void UpdateProgressLevel () {

		ProgressLevel = 0;

		if (Asymptote > 0)
			ProgressLevel = MathUtility.RoundToSixDecimals (Mathf.Clamp01 (Value / (float)Asymptote));
	}
	
	public void RecalculateAsymptote () {

		Asymptote = CalculateBaseAsymptote ();

		Group.Culture.Discoveries.ForEach (d => Asymptote = Mathf.Max (CalculateAsymptoteInternal (d), Asymptote));

		UpdateProgressLevel ();

		SetHighestAsymptote (Asymptote);
	}

	public void CalculateAsymptote (CellCulturalDiscovery discovery) {

		int newAsymptote = CalculateAsymptoteInternal (discovery);

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

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_UPDATE_VALUE_INTERNAL + RngOffset);
		randomModifier *= randomModifier;
		float randomFactor = specificModifier - randomModifier;
		randomFactor = Mathf.Clamp (randomFactor, -1, 1);

		float maxTargetValue = Asymptote;
		float minTargetValue = 0;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = Value + (maxTargetValue - Value) * randomFactor;
		} else {
			targetValue = Value - (minTargetValue - Value) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		float d;
		int newValue = (int)MathUtility.MergeAndGetDecimals (Value, targetValue, timeEffect, out d);

		if (d > Group.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_UPDATE_VALUE_INTERNAL_2 + RngOffset))
			newValue++;

		#if DEBUG
		if ((Asymptote > 1) && (newValue > Asymptote) && (newValue > Value)) {
			Debug.LogError ("UpdateValueInternal: new value " + newValue + " above Asymptote " + Asymptote);
		}
		#endif

		#if DEBUG
		if (newValue > 1000000) {
			Debug.LogError ("UpdateValueInternal: new value " + newValue + " above 1000000000");
		}
		#endif

		Value = newValue;
	}

	public abstract void PolityCulturalInfluence (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan);

	protected void PolityCulturalInfluenceInternal (CulturalKnowledge polityKnowledge, PolityInfluence polityInfluence, int timeSpan, float timeEffectFactor) {

		float targetValue = polityKnowledge.Value;
		float influenceEffect = polityInfluence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_POLITY_INFLUENCE + RngOffset + (int)polityInfluence.PolityId);

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		float d;
		int valueIncrease = (int)MathUtility.MultiplyAndGetDecimals (targetValue - Value, influenceEffect * timeEffect * randomEffect, out d);

		if (d > Group.GetNextLocalRandomFloat (RngOffsets.KNOWLEDGE_POLITY_INFLUENCE_2 + RngOffset + (int)polityInfluence.PolityId))
			valueIncrease++;

		Value += valueIncrease;
	}

	public abstract float CalculateExpectedProgressLevel ();
	public abstract float CalculateTransferFactor ();

	public abstract bool WillBeLost ();
	public abstract void LossConsequences ();

	protected abstract void UpdateInternal (int timeSpan);
	protected abstract int CalculateAsymptoteInternal (CulturalDiscovery discovery);
	protected abstract int CalculateBaseAsymptote ();
}

public class ShipbuildingKnowledge : CellCulturalKnowledge {

	public const string ShipbuildingKnowledgeId = "ShipbuildingKnowledge";
	public const string ShipbuildingKnowledgeName = "Shipbuilding";

	public const int ShipbuildingKnowledgeRandomOffset = 0;

	public const int MinKnowledgeValueForSailingSpawnEvent = 500;
	public const int MinKnowledgeValueForSailing = 300;
	public const int OptimalKnowledgeValueForSailing = 1000;

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;
	public const float NeighborhoodOceanPresenceModifier = 1.5f;

	public static int HighestAsymptote = 0;

	private float _neighborhoodOceanPresence;
	
	public ShipbuildingKnowledge () {

		if (Asymptote > HighestAsymptote) {
			
			HighestAsymptote = Asymptote;
		}
	}

	public ShipbuildingKnowledge (CellGroup group, int value = 100) : base (group, ShipbuildingKnowledgeId, ShipbuildingKnowledgeName, ShipbuildingKnowledgeRandomOffset, value) {
		
		CalculateNeighborhoodOceanPresence ();
	}

	public ShipbuildingKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, baseKnowledge.Value, baseKnowledge.Asymptote) {
		
		CalculateNeighborhoodOceanPresence ();
	}
	
	public ShipbuildingKnowledge (CellGroup group, ShipbuildingKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {
		
		CalculateNeighborhoodOceanPresence ();
	}

	public ShipbuildingKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {

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

	protected override int CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case BoatMakingDiscovery.BoatMakingDiscoveryId:
			return 1000;
		case SailingDiscovery.SailingDiscoveryId:
			return 3000;
		}

		return 0;
	}

	public override float CalculateExpectedProgressLevel ()
	{
		if (_neighborhoodOceanPresence <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / _neighborhoodOceanPresence, MinProgressLevel, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_neighborhoodOceanPresence * 0.9f) + 0.1f;
	}

	public override bool WillBeLost ()
	{
		if (Value < 100) {

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

	protected override int CalculateBaseAsymptote ()
	{
		return 0;
	}
}

public class AgricultureKnowledge : CellCulturalKnowledge {

	public const string AgricultureKnowledgeId = "AgricultureKnowledge";
	public const string AgricultureKnowledgeName = "Agriculture";

	public const int AgricultureKnowledgeRandomOffset = 100;

	public const float TimeEffectConstant = CellGroup.GenerationTime * 2000;
	public const float TerrainFactorModifier = 1.5f;
	public const float MinAccesibility = 0.2f;

	public static int HighestAsymptote = 0;

	private float _terrainFactor;

	public AgricultureKnowledge () {

		if (Asymptote > HighestAsymptote) {

			HighestAsymptote = Asymptote;
		}
	}

	public AgricultureKnowledge (CellGroup group, int value = 100) : base (group, AgricultureKnowledgeId, AgricultureKnowledgeName, AgricultureKnowledgeRandomOffset, value) {

		CalculateTerrainFactor ();
	}

	public AgricultureKnowledge (CellGroup group, AgricultureKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, baseKnowledge.Value, baseKnowledge.Asymptote) {

		CalculateTerrainFactor ();
	}

	public AgricultureKnowledge (CellGroup group, AgricultureKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {

		CalculateTerrainFactor ();
	}

	public AgricultureKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {

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

	protected override int CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case PlantCultivationDiscovery.PlantCultivationDiscoveryId:
			return 1000;
		}

		return 0;
	}

	public override float CalculateExpectedProgressLevel ()
	{
		if (_terrainFactor <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / _terrainFactor, MinProgressLevel, 1);
	}

	public override float CalculateTransferFactor ()
	{
		return (_terrainFactor * 0.9f) + 0.1f;
	}

	public override bool WillBeLost ()
	{
		if (Value < 100) {
		
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

	protected override int CalculateBaseAsymptote ()
	{
		return 0;
	}
}

public class SocialOrganizationKnowledge : CellCulturalKnowledge {

	public const string SocialOrganizationKnowledgeId = "SocialOrganizationKnowledge";
	public const string SocialOrganizationKnowledgeName = "Social Organization";

	public const int SocialOrganizationKnowledgeRandomOffset = 200;

	public const int MinKnowledgeValueForTribalismSpawnEvent = 500;
	public const int MinKnowledgeValueForTribalism = 400;
	public const int OptimalKnowledgeValueForTribalism = 1000;

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;
	public const float PopulationDensityModifier = 10000f;

	public static int HighestAsymptote = 0;

	public SocialOrganizationKnowledge () {

		if (Asymptote > HighestAsymptote) {

			HighestAsymptote = Asymptote;
		}
	}

	public SocialOrganizationKnowledge (CellGroup group, int value = 100) : base (group, SocialOrganizationKnowledgeId, SocialOrganizationKnowledgeName, SocialOrganizationKnowledgeRandomOffset, value) {

	}

	public SocialOrganizationKnowledge (CellGroup group, SocialOrganizationKnowledge baseKnowledge) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, baseKnowledge.Value, baseKnowledge.Asymptote) {

	}

	public SocialOrganizationKnowledge (CellGroup group, SocialOrganizationKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {

	}

	public SocialOrganizationKnowledge (CellGroup group, CulturalKnowledge baseKnowledge, int initialValue) : base (group, baseKnowledge.Id, baseKnowledge.Name, baseKnowledge.RngOffset, initialValue) {

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

		float densityFactor = PopulationDensityModifier * Asymptote * ValueScaleFactor * areaFactor;

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

		float totalFactor = populationFactor + (influenceFactor * (1 - populationFactor));

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

	protected override int CalculateAsymptoteInternal (CulturalDiscovery discovery)
	{
		switch (discovery.Id) {

		case TribalismDiscovery.TribalismDiscoveryId:
			return 3000;
		}

		return 0;
	}

	public override float CalculateExpectedProgressLevel ()
	{
		float populationFactor = CalculatePopulationFactor ();

		if (populationFactor <= 0)
			return 1;

		return Mathf.Clamp (ProgressLevel / populationFactor, MinProgressLevel, 1);
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

	protected override int CalculateBaseAsymptote ()
	{
		return 1000;
	}
}
