using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellEntity : Entity
{
    public TerrainCell Cell;

    public class BiomeTraitPresenceAttribute : NumericEntityAttribute
    {
        private CellEntity _cellEntity;

        private FixedStringExpression _argument;

        public BiomeTraitPresenceAttribute(CellEntity cellEntity, Expression[] arguments)
        {
            _cellEntity = cellEntity;

            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argument = FixedStringExpression.ValidateExpression(arguments[0]);
        }

        public override float GetValue()
        {
            return _cellEntity.Cell.GetBiomeTraitPresence(_argument.Value);
        }
    }

    public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
    {
        switch (attributeId)
        {
            case "biome_trait_presence":
                return new BiomeTraitPresenceAttribute(this, arguments);
        }

        throw new System.ArgumentException("Cell: Unable to find attribute: " + attributeId);
    }

    public void Set(TerrainCell cell)
    {
        Cell = cell;
    }
}
