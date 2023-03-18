using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;
using Gameplay.GameControllers.Bosses.BossFight;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Penitent.Gizmos;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.IA;

public class BejeweledSaintBehaviour : EnemyBehaviour
{
	public enum BOSS_STATES
	{
		WAITING,
		MID_ACTION,
		AVAILABLE_FOR_ACTION
	}

	public enum BEJEWELLED_ATTACKS
	{
		STAFF,
		BEAMS,
		HANDS,
		TRIPLE_STAFF,
		HANDS_LINE,
		ONSLAUGHT
	}

	[Serializable]
	public class BejewelledAttackConfig
	{
		public BEJEWELLED_ATTACKS atk;

		public float cooldown;

		public float currentTimer;

		public float recovery;

		public bool CanBeUsed()
		{
			return currentTimer <= 0f;
		}

		public void ResetCooldown()
		{
			currentTimer = cooldown;
		}
	}

	public enum Phases
	{
		Phase1,
		Phase2,
		Phase3
	}

	[FoldoutGroup("Attack config", true, 0)]
	public List<BejewelledAttackConfig> attacksConfig;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public BEJEWELLED_ATTACKS lastAttack;

	private Coroutine currentCoroutine;

	private List<BEJEWELLED_ATTACKS> currentlyAvailableAttacks;

	public List<BEJEWELLED_ATTACKS> Phase1AvailableAttacks;

	public List<BEJEWELLED_ATTACKS> Phase2AvailableAttacks;

	public List<BEJEWELLED_ATTACKS> Phase3AvailableAttacks;

	public StateMachine<BejeweledSaintBehaviour> _fsm;

	public State<BejeweledSaintBehaviour> stIntro;

	public State<BejeweledSaintBehaviour> stAction;

	public State<BejeweledSaintBehaviour> stChasePlayer;

	public State<BejeweledSaintBehaviour> stCollapsed;

	public State<BejeweledSaintBehaviour> stMoveToPoint;

	public State<BejeweledSaintBehaviour> stDeath;

	private BejeweledSaintHead _bejeweledSaintHead;

	public float ChaseCoolDown;

	private float _currentChaseCoolDown;

	public float holdersOffset;

	public float maxOffset = 1f;

	public float offsetFrequency = 1f;

	public float MinChasingDistance;

	public float smoothTime = 0.3f;

	public Vector2 movePoint;

	private Vector3 velocity = Vector3.zero;

	private float onslaughtCounter;

	public float MinHandsAttackInterval;

	public float MaxHandsAttackInterval;

	private BejewelledAttackConfig _currentConfig;

	public BossFightManager BossFight { get; set; }

	public Phases CurrentPhase { get; set; }

	public RootMotionDriver StaffRoot { get; private set; }

	public BejeweledSaintArmAttack ArmAttack { get; set; }

	public int SweepAttacksAmount { get; set; }

	public bool FightStarted { get; set; }

	public bool IsPerformingAttack { get; set; }

	public bool IsBossCollapsed { get; set; }

	public float HandsAttackInterval { get; set; }

	public bool HandsUp => _bejeweledSaintHead.WholeBoss.HandsManager.HandsUp;

	private float DistanceToStaff => Mathf.Abs(_bejeweledSaintHead.WholeBoss.transform.position.x - StaffRoot.transform.position.x);

	public event Action<BejeweledSaintBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		SetCurrentAttacks();
		stIntro = new BejeweledSaint_StIntro();
		stAction = new BejeweledSaint_StAction();
		stCollapsed = new BejeweledSaint_StCollapsed();
		stChasePlayer = new BejeweledSaint_StChasePlayer();
		stDeath = new BejeweledSaint_StDeath();
		stMoveToPoint = new BejeweledSaint_StMoveToPoint();
		_fsm = new StateMachine<BejeweledSaintBehaviour>(this, stAction);
	}

	public override void OnStart()
	{
		base.OnStart();
		_bejeweledSaintHead = (BejeweledSaintHead)Entity;
		_currentChaseCoolDown = ChaseCoolDown;
		StaffRoot = _bejeweledSaintHead.WholeBoss.AttackArm.StaffRoot;
		BossFight = UnityEngine.Object.FindObjectOfType<BossFightManager>();
		ArmAttack = _bejeweledSaintHead.WholeBoss.GetComponentInChildren<BejeweledSaintArmAttack>();
		VisualSensor.OnPenitentEnter += OnVisualSensorPenitentEnter;
		VisualSensor.OnPenitentExit += OnVisualSensorPenitentExit;
		HearingSensor.OnPenitentEnter += OnHearingSensorPenitentEnter;
		BsHolderManager holdersManager = _bejeweledSaintHead.WholeBoss.HoldersManager;
		holdersManager.OnBossCollapse = (Core.SimpleEvent)Delegate.Combine(holdersManager.OnBossCollapse, new Core.SimpleEvent(OnBossCollapse));
		BejeweledSaintBoss wholeBoss = _bejeweledSaintHead.WholeBoss;
		wholeBoss.OnRaised = (Core.SimpleEvent)Delegate.Combine(wholeBoss.OnRaised, new Core.SimpleEvent(OnRaised));
		_bejeweledSaintHead.OnDeath += BossOnDeath;
		HandsAttackInterval = UnityEngine.Random.Range(MinHandsAttackInterval, MaxHandsAttackInterval);
		_bejeweledSaintHead.WholeBoss.SetIntroPosition();
	}

	private void OnDestroy()
	{
		VisualSensor.OnPenitentEnter -= OnVisualSensorPenitentEnter;
		VisualSensor.OnPenitentExit -= OnVisualSensorPenitentExit;
		HearingSensor.OnPenitentEnter -= OnHearingSensorPenitentEnter;
		BsHolderManager holdersManager = _bejeweledSaintHead.WholeBoss.HoldersManager;
		holdersManager.OnBossCollapse = (Core.SimpleEvent)Delegate.Remove(holdersManager.OnBossCollapse, new Core.SimpleEvent(OnBossCollapse));
		BejeweledSaintBoss wholeBoss = _bejeweledSaintHead.WholeBoss;
		wholeBoss.OnRaised = (Core.SimpleEvent)Delegate.Remove(wholeBoss.OnRaised, new Core.SimpleEvent(OnRaised));
		_bejeweledSaintHead.OnDeath -= BossOnDeath;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateAttackTimers();
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

	public void LaunchAction(BEJEWELLED_ATTACKS atk)
	{
		_currentConfig = attacksConfig.Find((BejewelledAttackConfig x) => x.atk == atk);
		lastAttack = atk;
		switch (atk)
		{
		case BEJEWELLED_ATTACKS.STAFF:
			Debug.Log("STAFF");
			IssueStaff();
			break;
		case BEJEWELLED_ATTACKS.BEAMS:
			Debug.Log("BEAMS");
			IssueBeams();
			break;
		case BEJEWELLED_ATTACKS.HANDS:
			Debug.Log("HANDS");
			IssueHands();
			break;
		case BEJEWELLED_ATTACKS.TRIPLE_STAFF:
			Debug.Log("TRIPLE STAFF");
			IssueMultiStaff();
			break;
		case BEJEWELLED_ATTACKS.HANDS_LINE:
			Debug.Log("HANDS LINE");
			IssueHandsLine();
			break;
		case BEJEWELLED_ATTACKS.ONSLAUGHT:
			Debug.Log("ONSLAUGHT");
			IssueOnslaught();
			break;
		}
		_currentConfig.ResetCooldown();
	}

	private bool IfAttackOnCooldown(BEJEWELLED_ATTACKS atk)
	{
		BejewelledAttackConfig bejewelledAttackConfig = attacksConfig.Find((BejewelledAttackConfig x) => x.atk == atk);
		return !bejewelledAttackConfig.CanBeUsed();
	}

	public BEJEWELLED_ATTACKS GetNewAttack()
	{
		BEJEWELLED_ATTACKS[] array = new BEJEWELLED_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<BEJEWELLED_ATTACKS> list = new List<BEJEWELLED_ATTACKS>(array);
		list.Remove(lastAttack);
		list.RemoveAll(IfAttackOnCooldown);
		if (_bejeweledSaintHead.WholeBoss.HandsManager.IsBusy)
		{
			list.Remove(BEJEWELLED_ATTACKS.HANDS);
			list.Remove(BEJEWELLED_ATTACKS.HANDS_LINE);
			list.Remove(BEJEWELLED_ATTACKS.TRIPLE_STAFF);
		}
		if (list.Count == 0)
		{
			list.Add(BEJEWELLED_ATTACKS.STAFF);
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

	public IEnumerator WaitForState(State<BejeweledSaintBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	public void UpdateAttackTimers()
	{
		foreach (BejewelledAttackConfig item in attacksConfig)
		{
			if (item.currentTimer > 0f)
			{
				item.currentTimer -= Time.deltaTime;
			}
		}
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
		Debug.Log("WAIT PERIOD FINISH");
		ActionFinished();
	}

	public Phases GetCurrentPhase()
	{
		if (BossFight == null)
		{
			CurrentPhase = Phases.Phase1;
			return CurrentPhase;
		}
		switch (BossFight.CurrentBossPhaseId)
		{
		case "state1":
			CurrentPhase = Phases.Phase1;
			break;
		case "state2":
			CurrentPhase = Phases.Phase2;
			_bejeweledSaintHead.WholeBoss.HoldersManager.holdersToFall = 4;
			break;
		case "state3":
			CurrentPhase = Phases.Phase3;
			_bejeweledSaintHead.WholeBoss.HoldersManager.holdersToFall = 5;
			break;
		}
		return CurrentPhase;
	}

	public float DistanceToTarget(Vector3 target)
	{
		if (StaffRoot == null)
		{
			return 0f;
		}
		return Vector2.Distance(StaffRoot.transform.position, target);
	}

	public float XDistanceToTarget(Vector3 target)
	{
		if (StaffRoot == null)
		{
			return 0f;
		}
		return Mathf.Abs(StaffRoot.transform.position.x - target.x);
	}

	private void OnHearingSensorPenitentEnter()
	{
	}

	private void OnVisualSensorPenitentEnter()
	{
	}

	private void OnVisualSensorPenitentExit()
	{
	}

	private void SetCurrentAttacks()
	{
		CurrentPhase = GetCurrentPhase();
		switch (CurrentPhase)
		{
		case Phases.Phase1:
			currentlyAvailableAttacks = Phase1AvailableAttacks;
			break;
		case Phases.Phase2:
			currentlyAvailableAttacks = Phase2AvailableAttacks;
			break;
		case Phases.Phase3:
			currentlyAvailableAttacks = Phase3AvailableAttacks;
			break;
		}
	}

	private void OnRaised()
	{
		if (IsBossCollapsed)
		{
			IsBossCollapsed = false;
		}
		_fsm.ChangeState(stAction);
		SetCurrentAttacks();
		StartWaitingPeriod(0.5f);
	}

	private void OnBossCollapse()
	{
		if (!IsBossCollapsed)
		{
			IsBossCollapsed = true;
		}
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: false);
		StopCoroutine(currentCoroutine);
		_bejeweledSaintHead.AnimatorInyector.ResetAttack();
		_fsm.ChangeState(stCollapsed);
	}

	private void BossOnDeath()
	{
		_bejeweledSaintHead.WholeBoss.Audio.PlayDeath();
		StopBehaviour();
	}

	public override void Idle()
	{
		base.IsChasing = false;
		_currentChaseCoolDown = ChaseCoolDown;
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		_currentChaseCoolDown -= Time.deltaTime;
		if (!(_currentChaseCoolDown > 0f))
		{
			Transform transform = _bejeweledSaintHead.WholeBoss.transform;
			Vector3 position = _bejeweledSaintHead.Target.transform.position;
			position.x -= DistanceToStaff;
			position.y = transform.position.y;
			Vector3 position2 = Vector3.SmoothDamp(transform.position, position, ref velocity, smoothTime);
			transform.position = position2;
		}
	}

	public override void Attack()
	{
	}

	public override void Damage()
	{
		GetCurrentPhase();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public IEnumerator IntroCoroutine()
	{
		FightStarted = true;
		_bejeweledSaintHead.WholeBoss.IntroRaise();
		yield return new WaitForSeconds(2f);
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: true);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(2f, Vector3.up * 4f, 60, 0.02f, 0f, default(Vector3), 0.045f, ignoreTimeScale: true);
		yield return new WaitForSeconds(3f);
		EndIntro();
	}

	private void EndIntro()
	{
		_fsm.ChangeState(stAction);
		ArmAttack.SetCurrentFailedAttackLimit();
		StartWaitingPeriod(1.5f);
	}

	public void StartIntro()
	{
		SetCurrentCoroutine(StartCoroutine(IntroCoroutine()));
	}

	public void OnIntroEnds()
	{
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: false);
	}

	public bool IsCloseToPoint(Vector2 p)
	{
		return XDistanceToTarget(p) < 1f;
	}

	public void MoveTowards(Vector2 p)
	{
		Transform transform = _bejeweledSaintHead.WholeBoss.transform;
		p.x -= DistanceToStaff;
		p.y = transform.position.y;
		Vector3 position = Vector3.SmoothDamp(transform.position, p, ref velocity, 0.32f);
		transform.position = position;
	}

	public void StartChasingPlayer()
	{
		_fsm.ChangeState(stChasePlayer);
	}

	public bool IsPlayerInStaffRange()
	{
		return DistanceToTarget(GetTarget().position) < MinChasingDistance;
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	public void UpdateOffset()
	{
		holdersOffset = Mathf.Sin(Time.time * offsetFrequency) * maxOffset;
		Transform transform = _bejeweledSaintHead.WholeBoss.transform;
		transform.localPosition = new Vector2(transform.localPosition.x, _bejeweledSaintHead.WholeBoss.BossHeightPosition.y + holdersOffset);
	}

	public void IssueStaff()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(GetIntoStaffRange(StaffAttack)));
	}

	private IEnumerator GetIntoStaffRange(Action OnRangeCallback)
	{
		StartChasingPlayer();
		yield return StartCoroutine(WaitForState(stAction));
		OnRangeCallback();
	}

	private void StaffAttack()
	{
		SetCurrentCoroutine(StartCoroutine(StaffAttackCoroutine(0.4f, 1.6f, OnStaffAttackEnds)));
	}

	private IEnumerator StaffAttackCoroutine(float aimingSeconds, float recoverySeconds, Action OnAttackEnds)
	{
		_bejeweledSaintHead.AnimatorInyector.BasicStaffAttack();
		yield return new WaitForSeconds(aimingSeconds);
		_bejeweledSaintHead.WholeBoss.AttackArm.SetArmAngle();
		if (CurrentPhase == Phases.Phase3)
		{
			_bejeweledSaintHead.WholeBoss.CastArm.CastSingleBeamDelayed(_bejeweledSaintHead.WholeBoss.AttackArm.impactTransform.position, 0.2f);
		}
		yield return new WaitForSeconds(recoverySeconds);
		OnAttackEnds();
	}

	private void OnStaffAttackEnds()
	{
		_bejeweledSaintHead.WholeBoss.AttackArm.DefaultArmAngle();
		StartWaitingPeriod(0.5f);
	}

	private void IssueMultiStaff()
	{
		StartAttackAction();
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: true);
		Debug.Log("ISSUE MULTI: WAITING FOR RANGE");
		SetCurrentCoroutine(StartCoroutine(GetIntoStaffRange(MultiStaffAttack)));
	}

	private void MultiStaffAttack()
	{
		Debug.Log("MULTI STAFF ATTACK");
		_bejeweledSaintHead.WholeBoss.AttackArm.QuickAttackMode(mode: true);
		SetCurrentCoroutine(StartCoroutine(MultipleStaffAttackCoroutine(3)));
	}

	private IEnumerator MultipleStaffAttackCoroutine(int n)
	{
		while (n > 0)
		{
			yield return StartCoroutine(StaffAttackCoroutine(0.1f, 1f, OnOneMultiStaffAttackFinished));
			n--;
		}
		yield return new WaitForSeconds(0.4f);
		OnMultiStaffAttackEnds();
	}

	private void OnOneMultiStaffAttackFinished()
	{
	}

	private void OnMultiStaffAttackEnds()
	{
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: false);
		_bejeweledSaintHead.WholeBoss.AttackArm.DefaultArmAngle();
		_bejeweledSaintHead.WholeBoss.AttackArm.QuickAttackMode(mode: false);
		StartWaitingPeriod(0.5f);
	}

	private void IssueOnslaught()
	{
		Debug.Log("ISSUE ONSLAUGHT");
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(OnslaughtMasterCoroutine()));
	}

	private void StartMoveToPoint(Vector2 p)
	{
		movePoint = p;
		_fsm.ChangeState(stMoveToPoint);
	}

	private IEnumerator OnslaughtMasterCoroutine()
	{
		Vector2 originPoint = _bejeweledSaintHead.WholeBoss.LeftSweepAttackLimitPosition;
		Vector2 endPoint = _bejeweledSaintHead.WholeBoss.RightSweepAttackLimitPosition;
		int i = 6;
		int counter = 1;
		StartMoveToPoint(originPoint);
		yield return StartCoroutine(WaitForState(stAction));
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: true);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(1f, Vector3.up * 7f, 30, 0.02f, 0f, default(Vector3), 0.045f, ignoreTimeScale: true);
		yield return new WaitForSeconds(1f);
		_bejeweledSaintHead.WholeBoss.AttackArm.QuickAttackMode(mode: true);
		for (; counter <= i; counter++)
		{
			Vector2 nextPoint = Vector2.Lerp(originPoint, endPoint, (float)counter / (float)i);
			StartMoveToPoint(nextPoint);
			yield return StartCoroutine(WaitForState(stAction));
			yield return StartCoroutine(OnslaughtAttackCoroutine());
		}
		_bejeweledSaintHead.WholeBoss.AttackArm.QuickAttackMode(mode: false);
		StartWaitingPeriod(2f);
		_bejeweledSaintHead.AnimatorInyector.SetCackle(cackle: false);
	}

	private IEnumerator OnslaughtAttackCoroutine()
	{
		_bejeweledSaintHead.AnimatorInyector.BasicStaffAttack();
		yield return new WaitForSeconds(0.3f);
		_bejeweledSaintHead.WholeBoss.CastArm.CastSingleBeam(_bejeweledSaintHead.WholeBoss.AttackArm.angleCastCenter.position);
		yield return new WaitForSeconds(0.4f);
	}

	public void IssueHands()
	{
		StartAttackAction();
		SmashHandsAttack();
		StartWaitingPeriod(_currentConfig.recovery);
	}

	public void IssueHandsLine()
	{
		StartAttackAction();
		SmashLineHandsAttack();
		StartWaitingPeriod(_currentConfig.recovery);
	}

	public void SmashHandsAttack()
	{
		_bejeweledSaintHead.WholeBoss.HandsManager.SmashAttack();
	}

	public void SmashLineHandsAttack()
	{
		Vector3 vector = GetTarget().position - _bejeweledSaintHead.transform.position;
		Vector2 dir = Vector2.right * Mathf.Sign(vector.x);
		_bejeweledSaintHead.WholeBoss.HandsManager.LineAttack(base.transform.position, dir);
	}

	private void IssueBeams()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(BeamAttackCoroutine()));
	}

	private IEnumerator BeamAttackCoroutine()
	{
		yield return new WaitForSeconds(0.2f);
		DivineBeamAttack();
		StartWaitingPeriod(_currentConfig.recovery);
	}

	private void DivineBeamAttack()
	{
		_bejeweledSaintHead.WholeBoss.CastArm.DoCastSign();
	}
}
