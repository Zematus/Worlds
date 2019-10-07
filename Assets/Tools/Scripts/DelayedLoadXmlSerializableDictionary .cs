using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public delegate TValue GetValueDelegate<TKey, TValue>(TKey key);

[System.Obsolete]
public class DelayedLoadXmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    private HashSet<TKey> _keyBuffer = null;

    public void FinalizeLoad(GetValueDelegate<TKey, TValue> getValue)
    {
        if (_keyBuffer == null)
        {
            throw new System.Exception("_keyBuffer is not initialized");
        }

        foreach (TKey key in _keyBuffer)
        {
            this.Add(key, getValue(key));
        }

        _keyBuffer = null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        _keyBuffer = new HashSet<TKey>();

        XmlSerializer serializer = new XmlSerializer(typeof(TKey[]));

        if (reader.Read() && !reader.IsEmptyElement)
        {
            TKey[] keys = (TKey[])serializer.Deserialize(reader);

            for (int i = 0; i < keys.Length; i++)
            {
                _keyBuffer.Add(keys[i]);
            }
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TKey[]));

        TKey[] keys = new TKey[this.Count];
        TValue[] values = new TValue[this.Count];

        int index = 0;
        foreach (KeyValuePair<TKey,TValue> pair in this)
        {
            keys[index] = pair.Key;
            values[index] = pair.Value;
            index++;
        }

        // We need to recreate the table to eliminate future inconsistencies from loaded files
        Clear();
        for (int i = 0; i < values.Length; i++)
        {
            this.Add(keys[i], values[i]);
        }

        serializer.Serialize(writer, keys);
    }
}

