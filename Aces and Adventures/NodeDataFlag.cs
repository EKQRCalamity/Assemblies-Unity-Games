using System;
using ProtoBuf;

[ProtoContract]
[UIField]
public class NodeDataFlag : IEquatable<NodeDataFlag>
{
	public const string NULL_FLAG = "<i>Null";

	[ProtoMember(1)]
	private string _flag;

	public string flag
	{
		get
		{
			return _flag;
		}
		set
		{
			_flag = value;
		}
	}

	public string name => nodeGraphFlags.GetMedia(this).nameOverride ?? flag;

	public NodeGraphFlags nodeGraphFlags { get; set; }

	public CroppedImageRef croppedImage => nodeGraphFlags.GetMedia(this);

	public string description => nodeGraphFlags.GetMedia(this).description ?? "";

	public NodeDataFlag()
	{
	}

	public NodeDataFlag(string flagName, NodeGraphFlags flags)
	{
		_flag = flagName;
		nodeGraphFlags = flags;
	}

	public void Clear()
	{
		_flag = "<i>Null";
	}

	public bool Equals(NodeDataFlag other)
	{
		if (other != null)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(flag, other.flag);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is NodeDataFlag)
		{
			return this == (NodeDataFlag)obj;
		}
		return false;
	}

	public static bool operator ==(NodeDataFlag a, NodeDataFlag b)
	{
		if ((object)a == null || (object)b == null)
		{
			if ((object)a == null)
			{
				return (object)b == null;
			}
			return false;
		}
		if (a.nodeGraphFlags == b.nodeGraphFlags)
		{
			return a.Equals(b);
		}
		return false;
	}

	public static bool operator !=(NodeDataFlag a, NodeDataFlag b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		if (flag == null)
		{
			return int.MinValue;
		}
		return StringComparer.OrdinalIgnoreCase.GetHashCode(flag);
	}

	public override string ToString()
	{
		if (flag.IsNullOrEmpty())
		{
			return "<i>Null";
		}
		return flag;
	}

	public static implicit operator string(NodeDataFlag f)
	{
		if (!(f != null))
		{
			return "<i>Null";
		}
		return f.ToString();
	}

	public static implicit operator bool(NodeDataFlag f)
	{
		if (f != null && !f.ToString().IsNullOrEmpty())
		{
			return f.ToString() != "<i>Null";
		}
		return false;
	}
}
