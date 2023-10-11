public class UndoRecordActiveGraphChange : AUndoRecord
{
	private NodeGraphView _view;

	private NodeGraph _previousGraph;

	private NodeGraph _graph;

	public UndoRecordActiveGraphChange(NodeGraphView view, NodeGraph previousGraph, NodeGraph graph)
	{
		_view = view;
		_previousGraph = previousGraph;
		_graph = graph;
	}

	public override void Undo()
	{
		if (_previousGraph != null)
		{
			_view.parentNode = _previousGraph.parentNode;
		}
	}

	public override void Redo()
	{
		if (_graph != null)
		{
			_view.parentNode = _graph.parentNode;
		}
	}
}
