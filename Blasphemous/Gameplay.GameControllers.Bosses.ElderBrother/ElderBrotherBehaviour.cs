using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.ElderBrother;

public class ElderBrotherBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct ElderBrotherAttackConfig
	{
		public ELDER_BROTHER_ATTACKS attackType;

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

	public enum ELDER_BROTHER_ATTACKS
	{
		JUMP = 1,
		AREA
	}

	public Transform introJumpPoint;

	[FoldoutGroup("Attacks", true, 0)]
	public BossJumpAttack jumpAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack areaAttack;

	[FoldoutGroup("Attacks", true, 0)]
	public BossAreaSummonAttack maceImpact;

	[FoldoutGroup("Attacks", true, 0)]
	public List<ElderBrotherAttackConfig> attacksConfiguration;

	[FoldoutGroup("Traits", true, 0)]
	public EntityMotionChecker motionChecker;

	public GameObject corpseFxPrefab;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public ELDER_BROTHER_ATTACKS lastAttack;

	public Vector2 jumpOffset;

	public float minExtraWaitTime;

	public float maxExtraWaitTime;

	public bool canRepeatAttack;

	private Transform currentTarget;

	private List<ELDER_BROTHER_ATTACKS> currentlyAvailableAttacks;

	private List<ELDER_BROTHER_ATTACKS> queuedActions;

	private Coroutine currentCoroutine;

	public ElderBrother ElderBrother { get; private set; }

	public bool Awaken { get; private set; }

	private List<ELDER_BROTHER_ATTACKS> GetCurrentStateAttacks()
	{
		List<ELDER_BROTHER_ATTACKS> list = new List<ELDER_BROTHER_ATTACKS>();
		list.Add(ELDER_BROTHER_ATTACKS.AREA);
		list.Add(ELDER_BROTHER_ATTACKS.JUMP);
		return list;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		PoolManager.Instance.CreatePool(corpseFxPrefab, 30);
		ElderBrother = (ElderBrother)Entity;
		currentlyAvailableAttacks = GetCurrentStateAttacks();
	}

	public override void OnStart()
	{
		base.OnStart();
		ChangeBossState(BOSS_STATES.WAITING);
	}

	private void Update()
	{
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

	public ELDER_BROTHER_ATTACKS GetNewAttack()
	{
		ELDER_BROTHER_ATTACKS[] array = new ELDER_BROTHER_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<ELDER_BROTHER_ATTACKS> list = new List<ELDER_BROTHER_ATTACKS>(array);
		if (!canRepeatAttack)
		{
			list.Remove(lastAttack);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public void LaunchRandomAction()
	{
		LaunchAction(GetNewAttack());
	}

	private void QueuedActionsPush(ELDER_BROTHER_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<ELDER_BROTHER_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private ELDER_BROTHER_ATTACKS QueuedActionsPop()
	{
		ELDER_BROTHER_ATTACKS eLDER_BROTHER_ATTACKS = queuedActions[0];
		queuedActions.Remove(eLDER_BROTHER_ATTACKS);
		return eLDER_BROTHER_ATTACKS;
	}

	public void LaunchAction(ELDER_BROTHER_ATTACKS atk, bool checkReposition = true)
	{
		lastAttack = atk;
		switch (atk)
		{
		case ELDER_BROTHER_ATTACKS.JUMP:
			JumpAttack();
			break;
		case ELDER_BROTHER_ATTACKS.AREA:
			AreaAttack();
			break;
		default:
			throw new ArgumentOutOfRangeException("atk", atk, null);
		}
	}

	private ElderBrotherAttackConfig GetAttackConfig(ELDER_BROTHER_ATTACKS atk)
	{
		return attacksConfiguration.Find((ElderBrotherAttackConfig x) => x.attackType == atk);
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public void IntroJump()
	{
		StartCoroutine(JumpIntroCoroutine());
	}

	private IEnumerator JumpIntroCoroutine()
	{
		float jumpPreparationDuration = 0.65f;
		float jumpDuration = jumpAttack.moveSeconds;
		LookAtTarget(GetTarget().position);
		ElderBrother.AnimatorInyector.SetMidAir(midAir: true);
		yield return new WaitForSeconds(jumpPreparationDuration);
		jumpAttack.Use(base.transform, introJumpPoint.position);
		yield return new WaitForSeconds(jumpDuration * 0.9f);
		ElderBrother.AnimatorInyector.SetMidAir(midAir: false);
		yield return new WaitForSeconds(1f);
		base.BehaviourTree.StartBehaviour();
		StartWaitingPeriod(1f);
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

	private Vector3 GetTargetPredictedPos()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		Vector2 vector = penitent.transform.position;
		vector.x += penitent.GetVelocity().x;
		return vector;
	}

	public void JumpAttack()
	{
		StartAttackAction();
		currentTarget = GetTarget();
		jumpAttack.OnJumpLanded += OnJumpLanded;
		Debug.Log("ElderBrother: JUMP OUT");
		SetCurrentCoroutine(StartCoroutine(JumpAttackCoroutine()));
	}

	private IEnumerator JumpAttackCoroutine()
	{
		float jumpPreparationDuration = 0.65f;
		float jumpDuration = jumpAttack.moveSeconds;
		ElderBrother.AnimatorInyector.SetMidAir(midAir: true);
		yield return new WaitForSeconds(jumpPreparationDuration);
		Vector3 predictedPosition = GetTargetPredictedPos() + (Vector3)jumpOffset;
		jumpAttack.Use(base.transform, predictedPosition);
		yield return new WaitForSeconds(jumpDuration * 0.9f);
		ElderBrother.AnimatorInyector.SetMidAir(midAir: false);
		LookAtTarget(GetTarget().position);
	}

	private void OnJumpLanded()
	{
		jumpAttack.OnJumpLanded -= OnJumpLanded;
		ElderBrotherAttackConfig attackConfig = GetAttackConfig(ELDER_BROTHER_ATTACKS.JUMP);
		float extraWaitTime = GetExtraWaitTime();
		StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack + extraWaitTime);
	}

	public void AreaAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingChargeAttack()));
	}

	private IEnumerator PreparingChargeAttack()
	{
		currentTarget = GetTarget();
		LookAtTarget(currentTarget.position);
		Vector2 dir = ((ElderBrother.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		ElderBrother.AnimatorInyector.BigSmashPreparation();
		yield return new WaitForSeconds(attacksConfiguration.Find((ElderBrotherAttackConfig x) => x.attackType == ELDER_BROTHER_ATTACKS.AREA).preparationSeconds);
		Debug.Log("ElderBrother: DASH ATTACK");
		ElderBrother.AnimatorInyector.Smash();
		areaAttack.SummonAreas(Vector3.right * Mathf.Sign(dir.x));
		maceImpact.SummonAreas(Vector3.right * Mathf.Sign(dir.x));
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position + Vector3.right * Mathf.Sign(dir.x) * 1.5f, 0.4f, 0.3f, 1.4f);
		OnAreaAttackFinished();
	}

	private void OnAreaAttackFinished()
	{
		ElderBrotherAttackConfig attackConfig = GetAttackConfig(ELDER_BROTHER_ATTACKS.AREA);
		float extraWaitTime = GetExtraWaitTime();
		StartWaitingPeriod(attackConfig.waitingSecondsAfterAttack + extraWaitTime);
	}

	private float GetExtraWaitTime()
	{
		return UnityEngine.Random.Range(minExtraWaitTime, maxExtraWaitTime);
	}

	private void UnsubscribeFromEverything()
	{
		Debug.Log("UNSUBSCRIBING FROM EVERY EVENT");
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > ElderBrother.transform.position.x)
		{
			if (ElderBrother.Status.Orientation != 0)
			{
				ElderBrother.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (ElderBrother.Status.Orientation != EntityOrientation.Left)
		{
			ElderBrother.SetOrientation(EntityOrientation.Left);
		}
	}

	public void Death()
	{
		StopAllCoroutines();
		UnsubscribeFromEverything();
		StopMovement();
		jumpAttack.StopJump();
		base.BehaviourTree.StopBehaviour();
		ElderBrother.AnimatorInyector.Death();
	}

	public override void StopMovement()
	{
	}

	public override void Idle()
	{
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
	}
}
