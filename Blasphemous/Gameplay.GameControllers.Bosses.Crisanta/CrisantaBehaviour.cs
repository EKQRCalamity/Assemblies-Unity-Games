using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Crisanta.AI;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI.Widgets;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using Tools.Level.Interactables;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Crisanta;

public class CrisantaBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct CrisantaPhases
	{
		public Crisanta_PHASES phaseId;

		public List<Crisanta_ATTACKS> availableAttacks;
	}

	public enum Crisanta_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	[Serializable]
	public struct CrisantaAttackConfig
	{
		public Crisanta_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	public enum Crisanta_ATTACKS
	{
		UPWARDS_SLASH,
		DOWNWARDS_SLASH,
		BACKFLIP_LOW,
		LEFT_HORIZONTAL_BLINK,
		LEFT_DIAGONAL_BLINK,
		COUNTER_BLINK,
		GUARD,
		COMBO_SLASHES_A,
		COMBO_REST,
		BACKFLIP_HIGH,
		RIGHT_HORIZONTAL_BLINK,
		RIGHT_DIAGONAL_BLINK,
		COMBO_SLASHES_B,
		COMBO_BLINK_A,
		COMBO_BLINK_B,
		UNSEAL,
		DEATH,
		FORWARD_FLIP,
		INSTANT_GUARD,
		INSTANT_DOWNWARDS,
		CORNER_SHOCKWAVES,
		COMBO_BACKFLIP_SLASH_A
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public Crisanta_ATTACKS lastAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack lightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack bladeAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack instantLightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack dramaLightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack arcProjectiles;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossDashAttack slashDashAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossDashAttack diagonalSlashDashAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public CrisantaMeleeAttack lightAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public CrisantaMeleeAttack heavyAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform leftLimitTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform rightLimitTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform fightCenterTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public RuntimeAnimatorController secondPhaseController;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public GameObject tpoWaypointLeft;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public GameObject tpoWaypointRight;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public GameObject crisantaNPC;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private List<CrisantaPhases> phases;

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

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float introWaitTime = 1.5f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float comboRestTime = 1.5f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float afterBlinkComboWaitTime = 1.4f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float guardWaitTime = 1.5f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float afterParryWaitTime = 1.2f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float shortDisplDurationFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float shortDisplLengthFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float mediumDisplOffsetFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float mediumDisplSpeedFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float backDisplDurationFactor = 0.5f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float backDisplLengthFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float backflipAngleFactor = 1f;

	[SerializeField]
	[FoldoutGroup("Timing settings", 0)]
	private float backflipSpeedFactor = 1f;

	public GameObject particleParent;

	public GameObject particleParentMid;

	private ParticleSystem sparkParticles;

	private ParticleSystem sparkParticlesMid;

	private bool justParried;

	private bool _keepRunningAnimation;

	private bool _attackIfEnemyClose;

	public GameObject executionPrefab;

	public GameObject trueEndingExecutionPrefab;

	private Transform currentTarget;

	private StateMachine<CrisantaBehaviour> _fsm;

	private State<CrisantaBehaviour> stAction;

	private State<CrisantaBehaviour> stIntro;

	private State<CrisantaBehaviour> stGuard;

	private State<CrisantaBehaviour> stDeath;

	private Coroutine currentCoroutine;

	private Crisanta_PHASES _currentPhase;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<Crisanta_ATTACKS> comboSlashesA;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<Crisanta_ATTACKS> comboSlashesB;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<Crisanta_ATTACKS> comboBlinkA;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<Crisanta_ATTACKS> comboBlinkB;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<Crisanta_ATTACKS> comboBackflipSlashesA;

	private List<Crisanta_ATTACKS> currentlyAvailableAttacks;

	private List<Crisanta_ATTACKS> queuedActions;

	private RaycastHit2D[] results;

	private Vector2 _runPoint;

	private float _chaseCounter;

	private bool _tiredOfChasing;

	private bool _recovering;

	private int _currentRecoveryHits;

	private int _spinsToProjectile;

	private int comboActionsRemaining;

	private bool _waitingForAnimationFinish;

	private bool unveiled;

	[HideInInspector]
	public bool ignoreAnimDispl;

	public Crisanta Crisanta { get; set; }

	public event Action<CrisantaBehaviour> OnActionFinished;

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
		stIntro = new Crisanta_StIntro();
		stAction = new Crisanta_StAction();
		stDeath = new Crisanta_StDeath();
		stGuard = new Crisanta_StGuard();
		_fsm = new StateMachine<CrisantaBehaviour>(this, stIntro);
		results = new RaycastHit2D[1];
		currentlyAvailableAttacks = new List<Crisanta_ATTACKS>
		{
			Crisanta_ATTACKS.COMBO_SLASHES_A,
			Crisanta_ATTACKS.COMBO_BLINK_A
		};
		sparkParticles = particleParent.GetComponentInChildren<ParticleSystem>();
		sparkParticlesMid = particleParentMid.GetComponentInChildren<ParticleSystem>();
	}

	public override void OnStart()
	{
		base.OnStart();
		Crisanta = (Crisanta)Entity;
		heavyAttack.OnMeleeAttackGuarded += OnHeavyAttackGuarded;
		diagonalSlashDashAttack.OnDashBlockedEvent += OnDashBlocked;
		ChangeBossState(BOSS_STATES.WAITING);
		LookAtTarget(base.transform.position + Vector3.left);
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
		comboActionsRemaining--;
		if (comboActionsRemaining == 0)
		{
			QueuedActionsPush(Crisanta_ATTACKS.COMBO_REST);
		}
	}

	private void CancelAttacks()
	{
		lightAttack.damageOnEnterArea = false;
		heavyAttack.damageOnEnterArea = false;
	}

	private void CancelCombo()
	{
		Crisanta.AnimatorInyector.CancelAll();
		if (queuedActions != null)
		{
			queuedActions.Clear();
		}
		comboActionsRemaining = -1;
	}

	private void ActionFinished()
	{
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
		if (this.OnActionFinished != null)
		{
			this.OnActionFinished(this);
		}
	}

	public void LaunchAction(Crisanta_ATTACKS atk)
	{
		Debug.Log("TIME: " + Time.time + " Launching action: " + atk.ToString());
		SetRecovering(recovering: false);
		switch (atk)
		{
		case Crisanta_ATTACKS.UPWARDS_SLASH:
			IssueUpwardsSlash();
			break;
		case Crisanta_ATTACKS.DOWNWARDS_SLASH:
			IssueDownwardsSlash();
			break;
		case Crisanta_ATTACKS.INSTANT_DOWNWARDS:
			IssueInstantDownwardsSlash();
			break;
		case Crisanta_ATTACKS.BACKFLIP_LOW:
			IssueBackflip();
			break;
		case Crisanta_ATTACKS.BACKFLIP_HIGH:
			IssueHighBackflip();
			break;
		case Crisanta_ATTACKS.LEFT_HORIZONTAL_BLINK:
			IssueLeftBlinkSlash();
			break;
		case Crisanta_ATTACKS.COUNTER_BLINK:
			IssueDownwardsSlash();
			break;
		case Crisanta_ATTACKS.GUARD:
			IssueGuard();
			break;
		case Crisanta_ATTACKS.INSTANT_GUARD:
			IssueInstantGuard();
			break;
		case Crisanta_ATTACKS.UNSEAL:
			IssueUnseal();
			break;
		case Crisanta_ATTACKS.COMBO_SLASHES_A:
			IssueCombo(comboSlashesA);
			break;
		case Crisanta_ATTACKS.COMBO_REST:
			Crisanta.AnimatorInyector.ComboMode(active: false);
			StartWaitingPeriod(comboRestTime);
			break;
		case Crisanta_ATTACKS.RIGHT_HORIZONTAL_BLINK:
			IssueRightBlinkSlash();
			break;
		case Crisanta_ATTACKS.LEFT_DIAGONAL_BLINK:
			IssueLeftBlinkDiagonalSlash();
			break;
		case Crisanta_ATTACKS.RIGHT_DIAGONAL_BLINK:
			IssueRightBlinkDiagonalSlash();
			break;
		case Crisanta_ATTACKS.COMBO_SLASHES_B:
			IssueCombo(comboSlashesB);
			break;
		case Crisanta_ATTACKS.COMBO_BLINK_A:
			IssueCombo(comboBlinkA);
			break;
		case Crisanta_ATTACKS.COMBO_BLINK_B:
			IssueCombo(comboBlinkB);
			break;
		case Crisanta_ATTACKS.DEATH:
			DeathAction();
			break;
		case Crisanta_ATTACKS.FORWARD_FLIP:
			IssueEscapeAttack();
			break;
		case Crisanta_ATTACKS.CORNER_SHOCKWAVES:
			IssueCornerShockwaves();
			break;
		case Crisanta_ATTACKS.COMBO_BACKFLIP_SLASH_A:
			IssueCombo(comboBackflipSlashesA);
			break;
		}
		lastAttack = atk;
	}

	public Crisanta_ATTACKS GetNewAttack()
	{
		if (queuedActions != null && queuedActions.Count > 0)
		{
			return QueuedActionsPop();
		}
		if (_currentPhase == Crisanta_PHASES.SECOND && !unveiled && !Crisanta.IsCrisantaRedux)
		{
			return Crisanta_ATTACKS.UNSEAL;
		}
		Crisanta_ATTACKS[] array = new Crisanta_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<Crisanta_ATTACKS> list = new List<Crisanta_ATTACKS>(array);
		list.Remove(lastAttack);
		if (justParried)
		{
			list.Remove(Crisanta_ATTACKS.COMBO_SLASHES_A);
			list.Remove(Crisanta_ATTACKS.COMBO_SLASHES_B);
			justParried = false;
		}
		if (GetDirToPenitent().magnitude > 5f)
		{
			list.Remove(Crisanta_ATTACKS.BACKFLIP_LOW);
		}
		if (comboBlinkA.Contains(lastAttack) || comboBlinkB.Contains(lastAttack))
		{
			list.Remove(Crisanta_ATTACKS.COMBO_BLINK_A);
			list.Remove(Crisanta_ATTACKS.COMBO_BLINK_B);
		}
		if (lastAttack == Crisanta_ATTACKS.DOWNWARDS_SLASH || lastAttack == Crisanta_ATTACKS.INSTANT_DOWNWARDS)
		{
			list.Remove(Crisanta_ATTACKS.INSTANT_DOWNWARDS);
			list.Remove(Crisanta_ATTACKS.INSTANT_GUARD);
			list.Remove(Crisanta_ATTACKS.COMBO_SLASHES_A);
			list.Remove(Crisanta_ATTACKS.COMBO_BACKFLIP_SLASH_A);
		}
		if (lastAttack == Crisanta_ATTACKS.BACKFLIP_HIGH || lastAttack == Crisanta_ATTACKS.BACKFLIP_LOW || lastAttack == Crisanta_ATTACKS.FORWARD_FLIP)
		{
			list.Remove(Crisanta_ATTACKS.BACKFLIP_HIGH);
			list.Remove(Crisanta_ATTACKS.BACKFLIP_LOW);
			list.Remove(Crisanta_ATTACKS.FORWARD_FLIP);
		}
		if (Crisanta.IsCrisantaRedux && (lastAttack == Crisanta_ATTACKS.LEFT_HORIZONTAL_BLINK || lastAttack == Crisanta_ATTACKS.RIGHT_HORIZONTAL_BLINK || lastAttack == Crisanta_ATTACKS.CORNER_SHOCKWAVES))
		{
			list.Remove(Crisanta_ATTACKS.LEFT_HORIZONTAL_BLINK);
			list.Remove(Crisanta_ATTACKS.RIGHT_HORIZONTAL_BLINK);
			list.Remove(Crisanta_ATTACKS.CORNER_SHOCKWAVES);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public IEnumerator WaitForState(State<CrisantaBehaviour> st)
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

	private void QueuedActionsPush(Crisanta_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<Crisanta_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private Crisanta_ATTACKS QueuedActionsPop()
	{
		Crisanta_ATTACKS crisanta_ATTACKS = queuedActions[0];
		queuedActions.Remove(crisanta_ATTACKS);
		return crisanta_ATTACKS;
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public float GetHealthPercentage()
	{
		return Crisanta.CurrentLife / Crisanta.Stats.Life.Base;
	}

	private void SetPhase(CrisantaPhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
	}

	private void ChangePhase(Crisanta_PHASES p)
	{
		CrisantaPhases phase = phases.Find((CrisantaPhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
		float healthPercentage = GetHealthPercentage();
		switch (_currentPhase)
		{
		case Crisanta_PHASES.FIRST:
			if (healthPercentage < 0.6f)
			{
				ChangePhase(Crisanta_PHASES.SECOND);
			}
			break;
		case Crisanta_PHASES.SECOND:
			if (healthPercentage < 0.3f)
			{
				ChangePhase(Crisanta_PHASES.LAST);
			}
			break;
		}
	}

	public void IssueCombo(List<Crisanta_ATTACKS> testCombo)
	{
		for (int i = 0; i < testCombo.Count; i++)
		{
			QueuedActionsPush(testCombo[i]);
		}
		comboActionsRemaining = testCombo.Count;
		StartWaitingPeriod(0.1f);
		Crisanta.AnimatorInyector.ComboMode(active: true);
	}

	private IEnumerator GetIntoStateAndCallback(State<CrisantaBehaviour> newSt, float waitSeconds, Action callback)
	{
		_fsm.ChangeState(newSt);
		yield return new WaitForSeconds(2f);
		callback();
	}

	private void StartWaitingPeriod(float seconds)
	{
		Debug.Log(">> WAITING PERIOD: " + seconds);
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
		Debug.Log(">> READY FOR ACTION: " + Time.time);
		ActionFinished();
	}

	public void StartIntroSequence()
	{
		_fsm.ChangeState(stIntro);
		ActivateCollisions(activate: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(IntroSequenceCoroutine()));
	}

	public void StartReduxIntroSequence()
	{
		Crisanta.Controller.SmartPlatformCollider.enabled = false;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = false;
		base.transform.position = crisantaNPC.transform.position;
		Crisanta.Controller.SmartPlatformCollider.enabled = true;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
		Crisanta.Animator.runtimeAnimatorController = secondPhaseController;
		Crisanta.Animator.speed = 1.5f;
		Crisanta.Animator.Play("GUARD");
		Crisanta.Behaviour.SetGhostTrail(active: true);
		_fsm.ChangeState(stIntro);
		ActivateCollisions(activate: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(ReduxIntroSequenceCoroutine()));
	}

	private IEnumerator IntroSequenceCoroutine()
	{
		ChangePhase(Crisanta_PHASES.FIRST);
		LookAtPenitent();
		yield return new WaitForSeconds(introWaitTime);
		base.BehaviourTree.StartBehaviour();
		ActivateCollisions(activate: true);
		StartWaitingPeriod(0.1f);
	}

	private IEnumerator ReduxIntroSequenceCoroutine()
	{
		ChangePhase(Crisanta_PHASES.FIRST);
		LookAtPenitent();
		yield return new WaitForSeconds(introWaitTime);
		base.BehaviourTree.StartBehaviour();
		ActivateCollisions(activate: true);
		yield return new WaitForSeconds(1f);
		LaunchAction(Crisanta_ATTACKS.DOWNWARDS_SLASH);
	}

	private void ActivateCollisions(bool activate)
	{
		Crisanta.DamageArea.DamageAreaCollider.enabled = activate;
	}

	private void Shake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private void IssueInstantDownwardsSlash()
	{
		StartAttackAction();
		Crisanta.AnimatorInyector.ChangeStance();
		SetCurrentCoroutine(StartCoroutine(DownwardAttackCoroutine()));
	}

	private void IssueDownwardsSlash()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(DownwardAttackCoroutine()));
	}

	private IEnumerator DownwardAttackCoroutine()
	{
		LookAtPenitent();
		Crisanta.AnimatorInyector.DownwardsSlash();
		yield return StartCoroutine(BlockUntilAnimationEnds());
		OnDownwardAttackEnds();
	}

	private void OnHeavyAttackGuarded()
	{
		Parry();
	}

	private void OnDownwardAttackEnds()
	{
		StartWaitingPeriod(0.1f);
	}

	private void IssueUpwardsSlash()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(UpwardsSlashCoroutine()));
	}

	private IEnumerator UpwardsSlashCoroutine()
	{
		LookAtPenitent();
		Crisanta.AnimatorInyector.UpwardsSlash();
		yield return StartCoroutine(BlockUntilAnimationEnds());
		OnLightAttackEnds();
	}

	private void OnLightAttackEnds()
	{
		StartWaitingPeriod(0.1f);
	}

	private void IssueBackflip()
	{
		SetRecovering(recovering: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(BackflipCoroutine(isGreatJump: false)));
	}

	private void IssueHighBackflip()
	{
		SetRecovering(recovering: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(BackflipCoroutine(isGreatJump: true)));
	}

	private void IssueEscapeAttack()
	{
		SetRecovering(recovering: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(BackflipCoroutine(isGreatJump: true, forward: true)));
	}

	private Vector2 GetDirToPenitent()
	{
		if (Core.Logic.Penitent == null)
		{
			return Vector2.zero;
		}
		return Core.Logic.Penitent.transform.position + Vector3.up - base.transform.position;
	}

	private float DistanceToPenitent()
	{
		if (Core.Logic.Penitent == null)
		{
			return 0f;
		}
		return Vector2.Distance(Core.Logic.Penitent.transform.position + Vector3.up, base.transform.position);
	}

	private IEnumerator BackflipCoroutine(bool isGreatJump, bool forward = false)
	{
		LookAtPenitent();
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		float d = GetDirFromOrientation();
		float maxWalkSpeed = Crisanta.Controller.MaxWalkingSpeed;
		bool diagonalAttack = false;
		if (isGreatJump)
		{
			Crisanta.Controller.MaxWalkingSpeed *= 0.6f * backflipSpeedFactor;
			diagonalAttack = true;
		}
		else
		{
			Crisanta.Controller.MaxWalkingSpeed *= 1.4f * backflipSpeedFactor;
		}
		if (forward)
		{
			Crisanta.Input.HorizontalInput = d;
		}
		else
		{
			Crisanta.Input.HorizontalInput = 0f - d;
		}
		SetGhostTrail(active: true);
		Crisanta.AnimatorInyector.Backflip();
		yield return StartCoroutine(JumpPress(isGreatJump));
		if (diagonalAttack)
		{
			yield return StartCoroutine(WaitApexOfJump());
			Vector2 dir = new Vector2(GetDirFromOrientation(), -0.75f);
			Vector2 dirToPenitent = GetDirToPenitent();
			float ang = Vector2.Angle(dir, dirToPenitent);
			if (ang < 3f * backflipAngleFactor)
			{
				float num = ((!(Vector2.Dot(dir, dirToPenitent) > 0f)) ? 1 : (-1));
				num *= GetDirFromOrientation();
				dir = dirToPenitent;
				base.transform.rotation = Quaternion.Euler(0f, 0f, ang * num);
			}
			Crisanta.Input.HorizontalInput = 0f;
			Crisanta.Controller.MaxWalkingSpeed = maxWalkSpeed;
			SetCurrentCoroutine(StartCoroutine(DiagonalAttack(base.transform.position, dir, 35f)));
		}
		else
		{
			yield return new WaitForSeconds(0.2f);
			yield return StartCoroutine(WaitCloseToGround());
			Crisanta.AnimatorInyector.BackflipLand();
			SetGhostTrail(active: false);
			Crisanta.Input.HorizontalInput = 0f;
			Crisanta.Controller.MaxWalkingSpeed = maxWalkSpeed;
			OnBackflipEnds();
		}
	}

	private void IssueBounceAttack()
	{
		StopAllCoroutines();
		StartCoroutine(AttackAfterBounce());
	}

	private IEnumerator AttackAfterBounce()
	{
		yield return StartCoroutine(WaitApexOfJump());
		LookAtPenitent();
		Vector2 dir = new Vector2(GetDirFromOrientation(), -0.75f);
		Vector2 dirToPenitent = GetDirToPenitent();
		float ang = Vector2.Angle(dir, dirToPenitent);
		if (ang < 15f * backflipSpeedFactor)
		{
			float num = ((Vector2.Dot(dir, dirToPenitent) > 0f) ? 1 : (-1));
			num *= GetDirFromOrientation();
			dir = dirToPenitent;
			base.transform.rotation = Quaternion.Euler(0f, 0f, ang * num);
		}
		SetCurrentCoroutine(StartCoroutine(DiagonalAttack(base.transform.position, dir, 35f, alreadyBounced: true)));
	}

	private IEnumerator WaitApexOfJump()
	{
		while (Crisanta.Controller.PlatformCharacterPhysics.VSpeed >= 0f)
		{
			yield return null;
		}
	}

	private IEnumerator DiagonalAttack(Vector2 pos, Vector2 dir, float distance, bool alreadyBounced = false)
	{
		Crisanta.Controller.PlatformCharacterPhysics.Velocity = Vector2.zero;
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 0f;
		LookAtTarget(pos + dir);
		Crisanta.AnimatorInyector.AirAttack(active: true);
		Crisanta.Audio.PlayAirAttack_AUDIO();
		yield return new WaitForSeconds(0.4f);
		diagonalSlashDashAttack.Dash(base.transform, dir.normalized, distance, 0.1f);
		yield return new WaitForSeconds(0.2f);
		Crisanta.AnimatorInyector.AirAttack(active: false);
		base.transform.rotation = Quaternion.identity;
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.Audio.PlayLanding_AUDIO();
		if (!Crisanta.MotionChecker.HitsFloorInPosition(Crisanta.transform.position, Crisanta.MotionChecker.RangeGroundDetection, out var _))
		{
			yield return StartCoroutine(RecoveryBackflip(!alreadyBounced));
		}
		else
		{
			LandingShockwave();
			yield return StartCoroutine(RecoveryBackflip());
		}
		LookAtPenitent();
		SetGhostTrail(active: false);
		StartWaitingPeriod(0.1f);
	}

	private void LandingShockwave()
	{
		lightningAttack.SummonAreas(Vector2.right);
		lightningAttack.SummonAreas(Vector2.left);
	}

	private IEnumerator RecoveryBackflip(bool attack = false)
	{
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		float d = GetDirFromOrientation();
		float maxWalkSpeed = Crisanta.Controller.MaxWalkingSpeed;
		Crisanta.Controller.MaxWalkingSpeed *= 1f;
		Crisanta.Input.HorizontalInput = 0f - d;
		SetGhostTrail(active: true);
		SetRecovering(recovering: false);
		Crisanta.AnimatorInyector.Backflip();
		ForceJump(8f);
		yield return new WaitForSeconds(0.2f);
		if (attack)
		{
			SetGhostTrail(active: false);
			Crisanta.Input.HorizontalInput = 0f;
			Crisanta.Controller.MaxWalkingSpeed = maxWalkSpeed;
			IssueBounceAttack();
		}
		else
		{
			yield return StartCoroutine(WaitCloseToGround());
			Crisanta.AnimatorInyector.BackflipLand();
			SetGhostTrail(active: false);
			Crisanta.Input.HorizontalInput = 0f;
			Crisanta.Controller.MaxWalkingSpeed = maxWalkSpeed;
		}
	}

	private void OnDashBlocked(BossDashAttack obj)
	{
		Parry();
	}

	private IEnumerator WaitCloseToGround()
	{
		while (!Crisanta.Controller.IsGrounded)
		{
			yield return null;
		}
	}

	private void ForceJump(float vSpeed)
	{
		Crisanta.Controller.PlatformCharacterPhysics.VSpeed = vSpeed;
	}

	private IEnumerator JumpPress(bool isLongPress)
	{
		Crisanta.Input.Jump = true;
		yield return (!isLongPress) ? new WaitForSeconds(0.1f) : new WaitForSeconds(0.5f);
		Crisanta.Input.Jump = false;
	}

	private void OnBackflipEnds()
	{
		Crisanta.Audio.PlayLanding_AUDIO();
		StartWaitingPeriod(0.1f);
	}

	private void IssueGuard()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(GuardCoroutine()));
	}

	private IEnumerator GuardCoroutine()
	{
		Crisanta.AnimatorInyector.Guard(active: true);
		yield return new WaitForSeconds(guardWaitTime);
		Crisanta.AnimatorInyector.Guard(active: false);
		EndGuard();
	}

	private void EndGuard()
	{
		float seconds = ((!Crisanta.IsCrisantaRedux) ? 1f : 0.5f);
		StartWaitingPeriod(seconds);
	}

	private void IssueInstantGuard()
	{
		StartAttackAction();
		Crisanta.AnimatorInyector.ChangeStance();
		QueuedActionsPush(Crisanta_ATTACKS.DOWNWARDS_SLASH);
		SetCurrentCoroutine(StartCoroutine(GuardCoroutine()));
	}

	private void IssueUnseal()
	{
		StartAttackAction();
		Crisanta.AnimatorInyector.AnimationEvent_SetShieldedOn();
		SetCurrentCoroutine(StartCoroutine(UnsealCoroutine()));
	}

	private IEnumerator UnsealCoroutine()
	{
		Crisanta.Animator.runtimeAnimatorController = secondPhaseController;
		instantLightningAttack.SummonAreaOnPoint(base.transform.position);
		lightningAttack.totalAreas = 12;
		lightningAttack.seconds = 2f;
		Crisanta.AnimatorInyector.Unseal();
		yield return new WaitForSeconds(1f);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 1f, 0f, Crisanta.slowTimeCurve);
		LandingShockwave();
		yield return new WaitForSeconds(1f);
		EndUnseal();
	}

	private void EndUnseal()
	{
		unveiled = true;
		QueuedActionsPush(Crisanta_ATTACKS.COMBO_BLINK_A);
		Crisanta.AnimatorInyector.AnimationEvent_SetShieldedOff();
		StartWaitingPeriod(0.01f);
	}

	public void OnBlinkIn()
	{
		Crisanta.DamageArea.DamageAreaCollider.enabled = true;
	}

	public void OnBlinkOut()
	{
		Crisanta.DamageArea.DamageAreaCollider.enabled = false;
	}

	private bool IsPenitentOnRightSide()
	{
		return IsOnRightSide(Core.Logic.Penitent.transform);
	}

	private bool IsPenitentCloseToCenter()
	{
		return IsCloseToCenter(Core.Logic.Penitent.transform);
	}

	private bool IsCrisantaOnRightSide()
	{
		return IsOnRightSide(base.transform);
	}

	private bool IsCrisantaCloseToCenter()
	{
		return IsCloseToCenter(base.transform);
	}

	private bool IsOnRightSide(Transform t)
	{
		return t.position.x > fightCenterTransform.position.x;
	}

	private bool IsCloseToCenter(Transform t)
	{
		float num = 4f;
		return Mathf.Abs(t.position.x - fightCenterTransform.transform.position.x) < num;
	}

	private void IssueLeftBlinkSlash()
	{
		StartAttackAction();
		if (IsPenitentCloseToCenter() || IsPenitentOnRightSide())
		{
			LeftBlinkSlash();
		}
		else
		{
			RightBlinkSlash();
		}
	}

	private void IssueRightBlinkSlash()
	{
		StartAttackAction();
		if (IsPenitentCloseToCenter() || !IsPenitentOnRightSide())
		{
			RightBlinkSlash();
		}
		else
		{
			LeftBlinkSlash();
		}
	}

	private void LeftBlinkSlash()
	{
		SetCurrentCoroutine(StartCoroutine(BlinkSlashCoroutine(fightCenterTransform.position - Vector3.right * 8f - Vector3.up * 0.75f, Vector2.right, 18f)));
	}

	private void RightBlinkSlash()
	{
		SetCurrentCoroutine(StartCoroutine(BlinkSlashCoroutine(fightCenterTransform.position + Vector3.right * 8f - Vector3.up * 0.75f, -Vector2.right, 18f)));
	}

	private void IssueRightBlinkDiagonalSlash()
	{
		StartAttackAction();
		if (IsPenitentCloseToCenter() || !IsPenitentOnRightSide())
		{
			RightBlinkDiagonalSlash();
		}
		else
		{
			LeftBlinkDiagonalSlash();
		}
	}

	private void IssueLeftBlinkDiagonalSlash()
	{
		StartAttackAction();
		if (IsPenitentCloseToCenter() || IsPenitentOnRightSide())
		{
			LeftBlinkDiagonalSlash();
		}
		else
		{
			RightBlinkDiagonalSlash();
		}
	}

	private void RightBlinkDiagonalSlash()
	{
		SetCurrentCoroutine(StartCoroutine(BlinkSlashCoroutine(Core.Logic.Penitent.transform.position + Vector3.right * 6f + Vector3.up * 4f, new Vector2(-1f, -0.75f), 13f, diagonal: true)));
	}

	private void LeftBlinkDiagonalSlash()
	{
		SetCurrentCoroutine(StartCoroutine(BlinkSlashCoroutine(Core.Logic.Penitent.transform.position - Vector3.right * 6f + Vector3.up * 4f, new Vector2(1f, -0.75f), 13f, diagonal: true)));
	}

	private IEnumerator BlinkOut()
	{
		Crisanta.AnimatorInyector.BlinkOut();
		Crisanta.Audio.PlayTeleportOut_AUDIO();
		yield return new WaitForSeconds(0.8f);
	}

	private IEnumerator BlinkSlashCoroutine(Vector2 pos, Vector2 dir, float distance, bool diagonal = false)
	{
		Crisanta.DamageByContact = false;
		if (!Crisanta.Animator.GetCurrentAnimatorStateInfo(0).IsName("HIDDEN"))
		{
			yield return StartCoroutine(BlinkOut());
		}
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 0f;
		Crisanta.Controller.PlatformCharacterPhysics.Velocity = Vector2.zero;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = false;
		base.transform.position = pos;
		LookAtTarget(pos + dir);
		yield return new WaitForSeconds(0.1f);
		if (diagonal)
		{
			Debug.Log("DIAGONAL BLINK");
			Crisanta.Audio.PlayAirAttack_AUDIO();
			Crisanta.AnimatorInyector.AirAttack(active: true);
		}
		else
		{
			Debug.Log("HORIZONTAL_BLINK");
			Crisanta.Audio.PlayAirAttack_AUDIO();
			Crisanta.AnimatorInyector.Blinkslash(blinkIn: true);
		}
		yield return new WaitForSeconds(0.4f);
		if (diagonal)
		{
			diagonalSlashDashAttack.Dash(base.transform, dir, distance);
		}
		else
		{
			slashDashAttack.Dash(base.transform, dir, distance);
		}
		sparkParticlesMid.Play();
		yield return new WaitForSeconds(0.3f);
		sparkParticlesMid.Stop();
		if (diagonal)
		{
			Crisanta.AnimatorInyector.AirAttack(active: false);
		}
		else
		{
			Crisanta.AnimatorInyector.Blinkslash(blinkIn: false);
		}
		if (NoBlinkActionsRemaining())
		{
			yield return new WaitForSeconds(afterBlinkComboWaitTime);
			BlinkSlashEnds();
			yield return new WaitForSeconds(0.5f);
			Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
			Crisanta.DamageByContact = true;
			StartWaitingPeriod(0.1f);
		}
		else
		{
			yield return new WaitForSeconds(1.4f);
			StartWaitingPeriod(0.1f);
		}
	}

	private bool NoBlinkActionsRemaining()
	{
		return queuedActions == null || (!queuedActions.Contains(Crisanta_ATTACKS.LEFT_DIAGONAL_BLINK) && !queuedActions.Contains(Crisanta_ATTACKS.RIGHT_DIAGONAL_BLINK) && !queuedActions.Contains(Crisanta_ATTACKS.LEFT_HORIZONTAL_BLINK) && !queuedActions.Contains(Crisanta_ATTACKS.RIGHT_HORIZONTAL_BLINK));
	}

	private void BlinkSlashEnds()
	{
		Debug.Log("<color=red> BLINK SLASH ENDS</color>");
		Vector3 position = fightCenterTransform.transform.position;
		if (Core.Logic.Penitent.GetPosition().x > fightCenterTransform.transform.position.x)
		{
			position.x -= UnityEngine.Random.Range(3f, 5f);
		}
		else
		{
			position.x += UnityEngine.Random.Range(3f, 5f);
		}
		position.y += 0.1f;
		Crisanta.Controller.SmartPlatformCollider.enabled = false;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = false;
		base.transform.position = position;
		Crisanta.Controller.SmartPlatformCollider.enabled = true;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
		LookAtPenitent();
		Crisanta.Audio.PlayTeleportIn_AUDIO();
		Crisanta.AnimatorInyector.BlinkIn();
	}

	private void IssueCornerShockwaves()
	{
		StartAttackAction();
		if (IsPenitentCloseToCenter() || !IsPenitentOnRightSide())
		{
			RightCornerShockwaves();
		}
		else
		{
			LeftCornerShockwaves();
		}
	}

	private void RightCornerShockwaves()
	{
		Vector3 vector = fightCenterTransform.position + Vector3.right * 8f;
		SetCurrentCoroutine(StartCoroutine(CornerShockwavesCoroutine(vector, Vector2.left, 4)));
	}

	private void LeftCornerShockwaves()
	{
		Vector3 vector = fightCenterTransform.position - Vector3.right * 8f;
		SetCurrentCoroutine(StartCoroutine(CornerShockwavesCoroutine(vector, Vector2.right, 4)));
	}

	private IEnumerator CornerShockwavesCoroutine(Vector2 pos, Vector2 dir, int numShockWaves)
	{
		Crisanta.DamageByContact = false;
		if (!Crisanta.Animator.GetCurrentAnimatorStateInfo(0).IsName("HIDDEN"))
		{
			yield return StartCoroutine(BlinkOut());
		}
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 0f;
		Crisanta.Controller.PlatformCharacterPhysics.Velocity = Vector2.zero;
		Crisanta.Controller.SmartPlatformCollider.enabled = false;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = false;
		base.transform.position = pos;
		Crisanta.Controller.SmartPlatformCollider.enabled = true;
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
		LookAtPenitent();
		Crisanta.Audio.PlayTeleportIn_AUDIO();
		Crisanta.AnimatorInyector.BlinkIn();
		yield return new WaitForSeconds(0.5f);
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.DamageByContact = true;
		ignoreAnimDispl = true;
		float prevSpeed = bladeAttack.projectileSpeed;
		bool up = true;
		Crisanta.GhostTrail.EnableGhostTrail = false;
		Crisanta.Animator.speed = 1f;
		for (int i = 0; i < numShockWaves; i++)
		{
			if (up)
			{
				Crisanta.AnimatorInyector.UpwardsSlash();
			}
			else
			{
				Crisanta.AnimatorInyector.DownwardsSlash();
			}
			if (i == numShockWaves - 1)
			{
				bladeAttack.projectileSpeed *= 1.2f;
				Crisanta.AnimatorInyector.Guard(active: true);
				yield return new WaitForSeconds(0.2f);
				Crisanta.AnimatorInyector.Guard(active: false);
			}
			yield return new WaitUntil(() => lightAttack.damageOnEnterArea || heavyAttack.damageOnEnterArea);
			bladeAttack.Shoot(dir);
			yield return StartCoroutine(BlockUntilAnimationEnds());
			up = !up;
		}
		Crisanta.Animator.speed = 1.5f;
		Crisanta.GhostTrail.EnableGhostTrail = true;
		bladeAttack.projectileSpeed = prevSpeed;
		ignoreAnimDispl = false;
		float prevAnimSpeed = Crisanta.Animator.speed;
		Crisanta.Animator.speed = 0.6f;
		Crisanta.AnimatorInyector.SetStayKneeling(active: true);
		Crisanta.Animator.Play("HURT_2_KNEELED");
		yield return new WaitForSeconds(3f);
		Crisanta.AnimatorInyector.SetStayKneeling(active: false);
		yield return new WaitForSeconds(0.5f);
		Crisanta.Animator.speed = prevAnimSpeed;
		StartWaitingPeriod(0.1f);
	}

	public void OnEnterGuard()
	{
		_fsm.ChangeState(stGuard);
	}

	public void OnExitGuard()
	{
		_fsm.ChangeState(stAction);
	}

	public void OnExitsDownslash()
	{
		_waitingForAnimationFinish = false;
	}

	public void OnExitMeleeAttack()
	{
		Debug.Log("TIME: " + Time.time + " OnExitMeleeAttack called.");
		_waitingForAnimationFinish = false;
	}

	private IEnumerator BlockUntilAnimationEnds()
	{
		_waitingForAnimationFinish = true;
		while (_waitingForAnimationFinish)
		{
			yield return null;
		}
		Debug.Log("<color=yellow>Melee animation ended</color>");
	}

	public void CounterImpactShockwave()
	{
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		instantLightningAttack.SummonAreaOnPoint(base.transform.position - num * Vector3.right);
	}

	public void OnHitReactionAnimationCompleted()
	{
		Debug.Log("HIT REACTION COMPLETED. RECOVERING FALSE");
		SetRecovering(recovering: false);
		_currentRecoveryHits = 0;
		QueueRecoveryAction();
		StartWaitingPeriod(0.5f);
	}

	private void QueueRecoveryAction()
	{
		if (DistanceToPenitent() < 3f)
		{
			float dirFromOrientation = GetDirFromOrientation();
			Vector2 vector = new Vector2(0f - dirFromOrientation, 0f);
			if (HasSpaceInDirection(vector * 3f))
			{
				QueuedActionsPush(Crisanta_ATTACKS.BACKFLIP_LOW);
			}
			else if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				QueuedActionsPush(Crisanta_ATTACKS.FORWARD_FLIP);
			}
			else
			{
				QueuedActionsPush(Crisanta_ATTACKS.INSTANT_GUARD);
			}
		}
		else
		{
			QueuedActionsPush(Crisanta_ATTACKS.BACKFLIP_HIGH);
		}
	}

	public void AttackDisplacement(float duration, float displacement, bool trail)
	{
		duration *= shortDisplDurationFactor;
		displacement *= shortDisplLengthFactor;
		SetGhostTrail(trail);
		Crisanta.DamageByContact = false;
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

	public void AttackDisplacementToPoint(Vector2 point, float offset, float baseSpeed, bool trail)
	{
		offset *= mediumDisplOffsetFactor;
		baseSpeed *= mediumDisplSpeedFactor;
		SetGhostTrail(trail);
		sparkParticles.Play();
		Crisanta.DamageByContact = false;
		Ease ease = Ease.OutQuad;
		LookAtTarget(point);
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		point -= Vector2.right * num * offset;
		Vector2 vector = point - (Vector2)base.transform.position;
		float num2 = Vector2.Distance(point, base.transform.position);
		if (num2 < 2f)
		{
			num2 = 0f;
		}
		Vector2 dir = Vector2.right * num * num2;
		float duration = num2 / baseSpeed;
		dir = ClampToFightBoundaries(dir);
		base.transform.DOMove(base.transform.position + (Vector3)dir, duration).SetEase(ease).OnComplete(delegate
		{
			AfterDisplacement();
		});
	}

	private void AfterDisplacement()
	{
		sparkParticles.Stop();
		Crisanta.DamageByContact = true;
		SetGhostTrail(active: false);
	}

	public void BackDisplacement(float duration, float displacement)
	{
		SetGhostTrail(active: true);
		Crisanta.DamageByContact = false;
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
		Crisanta.Input.HorizontalInput = horizontalInput;
	}

	public void StopRunning()
	{
		Crisanta.Input.HorizontalInput = 0f;
		Crisanta.Controller.PlatformCharacterPhysics.HSpeed = 0f;
	}

	public bool CloseToPointX(Vector2 p, float closeDistance = 0.1f)
	{
		return Mathf.Abs(p.x - base.transform.position.x) < closeDistance;
	}

	public bool CloseToTarget(float closeDistance = 0.5f)
	{
		Transform target = GetTarget();
		return Mathf.Abs(target.position.x - base.transform.position.x) < closeDistance;
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	public void Death()
	{
		LaunchAction(Crisanta_ATTACKS.DEATH);
	}

	private IEnumerator DeathBackflip()
	{
		yield return StartCoroutine(WaitCloseToGround());
		LookAtPenitent();
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		float d = GetDirFromOrientation();
		float maxWalkSpeed = Crisanta.Controller.MaxWalkingSpeed;
		Crisanta.Controller.MaxWalkingSpeed *= 1f;
		Crisanta.Input.HorizontalInput = 0f - d;
		SetGhostTrail(active: true);
		CancelAttacks();
		Crisanta.AnimatorInyector.AirAttack(active: false);
		Crisanta.AnimatorInyector.Blinkslash(blinkIn: false);
		Crisanta.AnimatorInyector.BlinkIn();
		Crisanta.AnimatorInyector.DeathBackflip();
		yield return StartCoroutine(JumpPress(isLongPress: false));
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(WaitCloseToGround());
		RepositionBackflipEnd();
		Crisanta.Controller.MaxWalkingSpeed = maxWalkSpeed;
	}

	private IEnumerator DeathBackflipAfterRedux()
	{
		CancelAttacks();
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.AnimatorInyector.AirAttack(active: false);
		Crisanta.AnimatorInyector.Blinkslash(blinkIn: false);
		Crisanta.AnimatorInyector.BlinkIn();
		if (base.transform.position.x > fightCenterTransform.position.x)
		{
			RepositionBackflipStart(-1f, Vector3.right);
		}
		else
		{
			RepositionBackflipStart(1f, Vector3.left);
		}
		yield return StartCoroutine(JumpPress(isLongPress: false));
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(WaitCloseToGround());
		RepositionBackflipEnd();
	}

	private IEnumerator DeathBackflipForRedux()
	{
		CancelAttacks();
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.AnimatorInyector.AirAttack(active: false);
		Crisanta.AnimatorInyector.Blinkslash(blinkIn: false);
		Crisanta.AnimatorInyector.BlinkIn();
		yield return StartCoroutine(WaitCloseToGround());
		if (base.transform.position.x > fightCenterTransform.position.x)
		{
			RepositionBackflipStart(-1f, Vector3.right);
			Core.Logic.Penitent.DrivePlayer.MoveToPosition(tpoWaypointRight.transform.position, EntityOrientation.Left);
		}
		else
		{
			RepositionBackflipStart(1f, Vector3.left);
			Core.Logic.Penitent.DrivePlayer.MoveToPosition(tpoWaypointLeft.transform.position, EntityOrientation.Right);
		}
		yield return StartCoroutine(JumpPress(!IsCrisantaCloseToCenter()));
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(WaitCloseToGround());
		RepositionBackflipEnd();
	}

	private void RepositionBackflipStart(float dir, Vector3 dirToLook)
	{
		LookAtTarget(base.transform.position + dirToLook);
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.Input.HorizontalInput = dir;
		SetGhostTrail(active: true);
		CancelAttacks();
		Crisanta.AnimatorInyector.AirAttack(active: false);
		Crisanta.AnimatorInyector.DeathBackflip();
	}

	private void RepositionBackflipEnd()
	{
		Crisanta.AnimatorInyector.BackflipLand();
		Crisanta.GhostTrail.EnableGhostTrail = false;
		Crisanta.Input.HorizontalInput = 0f;
	}

	private void DeathAction()
	{
		StopBehaviour();
		StopAllCoroutines();
		base.transform.DOKill(complete: true);
		FadeWidget.instance.ClearFade();
		if (!Crisanta.Controller.SmartPlatformCollider.EnableCollision2D)
		{
			base.transform.position = new Vector3(base.transform.position.x, fightCenterTransform.position.y + 0.5f, base.transform.position.z);
			Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
		}
		StartAttackAction();
		if (Crisanta.IsCrisantaRedux)
		{
			StartCoroutine(DeathBackflipAfterRedux());
		}
		else if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))
		{
			StartCoroutine(DeathBackflip());
		}
		else
		{
			StartCoroutine(DeathBackflipForRedux());
		}
	}

	public void SubstituteForExecution()
	{
		CancelAttacks();
		if (Core.InventoryManager.IsTrueSwordHeartEquiped() || Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S19"))
		{
			if (Crisanta.IsCrisantaRedux)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(trueEndingExecutionPrefab, base.transform.position, base.transform.rotation);
				SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
				componentInChildren.flipX = Crisanta.Status.Orientation == EntityOrientation.Left;
				FakeExecution component = gameObject.GetComponent<FakeExecution>();
				Singleton<Core>.Instance.StartCoroutine(WaitAndSetDialogMode(component));
			}
			else
			{
				crisantaNPC.transform.position = base.transform.position;
				crisantaNPC.SetActive(value: true);
				if (IsPenitentOnRightSide())
				{
					crisantaNPC.transform.localScale = new Vector3(-1f, 1f, 1f);
				}
				EntityOrientation orientation = ((GetDirToPenitent().x > 0f) ? EntityOrientation.Left : EntityOrientation.Right);
				Core.Logic.Penitent.SetOrientation(orientation);
				PlayMakerFSM.BroadcastEvent("REPOSITION REDUX");
			}
		}
		else
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(executionPrefab, base.transform.position, base.transform.rotation);
			gameObject2.transform.localScale = ((Crisanta.Status.Orientation != EntityOrientation.Left) ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f));
		}
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator WaitAndSetDialogMode(FakeExecution execution)
	{
		Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
		yield return new WaitUntil(() => execution.BeingUsed);
		p.Shadow.ManuallyControllingAlpha = true;
		Tween t2 = DOTween.To(() => p.Shadow.GetShadowAlpha(), delegate(float x)
		{
			p.Shadow.SetShadowAlpha(x);
		}, 0f, 0.2f);
		EntityOrientation targetOrientation = ((!(execution.transform.position.x > p.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
		execution.InstanceOrientation = ((execution.transform.position.x > p.transform.position.x) ? EntityOrientation.Left : EntityOrientation.Right);
		p.Animator.SetBool("IS_DIALOGUE_MODE", value: true);
		yield return new WaitUntil(() => !execution.BeingUsed);
		p.SetOrientation(targetOrientation);
		t2 = DOTween.To(() => p.Shadow.GetShadowAlpha(), delegate(float x)
		{
			p.Shadow.SetShadowAlpha(x);
		}, 1f, 0.2f);
		t2.OnComplete(delegate
		{
			p.Shadow.ManuallyControllingAlpha = false;
		});
	}

	private bool IsPenitentClose()
	{
		return Vector2.Distance(Core.Logic.Penitent.GetPosition(), base.transform.position) < 3f;
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
		base.transform.rotation = Quaternion.identity;
		sparkParticles.Stop();
		sparkParticlesMid.Stop();
		if (Crisanta.Controller.PlatformCharacterPhysics.GravityScale == 0f)
		{
			Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
			Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
			Crisanta.DamageByContact = true;
			CancelAttacks();
			CancelCombo();
			StopAllCoroutines();
			SetGhostTrail(active: false);
			base.transform.DOKill();
			FadeWidget.instance.ClearFade();
			IssueBackflip();
			return;
		}
		Crisanta.Controller.SmartPlatformCollider.EnableCollision2D = true;
		Crisanta.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		Crisanta.DamageByContact = true;
		CancelAttacks();
		CancelCombo();
		StopAllCoroutines();
		SetGhostTrail(active: false);
		base.transform.DOKill();
		FadeWidget.instance.ClearFade();
		Crisanta.AnimatorInyector.Parry();
		if (Crisanta.IsCrisantaRedux)
		{
			Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
			StartCoroutine(FastFadeCoroutine());
			Vector3 position = base.transform.position;
			position.x = (position.x + Core.Logic.Penitent.GetPosition().x) / 2f;
			dramaLightningAttack.SummonAreaOnPoint(position);
			lightningAttack.SummonAreaOnPoint(position);
			Core.Logic.CameraManager.ShockwaveManager.Shockwave(position, 0.5f, 0.3f, 1.5f);
		}
		BackDisplacement(backDisplDurationFactor, backDisplLengthFactor);
		SetRecovering(recovering: true);
		justParried = true;
		StartWaitingPeriod(afterParryWaitTime);
	}

	private IEnumerator FastFadeCoroutine()
	{
		yield return FadeWidget.instance.FadeCoroutine(new Color(0f, 0f, 0f, 0.2f), new Color(0f, 0f, 0f, 0.4f), 0.05f, toBlack: true, null);
		yield return FadeWidget.instance.FadeCoroutine(new Color(1f, 1f, 1f, 0.4f), new Color(1f, 1f, 1f, 0.8f), 0.05f, toBlack: true, null);
		yield return FadeWidget.instance.FadeCoroutine(new Color(1f, 1f, 1f, 1f), Color.clear, 0.05f, toBlack: true, null);
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
			CancelCombo();
			CancelAttacks();
			StopAllCoroutines();
			Crisanta.AnimatorInyector.Hurt();
			base.transform.DOKill(complete: true);
			FadeWidget.instance.ClearFade();
			LookAtPenitent();
			BackDisplacement(0.3f, 0.4f);
			Debug.Log("HURT //" + _currentRecoveryHits + " // Recovering = " + ((!_recovering) ? "FALSE" : "TRUE"));
			_currentRecoveryHits++;
			if (_currentRecoveryHits >= maxHitsInRecovery)
			{
				Debug.Log("<color=magenta>COUNTER</color>");
				_currentRecoveryHits = 0;
				LaunchAction(Crisanta_ATTACKS.BACKFLIP_LOW);
			}
			else
			{
				StartWaitingPeriod(1f);
			}
		}
	}

	public void OnGuardHit()
	{
		if (_fsm.IsInState(stGuard))
		{
			Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
			Crisanta.Audio.PlayParry_AUDIO();
			StopAllCoroutines();
			CancelCombo();
			base.transform.DOKill(complete: true);
			FadeWidget.instance.ClearFade();
			LookAtPenitent();
			if (queuedActions != null)
			{
				queuedActions.Clear();
			}
			comboActionsRemaining = 0;
			CancelCombo();
			Crisanta.AnimatorInyector.Guard(active: false);
			LaunchAction(Crisanta_ATTACKS.COUNTER_BLINK);
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		Crisanta.SetOrientation((!(targetPos.x > Crisanta.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
		particleParent.transform.localScale = new Vector3(GetDirFromOrientation(), 1f, 1f);
	}

	public override void StopMovement()
	{
	}

	public void SetGhostTrail(bool active)
	{
		Crisanta.GhostTrail.EnableGhostTrail = active || Crisanta.IsCrisantaRedux;
	}

	private float GetDirFromOrientation()
	{
		return (Entity.Status.Orientation != 0) ? (-1f) : 1f;
	}

	private Vector2 ClampToFightBoundaries(Vector2 dir)
	{
		Vector2 vector = dir;
		Debug.Log("<color=cyan>DRAWING DIR LINE IN GREEN</color>");
		Debug.DrawLine(base.transform.position, base.transform.position + (Vector3)vector, Color.green, 5f);
		if (Physics2D.RaycastNonAlloc(base.transform.position, dir, results, dir.magnitude, fightBoundariesLayerMask) > 0)
		{
			Debug.DrawLine(base.transform.position, results[0].point, Color.red, 5f);
			vector = vector.normalized * results[0].distance;
			vector *= 0.9f;
			Debug.Log("<color=cyan>CLAMPING DISPLACEMENT</color>");
		}
		return vector;
	}

	public bool HasSpaceInDirection(Vector2 dir)
	{
		Vector2 vector = dir;
		Debug.DrawLine(base.transform.position, base.transform.position + (Vector3)vector, Color.green, 5f);
		if (Physics2D.RaycastNonAlloc(base.transform.position, dir, results, dir.magnitude, fightBoundariesLayerMask) > 0)
		{
			return false;
		}
		return true;
	}

	public void OnDrawGizmos()
	{
	}
}
