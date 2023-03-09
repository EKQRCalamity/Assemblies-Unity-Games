using System.Collections;
using System.Linq;
using UnityEngine;

public class MouseLevelBrokenCanMouse : LevelProperties.Mouse.Entity
{
	public enum State
	{
		Init,
		Down,
		Up,
		Dying
	}

	public enum Direction
	{
		Left,
		Right
	}

	private const int CART_LAYER = 0;

	private const int SCISSOR_LAYER = 1;

	private const int CANNON_LAYER = 2;

	private const int MOUSE_LAYER = 3;

	private const int FLAME_LAYER = 4;

	private const int CAN_LAYER = 5;

	[SerializeField]
	private MouseLevelFlame leftFlame;

	[SerializeField]
	private MouseLevelFlame rightFlame;

	[SerializeField]
	private SpriteRenderer leftFlameSprite;

	[SerializeField]
	private SpriteRenderer rightFlameSprite;

	[SerializeField]
	private Vector2 finalFlameScale;

	[SerializeField]
	private Transform leftBulletRoot;

	[SerializeField]
	private Transform rightBulletRoot;

	[SerializeField]
	private Transform mouse;

	[SerializeField]
	private Transform platform;

	[SerializeField]
	private BasicProjectile bulletPrefab;

	[SerializeField]
	private MouseLevelSawBladeManager sawBlades;

	[SerializeField]
	private MouseLevelCat cat;

	[SerializeField]
	private MouseLevelCatPeeking catPeeking;

	private Direction direction;

	private DamageReceiver damageReceiver;

	private DamageDealer damageDealer;

	private bool flameOn;

	private Vector2 platformLocalPos;

	private bool dead;

	private bool peeking = true;

	private Collider2D[] colliders;

	private bool overrideMove;

	private float overrideMoveX;

	private const float WHEEL_MOVE_FACTOR = 100f;

	public State state { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		damageDealer = DamageDealer.NewEnemy();
		platformLocalPos = platform.localPosition;
		platform.transform.parent = null;
		setFlameCollidersEnabled(enabled: false);
		colliders = mouse.GetComponents<Collider2D>();
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = false;
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		platform.position = new Vector2(base.transform.position.x, base.transform.position.y) + platformLocalPos;
		if (peeking && base.properties.CurrentHealth < base.properties.TotalHealth * catPeeking.Peek2Threshold)
		{
			catPeeking.StopPeeking();
			peeking = false;
		}
	}

	private void LateUpdate()
	{
		leftFlame.UpdateParentTransform(base.transform);
		rightFlame.UpdateParentTransform(base.transform);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void LevelInit(LevelProperties.Mouse properties)
	{
		base.LevelInit(properties);
		properties.OnBossDeath += OnBossDeath;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	private float hitPauseCoefficient()
	{
		return (!GetComponent<DamageReceiver>().IsHitPaused) ? 1f : 0f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		bulletPrefab = null;
	}

	public void StartPattern(Transform transform)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = true;
		}
		base.transform.position = transform.position;
		base.transform.localScale = transform.localScale;
		base.animator.SetTrigger("Continue");
		if (state != State.Dying)
		{
			state = State.Down;
			StartCoroutine("main_cr");
		}
		StartCoroutine(move_cr());
		direction = ((!(transform.localScale.x > 0f)) ? Direction.Right : Direction.Left);
	}

	private IEnumerator main_cr()
	{
		LevelProperties.Mouse.State patternState = base.properties.CurrentState;
		string[] pattern = patternState.brokenCanFlame.attackString.RandomChoice().Split(',');
		while (!dead)
		{
			if (patternState != base.properties.CurrentState)
			{
				if (!patternState.brokenCanFlame.attackString.SequenceEqual(base.properties.CurrentState.brokenCanFlame.attackString))
				{
					pattern = base.properties.CurrentState.brokenCanFlame.attackString.RandomChoice().Split(',');
				}
				patternState = base.properties.CurrentState;
			}
			LevelProperties.Mouse.BrokenCanFlame p = base.properties.CurrentState.brokenCanFlame;
			string[] array = pattern;
			foreach (string instruction in array)
			{
				state = ((state != State.Down) ? State.Down : State.Up);
				base.animator.SetTrigger((state != State.Down) ? "Up" : "Down");
				yield return base.animator.WaitForAnimationToEnd(this, (state != State.Down) ? "Going_Up" : "Going_Down", 1);
				base.animator.SetTrigger("Continue");
				yield return CupheadTime.WaitForSeconds(this, p.delayBeforeShot);
				if (instruction == "BF")
				{
					base.animator.SetTrigger("Shoot");
					BasicProjectile leftBullet = bulletPrefab.Create(leftBulletRoot.position, (base.transform.localScale.x > 0f) ? 180 : 0, p.shotSpeed);
					leftBullet.SetParryable(parryable: true);
					BasicProjectile rightBullet = bulletPrefab.Create(rightBulletRoot.position, (!(base.transform.localScale.x > 0f)) ? 180 : 0, p.shotSpeed);
					rightBullet.SetParryable(parryable: true);
					yield return base.animator.WaitForAnimationToEnd(this, "Fire", 2);
					yield return CupheadTime.WaitForSeconds(this, p.delayAfterShot);
				}
				StartCoroutine(scale_flames_cr(turningOn: true));
				base.animator.SetTrigger("Flame");
				yield return CupheadTime.WaitForSeconds(this, p.chargeTime);
				base.animator.SetTrigger("FlameContinue");
				setFlameCollidersEnabled(enabled: true);
				flameOn = true;
				yield return CupheadTime.WaitForSeconds(this, p.loopTime);
				setFlameCollidersEnabled(enabled: false);
				StartCoroutine(scale_flames_cr(turningOn: false));
				flameOn = false;
				base.animator.SetTrigger("FlameContinue");
				yield return base.animator.WaitForAnimationToEnd(this, "End", 4);
			}
		}
	}

	private IEnumerator scale_flames_cr(bool turningOn)
	{
		float t = 0f;
		float time = 1f;
		if (turningOn)
		{
			leftFlame.transform.SetScale(1f, 1f, 0f);
			rightFlame.transform.SetScale(1f, 1f, 0f);
			leftFlameSprite.transform.SetScale(finalFlameScale.x, finalFlameScale.y, 0f);
			rightFlameSprite.transform.SetScale(finalFlameScale.x, finalFlameScale.y, 0f);
			leftFlameSprite.enabled = true;
			rightFlameSprite.enabled = true;
		}
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			if (turningOn)
			{
				leftFlame.transform.SetScale(0f - t / time, t / time, 0f);
				rightFlame.transform.SetScale(t / time, t / time, 0f);
				leftFlameSprite.transform.SetScale(t / time * finalFlameScale.x, t / time * finalFlameScale.y, 0f);
				rightFlameSprite.transform.SetScale(t / time * finalFlameScale.x, t / time * finalFlameScale.y, 0f);
			}
			else
			{
				leftFlame.transform.SetScale(-1f + t / time, 1f - t / time, 0f);
				rightFlame.transform.SetScale(1f - t / time, 1f - t / time, 0f);
				leftFlameSprite.transform.SetScale(finalFlameScale.x - t / time * finalFlameScale.x, finalFlameScale.y - t / time * finalFlameScale.y, 0f);
				rightFlameSprite.transform.SetScale(finalFlameScale.x - t / time * finalFlameScale.x, finalFlameScale.y - t / time * finalFlameScale.y, 0f);
			}
			yield return null;
		}
		if (!turningOn)
		{
			leftFlame.transform.SetScale(0f, 0f, 0f);
			rightFlame.transform.SetScale(0f, 0f, 0f);
			leftFlameSprite.transform.SetScale(0f, 0f, 0f);
			rightFlameSprite.transform.SetScale(0f, 0f, 0f);
			leftFlameSprite.enabled = false;
			rightFlameSprite.enabled = false;
		}
		yield return null;
	}

	private void setFlameCollidersEnabled(bool enabled)
	{
		leftFlame.SetColliderEnabled(enabled);
		rightFlame.SetColliderEnabled(enabled);
		if (enabled)
		{
			SoundMouseFlameThrower();
		}
	}

	private IEnumerator moveToX_cr(float x)
	{
		overrideMove = true;
		overrideMoveX = x;
		while (overrideMove)
		{
			yield return null;
		}
	}

	private IEnumerator move_cr()
	{
		Vector2 startPos = base.transform.position;
		bool overridden;
		do
		{
			LevelProperties.Mouse.BrokenCanMove p = base.properties.CurrentState.brokenCanMove;
			Vector2 targetPos = startPos;
			overridden = false;
			if (overrideMove)
			{
				targetPos.x = overrideMoveX;
				overridden = true;
			}
			else if (direction == Direction.Left)
			{
				targetPos.x -= p.maxXPositionRange.RandomFloat();
			}
			else
			{
				targetPos.x += p.maxXPositionRange.RandomFloat();
			}
			yield return StartCoroutine(tween_cr(time: Mathf.Abs(targetPos.x - base.transform.position.x) / p.speed, trans: base.transform, start: base.transform.position, end: targetPos, ease: EaseUtils.EaseType.easeInOutSine));
			yield return CupheadTime.WaitForSeconds(this, 0.25f);
			direction = ((direction == Direction.Left) ? Direction.Right : Direction.Left);
		}
		while (!overrideMove || !overridden);
		overrideMove = false;
		base.animator.Play("Idle", 0);
	}

	private IEnumerator tween_cr(Transform trans, Vector2 start, Vector2 end, EaseUtils.EaseType ease, float time)
	{
		float t = 0f;
		trans.position = start;
		while (t < time)
		{
			float val = EaseUtils.Ease(ease, 0f, 1f, t / time);
			trans.position = Vector2.Lerp(start, end, val);
			t += (float)CupheadTime.Delta * hitPauseCoefficient();
			float wheelAnimProgress2 = (0f - base.transform.localScale.x) * base.transform.position.x / 100f;
			wheelAnimProgress2 %= 1f;
			if (wheelAnimProgress2 < 0f)
			{
				wheelAnimProgress2 += 1f;
			}
			base.animator.Play("Move", 0, wheelAnimProgress2);
			yield return null;
		}
		trans.position = end;
		yield return null;
	}

	public void OnBossDeath()
	{
		Collider2D[] componentsInChildren = mouse.GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren)
		{
			collider2D.enabled = false;
		}
		StopAllCoroutines();
		state = State.Dying;
		StartCoroutine(death_cr(transform: false));
	}

	public void Transform()
	{
		state = State.Dying;
		SoundMouseScreamVoice();
		StartCoroutine(death_cr(transform: true));
		base.properties.OnBossDeath -= OnBossDeath;
	}

	public void BeEaten()
	{
		Object.Destroy(leftFlame.gameObject);
		Object.Destroy(rightFlame.gameObject);
		Object.Destroy(platform.gameObject);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator death_cr(bool transform)
	{
		base.animator.SetTrigger("Die");
		sawBlades.Leave();
		if (transform)
		{
			base.animator.SetTrigger("Down");
		}
		else
		{
			base.animator.SetBool("CrazyScissor", value: true);
		}
		if (flameOn)
		{
			base.animator.SetTrigger("FlameContinue");
		}
		else
		{
			base.animator.ResetTrigger("Flame");
			AudioManager.Play("level_mouse_scream_death_voice");
		}
		leftFlame.SetColliderEnabled(enabled: false);
		rightFlame.SetColliderEnabled(enabled: false);
		StopCoroutine("main_cr");
		if (transform)
		{
			yield return StartCoroutine(moveToX_cr(0f));
			yield return base.animator.WaitForAnimationToStart(this, "Idle_Down", 1);
			cat.StartIntro();
		}
	}

	private void SoundMouseBrkCanScissorUp()
	{
		AudioManager.Play("level_mouse_broken_can_scissor_going_up");
		emitAudioFromObject.Add("level_mouse_broken_can_scissor_going_up");
	}

	private void SoundMouseBrkCanScissorDown()
	{
		AudioManager.Play("level_mouse_broken_can_scissor_going_down");
		emitAudioFromObject.Add("level_mouse_broken_can_scissor_going_down");
	}

	private void SoundMouseBrkCanStartUp()
	{
		AudioManager.Play("level_mouse_broken_can_start_up");
		emitAudioFromObject.Add("level_mouse_broken_can_start_up");
	}

	private void SoundMouseFlameThrower()
	{
		AudioManager.Play("level_mouse_flamethrower");
		emitAudioFromObject.Add("level_mouse_flamethrower");
	}

	private void SoundMouseSnarkyVoice()
	{
		AudioManager.Play("level_mouse_snarky_voice");
		emitAudioFromObject.Add("level_mouse_snarky_voice");
	}

	private void SoundMouseScreamVoice()
	{
		AudioManager.Play("level_mouse_scream_voice");
		emitAudioFromObject.Add("level_mouse_scream_voice");
	}
}
