using System.Collections;
using UnityEngine;

public class BeeLevelSecurityGuard : LevelProperties.Bee.Entity
{
	public enum State
	{
		Ready,
		Move,
		Attack,
		Leaving
	}

	[SerializeField]
	private Transform bombRoot;

	[SerializeField]
	private BeeLevelSecurityGuardBomb bombPrefab;

	private LevelProperties.Bee.SecurityGuard p;

	private DamageReceiver damageReceiver;

	private DamageDealer damageDealer;

	private CircleCollider2D circleCollider;

	public State state { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnTakeDamage;
		damageDealer = DamageDealer.NewEnemy();
		circleCollider = GetComponent<CircleCollider2D>();
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

	private void OnTakeDamage(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	public void StartSecurityGuard()
	{
		ResetGuard();
		p = base.properties.CurrentState.securityGuard;
		base.properties.OnStateChange += OnStateChange;
		StartCoroutine(go_cr());
	}

	private void OnStateChange()
	{
		base.properties.OnStateChange -= OnStateChange;
		Die();
	}

	private void Die()
	{
		StopAllCoroutines();
		StartCoroutine(leave_cr());
	}

	private void ResetGuard()
	{
		StopAllCoroutines();
	}

	private void SfxThrow()
	{
		AudioManager.Play("bee_guard_attack");
		emitAudioFromObject.Add("bee_guard_attack");
	}

	private void Attack()
	{
		bombPrefab.Create(bombRoot.position, -(int)base.transform.localScale.x, p.idleTime, p.warningTime, p.childSpeed, p.childCount);
	}

	private void AttackComplete()
	{
	}

	private void FlipX()
	{
		base.transform.SetScale(0f - base.transform.localScale.x, 1f, 1f);
	}

	private float hitPauseCoefficient()
	{
		return (!GetComponent<DamageReceiver>().IsHitPaused) ? 1f : 0f;
	}

	private IEnumerator go_cr()
	{
		AudioManager.Play("bee_guard_spawn");
		emitAudioFromObject.Add("bee_guard_spawn");
		AudioManager.PlayLoop("bee_guard_flying_loop");
		emitAudioFromObject.Add("bee_guard_flying_loop");
		while (true)
		{
			yield return StartCoroutine(move_cr());
			yield return StartCoroutine(attack_cr());
		}
	}

	private IEnumerator move_cr()
	{
		state = State.Move;
		float t = 0f;
		float time = p.attackDelay.RandomFloat();
		while (t < time)
		{
			base.transform.AddPositionForward2D((0f - p.speed) * (float)CupheadTime.Delta * base.transform.localScale.x * hitPauseCoefficient());
			if ((base.transform.localScale.x > 0f && base.transform.position.x <= -490f) || (base.transform.localScale.x < 0f && base.transform.position.x >= 490f))
			{
				yield return StartCoroutine(turn_cr());
			}
			t += (float)CupheadTime.Delta;
			yield return null;
		}
	}

	private IEnumerator attack_cr()
	{
		state = State.Attack;
		base.animator.SetTrigger("OnAttack");
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		yield return base.animator.WaitForAnimationToEnd(this, "Attack");
	}

	private IEnumerator leave_cr()
	{
		LevelBossDeathExploder exploder = GetComponent<LevelBossDeathExploder>();
		state = State.Leaving;
		exploder.StartExplosion();
		if (base.transform.localScale.x < 0f && base.transform.position.x < 0f)
		{
			base.transform.SetScale(-1f, 1f, 1f);
		}
		if (base.transform.localScale.x > 0f && base.transform.position.x > 0f)
		{
			base.transform.SetScale(1f, 1f, 1f);
		}
		base.animator.Play("Leave");
		AudioManager.Stop("bee_guard_flying_loop");
		AudioManager.Play("bee_guard_leave");
		emitAudioFromObject.Add("bee_guard_leave");
		circleCollider.enabled = false;
		yield return CupheadTime.WaitForSeconds(this, 2f);
		exploder.StopExplosions();
		AudioManager.Play("bee_guard_death");
		emitAudioFromObject.Add("bee_guard_death");
		bool leave = true;
		while (leave)
		{
			base.transform.AddPositionForward2D((0f - p.speed) * (float)CupheadTime.Delta * base.transform.localScale.x * hitPauseCoefficient());
			yield return null;
			if (base.transform.position.x > 1280f || base.transform.position.x < -1280f)
			{
				leave = false;
			}
		}
		state = State.Ready;
	}

	private IEnumerator turn_cr()
	{
		base.animator.Play("Turn");
		yield return base.animator.WaitForAnimationToEnd(this, "Turn");
		base.transform.SetScale(0f - base.transform.localScale.x, 1f, 1f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		bombPrefab = null;
	}
}
