using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellLayerValueCondition : CellCondition
{
    public const string Regex = @"^\s*cell_layer_value\s*" +
        @":\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    private string _layerId;

    public float MinValue;

    public CellLayerValueCondition(Match match)
    {
        _layerId = match.Groups["id"].Value;

        Layer layer;

        if (!Layer.Layers.TryGetValue(_layerId, out layer))
        {
            throw new System.ArgumentException("CellLayerValueCondition: Unable to find layer with id: " + _layerId);
        }
        
        string valueStr = match.Groups["value"].Value;

        if (!float.TryParse(valueStr, out MinValue))
        {
            throw new System.ArgumentException("CellLayerValueCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MinValue.IsInsideRange(0, layer.MaxPossibleValue))
        {
            throw new System.ArgumentException("CellLayerValueCondition: Min value is outside the range of 0 and " + layer.MaxPossibleValue + ": " + valueStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.GetLayerValue(_layerId) >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Layer Value' Condition, Layer Id: " + _layerId + ", Min Value: " + MinValue;
    }
}
