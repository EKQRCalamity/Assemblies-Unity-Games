using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if tip is unlocked.")]
public class IsTipUnlocked : FsmStateAction
{
	public FsmString popupId;

	public FsmEvent popupLocked;

	public FsmEvent popupUnlocked;

	public override void Reset()
	{
		if (popupId == null)
		{
			popupId = new FsmString();
			popupId.UseVariable = false;
		}
		popupId.Value = string.Empty;
	}

	public override void OnEnter()
	{
		string text = ((popupId == null) ? string.Empty : popupId.Value);
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Action IsTipUnlocked - popupId is blank");
		}
		else if (Core.TutorialManager.IsTutorialUnlocked(text))
		{
			base.Fsm.Event(popupUnlocked);
		}
		else
		{
			base.Fsm.Event(popupLocked);
		}
		Finish();
	}
}
