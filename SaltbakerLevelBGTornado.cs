using System;
using UnityEngine;

public class SaltbakerLevelBGTornado : AbstractMonoBehaviour
{
	[SerializeField]
	private float moveRange;

	[SerializeField]
	private float moveSpeed;

	private float t;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private float blurAmount;

	private void Start()
	{
		t = UnityEngine.Random.Range(0f, (float)Math.PI);
		rend.material.SetFloat("_BlurAmount", blurAmount * 5f);
		rend.material.SetFloat("_BlurLerp", blurAmount * 5f);
	}

	private void Update()
	{
		t += CupheadTime.Delta;
		base.transform.GetChild(0).localPosition = new Vector3(Mathf.Sin(t * moveSpeed) * moveRange, 0f);
	}
}
