using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Get")]
public class GetMiriamClosedPortals : FsmStateAction
{
	public FsmInt output;

	public override void Reset()
	{
		output = new FsmInt
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		output.Value = Core.Events.GetMiriamClosedPortals().Count;
		Finish();
	}
}
