using Gameplay.GameControllers.Entities.Guardian;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Prayers.GuardianPrayer;

public class GuardianPrayerTurnBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Entities.Guardian.GuardianPrayer guardian;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (guardian == null)
		{
			guardian = animator.GetComponentInParent<Gameplay.GameControllers.Entities.Guardian.GuardianPrayer>();
		}
		guardian.Behaviour.IsTurning = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		guardian.SetOrientation(guardian.Behaviour.GuessedOrientation);
		guardian.Behaviour.IsTurning = false;
	}
}
