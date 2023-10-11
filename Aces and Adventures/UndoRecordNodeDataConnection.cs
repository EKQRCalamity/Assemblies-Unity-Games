public class UndoRecordNodeDataConnection : AUndoRecord
{
	private NodeGraph _graph;

	private NodeDataConnection _connection;

	private EntryType _entryType;

	public override EntryType entryType => _entryType;

	public UndoRecordNodeDataConnection(NodeGraph graph, NodeDataConnection connection, EntryType entryType)
	{
		_graph = graph;
		_connection = connection;
		_entryType = entryType;
	}

	protected override void _Add()
	{
		_graph.AddConnection(_connection, _connection.id);
	}

	protected override void _Remove()
	{
		_graph.RemoveConnection(_connection);
	}
}
