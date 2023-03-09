using System;
using System.Collections;
using UnityEngine;

public class OldManLevelScubaGnome : AbstractProjectile
{
	private const float SPLASH_IN_TRIGGER_OFFSET = 75f;

	private const float SPLASH_X_POSITION_OFFSET = 35f;

	private const float UNDERWATER_FADE_OFFSET = -50f;

	private const float LOWEST_SHOOT_POS = 0f;

	[Header("Death FX")]
	[SerializeField]
	private Effect deathPuff;

	[SerializeField]
	private SpriteDeathParts[] deathParts;

	[Header("Prefabs")]
	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private Transform shootRoot;

	private const float Y_DIST_TO_SHOOT = 10f;

	private float hp;

	private bool isTypeA;

	private bool onLeft;

	private LevelProperties.OldMan.ScubaGnomes properties;

	private AbstractPlayerController player;

	private OldManLevelGnomeLeader leader;

	private DamageReceiver damageReceiver;

	private bool dartParryable;

	[SerializeField]
	private SpriteRenderer underwaterSprite;

	public virtual OldManLevelScubaGnome Init(Vector3 pos, AbstractPlayerController player, bool isTypeA, bool onLeft, bool dartParryable, LevelProperties.OldMan.ScubaGnomes properties, OldManLevelGnomeLeader leader)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		base.transform.SetScale((!onLeft) ? 1 : (-1));
		this.properties = properties;
		this.player = player;
		hp = properties.hp;
		this.isTypeA = isTypeA;
		this.onLeft = onLeft;
		this.leader = leader;
		this.dartParryable = dartParryable;
		base.animator.SetBool("IsGreen", Rand.Bool());
		base.animator.Play("Start");
		StartCoroutine(move_cr());
		return this;
	}

	private void OnEnable()
	{
		Level.Current.OnLevelEndEvent += Dead;
	}

	private void OnDisable()
	{
		if (Level.Current != null)
		{
			Level.Current.OnLevelEndEvent -= Dead;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void OnDestroy()
	{
		damageReceiver.OnDamageTaken -= OnDamageTaken;
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Dead();
		}
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float t = 0f;
		float start = base.transform.position.y;
		bool shotBullet = false;
		bool toTop = false;
		bool splashed = false;
		while (t < properties.scubaMoveTime)
		{
			TransformExtensions.SetPosition(y: start + Mathf.Sin(t / properties.scubaMoveTime * (float)Math.PI / 2f) * properties.jumpHeight, transform: base.transform);
			t += CupheadTime.FixedDelta;
			if (!splashed && base.transform.position.y > leader.splashHandler.transform.position.y)
			{
				leader.splashHandler.SplashIn(base.transform.position.x + 35f * base.transform.localScale.x);
				splashed = true;
				SFX_JumpOut();
				SFX_Vocal();
			}
			underwaterSprite.color = new Color(1f, 1f, 1f, (1f - Mathf.InverseLerp(leader.splashHandler.transform.position.y + -50f, leader.splashHandler.transform.position.y + -50f - 140f, base.transform.position.y)) * 0.5f);
			if (!toTop && t >= 0.8f)
			{
				base.animator.SetTrigger("ToTop");
				toTop = true;
			}
			yield return wait;
		}
		splashed = false;
		t = 0f;
		while (t < properties.scubaMoveTime)
		{
			TransformExtensions.SetPosition(y: start + Mathf.Sin((t / properties.scubaMoveTime + 1f) * (float)Math.PI / 2f) * properties.jumpHeight, transform: base.transform);
			t += (float)CupheadTime.Delta;
			if (!splashed && base.transform.position.y < leader.splashHandler.transform.position.y - 75f)
			{
				leader.splashHandler.SplashIn(base.transform.position.x + 35f * base.transform.localScale.x);
				splashed = true;
				SFX_DiveDown();
			}
			underwaterSprite.color = new Color(1f, 1f, 1f, (1f - Mathf.InverseLerp(leader.splashHandler.transform.position.y + -50f, leader.splashHandler.transform.position.y + -50f - 140f, base.transform.position.y)) * 0.5f);
			if (!toTop && t >= 0.8f)
			{
				base.animator.SetTrigger("ToTop");
				toTop = true;
			}
			float dist = shootRoot.position.y - (player.center.y + properties.shootDistOffset);
			if (!shotBullet && (dist < 10f || shootRoot.position.y < 0f))
			{
				base.animator.SetTrigger("Shoot");
				shotBullet = true;
			}
			yield return wait;
		}
		this.Recycle();
		yield return null;
	}

	private void Shoot()
	{
		float rotation = ((!(base.transform.position.x < 0f)) ? 180f : 0f);
		float speed = ((!isTypeA) ? properties.shotSpeedB : properties.shotSpeedA);
		BasicProjectile basicProjectile = projectile.Create(shootRoot.position, rotation, speed);
		basicProjectile.SetParryable(dartParryable);
		if (dartParryable)
		{
			basicProjectile.GetComponent<Animator>().Play("Pink");
		}
		basicProjectile.GetComponent<SpriteRenderer>().flipY = base.transform.position.x > 0f;
		SFX_ShootDart();
	}

	private void Dead()
	{
		deathPuff.Create(base.transform.position);
		for (int i = 0; i < deathParts.Length; i++)
		{
			if (i != 0 || UnityEngine.Random.Range(0, 10) == 0)
			{
				SpriteDeathParts spriteDeathParts = deathParts[i].CreatePart(base.transform.position);
				if (i != 0)
				{
					spriteDeathParts.animator.Play((!base.animator.GetBool("IsGreen")) ? "_Blue" : "_Teal");
				}
			}
		}
		AudioManager.Play("sfx_dlc_omm_gnome_death");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_death");
		AudioManager.Stop("sfx_dlc_omm_p3_gnomediver_vocal");
		this.Recycle();
	}

	private void SFX_DiveDown()
	{
		AudioManager.Play("sfx_dlc_omm_p3_gnomediver_divedown");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_gnomediver_divedown");
	}

	private void SFX_JumpOut()
	{
		AudioManager.Play("sfx_dlc_omm_p3_gnomediver_jumpout");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_gnomediver_jumpout");
	}

	private void SFX_ShootDart()
	{
		AudioManager.Play("sfx_dlc_omm_p3_gnomediver_shootdart");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_gnomediver_shootdart");
	}

	private void SFX_Vocal()
	{
		AudioManager.Play("sfx_dlc_omm_p3_gnomediver_vocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_gnomediver_vocal");
	}

	private void WORKAROUND_NullifyFields()
	{
		deathPuff = null;
		deathParts = null;
		projectile = null;
		shootRoot = null;
		player = null;
		leader = null;
		underwaterSprite = null;
	}
}
