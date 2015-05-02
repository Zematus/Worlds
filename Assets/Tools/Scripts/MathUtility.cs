using UnityEngine;
using System.Collections;

public abstract class MathUtility {

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
}
