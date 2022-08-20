using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryHeap<T>
{
    public bool LastInsertedOnTop { get; private set; }

    public int Count { get; private set; }

    private int _arraySize = 1000;

    private readonly Comparison<T> _comparison;

    private readonly List<T[]> _arrayList;

    private bool _balanced = true;

    public BinaryHeap(Comparison<T> comparison, int arraySize = 1000)
    {
        _comparison = comparison;

        _arraySize = arraySize;

        _arrayList = new List<T[]>
        {
            new T[_arraySize]
        };

        LastInsertedOnTop = false;
    }

    public void Clear()
    {
        _arrayList.Clear();
        _arrayList.Add(new T[_arraySize]);

        Count = 0;
    }

    private void InsertAt(T item, int index)
    {
        int listIndex = index / _arraySize;
        int arrayIndex = index % _arraySize;

        if (listIndex >= _arrayList.Count)
        {
            _arrayList.Add(new T[_arraySize]);
        }

        _arrayList[listIndex][arrayIndex] = item;
    }

    private T ItemAt(int index)
    {
        int listIndex = index / _arraySize;
        int arrayIndex = index % _arraySize;

        return _arrayList[listIndex][arrayIndex];
    }

    public void Insert(T item)
    {
        if (!_balanced)
        {
            InsertFromTop(item, true);
            Count++;
            return;
        }

        int index = Count;
        Count++;

        LastInsertedOnTop = index == 0;

        InsertAt(item, index);

        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;

            T parent = ItemAt(parentIndex);

            int compareResult = _comparison(item, parent);

            if (compareResult < 0)
            {
                InsertAt(item, parentIndex);
                InsertAt(parent, index);

                LastInsertedOnTop = parentIndex == 0;
            }
            else
            {
                return;
            }

            index = parentIndex;
        }
    }

    private void InsertFromTop(T item, bool isNewItem = false)
    {
        int lastIndex = Count;
        int index = 0;

        LastInsertedOnTop = isNewItem;

        while (true)
        {
            InsertAt(item, index);

            int leftChildIndex = (index * 2) + 1;
            int rightChildIndex = leftChildIndex + 1;

            if (leftChildIndex >= lastIndex)
                break;

            int childIndex;
            T child;

            T leftChild = ItemAt(leftChildIndex);

            if (rightChildIndex < lastIndex)
            {
                T rightChild = ItemAt(rightChildIndex);

                if (_comparison(leftChild, rightChild) <= 0)
                {
                    childIndex = leftChildIndex;
                    child = leftChild;
                }
                else
                {
                    childIndex = rightChildIndex;
                    child = rightChild;
                }
            }
            else
            {
                childIndex = leftChildIndex;
                child = leftChild;
            }

            if (_comparison(child, item) < 0)
            {
                InsertAt(child, index);
                index = childIndex;

                LastInsertedOnTop = false;
            }
            else
            {
                break;
            }
        }

        _balanced = true;
    }

    public void Rebalance(bool force = false)
    {
        if (_balanced && !force) return;

        T last = ItemAt(Count);

        InsertFromTop(last);
    }
    
    public T Extract(bool rebalance = true)
    {
        Rebalance();

        T root = ItemAt(0);

        Count--;

        if (Count > 0)
        {
            _balanced = false;

            if (rebalance)
            {
                Rebalance();
            }
        }

        return root;
    }

    public T PeekTop()
    {
        Rebalance();

        return ItemAt(0);
    }
}
