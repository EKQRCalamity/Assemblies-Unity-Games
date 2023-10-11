using System.Collections.Generic;

public class NodeDataYPositionComparer : IComparer<NodeData>
{
	public static NodeDataYPositionComparer Default = new NodeDataYPositionComparer();

	public int Compare(NodeData a, NodeData b)
	{
		int num = b.position.y.CompareTo(a.position.y);
		if (num == 0)
		{
			return a.CompareTo(b);
		}
		return num;
	}
}
