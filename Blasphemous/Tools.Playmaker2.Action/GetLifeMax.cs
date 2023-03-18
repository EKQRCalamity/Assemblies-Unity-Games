using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Get")]
public class GetLifeMax : FsmStateAction
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
		output.Value = Core.Logic.Penitent.Stats.Life.Base + Core.Logic.Penitent.Stats.Life.Bonus;
		Finish();
	}
}
