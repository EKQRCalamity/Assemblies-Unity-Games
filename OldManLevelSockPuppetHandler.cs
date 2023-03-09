using System;
using System.Collections;
using UnityEngine;

public class OldManLevelSockPuppetHandler : LevelProperties.OldMan.Entity
{
	public enum TransitionState
	{
		None,
		PlatformDestroyed,
		InStomach
	}

	private const float POST_DEATH_PRE_MOVE_TIME = 2f;

	[SerializeField]
	private MinMax idleHoldRange = new MinMax(0.01f, 0.1f);

	[SerializeField]
	private MinMax lookHoldRange = new MinMax(0.75f, 1.5f);

	[SerializeField]
	private float chanceToSwitchLookSides = 0.75f;

	[SerializeField]
	private float chanceToLaugh = 0.25f;

	public TransitionState transState;

	[SerializeField]
	private Transform oldManAngry;

	[SerializeField]
	private Transform oldManAngryNoseShadow;

	[SerializeField]
	private Collider2D mainPlatformCollider;

	[SerializeField]
	private OldManLevelPuppetBall puppetBallPrefab;

	[SerializeField]
	private OldManLevelSockPuppet sockPuppetLeft;

	[SerializeField]
	private OldManLevelSockPuppet sockPuppetRight;

	[SerializeField]
	private OldManLevelPlatformManager platformManager;

	[SerializeField]
	private Transform[] KDpuppetYPositions;

	[SerializeField]
	private Transform[] DpuppetYPositions;

	[SerializeField]
	private OldManLevelDwarf[] dwarves;

	[SerializeField]
	private GameObject dwarvesObject;

	[SerializeField]
	private GameObject handsParent;

	[SerializeField]
	private GameObject BGParent;

	[SerializeField]
	private GameObject beardObject;

	[SerializeField]
	private GameObject rocksUnderBeardObject;

	private DamageDealer damageDealer;

	public Action OnDeathEvent;

	private OldManLevelPuppetBall puppetBall;

	private int leftHandPos = 1;

	private int rightHandPos = 1;

	private int rightHandPosOld;

	private int leftHandPosOld;

	private bool triggerLaugh;

	private bool fromLeft;

	private void Start()
	{
		transState = TransitionState.None;
		dwarvesObject.gameObject.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public void StartPhase2()
	{
		dwarvesObject.gameObject.SetActive(value: true);
		sockPuppetLeft.gameObject.SetActive(value: true);
		sockPuppetRight.gameObject.SetActive(value: true);
		damageDealer = DamageDealer.NewEnemy();
		sockPuppetRight.GetComponent<CollisionChild>().OnPlayerCollision += OnCollisionPlayer;
		sockPuppetLeft.GetComponent<CollisionChild>().OnPlayerCollision += OnCollisionPlayer;
		sockPuppetRight.GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
		sockPuppetLeft.GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
		StartCoroutine(bounce_ball_cr());
		StartCoroutine(dwarves_arc_cr());
		((LevelPlayerController)PlayerManager.GetPlayer(PlayerId.PlayerOne)).motor.OnHitEvent += TriggerLaugh;
		if (PlayerManager.Multiplayer)
		{
			((LevelPlayerController)PlayerManager.GetPlayer(PlayerId.PlayerTwo)).motor.OnHitEvent += TriggerLaugh;
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public override void LevelInit(LevelProperties.OldMan properties)
	{
		base.LevelInit(properties);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator dwarves_arc_cr()
	{
		LevelProperties.OldMan.Dwarf p = base.properties.CurrentState.dwarf;
		PatternString arcAttackDelay = new PatternString(p.arcAttackDelayString);
		PatternString arcAttackPos = new PatternString(p.arcAttackPosString);
		PatternString arcShootHeight = new PatternString(p.arcShootHeightString);
		PatternString parryableString = new PatternString(p.parryString);
		int posIndex2 = 0;
		bool typeA = Rand.Bool();
		yield return CupheadTime.WaitForSeconds(this, 2f);
		while (true)
		{
			posIndex2 = arcAttackPos.PopInt();
			if (dwarves[posIndex2].inPlace)
			{
				dwarves[posIndex2].ShootInArc(arcShootHeight.PopFloat(), p.arcApex, p.arcHealth, typeA, parryableString.PopLetter() == 'P', p.arcAttackWarningTime);
				typeA = !typeA;
				yield return CupheadTime.WaitForSeconds(this, arcAttackDelay.PopFloat());
			}
			yield return null;
		}
	}

	private IEnumerator bounce_ball_cr()
	{
		LevelProperties.OldMan.Hands p = base.properties.CurrentState.hands;
		PatternString leftHandPosString = new PatternString(p.leftHandPosString);
		PatternString rightHandPosString = new PatternString(p.rightHandPosString);
		yield return CupheadTime.WaitForSeconds(this, 0.3f);
		fromLeft = Rand.Bool();
		if (fromLeft)
		{
			sockPuppetLeft.AnIEvent_HoldingBall();
		}
		else
		{
			sockPuppetRight.AnIEvent_HoldingBall();
		}
		sockPuppetLeft.MoveToPos(KDpuppetYPositions[leftHandPos].position.y, 1f);
		StartCoroutine(move_level_borders_time_cr(1060, Level.Current.Right, 0.5f));
		yield return CupheadTime.WaitForSeconds(this, 0.45f);
		base.animator.Play("Ph2_Enter");
		yield return base.animator.WaitForAnimationToStart(this, "LookUpLeftAndBack");
		yield return CupheadTime.WaitForSeconds(this, 0.3f);
		sockPuppetRight.MoveToPos(DpuppetYPositions[rightHandPos].position.y, 1f);
		StartCoroutine(move_level_borders_time_cr(-Level.Current.Left, 152, 0.5f));
		yield return CupheadTime.WaitForSeconds(this, 0.7f);
		base.animator.Play("LookUpRightAndBack");
		yield return null;
		base.animator.SetTrigger("EndIntroLook");
		bool first = true;
		StartCoroutine(animate_face_cr());
		while (true)
		{
			if (!first)
			{
				rightHandPosOld = rightHandPos;
				leftHandPosOld = leftHandPos;
				rightHandPos = rightHandPosString.PopInt();
				leftHandPos = leftHandPosString.PopInt();
				sockPuppetLeft.MoveToPos(KDpuppetYPositions[leftHandPos].position.y, Mathf.Abs(leftHandPosOld - leftHandPos));
				sockPuppetRight.MoveToPos(DpuppetYPositions[rightHandPos].position.y, Mathf.Abs(rightHandPosOld - rightHandPos));
			}
			first = false;
			sockPuppetRight.animator.SetBool("CanTaunt", fromLeft);
			sockPuppetLeft.animator.SetBool("CanTaunt", !fromLeft);
			while (!sockPuppetLeft.ready || !sockPuppetRight.ready)
			{
				yield return null;
			}
			sockPuppetRight.animator.SetBool("IsCatching", fromLeft);
			sockPuppetLeft.animator.SetBool("IsCatching", !fromLeft);
			yield return CupheadTime.WaitForSeconds(this, p.throwDelay);
			OldManLevelSockPuppet throwingPuppet = ((!fromLeft) ? sockPuppetRight : sockPuppetLeft);
			throwingPuppet.animator.SetTrigger("IsThrowing");
			sockPuppetRight.animator.SetBool("CanTaunt", value: false);
			sockPuppetLeft.animator.SetBool("CanTaunt", value: false);
			sockPuppetRight.StopTaunt();
			sockPuppetLeft.StopTaunt();
			yield return throwingPuppet.animator.WaitForAnimationToEnd(this, "Throw_Start");
			yield return CupheadTime.WaitForSeconds(this, p.throwWarningTime);
			throwingPuppet.animator.Play("Throw");
			yield return null;
			while (throwingPuppet.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
			{
				yield return null;
			}
			Vector3 startPos = ((!fromLeft) ? sockPuppetRight.throwPosition() : sockPuppetLeft.throwPosition());
			Vector3 endPos = ((!fromLeft) ? sockPuppetLeft.catchPosition() : sockPuppetRight.catchPosition());
			Vector3 pos = new Vector3(mainPlatformCollider.transform.position.x + (float)(leftHandPos - rightHandPos) * p.bouncePositionSpacing, mainPlatformCollider.bounds.max.y);
			puppetBall = puppetBallPrefab.Spawn();
			puppetBall.Init(startPos, pos, endPos, p);
			throwingPuppet.animator.Play("Throw_End");
			throwingPuppet.animator.Update(0f);
			throwingPuppet.AnIEvent_NotHoldingBall();
			while (!puppetBall.readyToCatch)
			{
				yield return null;
			}
			if (fromLeft)
			{
				sockPuppetRight.animator.Play("Catch");
				sockPuppetRight.animator.Update(0f);
				sockPuppetRight.animator.SetBool("IsCatching", value: false);
			}
			else
			{
				sockPuppetLeft.animator.Play("Catch");
				sockPuppetLeft.animator.Update(0f);
				sockPuppetLeft.animator.SetBool("IsCatching", value: false);
			}
			fromLeft = !fromLeft;
			yield return null;
		}
	}

	private IEnumerator animate_face_cr()
	{
		float t2 = 0f;
		float waitTime2 = 0f;
		bool lookLeft = Rand.Bool();
		PatternString laughString = new PatternString("L,N,N,N,N,N,N,N,L,N,N,N,N,L,N,N,N,N,N,N");
		while (true)
		{
			yield return base.animator.WaitForAnimationToStart(this, "Idle");
			if (triggerLaugh || laughString.PopLetter() == 'L')
			{
				base.animator.Play("Laugh");
				triggerLaugh = false;
				yield return null;
				continue;
			}
			waitTime2 = idleHoldRange.RandomFloat();
			t2 = 0f;
			while (t2 < waitTime2 && !triggerLaugh)
			{
				t2 += (float)CupheadTime.Delta;
				yield return null;
			}
			int curLook = ((!lookLeft) ? rightHandPos : leftHandPos);
			if (!lookLeft && !sockPuppetRight.ready && rightHandPos == 2)
			{
				curLook = 1;
			}
			base.animator.Play(lookLeft ? ((curLook <= 0) ? "LookLeft" : "LookUpLeft") : ((curLook <= 0) ? "LookRight" : ((curLook <= 1) ? "LookMidRight" : "LookUpRight")));
			yield return base.animator.WaitForAnimationToStart(this, lookLeft ? ((curLook <= 0) ? "LookLeftHold" : "LookUpLeftHold") : ((curLook <= 0) ? "LookRightHold" : ((curLook <= 1) ? "LookMidRightHold" : "LookUpRightHold")));
			waitTime2 = lookHoldRange.RandomFloat();
			t2 = 0f;
			while (t2 < waitTime2 && (t2 < lookHoldRange.min || ((!lookLeft) ? sockPuppetRight.ready : sockPuppetLeft.ready)) && !triggerLaugh)
			{
				t2 += (float)CupheadTime.Delta;
				yield return null;
			}
			base.animator.SetTrigger("Continue");
			if (UnityEngine.Random.Range(0f, 1f) < chanceToSwitchLookSides)
			{
				lookLeft = !lookLeft;
			}
		}
	}

	private void TriggerLaugh()
	{
		if (!base.animator.GetCurrentAnimatorStateInfo(0).IsName("Laugh"))
		{
			triggerLaugh = true;
		}
	}

	public void CatchBall()
	{
		puppetBall.GetCaught();
	}

	public void OnPhase3()
	{
		if (OnDeathEvent != null)
		{
			OnDeathEvent();
		}
		StopAllCoroutines();
		StartCoroutine(deathAnimation_cr());
	}

	private IEnumerator deathAnimation_cr()
	{
		((LevelPlayerController)PlayerManager.GetPlayer(PlayerId.PlayerOne)).motor.OnHitEvent -= TriggerLaugh;
		if (PlayerManager.Multiplayer)
		{
			((LevelPlayerController)PlayerManager.GetPlayer(PlayerId.PlayerTwo)).motor.OnHitEvent -= TriggerLaugh;
		}
		sockPuppetLeft.Die();
		sockPuppetRight.Die();
		if (puppetBall == null)
		{
			puppetBall = puppetBallPrefab.Spawn();
			puppetBall.transform.position = ((!fromLeft) ? sockPuppetRight.throwPosition() : sockPuppetLeft.throwPosition());
		}
		puppetBall.Explode();
		OldManLevelDwarf[] array = dwarves;
		foreach (OldManLevelDwarf oldManLevelDwarf in array)
		{
			oldManLevelDwarf.Death(parried: true);
		}
		base.animator.SetTrigger("Continue");
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		base.animator.Play("Angry");
		YieldInstruction wait = new WaitForFixedUpdate();
		Vector3 startPos = oldManAngry.localPosition;
		Vector3 endPos = new Vector3(oldManAngry.localPosition.x, 200f);
		Vector3 sockPuppetLeftStart = sockPuppetLeft.rootPosition;
		Vector3 sockPuppetRightStart = sockPuppetRight.rootPosition;
		Vector3 sockPuppetLeftEnd = ((Level.Current.mode != 0) ? new Vector3(sockPuppetLeft.rootPosition.x - 300f, -1100f) : new Vector3(sockPuppetLeft.rootPosition.x, KDpuppetYPositions[1].position.y));
		Vector3 sockPuppetRightEnd = ((Level.Current.mode != 0) ? new Vector3(sockPuppetRight.rootPosition.x + 300f, -1100f) : new Vector3(sockPuppetRight.rootPosition.x, DpuppetYPositions[1].position.y));
		yield return CupheadTime.WaitForSeconds(this, (Level.Current.mode != 0) ? 2f : 0.1f);
		float t = 0f;
		float time = ((Level.Current.mode != 0) ? base.properties.CurrentState.hands.endSlideUpTime : 2f);
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			if (Level.Current.mode != 0)
			{
				oldManAngry.SetPosition(null, Mathf.Lerp(startPos.y, endPos.y, t / time));
				oldManAngryNoseShadow.localPosition = new Vector3(oldManAngryNoseShadow.localPosition.x, Mathf.Lerp(0f, 10f, t / time));
			}
			sockPuppetLeft.rootPosition = new Vector3(Mathf.Lerp(sockPuppetLeftStart.x, sockPuppetLeftEnd.x, EaseUtils.EaseInSine(0f, 1f, t / time)), Mathf.Lerp(sockPuppetLeftStart.y, sockPuppetLeftEnd.y, EaseUtils.EaseInSine(0f, 1f, t / time)));
			sockPuppetRight.rootPosition = new Vector3(Mathf.Lerp(sockPuppetRightStart.x, sockPuppetRightEnd.x, EaseUtils.EaseInSine(0f, 1f, t / time)), Mathf.Lerp(sockPuppetRightStart.y, sockPuppetRightEnd.y, EaseUtils.EaseInSine(0f, 1f, t / time)));
			if (t <= 0.5f && t + CupheadTime.FixedDelta > 0.5f)
			{
				sockPuppetLeft.GetComponent<LevelBossDeathExploder>().StopExplosions();
				sockPuppetRight.GetComponent<LevelBossDeathExploder>().StopExplosions();
			}
			yield return wait;
		}
		UnityEngine.Object.Destroy(dwarvesObject.gameObject);
		if (Level.Current.mode != 0)
		{
			base.animator.SetTrigger("ContinueDeath");
			while (transState != TransitionState.PlatformDestroyed)
			{
				yield return null;
			}
			yield return StartCoroutine(move_level_borders_anim_sync_cr(925, 93, 56f));
		}
	}

	private IEnumerator move_level_borders_time_cr(int left, int right, float time)
	{
		float t = 0f;
		float startLeft = -Level.Current.Left;
		float startRight = Level.Current.Right;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			float tm = Mathf.InverseLerp(0f, time, t);
			Level.Current.SetBounds((int)Mathf.Lerp(startLeft, left, tm), (int)Mathf.Lerp(startRight, right, tm), null, null);
			yield return null;
		}
	}

	private IEnumerator move_level_borders_anim_sync_cr(int left, int right, float endFrame)
	{
		float startTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		float startLeft = -Level.Current.Left;
		float startRight = Level.Current.Right;
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < (endFrame + 1f) / 79f)
		{
			float tm = Mathf.InverseLerp(startTime, endFrame / 79f, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			Level.Current.SetBounds((int)Mathf.Lerp(startLeft, left, tm), (int)Mathf.Lerp(startRight, right, tm), null, null);
			yield return null;
		}
	}

	private IEnumerator shake_platform_cr()
	{
		float amount = 1.5f;
		while (transState != TransitionState.PlatformDestroyed)
		{
			handsParent.transform.localPosition = new Vector3(UnityEngine.Random.Range(0f - amount, amount), UnityEngine.Random.Range(0f - amount, amount));
			amount += 0.25f;
			yield return CupheadTime.WaitForSeconds(this, 1f / 60f);
		}
	}

	private void AniEvent_HandsGrip()
	{
		CupheadLevelCamera.Current.Shake(5f, 0.2f);
		beardObject.transform.parent = handsParent.transform;
		rocksUnderBeardObject.transform.parent = handsParent.transform;
		StartCoroutine(shake_platform_cr());
	}

	private void AniEvent_PlatformDestroyed()
	{
		beardObject.transform.parent = BGParent.transform;
		rocksUnderBeardObject.transform.parent = BGParent.transform;
		transState = TransitionState.PlatformDestroyed;
		CupheadLevelCamera.Current.Shake(30f, 0.7f);
	}

	private void AniEvent_FinishedSwallow()
	{
		transState = TransitionState.InStomach;
	}

	public void SwallowedPlayers()
	{
		base.animator.SetTrigger("SwallowedPlayers");
	}

	public void FinishPuppet()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void AnimationEvent_SFX_OMM_P2_EndBreakPlatformEat()
	{
		AudioManager.Play("sfx_dlc_omm_p2_end_breakplatformeat");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_end_breakplatformeat");
	}

	private void AnimationEvent_SFX_OMM_P2_EndBurp()
	{
		AudioManager.Play("sfx_dlc_omm_p2_end_burp");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_end_burp");
	}

	private void WORKAROUND_NullifyFields()
	{
		OnDeathEvent = null;
		idleHoldRange = null;
		lookHoldRange = null;
		oldManAngry = null;
		oldManAngryNoseShadow = null;
		mainPlatformCollider = null;
		puppetBallPrefab = null;
		sockPuppetLeft = null;
		sockPuppetRight = null;
		platformManager = null;
		KDpuppetYPositions = null;
		DpuppetYPositions = null;
		dwarves = null;
		dwarvesObject = null;
		handsParent = null;
		BGParent = null;
		beardObject = null;
		rocksUnderBeardObject = null;
		damageDealer = null;
		puppetBall = null;
	}
}
