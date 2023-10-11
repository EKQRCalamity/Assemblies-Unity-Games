using System;
using ProtoBuf;

[ProtoContract]
[UIField("Connection", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class NodeDataConnection : IComparable<NodeDataConnection>
{
	[ProtoMember(1)]
	public uint id { get; set; }

	[ProtoMember(2)]
	public uint outputNodeId { get; set; }

	[ProtoMember(3)]
	public uint inputNodeId { get; set; }

	[ProtoMember(4)]
	public NodeData.Type outputType { get; set; }

	public NodeGraph graph { get; set; }

	public NodeData outputNode => graph.GetNode(outputNodeId);

	public NodeData inputNode => graph.GetNode(inputNodeId);

	private NodeDataConnection()
	{
	}

	public NodeDataConnection(uint outputNodeId, uint inputNodeId, NodeData.Type outputType = NodeData.Type.True)
	{
		this.outputNodeId = outputNodeId;
		this.inputNodeId = inputNodeId;
		this.outputType = outputType;
	}

	public void AddToNodeConnections()
	{
		outputNode.AddConnection(this);
		inputNode.AddConnection(this);
	}

	public void RemoveFromNodeConnections()
	{
		outputNode.RemoveConnection(this);
		inputNode.RemoveConnection(this);
	}

	public bool CanBeAdded()
	{
		foreach (NodeDataConnection outputConnection in outputNode.GetOutputConnections())
		{
			if (IsRedundantWith(outputConnection))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsRedundantWith(NodeDataConnection other, bool checkGraphEquality = true)
	{
		if (other != null && outputNodeId == other.outputNodeId && inputNodeId == other.inputNodeId && outputType == other.outputType)
		{
			if (checkGraphEquality)
			{
				return graph == other.graph;
			}
			return true;
		}
		return false;
	}

	public bool Contains(uint nodeId)
	{
		if (outputNodeId != nodeId)
		{
			return inputNodeId == nodeId;
		}
		return true;
	}

	public NodeDataConnection SetOutputType(NodeData.Type type)
	{
		outputType = type;
		return this;
	}

	public static implicit operator uint(NodeDataConnection node)
	{
		return node?.id ?? 0;
	}

	public int CompareTo(NodeDataConnection other)
	{
		int num = outputType - other.outputType;
		if (num == 0)
		{
			return other.inputNode.position.y.CompareTo(inputNode.position.y);
		}
		return num;
	}
}
