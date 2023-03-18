using Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.BejeweledBoss;

public class DivineBeamFiringBehaviour : StateMachineBehaviour
{
	private BejeweledDivineBeam _divineBeam;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_divineBeam == null)
		{
			_divineBeam = animator.GetComponent<BejeweledDivineBeam>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_divineBeam.Dispose();
	}
}
