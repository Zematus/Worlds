using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : CulturalEntity<Polity>
{
    public const string ContactsAttributeId = "contacts";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string TypeAttributeId = "type";
    public const string LeaderAttributeId = "leader";
    public const string SplitAttributeId = "split";
    public const string MergeAttributeId = "merge";
    public const string NeighborRegionsAttributeId = "neighbor_regions";
    public const string AddCoreRegionAttributeId = "add_core_region";
    public const string CoreRegionSaturationAttributeId = "core_region_saturation";
    public const string FactionsAttributeId = "factions";

    public virtual Polity Polity
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Polity;

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _coreRegionSaturationAttribute;

    private AgentEntity _leaderEntity = null;
    private FactionEntity _dominantFactionEntity = null;

    private RegionCollectionEntity _neighborRegionsEntity = null;
    private ContactCollectionEntity _contactsEntity = null;
    private FactionCollectionEntity _factionsEntity = null;

    public override string GetDebugString() => $"polity:{Polity.GetName()}";

    public override string GetFormattedString() => Polity.Name.BoldText;

    public PolityEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public PolityEntity(
        ValueGetterMethod<Polity> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public PolityEntity(
        TryRequestGenMethod<Polity> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public EntityAttribute GetDominantFactionAttribute()
    {
        _dominantFactionEntity =
            _dominantFactionEntity ?? new FactionEntity(
            GetDominantFaction,
            Context,
            BuildAttributeId(DominantFactionAttributeId),
            this);

        return _dominantFactionEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetNeighborRegionsAttribute()
    {
        _neighborRegionsEntity =
            _neighborRegionsEntity ?? new RegionCollectionEntity(
            GetNeighborRegions,
            Context,
            BuildAttributeId(NeighborRegionsAttributeId), 
            this);

        return _neighborRegionsEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetContactsAttribute()
    {
        _contactsEntity =
            _contactsEntity ?? new ContactCollectionEntity(
            GetContacts,
            Context,
            BuildAttributeId(ContactsAttributeId),
            this);

        return _contactsEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetFactionsAttribute()
    {
        _factionsEntity =
            _factionsEntity ?? new FactionCollectionEntity(
            GetFactions,
            Context,
            BuildAttributeId(FactionsAttributeId),
            this);

        return _factionsEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new AgentEntity(
                GetLeader,
                Context,
                BuildAttributeId(LeaderAttributeId),
                this);

        return _leaderEntity.GetThisEntityAttribute();
    }

    protected override CulturalPreferencesEntity CreateCulturalPreferencesEntity() =>
        new CulturalPreferencesEntity(
            GetCulture,
            Context,
            BuildAttributeId(PreferencesAttributeId),
            this);

    public static PolityType ConvertToType(string typeStr)
    {
        typeStr = typeStr.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(typeStr))
        {
            return PolityType.Any;
        }

        switch (typeStr)
        {
            case "tribe":
                return PolityType.Tribe;
            default:
                throw new System.Exception($"Unhandled polity type: {typeStr}");
        }
    }

    public Faction GetDominantFaction() => Polity.DominantFaction;

    public ICollection<Region> GetNeighborRegions() => Polity.NeighborRegions;

    public ICollection<PolityContact> GetContacts() => Polity.GetContacts();

    public ICollection<Faction> GetFactions() => Polity.GetFactions();

    public Agent GetLeader() => Polity.CurrentLeader;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new ValueGetterEntityAttribute<string>(
                        TypeAttributeId, this, () => Polity.TypeStr);
                return _typeAttribute;

            case LeaderAttributeId:
                return GetLeaderAttribute();

            case DominantFactionAttributeId:
                return GetDominantFactionAttribute();

            case TransferInfluenceAttributeId:
                return new TransferInfluenceAttribute(this, arguments);

            case SplitAttributeId:
                return new SplitPolityAttribute(this, arguments);

            case MergeAttributeId:
                return new MergePolityAttribute(this, arguments);

            case NeighborRegionsAttributeId:
                return GetNeighborRegionsAttribute();

            case ContactsAttributeId:
                return GetContactsAttribute();

            case FactionsAttributeId:
                return GetFactionsAttribute();

            case AddCoreRegionAttributeId:
                return new AddCoreRegionAttribute(this, arguments);

            case CoreRegionSaturationAttributeId:
                _coreRegionSaturationAttribute =
                    _coreRegionSaturationAttribute ?? new ValueGetterEntityAttribute<float>(
                        CoreRegionSaturationAttributeId, this, () => Polity.CoreRegionSaturation);
                return _coreRegionSaturationAttribute;
        }

        return base.GetAttribute(attributeId, arguments);
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        _leaderEntity?.Reset();
        _dominantFactionEntity?.Reset();
        _neighborRegionsEntity?.Reset();
        _contactsEntity?.Reset();

        base.ResetInternal();
    }

    public override Culture GetCulture() => Polity.Culture;
}
