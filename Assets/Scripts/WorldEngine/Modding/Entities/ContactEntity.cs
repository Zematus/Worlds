
public class ContactEntity : DelayedSetEntity<PolityContact>
{
    public const string PolityAttributeId = "polity";

    public virtual PolityContact Contact
    {
        get => Setable;
        private set => Setable = value;
    }

    private DelayedSetPolityEntity _polityEntity = null;

    private bool _alreadyReset = false;

    public ContactEntity(Context c, string id) : base(c, id)
    {
    }

    public ContactEntity(
        ValueGetterMethod<PolityContact> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new DelayedSetPolityEntity(
                GetPolity,
                Context,
                BuildAttributeId(PolityAttributeId));

        return _polityEntity.GetThisEntityAttribute(this);
    }

    protected override object _reference => Contact;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case PolityAttributeId:
                return GetPolityAttribute();
        }

        throw new System.ArgumentException("Group: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return "contact:" + Contact.Polity.Id;
    }

    public override string GetFormattedString()
    {
        return Contact.Polity.Name.BoldText;
    }

    protected override void ResetInternal()
    {
        if (_isReset)
        {
            return;
        }

        _polityEntity?.Reset();
    }

    public Polity GetPolity() => Contact.Polity;
}
