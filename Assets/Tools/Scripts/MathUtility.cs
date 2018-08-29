using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MathUtility {

	public static Vector3 GetCartesianCoordinates (float alpha, float beta, float radius) {
		
		if ((alpha < 0) || (alpha > Mathf.PI)) throw new System.Exception("alpha value must be not less than 0 and not greater than Mathf.PI");
		
		while (beta < 0) beta += Mathf.PI;
		
		beta = Mathf.Repeat(beta, 2 * Mathf.PI);
		
		float sinAlpha = Mathf.Sin(alpha);
		
		float y = Mathf.Cos(alpha) * radius;
		float x = sinAlpha * Mathf.Cos(beta) * radius;
		float z = sinAlpha * Mathf.Sin(beta) * radius;

		return new Vector3(x,y,z);
	}

	public static Vector3 GetCartesianCoordinates (Vector3 sphericalVector) {
	
		return GetCartesianCoordinates(sphericalVector.x, sphericalVector.y, sphericalVector.z);
	}

	//public static float MixValues (float a, float b, float weightB) {
	
	//	return (b * weightB) + (a * (1f - weightB));
	//}
		
	public static float RoundToSixDecimals (float value) {

		#if DEBUG
		if ((value < 0) || (value > 1)) {
			Debug.LogWarning ("This function is meant to be used only with values between 0 and 1. Value = " + value);
		}
		#endif
	
		// To reduce rounding problems with float serialization we round serialized floats to six decimals while running the simulation
		return (float)System.Math.Round (value, 6);
	}

	public static float MultiplyAndGetDecimals (float a, float b, out float decimals) {

		float exact = a * b;
		float result = Mathf.Floor(exact);

		decimals = exact - result;

		return result;
	}

	public static float DivideAndGetDecimals (float a, float b, out float decimals) {

		float exact = a / b;
		float result = Mathf.Floor(exact);

		decimals = exact - result;

		return result;
	}

	public static float MergeAndGetDecimals (float a, float b, float f, out float decimals) {

		float ab = a * (1f - f) + b * f;
		float pab = Mathf.Floor (ab);

		decimals = ab - pab;

		return pab;
	}

	// Only for values between 0 and 1
	public static float DecreaseByPercent (float value, float percentage) {

		return value * (1f - percentage);
	}

	// Only for values between 0 and 1
	public static float IncreaseByPercent (float value, float percentage) {

		return value + ((1f - value) * percentage);
	}

	public static float ToPseudoLogaritmicScale01 (int value) {

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

	public static float ToPseudoLogaritmicScale01 (float value, float max) {

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
}
