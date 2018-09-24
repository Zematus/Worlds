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

    public static CellCulturalDiscovery CreateCellInstance(CulturalDiscovery baseDiscovery)
    {
        if (BoatMakingDiscovery.IsBoatMakingDiscovery(baseDiscovery))
        {
            return new BoatMakingDiscovery();
        }

        if (SailingDiscovery.IsSailingDiscovery(baseDiscovery))
        {
            return new SailingDiscovery();
        }

        if (PlantCultivationDiscovery.IsPlantCultivationDiscovery(baseDiscovery))
        {
            return new PlantCultivationDiscovery();
        }

        if (TribalismDiscovery.IsTribalismDiscovery(baseDiscovery))
        {
            return new TribalismDiscovery();
        }

        throw new System.Exception("Unexpected CulturalDiscovery type: " + baseDiscovery.Id);
    }

    public abstract bool CanBeHeld(CellGroup group);

    public virtual void LossConsequences(CellGroup group)
    {

    }

    public virtual void GainConsequences(CellGroup group)
    {

    }
}
