using System;
using UnityEngine;

public class AirplaneLevelSecretTerrierBullet : AbstractProjectile
{
	private float speed;

	private float arcHeight;

	private float splitAngle;

	private float splitSpeed;

	private float hp;

	private bool willSplit;

	private Vector3 startPos;

	private Vector3 destPos;

	private Vector3 lastPos;

	private float posTimer;

	private bool exploded;

	[SerializeField]
	private CircleCollider2D coll;

	[SerializeField]
	private BasicProjectile splitBulletPrefab;

	private DamageReceiver damageReceiver;

	public AirplaneLevelSecretTerrierBullet Create(Vector3 pos, Vector3 targetPos, LevelProperties.Airplane.SecretTerriers props, Vector3 scale)
	{
		AirplaneLevelSecretTerrierBullet airplaneLevelSecretTerrierBullet = base.Create() as AirplaneLevelSecretTerrierBullet;
		airplaneLevelSecretTerrierBullet.speed = props.dogBulletArcSpeed;
		airplaneLevelSecretTerrierBullet.arcHeight = props.dogBulletArcHeight;
		airplaneLevelSecretTerrierBullet.splitAngle = props.dogBulletSplitAngle;
		airplaneLevelSecretTerrierBullet.splitSpeed = props.dogBulletSplitSpeed;
		airplaneLevelSecretTerrierBullet.willSplit = props.dogBulletWillSplit;
		airplaneLevelSecretTerrierBullet.hp = props.dogBulletHealth;
		airplaneLevelSecretTerrierBullet.transform.position = pos;
		airplaneLevelSecretTerrierBullet.posTimer = 0f;
		airplaneLevelSecretTerrierBullet.startPos = pos;
		airplaneLevelSecretTerrierBullet.destPos = targetPos;
		airplaneLevelSecretTerrierBullet.lastPos = airplaneLevelSecretTerrierBullet.startPos;
		airplaneLevelSecretTerrierBullet.transform.localScale = scale;
		airplaneLevelSecretTerrierBullet.damageReceiver = airplaneLevelSecretTerrierBullet.GetComponent<DamageReceiver>();
		airplaneLevelSecretTerrierBullet.damageReceiver.OnDamageTaken += airplaneLevelSecretTerrierBullet.OnDamageTaken;
		return airplaneLevelSecretTerrierBullet;
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!exploded)
		{
			if (posTimer < 1f)
			{
				lastPos = base.transform.position;
				base.transform.position = Vector3.Lerp(startPos, destPos, posTimer) + Vector3.up * Mathf.Sin(posTimer * (float)Math.PI) * arcHeight;
				posTimer += speed * CupheadTime.FixedDelta;
			}
			else
			{
				base.transform.position += destPos - lastPos;
				lastPos += Vector3.up * arcHeight * CupheadTime.FixedDelta * 0.25f;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!exploded)
		{
			hp -= info.damage;
			if (hp <= 0f)
			{
				exploded = true;
				coll.enabled = false;
				base.animator.Play((!Rand.Bool()) ? "ExplodeB" : "ExplodeA");
				AudioManager.Play("sfx_dlc_dogfight_ps_terrier_pineappleexplode");
			}
		}
	}

	private void AniEvent_SpawnShrapnel()
	{
		splitBulletPrefab.Create(base.transform.position, MathUtils.DirectionToAngle(Vector3.right), splitSpeed);
		splitBulletPrefab.Create(base.transform.position, MathUtils.DirectionToAngle(Vector3.right) - splitAngle, splitSpeed);
		splitBulletPrefab.Create(base.transform.position, MathUtils.DirectionToAngle(Vector3.right) + splitAngle, splitSpeed);
	}

	private void AniEvent_EndExplosion()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
		AudioManager.Play("sfx_dlc_dogfight_ps_terrier_pineapplehitplayer");
	}
}
