using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepActionTargetSelect : GameStepActionTarget
{
	private const float TUTORIAL_TIME = 5f;

	private const float TANGENT_SCALE = 0.2f;

	private static readonly Quaternion START_ROTATION = Quaternion.AngleAxis(90f, Vector3.forward);

	private static readonly Quaternion END_ROTATION = Quaternion.AngleAxis(45f, Vector3.right);

	private int _count;

	private PoolKeepItemHashSetHandle<ATarget> _validTargets;

	private PoolKeepItemListHandle<ATarget> _selectedTargets;

	private float _elapsedTime;

	private Transform _startTargetLineTransform;

	protected ButtonDeckLayout buttonDeckLayout => base.state.buttonDeck.Layout<ButtonDeckLayout>();

	private Transform _start => _startTargetLineTransform ?? (_startTargetLineTransform = (base.context.ability ?? base.state.adventureDeck.GetCards(AdventureCard.Pile.SelectionHand).First()).view[CardTarget.Cost]);

	public override bool canSafelyCancelStack => true;

	public override bool countsTowardsStrategyTime => true;

	public GameStepActionTargetSelect(AAction action, ActionContext context, AAction.Target targeting)
		: base(action, context, targeting)
	{
	}

	private Action<TargetLineView> _UpdateTargetLine(ATarget card)
	{
		return delegate(TargetLineView targetLine)
		{
			Transform transform = _End(card);
			float t = Mathf.Pow(Mathf.Clamp01(MathUtil.Remap(Camera.main.WorldToViewportPoint(transform.position).y, new Vector2(0.666f, 1f), new Vector2(0f, 1f))), 0.25f);
			Vector3 vector = _start.position - transform.position;
			targetLine.targetEndRotation = ((Vector3.Dot(vector, transform.right) > 0f) ? Quaternion.Slerp(END_ROTATION, Quaternion.LookRotation(transform.forward, vector), t) : END_ROTATION);
			targetLine.targetEndTangentScale = Mathf.Lerp(1f, 0.8f, t);
		};
	}

	private Transform _End(ATarget card)
	{
		return card.view[CardTarget.Name];
	}

	private void _OnCardOver(ADeckLayoutBase deckLayout, int pile, ATarget card)
	{
		if ((bool)buttonDeckLayout[ButtonCardType.CancelAbility])
		{
			if (card is ResourceCard resourceCard && resourceCard.pile == ResourceCard.Pile.ActivationHand && base.state.player.resourceDeck.layout == deckLayout)
			{
				InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorBack);
			}
			else if (card is Ability ability && ability.abilityPile == Ability.Pile.ActivationHand && base.state.player.abilityDeck.layout == deckLayout)
			{
				InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorBack);
			}
		}
		if (!_validTargets.Contains(card))
		{
			return;
		}
		if (base.targeting.allowRepeats)
		{
			int num = _selectedTargets.value.Count((ATarget t) => t == card);
			TargetLineView.AddUnique(card, Colors.TARGET, _start, _End(card), START_ROTATION, END_ROTATION, (TargetLineTags)(1 << num), 1f + (float)num * 0.2f, 1f, _UpdateTargetLine(card));
			base.context.ability?.ShowPotentialDamage(base.context, _selectedTargets.value.Concat(card));
		}
		else
		{
			bool flag = _selectedTargets.value.Contains(card);
			TargetLineView.AddUnique(card, flag ? Colors.ATTACK : Colors.TARGET, _start, _End(card), START_ROTATION, END_ROTATION, TargetLineTags.Target, 1f, 1f, _UpdateTargetLine(card));
			base.context.ability?.ShowPotentialDamage(base.context, flag ? _selectedTargets.value.Where((ATarget t) => t != card) : _selectedTargets.value.Concat(card));
		}
	}

	private void _OnCardExit(ADeckLayoutBase deckLayout, int pile, ATarget card)
	{
		InputManager.I.ReleaseCursorOverride(this);
		if (!_validTargets.Contains(card))
		{
			return;
		}
		if (base.targeting.allowRepeats)
		{
			TargetLineView.RemoveOwnedBy(card, (TargetLineTags)(1 << _selectedTargets.value.Count((ATarget t) => t == card)));
			if (!_selectedTargets.value.Contains(card))
			{
				base.context.ability?.HidePotentialDamage();
			}
		}
		else if (_selectedTargets.value.Contains(card))
		{
			TargetLineView.AddUnique(card, Colors.TARGET_SELECTED, _start, _End(card), START_ROTATION, END_ROTATION, TargetLineTags.Target, 1f, 1f, _UpdateTargetLine(card));
		}
		else
		{
			TargetLineView.RemoveOwnedBy(card, TargetLineTags.Target);
		}
		base.context.ability?.ShowPotentialDamage(base.context, _selectedTargets.value);
	}

	private void _OnCardClicked(ADeckLayoutBase deckLayout, int pile, ATarget card)
	{
		if (_validTargets.Contains(card))
		{
			if (!base.targeting.allowRepeats)
			{
				if (_selectedTargets.value.Toggle(card))
				{
					TargetLineView.AddUnique(card, Colors.TARGET_SELECTED, _start, _End(card), START_ROTATION, END_ROTATION, TargetLineTags.Target, 1f, 1f, _UpdateTargetLine(card));
				}
				else
				{
					TargetLineView.RemoveOwnedBy(card, TargetLineTags.Target);
				}
			}
			else
			{
				_selectedTargets.Add(card);
			}
			_OnSelectedTargetsChange();
		}
		else if (card is ResourceCard)
		{
			if (pile == 2 && base.state.player.resourceDeck.layout == deckLayout && (bool)buttonDeckLayout[ButtonCardType.CancelAbility])
			{
				CancelGroup(GroupType.Context);
			}
		}
		else if (card is Ability)
		{
			if (pile == 2 && base.state.player.abilityDeck.layout == deckLayout && (bool)buttonDeckLayout[ButtonCardType.CancelAbility])
			{
				CancelGroup(GroupType.Context);
			}
		}
		else if (card is Stone && pile == 1)
		{
			base.view.LogError(EndTurnPreventedBy.UsingAbility.Localize(), base.state.player.audio.character.error[EndTurnPreventedBy.UsingAbility]);
		}
		else
		{
			base.targeting.OnInvalidTargetClicked(card);
		}
	}

	private void _OnSelectedTargetsChange()
	{
		if (_selectedTargets.Count == _count || (!base.targeting.allowRepeats && _selectedTargets.Count == _validTargets.Count) || _validTargets.Count == 0)
		{
			_SetTargets(_selectedTargets.value);
			return;
		}
		buttonDeckLayout.SetActive(ButtonCardType.ClearTargets, _count > 1 && base.targeting.allowRepeats && _selectedTargets.Count > 0);
		if (base.targeting.allowRepeats)
		{
			foreach (ATarget target in _selectedTargets.value)
			{
				int num = _selectedTargets.value.Count((ATarget t) => t == target) - 1;
				TargetLineView.AddUnique(this, Colors.TARGET_SELECTED, _start, _End(target), START_ROTATION, END_ROTATION, (TargetLineTags)(1 << num), 1f + (float)num * 0.2f, 1f, _UpdateTargetLine(target));
			}
		}
		foreach (ATarget item in _validTargets.value)
		{
			item.view.RequestGlow(this, _selectedTargets.value.Contains(item) ? Colors.SELECTED : Colors.TARGET);
		}
		base.context.ability?.ShowPotentialDamage(base.context, _selectedTargets.value);
		if (base.targeting.allowRepeats)
		{
			base.view.RefreshPointerOver();
		}
	}

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active)
		{
			switch (card.type)
			{
			case ButtonCardType.ClearTargets:
				_selectedTargets.value.Clear();
				TargetLineView.RemoveAll(TargetLineTags.Target | TargetLineTags.Target2 | TargetLineTags.Target3 | TargetLineTags.Target4 | TargetLineTags.Target5 | TargetLineTags.Target6 | TargetLineTags.Target7 | TargetLineTags.Target8 | TargetLineTags.Target9 | TargetLineTags.Target10);
				_OnSelectedTargetsChange();
				break;
			case ButtonCardType.CancelAbility:
				CancelGroup(GroupType.Context);
				break;
			}
		}
	}

	private void _OnBackPressed()
	{
		if (base.state.buttonDeck.Count(ButtonCard.Pile.Active) > 0)
		{
			_OnButtonClick(ButtonCard.Pile.Active, base.state.buttonDeck.GetCards(ButtonCard.Pile.Active).Last());
		}
	}

	private bool _CanBeCanceled()
	{
		if (base.context.hasAbility)
		{
			if (!base.context.ability.data.checkAllActionsForInitialTargeting)
			{
				return GetPreviousSteps().OfType<GameStepActionTarget>().None();
			}
			return GetPreviousSteps().OfType<GameStepActionTarget>().None((GameStepActionTarget t) => t.hasTargets);
		}
		return false;
	}

	public override void Start()
	{
		Ability ability = base.context.ability;
		if (ability != null && ability.isTrait)
		{
			_SetTargets(base.targeting.GetTargetable(base.context, base.action).Take(base.targeting.count.GetValue(base.context)));
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_count = base.targeting.count.GetValue(base.context);
		_validTargets = Pools.UseKeepItemHashSet(base.targeting.GetTargetable(base.context, base.action));
		_selectedTargets = Pools.UseKeepItemList<ATarget>();
		ADeckLayoutBase.OnPointerEnter += _OnCardOver;
		ADeckLayoutBase.OnPointerExit += _OnCardExit;
		ADeckLayoutBase.OnClick += _OnCardClicked;
		_OnSelectedTargetsChange();
		buttonDeckLayout.onPointerClick += _OnButtonClick;
		base.view.onBackPressed += _OnBackPressed;
		buttonDeckLayout.SetActive(ButtonCardType.CancelAbility, _CanBeCanceled());
		if (ProfileManager.options.game.preferences.autoSelectSingleTarget.AutoSelect(base.context.ability) && _validTargets.Count == 1)
		{
			AAction.Target target = base.targeting;
			IEnumerable<ATarget> targetsToSet;
			if (target == null || !target.allowRepeats)
			{
				IEnumerable<ATarget> value = _validTargets.value;
				targetsToSet = value;
			}
			else
			{
				targetsToSet = Enumerable.Repeat(_validTargets.value.First(), _count);
			}
			_SetTargets(targetsToSet);
		}
		_elapsedTime = 0f;
	}

	protected override IEnumerator Update()
	{
		while (!base.targetsHaveBeenSelected)
		{
			yield return null;
		}
	}

	protected override void LateUpdate()
	{
		if (AGameStepTurn.TickTutorialTimer(ref _elapsedTime, 5f))
		{
			base.view.LogMessage(MessageData.Instance.target.combined.SetVariables(("Count", _count), ("Choice", base.targeting.selectableTargetType.Localize())));
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Pools.Repool(ref _validTargets);
		Pools.Repool(ref _selectedTargets);
		ADeckLayoutBase.OnPointerEnter -= _OnCardOver;
		ADeckLayoutBase.OnPointerExit -= _OnCardExit;
		ADeckLayoutBase.OnClick -= _OnCardClicked;
		buttonDeckLayout.onPointerClick -= _OnButtonClick;
		base.view.onBackPressed -= _OnBackPressed;
		buttonDeckLayout.Deactivate(ButtonCardType.CancelAbility);
		buttonDeckLayout.Deactivate(ButtonCardType.ClearTargets);
		base.view.ClearMessage();
		base.context.ability?.HidePotentialDamage();
	}
}
