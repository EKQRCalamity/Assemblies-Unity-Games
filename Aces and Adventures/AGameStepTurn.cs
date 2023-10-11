using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AGameStepTurn : GameStep
{
	public static readonly Matrix4x4 OFFSET = Matrix4x4.TRS(new Vector3(0f, 0.0333f, 0f), Quaternion.AngleAxis(-15f, Vector3.right), Vector3.one);

	protected AEntity _entity;

	private bool _buttonsEnabled;

	protected IdDeck<ButtonCard.Pile, ButtonCard> buttonDeck => base.state.buttonDeck;

	protected ButtonDeckLayout buttonDeckLayout => buttonDeck.Layout<ButtonDeckLayout>();

	protected StoneDeckLayout stoneDeckLayout => base.state.stoneDeck.Layout<StoneDeckLayout>();

	protected IdDeck<AdventureCard.Pile, ATarget> adventureDeck => base.state.adventureDeck;

	protected IdDeck<Chip.Pile, Chip> chipDeck => base.state.chipDeck;

	protected virtual Stone.Pile? _enabledTurnStonePile => null;

	protected virtual Stone.Pile? _disabledTurnStonePile => null;

	public static void AddAttackTargetLine(object owner, ATarget start, ATarget end, Color color, Quaternion? startRotation = null, Quaternion? endRotation = null, float tangentScale = 1f, float endTangentScale = 1f, Vector3? endOffset = null, float endCapScale = 1f, TargetLineTags tags = (TargetLineTags)0)
	{
		TargetLineView.AddUnique(owner, color, start.view[CardTarget.Name], end.view[CardTarget.ImageCenter], startRotation ?? Quaternion.AngleAxis(45f, Vector3.right), endRotation ?? Quaternion.AngleAxis(-45f, Vector3.right), TargetLineTags.Attack | tags, tangentScale, endTangentScale, null, endOffset, endCapScale);
	}

	public static void AddEnemyAttackTargetLine(object owner, ATarget start, ATarget end, Color color, TargetLineTags tags = (TargetLineTags)0)
	{
		TargetLineView.AddUnique(owner, color, start.view[CardTarget.HP], end.view[CardTarget.ImageCenter], Quaternion.AngleAxis(-45f, Vector3.right), Quaternion.AngleAxis(45f, Vector3.right), TargetLineTags.Attack | tags);
	}

	public static bool TickTutorialTimer(ref float elapsedTime, float timer)
	{
		if (elapsedTime < timer && (elapsedTime += Time.deltaTime) >= timer)
		{
			return ProfileManager.options.game.ui.tutorialEnabled;
		}
		return false;
	}

	public static void ResetMessageTimer(ref float elapsedTime)
	{
		elapsedTime = 0f;
		GameStateView.Instance?.ClearMessage();
	}

	public AGameStepTurn(AEntity entity)
	{
		_entity = entity;
	}

	protected void _AddAttackTargetLine(object owner, ATarget start, ATarget end, Color color, Quaternion? startRotation = null, Quaternion? endRotation = null, float tangentScale = 1f, float endTangentScale = 1f, Vector3? endOffset = null, float endCapScale = 1f)
	{
		AddAttackTargetLine(owner, start, end, color, startRotation, endRotation, tangentScale, endTangentScale, endOffset, endCapScale);
	}

	protected void _RemoveAttackTargetLine(ATarget start, ATarget end)
	{
		TargetLineView.RemoveAtExtrema(start.view[CardTarget.Name], end.view[CardTarget.ImageCenter], TargetLineTags.Attack);
	}

	protected void _RemoveAttackTargetLine(ATarget end)
	{
		TargetLineView.RemoveEndingAt(end.view[CardTarget.ImageCenter], TargetLineTags.Attack);
	}

	protected void _AddEnemyAttackTargetLine(object owner, ATarget start, ATarget end, Color color, TargetLineTags tags = (TargetLineTags)0)
	{
		AddEnemyAttackTargetLine(owner, start, end, color, tags);
	}

	protected void _DisplayEffectiveEnemyOffense(ACombatant enemy)
	{
		if (enemy == null)
		{
			return;
		}
		enemy.combatantCard.offenseText.text = enemy.GetOffenseAgainst(base.state.player).ToStringPooled();
		ATextMeshProAnimator.CreateColor(enemy.combatantCard.offenseText, null, -1f, Colors.STAT_HIGHLIGHT_RED);
		ATextMeshProAnimator.CreateColor(base.state.player.combatantCard.defenseText, null, -1f, Colors.STAT_HIGHLIGHT_GREEN);
		foreach (IAnimatedUI item in enemy.combatantCard.offenseIcon.transform.parent.gameObject.GetComponentsInChildrenPooled<IAnimatedUI>())
		{
			item.AddStaggeredAnimations(new Vector3(0f, 0f, -20f), Vector3.zero, Vector3.one * 2f, 0.5f, 0.0333f, 5);
		}
		foreach (IAnimatedUI item2 in base.state.player.combatantCard.defenseIcon.transform.parent.gameObject.GetComponentsInChildrenPooled<IAnimatedUI>())
		{
			item2.AddStaggeredAnimations(new Vector3(0f, 0f, -20f), Vector3.zero, Vector3.one * 2f, 0.5f, 0.0333f, 5);
		}
	}

	protected void _StopDisplayingEffectiveEnemyOffense(ACombatant enemy)
	{
		if (enemy != null)
		{
			enemy.combatantCard.offenseText.text = enemy.stats.offense.value.ToStringPooled();
			ATextMeshProAnimator.StopAll(enemy.combatantCard.offenseText.transform);
			ATextMeshProAnimator.StopAll(base.state.player.combatantCard.defenseText.transform);
		}
	}

	protected void _ResetMessageTimer(ref float elapsedTime)
	{
		ResetMessageTimer(ref elapsedTime);
	}

	protected bool _TickTutorialTimer(ref float elapsedTime, float timer)
	{
		return TickTutorialTimer(ref elapsedTime, timer);
	}

	protected void _DoPointerOverTutorialDelay(ref float elapsedTime, ATarget target, int pile, float delay = 0.2f)
	{
		if (!(target is ResourceCard))
		{
			if (!(target is Ability))
			{
				if (target is AEntity && 3 == pile)
				{
					elapsedTime -= delay;
				}
			}
			else if ((Ability.Piles.Hand | Ability.Piles.HeroAct).Contains((Ability.Pile)pile))
			{
				elapsedTime -= delay;
			}
		}
		else if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains((ResourceCard.Pile)pile))
		{
			elapsedTime -= delay;
		}
		if (elapsedTime < 0f)
		{
			elapsedTime = 0f;
		}
	}

	protected virtual IEnumerable<ButtonCardType> _Buttons()
	{
		yield break;
	}

	protected virtual void _OnButtonOver(ButtonCard.Pile pile, ButtonCard card)
	{
	}

	protected virtual void _OnButtonExit(ButtonCard.Pile pile, ButtonCard card)
	{
	}

	protected virtual void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
	}

	protected virtual void _OnStoneClick(Stone.Pile pile, Stone card)
	{
	}

	protected virtual void _OnConfirmPressed()
	{
		if (buttonDeck.Count(ButtonCard.Pile.Active) > 0)
		{
			_OnButtonClick(ButtonCard.Pile.Active, buttonDeck.GetCards(ButtonCard.Pile.Active).First());
		}
	}

	protected virtual void _OnBackPressed()
	{
		if (buttonDeck.Count(ButtonCard.Pile.Active) > 1)
		{
			_OnButtonClick(ButtonCard.Pile.Active, buttonDeck.GetCards(ButtonCard.Pile.Active).Last());
		}
	}

	public virtual void DisplayEffectiveStats()
	{
	}

	public virtual void StopDisplayingEffectiveStats()
	{
	}

	protected override void OnEnable()
	{
		buttonDeckLayout.onPointerEnter += _OnButtonOver;
		buttonDeckLayout.onPointerExit += _OnButtonExit;
		buttonDeckLayout.onPointerClick += _OnButtonClick;
		stoneDeckLayout.onPointerClick += _OnStoneClick;
		base.view.onConfirmPressed += _OnConfirmPressed;
		base.view.onBackPressed += _OnBackPressed;
		_buttonsEnabled = true;
		foreach (ButtonCardType item in _Buttons())
		{
			buttonDeckLayout.Activate(item);
		}
		if (_enabledTurnStonePile.HasValue)
		{
			stoneDeckLayout.Transfer(StoneType.Turn, _enabledTurnStonePile.Value);
		}
		if (base.state.GetEncounterState() != EncounterState.Active)
		{
			Cancel();
		}
	}

	protected override void OnDisable()
	{
		if (_buttonsEnabled)
		{
			foreach (ButtonCardType item in _Buttons())
			{
				buttonDeckLayout.Deactivate(item);
			}
			if (_disabledTurnStonePile.HasValue)
			{
				stoneDeckLayout.Transfer(StoneType.Turn, _disabledTurnStonePile.Value);
			}
			_buttonsEnabled = false;
		}
		buttonDeckLayout.onPointerEnter -= _OnButtonOver;
		buttonDeckLayout.onPointerExit -= _OnButtonExit;
		buttonDeckLayout.onPointerClick -= _OnButtonClick;
		stoneDeckLayout.onPointerClick -= _OnStoneClick;
		base.view.onConfirmPressed -= _OnConfirmPressed;
		base.view.onBackPressed -= _OnBackPressed;
	}
}
