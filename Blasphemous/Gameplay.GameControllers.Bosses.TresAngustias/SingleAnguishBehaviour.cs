using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Bosses.TresAngustias.AI;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.TrinityMinion.AI;
using Gameplay.GameControllers.Entities;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class SingleAnguishBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct SingleAnguishAttackConfig
	{
		public ANGUISH_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	public enum ANGUISH_ATTACKS
	{
		SPEAR,
		SHIELD,
		MACE
	}

	public enum SINGLE_ANGUISH_STATES
	{
		DANCE,
		GO_TO_DANCE_POINT,
		STOP,
		GO_TO_MERGE_POINT,
		MERGED
	}

	public TresAngustiasMaster master;

	[FoldoutGroup("Dance path", 0)]
	public BezierSpline currentPath;

	[FoldoutGroup("Dance path", 0)]
	public AnimationCurve currentCurve;

	[FoldoutGroup("Dance path", 0)]
	public float secondsToFullLoop;

	[FoldoutGroup("Dance path", 0)]
	public float pathOffset;

	[FoldoutGroup("Boss attacks ref", 0)]
	public BossInstantProjectileAttack spearAttack;

	[FoldoutGroup("Boss attacks ref", 0)]
	public BossSplineFollowingProjectileAttack maceAttack;

	[FoldoutGroup("Boss attacks ref", 0)]
	public BossEnemySpawn shieldAttack;

	[FoldoutGroup("Boss attacks ref", 0)]
	public SplinePointInfo maceSurroundInfo;

	[FoldoutGroup("FX", 0)]
	public GameObject spearStartFx;

	[FoldoutGroup("Boss attacks config ", true, 0)]
	public List<SingleAnguishAttackConfig> attacksConfiguration;

	[FoldoutGroup("Boss attacks config ", true, 0)]
	public AnguishBossfightConfig bossfightConfig;

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	public FloatingWeapon spear;

	public FloatingWeapon mace;

	public FloatingWeapon shield;

	public GhostTrailGenerator ghostTrail;

	public ScrollableModulesManager scrollManager;

	private SingleAnguish SingleAnguish;

	private Transform currentTarget;

	private StateMachine<SingleAnguishBehaviour> _fsm;

	private State<SingleAnguishBehaviour> stDance;

	private State<SingleAnguishBehaviour> stGoToDancePoint;

	private State<SingleAnguishBehaviour> stGoToMergePoint;

	private State<SingleAnguishBehaviour> stGoToComboPoint;

	private State<SingleAnguishBehaviour> stAction;

	private State<SingleAnguishBehaviour> stDeath;

	private State<SingleAnguishBehaviour> stMerged;

	private State<SingleAnguishBehaviour> stIntro;

	private Coroutine currentCoroutine;

	private BossAttackWarning attackWarning;

	private Vector2 _currentTargetPoint;

	private int _multiAttacksRemaining;

	private float _updateCounter;

	private bool followScroll;

	public event Action<SingleAnguishBehaviour> OnActionFinished;

	public void SetScroll(bool s)
	{
		followScroll = s;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		stDance = new SingleAnguishSt_Dance();
		stGoToDancePoint = new SingleAnguishSt_GoToDance();
		stAction = new SingleAnguishSt_Action();
		stGoToMergePoint = new SingleAnguishSt_GoToMergePoint();
		stGoToComboPoint = new SingleAnguishSt_GoToComboPoint();
		stMerged = new SingleAnguishSt_Merged();
		stDeath = new SingleAnguishSt_Death();
		stIntro = new SingleAnguishSt_Intro();
		attackWarning = GetComponentInChildren<BossAttackWarning>();
		scrollManager.OnUpdateHeight += ScrollManager_OnUpdateHeight;
		_fsm = new StateMachine<SingleAnguishBehaviour>(this, stIntro);
	}

	private void ScrollManager_OnUpdateHeight(float delta)
	{
		if (followScroll)
		{
			base.transform.position += Vector3.up * delta;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		SingleAnguish = (SingleAnguish)Entity;
		ChangeBossState(BOSS_STATES.WAITING);
		PoolManager.Instance.CreatePool(spearStartFx, 1);
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

	private void ActionFinished()
	{
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
		if (this.OnActionFinished != null)
		{
			this.OnActionFinished(this);
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
		ActionFinished();
	}

	public void IssueSpearAttack(int n = 1, float delay = 1f)
	{
		_multiAttacksRemaining = n;
		LaunchSpearAttack(delay);
	}

	private void LaunchSpearAttack(float delay)
	{
		spear.ChangeState(FloatingWeapon.FLOATING_WEAPON_STATES.AIMING);
		if (spear.hidden)
		{
			spear.Show(vanishAnimation: true);
		}
		SingleAnguish.Audio.PlayLanceCharge();
		spear.AimToPlayer();
		SetCurrentCoroutine(StartCoroutine(PreparingSpearAttack(delay)));
	}

	private IEnumerator PreparingSpearAttack(float aimTime, float recoveryTime = 0.75f)
	{
		yield return new WaitForSeconds(aimTime);
		spear.ChangeState(FloatingWeapon.FLOATING_WEAPON_STATES.STOP);
		GameObject go = PoolManager.Instance.ReuseObject(spearStartFx, spear.transform.position, Quaternion.identity).GameObject;
		go.transform.SetParent(spear.transform);
		SingleAnguish.Audio.PlayLanceShot();
		go.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(spear.transform.right.y, spear.transform.right.x) * 57.29578f);
		yield return new WaitForSeconds(0.22f);
		attackWarning.ShowWarning(spear.transform.position);
		yield return new WaitForSeconds(0.4f);
		Debug.Log("SPEAR ATTACK");
		spear.Hide(vanishAnimation: true);
		spearAttack.Shoot(spear.transform.position, spear.transform.right);
		base.transform.DOPunchPosition(-spear.transform.right * 0.5f, 0.8f, 2);
		yield return new WaitForSeconds(recoveryTime);
		AfterSpearAttack(aimTime);
	}

	private void AfterSpearAttack(float delay)
	{
		_multiAttacksRemaining--;
		if (_multiAttacksRemaining > 0)
		{
			LaunchSpearAttack(delay);
			return;
		}
		spear.ChangeState(FloatingWeapon.FLOATING_WEAPON_STATES.FLOATING);
		spear.Show(vanishAnimation: true);
		ActionFinished();
	}

	public void IssueMaceAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingPathThrow()));
	}

	private IEnumerator PreparingPathThrow()
	{
		attackWarning.ShowWarning(base.transform.position);
		SingleAnguishAttackConfig config = attacksConfiguration.Find((SingleAnguishAttackConfig x) => x.attackType == ANGUISH_ATTACKS.MACE);
		SplinePointInfo info = bossfightConfig.GetMaceSplineInfo();
		BezierSpline spline = info.spline;
		Vector2 launchDir = spline.GetPoint(1f) - spline.GetPoint(0f);
		mace.transform.DOPunchPosition(launchDir.normalized, config.preparationSeconds, 1).SetEase(Ease.InCubic);
		yield return new WaitForSeconds(config.preparationSeconds);
		mace.Hide();
		maceAttack.Shoot(spline, info.speedCurve, info.time, base.transform.position);
		SingleAnguish.Audio.PlayMace();
		maceAttack.OnPathFinished += OnProjectilePathFinished;
	}

	private void OnProjectilePathFinished(BossSplineFollowingProjectileAttack obj)
	{
		SingleAnguish.Audio.StopMace();
		obj.OnPathFinished -= OnProjectilePathFinished;
		mace.transform.position = obj.lastPosition;
		mace.Show();
		ActionFinished();
	}

	public void IssueMaceSurroundAttack()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(PreparingSurround()));
	}

	private IEnumerator PreparingSurround()
	{
		attackWarning.ShowWarning(base.transform.position);
		yield return new WaitForSeconds(attacksConfiguration.Find((SingleAnguishAttackConfig x) => x.attackType == ANGUISH_ATTACKS.MACE).preparationSeconds);
		SplinePointInfo info = maceSurroundInfo;
		BezierSpline spline = info.spline;
		mace.Hide();
		maceAttack.Shoot(spline, info.speedCurve, info.time, base.transform.position);
		SingleAnguish.Audio.PlayMace();
		maceAttack.OnPathFinished += OnProjectilePathFinished;
	}

	public void IssueShieldAttack()
	{
		StartCoroutine(ShieldSummons());
	}

	private IEnumerator ShieldSummons()
	{
		int j = 3;
		for (int i = 0; i < j; i++)
		{
			Enemy e = shieldAttack.Spawn(base.transform.position, base.transform.right, 1f, OnSpawnFinished);
			SingleAnguish.Audio.PlaySpawn();
			TrinityMinionBehaviour tmb = e.GetComponent<TrinityMinionBehaviour>();
			currentTarget = GetTarget();
			tmb.SetTarget(currentTarget);
			yield return new WaitForSeconds(0.5f);
		}
		OnSpawnFinished();
	}

	private void OnSpawnFinished()
	{
		ActionFinished();
	}

	public void IssueMerge(Vector2 point)
	{
		_fsm.ChangeState(stGoToMergePoint);
		_currentTargetPoint = point;
	}

	public void StopDancing()
	{
		Debug.Log("STOP DANCING");
		_fsm.ChangeState(stAction);
	}

	public void BackToDance()
	{
		Debug.Log("BACK TO DANCE");
		_fsm.ChangeState(stGoToDancePoint);
	}

	public void BackToAction()
	{
		_fsm.ChangeState(stAction);
	}

	public void SetPath(BezierSpline s)
	{
		currentPath = s;
		Debug.Log("CURRENT PATH CHANGED");
	}

	public Vector3 GetNextPathPoint()
	{
		float t = currentCurve.Evaluate((_updateCounter + pathOffset) % secondsToFullLoop / secondsToFullLoop);
		return currentPath.GetPoint(t);
	}

	public void ForceWait(float s)
	{
		StartWaitingPeriod(s);
	}

	public void IssueCombo(Vector2 point)
	{
		StartAttackAction();
		_fsm.ChangeState(stGoToComboPoint);
		_currentTargetPoint = point;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
		if (!_fsm.IsInState(stIntro))
		{
			_updateCounter += Time.deltaTime;
		}
	}

	public void UpdateDanceState()
	{
		base.transform.position = GetNextPathPoint();
	}

	public void UpdateGoToDancePointState()
	{
		_currentTargetPoint = GetNextPathPoint();
		SingleAnguish.arriveBehaviour.target = _currentTargetPoint;
	}

	public void UpdateGoToTargetPoint()
	{
		SingleAnguish.arriveBehaviour.target = _currentTargetPoint;
	}

	public void StartIntro()
	{
		StartAttackAction();
		SingleAnguish.Audio.PlayAppear();
		SingleAnguish.SpriteRenderer.DOFade(1f, 3f).OnComplete(delegate
		{
			ActionFinished();
		});
	}

	public void NotifyMaster()
	{
		if (this.OnActionFinished != null)
		{
			this.OnActionFinished(this);
		}
	}

	public void ActivateWeapon(bool activate)
	{
		if (spear != null)
		{
			spear.Activate(activate, vanishAnimation: true);
		}
		if (shield != null)
		{
			shield.Activate(activate, vanishAnimation: true);
		}
		if (mace != null)
		{
			mace.Activate(activate, vanishAnimation: true);
		}
	}

	public bool IsCloseToTargetPoint(float closeRange = 0.5f)
	{
		float num = Vector2.Distance(base.transform.position, _currentTargetPoint);
		return num < closeRange;
	}

	public void ActivateSteering(bool enabled)
	{
		SingleAnguish.autonomousAgent.enabled = enabled;
		if (enabled)
		{
			SingleAnguish.autonomousAgent.currentVelocity = Vector3.zero;
		}
	}

	public void ActivateGhostMode(bool enabled)
	{
		ghostTrail.EnableGhostTrail = enabled;
	}

	public void ActivateCollider(bool enabled)
	{
		SingleAnguish.DamageArea.DamageAreaCollider.enabled = enabled;
	}

	public void ActivateSprite(bool activate)
	{
		SingleAnguish.SpriteRenderer.enabled = activate;
		SingleAnguish.DamageArea.DamageAreaCollider.enabled = activate;
	}

	public void ChangeToMerged()
	{
		_fsm.ChangeState(stMerged);
	}

	public void ChangeToDance()
	{
		_fsm.ChangeState(stDance);
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (Entity.Status.Dead)
		{
			return;
		}
		if (Entity.transform.position.x >= targetPos.x + 1f)
		{
			if (Entity.Status.Orientation != EntityOrientation.Left)
			{
				if (OnTurning != null)
				{
					OnTurning();
				}
				Entity.SetOrientation(EntityOrientation.Left);
			}
		}
		else if (Entity.transform.position.x < targetPos.x - 1f && Entity.Status.Orientation != 0)
		{
			if (OnTurning != null)
			{
				OnTurning();
			}
			Entity.SetOrientation(EntityOrientation.Right);
		}
	}

	public void Death()
	{
		SingleAnguish.AnimatorInyector.Death();
		ActivateSteering(enabled: false);
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
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}
}
