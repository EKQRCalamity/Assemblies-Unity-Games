using System.Collections;
using TMPro;
using UnityEngine;

public class GameStepLevelUpPourRebirthConfirm : GameStep
{
	protected override IEnumerator Update()
	{
		if (ProfileManager.progress.experience.read.HasRebirthed())
		{
			TransitionTo(new GameStepLevelUpPourRebirth());
			yield break;
		}
		base.view.screenCanvasGraphicRaycaster.enabled = true;
		GameObject popup = UIUtil.CreatePopup(MessageData.UIPopupTitle.ConfirmRebirth.GetTitle().Localize(), UIUtil.CreateMessageBox(MessageData.UIPopupMessage.ConfirmRebirth.GetMessage().Localize(), TextAlignmentOptions.MidlineLeft, 32, 712, 512, 16f), null, buttons: new string[2]
		{
			MessageData.UIPopupButton.Cancel.GetButton().Localize(),
			MessageData.UIPopupButton.ConfirmRebirth.GetButton().Localize()
		}, parent: base.view.screenCanvasContainer, size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
		{
			base.view.screenCanvasGraphicRaycaster.enabled = false;
		}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == MessageData.UIPopupButton.ConfirmRebirth.GetButton().Localize())
			{
				TransitionTo(new GameStepLevelUpPourRebirth());
			}
		});
		while ((bool)popup)
		{
			yield return null;
		}
		if (!base.finished)
		{
			base.state.levelUp.main.TransferPile(LevelUpPile.VialPour, LevelUpPile.Vial);
		}
	}
}
