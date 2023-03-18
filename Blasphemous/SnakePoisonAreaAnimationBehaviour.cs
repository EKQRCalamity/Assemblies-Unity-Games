using Sirenix.OdinInspector;
using UnityEngine;

public class SnakePoisonAreaAnimationBehaviour : StateMachineBehaviour
{
	public enum POSION_AREA_TRIGGER
	{
		SHOW,
		HIDE
	}

	private static readonly int T_SHOW = Animator.StringToHash("SHOW");

	private static readonly int T_HIDE = Animator.StringToHash("HIDE");

	[EnumToggleButtons]
	public POSION_AREA_TRIGGER TriggerToSet;

	public float WaitTimeBeforeSettingTrigger;

	private float timeWaited;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		timeWaited = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		timeWaited += Time.deltaTime;
		if (timeWaited > WaitTimeBeforeSettingTrigger)
		{
			if (TriggerToSet == POSION_AREA_TRIGGER.SHOW)
			{
				animator.SetTrigger(T_SHOW);
			}
			else
			{
				animator.SetTrigger(T_HIDE);
			}
		}
	}
}
