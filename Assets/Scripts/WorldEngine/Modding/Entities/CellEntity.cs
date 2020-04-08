using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellEntity : Entity
{
    public const string BiomeTraitPresenceAttributeId = "biome_trait_presence";

    public TerrainCell Cell;

    protected override object _reference => Cell;

    private class BiomeTraitPresenceAttribute : ValueEntityAttribute<float>
    {
        private CellEntity _cellEntity;

        private readonly IValueExpression<string> _argument;

        public BiomeTraitPresenceAttribute(CellEntity cellEntity, IExpression[] arguments)
            : base(BiomeTraitPresenceAttributeId, cellEntity, arguments)
        {
            _cellEntity = cellEntity;

            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argument = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);
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

    public override string GetFormattedString()
    {
        return Cell.Position.ToString();
    }

    public void Set(TerrainCell c) => Cell = c;

    public override void Set(object o)
    {
        if (o is CellEntity e)
        {
            Set(e.Cell);
        }
        else if (o is TerrainCell c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
