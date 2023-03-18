using Gameplay.GameControllers.Enemies.ChimeRinger;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.ChimeRinger;

public class ChimeRingerDeathBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Enemies.ChimeRinger.ChimeRinger ChimeRinger { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (ChimeRinger == null)
		{
			ChimeRinger = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.ChimeRinger.ChimeRinger>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Object.Destroy(ChimeRinger.gameObject);
	}
}
