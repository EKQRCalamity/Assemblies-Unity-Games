using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Gets the Parent of a Game Object.")]
public class GetParentMore : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[UIHint(UIHint.Variable)]
	public FsmGameObject storeResult;

	public FsmInt repetitions;

	private int repeat;

	public override void Reset()
	{
		gameObject = null;
		storeResult = null;
		repetitions = 0;
		repeat = 0;
	}

	public override void OnEnter()
	{
		repeat = repetitions.Value;
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (ownerDefaultTarget != null)
		{
			storeResult.Value = ((!(ownerDefaultTarget.transform.parent == null)) ? ownerDefaultTarget.transform.parent.gameObject : null);
			while (repeat > 0)
			{
				GameObject value = storeResult.Value;
				repeat--;
				storeResult.Value = ((!(value.transform.parent == null)) ? value.transform.parent.gameObject : null);
			}
		}
		else
		{
			storeResult.Value = null;
		}
		Finish();
	}
}
