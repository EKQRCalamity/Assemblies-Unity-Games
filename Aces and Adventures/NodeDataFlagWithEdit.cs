using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class NodeDataFlagWithEdit
{
	[ProtoMember(1)]
	[UIField(order = 2u)]
	[UIHideIf("_hideFlag")]
	private NodeDataFlag _flag;

	public NodeDataFlag flag => _flag ?? (_flag = new NodeDataFlag());

	public string name => flag.name;

	public NodeGraphFlags nodeGraphFlags
	{
		get
		{
			return flag.nodeGraphFlags;
		}
		set
		{
			flag.nodeGraphFlags = value;
		}
	}

	private bool _hideFlag
	{
		get
		{
			if (nodeGraphFlags != null)
			{
				return nodeGraphFlags.count == 0;
			}
			return true;
		}
	}

	private bool _hideEditFlags => nodeGraphFlags == null;

	public void Clear()
	{
		flag.Clear();
	}

	public NodeDataFlagWithEdit SetNodeGraphFlags(NodeGraphFlags graphFlags)
	{
		if (nodeGraphFlags.GetFlagTypeSafe().HasValue && nodeGraphFlags.GetFlagTypeSafe() != graphFlags.GetFlagTypeSafe())
		{
			Clear();
		}
		nodeGraphFlags = graphFlags ?? nodeGraphFlags;
		return this;
	}

	public void OnValidate()
	{
		if (nodeGraphFlags != null)
		{
			if ((bool)this)
			{
				nodeGraphFlags.Add(this);
			}
			else
			{
				flag.flag = nodeGraphFlags.GetFirstFlag();
			}
		}
	}

	[UIField(order = 1u)]
	[UIHideIf("_hideEditFlags")]
	private void _EditFlags()
	{
		GameObject mainContent = UIUtil.CreateReflectedObject(nodeGraphFlags, 1600f, 900f, persistUI: true);
		Transform transform = UIGeneratorType.GetActiveUITransform<NodeData>().gameObject.GetRootComponent<Canvas>().transform;
		Action onClose = UIGeneratorType.ValidateAllOfType<NodeData>;
		UIUtil.CreatePopup("Edit Flags", mainContent, null, null, null, null, null, onClose, true, true, null, null, null, transform, null, null);
	}

	public override string ToString()
	{
		return flag.ToString();
	}

	public static implicit operator string(NodeDataFlagWithEdit f)
	{
		if (f == null)
		{
			return "<i>Null";
		}
		return f.ToString();
	}

	public static implicit operator bool(NodeDataFlagWithEdit f)
	{
		if (f != null)
		{
			return f.flag;
		}
		return false;
	}
}
