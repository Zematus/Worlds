using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellEntity : DelayedSetEntity<TerrainCell>
{
    public const string BiomeTraitPresenceAttributeId = "biome_trait_presence";
    public const string BiomeTypePresenceAttributeId = "biome_type_presence";
    public const string NeighborsAttributeId = "neighbors";

    public virtual TerrainCell Cell
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Cell;

    private CellCollectionEntity _neighborsEntity = null;

    private class BiomeTraitPresenceAttribute : ValueEntityAttribute<float>
    {
        private CellEntity _cellEntity;

        private readonly IValueExpression<string> _argument;

        public BiomeTraitPresenceAttribute(CellEntity cellEntity, IExpression[] arguments)
            : base(BiomeTraitPresenceAttributeId, cellEntity, arguments, 1)
        {
            _cellEntity = cellEntity;
            _argument = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);
        }

        public override float Value => _cellEntity.Cell.GetBiomeTraitPresence(_argument.Value);
    }

    private class BiomeTypePresenceAttribute : ValueEntityAttribute<float>
    {
        private CellEntity _cellEntity;

        private readonly IValueExpression<string> _argument;
        private readonly bool _isFixed;
        private readonly BiomeTerrainType _fixedType;

        public BiomeTypePresenceAttribute(CellEntity cellEntity, IExpression[] arguments)
            : base(BiomeTypePresenceAttributeId, cellEntity, arguments, 1)
        {
            _cellEntity = cellEntity;
            _argument = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

            _isFixed = _argument is FixedStringValueExpression;

            if (_isFixed && !Biome.TryParseBiomeType(_argument.Value, out _fixedType))
            {
                throw new System.Exception($"'{_argument.Value}' is not a valid biome type.");
            }
        }

        public override float Value
        {
            get {
                BiomeTerrainType biomeType;

                if (_isFixed)
                {
                    biomeType = _fixedType;
                }
                else if (!Biome.TryParseBiomeType(_argument.Value, out biomeType))
                {
                    throw new System.Exception($"'{_argument.Value}' is not a valid biome type.");
                }

                return _cellEntity.Cell.GetBiomeTypePresence(biomeType);
            }
        }
    }

    public CellEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CellEntity(
        ValueGetterMethod<TerrainCell> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public CellEntity(
        TryRequestGenMethod<TerrainCell> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public ICollection<TerrainCell> GetNeighbors() => Cell.NeighborList;

    public EntityAttribute GetNeighborsAttribute()
    {
        _neighborsEntity =
            _neighborsEntity ?? new CellCollectionEntity(
            GetNeighbors,
            Context,
            BuildAttributeId(NeighborsAttributeId),
            this);

        return _neighborsEntity.GetThisEntityAttribute();
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case BiomeTraitPresenceAttributeId:
                return new BiomeTraitPresenceAttribute(this, arguments);
            case BiomeTypePresenceAttributeId:
                return new BiomeTypePresenceAttribute(this, arguments);
            case NeighborsAttributeId:
                return GetNeighborsAttribute();
        }

        return base.GetAttribute(attributeId, arguments);
    }

    public override string GetDebugString()
    {
        return $"cell:{Cell.Position}";
    }

    public override string GetFormattedString()
    {
        return Cell.Position.ToBoldString();
    }

    protected override void ResetInternal()
    {
        base.ResetInternal();

        if (_isReset) return;

        _neighborsEntity?.Reset();
    }
}
