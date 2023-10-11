using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public abstract class GameStepSelectAbilityFromDeck : IdDeck<Ability.Pile, Ability>.GameStepInspectPile<Ability.Piles>
{
	protected int _selectCount;

	private PoolKeepItemDictionaryHandle<Ability, ACardLayout> _originalCardsCopy;

	protected Ability _selectedAbility;

	protected ACardLayout _selectedAbilityLayout;

	protected PoolKeepItemHashSetHandle<Ability> _additionalAbilities;

	protected IEnumerable<Ability> originalCards
	{
		get
		{
			IEnumerable<Ability> enumerable = _originalCardsCopy?.value.Keys;
			return enumerable ?? Enumerable.Empty<Ability>();
		}
	}

	protected virtual CardHandLayoutSettings _layoutSettingsOverride => null;

	protected virtual bool _includeAppliedAbilities => false;

	protected virtual ContentRefDefaults.SelectAbilityData.SelectAbilityActions _selectedAbilityActions => null;

	protected override ButtonCardType _cancelButtonType => ButtonCardType.Skip;

	protected override bool _showViewMap => true;

	protected GameStepSelectAbilityFromDeck(IdDeck<Ability.Pile, Ability> deck, MessageData.GameTooltips message, int selectCount = 1)
		: base(deck, Ability.Piles.Draw | Ability.Piles.Hand | Ability.Piles.ActivationHand | Ability.Piles.Discard, (IComparer<Ability>)Comparer<Ability>.Default, (IEqualityComparer<CardLayoutElement>)((selectCount == 1) ? AbilityDataRefEqualityComparer.Default : null), (MessageData.GameTooltips?)message, (CardHandLayoutSettings)null, (IEnumerable<Ability>)null, (Func<Ability, bool>)null, (Action<Ability>)Ability.OnEnterInspect, (Action<Ability>)Ability.OnExitInspect, dragEntireHand: true)
	{
		_selectCount = selectCount;
	}

	private bool _CanInteractWithAbility(Ability card)
	{
		if (card.view.layout == base.view.adventureDeckLayout.inspectHand)
		{
			return _CanSelectAbility(card) != false;
		}
		return false;
	}

	private void _OnAbilityPointerClick(Ability.Pile pile, Ability card)
	{
		if (base._finishedAnimatingIn && !_done && _CanInteractWithAbility(card) && (_selectedAbility = card) != null && (bool)(_selectedAbilityLayout = _originalCardsCopy[_selectedAbility]))
		{
			_OnAbilityClick(card);
		}
	}

	private void _OnAbilityPointerEnter(Ability.Pile pile, Ability card)
	{
		if (_CanInteractWithAbility(card))
		{
			_OnAbilityEnter(card);
		}
	}

	private void _OnAbilityPointerExit(Ability.Pile pile, Ability card)
	{
		if (_CanInteractWithAbility(card))
		{
			_OnAbilityExit(card);
		}
	}

	private void _OnCardClick(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (target is Ability ability)
		{
			PoolKeepItemHashSetHandle<Ability> additionalAbilities = _additionalAbilities;
			if (additionalAbilities != null && additionalAbilities.value?.Contains(ability) == true)
			{
				_OnAbilityPointerClick(Ability.Pile.Hand, ability);
			}
		}
	}

	private void _OnAbilitySelected()
	{
		_selectedAbilityActions?.AppendSteps(base.state, _GetStepsToRunAfterSelectActions().Concat(new GameStepGenericSimple(_ClearSelectedAbility)), _selectedAbility);
	}

	private void _ClearSelectedAbility()
	{
		_selectedAbility = null;
	}

	protected virtual bool? _CanSelectAbility(Ability ability)
	{
		return true;
	}

	protected virtual void _OnAbilityClick(Ability ability)
	{
		_RemoveCard(ability);
		_done = --_selectCount <= 0 || base.count == 0;
		if (_done)
		{
			ability.view.inputEnabled = false;
		}
		else
		{
			_OnAbilitySelected();
		}
	}

	protected virtual void _OnAbilityEnter(Ability ability)
	{
	}

	protected virtual void _OnAbilityExit(Ability ability)
	{
	}

	protected virtual Color _CanSelectColor(Ability ability)
	{
		if (ability.abilityPile == Ability.Pile.Hand)
		{
			return Colors.CAN_BE_USED;
		}
		PoolKeepItemHashSetHandle<Ability> additionalAbilities = _additionalAbilities;
		if (additionalAbilities == null || additionalAbilities.value?.Contains(ability) != true)
		{
			return Colors.TARGET;
		}
		return Colors.ACTIVATE;
	}

	protected virtual IEnumerable<GameStep> _GetStepsToRunAfterSelectActions()
	{
		yield return new GameStepWait(1f);
		yield return new GameStepGenericSimple(delegate
		{
			_selectedAbilityLayout.Add(_selectedAbility.view);
			_selectedAbility.view.ClearExitTransitions();
		});
	}

	protected override LocalizedString _GetMessage()
	{
		return base.message?.Localize().SetArgumentsCloned(_selectCount);
	}

	protected override void OnFirstEnabled()
	{
		_layoutSettings = _layoutSettingsOverride;
		if (_includeAppliedAbilities)
		{
			_additionalCards = (_additionalAbilities = Pools.UseKeepItemHashSet(base.state.GetAbilitiesInTurnOrder())).value;
			_clearExitTransition = delegate(Ability ability)
			{
				PoolKeepItemHashSetHandle<Ability> additionalAbilities = _additionalAbilities;
				return additionalAbilities != null && additionalAbilities.value?.Contains(ability) == true;
			};
		}
		base.OnFirstEnabled();
		_originalCardsCopy = Pools.UseKeepItemDictionary(base.originalOrder);
		_selectCount = Math.Min(_selectCount, base.count);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ADeckLayoutBase.OnClick += _OnCardClick;
		base.state.abilityDeck.layout.onPointerClick += _OnAbilityPointerClick;
		base.state.abilityDeck.layout.onPointerEnter += _OnAbilityPointerEnter;
		base.state.abilityDeck.layout.onPointerExit += _OnAbilityPointerExit;
		foreach (Ability card in base._cards)
		{
			bool? flag = _CanSelectAbility(card);
			if (flag.HasValue)
			{
				bool valueOrDefault = flag.GetValueOrDefault();
				card.view.RequestGlow(this, valueOrDefault ? _CanSelectColor(card) : Colors.FAILURE);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ADeckLayoutBase.OnClick -= _OnCardClick;
		base.state.abilityDeck.layout.onPointerClick -= _OnAbilityPointerClick;
		base.state.abilityDeck.layout.onPointerEnter -= _OnAbilityPointerEnter;
		base.state.abilityDeck.layout.onPointerExit -= _OnAbilityPointerExit;
	}

	public override void OnCompletedSuccessfully()
	{
		if (_selectedAbility != null)
		{
			_OnAbilitySelected();
		}
	}

	protected override void OnDestroy()
	{
		Pools.Repool(ref _originalCardsCopy);
		Pools.Repool(ref _additionalAbilities);
	}
}
