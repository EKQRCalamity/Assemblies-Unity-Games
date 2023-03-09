using System;
using System.Collections;
using UnityEngine;

public class RumRunnersLevelBarrel : LevelProperties.RumRunners.Entity
{
	[SerializeField]
	private Effect deathPoof;

	[SerializeField]
	private Effect deathShrapnel;

	[SerializeField]
	private float verticalOffset;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private float runSpeed;

	private float HP;

	private float facingDirection;

	private Collider2D coll;

	private RumRunnersLevelWorm parent;

	private bool isCop;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		coll = GetComponent<Collider2D>();
	}

	public override void LevelInit(LevelProperties.RumRunners properties)
	{
		base.LevelInit(properties);
		((RumRunnersLevel)Level.Current).OnUpperBridgeDestroy += onUpperBridgeDestroy;
	}

	public void Initialize(float dir, Vector3 spawnPos, RumRunnersLevelWorm parent, bool parryable, bool isCop)
	{
		this.isCop = isCop;
		facingDirection = dir;
		base.transform.position = spawnPos;
		base.transform.localScale = new Vector3(dir, 1f);
		this.parent = parent;
		runSpeed = base.properties.CurrentState.barrels.barrelSpeed;
		HP = base.properties.CurrentState.barrels.barrelHP;
		_canParry = parryable;
		if (isCop)
		{
			base.animator.Play("Cop");
		}
		else if (Rand.Bool())
		{
			base.animator.Play((!base.canParry) ? "DanceA" : "DanceAParry");
		}
		else
		{
			base.animator.Play((!base.canParry) ? "DanceB" : "DanceBParry");
		}
		StartCoroutine(move_cr());
	}

	public override void OnParry(AbstractPlayerController player)
	{
		player.stats.OnParry();
		Die(immediate: false, spawnShrapnel: false);
		_canParry = false;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		HP -= info.damage;
		if (HP <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Die(immediate: false);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		damageDealer.DealDamage(hit);
	}

	private IEnumerator move_cr()
	{
		while (base.transform.position.x * facingDirection < 960f)
		{
			base.transform.position += Vector3.right * facingDirection * runSpeed * CupheadTime.FixedDelta;
			base.transform.SetPosition(null, RumRunnersLevel.GroundWalkingPosY(base.transform.position, coll, verticalOffset));
			if (Level.Current.mode == Level.Mode.Easy && parent.isDead)
			{
				Die(immediate: false);
				_canParry = false;
			}
			yield return new WaitForFixedUpdate();
		}
		Die(immediate: true);
	}

	public void Die(bool immediate, bool spawnShrapnel = true)
	{
		((RumRunnersLevel)Level.Current).OnUpperBridgeDestroy -= onUpperBridgeDestroy;
		StopAllCoroutines();
		if (immediate)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (isCop)
		{
			StartCoroutine(copDeath_cr());
			return;
		}
		if (base.transform.position.x * facingDirection < 960f)
		{
			Effect effect = deathPoof.Create(base.transform.position);
			if (!spawnShrapnel)
			{
				effect.GetComponent<Animator>().Play("Poof", 0, 1f / 12f);
			}
			SFX_RUMRUN_BarrelExplode();
			if (spawnShrapnel)
			{
				float num = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						float f = num + (float)Math.PI * 2f * (float)j / 4f;
						Vector3 vector = new Vector3(Mathf.Cos(f) * 50f, Mathf.Sin(f) * 50f);
						Effect effect2 = deathShrapnel.Create(base.transform.position + vector);
						effect2.animator.SetInteger("Effect", j);
						effect2.animator.SetBool("Parry", _canParry);
						if (i > 0)
						{
							SpriteRenderer component = effect2.GetComponent<SpriteRenderer>();
							component.sortingLayerName = "Background";
							component.sortingOrder = 95;
							component.color = new Color(0.7f, 0.7f, 0.7f, 1f);
							effect2.transform.SetScale(0.75f, 0.75f);
						}
						SpriteDeathParts component2 = effect2.GetComponent<SpriteDeathParts>();
						if (vector.x > 0f)
						{
							component2.SetVelocityX(0f, component2.VelocityXMax);
						}
						else
						{
							component2.SetVelocityX(component2.VelocityXMin, 0f);
						}
					}
				}
			}
		}
		if (!spawnShrapnel)
		{
			GetComponent<Collider2D>().enabled = false;
			runSpeed = 0f;
			StartCoroutine(destroy_with_delay_cr());
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator destroy_with_delay_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator copDeath_cr()
	{
		SFX_RUMRUN_Police_DiePoof();
		GetComponent<BoxCollider2D>().enabled = false;
		base.animator.SetTrigger("CopDeath");
		yield return base.animator.WaitForNormalizedTime(this, 1f, "CopDeath");
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void onUpperBridgeDestroy(Rangef effectRange)
	{
		if (!(base.transform.position.y < 0f) && effectRange.ContainsInclusive(base.transform.position.x))
		{
			Die(immediate: false);
			_canParry = false;
		}
	}

	private void SFX_RUMRUN_BarrelExplode()
	{
		AudioManager.Play("sfx_dlc_rumrun_barrel_explode");
		emitAudioFromObject.Add("sfx_dlc_rumrun_barrel_explode");
	}

	private void SFX_RUMRUN_Police_DiePoof()
	{
		AudioManager.Play("sfx_dlc_rumrun_lackey_poof");
		emitAudioFromObject.Add("sfx_dlc_rumrun_lackey_poof");
		AudioManager.Stop("sfx_dlc_rumrun_policegun_shoot");
	}
}
