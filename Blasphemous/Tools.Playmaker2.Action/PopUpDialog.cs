using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Show a PopUp.")]
public class PopUpDialog : FsmStateAction
{
	[Tooltip("String ID with message")]
	public FsmString textId;

	public FsmBool blockplayer;

	public FsmFloat timeToWait;

	public FsmString soundId;

	private bool IsModal;

	public override void OnEnter()
	{
		string message = Core.Localization.Get(textId.Value);
		IsModal = blockplayer.Value;
		if (IsModal)
		{
			PopUpWidget.OnDialogClose += DialogClose;
		}
		UIController.instance.ShowPopUp(message, soundId.Value, timeToWait.Value, blockplayer.Value);
		if (!IsModal)
		{
			Finish();
		}
	}

	public override void OnExit()
	{
		if (!IsModal)
		{
			PopUpWidget.OnDialogClose -= DialogClose;
		}
	}

	private void DialogClose()
	{
		Finish();
	}
}
