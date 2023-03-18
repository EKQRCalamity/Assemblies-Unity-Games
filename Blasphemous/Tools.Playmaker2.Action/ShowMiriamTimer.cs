using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows Miriam Timer and sets the Target Time.")]
public class ShowMiriamTimer : FsmStateAction
{
	public FsmBool SetTargetTime;

	public FsmFloat TargetTime;

	public override void OnEnter()
	{
		if (SetTargetTime != null && SetTargetTime.Value)
		{
			UIController.instance.SetMiriamTimerTargetTime(TargetTime.Value);
		}
		UIController.instance.ShowMiriamTimer();
		Finish();
	}
}
