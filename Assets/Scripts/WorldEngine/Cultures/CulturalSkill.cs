using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalSkillInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;

	[XmlAttribute("RO")]
	public int RngOffset;
	
	public CulturalSkillInfo () {
	}
	
	public CulturalSkillInfo (string id, string name, int rngOffset) {
		
		Id = id;
		Name = name;
		RngOffset = rngOffset;
	}
	
	public CulturalSkillInfo (CulturalSkillInfo baseInfo) {
		
		Id = baseInfo.Id;
		Name = baseInfo.Name;
		RngOffset = baseInfo.RngOffset;
	}
}

public class CulturalSkill : CulturalSkillInfo {

	[XmlAttribute]
	public float Value;

	public CulturalSkill () {
	}

	public CulturalSkill (string id, string name, int rngOffset, float value) : base (id, name, rngOffset) {

		Value = value;
	}

	public CulturalSkill (CulturalSkill baseSkill) : base (baseSkill) {

		Value = baseSkill.Value;
	}
}

public abstract class CellCulturalSkill : CulturalSkill, ISynchronizable {
	
	[XmlAttribute]
	public float AdaptationLevel;

	[XmlIgnore]
	public CellGroup Group;

	public float _newValue;
	
	public CellCulturalSkill () {
	}

	protected CellCulturalSkill (CellGroup group, string id, string name, int rngOffset, float value = 0) : base (id, name, rngOffset, value) {

		Group = group;

		_newValue = value;
	}

	public static CellCulturalSkill CreateCellInstance (CellGroup group, CulturalSkill baseSkill) {

		return CreateCellInstance (group, baseSkill, baseSkill.Value);
	}

	public static CellCulturalSkill CreateCellInstance (CellGroup group, CulturalSkill baseSkill, float initialValue) {

		if (BiomeSurvivalSkill.IsBiomeSurvivalSkill (baseSkill)) {
		
			return new BiomeSurvivalSkill (group, baseSkill, initialValue);
		}

		if (SeafaringSkill.IsSeafaringSkill (baseSkill)) {

			return new SeafaringSkill (group, baseSkill, initialValue);
		}

		throw new System.Exception ("Unhandled CulturalSkill type: " + baseSkill.Id);
	}

	public void Merge (CulturalSkill skill, float percentage) {

		// _newvalue should have been set correctly either by the constructor or by the Update function
		float value = _newValue * (1f - percentage) + skill.Value * percentage;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Group.Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"Merge - Group:" + groupId,
//					"CurrentDate: " + Group.World.CurrentDate + 
//					", Name: " + Name + 
//					", Value: " + Value + 
//					", source Value: " + skill.Value + 
//					", percentage: " + percentage + 
//					", new value: " + value + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		_newValue = value;
	}

	// This method should be called only once after a Skill is copied from another source group
	public void DecreaseValue (float percentage) {

		float value = _newValue * percentage;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Group.Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"ModifyValue - Group:" + groupId,
//					"CurrentDate: " + Group.World.CurrentDate + 
//					", Name: " + Name + 
//					", Value: " + Value + 
//					", percentage: " + percentage + 
//					", new value: " + value + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif
		
		_newValue = value;
	}

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {

	}

	public abstract void Update (long timeSpan);

	protected void UpdateInternal (long timeSpan, float timeEffectFactor, float specificModifier) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.SKILL_UPDATE + RngOffset);
		randomModifier *= randomModifier;
		float randomFactor = specificModifier - randomModifier;

		float maxTargetValue = 1.0f;
		float minTargetValue = -0.2f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = Value + (maxTargetValue - Value) * randomFactor;
		} else {
			targetValue = Value - (minTargetValue - Value) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		float newValue = (Value * (1 - timeEffect)) + (targetValue * timeEffect);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Group.Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"UpdateInternal - Group:" + groupId,
//					"CurrentDate: " + Group.World.CurrentDate + 
//					", Name: " + Name + 
//					", timeSpan: " + timeSpan + 
//					", timeEffectFactor: " + timeEffectFactor + 
//					", specificModifier: " + specificModifier + 
//					", randomModifier: " + randomModifier + 
//					", targetValue: " + targetValue + 
//					", Value: " + Value + 
//					", newValue: " + newValue + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		_newValue = newValue;
	}

	public abstract void PolityCulturalProminence (CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan);

	protected void PolityCulturalProminenceInternal (CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan, float timeEffectFactor) {

		float targetValue = politySkill.Value;
		float prominenceEffect = polityProminence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat (RngOffsets.SKILL_POLITY_PROMINENCE + RngOffset + (int)polityProminence.PolityId);

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		// _newvalue should have been set correctly either by the constructor or by the Update function
		float change = (targetValue - _newValue) * prominenceEffect * timeEffect * randomEffect;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (Group.Id == Manager.TracingData.GroupId) {
//
//				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"PolityCulturalProminenceInternal - Group:" + groupId,
//					"CurrentDate: " + Group.World.CurrentDate + 
//					", Name: " + Name + 
//					", timeSpan: " + timeSpan + 
//					", timeEffectFactor: " + timeEffectFactor + 
//					", randomEffect: " + randomEffect + 
//					", polity Id: " + polityProminence.PolityId + 
//					", polityProminence.Value: " + prominenceEffect + 
//					", politySkill.Value: " + targetValue + 
//					", Value: " + Value + 
//					", change: " + change + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		_newValue = _newValue + change;
	}

	protected void RecalculateAdaptation (float targetValue)
	{
		AdaptationLevel = MathUtility.RoundToSixDecimals (1 - Mathf.Abs (Value - targetValue));
	}

	public void PostUpdate () {

		Value = MathUtility.RoundToSixDecimals (Mathf.Clamp01 (_newValue));

		PostUpdateInternal ();
	}

	protected abstract void PostUpdateInternal ();
}

public class BiomeSurvivalSkill : CellCulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 1500;

	public const string BiomeSurvivalSkillIdPrefix = "BiomeSurvivalSkill_";
	public const int BiomeSurvivalSkillRngOffsetBase = 1000;
	
	[XmlAttribute]
	public string BiomeName;
	
	private float _neighborhoodBiomePresence;

	public static string GenerateId (Biome biome) {
	
		return BiomeSurvivalSkillIdPrefix + biome.Id;
	}
	
	public static string GenerateName (Biome biome) {
		
		return biome.Name + " Survival";
	}

	public static int GenerateRngOffset (Biome biome) {

		return BiomeSurvivalSkillRngOffsetBase + (biome.ColorId * 100);
	}
	
	public BiomeSurvivalSkill () {

	}

	public BiomeSurvivalSkill (CellGroup group, Biome biome, float value) : base (group, GenerateId (biome), GenerateName (biome), GenerateRngOffset (biome), value) {
	
		BiomeName = biome.Name;

		Group.AddBiomeSurvivalSkill (this);
		
		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, BiomeSurvivalSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value) {

		BiomeName = baseSkill.BiomeName;

		Group.AddBiomeSurvivalSkill (this);
		
		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, CulturalSkill baseSkill, float initialValue) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue) {

		int suffixIndex = baseSkill.Name.IndexOf (" Survival");

		BiomeName = baseSkill.Name.Substring (0, suffixIndex);

		Group.AddBiomeSurvivalSkill (this);

		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, CulturalSkill baseSkill) : this (group, baseSkill, baseSkill.Value) {

	}

	public static bool IsBiomeSurvivalSkill (CulturalSkill skill) {

		return skill.Id.Contains (BiomeSurvivalSkillIdPrefix);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Group.AddBiomeSurvivalSkill (this);

		CalculateNeighborhoodBiomePresence ();
	}
	
	public void CalculateNeighborhoodBiomePresence () {

		int groupCellBonus = 2;
		int cellCount = groupCellBonus;
		
		TerrainCell groupCell = Group.Cell;
		
		float totalPresence = groupCell.GetBiomePresence (BiomeName) * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {
			
			totalPresence += c.GetBiomePresence (BiomeName);
			cellCount++;
		}
		
		_neighborhoodBiomePresence = totalPresence / cellCount;

		if ((_neighborhoodBiomePresence < 0) || (_neighborhoodBiomePresence > 1)) {

			throw new System.Exception ("Neighborhood Biome Presence outside range: " + _neighborhoodBiomePresence);
		}

//		RecalculateAdaptation (_neighborhoodBiomePresence);
	}

	public override void Update (long timeSpan) {

		UpdateInternal (timeSpan, TimeEffectConstant, _neighborhoodBiomePresence);

//		RecalculateAdaptation (_neighborhoodBiomePresence);
	}

	public override void PolityCulturalProminence (CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan) {

		PolityCulturalProminenceInternal (politySkill, polityProminence, timeSpan, TimeEffectConstant);

//		RecalculateAdaptation (_neighborhoodBiomePresence);
	}

	protected override void PostUpdateInternal () {

		RecalculateAdaptation (_neighborhoodBiomePresence);
	}
}

public class SeafaringSkill : CellCulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

	public const string SeafaringSkillId = "SeafaringSkill";
	public const string SeafaringSkillName = "Seafaring";
	public const int SeafaringSkillRngOffset = 0;

	private float _neighborhoodOceanPresence;

	public SeafaringSkill () {

	}

	public SeafaringSkill (CellGroup group, float value = 0f) : base (group, SeafaringSkillId, SeafaringSkillName, SeafaringSkillRngOffset, value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public SeafaringSkill (CellGroup group, SeafaringSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public SeafaringSkill (CellGroup group, CulturalSkill baseSkill, float initialValue) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue) {

		CalculateNeighborhoodOceanPresence ();
	}

	public static bool IsSeafaringSkill (CulturalSkill skill) {

		return skill.Id.Contains (SeafaringSkillId);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodOceanPresence ();
	}

	public void CalculateNeighborhoodOceanPresence () {

		int groupCellBonus = 1;
		int cellCount = groupCellBonus;

		TerrainCell groupCell = Group.Cell;

		float totalPresence = groupCell.GetBiomePresence (Biome.Ocean.Name) * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {

			totalPresence += c.GetBiomePresence (Biome.Ocean.Name);
			cellCount++;
		}

		_neighborhoodOceanPresence = totalPresence / cellCount;

		if ((_neighborhoodOceanPresence < 0) || (_neighborhoodOceanPresence > 1)) {

			throw new System.Exception ("Neighborhood Ocean Presence outside range: " + _neighborhoodOceanPresence);
		}

//		RecalculateAdaptation (_neighborhoodOceanPresence);
	}

	public override void Update (long timeSpan) {

		UpdateInternal (timeSpan, TimeEffectConstant, _neighborhoodOceanPresence);

//		RecalculateAdaptation (_neighborhoodOceanPresence);
	}

	public override void PolityCulturalProminence (CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan) {

		PolityCulturalProminenceInternal (politySkill, polityProminence, timeSpan, TimeEffectConstant);

//		RecalculateAdaptation (_neighborhoodOceanPresence);
	}

	protected override void PostUpdateInternal () {

		RecalculateAdaptation (_neighborhoodOceanPresence);
	}
}
