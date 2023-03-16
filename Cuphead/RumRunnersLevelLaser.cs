using System.Collections;
using UnityEngine;

public class RumRunnersLevelLaser : AbstractCollidableObject
{
	[SerializeField]
	private SpriteRenderer[] mainRenderers;

	[SerializeField]
	private SpriteRenderer[] warningRenderers;

	[SerializeField]
	private SpriteRenderer[] notesRenderers;

	[SerializeField]
	private CollisionChild[] childColliders;

	[SerializeField]
	private GameObject laserMaskPrefab;

	[SerializeField]
	private Effect sparklesEffect;

	private DamageDealer damageDealer;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		CollisionChild[] array = childColliders;
		foreach (CollisionChild collisionChild in array)
		{
			collisionChild.OnPlayerCollision += OnCollisionPlayer;
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		float num = damageDealer.DealDamage(hit);
		if (num > 0f)
		{
			SFX_RUMRUN_Grammobeam_DamagePlayer();
		}
	}

	private IEnumerator moveMask_cr(GameObject laserMask, float startX, float endX, float duration, bool destroyMask)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			laserMask.transform.SetLocalPosition(EaseUtils.Linear(startX, endX, elapsedTime / duration));
		}
		if (destroyMask)
		{
			Object.Destroy(laserMask);
		}
	}

	public void Begin()
	{
		StartCoroutine(begin_cr(mainRenderers));
	}

	private IEnumerator begin_cr(SpriteRenderer[] renderers, float durationMultiplier = 1f)
	{
		MinMax DurationRange = new MinMax(1.2f, 1.5f);
		foreach (SpriteRenderer renderer in renderers)
		{
			renderer.enabled = true;
			GameObject laserMask = Object.Instantiate(laserMaskPrefab);
			laserMask.transform.parent = renderer.transform;
			laserMask.transform.ResetLocalRotation();
			laserMask.transform.localPosition = new Vector3(-400f, 0f);
			laserMask.GetComponent<RumRunnersLevelLaserMask>().Setup(renderer.sortingLayerID, renderer.sortingOrder);
			StartCoroutine(moveMask_cr(laserMask, -400f, 800f, DurationRange.RandomFloat() * durationMultiplier, destroyMask: true));
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
	}

	public void End()
	{
		StartCoroutine(end_cr());
	}

	private IEnumerator end_cr()
	{
		MinMax DurationRange = new MinMax(1.2f, 1.5f);
		Coroutine[] coroutines = new Coroutine[mainRenderers.Length];
		for (int i = 0; i < mainRenderers.Length; i++)
		{
			SpriteRenderer renderer = mainRenderers[i];
			GameObject laserMask = Object.Instantiate(laserMaskPrefab);
			laserMask.transform.parent = renderer.transform;
			laserMask.transform.ResetLocalRotation();
			laserMask.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			laserMask.transform.localPosition = new Vector3(220f, 0f);
			laserMask.GetComponent<RumRunnersLevelLaserMask>().Setup(renderer.sortingLayerID, renderer.sortingOrder);
			coroutines[i] = StartCoroutine(moveMask_cr(laserMask, 220f, 1600f, DurationRange.RandomFloat(), destroyMask: false));
			StartCoroutine(endSparkles_cr(laserMask.transform));
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
		Coroutine[] array = coroutines;
		for (int j = 0; j < array.Length; j++)
		{
			yield return array[j];
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator endSparkles_cr(Transform maskTransform)
	{
		MinMax SpawnRandomizationRange = new MinMax(-15f, 15f);
		float elapsedTime = 0f;
		while (true)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			if (elapsedTime >= 0.02f)
			{
				elapsedTime -= 0.02f;
				for (int i = 0; i < 1; i++)
				{
					sparklesEffect.Create(maskTransform.position + maskTransform.right * 280f + new Vector3(SpawnRandomizationRange.RandomFloat(), SpawnRandomizationRange.RandomFloat()));
				}
			}
		}
	}

	public void Warning()
	{
		StartCoroutine(warning_cr());
	}

	private IEnumerator warning_cr()
	{
		float elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta;
			float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 0.3f);
			for (int i = 0; i < mainRenderers.Length; i++)
			{
				Color color = mainRenderers[i].color;
				color.a = alpha;
				mainRenderers[i].color = color;
				color = warningRenderers[i].color;
				color.a = 1f - alpha;
				warningRenderers[i].color = color;
			}
		}
	}

	public void CancelWarning()
	{
		StopAllCoroutines();
		SpriteRenderer[] array = mainRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			Color color = spriteRenderer.color;
			color.a = 1f;
			spriteRenderer.color = color;
		}
		SpriteRenderer[] array2 = warningRenderers;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			Color color2 = spriteRenderer2.color;
			color2.a = 0f;
			spriteRenderer2.color = color2;
		}
	}

	public void Attack()
	{
		base.animator.SetBool("On", value: true);
		SpriteRenderer[] array = notesRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.enabled = false;
		}
		StartCoroutine(begin_cr(notesRenderers, 0.5f));
	}

	public void EndAttack()
	{
		base.animator.SetBool("On", value: false);
	}

	private void animationEvent_WarningToOnStarted()
	{
		SpriteRenderer[] array = mainRenderers;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			Color color = spriteRenderer.color;
			color.a = 1f;
			spriteRenderer.color = color;
		}
		SpriteRenderer[] array2 = warningRenderers;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			Color color2 = spriteRenderer2.color;
			color2.a = 0f;
			spriteRenderer2.color = color2;
		}
	}

	private void SFX_RUMRUN_Grammobeam_DamagePlayer()
	{
		AudioManager.Play("sfx_dlc_rumrun_p2_grammobeam_damageplayer");
	}
}
