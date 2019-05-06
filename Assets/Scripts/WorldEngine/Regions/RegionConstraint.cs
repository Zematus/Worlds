using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using BiomePresencePair = System.Collections.Generic.KeyValuePair<string, float>;

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
        BiomePresenceAbove,
        BiomePresenceBelow,
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
                float coast_percentage_above = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.CoastPercentageAbove, Value = coast_percentage_above };

            case "coast_percentage_below":
                float coast_percentage_below = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.CoastPercentageBelow, Value = coast_percentage_below };

            case "relative_altitude_above":
                float relative_altitude_above = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.RelativeAltitudeAbove, Value = relative_altitude_above };

            case "relative_altitude_below":
                float relative_altitude_below = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.RelativeAltitudeBelow, Value = relative_altitude_below };

            case "altitude_above":
                float altitude_above = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.AltitudeAbove, Value = altitude_above };

            case "altitude_below":
                float altitude_below = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.AltitudeBelow, Value = altitude_below };

            case "rainfall_above":
                float rainfall_above = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.RainfallAbove, Value = rainfall_above };

            case "rainfall_below":
                float rainfall_below = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.RainfallBelow, Value = rainfall_below };

            case "temperature_above":
                float temperature_above = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.TemperatureAbove, Value = temperature_above };

            case "temperature_below":
                float temperature_below = float.Parse(valueStr);

                return new RegionConstraint() { Type = ConstraintType.TemperatureBelow, Value = temperature_below };

            case "biome_presence_above":
                string[] valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_above = float.Parse(valueStrs[1]);

                BiomePresencePair biomePresencePair = new BiomePresencePair(valueStrs[0], presence_above);

                return new RegionConstraint() { Type = ConstraintType.BiomePresenceAbove, Value = biomePresencePair };

            case "biome_presence_below":
                valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_below = float.Parse(valueStrs[1]);

                biomePresencePair = new BiomePresencePair(valueStrs[0], presence_below);

                return new RegionConstraint() { Type = ConstraintType.BiomePresenceBelow, Value = biomePresencePair };

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

        foreach (RegionAttribute a in region.Attributes.Values)
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

            case ConstraintType.BiomePresenceAbove:
                BiomePresencePair biomePresencePair = (BiomePresencePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) >= biomePresencePair.Value;

            case ConstraintType.BiomePresenceBelow:
                biomePresencePair = (BiomePresencePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) < biomePresencePair.Value;

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
