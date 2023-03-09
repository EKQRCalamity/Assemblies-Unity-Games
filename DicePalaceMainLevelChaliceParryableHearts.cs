using System;
using System.Collections;
using UnityEngine;

public class DicePalaceMainLevelChaliceParryableHearts : AbstractProjectile
{
	[SerializeField]
	private Effect parrySpark;

	[SerializeField]
	protected float coolDown = 0.4f;

	public bool IsParryable { get; protected set; }

	public override float ParryMeterMultiplier => 0f;

	public event Action OnPrePauseActivate;

	protected override void Awake()
	{
		base.Awake();
		SetParryable(parryable: true);
		if (!(GetComponent<Collider2D>() == null))
		{
		}
	}

	public virtual void OnParryPrePause(AbstractPlayerController player)
	{
		if ((bool)parrySpark)
		{
			parrySpark.Create(base.transform.position);
		}
	}

	public override void OnParry(AbstractPlayerController player)
	{
		SetParryable(parryable: false);
		StartCoroutine(parryCooldown_cr());
	}

	private IEnumerator parryCooldown_cr()
	{
		float t = 0f;
		while (t < coolDown)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		Collider2D collider = GetComponent<Collider2D>();
		collider.enabled = true;
		SetParryable(parryable: true);
		yield return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		parrySpark = null;
	}
}
