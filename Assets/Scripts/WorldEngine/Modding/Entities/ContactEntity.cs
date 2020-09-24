
public class ContactEntity : Entity
{
    public const string PolityAttributeId = "polity";

    public virtual PolityContact Contact { get; private set; }

    private PolityEntity _polityEntity = null;

    public ContactEntity(Context c, string id) : base(c, id)
    {
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new PolityEntity(
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

    public void Set(PolityContact c)
    {
        Contact = c;

        _polityEntity?.Set(Contact.Polity);
    }

    public override void Set(object o)
    {
        if (o is ContactEntity e)
        {
            Set(e.Contact);
        }
        else if (o is PolityContact c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
