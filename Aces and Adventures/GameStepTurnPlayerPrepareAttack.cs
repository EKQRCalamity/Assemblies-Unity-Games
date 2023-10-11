using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepTurnPlayerPrepareAttack : AGameStepTurnAbility
{
	private const float TUTORIAL_TIME = 5f;

	private ACombatant _target;

	private float _elapsedTime;

	private bool _activationHandEnabled;

	protected override bool canAct => base.player.canAttack;

	protected override ResourceCard.Piles _backPiles => base._backPiles | ResourceCard.Piles.AttackHand;

	public GameStepTurnPlayerPrepareAttack(AEntity attacker, ACombatant target)
		: base(attacker)
	{
		_target = target;
	}

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (card != _target)
		{
			return;
		}
		if (base.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
		{
			if (!base.player.CanFormAttack(base.resourceDeck.GetCards(ResourceCard.Pile.AttackHand)))
			{
				(PoolKeepItemListHandle<ResourceCard>, PokerHandType) combatHand = base.player.resourceDeck.GetCards(ResourceCard.Pile.AttackHand).GetCombatHand(null, base.player.attackHandOrder);
				using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = combatHand.Item1;
				if (!poolKeepItemListHandle || poolKeepItemListHandle.Count != base.player.resourceDeck.Count(ResourceCard.Pile.AttackHand))
				{
					base.view.LogError(CanAttackResult.PreventedBy.InvalidHand.Localize(), base.player.audio.character.error[CanAttackResult.PreventedBy.InvalidHand, true]);
				}
				else if (!EnumUtil.HasFlagConvert(base.player.CanAttackWith(), combatHand.Item2))
				{
					base.view.LogError(((CanAttackResult.PreventedBy)(8 + combatHand.Item2)).Localize(), base.player.audio.character.error[CanAttackResult.PreventedBy.InvalidHand, true]);
				}
				else
				{
					base.view.LogError(((CanAttackResult.PreventedBy)(19 + combatHand.Item2)).Localize(), base.player.audio.character.error[CanAttackResult.PreventedBy.InvalidHand, true]);
				}
				return;
			}
			TransitionTo(new GameStepGroupDynamic(new GameStepLaunchAttack()));
		}
		else
		{
			base.resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.AttackHand);
		}
	}

	private void _OnAdventureEnter(AdventureCard.Pile pile, ATarget adventureCard)
	{
		if (adventureCard != _target)
		{
			return;
		}
		foreach (ResourceCard card in base.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand))
		{
			AGameStepTurn.AddAttackTargetLine(_target, card, _target, Colors.ATTACK, Quaternion.AngleAxis(45f, Vector3.forward));
		}
		InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorAttack);
	}

	private void _OnAdventureExit(AdventureCard.Pile pile, ATarget adventureCard)
	{
		TargetLineView.RemoveOwnedBy(_target);
		InputManager.I.ReleaseCursorOverride(this);
	}

	private void _OnChipClick(Chip.Pile pile, Chip card)
	{
		if (pile == Chip.Pile.ActiveAttack)
		{
			_OnAdventureClick(AdventureCard.Pile.TurnOrder, _target);
		}
	}

	private void _OnChipEnter(Chip.Pile pile, Chip card)
	{
		if (pile == Chip.Pile.ActiveAttack)
		{
			_OnAdventureEnter(AdventureCard.Pile.TurnOrder, _target);
		}
	}

	private void _OnChipExit(Chip.Pile pile, Chip card)
	{
		if (pile == Chip.Pile.ActiveAttack || pile == Chip.Pile.Inactive)
		{
			_OnAdventureExit(AdventureCard.Pile.TurnOrder, _target);
		}
	}

	protected override void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		base._OnResourceTransfer(card, oldPile, newPile);
		_ResetMessageTimer(ref _elapsedTime);
		if (newPile == ResourceCard.Pile.AttackHand)
		{
			_AddAttackTargetLine(card, card, _target, Colors.ATTACK_CONFIRM);
		}
		else if (oldPile == ResourceCard.Pile.AttackHand)
		{
			_RemoveAttackTargetLine(card, _target);
			if (base.resourceDeck.Count(ResourceCard.Pile.AttackHand) == 0)
			{
				Cancel();
			}
		}
	}

	protected override void _OnResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		switch (pile)
		{
		case ResourceCard.Pile.Hand:
			base.resourceDeck.Transfer(card, (_activationHandEnabled && base.player.HasAbilityThatCanActivateWhichUsesCard(card)) ? ResourceCard.Pile.ActivationHand : ResourceCard.Pile.AttackHand);
			break;
		case ResourceCard.Pile.ActivationHand:
			base.resourceDeck.Transfer(card, ResourceCard.Pile.Hand);
			break;
		case ResourceCard.Pile.AttackHand:
			base.resourceDeck.Transfer(card, ResourceCard.Pile.Hand);
			break;
		}
	}

	protected override void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		base._OnButtonClick(pile, card);
		switch (card.type)
		{
		case ButtonCardType.ConfirmAttack:
			_OnAdventureClick(AdventureCard.Pile.TurnOrder, _target);
			break;
		case ButtonCardType.CancelAttack:
			Cancel();
			break;
		}
	}

	private void _OnSmartDragPlayerResource(ACardLayout smartDragTarget, ResourceCard card)
	{
		if (smartDragTarget == base.resourceDeck.layout.GetLayout(ResourceCard.Pile.ActivationHand))
		{
			base.resourceDeck.Transfer(card, base.player.HasAbilityThatCanActivateWhichUsesCard(card) ? ResourceCard.Pile.ActivationHand : ((card.pile == ResourceCard.Pile.AttackHand) ? ResourceCard.Pile.Hand : ResourceCard.Pile.AttackHand));
		}
		else if (smartDragTarget == base.resourceDeck.layout.GetLayout(ResourceCard.Pile.AttackHand))
		{
			base.resourceDeck.Transfer(card, ResourceCard.Pile.AttackHand);
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

	private void _OnCombatantStatChange(int previous, int current)
	{
		_DisplayEffectiveEnemyDefense(_target);
	}

	private void _OnDragBegin(ACardLayout layout, CardLayoutElement card)
	{
		if (card.card == _target)
		{
			_StopDisplayingEffectiveEnemyDefense(_target);
		}
	}

	public override void StopDisplayingEffectiveStats()
	{
		_StopDisplayingEffectiveEnemyDefense(_target);
	}

	private void _OnDragEnd(ACardLayout layout, CardLayoutElement card)
	{
		if (card.card == _target)
		{
			_DisplayEffectiveEnemyDefense(_target);
		}
	}

	public override void DisplayEffectiveStats()
	{
		_DisplayEffectiveEnemyDefense(_target);
	}

	private void _TransferAttackChip(Chip.Pile transferTo)
	{
		Chip chip = base.state.chipDeck.FirstInPile(Chip.Pile.ActiveAttack);
		if (chip != null)
		{
			base.state.chipDeck.Transfer(chip, transferTo).view.ReleaseGlow(this, GlowTags.Persistent);
		}
	}

	private void _UpdatePotentialAttackDamage()
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(base.resourceDeck.GetCards(ResourceCard.Pile.AttackHand));
		if (base.player.CanFormAttack(poolKeepItemListHandle.value))
		{
			using (ResourceCard.WildSnapshot.Create(poolKeepItemListHandle.value))
			{
				if (!base.player.CanFormAttack(poolKeepItemListHandle.value, _target, poolKeepItemListHandle.value))
				{
					base.player.GetAttackCombatHand(ResourceCard.Pile.AttackHand, _target).WildIntoPokerHand();
				}
				_target.combatantCard.ShowPotentialDamage(this, base.state.activeCombat.potentialAttackDamage);
				return;
			}
		}
		_target.combatantCard.HidePotentialDamage(this);
	}

	private void _OnWildValueChanged(ResourceCard card, ResourceCard.WildContext context)
	{
		if (card.pile.IsCombat())
		{
			_UpdatePotentialAttackDamage();
		}
	}

	private void _OnResourceTransferPersistent(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (((oldPile?.IsCombat() ?? false) ^ (newPile?.IsCombat() ?? false)) && card.deck.Any(ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand))
		{
			_UpdatePotentialAttackDamage();
		}
	}

	private void _DisablePotentialAttackDamage()
	{
		_target.combatantCard.HidePotentialDamage(this);
		base.state.onWildValueChanged -= _OnWildValueChanged;
		base.state.activeCombat.attacker.resourceDeck.onTransfer -= _OnResourceTransferPersistent;
		base.state.activeCombat.defender.resourceDeck.onTransfer -= _OnResourceTransferPersistent;
	}

	protected override void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.TurnInactive)
		{
			base.view.LogError(EndTurnPreventedBy.PreparingAttack.Localize(), base.player.audio.character.error[EndTurnPreventedBy.PreparingAttack]);
		}
	}

	protected override IEnumerable<ButtonCardType> _Buttons()
	{
		yield return ButtonCardType.ConfirmAttack;
		yield return ButtonCardType.CancelAttack;
	}

	protected override void OnEnable()
	{
		_activationHandEnabled = base.player.HasAbilityThatCanActivateWhichRequiresCards();
		GameState gameState = base.state;
		if (gameState.activeCombat == null)
		{
			ActiveCombat activeCombat2 = (gameState.activeCombat = new ActiveCombat(base.player, _target));
		}
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType) attackCombatHand = base.player.GetAttackCombatHand(ResourceCard.Pile.AttackHand, _target);
		if ((bool)attackCombatHand.Item1 && (!base.resourceDeck.GetCards(ResourceCard.Pile.AttackHand).Any((ResourceCard c) => c.isWild) || base.player.GetAttackHand(_target).hand.Count != attackCombatHand.Item1.Count))
		{
			attackCombatHand.WildIntoPokerHand();
		}
		base.adventureDeck.layout.onPointerClick += _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter += _OnAdventureEnter;
		base.adventureDeck.layout.onPointerExit += _OnAdventureExit;
		base.chipDeck.layout.onPointerClick += _OnChipClick;
		base.chipDeck.layout.onPointerEnter += _OnChipEnter;
		base.chipDeck.layout.onPointerExit += _OnChipExit;
		_target.view.RequestGlow(this, Colors.ATTACK_CONFIRM);
		foreach (ResourceCard card in base.resourceDeck.GetCards(ResourceCard.Pile.AttackHand))
		{
			_AddAttackTargetLine(card, card, _target, Colors.ATTACK_CONFIRM);
		}
		base.resourceDeck.layout.AddSmartDragTarget(ResourceCard.Pile.Hand, ResourceCard.Pile.AttackHand);
		if (_activationHandEnabled)
		{
			base.resourceDeck.layout.AddSmartDragTarget(ResourceCard.Pile.Hand, ResourceCard.Pile.ActivationHand).AddSmartDragTarget(ResourceCard.Pile.AttackHand, ResourceCard.Pile.ActivationHand);
		}
		base.resourceDeck.layout.onSmartDrag += _OnSmartDragPlayerResource;
		base.abilityDeck.layout.onSmartDrag += _OnSmartDragAbility;
		if (base.state.chipDeck.Count(Chip.Pile.ActiveAttack) == 0)
		{
			base.state.chipDeck.Transfer(base.state.chipDeck.NextInPile(Chip.Pile.Attack), Chip.Pile.ActiveAttack);
		}
		if ((bool)base.player.statuses.safeAttack)
		{
			base.state.chipDeck.FirstInPile(Chip.Pile.ActiveAttack).view.RequestGlow(this, Colors.ACTIVATE, GlowTags.Persistent);
		}
		base.OnEnable();
		_UpdatePotentialAttackDamage();
	}

	public override void Start()
	{
		base.Start();
		ACardLayout.OnDragBegan += _OnDragBegin;
		ACardLayout.OnDragEnded += _OnDragEnd;
		_DisplayEffectiveEnemyDefense(_target);
		base.state.activeCombat.attacker.stats.offense.onValueChanged += _OnCombatantStatChange;
		base.state.activeCombat.defender.stats.defense.onValueChanged += _OnCombatantStatChange;
		base.state.onWildValueChanged += _OnWildValueChanged;
		base.state.activeCombat.attacker.resourceDeck.onTransfer += _OnResourceTransferPersistent;
		base.state.activeCombat.defender.resourceDeck.onTransfer += _OnResourceTransferPersistent;
	}

	protected override void LateUpdate()
	{
		if (_TickTutorialTimer(ref _elapsedTime, 5f) && base.player.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0 && base.player.CanFormAttack(base.player.resourceDeck.GetCards(ResourceCard.Pile.AttackHand), _target))
		{
			base.view.LogMessage(PlayerTurnTutorial.ConfirmAttack.Localize());
		}
		base.LateUpdate();
	}

	protected override void OnDisable()
	{
		base.adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter -= _OnAdventureEnter;
		base.adventureDeck.layout.onPointerExit -= _OnAdventureExit;
		base.chipDeck.layout.onPointerClick -= _OnChipClick;
		base.chipDeck.layout.onPointerEnter -= _OnChipEnter;
		base.chipDeck.layout.onPointerExit -= _OnChipExit;
		base.resourceDeck.layout.ClearSmartDragTargets();
		base.resourceDeck.layout.onSmartDrag -= _OnSmartDragPlayerResource;
		base.abilityDeck.layout.onSmartDrag -= _OnSmartDragAbility;
		_ResetMessageTimer(ref _elapsedTime);
		base.OnDisable();
	}

	protected override void OnCanceled()
	{
		_DisablePotentialAttackDamage();
		base.OnCanceled();
		base.resourceDeck.TransferPile(ResourceCard.Pile.AttackHand, ResourceCard.Pile.Hand);
		_TransferAttackChip(Chip.Pile.Attack);
		InputManager.I.ReleaseCursorOverride(this);
	}

	protected override void OnFinish()
	{
		_DisablePotentialAttackDamage();
	}

	public override void OnCompletedSuccessfully()
	{
		base.OnCompletedSuccessfully();
		if ((bool)base.player.statuses.redrawAttack)
		{
			AppendStep(base.player.DrawStep(base.resourceDeck.Count(ResourceCard.Pile.AttackHand)));
		}
		base.resourceDeck.TransferPile(ResourceCard.Pile.AttackHand, ResourceCard.Pile.DiscardPile);
		_TransferAttackChip(Chip.Pile.Inactive);
	}

	protected override void OnDestroy()
	{
		_DisablePotentialAttackDamage();
		ACardLayout.OnDragBegan -= _OnDragBegin;
		ACardLayout.OnDragEnded -= _OnDragEnd;
		base.state.activeCombat.attacker.stats.offense.onValueChanged -= _OnCombatantStatChange;
		base.state.activeCombat.defender.stats.defense.onValueChanged -= _OnCombatantStatChange;
		base.state.activeCombat = null;
		_StopDisplayingEffectiveEnemyDefense(_target);
		base.OnDestroy();
	}
}
