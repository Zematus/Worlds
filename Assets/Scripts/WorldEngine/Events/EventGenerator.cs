using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class EventGenerator
{
    public const string FactionTargetType = "faction";
    public const string GroupTargetType = "group";

    public static long CurrentUId = StartUId;

    public EventContext Context;

    public long UId;

    public int IdHash;

    public string Id;
    public string Name;

    public IBooleanExpression[] AssignmentConditions;
    public IBooleanExpression[] TriggerConditions;

    public INumericExpression TimeToTrigger;

    public IEffectExpression[] Effects;

    // TODO: Should start at zero
    private const long StartUId = WorldEvent.PlantCultivationDiscoveryEventId + 1;
    private static bool doOnce = true;

    public EventGenerator()
    {
#if DEBUG
        if (doOnce && (StartUId > 0))
        {
            Debug.LogWarning("Event UIds should start at 0. Currently starts at " + StartUId);
            doOnce = false;
        }
#endif
    }

    public bool CanAssignToTarget(IEventTarget target)
    {
        return false;
    }

    public SimulationEvent GenerateEvent()
    {
        SimulationEvent simEvent = new SimulationEvent()
        {
            Context = Context,
            Generator = this
        };

        return simEvent;
    }
}
