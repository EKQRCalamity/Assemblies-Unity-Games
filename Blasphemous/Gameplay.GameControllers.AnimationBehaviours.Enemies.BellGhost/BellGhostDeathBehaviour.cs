using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.BellGhost;

public class BellGhostDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.BellGhost.BellGhost _bellGhost;

	private bool _destroy;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_bellGhost == null)
		{
			_bellGhost = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.BellGhost.BellGhost>();
		}
		_destroy = false;
		if (_bellGhost.AttackArea != null)
		{
			_bellGhost.AttackArea.WeaponCollider.enabled = false;
		}
		_bellGhost.EntityDamageArea.DamageAreaCollider.enabled = false;
		if (_bellGhost.MotionLerper.IsLerping)
		{
			_bellGhost.MotionLerper.StopLerping();
		}
		_bellGhost.Audio.StopFloating();
		_bellGhost.Audio.StopChargeAttack();
		_bellGhost.Audio.StopAttack();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.95f && !_destroy)
		{
			_destroy = true;
			Object.Destroy(_bellGhost.gameObject);
		}
	}
}
