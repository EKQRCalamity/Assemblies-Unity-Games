using Gameplay.GameControllers.Enemies.Flagellant;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant;

public class FlagellantOverthrowBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Flagellant.Flagellant _flagellant;

	private readonly int _death = Animator.StringToHash("Death");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Flagellant.Flagellant>();
		}
		if (_flagellant.Status.Dead)
		{
			animator.Play(_death);
		}
		if (!_flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = true;
		}
		if (!_flagellant.Status.Dead)
		{
			_flagellant.Audio.PlayOverThrow();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.95f && _flagellant.Stats.Life.Current <= 0f)
		{
			animator.speed = 0f;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant.RigidBody.bodyType != RigidbodyType2D.Kinematic)
		{
			_flagellant.RigidBody.velocity = Vector3.zero;
			_flagellant.RigidBody.bodyType = RigidbodyType2D.Kinematic;
		}
		if (_flagellant.Status.IsHurt)
		{
			_flagellant.Status.IsHurt = !_flagellant.Status.IsHurt;
		}
	}
}
