using System.Xml.Serialization;
using ProtoBuf;

[ProtoContract]
public class CellLayerData
{
    [ProtoMember(1)]
    public string Id;

    public float Value;
    public float BaseValue;

    [ProtoMember(2)]
    public float Offset;

    public CellLayerData()
    {
    }

    public CellLayerData(CellLayerData source)
    {
        Id = source.Id;
        Value = source.Value;
        BaseValue = source.BaseValue;
        Offset = source.Offset;
    }
}
