using System;
using UnityEngine;

public class KitchenLevelGlowFader : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	[Range(0f, (float)Math.PI * 2f)]
	private float startOffset;

	private float t;

	[SerializeField]
	private float speedModifier = 1f;

	[SerializeField]
	private float alphaMin = 0.8f;

	private void Update()
	{
		t += CupheadTime.Delta;
		rend.color = new Color(1f, 1f, 1f, Mathf.Lerp(alphaMin, 1f, (Mathf.Sin(t * speedModifier) + 1f) / 2f));
	}
}
