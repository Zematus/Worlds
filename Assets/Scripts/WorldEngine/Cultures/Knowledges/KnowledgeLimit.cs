using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class KnowledgeLimit : ISynchronizable
{
    public const float MaxLimitValue = 10000;
    public const float MinLimitValue = 1;

    [XmlAttribute]
    public string Id;

    [XmlAttribute("V")]
    public float Value = 0;

    [XmlIgnore]
    public CellGroup Group;

    public KnowledgeLimit()
    {
        SetHighestValue(Value);
    }

    public KnowledgeLimit(
        CellGroup group,
        string id,
        float value = 0)
    {
        Id = id;
        Group = group;
        SetValueInternal(value);
    }

    public void SetValue(float value)
    {
        if (!value.IsInsideRange(MinLimitValue, MaxLimitValue))
        {
            string message =
                $"KnowledgeLimit: Limit can't be set below {MinLimitValue} or above {MaxLimitValue}" +
                $", id: {Id}, limit: {value}";
            Debug.LogWarning(message);

            value = Mathf.Clamp(value, MinLimitValue, MaxLimitValue);
        }

        SetValueInternal(value);
    }

    private void SetValueInternal(float value)
    {
        Value = value;

        SetHighestValue(value);
    }

    private void SetHighestValue(float value)
    {
        var knowledge = Knowledge.GetKnowledge(Id);

        if (knowledge == null)
        {
            string message =
                $"KnowledgeLimit: Knowledge '{Id}' not present";
            throw new System.Exception(message);
        }

        if (knowledge.HighestLimitValue < value)
        {
            knowledge.HighestLimitValue = value;
        }
    }

    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
    }
}
