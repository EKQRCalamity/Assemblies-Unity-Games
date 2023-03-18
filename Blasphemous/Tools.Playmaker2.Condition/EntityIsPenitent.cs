using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
public class EntityIsPenitent : FsmStateAction
{
	public FsmGameObject entity;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		base.Fsm.Event((!entity.Value.CompareTag("Penitent")) ? onFailure : onSuccess);
		Finish();
	}
}
