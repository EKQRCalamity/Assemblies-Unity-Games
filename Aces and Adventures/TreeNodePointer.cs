using System.Collections.Generic;

public class TreeNodePointer
{
	private List<int> _indexPath = new List<int>();

	public bool isRoot => _indexPath.Count == 0;

	public void Clear()
	{
		_indexPath.Clear();
	}

	public void Add(int index)
	{
		_indexPath.Add(index);
	}

	public void Reverse()
	{
		_indexPath.Reverse();
	}

	public ListEnumerator<int> Indices()
	{
		return _indexPath.Enumerate();
	}
}
