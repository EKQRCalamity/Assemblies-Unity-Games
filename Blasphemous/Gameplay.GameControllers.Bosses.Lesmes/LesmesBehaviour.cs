using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Lesmes;

public class LesmesBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct LesmesAttackConfig
	{
		public Lesmes_ATTACKS attackType;

		public bool requiresReposition;

		public bool invertedReposition;

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

	public enum Lesmes_STATE
	{
		SWORD,
		NO_SWORD
	}

	public enum Lesmes_ATTACKS
	{
		DASH,
		MULTIDASH,
		TELEPORT,
		PATH_THROW,
		SWORD_TOSS,
		MULTI_TELEPORT,
		SWORD_RECOVERY
	}

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float MinAttackDistance = 2f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float AttackCoolDown = 2f;

	private float _currentAttackLapse;

	private Transform currentTarget;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack dashAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossTeleportAttack teleportAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossTeleportAttack multiTeleportAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack plungeAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack multiDashAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossSplineFollowingProjectileAttack splineThrowAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossStraightProjectileAttack tossAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossDashAttack quickMove;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack areaSummonAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public List<LesmesAttackConfig> attacksConfiguration;

	[FoldoutGroup("Traits", true, 0)]
	public EntityMotionChecker motionChecker;

	private List<Lesmes_ATTACKS> currentlyAvailableAttacks;

	private List<Lesmes_ATTACKS> queuedActions;

	private Lesmes_STATE currentLesmesState;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public Lesmes_ATTACKS lastAttack;

	private Transform currentHang;

	private Coroutine currentCoroutine;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float DistanceToTarget { get; private set; }

	public Lesmes Lesmes { get; private set; }

	public bool Awaken { get; private set; }

	public int multiTeleportAttackNumber { get; private set; }

	private int dashRemainings { get; set; }

	public override void OnAwake()
	{
		base.OnAwake();
		Lesmes = (Lesmes)Entity;
		currentlyAvailableAttacks = GetCurrentStateAttacks();
	}

	private List<Lesmes_ATTACKS> GetCurrentStateAttacks()
	{
		List<Lesmes_ATTACKS> list;
		if (currentLesmesState == Lesmes_STATE.SWORD)
		{
			list = new List<Lesmes_ATTACKS>();
			list.Add(Lesmes_ATTACKS.DASH);
			list.Add(Lesmes_ATTACKS.TELEPORT);
			list.Add(Lesmes_ATTACKS.PATH_THROW);
			list.Add(Lesmes_ATTACKS.SWORD_TOSS);
			return list;
		}
		list = new List<Lesmes_ATTACKS>();
		list.Add(Lesmes_ATTACKS.MULTI_TELEPORT);
		list.Add(Lesmes_ATTACKS.MULTIDASH);
		list.Add(Lesmes_ATTACKS.SWORD_RECOVERY);
		return list;
	}

	public override void OnStart()
	{
		base.OnStart();
		ChangeBossState(BOSS_STATES.WAITING);
		SetLesmesState(Lesmes_STATE.SWORD);
		StartWaitingPeriod(1f);
	}

	private void Update()
	{
		DistanceToTarget = Vector2.Distance(Lesmes.transform.position, Lesmes.Target.transform.position);
		if (!base.IsAttacking)
		{
			_currentAttackLapse += Time.deltaTime;
		}
		if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !Awaken)
		{
			Awaken = true;
			Debug.Log("Lesmes: STARTING BEHAVIOR TREE");
			base.BehaviourTree.StartBehaviour();
		}
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
		ChangeBossState(BOSS_STATES.MID_ACTION);
	}

	public Lesmes_ATTACKS GetNewAttack()
	{
		Lesmes_ATTACKS[] array = new Lesmes_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<Lesmes_ATTACKS> list = new List<Lesmes_ATTACKS>(array);
		list.Remove(lastAttack);
		if (lastAttack == Lesmes_ATTACKS.PATH_THROW)
		{
			list.Remove(Lesmes_ATTACKS.TELEPORT);
		}
		else if (lastAttack == Lesmes_ATTACKS.SWORD_RECOVERY)
		{
			list.Remove(Lesmes_ATTACKS.TELEPORT);
			list.Remove(Lesmes_ATTACKS.SWORD_TOSS);
		}
		else if (lastAttack == Lesmes_ATTACKS.SWORD_TOSS)
		{
			list.Remove(Lesmes_ATTACKS.SWORD_RECOVERY);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public void LaunchRandomAction()
	{
		LaunchAction(GetNewAttack());
	}

	private void QueuedActionsPush(Lesmes_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<Lesmes_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private Lesmes_ATTACKS QueuedActionsPop()
	{
		Lesmes_ATTACKS lesmes_ATTACKS = queuedActions[0];
		queuedActions.Remove(lesmes_ATTACKS);
		return lesmes_ATTACKS;
	}

	public void LaunchAction(Lesmes_ATTACKS atk, bool checkReposition = true)
	{
		if (attacksConfiguration.Find((LesmesAttackConfig x) => x.attackType == atk).requiresReposition && checkReposition)
		{
			Vector3 zero = Vector3.zero;
			if (atk != 0)
			{
			}
			QueuedActionsPush(atk);
		}
		else
		{
			lastAttack = atk;
			if (atk == Lesmes_ATTACKS.DASH)
			{
				DashAttack();
			}
		}
	}

	public override void Idle()
	{
		Debug.Log("Lesmes: IDLE");
		StopMovement();
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
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
	}

	public void Reposition(Vector3 point)
	{
		StartAttackAction();
		LookAtTarget(point);
		Debug.Log("Lesmes: REPOSITION");
		Lesmes.AnimatorInyector.Dash(state: true);
		quickMove.OnDashFinishedEvent += OnRepositionFinished;
		quickMove.DashToPoint(base.transform, point);
	}

	private void OnRepositionFinished()
	{
		quickMove.OnDashFinishedEvent -= OnRepositionFinished;
		Lesmes.AnimatorInyector.Dash(state: false);
		Lesmes_ATTACKS atk = QueuedActionsPop();
		LaunchAction(atk, checkReposition: false);
	}

	public void DashAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingDashCoroutine()));
	}

	private IEnumerator PreparingDashCoroutine()
	{
		Lesmes.AnimatorInyector.BigDashPreparation();
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		yield return new WaitForSeconds(attacksConfiguration.Find((LesmesAttackConfig x) => x.attackType == Lesmes_ATTACKS.DASH).preparationSeconds);
		Debug.Log("Lesmes: DASH ATTACK");
		Lesmes.AnimatorInyector.Dash(state: true);
		dashAttack.OnDashFinishedEvent += OnDashAttackFinished;
		float d = Mathf.Sign(currentTarget.position.x - base.transform.position.x);
		dashAttack.Dash(base.transform, Vector3.right * d, 20f);
	}

	private void OnDashAttackFinished()
	{
		dashAttack.OnDashFinishedEvent -= OnDashAttackFinished;
		Lesmes.AnimatorInyector.Dash(state: false);
		StartWaitingPeriod(GetAttackConfig(Lesmes_ATTACKS.DASH).waitingSecondsAfterAttack);
	}

	public void NDashAttack()
	{
		StartAttackAction();
		dashRemainings = GetAttackConfig(Lesmes_ATTACKS.MULTIDASH).multiAttackTimes;
		SetCurrentCoroutine(StartCoroutine(PreparingMultiDash()));
	}

	private IEnumerator PreparingMultiDash()
	{
		Lesmes.AnimatorInyector.BigDashPreparation();
		yield return new WaitForSeconds(attacksConfiguration.Find((LesmesAttackConfig x) => x.attackType == Lesmes_ATTACKS.MULTIDASH).preparationSeconds);
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		Debug.Log("Lesmes: NDASH ATTACK");
		Lesmes.AnimatorInyector.Dash(state: true);
		multiDashAttack.OnDashFinishedEvent += OnMultiDashAttackFinished;
		float d = Mathf.Sign(currentTarget.position.x - base.transform.position.x);
		multiDashAttack.Dash(base.transform, Vector3.right * d, 20f);
	}

	private void OnMultiDashAttackFinished()
	{
		dashRemainings--;
		multiDashAttack.OnDashFinishedEvent -= OnMultiDashAttackFinished;
		Lesmes.AnimatorInyector.Dash(state: false);
		LesmesAttackConfig attackConfig = GetAttackConfig(Lesmes_ATTACKS.MULTIDASH);
		if (dashRemainings > 0)
		{
			SetCurrentCoroutine(StartCoroutine(PreparingMultiDash()));
		}
		else
		{
			StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack);
		}
	}

	public void TeleportAttack()
	{
		StartAttackAction();
		currentTarget = GetTarget();
		teleportAttack.OnTeleportInEvent += OnTeleportIn;
		Debug.Log("Lesmes: Teleport OUT");
		Lesmes.AnimatorInyector.TeleportOut();
		teleportAttack.Use(base.transform, currentTarget, Vector3.up * 4f);
	}

	private void OnTeleportIn()
	{
		Lesmes.AnimatorInyector.TeleportIn();
		teleportAttack.OnTeleportInEvent -= OnTeleportIn;
		PlungeAttack();
	}

	public void PlungeAttack()
	{
		Debug.Log("Lesmes: PLUNGE ATTACK");
		Lesmes.AnimatorInyector.Plunge(state: true);
		currentTarget = GetTarget();
		plungeAttack.OnDashFinishedEvent += OnPlungeAttackFinished;
		plungeAttack.Dash(base.transform, Vector3.down, 10f);
	}

	private void OnPlungeAttackFinished()
	{
		plungeAttack.OnDashFinishedEvent -= OnPlungeAttackFinished;
		Lesmes.AnimatorInyector.Plunge(state: false);
		StartWaitingPeriod(GetAttackConfig(Lesmes_ATTACKS.TELEPORT).waitingSecondsAfterAttack);
	}

	public void MultiTeleportAttack()
	{
		StartAttackAction();
		multiTeleportAttackNumber = GetAttackConfig(Lesmes_ATTACKS.MULTI_TELEPORT).multiAttackTimes;
		SetCurrentCoroutine(StartCoroutine(PreparingMultiTeleport()));
	}

	private IEnumerator PreparingMultiTeleport()
	{
		currentTarget = GetTarget();
		multiTeleportAttack.OnTeleportInEvent += OnMultiTeleportIn;
		Debug.Log("Lesmes: MULTI Teleport OUT");
		Lesmes.AnimatorInyector.TeleportOut();
		multiTeleportAttack.Use(base.transform, currentTarget, Vector3.up * 4f);
		yield return null;
	}

	private void OnMultiTeleportIn()
	{
		Debug.Log("Lesmes: MULTI Teleport IN");
		Lesmes.AnimatorInyector.TeleportIn();
		multiTeleportAttack.OnTeleportInEvent -= OnMultiTeleportIn;
		MultiPlungeAttack();
	}

	public void MultiPlungeAttack()
	{
		Debug.Log("Lesmes: MULTI PLUNGE ATTACK");
		Lesmes.AnimatorInyector.Plunge(state: true);
		currentTarget = GetTarget();
		plungeAttack.OnDashFinishedEvent += OnMultiPlungeAttackFinished;
		plungeAttack.Dash(base.transform, Vector3.down, 10f);
	}

	private void OnMultiPlungeAttackFinished()
	{
		Debug.Log("Lesmes: PLUNGE ATTACK FINISHED NUMBER:" + multiTeleportAttackNumber);
		plungeAttack.OnDashFinishedEvent -= OnMultiPlungeAttackFinished;
		Lesmes.AnimatorInyector.Plunge(state: false);
		multiTeleportAttackNumber--;
		if (multiTeleportAttackNumber > 0)
		{
			SetCurrentCoroutine(StartCoroutine(PreparingMultiTeleport()));
		}
		else
		{
			StartWaitingPeriod(GetAttackConfig(Lesmes_ATTACKS.MULTI_TELEPORT).waitingSecondsAfterAttack);
		}
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

	private LesmesAttackConfig GetAttackConfig(Lesmes_ATTACKS atk)
	{
		return attacksConfiguration.Find((LesmesAttackConfig x) => x.attackType == atk);
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public bool TargetCanBeVisible()
	{
		float num = Lesmes.Target.transform.position.y - Lesmes.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Lesmes.transform.position.x)
		{
			if (Lesmes.Status.Orientation != 0)
			{
				Lesmes.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (Lesmes.Status.Orientation != EntityOrientation.Left)
		{
			Lesmes.SetOrientation(EntityOrientation.Left);
		}
	}

	public void SetLesmesState(Lesmes_STATE st)
	{
		currentLesmesState = st;
		currentlyAvailableAttacks = GetCurrentStateAttacks();
	}

	public Lesmes_STATE GetLesmesState()
	{
		return currentLesmesState;
	}

	public override void Damage()
	{
	}

	public bool CanAttack()
	{
		return true;
	}

	public void Death()
	{
		StopAllCoroutines();
		UnsubscribeFromEverything();
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		Lesmes.AnimatorInyector.Death();
	}

	public void ResetCoolDown()
	{
		if (_currentAttackLapse > 0f)
		{
			_currentAttackLapse = 0f;
		}
	}

	public override void Attack()
	{
		if (!base.IsAttacking)
		{
			StopMovement();
			Transform target = GetTarget();
			LookAtTarget(target.position);
			if (_currentAttackLapse >= AttackCoolDown)
			{
				_currentAttackLapse = 0f;
			}
		}
	}

	public override void StopMovement()
	{
		Lesmes.Input.HorizontalInput = 0f;
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
}
