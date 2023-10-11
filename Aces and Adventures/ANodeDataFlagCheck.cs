using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Flag Check", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1010u)]
[YouTubeVideo("Flag Check Node Tutorial", "DMGerx4EWnM", -1, -1)]
[ProtoInclude(1000, typeof(NodeDataFlagCheck))]
[ProtoInclude(1001, typeof(NodeDataPersistedFlagCheck))]
public abstract class ANodeDataFlagCheck : NodeData
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	protected NodeDataFlagWithEdit _flag;

	[ProtoMember(2)]
	[UIField]
	[UIHideIf("_hideIf")]
	protected FlagCheckType _if;

	[ProtoMember(4)]
	[UIField(validateOnChange = true, excludedValuesMethod = "_ExcludeValueType")]
	protected FlagValueType _valueType;

	[ProtoMember(3)]
	[UIField(min = -999, max = 999)]
	[UIHideIf("_hideValue")]
	protected short _value;

	[ProtoMember(5)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnValueFlagTypeChange", excludedValuesMethod = "_ExcludeValueFlagType")]
	[UIHideIf("_hideValueFlag")]
	protected NodeDataFlagType _valueFlagType;

	[ProtoMember(6)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideValueFlag")]
	protected NodeDataFlagWithEdit _valueFlag;

	[ProtoMember(7)]
	[UIField(min = -10, max = 10, stepSize = 0.01f)]
	[DefaultValue(1)]
	[UIHideIf("_hideValueFlag")]
	protected float _valueMultiplier = 1f;

	[ProtoMember(8)]
	[UIField(min = -100, max = 100)]
	[UIHideIf("_hideValueFlag")]
	protected sbyte _valueOffset;

	[ProtoMember(9)]
	[UIField(onValueChangedMethod = "_OnDisableFalseOutputChange")]
	[UIMargin(24f, false)]
	protected bool _disableFalseOutput;

	[ProtoMember(10)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnConsumeValueIfTrueChange")]
	protected bool _consumeValueIfTrue;

	private NodeDataFlagWithEdit flag => _flag ?? (_flag = new NodeDataFlagWithEdit());

	private NodeDataFlagWithEdit valueFlag => _valueFlag ?? (_valueFlag = new NodeDataFlagWithEdit());

	protected string _valueString
	{
		get
		{
			if (_valueType != 0)
			{
				return ((Mathf.Abs(_valueMultiplier) == 1f) ? "" : (MathUtil.ToPercentInt(_valueMultiplier) + "% ")) + valueFlag.flag.flag + ((_valueOffset != 0) ? string.Format(" {0} {1}", (_valueOffset > 0) ? "+" : "-", Math.Abs(_valueOffset)) : "");
			}
			return _value.ToString();
		}
	}

	public string flagName => flag.flag.flag;

	public override string name
	{
		get
		{
			return _Name(_consumeValueIfTrue);
		}
		set
		{
		}
	}

	public override string searchName => _Name();

	public override string contextPath => "Advanced/Flags/";

	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.C);

	public override NodeDataIconType iconType => NodeDataIconType.FlagCheck;

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

	protected override bool _hideNameInInspector => true;

	public override NodeGraph graph
	{
		get
		{
			return base.graph;
		}
		set
		{
			base.graph = value;
			flag.nodeGraphFlags = _nodeGraphFlags;
		}
	}

	public override Type activeOutputType
	{
		get
		{
			if (!CheckFlag(_flags))
			{
				return Type.False;
			}
			return Type.True;
		}
	}

	public override IEnumerable<Couple<string, NodeDataFlagType>> usedFlags
	{
		get
		{
			if ((bool)flag)
			{
				yield return new Couple<string, NodeDataFlagType>(flag, flagType);
			}
			if (!_hideValueFlag && (bool)valueFlag && flag.flag != valueFlag.flag)
			{
				yield return new Couple<string, NodeDataFlagType>(valueFlag, _valueFlagType);
			}
		}
	}

	protected NodeGraphFlags _nodeGraphFlags => graph.GetNodeGraphFlags(flagType);

	protected StringIntFlags _flags => graph.GetFlags(flagType);

	public virtual NodeDataFlagType flagType => NodeDataFlagType.Standard;

	protected bool _hideValue => _valueType != FlagValueType.Constant;

	protected bool _hideValueFlag => _valueType == FlagValueType.Constant;

	protected bool _hideIf => _consumeValueIfTrue;

	private bool _flagSpecified => _flag;

	private bool _valueFlagSpecified => _valueFlag;

	private int _GetValue(StringIntFlags flags)
	{
		if (_valueType != 0)
		{
			return Mathf.RoundToInt((float)((_valueFlagType == flagType) ? flags : graph.GetFlags(_valueFlagType)).GetFlagValue(valueFlag) * _valueMultiplier) + _valueOffset;
		}
		return _value;
	}

	private string _Name(bool consume = false)
	{
		return string.Format("{3}If {0} {1} {2}", flag, _if.GetText(), _valueString, consume.ToText("<size=66%>Consume If True\n</size>"));
	}

	public override void PrepareForSave()
	{
		if (_hideValueFlag)
		{
			_valueFlag = null;
			_valueMultiplier = 1f;
			_valueFlagType = NodeDataFlagType.Standard;
			_valueOffset = 0;
		}
		else
		{
			_value = 0;
		}
	}

	public override void OnGenerate()
	{
		if (!_consumeValueIfTrue)
		{
			return;
		}
		ANodeDataFlagSet aNodeDataFlagSet = ((flagType == NodeDataFlagType.Standard) ? ((ANodeDataFlagSet)new NodeDataFlagSet()) : ((ANodeDataFlagSet)new NodeDataPersistedFlagSet()));
		graph.AddNode(aNodeDataFlagSet.SetData(_flag, FlagSetType.AddToCurrentValue, _valueType, (short)(-_value), _valueFlagType, _valueFlag, 0f - _valueMultiplier, (sbyte)(-_valueOffset)));
		using (PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetOutputConnections(Type.True))
		{
			if (poolKeepItemListHandle.Count > 0)
			{
				foreach (NodeDataConnection item in poolKeepItemListHandle.value)
				{
					graph.RouteConnection(item, aNodeDataFlagSet);
				}
			}
			else
			{
				graph.AddConnection(new NodeDataConnection(this, aNodeDataFlagSet));
			}
		}
		aNodeDataFlagSet.OnGenerate();
	}

	protected override void _GetSearchText(TextBuilder builder)
	{
		if (_consumeValueIfTrue)
		{
			builder.Append("Consume").Space();
		}
	}

	public bool CheckFlag(StringIntFlags flags)
	{
		return flags.CheckFlag(flag, _GetValue(flags), _if);
	}

	protected void OnValidateUI()
	{
		flag.OnValidate();
		if (!_hideValueFlag)
		{
			valueFlag.SetNodeGraphFlags(graph.GetNodeGraphFlagsSafe(ref _valueFlagType)).OnValidate();
		}
	}

	protected bool _ExcludeValueFlagType(NodeDataFlagType flagType)
	{
		if (_graph != null)
		{
			return _graph.GetNodeGraphFlags(flagType) == null;
		}
		return false;
	}

	protected bool _ExcludeValueType(FlagValueType valueType)
	{
		return valueType == FlagValueType.Range;
	}

	protected void _OnValueFlagTypeChange()
	{
		valueFlag.Clear();
		if (graph != null)
		{
			valueFlag.nodeGraphFlags = graph.GetNodeGraphFlags(_valueFlagType);
		}
	}

	protected void _OnDisableFalseOutputChange()
	{
		_OnOutputTypeChange();
	}

	protected void _OnConsumeValueIfTrueChange()
	{
		if (_consumeValueIfTrue)
		{
			_if = FlagCheckType.GreaterThanOrEqualTo;
		}
	}
}
