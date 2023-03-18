using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.AI;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Animator;
using Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras;

public class EsdrasBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct EsdrasPhases
	{
		public ESDRAS_PHASES phaseId;

		public List<Esdras_ATTACKS> availableAttacks;
	}

	public enum ESDRAS_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	[Serializable]
	public struct EsdrasAttackConfig
	{
		public Esdras_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	public enum Esdras_ATTACKS
	{
		SINGLE_SPIN,
		HEAVY_ATTACK,
		SPIN_LOOP,
		LIGHT_ATTACK,
		SUMMON_PERPETUA,
		COUNTER_IMPACT,
		SPIN_PROJECTILE
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public Esdras_ATTACKS lastAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack lightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack instantLightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack arcProjectiles;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private PerpetuaFightSpawner perpetuaSummoner;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public EsdrasMeleeAttack lightAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public EsdrasMeleeAttack heavyAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public EsdrasMeleeAttack singleSpinAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public BossDashAttack spinLoopAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public GameObject shockwave;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform leftLimitTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform rightLimitTransform;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private List<EsdrasPhases> phases;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float maxChaseTime = 5f;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private int maxHitsInRecovery = 3;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private int maxSpinsToProjectile = 2;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float spinDistance = 10f;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private LayerMask fightBoundariesLayerMask;

	private bool _keepRunningAnimation;

	private bool _attackIfEnemyClose;

	private Transform currentTarget;

	private StateMachine<EsdrasBehaviour> _fsm;

	private State<EsdrasBehaviour> stAction;

	private State<EsdrasBehaviour> stRun;

	private State<EsdrasBehaviour> stIntro;

	private State<EsdrasBehaviour> stPosture;

	private State<EsdrasBehaviour> stDeath;

	private Coroutine currentCoroutine;

	private ESDRAS_PHASES _currentPhase;

	private List<Esdras_ATTACKS> currentlyAvailableAttacks;

	private List<Esdras_ATTACKS> queuedActions;

	private RaycastHit2D[] results;

	private Vector2 _runPoint;

	private float _chaseCounter;

	private bool _tiredOfChasing;

	private bool _recovering;

	private int _currentRecoveryHits;

	private int _spinsToProjectile;

	private bool isBeingParried;

	private bool firstAttackDone;

	public Esdras Esdras { get; set; }

	public event Action<EsdrasBehaviour> OnActionFinished;

	public bool KeepRunningAnimation()
	{
		return _keepRunningAnimation;
	}

	public bool AttackIfEnemyClose()
	{
		return _attackIfEnemyClose;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		stIntro = new Esdras_StIntro();
		stAction = new Esdras_StAction();
		stRun = new Esdras_StRun();
		stDeath = new Esdras_StDeath();
		_fsm = new StateMachine<EsdrasBehaviour>(this, stIntro);
		results = new RaycastHit2D[1];
		currentlyAvailableAttacks = new List<Esdras_ATTACKS>
		{
			Esdras_ATTACKS.HEAVY_ATTACK,
			Esdras_ATTACKS.LIGHT_ATTACK,
			Esdras_ATTACKS.SINGLE_SPIN
		};
	}

	public override void OnStart()
	{
		base.OnStart();
		Esdras = (Esdras)Entity;
		ActivateCollisions(activate: false);
		ChangeBossState(BOSS_STATES.WAITING);
		PoolManager.Instance.CreatePool(shockwave, 1);
		firstAttackDone = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
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

	public void LaunchAction(Esdras_ATTACKS atk)
	{
		switch (atk)
		{
		case Esdras_ATTACKS.SINGLE_SPIN:
			IssueHorizontalAttack();
			break;
		case Esdras_ATTACKS.HEAVY_ATTACK:
			IssueHeavyAttack();
			break;
		case Esdras_ATTACKS.SPIN_LOOP:
			IssueSpinLoopAttack();
			break;
		case Esdras_ATTACKS.LIGHT_ATTACK:
			IssueLightAttack();
			break;
		case Esdras_ATTACKS.SUMMON_PERPETUA:
			IssueSummonPerpetua();
			break;
		case Esdras_ATTACKS.COUNTER_IMPACT:
			IssueCounterImpact();
			break;
		case Esdras_ATTACKS.SPIN_PROJECTILE:
			IssueProjectiles();
			break;
		}
		lastAttack = atk;
	}

	public Esdras_ATTACKS GetNewAttack()
	{
		if (queuedActions != null && queuedActions.Count > 0)
		{
			return QueuedActionsPop();
		}
		Esdras_ATTACKS[] array = new Esdras_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<Esdras_ATTACKS> list = new List<Esdras_ATTACKS>(array);
		list.Remove(lastAttack);
		Esdras_ATTACKS result = (firstAttackDone ? list[UnityEngine.Random.Range(0, list.Count)] : Esdras_ATTACKS.SPIN_LOOP);
		firstAttackDone = true;
		return result;
	}

	public IEnumerator WaitForState(State<EsdrasBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	public void LaunchRandomAction()
	{
		LaunchAction(GetNewAttack());
	}

	private void QueuedActionsPush(Esdras_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<Esdras_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private Esdras_ATTACKS QueuedActionsPop()
	{
		Esdras_ATTACKS esdras_ATTACKS = queuedActions[0];
		queuedActions.Remove(esdras_ATTACKS);
		return esdras_ATTACKS;
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public float GetHealthPercentage()
	{
		return Esdras.CurrentLife / Esdras.Stats.Life.Base;
	}

	private void SetPhase(EsdrasPhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
		if (_currentPhase == ESDRAS_PHASES.LAST)
		{
			QueuedActionsPush(Esdras_ATTACKS.SUMMON_PERPETUA);
		}
	}

	private void ChangePhase(ESDRAS_PHASES p)
	{
		EsdrasPhases phase = phases.Find((EsdrasPhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
		float healthPercentage = GetHealthPercentage();
		switch (_currentPhase)
		{
		case ESDRAS_PHASES.FIRST:
			if (healthPercentage < 0.6f)
			{
				ChangePhase(ESDRAS_PHASES.SECOND);
			}
			break;
		case ESDRAS_PHASES.SECOND:
			if (healthPercentage < 0.3f)
			{
				ChangePhase(ESDRAS_PHASES.LAST);
			}
			break;
		}
	}

	private IEnumerator GetIntoStateAndCallback(State<EsdrasBehaviour> newSt, float waitSeconds, Action callback)
	{
		_fsm.ChangeState(newSt);
		yield return new WaitForSeconds(2f);
		callback();
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

	public void StartIntroSequence()
	{
		_fsm.ChangeState(stIntro);
		ActivateCollisions(activate: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(IntroSequenceCoroutine()));
	}

	private IEnumerator IntroSequenceCoroutine()
	{
		ChangePhase(ESDRAS_PHASES.FIRST);
		Esdras.AnimatorInyector.Taunt();
		yield return new WaitForSeconds(1.5f);
		LookAtPenitent();
		yield return new WaitForSeconds(1.5f);
		base.BehaviourTree.StartBehaviour();
		ActivateCollisions(activate: true);
		StartWaitingPeriod(0.1f);
	}

	private void ActivateCollisions(bool activate)
	{
		Esdras.DamageArea.DamageAreaCollider.enabled = activate;
	}

	private void Shake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private void Wave()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.7f, 0.3f, 2f);
	}

	private void GetInRange(float range)
	{
		_attackIfEnemyClose = true;
		_runPoint = GetTarget().position;
		_runPoint += ((Vector2)base.transform.position - _runPoint).normalized * range;
		if (Mathf.Abs(_runPoint.x - base.transform.position.x) < range)
		{
			LookAtPenitent();
			_attackIfEnemyClose = false;
			_fsm.ChangeState(stAction);
		}
		else
		{
			_fsm.ChangeState(stRun);
		}
	}

	private void IssueHorizontalAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(HorizontalAttackCoroutine()));
	}

	private IEnumerator HorizontalAttackCoroutine()
	{
		SetGhostTrail(active: true);
		float runDistance = 1.5f;
		Vector3 dir = GetTarget().position - base.transform.position;
		_runPoint = base.transform.position + dir.normalized * runDistance;
		_keepRunningAnimation = true;
		_fsm.ChangeState(stRun);
		yield return StartCoroutine(WaitForState(stAction));
		Esdras.AnimatorInyector.SpinAttack();
		Esdras.Audio.QueueSlideAttack_AUDIO();
		yield return new WaitForSeconds(0.3f);
		if (_currentPhase == ESDRAS_PHASES.SECOND)
		{
			LaunchArcProjectile(new Vector2(dir.x, 0f).normalized);
		}
		_keepRunningAnimation = false;
		Esdras.AnimatorInyector.Run(run: false);
		yield return new WaitForSeconds(1.7f);
		Esdras.Audio.StopSlideAttack_AUDIO();
		SetGhostTrail(active: false);
		OnHorizontalAttackEnds();
	}

	private void OnHorizontalAttackEnds()
	{
		StartWaitingPeriod(0.6f);
	}

	private void IssueSpinLoopAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(SpinLoopAttackCoroutine()));
	}

	private Transform GetSpinLoopPoint()
	{
		if (Mathf.Abs(leftLimitTransform.position.x - base.transform.position.x) < Mathf.Abs(rightLimitTransform.position.x - base.transform.position.x))
		{
			return leftLimitTransform;
		}
		return rightLimitTransform;
	}

	private IEnumerator SpinLoopAttackCoroutine()
	{
		Transform p = GetSpinLoopPoint();
		Vector2 dir = ((!(p == leftLimitTransform)) ? Vector2.left : Vector2.right);
		_runPoint = p.position;
		_keepRunningAnimation = true;
		_fsm.ChangeState(stRun);
		Esdras.Audio.QueueSpinLoop();
		Esdras.AnimatorInyector.SpinLoop(active: true);
		yield return StartCoroutine(WaitForState(stAction));
		_keepRunningAnimation = false;
		Esdras.AnimatorInyector.SpinLoop(active: true);
		spinLoopAttack.OnDashBlockedEvent += OnSpinDashBlocked;
		spinLoopAttack.OnDashAdvancedEvent += OnSpinDashAdvanced;
		spinLoopAttack.OnDashFinishedEvent += OnSpinDashFinished;
		spinLoopAttack.Dash(base.transform, dir, spinDistance);
	}

	private void OnSpinDashAdvanced(float value)
	{
		if (value > 0.8f)
		{
			Esdras.AnimatorInyector.Run(run: false);
			Esdras.AnimatorInyector.SpinLoop(active: false);
		}
	}

	private void OnSpinDashBlocked(BossDashAttack obj)
	{
		Esdras.AnimatorInyector.SpinLoop(active: false);
		SpinLoopEnds();
	}

	private void OnSpinDashFinished()
	{
		Esdras.AnimatorInyector.SpinLoop(active: false);
		SpinLoopEnds();
	}

	private void SpinLoopEnds()
	{
		Esdras.Audio.StopSpinLoop_AUDIO();
		spinLoopAttack.OnDashBlockedEvent -= OnSpinDashBlocked;
		spinLoopAttack.OnDashAdvancedEvent -= OnSpinDashAdvanced;
		spinLoopAttack.OnDashFinishedEvent -= OnSpinDashFinished;
		LookAtPenitent();
		StartWaitingPeriod(1.2f);
	}

	private void IssueProjectiles()
	{
		StartAttackAction();
		SubscribeToSpin(subscribe: false);
		SetCurrentCoroutine(StartCoroutine(ProjectilesCoroutine()));
	}

	private IEnumerator ProjectilesCoroutine()
	{
		_spinsToProjectile = 0;
		float spinProjectileDuration = 2f;
		SetGhostTrail(active: true);
		float runDistance = 1.5f;
		Vector3 dir = GetTarget().position - base.transform.position;
		_runPoint = base.transform.position + dir.normalized * runDistance;
		_keepRunningAnimation = true;
		_fsm.ChangeState(stRun);
		yield return StartCoroutine(WaitForState(stAction));
		Esdras.AnimatorInyector.SpinLoop(active: true);
		Esdras.Audio.QueueSpinProjectile();
		SubscribeToSpin(subscribe: true);
		yield return new WaitForSeconds(0.2f);
		SetGhostTrail(active: false);
		singleSpinAttack.DealsDamage = true;
		instantLightningAttack.SummonAreaOnPoint(base.transform.position);
		yield return new WaitForSeconds(spinProjectileDuration);
		singleSpinAttack.DealsDamage = false;
		instantLightningAttack.SummonAreaOnPoint(base.transform.position);
		SubscribeToSpin(subscribe: false);
		Esdras.AnimatorInyector.SpinLoop(active: false);
		Esdras.Audio.StopSpinProjectile_AUDIO();
		_keepRunningAnimation = false;
		Esdras.AnimatorInyector.Run(run: false);
		yield return new WaitForSeconds(0.5f);
		ProjectilesEnd();
	}

	private void SubscribeToSpin(bool subscribe)
	{
		if (subscribe)
		{
			Esdras.AnimatorInyector.OnSpinProjectilePoint += OnSpinProjectilePoint;
		}
		else
		{
			Esdras.AnimatorInyector.OnSpinProjectilePoint -= OnSpinProjectilePoint;
		}
	}

	private void OnSpinProjectilePoint(EsdrasAnimatorInyector arg1, Vector2 dir)
	{
		LaunchArcProjectile(dir);
	}

	private void LaunchArcProjectile(Vector2 dir)
	{
		if (_spinsToProjectile >= maxSpinsToProjectile)
		{
			arcProjectiles.Shoot(dir, dir);
			_spinsToProjectile = 0;
		}
		else
		{
			_spinsToProjectile++;
		}
	}

	private void ProjectilesEnd()
	{
		LookAtPenitent();
		StartWaitingPeriod(1.2f);
	}

	private void IssueHeavyAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(HeavyAttackCoroutine()));
	}

	private IEnumerator HeavyAttackCoroutine()
	{
		GetInRange(2f);
		yield return StartCoroutine(WaitForState(stAction));
		_attackIfEnemyClose = false;
		Esdras.AnimatorInyector.OnHeavyAttackLightningSummon += OnHeavyAttackLightningSummon;
		Esdras.AnimatorInyector.HeavyAttack();
		yield return new WaitForSeconds(1.67f);
		OnHeavyAttackEnds();
	}

	private void OnHeavyAttackLightningSummon(EsdrasAnimatorInyector obj)
	{
		obj.OnHeavyAttackLightningSummon -= OnHeavyAttackLightningSummon;
		Vector2 right = Vector2.right;
		right *= (float)((Esdras.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		lightningAttack.SummonAreas(right);
	}

	private void OnHeavyAttackEnds()
	{
		StartWaitingPeriod(0.5f);
	}

	private void IssueLightAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(LightAttackCoroutine()));
	}

	private IEnumerator LightAttackCoroutine()
	{
		GetInRange(2.5f);
		yield return StartCoroutine(WaitForState(stAction));
		_attackIfEnemyClose = false;
		Esdras.AnimatorInyector.LightAttack();
		yield return new WaitForSeconds(1.1f);
		OnLightAttackEnds();
	}

	private void OnLightAttackEnds()
	{
		StartWaitingPeriod(0.5f);
	}

	private void IssueCounterImpact()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CounterImpactCoroutine()));
	}

	private IEnumerator CounterImpactCoroutine()
	{
		Esdras.AnimatorInyector.Taunt();
		yield return new WaitForSeconds(1.5f);
		OnCounterImpactEnds();
	}

	private void OnCounterImpactEnds()
	{
		StartWaitingPeriod(0.7f);
	}

	private void IssueSummonPerpetua()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(SummonPerpetuaCoroutine()));
	}

	private IEnumerator SummonPerpetuaCoroutine()
	{
		Esdras.AnimatorInyector.Taunt();
		yield return new WaitForSeconds(0.5f);
		lightningAttack.SummonAreas(Vector2.right);
		lightningAttack.SummonAreas(Vector2.left);
		yield return new WaitForSeconds(0.5f);
		Esdras.Audio.PlayCallSister_AUDIO();
		perpetuaSummoner.SpawnFightInPosition(base.transform.position);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
		yield return new WaitForSeconds(1f);
		Esdras.Audio.ChangeEsdrasMusic();
		OnPerpetuaSummoned();
	}

	private void OnPerpetuaSummoned()
	{
		StartWaitingPeriod(2f);
	}

	public void CounterImpactShockwave()
	{
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		instantLightningAttack.SummonAreaOnPoint(base.transform.position - num * Vector3.right);
		PoolManager.Instance.ReuseObject(shockwave, base.transform.position, Quaternion.identity);
	}

	public void OnHitReactionAnimationCompleted()
	{
		SetRecovering(recovering: false);
		_currentRecoveryHits = 0;
	}

	public void AttackDisplacement(float duration = 0.4f, float displacement = 2f, bool trail = true)
	{
		SetGhostTrail(trail);
		Esdras.DamageByContact = false;
		Ease ease = Ease.OutQuad;
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		num *= displacement;
		Vector2 dir = Vector2.right * num;
		dir = ClampToFightBoundaries(dir);
		base.transform.DOMove(base.transform.position + (Vector3)dir, duration).SetEase(ease).OnComplete(delegate
		{
			AfterDisplacement();
		});
	}

	private void AfterDisplacement()
	{
		Esdras.DamageByContact = true;
		SetGhostTrail(active: false);
	}

	public void BackDisplacement(float duration = 0.4f, float displacement = 2f)
	{
		SetGhostTrail(active: true);
		Esdras.DamageByContact = false;
		Ease ease = Ease.OutQuad;
		float num = ((Entity.Status.Orientation != 0) ? 1f : (-1f));
		num *= displacement;
		Vector2 dir = Vector2.right * num;
		dir = ClampToFightBoundaries(dir);
		base.transform.DOMove(base.transform.position + (Vector3)dir, duration).SetEase(ease).OnComplete(delegate
		{
			AfterDisplacement();
		});
	}

	public bool IsRecovering()
	{
		return _recovering;
	}

	public void SetRecovering(bool recovering)
	{
		_recovering = recovering;
	}

	public void RunToPoint(Vector2 position)
	{
		LookAtTarget(position);
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		Esdras.Input.HorizontalInput = horizontalInput;
	}

	public void StopRunning()
	{
		Esdras.Input.HorizontalInput = 0f;
		Esdras.Controller.PlatformCharacterPhysics.HSpeed = 0f;
	}

	public bool CloseToPointX(Vector2 p, float closeDistance = 0.1f)
	{
		return Mathf.Abs(p.x - base.transform.position.x) < closeDistance;
	}

	public bool CloseToTarget(float closeDistance = 1f)
	{
		Transform target = GetTarget();
		return Mathf.Abs(target.position.x - base.transform.position.x) < closeDistance;
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	public void SetRunAnimation(bool run)
	{
		Esdras.AnimatorInyector.Run(run);
	}

	public Vector2 GetRunPoint()
	{
		return _runPoint;
	}

	public void Death()
	{
		SetGhostTrail(active: false);
		perpetuaSummoner.DismissPerpetua();
		StopAllCoroutines();
		base.transform.DOKill(complete: true);
		StopBehaviour();
		Esdras.AnimatorInyector.Death();
		_fsm.ChangeState(stDeath);
	}

	public override void Idle()
	{
		StopMovement();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Parry()
	{
		base.Parry();
		StopAllCoroutines();
		SetGhostTrail(active: false);
		isBeingParried = true;
		base.transform.DOKill();
		Esdras.AnimatorInyector.Parry();
		BackDisplacement(0.5f, 1f);
		SetRecovering(recovering: true);
		StartWaitingPeriod(1f);
	}

	public void LookAtPenitent()
	{
		if ((bool)Core.Logic.Penitent)
		{
			LookAtTarget(Core.Logic.Penitent.transform.position);
		}
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public bool CanChase()
	{
		return true;
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		CheckNextPhase();
		if (_recovering && _currentRecoveryHits < maxHitsInRecovery)
		{
			StopAllCoroutines();
			Esdras.Audio.StopAll();
			Esdras.AnimatorInyector.Hurt();
			base.transform.DOKill(complete: true);
			LookAtPenitent();
			BackDisplacement(0.3f, 0.4f);
			_currentRecoveryHits++;
			if (_currentRecoveryHits >= maxHitsInRecovery)
			{
				Esdras.CounterFlash();
				IssueCounterImpact();
			}
			else
			{
				StartWaitingPeriod(1f);
			}
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		Esdras.SetOrientation((!(targetPos.x > Esdras.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public override void StopMovement()
	{
	}

	public void SetGhostTrail(bool active)
	{
		Esdras.GhostTrail.EnableGhostTrail = active;
	}

	private Vector2 ClampToFightBoundaries(Vector2 dir)
	{
		Vector2 vector = dir;
		Debug.DrawLine(base.transform.position, base.transform.position + (Vector3)vector, Color.green, 5f);
		if (Physics2D.RaycastNonAlloc(base.transform.position, dir, results, dir.magnitude, fightBoundariesLayerMask) > 0)
		{
			Debug.DrawLine(base.transform.position, results[0].point, Color.red, 5f);
			vector = vector.normalized * results[0].distance;
		}
		return vector;
	}

	public void OnDrawGizmos()
	{
	}
}
