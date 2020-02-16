using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellEntity : Entity
{
    public const string BiomeTraitPresenceAttributeId = "biome_trait_presence";

    public TerrainCell Cell;

    protected override object _reference => Cell;

    private class BiomeTraitPresenceAttribute : NumericEntityAttribute
    {
        private CellEntity _cellEntity;

        private IStringExpression _argument;

        public BiomeTraitPresenceAttribute(CellEntity cellEntity, IExpression[] arguments)
            : base(BiomeTraitPresenceAttributeId, cellEntity)
        {
            _cellEntity = cellEntity;

            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argument = ExpressionBuilder.ValidateStringExpression(arguments[0]);
        }

        public override float Value => _cellEntity.Cell.GetBiomeTraitPresence(_argument.Value);
    }

    public CellEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case BiomeTraitPresenceAttributeId:
                return new BiomeTraitPresenceAttribute(this, arguments);
        }

        throw new System.ArgumentException("Cell: Unable to find attribute: " + attributeId);
    }

    public void Set(TerrainCell cell)
    {
        Cell = cell;
    }
}
