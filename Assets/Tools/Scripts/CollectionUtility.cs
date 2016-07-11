using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CollectionUtility {

	public delegate float NormalizedValueGeneratorDelegate ();
	public delegate float GetWeightDelegate<T> (T element);

	public class ElementWeightPair<T> {

		public T Value;
		public float Weight;
	}

	public static T WeightedSelection<T> (ElementWeightPair<T>[] elementWeightPairs, float totalWeight, NormalizedValueGeneratorDelegate generator) {

		int count = elementWeightPairs.Length;

		if (count <= 0)
			return default(T);

		float selectionValue = generator ();

		if (totalWeight <= 0) {

			if (selectionValue == 1) selectionValue = 0;

			int index = (int)Mathf.Floor(selectionValue * count);

			int i = 0;
			foreach (ElementWeightPair<T> pair in elementWeightPairs) {

				if (i == index) return pair.Value;

				i++;
			}

			return default(T);
		}

		float totalNormalizedWeight = 0;
		foreach (ElementWeightPair<T> pair in elementWeightPairs) {

			totalNormalizedWeight += pair.Weight / totalWeight;

			if (totalNormalizedWeight >= selectionValue) return pair.Value;
		}

		return default(T);
	}

	public static T WeightedSelection<T> (IDictionary<T, float> elementWeightPairs, float totalWeight, NormalizedValueGeneratorDelegate generator) {

		int count = elementWeightPairs.Count;
		
		if (count <= 0)
			return default(T);
		
		float selectionValue = generator ();
		
		if (totalWeight <= 0) {
			
			if (selectionValue == 1) selectionValue = 0;
			
			int index = (int)Mathf.Floor(selectionValue * count);

			int i = 0;
			foreach (T key in elementWeightPairs.Keys) {
			
				if (i == index) return key;

				i++;
			}
			
			return default(T);
		}

		float totalNormalizedWeight = 0;
		foreach (KeyValuePair<T, float> pair in elementWeightPairs) {
			
			totalNormalizedWeight += pair.Value / totalWeight;
			
			if (totalNormalizedWeight >= selectionValue) return pair.Key;
		}
		
		return default(T);
	}
	
	public static T WeightedSelection<T> (IList<T> elements, GetWeightDelegate<T> evaluator, NormalizedValueGeneratorDelegate generator) {

		if (elements.Count <= 0)
			return default(T);

		float totalWeight = 0;

		float[] weights = new float[elements.Count];
		
		int i = 0;
		foreach (T element in elements) {

			float weight = evaluator(element);

			if (weight < 0) 
				throw new System.Exception ("Weight returned by " + evaluator + " is less than zero: " + weight);

			weights[i] = weight;
			
			totalWeight += weight;

			i++;
		}
		
		float selectionValue = generator ();

		if (totalWeight == 0) {

			if (selectionValue == 1) selectionValue = 0;

			int index = (int)Mathf.Floor(selectionValue * elements.Count);

			return elements[index];
		}

		i = 0;
		float totalNormalizedWeight = 0;
		foreach (T element in elements) {
		
			totalNormalizedWeight += weights[i] / totalWeight;

			if (totalNormalizedWeight >= selectionValue) return element;
			
			i++;
		}

		return default(T);
	}

	public delegate T ItemProcessDelegate<T,U> (U item);

	public static IEnumerable<T> Process<T,U> (this ICollection<U> collection, ItemProcessDelegate<T,U> p) {
	
//		List<T> values = new List<T> ();

		foreach (U item in collection) {
		
//			values.Add (p (item));
			yield return p (item);
		}

//		return values;
	}
}
