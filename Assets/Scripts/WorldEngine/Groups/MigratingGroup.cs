using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class MigratingGroup : HumanGroup
{
    [XmlAttribute]
    public float PercentPopulation;

    [XmlAttribute]
    public int TargetCellLongitude;
    [XmlAttribute]
    public int TargetCellLatitude;

    [XmlAttribute]
    public int Population = 0;

    [XmlAttribute]
    public long SourceGroupId;

    [XmlAttribute("MigDir")]
    public int MigrationDirectionInt;

    public BufferCulture Culture;

    [XmlIgnore]
    public List<Faction> FactionCoresToMigrate = new List<Faction>();

    [XmlIgnore]
    public TerrainCell TargetCell;

    [XmlIgnore]
    public CellGroup SourceGroup;

    [XmlIgnore]
    public List<PolityProminence> PolityProminences = new List<PolityProminence>();

    [XmlIgnore]
    public int PolityProminencesCount = 0;

    [XmlIgnore]
    public Direction MigrationDirection;

    public List<string> Attributes;

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

        Profiler.BeginSample("SourceGroup.SplitGroup");

        Population = SourceGroup.SplitGroup(this);

        Profiler.EndSample();

        if (Population <= 0)
            return false;

        Profiler.BeginSample("Culture = new BufferCulture");

        Culture = new BufferCulture(SourceGroup.Culture);

        Attributes = new List<string>(SourceGroup.GetProperties());

        Profiler.EndSample();

        PolityProminencesCount = SourceGroup.PolityProminences.Count;

        int minCopyCount = Mathf.Min(PolityProminencesCount, PolityProminences.Count);

        IEnumerator<PolityProminence> ppEnumerator = SourceGroup.PolityProminences.Values.GetEnumerator();

        for (int i = 0; i < minCopyCount; i++)
        {
            Profiler.BeginSample("PolityProminences[i].Set");

            ppEnumerator.MoveNext();
            PolityProminences[i].Set(ppEnumerator.Current);

            Profiler.EndSample();
        }

        for (int i = minCopyCount; i < PolityProminencesCount; i++)
        {
            Profiler.BeginSample("PolityProminences.Add");

            ppEnumerator.MoveNext();
            PolityProminences.Add(new PolityProminence(ppEnumerator.Current));

            Profiler.EndSample();
        }

        Profiler.BeginSample("TryMigrateFactionCores");

        TryMigrateFactionCores();

        Profiler.EndSample();

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
