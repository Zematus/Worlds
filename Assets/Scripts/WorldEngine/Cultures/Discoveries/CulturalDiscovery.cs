using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(BoatMakingDiscovery))]
[XmlInclude(typeof(SailingDiscovery))]
[XmlInclude(typeof(TribalismDiscovery))]
[XmlInclude(typeof(PlantCultivationDiscovery))]
[XmlInclude(typeof(FactionCulturalDiscovery))]
public class CulturalDiscovery : CulturalDiscoveryInfo, IFilterableValue
{
    [XmlAttribute("P")]
    public bool IsPresent;

    [XmlIgnore]
    public bool WasPresent { get; private set; }

    public CulturalDiscovery()
    {
    }

    public CulturalDiscovery(string id, string name) : base(id, name)
    {
        IsPresent = false;
        WasPresent = false;
    }

    public CulturalDiscovery(CulturalDiscovery baseDiscovery) : base(baseDiscovery)
    {
        IsPresent = true;
        WasPresent = false;
    }

    public void Set(bool isPresent)
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            FactionCulturalDiscovery thisFactionDiscovery = this as FactionCulturalDiscovery;

            if ((thisFactionDiscovery != null) && (thisFactionDiscovery.Faction.Id == Manager.TracingData.FactionId))
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
                string callingMethod1 = method1.Name;
                string callingClass1 = method1.DeclaringType.ToString();

                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
                string callingMethod2 = method2.Name;
                string callingClass2 = method2.DeclaringType.ToString();

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "FactionCulturalDiscovery.Set (isPresent) - Faction:" + thisFactionDiscovery.Faction.Id,
                    "CurrentDate: " + thisFactionDiscovery.Faction.World.CurrentDate +
                    ", Id: " + Id +
                    ", IsPresent: " + IsPresent +
                    ", isPresent: " + isPresent +
                    //", WasPresent: " + WasPresent +
                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
                    "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        WasPresent = (WasPresent || IsPresent) && !isPresent;
        IsPresent = isPresent;
    }

    public void Set()
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            FactionCulturalDiscovery thisFactionDiscovery = this as FactionCulturalDiscovery;

            if ((thisFactionDiscovery != null) && (thisFactionDiscovery.Faction.Id == Manager.TracingData.FactionId))
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
                string callingMethod1 = method1.Name;
                string callingClass1 = method1.DeclaringType.ToString();

                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
                string callingMethod2 = method2.Name;
                string callingClass2 = method2.DeclaringType.ToString();

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "FactionCulturalDiscovery.Set - Faction:" + thisFactionDiscovery.Faction.Id,
                    "CurrentDate: " + thisFactionDiscovery.Faction.World.CurrentDate +
                    ", Id: " + Id +
                    ", IsPresent: " + IsPresent +
                    //", WasPresent: " + WasPresent +
                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
                    "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        IsPresent = true;
        WasPresent = false;
    }

    public void Reset()
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            FactionCulturalDiscovery thisFactionDiscovery = this as FactionCulturalDiscovery;

            if ((thisFactionDiscovery != null) && (thisFactionDiscovery.Faction.Id == Manager.TracingData.FactionId))
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
                string callingMethod1 = method1.Name;
                string callingClass1 = method1.DeclaringType.ToString();

                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
                string callingMethod2 = method2.Name;
                string callingClass2 = method2.DeclaringType.ToString();

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "FactionCulturalDiscovery.Reset - Faction:" + thisFactionDiscovery.Faction.Id,
                    "CurrentDate: " + thisFactionDiscovery.Faction.World.CurrentDate +
                    ", Id: " + Id +
                    ", IsPresent: " + IsPresent +
                    //", WasPresent: " + WasPresent +
                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
                    "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        IsPresent = false;
        WasPresent = true;
    }

    public bool ShouldFilter()
    {
        return !IsPresent;
    }
}
