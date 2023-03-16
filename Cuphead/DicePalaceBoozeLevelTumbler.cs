using System.Collections;
using UnityEngine;

public class DicePalaceBoozeLevelTumbler : DicePalaceBoozeLevelBossBase
{
	[SerializeField]
	private BoxCollider2D sprayCollider;

	private int attackDelayIndex;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	protected override void Awake()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.Awake();
	}

	private void Update()
	{
		damageDealer.Update();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		float num = health;
		health -= info.damage;
		if (num > 0f)
		{
			Level.Current.timeline.DealDamage(Mathf.Clamp(num - health, 0f, num));
		}
		if (health < 0f && !base.isDead)
		{
			StartDying();
			TumblerDeathSFX();
		}
	}

	public override void LevelInit(LevelProperties.DicePalaceBooze properties)
	{
		attackDelayIndex = Random.Range(0, properties.CurrentState.tumbler.beamDelayString.Split(',').Length);
		Level.Current.OnIntroEvent += OnIntroEnd;
		Level.Current.OnWinEvent += HandleDead;
		health = properties.CurrentState.tumbler.tumblerHP;
		AudioManager.Play("booze_tumbler_intro");
		emitAudioFromObject.Add("booze_tumbler_intro");
		base.LevelInit(properties);
	}

	private void OnIntroEnd()
	{
		StartCoroutine(attack_cr());
	}

	private IEnumerator attack_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, Parser.FloatParse(base.properties.CurrentState.tumbler.beamDelayString.Split(',')[attackDelayIndex]) - DicePalaceBoozeLevelBossBase.ATTACK_DELAY);
			base.animator.SetTrigger("OnAttack");
			yield return base.animator.WaitForAnimationToEnd(this, "Attack_Start");
			yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.tumbler.beamWarningDuration);
			base.animator.SetTrigger("Continue");
			yield return base.animator.WaitForAnimationToStart(this, "Attack");
			AudioManager.Play("booze_tumbler_attack");
			emitAudioFromObject.Add("booze_tumbler_attack");
			yield return base.animator.WaitForAnimationToEnd(this, "Attack");
			attackDelayIndex = (attackDelayIndex + 1) % base.properties.CurrentState.tumbler.beamDelayString.Split(',').Length;
			yield return null;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void EnableSpray()
	{
		AudioManager.Play("booze_tumbler_attack_spray");
		emitAudioFromObject.Add("booze_tumbler_attack_spray");
		base.animator.Play("Attack_Spray");
	}

	private void TumblerDeathSFX()
	{
		AudioManager.Play("tumbler_death_vox");
		emitAudioFromObject.Add("tumbler_death_vox");
	}
}
