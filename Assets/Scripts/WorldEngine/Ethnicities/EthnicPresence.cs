
public class EthnicPresence
{
    public EthnicPresenceCluster Cluster { get; private set; }

    public CellGroup Group { get; private set; }

    public float Percentage { get; private set; }

    public int Population => (int)(Percentage * Group.ExactPopulation);

    public void SetNeedsCensus()
    {
        Cluster.SetNeedsCensus();
    }
}
