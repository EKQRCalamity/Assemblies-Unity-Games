using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class ParryRepostBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent Penitent { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!Penitent)
		{
			Penitent = Core.Logic.Penitent;
		}
		Penitent.Status.Invulnerable = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Penitent.Status.Invulnerable = false;
	}
}
