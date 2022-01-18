using System.Xml.Serialization;

public abstract class Identifiable
{
    // This property is to be used exclusively for XML serialization.
    // It needs to be public. But consider it private and don't access it ever
    [XmlAttribute("Id")]
    public string IdStr
    {
        get { return ToString(); }
        set { Init(value); }
    }

    [XmlIgnore]
    public long InitDate;

    protected long _idSuffix;

    protected Identifier _id = null;

    //NOTE: max long: 9,223,372,036,854,775,807

    public Identifiable()
    {
    }

    public Identifiable(long date, long idSuffix)
    {
        Init(date, idSuffix);
    }

    public Identifiable(string idString)
    {
        Init(idString);
    }

    public Identifiable(Identifiable identifiable)
    {
        Init(identifiable.InitDate, identifiable._idSuffix);
    }

    private void Init(string idString)
    {
        string[] parts = idString.Split(':');

        if (parts.Length != 2)
            throw new System.ArgumentException("Not a valid indentifier: " + idString);

        if (!long.TryParse(parts[0], out long date))
            throw new System.ArgumentException("Not a valid date part: " + parts[0]);

        if (!long.TryParse(parts[1], out long id))
            throw new System.ArgumentException("Not a valid id part: " + parts[1]);

        Init(date, id);
    }

    protected virtual void Init(long date, long idSuffix)
    {
        InitDate = date;
        _idSuffix = idSuffix;

        _id = new Identifier(InitDate, _idSuffix);
    }

    public override string ToString()
    {
        return InitDate.ToString("D1") + ":" + _idSuffix.ToString("D19");
    }

    public override int GetHashCode()
    {
        int hashCode = 1805739105;
        hashCode = hashCode * -1521134295 + InitDate.GetHashCode();
        hashCode = hashCode * -1521134295 + _idSuffix.GetHashCode();
        return hashCode;
    }

    public Identifier Id => _id;
}
