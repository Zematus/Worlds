using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using ProtoBuf;

[ProtoContract]
public class MigratingGroup : HumanGroup
{
    [ProtoMember(1)]
    public float PercentPopulation;

    [ProtoMember(2)]
    public int TargetCellLongitude;
    [ProtoMember(3)]
    public int TargetCellLatitude;

    [ProtoMember(4)]
    public int Population = 0;

    [ProtoMember(5)]
    public long SourceGroupId;

    [ProtoMember(6)]
    public int MigrationDirectionInt;

    [ProtoMember(7)]
    public BufferCulture Culture;

    public List<Faction> FactionCoresToMigrate = new List<Faction>();

    public TerrainCell TargetCell;

    public CellGroup SourceGroup;

    public List<PolityProminence> PolityProminences = new List<PolityProminence>();

    public int PolityProminencesCount = 0;

    public Direction MigrationDirection;

    public MigratingGroup()
    {
    }

    public MigratingGroup(World world, float percentPopulation, CellGroup sourceGroup, TerrainCell targetCell, Direction migrationDirection) : base(world)
    {
        Set(percentPopulation, sourceGroup, targetCell, migrationDirection);
    }

    public void Set(float percentPopulation, CellGroup sourceGroup, TerrainCell targetCell, Direction migrationDirection)
    {
        MigrationDirection = migrationDirection;

        PercentPopulation = percentPopulation;

#if DEBUG
        if (float.IsNaN(percentPopulation))
        {
            throw new System.Exception("float.IsNaN (percentPopulation)");
        }
#endif

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (sourceGroup.Id == Manager.TracingData.GroupId) {
        //				string groupId = "Id:" + sourceGroup.Id + "|Long:" + sourceGroup.Longitude + "|Lat:" + sourceGroup.Latitude;
        //				string targetInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"MigratingGroup:constructor - sourceGroup:" + groupId,
        //					"CurrentDate: " + World.CurrentDate + 
        //					", targetInfo: " + targetInfo + 
        //					", percentPopulation: " + percentPopulation + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

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

        //Profiler.BeginSample("SourceGroup.SplitGroup");

        Population = SourceGroup.SplitGroup(this);

        //Profiler.EndSample();

        if (Population <= 0)
            return false;

        //Profiler.BeginSample("Culture = new BufferCulture");

        Culture = new BufferCulture(SourceGroup.Culture);

        //Profiler.EndSample();

        PolityProminencesCount = SourceGroup.GetPolityProminences().Count;
        int currentPPCount = PolityProminences.Count;

        int i = 0;
        foreach (PolityProminence pp in SourceGroup.GetPolityProminences())
        {
            if (i < currentPPCount)
            {
                PolityProminences[i].Set(pp);
            }
            else
            {
                PolityProminences.Add(new PolityProminence(pp));
            }

            i++;
        }

        //Profiler.BeginSample("TryMigrateFactionCores");

        TryMigrateFactionCores();

        //Profiler.EndSample();

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

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (SourceGroupId == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "TryMigrateFactionCores - SourceGroup:" + SourceGroupId,
//                    "CurrentDate: " + World.CurrentDate +
//                    "SourceGroup.GetFactionCores().Count: " + SourceGroup.GetFactionCores().Count +
//                    ", FactionCoresToMigrate.Count: " + FactionCoresToMigrate.Count +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif
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

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (SourceGroupId == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "MoveToCell - SourceGroup:" + SourceGroupId,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", Population: " + Population +
//                    ", FactionCoresToMigrate.Count: " + FactionCoresToMigrate.Count +
//                    ", TargetCell.Position: " + TargetCell.Position +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif
    }

    public override void Synchronize()
    {
        MigrationDirectionInt = (int)MigrationDirection;
    }

    public override void FinalizeLoad()
    {
        MigrationDirection = (Direction)MigrationDirectionInt;

        base.FinalizeLoad();

        TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

        SourceGroup = World.GetGroup(SourceGroupId);
    }
}
