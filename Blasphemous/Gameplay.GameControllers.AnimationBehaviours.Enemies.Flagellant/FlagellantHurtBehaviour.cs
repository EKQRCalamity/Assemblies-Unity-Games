using Gameplay.GameControllers.Enemies.Flagellant.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant;

public class FlagellantHurtBehaviour : StateMachineBehaviour
{
	private Entity _flagellant;

	private FlagellantBehaviour _behaviour;

	private readonly int _overThrow = Animator.StringToHash("Overthrow");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Entity>();
			_behaviour = _flagellant.GetComponentInChildren<FlagellantBehaviour>();
		}
		if (!_flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = true;
		}
		if (_flagellant.Status.Dead)
		{
			animator.Play(_overThrow);
		}
		Hit lastHit = _flagellant.EntityDamageArea.LastHit;
		if (lastHit.DamageType == DamageArea.DamageType.Heavy)
		{
			_flagellant.HitDisplacement(lastHit.AttackingEntity.transform.position, lastHit.DamageType);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_behaviour.StopMovement();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_flagellant == null) && _flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = !_flagellant.Status.IsHurt;
		}
	}
}
