using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

public interface IFactionEventGenerator : IWorldEventGenerator
{
    bool TryGenerateEventAndAssign(
        Faction faction,
        WorldEvent originalEvent = null,
        bool reassign = false);

    string EventSetFlag { get; }
}
