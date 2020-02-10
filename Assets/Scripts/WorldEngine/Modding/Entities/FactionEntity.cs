using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionEntity : Entity
{
    public Faction Faction;

    private TypeAttribute _typeAttribute;

    private AdministrativeLoadAttribute _administrativeLoadAttribute;

    public class TypeAttribute : StringEntityAttribute
    {
        private FactionEntity _factionEntity;

        public TypeAttribute(FactionEntity factionEntity)
        {
            _factionEntity = factionEntity;
        }

        public override string GetValue()
        {
            return _factionEntity.Faction.Type;
        }
    }

    public class AdministrativeLoadAttribute : NumericEntityAttribute
    {
        private FactionEntity _factionEntity;

        public AdministrativeLoadAttribute(FactionEntity factionEntity)
        {
            _factionEntity = factionEntity;
        }

        public override float GetValue()
        {
            return _factionEntity.Faction.AdministrativeLoad;
        }
    }

    public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
    {
        switch (attributeId)
        {
            case "type":
                _typeAttribute =
                    _typeAttribute ?? new TypeAttribute(this);
                return _typeAttribute;

            case "administrative_load":
                _administrativeLoadAttribute =
                    _administrativeLoadAttribute ?? new AdministrativeLoadAttribute(this);
                return _administrativeLoadAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Faction faction)
    {
        Faction = faction;
    }
}
