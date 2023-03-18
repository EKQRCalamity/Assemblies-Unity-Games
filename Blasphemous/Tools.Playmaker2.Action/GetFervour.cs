using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Get")]
public class GetFervour : FsmStateAction
{
	public FsmFloat output;

	public override void Reset()
	{
		output = new FsmFloat
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		output.Value = Core.Logic.Penitent.Stats.Fervour.Current;
		Finish();
	}
}
