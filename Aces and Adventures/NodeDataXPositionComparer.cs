using System.Collections.Generic;

public class NodeDataXPositionComparer : IComparer<NodeData>
{
	public static NodeDataXPositionComparer Default = new NodeDataXPositionComparer();

	public int Compare(NodeData a, NodeData b)
	{
		return a.position.x.CompareTo(b.position.x);
	}
}
