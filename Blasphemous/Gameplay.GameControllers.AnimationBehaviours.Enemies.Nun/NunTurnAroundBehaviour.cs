using Gameplay.GameControllers.Enemies.Nun;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Nun;

public class NunTurnAroundBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.Nun.Nun Nun { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Nun == null)
		{
			Nun = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Nun.Nun>();
		}
		Nun.Behaviour.TurningAround = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Nun.Behaviour.TurningAround = false;
		Nun.SetOrientation(Nun.Status.Orientation);
		Nun.Audio.StopTurnAround();
	}
}
