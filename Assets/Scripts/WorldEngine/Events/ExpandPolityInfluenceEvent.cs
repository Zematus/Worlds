using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ExpandPolityProminenceEvent : CellGroupEvent
{
    public Identifier TargetGroupId;
    public Identifier PolityId;

    [XmlIgnore]
    public CellGroup TargetGroup;
    [XmlIgnore]
    public Polity Polity;

    public ExpandPolityProminenceEvent()
    {
        DoNotSerialize = true;
    }

    public ExpandPolityProminenceEvent(
        CellGroup group, Polity polity, CellGroup targetGroup, long triggerDate) :
        base(group, triggerDate, ExpandPolityProminenceEventId)
    {
        Polity = polity;

        PolityId = polity.Id;

        TargetGroup = targetGroup;

        TargetGroupId = TargetGroup.Id;

        DoNotSerialize = true;
    }

    public override bool IsStillValid()
    {
        if (!base.IsStillValid())
            return false;

        if (Polity == null)
            return false;

        if (!Polity.StillPresent)
            return false;

        if (TargetGroup == null)
            return false;

        if (!TargetGroup.StillPresent)
            return false;

        return true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        PolityProminence sourcePi = Group.GetPolityProminence(Polity);

        if (sourcePi == null)
            return false;

        return true;
    }

    public override void Trigger()
    {
        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (Group.Id == Manager.TracingData.GroupId) {
        //				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"ExpandPolityProminence:Trigger - Group:" + groupId,
        //					"CurrentDate: " + World.CurrentDate + 
        //					", TriggerDate: " + TriggerDate + 
        //					", SpawnDate: " + SpawnDate +
        //					", PolityId: " + PolityId + 
        //					", TargetGroup Id: " + TargetGroupId + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        float randomFactor = Group.Cell.GetNextLocalRandomFloat(RngOffsets.EVENT_TRIGGER + unchecked((int)Id));
        float percentToExpand = Mathf.Pow(randomFactor, 4);

        float populationFactor = Group.Population / (float)(Group.Population + TargetGroup.Population);
        percentToExpand *= populationFactor;

        float value = Group.GetPolityProminenceValue(Polity);

        TargetGroup.Culture.MergeCulture(Group.Culture, percentToExpand);

        TargetGroup.MergePolityProminence(Polity, value, percentToExpand);

        TryMigrateFactionCores();

        World.AddGroupToUpdate(Group);
        World.AddGroupToUpdate(TargetGroup);
    }

    private void TryMigrateFactionCores()
    {
        List<Faction> factionCoresToMigrate = new List<Faction>();

        foreach (Faction faction in Group.GetFactionCores())
        {
            if (faction.ShouldMigrateFactionCore(Group, TargetGroup))
            {
                factionCoresToMigrate.Add(faction);
            }
        }

        foreach (Faction faction in factionCoresToMigrate)
        {
            faction.SetToUpdate();

            faction.PrepareNewCoreGroup(TargetGroup);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        TargetGroup = World.GetGroup(TargetGroupId);
        Polity = World.GetPolity(PolityId);

        Group.PolityExpansionEvent = this;
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.HasPolityExpansionEvent = false;
        }

        base.DestroyInternal();
    }

    public void Reset(Polity polity, CellGroup targetGroup, long triggerDate)
    {
        TargetGroup = targetGroup;
        TargetGroupId = TargetGroup.Id;

        Polity = polity;
        PolityId = Polity.Id;

        Reset(triggerDate);
    }
}
