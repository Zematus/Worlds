using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool FilterNodeDelegate<TKey, TValue> (BinaryTreeNode<TKey, TValue> node);
public delegate void FilterNodeEffectDelegate<TKey, TValue>  (BinaryTreeNode<TKey, TValue> node);

public class BinaryTreeNode<TKey, TValue> {

	public BinaryTreeNode<TKey, TValue> Parent { get; set; }

	public BinaryTreeNode<TKey, TValue> Right { get; set; }
	public BinaryTreeNode<TKey, TValue> Left { get; set; }

	public TKey Key { get; set; }
	public TValue Value { get; set; }

	public bool Marked { get; set; }

	public bool MarkedForRemoval { get; set; }
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

			#if DEBUG
			try {
				return _leftmostItem.Value;
			} catch {
				Debug.Break ();

				throw;
			}
			#else
			return _leftmostItem.Value;
			#endif
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
		int level = 0;

		BinaryTreeNode<TKey, TValue> item = new BinaryTreeNode<TKey, TValue> ();

		item.Key = key;
		item.Value = value;
		item.Marked = false;

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

//			Profiler.BeginSample ("Binary Tree Node Compare");

			level++;

			int comp = Comparer.Compare (item.Key, node.Key);

			if (comp > 0) {

				isLeftmost = false;
			
				if (node.Right == null) {
				
					item.Parent = node;
					node.Right = item;

					if (isRightmost)
						_rightmostItem = item;

//					Profiler.EndSample ();

					break;
				}

				node = node.Right;

//				Profiler.EndSample ();

				continue;

			} else {

				isRightmost = false;
			
				if (node.Left == null) {

					item.Parent = node;
					node.Left = item;

					if (isLeftmost)
						_leftmostItem = item;

//					Profiler.EndSample ();

					break;
				}

				node = node.Left;

//				Profiler.EndSample ();

				continue;
			}

//			Profiler.EndSample ();
		}

//		#if DEBUG
//		float sqrtCount = Mathf.Sqrt (Count);
//
//		if (level > (sqrtCount * 2)) {
//		
//			bool debug = true;
//		}
//		#endif
	}

	public void FindRightmost (FilterNodeDelegate<TKey, TValue> filterNode, FilterNodeEffectDelegate<TKey, TValue> filterNodeEffect) {

		while (true) {
		
			if (_rightmostItem == null)
				return;

			if (filterNode (_rightmostItem))
				return;

			if (filterNodeEffect != null)
				filterNodeEffect (_rightmostItem);

			RemoveRightmost ();
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
			parent.Right = _rightmostItem.Left;

			if (parent.Right != null) {
				parent.Right.Parent = parent;
				_rightmostItem = parent.Right;

			} else {
				_rightmostItem = parent;
			}

		} else {
			_rightmostItem = _rightmostItem.Left;
			_root = _rightmostItem;

			_rightmostItem.Parent = null;
		}

		while (_rightmostItem.Right != null) {
		
			_rightmostItem = _rightmostItem.Right;
		}

		return value;
	}

	public void FindLeftmost (FilterNodeDelegate<TKey, TValue> filterNode, FilterNodeEffectDelegate<TKey, TValue> filterNodeEffect) {

		while (true) {

			if (_leftmostItem == null)
				return;

			if (filterNode (_leftmostItem))
				return;

			if (filterNodeEffect != null)
				filterNodeEffect (_leftmostItem);

			RemoveLeftmost ();
		}
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

			if (parent.Left != null) {
				parent.Left.Parent = parent;
				_leftmostItem = parent.Left;

			} else {
				_leftmostItem = parent;
			}

		} else {
			_leftmostItem = _leftmostItem.Right;
			_root = _leftmostItem;

			_leftmostItem.Parent = null;
		}

		while (_leftmostItem.Left != null) {

			_leftmostItem = _leftmostItem.Left;
		}

		return value;
	}

	public TValue RemoveNode (BinaryTreeNode <TKey, TValue> node) {

		if (_leftmostItem == node)
			return RemoveLeftmost ();

		if (_rightmostItem == node)
			return RemoveRightmost ();

		BinaryTreeNode <TKey, TValue> parentNode = node.Parent;
		BinaryTreeNode <TKey, TValue> leftNode = node.Left;
		BinaryTreeNode <TKey, TValue> rightNode = node.Right;

		BinaryTreeNode <TKey, TValue> replacementNode;

		if (leftNode != null) {

			if (rightNode != null) {
				BinaryTreeNode <TKey, TValue> leftNodeRightmost = leftNode;

				while (leftNodeRightmost.Right != null) {
			
					leftNodeRightmost = leftNodeRightmost.Right;
				}

				leftNodeRightmost.Right = rightNode;
				rightNode.Parent = leftNodeRightmost;
			}

			replacementNode = leftNode;

		} else {

			replacementNode = rightNode;
		}

		if (parentNode != null) {
		
			if (parentNode.Left == node) {
			
				parentNode.Left = replacementNode;

			} else {

				parentNode.Right = replacementNode;
			}

			if (replacementNode != null) {
				replacementNode.Parent = parentNode;
			}

		} else {
		
			_root = replacementNode;

			if (replacementNode != null) {
				replacementNode.Parent = null;
			}
		}

		return node.Value;
	}

	public List<TValue> GetValues (FilterNodeDelegate<TKey, TValue> filterNode, FilterNodeEffectDelegate<TKey, TValue> filterNodeEffect = null, bool removeNodesMarkedForRemoval = false) {

		#if DEBUG
		int removedValues = 0;
		int ignoredValues = 0;
		int addedValues = 0;
		#endif

		List<TValue> values = new List<TValue> (Count);

		// Copy items to list from leftmost to rightmost, marking all inserted items along the way
		BinaryTreeNode <TKey, TValue> currentNode = _leftmostItem;

		while (currentNode != null) {

			if ((currentNode.Left != null) && (!currentNode.Left.Marked)) {

				currentNode = currentNode.Left;
				continue;
			}

			if (!currentNode.Marked) {

				if (filterNode (currentNode)) {
					
					values.Add (currentNode.Value);

					#if DEBUG
					addedValues++;
					#endif
				} else {
					
					if (filterNodeEffect != null)
						filterNodeEffect (currentNode);

					if (currentNode.MarkedForRemoval) {

						if (removeNodesMarkedForRemoval)
							RemoveNode (currentNode);

						currentNode.MarkedForRemoval = false;

						#if DEBUG
						removedValues++;
						#endif
					} else {

						#if DEBUG
						ignoredValues++;
						#endif
					}
				}

				currentNode.Marked = true;
			}

			if ((currentNode.Right != null) && (!currentNode.Right.Marked)) {

				currentNode = currentNode.Right;
				continue;
			}

			currentNode = currentNode.Parent;
		}

		#if DEBUG
		float totalValues = addedValues + ignoredValues + removedValues;
		float percentAdded = addedValues / totalValues;
		float percentIgnored = ignoredValues / totalValues;
		float percentRemoved = removedValues / totalValues;

		Debug.Log ("Total Events: " + totalValues + ". Serialized: " + percentAdded.ToString ("P") + ", Ignored: " + percentIgnored.ToString ("P") + ", Removed: " + percentRemoved.ToString ("P"));
		#endif

		//
		// Remove mark from all copied items
		//

		currentNode = _leftmostItem;

		while (currentNode != null) {

			if ((currentNode.Left != null) && (currentNode.Left.Marked)) {

				currentNode = currentNode.Left;
				continue;
			}

			currentNode.Marked = false;

			if ((currentNode.Right != null) && (currentNode.Right.Marked)) {

				currentNode = currentNode.Right;
				continue;
			}

			currentNode = currentNode.Parent;
		}

		return values;
	}

	public List<TValue> Values {
		
		get { 

			List<TValue> values = new List<TValue> (Count);

			// Copy items to list from leftmost to rightmost, marking all inserted items along the way
			BinaryTreeNode <TKey, TValue> currentNode = _leftmostItem;

			while (currentNode != null) {

				if ((currentNode.Left != null) && (!currentNode.Left.Marked)) {
				
					currentNode = currentNode.Left;
					continue;
				}

				if (!currentNode.Marked) {
					values.Add (currentNode.Value);

					currentNode.Marked = true;
				}

				if ((currentNode.Right != null) && (!currentNode.Right.Marked)) {

					currentNode = currentNode.Right;
					continue;
				}

				currentNode = currentNode.Parent;
			}

			// Remove mark from all copied items
			currentNode = _leftmostItem;

			while (currentNode != null) {

				if ((currentNode.Left != null) && (currentNode.Left.Marked)) {

					currentNode = currentNode.Left;
					continue;
				}

				currentNode.Marked = false;

				if ((currentNode.Right != null) && (currentNode.Right.Marked)) {

					currentNode = currentNode.Right;
					continue;
				}

				currentNode = currentNode.Parent;
			}

			return values;
		}
	}
}
