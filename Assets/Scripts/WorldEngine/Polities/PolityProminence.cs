using ProtoBuf;

[ProtoContract]
public class PolityProminence : IKeyedValue<long>
{
    [ProtoMember(1)]
    public long PolityId;
    [ProtoMember(2)]
    public float Value;
    [ProtoMember(3)]
    public float FactionCoreDistance;
    [ProtoMember(4)]
    public float PolityCoreDistance;
    [ProtoMember(5)]
    public float AdministrativeCost;

    public float NewValue;
    public float NewFactionCoreDistance;
    public float NewPolityCoreDistance;

    public PolityProminenceCluster Cluster;

    //private bool _isMigratingGroup;

    public Polity Polity;

    public CellGroup Group;

    public long Id => Group.Id;

    public PolityProminence()
    {

    }

    public PolityProminence(PolityProminence polityProminence)
    {
        Group = polityProminence.Group;

        //_isMigratingGroup = true;

        Set(polityProminence);
    }

    public PolityProminence(CellGroup group, PolityProminence polityProminence)
    {
        Group = group;

        //_isMigratingGroup = false;

        Set(polityProminence);
    }

    public void Set(PolityProminence polityProminence)
    {
        PolityId = polityProminence.PolityId;
        Polity = polityProminence.Polity;
        Value = polityProminence.Value;
        NewValue = Value;

        AdministrativeCost = 0;
    }

    public PolityProminence(CellGroup group, Polity polity, float value, bool isMigratingGroup = false)
    {
        Group = group;

        //_isMigratingGroup = isMigratingGroup;

        Set(polity, value);
    }

    public void Set(Polity polity, float value)
    {
        PolityId = polity.Id;
        Polity = polity;
        Value = MathUtility.RoundToSixDecimals(value);
        NewValue = Value;

        AdministrativeCost = 0;
    }

    public void PostUpdate()
    {
        Value = NewValue;

        PolityCoreDistance = NewPolityCoreDistance;
        FactionCoreDistance = NewFactionCoreDistance;

        Cluster?.RequireNewCensus(true);

#if DEBUG
        if (FactionCoreDistance == -1)
        {
            throw new System.Exception("Core distance is not properly initialized");
        }
#endif

#if DEBUG
        if (PolityCoreDistance == -1)
        {
            throw new System.Exception("Core distance is not properly initialized");
        }
#endif
    }

    public long GetKey()
    {
        return PolityId;
    }
}
