using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameStepProceduralMap : AGameStepMap
{
	public class GameStepCreateMap : GameStep
	{
		private GameStepProceduralMap _mainStep;

		private DataRef<ProceduralGraphData> graphToCreate => _mainStep._graphToCreate;

		private ProceduralMap map => _mainStep.map;

		private ProceduralMapView mapView => (ProceduralMapView)map.view;

		public GameStepCreateMap(GameStepProceduralMap mainStep)
		{
			_mainStep = mainStep;
		}

		protected override IEnumerator Update()
		{
			if ((bool)graphToCreate)
			{
				ProceduralMap generatedMap = new ProceduralMap(graphToCreate.data.graph.GenerateGraph(base.state), graphToCreate)
				{
					generateOverTime = true
				};
				yield return null;
				base.state.mapDeck.Add(generatedMap, ProceduralMap.Pile.Closed);
				yield return null;
				IEnumerator generatedMapView = mapView.GenerateOverTime(generatedMap);
				while (generatedMapView.MoveNext())
				{
					yield return null;
				}
			}
		}

		protected override void End()
		{
			if (map.graph.lastSelectedNode != null)
			{
				return;
			}
			foreach (ProceduralNode selectableNode in map.graph.GetSelectableNodes())
			{
				selectableNode.isSelectable = true;
			}
		}
	}

	private DataRef<ProceduralGraphData> _graphToCreate;

	private bool _hasAppendedMapActiveInstructions;

	public static GameStepProceduralMap Instance { get; private set; }

	private ProceduralMap map => ProceduralMapView.Instance.map;

	private ProceduralMapView mapView => ProceduralMapView.Instance;

	private DragPlane3D dragPlane => base.view.mapDeckLayout.dragPlane;

	public bool setContentPositionOnEnable { get; private set; }

	public bool hasFocusedMap { get; private set; }

	protected override IEnumerable<ACardLayout> _layoutsToOffset
	{
		get
		{
			yield return base.view.playerResourceDeckLayout.hand;
		}
	}

	public override bool canSafelyCancelStack => true;

	public GameStepProceduralMap()
	{
	}

	public GameStepProceduralMap(DataRef<ProceduralGraphData> graphData)
	{
		_graphToCreate = graphData;
	}

	private void _OnNodeClicked(ProceduralNode node, PointerEventData eventData)
	{
		if ((!ProfileManager.options.devInputEnabled || !InputManager.I[KeyModifiers.Shift] || !InputManager.I[KeyCode.D][KState.Down] || !node.isActive || node.isCompleted || !(base.state.devCommandUsed = true)) && !map.graph.IsSelectableNode(node))
		{
			return;
		}
		bool hasAppendedMapActiveInstructions = (hasFocusedMap = false);
		_hasAppendedMapActiveInstructions = hasAppendedMapActiveInstructions;
		map.graph.MarkAsSelected(node, clearSelected: false);
		map.graph.MarkAsCompleted(node);
		node.data.GenerateCards(base.state);
		base.state.mapDeck.Transfer(map, ProceduralMap.Pile.Closed);
		AppendStep(new GameStepWaitForCardTransition(map.view));
		PoolKeepItemListHandle<AdventureCard.SelectInstruction> poolKeepItemListHandle = Pools.UseKeepItemList(node.selectInstructions.Concat(node.data.onSelectInstructions));
		AppendStep(new GameStepSaveGameState(poolKeepItemListHandle.AsEnumerable()));
		foreach (GameStep step in poolKeepItemListHandle.value.GetSteps(base.state))
		{
			AppendStep(step);
		}
	}

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.Back)
		{
			_OnBack();
		}
	}

	private void _OnBack()
	{
		AppendStep(new GameStepHideProceduralMap());
	}

	private void _OnHideRequested()
	{
		if (mapView.atRestInLayout)
		{
			_OnBack();
		}
	}

	private void _OnDraggedBeyondThreshold(Vector2 beyondThresholdRatio)
	{
		if (!(beyondThresholdRatio.y < 0.025f))
		{
			_OnBack();
			dragPlane.SetContentPosition(dragPlane.contentPositionAtBeginDrag);
		}
	}

	private bool _AppendMapActiveInstructions()
	{
		if (_hasAppendedMapActiveInstructions)
		{
			return false;
		}
		_hasAppendedMapActiveInstructions = true;
		AppendStep(new GameStepSaveGameState());
		foreach (GameStep step in map.data.onMapActivatedInstructions.GetSteps(base.state))
		{
			AppendStep(step);
		}
		return !base.isActiveStep;
	}

	private void _OnClearLayoutOffsetsDelayed()
	{
		_ResetLayouts();
	}

	protected override void Awake()
	{
		Instance = this;
	}

	protected override void OnAboutToEnableForFirstTime()
	{
		if (!base.state.compassDeck.Any())
		{
			base.state.compassDeck.Add(new MapCompass());
		}
		AppendStep(new GameStepCreateMap(this));
	}

	protected override void OnEnable()
	{
		ProceduralNode lastSelectedNode = map.graph.lastSelectedNode;
		if (lastSelectedNode != null)
		{
			if (lastSelectedNode.type == ProceduralNodeType.LevelUp && !new AAction.Condition.APlayer.CanLevelUp().IsValidTarget(base.state.player, base.state.player))
			{
				foreach (ProceduralNode node in map.graph.GetNodes())
				{
					if (node.type == ProceduralNodeType.LevelUp && node.isActive && !node.isSelected)
					{
						ProceduralNodePack.Selection selection = ContentRef.Defaults.data.proceduralLevelUpNodeFallback.GetSelection(base.state.random, map.graph);
						mapView[node.SetData(ProtoUtil.Clone(selection.node.data), selection.typeOverride, base.state).id].UpdateNodeType();
					}
				}
			}
			foreach (GameStep completedStep in lastSelectedNode.GetCompletedSteps(base.state))
			{
				AppendStep(completedStep);
			}
			base.finished |= lastSelectedNode.isEndNode;
			if (!base.isActiveStep || base.finished)
			{
				return;
			}
		}
		if (_AppendMapActiveInstructions())
		{
			return;
		}
		foreach (ProceduralNode selectableNode in map.graph.GetSelectableNodes())
		{
			selectableNode.isSelectable = true;
		}
		if (base.state.mapDeck.Count(ProceduralMap.Pile.Hidden) == 0)
		{
			FocusMap();
		}
		base.state.mapDeck.Transfer(map, ProceduralMap.Pile.Active);
		if (setContentPositionOnEnable && !(setContentPositionOnEnable = false))
		{
			dragPlane.SetContentPosition(mapView.transform.position);
		}
		base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: true);
		mapView.onNodeClicked += _OnNodeClicked;
		mapView.onHideRequested += _OnHideRequested;
		base.view.buttonDeckLayout.onPointerClick += _OnButtonClick;
		base.view.onBackPressed += _OnBack;
		dragPlane.onDragBeyondThreshold.AddListener(_OnDraggedBeyondThreshold);
		mapView.inputBlockerEnabled = true;
		_OffsetLayouts();
		LayoutOffset.OnClearLayoutOffsetsDelayed += _OnClearLayoutOffsetsDelayed;
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		foreach (ProceduralNode selectableNode in map.graph.GetSelectableNodes())
		{
			selectableNode.isSelectable = false;
		}
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false, forceUpdateCancelStone: true);
		base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
		mapView.onNodeClicked -= _OnNodeClicked;
		mapView.onHideRequested -= _OnHideRequested;
		base.view.buttonDeckLayout.onPointerClick -= _OnButtonClick;
		base.view.onBackPressed -= _OnBack;
		dragPlane.onDragBeyondThreshold.RemoveListener(_OnDraggedBeyondThreshold);
		mapView.inputBlockerEnabled = false;
		_ClearLayoutOffsets();
		LayoutOffset.OnClearLayoutOffsetsDelayed -= _OnClearLayoutOffsetsDelayed;
	}

	protected override void OnDestroy()
	{
		Instance = ((Instance == this) ? null : Instance);
	}

	public void SignalDraggedBackIntoView()
	{
		setContentPositionOnEnable = true;
	}

	public void FocusMap()
	{
		if (hasFocusedMap = true)
		{
			dragPlane.FocusOnPosition(mapView.nodeContainer, mapView.GetSelectableCentroid());
		}
	}
}
