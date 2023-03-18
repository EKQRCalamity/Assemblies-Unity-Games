using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.AI;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce;

public class QuirceBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct QuirceAttackConfig
	{
		public QUIRCE_ATTACKS attackType;

		public bool requiresReposition;

		public bool invertedReposition;

		public bool teleportReposition;

		public float preparationSeconds;

		public int multiAttackTimes;

		public float waitingSecondsAfterAttack;
	}

	public enum BOSS_STATES
	{
		WAITING,
		MID_ACTION,
		AVAILABLE_FOR_ACTION
	}

	public enum QUIRCE_STATE
	{
		SWORD,
		NO_SWORD
	}

	public enum QUIRCE_ATTACKS
	{
		DASH,
		MULTIDASH,
		TELEPORT,
		PATH_THROW,
		SWORD_TOSS,
		MULTI_TELEPORT,
		SWORD_RECOVERY
	}

	public Vector2 reversedLocalPositionOffset;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	private float _currentAttackLapse;

	public QuirceSwordBehaviour sword;

	private Transform currentTarget;

	private QuirceAttackConfig _lastAttackConfig;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack dashAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossTeleportAttack teleportAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossTeleportAttack multiTeleportAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack plungeAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack multiPlungeAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack multiDashAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossSplineFollowingProjectileAttack splineThrowAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack quickMove;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack areaSummonAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack landingAreaSummon;

	[FoldoutGroup("Attacks", true, 0)]
	public BossInstantProjectileAttack instantProjectileAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossInstantProjectileAttack instantProjectileMoveAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public List<QuirceAttackConfig> attacksConfiguration;

	[FoldoutGroup("Attacks", true, 0)]
	public List<QuirceAttackConfig> ngPlusAttacksConfiguration;

	[FoldoutGroup("Traits", true, 0)]
	public EntityMotionChecker motionChecker;

	[FoldoutGroup("VFX", true, 0)]
	public GameObject vFXExplosion;

	private List<QUIRCE_ATTACKS> currentlyAvailableAttacks;

	private List<QUIRCE_ATTACKS> queuedActions;

	private QUIRCE_STATE currentQuirceState;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public QUIRCE_ATTACKS lastAttack;

	private Transform currentHang;

	private SplinePointInfo currentPoint;

	private GhostTrailGenerator ghostTrail;

	private int _originAreas;

	private float _originAreaSeconds;

	private int actionsCounter;

	private Coroutine currentCoroutine;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public Quirce Quirce { get; private set; }

	public bool Awaken { get; private set; }

	public int multiTeleportAttackNumber { get; private set; }

	private int dashRemainings { get; set; }

	public override void OnAwake()
	{
		base.OnAwake();
		Quirce = (Quirce)Entity;
		currentlyAvailableAttacks = GetCurrentStateAttacks();
		ghostTrail = GetComponentInChildren<GhostTrailGenerator>();
		quickMove.SetRotatingFunction(RotateToLookAt);
		PoolManager.Instance.CreatePool(vFXExplosion, 16);
	}

	private List<QUIRCE_ATTACKS> GetCurrentStateAttacks()
	{
		List<QUIRCE_ATTACKS> list;
		if (currentQuirceState == QUIRCE_STATE.SWORD)
		{
			list = new List<QUIRCE_ATTACKS>();
			list.Add(QUIRCE_ATTACKS.DASH);
			list.Add(QUIRCE_ATTACKS.TELEPORT);
			list.Add(QUIRCE_ATTACKS.PATH_THROW);
			list.Add(QUIRCE_ATTACKS.SWORD_TOSS);
			return list;
		}
		list = new List<QUIRCE_ATTACKS>();
		list.Add(QUIRCE_ATTACKS.MULTI_TELEPORT);
		list.Add(QUIRCE_ATTACKS.MULTIDASH);
		list.Add(QUIRCE_ATTACKS.SWORD_RECOVERY);
		return list;
	}

	private void SetAttacksConfiguration()
	{
		if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS))
		{
			attacksConfiguration = ngPlusAttacksConfiguration;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		SetAttacksConfiguration();
		ChangeBossState(BOSS_STATES.WAITING);
		SetQuirceState(QUIRCE_STATE.SWORD);
		StartWaitingPeriod(1f);
		currentPoint = default(SplinePointInfo);
		currentPoint.nextValidPoints = new List<QuirceBossFightPoints.QUIRCE_FIGHT_SIDES>
		{
			QuirceBossFightPoints.QUIRCE_FIGHT_SIDES.LEFT,
			QuirceBossFightPoints.QUIRCE_FIGHT_SIDES.RIGHT
		};
		_originAreas = areaSummonAttack.totalAreas;
		_originAreaSeconds = areaSummonAttack.seconds;
	}

	private void LateUpdate()
	{
		Quirce.SpriteRenderer.gameObject.transform.localPosition = ((!Quirce.SpriteRenderer.flipY) ? Vector2.zero : reversedLocalPositionOffset);
	}

	private void ToggleShowSword(bool show)
	{
		sword.gameObject.SetActive(show);
	}

	private void SetCurrentCoroutine(Coroutine c)
	{
		if (currentCoroutine != null)
		{
			Debug.Log(">>>>STOPPING CURRENT COROUTINE");
			StopCoroutine(currentCoroutine);
		}
		Debug.Log(">>NEW COROUTINE");
		currentCoroutine = c;
	}

	private void ChangeBossState(BOSS_STATES newState)
	{
		currentState = newState;
	}

	private void StartAttackAction()
	{
		Quirce.AnimatorInyector.ResetHurt();
		ChangeBossState(BOSS_STATES.MID_ACTION);
	}

	public int GetActionsCounter()
	{
		return actionsCounter;
	}

	public void ResetActionsCounter()
	{
		actionsCounter = 0;
	}

	public QUIRCE_STATE GetQUIRCE_STATE()
	{
		return currentQuirceState;
	}

	public QUIRCE_ATTACKS GetNewAttack()
	{
		QUIRCE_ATTACKS[] array = new QUIRCE_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<QUIRCE_ATTACKS> list = new List<QUIRCE_ATTACKS>(array);
		list.Remove(lastAttack);
		if (lastAttack == QUIRCE_ATTACKS.PATH_THROW)
		{
			list.Remove(QUIRCE_ATTACKS.TELEPORT);
		}
		else if (lastAttack == QUIRCE_ATTACKS.SWORD_RECOVERY)
		{
			list.Remove(QUIRCE_ATTACKS.TELEPORT);
		}
		list.Remove(QUIRCE_ATTACKS.SWORD_TOSS);
		list.Remove(QUIRCE_ATTACKS.SWORD_RECOVERY);
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public void LaunchRandomAction()
	{
		actionsCounter++;
		LaunchAction(GetNewAttack());
	}

	public void LaunchRecoverAction()
	{
		LaunchAction(QUIRCE_ATTACKS.SWORD_RECOVERY);
	}

	public void LaunchTossAction()
	{
		LaunchAction(QUIRCE_ATTACKS.SWORD_TOSS);
	}

	private void QueuedActionsPush(QUIRCE_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<QUIRCE_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private QUIRCE_ATTACKS QueuedActionsPop()
	{
		QUIRCE_ATTACKS qUIRCE_ATTACKS = queuedActions[0];
		queuedActions.Remove(qUIRCE_ATTACKS);
		return qUIRCE_ATTACKS;
	}

	public void LaunchAction(QUIRCE_ATTACKS atk, bool checkReposition = true)
	{
		QuirceAttackConfig quirceAttackConfig = (_lastAttackConfig = attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == atk));
		if (quirceAttackConfig.requiresReposition && checkReposition)
		{
			Vector3 point = Vector3.zero;
			if (atk != 0)
			{
				if (atk != QUIRCE_ATTACKS.SWORD_TOSS)
				{
					if (atk == QUIRCE_ATTACKS.PATH_THROW)
					{
						Transform hangTransform = Quirce.BossFightPoints.GetHangTransform(currentPoint.nextValidPoints);
						currentPoint = Quirce.BossFightPoints.GetHangPointInfo(hangTransform);
						point = hangTransform.position;
						currentHang = hangTransform;
					}
				}
				else
				{
					point = Quirce.BossFightPoints.GetTossPoint();
				}
			}
			else
			{
				Transform dashPointTransform = Quirce.BossFightPoints.GetDashPointTransform(currentPoint.nextValidPoints);
				point = dashPointTransform.position;
				currentPoint = Quirce.BossFightPoints.GetDashPointInfo(dashPointTransform);
			}
			QueuedActionsPush(atk);
			Reposition(point);
			return;
		}
		lastAttack = atk;
		switch (atk)
		{
		case QUIRCE_ATTACKS.DASH:
			DashAttack();
			break;
		case QUIRCE_ATTACKS.MULTIDASH:
			NDashAttack();
			break;
		case QUIRCE_ATTACKS.TELEPORT:
			TeleportAttack();
			break;
		case QUIRCE_ATTACKS.MULTI_TELEPORT:
			MultiTeleportAttack();
			break;
		case QUIRCE_ATTACKS.PATH_THROW:
			PathThrowAttack();
			break;
		case QUIRCE_ATTACKS.SWORD_TOSS:
			TossAttack();
			break;
		case QUIRCE_ATTACKS.SWORD_RECOVERY:
			SwordRecoveryAttack();
			break;
		}
	}

	public override void Idle()
	{
		Debug.Log("Quirce: IDLE");
		StopMovement();
	}

	private void StartWaitingPeriod(float seconds)
	{
		ChangeBossState(BOSS_STATES.WAITING);
		if (Core.Logic.Penitent != null)
		{
			currentTarget = Core.Logic.Penitent.transform;
			LookAtTarget(currentTarget.position);
		}
		SetCurrentCoroutine(StartCoroutine(WaitingPeriodCoroutine(seconds, AfterWaitingPeriod)));
	}

	private IEnumerator WaitingPeriodCoroutine(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		callback();
	}

	private void AfterWaitingPeriod()
	{
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
	}

	public void Reposition(Vector3 point)
	{
		StartAttackAction();
		LookAtTarget(point);
		Debug.Log("Quirce: REPOSITION");
		StartCoroutine(PrepareAndReposition(point));
	}

	private IEnumerator PrepareAndReposition(Vector2 point)
	{
		sword.SetVisible(visible: false);
		Quirce.AnimatorInyector.TeleportOut();
		float teleportTime = 0.8f;
		yield return new WaitForSeconds(teleportTime);
		SetReversed(_lastAttackConfig.invertedReposition);
		base.transform.position = point;
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		Quirce.AnimatorInyector.TeleportIn();
		Quirce.Audio.PlayTeleportIn();
		float teleportAnimationLength = 0.4f;
		yield return new WaitForSeconds(teleportAnimationLength);
		Quirce.AnimatorInyector.Landing();
		OnRepositionFinished();
	}

	private void OnRepositionFinished()
	{
		sword.SetVisible(visible: true);
		QUIRCE_ATTACKS atk = QueuedActionsPop();
		LaunchAction(atk, checkReposition: false);
	}

	public void DashAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingDashCoroutine()));
	}

	private IEnumerator PreparingDashCoroutine()
	{
		sword.SetGhostTrail(active: true);
		ghostTrail.EnableGhostTrail = true;
		Quirce.Audio.PlayPreDash();
		Quirce.AnimatorInyector.BigDashPreparation();
		Quirce.IsGuarding = true;
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		yield return new WaitForSeconds(attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.DASH).preparationSeconds);
		Debug.Log("Quirce: DASH ATTACK");
		Quirce.Audio.PlayBigDash();
		Quirce.AnimatorInyector.Dash(state: true);
		dashAttack.OnDashFinishedEvent += OnDashAttackFinished;
		dashAttack.OnDashBlockedEvent += OnDashBlocked;
		float d = ((Quirce.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		dashAttack.Dash(base.transform, Vector3.right * d, 20f, 0.5f);
	}

	private void OnDashBlocked(BossDashAttack obj)
	{
		Core.Logic.ScreenFreeze.Freeze(0.1f, 1f, 0f, Quirce.slowTimeCurve);
		Quirce.IsGuarding = false;
	}

	private void OnDashAttackFinished()
	{
		sword.SetGhostTrail(active: false);
		ghostTrail.EnableGhostTrail = false;
		dashAttack.OnDashBlockedEvent -= OnDashBlocked;
		dashAttack.OnDashFinishedEvent -= OnDashAttackFinished;
		Quirce.AnimatorInyector.Dash(state: false);
		Quirce.IsGuarding = false;
		StartWaitingPeriod(GetAttackConfig(QUIRCE_ATTACKS.DASH).waitingSecondsAfterAttack);
	}

	public void SwordRecoveryAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingSwordRecovery()));
	}

	private void SetAreaAttackValuesFromHP()
	{
		int num = 5;
		int originAreas = _originAreas;
		float originAreaSeconds = _originAreaSeconds;
		int num2 = Mathf.RoundToInt(Mathf.Lerp(num, originAreas, 1f - GetHealthPercentage()));
		float num3 = originAreaSeconds * ((float)num2 / (float)originAreas);
		areaSummonAttack.totalAreas = num2;
	}

	private IEnumerator PreparingSwordRecovery()
	{
		Quirce.AnimatorInyector.TeleportOutSword();
		yield return new WaitForSeconds(1.6f);
		Quirce.transform.position = sword.transform.position;
		Quirce.SetOrientation(EntityOrientation.Right);
		ToggleShowSword(show: false);
		sword.SetAutoFollow(follow: false);
		Quirce.AnimatorInyector.TeleportInSword();
		Quirce.Audio.PlayTeleportIn();
		yield return new WaitForSeconds(1.4f);
		ToggleShowSword(show: true);
		sword.SetVisible(visible: false);
		Vector2 dir = ((Quirce.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		SetAreaAttackValuesFromHP();
		Quirce.Audio.PlayToss();
		areaSummonAttack.SummonAreas(dir);
		Quirce.BossFightPoints.ActivateWallMask(v: false);
		Debug.Log("Quirce: SWORD RECOVERY ATTACK");
		yield return new WaitForSeconds(1.6f);
		Quirce.transform.position = Quirce.BossFightPoints.GetCenter();
		Quirce.AnimatorInyector.TeleportIn();
		Quirce.Audio.PlayTeleportIn();
		sword.SetAutoFollow(follow: true);
		yield return new WaitForSeconds(0.4f);
		sword.SetVisible(visible: true);
		OnSwordRecoveryAttackFinished();
	}

	private void OnSwordRecoveryAttackFinished()
	{
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.SWORD_RECOVERY);
		SetQuirceState(QUIRCE_STATE.SWORD);
		StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
	}

	private IEnumerator RandomExplosions(float seconds, int totalExplosions, Transform center, float radius, GameObject poolableExplosion, Action OnExplosion = null, Action callback = null)
	{
		float counter = 0f;
		int expCounter = 0;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			float expRatio = (float)expCounter / (float)totalExplosions;
			if (counter / seconds > expRatio)
			{
				expCounter++;
				Vector2 vector = center.position + new Vector3(UnityEngine.Random.Range(0f - radius, radius), UnityEngine.Random.Range(0f - radius, radius));
				PoolManager.Instance.ReuseObject(poolableExplosion, vector, Quaternion.identity);
				OnExplosion?.Invoke();
			}
			yield return null;
		}
		callback?.Invoke();
	}

	public void NDashAttack()
	{
		StartAttackAction();
		dashRemainings = GetAttackConfig(QUIRCE_ATTACKS.MULTIDASH).multiAttackTimes;
		SetCurrentCoroutine(StartCoroutine(PreparingMultiDash()));
	}

	private IEnumerator PreparingMultiDash()
	{
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		float d = Mathf.Sign(currentTarget.position.x - base.transform.position.x);
		float waitSeconds = attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.MULTIDASH).preparationSeconds;
		StartCoroutine(RandomExplosions(waitSeconds * 0.8f, 20, base.transform, 0.6f, vFXExplosion));
		ghostTrail.EnableGhostTrail = true;
		Quirce.Audio.PlayPreDash();
		Quirce.AnimatorInyector.BigDashPreparation();
		yield return new WaitForSeconds(waitSeconds);
		Debug.Log("Quirce: NDASH ATTACK");
		Quirce.AnimatorInyector.Dash(state: true);
		Quirce.Audio.PlaySwordlessDash();
		multiDashAttack.OnDashFinishedEvent += OnMultiDashAttackFinished;
		multiDashAttack.Dash(base.transform, Vector3.right * d, 8f, 0.5f);
	}

	private void OnMultiDashAttackFinished()
	{
		ghostTrail.EnableGhostTrail = false;
		dashRemainings--;
		multiDashAttack.OnDashFinishedEvent -= OnMultiDashAttackFinished;
		Quirce.AnimatorInyector.Dash(state: false);
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.MULTIDASH);
		if (dashRemainings > 0)
		{
			SetCurrentCoroutine(StartCoroutine(PreparingMultiDash()));
		}
		else
		{
			StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
		}
	}

	public void PathThrowAttack()
	{
		SplinePointInfo hangPointInfo = Quirce.BossFightPoints.GetHangPointInfo(currentHang);
		Vector2 vector = hangPointInfo.spline.GetPoint(2f) - hangPointInfo.spline.GetPoint(0f);
		LookAtTarget((Vector2)base.transform.position - vector);
		sword.SetSpinning(spin: true);
		Quirce.Audio.PlaySpinSword();
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingPathThrow()));
	}

	private IEnumerator PreparingPathThrow()
	{
		yield return new WaitForSeconds(attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.PATH_THROW).preparationSeconds);
		Quirce.AnimatorInyector.Throw();
		yield return new WaitForSeconds(0.6f);
		SplinePointInfo info = Quirce.BossFightPoints.GetHangPointInfo(currentHang);
		BezierSpline spline = info.spline;
		splineThrowAttack.Shoot(spline, info.speedCurve, info.time, sword.transform.position, spline.GetPoint(spline.points.Length - 1));
		splineThrowAttack.OnPathFinished += OnProjectilePathFinished;
		splineThrowAttack.OnPathAdvanced += OnProjectilePathAdvanced;
		ToggleShowSword(show: false);
	}

	private void OnProjectilePathAdvanced(BossSplineFollowingProjectileAttack atk, float maxS, float elapS)
	{
		if (maxS - elapS < 1f)
		{
			atk.OnPathAdvanced -= OnProjectilePathAdvanced;
			Quirce.Audio.EndSwordSpinSound();
		}
	}

	private void OnProjectilePathFinished(BossSplineFollowingProjectileAttack obj)
	{
		obj.OnPathFinished -= OnProjectilePathFinished;
		sword.SetSpinning(spin: false);
		ToggleShowSword(show: true);
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.PATH_THROW);
		if (!IsDead())
		{
			StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
		}
	}

	private void CreateExplosion(Vector2 p)
	{
		PoolManager.Instance.ReuseObject(vFXExplosion, p, Quaternion.identity);
	}

	private IEnumerator EffectsInLine(Vector2 origin, Vector2 end, int explosions, float seconds, Action<Vector2> effectFunction)
	{
		float counter = 0f;
		int i = 0;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			if (counter / seconds > (float)i / (float)explosions)
			{
				Vector2 obj = Vector2.Lerp(origin, end, counter / seconds);
				effectFunction(obj);
				i++;
			}
			yield return null;
		}
	}

	public void TossAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingTossAttack()));
	}

	private void BeforeTossTeleportInWall()
	{
		SetReversed(reversed: false);
		Quirce.transform.position = Quirce.BossFightPoints.GetTossPoint();
		Quirce.SetOrientation(EntityOrientation.Left);
		Quirce.Audio.PlayTeleportIn();
		Quirce.AnimatorInyector.TeleportInSword();
	}

	private void TossAttackAnticipation()
	{
		ToggleShowSword(show: false);
		sword.SetAutoFollow(follow: false);
	}

	private void AfterTossReposition()
	{
		Quirce.transform.position = Quirce.BossFightPoints.GetCenter();
		Quirce.Audio.PlayTeleportIn();
		Quirce.AnimatorInyector.TeleportIn();
		Quirce.AnimatorInyector.SetToss(toss: false);
	}

	private IEnumerator PreparingTossAttack()
	{
		float timeBeforeReappear = 0.4f;
		float timeBeforeGrabsSword = 0.4f;
		float timeGrabLoopAnticipation = 0.6f;
		float timeProjectileAppears = 0.25f;
		float timeTrailAppears = 0.5f;
		float timeQuirceBackToCenter = 2f;
		yield return new WaitForSeconds(attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.SWORD_TOSS).preparationSeconds);
		Quirce.AnimatorInyector.TeleportOutSword();
		Quirce.AnimatorInyector.SetToss(toss: true);
		sword.SetVisible(visible: false);
		yield return new WaitForSeconds(timeBeforeReappear);
		sword.doFollow = true;
		BeforeTossTeleportInWall();
		yield return new WaitForSeconds(timeBeforeGrabsSword);
		TossAttackAnticipation();
		yield return new WaitForSeconds(timeGrabLoopAnticipation);
		Quirce.AnimatorInyector.Throw();
		Quirce.Audio.PlayToss();
		yield return new WaitForSeconds(timeProjectileAppears);
		Vector2 dir = ((Quirce.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Vector2 origin = (Vector2)base.transform.position + Vector2.left + Vector2.up * 0.5f;
		BezierSpline s = Quirce.BossFightPoints.spiralPointInfo.spline;
		AnimationCurve c = Quirce.BossFightPoints.spiralPointInfo.speedCurve;
		float seconds = Quirce.BossFightPoints.spiralPointInfo.time;
		splineThrowAttack.Shoot(s, c, seconds);
		StartCoroutine(EffectsInLine(origin, Quirce.BossFightPoints.GetSwordWallPoint().position, 10, 0.8f, CreateExplosion));
		yield return new WaitForSeconds(timeTrailAppears);
		instantProjectileAttack.Shoot(origin, dir);
		Quirce.Audio.PlayHitWall();
		SetSwordOnWall();
		yield return new WaitForSeconds(timeQuirceBackToCenter);
		AfterTossReposition();
		yield return new WaitForSeconds(0.4f);
		OnSwordTossAttackFinished();
	}

	private void OnSwordTossAttackFinished()
	{
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.SWORD_TOSS);
		SetQuirceState(QUIRCE_STATE.NO_SWORD);
		StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
	}

	private IEnumerator PreparingSpiral()
	{
		yield return new WaitForSeconds(attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.SWORD_TOSS).preparationSeconds);
		Debug.Log("Quirce: SPIRAL ATTACK");
		Quirce.AnimatorInyector.Spiral(on: true);
		SplinePointInfo info = Quirce.BossFightPoints.spiralPointInfo;
		BezierSpline spline = info.spline;
		splineThrowAttack.Shoot(spline, info.speedCurve, info.time);
		splineThrowAttack.OnPathFinished += OnSpiralPathFinished;
		ToggleShowSword(show: false);
		sword.SetAutoFollow(follow: false);
	}

	private void OnSpiralPathFinished(BossSplineFollowingProjectileAttack obj)
	{
		obj.OnPathFinished -= OnSpiralPathFinished;
		Quirce.AnimatorInyector.Spiral(on: false);
		SetSwordOnWall();
		SetQuirceState(QUIRCE_STATE.NO_SWORD);
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.SWORD_TOSS);
		if (!IsDead())
		{
			StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
		}
	}

	private void SetSwordOnWall()
	{
		sword.SetSpinning(spin: false);
		Quirce.BossFightPoints.ActivateWallMask(v: true);
		Transform swordWallPoint = Quirce.BossFightPoints.GetSwordWallPoint();
		sword.transform.SetPositionAndRotation(swordWallPoint.position, swordWallPoint.rotation);
		sword.SetReversed(reversed: false);
		ToggleShowSword(show: true);
	}

	public void TeleportAttack()
	{
		StartAttackAction();
		sword.SetVisible(visible: false);
		currentTarget = GetTarget();
		teleportAttack.OnTeleportInEvent += OnTeleportIn;
		Debug.Log("Quirce: Teleport OUT");
		Quirce.AnimatorInyector.TeleportOut();
		teleportAttack.Use(base.transform, currentTarget, Vector3.up * 3.5f);
	}

	private void OnTeleportIn()
	{
		SetReversed(reversed: false);
		Quirce.AnimatorInyector.TeleportIn();
		teleportAttack.OnTeleportInEvent -= OnTeleportIn;
		PlungeAttack();
	}

	public void PlungeAttack()
	{
		SetCurrentCoroutine(StartCoroutine(PreparePlungeAttack()));
	}

	private IEnumerator PreparePlungeAttack()
	{
		yield return new WaitForSeconds(attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == QUIRCE_ATTACKS.TELEPORT).preparationSeconds);
		Debug.Log("Quirce: PLUNGE ATTACK");
		Quirce.AnimatorInyector.Plunge(state: true);
		currentTarget = GetTarget();
		plungeAttack.OnDashFinishedEvent += OnPlungeAttackFinished;
		Quirce.Audio.PlayPlunge();
		plungeAttack.Dash(base.transform, Vector3.down, 10f);
	}

	private void OnPlungeAttackFinished()
	{
		plungeAttack.OnDashFinishedEvent -= OnPlungeAttackFinished;
		Quirce.AnimatorInyector.Plunge(state: false);
		QuirceAttackConfig attackConfig = GetAttackConfig(QUIRCE_ATTACKS.TELEPORT);
		currentPoint.nextValidPoints = new List<QuirceBossFightPoints.QUIRCE_FIGHT_SIDES>
		{
			QuirceBossFightPoints.QUIRCE_FIGHT_SIDES.LEFT,
			QuirceBossFightPoints.QUIRCE_FIGHT_SIDES.RIGHT
		};
		sword.SetVisible(visible: true);
		StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
	}

	public void MultiTeleportAttack()
	{
		StartAttackAction();
		StartCoroutine(MultiTeleportAnticipation());
	}

	private IEnumerator MultiTeleportAnticipation()
	{
		QuirceAttackConfig qac = GetAttackConfig(QUIRCE_ATTACKS.MULTI_TELEPORT);
		Quirce.AnimatorInyector.TeleportOut();
		yield return new WaitForSeconds(qac.preparationSeconds);
		multiTeleportAttackNumber = qac.multiAttackTimes;
		if ((double)GetHealthPercentage() < 0.75)
		{
			multiTeleportAttackNumber++;
		}
		if ((double)GetHealthPercentage() < 0.25)
		{
			multiTeleportAttackNumber++;
		}
		SetCurrentCoroutine(StartCoroutine(PreparingMultiTeleport()));
	}

	private IEnumerator PreparingMultiTeleport()
	{
		yield return new WaitForSeconds(0.1f);
		currentTarget = GetTarget();
		multiTeleportAttack.OnTeleportInEvent += OnMultiTeleportIn;
		Quirce.AnimatorInyector.TeleportOut();
		float waitTime = 1.2f;
		if (GetHealthPercentage() < 0.5f)
		{
			waitTime = 0.9f;
		}
		yield return new WaitForSeconds(waitTime);
		Transform teleportTarget = currentTarget;
		if (GetHealthPercentage() < 0.25f)
		{
			teleportTarget = Quirce.BossFightPoints.GetTeleportPlungeTransform();
		}
		multiTeleportAttack.Use(base.transform, teleportTarget, Vector3.up * 4f);
	}

	private void OnMultiTeleportIn()
	{
		Debug.Log("Quirce: MULTI Teleport IN");
		Quirce.AnimatorInyector.ResetTeleport();
		Quirce.AnimatorInyector.TeleportIn();
		multiTeleportAttack.OnTeleportInEvent -= OnMultiTeleportIn;
		StartCoroutine(MultiPlungeAnticipation());
	}

	private IEnumerator MultiPlungeAnticipation()
	{
		yield return new WaitForSeconds(0.3f);
		MultiPlungeAttack();
	}

	public void MultiPlungeAttack()
	{
		Debug.Log("Quirce: MULTI PLUNGE ATTACK");
		Quirce.AnimatorInyector.Plunge(state: true);
		currentTarget = GetTarget();
		multiPlungeAttack.OnDashFinishedEvent += OnMultiPlungeAttackFinished;
		Quirce.Audio.PlayPlunge();
		multiPlungeAttack.Dash(base.transform, Vector3.down, 10f);
	}

	private void OnMultiPlungeAttackFinished()
	{
		Debug.Log("Quirce: PLUNGE ATTACK FINISHED NUMBER:" + multiTeleportAttackNumber);
		multiPlungeAttack.OnDashFinishedEvent -= OnMultiPlungeAttackFinished;
		if (GetHealthPercentage() < 0.5f)
		{
			landingAreaSummon.SummonAreas(Vector3.right);
			landingAreaSummon.SummonAreas(Vector3.left);
		}
		Quirce.AnimatorInyector.Plunge(state: false);
		multiTeleportAttackNumber--;
		if (multiTeleportAttackNumber > 0)
		{
			SetCurrentCoroutine(StartCoroutine(PreparingMultiTeleport()));
		}
		else
		{
			StartWaitingPeriod(GetAttackConfig(QUIRCE_ATTACKS.MULTI_TELEPORT).waitingSecondsAfterAttack);
		}
	}

	public float GetHealthPercentage()
	{
		return Quirce.CurrentLife / Quirce.Stats.Life.Base;
	}

	public void OnTeleportOutAnimationStarts()
	{
		Quirce.ActivateColliders(activate: false);
		Quirce.Audio.PlayTeleportOut();
		Quirce.Status.CastShadow = false;
	}

	public void OnTeleportOutAnimationFinished()
	{
	}

	public void OnTeleportInAnimationStarts()
	{
	}

	public void OnTeleportInAnimationFinished()
	{
		Quirce.Status.CastShadow = true;
		Quirce.ActivateColliders(activate: true);
	}

	private void RotateToLookAt(Transform p, Vector3 point)
	{
		Vector3 vector = point - p.position;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		if (Quirce.Status.Orientation == EntityOrientation.Left)
		{
			num -= 180f;
		}
		p.rotation = Quaternion.Euler(0f, 0f, num);
	}

	private void UnsubscribeFromEverything()
	{
		Debug.Log("UNSUBSCRIBING FROM EVERY EVENT");
		multiTeleportAttack.OnTeleportInEvent -= OnMultiTeleportIn;
		plungeAttack.OnDashFinishedEvent -= OnMultiPlungeAttackFinished;
		plungeAttack.OnDashFinishedEvent -= OnPlungeAttackFinished;
		teleportAttack.OnTeleportInEvent -= OnTeleportIn;
		multiDashAttack.OnDashFinishedEvent -= OnMultiDashAttackFinished;
		dashAttack.OnDashFinishedEvent -= OnDashAttackFinished;
		quickMove.OnDashFinishedEvent -= OnRepositionFinished;
	}

	private QuirceAttackConfig GetAttackConfig(QUIRCE_ATTACKS atk)
	{
		return attacksConfiguration.Find((QuirceAttackConfig x) => x.attackType == atk);
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public bool TargetCanBeVisible()
	{
		float num = Quirce.Target.transform.position.y - Quirce.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Quirce.transform.position.x)
		{
			if (Quirce.Status.Orientation != 0)
			{
				Quirce.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (Quirce.Status.Orientation != EntityOrientation.Left)
		{
			Quirce.SetOrientation(EntityOrientation.Left);
		}
	}

	public void SetQuirceState(QUIRCE_STATE st)
	{
		currentQuirceState = st;
		currentlyAvailableAttacks = GetCurrentStateAttacks();
	}

	public QUIRCE_STATE GetQuirceState()
	{
		return currentQuirceState;
	}

	public override void Damage()
	{
		if (currentState != BOSS_STATES.MID_ACTION)
		{
			Quirce.AnimatorInyector.Hurt();
		}
	}

	public bool CanAttack()
	{
		return true;
	}

	public void Death()
	{
		StopAllCoroutines();
		ghostTrail.EnableGhostTrail = false;
		Debug.Log("DEATH REACHED");
		UnsubscribeFromEverything();
		base.BehaviourTree.StopBehaviour();
		Quirce.AnimatorInyector.Death();
		SplineFollowingProjectile currentProjectile = splineThrowAttack.GetCurrentProjectile();
		if (currentProjectile != null)
		{
			splineThrowAttack.SetProjectileWeaponDamage(currentProjectile, 0);
		}
	}

	public void ResetCoolDown()
	{
		if (_currentAttackLapse > 0f)
		{
			_currentAttackLapse = 0f;
		}
	}

	public void SetReversed(bool reversed)
	{
		Quirce.SpriteRenderer.flipY = reversed;
		sword.SetReversed(reversed);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
