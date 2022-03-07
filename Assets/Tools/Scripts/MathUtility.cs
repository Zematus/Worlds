using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathUtility
{
    public const int FloatToIntScalingFactor = 100;
    public const float IntToFloatScalingFactor = 1f / FloatToIntScalingFactor;

    public const float NormalAt0 = 0.398942f;
    public const float NormalAt1 = 0.241971f;
    public const float NormalAt2 = 0.053991f;
    public const float NormalAt3 = 0.004432f;
    public const float NormalAt4 = 0.000134f;
    public const float NormalAt5 = 0.000001f;

    /// <summary>
    /// Parses a float number using a culture invariant format.
    /// </summary>
    /// <param name="s">
    /// The float number string to parse.
    /// </param>
    /// <param name="result">
    /// (out paramenter) The float where to copy the parsed value.
    /// </param>
    /// <returns>
    ///   <c>true</c> if the string could be parsed into a float value correctly. Otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseCultureInvariant(string s, out float result)
    {
        return float.TryParse(
            s, 
            System.Globalization.NumberStyles.Float, 
            System.Globalization.CultureInfo.InvariantCulture, 
            out result);
    }

    /// <summary>
    /// Parses an integer number using a culture invariant format.
    /// </summary>
    /// <param name="s">
    /// The integer number string to parse.
    /// </param>
    /// <param name="result">
    /// (out paramenter) The integer where to copy the parsed value.
    /// </param>
    /// <returns>
    ///   <c>true</c> if the string could be parsed into a integer value correctly. Otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseCultureInvariant(string s, out int result)
    {
        return int.TryParse(
            s,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out result);
    }

    public static bool IsInsideRange(this float value, float minValue, float maxValue)
    {
        return (value >= minValue) && (value <= maxValue);
    }

    public static bool IsInsideRange(this int value, int minValue, int maxValue)
    {
        return (value >= minValue) && (value <= maxValue);
    }

    public static bool IsInsideRange(this long value, long minValue, long maxValue)
    {
        return (value >= minValue) && (value <= maxValue);
    }

    public static int ProtectedAbs(int x)
    {
        if (x == int.MinValue)
        {
            return int.MaxValue;
        }

        return Mathf.Abs(x);
    }

    public static float GetMagnitude(float c1, float c2)
    {
        return Mathf.Sqrt((c1 * c1) + (c2 * c2));
    }

    public static float GetComponent(float m, float c)
    {
        return Mathf.Sqrt((m * m) - (c * c));
    }

    public static Vector3 GetCartesianCoordinates(float alpha, float beta, float radius)
    {
        if ((alpha < 0) || (alpha > Mathf.PI)) throw new System.Exception("alpha value must be not less than 0 and not greater than Mathf.PI");

        while (beta < 0) beta += Mathf.PI;

        beta = Mathf.Repeat(beta, 2 * Mathf.PI);

        float sinAlpha = Mathf.Sin(alpha);

        float y = Mathf.Cos(alpha) * radius;
        float x = sinAlpha * Mathf.Cos(beta) * radius;
        float z = sinAlpha * Mathf.Sin(beta) * radius;

        return new Vector3(x, y, z);
    }

    public static Vector2 GetSphereProjection(float[] vector, float alpha, float beta)
    {
        return GetSphereProjection(vector[0], vector[1], vector[2], alpha, beta);
    }

    public static Vector2 GetSphereProjection(float x, float y, float z, float alpha, float beta)
    {
        if ((alpha < 0) || (alpha > Mathf.PI)) throw new System.Exception("alpha value must be not less than 0 and not greater than Mathf.PI");

        while (beta < 0) beta += Mathf.PI;

        beta = Mathf.Repeat(beta, 2 * Mathf.PI);

        float sinAlpha = Mathf.Sin(alpha);
        float cosAlpha = Mathf.Cos(alpha);
        float sinBeta = Mathf.Sin(beta);
        float cosBeta = Mathf.Cos(beta);

        float aX = x * -cosAlpha * cosBeta;
        float aY = y * sinAlpha;
        float aZ = z * -cosAlpha * sinBeta;

        float bX = x * sinBeta;
        float bZ = z * cosBeta;

        return new Vector2(aX + aY + aZ, bX + bZ);
    }

    public static float RoundToSixDecimals(float value)
    {
#if DEBUG
        if (!value.IsInsideRange(0,1))
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

            Debug.LogWarning("This function is meant to be used only with values between 0 and 1. Value = " +
                value + ", stackTrace:\n" + stackTrace);
        }
#endif

        // To reduce rounding problems with float serialization we round serialized floats to six decimals while running the simulation
        return (float)System.Math.Round(value, 6);
    }

    public static float MultiplyAndGetDecimals(float a, float b, out float decimals)
    {
        float exact = a * b;
        float result = Mathf.Floor(exact);

        decimals = exact - result;

        return result;
    }

    public static float DivideAndGetDecimals(float a, float b, out float decimals)
    {
        float exact = a / b;
        float result = Mathf.Floor(exact);

        decimals = exact - result;

        return result;
    }

    // a, b inputs need to be greater than 1 for the result to be meaningful
    public static int LerpToIntAndGetDecimals(int a, int b, float f, out float decimals)
    {
        float ab = Mathf.Lerp(a, b, f);
        int pab = Mathf.FloorToInt(ab);

        decimals = ab - pab;

        return pab;
    }

    /// <summary>
    /// Given b, c and f, return the original value for 'a' in c = lerp(a,b,f)
    /// NOTE: This is not the same as InverseLerp, which solves the equation for 'f'...
    /// </summary>
    /// <param name="c">The output of lerp(a,b,f)</param>
    /// <param name="b">The second input from lerp(a,b,f)</param>
    /// <param name="f">the lerp percentage</param>
    /// <returns>The first input from lerp(a,b,f)</returns>
    public static int ReverseLerp(int c, int b, float f)
    {
        float a = ReverseLerp(c, b, f);
        return Mathf.FloorToInt(a);
    }

    /// <summary>
    /// Given b, c and f, return the original value for 'a' in c = lerp(a,b,f)
    /// NOTE: This is not the same as InverseLerp, which solves the equation for 'f'...
    /// </summary>
    /// <param name="c">The output of lerp(a,b,f)</param>
    /// <param name="b">The second input from lerp(a,b,f)</param>
    /// <param name="f">the lerp percentage</param>
    /// <returns>The first input from lerp(a,b,f)</returns>
    public static float ReverseLerp(float c, float b, float f)
    {
        if (!f.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException("'f' must be a value between 0 and 1 (inclusive)");
        }

        if (f == 1)
        {
            return float.NaN;
        }

        float a = ((b * f) - c) / (f - 1);

        return a;
    }

    /// <summary>
    /// Given b, c and f, return a value for 'a' that approximates c = lerp(a,b,f)
    /// but doesn't leave the range between 0 and 1.
    /// </summary>
    /// <param name="c">The output of lerp(a,b,f)</param>
    /// <param name="b">The second input from lerp(a,b,f)</param>
    /// <param name="f">the lerp percentage</param>
    /// <returns>The first input from lerp(a,b,f)</returns>
    public static float UnLerp(float c, float b, float f)
    {
        if (!f.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException("'f' must be a value between 0 and 1 (inclusive)");
        }

        if (f == 1)
        {
            return float.NaN;
        }

        float a = ((b * f) - c) / (f - 1);
        float ca = c - a;

        if (b > c)
        {
            a = c * (1 - (ca / (ca + 1)));
        }
        else
        {
            ca = -ca;

            a = c + (1 - c) * (ca / (ca + 1));
        }

        return a;
    }

    // Only for values between 0 and 1
    public static float DecreaseByPercent(float value, float percentage)
    {
        return value * (1f - percentage);
    }

    // Only for values between 0 and 1
    public static float IncreaseByPercent(float value, float percentage)
    {
        return value + ((1f - value) * percentage);
    }

    // TODO: Lazy implementation. Do better...
    public static float GetPseudoNormalDistribution(float x)
    {
        x = Mathf.Abs(x);
        
        if (x < 0.05) return 0.398942f;
        if (x < 0.15) return 0.396953f;
        if (x < 0.25) return 0.391043f;
        if (x < 0.35) return 0.381388f;
        if (x < 0.45) return 0.368270f;
        if (x < 0.55) return 0.352065f;
        if (x < 0.65) return 0.333225f;
        if (x < 0.75) return 0.312254f;
        if (x < 0.85) return 0.289692f;
        if (x < 0.95) return 0.266085f;
        if (x < 1.05) return 0.241971f;
        if (x < 1.15) return 0.217852f;
        if (x < 1.25) return 0.194186f;
        if (x < 1.35) return 0.171369f;
        if (x < 1.45) return 0.149727f;
        if (x < 1.55) return 0.129518f;
        if (x < 1.65) return 0.110921f;
        if (x < 1.75) return 0.094049f;
        if (x < 1.85) return 0.078950f;
        if (x < 1.95) return 0.065616f;
        if (x < 2.05) return 0.053991f;
        if (x < 2.15) return 0.043984f;
        if (x < 2.25) return 0.035475f;
        if (x < 2.35) return 0.028327f;
        if (x < 2.45) return 0.022395f;
        if (x < 2.55) return 0.017528f;
        if (x < 2.65) return 0.013583f;
        if (x < 2.75) return 0.010421f;
        if (x < 2.85) return 0.007915f;
        if (x < 2.95) return 0.005953f;
        if (x < 3.05) return 0.004432f;
        if (x < 3.15) return 0.003267f;
        if (x < 3.25) return 0.002384f;
        if (x < 3.35) return 0.001723f;
        if (x < 3.45) return 0.001232f;
        if (x < 3.55) return 0.000873f;
        if (x < 3.65) return 0.000612f;
        if (x < 3.75) return 0.000425f;
        if (x < 3.85) return 0.000292f;
        if (x < 3.95) return 0.000199f;
        if (x < 4.05) return 0.000134f;
        if (x < 4.15) return 0.000089f;
        if (x < 4.25) return 0.000059f;
        if (x < 4.35) return 0.000039f;
        if (x < 4.45) return 0.000025f;
        if (x < 4.55) return 0.000016f;
        if (x < 4.65) return 0.000010f;
        if (x < 4.75) return 0.000006f;
        if (x < 4.85) return 0.000004f;
        if (x < 4.95) return 0.000002f;
        if (x < 5.05) return 0.000001f;
        
        return 0;
    }

    /// <summary>
    /// Scales the input so that the output equals 1 if value == max, and
    /// equals 0 if value == min
    /// </summary>
    /// <param name="value">The input</param>
    /// <param name="max">Max possible value for input</param>
    /// <param name="min">Min possible value for input</param>
    /// <returns>the normalized value</returns>
    public static float Normalize(float value, float max, float min)
    {
        return (value - min) / (max - min);
    }

    /// <summary>
    /// Converts value using a logaritmic scale to return a value between 0 and 1
    /// </summary>
    /// <param name="value">input to scale</param>
    /// <returns>logaritmically scaled value between 0 and 1</returns>
    public static float ToPseudoLogaritmicScale01(int value)
    {
        // 1, 3, 10, 32, 100, 316, 1000, 3162, 10000, 31623, 100000, 316227

        if (value >= 100)
        {
            if (value >= 3162)
            {
                if (value >= 10000)
                {
                    if (value >= 31623)
                    {
                        return 1f;
                    }

                    return Mathf.Lerp(0.9f, 1f, Normalize(value, 31623f, 10000f));
                }

                return Mathf.Lerp(0.8f, 0.9f, Normalize(value, 10000f, 3162f));
            }

            if (value >= 316)
            {
                if (value >= 1000)
                {
                    return Mathf.Lerp(0.7f, 8f, Normalize(value, 3162f, 1000f));
                }

                return Mathf.Lerp(0.6f, 0.7f, Normalize(value, 1000f, 316f));
            }

            return Mathf.Lerp(0.5f, 0.6f, Normalize(value, 316f, 100f));
        }

        if (value >= 3)
        {
            if (value >= 10)
            {
                if (value >= 32)
                {
                    return Mathf.Lerp(0.4f, 0.5f, Normalize(value, 100f, 32f));
                }

                return Mathf.Lerp(0.3f, 0.4f, Normalize(value, 32f, 10f));
            }

            return Mathf.Lerp(0.2f, 0.3f, Normalize(value, 10f, 3f));
        }

        if (value == 2)
        {
            return 0.15f;
        }

        if (value == 1)
        {
            return 0.1f;
        }

        return 0f;
    }

    /// <summary>
    /// Converts value to a value between 0 and 1 using a logaritmic scale
    /// but first it normalizes to max
    /// </summary>
    /// <param name="value">input to scale</param>
    /// <param name="max">normalization value</param>
    /// <returns>logaritmically scaled value between 0 and 1</returns>
    public static float ToPseudoLogaritmicScale01(float value, float max)
    {
        // 1, 3, 10, 32, 100, 316, 1000, 3162, 10000, 31623, 100000, 316227

        if (value >= max)
            return 1f;

        value = 31623f * value / max;

        if (value >= 100f)
        {
            if (value >= 3162f)
            {
                if (value >= 10000f)
                {
                    if (value >= 31623f)
                    {
                        return 1f;
                    }

                    return Mathf.Lerp(0.9f, 1f, Normalize(value, 31623f, 10000f));
                }

                return Mathf.Lerp(0.8f, 0.9f, Normalize(value, 10000f, 3162f));
            }

            if (value >= 316f)
            {
                if (value >= 1000f)
                {
                    return Mathf.Lerp(0.7f, 8f, Normalize(value, 3162f, 1000f));
                }

                return Mathf.Lerp(0.6f, 0.7f, Normalize(value, 1000f, 316f));
            }

            return Mathf.Lerp(0.5f, 0.6f, Normalize(value, 316f, 100f));
        }

        if (value >= 3f)
        {
            if (value >= 10f)
            {
                if (value >= 32f)
                {
                    return Mathf.Lerp(0.4f, 0.5f, Normalize(value, 100f, 32f));
                }

                return Mathf.Lerp(0.3f, 0.4f, Normalize(value, 32f, 10f));
            }

            return Mathf.Lerp(0.2f, 0.3f, Normalize(value, 10f, 3f));
        }

        if (value >= 1f)
        {
            return Mathf.Lerp(0.1f, 0.2f, Normalize(value, 3f, 1f));
        }

        return Mathf.Lerp(0.0f, 0.1f, value);
    }

    // TODO: Lazy implementation. Do better...
    public static T GetNextEnumValue<T>(this T value)
    {
        if (!typeof(T).IsEnum)
        {
            throw new System.ArgumentException("T must be an enumerated type");
        }

        bool found = false;

        System.Array values = System.Enum.GetValues(typeof(T));

        foreach (T v in values)
        {
            if (found)
                return v;

            if (v.Equals(value))
            found = true;
        }

        if (found)
        {
            return (T)values.GetValue(0);
        }

        return default;
    }

    public static int MinWrappedDist(int a, int b, int wrapLength)
    {
        int dist1 = Mathf.Abs(a - b);
        int dist2 = Mathf.Abs(a + wrapLength - b);

        return Mathf.Min(dist1, dist2);
    }

    public static void Extend(this ref RectInt rect, Vector2Int pos, int mapWidth)
    {
        rect.yMin = Mathf.Min(rect.yMin, pos.y);
        rect.yMax = Mathf.Max(rect.yMax, pos.y);

        // we can't have a rect with a width larger than the map length
        if (rect.width == mapWidth)
            return;

        int distLeft = 0;
        int distRight = 0;

        // this big if-block will take care of handling the case where the rect
        // could end up wrapping around the map longitude-wise
        if (pos.x < rect.xMin)
        {
            // if the wrapped around longitude falls inside the rect then
            // ignore it
            if ((pos.x + mapWidth) < rect.xMax)
                return;

            distLeft = rect.xMin - pos.x;
            distRight = pos.x + mapWidth - rect.xMax;
        }

        // this big if-block will take care of handling the case where the rect
        // could end up wrapping around the map longitude-wise on the reverse
        // direction
        if (pos.x > rect.xMax)
        {
            // if the wrapped around longitude falls inside the rect then
            // ignore it
            if ((pos.x - mapWidth) > rect.xMin)
                return;

            distLeft = rect.xMin + mapWidth - pos.x;
            distRight = pos.x - rect.xMax;
        }

        // if the distance between the pos x and the rect max x is less
        // than the distance between the pos x and the rect min x,
        // that means we can encompass the pos with a smaller rect by
        // increasing the rect max x instead of decreasing the rect min x
        if (distRight < distLeft)
        {
            rect.xMax += distRight;
        }
        else
        {
            rect.xMin -= distLeft;
        }

        if (rect.width > mapWidth)
        {
            // make sure the target rect width doesn't exceed the map width
            rect.xMax = rect.xMin + mapWidth;
        }
    }

    public static void Extend(this ref RectInt target, RectInt source, int mapWidth)
    {
        target.Extend(source.min, mapWidth);
        target.Extend(source.max, mapWidth);
    }

    public static Rect Lerp(Rect a, Rect b, float t)
    {
        Rect r = new Rect();

        r.xMin = Mathf.Lerp(a.xMin, b.xMin, t);
        r.xMax = Mathf.Lerp(a.xMax, b.xMax, t);
        r.yMin = Mathf.Lerp(a.yMin, b.yMin, t);
        r.yMax = Mathf.Lerp(a.yMax, b.yMax, t);

        return r;
    }
}
