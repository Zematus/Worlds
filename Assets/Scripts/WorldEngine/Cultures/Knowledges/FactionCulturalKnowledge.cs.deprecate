using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulturalKnowledge : CulturalKnowledge
{
    [XmlIgnore]
    public Faction Faction;

    [XmlIgnore]
    public PolityCulturalKnowledge PolityCulturalKnowledge;

    [XmlIgnore]
    public CellCulturalKnowledge CoreCulturalKnowledge;

    public FactionCulturalKnowledge()
    {
    }

    public FactionCulturalKnowledge(Faction faction, CellCulturalKnowledge coreKnowledge, PolityCulture polityCulture) : base(coreKnowledge)
    {
        Faction = faction;

        CoreCulturalKnowledge = coreKnowledge;

        SetPolityCulturalKnowledge(polityCulture);
    }

    public void SetPolityCulturalKnowledge(PolityCulture culture)
    {
//#if DEBUG
//        if (Manager.RegisterDebugEvent != null)
//        {
//            if (Manager.TracingData.Priority <= 1)
//            {
//                if (Faction.Polity.Id == Manager.TracingData.PolityId)
//                {
//                    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

//                    System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
//                    string callingMethod1 = method1.Name;
//                    string callingClass1 = method1.DeclaringType.ToString();

//                    System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
//                    string callingMethod2 = method2.Name;
//                    string callingClass2 = method2.DeclaringType.ToString();

//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "FactionCulturalKnowledge.SetPolityCulturalKnowledge - Faction:" + Faction.Id,
//                        "CurrentDate: " + Faction.World.CurrentDate +
//                        ", Faction.Polity.Id: " + Faction.Polity.Id +
//                        ", Name: " + Name +
//                        ", IsPresent: " + IsPresent +
//                        ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
//                        ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
//                        "", Faction.World.CurrentDate);

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//        }
//#endif

        PolityCulturalKnowledge = culture.GetKnowledge(Id) as PolityCulturalKnowledge;

        if (PolityCulturalKnowledge == null)
        {
            PolityCulturalKnowledge = new PolityCulturalKnowledge(Id, Name, 0);

            culture.AddKnowledge(PolityCulturalKnowledge, false);
        }
    }

    public void UpdatePolityKnowledge(float influence)
    {
        if (!IsPresent) return;

#if DEBUG
        if (Manager.RegisterDebugEvent != null)
        {
            if (Manager.TracingData.Priority <= 1)
            {
                if (Faction.Polity.Id == Manager.TracingData.PolityId)
                {
                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                        "FactionCulturalKnowledge.UpdatePolityKnowledge - Faction:" + Faction.Id,
                        "CurrentDate: " + Faction.World.CurrentDate +
                        ", Faction.Polity.Id: " + Faction.Polity.Id +
                        ", Name: " + Name +
                        ", IsPresent: " + IsPresent +
                        "", Faction.World.CurrentDate);

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
            }
        }
#endif

        //Profiler.BeginSample("PolityCulturalKnowledge.Set()");

        PolityCulturalKnowledge.Set();

        //Profiler.EndSample();

        //Profiler.BeginSample("PolityCulturalKnowledge.AccValue");

        PolityCulturalKnowledge.AccValue += Value * influence;

        //Profiler.EndSample();
    }

    public void UpdateFromCoreKnowledge(float timeFactor)
    {
        int targetValue = 0;

        if ((CoreCulturalKnowledge != null) && CoreCulturalKnowledge.IsPresent)
        {
            Set();

            targetValue = CoreCulturalKnowledge.Value;
        }

        if (!IsPresent)
            return;

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (Manager.TracingData.FactionId == Faction.Id)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "FactionCulturalKnowledge:UpdateFromCoreKnowledge - Faction.Id:" + Faction.Id,
//                    "CurrentDate: " + Faction.World.CurrentDate +
//                    ", Id: " + Id +
//                    ", IsPresent: " + IsPresent +
//                    ", Value: " + Value +
//                    ", targetValue: " + targetValue +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        float decimals;

        Value = MathUtility.LerpToIntAndGetDecimals(Value, targetValue, timeFactor, out decimals);

        if (decimals > Faction.GetNextLocalRandomFloat(RngOffsets.KNOWLEDGE_FACTION_CORE_UPDATE + unchecked((int)Faction.Id)))
        {
            Value++;
        }

        if (Value <= 0)
        {
            Reset();
        }
    }
}
