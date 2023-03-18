using System;
using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Damage;
using UnityEngine;

namespace Framework.Penitences;

public class PenitencePE01 : IPenitence
{
	private const string id = "PE01";

	private readonly IEnumerator fervourRegenCoroutine;

	private readonly Pe01Balance balance;

	private const string BALANCE_PATH = "PE01/PE01Balance";

	public string Id => "PE01";

	public bool Completed { get; set; }

	public bool Abandoned { get; set; }

	public PenitencePE01()
	{
		fervourRegenCoroutine = FervourRegenCoroutine();
		balance = Resources.Load<Pe01Balance>("PE01/PE01Balance");
		if (!balance)
		{
			Debug.LogError("Can't find PE01 balance at PE01/PE01Balance");
		}
	}

	public void Activate()
	{
		RemoveEffects();
		AddEffects();
	}

	public void Deactivate()
	{
		RemoveEffects();
	}

	private void RemoveEffects()
	{
		Core.Logic.Penitent.Stats.Strength.FinalStrengthMultiplier = balance.normalStrengthMultiplier;
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		Singleton<Core>.Instance.StopCoroutine(fervourRegenCoroutine);
	}

	private void AddEffects()
	{
		Core.Logic.Penitent.Stats.Strength.FinalStrengthMultiplier = balance.pe01StrengthMultiplier;
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		Singleton<Core>.Instance.StartCoroutine(fervourRegenCoroutine);
	}

	private void OnDamagedGlobal(Penitent damaged, Hit hit)
	{
		if (hit.DamageAmount > 0f)
		{
			float a = Core.Logic.Penitent.Stats.Fervour.Current - balance.fervourLostAmount;
			Core.Logic.Penitent.Stats.Fervour.Current = Mathf.Max(a, 0f);
		}
	}

	private IEnumerator FervourRegenCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(balance.timePerRegenerationTick);
			Core.Logic.Penitent.Stats.Fervour.Current += balance.fervourRecoveryAmount;
		}
	}
}
