using System;
using ProtoBuf;

[ProtoContract]
[UIField]
public class NodeDataTag : IEquatable<NodeDataTag>
{
	public const string DEFAULT_TAG = "New Tag";

	[ProtoMember(1)]
	[UIField]
	private string _tag;

	public string tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public NodeGraphTags graphTags { get; set; }

	private NodeDataTag()
	{
	}

	public NodeDataTag(string tag)
	{
		_tag = tag;
	}

	public NodeDataTag(string tag, NodeGraphTags graphTags)
		: this(tag)
	{
		this.graphTags = graphTags;
	}

	public bool Equals(NodeDataTag other)
	{
		if (other != null)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(_tag, other._tag);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is NodeDataTag)
		{
			return Equals((NodeDataTag)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(_tag);
	}

	public static implicit operator string(NodeDataTag tag)
	{
		if (!tag)
		{
			return "<i>Null</i>";
		}
		return tag._tag;
	}

	public static implicit operator bool(NodeDataTag tag)
	{
		if (tag != null)
		{
			return !tag._tag.IsNullOrEmpty();
		}
		return false;
	}

	public override string ToString()
	{
		return _tag;
	}
}
