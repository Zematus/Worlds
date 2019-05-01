using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SeafaringSkill : CellCulturalSkill
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    public const string SkillId = "SeafaringSkill";
    public const string SkillName = "Seafaring";
    public const int SkillRngOffset = 0;

    private float _neighborhoodSeaPresence;

    public SeafaringSkill()
    {

    }

    public SeafaringSkill(CellGroup group, float value = 0f) : base(group, SkillId, SkillName, SkillRngOffset, value)
    {
        CalculateNeighborhoodSeaPresence();
    }

    public SeafaringSkill(CellGroup group, SeafaringSkill baseSkill) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value)
    {
        CalculateNeighborhoodSeaPresence();
    }

    public SeafaringSkill(CellGroup group, CulturalSkill baseSkill, float initialValue) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue)
    {
        CalculateNeighborhoodSeaPresence();
    }

    public static bool IsSeafaringSkill(CulturalSkill skill)
    {
        return skill.Id.Contains(SkillId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateNeighborhoodSeaPresence();
    }

    public void CalculateNeighborhoodSeaPresence()
    {
        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = Group.Cell;

        float totalPresence = groupCell.SeaBiomePresence * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.SeaBiomePresence;
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

    public override void PolityCulturalProminence(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(politySkill, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override void PostUpdateInternal()
    {
        RecalculateAdaptation(_neighborhoodSeaPresence);
    }
}
