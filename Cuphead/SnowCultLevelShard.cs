using System;
using UnityEngine;

public class SnowCultLevelShard : BasicProjectileContinuesOnLevelEnd
{
	private Vector3 basePos;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 2f;

	private float wobbleTimer;

	private float speed;

	private float Health;

	private bool moving;

	private Vector2 pivotPos;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private Effect smoke;

	public virtual SnowCultLevelShard Init(Vector3 pivotPos, float angle, float loopSizeX, float loopSizeY, LevelProperties.SnowCult.ShardAttack properties)
	{
		ResetLifetime();
		ResetDistance();
		this.pivotPos = pivotPos;
		speed = properties.shardSpeed;
		Health = properties.shardHealth;
		GetComponent<Collider2D>().enabled = false;
		angle *= (float)Math.PI / 180f;
		base.transform.position = pivotPos + new Vector3((0f - Mathf.Sin(angle)) * loopSizeX, Mathf.Cos(angle) * loopSizeY);
		base.transform.SetEulerAngles(null, null, 90f + MathUtils.DirectionToAngle(pivotPos - base.transform.position));
		basePos = base.transform.position;
		SFX_SNOWCULT_JackFrostIceCreamProjSplatLoop();
		return this;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		Health -= info.damage;
		if (Health < 0f)
		{
			this.Recycle();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Appear()
	{
		base.animator.SetTrigger("Appear");
		SFX_SNOWCULT_JackFrostIcecreamAppear();
		smoke.Create(base.transform.position);
		GetComponent<Collider2D>().enabled = true;
	}

	public void LaunchProjectile()
	{
		base.animator.SetTrigger("StartMove");
		moving = true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!moving)
		{
			base.transform.position = basePos + Vector3.right * Mathf.Sin(wobbleTimer * 2f) * wobbleX + Vector3.up * Mathf.Cos(wobbleTimer * 3f) * wobbleY;
			wobbleTimer += CupheadTime.FixedDelta * wobbleSpeed;
		}
		else
		{
			base.transform.position += base.transform.up * (0f - speed) * CupheadTime.FixedDelta;
		}
	}

	private void SFX_SNOWCULT_JackFrostIceCreamProjSplatLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p3_snowflake_icecreamcone_splat_pre_loop");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_icecreamcone_splat_pre_loop");
	}

	private void SFX_SNOWCULT_JackFrostIcecreamAppear()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p3_snowflake_icecreamcone_splat_pre_loop");
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_icecreamcone_appear");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_icecreamcone_appear");
	}
}
