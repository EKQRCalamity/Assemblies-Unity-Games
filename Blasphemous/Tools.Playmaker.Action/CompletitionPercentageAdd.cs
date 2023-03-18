using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
public class CompletitionPercentageAdd : FsmStateAction
{
	public PersistentManager.PercentageType percentageType;

	public float CustomValue;

	public override void Reset()
	{
		percentageType = PersistentManager.PercentageType.BossDefeated_1;
		CustomValue = 0f;
	}

	public override void OnEnter()
	{
		Finish();
	}
}
