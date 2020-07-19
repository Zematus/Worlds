using System.Xml.Serialization;

public class Identifier : Identifiable, System.IComparable
{
    public Identifier()
    {
    }

    public Identifier(long date, long id) : base(date, id)
    {
    }

    public Identifier(Identifiable identifiable) : base(identifiable.InitDate, identifiable.InitId)
    {
    }

    public Identifier(string idString)
    {
        string[] parts = idString.Split(':');

        if (!long.TryParse(parts[0], out long date))
            throw new System.ArgumentException("Not a valid date part: " + parts[0]);

        if (!long.TryParse(parts[1], out long id))
            throw new System.ArgumentException("Not a valid id part: " + parts[1]);

        Init(date, id);
    }

    public static implicit operator Identifier(string idString)
    {
        return new Identifier(idString);
    }

    public override void Synchronize()
    {
    }

    public override void FinalizeLoad()
    {
        _id = this;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override void Init(long date, long id)
    {
        InitDate = date;
        InitId = id;

        _id = this;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        return obj is Identifier ident &&
               InitDate == ident.InitDate &&
               InitId == ident.InitId;
    }

    public int CompareTo(object obj)
    {
        Identifier ident = obj as Identifier;

        if (ident == null) throw new System.ArgumentNullException("identifier to compare can't be null");

        int dateCompare = InitDate.CompareTo(ident.InitDate);

        if (dateCompare != 0) return dateCompare;

        return InitId.CompareTo(ident.InitId);
    }

    public static bool operator ==(Identifier left, Identifier right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Identifier left, Identifier right)
    {
        return !(left == right);
    }
}
