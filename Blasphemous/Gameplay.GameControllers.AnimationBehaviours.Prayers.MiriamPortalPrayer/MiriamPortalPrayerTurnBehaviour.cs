using Gameplay.GameControllers.Entities.MiriamPortal;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Prayers.MiriamPortalPrayer;

public class MiriamPortalPrayerTurnBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Entities.MiriamPortal.MiriamPortalPrayer miriamPortal;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (miriamPortal == null)
		{
			miriamPortal = animator.GetComponentInParent<Gameplay.GameControllers.Entities.MiriamPortal.MiriamPortalPrayer>();
		}
		miriamPortal.Behaviour.IsTurning = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		miriamPortal.SetOrientation(miriamPortal.Behaviour.GuessedOrientation);
		miriamPortal.Behaviour.IsTurning = false;
	}
}
