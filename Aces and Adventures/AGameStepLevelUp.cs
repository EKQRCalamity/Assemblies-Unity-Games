using System;
using System.Collections;
using UnityEngine;

public abstract class AGameStepLevelUp : GameStep
{
	public LevelUpState levelUp => base.state.levelUp;

	public LevelUpPlant plant => base.state.levelUp.main.NextInPile(LevelUpPile.Pot) as LevelUpPlant;

	public ExperienceVial vial => (base.state.levelUp.main.NextInPile(LevelUpPile.VialPour) as ExperienceVial) ?? (base.state.levelUp.main.NextInPile(LevelUpPile.Vial) as ExperienceVial);

	public LevelUpStateView levelUpView => base.state.view.levelUp;

	public bool quickLevelUp => ProfileManager.options.game.preferences.quickLevelUp;

	public bool continueLeveling
	{
		get
		{
			if (quickLevelUp)
			{
				if (!InputManager.I[KeyCode.Mouse0].isDown)
				{
					return InputManager.I[KeyAction.Finish][KState.Down];
				}
				return true;
			}
			return false;
		}
	}

	protected virtual bool _backColliderEnabled => true;

	protected virtual bool _cancelStoneEnabled => false;

	protected virtual void _OnBackPressed()
	{
	}

	protected virtual void _OnConfirmPressed()
	{
	}

	protected virtual void _OnPointerClick(LevelUpPile pile, ATarget card)
	{
	}

	protected virtual void _OnPointerEnter(LevelUpPile pile, ATarget card)
	{
	}

	protected virtual void _OnPointerExit(LevelUpPile pile, ATarget card)
	{
	}

	protected virtual void _OnVialStandClick()
	{
	}

	protected virtual void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && card.type == ButtonCardType.Back)
		{
			_OnBackPressed();
		}
	}

	protected void _OnPointerEnterClassSeal(LevelUpPile pile, ATarget card)
	{
		if (card is ClassSeal classSeal)
		{
			ProjectedTooltipFitter.Create($"<line-height={((levelUp.readExperience.GetLevelWithRebirth(classSeal.character) > 0) ? 60 : 80)}%>" + ((RebirthLevel)levelUp.readExperience.GetRebirth(classSeal.character)).GetMessage().SizeIfNotEmpty().NoBreakIfNotEmpty()
				.NewLineIfNotEmpty() + "</line-height>" + ((LeafLevel)levelUp.readExperience.GetLevelWithRebirth(classSeal.character)).GetMessage().SizeIfNotEmpty().NoBreakIfNotEmpty()
				.LineHeightIfNotEmpty(80)
				.NewLineIfNotEmpty() + classSeal.characterClass.GetText(), classSeal.view.gameObject, base.view.tooltipCanvas, (pile != LevelUpPile.ActiveSeal) ? TooltipAlignment.TopCenter : TooltipAlignment.BottomCenter, 0);
		}
	}

	protected void _OnPointerExitClassSeal(LevelUpPile pile, ATarget card)
	{
		if (card is ClassSeal classSeal)
		{
			ProjectedTooltipFitter.Finish(classSeal.view.gameObject);
		}
	}

	protected override void OnEnable()
	{
		base.view.onBackPressed += _OnBackPressed;
		base.view.onConfirmPressed += _OnConfirmPressed;
		levelUpView.main.onPointerClick += _OnPointerClick;
		levelUpView.main.onPointerEnter += _OnPointerEnter;
		levelUpView.main.onPointerExit += _OnPointerExit;
		LevelUpStateView levelUpStateView = levelUpView;
		levelUpStateView.onBackColliderClick = (Action)Delegate.Combine(levelUpStateView.onBackColliderClick, new Action(_OnBackPressed));
		levelUpView.backColliderEnabled = _backColliderEnabled;
		LevelUpStateView levelUpStateView2 = levelUpView;
		levelUpStateView2.onVialStandClick = (Action)Delegate.Combine(levelUpStateView2.onVialStandClick, new Action(_OnVialStandClick));
		base.state.buttonDeck.layout.onPointerClick += _OnButtonClick;
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, _cancelStoneEnabled, forceUpdateCancelStone: true);
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
		if (quickLevelUp && InputManager.I[KeyAction.Finish][KState.JustHeld])
		{
			_OnConfirmPressed();
		}
	}

	protected override void OnDisable()
	{
		base.view.onBackPressed -= _OnBackPressed;
		base.view.onConfirmPressed -= _OnConfirmPressed;
		levelUpView.main.onPointerClick -= _OnPointerClick;
		levelUpView.main.onPointerEnter -= _OnPointerEnter;
		levelUpView.main.onPointerExit -= _OnPointerExit;
		LevelUpStateView levelUpStateView = levelUpView;
		levelUpStateView.onBackColliderClick = (Action)Delegate.Remove(levelUpStateView.onBackColliderClick, new Action(_OnBackPressed));
		levelUpView.backColliderEnabled = false;
		LevelUpStateView levelUpStateView2 = levelUpView;
		levelUpStateView2.onVialStandClick = (Action)Delegate.Remove(levelUpStateView2.onVialStandClick, new Action(_OnVialStandClick));
		base.state.buttonDeck.layout.onPointerClick -= _OnButtonClick;
	}
}
