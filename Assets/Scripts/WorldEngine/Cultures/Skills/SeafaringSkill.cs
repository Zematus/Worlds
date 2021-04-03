using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SeafaringSkill : CellCulturalSkill
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    public const string SkillId = "seafaring";
    public const string SkillName = "seafaring";
    public const int SkillRngOffset = 0;

    private float _neighborhoodSeaPresence;

    public SeafaringSkill()
    {

    }

    public SeafaringSkill(CellGroup group, float value = 0f) : base(group, SkillId, SkillName, SkillRngOffset, value)
    {
        CalculateNeighborhoodWaterPresence();
    }

    public SeafaringSkill(CellGroup group, SeafaringSkill baseSkill) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value)
    {
        CalculateNeighborhoodWaterPresence();
    }

    public SeafaringSkill(CellGroup group, CulturalSkill baseSkill, float initialValue) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue)
    {
        CalculateNeighborhoodWaterPresence();
    }

    public static bool IsSeafaringSkill(CulturalSkill skill)
    {
        return IsSeafaringSkill(skill.Id);
    }

    public static bool IsSeafaringSkill(string skillId)
    {
        return skillId == SkillId;
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateNeighborhoodWaterPresence();
    }

    public void CalculateNeighborhoodWaterPresence()
    {
        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = Group.Cell;

        float totalPresence = groupCell.WaterBiomePresence * groupCellBonus;

        foreach (TerrainCell c in groupCell.NeighborList)
        {
            totalPresence += c.WaterBiomePresence;
            cellCount++;
        }

        _neighborhoodSeaPresence = totalPresence / cellCount;

        if ((_neighborhoodSeaPresence < 0) || (_neighborhoodSeaPresence > 1))
        {
            throw new System.Exception("Neighborhood sea presence outside range: " + _neighborhoodSeaPresence);
        }
    }

    public override void Update(long timeSpan)
    {
        UpdateInternal(timeSpan, TimeEffectConstant, _neighborhoodSeaPresence);
    }

    public override void AddPolityProminenceEffect(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan)
    {
        AddPolityProminenceEffectInternal(politySkill, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override void PostUpdateInternal()
    {
        RecalculateAdaptation(_neighborhoodSeaPresence);
    }
}
