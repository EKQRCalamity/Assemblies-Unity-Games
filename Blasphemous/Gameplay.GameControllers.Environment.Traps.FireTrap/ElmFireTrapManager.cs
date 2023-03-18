using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

[RequireComponent(typeof(ElmFireTrap))]
public class ElmFireTrapManager : MonoBehaviour
{
	public List<ElmFireTrap> elmFireTrapNodes;

	public bool AutoLoops;

	[ShowIf("AutoLoops", true)]
	public float WaitTimeToStart;

	[ShowIf("AutoLoops", true)]
	public float WaitTimeToShowEachTrap;

	[ShowIf("AutoLoops", true)]
	public float WaitTimeBeforeStartCharging;

	[ShowIf("AutoLoops", true)]
	public float ChargingTime;

	[ShowIf("AutoLoops", true)]
	public float WaitTimeBeforeStartHiding;

	[ShowIf("AutoLoops", true)]
	public float WaitTimeToHideEachTrap;

	public float DamageForNG;

	public float DamageForNGPlus;

	[HideInInspector]
	public bool ElmFireLoopEndReached;

	[SerializeField]
	[OnValueChanged("OnEnableTrapPropertyChange", false)]
	private bool enableTraps = true;

	private ElmFireTrap _elmFireTrap;

	private ElmFireTrap _cycleFinisherTrap;

	private Action OnCycleFinish;

	private void Awake()
	{
		_elmFireTrap = GetComponent<ElmFireTrap>();
		if (_elmFireTrap.linkType != 0)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		CheckForUniqueChainCore(SubscribeCycleFinishEvent);
		if (AutoLoops)
		{
			InstantHideElmFireTraps();
			StartLoopCoroutine();
		}
		float damage = ((!Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS)) ? DamageForNG : DamageForNGPlus);
		RecursiveSetUpTrapDamage(damage);
	}

	public void RecursiveSetUpTrapDamage(float damage)
	{
		_elmFireTrap.LightningPrefab.GetComponentInChildren<BeamAttack>().lightningHit.DamageAmount = damage;
		RecursiveSetUpTrapDamage(_elmFireTrap, damage);
	}

	private void RecursiveSetUpTrapDamage(ElmFireTrap elmFireTrap, float damage)
	{
		elmFireTrap.Attack.ProximityHitAttack.DamageAmount = damage;
		elmFireTrap.SetDamage(damage);
		if (!(elmFireTrap.target != null))
		{
			return;
		}
		RecursiveSetUpTrapDamage(elmFireTrap.target, damage);
		if (!elmFireTrap.hasMoreTargets)
		{
			return;
		}
		foreach (ElmFireTrap additionalTarget in elmFireTrap.additionalTargets)
		{
			RecursiveSetUpTrapDamage(additionalTarget, damage);
		}
	}

	private void StartLoopCoroutine()
	{
		StartCoroutine(LoopCoroutine(WaitTimeToStart, WaitTimeToShowEachTrap, WaitTimeBeforeStartCharging, ChargingTime, WaitTimeBeforeStartHiding, WaitTimeToHideEachTrap));
		OnCycleFinish = (Action)Delegate.Remove(OnCycleFinish, new Action(StartLoopCoroutine));
		OnCycleFinish = (Action)Delegate.Combine(OnCycleFinish, new Action(StartLoopCoroutine));
	}

	private void SubscribeCycleFinishEvent()
	{
		foreach (ElmFireTrap elmFireTrapNode in elmFireTrapNodes)
		{
			if ((bool)elmFireTrapNode.target)
			{
				continue;
			}
			_cycleFinisherTrap = elmFireTrapNode;
			ElmFireTrap cycleFinisherTrap = _cycleFinisherTrap;
			cycleFinisherTrap.OnCycleFinished = (Core.SimpleEvent)Delegate.Combine(cycleFinisherTrap.OnCycleFinished, new Core.SimpleEvent(OnCycleFinished));
			break;
		}
	}

	private void CheckForUniqueChainCore(Action callbackAction)
	{
		int num = 0;
		foreach (ElmFireTrap elmFireTrapNode in elmFireTrapNodes)
		{
			if (elmFireTrapNode.linkType == ElmFireTrap.LinkType.Core)
			{
				num++;
			}
		}
		if (num == 1 && callbackAction != null)
		{
			callbackAction();
		}
		else
		{
			Debug.LogError("There isn't ONE core trap in the chain!");
		}
	}

	private void OnEnableTrapPropertyChange()
	{
		if (enableTraps)
		{
			EnableTraps();
		}
		else
		{
			DisableTraps();
		}
	}

	public void EnableTraps()
	{
		foreach (ElmFireTrap elmFireTrapNode in elmFireTrapNodes)
		{
			elmFireTrapNode.enabled = true;
		}
	}

	public void DisableTraps()
	{
		foreach (ElmFireTrap elmFireTrapNode in elmFireTrapNodes)
		{
			elmFireTrapNode.enabled = false;
		}
	}

	private void OnCycleFinished()
	{
		_elmFireTrap.ResetTrapCycle();
	}

	private void OnDestroy()
	{
		if ((bool)_cycleFinisherTrap)
		{
			ElmFireTrap cycleFinisherTrap = _cycleFinisherTrap;
			cycleFinisherTrap.OnCycleFinished = (Core.SimpleEvent)Delegate.Remove(cycleFinisherTrap.OnCycleFinished, new Core.SimpleEvent(OnCycleFinished));
		}
		OnCycleFinish = (Action)Delegate.Remove(OnCycleFinish, new Action(StartLoopCoroutine));
	}

	public IEnumerator LoopCoroutine(float waitTimeToStart, float waitTimeToShowEachTrap, float waitTimeBeforeStartCharging, float chargingTime, float waitTimeBeforeStartHiding, float waitTimeToHideEachTrap)
	{
		yield return new WaitForSeconds(waitTimeToStart);
		ShowElmFireTrapRecursively(elmFireTrapNodes[0], waitTimeToShowEachTrap, chargingTime, applyChargingTimeToAll: true);
		yield return new WaitForSeconds(waitTimeBeforeStartCharging);
		elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
		EnableTraps();
		yield return new WaitForSeconds(waitTimeBeforeStartHiding);
		DisableTraps();
		HideElmFireTrapRecursively(elmFireTrapNodes[0], waitTimeToHideEachTrap);
		yield return new WaitUntil(() => ElmFireLoopEndReached);
		if (OnCycleFinish != null)
		{
			OnCycleFinish();
		}
	}

	public void InstantHideElmFireTraps()
	{
		DisableTraps();
		InstantHideElmFireTrapRecursively(_elmFireTrap);
	}

	private void InstantHideElmFireTrapRecursively(ElmFireTrap elmFireTrap)
	{
		if ((bool)elmFireTrap.Collider)
		{
			elmFireTrap.Collider.enabled = false;
		}
		elmFireTrap.Animator.Play("Hidden");
		if (!(elmFireTrap.target != null))
		{
			return;
		}
		InstantHideElmFireTrapRecursively(elmFireTrap.target);
		if (!elmFireTrap.hasMoreTargets)
		{
			return;
		}
		foreach (ElmFireTrap additionalTarget in elmFireTrap.additionalTargets)
		{
			InstantHideElmFireTrapRecursively(additionalTarget);
		}
	}

	public void HideElmFireTrapRecursively(ElmFireTrap elmFireTrap, float waitTime)
	{
		ElmFireLoopEndReached = false;
		StartCoroutine(ShowOrHideTrapRecursiveCoroutine(elmFireTrap, waitTime, "HIDE", "SHOW"));
	}

	public void ShowElmFireTrapRecursively(ElmFireTrap elmFireTrap, float waitTime, float lightningChargeLapse, bool applyChargingTimeToAll)
	{
		ElmFireLoopEndReached = false;
		StartCoroutine(ShowOrHideTrapRecursiveCoroutine(elmFireTrap, waitTime, "SHOW", "HIDE", lightningChargeLapse, applyChargingTimeToAll));
	}

	private IEnumerator ShowOrHideTrapRecursiveCoroutine(ElmFireTrap elmFireTrap, float waitTime, string triggerNameToSet, string triggerNameToReset, float chargingTime = -1f, bool applyChargingTimeToAll = false)
	{
		if (chargingTime >= 0f)
		{
			elmFireTrap.chargingTime = chargingTime;
		}
		if (!applyChargingTimeToAll)
		{
			chargingTime = -1f;
		}
		elmFireTrap.Animator.SetTrigger(triggerNameToSet);
		elmFireTrap.Animator.ResetTrigger(triggerNameToReset);
		if (elmFireTrap.target != null)
		{
			yield return new WaitForSeconds(waitTime);
			StartCoroutine(ShowOrHideTrapRecursiveCoroutine(elmFireTrap.target, waitTime, triggerNameToSet, triggerNameToReset, chargingTime, applyChargingTimeToAll));
			if (!elmFireTrap.hasMoreTargets)
			{
				yield break;
			}
			{
				foreach (ElmFireTrap additionalTarget in elmFireTrap.additionalTargets)
				{
					StartCoroutine(ShowOrHideTrapRecursiveCoroutine(additionalTarget, waitTime, triggerNameToSet, triggerNameToReset, chargingTime, applyChargingTimeToAll));
				}
				yield break;
			}
		}
		ElmFireLoopEndReached = true;
	}
}
