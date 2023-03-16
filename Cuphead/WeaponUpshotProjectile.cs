using UnityEngine;

public class WeaponUpshotProjectile : AbstractProjectile
{
	public MinMax ySpeedMinMax;

	public float timeToArc;

	public float xSpeed;

	private float ySpeed;

	private float time;

	private bool onLeft;

	private float startAngle;

	protected override bool DestroyedAfterLeavingScreen => true;

	protected override void Start()
	{
		base.Start();
		damageDealer.isDLCWeapon = true;
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId);
		onLeft = player.transform.localScale.x < 0f;
		startAngle = base.transform.eulerAngles.z;
		SetAngle();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateSpeed();
		Move();
	}

	private void UpdateSpeed()
	{
		if (time < timeToArc)
		{
			time += CupheadTime.FixedDelta;
			ySpeed = ySpeedMinMax.GetFloatAt(time / timeToArc);
			ySpeed = ((!onLeft) ? ySpeed : (0f - ySpeed));
			SetAngle();
		}
	}

	private void Move()
	{
		if (!base.dead)
		{
			Vector3 vector = new Vector3(xSpeed, ySpeed);
			Quaternion quaternion = Quaternion.Euler(0f, 0f, startAngle);
			vector = quaternion * vector;
			base.transform.position += vector * CupheadTime.FixedDelta;
		}
	}

	private void SetAngle()
	{
		switch (Mathf.RoundToInt(startAngle))
		{
		case 0:
		case 180:
			base.transform.SetEulerAngles(null, null, (float)((!onLeft) ? (-45) : 225) + time * 2f / timeToArc * (float)((!onLeft) ? 45 : (-45)));
			break;
		case 45:
			base.transform.SetEulerAngles(null, null, time * 2f / timeToArc * 45f);
			break;
		case 90:
			base.transform.SetEulerAngles(null, null, (float)((!onLeft) ? 45 : (-225)) + time * 2f / timeToArc * (float)((!onLeft) ? 45 : (-45)));
			break;
		case 270:
			base.transform.SetEulerAngles(null, null, (float)((!onLeft) ? 225 : (-45)) + time * 2f / timeToArc * (float)((!onLeft) ? 45 : (-45)));
			break;
		case 135:
			base.transform.SetEulerAngles(null, null, 180f + time * 2f / timeToArc * -45f);
			break;
		case 225:
			base.transform.SetEulerAngles(null, null, 270f + time * 2f / timeToArc * -45f);
			break;
		case 315:
			base.transform.SetEulerAngles(null, null, 270f + time * 2f / timeToArc * 45f);
			break;
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemy(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void OnCollisionDie(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionDie(hit, phase);
		if (base.tag == "PlayerProjectile" && phase == CollisionPhase.Enter)
		{
			if ((bool)hit.GetComponent<DamageReceiver>() && hit.GetComponent<DamageReceiver>().enabled)
			{
				AudioManager.Play("player_shoot_hit_cuphead");
			}
			else
			{
				AudioManager.Play("player_weapon_peashot_miss");
			}
		}
	}

	protected override void Die()
	{
		base.Die();
		StopAllCoroutines();
	}
}
