using Gameplay.GameControllers.Enemies.Acolyte;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Hurt;

public class AcolyteOverthrowBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte _acolyte;

	private readonly int _deathAnim = Animator.StringToHash("Death");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte == null)
		{
			_acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
		}
		_acolyte.EnableEnemyLayer(enable: false);
		if (_acolyte.Status.Dead && !_acolyte.EnemyFloorChecker().IsGrounded)
		{
			animator.Play(_deathAnim);
		}
		else
		{
			_acolyte.Audio.PlayOverthrow();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte.Status.Dead)
		{
			animator.Play(_deathAnim);
		}
		if (!_acolyte.Status.IsHurt)
		{
			_acolyte.Status.IsHurt = true;
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
