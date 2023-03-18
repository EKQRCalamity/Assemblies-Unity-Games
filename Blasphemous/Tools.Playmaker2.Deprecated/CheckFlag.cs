using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory(ActionCategory.Events)]
[Tooltip("Checks if the flag is raised.")]
public class CheckFlag : FsmStateAction
{
	public FsmString flagName;

	public FsmEvent flagAvailable;

	public FsmEvent flagUnavailable;

	public override void OnEnter()
	{
		string id = flagName.Value.ToUpper().Replace(' ', '_');
		if (Core.Events.GetFlag(id))
		{
			base.Fsm.Event(flagAvailable);
		}
		else
		{
			base.Fsm.Event(flagUnavailable);
		}
		Finish();
	}
}
