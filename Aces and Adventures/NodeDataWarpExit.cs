using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Warp Exit", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1751u)]
[YouTubeVideo("Warp Exit Node Tutorial", "8iDz3kTqyTY", -1, -1)]
public class NodeDataWarpExit : ANodeDataWarp
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	protected NodeDataTags _identifiers;

	[ProtoMember(2)]
	[UIField(onValueChangedMethod = "_OnDisableFalseOutputChange", order = 1u)]
	private bool _disableFalseOutput;

	public override NodeDataTags identifiers => _identifiers ?? (_identifiers = new NodeDataTags());

	public override string name
	{
		get
		{
			return TextBuilder.SMALL_SIZE_TAG + "Arrive At\n</size>" + base.name;
		}
		set
		{
			base.name = value;
		}
	}

	public override string searchName => "Arrive At " + base.name;

	public override NodeDataIconType iconType => NodeDataIconType.WarpExit;

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.E);

	public override Type[] output
	{
		get
		{
			if (!_disableFalseOutput || base.generated)
			{
				return NodeData.BOOL_OUTPUT;
			}
			return base.output;
		}
	}

	protected override void _GetSearchText(TextBuilder builder)
	{
		if (identifiers.Count > 1)
		{
			identifiers.AppendSearchText(builder);
		}
	}

	public override void OnGenerate()
	{
		if (!identifiers.useMappedTags)
		{
			graph.AddWarpExitMap(this);
		}
	}

	private void _OnDisableFalseOutputChange()
	{
		_OnOutputTypeChange();
	}
}
