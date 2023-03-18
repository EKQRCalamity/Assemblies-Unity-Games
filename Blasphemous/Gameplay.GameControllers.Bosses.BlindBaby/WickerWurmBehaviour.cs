using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BlindBaby.AI;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BlindBaby;

public class WickerWurmBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct WickerWurmPhases
	{
		public WICKERWURM_PHASES phaseId;

		public List<WICKERWURM_ATTACKS> availableAttacks;
	}

	[Serializable]
	public struct MultiAttackConfig
	{
		public MULTI_ATTACKS atk;

		public float waitingTimeAfterAttack;
	}

	public enum WICKERWURM_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	public enum MULTI_ATTACKS
	{
		TACKLE,
		BOOMERANG,
		BOUNCING
	}

	public enum WICKERWURM_SIDES
	{
		LEFT,
		RIGHT
	}

	public enum WICKERWURM_ATTACKS
	{
		MOVEMENT,
		MULTI_ATTACK,
		BABY_GRAB,
		HEAD_BOB,
		TAIL_COMBO
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public WICKERWURM_ATTACKS lastAttack;

	[FoldoutGroup("Debug", true, 0)]
	public bool skipIntro;

	[FoldoutGroup("Design settings", 0)]
	public List<WickerWurmPhases> phases;

	[FoldoutGroup("Design settings", 0)]
	public List<MultiAttackConfig> multiAttacksConfig;

	[FoldoutGroup("Design settings", 0)]
	public Vector2 snakeAttackOffset;

	[FoldoutGroup("References", 0)]
	public BlindBabyGrabManager blindBaby;

	[FoldoutGroup("References", 0)]
	public BlindBabyPoints bossfightPoints;

	[FoldoutGroup("References", 0)]
	public WickerWurmTailAttack tailAttack;

	[FoldoutGroup("References", 0)]
	public WickerWurmTailAttack tailAttackTop;

	[FoldoutGroup("References", 0)]
	public EffectsOnBabyDeath deathEffects;

	public BlindBabyPoints.WickerWurmPathConfig headbobPathConfig;

	public bool moveHead;

	private Coroutine currentHeadBobCoroutine;

	private Transform currentTarget;

	private StateMachine<WickerWurmBehaviour> _fsm;

	private State<WickerWurmBehaviour> stIntro;

	private State<WickerWurmBehaviour> stMoving;

	private State<WickerWurmBehaviour> stFixed;

	private State<WickerWurmBehaviour> stStun;

	private State<WickerWurmBehaviour> stDead;

	private Coroutine currentCoroutine;

	private WICKERWURM_PHASES _currentPhase;

	private List<WICKERWURM_ATTACKS> currentlyAvailableAttacks;

	private List<MULTI_ATTACKS> availableMultiAttacks;

	private List<WICKERWURM_ATTACKS> queuedActions;

	public BossStraightProjectileAttack shootingAttack;

	public BossBoomerangProjectileAttack boomerangAttack;

	public BossStraightProjectileAttack bouncingAttack;

	public WICKERWURM_SIDES currentSide;

	public bool lookAtPlayer = true;

	private int multiAttackCounter;

	private BlindBabyPoints.WickerWurmPathConfig currentBobSpline;

	public SplineFollower SplineFollower { get; set; }

	public BodyChainMaster ChainMaster { get; set; }

	public WickerWurm WickerWurm { get; set; }

	public event Action<WickerWurmBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		currentlyAvailableAttacks = new List<WICKERWURM_ATTACKS>
		{
			WICKERWURM_ATTACKS.MOVEMENT,
			WICKERWURM_ATTACKS.MULTI_ATTACK
		};
		availableMultiAttacks = new List<MULTI_ATTACKS>
		{
			MULTI_ATTACKS.TACKLE,
			MULTI_ATTACKS.BOOMERANG,
			MULTI_ATTACKS.BOUNCING
		};
		queuedActions = new List<WICKERWURM_ATTACKS>();
	}

	public override void OnStart()
	{
		base.OnStart();
		WickerWurm = (WickerWurm)Entity;
		SplineFollower = GetComponent<SplineFollower>();
		ChainMaster = GetComponent<BodyChainMaster>();
		ChangeBossState(BOSS_STATES.WAITING);
		stIntro = new WickerWurm_StIntro();
		stFixed = new WickerWurm_StFixed();
		stMoving = new WickerWurm_StMoving();
		stStun = new WickerWurm_StStun();
		stDead = new WickerWurm_StDeath();
		_fsm = new StateMachine<WickerWurmBehaviour>(this, stIntro);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
		if (lookAtPlayer)
		{
			UpdateLookAtPlayer();
		}
	}

	public void StartIntroSequence()
	{
		_fsm.ChangeState(stIntro);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(IntroSequenceCoroutine()));
	}

	private IEnumerator IntroSequenceCoroutine()
	{
		WickerWurm.Audio.PlayBabyAppear_AUDIO();
		blindBaby.BabyIntro();
		yield return new WaitForSeconds(3f);
		ChangePhase(WICKERWURM_PHASES.FIRST);
		MoveRightIntro();
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(WaitForState(stFixed));
		lastAttack = WICKERWURM_ATTACKS.MOVEMENT;
		base.BehaviourTree.StartBehaviour();
		PushToActionQueue(WICKERWURM_ATTACKS.BABY_GRAB);
		StartWaitingPeriod(0.1f);
	}

	private void Shake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private void Wave()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.7f, 0.3f, 2f);
	}

	public float GetHealthPercentage()
	{
		return WickerWurm.CurrentLife / WickerWurm.Stats.Life.Base;
	}

	private void SetPhase(WickerWurmPhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
	}

	private void OnSpawnFinished()
	{
	}

	private void ChangePhase(WICKERWURM_PHASES p)
	{
		WickerWurmPhases phase = phases.Find((WickerWurmPhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
		float healthPercentage = GetHealthPercentage();
		switch (_currentPhase)
		{
		case WICKERWURM_PHASES.FIRST:
			if (healthPercentage < 0.75f)
			{
				ChangePhase(WICKERWURM_PHASES.SECOND);
			}
			break;
		case WICKERWURM_PHASES.SECOND:
			if (healthPercentage < 0.5f)
			{
				ChangePhase(WICKERWURM_PHASES.LAST);
			}
			break;
		}
	}

	private void SetCurrentCoroutine(Coroutine c)
	{
		if (currentCoroutine != null)
		{
			StopCoroutine(currentCoroutine);
		}
		currentCoroutine = c;
	}

	private void ChangeBossState(BOSS_STATES newState)
	{
		currentState = newState;
	}

	private void StartAttackAction()
	{
		ChangeBossState(BOSS_STATES.MID_ACTION);
	}

	private void ActionFinished()
	{
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
		if (this.OnActionFinished != null)
		{
			this.OnActionFinished(this);
		}
	}

	public void LaunchAction(WICKERWURM_ATTACKS atk)
	{
		lastAttack = atk;
		switch (atk)
		{
		case WICKERWURM_ATTACKS.MULTI_ATTACK:
			IssueMultiAttack();
			multiAttackCounter++;
			break;
		case WICKERWURM_ATTACKS.BABY_GRAB:
			multiAttackCounter = 0;
			IssueBabyGrabAttack();
			break;
		case WICKERWURM_ATTACKS.MOVEMENT:
			MoveToOtherSide();
			break;
		case WICKERWURM_ATTACKS.HEAD_BOB:
			IssueHeadBob();
			break;
		case WICKERWURM_ATTACKS.TAIL_COMBO:
			IssueTailStingCombo();
			break;
		}
	}

	private void PushToActionQueue(WICKERWURM_ATTACKS atk)
	{
		queuedActions.Add(atk);
	}

	private WICKERWURM_ATTACKS PopFromActionQueue()
	{
		WICKERWURM_ATTACKS wICKERWURM_ATTACKS = queuedActions[0];
		queuedActions.Remove(wICKERWURM_ATTACKS);
		return wICKERWURM_ATTACKS;
	}

	public WICKERWURM_ATTACKS GetNewAttack()
	{
		if (queuedActions.Count > 0)
		{
			return PopFromActionQueue();
		}
		WICKERWURM_ATTACKS[] array = new WICKERWURM_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<WICKERWURM_ATTACKS> list = new List<WICKERWURM_ATTACKS>(array);
		list.Remove(lastAttack);
		WICKERWURM_ATTACKS result = list[UnityEngine.Random.Range(0, list.Count)];
		if (multiAttackCounter == 2)
		{
			result = WICKERWURM_ATTACKS.BABY_GRAB;
		}
		return result;
	}

	public void LaunchRandomAction()
	{
		if (!WickerWurm.Status.Dead)
		{
			LaunchAction(GetNewAttack());
		}
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public IEnumerator WaitForState(State<WickerWurmBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	private IEnumerator GetIntoStateAndCallback(State<WickerWurmBehaviour> newSt, float waitSeconds, Action callback)
	{
		_fsm.ChangeState(newSt);
		yield return new WaitForSeconds(waitSeconds);
		callback?.Invoke();
	}

	private void StartWaitingPeriod(float seconds)
	{
		ChangeBossState(BOSS_STATES.WAITING);
		SetCurrentCoroutine(StartCoroutine(WaitingPeriodCoroutine(seconds, AfterWaitingPeriod)));
	}

	private IEnumerator WaitingPeriodCoroutine(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		callback();
	}

	private void AfterWaitingPeriod()
	{
		ActionFinished();
	}

	private void IssueTackle()
	{
		ChainMaster.SnakeAttack(snakeAttackOffset);
	}

	private void IssueHeadBob()
	{
		StartAttackAction();
		lookAtPlayer = true;
		SetCurrentCoroutine(StartCoroutine(HeadBobCoroutine(headbobPathConfig, 2, EndHeadBob)));
	}

	private void IssueIdleHeadBob()
	{
		lookAtPlayer = true;
		moveHead = true;
		if (currentHeadBobCoroutine != null)
		{
			StopCoroutine(currentHeadBobCoroutine);
		}
		currentHeadBobCoroutine = StartCoroutine(SimpleHeadBobCoroutine(headbobPathConfig, EndHeadBob));
	}

	private void EndSimpleHeadBob()
	{
		lookAtPlayer = false;
	}

	private void EndHeadBob()
	{
		lookAtPlayer = false;
		StartWaitingPeriod(1f);
	}

	private void StartNewHeadbobLoop()
	{
		currentBobSpline.spline.gameObject.transform.position = base.transform.position;
		WickerWurm.Audio.PlaySnakeLongMove_AUDIO();
		SetFirstPointToPosition(currentBobSpline.spline, alsoLast: true);
		SetSplineConfig(currentBobSpline);
	}

	private IEnumerator WaitHeadbobSplineCompleted()
	{
		while (!SplineFollower.HasFinished() && moveHead)
		{
			yield return null;
		}
	}

	private IEnumerator SimpleHeadBobCoroutine(BlindBabyPoints.WickerWurmPathConfig headbobSpline, Action OnFinish = null)
	{
		moveHead = true;
		bool right = currentSide == WICKERWURM_SIDES.RIGHT;
		currentBobSpline = headbobSpline;
		List<Transform> allPoints = bossfightPoints.GetMultiAttackPoints(currentSide);
		Vector2 point1 = allPoints[0].position;
		Vector2 point2 = allPoints[1].position;
		Vector2 point3 = allPoints[2].position;
		bool first = true;
		while (moveHead)
		{
			if (first)
			{
				Tween tween = ChainMaster.MoveWithEase(point2, 0.5f, Ease.OutQuad);
				yield return tween.WaitForCompletion();
				first = false;
			}
			StartNewHeadbobLoop();
			yield return StartCoroutine(WaitHeadbobSplineCompleted());
		}
		OnFinish?.Invoke();
	}

	private void IssueTailStingCombo()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(TailStingCombo()));
	}

	private void ShootBall()
	{
		Vector2 right = Vector2.right;
		if (currentSide == WICKERWURM_SIDES.RIGHT)
		{
			right *= -1f;
		}
		right += Vector2.up * 0.2f;
		bouncingAttack.Shoot(right);
		base.transform.DOPunchPosition(-right, 0.2f);
	}

	private IEnumerator ShootBallCoroutine(int balls, float delayBeforeBall, float delayBeforeClosingMouth)
	{
		SplineFollower.followActivated = false;
		WickerWurm.AnimatorInyector.PlayGrowl();
		yield return new WaitForSeconds(0.7f);
		SplineFollower.followActivated = false;
		WickerWurm.AnimatorInyector.SetOpen(open: true);
		for (int i = 0; i < balls; i++)
		{
			yield return new WaitForSeconds(delayBeforeBall);
			ShootBall();
		}
		yield return new WaitForSeconds(delayBeforeClosingMouth);
		lookAtPlayer = true;
		WickerWurm.AnimatorInyector.SetOpen(open: false);
	}

	private IEnumerator ShootBoomerangCoroutine(int balls, float delayBeforeShooting, float delayBeforeClosingMouth)
	{
		SplineFollower.followActivated = false;
		WickerWurm.AnimatorInyector.PlayGrowl();
		yield return new WaitForSeconds(0.5f);
		SplineFollower.followActivated = false;
		WickerWurm.AnimatorInyector.SetOpen(open: true);
		for (int i = 0; i < balls; i++)
		{
			yield return new WaitForSeconds(delayBeforeShooting);
			boomerangAttack.Shoot(Core.Logic.Penitent.transform);
		}
		yield return new WaitForSeconds(delayBeforeClosingMouth);
		lookAtPlayer = true;
		WickerWurm.AnimatorInyector.SetOpen(open: false);
	}

	private IEnumerator TailStingCombo()
	{
		bool right = currentSide == WICKERWURM_SIDES.RIGHT;
		float maxSecondsToAttack = 3f;
		float minSecondsToAttack = 2f;
		IssueIdleHeadBob();
		for (int i = 3; i > 0; i--)
		{
			Vector2 tailPoint = Core.Logic.Penitent.transform.position;
			tailAttack.ShowTail(tailPoint, right, 0f);
			float r = UnityEngine.Random.Range(minSecondsToAttack, maxSecondsToAttack);
			yield return new WaitForSeconds(r - 1f);
			lookAtPlayer = false;
			yield return StartCoroutine(ShootBallCoroutine(3, 0.75f, 0.4f));
			SplineFollower.followActivated = true;
			QuickStingAttackToPoint(right, tailPoint);
			yield return new WaitForSeconds(3.5f);
		}
		moveHead = false;
		StartWaitingPeriod(2f);
	}

	private IEnumerator HeadBobCoroutine(BlindBabyPoints.WickerWurmPathConfig headbobSpline, int loops, Action OnFinish = null)
	{
		int i = 0;
		bool right = currentSide == WICKERWURM_SIDES.RIGHT;
		float maxSecondsToAttack = 5f;
		float minSecondsToAttack = 3f;
		float counter = UnityEngine.Random.Range(minSecondsToAttack, maxSecondsToAttack);
		while (i < loops)
		{
			headbobSpline.spline.gameObject.transform.position = base.transform.position;
			WickerWurm.Audio.PlaySnakeLongMove_AUDIO();
			SetFirstPointToPosition(headbobSpline.spline, alsoLast: true);
			SetSplineConfig(headbobSpline);
			yield return new WaitForSeconds(SplineFollower.duration);
			i++;
			if (i == loops - 1)
			{
				WickerWurm.AnimatorInyector.PlayGrowl();
				blindBaby.StartGrabAttack();
			}
		}
		Vector2 tailPoint = Core.Logic.Penitent.transform.position;
		tailAttack.ShowTail(tailPoint, right, 0f);
		yield return new WaitForSeconds(0.4f);
		WickerWurm.AnimatorInyector.PlayAttack();
		yield return new WaitForSeconds(0.4f);
		AttackWithTailStingToPoint(right, tailPoint);
		yield return new WaitForSeconds(6f);
		OnFinish?.Invoke();
	}

	private void AttackWithTailStingToPoint(bool right, Vector2 p)
	{
		Vector2 vector = Vector2.right * 2f * ((!right) ? 1 : (-1));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpion1_AUDIO, 0f));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpion2_AUDIO, 1.5f));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpionHit_AUDIO, 2f));
		StartCoroutine(DelayedTailAttack(tailAttack, p, 0f, right, 4f));
		StartCoroutine(DelayedTailAttack(tailAttackTop, p + vector, 1f, right, 1f));
	}

	private void AttackWithTailSting(bool right)
	{
		Vector3 vector = Vector2.right * 2f * ((!right) ? 1 : (-1));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpion1_AUDIO, 0f));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpion2_AUDIO, 1.5f));
		StartCoroutine(DelayedFunction(WickerWurm.Audio.PlayScorpionHit_AUDIO, 2f));
		StartCoroutine(DelayedTailAttack(tailAttack, Core.Logic.Penitent.transform.position, 0f, right, 4f));
		StartCoroutine(DelayedTailAttack(tailAttackTop, Core.Logic.Penitent.transform.position + vector, 1f, right, 1f));
	}

	private void QuickStingAttack(bool right)
	{
		WickerWurm.Audio.PlayScorpion1_AUDIO();
		StartCoroutine(DelayedTailAttack(tailAttack, Core.Logic.Penitent.transform.position, 0f, right, 0.5f));
	}

	private void QuickStingAttackToPoint(bool right, Vector2 p)
	{
		WickerWurm.Audio.PlayScorpion1_AUDIO();
		StartCoroutine(DelayedTailAttack(tailAttack, p, 0f, right, 0.5f));
	}

	private IEnumerator DelayedFunction(Action function, float delay)
	{
		yield return new WaitForSeconds(delay);
		function();
	}

	private IEnumerator DelayedTailAttack(WickerWurmTailAttack atk, Vector3 point, float seconds, bool right, float delay)
	{
		yield return new WaitForSeconds(seconds);
		atk.TailAttack(point, right, delay);
	}

	public void MoveToOtherSide()
	{
		StartAttackAction();
		if (currentSide == WICKERWURM_SIDES.RIGHT)
		{
			MoveLeft();
		}
		else
		{
			MoveRight();
		}
	}

	public void MoveRightIntro()
	{
		WickerWurm.Audio.PlayAlive_AUDIO();
		BlindBabyPoints.WickerWurmPathConfig pathConfig = bossfightPoints.GetPathConfig(0);
		StartCoroutine(MoveToFixPosition(pathConfig, bossfightPoints.GetPathConfig(BlindBabyPoints.WURM_PATHS.RIGHT_TO_FIX)));
	}

	public void MoveRight()
	{
		StartCoroutine(MoveToFixPosition(bossfightPoints.GetPathConfig(BlindBabyPoints.WURM_PATHS.TO_RIGHT), bossfightPoints.GetPathConfig(BlindBabyPoints.WURM_PATHS.RIGHT_TO_FIX), OnMovementManeouverFinished));
	}

	public void MoveLeft()
	{
		StartCoroutine(MoveToFixPosition(bossfightPoints.GetPathConfig(BlindBabyPoints.WURM_PATHS.TO_LEFT), bossfightPoints.GetPathConfig(BlindBabyPoints.WURM_PATHS.LEFT_TO_FIX), OnMovementManeouverFinished));
	}

	private void SetFirstPointToPosition(BezierSpline spline, bool alsoLast = false)
	{
		Vector2 vector = spline.points[1] - spline.points[0];
		ref Vector3 reference = ref spline.points[0];
		reference = spline.transform.InverseTransformPoint(base.transform.position);
		ref Vector3 reference2 = ref spline.points[1];
		reference2 = spline.points[0] + (Vector3)vector;
		if (alsoLast)
		{
			int num = spline.points.Length - 1;
			Vector2 vector2 = spline.points[num - 1] - spline.points[num];
			ref Vector3 reference3 = ref spline.points[num];
			reference3 = spline.transform.InverseTransformPoint(base.transform.position);
		}
	}

	private IEnumerator MoveToFixPosition(BlindBabyPoints.WickerWurmPathConfig movementSplineConfig, BlindBabyPoints.WickerWurmPathConfig fixSplineConfig, Action OnFinish = null)
	{
		bool right = currentSide == WICKERWURM_SIDES.LEFT;
		_fsm.ChangeState(stMoving);
		WickerWurm.Audio.PlaySnakeLongMove_AUDIO();
		SetFirstPointToPosition(movementSplineConfig.spline);
		ChainMaster.FlipAllSprites(!right);
		SetSplineConfig(movementSplineConfig);
		yield return new WaitForSeconds(SplineFollower.duration);
		WickerWurm.Audio.PlayPreAttack_AUDIO();
		ChainMaster.FlipAllSprites(right);
		SetSplineConfig(fixSplineConfig);
		yield return new WaitForSeconds(SplineFollower.duration);
		_fsm.ChangeState(stFixed);
		currentSide = fixSplineConfig.side;
		OnFinish?.Invoke();
	}

	private void OnMovementManeouverFinished()
	{
		StartWaitingPeriod(0.2f);
	}

	private void SetSplineConfig(BlindBabyPoints.WickerWurmPathConfig config)
	{
		SplineFollower.spline = config.spline;
		SplineFollower.currentCounter = 0f;
		SplineFollower.duration = config.duration;
		SplineFollower.movementCurve = config.curve;
		SplineFollower.followActivated = true;
	}

	private void IssueBabyGrabAttack()
	{
		StartAttackAction();
		IssueIdleHeadBob();
		SetCurrentCoroutine(StartCoroutine(BabyGrabSequence()));
	}

	private IEnumerator BabyGrabSequence()
	{
		float dir = ((currentSide == WICKERWURM_SIDES.LEFT) ? 1 : (-1));
		blindBaby.StartGrabAttack();
		_fsm.ChangeState(stStun);
		yield return new WaitForSeconds(7f);
		_fsm.ChangeState(stFixed);
		moveHead = false;
		StartWaitingPeriod(0.5f);
	}

	private void IssueMultiAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(MultiAttackCoroutine()));
	}

	private IEnumerator MultiAttackCoroutine()
	{
		List<MULTI_ATTACKS> availableAttacks = new List<MULTI_ATTACKS>
		{
			MULTI_ATTACKS.BOOMERANG,
			MULTI_ATTACKS.BOUNCING,
			MULTI_ATTACKS.TACKLE
		};
		int counter = 3;
		List<Transform> allPoints = bossfightPoints.GetMultiAttackPoints(currentSide);
		Vector2 point1 = allPoints[0].position;
		Vector2 point2 = allPoints[1].position;
		Vector2 point3 = allPoints[2].position;
		Debug.DrawLine(point1, point1 + Vector2.up, Color.red, 2f);
		Debug.DrawLine(point2, point2 + Vector2.up, Color.yellow, 2f);
		Debug.DrawLine(point3, point3 + Vector2.up, Color.green, 2f);
		List<Vector2> points = new List<Vector2> { point1, point2, point3 };
		int r = UnityEngine.Random.Range(0, availableMultiAttacks.Count);
		for (int i = 0; i < counter; i++)
		{
			WickerWurm.Audio.PlaySnakeMove_AUDIO();
			Tween tween = ChainMaster.MoveWithEase(points[i], 0.5f, Ease.InOutQuad);
			yield return tween.WaitForCompletion();
			MULTI_ATTACKS curAttack = availableMultiAttacks[r];
			if (curAttack == MULTI_ATTACKS.BOUNCING)
			{
				yield return ShootBallCoroutine(3, 0.6f, 0.6f);
			}
			else if (curAttack == MULTI_ATTACKS.BOOMERANG)
			{
				yield return ShootBoomerangCoroutine(1, 1f, 0.4f);
			}
			else
			{
				LaunchSingleAttack(curAttack);
			}
			yield return new WaitForSeconds(multiAttacksConfig.Find((MultiAttackConfig x) => x.atk == curAttack).waitingTimeAfterAttack);
		}
		StartWaitingPeriod(0.2f);
	}

	private void SnakeAttackCallback()
	{
		WickerWurm.Audio.PlayAttack_AUDIO();
	}

	private void LaunchSingleAttack(MULTI_ATTACKS atk)
	{
		switch (atk)
		{
		case MULTI_ATTACKS.TACKLE:
			ChainMaster.SnakeAttack(snakeAttackOffset, SnakeAttackCallback);
			break;
		}
	}

	public void EnterStun()
	{
	}

	public void ExitStun()
	{
	}

	public void UpdateLookAtPlayer()
	{
		Vector2 vector = Core.Logic.Penitent.transform.position + Vector3.up * 3.5f;
		if ((currentSide == WICKERWURM_SIDES.RIGHT && vector.x < base.transform.position.x - 2f) || (currentSide == WICKERWURM_SIDES.LEFT && vector.x < base.transform.position.x + 2f))
		{
			ChainMaster.LookAtTarget(vector, 2f);
		}
		else
		{
			ChainMaster.LookAtTarget(base.transform.position + Vector3.right * ((currentSide != WICKERWURM_SIDES.RIGHT) ? 1 : (-1)), 2f);
		}
	}

	public void UpdateLookAtPath()
	{
		ChainMaster.LookAtTarget((Vector2)base.transform.position + SplineFollower.GetDirection());
	}

	public void EnableDamage(bool enable)
	{
		WickerWurm.DamageArea.DamageAreaCollider.enabled = enable;
	}

	public void SetMoving(bool moving)
	{
		SplineFollower.followActivated = moving;
	}

	public void AffixBody(bool affix)
	{
		ChainMaster.AffixBody(affix, 13);
	}

	public void StartDeathSequence()
	{
		ClearAll();
		WickerWurm.AnimatorInyector.Death();
		WickerWurm.Audio.PlayDeath_AUDIO();
		blindBaby.PlayDeath();
		SetCurrentCoroutine(StartCoroutine(DeathSequenceCoroutine()));
	}

	private void ClearAll()
	{
		StopAllCoroutines();
		blindBaby.StopAllCoroutines();
	}

	private IEnumerator DeathSequenceCoroutine()
	{
		Debug.Log("STARTING DEATH SEQUENCE");
		StartAttackAction();
		WickerWurm.Behaviour.StopBehaviour();
		ChainMaster.StartDeathSequence();
		deathEffects.ActivateEffects();
		WickerWurm.Audio.PlayFire_AUDIO();
		Core.Logic.Penitent.Status.Invulnerable = true;
		Core.Logic.Penitent.Status.Unattacable = true;
		yield return new WaitForSeconds(6f);
		Core.Logic.Penitent.Status.Invulnerable = false;
		Core.Logic.Penitent.Status.Unattacable = false;
	}

	public void Death()
	{
		_fsm.ChangeState(stDead);
	}

	public override void Idle()
	{
		throw new NotImplementedException();
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

	public override void Damage()
	{
		WickerWurm.Audio.PlayHit_AUDIO();
		CheckNextPhase();
		if (CanBeRepulloed())
		{
			ChainMaster.Repullo();
			StartWaitingPeriod(1.5f);
		}
	}

	public bool HasGrabbedPenitent()
	{
		return blindBaby.HasGrabbedPenitent();
	}

	private bool CanBeRepulloed()
	{
		return _fsm.IsInState(stFixed) && ChainMaster.IsAttacking;
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void OnShootPointTouched()
	{
		shootingAttack.Shoot(Vector2.down);
	}

	public void OnDrawGizmos()
	{
	}
}
