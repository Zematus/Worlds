public interface ICellGroupEventGenerator : IWorldEventGenerator
{
    bool CanAssignEventTypeToGroup(CellGroup group);
    CellGroupEvent GenerateAndAssignEvent(CellGroup group);

    string GetEventSetFlag();
}
