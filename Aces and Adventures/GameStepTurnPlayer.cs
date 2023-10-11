using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepTurnPlayer : AGameStepTurnAbility
{
	public const float IDLE_TUTORIAL_TIME = 15f;

	private const float ACT_TUTORIAL_TIME = 5f;

	private float _elapsedTime;

	protected override Stone.Pile? _enabledTurnStonePile => Stone.Pile.PlayerTurn;

	protected override Stone.Pile? _disabledTurnStonePile => Stone.Pile.TurnInactive;

	protected override bool canAct => true;

	public GameStepTurnPlayer(AEntity entity)
		: base(entity)
	{
	}

	protected override void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		base._OnResourceTransfer(card, oldPile, newPile);
		if (oldPile == ResourceCard.Pile.ActivationHand)
		{
			TargetLineView.RemoveOwnedBy(card);
			if (base.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
			{
				base.view.RefreshPointerOver();
			}
		}
	}

	private void _OnAdventurePointerOver(AdventureCard.Pile pile, ATarget card)
	{
		if (pile != AdventureCard.Pile.TurnOrder || !(card is ACombatant aCombatant) || !base.player.IsEnemy(aCombatant) || !base.state.player.CanAttack(aCombatant))
		{
			return;
		}
		if (base.player.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
		{
			(PoolKeepItemListHandle<ResourceCard>, PokerHandType) attackCombatHand = base.player.GetAttackCombatHand(ResourceCard.Pile.Hand, aCombatant);
			if (!attackCombatHand.Item1)
			{
				return;
			}
			using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = attackCombatHand.Item1;
			foreach (ResourceCard item in poolKeepItemListHandle.value)
			{
				item.view.RequestGlow(this, Colors.USED);
				_AddAttackTargetLine(card, item, card, Colors.USED, AGameStepTurnAbility.AUTO_COMBAT_START_ROTATION, AGameStepTurnAbility.AUTO_COMBAT_END_ROTATION, AGameStepTurnAbility.AUTO_COMBAT_TANGENT_SCALE, AGameStepTurnAbility.AUTO_COMBAT_END_TANGENT_SCALE, AGameStepTurnAbility.AUTO_COMBAT_END_OFFSET);
			}
			card.view.RequestGlow(this, Colors.ATTACK);
			int? potentialAttackDamage = ActiveCombat.GetPotentialAttackDamage(base.player, aCombatant, poolKeepItemListHandle.value);
			if (potentialAttackDamage.HasValue)
			{
				int valueOrDefault = potentialAttackDamage.GetValueOrDefault();
				aCombatant.combatantCard.ShowPotentialDamage(this, valueOrDefault);
			}
		}
		else
		{
			foreach (ResourceCard card2 in base.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand))
			{
				AGameStepTurn.AddAttackTargetLine(card2, card2, card, Colors.ATTACK, Quaternion.AngleAxis(45f, Vector3.forward));
			}
			int? potentialAttackDamage = ActiveCombat.GetPotentialAttackDamage(base.player, aCombatant, base.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand));
			if (potentialAttackDamage.HasValue)
			{
				int valueOrDefault2 = potentialAttackDamage.GetValueOrDefault();
				aCombatant.combatantCard.ShowPotentialDamage(this, valueOrDefault2);
			}
		}
		_DisplayEffectiveEnemyDefense(aCombatant);
		InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorAttack);
	}

	private void _OnAdventurePointerExit(AdventureCard.Pile pile, ATarget card)
	{
		if (pile == AdventureCard.Pile.TurnOrder && card is ACombatant aCombatant)
		{
			if (base.player.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
			{
				_ClearGlowsFor<ResourceCard>();
				card.view.ReleaseGlow(this);
			}
			_RemoveAttackTargetLine(card);
			if (base.resourceDeck.Count(ResourceCard.Pile.AttackHand) == 0)
			{
				_StopDisplayingEffectiveEnemyDefense(aCombatant);
			}
			InputManager.I.ReleaseCursorOverride(this);
			aCombatant.combatantCard.HidePotentialDamage(this);
		}
	}

	private void _OnAdventureClick(AdventureCard.Pile pile, ATarget card)
	{
		if (pile != AdventureCard.Pile.TurnOrder || !(card is ACombatant aCombatant) || !base.player.IsEnemy(aCombatant) || !base.state.player.CanAttack(aCombatant).Message())
		{
			return;
		}
		if (base.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
		{
			(PoolKeepItemListHandle<ResourceCard>, PokerHandType) attackCombatHand = base.player.GetAttackCombatHand(ResourceCard.Pile.Hand, aCombatant);
			if ((bool)attackCombatHand.Item1)
			{
				foreach (ResourceCard item in attackCombatHand.Item1)
				{
					base.player.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
				}
			}
		}
		base.resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.AttackHand);
		if (base.player.resourceDeck.Count(ResourceCard.Pile.AttackHand) > 0)
		{
			AppendStep(new GameStepTurnPlayerPrepareAttack(base.player, aCombatant));
		}
		else
		{
			base.view.LogError(AbilityPreventedBy.ResourceCard.LocalizeError(), base.state.player.audio.character.error[AbilityPreventedBy.ResourceCard]);
		}
	}

	protected override void _OnActivationHandChange()
	{
		base._OnActivationHandChange();
		_ResetMessageTimer(ref _elapsedTime);
		if (!base.state.player.canAttack || !base.state.player.CanFormAttack(base.resourceDeck.GetCards(ResourceCard.Pile.ActivationHand)))
		{
			_ClearGlowsFor<ACombatant>();
			return;
		}
		foreach (AEntity enemy in base.state.GetEnemies(base.state.player))
		{
			if (enemy.canBeAttacked)
			{
				enemy.view.RequestGlow(this, Colors.ATTACK);
			}
		}
	}

	protected override void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		base._OnButtonClick(pile, card);
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.EndTurn)
		{
			Cancel();
		}
	}

	protected override void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.PlayerTurn)
		{
			Cancel();
		}
	}

	private void _OnPointerEnter(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (base.player.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0 && _elapsedTime < 15f)
		{
			_DoPointerOverTutorialDelay(ref _elapsedTime, target, pile);
		}
	}

	protected override IEnumerable<ButtonCardType> _Buttons()
	{
		yield return ButtonCardType.EndTurn;
	}

	protected override void OnEnable()
	{
		ADeckLayoutBase.OnPointerEnter += _OnPointerEnter;
		base.adventureDeck.layout.onPointerClick += _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter += _OnAdventurePointerOver;
		base.adventureDeck.layout.onPointerExit += _OnAdventurePointerExit;
		Color color = ((!base.player.canAct) ? Colors.END_TURN : ((!base.player.canAttack) ? Colors.END_TURN_CAUTION : Colors.TRANSPARENT));
		base.buttonDeckLayout[ButtonCardType.EndTurn].view.RequestGlow(this, color);
		base.stoneDeckLayout[StoneType.Turn].view.RequestGlow(this, color);
		base.OnEnable();
	}

	public override void Start()
	{
		base.state.SignalTurnStartLate(base.player);
	}

	protected override void LateUpdate()
	{
		if (base.player.resourceDeck.Count(ResourceCard.Pile.ActivationHand) == 0)
		{
			if (_TickTutorialTimer(ref _elapsedTime, 15f))
			{
				base.view.LogMessage(((!base.player.canAttack) ? (base.player.HasAbilityThatCanActivate() ? PlayerTurnTutorial.IdleAbility : PlayerTurnTutorial.IdleEndTurn) : PlayerTurnTutorial.IdleAttack).Localize());
			}
		}
		else if (_TickTutorialTimer(ref _elapsedTime, 5f))
		{
			bool flag = base.state.GetEntities(Faction.Enemy).Any((AEntity enemy) => enemy.view.hasGlow);
			bool flag2 = base.player.HasAbilityThatCanActivate();
			base.view.LogMessage(((!flag) ? (flag2 ? PlayerTurnTutorial.ActAbility : PlayerTurnTutorial.ActInvalid) : (flag2 ? PlayerTurnTutorial.ActAttackAndAbility : PlayerTurnTutorial.ActAttack)).Localize());
		}
		base.LateUpdate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ADeckLayoutBase.OnPointerEnter -= _OnPointerEnter;
		base.adventureDeck.layout.onPointerClick -= _OnAdventureClick;
		base.adventureDeck.layout.onPointerEnter -= _OnAdventurePointerOver;
		base.adventureDeck.layout.onPointerExit -= _OnAdventurePointerExit;
		_ResetMessageTimer(ref _elapsedTime);
	}

	protected override void OnDestroy()
	{
		base.player.hasTakenTurn.value = true;
	}
}
