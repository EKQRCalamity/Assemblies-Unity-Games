using Gameplay.GameControllers.Enemies.Acolyte;
using Gameplay.GameControllers.Enemies.Acolyte.IA;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Attack;

public class AcolyteAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte _acolyte;

	private AcolyteAttack _attack;

	[Range(0f, 1f)]
	public float desiredPlaybackTimeOnRunning = 0.4f;

	private AcolyteBehaviour _behaviour;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte == null)
		{
			_acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
		}
		if (_behaviour == null)
		{
			_behaviour = (AcolyteBehaviour)_acolyte.EnemyBehaviour;
		}
		if (_attack == null)
		{
			_attack = (AcolyteAttack)_acolyte.EnemyAttack();
		}
		if (animator.speed > 1f)
		{
			animator.speed = 1f;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime < 0.1f)
		{
			_attack.TriggerAttack = false;
		}
		_acolyte.IsAttacking = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte.RigidbodyType != RigidbodyType2D.Kinematic)
		{
			_acolyte.Rigidbody.velocity = Vector2.zero;
			_acolyte.RigidbodyType = RigidbodyType2D.Kinematic;
		}
		_behaviour.IsAttackWindowOpen = false;
		_acolyte.IsAttacking = false;
	}
}
