using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Linq;

public delegate int GetRandomIntDelegate(int maxValue);
public delegate int GetRandomIntWithOffsetDelegate(int offset, int maxValue);
public delegate float GetRandomFloatDelegate();
public delegate float GetRandomFloatWithOffsetDelegate(int offset);

public static class CollectionUtility
{
    public delegate float NormalizedValueGeneratorDelegate();

    public abstract class ElementWeightPair<T>
    {
        public T Value;

        [XmlAttribute]
        public float Weight;

        public ElementWeightPair()
        {

        }

        public ElementWeightPair(T value, float weight)
        {
            Value = value;
            Weight = weight;
        }

        public static implicit operator T(ElementWeightPair<T> pair)
        {
            if (pair == null)
                return default(T);

            return pair.Value;
        }
    }

    public static ElementWeightPair<T> WeightedSelection<T>(ElementWeightPair<T>[] elementWeightPairs, float totalWeight, float selectionValue)
    {
        int count = elementWeightPairs.Length;

        if (count <= 0)
            return null;

        if (totalWeight <= 0)
        {
            if (selectionValue == 1) selectionValue = 0;

            int index = (int)(selectionValue * count);

            int i = 0;
            foreach (ElementWeightPair<T> pair in elementWeightPairs)
            {
                if (i == index) return pair;

                i++;
            }

            return null;
        }

        float totalNormalizedWeight = 0;
        foreach (ElementWeightPair<T> pair in elementWeightPairs)
        {
            totalNormalizedWeight += pair.Weight / totalWeight;

            if (totalNormalizedWeight >= selectionValue) return pair;
        }

        return null;
    }

    public static T RandomSelectAndRemove<T>(this List<T> list, GetRandomIntDelegate getRandomInt)
    {
        return list.RandomSelect(getRandomInt, 0, true);
    }

    public static T RandomSelect<T>(this List<T> list, GetRandomIntDelegate getRandomInt, int emptyInstances = 0, bool remove = false)
    {
        if (list.Count <= 0)
            return default(T);

        int index = getRandomInt(list.Count + emptyInstances);

        if (index >= list.Count)
            return default(T);

        T item = list[index];

        if (remove)
            list.RemoveAt(index);

        return item;
    }

    public static T RandomSelect<T>(this IEnumerable<T> enumerable, GetRandomIntDelegate getRandomInt, int emptyInstances = 0)
    {
        int count = enumerable.Count();

        int index = getRandomInt(count + emptyInstances);

        if (index >= count)
            return default(T);

        return enumerable.ElementAt(index);
    }

    public static T RandomSelect<T>(this ICollection<T> collection, GetRandomIntDelegate getRandomInt, int emptyInstances = 0)
    {
        int index = getRandomInt(collection.Count + emptyInstances);

        if (index >= collection.Count)
            return default(T);

        return collection.ElementAt(index);
    }

    public static T RandomSelect<T>(
        this ICollection<T> collection,
        GetRandomIntWithOffsetDelegate getRandomInt,
        int offset,
        int emptyInstances = 0)
    {
        int index = getRandomInt(offset, collection.Count + emptyInstances);

        if (index >= collection.Count)
            return default(T);

        return collection.ElementAt(index);
    }

    public static T ReturnNBest<T>(this ICollection<T> collection, int n, Comparison<T> comp)
    {
        T[] tArray = new T[n];
        int existing = 0;

        if (collection.Count <= 0)
        {
            throw new System.ArgumentException("Collection has 0 or less elements");
        }

        foreach (T item in collection)
        {
            T current = item;

            int i;
            for (i = 0; i < existing; i++)
            {
                if (comp(current, tArray[i]) <= 0) continue;

                T temp = current;
                current = tArray[i];
                tArray[i] = temp;
            }

            if (existing < n)
            {
                tArray[existing] = current;
                existing++;
            }
        }

        return tArray[existing];
    }
}
