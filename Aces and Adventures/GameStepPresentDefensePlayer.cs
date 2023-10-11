using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepPresentDefensePlayer : AGameStepTurnAbility
{
	private const float IDLE_TUTORIAL_TIME = 15f;

	private const float CONFIRM_TUTORIAL_TIME = 5f;

	private DefenseResultType? _defenseResult;

	private DefenseResultType? _previousDefenseResult;

	private bool _activationHandEnabled;

	private float _elapsedTime;

	private ResourceCard.CustomWildTracker _customWildTracker;

	private bool _findAutoDefenseActive;

	private ActiveCombat combat => base.state.activeCombat;

	private ACombatant attacker => combat.attacker;

	private ACombatant defender => combat.defender;

	private DefenseResultType defenseResult
	{
		get
		{
			return _defenseResult.GetValueOrDefault();
		}
		set
		{
			_previousDefenseResult = _defenseResult;
			_defenseResult = value;
			_OnDefenseResultChange();
		}
	}

	private ResourceCard.CustomWildTracker customWildTracker => _customWildTracker ?? (_customWildTracker = ResourceCard.CustomWildTracker.Create());

	private bool _shouldIgnoreDefenseResultChange
	{
		get
		{
			if (defender.resourceDeck.isSuppressingEvents && _previousDefenseResult.HasValue && _previousDefenseResult != DefenseResultType.Invalid)
			{
				return defenseResult == DefenseResultType.Invalid;
			}
			return false;
		}
	}

	protected override bool canAct => true;

	protected override ResourceCard.Piles _enemyWildPiles => ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;

	protected override ResourceCard.Piles _backPiles => base._backPiles | ResourceCard.Piles.DefenseHand;

	protected override bool shouldBeCanceled => base.state.combatShouldBeCanceled;

	public GameStepPresentDefensePlayer(AEntity entity)
		: base(entity)
	{
	}

	private void _UpdateAttackCardGlows(DefenseResultType defenseResultType)
	{
		TargetLineView.RemoveOwnedBy(this);
		foreach (ResourceCard card in attacker.resourceDeck.GetCards(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand))
		{
			card.view.offsets.Clear();
		}
		foreach (ResourceCard card2 in attacker.resourceDeck.GetCards(ResourceCard.Pile.AttackHand))
		{
			card2.view.RequestGlow(attacker.view, defenseResultType.Opposite().GetTint(), GlowTags.Persistent | GlowTags.Attack);
			_AddEnemyAttackTargetLine(this, card2, defender, defenseResultType.Opposite().GetTint(), TargetLineTags.Persistent);
			if (attacker.resourceDeck.Count(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) > attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand))
			{
				card2.view.offsets.Add(Matrix4x4.Translate(base.view.enemyResourceDeckLayout.combatHand.transform.forward * -0.008f));
			}
		}
	}

	private void _UpdateDefenderCardGlows(IEnumerable<ResourceCard> defenseCards, DefenseResultType defenseResultType)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(defenseCards);
		bool flag = poolKeepItemListHandle.Count < attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand);
		if (defender.resourceDeck.isSuppressingEvents && defenseResultType == DefenseResultType.Invalid && !flag)
		{
			return;
		}
		defender.view.ReleaseOwnedGlowRequests();
		TargetLineView.RemoveOwnedBy(defender.view);
		Color tint = defenseResultType.GetTint(flag);
		foreach (ResourceCard item in poolKeepItemListHandle.value)
		{
			item.view.RequestGlow(defender.view, tint);
			if (item.pile != ResourceCard.Pile.DefenseHand)
			{
				_AddAttackTargetLine(defender.view, item, attacker, tint, AGameStepTurnAbility.AUTO_COMBAT_START_ROTATION, AGameStepTurnAbility.AUTO_COMBAT_END_ROTATION, AGameStepTurnAbility.AUTO_COMBAT_TANGENT_SCALE, AGameStepTurnAbility.AUTO_COMBAT_END_TANGENT_SCALE, AGameStepTurnAbility.AUTO_COMBAT_END_OFFSET);
			}
			else
			{
				_AddAttackTargetLine(defender.view, item, attacker, tint);
			}
		}
	}

	private void _CalculateDefenseResult()
	{
		defenseResult = defender.CanFormDefense(defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand), attacker, attacker.GetAttackHand(defender), _customWildTracker.CardsToFreeze());
	}

	private void _OnDefenseResultChange()
	{
		if (!_shouldIgnoreDefenseResultChange)
		{
			attacker.view.ReleaseOwnedGlowRequests();
			_UpdateDefenderCardGlows(base.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand), defenseResult);
			defender.view.RequestGlow(attacker.view, defenseResult.FailureIfInvalid().GetTint());
			base.buttonDeckLayout[ButtonCardType.ConfirmDefense].view.RequestGlow(attacker.view, defenseResult.GetTint());
			_UpdateAttackCardGlows(defenseResult);
			attacker.view.RequestGlow(attacker.view, defenseResult.Opposite().GetTint());
		}
	}

	private void _UpdateDefenseHand()
	{
		_CalculateDefenseResult();
		if (_shouldIgnoreDefenseResultChange)
		{
			return;
		}
		if (defenseResult >= DefenseResultType.Tie)
		{
			PokerHand attackHand = attacker.GetAttackHand(defender);
			if (defender.CanFormDefense(defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand), attacker, attackHand, defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand)) < defenseResult)
			{
				defender.GetDefenseCombatHand(ResourceCard.Pile.DefenseHand, attacker, attackHand).WildIntoPokerHand();
			}
		}
		if (!defender.resourceDeck.isSuppressingEvents)
		{
			base.buttonDeckLayout.SetActive(ButtonCardType.CancelDefense, base.resourceDeck.Count(ResourceCard.Pile.DefenseHand) > 0);
		}
		_UpdatePotentialCombatDamage();
	}

	private void _OnAttackerResourceOver(ResourceCard.Pile pile, ResourceCard card)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(pile))
		{
			_OnAdventureOver(AdventureCard.Pile.TurnOrder, attacker);
		}
	}

	private void _OnAttackerResourceExit(ResourceCard.Pile pile, ResourceCard card)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(pile))
		{
			_OnAdventureExit(AdventureCard.Pile.TurnOrder, attacker);
		}
	}

	private void _OnAttackerResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(pile))
		{
			_OnAdventureClick(AdventureCard.Pile.TurnOrder, attacker);
		}
	}

	private void _MoveCardsIntoDefenseHandAndUpdate(IEnumerable<ResourceCard> cards)
	{
		using PoolKeepItemDictionaryHandle<ResourceCard, ResourceCard.Pile> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<ResourceCard, ResourceCard.Pile>();
		foreach (ResourceCard card in cards)
		{
			poolKeepItemDictionaryHandle[card] = card.pile;
		}
		int suppressEvents;
		using (ResourceCard.WildSnapshot.Create(poolKeepItemDictionaryHandle.value.Keys))
		{
			IdDeck<ResourceCard.Pile, ResourceCard> idDeck = defender.resourceDeck;
			suppressEvents = idDeck.suppressEvents + 1;
			idDeck.suppressEvents = suppressEvents;
			defender.resourceDeck.Transfer(poolKeepItemDictionaryHandle.value.Keys, ResourceCard.Pile.DefenseHand);
			_UpdateDefenseHand();
		}
		foreach (KeyValuePair<ResourceCard, ResourceCard.Pile> item in poolKeepItemDictionaryHandle.value)
		{
			defender.resourceDeck.Transfer(item.Key, item.Value);
		}
		IdDeck<ResourceCard.Pile, ResourceCard> idDeck2 = defender.resourceDeck;
		suppressEvents = idDeck2.suppressEvents - 1;
		idDeck2.suppressEvents = suppressEvents;
	}

	protected override void _OnResourceOver(ResourceCard.Pile pile, ResourceCard card)
	{
		base._OnResourceOver(pile, card);
		if (card.pile == ResourceCard.Pile.Hand)
		{
			_MoveCardsIntoDefenseHandAndUpdate(Enumerable.Repeat(card, 1));
		}
	}

	protected override void _OnResourceExit(ResourceCard.Pile pile, ResourceCard card)
	{
		base._OnResourceExit(pile, card);
		if (card.pile == ResourceCard.Pile.Hand || pile == ResourceCard.Pile.ActivationHand)
		{
			_UpdateDefenseHand();
		}
	}

	protected override void _OnResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		switch (pile)
		{
		case ResourceCard.Pile.Hand:
			base.resourceDeck.Transfer(card, ResourceCard.Pile.DefenseHand);
			break;
		case ResourceCard.Pile.ActivationHand:
			base.resourceDeck.Transfer(card, ResourceCard.Pile.Hand);
			break;
		case ResourceCard.Pile.DefenseHand:
			base.resourceDeck.Transfer(card, (!_activationHandEnabled || !base.player.HasAbilityThatCanActivateWhichCouldUseCardButIsNotCurrently(card)) ? ResourceCard.Pile.Hand : ResourceCard.Pile.ActivationHand);
			break;
		case ResourceCard.Pile.AttackHand:
			break;
		}
	}

	protected override void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		base._OnResourceTransfer(card, oldPile, newPile);
		_ResetMessageTimer(ref _elapsedTime);
		if (oldPile == ResourceCard.Pile.DefenseHand || newPile == ResourceCard.Pile.DefenseHand)
		{
			customWildTracker[card] = false;
			_UpdateDefenseHand();
		}
	}

	private void _OnButtonTransfer(ButtonCard card, ButtonCard.Pile? oldPile, ButtonCard.Pile? newPile)
	{
		if ((ButtonCardType)card == ButtonCardType.CancelDefense)
		{
			base.view.RefreshPointerOver();
		}
	}

	protected override void _OnButtonOver(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.ConfirmDefense)
		{
			_OnAdventureOver(AdventureCard.Pile.TurnOrder, attacker);
		}
	}

	protected override void _OnButtonExit(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.ConfirmDefense)
		{
			_OnAdventureExit(AdventureCard.Pile.TurnOrder, attacker);
		}
	}

	protected override void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		base._OnButtonClick(pile, card);
		if (pile == ButtonCard.Pile.Active)
		{
			if ((ButtonCardType)card == ButtonCardType.ConfirmDefense)
			{
				_OnAdventureClick(AdventureCard.Pile.TurnOrder, attacker);
			}
			else if ((ButtonCardType)card == ButtonCardType.CancelDefense)
			{
				base.resourceDeck.TransferPile(ResourceCard.Pile.DefenseHand, ResourceCard.Pile.Hand);
			}
		}
	}

	private void _FindAutoDefense()
	{
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType, DefenseResultType) tuple = defender.FindDefenseHandAgainstAllowInvalid(attacker, _customWildTracker.CardsToFreeze());
		using (tuple.Item1)
		{
			_UpdateDefenderCardGlows(tuple.Item1.value.Concat(defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand)).Distinct(), tuple.Item3);
			_UpdateAttackCardGlows(tuple.Item3);
			attacker.view.RequestGlow(defender.view, tuple.Item3.Opposite().GetTint());
			defender.view.RequestGlow(defender.view, tuple.Item3.GetTint());
			base.buttonDeckLayout[ButtonCardType.ConfirmDefense].view.RequestGlow(defender.view, tuple.Item3.GetTint());
			_MoveCardsIntoDefenseHandAndUpdate(tuple.Item1.value);
		}
	}

	private void _OnAdventureOver(AdventureCard.Pile pile, ATarget card)
	{
		if (card == attacker || card == defender)
		{
			InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorDefend);
			if (defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) < attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand))
			{
				_findAutoDefenseActive = true;
				_FindAutoDefense();
			}
		}
	}

	private void _OnAdventureExit(AdventureCard.Pile pile, ATarget card)
	{
		InputManager.I.ReleaseCursorOverride(this);
		_UpdateAttackCardGlows(defenseResult);
		_UpdateDefenderCardGlows(base.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand), defenseResult);
		_findAutoDefenseActive = false;
		_UpdateDefenseHand();
	}

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (card != attacker && card != defender)
		{
			return;
		}
		if (defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) < attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand))
		{
			bool flag = defender.resourceDeck.Any(ResourceCard.Pile.DefenseHand);
			foreach (ResourceCard item in defender.FindDefenseHandAgainstAllowInvalid(attacker, _customWildTracker.CardsToFreeze()).hand)
			{
				defender.resourceDeck.Transfer(item, ResourceCard.Pile.DefenseHand);
			}
			if ((flag && defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) < attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand)) || defender.resourceDeck.Count(ResourceCard.Pile.DefenseHand) == 0)
			{
				base.finished = true;
			}
		}
		else if (defenseResult != 0)
		{
			base.finished = true;
		}
		else
		{
			base.view.LogError(CanAttackResult.PreventedBy.InvalidDefenseHandCount.Localize(), base.state.player.audio.character.error[CanAttackResult.PreventedBy.InvalidDefenseHandCount, false]);
		}
	}

	private void _OnSmartDragPlayerResource(ACardLayout smartDragTarget, ResourceCard card)
	{
		if (smartDragTarget == base.resourceDeck.layout.GetLayout(ResourceCard.Pile.ActivationHand))
		{
			base.resourceDeck.Transfer(card, base.player.HasAbilityThatCanActivateWhichUsesCard(card) ? ResourceCard.Pile.ActivationHand : ResourceCard.Pile.DefenseHand);
		}
		else if (smartDragTarget == base.resourceDeck.layout.GetLayout(ResourceCard.Pile.DefenseHand))
		{
			base.resourceDeck.Transfer(card, ResourceCard.Pile.DefenseHand);
		}
		else if (smartDragTarget == base.resourceDeck.layout.GetLayout(ResourceCard.Pile.Hand))
		{
			base.resourceDeck.Transfer(card, ResourceCard.Pile.Hand);
		}
	}

	private void _OnSmartDragAbility(ACardLayout smartDragTarget, Ability card)
	{
		_OnAbilityClick(card.abilityPile, card);
	}

	private void _OnPointerEnter(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (base.player.resourceDeck.Count(ResourceCard.Pile.DefenseHand) == 0 && _elapsedTime < 15f)
		{
			_DoPointerOverTutorialDelay(ref _elapsedTime, target, pile);
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if (card.faction == Faction.Player)
		{
			if (card.pile == ResourceCard.Pile.DefenseHand)
			{
				customWildTracker[card] = wildContext == ResourceCard.WildContext.UserInput;
				_CalculateDefenseResult();
				_UpdatePotentialCombatDamage();
			}
		}
		else if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(card.pile))
		{
			_defenseResult = null;
			_UpdateDefenseHand();
			if (_findAutoDefenseActive)
			{
				_FindAutoDefense();
			}
		}
	}

	private void _UpdatePotentialCombatDamage()
	{
		if (base.state.activeCombat != null && !base.finished)
		{
			if (_defenseResult == DefenseResultType.Success)
			{
				base.state.activeCombat.attacker.combatantCard.ShowPotentialDamage(this, (!base.state.activeCombat.attacker.statuses.safeAttack) ? base.state.activeCombat.potentialDefenseDamage : 0, 0, base.state.activeCombat.GetAttackerSafeAttackTraits());
				base.state.activeCombat.defender.combatantCard.HidePotentialDamage(this);
			}
			else if (_defenseResult == DefenseResultType.Tie)
			{
				_HidePotentialCombatDamage();
			}
			else
			{
				base.state.activeCombat.defender.combatantCard.ShowPotentialDamage(this, base.state.activeCombat.potentialAttackDamage);
				base.state.activeCombat.attacker.combatantCard.HidePotentialDamage(this);
			}
		}
	}

	private void _HidePotentialCombatDamage()
	{
		base.state.activeCombat.attacker.combatantCard.HidePotentialDamage(this);
		base.state.activeCombat.defender.combatantCard.HidePotentialDamage(this);
	}

	protected override IEnumerable<ButtonCardType> _Buttons()
	{
		yield return ButtonCardType.ConfirmDefense;
		if (base.resourceDeck.Count(ResourceCard.Pile.DefenseHand) > 0)
		{
			yield return ButtonCardType.CancelDefense;
		}
	}

	protected override void OnAboutToEnable()
	{
		int num = attacker.resourceDeck.Count(ResourceCard.Pile.AttackHand) - base.player.resourceDeck.Count(ResourceCard.Piles.Hand | ResourceCard.Piles.DefenseHand);
		if (num > 0 && base.player.resourceDeck.CanDraw())
		{
			AppendStep(base.player.resourceDeck.DrawStep(num));
		}
	}

	protected override void OnEnable()
	{
		_activationHandEnabled = base.player.HasAbilityThatCanActivateWhichRequiresCards();
		attacker.resourceDeck.layout.onPointerEnter += _OnAttackerResourceOver;
		attacker.resourceDeck.layout.onPointerExit += _OnAttackerResourceExit;
		attacker.resourceDeck.layout.onPointerClick += _OnAttackerResourceClick;
		base.adventureDeck.layout.onPointerClick += _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter += _OnAdventureOver;
		base.adventureDeck.layout.onPointerExit += _OnAdventureExit;
		base.buttonDeck.onTransfer += _OnButtonTransfer;
		base.resourceDeck.layout.onSmartDrag += _OnSmartDragPlayerResource;
		base.resourceDeck.layout.AddSmartDragTarget(ResourceCard.Pile.Hand, ResourceCard.Pile.DefenseHand);
		if (_activationHandEnabled)
		{
			base.resourceDeck.layout.AddSmartDragTarget(ResourceCard.Pile.Hand, ResourceCard.Pile.ActivationHand).AddSmartDragTarget(ResourceCard.Pile.DefenseHand, ResourceCard.Pile.ActivationHand);
		}
		base.abilityDeck.layout.onSmartDrag += _OnSmartDragAbility;
		ADeckLayoutBase.OnPointerEnter += _OnPointerEnter;
		base.state.onWildValueChanged += _OnWildValueChange;
		base.OnEnable();
		_UpdateDefenseHand();
		_UpdatePotentialCombatDamage();
		attacker?.resourceDeck?.layout?.GetLayout(ResourceCard.Pile.AttackHand)?.SetDirty();
	}

	public override void Start()
	{
		base.Start();
		base.state.SignalDefensePresent();
	}

	protected override void LateUpdate()
	{
		if (base.player.resourceDeck.Count(ResourceCard.Pile.DefenseHand) == 0)
		{
			if (_TickTutorialTimer(ref _elapsedTime, 15f))
			{
				base.view.LogMessage(PlayerTurnTutorial.Defend.Localize());
			}
		}
		else if (_TickTutorialTimer(ref _elapsedTime, 5f) && _defenseResult.HasValue && _defenseResult != DefenseResultType.Invalid)
		{
			base.view.LogMessage(PlayerTurnTutorial.ConfirmDefense.Localize());
		}
		base.LateUpdate();
	}

	protected override void OnDisable()
	{
		_defenseResult = null;
		attacker.resourceDeck.layout.onPointerEnter -= _OnAttackerResourceOver;
		attacker.resourceDeck.layout.onPointerExit -= _OnAttackerResourceExit;
		attacker.resourceDeck.layout.onPointerClick -= _OnAttackerResourceClick;
		base.adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter -= _OnAdventureOver;
		base.adventureDeck.layout.onPointerExit -= _OnAdventureExit;
		base.buttonDeck.onTransfer -= _OnButtonTransfer;
		base.buttonDeckLayout.Deactivate(ButtonCardType.CancelDefense);
		base.resourceDeck.layout.onSmartDrag -= _OnSmartDragPlayerResource;
		base.resourceDeck.layout.ClearSmartDragTargets();
		base.abilityDeck.layout.onSmartDrag -= _OnSmartDragAbility;
		_ResetMessageTimer(ref _elapsedTime);
		ADeckLayoutBase.OnPointerEnter -= _OnPointerEnter;
		base.state.onWildValueChanged -= _OnWildValueChange;
		base.OnDisable();
	}

	protected override void OnFinish()
	{
		_HidePotentialCombatDamage();
		defender.resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.Hand);
	}

	protected override void End()
	{
		base.state.SignalDefenseLaunched();
		AppendStep(new GameStepDecideCombatVictor());
	}

	protected override void OnDestroy()
	{
		_HidePotentialCombatDamage();
		if (base.state.activeCombat.canceled && !base.state.activeCombat.defenseHasBeenLaunched)
		{
			base.player.resourceDeck.TransferPile(ResourceCard.Pile.DefenseHand, ResourceCard.Pile.Hand);
		}
		AppendStep(base.player.DrawStep(base.player.resourceDeck.Count(ResourceCard.Pile.DefenseHand)));
		base.player.resourceDeck.TransferPile(ResourceCard.Pile.DefenseHand, ResourceCard.Pile.DiscardPile);
		base.view.ClearPersistentAttackGlowsAndLines();
		Pools.Repool(ref _customWildTracker);
	}
}
