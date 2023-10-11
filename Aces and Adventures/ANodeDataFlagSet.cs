using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Flag Set", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1001u)]
[YouTubeVideo("Flag Set Node Tutorial", "p6p7Ybvmw6s", -1, -1)]
[ProtoInclude(1000, typeof(NodeDataFlagSet))]
[ProtoInclude(1001, typeof(NodeDataPersistedFlagSet))]
public abstract class ANodeDataFlagSet : NodeData
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	protected NodeDataFlagWithEdit _flag;

	[ProtoMember(2)]
	[UIField]
	protected FlagSetType _function;

	[ProtoMember(4)]
	[UIField(validateOnChange = true)]
	protected FlagValueType _valueType;

	[ProtoMember(3)]
	[UIField(null, 0u, null, null, null, null, null, null, false, new string[] { "Value Max" }, 5, false, null, min = -999, max = 999, onValueChangedMethod = "_OnValueChange")]
	[DefaultValue(1)]
	[UIHideIf("_hideValue")]
	protected short _value = 1;

	[ProtoMember(8)]
	[UIField(null, 0u, null, null, null, null, null, null, false, new string[] { "Value" }, 5, false, null, min = -999, max = 999, onValueChangedMethod = "_OnValueMaxChange")]
	[DefaultValue(2)]
	[UIHideIf("_hideValueMax")]
	protected short _valueMax = 2;

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

	[ProtoMember(9)]
	[UIField(min = -100, max = 100)]
	[UIHideIf("_hideValueFlag")]
	protected sbyte _valueOffset;

	private Stack<int> _preprocessValues;

	private int _warpCount;

	private short? _rangeValue;

	private NodeDataFlagWithEdit flag => _flag ?? (_flag = new NodeDataFlagWithEdit());

	private NodeDataFlagWithEdit valueFlag => _valueFlag ?? (_valueFlag = new NodeDataFlagWithEdit());

	private string _valueString => _valueType switch
	{
		FlagValueType.Constant => _value.ToString(), 
		FlagValueType.Variable => ((Mathf.Abs(_valueMultiplier) == 1f) ? "" : (MathUtil.ToPercentInt(Mathf.Abs(_valueMultiplier)) + "% ")) + valueFlag.flag.flag + ((_valueOffset != 0) ? string.Format(" {0} {1}", (_valueOffset >= 0) ? "+" : "-", Math.Abs(_valueOffset)) : ""), 
		FlagValueType.Range => StringUtil.RangeToString(_value, _valueMax), 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private int _valueSign
	{
		get
		{
			if (_valueType == FlagValueType.Variable)
			{
				return Math.Sign(_valueMultiplier);
			}
			return Math.Sign(_value);
		}
	}

	public string flagName => flag.flag.flag;

	private Stack<int> preprocessValues => _preprocessValues ?? (_preprocessValues = new Stack<int>());

	public override string name
	{
		get
		{
			if (_function != 0)
			{
				return $"{flag} = <nobr>{_valueString}</nobr>";
			}
			return string.Format("{0} <nobr>{1}{2}</nobr>", flag, (_valueSign >= 0) ? "+" : (_hideValueFlag ? "" : "-"), _valueString);
		}
		set
		{
		}
	}

	public override string contextPath => "Advanced/Flags/";

	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.S);

	public override NodeDataIconType iconType => NodeDataIconType.FlagSet;

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
			valueFlag.nodeGraphFlags = value.GetNodeGraphFlags(_valueFlagType);
		}
	}

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

	public override Type activeOutputType
	{
		get
		{
			if (!HasOutput(Type.False, checkIfConnectionExists: true))
			{
				return Type.True;
			}
			return Type.False;
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

	protected bool _hideValue => _valueType == FlagValueType.Variable;

	protected bool _hideValueMax => _valueType != FlagValueType.Range;

	protected bool _hideValueFlag => _valueType != FlagValueType.Variable;

	private bool _flagSpecified => _flag;

	private bool _valueFlagSpecified => _valueFlag;

	private int _GetValue(StringIntFlags flags, System.Random random)
	{
		switch (_valueType)
		{
		case FlagValueType.Constant:
			return _value;
		case FlagValueType.Variable:
			return Mathf.RoundToInt((float)((_valueFlagType == flagType) ? flags : graph.GetFlags(_valueFlagType)).GetFlagValue(valueFlag) * _valueMultiplier) + _valueOffset;
		case FlagValueType.Range:
		{
			short? rangeValue = _rangeValue;
			if (!rangeValue.HasValue)
			{
				short? num = (_rangeValue = (short)random.RangeInt(_value, _valueMax));
				return num.Value;
			}
			return rangeValue.GetValueOrDefault();
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void _Process()
	{
		preprocessValues.Push(_flags.GetFlagValue(flag));
		AdjustFlagValue(_flags, base.random);
	}

	private void _UndoProcess()
	{
		_flags.SetFlagValue(flag, preprocessValues.Pop(), signalChange: false);
	}

	public override void Preprocess()
	{
		_warpCount = 0;
		RemoveOutputConnections(Type.False);
		foreach (NodeDataEventWarpExit registeredEventNode in graph.GetRegisteredEventNodes(flag.flag))
		{
			registeredEventNode.isValid = false;
			_Process();
			bool isValid = registeredEventNode.isValid;
			_UndoProcess();
			if (!isValid)
			{
				continue;
			}
			foreach (NodeDataConnection outputConnection in registeredEventNode.GetOutputConnections(Type.True))
			{
				graph.AddConnection(new NodeDataConnection(this, outputConnection.inputNodeId, Type.False));
			}
			graph.AddWarpEnterId(this);
			_warpCount++;
		}
		_Process();
	}

	public override void UndoPreprocess()
	{
		_UndoProcess();
		for (int i = 0; i < _warpCount; i++)
		{
			graph.RemoveLastWarpEnterId();
		}
		_warpCount = 0;
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
		_rangeValue = null;
		preprocessValues.Pop();
	}

	public override void PrepareForSave()
	{
		if (_hideValueFlag)
		{
			_valueFlag = null;
			_valueMultiplier = 1f;
			_valueOffset = 0;
			_valueFlagType = NodeDataFlagType.Standard;
			if (_value == _valueMax)
			{
				_valueType = FlagValueType.Constant;
			}
			if (_hideValueMax)
			{
				_valueMax = 2;
			}
		}
		else
		{
			_value = 1;
			_valueMax = 2;
		}
	}

	public ANodeDataFlagSet SetData(NodeDataFlagWithEdit flag, FlagSetType function, FlagValueType valueType, short value, NodeDataFlagType valueFlagType, NodeDataFlagWithEdit valueFlag, float valueMultiplier, sbyte valueOffset)
	{
		_flag = flag;
		_function = function;
		_valueType = valueType;
		_value = value;
		_valueMax = value;
		_valueFlagType = valueFlagType;
		_valueFlag = valueFlag;
		_valueMultiplier = valueMultiplier;
		_valueOffset = valueOffset;
		return this;
	}

	public void AdjustFlagValue(StringIntFlags flags, System.Random random)
	{
		flags.AdjustFlagValue(flag, _GetValue(flags, random), _function);
	}

	protected void OnValidateUI()
	{
		flag.OnValidate();
		if (!_hideValueFlag)
		{
			valueFlag.SetNodeGraphFlags(graph.GetNodeGraphFlagsSafe(ref _valueFlagType)).OnValidate();
		}
		if (!_hideValueMax)
		{
			_OnValueChange();
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

	protected void _OnValueFlagTypeChange()
	{
		valueFlag.Clear();
		if (graph != null)
		{
			valueFlag.nodeGraphFlags = graph.GetNodeGraphFlags(_valueFlagType);
		}
	}

	protected void _OnValueChange()
	{
		_valueMax = Math.Max(_value, _valueMax);
	}

	protected void _OnValueMaxChange()
	{
		_value = Math.Min(_value, _valueMax);
	}
}
