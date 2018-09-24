using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BiomeSurvivalSkill : CellCulturalSkill
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 1500;

    public const string BiomeSurvivalSkillIdPrefix = "BiomeSurvivalSkill_";
    public const int BiomeSurvivalSkillRngOffsetBase = 1000;

    [XmlAttribute]
    public string BiomeName;

    private float _neighborhoodBiomePresence;

    public static string GenerateId(Biome biome)
    {
        return BiomeSurvivalSkillIdPrefix + biome.Id;
    }

    public static string GenerateName(Biome biome)
    {
        return biome.Name + " Survival";
    }

    public static int GenerateRngOffset(Biome biome)
    {
        return BiomeSurvivalSkillRngOffsetBase + (biome.ColorId * 100);
    }

    public BiomeSurvivalSkill()
    {

    }

    public BiomeSurvivalSkill(CellGroup group, Biome biome, float value) : base(group, GenerateId(biome), GenerateName(biome), GenerateRngOffset(biome), value)
    {
        BiomeName = biome.Name;

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, BiomeSurvivalSkill baseSkill) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value)
    {
        BiomeName = baseSkill.BiomeName;

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, CulturalSkill baseSkill, float initialValue) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue)
    {
        int suffixIndex = baseSkill.Name.IndexOf(" Survival");

        BiomeName = baseSkill.Name.Substring(0, suffixIndex);

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public BiomeSurvivalSkill(CellGroup group, CulturalSkill baseSkill) : this(group, baseSkill, baseSkill.Value)
    {

    }

    public static bool IsBiomeSurvivalSkill(CulturalSkill skill)
    {
        return skill.Id.Contains(BiomeSurvivalSkillIdPrefix);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Group.AddBiomeSurvivalSkill(this);

        CalculateNeighborhoodBiomePresence();
    }

    public void CalculateNeighborhoodBiomePresence()
    {
        int groupCellBonus = 2;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = Group.Cell;

        float totalPresence = groupCell.GetBiomePresence(BiomeName) * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.GetBiomePresence(BiomeName);
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

    public override void PolityCulturalProminence(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(politySkill, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override void PostUpdateInternal()
    {
        RecalculateAdaptation(_neighborhoodBiomePresence);
    }
}

