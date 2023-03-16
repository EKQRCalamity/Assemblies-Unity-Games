using UnityEngine;

public class PlaneWeaponChaliceBombExProjectile : AbstractProjectile
{
	public enum State
	{
		Idle,
		Frozen
	}

	public float MaxSpeed;

	public float Acceleration;

	public float FreezeTime;

	[SerializeField]
	private Effect chompFxPrefab;

	[SerializeField]
	private Transform chompFxRoot;

	public Vector3 Velocity;

	public float Gravity;

	public float DamageRateIncrease;

	private State state;

	private float timeSinceFrozen;

	public float speed;

	protected override void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		base.OnDealDamage(damage, receiver, damageDealer);
		DamageRate += DamageRateIncrease;
		damageDealer.SetRate(DamageRate);
		chompFxPrefab.Create(chompFxRoot.position);
		state = State.Frozen;
		speed = 0f;
		timeSinceFrozen = 0f;
		AudioManager.Play("player_plane_weapon_ex_chomp");
		emitAudioFromObject.Add("player_plane_weapon_ex_chomp");
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.dead)
		{
			return;
		}
		switch (state)
		{
		case State.Idle:
			Velocity.y -= Gravity * CupheadTime.FixedDelta;
			base.transform.position += Velocity * CupheadTime.FixedDelta;
			base.transform.rotation = Quaternion.Euler(0f, 0f, MathUtils.DirectionToAngle(Velocity));
			break;
		case State.Frozen:
			timeSinceFrozen += CupheadTime.FixedDelta;
			if (timeSinceFrozen > FreezeTime)
			{
				state = State.Idle;
			}
			break;
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		DealDamage(hit);
		base.OnCollisionEnemy(hit, phase);
	}

	protected override void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
		if (!(hit.tag == "Parry"))
		{
			base.OnCollisionOther(hit, phase);
		}
	}

	private void DealDamage(GameObject hit)
	{
		damageDealer.DealDamage(hit);
	}

	public override void OnLevelEnd()
	{
	}
}
