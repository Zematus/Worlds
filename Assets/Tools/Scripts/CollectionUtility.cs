using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

public static class CollectionUtility {

	public delegate float NormalizedValueGeneratorDelegate ();

	public abstract class ElementWeightPair<T> {

		public T Value;

		[XmlAttribute]
		public float Weight;

		public ElementWeightPair () {

		}

		public ElementWeightPair (T value, float weight) {

			Value = value;
			Weight = weight;
		}
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

	public static IEnumerable<T> FindAll<T> (this ICollection<T> collection, Predicate<T> p) {

		foreach (T item in collection) {

			if (p (item))
				yield return item;
		}
	}
}
