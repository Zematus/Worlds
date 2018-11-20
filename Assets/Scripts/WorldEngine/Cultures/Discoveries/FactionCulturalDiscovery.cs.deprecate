using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulturalDiscovery : CulturalDiscovery
{
#if DEBUG
    [XmlIgnore]
    public long AcquisitionDate = -1; // This property is used for debugging purposes
#endif

    [XmlIgnore]
    public Faction Faction;

    [XmlIgnore]
    public CulturalDiscovery PolityCulturalDiscovery;

    [XmlIgnore]
    public CellCulturalDiscovery CoreCulturalDiscovery;

    public FactionCulturalDiscovery()
    {
    }

    public FactionCulturalDiscovery(Faction faction, CellCulturalDiscovery coreDiscovery, PolityCulture polityCulture) : base(coreDiscovery)
    {
        Faction = faction;

        CoreCulturalDiscovery = coreDiscovery;

        SetPolityCulturalDiscovery(polityCulture);

#if DEBUG
        //if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //{
        //    if (Faction.Id == Manager.TracingData.FactionId)
        //    {
        //        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //        System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
        //        string callingMethod1 = method1.Name;
        //        string callingClass1 = method1.DeclaringType.ToString();

        //        System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
        //        string callingMethod2 = method2.Name;
        //        string callingClass2 = method2.DeclaringType.ToString();

        //        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //            "FactionCulturalDiscovery.FactionCulturalDiscovery - Faction:" + Faction.Id,
        //            "CurrentDate: " + Faction.World.CurrentDate +
        //            ", Id: " + Id +
        //            ", IsPresent: " + IsPresent +
        //            ", ((CoreCulturalDiscovery != null) && (CoreCulturalDiscovery.IsPresent)): " 
        //            + ((CoreCulturalDiscovery != null) && (CoreCulturalDiscovery.IsPresent)) +
        //            //", WasPresent: " + WasPresent +
        //            ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
        //            ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
        //            "");

        //        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //    }
        //}

        AcquisitionDate = faction.World.CurrentDate;
#endif
    }

    public void SetPolityCulturalDiscovery(PolityCulture culture)
    {
        PolityCulturalDiscovery = culture.GetDiscovery(Id);

        if (PolityCulturalDiscovery == null)
        {
            PolityCulturalDiscovery = new CulturalDiscovery(Id, Name);

            culture.AddDiscovery(PolityCulturalDiscovery, false); // will be updated as present if needed by the polity's cultural update
        }
    }

    public void UpdatePolityDiscovery()
    {
        if (IsPresent)
        {
            //Profiler.BeginSample("PolityCulturalDiscovery.Set(true)");

            PolityCulturalDiscovery.Set(true);

            //Profiler.EndSample();
        }
    }

    public void UpdateFromCoreDiscovery()
    {
        Set(true);

#if DEBUG
        //if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //{
        //    if (Faction.Id == Manager.TracingData.FactionId)
        //    {
        //        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //        System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
        //        string callingMethod1 = method1.Name;
        //        string callingClass1 = method1.DeclaringType.ToString();

        //        System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
        //        string callingMethod2 = method2.Name;
        //        string callingClass2 = method2.DeclaringType.ToString();

        //        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //            "FactionCulturalDiscovery.UpdateFromCoreDiscovery - Faction:" + Faction.Id,
        //            "CurrentDate: " + Faction.World.CurrentDate +
        //            ", Id: " + Id +
        //            ", IsPresent: " + IsPresent +
        //            //", WasPresent: " + WasPresent +
        //            ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
        //            ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
        //            "");

        //        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //    }
        //}

        AcquisitionDate = Faction.World.CurrentDate;
#endif
    }
}
