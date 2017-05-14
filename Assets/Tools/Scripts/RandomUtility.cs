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
}
