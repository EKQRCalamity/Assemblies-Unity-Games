using Gameplay.GameControllers.Enemies.Swimmer;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.UndergroundSwimmer;

public class UndergroundSwimmerJumpBehaviour : StateMachineBehaviour
{
	private bool _fireProjectile;

	public Swimmer Swimmer { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Swimmer == null)
		{
			Swimmer = animator.GetComponentInParent<Swimmer>();
		}
		_fireProjectile = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!_fireProjectile && stateInfo.normalizedTime > 0.1f)
		{
			_fireProjectile = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
}
