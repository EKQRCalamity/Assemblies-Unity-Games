using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Add or remove fervour.")]
public class FervourAdd : FsmStateAction
{
	public FsmFloat value;

	public override void OnEnter()
	{
		float num = Core.Logic.Penitent.Stats.Fervour.Current + value.Value;
		if (num < 0f)
		{
			num = 0f;
		}
		Core.Logic.Penitent.Stats.Fervour.Current = num;
		Finish();
	}
}
