using UnityEngine;

public class OldManLevelBubbleController : MonoBehaviour
{
	[SerializeField]
	private float minDelay = 0.05f;

	[SerializeField]
	private float maxDelay = 0.5f;

	[SerializeField]
	private float minTimeToRepeat = 2f;

	[SerializeField]
	private Animator[] animators;

	private float timer;

	private void Start()
	{
		for (int i = 0; i < animators.Length; i++)
		{
			animators[i].Play("Bubble", 0, 1f);
		}
	}

	private void FixedUpdate()
	{
		timer -= CupheadTime.FixedDelta;
		if (timer <= 0f)
		{
			int num = Random.Range(0, animators.Length);
			if (animators[num].GetCurrentAnimatorStateInfo(0).normalizedTime > minTimeToRepeat)
			{
				animators[num].Play("Bubble", 0, 0f);
			}
			timer = Random.Range(minDelay, maxDelay);
		}
	}
}
