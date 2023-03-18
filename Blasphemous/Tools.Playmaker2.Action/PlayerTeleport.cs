using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.PlayMaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Instantly teleports the player to the specific GO position.")]
public class PlayerTeleport : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault teleportTarget;

	public override void OnEnter()
	{
		GameObject gameObject = Core.Logic.Penitent.gameObject;
		gameObject.SetActive(value: false);
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(teleportTarget);
		gameObject.transform.position = ownerDefaultTarget.transform.position;
		gameObject.SetActive(value: true);
		Finish();
	}
}
