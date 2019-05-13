using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathUtility
{
    public const float NormalAt0 = 0.398942f;
    public const float NormalAt1 = 0.241971f;
    public const float NormalAt2 = 0.053991f;
    public const float NormalAt3 = 0.004432f;
    public const float NormalAt4 = 0.000134f;
    public const float NormalAt5 = 0.000001f;

    public static bool IsInsideRange(this float value, float minValue, float maxValue)
    {
        return (value >= minValue) && (value <= maxValue);
    }

    public static bool IsInsideRange(this int value, int minValue, int maxValue)
    {
        return (value >= minValue) && (value <= maxValue);
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

    public static Vector3 GetCartesianCoordinates(Vector3 sphericalVector)
    {
        return GetCartesianCoordinates(sphericalVector.x, sphericalVector.y, sphericalVector.z);
    }

    public static float RoundToSixDecimals(float value)
    {
#if DEBUG
        if ((value < 0) || (value > 1))
        {
            Debug.LogWarning("This function is meant to be used only with values between 0 and 1. Value = " + value);
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

    // TODO: Lazy implementation. Do better...
    public static float ToPseudoLogaritmicScale01(int value)
    {
        // 1, 3, 10, 32, 100, 316, 1000, 3162, 10000, 31623, 100000, 316227

        if (value >= 31623)
            return 1f;

        if (value >= 10000)
            return 0.9f;

        if (value >= 3162)
            return 0.8f;

        if (value >= 1000)
            return 0.7f;

        if (value >= 316)
            return 0.6f;

        if (value >= 100)
            return 0.5f;

        if (value >= 32)
            return 0.4f;

        if (value >= 10)
            return 0.3f;

        if (value >= 3)
            return 0.2f;

        if (value >= 1)
            return 0.1f;

        return 0f;
    }

    // TODO: Lazy implementation. Do better...
    public static float ToPseudoLogaritmicScale01(float value, float max)
    {
        // 1, 3, 10, 32, 100, 316, 1000, 3162, 10000, 31623, 100000, 316227

        if (value >= max)
            return 1f;

        float scaledMax = max / 31623;

        if (value >= scaledMax * 10000)
            return 0.9f;

        if (value >= scaledMax * 3162)
            return 0.8f;

        if (value >= scaledMax * 1000)
            return 0.7f;

        if (value >= scaledMax * 316)
            return 0.6f;

        if (value >= scaledMax * 100)
            return 0.5f;

        if (value >= scaledMax * 32)
            return 0.4f;

        if (value >= scaledMax * 10)
            return 0.3f;

        if (value >= scaledMax * 3)
            return 0.2f;

        if (value >= scaledMax)
            return 0.1f;

        return 0f;
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

        return default(T);
    }
}
