using System.Collections;
using UnityEngine;

public class DicePalaceFlyingMemoryLevelStuffedToy : LevelProperties.DicePalaceFlyingMemory.Entity
{
	public enum State
	{
		Open,
		Closed
	}

	[SerializeField]
	private Transform projectileRoot;

	[SerializeField]
	private DicePalaceFlyingMemoryMusicNote projectile;

	[SerializeField]
	private DicePalaceFlyingMemoryLevelSpiralProjectile spiralProjectile;

	[SerializeField]
	private GameObject sprite;

	[SerializeField]
	private SpriteRenderer hand;

	public bool guessedWrong;

	public State state;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private Vector3 velocity;

	private int bounceCounter;

	private int shotIndex;

	private float speed;

	private float newAngle;

	private float currentAngle;

	private float maxCount;

	private float timer;

	private bool isMoving;

	private bool startedPunishment;

	public bool currentlyColliding;

	private string[] shotPattern;

	private bool VOXAngryActive;

	private bool SFXAnticipationActive;

	protected override void Awake()
	{
		base.Awake();
		state = State.Closed;
		GetComponent<DamageReceiver>().enabled = false;
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	public override void LevelInit(LevelProperties.DicePalaceFlyingMemory properties)
	{
		base.LevelInit(properties);
		shotPattern = properties.CurrentState.stuffedToy.shotType.GetRandom().Split(',');
		shotIndex = 0;
		Level.Current.OnWinEvent += OnDeath;
		StartCoroutine(intro_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (base.properties.CurrentHealth > 0f)
		{
			base.properties.DealDamage(info.damage);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		projectile = null;
		spiralProjectile = null;
	}

	private IEnumerator intro_cr()
	{
		float t = 0f;
		float time = 1.5f;
		Vector3 end = new Vector3(base.transform.position.x, 0f);
		Vector3 start = base.transform.position;
		while (t < time)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t / time);
			base.transform.position = Vector2.Lerp(start, end, val);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.transform.position = end;
		base.animator.SetTrigger("Continue");
		AudioManager.Play("dice_palace_memory_monkey_intro");
		emitAudioFromObject.Add("dice_palace_memory_monkey_intro");
		yield return base.animator.WaitForAnimationToEnd(this, "Intro");
		StartCoroutine(check_boundaries_cr());
		StartCoroutine(pick_angle_cr());
		yield return null;
	}

	private void FireSingle(float speed)
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		Vector3 vector = next.transform.position - base.transform.position;
		float rotation = MathUtils.DirectionToAngle(vector);
		projectile.Create(projectileRoot.transform.position, rotation, speed);
	}

	private void FireSpreadshot()
	{
		LevelProperties.DicePalaceFlyingMemory.StuffedToy stuffedToy = base.properties.CurrentState.stuffedToy;
		for (int i = 0; i < stuffedToy.spreadBullets; i++)
		{
			float floatAt = stuffedToy.spreadAngle.GetFloatAt((float)i / ((float)stuffedToy.spreadBullets - 1f));
			projectile.Create(projectileRoot.transform.position, floatAt, stuffedToy.spreadSpeed, stuffedToy.musicDeathTimer);
		}
	}

	private void FireSpiral()
	{
		LevelProperties.DicePalaceFlyingMemory.StuffedToy stuffedToy = base.properties.CurrentState.stuffedToy;
		spiralProjectile.Create(projectileRoot.transform.position, 0f, stuffedToy.spiralSpeed, stuffedToy.spiralMovementRate, 1);
	}

	private IEnumerator punishment_cr()
	{
		timer = 0f;
		LevelProperties.DicePalaceFlyingMemory.StuffedToy p = base.properties.CurrentState.stuffedToy;
		bool speedUp2 = true;
		startedPunishment = true;
		base.animator.SetTrigger("OnNoMatch");
		AudioManager.PlayLoop("dice_palace_memory_monkey_shake");
		emitAudioFromObject.Add("dice_palace_memory_monkey_shake");
		while (speedUp2)
		{
			if (speed < p.punishSpeed)
			{
				speed += p.incrementSpeedBy;
				yield return null;
				continue;
			}
			speedUp2 = false;
			break;
		}
		speed = p.punishSpeed;
		while (timer < p.punishTime && state == State.Closed)
		{
			timer += CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetTrigger("Continue");
		while (speed > p.bounceSpeed)
		{
			speed -= p.incrementSpeedBy;
			yield return null;
		}
		AudioManager.Stop("dice_palace_memory_monkey_shake");
		speed = p.bounceSpeed;
		startedPunishment = false;
		SFXAllowAnticipation();
		yield return null;
	}

	private IEnumerator pick_angle_cr()
	{
		LevelProperties.DicePalaceFlyingMemory.StuffedToy p = base.properties.CurrentState.stuffedToy;
		string[] angleString = p.angleString.GetRandom().Split(',');
		string[] countString = p.bounceCount.GetRandom().Split(',');
		string[] angleAddString = p.angleAdditionString.GetRandom().Split(',');
		int angleIndex = Random.Range(0, angleString.Length);
		int maxCountIndex = Random.Range(0, countString.Length);
		int angleAddIndex = Random.Range(0, angleAddString.Length);
		float chosenAngle2 = 0f;
		float angle = 0f;
		float angleAdd = 0f;
		float t = 0f;
		float dirChangeTime = p.directionChangeDelay;
		Parser.FloatTryParse(angleString[angleIndex], out angle);
		Parser.FloatTryParse(countString[maxCountIndex], out maxCount);
		Parser.FloatTryParse(angleAddString[angleAddIndex], out angleAdd);
		maxCount = 0f;
		currentAngle = angle;
		base.transform.SetEulerAngles(0f, 0f, angle);
		sprite.transform.SetEulerAngles(0f, 0f, 0f);
		StartCoroutine(move_cr());
		yield return null;
		while (true)
		{
			if (((float)bounceCounter >= maxCount && state == State.Closed) || guessedWrong)
			{
				if (guessedWrong)
				{
					if (!startedPunishment)
					{
						StartCoroutine(punishment_cr());
					}
					else
					{
						timer = 0f;
					}
					while (currentlyColliding)
					{
						yield return new WaitForEndOfFrame();
					}
					isMoving = false;
					while (t < dirChangeTime)
					{
						t += CupheadTime.FixedDelta;
						yield return new WaitForFixedUpdate();
					}
					isMoving = true;
					while (currentlyColliding)
					{
						yield return new WaitForEndOfFrame();
					}
					angleIndex = (angleIndex + 1) % angleString.Length;
					Parser.FloatTryParse(angleString[angleIndex], out angle);
					t = 0f;
				}
				angleAddIndex = (angleAddIndex + 1) % angleAddString.Length;
				maxCountIndex = (maxCountIndex + 1) % countString.Length;
				Parser.FloatTryParse(countString[maxCountIndex], out maxCount);
				Parser.FloatTryParse(angleAddString[angleAddIndex], out angleAdd);
				chosenAngle2 = (guessedWrong ? angle : (currentAngle + angleAdd));
				bounceCounter = 0;
				guessedWrong = false;
				base.transform.SetEulerAngles(0f, 0f, chosenAngle2);
				sprite.transform.SetEulerAngles(0f, 0f, 0f);
				velocity = base.transform.right;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	public void Open()
	{
		state = State.Open;
		base.animator.SetTrigger("OnMatch");
		base.animator.SetBool("OnClosing", value: false);
		StartCoroutine(open_cr());
	}

	private IEnumerator open_cr()
	{
		LevelProperties.DicePalaceFlyingMemory.StuffedToy p = base.properties.CurrentState.stuffedToy;
		int shot = 0;
		AudioManager.Stop("dice_palace_memory_monkey_shake");
		yield return base.animator.WaitForAnimationToStart(this, "Open_Attack_A");
		isMoving = false;
		GetComponent<DamageReceiver>().enabled = true;
		while (currentlyColliding)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, p.directionChangeDelay);
		yield return CupheadTime.WaitForSeconds(this, p.attackAnti);
		isMoving = true;
		base.animator.SetTrigger("Continue");
		while (state == State.Open)
		{
			Parser.IntTryParse(shotPattern[shotIndex], out shot);
			switch (shot)
			{
			case 1:
				FireSingle(p.regularSpeed);
				break;
			case 2:
				FireSpreadshot();
				break;
			case 3:
				FireSpiral();
				break;
			}
			yield return null;
			shotIndex = (shotIndex + 1) % shotPattern.Length;
			yield return CupheadTime.WaitForSeconds(this, p.shotDelayRange);
			if (state == State.Open)
			{
				base.animator.SetTrigger("OnAttack");
				yield return base.animator.WaitForAnimationToStart(this, "Open_Attack_B");
				isMoving = false;
				yield return null;
				yield return CupheadTime.WaitForSeconds(this, p.attackAnti);
				base.animator.SetTrigger("Continue");
				isMoving = true;
				continue;
			}
			break;
		}
		yield return null;
	}

	public void Closed()
	{
		StartCoroutine(closed_cr());
	}

	private IEnumerator closed_cr()
	{
		state = State.Closed;
		base.animator.SetBool("OnClosing", value: true);
		yield return base.animator.WaitForAnimationToStart(this, "Idle_Closed");
		GetComponent<DamageReceiver>().enabled = false;
		yield return null;
	}

	private void DisableDamageReceiver()
	{
		GetComponent<DamageReceiver>().enabled = false;
	}

	private void ChangeLayer(int layer)
	{
		hand.GetComponent<SpriteRenderer>().sortingOrder = layer;
	}

	protected IEnumerator move_cr()
	{
		bool soundLooping = true;
		isMoving = true;
		velocity = base.transform.right;
		speed = base.properties.CurrentState.stuffedToy.bounceSpeed;
		AudioManager.Stop("dice_palace_memory_monkey_shake");
		AudioManager.PlayLoop("dice_palace_memory_monkey_crane_movement");
		emitAudioFromObject.Add("dice_palace_memory_monkey_crane_movement");
		while (true)
		{
			if (isMoving)
			{
				if (!soundLooping)
				{
					AudioManager.PlayLoop("dice_palace_memory_monkey_crane_movement");
					emitAudioFromObject.Add("dice_palace_memory_monkey_crane_movement");
					soundLooping = true;
				}
				base.transform.position += base.transform.right * speed * CupheadTime.FixedDelta;
			}
			else if (soundLooping)
			{
				AudioManager.Stop("dice_palace_memory_monkey_crane_movement");
				soundLooping = false;
				SFXAnticipationActive = false;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	protected override void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionCeiling(hit, phase);
		if (phase == CollisionPhase.Enter || phase == CollisionPhase.Stay)
		{
			currentlyColliding = true;
		}
		if (phase == CollisionPhase.Exit)
		{
			currentlyColliding = false;
		}
		if (currentlyColliding)
		{
			Vector3 newVelocity = velocity;
			newVelocity.y = Mathf.Min(newVelocity.y, 0f - newVelocity.y);
			ChangeDir(newVelocity);
		}
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionGround(hit, phase);
		if (phase == CollisionPhase.Enter || phase == CollisionPhase.Stay)
		{
			currentlyColliding = true;
		}
		if (phase == CollisionPhase.Exit)
		{
			currentlyColliding = false;
		}
		if (currentlyColliding)
		{
			Vector3 newVelocity = velocity;
			newVelocity.y = Mathf.Max(newVelocity.y, 0f - newVelocity.y);
			ChangeDir(newVelocity);
		}
	}

	protected override void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionWalls(hit, phase);
		if (phase == CollisionPhase.Enter || phase == CollisionPhase.Stay)
		{
			currentlyColliding = true;
		}
		if (phase == CollisionPhase.Exit)
		{
			currentlyColliding = false;
		}
		if (currentlyColliding)
		{
			Vector3 newVelocity = velocity;
			if (base.transform.position.x > 0f)
			{
				newVelocity.x = Mathf.Min(newVelocity.x, 0f - newVelocity.x);
				ChangeDir(newVelocity);
			}
			else
			{
				newVelocity.x = Mathf.Max(newVelocity.x, 0f - newVelocity.x);
				ChangeDir(newVelocity);
			}
		}
	}

	protected void ChangeDir(Vector3 newVelocity)
	{
		if (state == State.Closed)
		{
			bounceCounter++;
		}
		velocity = newVelocity;
		currentAngle = Mathf.Atan2(velocity.y, velocity.x) * 57.29578f;
		base.transform.SetEulerAngles(0f, 0f, currentAngle);
		sprite.transform.SetEulerAngles(0f, 0f, 0f);
	}

	private void OnDeath()
	{
		StopAllCoroutines();
		AudioManager.PlayLoop("dice_palace_memory_monkey_death");
		emitAudioFromObject.Add("dice_palace_memory_monkey_death");
		base.animator.SetTrigger("OnDeath");
		GetComponent<Collider2D>().enabled = false;
	}

	private IEnumerator check_boundaries_cr()
	{
		while (!(base.transform.position.y > 720f) && !(base.transform.position.y < -720f) && !(base.transform.position.x < -1280f) && !(base.transform.position.x > 1280f))
		{
			yield return null;
		}
		base.properties.DealDamage(base.properties.CurrentHealth);
	}

	private void AttackSFX()
	{
		AudioManager.Play("dice_palace_memory_monkey_open_attack");
		emitAudioFromObject.Add("dice_palace_memory_monkey_open_attack");
		VOXAngryActive = false;
	}

	private void AttackEndSFX()
	{
		AudioManager.Play("dice_palace_memory_monkey_attack_end");
		emitAudioFromObject.Add("dice_palace_memory_monkey_attack_end");
	}

	private void SFXOpentoClose()
	{
		AudioManager.Play("dice_palace_memory_monkey_open_to_close");
		emitAudioFromObject.Add("dice_palace_memory_monkey_open_to_close");
	}

	private void SFXShake()
	{
		AudioManager.PlayLoop("shake_sound");
		emitAudioFromObject.Add("shake_sound");
	}

	private void SFXShakeStop()
	{
		AudioManager.Stop("shake_sound");
	}

	private void SFXVOXAngry()
	{
		if (!VOXAngryActive)
		{
			AudioManager.Play("vox_angry");
			emitAudioFromObject.Add("vox_angry");
			VOXAngryActive = true;
		}
	}

	private void SFXVOXAngryAnim()
	{
		AudioManager.Play("vox_angry");
		emitAudioFromObject.Add("vox_angry");
	}

	private void SFXVOXAnticipation()
	{
		if (!SFXAnticipationActive)
		{
			AudioManager.Play("vox_anticipation");
			emitAudioFromObject.Add("vox_anticipation");
			SFXAnticipationActive = true;
		}
	}

	private void SFXAllowAnticipation()
	{
		SFXAnticipationActive = false;
	}
}
