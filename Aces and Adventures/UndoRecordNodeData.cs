public class UndoRecordNodeData : AUndoRecord
{
	private NodeGraph _graph;

	private NodeData _node;

	private EntryType _entryType;

	public override EntryType entryType => _entryType;

	public UndoRecordNodeData(NodeGraph graph, NodeData node, EntryType entryType)
	{
		_graph = graph;
		_node = node;
		_entryType = entryType;
	}

	protected override void _Add()
	{
		_graph.AddNode(_node, _node.id);
	}

	protected override void _Remove()
	{
		_graph.RemoveNode(_node);
	}
}
