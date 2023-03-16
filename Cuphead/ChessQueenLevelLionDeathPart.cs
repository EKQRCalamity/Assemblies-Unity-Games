using System.Collections;
using UnityEngine;

public class ChessQueenLevelLionDeathPart : AbstractPausableComponent
{
	[SerializeField]
	private float growthSpeed;

	private void Start()
	{
		base.animator.Play("Loop", 0, Random.Range(0f, 1f));
		StartCoroutine(grow_cr());
	}

	private IEnumerator grow_cr()
	{
		float elapsed = 0f;
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		while (true)
		{
			yield return wait;
			elapsed += wait.frameTime + wait.accumulator;
			Vector3 scale = base.transform.localScale;
			scale.x = (1f + elapsed * growthSpeed) * Mathf.Sign(scale.x);
			scale.y = 1f + elapsed * growthSpeed;
			base.transform.localScale = scale;
		}
	}
}
