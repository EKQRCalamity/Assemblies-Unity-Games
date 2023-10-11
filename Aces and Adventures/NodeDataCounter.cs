using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Loop", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1100u)]
[YouTubeVideo("Loop Node Tutorial", "3O8fX7FoViI", -1, -1)]
public class NodeDataCounter : NodeData
{
	[ProtoMember(1)]
	[UIField(min = 1, max = 100)]
	[DefaultValue(1)]
	private byte _count = 1;

	[ProtoMember(2)]
	[UIField(onValueChangedMethod = "_OnDisableFalseOutputChange")]
	protected bool _disableFalseOutput;

	protected override bool _hideNameInInspector => true;

	public override string name
	{
		get
		{
			return "Loop: " + _count;
		}
		set
		{
		}
	}

	public override string contextPath => "Advanced/Flags/";

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.Alpha1);

	public override NodeDataIconType iconType => NodeDataIconType.Counter;

	public override Type[] output
	{
		get
		{
			if (_disableFalseOutput)
			{
				return base.output;
			}
			return NodeData.BOOL_OUTPUT;
		}
	}

	public override Type activeOutputType
	{
		get
		{
			if (_count <= 0)
			{
				return Type.False;
			}
			return Type.True;
		}
	}

	public override void Execute()
	{
		if (_count > 0)
		{
			_count--;
		}
	}

	protected void _OnDisableFalseOutputChange()
	{
		_OnOutputTypeChange();
	}
}
