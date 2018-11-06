using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public interface IKeyedValue<TKey>
{
    TKey GetKey();
}

public class XmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TValue : IKeyedValue<TKey>
{
    private void Recreate(TValue[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            this.Add(values[i].GetKey(), values[i]);
        }
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TValue[]));

        if (reader.Read() && !reader.IsEmptyElement)
        {
            TValue[] values = (TValue[])serializer.Deserialize(reader);

            Recreate(values);
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TValue[]));

        TValue[] values = new TValue[this.Count];

        int index = 0;
        foreach (TValue value in Values)
        {
            values[index] = value;
            index++;
        }

        // We need to recreate the table to eliminate future inconsistencies from loaded files
        Clear();
        Recreate(values);

        serializer.Serialize(writer, values);
    }
}

