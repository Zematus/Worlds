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
    public const float ValueScaleFactor = 0.01f;
    
    [XmlAttribute("V")]
    public int Value;

    [XmlAttribute("L")]
    public int Limit = -1;

    [XmlAttribute("PL")]
    public float ProgressLevel;

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value, int limit) : base(id, name)
    {
        Value = value;

        SetLimit(limit);
    }

    protected void SetLimit(int limit)
    {
        Limit = limit;

        UpdateProgressLevel();

        SetHighestLimit(limit);
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;
        Limit = baseKnowledge.Limit;
    }

    public float ScaledValue
    {
        get { return Value * ValueScaleFactor; }
    }

    public float ScaledLimit
    {
        get { return Limit * ValueScaleFactor; }
    }

    public void UpdateProgressLevel()
    {
        ProgressLevel = 0;

        if (Limit > 0)
            ProgressLevel = MathUtility.RoundToSixDecimals(Mathf.Clamp01(Value / (float)Limit));

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Group.Id == Manager.TracingData.GroupId)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.UpdateProgressLevel - Knowledge.Id:" + Id + ", Group.Id:" + Group.Id,
        //                    "CurrentDate: " + Group.World.CurrentDate +
        //                    ", ProgressLevel: " + ProgressLevel +
        //                    ", Value: " + Value +
        //                    ", Limit: " + Limit +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif
    }

    public int GetHighestLimit()
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        return (int)fInfo.GetValue(this);
    }

    public void SetHighestLimit(int value)
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        int currentValue = (int)fInfo.GetValue(this);

        if (value > currentValue)
        {
            fInfo.SetValue(this, value);
        }
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
        Limit = 0;
    }
}
