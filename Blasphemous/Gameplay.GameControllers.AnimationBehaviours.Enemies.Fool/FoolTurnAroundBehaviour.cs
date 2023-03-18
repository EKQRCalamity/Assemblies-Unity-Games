using Gameplay.GameControllers.Enemies.Fool;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Fool;

public class FoolTurnAroundBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Fool.Fool Fool { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Fool == null)
		{
			Fool = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Fool.Fool>();
		}
		Fool.Behaviour.TurningAround = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Fool.SetOrientation(Fool.Status.Orientation);
		Fool.Behaviour.TurningAround = false;
	}
}
