using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public delegate TValue GetValueDelegate<TKey, TValue>(TKey key);

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

        int index = 0;
        foreach (TKey key in Keys)
        {
            keys[index] = key;
            index++;
        }

        serializer.Serialize(writer, keys);
    }
}

