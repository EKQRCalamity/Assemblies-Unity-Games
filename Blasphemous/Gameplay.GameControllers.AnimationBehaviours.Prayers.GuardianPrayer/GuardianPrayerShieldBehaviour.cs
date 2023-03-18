using Gameplay.GameControllers.Entities.Guardian;
using Gameplay.GameControllers.Entities.Guardian.AI;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Prayers.GuardianPrayer;

public class GuardianPrayerShieldBehaviour : StateMachineBehaviour
{
	public Gameplay.GameControllers.Entities.Guardian.GuardianPrayer GuardianPrayer { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (GuardianPrayer == null)
		{
			GuardianPrayer = animator.GetComponentInParent<Gameplay.GameControllers.Entities.Guardian.GuardianPrayer>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		GuardianPrayer.Behaviour.SetState(GuardianPrayerBehaviour.GuardianState.Follow);
	}
}
