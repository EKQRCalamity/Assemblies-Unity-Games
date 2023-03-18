using Gameplay.GameControllers.Enemies.Firethrower;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Firethrower;

public class FirethrowerTurnAroundBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Firethrower.Firethrower Firethrower { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Firethrower == null)
		{
			Firethrower = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Firethrower.Firethrower>();
		}
		Firethrower.Behaviour.TurningAround = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Firethrower.Behaviour.TurningAround = false;
		Firethrower.SetOrientation(Firethrower.Status.Orientation);
	}
}
