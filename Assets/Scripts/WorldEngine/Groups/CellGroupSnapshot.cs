using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine.Profiling;

public class CellGroupSnapshot
{
    public long Id;

    public bool HasMigrationEvent;
    public long MigrationEventDate;
    public int MigrationTargetLongitude;
    public int MigrationTargetLatitude;

    public bool HasPolityExpansionEvent;
    public long PolityExpansionEventDate;
    public long ExpansionTargetGroupId;
    public long ExpandingPolityId;

    public bool HasTribeFormationEvent;
    public long TribeFormationEventDate;

    public CellGroupSnapshot(CellGroup c)
    {
        Id = c.Id;

        HasMigrationEvent = c.HasMigrationEvent;
        MigrationEventDate = c.MigrationEventDate;
        MigrationTargetLongitude = c.MigrationTargetLongitude;
        MigrationTargetLatitude = c.MigrationTargetLatitude;

        HasPolityExpansionEvent = c.HasPolityExpansionEvent;
        PolityExpansionEventDate = c.PolityExpansionEventDate;
        ExpansionTargetGroupId = c.ExpansionTargetGroupId;
        ExpandingPolityId = c.ExpandingPolityId;

        HasTribeFormationEvent = c.HasTribeFormationEvent;
        TribeFormationEventDate = c.TribeFormationEventDate;
    }
}
