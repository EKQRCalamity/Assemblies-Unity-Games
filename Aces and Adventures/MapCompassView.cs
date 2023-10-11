using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MapCompassView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Procedural/MapCompassView";

	public UnityEvent onAtRestInActivePile;

	private bool _cancelStoneSlotAvailable;

	private bool _activeStepAllowsViewMap;

	private bool _canBeActiveWhileCancelIsActive;

	private MapCompass.Pile? _pendingPile;

	private bool? _pendingActiveLayoutFloating;

	public static MapCompassView Instance { get; private set; }

	public MapCompass compass
	{
		get
		{
			return (MapCompass)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	public MapCompassDeckLayout deckLayout => base.deck as MapCompassDeckLayout;

	private bool cancelStoneSlotAvailable
	{
		get
		{
			return _cancelStoneSlotAvailable;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _cancelStoneSlotAvailable, value))
			{
				_UpdatePile();
			}
		}
	}

	private bool activeStepAllowsViewMap
	{
		get
		{
			return _activeStepAllowsViewMap;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _activeStepAllowsViewMap, value))
			{
				_UpdatePile();
			}
		}
	}

	private bool active
	{
		get
		{
			if (cancelStoneSlotAvailable || canBeActiveWhileCancelIsActive)
			{
				return activeStepAllowsViewMap;
			}
			return false;
		}
	}

	public bool canBeActiveWhileCancelIsActive
	{
		get
		{
			return _canBeActiveWhileCancelIsActive;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _canBeActiveWhileCancelIsActive, value))
			{
				_OnCanBeActiveWhileCancelIsActiveChange();
			}
		}
	}

	public static MapCompassView Create(MapCompass compass, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<MapCompassView>()._SetData(compass);
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		if (active)
		{
			compass.gameState.stack.Push(new GameStepViewMap());
		}
	}

	private void _OnPointerOver(PointerEventData eventData)
	{
		if (active)
		{
			ProjectedTooltipFitter.Create(MessageData.GameTooltips.ViewMap.Localize().Localize(), base.gameObject, base.view.tooltipCanvas);
		}
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		ProjectedTooltipFitter.Finish(base.gameObject, allowGoingOffScreen: true);
	}

	private void _OnStoneTransfer(Stone stone, Stone.Pile? oldPile, Stone.Pile? newPile)
	{
		if (stone.type == StoneType.Cancel)
		{
			if (newPile == Stone.Pile.Cancel)
			{
				cancelStoneSlotAvailable = false;
			}
			else if (newPile == Stone.Pile.CancelInactive)
			{
				cancelStoneSlotAvailable = true;
			}
		}
	}

	private void _OnEnabledStepChange(GameStep step, bool stepEnabled)
	{
		activeStepAllowsViewMap = stepEnabled && step.canSafelyCancelStack;
	}

	private void _OnBackPressed()
	{
		if (base.view.state.compassDeck.Any(MapCompass.Pile.Active))
		{
			_OnPointerClick(null);
		}
	}

	private void _OnRest(MapCompass.Pile pile, MapCompass mapCompass)
	{
		if (pile == MapCompass.Pile.Active)
		{
			onAtRestInActivePile?.Invoke();
		}
	}

	private void _UpdatePile()
	{
		_pendingPile = (active ? MapCompass.Pile.Active : MapCompass.Pile.Inactive);
	}

	private void _OnCanBeActiveWhileCancelIsActiveChange()
	{
		_pendingActiveLayoutFloating = canBeActiveWhileCancelIsActive;
		_UpdatePile();
	}

	private MapCompassView _SetData(MapCompass mapCompass)
	{
		compass = mapCompass;
		base.pointerClick.OnClick.AddListener(_OnPointerClick);
		base.pointerOver.OnEnter.AddListener(_OnPointerOver);
		base.pointerOver.OnExit.AddListener(_OnPointerExit);
		cancelStoneSlotAvailable = base.view.state.stoneDeck.Count(Stone.Pile.Cancel) == 0;
		base.view.state.stoneDeck.onTransfer += _OnStoneTransfer;
		base.view.state.stack.onEnabledChange += _OnEnabledStepChange;
		base.view.onBackPressed += _OnBackPressed;
		base.view.compassDeckLayout.onRest += _OnRest;
		return this;
	}

	protected override void Start()
	{
		Instance = this;
		base.Start();
	}

	protected override void OnDestroy()
	{
		Instance = ((Instance == this) ? null : Instance);
		if ((bool)base.view)
		{
			if (base.view.state != null)
			{
				base.view.state.stoneDeck.onTransfer -= _OnStoneTransfer;
				base.view.state.stack.onEnabledChange -= _OnEnabledStepChange;
			}
			base.view.onBackPressed -= _OnBackPressed;
			base.view.compassDeckLayout.onRest -= _OnRest;
		}
		base.OnDestroy();
	}

	private void LateUpdate()
	{
		if (_pendingActiveLayoutFloating == true)
		{
			deckLayout.SetLayout(MapCompass.Pile.Active, deckLayout.activeFloating);
			compass.view.RequestGlow(this, Colors.TARGET);
		}
		MapCompass.Pile? pendingPile = _pendingPile;
		if (pendingPile.HasValue)
		{
			MapCompass.Pile valueOrDefault = pendingPile.GetValueOrDefault();
			pendingPile = (_pendingPile = null);
			if (!pendingPile.HasValue)
			{
				base.view.state.compassDeck.Transfer(compass, valueOrDefault);
			}
		}
		if (_pendingActiveLayoutFloating == false)
		{
			deckLayout.RestoreLayoutToDefault(MapCompass.Pile.Active);
			compass.view.ReleaseGlow(this);
		}
		_pendingActiveLayoutFloating = null;
	}
}
