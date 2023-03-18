using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Uses an actionable object.")]
public class UnlockActionable : FsmStateAction
{
	public FsmGameObject target;

	public FsmBool locked;

	public override void OnEnter()
	{
		IActionable component = target.Value.GetComponent<IActionable>();
		if (component != null)
		{
			component.Locked = locked.Value;
		}
		Finish();
	}
}
