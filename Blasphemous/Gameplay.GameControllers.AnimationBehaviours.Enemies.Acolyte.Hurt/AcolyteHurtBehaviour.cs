using Gameplay.GameControllers.Enemies.Acolyte;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Hurt;

public class AcolyteHurtBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte _acolyte;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte == null)
		{
			_acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
		}
		Hit lastHit = _acolyte.EntityDamageArea.LastHit;
		if (lastHit.DamageType == DamageArea.DamageType.Heavy && !_acolyte.EnemyFloorChecker().IsSideBlocked)
		{
			_acolyte.HitDisplacement(lastHit.AttackingEntity.transform.position, lastHit.DamageType);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_acolyte.Status.IsHurt)
		{
			_acolyte.Status.IsHurt = true;
		}
		if (_acolyte.EntityAttack.IsWeaponBlowingUp)
		{
			_acolyte.EntityAttack.IsWeaponBlowingUp = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte.Status.IsHurt)
		{
			_acolyte.Status.IsHurt = !_acolyte.Status.IsHurt;
		}
	}
}
