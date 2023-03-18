using Framework.Managers;
using Gameplay.UI.Others.MenuLogic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Show a Message")]
public class ShowMessage : FsmStateAction
{
	public FsmString category;

	public FsmString textId;

	public FsmInt line = 0;

	public FsmBool blockplayer;

	public FsmFloat timeToWait;

	public FsmString soundId;

	private bool IsModal;

	private float timeLeft;

	public override void OnEnter()
	{
		string text = ((textId == null) ? string.Empty : textId.Value);
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Action Show Message - textId title is blank");
			Finish();
			return;
		}
		bool blockPlayer = blockplayer != null && blockplayer.Value;
		int num = ((line != null) ? line.Value : 0);
		float num2 = ((timeToWait == null) ? 0f : timeToWait.Value);
		string eventSound = ((soundId == null) ? string.Empty : soundId.Value);
		if (IsModal)
		{
			PopUpWidget.OnDialogClose += DialogClose;
		}
		bool flag = Core.Dialog.ShowMessage(text, num, eventSound, num2, blockPlayer);
		if (num2 <= 0f && (!flag || !IsModal))
		{
			Finish();
		}
		else
		{
			timeLeft = num2;
		}
	}

	public override void OnUpdate()
	{
		timeLeft -= Time.deltaTime;
		if (timeLeft < 0f)
		{
			timeLeft = 0f;
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
