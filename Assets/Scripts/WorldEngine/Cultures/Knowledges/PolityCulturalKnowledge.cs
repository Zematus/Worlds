using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PolityCulturalKnowledge : CulturalKnowledge
{
    public float AccValue = 0;
    
    public PolityCulturalKnowledge()
    {
    }

    public PolityCulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = 0; // this should be set by calling FinalizeUpdateFromFactions afterwards
    }

    public void FinalizeUpdateFromFactions()
    {
        Value = Mathf.FloorToInt(AccValue);
    }

    public override void Reset()
    {
        AccValue = 0;

        base.Reset();
    }
}
