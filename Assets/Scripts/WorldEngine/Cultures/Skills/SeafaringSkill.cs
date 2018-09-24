using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SeafaringSkill : CellCulturalSkill
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    public const string SeafaringSkillId = "SeafaringSkill";
    public const string SeafaringSkillName = "Seafaring";
    public const int SeafaringSkillRngOffset = 0;

    private float _neighborhoodOceanPresence;

    public SeafaringSkill()
    {

    }

    public SeafaringSkill(CellGroup group, float value = 0f) : base(group, SeafaringSkillId, SeafaringSkillName, SeafaringSkillRngOffset, value)
    {
        CalculateNeighborhoodOceanPresence();
    }

    public SeafaringSkill(CellGroup group, SeafaringSkill baseSkill) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, baseSkill.Value)
    {
        CalculateNeighborhoodOceanPresence();
    }

    public SeafaringSkill(CellGroup group, CulturalSkill baseSkill, float initialValue) : base(group, baseSkill.Id, baseSkill.Name, baseSkill.RngOffset, initialValue)
    {
        CalculateNeighborhoodOceanPresence();
    }

    public static bool IsSeafaringSkill(CulturalSkill skill)
    {
        return skill.Id.Contains(SeafaringSkillId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateNeighborhoodOceanPresence();
    }

    public void CalculateNeighborhoodOceanPresence()
    {
        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = Group.Cell;

        float totalPresence = groupCell.GetBiomePresence(Biome.Ocean.Name) * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.GetBiomePresence(Biome.Ocean.Name);
            cellCount++;
        }

        _neighborhoodOceanPresence = totalPresence / cellCount;

        if ((_neighborhoodOceanPresence < 0) || (_neighborhoodOceanPresence > 1))
        {

            throw new System.Exception("Neighborhood Ocean Presence outside range: " + _neighborhoodOceanPresence);
        }
    }

    public override void Update(long timeSpan)
    {
        UpdateInternal(timeSpan, TimeEffectConstant, _neighborhoodOceanPresence);
    }

    public override void PolityCulturalProminence(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(politySkill, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override void PostUpdateInternal()
    {
        RecalculateAdaptation(_neighborhoodOceanPresence);
    }
}
