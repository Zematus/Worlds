public interface IFlagHolder
{
    void SetFlag(string flag);
    void UnsetFlag(string flag);

    bool IsFlagSet(string flag);
}
