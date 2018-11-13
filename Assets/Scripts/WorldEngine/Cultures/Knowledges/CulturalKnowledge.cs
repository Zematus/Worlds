using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
[XmlInclude(typeof(FactionCulturalKnowledge))]
[XmlInclude(typeof(PolityCulturalKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo, IFilterableValue
{
    public const float ValueScaleFactor = 0.01f;

    [XmlAttribute("P")]
    public bool IsPresent;

    [XmlAttribute("V")]
    public int Value;

    [XmlIgnore]
    public bool WasPresent { get; private set; }

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value) : base(id, name)
    {
        Value = value;

        IsPresent = false;
        WasPresent = false;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;

        IsPresent = baseKnowledge.IsPresent;
        WasPresent = false;
    }

    public float ScaledValue
    {
        get { return Value * ValueScaleFactor; }
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

        IsPresent = false;
        WasPresent = true;
    }

    public void Set()
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

//                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
//                string callingMethod2 = method2.Name;
//                string callingClass2 = method2.DeclaringType.ToString();

//                CellGroup group = thisCellKnowledge.Group;

//                string groupId = "Id:" + group.Id + "|Long:" + group.Longitude + "|Lat:" + group.Latitude;

//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "CellCulturalKnowledge.Set - Group:" + groupId,
//                    "CurrentDate: " + group.World.CurrentDate +
//                    ", Id: " + Id +
//                    ", IsPresent: " + IsPresent +
//                    //", WasPresent: " + WasPresent +
//                    ", Value: " + Value +
//                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
//                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        IsPresent = true;
        WasPresent = false;
    }

    public bool ShouldFilter()
    {
        return !IsPresent;
    }
}
