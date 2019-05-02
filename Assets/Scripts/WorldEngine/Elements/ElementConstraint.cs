using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class ElementConstraint
{
    public enum ConstraintType
    {
        AltitudeAbove,
        AltitudeBelow,
        RainfallAbove,
        RainfallBelow,
        TemperatureAbove,
        TemperatureBelow,
        NoAttribute,
        AnyAttribute,
        AnyBiome,
        MainBiome
    }

    public ConstraintType Type;
    public object Value;

    public static Regex ConstraintRegex = new Regex(@"^(?<type>[\w_]+):(?<value>.+)$");

    public static ElementConstraint BuildConstraint(string constraint)
    {
        Match match = ConstraintRegex.Match(constraint);

        if (!match.Success)
            throw new System.Exception("Unparseable constraint: " + constraint);

        string type = match.Groups["type"].Value;
        string valueStr = match.Groups["value"].Value;

        switch (type)
        {
            case "altitude_above":
                float altitude_above = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.AltitudeAbove, Value = altitude_above };

            case "altitude_below":
                float altitude_below = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.AltitudeBelow, Value = altitude_below };

            case "rainfall_above":
                float rainfall_above = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.RainfallAbove, Value = rainfall_above };

            case "rainfall_below":
                float rainfall_below = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.RainfallBelow, Value = rainfall_below };

            case "temperature_above":
                float temperature_above = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.TemperatureAbove, Value = temperature_above };

            case "temperature_below":
                float temperature_below = float.Parse(valueStr);

                return new ElementConstraint() { Type = ConstraintType.TemperatureBelow, Value = temperature_below };

            case "no_attribute":
                string[] attributeStrs = valueStr.Split(new char[] { ',' });

                RegionAttribute[] attributes = attributeStrs.Select(s =>
                {
                    if (!RegionAttribute.Attributes.ContainsKey(s))
                    {
                        throw new System.Exception("Attribute not present: " + s);
                    }

                    return RegionAttribute.Attributes[s];
                }).ToArray();

                return new ElementConstraint() { Type = ConstraintType.NoAttribute, Value = attributes };

            case "any_attribute":
                attributeStrs = valueStr.Split(new char[] { ',' });

                attributes = attributeStrs.Select(s =>
                {
                    if (!RegionAttribute.Attributes.ContainsKey(s))
                    {

                        throw new System.Exception("Attribute not present: " + s);
                    }

                    return RegionAttribute.Attributes[s];
                }).ToArray();

                return new ElementConstraint() { Type = ConstraintType.AnyAttribute, Value = attributes };

            case "any_biome":
                string[] biomeStrs = valueStr.Split(new char[] { ',' });

                Biome[] biomes = biomeStrs.Select(s =>
                {
                    if (!Biome.Biomes.ContainsKey(s))
                    {
                        throw new System.Exception("Biome not present: " + s);
                    }

                    return Biome.Biomes[s];
                }).ToArray();

                return new ElementConstraint() { Type = ConstraintType.AnyBiome, Value = biomes };

            case "main_biome":
                biomeStrs = valueStr.Split(new char[] { ',' });

                biomes = biomeStrs.Select(s =>
                {
                    if (!Biome.Biomes.ContainsKey(s))
                    {
                        throw new System.Exception("Biome not present: " + s);
                    }

                    return Biome.Biomes[s];
                }).ToArray();

                return new ElementConstraint() { Type = ConstraintType.MainBiome, Value = biomes };
        }

        throw new System.Exception("Unhandled constraint type: " + type);
    }

    private bool IsAnyAttribute(Region region, RegionAttribute[] attributes)
    {
        foreach (RegionAttribute a in attributes)
        {
            if (region.Attributes.Contains(a)) return true;
        }

        return false;
    }

    private bool IsAnyBiome(Region region, Biome[] biomes)
    {
        foreach (Biome b in biomes)
        {
            if (region.PresentBiomeIds.Contains(b.Id)) return true;
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

    public bool Validate(Region region)
    {
        switch (Type)
        {
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

            case ConstraintType.NoAttribute:
                return !IsAnyAttribute(region, (RegionAttribute[])Value);

            case ConstraintType.AnyAttribute:
                return IsAnyAttribute(region, (RegionAttribute[])Value);

            case ConstraintType.AnyBiome:
                return IsAnyBiome(region, (Biome[])Value);

            case ConstraintType.MainBiome:
                return IsMainBiome(region, (Biome[])Value);
        }

        throw new System.Exception("Unhandled constraint type: " + Type);
    }
}
