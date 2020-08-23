using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine.Profiling;

public class CellGroupSnapshot
{
    public Identifier Id;

    public bool HasMigrationEvent;
    public long MigrationEventDate;
    public int MigrationTargetLongitude;
    public int MigrationTargetLatitude;

    public bool HasTribeFormationEvent;
    public long TribeFormationEventDate;

    public CellGroupSnapshot(CellGroup group)
    {
        Id = group.Id;

        HasMigrationEvent = group.HasMigrationEvent;
        MigrationEventDate = group.MigrationEventDate;
        MigrationTargetLongitude = group.MigrationTargetLongitude;
        MigrationTargetLatitude = group.MigrationTargetLatitude;

        HasTribeFormationEvent = group.HasTribeFormationEvent;
        TribeFormationEventDate = group.TribeFormationEventDate;
    }
}
