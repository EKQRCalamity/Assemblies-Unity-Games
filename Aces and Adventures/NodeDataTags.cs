using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class NodeDataTags
{
	private static TextBuilder _Builder;

	[ProtoMember(1, OverwriteList = true)]
	[UIField(dynamicInitMethod = "_InitTags", collapse = UICollapseType.Open)]
	[UIFieldCollectionItem(dynamicInitMethod = "_InitNodeDataTag")]
	[UIHideIf("_hideTags")]
	private HashSet<NodeDataTag> _tags;

	[ProtoMember(2)]
	[UIField(order = 1u)]
	[UIHideIf("_hideUseMappedTags")]
	[UIHorizontalLayout("Edit", flexibleWidth = 0f, preferredWidth = 1f, minWidth = 250f)]
	private bool _useMappedTags;

	private NodeGraph _graph;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	public bool useMappedTags => _useMappedTags;

	public HashSet<NodeDataTag> tags => _tags ?? (_tags = new HashSet<NodeDataTag>());

	public PoolKeepItemHashSetHandle<string> activeTags
	{
		get
		{
			if (_useMappedTags)
			{
				return graph.nodeGraphTagMap.GetMappedTags(tags);
			}
			PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
			foreach (NodeDataTag tag in tags)
			{
				poolKeepItemHashSetHandle.Add(tag);
			}
			return poolKeepItemHashSetHandle;
		}
	}

	public NodeGraph graph
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = value;
		}
	}

	public NodeGraphTags graphTags
	{
		get
		{
			if (_graph == null)
			{
				return null;
			}
			return _graph.nodeGraphTags;
		}
	}

	public string name => _useMappedTags.ToText("*") + ((tags.Count > 0) ? string.Concat(tags.First(), (tags.Count > 1) ? "…" : "") : "[Unidentified]");

	public int Count => tags.Count;

	private bool _hideTags
	{
		get
		{
			if (graphTags != null)
			{
				return graphTags.tags.Count == 0;
			}
			return true;
		}
	}

	private bool _hideUseMappedTags => _tags.IsNullOrEmpty();

	public static void CreateEditPopup(NodeGraphTags graphTags)
	{
		GameObject mainContent = UIUtil.CreateReflectedObject(graphTags, 1600f, 900f, persistUI: true);
		Transform transform = UIGeneratorType.GetActiveUITransform<NodeData>().gameObject.GetRootComponent<Canvas>().transform;
		Action onClose = UIGeneratorType.ValidateAllOfType<NodeData>;
		UIUtil.CreatePopup("Edit Tags", mainContent, null, null, null, null, null, onClose, true, true, null, null, null, transform, null, null);
	}

	public bool MatchFound(NodeDataTags otherTags, bool matchIfEmpty = false)
	{
		if (this.IsNullOrEmpty())
		{
			return matchIfEmpty;
		}
		if (otherTags.IsNullOrEmpty())
		{
			return false;
		}
		using (PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = otherTags.activeTags)
		{
			foreach (string activeTag in activeTags)
			{
				if (poolKeepItemHashSetHandle.Contains(activeTag))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool MatchFound(HashSet<string> otherTags, bool matchIfEmpty = false)
	{
		if (this.IsNullOrEmpty())
		{
			return matchIfEmpty;
		}
		if (otherTags.IsNullOrEmpty())
		{
			return false;
		}
		foreach (string activeTag in activeTags)
		{
			if (otherTags.Contains(activeTag))
			{
				return true;
			}
		}
		return false;
	}

	public static implicit operator string(NodeDataTags tags)
	{
		if (tags == null)
		{
			return "";
		}
		return tags.AppendText(Builder);
	}

	public override string ToString()
	{
		return this;
	}

	[UIField(order = 2u)]
	[UIHorizontalLayout("Edit", flexibleWidth = 999f)]
	private void _EditTags()
	{
		CreateEditPopup(graphTags);
	}

	private void _InitTags(UIFieldAttribute uiField)
	{
		uiField.maxCount = Mathf.Min(5, (graphTags != null) ? graphTags.tags.Count : 5);
	}

	private void _InitNodeDataTag(UIFieldAttribute uiField)
	{
		uiField.defaultValue = new NodeDataTag(graphTags.tags.FirstOrDefault((string s) => tags.All((NodeDataTag t) => t != s)) ?? "New Tag", graphTags);
		uiField.filter = tags;
	}

	private void OnValidateUI()
	{
		foreach (NodeDataTag tag in tags)
		{
			tag.graphTags = graphTags;
			graphTags.tags.Add(tag);
		}
	}
}
