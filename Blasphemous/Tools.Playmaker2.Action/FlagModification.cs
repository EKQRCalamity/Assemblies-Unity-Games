using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Modifies the value of a flag.")]
public class FlagModification : FsmStateAction
{
	public FsmString category;

	public FsmString flagName;

	public FsmBool state;

	public bool runtimeFlag;

	public override void OnEnter()
	{
		Core.Events.SetFlag(flagName.Value, state.Value);
		Finish();
	}
}
