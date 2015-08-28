using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public static float MixValues (float a, float b, float weightB) {
	
		return (b * weightB) + (a * (1f - weightB));
	}

	public delegate float GetWeightDelegate<T> (T element);
	
	public static T WeightedSelection<T> (float score, ICollection<T> elements, GetWeightDelegate<T> evaluator) {

		if (elements.Count <= 0)
			return default(T);

		float totalWeight = 0;

		float[] weights = new float[elements.Count];
		
		int i = 0;
		foreach (T element in elements) {

			float weight = evaluator(element);

			weights[i] = weight;
			
			totalWeight += weight;

			i++;
		}

		if (totalWeight == 0) {

			float length = weights.Length;
		
			for (int j = 0; j < length; j++)
			{
				weights[i] = 1 / length;
			}

			totalWeight = 1;
		}

		i = 0;
		float totalNormalizedWeight = 0;
		foreach (T element in elements) {
		
			totalNormalizedWeight += weights[i] / totalWeight;

			if (totalNormalizedWeight >= score) return element;
			
			i++;
		}

		return default(T);
	}
}
