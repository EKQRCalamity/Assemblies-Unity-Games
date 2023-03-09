using System.Collections.Generic;
using UnityEngine;

public class SnowCultHandleBackground : AbstractPausableComponent
{
	private float fadeTimer;

	[SerializeField]
	private SpriteRenderer[] fadeRenderers;

	[SerializeField]
	private float[] fadePeriod;

	[SerializeField]
	private float[] fadeOffset;

	[SerializeField]
	private float[] fadeMin;

	[SerializeField]
	private float[] fadeMax;

	[SerializeField]
	private Animator[] candles;

	[SerializeField]
	private Animator glimmer;

	private float glimmerTimer = 2f;

	[SerializeField]
	private Animator[] sparkles;

	private float sparkleTimer = 1f;

	private List<int> sparkleList = new List<int>();

	private void Update()
	{
		fadeTimer += CupheadTime.Delta;
		for (int i = 0; i < fadeRenderers.Length; i++)
		{
			fadeRenderers[i].color = new Color(1f, 1f, 1f, Mathf.Lerp(fadeMin[i], fadeMax[i], Mathf.Abs((fadeTimer + fadeOffset[i] * fadePeriod[i]) % fadePeriod[i] - fadePeriod[i] / 2f)) / (fadePeriod[i] / 2f));
		}
		glimmerTimer -= CupheadTime.Delta;
		if (glimmerTimer <= 0f)
		{
			glimmer.Play("Glimmer", 0, 0f);
			glimmerTimer += Random.Range(3.5f, 6.5f);
		}
		sparkleTimer -= CupheadTime.Delta;
		if (!(sparkleTimer <= 0f))
		{
			return;
		}
		if (sparkleList.Count == 0)
		{
			for (int j = 0; j < sparkles.Length; j++)
			{
				sparkleList.Add(j);
			}
		}
		int index = Random.Range(0, sparkleList.Count);
		sparkles[sparkleList[index]].Play("Sparkle", 0, 0f);
		sparkleList.RemoveAt(index);
		sparkleTimer = Random.Range(0.25f, 0.75f);
	}

	public void CandleGust()
	{
		Animator[] array = candles;
		foreach (Animator animator in array)
		{
			animator.SetTrigger("OnGust");
		}
	}
}
