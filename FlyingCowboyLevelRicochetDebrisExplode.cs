using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelRicochetDebrisExplode : Effect
{
	private void Start()
	{
		Vector3 position = base.transform.position;
		position.z = Random.Range(0f, 1f);
		base.transform.position = position;
	}

	private IEnumerator movement_cr()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		float elapsedTime = 0f;
		while (elapsedTime < 0.5f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			Vector3 position = base.transform.position;
			position.x -= 900f * (float)CupheadTime.Delta;
			base.transform.position = position;
			Color color = renderer.color;
			color.a = Mathf.Lerp(1f, 0f, elapsedTime / 0.5f);
			renderer.color = color;
		}
		OnEffectComplete();
	}

	private void animationEvent_StartMovement()
	{
		StartCoroutine(movement_cr());
	}
}
