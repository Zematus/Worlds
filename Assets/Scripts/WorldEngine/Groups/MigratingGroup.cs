using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

//NOTE: This class is not serialized
public class MigratingGroup : HumanGroup
{
    public float PercentPopulation;

    public int TargetCellLongitude;
    public int TargetCellLatitude;

    public int Population = 0;

    public int MigrationDirectionInt;

    public Identifier SourceGroupId;

    public BufferCulture Culture;

    public List<Faction> FactionCoresToMigrate = new List<Faction>();

    public TerrainCell TargetCell;

    public CellGroup SourceGroup;

    public float TotalProminenceValue = 0;

    public Dictionary<Polity, float> PolityProminences = new Dictionary<Polity, float>();

    public Direction MigrationDirection;

    public MigratingGroup()
    {
        throw new System.InvalidOperationException("This class doesn't support serialization");
    }

    public MigratingGroup(
        World world,
        float percentPopulation,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection) :
        base(world)
    {
        Set(percentPopulation, sourceGroup, targetCell, migrationDirection);
    }

    public void Set(
        float percentPopulation,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection)
    {
        Init(sourceGroup);

        MigrationDirection = migrationDirection;

        PercentPopulation = percentPopulation;

        if (float.IsNaN(percentPopulation))
        {
            throw new System.Exception("float.IsNaN(percentPopulation)");
        }

        TargetCell = targetCell;
        SourceGroup = sourceGroup;

        SourceGroupId = SourceGroup.Id;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;
    }

    public bool SplitFromSourceGroup()
    {
        if (SourceGroup == null)
            return false;

        if (!SourceGroup.StillPresent)
            return false;

        Population = SourceGroup.SplitGroup(this);

        if (Population <= 0)
            return false;

        Culture = new BufferCulture(SourceGroup.Culture);

        TotalProminenceValue = SourceGroup.TotalPolityProminenceValue;
        PolityProminences.Clear();

        foreach (PolityProminence pp in SourceGroup.GetPolityProminences())
        {
            PolityProminences.Add(pp.Polity, pp.Value);
        }

        TryMigrateFactionCores();

        return true;
    }

    private void TryMigrateFactionCores()
    {
        int targetPopulation = 0;
        int targetNewPopulation = Population;

        CellGroup targetGroup = TargetCell.Group;
        if (targetGroup != null)
        {
            targetPopulation = targetGroup.Population;
            targetNewPopulation += targetPopulation;
        }

        FactionCoresToMigrate.Clear();

        foreach (Faction faction in SourceGroup.GetFactionCores())
        {
            PolityProminence pi = SourceGroup.GetPolityProminence(faction.Polity);

            if (pi == null)
            {
                Debug.LogError("Unable to find Polity with Id: " + faction.Polity.Id);
            }

            float sourceGroupProminence = pi.Value;
            float targetGroupProminence = sourceGroupProminence;

            if (targetGroup != null)
            {
                PolityProminence piTarget = targetGroup.GetPolityProminence(faction.Polity);

                if (piTarget != null)
                    targetGroupProminence = piTarget.Value;
                else
                    targetGroupProminence = 0f;
            }

            float targetNewGroupProminence = ((sourceGroupProminence * Population) + (targetGroupProminence * targetPopulation)) / targetNewPopulation;

            if (faction.ShouldMigrateFactionCore(SourceGroup, TargetCell, targetNewGroupProminence, targetNewPopulation))
                FactionCoresToMigrate.Add(faction);
        }
    }

    public void MoveToCell()
    {
        if (Population <= 0)
            return;

        CellGroup targetGroup = TargetCell.Group;

        if (targetGroup != null)
        {
            if (targetGroup.StillPresent)
            {
                Profiler.BeginSample("targetGroup.MergeGroup");

                targetGroup.MergeGroup(this);

                Profiler.EndSample();

                if (SourceGroup.MigrationTagged)
                {
                    World.MigrationTagGroup(TargetCell.Group);
                }
            }
        }
        else
        {
            Profiler.BeginSample("targetGroup = new CellGroup");

            targetGroup = new CellGroup(this, Population);

            Profiler.EndSample();

            Profiler.BeginSample("World.AddGroup");

            World.AddGroup(targetGroup);

            Profiler.EndSample();

            if (SourceGroup.MigrationTagged)
            {
                World.MigrationTagGroup(targetGroup);
            }
        }

        foreach (Faction faction in FactionCoresToMigrate)
        {
            World.AddFactionToUpdate(faction);
            World.AddPolityToUpdate(faction.Polity);

            faction.PrepareNewCoreGroup(targetGroup);
        }
    }

    public override void Synchronize()
    {
        MigrationDirectionInt = (int)MigrationDirection;
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        MigrationDirection = (Direction)MigrationDirectionInt;

        TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

        SourceGroup = World.GetGroup(SourceGroupId);
    }
}
