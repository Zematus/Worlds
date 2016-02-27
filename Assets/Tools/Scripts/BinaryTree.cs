using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BinaryTreeNode<TKey, TValue> {

	public BinaryTreeNode<TKey, TValue> Parent { get; set; }

	public BinaryTreeNode<TKey, TValue> Right { get; set; }
	public BinaryTreeNode<TKey, TValue> Left { get; set; }

	public TKey Key { get; set; }
	public TValue Value { get; set; }
}

public class BinaryTree<TKey, TValue> {

	public IComparer<TKey> Comparer { get; set; }

	public int Count { get; private set; }

	private BinaryTreeNode<TKey, TValue> _rightmostItem = null;
	private BinaryTreeNode<TKey, TValue> _leftmostItem = null;

	private BinaryTreeNode<TKey, TValue> _root = null;

	public TValue Rightmost { 
		get { 
			return _rightmostItem.Value;
		} 
	}

	public TValue Leftmost { 
		get { 
			return _leftmostItem.Value;
		} 
	}

	public BinaryTree () {

		Count = 0;

		Comparer = Comparer<TKey>.Default;
	}

	public BinaryTree (IComparer<TKey> comparer) {

		Count = 0;

		Comparer = comparer;
	}

	public void Insert (TKey key, TValue value) {

		Count++;

		BinaryTreeNode<TKey, TValue> item = new BinaryTreeNode<TKey, TValue> ();

		item.Key = key;
		item.Value = value;

		if (_root == null) {

			_root = item;
			_rightmostItem = item;
			_leftmostItem = item;
			return;
		}

		BinaryTreeNode<TKey, TValue> node = _root;

		bool isRightmost = true;
		bool isLeftmost = true;

		while (true) {

			int comp = Comparer.Compare (item.Key, node.Key);

			if (comp > 0) {

				isLeftmost = false;
			
				if (node.Right == null) {
				
					item.Parent = node;
					node.Right = item;

					if (isRightmost)
						_rightmostItem = item;

					return;
				}

				node = node.Right;
				continue;

			} else {

				isRightmost = false;
			
				if (node.Left == null) {

					item.Parent = node;
					node.Left = item;

					if (isLeftmost)
						_leftmostItem = item;

					return;
				}

				node = node.Left;
				continue;
			}
		}
	}

	public TValue RemoveRightmost () {

		if (_rightmostItem == null)
			return default(TValue);

		Count--;

		TValue value = _rightmostItem.Value;

		if (_rightmostItem == _leftmostItem) {
		
			_root = null;
			_rightmostItem = null;
			_leftmostItem = null;

			return value;
		}

		BinaryTreeNode<TKey, TValue> parent = _rightmostItem.Parent;

		if (parent != null) {
			parent.Right = _leftmostItem.Left;

			if (parent.Right != null)
				parent.Right.Parent = parent;

			_rightmostItem = parent;
		} else {
			_rightmostItem = _rightmostItem.Left;
			_root = _rightmostItem;
		}

		while (_rightmostItem.Right != null) {
		
			_rightmostItem = _rightmostItem.Right;
		}

		return value;
	}

	public TValue RemoveLeftmost () {

		if (_leftmostItem == null)
			return default(TValue);

		Count--;

		TValue value = _leftmostItem.Value;

		if (_leftmostItem == _rightmostItem) {

			_root = null;
			_rightmostItem = null;
			_leftmostItem = null;

			return value;
		}

		BinaryTreeNode<TKey, TValue> parent = _leftmostItem.Parent;

		if (parent != null) {
			parent.Left = _leftmostItem.Right;

			if (parent.Left != null)
				parent.Left.Parent = parent;

			_leftmostItem = parent;
		} else {
			_leftmostItem = _leftmostItem.Right;
			_root = _leftmostItem;
		}

		while (_leftmostItem.Left != null) {

			_leftmostItem = _leftmostItem.Left;
		}

		return value;
	}

	public List<TValue> Values {
		
		get { 

			List<TValue> values = new List<TValue> (Count);

			while (_leftmostItem != null) {
			
				values.Add (RemoveLeftmost ());
			}

			return values;
		}
	}
}
