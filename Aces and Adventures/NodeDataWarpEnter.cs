using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Warp Enter", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1750u)]
[YouTubeVideo("Warp Enter Node Tutorial", "tTXFWfZFZwo", -1, -1)]
public class NodeDataWarpEnter : ANodeDataWarp
{
	public enum WarpTarget : byte
	{
		Previous,
		Next
	}

	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	private bool _returnToLastWarpEnter;

	[ProtoMember(2)]
	[UIField(tooltip = "Determines if the warp destination should be searched for backwards or forwards from this node in the graph.\n<i>For the case where several exits with the same identifier tags exist.</i>")]
	[UIHideIf("_hideIdentifiers")]
	private WarpTarget _warpTarget;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideIdentifiers")]
	protected NodeDataTags _identifiers;

	private uint _validWarpId;

	public bool returnToLastWarpEnter => _returnToLastWarpEnter;

	public override bool isExecutionStoppingPoint => true;

	public override NodeDataTags identifiers => _identifiers ?? (_identifiers = new NodeDataTags());

	public override string name
	{
		get
		{
			if (!_returnToLastWarpEnter)
			{
				return TextBuilder.SMALL_SIZE_TAG + "Go To\n</size>" + base.name;
			}
			return "[Return]";
		}
		set
		{
			base.name = value;
		}
	}

	public override string searchName
	{
		get
		{
			if (!_returnToLastWarpEnter)
			{
				return "Go To " + base.name;
			}
			return "Return";
		}
	}

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.W);

	public override NodeDataIconType iconType => NodeDataIconType.WarpEnter;

	public override Type[] output
	{
		get
		{
			if (!base.generated)
			{
				return base.output;
			}
			return NodeData.BOOL_OUTPUT;
		}
	}

	protected bool _hideIdentifiers => _returnToLastWarpEnter;

	protected override void _GetSearchText(TextBuilder builder)
	{
		if (identifiers.Count > 1)
		{
			identifiers.AppendSearchText(builder);
		}
	}

	public override void Preprocess()
	{
		_validWarpId = 0u;
		RemoveOutputConnections(Type.False);
		NodeData nodeData;
		if (_returnToLastWarpEnter)
		{
			nodeData = graph.GetLastWarpEnterNode();
		}
		else
		{
			using (PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = identifiers.activeTags)
			{
				nodeData = graph.GetMappedWarpExit(poolKeepItemHashSetHandle);
			}
			nodeData = nodeData ?? (from exit in ((_warpTarget == WarpTarget.Previous) ? GetInputNodesByDepth() : GetOutputNodesByDepth()).Select((Couple<NodeData, int> c) => c.a).OfType<NodeDataWarpExit>()
				where exit.identifiers != null
				select exit).FirstOrDefault((NodeDataWarpExit exit) => identifiers.MatchFound(exit.identifiers, matchIfEmpty: true));
			nodeData = nodeData ?? (from exit in (from c in graph.GetRootNodes().AsEnumerable().SelectMany((NodeData n) => n.GetOutputNodesByDepth(null, includeSelf: true))
					orderby c.b
					select c.a).OfType<NodeDataWarpExit>()
				where exit.identifiers != null
				select exit).FirstOrDefault((NodeDataWarpExit exit) => identifiers.MatchFound(exit.identifiers, matchIfEmpty: true));
		}
		if ((_validWarpId = nodeData) != 0)
		{
			foreach (NodeDataConnection outputConnection in nodeData.GetOutputConnections(Type.True))
			{
				graph.AddConnection(new NodeDataConnection(this, outputConnection.inputNodeId, Type.False));
			}
		}
		else
		{
			CopyOutputConnectionsFromTypeToType(Type.True, Type.False);
		}
		if (_validWarpId != 0)
		{
			if (!_returnToLastWarpEnter)
			{
				graph.AddWarpEnterId(this);
			}
			else
			{
				graph.RemoveLastWarpEnterId();
			}
		}
	}

	public override void UndoPreprocess()
	{
		if (_validWarpId != 0)
		{
			if (!_returnToLastWarpEnter)
			{
				graph.RemoveLastWarpEnterId();
			}
			else
			{
				graph.AddWarpEnterId(_validWarpId);
			}
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
}
