using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
[XmlInclude(typeof(PolityCulturalKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo
{
    [XmlAttribute("V")]
    public float Value;

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, float value) : base(id, name)
    {
        Value = value;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;
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
