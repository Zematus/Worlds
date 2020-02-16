using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionEntity : Entity
{
    public const string TypeAttributeId = "type";
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string PreferencesAttributeId = "preferences";

    public Faction Faction;

    private TypeAttribute _typeAttribute;

    private AdministrativeLoadAttribute _administrativeLoadAttribute;

    private CulturalPreferencesEntity _preferencesEntity =
        new CulturalPreferencesEntity(PreferencesAttributeId);
    private EntityAttribute _preferencesAttribute;

    protected override object _reference => Faction;

    public class TypeAttribute : StringEntityAttribute
    {
        private FactionEntity _factionEntity;

        public TypeAttribute(FactionEntity factionEntity)
            : base(TypeAttributeId, factionEntity)
        {
            _factionEntity = factionEntity;
        }

        public override string Value => _factionEntity.Faction.Type;
    }

    public class AdministrativeLoadAttribute : NumericEntityAttribute
    {
        private FactionEntity _factionEntity;

        public AdministrativeLoadAttribute(FactionEntity factionEntity)
            : base(AdministrativeLoadAttributeId, factionEntity)
        {
            _factionEntity = factionEntity;
        }

        public override float Value => _factionEntity.Faction.AdministrativeLoad;
    }

    public FactionEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new TypeAttribute(this);
                return _typeAttribute;

            case AdministrativeLoadAttributeId:
                _administrativeLoadAttribute =
                    _administrativeLoadAttribute ?? new AdministrativeLoadAttribute(this);
                return _administrativeLoadAttribute;

            case PreferencesAttributeId:
                _preferencesAttribute =
                    _preferencesAttribute ??
                    new FixedEntityEntityAttribute(_preferencesEntity, PreferencesAttributeId, this);
                return _preferencesAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Faction faction)
    {
        Faction = faction;

        _preferencesEntity.Set(faction.Culture);
    }
}
