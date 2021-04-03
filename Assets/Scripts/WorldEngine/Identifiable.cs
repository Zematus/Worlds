using System.Xml.Serialization;

public abstract class Identifiable : ISynchronizable
{
    // This property is to be used exclusively for XML serialization.
    // It needs to be public. But consider it 'read-only protected' and don't access directly
    [XmlAttribute("ID")]
    public long InitDate;

    // This property is to be used exclusively for XML serialization.
    // It needs to be public. But consider it private and don't access it ever
    [XmlAttribute("IId")]
    public long InitId;

    protected Identifier _id = null;

    //NOTE: max long: 9,223,372,036,854,775,807

    public Identifiable()
    {
    }

    public Identifiable(long date, long id)
    {
        Init(date, id);
    }

    public Identifiable(Identifiable identifiable)
    {
        Init(identifiable);
    }

    protected void Init(Identifiable identifiable)
    {
        Init(identifiable.InitDate, identifiable.InitId);
    }

    protected virtual void Init(long date, long id)
    {
        InitDate = date;
        InitId = id;

        _id = new Identifier(InitDate, InitId);
    }

    public override string ToString()
    {
        return InitDate.ToString("D19") + ":" + InitId.ToString("D19");
    }

    public override int GetHashCode()
    {
        int hashCode = 1805739105;
        hashCode = hashCode * -1521134295 + InitDate.GetHashCode();
        hashCode = hashCode * -1521134295 + InitId.GetHashCode();
        return hashCode;
    }

    public abstract void Synchronize();

    public virtual void FinalizeLoad()
    {
        _id = new Identifier(InitDate, InitId);
    }

    public Identifier Id => _id;
}
