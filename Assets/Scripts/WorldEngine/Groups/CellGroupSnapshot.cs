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

    public bool HasBandMigrationEvent;
    public long BandMigrationEventDate;
    public int BandMigrationTargetLongitude;
    public int BandMigrationTargetLatitude;

    public bool HasPolityExpansionEvent;
    public long PolityExpansionEventDate;
    public Identifier ExpansionTargetGroupId;
    public Identifier ExpandingPolityId;

    public bool HasTribeFormationEvent;
    public long TribeFormationEventDate;

    public CellGroupSnapshot(CellGroup group)
    {
        Id = group.Id;

        HasBandMigrationEvent = group.HasBandMigrationEvent;
        BandMigrationEventDate = group.BandMigrationEventDate;
        BandMigrationTargetLongitude = group.BandMigrationTargetLongitude;
        BandMigrationTargetLatitude = group.BandMigrationTargetLatitude;

        HasPolityExpansionEvent = group.HasPolityExpansionEvent;
        PolityExpansionEventDate = group.PolityExpansionEventDate;
        ExpansionTargetGroupId = group.ExpansionTargetGroupId;
        ExpandingPolityId = group.ExpandingPolityId;

        HasTribeFormationEvent = group.HasTribeFormationEvent;
        TribeFormationEventDate = group.TribeFormationEventDate;
    }
}
