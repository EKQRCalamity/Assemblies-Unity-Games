using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Random", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1100u)]
[YouTubeVideo("Random Node Tutorial", "3LBDcCqmM-0", -1, -1)]
public class NodeDataRandom : NodeData
{
	public enum ChanceType : byte
	{
		Constant,
		Variable
	}

	[ProtoContract(EnumPassthru = true)]
	public enum NoRepeatType : byte
	{
		[UITooltip("Items may be selected again without restriction.")]
		Disabled,
		[UITooltip("Once an item has been selected, it will never be selected again.\nOnce all items have been selected, the output of this node will become false.")]
		Ever,
		[UITooltip("Once an item has been selected, all other items must be selected before it can be selected again.")]
		UntilAllHaveBeenSelected,
		[UITooltip("Once an item has been selected, all other valid items must be selected before it can be selected again.\n<i>Used in conjunction with <b>evaluate branches before picking</b> option.</i>")]
		UntilNoEvaluatedBranchRemains
	}

	private class OriginConnectionIndicesComparer : IComparer<NodeDataConnection>
	{
		public static OriginConnectionIndicesComparer Default = new OriginConnectionIndicesComparer();

		public int Compare(NodeDataConnection x, NodeDataConnection y)
		{
			return OriginalConnectionIndices[x] - OriginalConnectionIndices[y];
		}
	}

	private static readonly RangeByte DEFAULT_PICK_AMOUNT = new RangeByte(1, 1, 1, 10, 0, 0);

	private static Dictionary<uint, int> _OriginalConnectionIndices;

	[ProtoMember(1)]
	[UIField]
	private RangeByte _pick = DEFAULT_PICK_AMOUNT;

	[ProtoMember(2)]
	[UIField(validateOnChange = true)]
	private RandomizationType _randomization;

	[ProtoMember(7)]
	[UIField(validateOnChange = true, excludedValuesMethod = "_ExcludeChanceType")]
	[UIMargin(24f, false)]
	private ChanceType _chanceType;

	[ProtoMember(3)]
	[UIField(min = 1, max = 100)]
	[DefaultValue(100)]
	[UIHideIf("_hideChance")]
	private byte _chance = 100;

	[ProtoMember(8)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnFlagTypeChange")]
	[UIHideIf("_hideFlag")]
	private NodeDataFlagType _chanceFlagType;

	[ProtoMember(9)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideFlag")]
	private NodeDataFlagWithEdit _chanceFlag;

	[ProtoMember(10)]
	[UIField(min = -10, max = 10)]
	[DefaultValue(1)]
	[UIHideIf("_hideFlag")]
	private float _chanceFlagMultiplier = 1f;

	[ProtoMember(4)]
	[UIField(onValueChangedMethod = "_OnEnableFalseOutputChange")]
	[UIMargin(24f, false)]
	private bool _enableFalseOutput;

	[ProtoMember(5)]
	[UIField(validateOnChange = true)]
	[UIHideIf("_hideEvaluateBranches")]
	private bool _evaluateBranchesBeforePicking;

	[ProtoMember(6)]
	[UIField(tooltip = "If enabled, insures the same item is not selected more than once until all other possibilities have been selected.", excludedValuesMethod = "_ExcludeNoRepeats")]
	[UIHideIf("_hideNoRepeats")]
	private NoRepeatType _noRepeats;

	[ProtoMember(15, OverwriteList = true)]
	private List<NodeDataConnection> _originalOutputs;

	[ProtoMember(16, OverwriteList = true)]
	private List<NodeDataConnection> _noRepeatOriginalOutputs;

	private static Dictionary<uint, int> OriginalConnectionIndices => _OriginalConnectionIndices ?? (_OriginalConnectionIndices = new Dictionary<uint, int>());

	[ProtoMember(13)]
	private byte _lastPickCount { get; set; }

	[ProtoMember(14)]
	private bool _evaluationDirty { get; set; }

	private float chance => ((_chanceType == ChanceType.Constant) ? ((float)(int)_chance) : ((float)graph.GetFlags(_chanceFlagType).GetFlagValue(_chanceFlag) * _chanceFlagMultiplier)) * 0.01f;

	private NodeDataFlagWithEdit chanceFlag => _chanceFlag ?? (_chanceFlag = new NodeDataFlagWithEdit());

	private bool evaluateBranchesBeforePicking
	{
		get
		{
			if (_evaluateBranchesBeforePicking)
			{
				return !_hideEvaluateBranches;
			}
			return false;
		}
	}

	private bool noRepeats
	{
		get
		{
			if (_noRepeats != 0)
			{
				return !_hideNoRepeats;
			}
			return false;
		}
	}

	private string _chanceString
	{
		get
		{
			if (_chanceType != 0)
			{
				return "<nobr>" + ((_chanceFlagMultiplier == 1f) ? string.Concat(chanceFlag, "%") : $"({chanceFlag} x {_chanceFlagMultiplier})%") + "</nobr> ";
			}
			if (_chance >= 100)
			{
				return "";
			}
			return _chance + "% ";
		}
	}

	private bool isStaticEvaluated
	{
		get
		{
			if (evaluateBranchesBeforePicking && _randomization != RandomizationType.DynamicEvenWhenNotExecuted && _pick.max > 0)
			{
				return _pick.min < _originalOutputs.Count;
			}
			return false;
		}
	}

	public override string name
	{
		get
		{
			return _Name(addHeader: true);
		}
		set
		{
		}
	}

	public override string searchName => _Name();

	public override NodeGraph graph
	{
		get
		{
			return base.graph;
		}
		set
		{
			base.graph = value;
			value.SetGraph(ref _chanceFlag, _chanceFlagType);
		}
	}

	public override RandomizationType? randomization => _randomization;

	public override bool shouldCheckForBranchLinearization => false;

	public override Type[] output
	{
		get
		{
			if (!_enableFalseOutput)
			{
				return base.output;
			}
			return NodeData.BOOL_OUTPUT;
		}
	}

	public override string contextPath => "Advanced/";

	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.R);

	public override NodeDataIconType iconType => NodeDataIconType.Random;

	protected override bool _hideNameInInspector => true;

	private bool _hideEvaluateBranches => !_randomization.IsDynamic();

	private bool _hideNoRepeats => !_randomization.IsDynamic();

	private bool _hideChance => _chanceType != ChanceType.Constant;

	private bool _hideFlag => _chanceType != ChanceType.Variable;

	private bool _pickSpecified => _pick != DEFAULT_PICK_AMOUNT;

	private string _GetNameHeader()
	{
		if (!evaluateBranchesBeforePicking && !noRepeats)
		{
			return "";
		}
		return string.Format("<nobr><size=66%>{0}{1}</nobr>\n</size>", evaluateBranchesBeforePicking ? "Evaluate " : "", noRepeats ? (((int)_noRepeats >= 2).ToText("*") + "No Repeat") : "");
	}

	private string _Name(bool addHeader = false)
	{
		return string.Format("{3}{2}{1}<nobr>Pick {0}</nobr>", (_pick.max == 0) ? "All" : _pick.ToRangeString(), _randomization.NamePrefix(), _chanceString, addHeader ? _GetNameHeader() : "");
	}

	private void _CacheOriginalOutputs()
	{
		_originalOutputs = _originalOutputs ?? new List<NodeDataConnection>(GetOutputConnections().AsEnumerable());
		if (!noRepeats)
		{
			return;
		}
		List<NodeDataConnection> list = _noRepeatOriginalOutputs;
		if (list == null)
		{
			IEnumerable<NodeDataConnection> collection;
			if (_noRepeats != NoRepeatType.Ever)
			{
				IEnumerable<NodeDataConnection> originalOutputs = _originalOutputs;
				collection = originalOutputs;
			}
			else
			{
				collection = _originalOutputs.Where((NodeDataConnection c) => c.outputType == Type.False);
			}
			list = new List<NodeDataConnection>(collection);
		}
		_noRepeatOriginalOutputs = list;
	}

	private PoolKeepItemHashSetHandle<NodeDataConnection> _EvaluateBranches(bool removeInvalidOutputs, bool reevaluateIfNeeded = true)
	{
		PoolKeepItemHashSetHandle<NodeDataConnection> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeDataConnection>();
		foreach (KeyValuePair<NodeData, PoolKeepItemHashSetHandle<NodeData>> processedOutputBranch in GetProcessedOutputBranches(NodeData._ProcessStopBranchingAt, checkSelf: false, doProcesses: true, UndoProcessType.All, processLeaves: false, NodeData._ProcessIsValidBranch))
		{
			if (processedOutputBranch.Key.isExecutionStoppingPoint || !processedOutputBranch.Key.hasOutput)
			{
				poolKeepItemHashSetHandle.Add((processedOutputBranch.Value.value.FirstOrDefault() ?? processedOutputBranch.Key).GetConnectionTo(this, IOType.Input, activeOutputType));
			}
		}
		if (removeInvalidOutputs)
		{
			foreach (NodeDataConnection outputConnection in GetOutputConnections(activeOutputType))
			{
				if (!poolKeepItemHashSetHandle.Contains(outputConnection))
				{
					graph.RemoveConnection(outputConnection);
				}
			}
			if (poolKeepItemHashSetHandle.Count == 0 && _noRepeats == NoRepeatType.UntilNoEvaluatedBranchRemains && reevaluateIfNeeded)
			{
				_originalOutputs.CopyFrom(_noRepeatOriginalOutputs);
				_RestoreOriginalOutputs(clearCompletionType: false);
				return _EvaluateBranches(removeInvalidOutputs: true, reevaluateIfNeeded: false);
			}
		}
		return poolKeepItemHashSetHandle;
	}

	private void _ReevaluateBranches()
	{
		_evaluationDirty = false;
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetOutputConnections(activeOutputType);
		_AddOriginalOutputConnections();
		using PoolKeepItemHashSetHandle<NodeDataConnection> poolKeepItemHashSetHandle = _EvaluateBranches(removeInvalidOutputs: false);
		using PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle2 = Pools.UseKeepItemList<NodeDataConnection>();
		foreach (NodeDataConnection item2 in poolKeepItemListHandle.value)
		{
			if (poolKeepItemHashSetHandle.Contains(item2))
			{
				poolKeepItemListHandle2.Add(item2);
				poolKeepItemHashSetHandle.Remove(item2);
			}
		}
		for (int i = 0; i < _originalOutputs.Count; i++)
		{
			OriginalConnectionIndices.Add(_originalOutputs[i], i);
		}
		for (int j = poolKeepItemListHandle2.Count; j < _lastPickCount; j++)
		{
			if (poolKeepItemHashSetHandle.Count == 0)
			{
				break;
			}
			NodeDataConnection item = base.random.Item(poolKeepItemHashSetHandle.value);
			poolKeepItemHashSetHandle.value.Remove(item);
			poolKeepItemListHandle2.value.AddSorted(item, OriginConnectionIndicesComparer.Default);
		}
		OriginalConnectionIndices.Clear();
		RemoveOutputConnections(activeOutputType);
		foreach (NodeDataConnection item3 in poolKeepItemListHandle2.value)
		{
			graph.AddConnection(item3, item3);
		}
		if (poolKeepItemListHandle2.Count == 0)
		{
			base.completionType = Type.False;
		}
	}

	private void _RandomizeOutputs()
	{
		base.completionType = base.random.Chance(chance).ToNodeOutput();
		if (evaluateBranchesBeforePicking)
		{
			using (_EvaluateBranches(removeInvalidOutputs: true))
			{
			}
		}
		using (PoolKeepItemListHandle<NodeDataConnection> poolKeepItemListHandle = GetOutputConnections(base.completionType.Value))
		{
			if (poolKeepItemListHandle.Count == 0)
			{
				base.completionType = Type.False;
			}
			int num = base.random.RangeInt(_pick);
			if (isStaticEvaluated)
			{
				_lastPickCount = (byte)num;
			}
			int num2 = ((_pick.max != 0) ? (poolKeepItemListHandle.Count - Mathf.Min(poolKeepItemListHandle.Count, num)) : 0);
			for (int i = 0; i < num2; i++)
			{
				int index = base.random.Next(0, poolKeepItemListHandle.Count);
				if (_randomization == RandomizationType.OneTime)
				{
					graph.RemoveConnectionDeep(poolKeepItemListHandle[index]);
				}
				else
				{
					graph.RemoveConnection(poolKeepItemListHandle[index]);
				}
				poolKeepItemListHandle.RemoveAt(index);
			}
		}
		RemoveOutputConnections(base.completionType.Value.Opposite(), _randomization == RandomizationType.OneTime);
	}

	private void _AddOriginalOutputConnections()
	{
		foreach (NodeDataConnection originalOutput in _originalOutputs)
		{
			graph.AddConnection(originalOutput, originalOutput);
		}
	}

	private void _RestoreOriginalOutputs(bool clearCompletionType = true)
	{
		_AddOriginalOutputConnections();
		if (clearCompletionType)
		{
			base.completionType = null;
		}
		_evaluationDirty = false;
	}

	private void _OnProcessExecuted(Dictionary<NodeData, PoolKeepItemHashSetHandle<NodeData>> executedBranches)
	{
		if (noRepeats || isStaticEvaluated)
		{
			using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<NodeData>();
			foreach (KeyValuePair<NodeData, PoolKeepItemHashSetHandle<NodeData>> executedBranch in executedBranches)
			{
				poolKeepItemHashSetHandle.AddReturn(executedBranch.Key).CopyFrom(executedBranch.Value.value);
			}
			if (noRepeats)
			{
				using PoolKeepItemHashSetHandle<NodeData> poolKeepItemHashSetHandle2 = executedBranches.Flatten();
				foreach (NodeData outputNode in GetOutputNodes(activeOutputType))
				{
					if (!poolKeepItemHashSetHandle2.Contains(outputNode))
					{
						continue;
					}
					NodeDataConnection connectionTo = GetConnectionTo(outputNode, IOType.Output, activeOutputType);
					for (int num = _originalOutputs.Count - 1; num >= 0; num--)
					{
						if (_originalOutputs[num].IsRedundantWith(connectionTo, checkGraphEquality: false))
						{
							_originalOutputs.RemoveAt(num);
						}
					}
					graph.RemoveConnection(connectionTo);
				}
				if (_originalOutputs.Count == 0)
				{
					_originalOutputs.CopyFrom(_noRepeatOriginalOutputs);
					if (_noRepeats == NoRepeatType.Ever)
					{
						_chance = 0;
					}
					else
					{
						_RestoreOriginalOutputs();
					}
				}
			}
			if (isStaticEvaluated && !poolKeepItemHashSetHandle.Contains(this))
			{
				_evaluationDirty = true;
			}
		}
		if (_randomization == RandomizationType.DynamicEvenWhenNotExecuted)
		{
			_RestoreOriginalOutputs();
		}
		graph.onProcessExecuted -= _OnProcessExecuted;
	}

	protected override void _GetSearchText(TextBuilder builder)
	{
		if (evaluateBranchesBeforePicking)
		{
			builder.Append("Evaluate").Space();
		}
		if (noRepeats)
		{
			builder.Append("No Repeat").Space();
		}
	}

	public override void PrepareForSave()
	{
		if (_hideEvaluateBranches)
		{
			_evaluateBranchesBeforePicking = false;
		}
		if (_hideNoRepeats)
		{
			_noRepeats = NoRepeatType.Disabled;
		}
		if (_hideFlag)
		{
			_chanceFlagType = EnumUtil<NodeDataFlagType>.Min;
			_chanceFlag = null;
			_chanceFlagMultiplier = 1f;
		}
		else
		{
			_chance = 100;
		}
	}

	public override void Preprocess()
	{
		graph.onProcessExecuted -= _OnProcessExecuted;
		graph.onProcessExecuted += _OnProcessExecuted;
		if (!base.completionType.HasValue || _evaluationDirty)
		{
			_CacheOriginalOutputs();
			if (_evaluationDirty)
			{
				_ReevaluateBranches();
			}
			else
			{
				_RandomizeOutputs();
			}
		}
	}

	public override void Execute()
	{
		if (_randomization == RandomizationType.Dynamic)
		{
			_RestoreOriginalOutputs();
		}
	}

	public void ExecuteRandomNode()
	{
		if (!_randomization.IsDynamic())
		{
			_RandomizeOutputs();
			RemoveNodeAndPatchConnections();
		}
	}

	private void _OnEnableFalseOutputChange()
	{
		_OnOutputTypeChange();
	}

	private void _OnFlagTypeChange()
	{
		chanceFlag.Clear();
		if (graph != null)
		{
			chanceFlag.nodeGraphFlags = graph.GetNodeGraphFlags(_chanceFlagType);
		}
	}

	private bool _ExcludeChanceType(ChanceType chanceType)
	{
		if (!_randomization.IsDynamic())
		{
			return chanceType == ChanceType.Variable;
		}
		return false;
	}

	private bool _ExcludeNoRepeats(NoRepeatType noRepeatType)
	{
		if (noRepeatType == NoRepeatType.UntilNoEvaluatedBranchRemains)
		{
			return !_evaluateBranchesBeforePicking;
		}
		return false;
	}

	private void OnValidateUI()
	{
		_pick = _pick.SetMinRange((byte)evaluateBranchesBeforePicking.ToInt(0, 1));
		chanceFlag.OnValidate();
	}

	public override void SetGraphOnDeserialize(NodeGraph graphToSet)
	{
		base.SetGraphOnDeserialize(graphToSet);
		if (base.completionType.HasValue)
		{
			_RestoreOriginalOutputs();
		}
	}
}
