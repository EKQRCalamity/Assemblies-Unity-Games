using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Induces the game into dialog mode.")]
public class DialogStart : FsmStateAction
{
	[RequiredField]
	[Tooltip("The conversation to start")]
	public FsmString conversation;

	public FsmString category;

	public FsmBool modal;

	public FsmBool useOnlyLast;

	public FsmBool dontCloseWidetAtEnd;

	public FsmBool useFullScreenBackgound;

	public FsmBool remainDialogueMode;

	public FsmBool enablePlayerDialogueMode;

	public FsmFloat purge = 0f;

	public FsmEvent answer1;

	public FsmEvent answer2;

	public FsmEvent answer3;

	public FsmEvent answer4;

	public FsmEvent answer5;

	public override void Reset()
	{
		if (conversation != null)
		{
			conversation.Value = string.Empty;
		}
		if (modal == null)
		{
			modal = new FsmBool();
		}
		if (useOnlyLast == null)
		{
			useOnlyLast = new FsmBool();
		}
		if (dontCloseWidetAtEnd == null)
		{
			dontCloseWidetAtEnd = new FsmBool();
		}
		if (useFullScreenBackgound == null)
		{
			useFullScreenBackgound = new FsmBool();
		}
		if (remainDialogueMode == null)
		{
			remainDialogueMode = new FsmBool();
		}
		if (enablePlayerDialogueMode == null)
		{
			enablePlayerDialogueMode = new FsmBool();
		}
		modal.Value = true;
		dontCloseWidetAtEnd.Value = false;
		useOnlyLast.Value = false;
		useFullScreenBackgound.Value = false;
		remainDialogueMode.Value = false;
		enablePlayerDialogueMode = true;
		if (purge == null)
		{
			purge = new FsmFloat();
		}
		purge.Value = 0f;
	}

	public override void OnEnter()
	{
		string text = ((conversation == null) ? string.Empty : conversation.Value);
		bool flag = modal == null || modal.Value;
		bool flag2 = useOnlyLast != null && useOnlyLast.Value;
		bool flag3 = dontCloseWidetAtEnd != null && dontCloseWidetAtEnd.Value;
		bool useBackground = useFullScreenBackgound != null && useFullScreenBackgound.Value;
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Action Start Conversation - conversation title is blank");
		}
		else if (Core.Dialog.StartConversation(text, flag, flag2, !flag3, (int)purge.Value, useBackground))
		{
			Core.Dialog.OnDialogFinished += DialogEnded;
		}
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", enablePlayerDialogueMode.Value);
		if (!enablePlayerDialogueMode.Value)
		{
			remainDialogueMode.Value = false;
		}
	}

	public void DialogEnded(string id, int response)
	{
		Core.Dialog.OnDialogFinished -= DialogEnded;
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", dontCloseWidetAtEnd.Value || remainDialogueMode.Value);
		switch (response)
		{
		case 0:
			base.Fsm.Event(answer1);
			break;
		case 1:
			base.Fsm.Event(answer2);
			break;
		case 2:
			base.Fsm.Event(answer3);
			break;
		case 3:
			base.Fsm.Event(answer4);
			break;
		case 4:
			base.Fsm.Event(answer5);
			break;
		default:
			Finish();
			break;
		}
	}
}
