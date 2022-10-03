using System.Collections.Generic;

public class Ethnicity
{
    public List<EthnicPresenceCluster> Clusters { get; private set; } = new List<EthnicPresenceCluster>();

    public int Population 
    {
        get 
        {
            RunCensus();

            return _population;
        }
    }

    private int _population = 0;

    private bool _needsCensus = true;

    public void SetNeedsCensus()
    {
        _needsCensus = true;
    }

    private void RunCensus()
    {
        if (!_needsCensus)
        {
            return;
        }

        _population = 0;

        foreach (var cluster in Clusters)
        {
            _population += cluster.Population;
        }

        _needsCensus = false;
    }
}
