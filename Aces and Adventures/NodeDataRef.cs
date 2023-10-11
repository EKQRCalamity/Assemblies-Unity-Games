using ProtoBuf;

[ProtoContract]
[UIField("Sub Graph Reference", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1101u)]
public class NodeDataRef : ANodeDataSubGraph
{
	[ProtoMember(1)]
	private DataRef<NodeGraphRef> _subGraphRef;

	[ProtoMember(2)]
	private bool _isDirty;

	private NodeGraphRef _snapshot;

	public DataRef<NodeGraphRef> nodeGraphRef => _subGraphRef;

	public bool canRevertChanges => _subGraphRef.CanRevertChanges();

	public override bool isRecursivelyNested
	{
		get
		{
			for (NodeGraph parentGraph = graph; parentGraph != null; parentGraph = parentGraph.parentGraph)
			{
				if (parentGraph.parentNode is NodeDataRef && ((NodeDataRef)parentGraph.parentNode).nodeGraphRef.Equals(nodeGraphRef))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override string name
	{
		get
		{
			if (_subGraphRef == null)
			{
				return subGraph.name;
			}
			return _subGraphRef.GetFriendlyName();
		}
		set
		{
			(_subGraphRef.IsValid() ? _subGraphRef.data : subGraph).name = value;
		}
	}

	public override NodeDataSpriteType spriteType => NodeDataSpriteType.SubGraph;

	public override NodeDataIconType iconType => NodeDataIconType.SubGraphRef;

	public override NodeGraph subGraph
	{
		get
		{
			if (_snapshot == null)
			{
				_snapshot = (NodeGraphRef)ProtoUtil.Clone(_subGraphRef.DataOrDefault()).SetParentNode(this);
				if (graph != null)
				{
					_snapshot.RestoreFlagMedia(graph);
				}
			}
			return _snapshot;
		}
	}

	public override bool hideFromContext => true;

	public override bool isSubGraph => true;

	private NodeDataRef()
	{
	}

	public NodeDataRef(NodeGraph subGraph)
	{
		_subGraphRef = new DataRef<NodeGraphRef>(new NodeGraphRef(subGraph));
	}

	public NodeDataRef(DataRef<NodeGraphRef> subGraphRef)
	{
		_subGraphRef = subGraphRef;
	}

	public override void SetGraphOnDeserialize(NodeGraph graphToSet)
	{
		_graph = graphToSet;
	}

	public override void PrepareForSave()
	{
		if (ContentRef.IsSavingContent && !(_isDirty = false) && _subGraphRef.IsValid())
		{
			_subGraphRef.Save();
		}
	}

	public override NodeData OnOpenSubGraphView(NodeData previousParentNode)
	{
		if (!NodeGraph.ParentNodeEqualityComparer.Default.Equals(this, (previousParentNode is NodeDataRef) ? previousParentNode : previousParentNode?.graph.owningReferenceNode) && !ProtoUtil.Equal(_snapshot, _subGraphRef.DataOrDefault()))
		{
			_snapshot = null;
		}
		return this;
	}

	public override void OnCloseSubGraphView(NodeData nextParentNode)
	{
		if (!NodeGraph.ParentNodeEqualityComparer.Default.Equals(this, (nextParentNode is NodeDataRef) ? nextParentNode : nextParentNode?.graph.owningReferenceNode) && _snapshot != null && _subGraphRef.IsValid())
		{
			_snapshot.SaveFlagMedia(graph);
			byte[] array = ProtoUtil.ToByteArray(_snapshot);
			_subGraphRef.committed = !(_isDirty = _subGraphRef.ContentInMemoryDistinctFromDisk(array));
			_subGraphRef.SetContent(ProtoUtil.FromBytes<NodeGraphRef>(array));
			if (nextParentNode == null)
			{
				base.OnCloseSubGraphView(null);
			}
		}
	}

	public NodeDataRef RevertChanges()
	{
		if (canRevertChanges)
		{
			_subGraphRef.RevertChanges();
		}
		return this;
	}
}
