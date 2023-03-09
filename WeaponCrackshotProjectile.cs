using System.Collections.Generic;
using UnityEngine;

public class WeaponCrackshotProjectile : BasicProjectile
{
	private Collider2D target;

	private bool cracked;

	public float maxAngleRange;

	public int variant;

	public bool useBComet;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private Effect crackFX;

	protected override void Start()
	{
		base.Start();
		base.animator.Play(variant.ToString(), 0, Random.Range(0f, 1f));
		damageDealer.isDLCWeapon = true;
		AudioManager.Play("player_weapon_crackshot_shoot");
		emitAudioFromObject.Add("player_weapon_crackshot_shoot");
	}

	protected override void OnDieDistance()
	{
		if (!base.dead)
		{
			Die();
			base.animator.SetTrigger("OnDistanceDie");
		}
	}

	protected override void Die()
	{
		move = false;
		base.Die();
		coll.enabled = false;
		if (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Comet"))
		{
			base.animator.Play((!Rand.Bool()) ? "ImpactCometB" : "ImpactCometA");
		}
		else
		{
			base.animator.Play((!Rand.Bool()) ? "ImpactSmallB" : "ImpactSmallA");
		}
	}

	private void _OnDieAnimComplete()
	{
		Object.Destroy(base.gameObject);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (cracked || !(base.distance > WeaponProperties.LevelWeaponCrackshot.Basic.crackDistance) || base.dead)
		{
			return;
		}
		cracked = true;
		crackFX.Create(base.transform.position);
		base.animator.SetBool("IsB", useBComet);
		base.animator.Play((!Rand.Bool()) ? "CometStartA" : "CometStartB");
		AudioManager.Play("player_weapon_crackshot_shootfast");
		emitAudioFromObject.Add("player_weapon_crackshot_shootfast");
		damageDealer.SetDamage(WeaponProperties.LevelWeaponCrackshot.Basic.crackedDamage);
		Speed = WeaponProperties.LevelWeaponCrackshot.Basic.crackedSpeed;
		FindTarget();
		if (!(target != null))
		{
			return;
		}
		if (Vector3.Angle(base.transform.right, target.bounds.center - base.transform.position) > maxAngleRange)
		{
			if (Mathf.Abs(base.transform.right.y) < 0.05f)
			{
				base.transform.eulerAngles = new Vector3(0f, 0f, (!(base.transform.eulerAngles.z > 90f)) ? (MathUtils.DirectionToAngle(base.transform.right) + maxAngleRange) : (MathUtils.DirectionToAngle(base.transform.right) - maxAngleRange));
			}
			else
			{
				base.transform.eulerAngles = new Vector3(0f, 0f, MathUtils.DirectionToAngle(base.transform.right) + maxAngleRange);
			}
		}
		else
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, MathUtils.DirectionToAngle(target.bounds.center - base.transform.position));
		}
	}

	public void FindTarget()
	{
		target = findBestTarget(AbstractProjectile.FindOverlapScreenDamageReceivers());
	}

	private Collider2D findBestTarget(IEnumerable<DamageReceiver> damageReceivers)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		Collider2D collider2D = null;
		Collider2D collider2D2 = null;
		Vector2 vector = base.transform.position;
		foreach (DamageReceiver damageReceiver in damageReceivers)
		{
			if (!damageReceiver.gameObject.activeInHierarchy || !damageReceiver.enabled || damageReceiver.type != 0)
			{
				continue;
			}
			Collider2D[] components = damageReceiver.GetComponents<Collider2D>();
			foreach (Collider2D collider2D3 in components)
			{
				if (collider2D3.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D3.bounds.center, collider2D3.bounds.size / 2f))
				{
					float sqrMagnitude = (vector - (Vector2)collider2D3.bounds.center).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						num2 = sqrMagnitude;
						collider2D2 = collider2D3;
					}
					if (sqrMagnitude < num && Vector3.Angle(base.transform.right, (Vector2)collider2D3.bounds.center - vector) < maxAngleRange)
					{
						num = sqrMagnitude;
						collider2D = collider2D3;
					}
				}
			}
			DamageReceiverChild[] componentsInChildren = damageReceiver.GetComponentsInChildren<DamageReceiverChild>();
			foreach (DamageReceiverChild damageReceiverChild in componentsInChildren)
			{
				Collider2D[] components2 = damageReceiverChild.GetComponents<Collider2D>();
				foreach (Collider2D collider2D4 in components2)
				{
					if (collider2D4.isActiveAndEnabled && CupheadLevelCamera.Current.ContainsPoint(collider2D4.bounds.center, collider2D4.bounds.size / 2f))
					{
						float sqrMagnitude2 = (vector - (Vector2)collider2D4.bounds.center).sqrMagnitude;
						if (sqrMagnitude2 < num2)
						{
							num2 = sqrMagnitude2;
							collider2D2 = collider2D4;
						}
						if (sqrMagnitude2 < num && Vector3.Angle(base.transform.right, (Vector2)collider2D4.bounds.center - vector) < maxAngleRange)
						{
							num = sqrMagnitude2;
							collider2D = collider2D4;
						}
					}
				}
			}
		}
		return (!(collider2D == null)) ? collider2D : collider2D2;
	}
}
