using System;
using System.Collections;
using UnityEngine;

public class TrainLevelBlindSpecter : LevelProperties.Train.Entity
{
	public delegate void OnDamageTakenHandler(float damage);

	[SerializeField]
	private Transform eyeRoot;

	[SerializeField]
	private TrainLevelBlindSpecterEyeProjectile eyePrefab;

	private SpriteRenderer spriteRenderer;

	private LevelProperties.Train.BlindSpecter blindSpecterProperties;

	private int shots;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private float health;

	private bool dead;

	public event OnDamageTakenHandler OnDamageTakenEvent;

	public event Action OnDeathEvent;

	protected override void Awake()
	{
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		damageDealer = DamageDealer.NewEnemy();
		base.animator.enabled = false;
		spriteRenderer.enabled = false;
	}

	private void Start()
	{
		Level.Current.OnIntroEvent += OnIntro;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
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

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!dead)
		{
			if (this.OnDamageTakenEvent != null)
			{
				this.OnDamageTakenEvent(info.damage);
			}
			health -= info.damage;
			if (health <= 0f)
			{
				Die();
			}
		}
	}

	public override void LevelInit(LevelProperties.Train properties)
	{
		base.LevelInit(properties);
		health = properties.CurrentState.blindSpecter.health;
	}

	private void OnIntro()
	{
		StartCoroutine(loop_cr());
	}

	private void Die()
	{
		if (!dead)
		{
			GetComponent<LevelBossDeathExploder>().StartExplosion();
			dead = true;
			damageReceiver.enabled = false;
			StopAllCoroutines();
			base.animator.Play("Death");
			AudioManager.Play("train_blindspector_death");
			emitAudioFromObject.Add("train_blindspector_death");
		}
	}

	private void OnDeathAnimComplete()
	{
		if (this.OnDeathEvent != null)
		{
			this.OnDeathEvent();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void SfxIntro()
	{
		AudioManager.Play("level_train_blindspecter_intro");
	}

	private void FireEyeball()
	{
		AudioManager.Play("train_blindspector_attack");
		emitAudioFromObject.Add("train_blindspector_attack");
		float value = UnityEngine.Random.value;
		Vector2 time = new Vector2(blindSpecterProperties.timeX.RandomFloat(), blindSpecterProperties.timeY.GetFloatAt(value));
		eyePrefab.Create(eyeRoot.position, time, blindSpecterProperties.heightMax.GetFloatAt(value), shots % 2 > 0, blindSpecterProperties.eyeHealth);
		shots++;
	}

	private IEnumerator loop_cr()
	{
		base.animator.enabled = true;
		spriteRenderer.enabled = true;
		base.animator.Play("Intro");
		blindSpecterProperties = base.properties.CurrentState.blindSpecter;
		yield return base.animator.WaitForAnimationToEnd(this, "Intro");
		yield return CupheadTime.WaitForSeconds(this, 2f);
		while (true)
		{
			shots = 0;
			base.animator.Play("Attack_Start");
			while (shots < blindSpecterProperties.attackLoops * 2)
			{
				yield return null;
			}
			base.animator.SetTrigger("Continue");
			yield return base.animator.WaitForAnimationToEnd(this, "Attack_End");
			yield return CupheadTime.WaitForSeconds(this, blindSpecterProperties.hesitate);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		eyePrefab = null;
	}
}
