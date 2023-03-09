using System.Collections;
using UnityEngine;

public class FrogsLevelShort : LevelProperties.Frogs.Entity
{
	public enum State
	{
		Idle = 0,
		Rage = 1,
		Roll = 2,
		Clap = 3,
		Morphing = 4,
		Complete = 1000,
		Morphed = 1001
	}

	public enum Direction
	{
		Left,
		Right
	}

	[SerializeField]
	private Effect introDust;

	[SerializeField]
	private Transform[] rageRoots;

	[SerializeField]
	private FrogsLevelShortRageBullet rageFireball;

	[SerializeField]
	private Effect rageFireballSpark;

	[SerializeField]
	private FrogsLevelShortClapBullet clapBullet;

	[SerializeField]
	private Effect clapEffect;

	[SerializeField]
	private Transform clapRoot;

	private SpriteRenderer spriteRenderer;

	private DamageReceiver damageReceiver;

	private DamageDealer damageDealer;

	private LevelProperties.Frogs.ShortClap clapProperties;

	private FrogsLevelShortClapBullet.Direction clapDirection;

	private const float MORPH_ROLL_TIME = 1f;

	public State state { get; private set; }

	public Direction direction { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		damageDealer = new DamageDealer(1f, 0.3f, DamageDealer.DamageSource.Enemy, damagesPlayer: true, damagesEnemy: false, damagesOther: false);
	}

	private void Start()
	{
		Level.Current.OnIntroEvent += OnLevelIntro;
	}

	private void Update()
	{
		damageDealer.Update();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void LevelInit(LevelProperties.Frogs properties)
	{
		base.LevelInit(properties);
		properties.OnBossDeath += OnBossDeath;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!FrogsLevel.FINAL_FORM)
		{
			base.properties.DealDamage(info.damage);
		}
	}

	private void OnBossDeath()
	{
		AudioManager.Play("level_frogs_short_death");
		emitAudioFromObject.Add("level_frogs_short_death");
		AudioManager.PlayLoop("level_frogs_short_death_loop");
		emitAudioFromObject.Add("level_frogs_short_death_loop");
		StopAllCoroutines();
		base.animator.SetTrigger("OnDeath");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		introDust = null;
		rageFireball = null;
		rageFireballSpark = null;
		clapBullet = null;
		clapEffect = null;
	}

	private void SfxClap()
	{
		AudioManager.Play("level_frogs_short_clap_shock");
		emitAudioFromObject.Add("level_frogs_short_clap_shock");
	}

	private void SfxEndIntro()
	{
		AudioManager.Stop("level_frogs_short_intro_loop");
		AudioManager.Play("level_frogs_short_intro_start");
		emitAudioFromObject.Add("level_frogs_short_intro_start");
	}

	private void OnLevelIntro()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		AudioManager.PlayLoop("level_frogs_short_intro_loop");
		emitAudioFromObject.Add("level_frogs_short_intro_loop");
		base.animator.Play("Intro");
	}

	private void PlayIntroEffect()
	{
		introDust.Create(base.transform.position);
	}

	public void StartRage()
	{
		if (state == State.Idle || state == State.Complete)
		{
			state = State.Rage;
			StartCoroutine(rage_cr());
		}
	}

	private void Shoot(LevelProperties.Frogs.ShortRage properties, Vector3 pos, bool parry)
	{
		int num = ((direction != 0) ? 1 : (-1));
		AudioManager.Play("level_frogs_short_fireball");
		emitAudioFromObject.Add("level_frogs_short_fireball");
		rageFireballSpark.Create(pos, new Vector3(num, num, 1f));
		BasicProjectile basicProjectile = rageFireball.Create(pos, 0f, new Vector2(num, num), properties.shotSpeed * (float)num);
		basicProjectile.SetParryable(parry);
		basicProjectile.CollisionDeath.OnlyPlayer();
	}

	private IEnumerator rage_cr()
	{
		LevelProperties.Frogs.ShortRage p = base.properties.CurrentState.shortRage;
		base.animator.SetTrigger("OnRage");
		yield return base.animator.WaitForAnimationToEnd(this, "Rage");
		yield return CupheadTime.WaitForSeconds(this, p.anticipationDelay);
		base.animator.SetTrigger("OnRageAttack");
		yield return base.animator.WaitForAnimationToEnd(this, "Rage_Anticipate_End");
		AudioManager.PlayLoop("level_frogs_short_ragefist_attack_loop");
		emitAudioFromObject.Add("level_frogs_short_ragefist_attack_loop");
		int shotCount = p.shotCount;
		int root = 0;
		string parryString = p.parryPatterns[Random.Range(0, p.parryPatterns.Length)].ToLower();
		int parryIndex = 0;
		while (shotCount > 0)
		{
			yield return CupheadTime.WaitForSeconds(this, p.shotDelay);
			shotCount--;
			Shoot(p, rageRoots[root].position, parryString[parryIndex] == 'p');
			root = (int)Mathf.Repeat(root + 1, rageRoots.Length);
			parryIndex = (int)Mathf.Repeat(parryIndex + 1, parryString.Length);
		}
		yield return CupheadTime.WaitForSeconds(this, p.shotDelay);
		base.animator.SetTrigger("OnRageEnd");
		AudioManager.Stop("level_frogs_short_ragefist_attack_loop");
		yield return CupheadTime.WaitForSeconds(this, p.hesitate);
		state = State.Complete;
	}

	public void StartClap()
	{
		if (state == State.Idle || state == State.Complete)
		{
			state = State.Clap;
			StartCoroutine(clap_cr());
		}
	}

	private void ShootClap()
	{
		SfxClap();
		clapEffect.Create(clapRoot.position);
		clapBullet.Create(direction, clapDirection, clapRoot.position, clapRoot.right * clapProperties.bulletSpeed);
	}

	private IEnumerator clap_cr()
	{
		clapProperties = base.properties.CurrentState.shortClap;
		clapDirection = (Rand.Bool() ? FrogsLevelShortClapBullet.Direction.Down : FrogsLevelShortClapBullet.Direction.Up);
		clapRoot.SetEulerAngles(0f, 0f, clapProperties.angles.GetRandom());
		string patternString = clapProperties.patterns[Random.Range(0, clapProperties.patterns.Length)];
		KeyValue[] pattern = KeyValue.ListFromString(patternString, new char[2] { 'S', 'D' });
		base.animator.SetTrigger("OnClap");
		base.animator.SetBool("Clapping", value: true);
		yield return CupheadTime.WaitForSeconds(this, 1f + clapProperties.shotDelay);
		for (int i = 0; i < pattern.Length; i++)
		{
			if (pattern[i].key == "S")
			{
				for (int ii = 0; (float)ii < pattern[i].value; ii++)
				{
					clapDirection = ((clapDirection != FrogsLevelShortClapBullet.Direction.Down) ? FrogsLevelShortClapBullet.Direction.Down : FrogsLevelShortClapBullet.Direction.Up);
					if (i >= pattern.Length - 1 && (float)ii >= pattern[i].value - 1f)
					{
						base.animator.Play("Clap_End");
						yield return base.animator.WaitForAnimationToEnd(this, "Clap_End");
					}
					else
					{
						base.animator.Play("Clap_Shoot");
					}
					yield return CupheadTime.WaitForSeconds(this, 0.5f);
				}
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, pattern[i].value);
			}
		}
		base.animator.Play("Idle");
		base.animator.ResetTrigger("OnClap");
		base.animator.SetBool("Clapping", value: false);
		yield return CupheadTime.WaitForSeconds(this, clapProperties.hesitate);
		state = State.Complete;
	}

	public void StartRoll()
	{
		StopAllCoroutines();
		base.animator.Play("Idle");
		state = State.Roll;
		StartCoroutine(roll_cr());
	}

	private bool CheckRollable()
	{
		return state == State.Complete || state == State.Idle;
	}

	private IEnumerator roll_cr()
	{
		yield return null;
		float startX = base.transform.position.x;
		float endX = 0f - (startX + 240f);
		LevelProperties.Frogs.ShortRoll p = base.properties.CurrentState.shortRoll;
		base.animator.SetTrigger("OnRoll");
		yield return CupheadTime.WaitForSeconds(this, 1.2f + p.delay);
		base.animator.SetTrigger("OnRollContinue");
		CupheadLevelCamera.Current.StartShake(4f);
		yield return CupheadTime.WaitForSeconds(this, 1f);
		yield return base.animator.WaitForAnimationToStart(this, "Roll_Loop");
		float t = 0f;
		while (t < p.time)
		{
			float val = t / p.time;
			TransformExtensions.SetPosition(x: EaseUtils.Ease(EaseUtils.EaseType.easeInSine, startX, endX, val), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.transform.SetPosition(endX);
		yield return null;
		CupheadLevelCamera.Current.EndShake(0.5f);
		AudioManager.Stop("level_frogs_short_rolling_loop");
		AudioManager.Play("level_frogs_short_rolling_crash");
		emitAudioFromObject.Add("level_frogs_short_rolling_crash");
		spriteRenderer.enabled = false;
		direction = Direction.Right;
		yield return CupheadTime.WaitForSeconds(this, p.returnDelay);
		base.transform.SetScale(-1f);
		base.transform.SetPosition(0f - (startX + 140f));
		base.animator.SetTrigger("OnRollContinue");
		AudioManager.Play("level_frogs_short_rolling_end");
		emitAudioFromObject.Add("level_frogs_short_rolling_end");
		spriteRenderer.enabled = true;
		yield return CupheadTime.WaitForSeconds(this, 1f + p.hesitate);
		state = State.Complete;
		AudioManager.Stop("level_frogs_short_rolling_loop");
	}

	public void PlayRollSfx()
	{
		AudioManager.PlayLoop("level_frogs_short_rolling_loop");
		emitAudioFromObject.Add("level_frogs_short_rolling_loop");
		AudioManager.Play("level_frogs_short_rolling_start");
		emitAudioFromObject.Add("level_frogs_short_rolling_start");
	}

	public void StartMorph()
	{
		StopAllCoroutines();
		base.animator.Play("Idle");
		state = State.Morphing;
		StartCoroutine(morphRoll_cr());
	}

	private IEnumerator morphRoll_cr()
	{
		Vector2 start = base.transform.position;
		Vector2 end = FrogsLevelTall.Current.shortMorphRoot.position;
		base.animator.SetTrigger("OnRoll");
		yield return base.animator.WaitForAnimationToEnd(this, "Roll");
		yield return CupheadTime.WaitForSeconds(this, 1.5f);
		base.animator.SetTrigger("OnRollContinue");
		CupheadLevelCamera.Current.StartShake(4f);
		AudioManager.PlayLoop("level_frogs_short_rolling_loop");
		emitAudioFromObject.Add("level_frogs_short_rolling_loop");
		AudioManager.Play("level_frogs_short_rolling_start");
		emitAudioFromObject.Add("level_frogs_short_rolling_start");
		float t = 0f;
		while (t < 1f)
		{
			float val = t / 1f;
			float x = EaseUtils.Ease(EaseUtils.EaseType.linear, start.x, end.x, val);
			TransformExtensions.SetPosition(y: EaseUtils.Ease(EaseUtils.EaseType.linear, start.y, end.y, val), transform: base.transform, x: x);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		AudioManager.Stop("level_frogs_short_rolling_loop");
		base.transform.SetPosition(end.x, end.y);
		CupheadLevelCamera.Current.EndShake(0.5f);
		yield return null;
		FrogsLevelTall.Current.ContinueMorph();
		spriteRenderer.enabled = false;
		base.properties.OnBossDeath -= OnBossDeath;
		base.gameObject.SetActive(value: false);
	}
}
