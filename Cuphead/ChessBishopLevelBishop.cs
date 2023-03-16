using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChessBishopLevelBishop : LevelProperties.ChessBishop.Entity
{
	private enum DisappearingState
	{
		None,
		Disappearing,
		Reappearing
	}

	private const int MULTIPLAYER_CANDLE_DIFFERENCE = 3;

	private const float VER_LOOPSIZEY = 150f;

	private const float VER_LOOPSIZEX = 500f;

	private const float H_EASE_DISTANCE = 100f;

	[SerializeField]
	private ChessBishopLevelBell bellProjectile;

	[SerializeField]
	private SpriteRenderer mainRenderer;

	[SerializeField]
	private SpriteRenderer summonOverlayRenderer;

	[SerializeField]
	private Transform projectileSpawnPoint;

	[SerializeField]
	private Transform pivotPoint;

	[SerializeField]
	private GameObject candlesHolder;

	[SerializeField]
	private ChessBishopLevelCandle[] candles;

	[SerializeField]
	private Animator bodyAnimator;

	[SerializeField]
	private Effect bodyExplosion;

	[SerializeField]
	private Transform bodyExplosionSpawnPoint;

	[SerializeField]
	private SpriteRenderer bodyRenderer;

	private float bodyOpacity = 1f;

	[SerializeField]
	private float fadeRate = 0.75f;

	[SerializeField]
	private GameObject[] playerMask;

	private int candleOrderMainIndex;

	private bool invert;

	private bool isPathTwo;

	private DamageDealer damageDealer;

	private PatternString invisibleTime;

	private bool canMove;

	private bool isFirstPhase = true;

	private bool stateDidChange;

	private DisappearingState disappearingState;

	private bool dead;

	private bool introPlaying = true;

	private Coroutine bulletSpawnCoroutine;

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		for (int i = 0; i < candles.Length; i++)
		{
			candles[i].Init(base.properties.CurrentState.candle.candleDistToBlowout);
		}
	}

	public override void LevelInit(LevelProperties.ChessBishop properties)
	{
		base.LevelInit(properties);
		Level.Current.OnIntroEvent += onIntroEventHandler;
		setupPatternStrings();
	}

	private void UpdateBodyFade()
	{
		float num = Mathf.Clamp(bodyOpacity, 0f, 1f);
		bodyRenderer.color = new Color(1f, 1f, 1f, num);
		bodyRenderer.material.SetFloat("_BlurAmount", (1f - num) * 5f);
		bodyRenderer.material.SetFloat("_BlurLerp", (1f - num) * 5f);
	}

	private void FixedUpdate()
	{
		if ((bool)PlayerManager.GetPlayer(PlayerId.PlayerOne) && !PlayerManager.GetPlayer(PlayerId.PlayerOne).IsDead)
		{
			playerMask[0].transform.position = PlayerManager.GetPlayer(PlayerId.PlayerOne).transform.position + Vector3.up * 50f;
		}
		if ((bool)PlayerManager.GetPlayer(PlayerId.PlayerTwo) && !PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead)
		{
			playerMask[1].transform.position = PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position + Vector3.up * 50f;
		}
		if (!introPlaying && !dead)
		{
			bodyOpacity -= CupheadTime.FixedDelta * fadeRate;
			UpdateBodyFade();
			if (damageDealer != null)
			{
				damageDealer.FixedUpdate();
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		damageDealer.DealDamage(hit);
	}

	public void StartNewPhase()
	{
		stateDidChange = true;
		cancelShoot();
		StopAllCoroutines();
		candleOrderMainIndex %= base.properties.CurrentState.candle.candleOrder.Length;
		setupPatternStrings();
		StartCoroutine(disappear_cr());
	}

	public override void OnParry(AbstractPlayerController player)
	{
		base.OnParry(player);
		cancelShoot();
		base.properties.DealDamage((!PlayerManager.BothPlayersActive()) ? 10f : ChessKingLevelKing.multiplayerDamageNerf);
		if (base.properties.CurrentHealth <= 0f)
		{
			die();
			return;
		}
		bodyOpacity = 1.75f;
		bodyAnimator.SetTrigger("Hit");
		bodyExplosion.Create(bodyExplosionSpawnPoint.position);
		turnDormant(stateDidChange);
		stateDidChange = false;
	}

	private void onIntroEventHandler()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		bodyAnimator.SetTrigger("StartIntro");
		yield return bodyAnimator.WaitForAnimationToEnd(this, "Intro.End");
		isPathTwo = MathUtils.RandomBool();
		startPath();
		yield return CupheadTime.WaitForSeconds(this, 0.55f);
		base.animator.SetBool("CanParry", value: true);
		base.animator.SetTrigger("Appear");
		yield return base.animator.WaitForAnimationToEnd(this, "AppearActive");
		candleOrderMainIndex = UnityEngine.Random.Range(0, base.properties.CurrentState.candle.candleOrder.Length);
		candlesHolder.SetActive(value: true);
		canMove = true;
		introPlaying = false;
	}

	private void turnDormant(bool willDisappear)
	{
		_canParry = false;
		if (base.properties.CurrentHealth > 0f)
		{
			StartCoroutine(candles_cr());
		}
		if (!willDisappear)
		{
			base.animator.SetTrigger("ToDormant");
			StartCoroutine(postHitToggleCollider_cr());
		}
	}

	private IEnumerator postHitToggleCollider_cr()
	{
		GetComponent<Collider2D>().enabled = false;
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.bishop.colliderOffTime);
		GetComponent<Collider2D>().enabled = true;
	}

	private IEnumerator candles_cr()
	{
		while (disappearingState == DisappearingState.Disappearing)
		{
			yield return null;
		}
		LevelProperties.ChessBishop.Candle p = base.properties.CurrentState.candle;
		string[] candleOrder = p.candleOrder[candleOrderMainIndex].Split(',');
		int length = candleOrder.Length + (PlayerManager.BothPlayersActive() ? 3 : 0);
		ChessBishopLevelCandle[] activeCandles = new ChessBishopLevelCandle[length];
		int index2 = 0;
		for (int i = 0; i < candleOrder.Length; i++)
		{
			Parser.IntTryParse(candleOrder[i], out index2);
			candles[index2].LightUp();
			activeCandles[i] = candles[index2];
		}
		if (PlayerManager.BothPlayersActive())
		{
			List<ChessBishopLevelCandle> list = candles.Where((ChessBishopLevelCandle c) => !c.isLit).ToList();
			for (int j = 0; j < 3; j++)
			{
				if (list.Count > 0)
				{
					index2 = UnityEngine.Random.Range(0, list.Count);
					list[index2].LightUp();
					activeCandles[candleOrder.Length + j] = list[index2];
					list.RemoveAt(index2);
				}
			}
		}
		candleOrderMainIndex = (candleOrderMainIndex + 1) % p.candleOrder.Length;
		yield return null;
		bool candlesStillLit = true;
		while (candlesStillLit)
		{
			candlesStillLit = false;
			for (int k = 0; k < activeCandles.Length; k++)
			{
				if (activeCandles[k] != null && activeCandles[k].isLit)
				{
					candlesStillLit = true;
					break;
				}
			}
			yield return null;
		}
		cancelShoot();
		if (disappearingState == DisappearingState.Disappearing)
		{
			_canParry = true;
			yield break;
		}
		while (disappearingState == DisappearingState.Reappearing)
		{
			yield return null;
		}
		_canParry = true;
		base.animator.SetTrigger("ToActive");
	}

	private IEnumerator disappear_cr()
	{
		disappearingState = DisappearingState.Disappearing;
		isFirstPhase = false;
		canMove = false;
		Collider2D collider = GetComponent<Collider2D>();
		collider.enabled = false;
		base.animator.SetTrigger("HitDisappear");
		yield return base.animator.WaitForAnimationToEnd(this, "HitDisappear");
		yield return CupheadTime.WaitForSeconds(this, invisibleTime.PopFloat());
		disappearingState = DisappearingState.Reappearing;
		isPathTwo = !isPathTwo;
		startPath();
		base.animator.SetBool("CanParry", _canParry);
		base.animator.SetTrigger("Appear");
		yield return AnimatorExtensions.WaitForAnimationToEnd(name: (!base.canParry) ? "AppearDormant" : "AppearActive", animator: base.animator, parent: this);
		canMove = true;
		collider.enabled = true;
		disappearingState = DisappearingState.None;
	}

	private void startPath()
	{
		if (isPathTwo)
		{
			StartCoroutine(moveHorizontal_cr());
		}
		else
		{
			StartCoroutine(moveVertical_cr());
		}
	}

	private IEnumerator moveVertical_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		Vector3 pivotOffset = Vector3.up * 2f * 150f;
		invert = true;
		float value = -1f;
		float speed = base.properties.CurrentState.bishop.movementSpeed;
		float angle = 3.9269907f;
		if (!isFirstPhase)
		{
			float minimumDistance = GetComponent<CircleCollider2D>().radius * 1.5f;
			angle = findMoveVerticalInitialAngle(minimumDistance, value, invert, speed, pivotPoint, pivotOffset);
		}
		StartCoroutine(spawnProjectiles_cr());
		while (true)
		{
			base.transform.position = calculateMoveVerticalPosition(ref angle, ref value, ref invert, speed, pivotPoint, pivotOffset);
			while (!canMove)
			{
				yield return null;
			}
			yield return wait;
		}
	}

	private static float findMoveVerticalInitialAngle(float minimumDistance, float value, bool invert, float speed, Transform pivotPoint, Vector3 pivotOffset)
	{
		float num = minimumDistance * minimumDistance;
		List<Vector3> list = new List<Vector3>();
		if (PlayerManager.DoesPlayerExist(PlayerId.PlayerOne))
		{
			list.Add(PlayerManager.GetPlayer(PlayerId.PlayerOne).center);
		}
		if (PlayerManager.DoesPlayerExist(PlayerId.PlayerTwo))
		{
			list.Add(PlayerManager.GetPlayer(PlayerId.PlayerTwo).center);
		}
		int num2 = 0;
		float num3 = 0f;
		while (num2 < 20)
		{
			num2++;
			num3 = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
			float angle = num3;
			float value2 = value;
			bool flag = invert;
			Vector3 vector = calculateMoveVerticalPosition(ref angle, ref value2, ref flag, speed, pivotPoint, pivotOffset);
			bool flag2 = false;
			foreach (Vector3 item in list)
			{
				if ((vector - item).sqrMagnitude < num)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		return num3;
	}

	private static Vector3 calculateMoveVerticalPosition(ref float angle, ref float value, ref bool invert, float speed, Transform pivotPoint, Vector3 pivotOffset)
	{
		angle += speed * CupheadTime.FixedDelta;
		if (angle > (float)Math.PI * 2f)
		{
			invert = !invert;
			angle -= (float)Math.PI * 2f;
		}
		if (angle < 0f)
		{
			angle += (float)Math.PI * 2f;
		}
		Vector3 vector;
		if (invert)
		{
			vector = pivotPoint.position + pivotOffset;
			value = -1f;
		}
		else
		{
			vector = pivotPoint.position;
			value = 1f;
		}
		Vector3 vector2 = new Vector3((0f - Mathf.Sin(angle)) * 500f, Mathf.Cos(angle) * value * 150f, 0f);
		return vector + vector2;
	}

	private IEnumerator moveHorizontal_cr()
	{
		LevelProperties.ChessBishop.Bishop p = base.properties.CurrentState.bishop;
		YieldInstruction wait = new WaitForFixedUpdate();
		invert = true;
		float xSpeed = p.xSpeed;
		float amplitude = p.amplitude;
		float frequency = p.freqMultiplier * 2f * (float)Math.PI / (p.maxDistance * 2f);
		if (isFirstPhase)
		{
			base.transform.position = new Vector3(500f, base.transform.position.y);
		}
		else
		{
			float minimumDistance = GetComponent<CircleCollider2D>().radius * 1.5f;
			base.transform.position = findMoveHorizontalInitialPosition(minimumDistance, base.transform.position.y, p.maxDistance, xSpeed, amplitude, frequency);
		}
		StartCoroutine(spawnProjectiles_cr());
		Vector3 goalPos = base.transform.position;
		float distanceTraveled = (float)Math.PI / 2f;
		while (true)
		{
			base.transform.position = calculateMoveHorizontalPosition(ref goalPos, ref xSpeed, ref distanceTraveled, amplitude, frequency, p.maxDistance);
			while (!canMove)
			{
				yield return null;
			}
			yield return wait;
		}
	}

	private static Vector3 findMoveHorizontalInitialPosition(float minimumDistance, float yPosition, float maxDistance, float xSpeed, float amplitude, float frequency)
	{
		float num = minimumDistance * minimumDistance;
		List<Vector3> list = new List<Vector3>();
		if (PlayerManager.DoesPlayerExist(PlayerId.PlayerOne))
		{
			list.Add(PlayerManager.GetPlayer(PlayerId.PlayerOne).center);
		}
		if (PlayerManager.DoesPlayerExist(PlayerId.PlayerTwo))
		{
			list.Add(PlayerManager.GetPlayer(PlayerId.PlayerTwo).center);
		}
		int num2 = 0;
		Vector3 vector = Vector3.zero;
		while (num2 < 20)
		{
			num2++;
			vector = new Vector3(UnityEngine.Random.Range(0f - maxDistance, maxDistance), yPosition);
			Vector3 goalPosition = vector;
			float distanceTravelled = (float)Math.PI / 2f;
			float xSpeed2 = xSpeed;
			calculateMoveHorizontalPosition(ref goalPosition, ref xSpeed2, ref distanceTravelled, amplitude, frequency, maxDistance);
			bool flag = false;
			foreach (Vector3 item in list)
			{
				if ((vector - item).sqrMagnitude < num)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return vector;
	}

	private static Vector3 calculateMoveHorizontalPosition(ref Vector3 goalPosition, ref float xSpeed, ref float distanceTravelled, float amplitude, float frequency, float maxDistance)
	{
		Vector3 vector = goalPosition;
		vector.x += xSpeed * CupheadTime.FixedDelta;
		distanceTravelled += Mathf.Abs(xSpeed) * CupheadTime.FixedDelta;
		if (vector.x > maxDistance || vector.x < 0f - maxDistance)
		{
			xSpeed *= -1f;
		}
		vector.y = amplitude * Mathf.Sin(frequency * distanceTravelled);
		goalPosition = vector;
		if (vector.x < 0f - maxDistance + 100f || vector.x > maxDistance - 100f)
		{
			float num = Mathf.InverseLerp(maxDistance - 100f, maxDistance, Mathf.Abs(vector.x));
			num *= (float)Math.PI / 2f;
			num = Mathf.Sin(num) * 100f / 2f;
			vector.x = (maxDistance - 100f + num) * Mathf.Sign(vector.x);
		}
		return vector;
	}

	private IEnumerator spawnProjectiles_cr()
	{
		while (!canMove)
		{
			yield return null;
		}
		LevelProperties.ChessBishop.Bishop p = base.properties.CurrentState.bishop;
		PatternString delayPattern = new PatternString(p.attackDelayString);
		while (true)
		{
			float delay = delayPattern.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delay);
			bulletSpawnCoroutine = StartCoroutine(shoot_cr());
			while (bulletSpawnCoroutine != null)
			{
				yield return null;
			}
		}
	}

	private IEnumerator shoot_cr()
	{
		float previousTime = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		float currentTime = previousTime;
		while (!(previousTime < 0.625f) || !(currentTime > 0.625f))
		{
			yield return null;
			previousTime = currentTime;
			currentTime = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		}
		if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("IdleActive") || base.animator.GetCurrentAnimatorStateInfo(0).IsName("IsDormant"))
		{
			mainRenderer.enabled = false;
		}
		summonOverlayRenderer.enabled = true;
		float num;
		currentTime = (num = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
		previousTime = num;
		while (previousTime <= currentTime)
		{
			yield return null;
			previousTime = currentTime;
			currentTime = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		}
		ChessBishopLevelBell bell = bellProjectile.Spawn();
		SFX_KOG_Bishop_Shoot();
		bell.Init(projectileSpawnPoint.position, PlayerManager.GetNext(), base.properties.CurrentState.bishop);
		currentTime = (num = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
		previousTime = num;
		while (!(previousTime < 0.525f) || !(currentTime > 0.525f))
		{
			yield return null;
			previousTime = currentTime;
			currentTime = MathUtilities.DecimalPart(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		}
		mainRenderer.enabled = true;
		summonOverlayRenderer.enabled = false;
		bulletSpawnCoroutine = null;
	}

	private void cancelShoot()
	{
		if (bulletSpawnCoroutine != null)
		{
			StopCoroutine(bulletSpawnCoroutine);
			bulletSpawnCoroutine = null;
		}
		mainRenderer.enabled = true;
		summonOverlayRenderer.enabled = false;
	}

	private void die()
	{
		if (!dead)
		{
			dead = true;
			bodyOpacity = 1f;
			UpdateBodyFade();
			StopAllCoroutines();
			GetComponent<Collider2D>().enabled = false;
			SFX_KOG_Bishop_Death();
			bodyAnimator.Play("Death");
			base.animator.Play("Death");
			base.animator.Update(0f);
		}
	}

	private void setupPatternStrings()
	{
		invisibleTime = new PatternString(base.properties.CurrentState.bishop.invisibleTimeString);
	}

	private void AnimationEvent_SFX_KOG_Bishop_Wakeup()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_wakeup");
	}

	private void AnimationEvent_SFX_KOG_Bishop_HeadDisappearsFromBody()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_headdisappearsfrombody");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_headdisappearsfrombody");
	}

	private void AnimationEvent_SFX_KOG_Bishop_HeadReappears()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_headreappears");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_headreappears");
	}

	private void SFX_KOG_Bishop_Shoot()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_shoot");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_shoot");
	}

	private void SFX_KOG_Bishop_Death()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_death");
		AudioManager.Play("sfx_level_knockout_boom");
	}

	private void AnimationEvent_SFX_KOG_Bishop_Vocal()
	{
		StartCoroutine(SFX_KOG_Bihop_Vocal_cr());
	}

	private IEnumerator SFX_KOG_Bihop_Vocal_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.Play("sfx_dlc_kog_bishop_vocal");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_vocal");
	}
}
