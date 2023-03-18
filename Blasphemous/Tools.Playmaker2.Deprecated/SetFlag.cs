using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Sets the value of a flag.")]
public class SetFlag : FsmStateAction
{
	public FsmString flagName;

	public FsmBool state;

	public bool runtimeFlag;

	public override void OnEnter()
	{
		Core.Events.SetFlag(flagName.Value, state.Value);
		Finish();
	}
}
