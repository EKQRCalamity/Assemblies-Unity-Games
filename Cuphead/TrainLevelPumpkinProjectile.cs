using System.Collections;
using UnityEngine;

public class TrainLevelPumpkinProjectile : AbstractProjectile
{
	private const float DEATH_Y = -325f;

	public float fallTime;

	private bool hasDied;

	protected override void Start()
	{
		base.Start();
		SetParryable(parryable: true);
		StartCoroutine(float_cr());
	}

	protected override void Update()
	{
		base.Update();
		if (!hasDied && base.transform.position.y < -325f)
		{
			Die();
		}
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (phase != 0)
		{
			return;
		}
		if (hit.tag == "ParrySwitch")
		{
			ParrySwitch component = hit.GetComponent<ParrySwitch>();
			if (!(component.name != "Right") || !(component.name != "Left"))
			{
				component.ActivateFromOtherSource();
				Die();
			}
		}
		else if (hit.name == "HandCar")
		{
			Die();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Drop()
	{
		base.transform.SetParent(null);
		StopAllCoroutines();
		base.animator.Play("Fall");
		StartCoroutine(drop_cr());
	}

	protected override void Die()
	{
		if (!hasDied)
		{
			hasDied = true;
			StopAllCoroutines();
			base.Die();
		}
	}

	private IEnumerator float_cr()
	{
		float top = base.transform.localPosition.y;
		float bottom = top - 20f;
		float time = 0.4f;
		while (true)
		{
			yield return TweenLocalPositionY(top, bottom, time, EaseUtils.EaseType.easeInOutSine);
			yield return TweenLocalPositionY(bottom, top, time, EaseUtils.EaseType.easeInOutSine);
		}
	}

	private IEnumerator drop_cr()
	{
		float top = base.transform.position.y;
		yield return TweenPositionY(top, -340f, fallTime, EaseUtils.EaseType.easeInSine);
		Die();
	}
}
