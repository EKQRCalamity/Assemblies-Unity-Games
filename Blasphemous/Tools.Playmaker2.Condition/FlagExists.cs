using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if the chosen flags exists.")]
public class FlagExists : FsmStateAction
{
	public FsmString category;

	public FsmString flagName;

	public bool runtimeFlag;

	public FsmEvent flagAvailable;

	public FsmEvent flagUnavailable;

	public FsmBool outValue;

	public override void Reset()
	{
		outValue = new FsmBool
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		string id = flagName.Value.ToUpper().Replace(' ', '_');
		bool flag = Core.Events.GetFlag(id);
		if (outValue != null)
		{
			outValue.Value = flag;
		}
		if (flag)
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
