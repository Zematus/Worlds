using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
[XmlInclude(typeof(PolityCulturalKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo
{
    public const int ScaledMaxLimitValue = 10000;
    public const int ScaledMinLimitValue = 1;

    public const int MinLimitValue = ScaledMinLimitValue * MathUtility.FloatToIntScalingFactor;
    public const int MaxLimitValue = ScaledMaxLimitValue * MathUtility.FloatToIntScalingFactor;

    [XmlAttribute("V")]
    public int Value;

    [XmlAttribute("PL")]
    public float ProgressLevel;

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value) : base(id, name)
    {
        Value = value;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;
    }

    public float ScaledValue
    {
        get { return Value * MathUtility.IntToFloatScalingFactor; }
    }

    public int GetHighestLimit()
    {
        //Profiler.BeginSample("Reflection...");

        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        //Profiler.EndSample();

        return (int)fInfo.GetValue(this);
    }

    public void SetHighestLimit(int value)
    {
        //Profiler.BeginSample("Reflection...");

        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        int currentValue = (int)fInfo.GetValue(this);

        if (value > currentValue)
        {
            fInfo.SetValue(this, value);
        }

        //Profiler.EndSample();
    }

    public virtual void Reset()
    {
        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            CellCulturalKnowledge thisCellKnowledge = this as CellCulturalKnowledge;

        //            if ((thisCellKnowledge != null) && (thisCellKnowledge.Group.Id == Manager.TracingData.GroupId))
        //            {
        //                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
        //                string callingMethod1 = method1.Name;
        //                string callingClass1 = method1.DeclaringType.ToString();

        //                //System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
        //                //string callingMethod2 = method2.Name;
        //                //string callingClass2 = method2.DeclaringType.ToString();

        //                CellGroup group = thisCellKnowledge.Group;

        //                string groupId = "Id:" + group.Id + "|Long:" + group.Longitude + "|Lat:" + group.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.Reset - Group:" + groupId,
        //                    "CurrentDate: " + group.World.CurrentDate +
        //                    ", Id: " + Id +
        //                    ", IsPresent: " + IsPresent +
        //                    //", WasPresent: " + WasPresent +
        //                    ", Value: " + Value +
        //                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
        //                    //", Calling method 2: " + callingClass2 + "." + callingMethod2 +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        Value = 0;
    }
}
