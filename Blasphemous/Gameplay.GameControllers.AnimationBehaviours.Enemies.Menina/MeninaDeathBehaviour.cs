using Gameplay.GameControllers.Enemies.Menina;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Menina;

public class MeninaDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Menina.Menina _menina;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_menina == null)
		{
			_menina = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Menina.Menina>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Object.Destroy(_menina.gameObject);
	}
}
