using System.Collections.Generic;
using System.Linq;

public class GameStepReaction : AGameStepTurnAbility
{
	private static Stack<GameStepReaction> _ActiveStack = new Stack<GameStepReaction>();

	private static GameStepReaction _Pending;

	private PoolDictionaryValuesHandle<Ability, PoolKeepItemHashSetHandle<AEntity>> _targets;

	private PoolKeepItemDictionaryHandle<Ability, Ability.ReactionAvailability> _availability;

	private PoolKeepItemDictionaryHandle<Ability, ReactionContext> _reactionContexts;

	private PoolKeepItemListHandle<ResourceCard> _previousResourceActivationHand;

	private PoolKeepItemListHandle<Ability> _previousAbilityActivationHand;

	private AbilityPreventedBy? _reactingTo;

	private float _elapsed;

	public static GameStepReaction Active => _ActiveStack.FirstOrDefault();

	public static GameStepReaction Pending
	{
		get
		{
			return _Pending ?? (_Pending = GameState.Instance.stack.Push(new GameStepReaction(GameState.Instance.player)) as GameStepReaction);
		}
		private set
		{
			_Pending = value;
		}
	}

	protected override ResourceCard.Piles _wildPiles => _reactingTo.WildPiles() ?? EnumUtil.Subtract(base._wildPiles, ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand);

	protected override ResourceCard.Piles _enemyWildPiles => _reactingTo.EnemyWildPiles() ?? base._enemyWildPiles;

	protected override bool canAct
	{
		get
		{
			if (base.canAct)
			{
				return base.player.HasReactionWithTarget();
			}
			return false;
		}
	}

	protected override Stone.Pile? _enabledTurnStonePile => Stone.Pile.PlayerReaction;

	protected override Stone.Pile? _disabledTurnStonePile => Stone.Pile.TurnInactive;

	protected override ControlGainType _controlType => ControlGainType.PlayerReaction;

	private GameStepReaction(AEntity entity)
		: base(entity)
	{
		_targets = Pools.UseDictionaryValues<Ability, PoolKeepItemHashSetHandle<AEntity>>();
		_availability = Pools.UseKeepItemDictionary<Ability, Ability.ReactionAvailability>();
		_reactionContexts = Pools.UseKeepItemDictionary<Ability, ReactionContext>();
	}

	private void _TransferActivationHandsIntoWaitingPiles()
	{
		_previousResourceActivationHand = base.resourceDeck.TransferPileReturn(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.ActivationHandWaiting);
		_previousAbilityActivationHand = base.abilityDeck.TransferPileReturn(Ability.Pile.ActivationHand, Ability.Pile.ActivationHandWaiting);
		foreach (ResourceCard item in _previousResourceActivationHand.value)
		{
			item.view.ClearTransitions();
		}
		foreach (Ability item2 in _previousAbilityActivationHand.value)
		{
			item2.view.ClearTransitions();
		}
	}

	private void _RestorePreviousActivationHands()
	{
		if ((bool)_previousResourceActivationHand)
		{
			foreach (ResourceCard item in _previousResourceActivationHand)
			{
				base.resourceDeck.Transfer(item, ResourceCard.Pile.ActivationHand).view.ClearTransitions();
			}
		}
		if (!_previousAbilityActivationHand)
		{
			return;
		}
		foreach (Ability item2 in _previousAbilityActivationHand)
		{
			base.abilityDeck.Transfer(item2, Ability.Pile.ActivationHand).view.ClearTransitions();
		}
	}

	protected override IEnumerable<ButtonCardType> _Buttons()
	{
		yield return ButtonCardType.EndReaction;
	}

	protected override void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		base._OnButtonClick(pile, card);
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.EndReaction)
		{
			Cancel();
		}
	}

	protected override void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.PlayerReaction)
		{
			Cancel();
		}
	}

	protected override void OnFirstEnabled()
	{
		base.OnFirstEnabled();
		Pending = null;
		_ActiveStack.Push(this);
		_TransferActivationHandsIntoWaitingPiles();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.canceled)
		{
			return;
		}
		foreach (KeyValuePair<Ability, ReactionContext> item in _reactionContexts.value)
		{
			item.Key.reactionContext = item.Value;
		}
		base.buttonDeckLayout[ButtonCardType.EndReaction].view.RequestGlow(this, Colors.TARGET);
		base.stoneDeckLayout[StoneType.Turn].view.RequestGlow(this, Colors.TARGET);
		base.view.LogMessage(_reactingTo.GetValueOrDefault().LocalizeReaction());
		_elapsed = 0f;
	}

	public override void Start()
	{
		base.state.activeCombat?.BeginShowPotentialDamage(base.state);
	}

	protected override void LateUpdate()
	{
		if (_TickTutorialTimer(ref _elapsed, 15f))
		{
			Stone stone = base.state.stoneDeck.NextInPile(Stone.Pile.PlayerReaction);
			if (stone != null)
			{
				ProjectedTooltipFitter.Create(Stone.Pile.PlayerReaction.GetTooltip(), stone.view.gameObject, base.view.tooltipCanvas, TooltipAlignment.BottomCenter);
			}
		}
		base.LateUpdate();
	}

	protected override void OnDisable()
	{
		base.view.ClearMessage();
		if (_elapsed >= 15f)
		{
			Stone stone = base.state.stoneDeck.NextInPile(Stone.Pile.PlayerReaction);
			if (stone != null)
			{
				ProjectedTooltipFitter.Finish(stone.view.gameObject);
			}
		}
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_ActiveStack.Pop();
		_RestorePreviousActivationHands();
		Pools.Repool(ref _targets);
		Pools.Repool(ref _availability);
		Pools.Repool(ref _reactionContexts);
		Pools.Repool(ref _previousResourceActivationHand);
		Pools.Repool(ref _previousAbilityActivationHand);
		base.state.activeCombat?.EndShowPotentialDamage(base.state);
	}

	public void AddTarget(Ability ability, AEntity target)
	{
		PoolDictionaryValuesHandle<Ability, PoolKeepItemHashSetHandle<AEntity>> targets = _targets;
		PoolKeepItemHashSetHandle<AEntity> obj = _targets.value.GetValueOrDefault(ability) ?? Pools.UseKeepItemHashSet<AEntity>();
		PoolKeepItemHashSetHandle<AEntity> poolKeepItemHashSetHandle = obj;
		targets[ability] = obj;
		poolKeepItemHashSetHandle.Add(target);
	}

	public void SetAvailability(Ability ability, Ability.ReactionAvailability availability)
	{
		AbilityPreventedBy? reactingTo = _reactingTo;
		if (!reactingTo.HasValue)
		{
			_reactingTo = ability.data.activation.canAlsoActivateWhen.Select((AAction.Trigger trigger) => trigger.abilityPreventedBy).FirstOrDefault((AbilityPreventedBy? preventedBy) => preventedBy.HasValue);
		}
		_availability[ability] = EnumUtil.Maximum(availability, _availability.value.GetValueOrDefault(ability));
	}

	public void SetReactionContext(Ability ability, ReactionContext reactionContext)
	{
		_reactionContexts[ability] = reactionContext;
	}

	public HashSet<AEntity> GetTargets(Ability ability)
	{
		if (!_targets.ContainsKey(ability))
		{
			return null;
		}
		return _targets[ability].value;
	}

	public Ability.ReactionAvailability GetAvailability(Ability ability)
	{
		if (!ability.data.activation[CanActivateOn.OwnerReaction])
		{
			return _availability.value.GetValueOrDefault(ability);
		}
		return Ability.ReactionAvailability.Available;
	}

	public bool HasActiveReaction(Ability ability)
	{
		return _availability.value.GetValueOrDefault(ability) != Ability.ReactionAvailability.Unavailable;
	}

	public void ClearActiveReaction(Ability ability)
	{
		_availability.value.Remove(ability);
	}
}
