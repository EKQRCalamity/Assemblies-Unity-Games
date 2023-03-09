using System;
using System.Collections;
using UnityEngine;

public class OneTimeScrollingSprite : AbstractPausableComponent
{
	private const float X_OUT = -1280f;

	public float speed;

	public Func<bool> OutCondition;

	private void Start()
	{
		StartCoroutine(loop_cr());
	}

	private IEnumerator loop_cr()
	{
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		while (base.transform.position.x + spriteRenderer.bounds.size.x / 2f > -1280f)
		{
			if (OutCondition != null && OutCondition())
			{
				yield break;
			}
			Vector2 position = base.transform.localPosition;
			position.x -= speed * (float)CupheadTime.Delta;
			base.transform.localPosition = position;
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
