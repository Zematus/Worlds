using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using BiomePresencePair = System.Collections.Generic.KeyValuePair<string, float>;

public class RegionAttributeConstraint
{
    public enum ConstraintType
    {
        CoastPercentageAbove,
        CoastPercentageBelow,
        RelativeAverageAltitudeAbove,
        RelativeAverageAltitudeBelow,
        AverageAltitudeAbove,
        AverageAltitudeBelow,
        AverageRainfallAbove,
        AverageRainfallBelow,
        AverageTemperatureAbove,
        AverageTemperatureBelow,
        BiomePresenceAbove,
        BiomePresenceBelow
    }

    public ConstraintType Type;
    public object Value;

    public static Regex ConstraintRegex = new Regex(@"^(?<type>[\w_]+):(?<value>.+)$");

    public static RegionAttributeConstraint BuildConstraint(string constraint)
    {
        Match match = ConstraintRegex.Match(constraint);

        if (!match.Success)
            throw new System.ArgumentException("Unparseable constraint: " + constraint);

        string type = match.Groups["type"].Value;
        string valueStr = match.Groups["value"].Value;

        switch (type)
        {
            case "coast_percentage_above":
                float coast_percentage_above = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.CoastPercentageAbove, Value = coast_percentage_above };

            case "coast_percentage_below":
                float coast_percentage_below = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.CoastPercentageBelow, Value = coast_percentage_below };

            case "relative_average_altitude_above":
                float relative_average_altitude_above = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.RelativeAverageAltitudeAbove, Value = relative_average_altitude_above };

            case "relative_average_altitude_below":
                float relative_average_altitude_below = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.RelativeAverageAltitudeBelow, Value = relative_average_altitude_below };

            case "average_altitude_above":
                float average_altitude_above = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageAltitudeAbove, Value = average_altitude_above };

            case "average_altitude_below":
                float average_altitude_below = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageAltitudeBelow, Value = average_altitude_below };

            case "average_rainfall_above":
                float average_rainfall_above = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageRainfallAbove, Value = average_rainfall_above };

            case "average_rainfall_below":
                float average_rainfall_below = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageRainfallBelow, Value = average_rainfall_below };

            case "average_temperature_above":
                float average_temperature_above = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageTemperatureAbove, Value = average_temperature_above };

            case "average_temperature_below":
                float average_temperature_below = float.Parse(valueStr);

                return new RegionAttributeConstraint() { Type = ConstraintType.AverageTemperatureBelow, Value = average_temperature_below };

            case "biome_presence_above":
                string[] valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_above = float.Parse(valueStrs[1]);

                BiomePresencePair biomePresencePair = new BiomePresencePair(valueStrs[0], presence_above);

                return new RegionAttributeConstraint() { Type = ConstraintType.BiomePresenceAbove, Value = biomePresencePair };

            case "biome_presence_below":
                valueStrs = valueStr.Split(new char[] { ',' });

                if (valueStrs.Length != 2)
                    throw new System.ArgumentException("constraint 'biome_presence_above' has invalid number of parameters: " + valueStrs.Length);

                float presence_below = float.Parse(valueStrs[1]);

                biomePresencePair = new BiomePresencePair(valueStrs[0], presence_below);

                return new RegionAttributeConstraint() { Type = ConstraintType.BiomePresenceBelow, Value = biomePresencePair };
        }

        throw new System.Exception("Unhandled constraint type: " + type);
    }

    public bool Validate(Region region)
    {
        switch (Type)
        {
            case ConstraintType.CoastPercentageAbove:
                return region.CoastPercentage >= (float)Value;

            case ConstraintType.CoastPercentageBelow:
                return region.CoastPercentage < (float)Value;

            case ConstraintType.RelativeAverageAltitudeAbove:
                return (region.AverageAltitude - region.AverageOuterBorderAltitude) >= (float)Value;

            case ConstraintType.RelativeAverageAltitudeBelow:
                return (region.AverageAltitude - region.AverageOuterBorderAltitude) < (float)Value;

            case ConstraintType.AverageAltitudeAbove:
                return region.AverageAltitude >= (float)Value;

            case ConstraintType.AverageAltitudeBelow:
                return region.AverageAltitude < (float)Value;

            case ConstraintType.AverageRainfallAbove:
                return region.AverageRainfall >= (float)Value;

            case ConstraintType.AverageRainfallBelow:
                return region.AverageRainfall < (float)Value;

            case ConstraintType.AverageTemperatureAbove:
                return region.AverageTemperature >= (float)Value;

            case ConstraintType.AverageTemperatureBelow:
                return region.AverageTemperature < (float)Value;

            case ConstraintType.BiomePresenceAbove:
                BiomePresencePair biomePresencePair = (BiomePresencePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) >= biomePresencePair.Value;

            case ConstraintType.BiomePresenceBelow:
                biomePresencePair = (BiomePresencePair)Value;

                return region.GetBiomePresence(biomePresencePair.Key) < biomePresencePair.Value;
        }

        throw new System.Exception("Unhandled constraint type: " + Type);
    }
}
