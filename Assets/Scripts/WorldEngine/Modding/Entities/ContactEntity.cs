
public class ContactEntity : DelayedSetEntity<PolityContact>
{
    public const string PolityAttributeId = "polity";
    public const string StrenghtAttributeId = "strength";

    public virtual PolityContact Contact
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<float> _strengthAttribute;

    private PolityEntity _polityEntity = null;

    public ContactEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ContactEntity(
        ValueGetterMethod<PolityContact> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public ContactEntity(
        TryRequestGenMethod<PolityContact> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new PolityEntity(
                GetPolity,
                Context,
                BuildAttributeId(PolityAttributeId),
                this);

        return _polityEntity.GetThisEntityAttribute();
    }

    protected override object _reference => Contact;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case PolityAttributeId:
                return GetPolityAttribute();

            case StrenghtAttributeId:
                _strengthAttribute =
                    _strengthAttribute ?? new ValueGetterEntityAttribute<float>(
                        StrenghtAttributeId, this, GetStrength);
                return _strengthAttribute;
        }

        return base.GetAttribute(attributeId, arguments);
    }

    public override string GetDebugString()
    {
        return "contact:" + Contact.NeighborPolity.Id;
    }

    public override string GetFormattedString() => Contact.NeighborPolity.Name.BoldText;

    protected override void ResetInternal()
    {
        if (_isReset) return;

        _polityEntity?.Reset();
    }

    public float GetStrength()
    {
        if (Contact == null)
        {
            return 0;
        }

        return Contact.Strength;
    }

    public Polity GetPolity()
    {
        if (Contact == null)
        {
            return null;
        }

        return Contact.NeighborPolity;
    }
}
