using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class CombatantCardView : AdventureTargetView
{
	[Header("Combatant")]
	public StringEvent onOffenseChange;

	public StringEvent onDefenseChange;

	public StringEvent onHealthChange;

	public IntEvent onPotentialDamageChange;

	public BoolEvent onShowPotentialDamageChange;

	public BoolEvent onShowPotentialDamageEnabledChange;

	public BoolEvent onTappedChange;

	public BoolEvent onActiveTappedChange;

	public BoolEvent onCannotUntapChange;

	public Transform hpDiceContainer;

	public Transform shieldDiceContainer;

	public TextMeshProUGUI offenseText;

	public AnimatedImage offenseIcon;

	public TextMeshProUGUI healthText;

	public AnimatedImage healthIcon;

	public TextMeshProUGUI defenseText;

	public AnimatedImage defenseIcon;

	public CardTooltipLayout tooltipLayout;

	private AppliedAbilityDeckLayout _appliedAbilityLayout;

	private AppliedResourceDeckLayout _appliedResourceLayout;

	protected DiceIntView _hpDice;

	protected DiceIntView _shieldDice;

	private CombatText _combatText;

	private bool _showPotentialDamage;

	private int _potentialDamage = int.MinValue;

	private readonly List<(object requester, int damage, PoolKeepItemListHandle<Ability> abilitiesThatProcessedDamage)> _potentialDamages = new List<(object, int, PoolKeepItemListHandle<Ability>)>();

	public ACombatant combatant => (ACombatant)base.target;

	public AppliedAbilityDeckLayout appliedAbilityLayout => base.gameObject.CacheComponentInChildren(ref _appliedAbilityLayout, includeInactive: true);

	public AppliedResourceDeckLayout appliedResourceLayout => base.gameObject.CacheComponentInChildren(ref _appliedResourceLayout, includeInactive: true);

	protected CombatText combatText => this.CacheComponentInChildren(ref _combatText, includeInactive: true);

	private bool showPotentialDamage
	{
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showPotentialDamage, value))
			{
				onShowPotentialDamageChange?.Invoke(_showPotentialDamage);
			}
		}
	}

	private int potentialDamage
	{
		set
		{
			if (SetPropertyUtility.SetStruct(ref _potentialDamage, value) && value >= 0)
			{
				onPotentialDamageChange?.Invoke(value);
			}
		}
	}

	public int displayedOffense
	{
		get
		{
			if (!int.TryParse(offenseText.text, out var result))
			{
				return combatant.stats.offense;
			}
			return result;
		}
	}

	public int displayedDefense
	{
		get
		{
			if (!int.TryParse(defenseText.text, out var result))
			{
				return combatant.stats.defense;
			}
			return result;
		}
	}

	public DiceType? hpDiceType => _hpDice?.diceType;

	public bool generateDiceWhenPlacedIntoTurnOrder { get; set; } = true;


	private void _CommonStatChange(StringEvent stringEvent, int amount, bool hideIfZero = false)
	{
		stringEvent?.Invoke((amount == 0 && hideIfZero) ? "" : amount.ToString());
	}

	private void _OnHealthChange(int previousHealth, int health)
	{
		_CommonStatChange(onHealthChange, health);
		if (combatant.pile == AdventureCard.Pile.TurnOrder)
		{
			if ((bool)_hpDice && _hpDice.max < health)
			{
				_RemoveDice(ref _hpDice);
			}
			if (!_hpDice && health > 0 && (int)combatant.HP > 0)
			{
				_GenerateDice(ref _hpDice, hpDiceContainer, DiceSkinType.HP, combatant.HP, combatant.stats.health);
			}
		}
	}

	private void _OnHPChange(int previousHP, int hp)
	{
		if (previousHP <= 0 && hp > 0)
		{
			_GenerateDice(ref _hpDice, hpDiceContainer, DiceSkinType.HP, hp, combatant.stats.health);
		}
		if ((bool)_hpDice)
		{
			_hpDice.value = hp;
		}
		if (hp <= 0)
		{
			_RemoveDice(ref _hpDice);
		}
		if (combatant.pile == AdventureCard.Pile.TurnOrder)
		{
			if (hp < previousHP)
			{
				VoiceManager.Instance.Play(base.transform, combatant.audio.Hurt(previousHP - hp), interrupt: true);
			}
			if (hp != previousHP)
			{
				combatText.AddDelta(hp - previousHP);
			}
		}
	}

	protected virtual void _OnShieldChange(int previousShield, int shield)
	{
		if (shield > 0)
		{
			if ((bool)_shieldDice && _shieldDice.max < shield)
			{
				_RemoveDice(ref _shieldDice);
			}
			if (!_shieldDice)
			{
				_GenerateDice(ref _shieldDice, shieldDiceContainer, DiceSkinType.Shield, combatant.shield, combatant.shield);
			}
			_shieldDice.value = shield;
		}
		else
		{
			_RemoveDice(ref _shieldDice);
		}
		if (!combatant.gameState.encounterActive || base.view.suppressShieldEvents > 0)
		{
			return;
		}
		if (combatant.pile == AdventureCard.Pile.TurnOrder)
		{
			combatText.AddDelta(shield - previousShield);
		}
		if (previousShield > shield)
		{
			ProjectileMediaPack shieldLost = ContentRef.Defaults.media.shieldLost;
			if (shieldLost != null)
			{
				combatant.gameState.stack.Append(new GameStepProjectileMedia(shieldLost, new ActionContext(combatant, null, combatant)));
			}
		}
	}

	private void _OnTappedChange(bool tapped)
	{
		onTappedChange?.Invoke(tapped);
	}

	private void _OnActiveTappedChange(bool activeTapped)
	{
		onActiveTappedChange?.Invoke(activeTapped);
	}

	private void _OnCannotUntapChange(bool cannotUntap)
	{
		onCannotUntapChange?.Invoke(cannotUntap);
	}

	private void _OnAdventureCardTransfer(ATarget aTarget, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
	{
		if (aTarget != base.target)
		{
			return;
		}
		if (newPile == AdventureCard.Pile.TurnOrder)
		{
			if (generateDiceWhenPlacedIntoTurnOrder)
			{
				_GenerateDice(ref _hpDice, hpDiceContainer, DiceSkinType.HP, combatant.HP, combatant.stats.health);
			}
			base.ignoreOffsetInExitTarget = generateDiceWhenPlacedIntoTurnOrder;
			_OnShowPotentialDamageEnableChange(ProfileManager.options.game.ui.potentialCombatDamage);
			ProfileOptions.GameOptions.UIOptions.OnShowPotentialDamageChange += _OnShowPotentialDamageEnableChange;
			base.pointerDrag.OnBegin.AddListener(_OnTurnOrderDragBegin);
			base.pointerDrag.OnEnd.AddListener(_OnTurnOrderDragEnd);
		}
		else if (oldPile == AdventureCard.Pile.TurnOrder)
		{
			_RemoveDice(ref _hpDice);
			ProfileOptions.GameOptions.UIOptions.OnShowPotentialDamageChange -= _OnShowPotentialDamageEnableChange;
			base.pointerDrag.OnBegin.RemoveListener(_OnTurnOrderDragBegin);
			base.pointerDrag.OnEnd.RemoveListener(_OnTurnOrderDragEnd);
		}
	}

	private void _OnTurnOrderDragBegin(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && _potentialDamages.Count > 0)
		{
			ShowPotentialDamage(this, -1, -1);
		}
	}

	private void _OnTurnOrderDragEnd(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && _potentialDamages.Count > 0)
		{
			HidePotentialDamage(this);
		}
	}

	private void _GenerateDice(ref DiceIntView dice, Transform container, DiceSkinType type, int current, int max)
	{
		if (!dice)
		{
			dice = DiceIntView.Create(max, container).SetValueReturn(current);
			dice.GetComponentInChildren<Renderer>().sharedMaterial = dice.GetComponentInChildren<DiceSkin>()[combatant.faction, type];
			dice.GetComponentInChildren<TooltipVisibility>().OnShowTooltip.AddListener((type == DiceSkinType.HP) ? new UnityAction(_CreateHPTooltip) : new UnityAction(_CreateShieldTooltip));
			dice.GetComponentInChildren<TooltipVisibility>().OnHideTooltip.AddListener((type == DiceSkinType.HP) ? new UnityAction(_ClearHPTooltip) : new UnityAction(_ClearShieldTooltip));
		}
	}

	private void _RemoveDice(ref DiceIntView dice)
	{
		if ((bool)dice)
		{
			dice.transform.SetParent(null, worldPositionStays: true);
			dice.GetComponentInChildren<ToggleFloat>().toggle = false;
			dice.GetComponentInChildren<TooltipVisibility>().Clear();
			dice = null;
		}
	}

	private void _CreateHPTooltip()
	{
		ProjectedTooltipFitter.Create(MessageData.GameTooltips.HP.Localize().SetArguments(combatant.HP).Localize(), _hpDice.gameObject, GameStateView.Instance.tooltipCanvas, TooltipAlignment.MiddleRight);
	}

	private void _ClearHPTooltip()
	{
		ProjectedTooltipFitter.Finish(_hpDice.gameObject);
	}

	private void _CreateShieldTooltip()
	{
		ProjectedTooltipFitter.Create(MessageData.GameTooltips.Shield.Localize().SetArguments(combatant.shield).Localize(), _shieldDice.gameObject, GameStateView.Instance.tooltipCanvas, TooltipAlignment.MiddleRight);
	}

	private void _ClearShieldTooltip()
	{
		ProjectedTooltipFitter.Finish(_shieldDice.gameObject);
	}

	private void _EndPotentialDamageHighlightsAtIndex(int x)
	{
		foreach (Ability item in _potentialDamages[x].abilitiesThatProcessedDamage.value)
		{
			if (base.view.ReleaseAbilityHighlightRequest(item))
			{
				item.EndHighlightAbilityName();
			}
		}
	}

	private void _RemovePotentialDamageAtIndex(int x)
	{
		if (x == _potentialDamages.Count - 1)
		{
			_EndPotentialDamageHighlightsAtIndex(x);
		}
		Pools.Repool(_potentialDamages[x].abilitiesThatProcessedDamage);
		_potentialDamages.RemoveAt(x);
	}

	private void _UpdatePotentialDamage()
	{
		(object, int, PoolKeepItemListHandle<Ability>) tuple = _potentialDamages[^1];
		foreach (Ability item in tuple.Item3.value)
		{
			if (base.view.RequestAbilityHighlight(item))
			{
				item.HighlightAbilityName(float.MaxValue);
			}
		}
		potentialDamage = tuple.Item2;
		showPotentialDamage = tuple.Item2 >= 0;
	}

	private void _OnShowPotentialDamageEnableChange(bool enable)
	{
		onShowPotentialDamageEnabledChange?.Invoke(enable);
	}

	protected virtual IEnumerable<string> _GetTooltips()
	{
		yield break;
	}

	protected virtual void _OnOffenseChange(int previousOffense, int offense)
	{
		_CommonStatChange(onOffenseChange, offense);
	}

	protected virtual void _OnDefenseChange(int previousDefense, int defense)
	{
		_CommonStatChange(onDefenseChange, defense);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget is ACombatant aCombatant)
		{
			_UnregisterCombatant(aCombatant);
		}
		base._OnTargetChange(oldTarget, newTarget);
	}

	protected override void _OnCardChange()
	{
		base._OnCardChange();
		_OnOffenseChange(combatant.stats.offense, combatant.stats.offense);
		_OnDefenseChange(combatant.stats.defense, combatant.stats.defense);
		_OnHealthChange(combatant.stats.health, combatant.stats.health);
		_OnHPChange(combatant.HP, combatant.HP);
		_OnShieldChange(combatant.shield, combatant.shield);
		combatant.stats[StatType.Offense].onValueChanged += _OnOffenseChange;
		combatant.stats[StatType.Defense].onValueChanged += _OnDefenseChange;
		combatant.stats[StatType.Health].onValueChanged += _OnHealthChange;
		combatant.HP.onValueChanged += _OnHPChange;
		combatant.shield.onValueChanged += _OnShieldChange;
		AppliedAbilityDeckLayout appliedAbilityDeckLayout = appliedAbilityLayout;
		List<AppliedPile> faceDownStacks = (appliedAbilityLayout.faceUpStacks = new List<AppliedPile>());
		appliedAbilityDeckLayout.faceDownStacks = faceDownStacks;
		appliedAbilityLayout.deck = combatant.appliedAbilities;
		appliedResourceLayout.deck = combatant.appliedResources;
		foreach (CardHandLayout item in base.gameObject.GetComponentsInChildrenPooled<CardHandLayout>())
		{
			item.dragTarget.transform = GameStateView.Instance?.inspectDragPlane;
		}
		_OnTappedChange(combatant.tapped);
		if ((bool)combatant.activeTap)
		{
			_OnActiveTappedChange(activeTapped: true);
		}
		if ((bool)combatant.statuses.cannotUntap)
		{
			_OnCannotUntapChange(cannotUntap: true);
		}
		combatant.tapped.onValueChanged += _OnTappedChange;
		combatant.activeTap.onValueChanged += _OnActiveTappedChange;
		combatant.statuses.cannotUntap.onValueChanged += _OnCannotUntapChange;
		combatant.gameState.adventureDeck.onTransfer += _OnAdventureCardTransfer;
		if (combatant.pile == AdventureCard.Pile.TurnOrder)
		{
			_OnAdventureCardTransfer(combatant, null, AdventureCard.Pile.TurnOrder);
		}
	}

	protected virtual void _UnregisterCombatant(ACombatant combatant)
	{
		combatant.stats[StatType.Offense].onValueChanged -= _OnOffenseChange;
		combatant.stats[StatType.Defense].onValueChanged -= _OnDefenseChange;
		combatant.stats[StatType.Health].onValueChanged -= _OnHealthChange;
		combatant.HP.onValueChanged -= _OnHPChange;
		combatant.shield.onValueChanged -= _OnShieldChange;
		combatant.tapped.onValueChanged -= _OnTappedChange;
		combatant.activeTap.onValueChanged -= _OnActiveTappedChange;
		combatant.statuses.cannotUntap.onValueChanged -= _OnCannotUntapChange;
		combatant.gameState.adventureDeck.onTransfer -= _OnAdventureCardTransfer;
	}

	public override void ShowTooltips()
	{
		tooltipLayout.Show(_GetTooltips());
	}

	public void ShowTooltips(CardTooltipLayout.DirectionAlongAxis direction)
	{
		tooltipLayout.Show(_GetTooltips(), direction);
	}

	public override void HideTooltips()
	{
		tooltipLayout.Hide();
	}

	public void ShowPotentialDamage(object requester, int damage, int minDamage = 0, IEnumerable<Ability> additionalAbilitiesToHighlight = null)
	{
		damage = Mathf.Max(minDamage, damage);
		for (int num = _potentialDamages.Count - 1; num >= 0; num--)
		{
			if (requester == _potentialDamages[num].requester)
			{
				_RemovePotentialDamageAtIndex(num);
				break;
			}
		}
		if (_potentialDamages.Count > 0)
		{
			_EndPotentialDamageHighlightsAtIndex(_potentialDamages.Count - 1);
		}
		_potentialDamages.Add((requester, damage, Pools.UseKeepItemList(base.view.GetAbilitiesThatProcessedDamage().ConcatIfNotNull(additionalAbilitiesToHighlight))));
		_UpdatePotentialDamage();
	}

	public void HidePotentialDamage(object requester)
	{
		int num = 0;
		for (int num2 = _potentialDamages.Count - 1; num2 >= 0; num2--)
		{
			if (requester == _potentialDamages[num2].requester)
			{
				_RemovePotentialDamageAtIndex(num = num2);
				break;
			}
		}
		if (_potentialDamages.Count == 0)
		{
			showPotentialDamage = false;
		}
		else if (num == _potentialDamages.Count)
		{
			_UpdatePotentialDamage();
		}
	}
}
