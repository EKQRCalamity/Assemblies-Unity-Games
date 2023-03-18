using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Uses an actionable object.")]
public class ActionableActivation : FsmStateAction
{
	public FsmGameObject target;

	public override void OnEnter()
	{
		if ((bool)target.Value)
		{
			target.Value.GetComponent<IActionable>()?.Use();
		}
		Finish();
	}
}
