using System.Collections.Generic;

public class EthnicPresenceCluster
{
    public Ethnicity Ethnicity { get; private set; }

    public List<EthnicPresence> Presences { get; private set; } = new List<EthnicPresence>();

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
        Ethnicity.SetNeedsCensus();
    }

    private void RunCensus()
    {
        if (!_needsCensus)
        {
            return;
        }

        _population = 0;

        foreach (var presence in Presences)
        {
            _population += presence.Population;
        }

        _needsCensus = false;
    }
}
