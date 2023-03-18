using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class CinematicsAnimator : MonoBehaviour
{
	[InfoBox("You must add an animation event called OnCinematicsEnd", InfoMessageType.Info, null)]
	public string TriggerName = "NEXT";

	private Animator animator;

	public void SetTriggerList(List<float> triggerTimes)
	{
		if (triggerTimes.Count != 0)
		{
			animator = GetComponent<Animator>();
			StartCoroutine(WaitForExecuteTriggers(triggerTimes));
		}
	}

	private IEnumerator WaitForExecuteTriggers(List<float> triggerTimes)
	{
		int idx = 0;
		float oldTime = 0f;
		for (; idx < triggerTimes.Count; idx++)
		{
			float waitTime = triggerTimes[idx] - oldTime;
			Debug.Log("--- Wait idx:" + idx.ToString() + "  Global:" + triggerTimes[idx] + "  Wait:" + waitTime);
			yield return new WaitForSeconds(waitTime);
			oldTime = triggerTimes[idx];
			animator.SetTrigger(TriggerName);
		}
	}

	public void OnCinematicsEnd()
	{
		Core.Cinematics.OnCinematicsAnimationEnd();
	}
}
