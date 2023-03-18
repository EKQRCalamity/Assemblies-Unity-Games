using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if the chosen DLCs is present.")]
public class IsDLCDownloaded : FsmStateAction
{
	[RequiredField]
	public FsmString dlcId;

	public FsmBool checkAgain;

	public FsmEvent dlcAvailable;

	public FsmEvent dlcUnavailable;

	public override void Reset()
	{
		if (dlcId == null)
		{
			dlcId = new FsmString();
		}
		if (checkAgain == null)
		{
			checkAgain = new FsmBool();
		}
		dlcId.Value = string.Empty;
		checkAgain.Value = false;
	}

	public override void OnEnter()
	{
		string text = ((dlcId == null) ? string.Empty : dlcId.Value);
		bool recheck = checkAgain != null && checkAgain.Value;
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Action IsDLCDownloaded - dlcId is blank");
		}
		if (Core.DLCManager.IsDLCDownloaded(text, recheck))
		{
			base.Fsm.Event(dlcAvailable);
		}
		else
		{
			base.Fsm.Event(dlcUnavailable);
		}
		Finish();
	}
}
