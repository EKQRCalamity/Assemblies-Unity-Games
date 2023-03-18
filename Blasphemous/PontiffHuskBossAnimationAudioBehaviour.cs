using Sirenix.OdinInspector;
using UnityEngine;

public class PontiffHuskBossAnimationAudioBehaviour : StateMachineBehaviour
{
	public string eventId;

	public bool isOneShot = true;

	[ShowIf("isOneShot", true)]
	[Range(0f, 1f)]
	public float percentageToThrowEvent;

	[HideIf("isOneShot", true)]
	public bool playsAudioEvent;

	[ShowIf("playsAudioEvent", true)]
	[Range(0f, 1f)]
	public float percentageToPlayEvent;

	[HideIf("isOneShot", true)]
	public bool stopsAudioEvent;

	[ShowIf("stopsAudioEvent", true)]
	[Range(0f, 1f)]
	public float percentageToStopEvent;

	private PontiffHuskBossAnimationEventsController eventsController;

	private bool played;

	private bool stopped;

	private int lastLoop = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		played = false;
		stopped = false;
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
			played = false;
			stopped = false;
			return;
		}
		float num2 = stateInfo.normalizedTime - (float)num;
		if (isOneShot)
		{
			if (!played && num2 >= percentageToPlayEvent)
			{
				eventsController.Animation_PlayOneShot(eventId);
				played = true;
			}
			return;
		}
		if (playsAudioEvent && !played && num2 >= percentageToPlayEvent)
		{
			eventsController.Animation_PlayAudio(eventId);
			played = true;
		}
		if (stopsAudioEvent && !stopped && num2 >= percentageToStopEvent)
		{
			eventsController.Animation_StopAudio(eventId);
			stopped = true;
		}
	}
}
