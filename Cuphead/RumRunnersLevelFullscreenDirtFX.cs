using System.Collections;
using UnityEngine;

public class RumRunnersLevelFullscreenDirtFX : Effect
{
	private static int PreviousEffectA = -1;

	private static int PreviousEffectB = -1;

	[SerializeField]
	private float loopDirtSpeed;

	public override void Initialize(Vector3 position, Vector3 scale, bool randomR)
	{
		base.Initialize(position, scale, randomR);
		int num = base.animator.GetInteger("Effect");
		while (num == PreviousEffectA || num == PreviousEffectB)
		{
			num = Random.Range(0, base.animator.GetInteger("Count"));
			base.animator.SetInteger("Effect", num);
		}
		PreviousEffectB = PreviousEffectA;
		PreviousEffectA = num;
		base.animator.Update(0f);
		float y = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
		if (num == 0 || num == 1)
		{
			StartCoroutine(fall_cr(loopDirtSpeed, y));
			return;
		}
		float currentClipLength = base.animator.GetCurrentClipLength();
		if (currentClipLength == 0f)
		{
			OnEffectComplete();
		}
		base.animator.Update(0f);
		float speed = (887.79285f + y) / currentClipLength;
		StartCoroutine(fall_cr(speed, y));
	}

	private IEnumerator fall_cr(float speed, float spriteHeight)
	{
		while (base.transform.position.y > -360f - spriteHeight - 100f)
		{
			yield return null;
			Vector3 position = base.transform.position;
			position.y -= speed * (float)CupheadTime.Delta;
			base.transform.position = position;
		}
		OnEffectComplete();
	}
}
