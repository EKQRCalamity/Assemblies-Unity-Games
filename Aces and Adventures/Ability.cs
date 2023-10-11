using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[ProtoContract]
[ProtoInclude(14, typeof(ItemCard))]
public class Ability : AEntity, IComparable<Ability>, IAdventureCard
{
	public enum ReactionAvailability
	{
		Unavailable,
		AvailableOnSpecificTargets,
		Available
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Draw,
		Hand,
		ActivationHand,
		HeroAct,
		HeroPassive,
		ActivationHandWaiting,
		Discard
	}

	[Flags]
	public enum Piles
	{
		Draw = 1,
		Hand = 2,
		ActivationHand = 4,
		HeroAct = 8,
		HeroPassive = 0x10,
		ActivationHandWaiting = 0x20,
		Discard = 0x40
	}

	public struct CanActivateResult
	{
		private Ability _ability;

		private AbilityPreventedBy _preventedBy;

		public bool canActivate => _preventedBy == AbilityPreventedBy.Nothing;

		private AbilityData data => _ability.data;

		private AbilityData.Activation activation => data.activation;

		private AResourceCosts cost => _ability.cost;

		private ACombatant owner => _ability.owner;

		public CanActivateResult(Ability ability, ResourceCard.Piles? activationPilesOverride = null)
		{
			_ability = ability;
			_preventedBy = AbilityPreventedBy.Nothing;
			_ActivationPilesOverride = activationPilesOverride;
			_preventedBy = _GetPreventedBy() ?? _preventedBy;
			_ActivationPilesOverride = null;
		}

		private AbilityPreventedBy? _AvailableToActivate()
		{
			if (GameStepReaction.Active == null)
			{
				if (data.onlyAvailableDuringReaction)
				{
					return AbilityPreventedBy.OnlyAvailableDuringReaction;
				}
				if (owner.isPreparingAttack)
				{
					if (!activation[CanActivateOn.OwnerPrepareAttack])
					{
						return AbilityPreventedBy.NotAvailableWhilePreparingAttack;
					}
					return null;
				}
				if (owner.isPreparingDefense)
				{
					if (!activation[CanActivateOn.OwnerPrepareDefense])
					{
						return AbilityPreventedBy.NotAvailableWhilePreparingDefense;
					}
					return null;
				}
				if (owner.isAttacking)
				{
					return AbilityPreventedBy.NotAvailableWhileAttacking;
				}
				if (owner.isTakingTurn)
				{
					if (!activation[CanActivateOn.OwnerTurn])
					{
						return data.onlyAvailableDuringDefense ? AbilityPreventedBy.OnlyAvailableDuringDefense : (data.hasReaction ? AbilityPreventedBy.ConditionNotMet : AbilityPreventedBy.NotAvailableOnOwnerTurn);
					}
					return null;
				}
				return AbilityPreventedBy.OnlyAvailableDuringOwnerTurn;
			}
			if (_ability.reactionAvailability != 0)
			{
				return null;
			}
			return AbilityPreventedBy.NotAvailableDuringReaction;
		}

		private AbilityPreventedBy? _GetActivationCards()
		{
			using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = _ability.GetActivationCards();
			return (!poolKeepItemListHandle) ? new AbilityPreventedBy?(AbilityPreventedBy.LackingResources) : ((_ability._activationPiles == ResourceCard.Piles.ActivationHand && poolKeepItemListHandle.Count != owner.resourceDeck.Count(ResourceCard.Pile.ActivationHand)) ? new AbilityPreventedBy?(AbilityPreventedBy.AdditionalCardsInActionHand) : null);
		}

		private AbilityPreventedBy? _HasResourcesToActivate()
		{
			if (!_ability.isOutOfUsesThisRound)
			{
				return cost.HasResources(owner) ?? _GetActivationCards();
			}
			return AbilityPreventedBy.OutOfUsesThisRound;
		}

		private AbilityPreventedBy? _HasTargets()
		{
			if (!_ability.GetTargets().Any())
			{
				return AbilityPreventedBy.NoValidTargets;
			}
			return null;
		}

		private AbilityPreventedBy? _GetPreventedBy()
		{
			return _AvailableToActivate() ?? _HasResourcesToActivate() ?? _HasTargets();
		}

		public CanActivateResult Message()
		{
			if (_preventedBy == AbilityPreventedBy.LackingResources)
			{
				_preventedBy = cost.GetMissingResourceType(_ability._activationCards) ?? _preventedBy;
			}
			else if (_preventedBy == AbilityPreventedBy.OnlyAvailableDuringReaction)
			{
				_preventedBy = activation.canAlsoActivateWhen.FirstOrDefault((AAction.Trigger trigger) => trigger.abilityPreventedBy.HasValue)?.abilityPreventedBy ?? _preventedBy;
			}
			_ability.owner.gameState.view.LogError(_preventedBy.LocalizeError(), _ability.owner.gameState.player.audio.character.error[_preventedBy]);
			if (_preventedBy.IsResource())
			{
				int num = -1;
				RectTransform costContainer = _ability.abilityCard.costContainer;
				foreach (AbilityPreventedBy? abilityPreventedBy in cost.GetAbilityPreventedBys())
				{
					if (++num >= costContainer.childCount)
					{
						break;
					}
					if (abilityPreventedBy != _preventedBy)
					{
						continue;
					}
					foreach (IAnimatedUI item in costContainer.GetChild(num).gameObject.GetComponentsInChildrenPooled<IAnimatedUI>())
					{
						item.AddStaggeredAnimations(new Vector3(0f, 0f, -10f), Vector3.zero, Vector3.one * 1.75f, 0.25f, 0.0333f, 5);
					}
				}
			}
			return this;
		}

		public static implicit operator bool(CanActivateResult result)
		{
			return result.canActivate;
		}

		public static implicit operator bool(CanActivateResult? result)
		{
			if (result.HasValue)
			{
				return result.Value.canActivate;
			}
			return false;
		}
	}

	public class IdAbilityRefEqualityComparer : IEqualityComparer<Id<Ability>>
	{
		public static readonly IdAbilityRefEqualityComparer Default = new IdAbilityRefEqualityComparer();

		public bool Equals(Id<Ability> x, Id<Ability> y)
		{
			return ContentRef.Equal(x.value?.dataRef, y.value?.dataRef);
		}

		public int GetHashCode(Id<Ability> id)
		{
			return id.value?.dataRef.GetHashCode() ?? 0;
		}
	}

	[ProtoContract]
	public class RefreshTargetsRegister : ATarget
	{
		[ProtoMember(1)]
		private Id<Ability> _abilityId;

		private bool _abilityIdSpecified => _abilityId;

		protected RefreshTargetsRegister()
		{
		}

		public RefreshTargetsRegister(Id<Ability> abilityId)
		{
			_abilityId = abilityId;
		}

		public override void _Register()
		{
			Ability value = _abilityId.value;
			if (value != null)
			{
				value._RegisterRefreshTargetEvents();
				value._registeredRefreshTargets = this;
			}
		}

		public override void _Unregister()
		{
			Ability value = _abilityId.value;
			if (value != null)
			{
				value._UnregisterRefreshTargetEvents();
				value._registeredRefreshTargets = Id<RefreshTargetsRegister>.Null;
				this.ToId<RefreshTargetsRegister>().ReleaseId();
			}
		}
	}

	private static readonly HashSet<AEntity> EMPTY_REACTION_TAGRETS = new HashSet<AEntity>();

	private static readonly List<AAction.Trigger> EMPTY_REACTIONS = new List<AAction.Trigger>();

	public const Piles ACT_PILES = Piles.Hand | Piles.HeroAct;

	private static TextBuilder _Builder;

	private static ResourceCard.Piles? _ActivationPilesOverride;

	public static readonly Func<Ability, bool> IsPermanent = (Ability card) => card.permanent;

	private static Dictionary<string, AbilityKeyword> _SearchKeywordFilters;

	[ProtoMember(1)]
	private DataRef<AbilityData> _abilityDataRef;

	[ProtoMember(2)]
	private Id<ACombatant> _owner;

	[ProtoMember(3, OverwriteList = true)]
	private List<ResourceCostModifier> _resourceCostModifiers;

	[ProtoMember(5)]
	private BBool _ephemeral;

	private AResourceCosts _cachedResourceCosts;

	private int _reactionCapturedValue;

	private List<AAction.Trigger> _reactions;

	private List<AAction.Duration> _refreshTargetsWhen;

	private List<AAction.Target> _registeredTargets;

	private Id<RefreshTargetsRegister> _registeredRefreshTargets;

	private bool? _targetsResourceCards;

	private bool? _dealsDamage;

	private bool? _affectsTurnOrder;

	protected static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	public static Dictionary<string, AbilityKeyword> SearchKeywordFilters
	{
		get
		{
			if (_SearchKeywordFilters == null)
			{
				_SearchKeywordFilters = new Dictionary<string, AbilityKeyword>();
				AbilityKeyword[] values = EnumUtil<AbilityKeyword>.Values;
				foreach (AbilityKeyword abilityKeyword in values)
				{
					string searchFilter = abilityKeyword.GetSearchFilter();
					if (!searchFilter.HasVisibleCharacter())
					{
						continue;
					}
					foreach (string diacriticAndNonDiacriticString in searchFilter.GetDiacriticAndNonDiacriticStrings())
					{
						_SearchKeywordFilters[diacriticAndNonDiacriticString.ToLower()] = abilityKeyword;
					}
				}
			}
			return _SearchKeywordFilters;
		}
	}

	public Ability root
	{
		get
		{
			Ability ability = this;
			while (true)
			{
				Id<Ability>? id = base.gameState?.addedTraitMap.GetValueOrDefault(ability);
				if (!id.HasValue)
				{
					break;
				}
				Id<Ability> valueOrDefault = id.GetValueOrDefault();
				if (!valueOrDefault)
				{
					break;
				}
				ability = valueOrDefault;
			}
			return ability;
		}
	}

	public DataRef<AbilityData> dataRef => _abilityDataRef;

	public AbilityData data => _abilityDataRef.data;

	public ACombatant owner => _owner;

	private Player _playerOwner => owner as Player;

	public AResourceCosts cost => _cachedResourceCosts ?? (_cachedResourceCosts = _CalculateResourceCosts());

	public AResourceCosts naturalCost => data.activation.cost;

	protected ActionContext _ownerActionContext => new ActionContext(owner, this, owner);

	public int reactionCapturedValue => _reactionCapturedValue;

	protected HashSet<AEntity> reactionTargets => GameStepReaction.Active?.GetTargets(this) ?? EMPTY_REACTION_TAGRETS;

	public ReactionAvailability reactionAvailability => GameStepReaction.Active?.GetAvailability(this) ?? ReactionAvailability.Unavailable;

	public bool hasActiveReaction => GameStepReaction.Active?.HasActiveReaction(this) ?? false;

	protected virtual bool _canReact
	{
		get
		{
			if (cost.additionalCosts.heroAbility != 0)
			{
				return (int)(owner as Player)?.numberOfHeroAbilities > 0;
			}
			return true;
		}
	}

	protected List<ResourceCostModifier> resourceCostModifiers => _resourceCostModifiers ?? (_resourceCostModifiers = new List<ResourceCostModifier>());

	public bool isTrait => data.type.IsTrait();

	public bool isBuff => data.type.IsBuffOrDebuff();

	public bool isSummon => data.type == AbilityData.Type.Summon;

	public bool isConsumedOnAct => data.type == AbilityData.Type.Standard;

	public virtual bool isOutOfUsesThisRound => false;

	public virtual bool hasUsesRemaining
	{
		get
		{
			if (cost.additionalCosts.heroAbility != 0)
			{
				BInt bInt = _playerOwner?.numberOfHeroAbilities;
				return (((object)bInt == null || (int)bInt != 0) ? 1 : 0) > (false ? 1 : 0);
			}
			return true;
		}
	}

	public virtual Pile actPile
	{
		get
		{
			if (data.category != 0)
			{
				if (!isTrait)
				{
					return Pile.HeroAct;
				}
				return Pile.HeroPassive;
			}
			return Pile.Hand;
		}
	}

	public Pile abilityPile => owner.abilityDeck[this].GetValueOrDefault();

	public Pile? nullablePile => owner?.abilityDeck[this];

	public AbilityCardView abilityCard => base.view as AbilityCardView;

	private ResourceCard.Piles _activationPiles
	{
		get
		{
			if (!cost.usesCards)
			{
				return ResourceCard.Piles.ActivationHand;
			}
			if (_ActivationPilesOverride.HasValue)
			{
				return _ActivationPilesOverride.Value;
			}
			if (owner.resourceDeck.Count(ResourceCard.Pile.ActivationHand) > 0)
			{
				return ResourceCard.Piles.ActivationHand;
			}
			if (owner.isPreparingDefense)
			{
				int num = owner.resourceDeck.Count(ResourceCard.Pile.DefenseHand);
				if (num > 0)
				{
					PoolKeepItemListHandle<ResourceCard> activationCards = cost.GetActivationCards(owner.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand));
					if (activationCards != null)
					{
						using (activationCards)
						{
							if (activationCards.Count == num)
							{
								using (PoolKeepItemHashSetHandle<ATarget> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(GetTargets()))
								{
									foreach (ResourceCard item in activationCards.value)
									{
										if (poolKeepItemHashSetHandle.Contains(item))
										{
											return ResourceCard.Piles.Hand;
										}
									}
									return ResourceCard.Piles.DefenseHand;
								}
							}
						}
					}
				}
			}
			return ResourceCard.Piles.Hand;
		}
	}

	private IEnumerable<ResourceCard> _activationCards => owner.resourceDeck.GetCards(_activationPiles);

	private List<AAction.Trigger> reactions
	{
		get
		{
			if (data.activation.canAlsoActivateWhen.Count <= 0)
			{
				return EMPTY_REACTIONS;
			}
			return _reactions ?? (_reactions = ProtoUtil.Clone(data.activation.canAlsoActivateWhen));
		}
	}

	public BBool ephemeral => _ephemeral ?? (_ephemeral = new BBool());

	public bool permanent => !ephemeral;

	public virtual bool isEncounterAbility => false;

	public bool isEncounterCondition
	{
		get
		{
			if (isEncounterAbility)
			{
				return isTrait;
			}
			return false;
		}
	}

	public ReactionContext reactionContext { get; set; }

	public IdDeck<Pile, Ability> deck => owner.abilityDeck;

	public virtual ItemCardType? itemType => null;

	private bool _shouldRegisterRefreshTargets
	{
		get
		{
			if (data.canRegisterRefreshTargets)
			{
				if (!data.hasRegisterRefreshTargets)
				{
					return data.actions.Any((AAction action) => action.isApplied && (action.target?.registers ?? false));
				}
				return true;
			}
			return false;
		}
	}

	public bool targetsResourceCards
	{
		get
		{
			bool? flag = _targetsResourceCards;
			if (!flag.HasValue)
			{
				bool? flag2 = (_targetsResourceCards = _TargetsResourceCards());
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	public bool dealsDamage
	{
		get
		{
			bool? flag = _dealsDamage;
			if (!flag.HasValue)
			{
				bool? flag2 = (_dealsDamage = data.actions.Any((AAction a) => a.dealsDamage));
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	public bool affectsTurnOrder
	{
		get
		{
			bool? flag = _affectsTurnOrder;
			if (!flag.HasValue)
			{
				bool? flag2 = (_affectsTurnOrder = data.actions.Any((AAction a) => a.affectsTurnOrder));
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	public override bool canBePooled => true;

	public override Faction faction => owner?.faction ?? Faction.Player;

	public override bool shouldRegisterDuringGameStateInitialization => nullablePile == Pile.Hand;

	public override int registerDuringGameStateInitializationOrder => 10;

	public string name
	{
		get
		{
			AdventureCard.Common common = adventureCardCommon;
			if (common == null || !common.name.HasVisibleCharacter())
			{
				return data.name;
			}
			return adventureCardCommon.name;
		}
	}

	public string description
	{
		get
		{
			string text = data.description;
			AdventureCard.Common common = adventureCardCommon;
			if (common != null && !common.description.IsNullOrEmpty())
			{
				text = text + ((text.Length > 0) ? "\n" : "") + "<size=80%><align=center>\"<i>" + adventureCardCommon?.description + "</i>\"</align></size>";
			}
			return text;
		}
	}

	public CroppedImageRef image
	{
		get
		{
			if (!(adventureCardCommon?.image))
			{
				return data.cosmetic.image;
			}
			return adventureCardCommon?.image;
		}
	}

	public ATarget adventureCard => this;

	public AdventureCard.Pile pileToTransferToOnDraw => AdventureCard.Pile.SelectionHand;

	public AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.Discard;

	public GameStep selectTransferStep => base.gameState.player.abilityDeck.TransferCardStep(this, actPile);

	[ProtoMember(15)]
	public AdventureCard.Common adventureCardCommon { get; set; }

	public virtual IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield break;
		}
	}

	public virtual ResourceBlueprint<GameObject> blueprint => AbilityCardView.Blueprint;

	private bool _ephemeralSpecified => _ephemeral;

	public event Action<AResourceCosts> onResourceCostChange;

	public event Action<bool> onHasUsesRemainingChange;

	public static void ClearSearchKeywordFilters()
	{
		_SearchKeywordFilters = null;
	}

	public static void OnEnterInspect(Ability ability)
	{
		if (ability != null && ability.isSummon)
		{
			ability.abilityCard?.onTappedChange?.Invoke(arg0: false);
		}
	}

	public static void OnExitInspect(Ability ability)
	{
		if (ability != null && ability.isSummon)
		{
			ability.abilityCard?.onTappedChange?.Invoke(ability.tapped);
		}
	}

	protected Ability()
	{
	}

	protected void _Initialize(DataRef<AbilityData> abilityDataRef, ACombatant owner = null, bool signalAbilityAdded = true)
	{
		_abilityDataRef = abilityDataRef;
		_owner = owner;
		if (signalAbilityAdded)
		{
			GameState.Instance?.SignalAbilityAdded(this);
		}
	}

	public Ability(DataRef<AbilityData> abilityDataRef, ACombatant owner = null, bool signalAbilityAdded = true)
	{
		_Initialize(abilityDataRef, owner, signalAbilityAdded);
	}

	private void _OnReactionTrigger(ReactionContext reactionContext, TargetedReactionFilter filter, int capturedValue)
	{
		if (!filter.IsValid(reactionContext, _ownerActionContext))
		{
			return;
		}
		_reactionCapturedValue = capturedValue;
		if (!isTrait)
		{
			if (_canReact)
			{
				AEntity target = filter.GetTarget(reactionContext, _ownerActionContext);
				GameStepReaction.Pending.SetAvailability(this, (target != null) ? ReactionAvailability.AvailableOnSpecificTargets : ReactionAvailability.Available);
				GameStepReaction.Pending.SetReactionContext(this, reactionContext);
				if (target != null)
				{
					GameStepReaction.Pending.AddTarget(this, target);
				}
			}
		}
		else
		{
			this.reactionContext = reactionContext.SetTarget(filter, _ownerActionContext);
			Activate();
		}
	}

	private AResourceCosts _CalculateResourceCosts()
	{
		AResourceCosts aResourceCosts = ProtoUtil.Clone(data.activation.cost);
		foreach (ResourceCostModifier resourceCostModifier in resourceCostModifiers)
		{
			aResourceCosts = resourceCostModifier.ProcessCost(aResourceCosts);
		}
		this.onResourceCostChange?.Invoke(aResourceCosts);
		return aResourceCosts;
	}

	private void _OnNumberOfHeroAbilitiesChange(int previous, int current)
	{
		if ((previous > 0) ^ (current > 0))
		{
			_SignalHasUsesRemaining(current > 0 || cost.additionalCosts.heroAbility == 0);
		}
	}

	private void _OnHeroAbilityResourceCostChange(AResourceCosts newCost)
	{
		int usesRemaining;
		if (newCost.additionalCosts.heroAbility != 0)
		{
			BInt bInt = _playerOwner?.numberOfHeroAbilities;
			usesRemaining = (((((object)bInt == null || (int)bInt != 0) ? 1 : 0) > (false ? 1 : 0)) ? 1 : 0);
		}
		else
		{
			usesRemaining = 1;
		}
		_SignalHasUsesRemaining((byte)usesRemaining != 0);
	}

	protected void _SignalHasUsesRemaining(bool usesRemaining)
	{
		this.onHasUsesRemainingChange?.Invoke(usesRemaining);
	}

	public override AGameStepTurn GetTurnStep()
	{
		return new GameStepTurnSummon(this);
	}

	public override void _Register()
	{
		foreach (AAction.Trigger reaction in reactions)
		{
			reaction.Register(_ownerActionContext);
			reaction.onTrigger += _OnReactionTrigger;
		}
		if (data.category == AbilityData.Category.HeroAbility)
		{
			Player playerOwner = _playerOwner;
			if (playerOwner != null)
			{
				playerOwner.numberOfHeroAbilities.onValueChanged += _OnNumberOfHeroAbilitiesChange;
				onResourceCostChange += _OnHeroAbilityResourceCostChange;
			}
		}
	}

	public override void _Unregister()
	{
		foreach (AAction.Trigger reaction in reactions)
		{
			reaction.Unregister(_ownerActionContext);
			reaction.onTrigger -= _OnReactionTrigger;
		}
		if (data.category == AbilityData.Category.HeroAbility)
		{
			Player playerOwner = _playerOwner;
			if (playerOwner != null)
			{
				playerOwner.numberOfHeroAbilities.onValueChanged -= _OnNumberOfHeroAbilitiesChange;
				onResourceCostChange -= _OnHeroAbilityResourceCostChange;
			}
		}
	}

	public bool TargetRestrictedByReaction(ATarget target)
	{
		if (target is AEntity item && reactionAvailability == ReactionAvailability.AvailableOnSpecificTargets)
		{
			return !reactionTargets.Contains(item);
		}
		return false;
	}

	public bool IsTargetOfReaction(ATarget target)
	{
		if (target is AEntity item)
		{
			return reactionTargets.Contains(item);
		}
		return false;
	}

	public void ClearActiveReaction()
	{
		GameStepReaction.Active?.ClearActiveReaction(this);
	}

	public IEnumerable<ATarget> GetTargets()
	{
		bool targetFound = !data.checkAllActionsForInitialTargeting;
		foreach (AAction action in data.actions)
		{
			if (action.target.inheritsTargets)
			{
				continue;
			}
			foreach (ATarget item in action.GetTargetableWithPostProcessing(new ActionContext(owner, this)))
			{
				targetFound = true;
				yield return item;
			}
			if (targetFound)
			{
				yield break;
			}
		}
	}

	private bool _TargetsResourceCards()
	{
		foreach (AAction action in data.actions)
		{
			if (action.target is AAction.Target.Resource)
			{
				return true;
			}
		}
		return false;
	}

	public PoolKeepItemListHandle<ResourceCard> GetActivationCards()
	{
		if (!targetsResourceCards || !cost.usesCards || _activationPiles == ResourceCard.Piles.ActivationHand)
		{
			return cost.GetActivationCards(_activationCards);
		}
		using PoolKeepItemHashSetHandle<ResourceCard> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(_activationCards);
		using PoolKeepItemHashSetHandle<ATarget> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet<ATarget>();
		while (true)
		{
			PoolKeepItemListHandle<ResourceCard> activationCards = cost.GetActivationCards(poolKeepItemHashSetHandle.value);
			if (activationCards == null)
			{
				break;
			}
			bool flag = poolKeepItemHashSetHandle2.Count == 0;
			IEnumerable<ATarget> enumerable;
			if (!flag)
			{
				IEnumerable<ATarget> value = poolKeepItemHashSetHandle2.value;
				enumerable = value;
			}
			else
			{
				enumerable = GetTargets();
			}
			foreach (ATarget item2 in enumerable)
			{
				if (!(item2 is ResourceCard item) || !activationCards.value.Contains(item))
				{
					return activationCards;
				}
				if (flag)
				{
					poolKeepItemHashSetHandle2.Add(item2);
				}
			}
			if (poolKeepItemHashSetHandle2.Count == 0)
			{
				return activationCards;
			}
			foreach (ResourceCard item3 in activationCards)
			{
				if (poolKeepItemHashSetHandle2.Contains(item3))
				{
					poolKeepItemHashSetHandle.Remove(item3);
				}
			}
		}
		return null;
	}

	public CanActivateResult CanActivate(ResourceCard.Piles? activationPilesOverride = null)
	{
		return new CanActivateResult(this, activationPilesOverride);
	}

	public void Activate()
	{
		base.gameState.stack.Push(new GameStepGroupAbilityAct(this));
	}

	public virtual void Consume()
	{
		if (data.category == AbilityData.Category.TrumpCard)
		{
			base.gameState.heroDeck.Discard(this);
		}
		else
		{
			owner.abilityDeck.Transfer(this, (data.category == AbilityData.Category.Ability) ? Pile.Discard : (isTrait ? Pile.HeroPassive : Pile.HeroAct));
		}
		if (!hasUsesRemaining)
		{
			_SignalHasUsesRemaining(usesRemaining: false);
		}
	}

	public Ability Unapply()
	{
		for (int num = base.gameState.appliedActions.Count - 1; num >= 0; num--)
		{
			AppliedAction appliedAction = base.gameState.appliedActions[num];
			if (appliedAction != null && appliedAction.context.ability == this)
			{
				appliedAction.Unapply(clearPendingTicks: true);
			}
		}
		return this;
	}

	public void Remove(ActionContext context, bool discard = true)
	{
		if (isBuff)
		{
			ACombatant target = context.GetTarget<ACombatant>();
			if (target != null && target.appliedAbilities.Contains(this))
			{
				context.gameState.SignalBuffRemoved(context.GetTarget<ACombatant>(ActionContextTarget.Owner), target, this);
			}
		}
		Unapply();
		if (discard && context.actor is Player player && !isTrait)
		{
			player.abilityDeck.Transfer(this, Pile.Discard);
		}
	}

	public void AddResourceCostModifier(ResourceCostModifier modifier)
	{
		if (resourceCostModifiers.AddSorted(modifier) >= 0)
		{
			_cachedResourceCosts = null;
		}
	}

	public void RemoveResourceCostModifier(ResourceCostModifier modifier)
	{
		if (resourceCostModifiers.Remove(modifier))
		{
			_cachedResourceCosts = null;
		}
	}

	public void RefreshResourceCost()
	{
		_ = cost;
	}

	public IEnumerable<string> GetDisplayedTags(Locale locale = null)
	{
		foreach (AbilityKeyword tagKeyword in GetTagKeywords())
		{
			yield return tagKeyword.GetTag(locale);
		}
	}

	public virtual string GetDisplayedTagString(Locale locale = null)
	{
		return data.GetDisplayedTagString();
	}

	public virtual string GetLocalizedTagString()
	{
		return data.GetLocalizedTagString();
	}

	public virtual IEnumerable<AbilityKeyword> GetTagKeywords()
	{
		return data.GetTagKeywords();
	}

	public void RegisterRefreshTargetEvents()
	{
		if (_shouldRegisterRefreshTargets && !_registeredRefreshTargets)
		{
			new RefreshTargetsRegister(this).Register();
		}
	}

	private void _RegisterRefreshTargetEvents()
	{
		foreach (AAction action in data.actions)
		{
			if (!action.isApplied)
			{
				continue;
			}
			AAction.Target target = action.target;
			if (target != null && target.registers)
			{
				AAction.Target target2 = ProtoUtil.Clone(target);
				if (target2 != null)
				{
					(_registeredTargets ?? (_registeredTargets = new List<AAction.Target>())).Add(target2);
					target2.Register(_ownerActionContext, action, this);
					target2.onTargetAdded = (Action<AAction, ATarget>)Delegate.Combine(target2.onTargetAdded, new Action<AAction, ATarget>(_OnTargetAdded));
				}
			}
		}
		if (!data.hasRegisterRefreshTargets)
		{
			return;
		}
		_refreshTargetsWhen = ProtoUtil.Clone(data.refreshTargetsWhen);
		foreach (AAction.Duration item in _refreshTargetsWhen)
		{
			item.Register(_ownerActionContext);
			item.onDurationComplete += _OnRefreshTarget;
		}
	}

	public void UnregisterRefreshTargetEvents()
	{
		if ((bool)_registeredRefreshTargets)
		{
			_registeredRefreshTargets.value.Unregister();
		}
	}

	private void _UnregisterRefreshTargetEvents()
	{
		if (!_registeredTargets.IsNullOrEmpty())
		{
			foreach (AAction.Target registeredTarget in _registeredTargets)
			{
				registeredTarget.Unregister(_ownerActionContext);
				registeredTarget.onTargetAdded = (Action<AAction, ATarget>)Delegate.Remove(registeredTarget.onTargetAdded, new Action<AAction, ATarget>(_OnTargetAdded));
			}
		}
		_registeredTargets = null;
		if (!_refreshTargetsWhen.IsNullOrEmpty())
		{
			foreach (AAction.Duration item in _refreshTargetsWhen)
			{
				item.Unregister(_ownerActionContext);
				item.onDurationComplete -= _OnRefreshTarget;
			}
		}
		_refreshTargetsWhen = null;
	}

	private void _OnTargetAdded(AAction action, ATarget target)
	{
		AppliedAction.Apply(_ownerActionContext.SetTarget(target), action);
	}

	private void _OnRefreshTarget(ReactionContext reactionContext, ReactionFilter reactionFilter)
	{
		if (reactionFilter.IsValid(reactionContext, _ownerActionContext))
		{
			base.gameState.stack.Push(new GameStepGrouper(_RefreshTargetSteps()));
		}
	}

	private IEnumerable<GameStep> _RefreshTargetSteps()
	{
		yield return new GameStepGroupCanceler(() => abilityPile == Pile.Discard);
		yield return new GameStepGeneric
		{
			onStart = delegate
			{
				Remove(_ownerActionContext, discard: false);
			}
		};
		foreach (AAction action in data.actions)
		{
			if (!action.isApplied)
			{
				continue;
			}
			foreach (GameStep actGameStep in action.GetActGameSteps(_ownerActionContext))
			{
				yield return actGameStep;
			}
		}
	}

	public void HighlightAbilityName(float duration = 6f)
	{
		if (!isTrait && abilityPile == Pile.Discard)
		{
			return;
		}
		TextMeshProUGUI textMeshProUGUI = null;
		if (base.view is AbilityCardView abilityCardView)
		{
			textMeshProUGUI = abilityCardView.nameText;
		}
		else if (isTrait && owner.view is EnemyCardView enemyCardView)
		{
			textMeshProUGUI = enemyCardView.descriptionText;
		}
		else
		{
			Id<Ability> valueOrDefault = owner.gameState.addedTraitMap.GetValueOrDefault(this);
			if ((bool)valueOrDefault)
			{
				valueOrDefault.value.HighlightAbilityName(duration);
			}
		}
		if ((bool)textMeshProUGUI)
		{
			ATextMeshProAnimator.CreateHighlight(textMeshProUGUI, data.name, duration);
		}
	}

	public void EndHighlightAbilityName()
	{
		TextMeshProUGUI textMeshProUGUI = null;
		if (base.view is AbilityCardView abilityCardView)
		{
			textMeshProUGUI = abilityCardView.nameText;
		}
		else if (isTrait && owner.view is EnemyCardView enemyCardView)
		{
			textMeshProUGUI = enemyCardView.descriptionText;
		}
		else
		{
			Id<Ability> valueOrDefault = owner.gameState.addedTraitMap.GetValueOrDefault(this);
			if ((bool)valueOrDefault)
			{
				valueOrDefault.value.EndHighlightAbilityName();
			}
		}
		if ((bool)textMeshProUGUI)
		{
			ATextMeshProAnimator.EndHighlights(textMeshProUGUI.gameObject, data.name);
		}
	}

	public IEnumerable<string> GetTooltips()
	{
		return data.GetTooltips(this);
	}

	public bool IsValidBuffTarget(ACombatant target)
	{
		if (data.type != AbilityData.Type.Buff)
		{
			return target is Enemy;
		}
		return target is Player;
	}

	public bool ShowPotentialDamage(ActionContext? contextOverride = null, IEnumerable<ATarget> selectedTargets = null)
	{
		if (!dealsDamage)
		{
			return false;
		}
		using PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList(selectedTargets ?? Enumerable.Empty<ATarget>());
		if (selectedTargets != null && poolKeepItemListHandle.Count == 0)
		{
			return HidePotentialDamage();
		}
		int num = 0;
		ActionContext context = contextOverride ?? _ownerActionContext;
		if (hasActiveReaction)
		{
			context = context.SetCapturedValue(_reactionCapturedValue);
		}
		bool flag = !owner.resourceDeck.Any(ResourceCard.Pile.ActivationHand) && cost.usesCards;
		Pile? pile = nullablePile;
		(Pile, int)? obj;
		if (pile.HasValue)
		{
			Pile valueOrDefault = pile.GetValueOrDefault();
			if (valueOrDefault != Pile.ActivationHand)
			{
				obj = (valueOrDefault, deck.IndexOf(this));
				goto IL_00cb;
			}
		}
		obj = null;
		goto IL_00cb;
		IL_00cb:
		(Pile, int)? tuple = obj;
		PoolKeepItemDictionaryHandle<ResourceCard, ResourceCard.Pile> poolKeepItemDictionaryHandle = null;
		if (tuple.HasValue)
		{
			IdDeck<Pile, Ability> idDeck = deck;
			int suppressEvents = idDeck.suppressEvents + 1;
			idDeck.suppressEvents = suppressEvents;
			deck.Transfer(this, Pile.ActivationHand);
		}
		if (flag)
		{
			IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck = owner.resourceDeck;
			int suppressEvents = resourceDeck.suppressEvents + 1;
			resourceDeck.suppressEvents = suppressEvents;
			poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<ResourceCard, ResourceCard.Pile>();
			foreach (ResourceCard item in GetActivationCards()?.AsEnumerable() ?? Enumerable.Empty<ResourceCard>())
			{
				poolKeepItemDictionaryHandle[item] = item.pile;
				owner.resourceDeck.Transfer(item, ResourceCard.Pile.ActivationHand);
			}
		}
		PoolKeepItemListHandle<ATarget> poolKeepItemListHandle2 = null;
		if (affectsTurnOrder)
		{
			IdDeck<AdventureCard.Pile, ATarget> adventureDeck = base.gameState.adventureDeck;
			int suppressEvents = adventureDeck.suppressEvents + 1;
			adventureDeck.suppressEvents = suppressEvents;
			poolKeepItemListHandle2 = base.gameState.adventureDeck.GetCardsSafe(AdventureCard.Pile.TurnOrder);
		}
		using (ResourceCard.WildSnapshot.Create(owner.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand)))
		{
			cost.WildIntoCost(owner.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand));
			using PoolDictionaryValuesHandle<AAction, PoolKeepItemListHandle<ATarget>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<AAction, PoolKeepItemListHandle<ATarget>>();
			using PoolKeepItemListHandle<AAction> poolKeepItemListHandle4 = Pools.UseKeepItemList<AAction>();
			foreach (AAction action in data.actions)
			{
				if ((action.target.targetType != typeof(ACombatant) && !action.affectsTurnOrder) || !action.CanAttemptToAct(context))
				{
					continue;
				}
				PoolKeepItemListHandle<ATarget> poolKeepItemListHandle3 = Pools.UseKeepItemList<ATarget>();
				if (!action.requiresUserInput)
				{
					if (action.target.inheritsTargets)
					{
						AAction key = poolDictionaryValuesHandle.value.Reverse().FirstOrDefault((KeyValuePair<AAction, PoolKeepItemListHandle<ATarget>> previousActionTargets) => !previousActionTargets.Key.target.inheritsTargets && previousActionTargets.Value.Count > 0 && action.target.targetType == previousActionTargets.Key.target.targetType).Key;
						if (key != null)
						{
							action.target.targetCountType = key.target.targetCountType;
							poolKeepItemListHandle3.value.AddMany(action.CanAttemptToAct(context) ? action.target.FilterTargetsByConditions(context, poolDictionaryValuesHandle[key].value) : Enumerable.Empty<ATarget>());
						}
					}
					else
					{
						poolKeepItemListHandle3.value.AddMany(action.target.GetTargetable(context, action));
					}
					goto IL_043f;
				}
				if (selectedTargets != null)
				{
					poolKeepItemListHandle3.value.AddMany(poolKeepItemListHandle.value);
					goto IL_043f;
				}
				if (ProfileManager.options.game.preferences.autoSelectSingleTarget.AutoSelect(this) && action.target.GetTargetable(context, action).Count() == 1)
				{
					poolKeepItemListHandle3.value.AddMany(Enumerable.Repeat(action.target.GetTargetable(context, action).First(), (!action.target.allowRepeats) ? 1 : action.target.count.GetValue(context)));
					goto IL_043f;
				}
				goto end_IL_0229;
				IL_043f:
				if (action.target.isRandom && poolKeepItemListHandle3.Count > 1)
				{
					Pools.Repool(poolKeepItemListHandle3);
					continue;
				}
				action.target.PostProcessTargets(context, poolKeepItemListHandle3);
				if (action is CaptureValueAction captureValueAction)
				{
					foreach (ATarget item2 in poolKeepItemListHandle3.value)
					{
						context = context.SetCapturedValue(captureValueAction.GetCapturedValue(context.SetTarget(item2)));
					}
				}
				if (action.affectsTurnOrder)
				{
					foreach (ATarget item3 in poolKeepItemListHandle3.value)
					{
						action.Tick(context.SetTarget(item3));
					}
				}
				poolDictionaryValuesHandle[action] = poolKeepItemListHandle3;
				if ((action.dealsDamage || action.processesDamage || action.affectsTurnOrder) && !action.isTicking)
				{
					poolKeepItemListHandle4.Add(action);
				}
			}
			if (affectsTurnOrder)
			{
				base.gameState.adventureDeck.Set(poolKeepItemListHandle2.value, AdventureCard.Pile.TurnOrder);
			}
			if (poolKeepItemListHandle4.Count > 0)
			{
				GameState obj2 = context.gameState;
				int suppressEvents = obj2.suppressProcessDamageReactions + 1;
				obj2.suppressProcessDamageReactions = suppressEvents;
				using PoolKeepItemListHandle<AppliedAction> poolKeepItemListHandle5 = Pools.UseKeepItemList<AppliedAction>();
				using PoolKeepItemDictionaryHandle<ACombatant, int> poolKeepItemDictionaryHandle2 = Pools.UseKeepItemDictionary<ACombatant, int>();
				foreach (AAction item4 in poolKeepItemListHandle4.value)
				{
					if (item4.affectsTurnOrder)
					{
						foreach (ATarget item5 in poolDictionaryValuesHandle[item4].value)
						{
							item4.Tick(context.SetTarget(item5));
						}
						continue;
					}
					foreach (ATarget item6 in poolDictionaryValuesHandle[item4].value)
					{
						if (item6 is ACombatant aCombatant && aCombatant.inTurnOrder)
						{
							if (item4.dealsDamage)
							{
								poolKeepItemDictionaryHandle2[aCombatant] = poolKeepItemDictionaryHandle2.value.GetValueOrDefault(aCombatant) + item4.GetPotentialDamage(context.SetTarget(aCombatant));
							}
							else if (item4.processesDamage)
							{
								poolKeepItemListHandle5.Add(AppliedAction.Apply(context.SetTarget(aCombatant), item4));
							}
						}
					}
				}
				foreach (KeyValuePair<ACombatant, int> item7 in poolKeepItemDictionaryHandle2.value)
				{
					item7.Key.combatantCard.ShowPotentialDamage(this, item7.Value);
				}
				num = poolKeepItemDictionaryHandle2.Count;
				foreach (AppliedAction item8 in poolKeepItemListHandle5.value)
				{
					item8.Unapply();
				}
				GameState obj3 = context.gameState;
				suppressEvents = obj3.suppressProcessDamageReactions - 1;
				obj3.suppressProcessDamageReactions = suppressEvents;
			}
			end_IL_0229:;
		}
		if (affectsTurnOrder)
		{
			base.gameState.adventureDeck.Set(poolKeepItemListHandle2.AsEnumerable(), AdventureCard.Pile.TurnOrder);
			IdDeck<AdventureCard.Pile, ATarget> adventureDeck2 = base.gameState.adventureDeck;
			int suppressEvents = adventureDeck2.suppressEvents - 1;
			adventureDeck2.suppressEvents = suppressEvents;
		}
		if (flag)
		{
			foreach (KeyValuePair<ResourceCard, ResourceCard.Pile> item9 in poolKeepItemDictionaryHandle)
			{
				owner.resourceDeck.Transfer(item9.Key, item9.Value);
			}
			IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck2 = owner.resourceDeck;
			int suppressEvents = resourceDeck2.suppressEvents - 1;
			resourceDeck2.suppressEvents = suppressEvents;
		}
		if (tuple.HasValue)
		{
			deck.Transfer(this, tuple.Value.Item1, tuple.Value.Item2);
			IdDeck<Pile, Ability> idDeck2 = deck;
			int suppressEvents = idDeck2.suppressEvents - 1;
			idDeck2.suppressEvents = suppressEvents;
		}
		return num > 0 || HidePotentialDamage();
	}

	public bool HidePotentialDamage()
	{
		if (dealsDamage)
		{
			foreach (AEntity item in base.gameState.turnOrderQueue)
			{
				if (item is ACombatant aCombatant)
				{
					aCombatant.combatantCard.HidePotentialDamage(this);
				}
			}
		}
		return false;
	}

	public void SetAbilityDataReference(DataRef<AbilityData> newAbilityRef)
	{
		bool flag = this.IsRegistered();
		if (flag)
		{
			this.Unregister();
		}
		bool num = _registeredRefreshTargets;
		if (num)
		{
			UnregisterRefreshTargetEvents();
		}
		ClearActiveReaction();
		_cachedResourceCosts = null;
		_reactionCapturedValue = 0;
		_reactions = null;
		_refreshTargetsWhen = null;
		_registeredTargets = null;
		_registeredRefreshTargets = Id<RefreshTargetsRegister>.Null;
		_targetsResourceCards = null;
		_dealsDamage = null;
		_affectsTurnOrder = null;
		_abilityDataRef = newAbilityRef;
		if (flag)
		{
			this.Register();
		}
		if (num)
		{
			RegisterRefreshTargetEvents();
		}
		AbilityCardView abilityCardView = abilityCard;
		abilityCardView.ability = null;
		abilityCardView.ability = this;
	}

	public Ability CopyResourceCostModifiersFrom(Ability copyFrom)
	{
		resourceCostModifiers.Clear();
		foreach (ResourceCostModifier resourceCostModifier in copyFrom.resourceCostModifiers)
		{
			AddResourceCostModifier(resourceCostModifier);
		}
		return this;
	}

	public int CompareTo(Ability other)
	{
		int num = itemType.SortOrder(isTrait) - other.itemType.SortOrder(other.isTrait);
		if (num != 0)
		{
			return num;
		}
		int num2 = data.category.SortOrder() - other.data.category.SortOrder();
		if (num2 != 0)
		{
			return num2;
		}
		return string.Compare(data.name, other.data.name, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		return data?.name ?? base.ToString();
	}
}
