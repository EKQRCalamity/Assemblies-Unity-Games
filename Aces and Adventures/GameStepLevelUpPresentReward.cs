using System;
using TMPro;

public class GameStepLevelUpPresentReward : AGameStepLevelUp
{
	protected override void _OnPointerEnter(LevelUpPile pile, ATarget card)
	{
		if (pile == LevelUpPile.PresentLevelUp)
		{
			card.view.RequestGlow(this, Colors.TARGET);
			card.view.ShowTooltips();
		}
	}

	protected override void _OnPointerExit(LevelUpPile pile, ATarget card)
	{
		card.view.ReleaseGlow(this);
		card.view.HideTooltips();
	}

	protected override void _OnPointerClick(LevelUpPile pile, ATarget card)
	{
		if (pile == LevelUpPile.PresentLevelUp)
		{
			_OnConfirmPressed();
		}
	}

	protected override void _OnConfirmPressed()
	{
		base.finished = true;
	}

	protected override void _OnBackPressed()
	{
		_OnConfirmPressed();
	}

	protected override void OnFirstEnabled()
	{
		base.levelUp.writeExperience.LevelUp(base.levelUp.selectedCharacter);
		base.levelUpView.levelAmount = base.levelUp.readExperience.pendingLevelUps;
		if (base.levelUp.readExperience.GetRebirth(base.levelUp.selectedCharacter) > 0)
		{
			base.finished = true;
			if (base.levelUp.readExperience.GetLevelWithRebirth(base.levelUp.selectedCharacter) == 30 && base.levelUp.readExperience.IsHighestLevel(base.levelUp.selectedCharacter))
			{
				base.view.screenCanvasGraphicRaycaster.enabled = true;
				UIUtil.CreatePopup(MessageData.UIPopupTitle.ConfirmRebirth.GetTitle().Localize(), UIUtil.CreateMessageBox(MessageData.UIPopupMessage.NewRebirthOptions.GetMessage().Localize(), TextAlignmentOptions.Center, 32, 600, 300, 16f), null, parent: base.view.screenCanvasContainer, size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
				{
					base.view.screenCanvasGraphicRaycaster.enabled = false;
				}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: null, rayCastBlockerColor: null, delayClose: null, referenceResolution: null, buttons: Array.Empty<string>());
			}
		}
		if (!base.continueLeveling && !base.finished)
		{
			base.levelUpView.SetCloseupFocalDistance();
		}
	}

	public override void Start()
	{
		base.levelUp.main.Transfer(base.levelUp.main.Add(base.levelUp.selectedCharacter.data.GetLevelUp(base.levelUp.readExperience.GetLevel(base.levelUp.selectedCharacter)).data.GenerateCard(base.levelUp.selectedClass), LevelUpPile.LeafExit), LevelUpPile.PresentLevelUp);
		if (base.continueLeveling)
		{
			_OnConfirmPressed();
		}
	}

	protected override void OnFinish()
	{
		base.levelUp.main.TransferPile(LevelUpPile.PresentLevelUp, LevelUpPile.LevelUps);
	}

	protected override void End()
	{
		base.levelUp.main.Transfer(base.levelUp.main.Add(new ExperienceVial(base.levelUp.readExperience.currentVialXP), LevelUpPile.VialDraw), LevelUpPile.Vial);
		if (base.continueLeveling && ProfileManager.progress.experience.read.CanLevelUp(base.levelUp.selectedCharacter))
		{
			AppendStep(new GameStepLevelUpPreparePour());
		}
	}

	public override void OnCompletedSuccessfully()
	{
		base.levelUpView.ResetFocalDistance();
	}
}
