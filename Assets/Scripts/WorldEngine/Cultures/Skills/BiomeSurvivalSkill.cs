using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BiomeSurvivalSkill : CellCulturalSkill
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 1500;

    public const string SkillIdSuffix = "_survival";
    public const int BiomeSurvivalSkillRngOffsetBase = 1000;

    [XmlIgnore]
    public string BiomeId;

    private float _neighborhoodBiomePresence;

    public static string GenerateId(Biome biome)
    {
        return biome.Id + SkillIdSuffix;
    }

    public static string GenerateName(Biome biome)
    {
        return biome.Name + " survival";
    }

    public static int GenerateRngOffset(Biome biome)
    {
        return BiomeSurvivalSkillRngOffsetBase + biome.IdHash;
    }

    public BiomeSurvivalSkill()
    {

    }

    public BiomeSurvivalSkill(CellGroup group, Biome biome, float value) : base(group, GenerateId(biome), GenerateName(biome), GenerateRngOffset(biome), value)
    {
        BiomeId = biome.Id;

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, BiomeSurvivalSkill baseSkill) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value)
    {
        BiomeId = baseSkill.BiomeId;

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, CulturalSkill baseSkill, float initialValue) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue)
    {
        BiomeId = GetBiomeId(baseSkill.Id);

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, CulturalSkill baseSkill) : this(group, baseSkill, baseSkill.Value)
    {

    }

    public static bool IsBiomeSurvivalSkill(CulturalSkill skill)
    {
        return IsBiomeSurvivalSkill(skill.Id);
    }

    public static bool IsBiomeSurvivalSkill(string skillId)
    {
        return skillId.Contains(SkillIdSuffix);
    }

    public static string GetBiomeId(string skillId)
    {
        return skillId.Substring(0, skillId.Length - SkillIdSuffix.Length);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        BiomeId = GetBiomeId(Id);

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public void CalculateNeighborhoodBiomePresence()
    {
        int groupCellBonus = 2;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = Group.Cell;

        float totalPresence = groupCell.GetBiomeRelPresence(BiomeId) * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.GetBiomeRelPresence(BiomeId);
            cellCount++;
        }

        _neighborhoodBiomePresence = totalPresence / cellCount;

        if ((_neighborhoodBiomePresence < 0) || (_neighborhoodBiomePresence > 1))
        {
            throw new System.Exception("Neighborhood Biome Presence outside range: " + _neighborhoodBiomePresence);
        }
    }

    public override void Update(long timeSpan)
    {
        UpdateInternal(timeSpan, TimeEffectConstant, _neighborhoodBiomePresence);
    }

    public override void AddPolityProminenceEffect(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan)
    {
        AddPolityProminenceEffectInternal(politySkill, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override void PostUpdateInternal()
    {
        RecalculateAdaptation(_neighborhoodBiomePresence);
    }
}

