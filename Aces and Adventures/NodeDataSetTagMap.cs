using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Set Tag Mapping", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1102u)]
[YouTubeVideo("Set Tag Mapping Node Tutorial", "hxhnWXjMhWk", -1, -1)]
public class NodeDataSetTagMap : NodeData
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	private NodeDataTagWithEdit _tagToSet;

	[ProtoMember(2)]
	[UIField]
	private NodeDataTag _setEqualTo;

	private Stack<string> _previousValues;

	private NodeDataTagWithEdit tagToSet => _tagToSet ?? (_tagToSet = new NodeDataTagWithEdit(null, graph.nodeGraphTags));

	private NodeDataTag setEqualTo => _setEqualTo ?? (_setEqualTo = new NodeDataTag(null, graph.nodeGraphTags));

	private IEnumerable<NodeDataTag> tags
	{
		get
		{
			yield return tagToSet;
			yield return setEqualTo;
		}
	}

	private Stack<string> previousValues => _previousValues ?? (_previousValues = new Stack<string>());

	protected override bool _hideNameInInspector => true;

	public override string name
	{
		get
		{
			return string.Concat("<nobr>", _tagToSet, "</nobr> = <nobr>", _setEqualTo, "</nobr>");
		}
		set
		{
		}
	}

	public override string contextPath => "Advanced/Flags/";

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.Alpha3);

	public override NodeDataIconType iconType => NodeDataIconType.SetTagMap;

	public override NodeGraph graph
	{
		get
		{
			return base.graph;
		}
		set
		{
			base.graph = value;
			_tagToSet.SetGraphTags(value.nodeGraphTags);
			_setEqualTo.SetGraphTags(value.nodeGraphTags);
		}
	}

	private bool _tagToSetSpecified => _tagToSet;

	private bool _setEqualToSpecified => _setEqualTo;

	public override void Preprocess()
	{
		if ((bool)this)
		{
			previousValues.Push(graph.nodeGraphTagMap[_tagToSet]);
			graph.nodeGraphTagMap[_tagToSet] = _setEqualTo;
		}
	}

	public override void UndoPreprocess()
	{
		if ((bool)this)
		{
			graph.nodeGraphTagMap[_tagToSet] = previousValues.Pop();
		}
	}

	public override void Process()
	{
		Preprocess();
	}

	public override void UndoProcess()
	{
		UndoPreprocess();
	}

	public override void Execute()
	{
		previousValues.Pop();
	}

	public static implicit operator bool(NodeDataSetTagMap tagMap)
	{
		if (tagMap != null && (bool)tagMap._tagToSet)
		{
			return tagMap._setEqualTo;
		}
		return false;
	}

	private void OnValidateUI()
	{
		foreach (NodeDataTag tag in tags)
		{
			tag.graphTags = graph.nodeGraphTags;
			if ((bool)tag)
			{
				graph.nodeGraphTags.tags.Add(tag);
			}
		}
	}
}
