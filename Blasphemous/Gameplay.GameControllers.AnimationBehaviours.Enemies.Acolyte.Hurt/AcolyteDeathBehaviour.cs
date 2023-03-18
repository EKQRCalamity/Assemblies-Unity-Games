using Gameplay.GameControllers.Enemies.Acolyte;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Acolyte.Hurt;

public class AcolyteDeathBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Acolyte.Acolyte _acolyte;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_acolyte == null)
		{
			_acolyte = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Acolyte.Acolyte>();
		}
		animator.speed = 1f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.95f)
		{
			EntityShadow componentInChildren = _acolyte.GetComponentInChildren<EntityShadow>();
			if (componentInChildren != null)
			{
				componentInChildren.RemoveBlobShadow();
			}
			Object.Destroy(_acolyte.gameObject);
		}
	}
}
