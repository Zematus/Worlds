using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Profiling;

public class PolityProminenceCluster : ISynchronizable
{
    public const int MaxSize = 50;
    public const int MinSplitSize = 25;

    [XmlAttribute("Id")]
    public long Id;

    [XmlAttribute("TAC")]
    public float TotalAdministrativeCost = 0;

    [XmlAttribute("TP")]
    public float TotalPopulation = 0;

    [XmlAttribute("PA")]
    public float ProminenceArea = 0;

    [XmlAttribute("NC")]
    public bool NeedsNewCensus = true;

    public DelayedLoadXmlSerializableDictionary<long, PolityProminence> Prominences = new DelayedLoadXmlSerializableDictionary<long, PolityProminence>();
    
    [XmlIgnore]
    public Polity Polity;

    public int Size
    {
        get
        {
            return Prominences.Count;
        }
    }

    public PolityProminenceCluster()
    {
    }

    public PolityProminenceCluster(PolityProminence startProminence)
    {
        Id = startProminence.Id;
        Polity = startProminence.Polity;

        AddProminence(startProminence);
    }

    public void RunCensus()
    {
        TotalAdministrativeCost = 0;
        TotalPopulation = 0;
        ProminenceArea = 0;

        Profiler.BeginSample("foreach group");

        foreach (PolityProminence prominence in Prominences.Values)
        {
            Profiler.BeginSample("add administrative cost");

            if (prominence.AdministrativeCost < float.MaxValue)
                TotalAdministrativeCost += prominence.AdministrativeCost;
            else
                TotalAdministrativeCost = float.MaxValue;

            Profiler.EndSample();

            Profiler.BeginSample("add pop");

            float polityPop = prominence.Group.Population * prominence.Value;

            TotalPopulation += polityPop;

            Profiler.EndSample();

            Profiler.BeginSample("add area");

            ProminenceArea += prominence.Group.Cell.Area;

            Profiler.EndSample();
        }

        Profiler.EndSample();

        NeedsNewCensus = false;
    }

    public void RequireNewCensus(bool state)
    {
        NeedsNewCensus |= state;

        Polity.NeedsNewCensus |= state;
    }

    public void AddProminence(PolityProminence prominence)
    {
        Prominences.Add(prominence.Id, prominence);
        prominence.Cluster = this;

        RequireNewCensus(true);
    }

    public void RemoveProminence(PolityProminence prominence)
    {
        Prominences.Remove(prominence.Id);

        RequireNewCensus(true);

        prominence.Cluster = null;
    }

    public bool HasPolityProminence(PolityProminence prominence)
    {
        return Prominences.ContainsKey(prominence.Id);
    }

    public bool HasPolityProminence(CellGroup group)
    {
        return Prominences.ContainsKey(group.Id);
    }

    public PolityProminence GetPolityProminence(CellGroup group)
    {
        if (Prominences.ContainsKey(group.Id))
        {
            return Prominences[group.Id];
        }

        return null;
    }

    private int _rngOffset;

    private int GetNextLocalRandomInt(int maxValue)
    {
        return Polity.GetNextLocalRandomInt(unchecked((int)Id) + _rngOffset, maxValue);
    }

    public PolityProminence GetRandomProminence(int rngOffset)
    {
        _rngOffset = rngOffset;
        
        return Prominences.Values.RandomSelect(GetNextLocalRandomInt);
    }

    public PolityProminenceCluster Split(PolityProminence startProminence)
    {
#if DEBUG
        int oldSize = Size;
#endif

        RemoveProminence(startProminence);
        PolityProminenceCluster splitCluster = new PolityProminenceCluster(startProminence);

        HashSet<CellGroup> groupsAlreadyTested = new HashSet<CellGroup>
        {
            startProminence.Group
        };

        Queue<PolityProminence> prominencesToExplore = new Queue<PolityProminence>(Prominences.Count + 1);
        prominencesToExplore.Enqueue(startProminence);

        bool continueExploring = true;
        while (continueExploring && (prominencesToExplore.Count > 0))
        {
            PolityProminence prominenceToExplore = prominencesToExplore.Dequeue();

            foreach (CellGroup nGroup in prominenceToExplore.Group.Neighbors.Values)
            {
                if (groupsAlreadyTested.Contains(nGroup))
                    continue;

                if (HasPolityProminence(nGroup))
                {
                    PolityProminence prominenceToAdd = Prominences[nGroup.Id];

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

    private PolityProminence GetProminenceOrThrow(long id)
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

    public void FinalizeLoad()
    {
        Prominences.FinalizeLoad(GetProminenceOrThrow);

        foreach (KeyValuePair<long, PolityProminence> pair in Prominences)
        {
            PolityProminence p = pair.Value;

            p.Group = Polity.World.GetGroup(pair.Key);
            p.Polity = Polity;
            p.Cluster = this;
        }
    }

    public void Synchronize()
    {
    }
}
