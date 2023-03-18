using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class ReleaseTopLadderBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.Physics.EnablePhysics(enable: false);
		animator.speed = 1f;
		_penitent.DamageArea.EnableEnemyAttack(enable: false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.Physics.EnablePhysics();
		Vector2 vector = new Vector2(_penitent.transform.position.x, _penitent.transform.position.y - _penitent.PlatformCharacterController.GroundDist);
		_penitent.transform.position = vector;
		_penitent.IsGrabbingLadder = false;
		_penitent.DamageArea.EnableEnemyAttack();
	}
}
