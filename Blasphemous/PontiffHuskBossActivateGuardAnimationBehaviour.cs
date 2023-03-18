using Sirenix.OdinInspector;
using UnityEngine;

public class PontiffHuskBossActivateGuardAnimationBehaviour : StateMachineBehaviour
{
	public bool activatesGuard;

	public bool deactivatesGuard;

	[ShowIf("activatesGuard", true)]
	[Range(0f, 1f)]
	public float percentageToActivateGuard;

	[ShowIf("deactivatesGuard", true)]
	[Range(0f, 1f)]
	public float percentageToDeactivateGuard;

	private PontiffHuskBossAnimationEventsController eventsController;

	private bool activated;

	private bool deactivated;

	private int lastLoop = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		activated = false;
		deactivated = false;
		if (eventsController == null)
		{
			eventsController = animator.GetComponent<PontiffHuskBossAnimationEventsController>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int num = Mathf.FloorToInt(stateInfo.normalizedTime);
		if (num > lastLoop)
		{
			lastLoop = num;
			activated = false;
			deactivated = false;
			return;
		}
		float num2 = stateInfo.normalizedTime - (float)num;
		if (activatesGuard && !activated && num2 >= percentageToActivateGuard)
		{
			eventsController.Animation_DoActivateGuard(act: true);
			activated = true;
		}
		if (deactivatesGuard && !deactivated && num2 >= percentageToDeactivateGuard)
		{
			eventsController.Animation_DoActivateGuard(act: false);
			deactivated = true;
		}
	}
}
