using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

public delegate int GetRandomIntDelegate (int maxValue);
public delegate float GetRandomFloatDelegate ();

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

		public static implicit operator T(ElementWeightPair<T> pair) {

			if (pair == null)
				return default(T);

			return pair.Value;
		}
	}

	public static ElementWeightPair<T> WeightedSelection<T> (ElementWeightPair<T>[] elementWeightPairs, float totalWeight, float selectionValue) {

		int count = elementWeightPairs.Length;

		if (count <= 0)
			return null;

		if (totalWeight <= 0) {

			if (selectionValue == 1) selectionValue = 0;

			int index = (int)Mathf.Floor(selectionValue * count);

			int i = 0;
			foreach (ElementWeightPair<T> pair in elementWeightPairs) {

				if (i == index) return pair;

				i++;
			}

			return null;
		}

		float totalNormalizedWeight = 0;
		foreach (ElementWeightPair<T> pair in elementWeightPairs) {

			totalNormalizedWeight += pair.Weight / totalWeight;

			if (totalNormalizedWeight >= selectionValue) return pair;
		}

		return null;
	}

	public static T RandomSelectAndRemove<T> (this List<T> list, GetRandomIntDelegate getRandomInt) {

		return list.RandomSelect (getRandomInt, 0, true);
	}

	public static T RandomSelect<T> (this List<T> list, GetRandomIntDelegate getRandomInt, int emptyInstances = 0, bool remove = false) {

		if (list.Count <= 0)
			return default(T);

		int index = getRandomInt (list.Count + emptyInstances);

		if (index >= list.Count)
			return default(T);

		T item = list [index];

		if (remove)
			list.RemoveAt (index);

		return item;
	}

	public static T RandomSelect<T> (this IEnumerable<T> enumerable, GetRandomIntDelegate getRandomInt, int emptyInstances = 0) {

		return new List<T> (enumerable).RandomSelect (getRandomInt, emptyInstances);
	}

	public static T RandomSelect<T> (this ICollection<T> collection, GetRandomIntDelegate getRandomInt, int emptyInstances = 0) {

		return new List<T> (collection).RandomSelect (getRandomInt, emptyInstances);
	}
}
