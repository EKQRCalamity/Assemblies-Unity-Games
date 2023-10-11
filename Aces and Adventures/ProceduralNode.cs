using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ProceduralNode
{
	[Flags]
	[ProtoContract(EnumPassthru = true)]
	public enum State
	{
		Inactive = 1,
		Selected = 2,
		Completed = 4,
		Selectable = 8,
		LastSelected = 0x10,
		Marked = 0x20,
		CustomHighlighted = 0x40,
		Highlighted = 0x80,
		PointerOver = 0x100
	}

	[ProtoMember(1)]
	private uint _id;

	[ProtoMember(2)]
	private Vector2 _position;

	[ProtoMember(3, OverwriteList = true)]
	private HashSet<uint> _connections;

	[ProtoMember(4)]
	[UIField(min = 0, max = 100, tooltip = "Percent chance that node type will be hidden from player.")]
	[UIHeader("Selected Node")]
	private byte _unknownChance;

	[ProtoMember(5, OverwriteList = true)]
	[UIField(maxCount = 0, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	private List<ProceduralNodePack> _packs;

	[ProtoMember(6)]
	[UIField(maxCount = 10, tooltip = "Instructions that will be executed when node is selected.\n<i>Regardless of random node data selected.</i>")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<AdventureCard.SelectInstruction> _selectInstructions;

	[ProtoMember(7)]
	[UIField(maxCount = 10, tooltip = "Instructions that will be executed once node is completed.\n<i>Regardless of random node data selected.</i>")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<AdventureCard.SelectInstruction> _completeInstructions;

	[ProtoMember(8, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<PostProcessGraphInstruction> _postProcessGraphInstructions;

	[ProtoMember(14)]
	private State _state;

	[ProtoMember(15)]
	private ProceduralNodeData _data;

	public uint id => _id;

	public bool hasValidId => id != 0;

	public Vector2 position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public HashSet<uint> connections => _connections ?? (_connections = new HashSet<uint>());

	public ProceduralNodeData data => _data;

	public bool isEndNode => _connections.IsNullOrEmpty();

	public bool hasBeenGenerated => _data != null;

	public ProceduralNodeType type => _data?.type ?? _HighestWeightNodeType() ?? ProceduralNodeType.Empty;

	public State state
	{
		get
		{
			return _state;
		}
		private set
		{
			State arg = _state;
			if (SetPropertyUtility.SetStruct(ref _state, value))
			{
				this.onStateChange?.Invoke(arg, value);
			}
		}
	}

	public bool isActive => !isInactive;

	public bool isInactive
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Inactive);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Inactive, value);
		}
	}

	public bool isSelectable
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Selectable);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Selectable, value);
		}
	}

	public bool isSelected
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Selected);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Selected, value);
		}
	}

	public bool isCompleted
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Completed);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Completed, value);
		}
	}

	public bool isLastSelected
	{
		get
		{
			return EnumUtil.HasFlag(state, State.LastSelected);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.LastSelected, value);
		}
	}

	public bool marked
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Marked);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Marked, value);
		}
	}

	public bool customHighlighted
	{
		get
		{
			return EnumUtil.HasFlag(state, State.CustomHighlighted);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.CustomHighlighted, value);
		}
	}

	public bool highlighted
	{
		get
		{
			return EnumUtil.HasFlag(state, State.Highlighted);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.Highlighted, value);
		}
	}

	public bool isPointerOver
	{
		get
		{
			return EnumUtil.HasFlag(state, State.PointerOver);
		}
		set
		{
			state = EnumUtil.SetFlag(state, State.PointerOver, value);
		}
	}

	public IEnumerable<AdventureCard.SelectInstruction> selectInstructions
	{
		get
		{
			IEnumerable<AdventureCard.SelectInstruction> enumerable = _selectInstructions;
			return enumerable ?? Enumerable.Empty<AdventureCard.SelectInstruction>();
		}
	}

	private bool _stateSpecified => LaunchManager.InGame;

	public event Action<State, State> onStateChange;

	private ProceduralNode()
	{
	}

	public ProceduralNode(uint id)
	{
		_id = id;
	}

	private ProceduralNodeData _GetSelection(System.Random random, ProceduralGraph graph)
	{
		using PoolWRandomDHandle<ProceduralNodePack.Selection> poolWRandomDHandle = Pools.UseWRandomD<ProceduralNodePack.Selection>();
		if (!_packs.IsNullOrEmpty())
		{
			foreach (ProceduralNodePack pack in _packs)
			{
				ProceduralNodePack.Selection selection = pack.GetSelection(random, graph);
				poolWRandomDHandle.value.Add(selection.weight, selection);
			}
		}
		if (poolWRandomDHandle.value.Count == 0)
		{
			return null;
		}
		ProceduralNodePack.Selection selection2 = poolWRandomDHandle.value.Random(random.NextDouble());
		ProceduralNodeData proceduralNodeData = ProtoUtil.Clone(selection2.node.data);
		ProceduralNodeType? typeOverride = selection2.typeOverride;
		if (typeOverride.HasValue)
		{
			ProceduralNodeType valueOrDefault = typeOverride.GetValueOrDefault();
			proceduralNodeData.type = ((valueOrDefault.IsEncounter() && proceduralNodeData.type.IsEncounter()) ? proceduralNodeData.type.SetEncounterDifficulty(valueOrDefault.EncounterDifficulty()) : valueOrDefault);
		}
		if (random.Chance((float)(int)_unknownChance * 0.01f))
		{
			proceduralNodeData.type = ProceduralNodeType.Unknown;
		}
		if (proceduralNodeData.type.PreventRepeats() && graph != null && !graph.PreventRepeatedNodeDataAdd(selection2.node))
		{
			Debug.Log("Repeated Node Generated: " + selection2.node.friendlyName);
		}
		return proceduralNodeData;
	}

	private PoolWRandomDHandle<ProceduralNodePack.Selection> _GetWeightedSelections()
	{
		PoolWRandomDHandle<ProceduralNodePack.Selection> poolWRandomDHandle = Pools.UseWRandomD<ProceduralNodePack.Selection>();
		if (!_packs.IsNullOrEmpty())
		{
			foreach (ProceduralNodePack pack in _packs)
			{
				using PoolWRandomDHandle<ProceduralNodePack.Selection> poolWRandomDHandle2 = pack.GetWeightedSelections();
				foreach (Couple<double, ProceduralNodePack.Selection> item in poolWRandomDHandle2.value.EnumeratePairInOrder())
				{
					poolWRandomDHandle.value.Add(item, item);
				}
			}
			return poolWRandomDHandle;
		}
		return poolWRandomDHandle;
	}

	private ProceduralNodeType? _HighestWeightNodeType()
	{
		using PoolWRandomDHandle<ProceduralNodePack.Selection> poolWRandomDHandle = _GetWeightedSelections();
		if (poolWRandomDHandle.value.Count == 0)
		{
			return null;
		}
		return (from c in poolWRandomDHandle.value.EnumeratePairInOrder()
			select c.b into s
			group s by s.type).MaxBy((IGrouping<ProceduralNodeType, ProceduralNodePack.Selection> g) => g.Sum((ProceduralNodePack.Selection a) => (float)a.weight)).Key;
	}

	public void PrepareDataForSave()
	{
		if (!_packs.IsNullOrEmpty())
		{
			foreach (ProceduralNodePack pack in _packs)
			{
				pack.PrepareDataForSave();
			}
		}
		_data = null;
		_state = (State)0;
	}

	public void Generate(System.Random random, ProceduralGraph graph, GameState gameState)
	{
		_data = _GetSelection(random, graph);
		data?.OnGraphGenerated(gameState);
		_unknownChance = 0;
		_packs = null;
	}

	public void PostProcessGraph(System.Random random, ProceduralGraph graph)
	{
		data?.PostProcessGraph(random, graph, this);
		if (_postProcessGraphInstructions.IsNullOrEmpty())
		{
			return;
		}
		foreach (PostProcessGraphInstruction postProcessGraphInstruction in _postProcessGraphInstructions)
		{
			postProcessGraphInstruction.PostProcess(random, graph, this);
		}
	}

	public IEnumerable<GameStep> GetCompletedSteps(GameState state)
	{
		if (!_completeInstructions.IsNullOrEmpty())
		{
			foreach (AdventureCard.SelectInstruction completeInstruction in _completeInstructions)
			{
				foreach (GameStep gameStep in completeInstruction.GetGameSteps(state))
				{
					yield return gameStep;
				}
			}
		}
		_completeInstructions = null;
	}

	public void SetId(uint newId)
	{
		_id = newId;
	}

	public ProceduralNode ClearDataForCopy()
	{
		connections.Clear();
		_id = 0u;
		return this;
	}

	public ProceduralNode SetData(ProceduralNodeData dataToSet, ProceduralNodeType? typeOverride = null, GameState bakeIntoGameState = null)
	{
		if (typeOverride.HasValue)
		{
			dataToSet.type = typeOverride.Value;
		}
		_data = dataToSet;
		if (bakeIntoGameState != null)
		{
			using (bakeIntoGameState.OverrideRandom((int)id))
			{
				ProceduralNodeData proceduralNodeData = data;
				if (proceduralNodeData != null)
				{
					proceduralNodeData.OnGraphGenerated(bakeIntoGameState);
					return this;
				}
				return this;
			}
		}
		return this;
	}
}
