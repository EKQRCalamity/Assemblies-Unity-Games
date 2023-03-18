using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace.AI;

public class BurntFaceBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct BurntFaceAttackConfig
	{
		public BURNTFACE_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	[Serializable]
	public struct BurntfacePhases
	{
		public BURNTFACE_PHASES phaseId;

		public List<BURNTFACE_ATTACKS> availableAttacks;
	}

	public enum BURNTFACE_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	public enum BURNTFACE_ATTACKS
	{
		ROSARY_CROSS = 0,
		ROSARY_CIRCLE = 1,
		ROSARY_SWEEP = 2,
		ROSARY_INVERTED_CIRCLE = 3,
		HOMING_LASER = 5,
		HOMING_BALLS = 6,
		RELEASE_SECOND_HAND = 7,
		TRIPLEBEAM = 8
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public BURNTFACE_ATTACKS lastAttack;

	[FoldoutGroup("References", 0)]
	public GameObject eyesGameObject;

	[FoldoutGroup("References", 0)]
	public BossPlayerAwareness awareness;

	[FoldoutGroup("References", 0)]
	public BurntFaceHandBehaviour firstHand;

	[FoldoutGroup("References", 0)]
	public BurntFaceHandBehaviour secondHand;

	[FoldoutGroup("Design settings", 0)]
	public List<BurntfacePhases> phases;

	public Transform damageCenterTransform;

	private Transform currentTarget;

	private StateMachine<BurntFaceBehaviour> _fsm;

	private State<BurntFaceBehaviour> stHidden;

	private State<BurntFaceBehaviour> stEyes;

	private State<BurntFaceBehaviour> stHead;

	private State<BurntFaceBehaviour> stIntro;

	private State<BurntFaceBehaviour> stDeath;

	private GameObject _currentPoisonFog;

	private Coroutine currentCoroutine;

	private Vector2 _currentTargetPoint;

	private int _multiAttacksRemaining;

	private string _currentPattern;

	private BOSS_POSITIONS _currentPosition = BOSS_POSITIONS.CENTER;

	private BURNTFACE_PHASES _currentPhase;

	private List<BURNTFACE_ATTACKS> currentlyAvailableAttacks;

	private List<BURNTFACE_ATTACKS> queuedActions;

	private bool useBothHands;

	public int maxLaserAttacks = 3;

	private int _laserAttacks;

	private const float WAITING_PERIOD_AFTER_HEAD_REPOSITION = 0.5f;

	private const string DEACTIVATED_ROSARY_PATTERN = "EMPTY";

	private const float FAST_MOVEMENT_DISTANCE = 4f;

	public BurntFace BurntFace { get; set; }

	public int HandAttackCounter { get; set; }

	public event Action<BurntFaceBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		stEyes = new BurntFaceSt_Eyes();
		stHidden = new BurntFaceSt_Hidden();
		stHead = new BurntFaceSt_Head();
		stIntro = new BurntFaceSt_Intro();
		stDeath = new BurntFaceSt_Death();
		currentlyAvailableAttacks = new List<BURNTFACE_ATTACKS>
		{
			BURNTFACE_ATTACKS.ROSARY_CIRCLE,
			BURNTFACE_ATTACKS.ROSARY_CROSS
		};
		queuedActions = new List<BURNTFACE_ATTACKS>();
	}

	public override void OnStart()
	{
		base.OnStart();
		BurntFace = (BurntFace)Entity;
		ChangeBossState(BOSS_STATES.WAITING);
		_fsm = new StateMachine<BurntFaceBehaviour>(this, stHidden);
		_fsm.ChangeState(stHidden);
	}

	public void StartBossFight()
	{
		_fsm.ChangeState(stIntro);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
	}

	public void StartIntroSequence()
	{
		SetCurrentCoroutine(StartCoroutine(IntroSequenceCoroutine()));
	}

	private IEnumerator IntroSequenceCoroutine()
	{
		_fsm.ChangeState(stEyes);
		Wave();
		ChangePhase(BURNTFACE_PHASES.FIRST);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f);
		yield return new WaitForSeconds(1f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(2.5f, Vector3.down * 1f, 50, 0.2f, 0f);
		base.transform.DOMoveY(base.transform.position.y + 3f, 3.5f);
		yield return new WaitForSeconds(1.5f);
		_fsm.ChangeState(stHead);
		ActivateColliders(activate: false);
		MoveHandBackIn(firstHand, 4.5f, reposition: false);
		yield return new WaitForSeconds(4.5f);
		HandAttackCounter = 0;
		ActivateColliders(activate: true);
		PushToActionQueue(BURNTFACE_ATTACKS.TRIPLEBEAM);
		StartWaitingPeriod(0.1f);
	}

	private void ActivateColliders(bool activate)
	{
		BurntFace.DamageArea.DamageAreaCollider.enabled = activate;
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
		return BurntFace.CurrentLife / BurntFace.Stats.Life.Base;
	}

	private void SetPhase(BurntfacePhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
	}

	private void OnSpawnFinished()
	{
	}

	private void ChangePhase(BURNTFACE_PHASES p)
	{
		BurntfacePhases phase = phases.Find((BurntfacePhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
		float healthPercentage = GetHealthPercentage();
		switch (_currentPhase)
		{
		case BURNTFACE_PHASES.FIRST:
			if (healthPercentage < 0.75f)
			{
				ChangePhase(BURNTFACE_PHASES.SECOND);
			}
			break;
		case BURNTFACE_PHASES.SECOND:
			if (healthPercentage < 0.5f)
			{
				ChangePhase(BURNTFACE_PHASES.LAST);
				Debug.Log("PUSHING RELEASE SECOND HAND INTO QUEUE");
				PushToActionQueue(BURNTFACE_ATTACKS.RELEASE_SECOND_HAND);
			}
			break;
		}
	}

	private void SetCurrentCoroutine(Coroutine c)
	{
		if (currentCoroutine != null)
		{
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

	public void LaunchAction(BURNTFACE_ATTACKS atk)
	{
		if (atk != BURNTFACE_ATTACKS.RELEASE_SECOND_HAND && HandAttackCounter > 2)
		{
			MoveHead(GetNewPosition(), 0.5f);
			Core.Logic.CameraManager.ProCamera2DShake.Shake(1f, Vector3.down * 1f, 30, 0.2f, 0f, default(Vector3), 0f);
			HandAttackCounter = 0;
			return;
		}
		lastAttack = atk;
		switch (atk)
		{
		case BURNTFACE_ATTACKS.ROSARY_CROSS:
			IssueRosaryAttack(firstHand, "CROSS");
			if (useBothHands)
			{
				IssueRosaryAttack(secondHand, "CIRCLE");
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.ROSARY_CIRCLE:
			IssueRosaryAttack(firstHand, "CIRCLE");
			if (useBothHands)
			{
				IssueHomingLaser(secondHand);
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.ROSARY_SWEEP:
			IssueRosaryAttack(firstHand, "SWEEP");
			if (useBothHands)
			{
				IssueHomingBalls(secondHand);
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.ROSARY_INVERTED_CIRCLE:
			IssueRosaryAttack(firstHand, "INVERTED_CIRCLE");
			if (useBothHands)
			{
				IssueHomingBalls(secondHand);
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.TRIPLEBEAM:
			IssueTripleHomingLaser(firstHand);
			if (useBothHands)
			{
				IssueHomingLaser(secondHand);
			}
			PushToActionQueue(BURNTFACE_ATTACKS.ROSARY_INVERTED_CIRCLE);
			break;
		case BURNTFACE_ATTACKS.HOMING_LASER:
			IssueHomingLaser(firstHand);
			if (useBothHands)
			{
				IssueHomingLaser(secondHand);
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.HOMING_BALLS:
			IssueHomingBalls(firstHand);
			if (useBothHands)
			{
				IssueRosaryAttack(secondHand, "CIRCLE");
			}
			HandAttackCounter++;
			break;
		case BURNTFACE_ATTACKS.RELEASE_SECOND_HAND:
			IssueReleaseSecondHand();
			break;
		case (BURNTFACE_ATTACKS)4:
			break;
		}
	}

	private void PushToActionQueue(BURNTFACE_ATTACKS atk)
	{
		queuedActions.Add(atk);
	}

	private BURNTFACE_ATTACKS PopFromActionQueue()
	{
		BURNTFACE_ATTACKS bURNTFACE_ATTACKS = queuedActions[0];
		queuedActions.Remove(bURNTFACE_ATTACKS);
		Debug.Log("<color=red> POPPING ACTION FROM QUEUE</color>");
		return bURNTFACE_ATTACKS;
	}

	public BURNTFACE_ATTACKS GetNewAttack()
	{
		if (queuedActions.Count > 0)
		{
			return PopFromActionQueue();
		}
		BURNTFACE_ATTACKS[] array = new BURNTFACE_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<BURNTFACE_ATTACKS> list = new List<BURNTFACE_ATTACKS>(array);
		list.Remove(lastAttack);
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

	public IEnumerator WaitForState(State<BurntFaceBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	private BOSS_POSITIONS GetNewPosition()
	{
		List<BOSS_POSITIONS> list = new List<BOSS_POSITIONS>();
		list.Add(BOSS_POSITIONS.LEFT);
		list.Add(BOSS_POSITIONS.RIGHT);
		list.Add(BOSS_POSITIONS.CENTER);
		List<BOSS_POSITIONS> list2 = list;
		list2.Remove(_currentPosition);
		return list2[UnityEngine.Random.Range(0, list2.Count)];
	}

	private void MoveHead(BOSS_POSITIONS newPos, float waitingPeriodOnEnd)
	{
		StartAttackAction();
		Debug.Log(">Moving head to position: " + newPos);
		SetCurrentCoroutine(StartCoroutine(MovingIntoRosaryPoint(newPos, waitingPeriodOnEnd)));
	}

	private IEnumerator MovingIntoRosaryPoint(BOSS_POSITIONS pos, float waitingPeriodOnEnd)
	{
		_currentPosition = pos;
		Vector2 point = BurntFace.bossFightPoints.GetHeadPoint(pos.ToString()).position;
		MoveHandOut(2f);
		if (useBothHands)
		{
			MoveHandOut(2f, rightHand: false);
		}
		yield return StartCoroutine(GetIntoStateAndCallback(stEyes, 1.1f, null));
		StartCoroutine(MoveFaceToPosition(point));
		yield return new WaitForSeconds(2.5f);
		OnHeadMoveFinished(waitingPeriodOnEnd);
	}

	private IEnumerator MoveFaceToPosition(Vector2 p)
	{
		float d = Vector2.Distance(p, base.transform.position);
		float time = 1.5f;
		if (d < 4f)
		{
			time = 1f;
		}
		yield return StartCoroutine(GameplayUtils.LerpMoveWithCurveCoroutine(base.transform, base.transform.position, p, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), time));
	}

	private void MoveHandOut(float seconds, bool rightHand = true)
	{
		BurntFaceHandBehaviour burntFaceHandBehaviour = ((!rightHand) ? secondHand : firstHand);
		burntFaceHandBehaviour.Hide(seconds * 0.5f);
		Debug.Log(">Moving hand out of screen");
		burntFaceHandBehaviour.MoveToPosition(new Vector2(burntFaceHandBehaviour.transform.position.x, BurntFace.bossFightPoints.transform.position.y - 15f), seconds, AfterHandOutOfScreen);
	}

	private void AfterHandOutOfScreen(BurntFaceHandBehaviour hand)
	{
		Debug.Log("Hand moved out of screen");
		bool flag = hand == secondHand;
		switch (_currentPosition)
		{
		case BOSS_POSITIONS.LEFT:
			hand.SetFlipX(flag);
			break;
		case BOSS_POSITIONS.CENTER:
			hand.SetFlipX(flag);
			break;
		case BOSS_POSITIONS.RIGHT:
			hand.SetFlipX(!flag);
			break;
		}
		MoveHandBackIn(hand, 1.5f);
	}

	private void DrawDebugCross(Vector2 point, Color c, float seconds)
	{
		float num = 0.6f;
		Debug.DrawLine(point - Vector2.up * num, point + Vector2.up * num, c, seconds);
		Debug.DrawLine(point - Vector2.right * num, point + Vector2.right * num, c, seconds);
	}

	private void MoveHandBackIn(BurntFaceHandBehaviour hand, float seconds, bool reposition = true)
	{
		hand.Show(seconds * 0.5f);
		Vector2 handPosition = GetHandPosition(hand);
		if (!reposition)
		{
			handPosition.x = firstHand.transform.position.x;
		}
		Vector2 vector = new Vector2(handPosition.x, handPosition.y - 8f);
		hand.transform.position = vector;
		hand.MoveToPosition(handPosition, seconds, AfterHandBackIn);
	}

	private Vector2 GetHandPosition(BurntFaceHandBehaviour hand)
	{
		Transform rosaryPoint = BurntFace.bossFightPoints.GetRosaryPoint(_currentPosition.ToString(), hand == secondHand);
		return rosaryPoint.position;
	}

	private void AfterHandBackIn(BurntFaceHandBehaviour h)
	{
	}

	private void OnHeadMoveFinished(float waitingPeriod)
	{
		Debug.Log("<color=cyan>Head move finished. Waiting period: </color>" + waitingPeriod);
		SetCurrentCoroutine(StartCoroutine(GetIntoStateAndCallback(stHead, 0.3f, null)));
		if (waitingPeriod >= 0f)
		{
			StartWaitingPeriod(waitingPeriod);
		}
	}

	private void IssueRosaryAttack(BurntFaceHandBehaviour hand, string patternId)
	{
		if (!IsSecondHand(hand))
		{
			StartAttackAction();
		}
		_currentPattern = patternId;
		Vector2 handPosition = GetHandPosition(hand);
		hand.MoveToPosition(handPosition, 1f, OnHandReachedPosition);
	}

	private void OnHandReachedPosition(BurntFaceHandBehaviour hand)
	{
		if (!BurntFace.Status.Dead)
		{
			StartRosaryAttack(hand);
		}
	}

	private void StartRosaryAttack(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: true);
		SetCurrentCoroutine(StartCoroutine(RosaryAttack(hand)));
	}

	private IEnumerator RosaryAttack(BurntFaceHandBehaviour hand)
	{
		yield return new WaitForSeconds(0.1f);
		hand.rosary.SetPatternFromDatabase(_currentPattern);
		hand.rosary.OnPatternEnded += OnRosaryPatternEnded;
	}

	private void OnRosaryPatternEnded(BurntFaceRosaryManager rosary)
	{
		rosary.OnPatternEnded -= OnRosaryPatternEnded;
		BurntFaceHandBehaviour hand = ((!(firstHand.rosary == rosary)) ? secondHand : firstHand);
		OnRosaryAttackEnds(hand);
	}

	private void OnRosaryAttackEnds(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: false);
		if (hand == firstHand)
		{
			StartWaitingPeriod(0.1f);
		}
	}

	private void IssueTripleHomingLaser(BurntFaceHandBehaviour hand)
	{
		if (!IsSecondHand(hand))
		{
			StartAttackAction();
		}
		Vector2 handPosition = GetHandPosition(hand);
		_laserAttacks = maxLaserAttacks;
		hand.MoveToPosition(handPosition, 0.75f, OnHandReachedTripleLaserPosition);
	}

	private void IssueHomingLaser(BurntFaceHandBehaviour hand)
	{
		if (!IsSecondHand(hand))
		{
			StartAttackAction();
		}
		Vector2 handPosition = GetHandPosition(hand);
		hand.MoveToPosition(handPosition, 1f, OnHandReachedLaserPosition);
	}

	private void OnHandReachedLaserPosition(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: true);
		float duration = ((!IsSecondHand(hand)) ? 2f : 1.5f);
		hand.targetedBeamAttack.DelayedTargetedBeam(Core.Logic.Penitent.transform, 0.75f, duration);
		hand.targetedBeamAttack.OnAttackFinished += OnLaserAttackFinished;
	}

	private void OnHandReachedTripleLaserPosition(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: true);
		float duration = ((!IsSecondHand(hand)) ? 1f : 1f);
		hand.targetedBeamAttack.DelayedTargetedBeam(Core.Logic.Penitent.transform, 0.5f, duration);
		hand.targetedBeamAttack.OnAttackFinished += OnNumberedLaserAttackFinished;
	}

	private bool IsSecondHand(BurntFaceHandBehaviour hand)
	{
		return hand == secondHand;
	}

	private void OnNumberedLaserAttackFinished(BossHomingLaserAttack laser)
	{
		BurntFaceHandBehaviour burntFaceHandBehaviour = ((!(firstHand.targetedBeamAttack == laser)) ? secondHand : firstHand);
		burntFaceHandBehaviour.SetMuzzleFlash(on: false);
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnNumberedLaserAttackFinished;
		_laserAttacks--;
		Vector2 handPosition = GetHandPosition(burntFaceHandBehaviour);
		if (_laserAttacks > 0)
		{
			burntFaceHandBehaviour.MoveToPosition(handPosition, 1f, OnHandReachedTripleLaserPosition);
		}
		else if (burntFaceHandBehaviour == firstHand)
		{
			StartWaitingPeriod(0.1f);
		}
	}

	private void OnLaserAttackFinished(BossHomingLaserAttack laser)
	{
		BurntFaceHandBehaviour burntFaceHandBehaviour = ((!(firstHand.targetedBeamAttack == laser)) ? secondHand : firstHand);
		burntFaceHandBehaviour.SetMuzzleFlash(on: false);
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnLaserAttackFinished;
		if (burntFaceHandBehaviour == firstHand)
		{
			StartWaitingPeriod(0.1f);
		}
	}

	private void IssueReleaseSecondHand()
	{
		StartAttackAction();
		Debug.Log("RELEASING SECOND HAND");
		Vector2 handPosition = GetHandPosition(firstHand);
		firstHand.MoveToPosition(handPosition, 1f, OnHandReachedReleasePosition);
	}

	private void OnHandReachedReleasePosition(BurntFaceHandBehaviour hand)
	{
		AfterHandOutOfScreen(secondHand);
		EndReleaseHand();
	}

	private void EndReleaseHand()
	{
		useBothHands = true;
		StartWaitingPeriod(2f);
	}

	private void IssueHomingBalls(BurntFaceHandBehaviour hand)
	{
		if (!IsSecondHand(hand))
		{
			StartAttackAction();
		}
		Vector2 handPosition = GetHandPosition(hand);
		hand.MoveToPosition(handPosition, 1f, OnHandReachedHomingBallsPosition);
	}

	private void OnHandReachedHomingBallsPosition(BurntFaceHandBehaviour hand)
	{
		StartHomingBallsAttack(hand);
	}

	private void StartHomingBallsAttack(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: true);
		SetCurrentCoroutine(StartCoroutine(HomingBallsCoroutine(hand)));
	}

	private IEnumerator HomingBallsCoroutine(BurntFaceHandBehaviour hand)
	{
		yield return new WaitForSeconds(0.5f);
		int j = 8;
		j += UnityEngine.Random.Range(-1, 3);
		float minrandomX = -1f;
		float maxrandomX = 1f;
		float minrandomY = -0.5f;
		float maxrandomY = 0.2f;
		for (int i = 0; i < j; i++)
		{
			hand.homingBallsLauncher.projectileSource = hand.targetedBeamAttack.transform;
			Vector2 dirToPen = GetDirToPenitent(hand.targetedBeamAttack.transform.position);
			Vector2 dir = dirToPen;
			dir += new Vector2(UnityEngine.Random.Range(minrandomX, maxrandomX), UnityEngine.Random.Range(minrandomY, maxrandomY));
			StraightProjectile p = hand.homingBallsLauncher.Shoot(dir.normalized);
			AcceleratedProjectile ap = p.GetComponent<AcceleratedProjectile>();
			ap.SetAcceleration(dirToPen.normalized * 6f);
			ap.SetBouncebackData(damageCenterTransform, Vector2.zero, Mathf.RoundToInt(Core.Logic.Penitent.Stats.Strength.Final * 4f));
			yield return new WaitForSeconds(0.5f);
		}
		OnCastHomingBallsEnds(hand);
	}

	private Vector2 GetDirToPenitent(Vector3 from)
	{
		return Core.Logic.Penitent.transform.position + Vector3.up * 0.8f - from;
	}

	private void OnCastHomingBallsEnds(BurntFaceHandBehaviour hand)
	{
		hand.SetMuzzleFlash(on: false);
		if (hand == firstHand)
		{
			StartWaitingPeriod(0.1f);
		}
	}

	private IEnumerator GetIntoStateAndCallback(State<BurntFaceBehaviour> newSt, float waitSeconds, Action callback)
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

	public void SetHidingLevel(int i)
	{
		BurntFace.AnimatorInyector.SetHidingLevel(i);
	}

	public void EnableDamage(bool enable)
	{
		BurntFace.DamageArea.DamageAreaCollider.enabled = enable;
	}

	public void ShowEyes(bool show)
	{
		eyesGameObject.SetActive(show);
	}

	public void StartDeathSequence()
	{
		Core.Logic.Penitent.Status.Invulnerable = true;
		BurntFace.AnimatorInyector.Death();
		StopAllCoroutines();
		SetCurrentCoroutine(StartCoroutine(DeathSequenceCoroutine()));
	}

	private void ClearScene()
	{
		GameplayUtils.DestroyAllProjectiles();
		UnsubscribeFromAll();
		secondHand.ClearAll();
		firstHand.ClearAll();
	}

	private void UnsubscribeFromAll()
	{
		BurntFaceHandBehaviour burntFaceHandBehaviour = firstHand;
		burntFaceHandBehaviour.rosary.OnPatternEnded -= OnRosaryPatternEnded;
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnNumberedLaserAttackFinished;
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnLaserAttackFinished;
		burntFaceHandBehaviour = secondHand;
		burntFaceHandBehaviour.rosary.OnPatternEnded -= OnRosaryPatternEnded;
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnNumberedLaserAttackFinished;
		burntFaceHandBehaviour.targetedBeamAttack.OnAttackFinished -= OnLaserAttackFinished;
	}

	private IEnumerator DeathSequenceCoroutine()
	{
		ClearScene();
		StartAttackAction();
		BurntFace.Behaviour.StopBehaviour();
		Core.Logic.CameraManager.ProCamera2DShake.Shake(1.5f, Vector3.down * 2f, 30, 0.2f, 0f);
		firstHand.Hide(4f);
		secondHand.Hide(4f);
		yield return new WaitForSeconds(3f);
		base.transform.DOMoveY(base.transform.position.y - 25f, 7.5f).SetEase(Ease.InOutQuad);
		yield return new WaitForSeconds(5f);
		Core.Logic.Penitent.Status.Invulnerable = false;
	}

	public void Death()
	{
		ActivateColliders(activate: false);
		_fsm.ChangeState(stDeath);
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
		CheckNextPhase();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public bool IsPlayerInRegion(string regionName)
	{
		return awareness.AreaContainsPlayer(regionName);
	}

	public bool IsPlayerInAnyTrapRegion()
	{
		return awareness.AreaContainsPlayer("WALL_W") || awareness.AreaContainsPlayer("WALL_E");
	}

	public void OnDrawGizmos()
	{
		if (_fsm != null)
		{
			if (_fsm.IsInState(stEyes))
			{
				Gizmos.color = Color.yellow;
			}
			else if (_fsm.IsInState(stHead))
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.black;
			}
			Gizmos.DrawWireSphere(base.transform.position, 0.5f);
		}
	}
}
