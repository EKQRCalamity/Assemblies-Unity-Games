using Gameplay.GameControllers.Entities.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

public class AmanecidaAnimationAudioBehaviour : StateMachineBehaviour
{
	public string eventId;

	public bool isOneShot = true;

	[ShowIf("isOneShot", true)]
	public bool playSfxOnlyInBattlebounds = true;

	[ShowIf("isOneShot", true)]
	[Range(0f, 1f)]
	public float percentageToThrowEvent;

	[ShowIf("isOneShot", true)]
	[EnumToggleButtons]
	public EntityAudio.FxSoundCategory soundCategory;

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

	private AmanecidasAnimationEventsController amanecidasAnimationEventsController;

	private bool played;

	private bool stopped;

	private const float maxPercentageThreshold = 0.96f;

	private int lastLoop = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		played = false;
		stopped = false;
		if (amanecidasAnimationEventsController == null)
		{
			amanecidasAnimationEventsController = animator.GetComponent<AmanecidasAnimationEventsController>();
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
			if (played || !(num2 >= percentageToPlayEvent))
			{
				return;
			}
			bool flag = true;
			if (playSfxOnlyInBattlebounds)
			{
				SpriteRenderer spriteRenderer = animator.GetComponent<SpriteRenderer>();
				if (spriteRenderer == null)
				{
					spriteRenderer = animator.GetComponentInChildren<SpriteRenderer>();
				}
				flag = spriteRenderer.isVisible;
			}
			if (flag)
			{
				amanecidasAnimationEventsController.AnimationEvent_PlayAnimationOneShot(eventId, soundCategory);
				played = true;
			}
		}
		else
		{
			if (playsAudioEvent && !played && num2 >= percentageToPlayEvent)
			{
				amanecidasAnimationEventsController.AnimationEvent_PlayAnimationAudio(eventId);
				played = true;
			}
			if (stopsAudioEvent && !stopped && num2 >= percentageToStopEvent)
			{
				amanecidasAnimationEventsController.AnimationEvent_StopAnimationAudio(eventId);
				stopped = true;
			}
		}
	}
}
