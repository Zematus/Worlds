using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// Identifies if the migration is over land or water
/// </summary>
public enum MigrationType
{
    Land = 0,
    Sea = 1
}

/// <summary>
/// Defines a generic population migration event
/// </summary>
public class MigratePopulationEvent : CellGroupEvent
{
    [XmlAttribute("TLon")]
    public int TargetCellLongitude;
    [XmlAttribute("TLat")]
    public int TargetCellLatitude;

    [XmlAttribute("MDir")]
    public int MigrationDirectionInt;

    [XmlAttribute("MTyp")]
    public int MigrationTypeInt;

    [XmlAttribute("PPer")]
    public float ProminencePercent;

    public Identifier PolityId = null;

    [XmlIgnore]
    public TerrainCell TargetCell;

    [XmlIgnore]
    public Direction MigrationDirection;

    [XmlIgnore]
    public MigrationType MigrationType;

    [XmlIgnore]
    public Polity Polity;

    private int _population;

    private float _prominenceValueDelta;

    /// <summary>
    /// Deserialization constructor
    /// </summary>
    public MigratePopulationEvent()
    {
        throw new System.InvalidOperationException("This type is not serializable");
    }

    /// <summary>
    /// constructs a new population migration event
    /// </summary>
    /// <param name="group">the group the migration originates from</param>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="polityId">identifier of the polity whose population will migrate</param>
    /// <param name="triggerDate">the date this event will trigger</param>
    /// <param name="originalSpawnDate">the date this event was initiated</param>
    public MigratePopulationEvent(
        CellGroup group,
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        float prominencePercent,
        Identifier polityId,
        long triggerDate,
        long originalSpawnDate = - 1) : 
        base(group, triggerDate, MigrateGroupEventId, originalSpawnDate: originalSpawnDate)
    {
        Set(targetCell, migrationDirection, migrationType, prominencePercent, polityId);

        DoNotSerialize = true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        Polity = null;
        if (!(PolityId is null))
        {
            PolityProminence prominence = Group.GetPolityProminence(PolityId);

            // the polity might no longer have a prominence in the group
            if (prominence is null)
                return false;

            Polity = prominence.Polity;

            _prominenceValueDelta = ProminencePercent * prominence.Value;
        }
        else
        {
            float prominenceValue = Group.GetUBandsProminenceValue();

            _prominenceValueDelta = ProminencePercent * prominenceValue;
        }

        _population = (int)(Group.Population * _prominenceValueDelta);

        if (_population <= 0)
            return false;

        return true;
    }

    public override void Trigger()
    {
        Group.SetMigratingPopulation(
            TargetCell, MigrationDirection, ProminencePercent, _prominenceValueDelta, _population, Polity);
    }

    public override void Synchronize()
    {
        MigrationDirectionInt = (int)MigrationDirection;
        MigrationTypeInt = (int)MigrationType;
    }

    public override void FinalizeLoad()
    {
        MigrationDirection = (Direction)MigrationDirectionInt;
        MigrationType = (MigrationType)MigrationTypeInt;

        base.FinalizeLoad();

        TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

        Group.PopulationMigrationEvent = this;
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.HasMigrationEvent = false;

            if (MigrationType == MigrationType.Sea)
            {
                Group.ResetSeaMigrationRoute();
            }
        }

        base.DestroyInternal();
    }

    /// <summary>
    /// Resets all properties of the event to be able to reuse the object
    /// </summary>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="polityId">identifier of the polity whose population will migrate</param>
    /// <param name="triggerDate">the date this event will trigger</param>
    public void Reset(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        float prominencePercent,
        Identifier polityId,
        long triggerDate)
    {
        Set(targetCell, migrationDirection, migrationType, prominencePercent, polityId);

        Reset(triggerDate);
    }

    /// <summary>
    /// Sets most properties of this event
    /// </summary>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="polityId">identifier of the polity whose population will migrate</param>
    private void Set(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        float prominencePercent,
        Identifier polityId)
    {
        TargetCell = targetCell;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;

        MigrationDirection = migrationDirection;
        MigrationType = migrationType;

        ProminencePercent = prominencePercent;

        PolityId = polityId;
    }
}
