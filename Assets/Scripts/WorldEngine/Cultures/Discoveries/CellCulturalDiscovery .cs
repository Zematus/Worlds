using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class CellCulturalDiscovery : CulturalDiscovery
{
    public CellCulturalDiscovery(string id, string name) : base(id, name)
    {

    }

    public static CellCulturalDiscovery CreateCellInstance(string id)
    {
        switch (id)
        {
            case BoatMakingDiscovery.DiscoveryId:
                return new BoatMakingDiscovery();

            case SailingDiscovery.DiscoveryId:
                return new SailingDiscovery();

            case PlantCultivationDiscovery.DiscoveryId:
                return new PlantCultivationDiscovery();

            case TribalismDiscovery.DiscoveryId:
                return new TribalismDiscovery();
        }

        throw new System.Exception("Unexpected CulturalDiscovery type: " + id);
    }

    public abstract bool CanBeHeld(CellGroup group);

    public virtual void LossConsequences(CellGroup group)
    {

    }

    public virtual void GainConsequences(CellGroup group)
    {

    }
}
