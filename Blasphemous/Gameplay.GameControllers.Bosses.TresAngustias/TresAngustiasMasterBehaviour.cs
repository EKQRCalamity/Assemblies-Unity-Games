using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class TresAngustiasMasterBehaviour : EnemyBehaviour
{
	public BezierSpline currentPath;

	public AnimationCurve currentCurve;

	public TresAngustiasMaster TresAngustias;

	public float secondsToFullLoop;

	private Vector3 _pathOrigin;

	private float _updateCounter;

	private bool followPath = true;

	public SingleAnguish singleAnguishLance;

	public SingleAnguish singleAnguishShield;

	public SingleAnguish singleAnguishMace;

	public ScrollableModulesManager scrollManager;

	public GameObject explosionPrefab;

	public BossAreaSummonAttack bossAreaAttack;

	public Coroutine _currentCoroutine;

	public List<Transform> mergePointMarkers;

	public BOSS_STATES currentState = BOSS_STATES.AVAILABLE_FOR_ACTION;

	public MASTER_ANGUISH_STATES currentMasterAnguishState;

	public List<MasterAnguishAttackConfig> attacksConfig;

	private int _mergeCounter;

	private int _repositionCounter;

	private List<MASTER_ANGUISH_ATTACKS> _currentlyAvailableAttacks;

	private MASTER_ANGUISH_ATTACKS _lastAttack;

	private Transform _currentBeamTransform;

	private float beamOffsetY = 4.5f;

	public override void OnAwake()
	{
		base.OnAwake();
		PoolManager.Instance.CreatePool(explosionPrefab, 12);
		_pathOrigin = currentPath.transform.localPosition;
		StartWaitingPeriod(3f);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (followPath)
		{
			FollowPathUpdate();
		}
	}

	private void LateUpdate()
	{
		currentPath.transform.localPosition = _pathOrigin;
	}

	public MASTER_ANGUISH_STATES GetAnguishState()
	{
		return currentMasterAnguishState;
	}

	private void SetCurrentCoroutine(Coroutine c)
	{
		if (_currentCoroutine != null)
		{
			Debug.Log(">>>>STOPPING CURRENT COROUTINE");
			StopCoroutine(_currentCoroutine);
		}
		Debug.Log(">>NEW COROUTINE");
		_currentCoroutine = c;
	}

	public void SetPath(BezierSpline s)
	{
		currentPath = s;
		Debug.Log("CURRENT PATH CHANGED");
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
		_currentlyAvailableAttacks = GetCurrentStateAttacks();
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
		_currentlyAvailableAttacks = GetCurrentStateAttacks();
	}

	public void LaunchIntro()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(IntroCoroutine()));
	}

	private IEnumerator IntroCoroutine()
	{
		singleAnguishLance.Behaviour.StartIntro();
		yield return new WaitForSeconds(1.5f);
		TresAngustias.bossfightPoints.ShowFlameWall();
		TresAngustias.Invencible = false;
		scrollManager.ActivateDeathCollider();
		singleAnguishMace.Behaviour.StartIntro();
		yield return new WaitForSeconds(1f);
		singleAnguishShield.Behaviour.StartIntro();
		StartCoroutine(WaitAllFreeAndCallback(OnIntroFinished));
	}

	private void OnIntroFinished()
	{
		singleAnguishShield.Behaviour.BackToDance();
		singleAnguishLance.Behaviour.BackToDance();
		singleAnguishMace.Behaviour.BackToDance();
		scrollManager.scrollActive = true;
		scrollManager.ActivateDeathCollider();
		StartWaitingPeriod(2f);
	}

	private List<MASTER_ANGUISH_ATTACKS> GetCurrentStateAttacks()
	{
		float healthPercentage = GetHealthPercentage();
		List<MASTER_ANGUISH_ATTACKS> list2;
		if (currentMasterAnguishState == MASTER_ANGUISH_STATES.DIVIDED)
		{
			List<MASTER_ANGUISH_ATTACKS> list = new List<MASTER_ANGUISH_ATTACKS>();
			list.Add(MASTER_ANGUISH_ATTACKS.SPEAR);
			list.Add(MASTER_ANGUISH_ATTACKS.MACE);
			list.Add(MASTER_ANGUISH_ATTACKS.MACE_AROUND);
			list.Add(MASTER_ANGUISH_ATTACKS.SHIELD);
			list.Add(MASTER_ANGUISH_ATTACKS.MERGE);
			list.Add(MASTER_ANGUISH_ATTACKS.COMBO2);
			list2 = list;
			if (healthPercentage < 0.6f)
			{
				list2.Add(MASTER_ANGUISH_ATTACKS.COMBO1);
			}
		}
		else if (currentMasterAnguishState == MASTER_ANGUISH_STATES.MERGED)
		{
			List<MASTER_ANGUISH_ATTACKS> list = new List<MASTER_ANGUISH_ATTACKS>();
			list.Add(MASTER_ANGUISH_ATTACKS.AREA);
			list.Add(MASTER_ANGUISH_ATTACKS.DIVIDE);
			list2 = list;
		}
		else
		{
			List<MASTER_ANGUISH_ATTACKS> list = new List<MASTER_ANGUISH_ATTACKS>();
			list.Add(MASTER_ANGUISH_ATTACKS.AREA);
			list.Add(MASTER_ANGUISH_ATTACKS.MULTIAREA);
			list2 = list;
		}
		return list2;
	}

	public void LaunchRandomAction()
	{
		SetCurrentCoroutine(StartCoroutine(LaunchAction(GetNewAttack())));
	}

	public MASTER_ANGUISH_ATTACKS GetNewAttack()
	{
		MASTER_ANGUISH_ATTACKS[] array = new MASTER_ANGUISH_ATTACKS[_currentlyAvailableAttacks.Count];
		_currentlyAvailableAttacks.CopyTo(array);
		List<MASTER_ANGUISH_ATTACKS> list = new List<MASTER_ANGUISH_ATTACKS>(array);
		list.Remove(_lastAttack);
		if (_lastAttack == MASTER_ANGUISH_ATTACKS.MERGE)
		{
			list.Remove(MASTER_ANGUISH_ATTACKS.DIVIDE);
		}
		else if (_lastAttack == MASTER_ANGUISH_ATTACKS.DIVIDE)
		{
			list.Remove(MASTER_ANGUISH_ATTACKS.MERGE);
		}
		if (UnityEngine.Random.Range(0f, 1f) > 0.6f)
		{
			list.Remove(MASTER_ANGUISH_ATTACKS.MERGE);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	private MasterAnguishAttackConfig GetAttackConfig(MASTER_ANGUISH_ATTACKS atk)
	{
		return attacksConfig.Find((MasterAnguishAttackConfig x) => x.atk == atk);
	}

	public void LaunchActionFromBehaviourTree(MASTER_ANGUISH_ATTACKS atk)
	{
		SetCurrentCoroutine(StartCoroutine(LaunchAction(atk)));
	}

	private IEnumerator LaunchAction(MASTER_ANGUISH_ATTACKS atk)
	{
		StartAttackAction();
		_lastAttack = atk;
		MasterAnguishAttackConfig currentConfig = GetAttackConfig(atk);
		yield return new WaitForSeconds(currentConfig.preparationSeconds);
		switch (atk)
		{
		case MASTER_ANGUISH_ATTACKS.SPEAR:
		{
			Debug.Log("LAUNCHING SPEAR ACTION");
			int nSpears = GetSpearNumber();
			singleAnguishLance.Behaviour.StopDancing();
			singleAnguishLance.Behaviour.OnActionFinished += OnSingleAttackFinished;
			singleAnguishLance.Behaviour.IssueSpearAttack(nSpears);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.MACE:
			singleAnguishMace.Behaviour.StopDancing();
			singleAnguishMace.Behaviour.OnActionFinished += OnSingleAttackFinished;
			singleAnguishMace.Behaviour.IssueMaceAttack();
			break;
		case MASTER_ANGUISH_ATTACKS.MACE_AROUND:
			singleAnguishMace.Behaviour.StopDancing();
			singleAnguishMace.Behaviour.OnActionFinished += OnSingleAttackFinished;
			singleAnguishMace.Behaviour.IssueMaceSurroundAttack();
			break;
		case MASTER_ANGUISH_ATTACKS.SHIELD:
		{
			int nSpears = GetSpearNumber();
			singleAnguishShield.Behaviour.StopDancing();
			singleAnguishShield.Behaviour.OnActionFinished += OnSingleAttackFinished;
			singleAnguishShield.Behaviour.IssueSpearAttack(nSpears);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.MERGE:
		{
			followPath = false;
			_currentBeamTransform = TresAngustias.bossfightPoints.GetRandomBeamPoint();
			base.transform.position = _currentBeamTransform.position;
			Vector2 p2 = GetMergePoint(0);
			Vector2 p4 = GetMergePoint(1);
			Vector2 p6 = GetMergePoint(2);
			_mergeCounter = 3;
			singleAnguishLance.Behaviour.OnActionFinished += OnMergeRepositionFinished;
			singleAnguishLance.Behaviour.IssueMerge(p2);
			singleAnguishMace.Behaviour.OnActionFinished += OnMergeRepositionFinished;
			singleAnguishMace.Behaviour.IssueMerge(p4);
			singleAnguishShield.Behaviour.OnActionFinished += OnMergeRepositionFinished;
			singleAnguishShield.Behaviour.IssueMerge(p6);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.DIVIDE:
			TresAngustias.AnimatorInyector.Divide();
			TresAngustias.Audio.PlayDivide();
			yield return new WaitForSeconds(1f);
			DivideIntoThree(delegate
			{
				StartWaitingPeriod(currentConfig.preparationSeconds);
			});
			singleAnguishLance.Behaviour.BackToDance();
			singleAnguishMace.Behaviour.BackToDance();
			singleAnguishShield.Behaviour.BackToDance();
			followPath = true;
			break;
		case MASTER_ANGUISH_ATTACKS.AREA:
		{
			GameObject area = bossAreaAttack.SummonAreaOnPoint(_currentBeamTransform.position - Vector3.up * beamOffsetY);
			area.transform.parent = TresAngustias.bossfightPoints.transform;
			StartWaitingPeriod(currentConfig.recoverySeconds);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.COMBO1:
			_repositionCounter = 3;
			singleAnguishLance.Behaviour.OnActionFinished += OnComboReposition;
			singleAnguishLance.Behaviour.IssueCombo(TresAngustias.bossfightPoints.beamPoints[1].position + Vector3.up * 4f);
			yield return new WaitForSeconds(0.4f);
			singleAnguishMace.Behaviour.OnActionFinished += OnComboReposition;
			singleAnguishMace.Behaviour.IssueCombo(TresAngustias.bossfightPoints.beamPoints[0].position + Vector3.up * 4f);
			yield return new WaitForSeconds(0.4f);
			singleAnguishShield.Behaviour.OnActionFinished += OnComboReposition;
			singleAnguishShield.Behaviour.IssueCombo(TresAngustias.bossfightPoints.beamPoints[2].position + Vector3.up * 4f);
			break;
		case MASTER_ANGUISH_ATTACKS.COMBO2:
		{
			_repositionCounter = 3;
			List<Transform> comboPoints = TresAngustias.bossfightPoints.GetSpearPoints();
			singleAnguishLance.Behaviour.OnActionFinished += OnSpearComboReposition;
			singleAnguishLance.Behaviour.IssueCombo(comboPoints[0].position);
			yield return new WaitForSeconds(0.5f);
			singleAnguishMace.Behaviour.OnActionFinished += OnSpearComboReposition;
			singleAnguishMace.Behaviour.IssueCombo(comboPoints[1].position);
			yield return new WaitForSeconds(0.5f);
			singleAnguishShield.Behaviour.OnActionFinished += OnSpearComboReposition;
			singleAnguishShield.Behaviour.IssueCombo(comboPoints[2].position);
			yield return new WaitForSeconds(0.5f);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.MULTIAREA:
		{
			Transform newPoint = TresAngustias.bossfightPoints.GetDifferentBeamTransform(_currentBeamTransform);
			Vector2 pToTeleport = newPoint.position;
			GameObject firstArea = bossAreaAttack.SummonAreaOnPoint(_currentBeamTransform.position - Vector3.up * beamOffsetY);
			GameObject secondArea = bossAreaAttack.SummonAreaOnPoint(pToTeleport);
			secondArea.transform.position = new Vector3(secondArea.transform.position.x, firstArea.transform.position.y, 0f);
			firstArea.transform.parent = TresAngustias.bossfightPoints.transform;
			secondArea.transform.parent = TresAngustias.bossfightPoints.transform;
			yield return new WaitForSeconds(2f);
			base.transform.position = pToTeleport;
			_currentBeamTransform = newPoint;
			StartWaitingPeriod(currentConfig.recoverySeconds);
			break;
		}
		case MASTER_ANGUISH_ATTACKS.HORIZONTALAREA:
		{
			Vector2 p2 = TresAngustias.bossfightPoints.beamPoints[0].position;
			Vector2 p4 = TresAngustias.bossfightPoints.beamPoints[1].position;
			Vector2 p6 = TresAngustias.bossfightPoints.beamPoints[2].position;
			bossAreaAttack.SummonAreaOnPoint(p4 - Vector2.up);
			yield return new WaitForSeconds(2.5f);
			bossAreaAttack.SummonAreaOnPoint(p2 - Vector2.up - Vector2.right * 2f, -90f);
			yield return new WaitForSeconds(2.5f);
			bossAreaAttack.SummonAreaOnPoint(p6 - Vector2.up + Vector2.right * 2f, 90f);
			break;
		}
		}
	}

	private void OnSingleAttackFinished(SingleAnguishBehaviour obj)
	{
		Debug.Log("SINGLE ATTACK FINISHED: " + _lastAttack);
		obj.OnActionFinished -= OnSingleAttackFinished;
		obj.BackToDance();
		StartWaitingPeriod(GetWaitingPeriodFromHP());
	}

	private void OnComboReposition(SingleAnguishBehaviour obj)
	{
		obj.OnActionFinished -= OnComboReposition;
		_repositionCounter--;
		if (_repositionCounter == 0)
		{
			OnAllSingleAnguishInComboPosition();
		}
	}

	private void OnSpearComboReposition(SingleAnguishBehaviour obj)
	{
		obj.OnActionFinished -= OnSpearComboReposition;
		_repositionCounter--;
		if (_repositionCounter == 0)
		{
			OnAllSingleAnguishInCombo2Position();
		}
	}

	private void OnAllSingleAnguishInCombo2Position()
	{
		singleAnguishShield.Behaviour.ChangeToAction();
		singleAnguishLance.Behaviour.ChangeToAction();
		singleAnguishMace.Behaviour.ChangeToAction();
		SetCurrentCoroutine(StartCoroutine(Combo2Coroutine()));
	}

	private void OnAllSingleAnguishInComboPosition()
	{
		singleAnguishShield.Behaviour.ChangeToAction();
		singleAnguishLance.Behaviour.ChangeToAction();
		singleAnguishMace.Behaviour.ChangeToAction();
		SetCurrentCoroutine(StartCoroutine(Combo1Coroutine()));
	}

	private IEnumerator Combo1Coroutine()
	{
		singleAnguishShield.Behaviour.IssueSpearAttack(2);
		yield return new WaitForSeconds(1.5f);
		singleAnguishLance.Behaviour.IssueSpearAttack(2);
		yield return new WaitForSeconds(1.5f);
		singleAnguishMace.Behaviour.IssueMaceAttack();
		StartCoroutine(WaitAllFreeAndCallback(OnAllComboAttacks));
	}

	private IEnumerator Combo2Coroutine()
	{
		singleAnguishMace.Behaviour.IssueMaceSurroundAttack();
		yield return new WaitForSeconds(0.5f);
		singleAnguishShield.Behaviour.IssueSpearAttack(2, 1.25f);
		yield return new WaitForSeconds(1.5f);
		singleAnguishLance.Behaviour.IssueSpearAttack(2, 1.25f);
		StartCoroutine(WaitAllFreeAndCallback(OnAllComboAttacks));
	}

	private IEnumerator WaitAllFreeAndCallback(Action callback)
	{
		while (IsEverySingleAnguishBusy())
		{
			yield return null;
		}
		callback();
	}

	private bool IsEverySingleAnguishBusy()
	{
		return singleAnguishLance.Behaviour.currentState != BOSS_STATES.AVAILABLE_FOR_ACTION || singleAnguishMace.Behaviour.currentState != BOSS_STATES.AVAILABLE_FOR_ACTION || singleAnguishShield.Behaviour.currentState != BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	private void OnAllComboAttacks()
	{
		singleAnguishShield.Behaviour.BackToDance();
		singleAnguishLance.Behaviour.BackToDance();
		singleAnguishMace.Behaviour.BackToDance();
		StartWaitingPeriod(3f);
	}

	private void DivideIntoThree(Action callback = null)
	{
		RevealBody(reveal: false);
		Vector2 mergePoint = GetMergePoint(0);
		Vector2 mergePoint2 = GetMergePoint(1);
		Vector2 mergePoint3 = GetMergePoint(2);
		singleAnguishLance.transform.position = mergePoint;
		singleAnguishMace.transform.position = mergePoint2;
		singleAnguishShield.transform.position = mergePoint3;
		currentMasterAnguishState = MASTER_ANGUISH_STATES.DIVIDED;
		callback?.Invoke();
	}

	public float GetHealthPercentage()
	{
		return TresAngustias.CurrentLife / TresAngustias.Stats.Life.Base;
	}

	private int GetSpearNumber()
	{
		float healthPercentage = GetHealthPercentage();
		if (healthPercentage > 0.75f)
		{
			return 1;
		}
		if (healthPercentage > 0.5f)
		{
			return 2;
		}
		if (healthPercentage > 0.3f)
		{
			return 3;
		}
		return 4;
	}

	private float GetWaitingPeriodFromHP()
	{
		float healthPercentage = GetHealthPercentage();
		float a = 0.5f;
		float b = 3f;
		return Mathf.Lerp(a, b, healthPercentage);
	}

	private void OnMergeRepositionFinished(SingleAnguishBehaviour obj)
	{
		obj.OnActionFinished -= OnMergeRepositionFinished;
		_mergeCounter--;
		if (_mergeCounter == 0)
		{
			OnAllSingleAnguishInMergePosition();
		}
	}

	private void OnAllSingleAnguishInMergePosition()
	{
		if (!TresAngustias.Status.Dead)
		{
			singleAnguishShield.Behaviour.ChangeToMerged();
			singleAnguishLance.Behaviour.ChangeToMerged();
			singleAnguishMace.Behaviour.ChangeToMerged();
			TresAngustias.Audio.PlayMerge();
			TresAngustias.AnimatorInyector.Merge();
			RevealBody(reveal: true);
			TriggerTraps();
			StartCoroutine(AfterMerge());
		}
	}

	private void RevealBody(bool reveal)
	{
		TresAngustias.SpriteRenderer.enabled = reveal;
		TresAngustias.DamageArea.DamageAreaCollider.enabled = reveal;
	}

	private void TriggerTraps()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		TriggerBasedTrap[] array = UnityEngine.Object.FindObjectsOfType<TriggerBasedTrap>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Use();
		}
	}

	private IEnumerator AfterMerge()
	{
		MasterAnguishAttackConfig currentConfig = GetAttackConfig(MASTER_ANGUISH_ATTACKS.MERGE);
		yield return new WaitForSeconds(currentConfig.recoverySeconds);
		if (IsHPUnderFinalThreshold())
		{
			currentMasterAnguishState = MASTER_ANGUISH_STATES.FINAL;
		}
		else
		{
			currentMasterAnguishState = MASTER_ANGUISH_STATES.MERGED;
		}
		ActionFinished();
	}

	private Vector2 GetMergePoint(int index)
	{
		return mergePointMarkers[index].transform.position;
	}

	private void FollowPathUpdate()
	{
		float t = currentCurve.Evaluate(_updateCounter / secondsToFullLoop);
		Vector3 point = currentPath.GetPoint(t);
		base.transform.position = point;
		_updateCounter += Time.deltaTime;
		_updateCounter %= secondsToFullLoop;
	}

	public bool IsHPUnderFinalThreshold()
	{
		return TresAngustias.CurrentLife < TresAngustias.Stats.Life.MaxValue / 4f;
	}

	private void ClearAll()
	{
		bossAreaAttack.ClearAll();
	}

	public void Death()
	{
		ClearAll();
		TresAngustias.Audio.PlayPreDeath();
		scrollManager.scrollActive = false;
		StopAllCoroutines();
		StartAttackAction();
		followPath = false;
		if (currentMasterAnguishState == MASTER_ANGUISH_STATES.DIVIDED || currentMasterAnguishState == MASTER_ANGUISH_STATES.MERGED)
		{
			StartDividedDeathSequence();
			return;
		}
		Vector3 position = TresAngustias.bossfightPoints.beamPoints[1].position;
		base.transform.DOMove(position, 3f).SetEase(Ease.InOutCubic);
		Debug.Log("DEAD; STARTING SEQUENCE");
		StartDeathSequence();
	}

	private void StartDividedDeathSequence()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		TresAngustias.AnimatorInyector.Disappear();
		SetCurrentCoroutine(StartCoroutine(DeathSequenceDivided()));
		TresAngustias.bossfightPoints.HideFlameWall();
		Core.Logic.Penitent.Status.Invulnerable = true;
		Core.Logic.CameraManager.ProCamera2DShake.Shake(5f, Vector2.up * 2f, 140, 0.25f, 0f, default(Vector3), 0.06f, ignoreTimeScale: true);
		Core.Input.SetBlocker("CINEMATIC", blocking: true);
	}

	private IEnumerator DeathSequenceDivided()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		yield return new WaitForSeconds(0.1f);
		singleAnguishLance.Behaviour.ForceWait(0.2f);
		singleAnguishMace.Behaviour.ForceWait(0.2f);
		singleAnguishShield.Behaviour.ForceWait(0.2f);
		StartCoroutine(WaitAllFreeAndCallback(OnAllOnDeathPositionDivided));
	}

	private void OnAllOnDeathPositionDivided()
	{
		StartCoroutine(DelayedDeaths(0.5f));
	}

	private void StartDeathSequence()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		StartCoroutine(RandomExplosions(3f, 24, base.transform, 1.5f, explosionPrefab, TresAngustias.DamageFlash, AfterExplosions));
		TresAngustias.bossfightPoints.HideFlameWall();
		Core.Logic.Penitent.Status.Invulnerable = true;
		Core.Logic.CameraManager.ProCamera2DShake.Shake(5f, Vector2.up * 2f, 140, 0.25f, 0f, default(Vector3), 0.06f, ignoreTimeScale: true);
		Core.Input.SetBlocker("CINEMATIC", blocking: true);
	}

	private void AfterExplosions()
	{
		SetCurrentCoroutine(StartCoroutine(DeathSequence()));
	}

	private IEnumerator DeathSequence()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		yield return new WaitForSeconds(1f);
		TresAngustias.AnimatorInyector.Divide();
		TresAngustias.Audio.PlayDivide();
		yield return new WaitForSeconds(0.9f);
		DivideIntoThree();
		singleAnguishLance.Behaviour.ForceWait(0.2f);
		singleAnguishMace.Behaviour.ForceWait(0.2f);
		singleAnguishShield.Behaviour.ForceWait(0.2f);
		StartCoroutine(WaitAllFreeAndCallback(OnAllOnDeathPosition));
	}

	private void OnAllOnDeathPosition()
	{
		singleAnguishLance.Behaviour.BackToAction();
		singleAnguishMace.Behaviour.BackToAction();
		singleAnguishShield.Behaviour.BackToAction();
		StartCoroutine(DelayedDeaths());
	}

	private IEnumerator DelayedDeaths(float initDelay = 0f)
	{
		yield return new WaitForSeconds(initDelay);
		TresAngustias.Audio.PlayDeath();
		yield return new WaitForSeconds(0.5f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(singleAnguishLance.transform.position, 0.3f, 0.3f, 1f);
		singleAnguishLance.Kill();
		singleAnguishLance.Behaviour.ActivateWeapon(activate: false);
		yield return new WaitForSeconds(0.5f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(singleAnguishMace.transform.position, 0.3f, 0.3f, 1f);
		singleAnguishMace.Kill();
		singleAnguishMace.Behaviour.ActivateWeapon(activate: false);
		yield return new WaitForSeconds(0.5f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(singleAnguishShield.transform.position, 0.3f, 0.3f, 1f);
		singleAnguishShield.Kill();
		singleAnguishShield.Behaviour.ActivateWeapon(activate: false);
		OnDeathSequenceEnds();
	}

	private void OnDeathSequenceEnds()
	{
		Core.Logic.Penitent.Status.Invulnerable = false;
		Core.Input.SetBlocker("CINEMATIC", blocking: false);
	}

	private IEnumerator RandomExplosions(float seconds, int totalExplosions, Transform center, float radius, GameObject poolableExplosion, Action OnExplosion = null, Action callback = null)
	{
		float counter = 0f;
		int expCounter = 0;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			if (counter > ((float)expCounter + 1f) / seconds)
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
