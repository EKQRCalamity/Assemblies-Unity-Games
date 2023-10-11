using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ScreenplayBuilder
{
	private ScreenplayBuilder()
	{
	}

	public ScreenplayBuilder(string title, IEnumerable<NodeData> subGraphNodes, Transform parent)
	{
	}

	private ScreenplayBuilder _GenerateScreenplay()
	{
		return this;
	}

	private void _AppendScreenPlayText(NodeData subGraphNode)
	{
	}

	public ScreenplayBuilder AppendSubToMain()
	{
		return this;
	}
}
