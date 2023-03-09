using System;
using UnityEngine;

public class SnowCultLevelBat : AbstractProjectile
{
	private const float PADDING_FLOOR = 100f;

	private const float PADDING_CEILING = 100f;

	private const float DRIP_TIME_MIN = 0.3f;

	private const float DRIP_TIME_MAX = 0.7f;

	private DamageReceiver damageReceiver;

	private SnowCultLevelYeti parent;

	private float speed;

	private float Health;

	public bool reachedCircle;

	public bool moving;

	private Vector3 launchVelocity;

	private float attackHeight;

	private float attackWidth;

	private Vector3 attackStart;

	private float attackTime;

	private float arcModifier = 1f;

	private float dripTimer;

	private float shotSpeed;

	private bool readdOnEscape;

	private Vector3 lastPos;

	[SerializeField]
	private SnowCultLevelBatEffect explosionPrefab;

	[SerializeField]
	private SnowCultLevelBatEffect dripPrefab;

	[SerializeField]
	private Collider2D collider;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private string animatorSuffix;

	public virtual SnowCultLevelBat Init(Vector3 startPos, Vector3 launchVel, LevelProperties.SnowCult.Snowball properties, SnowCultLevelYeti parent, bool parryable, string suffix)
	{
		ResetLifetime();
		ResetDistance();
		this.parent = parent;
		this.parent.OnDeathEvent += Dead;
		base.transform.position = startPos;
		speed = properties.batAttackSpeed;
		readdOnEscape = properties.batsReaddedOnEscape;
		moving = false;
		launchVelocity = launchVel;
		base.transform.localScale = new Vector3(Mathf.Sign(0f - launchVel.x), 1f);
		Health = properties.batHP;
		shotSpeed = properties.batShotSpeed;
		animatorSuffix = suffix;
		SetParryable(parryable);
		base.animator.Play("Slowdown" + animatorSuffix, 0, UnityEngine.Random.Range(0f, 0.33f));
		return this;
	}

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void OnDieLifetime()
	{
	}

	protected override void OnDieDistance()
	{
	}

	public override void OnParryDie()
	{
		if (Level.Current.mode == Level.Mode.Easy)
		{
			EasyModeDie();
		}
		else
		{
			base.OnParryDie();
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

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		Health -= info.damage;
		if (Health < 0f)
		{
			Level.Current.RegisterMinionKilled();
			Dead();
		}
	}

	public void AttackPlayer(Vector3 startPos, float height, float width, float arc)
	{
		moving = true;
		attackStart = startPos;
		attackHeight = startPos.y - (CupheadLevelCamera.Current.Bounds.y + 100f) - height;
		attackWidth = width;
		base.transform.localScale = new Vector3(Mathf.Sign(0f - attackWidth), 1f);
		attackTime = 0f;
		arcModifier = arc;
		base.animator.SetFloat("YSpeed", -10f);
		base.animator.Play("Enter" + animatorSuffix);
		spriteRenderer.sortingOrder = 30;
		collider.enabled = true;
		reachedCircle = true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!reachedCircle)
		{
			if (base.transform.position.y >= 460f)
			{
				reachedCircle = true;
				collider.enabled = false;
			}
			else
			{
				base.transform.position += launchVelocity * CupheadTime.FixedDelta;
				launchVelocity += Vector3.up * 500f * CupheadTime.FixedDelta;
			}
		}
		if (!moving)
		{
			return;
		}
		attackTime += CupheadTime.FixedDelta * speed;
		if (attackTime < 0.9f)
		{
			lastPos = base.transform.position;
			base.transform.position = new Vector3(Mathf.Lerp(attackStart.x, attackStart.x + attackWidth, attackTime), attackStart.y + Mathf.Pow(Mathf.Sin(attackTime * (float)Math.PI), arcModifier) * (0f - attackHeight));
			base.animator.SetFloat("YSpeed", base.transform.position.y - lastPos.y);
		}
		else
		{
			Vector3 vector = new Vector3(attackStart.x + attackWidth, attackStart.y) - lastPos;
			if (vector.magnitude > 15f)
			{
				vector = vector.normalized * 15f;
			}
			base.transform.position += vector;
			base.animator.SetFloat("YSpeed", vector.y);
		}
		if (base.transform.position.y - lastPos.y > -6f)
		{
			dripTimer -= CupheadTime.FixedDelta;
			if (dripTimer <= 0f)
			{
				SnowCultLevelBatDrip snowCultLevelBatDrip = dripPrefab.Create(base.transform.position + Vector3.down * 50f) as SnowCultLevelBatDrip;
				snowCultLevelBatDrip.SetColor(animatorSuffix);
				snowCultLevelBatDrip.vel.x = (base.transform.position.x - lastPos.x) / 2f;
				dripTimer = UnityEngine.Random.Range(0.3f, 0.7f);
			}
		}
		if (attackTime > 1.2f)
		{
			if (readdOnEscape)
			{
				moving = false;
				collider.enabled = false;
				parent.ReturnBatToList(this);
			}
			else
			{
				Dead();
			}
		}
	}

	public void Dead()
	{
		if (base.transform.position.y < 360f)
		{
			((SnowCultLevelBatEffect)explosionPrefab.Create(base.transform.position)).SetColor(animatorSuffix);
			SFX_SNOWCULT_BatDie();
		}
		if (Level.Current.mode == Level.Mode.Easy)
		{
			EasyModeDie();
			return;
		}
		StopAllCoroutines();
		this.Recycle();
	}

	private void EasyModeDie()
	{
		moving = false;
		collider.enabled = false;
		parent.ReturnBatToList(this);
		base.transform.position = new Vector3(base.transform.position.x, 460f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		parent.OnDeathEvent -= Dead;
	}

	private void SFX_SNOWCULT_BatDie()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_popsicle_bat_death");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_popsicle_bat_death");
	}
}
