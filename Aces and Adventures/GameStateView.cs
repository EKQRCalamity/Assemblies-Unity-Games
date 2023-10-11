using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class GameStateView : MonoBehaviour
{
	public struct TooltipData
	{
		public Func<IEnumerable<ATarget>> generateCards;

		private Transform _creator;

		private float _elapsed;

		private bool _show;

		private bool _isShowing;

		private float _pivot;

		private float? _pivotOverride;

		public Transform creator
		{
			get
			{
				return _creator;
			}
			set
			{
				if (!(_creator = value))
				{
					show = (_isShowing = false);
				}
			}
		}

		public bool show
		{
			get
			{
				return _show;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref _show, value))
				{
					_elapsed = 0f;
				}
			}
		}

		public float pivotThreshold
		{
			get
			{
				if (!(_pivot > 0f))
				{
					return 0.49f;
				}
				return 0.51f;
			}
		}

		public float pivot
		{
			get
			{
				return _pivot;
			}
			set
			{
				_pivot = value;
			}
		}

		public float? pivotOverride
		{
			get
			{
				return _pivotOverride;
			}
			set
			{
				_pivotOverride = value;
			}
		}

		public float elapsed
		{
			get
			{
				return _elapsed;
			}
			set
			{
				_elapsed = value;
			}
		}

		public bool Update(float gracePeriod)
		{
			if (show)
			{
				if (!_isShowing)
				{
					return _isShowing = (_elapsed += Time.deltaTime) >= gracePeriod;
				}
				return false;
			}
			if (_elapsed <= 0f && InputManager.I.eventSystem.IsPointerDraggingCheckAllButtons())
			{
				return false;
			}
			if ((_elapsed += Time.deltaTime) >= gracePeriod)
			{
				_isShowing = false;
				return true;
			}
			return false;
		}

		public static implicit operator bool(TooltipData data)
		{
			return data._creator;
		}
	}

	private static ResourceBlueprint<GameObject> _Blueprint = "GameState/GameStateView";

	private static ResourceBlueprint<GameObject> _LevelUpBlueprint = "GameState/GameStateViewLevelUp";

	private static GameStateView _Instance;

	public DepthOfFieldShifter dofShifter;

	public Transform inspectDragPlane;

	public Transform inspectDragPlaneFlipped;

	public CardTargetTransforms turnOrderTarget;

	[Header("Decks")]
	public AdventureDeckLayout adventureDeckLayout;

	public ResourceDeckLayout enemyResourceDeckLayout;

	public ButtonDeckLayout buttonDeckLayout;

	public TurnOrderSpaceLayout turnOrderSpaceLayout;

	public ChipDeckLayout chipDeckLayout;

	public StoneDeckLayout stoneDeckLayout;

	public TutorialDeckLayout tutorialDeckLayout;

	public ResourceDeckLayout playerResourceDeckLayout;

	public AbilityDeckLayout playerAbilityDeckLayout;

	public HeroDeckLayout heroDeckLayout;

	public ExileDeckLayout exileDeckLayout;

	public DecksLayout decksLayout;

	public RewardDeckLayout rewardDeckLayout;

	public GameStoneDeckLayout gameStoneDeckLayout;

	public ProceduralMapDeckLayout mapDeckLayout;

	public MapCompassDeckLayout compassDeckLayout;

	[Range(-1f, 0f)]
	public float turnStoneOffset = -0.05f;

	public CardAdditionalFoleySoundPack foleySoundPack;

	[Header("UI")]
	[SerializeField]
	protected Canvas _canvas;

	[SerializeField]
	protected Canvas _screenCanvas;

	[SerializeField]
	protected RectTransform _screenCanvasContainer;

	[SerializeField]
	protected RectTransform _errorMessageContainer;

	[SerializeField]
	protected RectTransform _mainMessageContainer;

	[SerializeField]
	protected AnimatedMessage _errorMessageBlueprint;

	[SerializeField]
	protected AnimatedMessage _mainMessageBlueprint;

	[SerializeField]
	protected GameObject _raycastBlocker;

	[SerializeField]
	protected CardHandLayout _tooltipHand;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _tooltipHandGracePeriod = 0.1f;

	private GameState _state;

	private DeckCreationStateView _deckCreation;

	private LevelUpStateView _levelUp;

	private Light _mainLight;

	private bool _cancelStoneDirty;

	private ResourceCard.Piles _wildPiles;

	private ResourceCard.Piles _enemyWildPiles;

	private HashSet<ACardLayout> _depthOfFieldLayouts = new HashSet<ACardLayout>();

	private GraphicRaycaster _screenCanvasGraphicRaycaster;

	private CardDimensionSettings _mainHandScale;

	private Ability _hoverOverAbility;

	private float _hoverOverAbilityTime;

	private CardTooltipLayout.DirectionAlongAxis _hoverOverAbilityTooltipDirection;

	private bool _hoverOverAbilityTooltipActive;

	private List<LocalizedString> _cancelStoneTooltipOverrides;

	private ACombatant _hoverOverCombatant;

	private float _hoverOverCombatantTime;

	private bool _hoverOverCombatantTooltipActive;

	private Matrix4x4 _hoverOverCombatantOffset;

	private readonly HashSet<Ability> _abilitiesThatAreProcessingPotentialDamage = new HashSet<Ability>();

	private readonly Dictionary<uint, int> _abilityHighlightRequests = new Dictionary<uint, int>();

	private TooltipData _tooltipHandData;

	public static GameStateView Instance
	{
		get
		{
			if (!_Instance)
			{
				return _Instance = ((!UJobManager.IsQuitting) ? UnityEngine.Object.Instantiate(_Blueprint.value).GetComponent<GameStateView>() : null);
			}
			return _Instance;
		}
		private set
		{
			_Instance = value;
		}
	}

	public static bool HasActiveInstance => _Instance;

	public GameState state
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				_OnStateChange(_state, _state = value);
			}
		}
	}

	public Canvas tooltipCanvas => _screenCanvas;

	public Transform screenCanvasContainer
	{
		get
		{
			_screenCanvas.gameObject.SetActive(value: true);
			return _screenCanvasContainer;
		}
	}

	public GraphicRaycaster screenCanvasGraphicRaycaster => _screenCanvas.CacheComponentInChildren(ref _screenCanvasGraphicRaycaster);

	public ResourceCard.Piles wildPiles
	{
		get
		{
			return _wildPiles;
		}
		set
		{
			_wildPiles = value;
		}
	}

	public ResourceCard.Piles enemyWildPiles
	{
		get
		{
			return _enemyWildPiles;
		}
		set
		{
			_enemyWildPiles = value;
		}
	}

	public PoolKeepItemListHandle<ADeckLayoutBase> decks => base.gameObject.GetComponentsInChildrenPooled<ADeckLayoutBase>();

	public DeckCreationStateView deckCreation => this.CacheComponentInChildren(ref _deckCreation);

	public LevelUpStateView levelUp => this.CacheComponentInChildren(ref _levelUp);

	public int suppressShieldEvents { get; set; }

	private List<LocalizedString> cancelStoneTooltipOverrides => _cancelStoneTooltipOverrides ?? (_cancelStoneTooltipOverrides = new List<LocalizedString>());

	public float? abilityHoverOverTooltipTimeOverride { get; set; }

	public float abilityHoverOverTooltipTime => abilityHoverOverTooltipTimeOverride ?? ProfileManager.options.game.ui.abilityHoverOverTooltipTime;

	public event Action onClick;

	public event Action onConfirmPressed;

	public event Action onBackPressed;

	public event Action onRefreshPointerOverRequest;

	private static GameStateView _CreateBlueprint(ResourceBlueprint<GameObject> gameStateViewBlueprint, Transform parent = null)
	{
		_DestroyState();
		return _Instance = UnityEngine.Object.Instantiate(gameStateViewBlueprint.value, parent, worldPositionStays: true).GetComponent<GameStateView>();
	}

	public static GameStateView CreateAdventureView()
	{
		return _CreateBlueprint(_Blueprint);
	}

	public static GameStateView CreateLevelUpView(Transform parent = null)
	{
		return _CreateBlueprint(_LevelUpBlueprint, parent);
	}

	public static void DestroyInstance()
	{
		if ((bool)_Instance)
		{
			_Instance.gameObject.DestroyHierarchyBottomUp();
		}
	}

	public static void _DestroyState()
	{
		if ((bool)_Instance)
		{
			if (_Instance.state != null)
			{
				_Instance.state.Destroy();
			}
			else
			{
				DestroyInstance();
			}
		}
	}

	private void _OnStateChange(GameState oldState, GameState newState)
	{
		if (newState == null)
		{
			return;
		}
		adventureDeckLayout.deck = newState.adventureDeck;
		foreach (ResourceCard card in newState.enemyResourceDeck.GetCards())
		{
			card.skin = PlayingCardSkinType.Enemy;
		}
		enemyResourceDeckLayout.deck = newState.enemyResourceDeck;
		buttonDeckLayout.deck = newState.buttonDeck;
		turnOrderSpaceLayout.deck = newState.turnOrderSpaceDeck;
		chipDeckLayout.deck = newState.chipDeck;
		stoneDeckLayout.faceDownStacks = new List<Stone.Pile>
		{
			Stone.Pile.Inactive,
			Stone.Pile.CancelInactive
		};
		stoneDeckLayout.faceUpStacks = new List<Stone.Pile>();
		stoneDeckLayout.deck = newState.stoneDeck;
		tutorialDeckLayout.deck = newState.tutorialDeck;
		playerResourceDeckLayout.deck = newState.playerResourceDeck;
		playerResourceDeckLayout.deck.AddSortedPiles(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand);
		playerResourceDeckLayout.onPointerClick += _OnPlayerResourceClick;
		playerAbilityDeckLayout.deck = newState.abilityDeck;
		playerAbilityDeckLayout.deck.AddSortedPiles(Ability.Piles.HeroAct | Ability.Piles.HeroPassive);
		playerAbilityDeckLayout.onPointerEnter += _OnPlayerAbilityPointerEnter;
		playerAbilityDeckLayout.onPointerExit += _OnPlayerAbilityPointerExit;
		playerAbilityDeckLayout.onPointerClick += _OnPlayerAbilityPointerClick;
		adventureDeckLayout.onPointerEnter += _OnAdventurePointerEnter;
		adventureDeckLayout.onPointerExit += _OnAdventurePointerExit;
		adventureDeckLayout.onPointerClick += _OnAdventurePointerClick;
		heroDeckLayout.onPointerEnter += _OnHeroPointerEnter;
		heroDeckLayout.onPointerExit += _OnHeroPointerExit;
		rewardDeckLayout.onPointerEnter += _OnRewardPointerEnter;
		rewardDeckLayout.onPointerExit += _OnRewardPointerExit;
		rewardDeckLayout.onPointerClick += _OnRewardClick;
		foreach (ACardLayout item in _LayoutsThatHandleAbilityHoverOverTooltips())
		{
			item.onDragBegin += _OnAbilityHoverLayoutDragBegin;
			item.onDragEnd += _OnAbilityHoverLayoutDragEnd;
			item.onCardRemoved += _OnAbilityHoverLayoutCardRemoved;
		}
		heroDeckLayout.deck = newState.heroDeck;
		exileDeckLayout.deck = newState.exileDeck;
		decksLayout.deck = newState.decks;
		rewardDeckLayout.deck = newState.rewardDeck;
		gameStoneDeckLayout.deck = newState.gameStoneDeck;
		gameStoneDeckLayout.onPointerEnter += _OnGameStonePointerEnter;
		gameStoneDeckLayout.onPointerExit += _OnGameStonePointerExit;
		if ((bool)mapDeckLayout)
		{
			mapDeckLayout.deck = newState.mapDeck;
		}
		if ((bool)compassDeckLayout)
		{
			compassDeckLayout.deck = newState.compassDeck;
		}
		newState.stack.onEnabledChange += _OnGameStepEnableChange;
		newState.buttonDeck.onTransfer += _OnButtonDeckTransfer;
		newState.onDamageDealt += _OnDamageDealt;
		chipDeckLayout.onPointerEnter += _OnChipPointerEnter;
		chipDeckLayout.onPointerExit += _OnChipPointerExit;
		stoneDeckLayout.onPointerClick += _OnStonePointerClick;
		stoneDeckLayout.onPointerEnter += _OnStonePointerEnter;
		stoneDeckLayout.onPointerExit += _OnStonePointerExit;
		stoneDeckLayout.GetLayouts().AsEnumerable().EffectAll(delegate(ACardLayout layout)
		{
			layout.disableInputColliderManagement = true;
		});
		playerResourceDeckLayout.onPointerEnter += _OnResourcePointerEnter;
		playerResourceDeckLayout.onPointerExit += _OnResourcePointerExit;
		enemyResourceDeckLayout.onPointerEnter += _OnResourcePointerEnter;
		enemyResourceDeckLayout.onPointerExit += _OnResourcePointerExit;
		enemyResourceDeckLayout.onPointerClick += _OnEnemyResourceClick;
		exileDeckLayout.deck.onTransfer += _OnExileTransfer;
		exileDeckLayout.onRest += _OnExileRest;
		exileDeckLayout.onPointerExit += _OnExilePointerExit;
		rewardDeckLayout.onRest += _OnRewardRest;
		adventureDeckLayout.onRest += _OnAdventureRest;
		newState.playerResourceDeck.onTransfer += _OnPlayerResourceTransfer;
		newState.enemyResourceDeck.onTransfer += _OnEnemyResourceTransfer;
		newState.abilityDeck.onTransfer += _OnAbilityTransfer;
		Camera.main.transparencySortMode = TransparencySortMode.Orthographic;
		if ((bool)deckCreation)
		{
			deckCreation.SetState(newState.deckCreation);
			deckCreation.abilities.onPointerEnter += _OnDeckEditorAbilityPointerEnter;
			deckCreation.abilities.onPointerExit += _OnDeckEditorAbilityPointerExit;
		}
		if ((bool)levelUp)
		{
			levelUp.SetState(newState.levelUp);
		}
		_AddLayoutsToDepthOfFieldManagers();
		ACardLayout.OnDragBegan += _OnDragBegan;
		ACardLayout.OnDragEnded += _OnDragEnded;
		ADeckLayoutBase.OnPointerEnter += _OnPointerEnter;
		ADeckLayoutBase.OnPointerExit += _OnPointerExit;
		_mainHandScale = UnityEngine.Object.Instantiate(playerResourceDeckLayout.hand.size);
		playerResourceDeckLayout.hand.size = (((CardStackLayout)playerResourceDeckLayout.draw).dragLayout.size = (((CardStackLayout)playerAbilityDeckLayout.draw).dragLayout.size = (((CardStackLayout)enemyResourceDeckLayout.draw).dragLayout.size = _mainHandScale)));
		((CardStackLayout)playerResourceDeckLayout.discard).dragLayout.size = (((CardStackLayout)playerAbilityDeckLayout.discard).dragLayout.size = (((CardStackLayout)enemyResourceDeckLayout.discard).dragLayout.size = (((CardStackLayout)adventureDeckLayout.discard).dragLayout.size = _mainHandScale)));
		if ((bool)levelUp)
		{
			levelUp.main.levelUpsView.size = _mainHandScale;
		}
		if ((bool)mapDeckLayout)
		{
			mapDeckLayout.active.size = _mainHandScale;
		}
		playerResourceDeckLayout.select.size = _mainHandScale;
		_OnMainHandScaleChange(ProfileManager.options.game.ui.mainHandScale);
		ProfileOptions.GameOptions.UIOptions.OnMainHandScaleChange += _OnMainHandScaleChange;
		if ((bool)heroDeckLayout?.selectionHandUnrestricted)
		{
			heroDeckLayout.selectionHandUnrestricted.groupingEqualityComparer = AbilityCategoryEqualityComparer.Default;
		}
		if ((bool)mapDeckLayout)
		{
			mapDeckLayout.onRest += _OnMapRest;
		}
		_tooltipHand.settings = UnityEngine.Object.Instantiate(_tooltipHand.settings);
	}

	private IEnumerable<ACardLayout> _LayoutsThatHandleAbilityHoverOverTooltips()
	{
		yield return playerAbilityDeckLayout.hand;
		yield return playerAbilityDeckLayout.select;
		yield return playerAbilityDeckLayout.activationHand;
		yield return playerAbilityDeckLayout.hero;
		yield return playerAbilityDeckLayout.heroPassive;
		yield return adventureDeckLayout.selectionHand;
		yield return heroDeckLayout.selectionHand;
		yield return rewardDeckLayout.cardPackSelect;
		if ((bool)deckCreation)
		{
			yield return deckCreation.abilities.list;
			foreach (ACardLayout cardLayout in deckCreation.abilities.results.GetCardLayouts())
			{
				yield return cardLayout;
			}
		}
		yield return adventureDeckLayout.inspectHand;
		foreach (ACardLayout cardLayout2 in heroDeckLayout.selectionHandUnrestricted.GetCardLayouts())
		{
			yield return cardLayout2;
		}
	}

	private void _BeginAbilityTooltipTimer(Ability ability, CardTooltipLayout.DirectionAlongAxis direction = CardTooltipLayout.DirectionAlongAxis.Auto, float initialHoverOverTime = 0f)
	{
		if (ProfileManager.options.game.ui.abilityHoverTips)
		{
			_hoverOverAbility = ability;
			_hoverOverAbilityTooltipDirection = direction;
			_hoverOverAbilityTime = ((_hoverOverAbilityTime > 0f) ? _hoverOverAbilityTime : initialHoverOverTime);
		}
	}

	private void _EndAbilityTooltipTimer(Ability ability, bool isPointerExit = true)
	{
		if (ability == _hoverOverAbility && (!isPointerExit || !(ability.view.layout.pointerOver == ability.view)))
		{
			_hoverOverAbility?.abilityCard?.HideTooltips();
			_hoverOverAbility = null;
			_hoverOverAbilityTime = 0f;
			_hoverOverAbilityTooltipActive = false;
		}
	}

	private void _BeginCombatantTooltipTimer(ACombatant combatant)
	{
		if (ProfileManager.options.game.ui.enemyHoverTips)
		{
			_hoverOverCombatant = combatant;
			_hoverOverCombatantTime = 0f;
		}
	}

	private void _EndCombatantTooltipTimer(ACombatant combatant, bool isPointerExit = true)
	{
		if (combatant == _hoverOverCombatant && (!isPointerExit || !(combatant.view.layout.pointerOver == combatant.view)))
		{
			if (_hoverOverCombatantTooltipActive)
			{
				_hoverOverCombatant?.combatantCard?.HideTooltips();
				_hoverOverCombatant?.view?.offsets.Remove(_hoverOverCombatantOffset);
				_hoverOverCombatant?.view?.layout.SetPointerOverPaddingOverride(null);
				_hoverOverCombatant?.combatantCard?.appliedAbilityLayout.debuff.SetPointerOverPaddingOverride(null);
				_hoverOverCombatant?.combatantCard?.onTappedChange?.Invoke(_hoverOverCombatant?.tapped);
			}
			_hoverOverCombatant = null;
			_hoverOverCombatantTime = 0f;
			_hoverOverCombatantTooltipActive = false;
		}
	}

	private void _OnHeroPointerEnter(HeroDeckPile pile, Ability ability)
	{
		if (pile == HeroDeckPile.SelectionHand && !ability.view.hasOffset)
		{
			_BeginAbilityTooltipTimer(ability);
		}
	}

	private void _OnHeroPointerExit(HeroDeckPile pile, Ability ability)
	{
		_EndAbilityTooltipTimer(ability);
	}

	private void _OnAdventurePointerEnter(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.SelectionHand && card is Ability ability)
		{
			_BeginAbilityTooltipTimer(ability);
		}
		else if (pile == AdventureCard.Pile.TurnOrder && card is Enemy combatant)
		{
			_BeginCombatantTooltipTimer(combatant);
		}
	}

	private void _OnAdventurePointerExit(AdventureCard.Pile pile, ATarget card)
	{
		if (card is Ability ability)
		{
			_EndAbilityTooltipTimer(ability);
		}
		else if (card is Enemy combatant)
		{
			_EndCombatantTooltipTimer(combatant);
		}
	}

	private void _OnRewardPointerEnter(RewardPile pile, ATarget card)
	{
		if (pile == RewardPile.CardPackSelect && card is Ability ability && !card.view.hasOffset)
		{
			_BeginAbilityTooltipTimer(ability);
		}
	}

	private void _OnRewardPointerExit(RewardPile pile, ATarget card)
	{
		if (card is Ability ability)
		{
			_EndAbilityTooltipTimer(ability);
		}
	}

	private void _OnRewardClick(RewardPile pile, ATarget card)
	{
		if (pile == RewardPile.CardPackSelect && card is Ability ability && card.view.hasOffset)
		{
			_BeginAbilityTooltipTimer(ability, CardTooltipLayout.DirectionAlongAxis.Auto, -1f);
		}
	}

	private void _OnPlayerAbilityPointerEnter(Ability.Pile pile, Ability ability)
	{
		if (pile == Ability.Pile.Hand || pile == Ability.Pile.ActivationHand || pile == Ability.Pile.HeroAct || pile == Ability.Pile.HeroPassive || ability.view.layout == adventureDeckLayout.inspectHand)
		{
			_BeginAbilityTooltipTimer(ability, (pile == Ability.Pile.Hand || ability.view.layout == adventureDeckLayout.inspectHand) ? CardTooltipLayout.DirectionAlongAxis.Negative : CardTooltipLayout.DirectionAlongAxis.Auto);
		}
	}

	private void _OnPlayerAbilityPointerExit(Ability.Pile pile, Ability ability)
	{
		_EndAbilityTooltipTimer(ability);
	}

	private void _OnPlayerAbilityPointerClick(Ability.Pile pile, Ability card)
	{
		GameStep activeStep = state.stack.activeStep;
		if (activeStep != null && activeStep.canInspect)
		{
			if (pile == Ability.Pile.Draw || (pile == Ability.Pile.Discard && !state.abilityDeck.Any(Ability.Pile.Draw)))
			{
				state.stack.Append(InspectAllAbilitiesInDeckStep());
			}
			else if (pile == Ability.Pile.Discard)
			{
				state.stack.Append(state.abilityDeck.InspectStep(Ability.Piles.Discard, null, null, MessageData.GameTooltips.ViewAbilityDiscard, adventureDeckLayout.inspectLargeSettings));
			}
		}
	}

	private void _OnPlayerResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		GameStep activeStep = state.stack.activeStep;
		if (activeStep != null && activeStep.canInspect)
		{
			if (pile == ResourceCard.Pile.DrawPile || (pile == ResourceCard.Pile.DiscardPile && !state.playerResourceDeck.Any(ResourceCard.Pile.DrawPile)))
			{
				state.stack.Append(state.playerResourceDeck.InspectStep(EnumUtil<ResourceCard.Piles>.AllFlags, ResourceCard.NaturalValueComparer.Ascending, ResourceCardViewNaturalValueEqualityComparer.Default, MessageData.GameTooltips.ViewPlayerDeck, adventureDeckLayout.inspectHugeGroupSettings));
			}
			else if (pile == ResourceCard.Pile.DiscardPile)
			{
				state.stack.Append(state.playerResourceDeck.InspectStep(ResourceCard.Piles.DiscardPile, null, null, MessageData.GameTooltips.ViewPlayerCardDiscard, adventureDeckLayout.inspectLargeSettings));
			}
		}
	}

	private void _OnEnemyResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		GameStep activeStep = state.stack.activeStep;
		if (activeStep != null && activeStep.canInspect)
		{
			switch (pile)
			{
			case ResourceCard.Pile.DrawPile:
				state.stack.Append(state.enemyResourceDeck.InspectStep(ResourceCard.Piles.DrawPile, ResourceCard.NaturalValueComparer.Ascending, ResourceCardViewNaturalValueEqualityComparer.Default, MessageData.GameTooltips.ViewEnemyCardDraw, adventureDeckLayout.inspectHugeGroupSettings));
				break;
			case ResourceCard.Pile.DiscardPile:
				state.stack.Append(state.enemyResourceDeck.InspectStep(ResourceCard.Piles.DiscardPile, null, null, MessageData.GameTooltips.ViewEnemyCardDiscard, adventureDeckLayout.inspectLargeSettings));
				break;
			}
		}
	}

	private void _OnAdventurePointerClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.Discard)
		{
			GameStep activeStep = state.stack.activeStep;
			if (activeStep != null && activeStep.canInspect)
			{
				state.stack.Append(state.adventureDeck.InspectStep(AdventureCard.Piles.Discard, null, null, MessageData.GameTooltips.ViewAdventureDiscard, adventureDeckLayout.inspectLargeSettings));
			}
		}
	}

	private void _OnDeckEditorAbilityPointerEnter(DeckCreationPile pile, Ability card)
	{
		if (pile == DeckCreationPile.List || pile == DeckCreationPile.Results)
		{
			_BeginAbilityTooltipTimer(card);
		}
	}

	private void _OnDeckEditorAbilityPointerExit(DeckCreationPile pile, Ability card)
	{
		_EndAbilityTooltipTimer(card);
	}

	private void _OnAbilityHoverLayoutDragBegin(PointerEventData eventData, CardLayoutElement card)
	{
		if (card.card is Ability ability)
		{
			_EndAbilityTooltipTimer(ability, isPointerExit: false);
		}
	}

	private void _OnAbilityHoverLayoutDragEnd(PointerEventData eventData, CardLayoutElement card)
	{
		if (card.layout.pointerOver == card && card.card is Ability ability)
		{
			_BeginAbilityTooltipTimer(ability, (card.layout == playerAbilityDeckLayout.hand) ? CardTooltipLayout.DirectionAlongAxis.Negative : CardTooltipLayout.DirectionAlongAxis.Auto);
		}
	}

	private void _OnAbilityHoverLayoutCardRemoved(ACardLayout layout, CardLayoutElement card)
	{
		if (card.card is Ability ability)
		{
			_EndAbilityTooltipTimer(ability, isPointerExit: false);
		}
	}

	private void _OnMainHandScaleChange(float scale)
	{
		if ((bool)_mainHandScale)
		{
			_mainHandScale.scale = scale;
		}
		if ((bool)inspectDragPlane)
		{
			inspectDragPlane.localScale = scale.ToVector3();
		}
		if ((bool)inspectDragPlaneFlipped)
		{
			inspectDragPlaneFlipped.localScale = scale.ToVector3();
		}
	}

	private void _OnCommonResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (newPile == ResourceCard.Pile.DiscardPile && (bool)card.ephemeral)
		{
			card.gameState.exileDeck.Transfer(card, card.exilePile);
		}
	}

	private void _OnEnemyResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		_OnCommonResourceTransfer(card, oldPile, newPile);
		if (oldPile.HasValue && oldPile.GetValueOrDefault().IsCombat() && !(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(newPile))
		{
			card.view.offsets.Clear();
		}
	}

	private void _OnPlayerResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		_OnCommonResourceTransfer(card, oldPile, newPile);
	}

	private void _OnAbilityTransfer(Ability card, Ability.Pile? oldPile, Ability.Pile? newPile)
	{
		if (newPile == Ability.Pile.Discard)
		{
			if ((bool)card.ephemeral)
			{
				card.gameState.exileDeck.Transfer(card, ExilePile.PlayerResource);
			}
			ATextMeshProAnimator.EndHighlights((card.view as AbilityCardView)?.nameText.gameObject);
		}
	}

	private void _OnResourcePointerEnter(ResourceCard.Pile pile, ResourceCard card)
	{
		(card.view as ResourceCardView)?.OnPointerEnter();
	}

	private void _OnResourcePointerExit(ResourceCard.Pile pile, ResourceCard card)
	{
		(card.view as ResourceCardView)?.OnPointerExit();
	}

	private void _OnGameStepEnableChange(GameStep step, bool isEnabled)
	{
		if (!isEnabled && !step.isParallel)
		{
			ATargetView.ClearAllGlowRequestsExcept(GlowTags.Persistent);
			TargetLineView.RemoveAllExcept(TargetLineTags.Persistent);
			ClearError();
			ResourceCard.Piles piles2 = (enemyWildPiles = (ResourceCard.Piles)0);
			wildPiles = piles2;
			_hoverOverCombatantTime = 0f;
			if (_hoverOverCombatantTooltipActive && !step.canSafelyCancelStack)
			{
				_EndCombatantTooltipTimer(_hoverOverCombatant, isPointerExit: false);
				_BeginCombatantTooltipTimer(_hoverOverCombatant);
			}
		}
	}

	private void _OnButtonDeckTransfer(ButtonCard card, ButtonCard.Pile? oldPile, ButtonCard.Pile? newPile)
	{
		ClearError();
		_cancelStoneDirty = true;
	}

	private void _OnDamageDealt(ActionContext context, AAction action, int damage, DamageSource source)
	{
		if (context.target is ACombatant aCombatant && aCombatant.pile == AdventureCard.Pile.TurnOrder)
		{
			context.target.view.velocities.position += Mathf.Pow(damage, 0.333f) * (Quaternion.AngleAxis(state.cosmeticRandom.Range(-10f, 10f), Vector3.up) * (context.target.view.transform.position - context.actor.view.transform.position).Project(AxisType.Y).Unproject(AxisType.Y).NormalizeSafe(-Vector3.forward)) * 0.4f;
		}
	}

	private void _OnChipPointerEnter(Chip.Pile pile, Chip card)
	{
		if (pile == Chip.Pile.Attack || pile == Chip.Pile.ActiveAttack)
		{
			ProjectedTooltipFitter.Create((pile == Chip.Pile.ActiveAttack) ? ButtonCardType.ConfirmAttack.GetTooltip() : ((NumberOfAttacks)state.player.numberOfAttacks.value).GetMessage(), card.view.layout.gameObject, tooltipCanvas);
		}
	}

	private void _OnChipPointerExit(Chip.Pile pile, Chip card)
	{
		if (pile == Chip.Pile.Attack || pile == Chip.Pile.ActiveAttack)
		{
			ProjectedTooltipFitter.Finish(card.view.layout.gameObject);
		}
	}

	private void _OnStonePointerClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.Cancel && !state.compassDeck.Any(MapCompass.Pile.Active))
		{
			this.onBackPressed?.Invoke();
		}
	}

	private void _OnStonePointerEnter(Stone.Pile pile, Stone card)
	{
		if ((StoneType)card == StoneType.Turn)
		{
			ProjectedTooltipFitter.Create(pile.GetTooltip(), card.view.gameObject, tooltipCanvas, TooltipAlignment.BottomCenter);
		}
		else if (pile == Stone.Pile.Cancel && !_cancelStoneDirty)
		{
			ProjectedTooltipFitter.Create(cancelStoneTooltipOverrides.LastOrDefault()?.Localize() ?? buttonDeckLayout.deck.NextInPile(ButtonCard.Pile.Active)?.type.GetTooltip(), card.view.gameObject, tooltipCanvas);
		}
	}

	private void _OnStonePointerExit(Stone.Pile pile, Stone card)
	{
		ProjectedTooltipFitter.Finish(card.view.gameObject);
	}

	private void _OnExileTransfer(ATarget card, ExilePile? oldPile, ExilePile? newPile)
	{
		state.ClearAppliedActionsOn<AAction>(card);
	}

	private void _OnExileRest(ExilePile pile, ATarget card)
	{
		if (!state.saving)
		{
			if (card is ResourceCard resourceCard && !resourceCard.permanent)
			{
				resourceCard.view.DestroyCard();
			}
			else if (card is Ability ability && !ability.permanent)
			{
				ability.view.DestroyCard();
			}
			else if (pile == ExilePile.ClearGameState && card.canBePooled && (bool)card.view)
			{
				card.view.RepoolCard();
			}
			else if (card is Stone stone && (bool)stone.view && stone.type == StoneType.Cancel)
			{
				stone.view.DestroyCard();
			}
			else if (pile == ExilePile.ClearGameState && card is Leaderboard leaderboard && (bool)leaderboard.view)
			{
				leaderboard.view.DestroyCard();
			}
		}
	}

	private void _OnExilePointerExit(ExilePile pile, ATarget card)
	{
		ProjectedTooltipFitter.Finish(card.view.gameObject);
	}

	private void _OnRewardRest(RewardPile pile, ATarget card)
	{
		if (pile == RewardPile.Discard)
		{
			card.view.DestroyCard();
		}
	}

	private void _OnAdventureRest(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.Discard && card.view.layout == rewardDeckLayout.discard)
		{
			card.view.DestroyCard();
		}
	}

	private void _OnGameStonePointerEnter(GameStone.Pile pile, GameStone card)
	{
		ProjectedTooltipFitter.Create(card.game.data.GetTitle(), card.view.gameObject, tooltipCanvas);
	}

	private void _OnGameStonePointerExit(GameStone.Pile pile, GameStone card)
	{
		ProjectedTooltipFitter.Finish(card.view.gameObject);
	}

	private void _OnMapRest(ProceduralMap.Pile pile, ProceduralMap card)
	{
		if (pile == ProceduralMap.Pile.Closed)
		{
			card.view.inputEnabled = false;
		}
	}

	private void _AddLayoutsToDepthOfFieldManagers()
	{
		_AddLayoutToDepthOfFieldManagers(adventureDeckLayout.selectionHand);
		_AddLayoutToDepthOfFieldManagers(playerAbilityDeckLayout.select);
		_AddLayoutToDepthOfFieldManagers(playerResourceDeckLayout.select);
		_AddLayoutToDepthOfFieldManagers(rewardDeckLayout.select);
		foreach (ACardLayout item in (rewardDeckLayout.cardPackSelect as ACardLayoutGroup)?.GetLayouts().AsEnumerable() ?? Enumerable.Repeat(rewardDeckLayout.cardPackSelect, 1))
		{
			_AddLayoutToDepthOfFieldManagers(item);
		}
		_AddLayoutToDepthOfFieldManagers(stoneDeckLayout.cancelFloating);
		_AddLayoutToDepthOfFieldManagers(mapDeckLayout.active);
		foreach (ACardLayout cardLayout in heroDeckLayout.selectionHandUnrestricted.GetCardLayouts())
		{
			_AddLayoutToDepthOfFieldManagers(cardLayout);
		}
		_AddLayoutToDepthOfFieldManagers(compassDeckLayout.activeFloating);
		_AddLayoutToDepthOfFieldManagers(rewardDeckLayout.leaderboard);
	}

	private void _AddLayoutToDepthOfFieldManagers(ACardLayout layout)
	{
		if (_depthOfFieldLayouts.Add(layout))
		{
			layout.onCardAdded += _OnAddedToDepthOfFieldLayout;
			layout.onCardRemoved += _OnRemovedFromDepthOfFieldLayout;
		}
	}

	private void _OnAddedToDepthOfFieldLayout(ACardLayout layout, CardLayoutElement card)
	{
		dofShifter.AddTarget(card.transform);
	}

	private void _OnRemovedFromDepthOfFieldLayout(ACardLayout layout, CardLayoutElement card)
	{
		dofShifter.RemoveTarget(card.transform);
	}

	private void _OnDragBegan(ACardLayout layout, CardLayoutElement card)
	{
		if (layout.draggingEntireHand)
		{
			dofShifter.focalLengthOverride = 0f;
		}
		else
		{
			dofShifter.targetOverrides.Add(card.transform);
		}
		if (card.card is Ability && card.deck is AppliedAbilityDeckLayout)
		{
			_OnAbilityHoverLayoutDragBegin(null, card);
			ACombatant aCombatant = card.deck.GetComponentInParent<CombatantCardView>()?.combatant;
			if (aCombatant != null)
			{
				_EndCombatantTooltipTimer(aCombatant, isPointerExit: false);
			}
		}
		else if (card.card is Enemy enemy && enemy.pile == AdventureCard.Pile.TurnOrder)
		{
			_EndCombatantTooltipTimer(enemy, isPointerExit: false);
		}
	}

	private void _OnDragEnded(ACardLayout layout, CardLayoutElement card)
	{
		dofShifter.focalLengthOverride = null;
		dofShifter.targetOverrides.Remove(card.transform);
		if (card.card is Ability && card.deck is AppliedAbilityDeckLayout)
		{
			_OnAbilityHoverLayoutDragEnd(null, card);
			ACombatant aCombatant = card.deck.GetComponentInParent<CombatantCardView>()?.combatant;
			if (aCombatant != null && aCombatant is Enemy && aCombatant.view.layout.pointerOver == aCombatant.view)
			{
				_BeginCombatantTooltipTimer(aCombatant);
			}
		}
	}

	private void _OnPointerEnter(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (target is Ability ability && (deckLayout is AppliedAbilityDeckLayout || (ability.isSummon && ability.inTurnOrder && ability.view.layout == adventureDeckLayout.inspectHand)))
		{
			_OnPlayerAbilityPointerEnter(Ability.Pile.HeroPassive, ability);
		}
	}

	private void _OnPointerExit(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (target is Ability ability && (deckLayout is AppliedAbilityDeckLayout || (ability.isSummon && ability.inTurnOrder && ability.view.layout == adventureDeckLayout.inspectHand)))
		{
			_OnPlayerAbilityPointerExit(Ability.Pile.HeroPassive, ability);
		}
	}

	private void _UpdateInputs()
	{
		if (!CanvasInputFocus.HasActiveComponents)
		{
			if (InputManager.I[KeyCode.Mouse0][KState.Clicked])
			{
				this.onClick?.Invoke();
			}
			if (InputManager.I[KeyAction.Finish][KState.Clicked])
			{
				this.onConfirmPressed?.Invoke();
			}
			else if (InputManager.I[KeyAction.Back][KState.Clicked])
			{
				this.onBackPressed?.Invoke();
			}
		}
	}

	private void _UpdateCancelStone()
	{
		if (!_cancelStoneDirty)
		{
			return;
		}
		_cancelStoneDirty = false;
		bool flag = false;
		if (ProfileManager.options.game.ui.cancelButtonEnabled)
		{
			foreach (ButtonCard item in buttonDeckLayout.deck.GetCardsSafe(ButtonCard.Pile.Active))
			{
				if (ButtonCard.CancelButtons.Contains(item.type))
				{
					flag = true;
					break;
				}
			}
		}
		stoneDeckLayout.Transfer(StoneType.Cancel, flag ? Stone.Pile.Cancel : Stone.Pile.CancelInactive);
	}

	private void _UpdateTooltipHandPosition()
	{
		if (!_tooltipHandData)
		{
			return;
		}
		Rect3D rect3D = default(Rect3D);
		if (_tooltipHandData.creator is RectTransform rectTransform)
		{
			rect3D = new Rect3D(rectTransform);
		}
		Rect3D rect3D2 = rect3D.WorldToViewportRect(Camera.main);
		float num2 = (_tooltipHandData.pivot = ((rect3D2.center.x < _tooltipHandData.pivotThreshold) ? 1 : 0));
		float num3 = num2;
		if (_tooltipHandData.pivotOverride.HasValue)
		{
			num3 = _tooltipHandData.pivotOverride.Value;
		}
		Transform cardContainer = _tooltipHand.cardContainer;
		Transform obj = _tooltipHand.layoutTarget.transform;
		obj.position = rect3D.Lerp(new Vector2(num3, 0.5f)) - Camera.main.transform.forward * 0.0075f + Camera.main.transform.right * _tooltipHand.size.width * MathUtil.Remap(num3, new Vector2(0f, 1f), new Vector2(-0.55f, 0.55f));
		_tooltipHand.settings.alignmentCenter = num3;
		float num4 = Vector3.Dot(obj.position - Camera.main.transform.position, Camera.main.transform.forward);
		Rect3D worldFrustumRectAtDepth = Camera.main.GetWorldFrustumRectAtDepth(num4);
		Vector3 vector = obj.forward * _tooltipHand.size.height;
		Vector3 vector2 = obj.right * _tooltipHand.size.width;
		Vector3 vector3 = obj.position - vector * 0.5f - vector2 * (1f - num3);
		Vector3 vector4 = vector3 + vector;
		Rect3D rect3D3 = new Rect3D(vector3, vector4, vector4 + vector2);
		Rect3D rect3D4 = rect3D3.FitIntoRange(worldFrustumRectAtDepth.min, worldFrustumRectAtDepth.max);
		obj.position += rect3D4.bottomLeft - rect3D3.bottomLeft;
		_tooltipHand.settings.maxTotalSpacing = ((num3 > 0f) ? Vector3.Dot(worldFrustumRectAtDepth.topRight - rect3D4.bottomLeft, worldFrustumRectAtDepth.right) : Vector3.Dot(rect3D4.topRight - worldFrustumRectAtDepth.bottomLeft, worldFrustumRectAtDepth.right)) / _tooltipHand.size.width + 0.5f;
		cardContainer.transform.position = Camera.main.transform.position + Camera.main.transform.forward * num4;
		if (!_tooltipHandData.Update(_tooltipHandGracePeriod))
		{
			return;
		}
		if (_tooltipHandData.show)
		{
			foreach (ATarget item in _tooltipHandData.generateCards())
			{
				state.exileDeck.Add(item, ExilePile.Character);
				_tooltipHand.Add(item.view);
				if (item is Enemy enemy)
				{
					enemy.enemyCard.ShowTraitsWithoutFadingInactive();
				}
				item.view.ClearExitTransitions();
				item.view.frontIsVisible = true;
			}
			return;
		}
		_HideTooltipHand();
	}

	private void _HideTooltipHand()
	{
		_tooltipHandData.creator = null;
		foreach (CardLayoutElement card in _tooltipHand.GetCards())
		{
			bool num = card.targets.Count > 0;
			state.exileDeck.Transfer(card.card, ExilePile.ClearGameState);
			if (num)
			{
				_OnExileRest(ExilePile.ClearGameState, card.card);
			}
			else
			{
				card.ClearEnterTransitions();
			}
		}
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Update()
	{
		_UpdateInputs();
		state?.stack.Update();
	}

	private void LateUpdate()
	{
		if (_hoverOverAbility != null && !_hoverOverAbilityTooltipActive && (_hoverOverAbilityTime += Time.deltaTime) >= abilityHoverOverTooltipTime && (_hoverOverAbilityTooltipActive = true))
		{
			_hoverOverAbility.abilityCard?.ShowTooltips(_hoverOverAbilityTooltipDirection);
		}
		if (_hoverOverCombatant != null && !_hoverOverCombatantTooltipActive)
		{
			GameStep activeStep = state.stack.activeStep;
			if (activeStep != null && activeStep.canSafelyCancelStack && (_hoverOverCombatantTime += Time.deltaTime) >= ProfileManager.options.game.ui.enemyHoverOverTooltipTime && (_hoverOverCombatantTooltipActive = true))
			{
				int value = _hoverOverCombatant.turnOrder - state.player.turnOrder;
				_hoverOverCombatant.combatantCard?.ShowTooltips((Math.Abs(value) == 1 && state.player.view.hasOffset) ? ((Math.Sign(value) != 1) ? CardTooltipLayout.DirectionAlongAxis.Negative : CardTooltipLayout.DirectionAlongAxis.Positive) : CardTooltipLayout.DirectionAlongAxis.Auto);
				_hoverOverCombatantOffset = GetCombatantOffset(_hoverOverCombatant.view, _hoverOverCombatant.view.hasOffset);
				_hoverOverCombatant.view.offsets.Insert(0, _hoverOverCombatantOffset);
				_hoverOverCombatant.view.layout.SetPointerOverPaddingOverride(new CardLayoutElement.PointerOverPadding(new Vector2(0.333f, 0.333f), Vector2.zero, new Vector2(0.47f, (_hoverOverCombatant[AppliedPile.Debuff] != null) ? 0f : 0.15f)));
				_hoverOverCombatant.combatantCard?.appliedAbilityLayout.debuff.SetPointerOverPaddingOverride(new CardLayoutElement.PointerOverPadding(new Vector2(0.333f, 0.333f), Vector2.zero, new Vector2(0.47f, 0f)));
				_hoverOverCombatant.combatantCard?.onTappedChange?.Invoke(arg0: false);
			}
		}
		if (_hoverOverCombatantTooltipActive)
		{
			ACombatant hoverOverCombatant = _hoverOverCombatant;
			if (hoverOverCombatant != null && hoverOverCombatant.view?.offsets.Count > 1 && _hoverOverCombatant.view.offsets.Contains(AGameStepTurn.OFFSET) && _hoverOverCombatantOffset.rotation != Quaternion.identity)
			{
				_hoverOverCombatant.view.offsets[0] = (_hoverOverCombatantOffset = GetCombatantOffset(_hoverOverCombatant.view, hasOffset: true));
			}
			else
			{
				ACombatant hoverOverCombatant2 = _hoverOverCombatant;
				if (hoverOverCombatant2 != null && hoverOverCombatant2.view?.offsets.Count == 1 && _hoverOverCombatantOffset.rotation == Quaternion.identity)
				{
					_hoverOverCombatant.view.offsets[0] = (_hoverOverCombatantOffset = GetCombatantOffset(_hoverOverCombatant.view, hasOffset: false));
				}
			}
		}
		Transform transform = stoneDeckLayout?.playerTurn?.transform.parent;
		if ((object)transform != null)
		{
			CardLayoutElement cardLayoutElement = turnOrderSpaceLayout.FirstActive();
			if ((object)cardLayoutElement != null)
			{
				float value2 = cardLayoutElement.GetLayoutTarget().position.x + turnStoneOffset;
				foreach (ACardLayout item in transform.gameObject.GetComponentsInChildrenPooled<ACardLayout>())
				{
					item.layoutTarget.transform.position = item.layoutTarget.transform.position.SetAxis(AxisType.X, value2);
				}
			}
		}
		_UpdateCancelStone();
		_UpdateTooltipHandPosition();
		_abilitiesThatAreProcessingPotentialDamage.Clear();
		Matrix4x4 GetCombatantOffset(CardLayoutElement card, bool hasOffset)
		{
			if (hasOffset)
			{
				return Matrix4x4.Translate(GetCombatantTranslation(card, new Vector3(0f, 0.1f, 0f)) - new Vector3(0f, AGameStepTurn.OFFSET.GetTranslation().y, 0f));
			}
			return Matrix4x4.TRS(GetCombatantTranslation(card, new Vector3(0f, 0.1f, 0f)), AGameStepTurn.OFFSET.rotation, Vector3.one);
		}
		Vector3 GetCombatantTranslation(CardLayoutElement card, Vector3 offset)
		{
			Vector3 vector = card.GetLayoutTarget().position + new Vector3(0f, 0f, (_hoverOverCombatant?[AppliedPile.Debuff] != null) ? (-0.025f) : (-0.075f));
			return Camera.main.GetPositionOnPlaneFromADifferentDistance(vector, offset) - vector;
		}
	}

	private void OnDestroy()
	{
		state?.stack?.Unregister();
		ACardLayout.OnDragBegan -= _OnDragBegan;
		ACardLayout.OnDragEnded -= _OnDragEnded;
		ADeckLayoutBase.OnPointerEnter -= _OnPointerEnter;
		ADeckLayoutBase.OnPointerExit -= _OnPointerExit;
		ProfileOptions.GameOptions.UIOptions.OnMainHandScaleChange -= _OnMainHandScaleChange;
	}

	public void RefreshPointerOver()
	{
		this.onRefreshPointerOverRequest?.Invoke();
	}

	public bool WildInputEnabled(ResourceCard card)
	{
		if (card != null && state.parameters.adventureStarted && ((card.faction == Faction.Player) ? wildPiles : enemyWildPiles).Contains(card.pile))
		{
			return card.hasWild;
		}
		return false;
	}

	public void ClearPersistentAttackGlowsAndLines()
	{
		ATargetView.ClearAllGlowRequests(GlowTags.Persistent | GlowTags.Attack);
		TargetLineView.RemoveAll(TargetLineTags.Attack | TargetLineTags.Persistent);
	}

	public void OffsetTurnOrderForCombat(bool offset)
	{
		float value = (offset ? (turnStoneOffset * -0.5f) : 0f);
		ACardLayout layout = adventureDeckLayout.GetLayout(AdventureCard.Pile.TurnOrder);
		if ((object)layout != null)
		{
			layout.layoutTarget.transform.localPosition = layout.layoutTarget.transform.localPosition.SetAxis(AxisType.X, value);
		}
		ACardLayout layout2 = turnOrderSpaceLayout.GetLayout(TurnOrderSpace.Pile.Active);
		if ((object)layout2 != null)
		{
			layout2.layoutTarget.transform.localPosition = layout2.layoutTarget.transform.localPosition.SetAxis(AxisType.X, value);
		}
	}

	public void ForceUpdateCancelStone()
	{
		_UpdateCancelStone();
	}

	public void AddCancelStoneTooltipOverride(LocalizedString localizedString)
	{
		cancelStoneTooltipOverrides.Add(localizedString);
	}

	public void RemoveCancelStoneTooltipOverride(LocalizedString localizedString)
	{
		cancelStoneTooltipOverrides.RemoveFromEnd(localizedString);
	}

	public void SignalAbilityProcessingDamage(Ability ability)
	{
		if (ability != null)
		{
			_abilitiesThatAreProcessingPotentialDamage.Add(ability);
		}
	}

	public IEnumerable<Ability> GetAbilitiesThatProcessedDamage()
	{
		foreach (Ability item in _abilitiesThatAreProcessingPotentialDamage)
		{
			yield return item;
		}
		_abilitiesThatAreProcessingPotentialDamage.Clear();
	}

	public bool RequestAbilityHighlight(Ability ability)
	{
		int num2 = (_abilityHighlightRequests[ability.id] = _abilityHighlightRequests.GetValueOrDefault(ability.id) + 1);
		return num2 == 1;
	}

	public bool ReleaseAbilityHighlightRequest(Ability ability)
	{
		int num2 = (_abilityHighlightRequests[ability.id] = _abilityHighlightRequests.GetValueOrDefault(ability.id) - 1);
		if (num2 <= 0)
		{
			return _abilityHighlightRequests.Remove(ability.id);
		}
		return false;
	}

	public void BlockRaycast(Transform layoutPlane, float offset = 0.01f)
	{
		_raycastBlocker.transform.CopyFrom(layoutPlane, copyPosition: true, copyRotation: true, copyScale: false);
		_raycastBlocker.transform.position += Camera.main.transform.forward * offset;
		_raycastBlocker.SetActive(value: true);
	}

	public void UnblockRaycast()
	{
		_raycastBlocker.SetActive(value: false);
	}

	public void ShowCardsAsTooltip(Func<IEnumerable<ATarget>> generateCards, Transform tooltipCreator, float? pivotOverride = null)
	{
		if ((bool)_tooltipHandData && _tooltipHandData.creator != tooltipCreator)
		{
			_HideTooltipHand();
		}
		_tooltipHandData.creator = tooltipCreator;
		_tooltipHandData.generateCards = generateCards;
		_tooltipHandData.show = true;
		_tooltipHandData.pivotOverride = pivotOverride;
	}

	public void HideCardsShownAsTooltip(bool immediate = false)
	{
		_tooltipHandData.show = false;
		if (immediate)
		{
			_tooltipHandData.elapsed = _tooltipHandGracePeriod - 0.0001f;
		}
	}

	public void CheckForExiledCardsAtRest()
	{
		foreach (ATarget item in state.exileDeck.GetCardsSafe())
		{
			ATargetView view = item.view;
			if ((object)view != null && view.atRestInLayout)
			{
				_OnExileRest(state.exileDeck[item] ?? ExilePile.ClearGameState, item);
			}
		}
	}

	public GameStep InspectAllAbilitiesInDeckStep()
	{
		Ability.Piles flags = Ability.Piles.Draw | Ability.Piles.Hand | Ability.Piles.ActivationHand | Ability.Piles.Discard;
		int num = state.abilityDeck.Count(flags);
		return state.abilityDeck.InspectStep(flags, Comparer<Ability>.Default, AbilityDataRefEqualityComparer.Default, MessageData.GameTooltips.ViewAbilityDeck, (num < 25) ? null : adventureDeckLayout.inspectHugeGroupSettings, state.GetAbilitiesInTurnOrder(), (Ability a) => a.view.deck != playerAbilityDeckLayout, Ability.OnEnterInspect, Ability.OnExitInspect);
	}

	private void _CreateMessage(AnimatedMessage blueprint, RectTransform container, string message)
	{
		Pools.Unpool(blueprint.gameObject, container).GetComponent<AnimatedMessage>().SetMessage(message);
	}

	private void _CreateMessage(AnimatedMessage blueprint, RectTransform container, LocalizedString message)
	{
		Pools.Unpool(blueprint.gameObject, container).GetComponent<AnimatedMessage>().SetMessage(message);
	}

	private void _ClearMessages(RectTransform messageContainer)
	{
		if (messageContainer.childCount <= 0)
		{
			return;
		}
		foreach (AnimatedMessage item in messageContainer.gameObject.GetComponentsInChildrenPooled<AnimatedMessage>())
		{
			item.Finish();
		}
	}

	private bool _IsMessageActive(RectTransform messageContainer, LocalizedString localizedString)
	{
		if (messageContainer.childCount > 0)
		{
			foreach (AnimatedMessage item in messageContainer.gameObject.GetComponentsInChildrenPooled<AnimatedMessage>())
			{
				if (item.activeMessage == localizedString)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearError()
	{
		_ClearMessages(_errorMessageContainer);
	}

	public void ClearMessage()
	{
		_ClearMessages(_mainMessageContainer);
	}

	public void LogError(LocalizedString error, SoundPack soundPack = null, Transform playAudioFrom = null)
	{
		if (error.Localize().HasVisibleCharacter())
		{
			ClearError();
			_CreateMessage(_errorMessageBlueprint, _errorMessageContainer, error);
			if ((bool)soundPack)
			{
				VoiceManager.Instance.Play(playAudioFrom ?? state.player.view.transform, soundPack, interrupt: true);
			}
		}
	}

	public void LogMessage(LocalizedString message)
	{
		if (message.Localize().HasVisibleCharacter())
		{
			ClearMessage();
			_CreateMessage(_mainMessageBlueprint, _mainMessageContainer, message);
		}
	}

	public void UpdateLogMessage(LocalizedString message)
	{
		if (!_IsMessageActive(_mainMessageContainer, message))
		{
			LogMessage(message);
		}
	}

	public LocalizedString GetActiveMessage()
	{
		if (_mainMessageContainer.childCount > 0)
		{
			foreach (AnimatedMessage item in _mainMessageContainer.gameObject.GetComponentsInChildrenPooled<AnimatedMessage>())
			{
				LocalizedString activeMessage = item.activeMessage;
				if (activeMessage != null)
				{
					return activeMessage;
				}
			}
		}
		return null;
	}
}
