using ProtoBuf;

[ProtoContract]
[UIField]
public class NodeDataTagWithEdit
{
	[ProtoMember(1)]
	[UIField]
	[UIHideIf("_hideTag")]
	private NodeDataTag _tag;

	public NodeDataTag tag => _tag ?? (_tag = new NodeDataTag(null));

	public NodeGraphTags graphTags
	{
		get
		{
			return tag.graphTags;
		}
		set
		{
			tag.graphTags = value;
		}
	}

	private bool _hideTag
	{
		get
		{
			if (graphTags != null)
			{
				return graphTags.tags.IsNullOrEmpty();
			}
			return true;
		}
	}

	private NodeDataTagWithEdit()
	{
	}

	public NodeDataTagWithEdit(string tag, NodeGraphTags graphTags)
	{
		_tag = new NodeDataTag(tag, graphTags);
	}

	public void OnValidate()
	{
		if (graphTags != null)
		{
			graphTags.tags.Add(tag);
		}
	}

	public override string ToString()
	{
		return this;
	}

	public static implicit operator NodeDataTag(NodeDataTagWithEdit nodeDataTagWithEdit)
	{
		return nodeDataTagWithEdit?._tag;
	}

	public static implicit operator string(NodeDataTagWithEdit nodeDataTagWithEdit)
	{
		if (nodeDataTagWithEdit == null)
		{
			return "<i>Null</i>";
		}
		return nodeDataTagWithEdit._tag;
	}

	public static implicit operator bool(NodeDataTagWithEdit nodeDataTagWithEdit)
	{
		if (nodeDataTagWithEdit != null)
		{
			return nodeDataTagWithEdit._tag;
		}
		return false;
	}

	[UIField(order = 1u)]
	private void _EditTags()
	{
		NodeDataTags.CreateEditPopup(graphTags);
	}
}
