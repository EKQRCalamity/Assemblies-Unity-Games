using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AGameStepTurnAbility : AGameStepTurn
{
	protected static readonly Quaternion AUTO_COMBAT_START_ROTATION = Quaternion.AngleAxis(120f, Vector3.right);

	protected static readonly Quaternion AUTO_COMBAT_END_ROTATION = Quaternion.AngleAxis(-60f, Vector3.right);

	protected static readonly float AUTO_COMBAT_TANGENT_SCALE = 0.75f;

	protected static readonly float AUTO_COMBAT_END_TANGENT_SCALE = 2f;

	protected static readonly Vector3 AUTO_COMBAT_END_OFFSET = new Vector3(0f, 0.002f, 0f);

	private Ability _showDamageAbility;

	protected Player player => (Player)_entity;

	protected IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck => base.state.player.resourceDeck;

	protected IdDeck<Ability.Pile, Ability> abilityDeck => base.state.player.abilityDeck;

	protected virtual bool canAct => player.HasAbilityThatCanActivate();

	protected virtual ResourceCard.Piles _backPiles => ResourceCard.Piles.ActivationHand;

	protected virtual ResourceCard.Piles _wildPiles => ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;

	protected virtual ResourceCard.Piles _enemyWildPiles => (ResourceCard.Piles)0;

	protected virtual ControlGainType _controlType => ControlGainType.Player;

	public override bool canSafelyCancelStack => true;

	public override bool countsTowardsStrategyTime => true;

	public AGameStepTurnAbility(AEntity entity)
		: base(entity)
	{
	}

	private void _UpdateAbilityCanActivateGlows()
	{
		foreach (Ability card in abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct))
		{
			if ((bool)card.CanActivate())
			{
				card.view.RequestGlow(this, Colors.ACTIVATE, request: true);
			}
			else if (card.hasActiveReaction)
			{
				card.view.RequestGlow(this, Colors.FAILURE, request: true);
			}
			else
			{
				card.view.RequestGlow(this, Colors.ACTIVATE, request: false);
			}
		}
	}

	protected void _DisplayEffectiveEnemyDefense(ACombatant enemy)
	{
		if (enemy == null)
		{
			return;
		}
		int defenseAgainst = enemy.GetDefenseAgainst(player);
		bool num = enemy.combatantCard.displayedDefense != defenseAgainst;
		enemy.combatantCard.defenseText.text = defenseAgainst.ToStringPooled();
		ATextMeshProAnimator.CreateColor(enemy.combatantCard.defenseText, null, -1f, Colors.STAT_HIGHLIGHT_RED);
		ATextMeshProAnimator.CreateColor(player.combatantCard.offenseText, null, -1f, Colors.STAT_HIGHLIGHT_GREEN);
		if (!num)
		{
			return;
		}
		foreach (IAnimatedUI item in enemy.combatantCard.defenseIcon.transform.parent.gameObject.GetComponentsInChildrenPooled<IAnimatedUI>())
		{
			item.AddStaggeredAnimations(new Vector3(0f, 0f, -20f), Vector3.zero, Vector3.one * 2f, 0.25f, 0.0333f, 5);
		}
		foreach (IAnimatedUI item2 in player.combatantCard.offenseIcon.transform.parent.gameObject.GetComponentsInChildrenPooled<IAnimatedUI>())
		{
			item2.AddStaggeredAnimations(new Vector3(0f, 0f, -20f), Vector3.zero, Vector3.one * 2f, 0.25f, 0.0333f, 5);
		}
	}

	protected void _StopDisplayingEffectiveEnemyDefense(ACombatant enemy)
	{
		if (enemy != null)
		{
			enemy.combatantCard.defenseText.text = enemy.stats.defense.value.ToStringPooled();
			ATextMeshProAnimator.StopAll(enemy.combatantCard.defenseText.transform);
			ATextMeshProAnimator.StopAll(player.combatantCard.offenseText.transform);
		}
	}

	private void _HidePotentialDamage()
	{
		_showDamageAbility?.HidePotentialDamage();
		_showDamageAbility = null;
	}

	protected virtual void _OnResourceOver(ResourceCard.Pile pile, ResourceCard card)
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(pile))
		{
			foreach (Ability card2 in player.abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct))
			{
				if (card2.cost.GetResourceFilters().Any((PlayingCard.Filter f) => (bool)f && f.AreValid(card)))
				{
					card2.view.RequestGlow(card.view, Colors.CAN_BE_USED);
				}
			}
		}
		if (_backPiles.Contains(pile))
		{
			InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorBack);
		}
	}

	protected virtual void _OnResourceExit(ResourceCard.Pile pile, ResourceCard card)
	{
		card.view.ReleaseOwnedGlowRequests();
		InputManager.I.ReleaseCursorOverride(this);
	}

	protected virtual void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (oldPile == ResourceCard.Pile.ActivationHand || newPile == ResourceCard.Pile.ActivationHand)
		{
			_OnActivationHandChange();
		}
		else if (oldPile == ResourceCard.Pile.Hand || newPile == ResourceCard.Pile.Hand)
		{
			_UpdateAbilityCanActivateGlows();
		}
	}

	protected virtual void _OnResourceClick(ResourceCard.Pile pile, ResourceCard card)
	{
		switch (pile)
		{
		case ResourceCard.Pile.Hand:
			resourceDeck.Transfer(card, ResourceCard.Pile.ActivationHand);
			break;
		case ResourceCard.Pile.ActivationHand:
			resourceDeck.Transfer(card, ResourceCard.Pile.Hand);
			break;
		}
	}

	private Ability.CanActivateResult? _CanActivate(Ability.Pile? pile, Ability card)
	{
		if (!(Ability.Piles.Hand | Ability.Piles.HeroAct).Contains(pile))
		{
			return null;
		}
		return card.CanActivate();
	}

	protected virtual void _OnAbilityTransfer(Ability card, Ability.Pile? oldPile, Ability.Pile? newPile)
	{
		card.view.RequestGlow(this, Colors.ACTIVATE, _CanActivate(newPile, card));
	}

	protected virtual void _OnAbilityClick(Ability.Pile pile, Ability card)
	{
		if ((bool)_CanActivate(pile, card).Message())
		{
			card.Activate();
		}
	}

	protected virtual void _OnAbilityOver(Ability.Pile pile, Ability card)
	{
		if ((Ability.Piles.Hand | Ability.Piles.HeroAct).Contains(pile))
		{
			foreach (ResourceCard resourceCard in resourceDeck.GetCards(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand))
			{
				if (card.cost.GetResourceFilters().Any((PlayingCard.Filter t) => t.AreValid(resourceCard)))
				{
					resourceCard.view.RequestGlow(card.view, Colors.CAN_BE_USED);
				}
			}
			foreach (ATargetView additionalResourceView in player.playerCard.GetAdditionalResourceViews(card.cost.additionalCosts))
			{
				additionalResourceView.RequestGlow(card.view, Colors.CAN_BE_USED);
			}
		}
		if (!_CanActivate(pile, card))
		{
			return;
		}
		foreach (ATarget target in card.GetTargets())
		{
			target.view.RequestGlow(card.view, Colors.TARGET);
		}
		PoolKeepItemListHandle<ResourceCard> activationCards = card.GetActivationCards();
		if (activationCards != null)
		{
			int num = 0;
			foreach (ResourceCard item in activationCards)
			{
				item.view.RequestGlow(card.view, Colors.USED);
				TargetLineView.AddUnique(card, Colors.USED, item.view[CardTarget.Name], card.view[CardTarget.Cost], Quaternion.AngleAxis(90f, Vector3.right), Quaternion.AngleAxis(60f, Vector3.right), (TargetLineTags)0, 1f, 1f, null, new Vector3((float)num++ * 0.0044f, 0.0005f, 0.0025f), 0.7f);
			}
			foreach (ATargetView additionalResourceView2 in player.playerCard.GetAdditionalResourceViews(card.cost.additionalCosts))
			{
				additionalResourceView2.RequestGlow(card.view, Colors.USED);
				TargetLineView.AddUnique(card, Colors.USED, additionalResourceView2[CardTarget.Center], card.view[CardTarget.Cost], Quaternion.AngleAxis(0f, Vector3.right), Quaternion.AngleAxis(60f, Vector3.right), (TargetLineTags)0, 1f, 1f, null, new Vector3((float)num++ * 0.0044f, 0.0005f, 0.0025f), 0.7f);
			}
		}
		if (card.ShowPotentialDamage())
		{
			_showDamageAbility = card;
		}
	}

	protected virtual void _OnAbilityExit(Ability.Pile pile, Ability card)
	{
		card.view.ReleaseOwnedGlowRequests();
		TargetLineView.RemoveOwnedByExcept(card, TargetLineTags.Persistent);
		_HidePotentialDamage();
	}

	protected virtual void _OnActivationHandChange()
	{
		base.buttonDeckLayout.SetActive(ButtonCardType.ClearActionHand, resourceDeck.Count(ResourceCard.Pile.ActivationHand) > 0);
		_UpdateAbilityCanActivateGlows();
	}

	protected override void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		base._OnButtonClick(pile, card);
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.ClearActionHand)
		{
			resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.Hand);
		}
	}

	protected override void _OnBackPressed()
	{
		base._OnBackPressed();
		if (!base.canceled)
		{
			base.view.RefreshPointerOver();
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext context)
	{
		if (card.faction == Faction.Enemy && (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(card.pile))
		{
			_UpdateAbilityCanActivateGlows();
		}
	}

	private void _OnPointerClick(ACardLayout layout, CardLayoutElement card, PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Right || !ProfileManager.options.devInputEnabled)
		{
			return;
		}
		ATarget card2 = card.card;
		if (card2 is ResourceCard resourceCard)
		{
			if (resourceCard.faction == Faction.Player)
			{
				if (resourceCard.pile == ResourceCard.Pile.Hand)
				{
					AppendStep(player.resourceDeck.DiscardStep(resourceCard));
				}
				else if (resourceCard.pile == ResourceCard.Pile.DrawPile)
				{
					AppendStep(player.resourceDeck.DrawStep());
				}
				else if (resourceCard.pile == ResourceCard.Pile.DiscardPile)
				{
					AppendStep(player.resourceDeck.DrawStep(1, ResourceCard.Pile.DiscardPile));
				}
			}
			else if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(resourceCard.pile))
			{
				AppendStep(resourceCard.deck.DiscardStep(resourceCard));
			}
		}
		else if (card2 is Ability ability)
		{
			if (ability.owner is Player)
			{
				if (ability.abilityPile == Ability.Pile.Hand)
				{
					AppendStep(player.abilityDeck.DiscardStep(ability));
				}
				else if (ability.nullablePile == Ability.Pile.Draw)
				{
					AppendStep(player.abilityDeck.DrawStep());
				}
				else if (ability.abilityPile == Ability.Pile.Discard)
				{
					AppendStep(player.abilityDeck.DrawStep(1, Ability.Pile.Discard));
				}
				else if (ability.data.category == AbilityData.Category.HeroAbility && (int)player.numberOfHeroAbilities == 0)
				{
					AppendStep(new GameStepGenericSimple(delegate
					{
						player.numberOfHeroAbilities.value++;
					}));
				}
				else
				{
					ItemCard item = ability as ItemCard;
					if (item != null && !item.isConsumable && ability.abilityPile == Ability.Pile.HeroAct)
					{
						AppendStep(new GameStepGenericSimple(delegate
						{
							item.hasBeenUsedThisRound = false;
						}));
					}
					else if (ability.itemType.IsCondition())
					{
						base.state.adventureDeck.Discard(ability.Unapply());
					}
					else if (ability.data.type == AbilityData.Type.Summon)
					{
						base.state.stack.activeStep.AppendStep(new GameStepRemoveSummon(null, new ActionContext(player, ability, ability), ability, isBeingReplaced: false));
					}
					else if (ability.isBuff && (bool)ability.view?.GetComponentInParent<CombatantCardView>())
					{
						ability.Remove(new ActionContext(player, ability, ability));
					}
					else if (ability.nullablePile == Ability.Pile.HeroPassive && ability.isTrait)
					{
						player.RemoveTrait(ability);
						player.AddTrait(ability);
					}
				}
			}
		}
		else if (card2 is Player)
		{
			AppendStep(new GameStepGenericSimple(delegate
			{
				if (player.HPMissing > 0)
				{
					player.HP.value++;
				}
				player.numberOfAttacks.value++;
			}));
		}
		else
		{
			Enemy enemy = card2 as Enemy;
			if (enemy != null && enemy.pile == AdventureCard.Pile.TurnOrder)
			{
				AppendStep(new GameStepGenericSimple(delegate
				{
					base.state.KillCombatant(enemy);
				}));
			}
		}
		base.state.devCommandUsed = true;
	}

	protected override void OnEnable()
	{
		if (!canAct)
		{
			Cancel();
			return;
		}
		base.state.SignalControlGained(player, _controlType);
		resourceDeck.onTransfer += _OnResourceTransfer;
		resourceDeck.layout.onPointerClick += _OnResourceClick;
		resourceDeck.layout.onPointerEnter += _OnResourceOver;
		resourceDeck.layout.onPointerExit += _OnResourceExit;
		abilityDeck.onTransfer += _OnAbilityTransfer;
		abilityDeck.layout.onPointerClick += _OnAbilityClick;
		abilityDeck.layout.onPointerEnter += _OnAbilityOver;
		abilityDeck.layout.onPointerExit += _OnAbilityExit;
		base.state.onWildValueChanged += _OnWildValueChange;
		ACardLayout.OnPointerClick += _OnPointerClick;
		base.OnEnable();
		_OnActivationHandChange();
		base.view.wildPiles = _wildPiles;
		base.view.enemyWildPiles = _enemyWildPiles;
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void LateUpdate()
	{
		if (ProfileManager.options.devInputEnabled && InputManager.I[KeyCode.K][KState.JustPressed] && (base.state.devCommandUsed = true))
		{
			AppendStep(new GameStepGenericSimple(delegate
			{
				base.state.KillAllEnemies();
			}));
		}
	}

	protected override void OnCanceled()
	{
		resourceDeck.TransferPile(ResourceCard.Pile.ActivationHand, ResourceCard.Pile.Hand);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		resourceDeck.onTransfer -= _OnResourceTransfer;
		resourceDeck.layout.onPointerClick -= _OnResourceClick;
		resourceDeck.layout.onPointerEnter -= _OnResourceOver;
		resourceDeck.layout.onPointerExit -= _OnResourceExit;
		abilityDeck.onTransfer -= _OnAbilityTransfer;
		abilityDeck.layout.onPointerClick -= _OnAbilityClick;
		abilityDeck.layout.onPointerEnter -= _OnAbilityOver;
		abilityDeck.layout.onPointerExit -= _OnAbilityExit;
		base.state.onWildValueChanged -= _OnWildValueChange;
		ACardLayout.OnPointerClick -= _OnPointerClick;
		base.buttonDeckLayout.SetActive(ButtonCardType.ClearActionHand, setActive: false);
		_HidePotentialDamage();
	}
}
