using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Event Warp Exit", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1752u)]
public class NodeDataEventWarpExit : ANodeDataWarp
{
	public enum EventType : byte
	{
		FallsBelow,
		RisesAbove,
		Equals,
		Incremented,
		Decremented
	}

	[ProtoMember(1)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnFlagTypeChange")]
	protected NodeDataFlagType _flagType;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	protected NodeDataFlagWithEdit _flag;

	[ProtoMember(3)]
	[UIField(validateOnChange = true)]
	protected EventType _triggerWhenValue;

	[ProtoMember(4)]
	[UIField(validateOnChange = true)]
	protected FlagValueType _valueType;

	[ProtoMember(5)]
	[UIField(min = -9999, max = 9999)]
	[UIHideIf("_hideValue")]
	protected int _value;

	[ProtoMember(6)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnValueFlagTypeChange")]
	[UIHideIf("_hideValueFlag")]
	protected NodeDataFlagType _valueFlagType;

	[ProtoMember(7)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideValueFlag")]
	protected NodeDataFlagWithEdit _valueFlag;

	[ProtoMember(8)]
	[UIField(min = -10, max = 10, stepSize = 0.01f)]
	[DefaultValue(1)]
	[UIHideIf("_hideValueFlag")]
	protected float _valueMultiplier = 1f;

	[ProtoMember(9)]
	[UIField(min = -100, max = 100)]
	[UIHideIf("_hideValueFlag")]
	protected sbyte _valueOffset;

	[ProtoMember(10)]
	[UIField(onValueChangedMethod = "_OnDisableFalseOutputChange")]
	private bool _disableFalseOutput;

	[ProtoMember(15)]
	private bool _registered;

	private bool _isValid;

	public NodeDataFlagWithEdit flag => _flag ?? (_flag = new NodeDataFlagWithEdit());

	public NodeDataFlagWithEdit valueFlag => _valueFlag ?? (_valueFlag = new NodeDataFlagWithEdit());

	public bool isValid
	{
		get
		{
			return _isValid;
		}
		set
		{
			_isValid = value;
		}
	}

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

	public NodeDataFlagType flagType => _flagType;

	public string flagName => flag.flag.flag;

	public override string name
	{
		get
		{
			return string.Concat(flag, TextBuilder.SMALL_SIZE_TAG, "\n", EnumUtil.FriendlyName(_triggerWhenValue), (_hideValue && _hideValueFlag) ? "" : (" " + _valueString));
		}
		set
		{
		}
	}

	public override string searchName => string.Concat(flag, " ", EnumUtil.FriendlyName(_triggerWhenValue), (_hideValue && _hideValueFlag) ? "" : (" " + _valueString));

	public override NodeDataIconType iconType => NodeDataIconType.EventWarpExit;

	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.E);

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

	public override IEnumerable<Couple<string, NodeDataFlagType>> usedFlags
	{
		get
		{
			if ((bool)flag)
			{
				yield return new Couple<string, NodeDataFlagType>(flag, _flagType);
			}
			if (!_hideValueFlag && (bool)valueFlag && flag.flag != valueFlag.flag)
			{
				yield return new Couple<string, NodeDataFlagType>(valueFlag, _valueFlagType);
			}
		}
	}

	public override NodeGraph graph
	{
		get
		{
			return base.graph;
		}
		set
		{
			base.graph = value;
			flag.nodeGraphFlags = value.GetNodeGraphFlags(_flagType);
			valueFlag.nodeGraphFlags = value.GetNodeGraphFlags(_valueFlagType);
			if (_registered)
			{
				_Register();
			}
		}
	}

	private bool _hideValue
	{
		get
		{
			if ((int)_triggerWhenValue < 3)
			{
				return _valueType != FlagValueType.Constant;
			}
			return true;
		}
	}

	private bool _hideValueFlag
	{
		get
		{
			if ((int)_triggerWhenValue < 3)
			{
				return _valueType == FlagValueType.Constant;
			}
			return true;
		}
	}

	private void _SetRegistered()
	{
		_registered = true;
		_Register();
	}

	private void _Register()
	{
		if (graph.registeredEventIds.Add(this))
		{
			graph.GetFlags(_flagType).onValueChange += _OnValueChange;
		}
	}

	private void _OnValueChange(string flag, int previousValue, int newValue)
	{
		if (!isValid)
		{
			if (_valueType == FlagValueType.Variable)
			{
				_value = Mathf.RoundToInt((float)graph.GetFlags(_valueFlagType).GetFlagValueSafe(valueFlag) * _valueMultiplier) + _valueOffset;
			}
			switch (_triggerWhenValue)
			{
			case EventType.FallsBelow:
				_isValid = previousValue >= _value && newValue < _value;
				break;
			case EventType.RisesAbove:
				_isValid = previousValue <= _value && newValue > _value;
				break;
			case EventType.Equals:
				_isValid = previousValue != _value && newValue == _value;
				break;
			case EventType.Incremented:
				_isValid = newValue > previousValue;
				break;
			case EventType.Decremented:
				_isValid = newValue < previousValue;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public override void OnGenerate()
	{
		if (!base.hasInput)
		{
			_SetRegistered();
		}
	}

	public override void Preprocess()
	{
		_SetRegistered();
	}

	public override void PrepareForSave()
	{
		if (_hideValue)
		{
			_value = 0;
		}
		if (_hideValueFlag)
		{
			_valueFlagType = NodeDataFlagType.Standard;
			_valueFlag = null;
			_valueMultiplier = 1f;
			_valueOffset = 0;
		}
	}

	public bool CanBeTriggeredBy(ANodeDataFlagSet flagSet)
	{
		if (flagType == flagSet.flagType)
		{
			return flagName == flagSet.flagName;
		}
		return false;
	}

	private void OnValidateUI()
	{
		flag.OnValidate();
		if (!_hideValueFlag)
		{
			valueFlag.SetNodeGraphFlags(graph.GetNodeGraphFlagsSafe(ref _valueFlagType)).OnValidate();
		}
	}

	protected void _OnFlagTypeChange()
	{
		flag.Clear();
		if (graph != null)
		{
			flag.nodeGraphFlags = graph.GetNodeGraphFlags(_flagType);
		}
	}

	protected void _OnValueFlagTypeChange()
	{
		valueFlag.Clear();
		if (graph != null)
		{
			valueFlag.nodeGraphFlags = graph.GetNodeGraphFlags(_valueFlagType);
		}
	}

	private void _OnDisableFalseOutputChange()
	{
		_OnOutputTypeChange();
	}
}
