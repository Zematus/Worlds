using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class ElementConstraint
{
    public string Type;
    public object Value;

    public static Regex ConstraintRegex = new Regex(@"^(?<type>[\w_]+):(?<value>.+)$");

    private ElementConstraint(string type, object value)
    {
        Type = type;
        Value = value;
    }

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

                return new ElementConstraint(type, altitude_above);

            case "altitude_below":
                float altitude_below = float.Parse(valueStr);

                return new ElementConstraint(type, altitude_below);

            case "rainfall_above":
                float rainfall_above = float.Parse(valueStr);

                return new ElementConstraint(type, rainfall_above);

            case "rainfall_below":
                float rainfall_below = float.Parse(valueStr);

                return new ElementConstraint(type, rainfall_below);

            case "temperature_above":
                float temperature_above = float.Parse(valueStr);

                return new ElementConstraint(type, temperature_above);

            case "temperature_below":
                float temperature_below = float.Parse(valueStr);

                return new ElementConstraint(type, temperature_below);

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

                return new ElementConstraint(type, attributes);

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

                return new ElementConstraint(type, attributes);

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

                return new ElementConstraint(type, biomes);

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

                return new ElementConstraint(type, biomes);
        }

        throw new System.Exception("Unhandled constraint type: " + type);
    }

    public bool Validate(Region region)
    {
        switch (Type)
        {
            case "altitude_above":
                return region.AverageAltitude >= (float)Value;

            case "altitude_below":
                return region.AverageAltitude < (float)Value;

            case "rainfall_above":
                return region.AverageRainfall >= (float)Value;

            case "rainfall_below":
                return region.AverageRainfall < (float)Value;

            case "temperature_above":
                return region.AverageTemperature >= (float)Value;

            case "temperature_below":
                return region.AverageTemperature < (float)Value;

            case "no_attribute":
                return !((RegionAttribute[])Value).Any(a => region.Attributes.Contains(a));

            case "any_attribute":
                return ((RegionAttribute[])Value).Any(a => region.Attributes.Contains(a));

            case "any_biome":
                return ((Biome[])Value).Any(b => region.PresentBiomeIds.Contains(b.Id));

            case "main_biome":
                return ((Biome[])Value).Any(b => region.BiomeWithMostPresence == b.Id);
        }

        throw new System.Exception("Unhandled constraint type: " + Type);
    }
}
