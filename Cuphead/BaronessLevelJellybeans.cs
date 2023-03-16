using System.Collections;
using UnityEngine;

public class BaronessLevelJellybeans : AbstractProjectile
{
	public enum State
	{
		Dead,
		Run,
		Jump
	}

	[SerializeField]
	private Effect explosion;

	private float health;

	private float speed;

	private float velocity;

	private Vector3 originalPos;

	private LevelProperties.Baroness.Jellybeans properties;

	private DamageReceiver damageReceiver;

	public State state { get; private set; }

	public BaronessLevelJellybeans Create(LevelProperties.Baroness.Jellybeans properties, Vector3 pos, float speed, float health)
	{
		BaronessLevelJellybeans baronessLevelJellybeans = base.Create() as BaronessLevelJellybeans;
		baronessLevelJellybeans.properties = properties;
		baronessLevelJellybeans.speed = speed;
		baronessLevelJellybeans.health = health;
		baronessLevelJellybeans.transform.position = pos;
		return baronessLevelJellybeans;
	}

	protected override void Start()
	{
		base.Start();
		GetComponent<Collider2D>().enabled = true;
		GetComponent<SpriteRenderer>().enabled = true;
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		state = State.Run;
		AudioManager.Play("level_baroness_jellybean_spawn");
		emitAudioFromObject.Add("level_baroness_jellybean_spawn");
		StartCoroutine(fade_color_cr());
		StartCoroutine(beginning_offset_cr());
		StartCoroutine(move_cr());
	}

	private void KillJelly()
	{
		GetComponent<Collider2D>().enabled = false;
		Die();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health < 0f && state != 0)
		{
			state = State.Dead;
			GetComponent<Collider2D>().enabled = false;
			base.animator.Play((!Rand.Bool()) ? "Jellybean_Death_B" : "Jellybean_Death_A");
		}
	}

	protected virtual float hitPauseCoefficient()
	{
		return (!GetComponent<DamageReceiver>().IsHitPaused) ? 1f : 0f;
	}

	private IEnumerator fade_color_cr()
	{
		Color endColor = GetComponent<SpriteRenderer>().color;
		float fadeTime = 0.2f;
		float t = 0f;
		Color start = new Color(0f, 0f, 0f, 1f);
		while (t < fadeTime)
		{
			GetComponent<SpriteRenderer>().color = Color.Lerp(start, endColor, t / fadeTime);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		GetComponent<SpriteRenderer>().color = endColor;
		yield return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		explosion = null;
	}

	private IEnumerator beginning_offset_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		Vector3 pos = base.transform.position;
		Vector3 startPos = base.transform.position;
		velocity = properties.jumpSpeed;
		pos.y = base.transform.position.y;
		startPos.y = base.transform.position.y + 40f;
		originalPos = pos;
		base.transform.position = startPos;
		while (base.transform.position.y >= pos.y)
		{
			if (state == State.Run)
			{
				base.transform.AddPosition(0f, -100f * CupheadTime.FixedDelta * hitPauseCoefficient());
			}
			yield return wait;
		}
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		state = State.Run;
		float offset = 200f;
		while (base.transform.position.x > -640f - offset)
		{
			if (state != State.Jump)
			{
				Vector3 pos = base.transform.position;
				pos.x += (0f - speed) * CupheadTime.FixedDelta * hitPauseCoefficient();
				base.transform.position = pos;
			}
			yield return wait;
		}
		Die();
	}

	private void StartJump()
	{
		state = State.Jump;
		StartCoroutine(jump_cr());
	}

	private IEnumerator jump_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		velocity = properties.jumpSpeed;
		float decrement = 1f;
		Vector3 pos = base.transform.position;
		bool jumping = true;
		bool landing = false;
		base.animator.Play("Jellybean_Jump_Antic");
		yield return base.animator.WaitForAnimationToEnd(this, "Jellybean_Jump_Antic");
		while (jumping)
		{
			base.transform.AddPosition(0f, velocity * CupheadTime.FixedDelta * hitPauseCoefficient());
			if (base.transform.position.y >= properties.heightDefault + properties.jumpHeight.RandomFloat())
			{
				velocity -= decrement;
				if (!landing)
				{
					velocity = 0f - velocity;
					base.animator.SetTrigger("Land");
					landing = true;
				}
			}
			if (base.transform.position.y <= originalPos.y)
			{
				if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("Jellybean_Jump_Land"))
				{
					yield return base.animator.WaitForAnimationToEnd(this, "Jellybean_Jump_Land");
				}
				jumping = false;
			}
			yield return wait;
		}
		StartCoroutine(timer_cr());
		pos.y = originalPos.y;
		base.transform.position = pos;
		state = State.Run;
		yield return null;
	}

	private IEnumerator timer_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, properties.afterJumpDuration);
	}

	private void DeathComplete()
	{
		explosion.Create(base.transform.position);
		AudioManager.Play("level_baroness_jellybean_death");
		emitAudioFromObject.Add("level_baroness_jellybean_death");
		Object.Destroy(base.gameObject);
	}

	protected override void Die()
	{
		base.Die();
		StopAllCoroutines();
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
