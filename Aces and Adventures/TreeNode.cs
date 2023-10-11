using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class TreeNode<T> : IEquatable<TreeNode<T>>, IComparable<TreeNode<T>>
{
	public struct TreeNodesDepthFirst
	{
		public struct Enumerator
		{
			private TreeNode<T> _current;

			private int _indexIntoChildIndices;

			private PoolStructListHandle<int> _childIndicesHandle;

			public TreeNode<T> Current
			{
				get
				{
					int num = _childIndicesHandle[_indexIntoChildIndices];
					if (num >= 0)
					{
						return _current.children[num];
					}
					return _current;
				}
			}

			public Enumerator(TreeNode<T> root)
			{
				_current = root;
				_childIndicesHandle = Pools.UseStructList<int>();
				_indexIntoChildIndices = 0;
				_childIndicesHandle.Add(-2);
			}

			public bool MoveNext()
			{
				int num = ++_childIndicesHandle[_indexIntoChildIndices];
				if (num < 0)
				{
					return true;
				}
				if (_current.children.Count > num)
				{
					_current = _current.children[num];
					_childIndicesHandle.Add(-1);
					_indexIntoChildIndices++;
					return true;
				}
				_current = _current.parent;
				_childIndicesHandle.RemoveAt(_childIndicesHandle.Count - 1);
				_indexIntoChildIndices--;
				if (_childIndicesHandle.Count == 0)
				{
					return _Dispose();
				}
				return MoveNext();
			}

			private bool _Dispose()
			{
				Pools.Repool(_childIndicesHandle);
				return false;
			}
		}

		private TreeNode<T> _root;

		public TreeNodesDepthFirst(TreeNode<T> root)
		{
			_root = root;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_root);
		}
	}

	[ProtoMember(1)]
	[UIField]
	public T value;

	[ProtoMember(2)]
	[UIField]
	public List<TreeNode<T>> children;

	public TreeNode<T> parent;

	public TreeNode<T> Root
	{
		get
		{
			TreeNode<T> treeNode = this;
			while (treeNode.parent != null)
			{
				treeNode = treeNode.parent;
			}
			return treeNode;
		}
	}

	public int Count
	{
		get
		{
			int count = 0;
			return _count(ref count);
		}
	}

	public int DepthLevel
	{
		get
		{
			int num = 0;
			for (TreeNode<T> treeNode = parent; treeNode != null; treeNode = treeNode.parent)
			{
				num++;
			}
			return num;
		}
	}

	private TreeNode()
	{
	}

	public TreeNode(T value)
	{
		this.value = value;
		children = new List<TreeNode<T>>();
	}

	private int _count(ref int count)
	{
		count++;
		foreach (TreeNode<T> child in children)
		{
			child._count(ref count);
		}
		return count;
	}

	public TreeNode<T> AddChild(T value)
	{
		return AddChild(new TreeNode<T>(value));
	}

	public TreeNode<T> AddChild(TreeNode<T> node)
	{
		node.parent = this;
		children.Add(node);
		return node;
	}

	private TreeNode<T> _AddChildBranch(int index, params T[] values)
	{
		if (index == values.Length)
		{
			return this;
		}
		T val = values[index++];
		for (int i = 0; i < children.Count; i++)
		{
			TreeNode<T> treeNode = children[i];
			if (val.Equals(treeNode.value))
			{
				return treeNode._AddChildBranch(index, values);
			}
		}
		return AddChild(val)._AddChildBranch(index, values);
	}

	public TreeNode<T> AddChildBranch(params T[] values)
	{
		return _AddChildBranch(0, values);
	}

	private void _AddChildBranchCopy(TreeNode<T> rootOfBranch, Func<TreeNode<T>, bool> validChild)
	{
		if (validChild(rootOfBranch))
		{
			TreeNode<T> treeNode = AddChild(rootOfBranch.value);
			for (int i = 0; i < rootOfBranch.children.Count; i++)
			{
				treeNode._AddChildBranchCopy(rootOfBranch.children[i], validChild);
			}
		}
	}

	public void SetParent(TreeNode<T> newParent, Func<TreeNode<T>, bool> canSwapWithChild = null)
	{
		if (newParent != this && !_SwapWithChild(newParent, canSwapWithChild))
		{
			RemoveFromParent();
			newParent.AddChild(this);
		}
	}

	private bool _SwapWithChild(TreeNode<T> node, Func<TreeNode<T>, bool> canSwapWithChild)
	{
		if (parent == null || node.parent == null)
		{
			return false;
		}
		if (canSwapWithChild != null && (!canSwapWithChild(this) || !canSwapWithChild(node)))
		{
			return false;
		}
		if (!Parents().Contains(node) && !node.Parents().Contains(this))
		{
			return false;
		}
		T val = value;
		value = node.value;
		node.value = val;
		return true;
	}

	public bool RemoveChild(TreeNode<T> node)
	{
		if (children.Remove(node))
		{
			node.parent = null;
			return true;
		}
		using (List<TreeNode<T>>.Enumerator enumerator = children.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.RemoveChild(node);
			}
		}
		return false;
	}

	public bool RemoveFromParent()
	{
		if (parent != null)
		{
			return parent.RemoveChild(this);
		}
		return false;
	}

	public void RemoveFromTree()
	{
		foreach (TreeNode<T> child in children)
		{
			child.parent = parent;
			if (parent != null)
			{
				parent.children.Add(child);
			}
		}
		RemoveFromParent();
	}

	public IEnumerable<TreeNode<T>> Parents(bool includeSelf = false)
	{
		if (includeSelf)
		{
			yield return this;
		}
		for (TreeNode<T> p = parent; p != null; p = p.parent)
		{
			yield return p;
		}
	}

	public bool ContainsParent(T valueToMatch, bool includeSelf = true, IEqualityComparer<T> equalityComparer = null)
	{
		TreeNode<T> treeNode = (includeSelf ? this : parent);
		IEqualityComparer<T> equalityComparer2 = equalityComparer ?? EqualityComparer<T>.Default;
		while (treeNode != null)
		{
			if (equalityComparer2.Equals(treeNode.value, valueToMatch))
			{
				return true;
			}
			treeNode = treeNode.parent;
		}
		return false;
	}

	public TreeNode<T> GetChildNodeByValueReference(T reference)
	{
		if (ObjectReferenceEqualityComparator.Default.Equals(value, reference))
		{
			return this;
		}
		for (int i = 0; i < children.Count; i++)
		{
			TreeNode<T> childNodeByValueReference = children[i].GetChildNodeByValueReference(reference);
			if (childNodeByValueReference != null)
			{
				return childNodeByValueReference;
			}
		}
		return null;
	}

	public PoolKeepItemListHandle<TreeNode<T>> GetPath(Func<TreeNode<T>, bool> stopAt = null)
	{
		PoolKeepItemListHandle<TreeNode<T>> poolKeepItemListHandle = Pools.UseKeepItemList<TreeNode<T>>();
		TreeNode<T> treeNode = this;
		do
		{
			poolKeepItemListHandle.Add(treeNode);
		}
		while ((stopAt == null || !stopAt(treeNode)) && (treeNode = treeNode.parent) != null);
		poolKeepItemListHandle.value.Reverse();
		return poolKeepItemListHandle;
	}

	public TreeNode<T> GetChild(Func<TreeNode<T>, bool> validChild)
	{
		if (children != null)
		{
			foreach (TreeNode<T> child in children)
			{
				if (validChild(child))
				{
					return child;
				}
			}
		}
		return null;
	}

	public TreeNode<T> GetSibling(Func<TreeNode<T>, bool> validSibling)
	{
		if (parent == null)
		{
			return null;
		}
		List<TreeNode<T>> list = parent.children;
		for (int i = 0; i < list.Count; i++)
		{
			if (validSibling(list[i]))
			{
				return list[i];
			}
		}
		return null;
	}

	public TreeNode<T> CreateCopyOfTreeByBranch<V, P>(Func<TreeNode<T>, bool> validChild) where V : T where P : T
	{
		TreeNode<T> treeNode = new TreeNode<T>(value);
		using PoolHashSetHandle<TreeNode<T>> poolHashSetHandle = Pools.UseHashSet<TreeNode<T>>();
		foreach (TreeNode<T> item in GetNodesOfType<V>())
		{
			TreeNode<T> rootOfBranch;
			using (PoolListHandle<TreeNode<T>> poolListHandle = item.GetParentNodesOfType<P>())
			{
				rootOfBranch = poolListHandle.value.LastRef() ?? item;
			}
			if (poolHashSetHandle.Add(rootOfBranch))
			{
				treeNode._AddChildBranchCopy(rootOfBranch, validChild);
			}
		}
		return treeNode;
	}

	public Dictionary<TreeNode<T>, Vector2> GetNodeRenderPositions(Vector2 position, Vector2 nodeSize, out Rect bounds, float yDirection = 1f)
	{
		Dictionary<TreeNode<T>, Vector2> dictionary = new Dictionary<TreeNode<T>, Vector2>(ReferenceEqualityComparer<TreeNode<T>>.Default);
		bounds = new Rect(position, nodeSize);
		_GetNodeRenderPositins(position, nodeSize, dictionary, ref bounds, yDirection);
		return dictionary;
	}

	private void _GetNodeRenderPositins(Vector2 position, Vector2 nodeSize, Dictionary<TreeNode<T>, Vector2> nodePositions, ref Rect bounds, float yDirection)
	{
		nodePositions.Add(this, position);
		bounds = bounds.Encapsulate(new Rect(position, nodeSize));
		float num = position.y + (float)(children.Count - 1) * nodeSize.y * 0.5f * yDirection;
		ListEnumerator<TreeNode<T>>.Enumerator enumerator = children.Enumerate().GetEnumerator();
		while (enumerator.MoveNext())
		{
			TreeNode<T> current = enumerator.Current;
			current._GetNodeRenderPositins(new Vector2(position.x + nodeSize.x, num), nodeSize, nodePositions, ref bounds, yDirection);
			num -= (float)Mathf.Max(1, current.children.Count) * nodeSize.y * yDirection;
		}
	}

	public void Sort()
	{
		children.Sort();
		for (int i = 0; i < children.Count; i++)
		{
			children[i].Sort();
		}
	}

	public void StableSort()
	{
		children.StableSort();
		for (int i = 0; i < children.Count; i++)
		{
			children[i].StableSort();
		}
	}

	public IEnumerable<T> DepthFirstEnum()
	{
		yield return value;
		for (int x = 0; x < children.Count; x++)
		{
			foreach (T item in children[x].DepthFirstEnum())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<TreeNode<T>> DepthFirstEnumNodes()
	{
		yield return this;
		for (int x = 0; x < children.Count; x++)
		{
			foreach (TreeNode<T> item in children[x].DepthFirstEnumNodes())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<TreeNode<T>> DepthFirstEnumNodes(Func<TreeNode<T>, bool> isValid)
	{
		if (!isValid(this))
		{
			yield break;
		}
		yield return this;
		for (int x = 0; x < children.Count; x++)
		{
			foreach (TreeNode<T> item in children[x].DepthFirstEnumNodes(isValid))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<TreeNode<T>> GetLeafNodes()
	{
		if (children.Count == 0)
		{
			yield return this;
		}
		for (int x = 0; x < children.Count; x++)
		{
			foreach (TreeNode<T> leafNode in children[x].GetLeafNodes())
			{
				yield return leafNode;
			}
		}
	}

	public PoolListHandle<TreeNode<T>> GetNodesOfType<K>() where K : T
	{
		PoolListHandle<TreeNode<T>> poolListHandle = Pools.UseList<TreeNode<T>>();
		_GetNodesOfType<K>(poolListHandle);
		return poolListHandle;
	}

	private void _GetNodesOfType<K>(PoolListHandle<TreeNode<T>> listHandle) where K : T
	{
		if (value is K)
		{
			listHandle.Add(this);
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i]._GetNodesOfType<K>(listHandle);
		}
	}

	public PoolKeepItemListHandle<V> GetValuesOfType<V>() where V : T
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		_GetNodesOfValue(poolKeepItemListHandle);
		return poolKeepItemListHandle;
	}

	private void _GetNodesOfValue<V>(PoolKeepItemListHandle<V> listHandle) where V : T
	{
		if (value is V)
		{
			listHandle.value.Add((V)(object)value);
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i]._GetNodesOfValue(listHandle);
		}
	}

	public PoolKeepItemListHandle<V> GetChildrenValuesOfType<V>() where V : T
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		for (int i = 0; i < children.Count; i++)
		{
			children[i]._GetNodesOfValue(poolKeepItemListHandle);
		}
		return poolKeepItemListHandle;
	}

	public PoolListHandle<TreeNode<T>> GetParentNodesOfType<K>() where K : T
	{
		PoolListHandle<TreeNode<T>> poolListHandle = Pools.UseList<TreeNode<T>>();
		_GetParentNodesOfType<K>(poolListHandle);
		return poolListHandle;
	}

	private void _GetParentNodesOfType<K>(PoolListHandle<TreeNode<T>> listHandle) where K : T
	{
		if (parent != null)
		{
			if (parent.value is K)
			{
				listHandle.Add(parent);
			}
			parent._GetParentNodesOfType<K>(listHandle);
		}
	}

	public PoolKeepItemListHandle<V> GetParentValuesOfType<V>() where V : T
	{
		PoolKeepItemListHandle<V> poolKeepItemListHandle = Pools.UseKeepItemList<V>();
		_GetParentValuesOfType(poolKeepItemListHandle);
		return poolKeepItemListHandle;
	}

	private void _GetParentValuesOfType<V>(PoolKeepItemListHandle<V> listHandle) where V : T
	{
		if (parent != null)
		{
			if (parent.value is V)
			{
				listHandle.Add((V)(object)parent.value);
			}
			parent._GetParentValuesOfType(listHandle);
		}
	}

	public PoolDictionaryHandle<TreeNode<T>, PoolListHandle<TreeNode<T>>> GetNodesOfTypeByParent<K>() where K : T
	{
		PoolDictionaryHandle<TreeNode<T>, PoolListHandle<TreeNode<T>>> poolDictionaryHandle = Pools.UseDictionary<TreeNode<T>, PoolListHandle<TreeNode<T>>>();
		_GetNodesOfTypeByParent<K>(poolDictionaryHandle);
		return poolDictionaryHandle;
	}

	private void _GetNodesOfTypeByParent<K>(PoolDictionaryHandle<TreeNode<T>, PoolListHandle<TreeNode<T>>> dictionaryHandle) where K : T
	{
		if (parent != null && value is K)
		{
			if (!dictionaryHandle.ContainsKey(parent))
			{
				dictionaryHandle.Add(parent, Pools.UseList<TreeNode<T>>());
			}
			dictionaryHandle[parent].Add(this);
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i]._GetNodesOfTypeByParent<K>(dictionaryHandle);
		}
	}

	public TreeNodesDepthFirst EnumerateDepthFirst()
	{
		return new TreeNodesDepthFirst(this);
	}

	public bool Equals(TreeNode<T> other)
	{
		return value.Equals(other.value);
	}

	public int CompareTo(TreeNode<T> other)
	{
		return Comparer<T>.Default.Compare(value, other.value);
	}

	public override string ToString()
	{
		object obj = value?.ToString();
		if (obj == null)
		{
			obj = "EMPTY_NODE";
		}
		return (string)obj;
	}

	[ProtoAfterDeserialization]
	private void AfterDeserialization()
	{
		children = children ?? new List<TreeNode<T>>();
		SetParentHierarchy();
	}

	private void SetParentHierarchy()
	{
		foreach (TreeNode<T> child in children)
		{
			child.parent = this;
			child.SetParentHierarchy();
		}
	}
}
