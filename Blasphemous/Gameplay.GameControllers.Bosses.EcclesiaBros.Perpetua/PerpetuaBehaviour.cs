using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua.AI;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;

public class PerpetuaBehaviour : EnemyBehaviour
{
	public enum Perpetua_ATTACKS
	{
		SPEAR_DASH,
		LIGHTNING_STRIKE,
		SINGLE_SPEAR
	}

	private PerpetuaAttackConfig _currentConfig;

	private int _currentRepetitions;

	private Vector2 _flapPoint;

	private StateMachine<PerpetuaBehaviour> _fsm;

	public BezierSpline bezierL;

	public BezierSpline bezierR;

	[FoldoutGroup("Design settings", true, 0)]
	public float chargePositioningRandomRadius = 1f;

	public PerpetuaScriptableFightConfig config;

	private Coroutine currentCoroutine;

	public List<Perpetua_ATTACKS> currentlyAvailableAttacks;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	private Transform currentTarget;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack dashAttack;

	public Vector2 dashOffset;

	public float followBrakeRadius = 1f;

	public float followBrakeSeconds = 1f;

	public float followPlayerAcceleration = 5f;

	public Vector2 followPlayerMaxOffset;

	public float followPlayerMaxSpeed = 10f;

	public Vector2 followPlayerMinOffset;

	public Vector2 followPlayerOffset;

	public Vector2 followPlayerVelocity;

	[FoldoutGroup("Attacks", true, 0)]
	public BossInstantProjectileAttack instantProjectileAttack;

	private bool isDashing;

	public bool IsSpelling;

	private bool isSummoningAttack;

	[FoldoutGroup("Debug", true, 0)]
	public Perpetua_ATTACKS lastAttack;

	private List<Perpetua_ATTACKS> queuedActions;

	public SplineFollower splineFollower;

	private State<PerpetuaBehaviour> stAction;

	private State<PerpetuaBehaviour> stDeath;

	private State<PerpetuaBehaviour> stFlapToPoint;

	private State<PerpetuaBehaviour> stFollowPlayer;

	private State<PerpetuaBehaviour> stIntro;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack summonAttack;

	private Collider2D[] colliders;

	private ContactFilter2D contactFilter;

	private Collider2D collider;

	public Perpetua Perpetua { get; set; }

	private Vector3 GetDashDir
	{
		get
		{
			Vector3 right = Vector3.right;
			if (Core.Logic.Penitent.transform.position.x < base.transform.position.x)
			{
				right *= -1f;
			}
			return right;
		}
	}

	private Vector3 DashOffset
	{
		get
		{
			float x = ((Entity.Status.Orientation != EntityOrientation.Left) ? (0f - dashOffset.x) : dashOffset.x);
			return new Vector2(x, dashOffset.y);
		}
	}

	public event Action<PerpetuaBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		stIntro = new Perpetua_StIntro();
		stFlapToPoint = new Perpetua_StFlapToPoint();
		stAction = new Perpetua_StAction();
		stFollowPlayer = new Perpetua_StFollowPlayer();
		stDeath = new Perpetua_StDeath();
		_fsm = new StateMachine<PerpetuaBehaviour>(this, stIntro);
		Perpetua = (Perpetua)Entity;
		colliders = new Collider2D[1];
		contactFilter = new ContactFilter2D
		{
			layerMask = LayerMask.NameToLayer("Penitent"),
			useLayerMask = true,
			useTriggers = true
		};
		collider = GetComponent<BoxCollider2D>();
	}

	public override void OnStart()
	{
		base.OnStart();
		ChangeBossState(BOSS_STATES.WAITING);
		bezierL.transform.SetParent(base.transform.parent);
		bezierR.transform.SetParent(base.transform.parent);
		dashAttack.OnDashAdvancedEvent += OnDashing;
		dashAttack.OnDashFinishedEvent += OnStopDash;
		collider.enabled = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Entity.Status.Dead)
		{
			base.BehaviourTree.StopBehaviour();
			return;
		}
		if ((bool)Core.Logic.Penitent)
		{
			LookAtTarget(Core.Logic.Penitent.transform.position);
		}
		_fsm.DoUpdate();
	}

	public void InitIntro()
	{
		SetCurrentCoroutine(StartCoroutine(IntroSequence()));
	}

	private IEnumerator IntroSequence()
	{
		yield return null;
		Vector3 center = Perpetua.PerpetuaPoints.centerPoint.position;
		Perpetua.AnimatorInyector.Appear();
		yield return new WaitForSeconds(1f);
		collider.enabled = true;
		yield return new WaitForSeconds(1f);
		Perpetua.AnimatorInyector.Spell();
		yield return new WaitForSeconds(1f);
		LaunchLightningSpear(center);
		yield return new WaitForSeconds(2f);
		StartBehaviour();
		StartWaitingPeriod(0.1f);
	}

	private void LaunchLightningSpear(Vector3 p)
	{
		instantProjectileAttack.Shoot(p + Vector3.up * 16f, Vector2.down);
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
		_fsm.ChangeState(stAction);
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

	public void LaunchAction(Perpetua_ATTACKS atk)
	{
		_currentConfig = config.GetAttack(atk);
		switch (atk)
		{
		case Perpetua_ATTACKS.SPEAR_DASH:
			IssueSpearDash();
			break;
		case Perpetua_ATTACKS.LIGHTNING_STRIKE:
			IssueMultiLightning();
			break;
		case Perpetua_ATTACKS.SINGLE_SPEAR:
			IssueSpear();
			break;
		}
		lastAttack = atk;
	}

	public Perpetua_ATTACKS GetNewAttack()
	{
		Perpetua_ATTACKS[] array = new Perpetua_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<Perpetua_ATTACKS> list = new List<Perpetua_ATTACKS>(array);
		if (list.Count > 1)
		{
			list.Remove(lastAttack);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public void LaunchRandomAction()
	{
		LaunchAction(GetNewAttack());
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public IEnumerator WaitForState(State<PerpetuaBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	private void StartWaitingPeriod(float seconds)
	{
		ChangeBossState(BOSS_STATES.WAITING);
		SetCurrentCoroutine(StartCoroutine(WaitingPeriodCoroutine(seconds, AfterWaitingPeriod)));
		_fsm.ChangeState(stFollowPlayer);
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

	private void IssueSpearDash()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PrepareSpearDash()));
	}

	private IEnumerator PrepareSpearDash()
	{
		Vector3 targetPos = Core.Logic.Penitent.transform.position + DashOffset;
		FlapToPoint(targetPos, GetDashDir.x < 0f);
		Perpetua.AnimatorInyector.Flap(activate: true);
		yield return StartCoroutine(WaitForState(stAction));
		Perpetua.IsGuarding = true;
		Perpetua.AnimatorInyector.Flap(activate: false);
		Perpetua.AnimatorInyector.ChargeDash(activate: true);
		yield return new WaitForSeconds(_currentConfig.anticipationWait);
		Perpetua.IsGuarding = false;
		Perpetua.AnimatorInyector.Dash();
		dashAttack.OnDashFinishedEvent += OnDashFinished;
		dashAttack.Dash(base.transform, GetDashDir, 12f);
		AttackOnStartDash();
	}

	private void AttackOnStartDash()
	{
		if (Physics2D.OverlapCollider(dashAttack.AttackArea.WeaponCollider, contactFilter, colliders) > 0)
		{
			Perpetua.Target.GetComponentInParent<IDamageable>().Damage(dashAttack.BossDashHit);
		}
	}

	private void OnDashing(float value)
	{
		isDashing = true;
	}

	private void OnStopDash()
	{
		isDashing = false;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (isDashing)
		{
			return;
		}
		if (targetPos.x > Entity.transform.position.x)
		{
			if (Entity.Status.Orientation != 0)
			{
				Entity.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (Entity.Status.Orientation != EntityOrientation.Left)
		{
			Entity.SetOrientation(EntityOrientation.Left);
		}
	}

	private void OnDashFinished()
	{
		Perpetua.AnimatorInyector.ChargeDash(activate: false);
		StartWaitingPeriod(_currentConfig.recoveryWait);
	}

	private void IssueMultiLightning()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PrepareSpearSummon()));
	}

	private Vector2 GetSummonPoint()
	{
		Vector2 centerPos = Perpetua.PerpetuaPoints.GetCenterPos();
		float num = 5f;
		return centerPos + Vector2.right * (UnityEngine.Random.Range(-1f, 1f) * num);
	}

	private IEnumerator PrepareSpearSummon()
	{
		isSummoningAttack = true;
		Perpetua.AnimatorInyector.Flap(activate: true);
		FlapToPoint(Perpetua.PerpetuaPoints.GetCenterPos() + Vector2.up * 4f, rightCurve: true);
		yield return StartCoroutine(WaitForState(stAction));
		Perpetua.AnimatorInyector.Flap(activate: false);
		yield return new WaitForSeconds(_currentConfig.anticipationWait);
		Perpetua.AnimatorInyector.Spell();
		yield return new WaitForSeconds(0.75f);
		StartSummonLoop();
	}

	private void StartSummonLoop()
	{
		_currentRepetitions = 1 + _currentConfig.repetitions;
		SetCurrentCoroutine(StartCoroutine(SpearSummonLoop()));
	}

	private IEnumerator SpearSummonLoop()
	{
		while (_currentRepetitions >= 0)
		{
			Vector2 p = GetSummonPoint();
			summonAttack.SummonAreaOnPoint(p);
			_currentRepetitions--;
			yield return new WaitForSeconds(_currentConfig.waitBetweenRepetitions);
		}
		OnSummonFinished();
	}

	private void OnSummonFinished()
	{
		isSummoningAttack = false;
		StartWaitingPeriod(_currentConfig.recoveryWait);
	}

	private void IssueSpear()
	{
		isSummoningAttack = true;
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PrepareSingleSpearSummon()));
	}

	private IEnumerator PrepareSingleSpearSummon()
	{
		yield return new WaitForSeconds(_currentConfig.anticipationWait);
		Perpetua.AnimatorInyector.Spell();
		yield return new WaitForSeconds(0.5f);
		Vector2 t = GetTarget().position;
		yield return new WaitForSeconds(0.25f);
		LaunchLightningSpear(t);
		OnSingleSpearFinished();
	}

	private void OnSingleSpearFinished()
	{
		isSummoningAttack = false;
		StartWaitingPeriod(_currentConfig.recoveryWait);
	}

	public void ResetVelocity()
	{
		followPlayerVelocity = Vector2.zero;
	}

	public void ChooseSide()
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		float num2 = ((num > 0.5f) ? 1 : (-1));
		followPlayerOffset = new Vector2(UnityEngine.Random.Range(followPlayerMinOffset.x, followPlayerMaxOffset.x), UnityEngine.Random.Range(followPlayerMinOffset.y, followPlayerMaxOffset.y));
		followPlayerOffset.x = Mathf.Abs(followPlayerOffset.x) * num2;
	}

	public void FollowPlayer()
	{
		if (!IsSpelling)
		{
			Vector2 vector = (Vector2)GetTarget().position + followPlayerOffset;
			Vector2 vector2 = (vector - (Vector2)base.transform.position).normalized * followPlayerAcceleration * Time.deltaTime;
			if (Vector2.Distance(vector, base.transform.position) < followBrakeRadius)
			{
				vector2 = -followPlayerVelocity * Time.deltaTime * (1f / followBrakeSeconds);
			}
			followPlayerVelocity += vector2;
			followPlayerVelocity = Vector2.ClampMagnitude(followPlayerVelocity, followPlayerMaxSpeed);
			Vector3 vector3 = followPlayerVelocity * Time.deltaTime;
			base.transform.position += vector3;
		}
	}

	private void SetFirstPointToPosition(BezierSpline spline, Vector2 position)
	{
		Vector2 vector = spline.points[1] - spline.points[0];
		ref Vector3 reference = ref spline.points[0];
		reference = spline.transform.InverseTransformPoint(position);
		ref Vector3 reference2 = ref spline.points[1];
		reference2 = spline.points[0] + (Vector3)vector;
	}

	private void SetLastPointToPosition(BezierSpline spline, Vector2 position)
	{
		int num = spline.points.Length - 1;
		Vector2 vector = spline.points[num - 1] - spline.points[num];
		ref Vector3 reference = ref spline.points[num];
		reference = spline.transform.InverseTransformPoint(position);
		ref Vector3 reference2 = ref spline.points[num - 1];
		reference2 = spline.points[num] + (Vector3)vector;
	}

	public void FlapToPoint(Vector2 point, bool rightCurve)
	{
		BezierSpline bezierSpline = null;
		bezierSpline = ((!rightCurve) ? bezierL : bezierR);
		SetFirstPointToPosition(bezierSpline, base.transform.position);
		SetLastPointToPosition(bezierSpline, point);
		splineFollower.spline = bezierSpline;
		splineFollower.currentCounter = 0f;
		splineFollower.followActivated = true;
		splineFollower.OnMovementCompleted += OnMovementCompleted;
		_flapPoint = point;
		_fsm.ChangeState(stFlapToPoint);
	}

	private void OnMovementCompleted()
	{
		ChangeToAction();
		splineFollower.OnMovementCompleted -= OnMovementCompleted;
	}

	public bool IsCloseToFlapPoint(float closeRange = 0.5f)
	{
		float num = Vector2.Distance(base.transform.position, _flapPoint);
		return num < closeRange;
	}

	public void ActivateSteering(bool enabled)
	{
		Perpetua.autonomousAgent.enabled = enabled;
		if (enabled)
		{
			Perpetua.autonomousAgent.currentVelocity = Vector3.zero;
		}
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	private void OnDestroy()
	{
		dashAttack.OnDashAdvancedEvent -= OnDashing;
		dashAttack.OnDashFinishedEvent -= OnStopDash;
	}

	public void Death()
	{
		StopAllCoroutines();
		summonAttack.StopAllCoroutines();
		summonAttack.ClearAll();
		Perpetua.AnimatorInyector.Death();
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
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
