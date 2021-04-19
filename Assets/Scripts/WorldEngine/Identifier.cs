using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Identifier : Identifiable, System.IComparable
{
    public Identifier()
    {
    }

    public Identifier(long date, long idSuffix) : base(date, idSuffix)
    {
    }

    public Identifier(Identifiable identifiable) : base(identifiable)
    {
    }

    public Identifier(string idString) : base(idString)
    {
    }

    public static implicit operator Identifier(string idString)
    {
        return string.IsNullOrEmpty(idString) ? null : new Identifier(idString);
    }

    public static implicit operator string(Identifier id)
    {
        return id?.ToString() ?? string.Empty;
    }

    public override int GetHashCode() => base.GetHashCode();

    protected override void Init(long date, long id)
    {
        InitDate = date;
        _idSuffix = id;

        _id = this;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        return obj is Identifier ident &&
               InitDate == ident.InitDate &&
               _idSuffix == ident._idSuffix;
    }

    public int CompareTo(object obj)
    {
        Identifier ident = obj as Identifier;

        if (ident == null)
            throw new System.ArgumentNullException("identifier to compare can't be null");

        int dateCompare = InitDate.CompareTo(ident.InitDate);

        if (dateCompare != 0) return dateCompare;

        return _idSuffix.CompareTo(ident._idSuffix);
    }

    public static bool operator ==(Identifier left, Identifier right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Identifier left, Identifier right)
    {
        return !(left == right);
    }
}
