using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class RandomUtility {

	public static float Range (float min, float max) {
		
		float output = Random.Range (0f, 1f);
		output = (float)System.Math.Round (output, 4);

		return ((max - min) * output) + min;
	}

	public static Vector3 insideUnitSphere
	{
		get {
			Vector3 randVector = Random.insideUnitSphere;

			randVector.x = (float)System.Math.Round (randVector.x, 4);
			randVector.y = (float)System.Math.Round (randVector.y, 4);
			randVector.z = (float)System.Math.Round (randVector.z, 4);

			return randVector;
		}
	}

	public delegate int RngOffsetIntDelegate (int seed, int offset, int maxValue);

	// must return a value between 0 and 1
	public delegate float RngFloatDelegate (int offset);

	public static int RandomRound (int targetVal, int minVal, int maxVal, int minRound, RngOffsetIntDelegate rngDelegate, int offset = 0) {

		int currentMaxVal = maxVal;
		int currentMinVal = minVal;

		while (true) {
			if ((targetVal - currentMinVal) < minRound)
				return currentMinVal;

			int splitVal = currentMinVal + rngDelegate (currentMinVal, offset, currentMaxVal - currentMinVal);

			if (splitVal < targetVal) {
				if (splitVal == currentMinVal)
					splitVal++;

				currentMinVal = splitVal;
			} else {
			
				if (splitVal == currentMaxVal)
					splitVal--;

				currentMaxVal = splitVal;
			}
		}
	}

	public static float QuasiNormalDistribution (float x) {

		float val1 = 2f * ((x > 0.5) ? 1f - x : x);

		return val1 * val1;
	}

	public static int QuasiNormalDistribution (float x, int maxValue) {
	
		float y = QuasiNormalDistribution (x);

		return (int)(y * maxValue);
	}

	public static int NoOffsetRange (float x, float cutBelow = 0.25f, float cutAbove = 0.75f) {

		if (x < cutBelow)
			return -1;
	
		if (x > cutAbove)
			return 1;

		return 0;
	}
}
