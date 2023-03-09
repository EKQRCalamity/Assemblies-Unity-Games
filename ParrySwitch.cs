using System;
using System.Collections;
using UnityEngine;

public class ParrySwitch : AbstractSwitch
{
	[SerializeField]
	protected Effect parrySpark;

	[SerializeField]
	protected float coolDown = 0.4f;

	public bool IsParryable { get; protected set; }

	public event Action OnPrePauseActivate;

	protected void FirePrePauseEvent()
	{
		if (this.OnPrePauseActivate != null)
		{
			this.OnPrePauseActivate();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.tag = "ParrySwitch";
		IsParryable = true;
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
		FirePrePauseEvent();
	}

	public virtual void OnParryPostPause(AbstractPlayerController player)
	{
		DispatchEvent();
	}

	public void ActivateFromOtherSource()
	{
		if ((bool)parrySpark)
		{
			parrySpark.Create(base.transform.position);
		}
		DispatchEvent();
	}

	public void StartParryCooldown()
	{
		GetComponent<Collider2D>().enabled = false;
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
		yield return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		parrySpark = null;
	}
}
