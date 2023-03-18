using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat;

public class PietatAwakeningBehaviour : StateMachineBehaviour
{
	private Enemy _pietat;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_pietat == null)
		{
			_pietat = animator.GetComponent<Enemy>();
		}
		animator.speed = 0f;
	}
}
