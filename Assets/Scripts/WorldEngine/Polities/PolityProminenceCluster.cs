using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Profiling;

public class PolityProminenceCluster : Identifiable
{
    public const int MaxSize = 50;
    public const int MinSplitSize = 25;

    [XmlAttribute("TAC")]
    public float TotalAdministrativeCost = 0;

    [XmlAttribute("TP")]
    public float TotalPopulation = 0;

    [XmlAttribute("PA")]
    public float ProminenceArea = 0;

    [XmlAttribute("NC")]
    public bool NeedsNewCensus = true;

    public List<Identifier> ProminenceIds = null;

    [XmlIgnore]
    public Polity Polity;

    private int _rngOffset;

#if DEBUG
    [XmlIgnore]
    public long LastProminenceChangeDate = -1;
#endif

    public int Size => _prominences.Count;

    private Dictionary<Identifier, PolityProminence> _prominences =
        new Dictionary<Identifier, PolityProminence>();

    public PolityProminenceCluster()
    {
    }

    public PolityProminenceCluster(PolityProminence startProminence) :
        base(startProminence.Group)
    {
        Polity = startProminence.Polity;

        AddProminence(startProminence);
    }

    public void RunCensus()
    {
        TotalAdministrativeCost = 0;
        TotalPopulation = 0;
        ProminenceArea = 0;

        foreach (PolityProminence prominence in _prominences.Values)
        {
            if (prominence.AdministrativeCost < float.MaxValue)
                TotalAdministrativeCost += prominence.AdministrativeCost;
            else
                TotalAdministrativeCost = float.MaxValue;

            float polityPop = prominence.Group.Population * prominence.Value;

            TotalPopulation += polityPop;

            ProminenceArea += prominence.Group.Cell.Area;
        }

        NeedsNewCensus = false;
    }

    public void RequireNewCensus(bool state)
    {
        NeedsNewCensus |= state;

        Polity.NeedsNewCensus |= state;
    }

    public void AddProminence(PolityProminence prominence)
    {
        _prominences.Add(prominence.Id, prominence);
        prominence.Cluster = this;

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Manager.TracingData.ClusterId == Id)
        //            {
        //                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

        //                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
        //                string callingMethod1 = method1.Name;
        //                string callingClass1 = method1.DeclaringType.ToString();
        //                int line1 = stackTrace.GetFrame(1).GetFileLineNumber();

        //                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
        //                string callingMethod2 = method2.Name;
        //                string callingClass2 = method2.DeclaringType.ToString();
        //                int line2 = stackTrace.GetFrame(2).GetFileLineNumber();

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("PolityProminenceCluster.AddProminence - Cluster:" + Id,
        //                    "CurrentDate: " + Polity.World.CurrentDate +
        //                    ", Size: " + Size +
        //                    ", Polity.Id: " + Polity.Id +
        //                    ", LastProminenceChangeDate: " + LastProminenceChangeDate +
        //                    " [offset: " + (LastProminenceChangeDate - Manager.TracingData.LastSaveDate) + "]" +
        //                    ", prominence.Id: " + prominence.Id +
        //                    ", prominence.Group.LastUpdateDate: " + prominence.Group.LastUpdateDate +
        //                    " [offset: " + (prominence.Group.LastUpdateDate - Manager.TracingData.LastSaveDate) + "]" +
        //                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 + " [line:" + line1 + "]" +
        //                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 + " [line:" + line2 + "]" +
        //                    "", Polity.World.CurrentDate);

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }

        //        LastProminenceChangeDate = Polity.World.CurrentDate;
        //#endif

        RequireNewCensus(true);
    }

    public void RemoveProminence(PolityProminence prominence)
    {
        _prominences.Remove(prominence.Id);

        RequireNewCensus(true);

        prominence.Cluster = null;

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Manager.TracingData.ClusterId == Id)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("PolityProminenceCluster.RemoveProminence - Cluster:" + Id,
        //                    "CurrentDate: " + Polity.World.CurrentDate +
        //                    ", Size: " + Size +
        //                    ", LastProminenceChangeDate: " + LastProminenceChangeDate +
        //                    " [offset: " + (LastProminenceChangeDate - Manager.TracingData.LastSaveDate) + "]" +
        //                    ", prominence.Id: " + prominence.Id +
        //                    ", prominence.Group.LastUpdateDate: " + prominence.Group.LastUpdateDate +
        //                    " [offset: " + (prominence.Group.LastUpdateDate - Manager.TracingData.LastSaveDate) + "]" +
        //                    "", Polity.World.CurrentDate);

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }

        //        LastProminenceChangeDate = Polity.World.CurrentDate;
        //#endif
    }

    public bool HasPolityProminence(PolityProminence prominence)
    {
        return _prominences.ContainsKey(prominence.Id);
    }

    public bool HasPolityProminence(CellGroup group)
    {
        return _prominences.ContainsKey(group.Id);
    }

    public ICollection<PolityProminence> GetPolityProminences()
    {
        return _prominences.Values;
    }

    public PolityProminence GetPolityProminence(CellGroup group)
    {
        if (_prominences.ContainsKey(group.Id))
        {
            return _prominences[group.Id];
        }

        return null;
    }

    private int GetNextLocalRandomInt(int maxValue)
    {
        return Polity.GetNextLocalRandomInt(GetHashCode() + _rngOffset, maxValue);
    }

    public PolityProminence GetRandomProminence(int rngOffset)
    {
        _rngOffset = rngOffset;

        return _prominences.Values.RandomSelect(GetNextLocalRandomInt);
    }

    public PolityProminenceCluster Split(PolityProminence startProminence)
    {
#if DEBUG
        int oldSize = Size;
#endif

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Manager.TracingData.ClusterId == Id)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("PolityProminenceCluster.Split - Cluster:" + Id,
        //                    "CurrentDate: " + Polity.World.CurrentDate +
        //                    ", Size: " + Size +
        //                    ", LastProminenceChangeDate: " + LastProminenceChangeDate +
        //                    " [offset: " + (LastProminenceChangeDate - Manager.TracingData.LastSaveDate) + "]" +
        //                    "", Polity.World.CurrentDate);

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }

        //        LastProminenceChangeDate = Polity.World.CurrentDate;
        //#endif

        RemoveProminence(startProminence);
        PolityProminenceCluster splitCluster = new PolityProminenceCluster(startProminence);

        HashSet<CellGroup> groupsAlreadyTested = new HashSet<CellGroup>
        {
            startProminence.Group
        };

        Queue<PolityProminence> prominencesToExplore = new Queue<PolityProminence>(_prominences.Count + 1);
        prominencesToExplore.Enqueue(startProminence);

        bool continueExploring = true;
        while (continueExploring && (prominencesToExplore.Count > 0))
        {
            PolityProminence prominenceToExplore = prominencesToExplore.Dequeue();

            foreach (CellGroup nGroup in prominenceToExplore.Group.NeighborGroups)
            {
                if (groupsAlreadyTested.Contains(nGroup))
                    continue;

                if (HasPolityProminence(nGroup))
                {
                    PolityProminence prominenceToAdd = _prominences[nGroup.Id];

                    RemoveProminence(prominenceToAdd);
                    splitCluster.AddProminence(prominenceToAdd);

                    Manager.AddUpdatedCell(prominenceToAdd.Group.Cell, CellUpdateType.Cluster, CellUpdateSubType.Membership);

                    if (splitCluster.Size >= MinSplitSize)
                    {
                        continueExploring = false;
                        break;
                    }

                    prominencesToExplore.Enqueue(prominenceToAdd);
                }

                groupsAlreadyTested.Add(nGroup);
            }
        }

        //#if DEBUG
        //        Debug.Log("Splitted cluster (Id: " + Id + ") of size " + oldSize + " and created new cluster (Id: " + splitCluster.Id + ") of size " + splitCluster.Size);
        //#endif

        return splitCluster;
    }

    private PolityProminence GetProminenceOrThrow(Identifier id)
    {
        CellGroup group = Polity.World.GetGroup(id);

        if (group == null)
        {
            string message = "Missing Group with Id " + id + " in PolityProminenceCluster of Polity with Id " + Polity.Id;
            throw new System.Exception(message);
        }

        PolityProminence prominence = group.GetPolityProminence(Polity);

        if (prominence == null)
        {
            string message = "Missing polity prominence with Id " + id + " in PolityProminenceCluster of Polity with Id " + Polity.Id;
            throw new System.Exception(message);
        }

        return prominence;
    }

    private void LoadProminences()
    {
        foreach (Identifier id in ProminenceIds)
        {
            _prominences.Add(id, GetProminenceOrThrow(id));
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        LoadProminences();

        foreach (KeyValuePair<Identifier, PolityProminence> pair in _prominences)
        {
            PolityProminence p = pair.Value;

            World world = Polity.World;

            p.Group = world.GetGroup(pair.Key);
            p.Polity = Polity;
            p.ClosestFaction = Polity.GetFaction(p.ClosestFactionId);
            p.Cluster = this;
        }
    }

    public override void Synchronize()
    {
        ProminenceIds = new List<Identifier>(_prominences.Keys);

        // Reload prominences to make sure they are ordered as in the save file
        _prominences.Clear();
        LoadProminences();
    }
}
