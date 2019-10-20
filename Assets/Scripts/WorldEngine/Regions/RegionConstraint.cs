using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using IdentifierValuePair = System.Collections.Generic.KeyValuePair<string, float>;

public class RegionConstraint
{
    public enum ConstraintType
    {
        CoastPercentageAbove,
        CoastPercentageBelow,
        RelativeAltitudeAbove,
        RelativeAltitudeBelow,
        AltitudeAbove,
        AltitudeBelow,
        RainfallAbove,
        RainfallBelow,
        TemperatureAbove,
        TemperatureBelow,
        FlowingWaterAbove,
        FlowingWaterBelow,
        BiomePresenceAbove,
        BiomePresenceBelow,
        LayerValueAbove,
        LayerValueBelow,
        NoAttribute,
        AnyAttribute,
        ZeroPrimaryAttributes,
        MainBiome
    }

    public ConstraintType Type;
    public object Value;

    public static Regex ConstraintRegex = new Regex(@"^(?<type>[\w_]+)(?::(?<value>.+))?$");

    public static RegionConstraint BuildConstraint(string constraint)
    {
        Match match = ConstraintRegex.Match(constraint);

        if (!match.Success)
            throw new System.ArgumentException("Unparseable constraint: " + constraint);

        string type = match.Groups["type"].Value;
        string valueStr = null;

        if (match.Groups["value"] != null)
        {
            valueStr = match.Groups["value"].Value;
        }

        switch (type)
        {
            case "coast_percentage_above":
                float coast_percentage_above;
                if (!float.TryParse(valueStr, out coast_percentage_above))
                    throw new System.ArgumentException("Invalid constraint 'coast_percentage_above' value: " + coast_percentage_above);

                if (!coast_percentage_above.IsInsideRange(0, 1))
                    throw new System.ArgumentException("'coast_percentage_above' must be a value between 0 and 1 (inclusive)");

                return new RegionConstraint() { Type = ConstraintType.CoastPercentageAbove, Value = coast_percentage_above };

            case "coast_percentage_below":
                float coast_percentage_below;
                if (!float.TryParse(valueStr, out coast_percentage_below))
                    throw new System.ArgumentException("Invalid constraint 'coast_percentage_above' value: " + coast_percentage_below);

                if (!coast_percentage_below.IsInsideRange(0, 1))
                    throw new System.ArgumentException("'coast_percentage_below' must be a value between 0 and 1 (inclusive)");

                return new RegionConstraint() { Type = ConstraintType.CoastPercentageBelow, Value = coast_percentage_below };

            case "relative_altitude_above":
                float relative_altitude_above;
                if (!float.TryParse(valueStr, out relative_altitude_above))
                    throw new System.ArgumentException("Invalid constraint 'relative_altitude_above' value: " + relative_altitude_above);

                return new RegionConstraint() { Type = ConstraintType.RelativeAltitudeAbove, Value = relative_altitude_above };

            case "relative_altitude_below":
                float relative_altitude_below;
                if (!float.TryParse(valueStr, out relative_altitude_below))
                    throw new System.ArgumentException("Invalid constraint 'relative_altitude_below' value: " + relative_altitude_below);

                return new RegionConstraint() { Type = ConstraintType.RelativeAltitudeBelow, Value = relative_altitude_below };

            case "altitude_above":
                float altitude_above;
                if (!float.TryParse(valueStr, out altitude_above))
                    throw new System.ArgumentException("Invalid constraint 'altitude_above' value: " + altitude_above);

                return new RegionConstraint() { Type = ConstraintType.AltitudeAbove, Value = altitude_above };

            case "altitude_below":
                float altitude_below;
                if (!float.TryParse(valueStr, out altitude_below))
                    throw new System.ArgumentException("Invalid constraint 'altitude_below' value: " + altitude_below);

                return new RegionConstraint() { Type = ConstraintType.AltitudeBelow, Value = altitude_below };

            case "rainfall_above":
                float rainfall_above;
                if (!float.TryParse(valueStr, out rainfall_above))
                    throw new System.ArgumentException("Invalid constraint 'rainfall_above' value: " + rainfall_above);

                return new RegionConstraint() { Type = ConstraintType.RainfallAbove, Value = rainfall_above };

            case "rainfall_below":
                float rainfall_below;
                if (!float.TryParse(valueStr, out rainfall_below))
                    throw new System.ArgumentException("Invalid constraint 'rainfall_below' value: " + rainfall_below);

                return new RegionConstraint() { Type = ConstraintType.RainfallBelow, Value = rainfall_below };

            case "temperature_above":
                float temperature_above;
                if (!float.TryParse(valueStr, out temperature_above))
                    throw new System.ArgumentException("Invalid constraint 'temperature_above' value: " + temperature_above);

                return new RegionConstraint() { Type = ConstraintType.TemperatureAbove, Value = temperature_above };

            case "temperature_below":
                float temperature_below;
                if (!float.TryParse(valueStr, out temperature_below))
                    throw new System.ArgumentException("Invalid constraint 'temperature_below' value: " + temperature_below);

                return new RegionConstraint() { Type = ConstraintType.TemperatureBelow, Value = temperature_below };

            case "flowing_water_above":
                float flowing_water_above;
                if (!float.TryParse(valueStr, out flowing_water_above))
                    throw new System.ArgumentException("Invalid constraint 'flowing_water_above' value: " + flowing_water_above);

                return new RegionConstraint() { Type = ConstraintType.FlowingWaterAbove, Value = flowing_water_above };

            case "flowing_water_below":
                float flowing_water_below;
                if (!float.TryParse(valueStr, out flowing_water_below))
                    throw new System.ArgumentException("Invalid constraint 'flowing_water_below' value: " + flowing_water_below);

                return new RegionConstraint() { Type = ConstraintType.FlowingWaterBelow, Value = flowing_water_below };

            case "biome_presence_above":
                string[] valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_above;
                if (!float.TryParse(valueStrs[1], out presence_above))
                    throw new System.ArgumentException("Invalid constraint 'biome_presence_above' second input value: " + presence_above);

                if (!presence_above.IsInsideRange(0, 1))
                    throw new System.ArgumentException("'biome_presence_above' second input must be a value between 0 and 1 (inclusive):" + presence_above);

                IdentifierValuePair biomePresencePair = new IdentifierValuePair(valueStrs[0], presence_above);

                return new RegionConstraint() { Type = ConstraintType.BiomePresenceAbove, Value = biomePresencePair };

            case "biome_presence_below":
                valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_below;
                if (!float.TryParse(valueStrs[1], out presence_below))
                    throw new System.ArgumentException("Invalid constraint 'biome_presence_below' second input value: " + presence_below);

                if (!presence_below.IsInsideRange(0, 1))
                    throw new System.ArgumentException("'biome_presence_below' second input must be a value between 0 and 1 (inclusive):" + presence_below);

                biomePresencePair = new IdentifierValuePair(valueStrs[0], presence_below);

                return new RegionConstraint() { Type = ConstraintType.BiomePresenceBelow, Value = biomePresencePair };

            case "layer_value_above":
                valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'layer_value_above' has invalid number of parameters: " + valueStrs.Length);

                float value_above;
                if (!float.TryParse(valueStrs[1], out value_above))
                    throw new System.ArgumentException("Invalid constraint 'layer_value_above' second input value: " + value_above);

                IdentifierValuePair layerValuePair = new IdentifierValuePair(valueStrs[0], value_above);

                return new RegionConstraint() { Type = ConstraintType.LayerValueAbove, Value = layerValuePair };

            case "layer_value_below":
                valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'layer_value_below' has invalid number of parameters: " + valueStrs.Length);

                float value_below;
                if (!float.TryParse(valueStrs[1], out value_below))
                    throw new System.ArgumentException("Invalid constraint 'layer_value_below' second input value: " + value_below);

                layerValuePair = new IdentifierValuePair(valueStrs[0], value_below);

                return new RegionConstraint() { Type = ConstraintType.LayerValueBelow, Value = layerValuePair };

            case "no_attribute":
                string[] attributeStrs = valueStr.Split(new char[] { ',' });

                for (int i = 0; i < attributeStrs.Length; i++)
                {
                    attributeStrs[i] = attributeStrs[i].Trim();
                }

                return new RegionConstraint() { Type = ConstraintType.NoAttribute, Value = attributeStrs };

            case "any_attribute":
                attributeStrs = valueStr.Split(new char[] { ',' });

                for (int i = 0; i < attributeStrs.Length; i++)
                {
                    attributeStrs[i] = attributeStrs[i].Trim();
                }

                return new RegionConstraint() { Type = ConstraintType.AnyAttribute, Value = attributeStrs };

            case "main_biome":
                string[] biomeStrs = valueStr.Split(new char[] { ',' });

                for (int i = 0; i < biomeStrs.Length; i++)
                {
                    biomeStrs[i] = biomeStrs[i].Trim();
                }

                Biome[] biomes = biomeStrs.Select(s =>
                {
                    if (!Biome.Biomes.ContainsKey(s))
                    {
                        throw new System.Exception("Biome not present: " + s);
                    }

                    return Biome.Biomes[s];
                }).ToArray();

                return new RegionConstraint() { Type = ConstraintType.MainBiome, Value = biomes };

            case "zero_primary_attributes":
                return new RegionConstraint() { Type = ConstraintType.ZeroPrimaryAttributes, Value = null };
        }

        throw new System.Exception("Unhandled constraint type: " + type);
    }

    private bool IsAnyAttribute(Region region, string[] attributeStrs)
    {
        foreach (string str in attributeStrs)
        {
            if (region.Attributes.ContainsKey(str)) return true;
        }

        return false;
    }

    private bool IsMainBiome(Region region, Biome[] biomes)
    {
        foreach (Biome b in biomes)
        {
            if (region.BiomeWithMostPresence == b.Id) return true;
        }

        return false;
    }

    private int GetPrimaryAttributeCount(Region region)
    {
        int count = 0;

        foreach (RegionAttribute.Instance a in region.Attributes.Values)
        {
            if (!a.Secondary) count++;
        }

        return count;
    }

    public bool Validate(Region region)
    {
        switch (Type)
        {
            case ConstraintType.CoastPercentageAbove:
                return region.CoastPercentage >= (float)Value;

            case ConstraintType.CoastPercentageBelow:
                return region.CoastPercentage < (float)Value;

            case ConstraintType.RelativeAltitudeAbove:
                return (region.AverageAltitude - region.AverageOuterBorderAltitude) >= (float)Value;

            case ConstraintType.RelativeAltitudeBelow:
                return (region.AverageAltitude - region.AverageOuterBorderAltitude) < (float)Value;

            case ConstraintType.AltitudeAbove:
                return region.AverageAltitude >= (float)Value;

            case ConstraintType.AltitudeBelow:
                return region.AverageAltitude < (float)Value;

            case ConstraintType.RainfallAbove:
                return region.AverageRainfall >= (float)Value;

            case ConstraintType.RainfallBelow:
                return region.AverageRainfall < (float)Value;

            case ConstraintType.TemperatureAbove:
                return region.AverageTemperature >= (float)Value;

            case ConstraintType.TemperatureBelow:
                return region.AverageTemperature < (float)Value;

            case ConstraintType.FlowingWaterAbove:
                return region.AverageFlowingWater >= (float)Value;

            case ConstraintType.FlowingWaterBelow:
                return region.AverageFlowingWater < (float)Value;

            case ConstraintType.BiomePresenceAbove:
                IdentifierValuePair biomePresencePair = (IdentifierValuePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) >= biomePresencePair.Value;

            case ConstraintType.BiomePresenceBelow:
                biomePresencePair = (IdentifierValuePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) < biomePresencePair.Value;

            case ConstraintType.LayerValueAbove:
                IdentifierValuePair layerValuePair = (IdentifierValuePair)Value;

                return region.GetBiomePresence(layerValuePair.Key) >= layerValuePair.Value;

            case ConstraintType.LayerValueBelow:
                layerValuePair = (IdentifierValuePair)Value;

                return region.GetBiomePresence(layerValuePair.Key) < layerValuePair.Value;

            case ConstraintType.NoAttribute:
                return !IsAnyAttribute(region, (string[])Value);

            case ConstraintType.AnyAttribute:
                return IsAnyAttribute(region, (string[])Value);

            case ConstraintType.MainBiome:
                return IsMainBiome(region, (Biome[])Value);

            case ConstraintType.ZeroPrimaryAttributes:
                return GetPrimaryAttributeCount(region) == 0;
        }

        throw new System.Exception("Unhandled constraint type: " + Type);
    }
}
