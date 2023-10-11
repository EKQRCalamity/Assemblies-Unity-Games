public class UndoRecordNodeDataPastedValue : AUndoRecord
{
	private NodeData _nodePastedOver;

	private byte[] _copiedValueData;

	private byte[] _nodePastedOverData;

	public UndoRecordNodeDataPastedValue(NodeData copiedValue, NodeData nodePastedOver, byte[] nodePastedOverData)
	{
		_nodePastedOver = nodePastedOver;
		_copiedValueData = ProtoUtil.ToByteArray(copiedValue);
		_nodePastedOverData = nodePastedOverData;
	}

	public override void Undo()
	{
		NodeGraph.PasteValues(ProtoUtil.FromBytes<NodeData>(_nodePastedOverData), _nodePastedOver, createNodePastedOverData: false);
	}

	public override void Redo()
	{
		NodeGraph.PasteValues(ProtoUtil.FromBytes<NodeData>(_copiedValueData), _nodePastedOver, createNodePastedOverData: false);
	}
}
