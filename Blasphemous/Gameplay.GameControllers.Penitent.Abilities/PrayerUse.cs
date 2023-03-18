using System;
using FMOD.Studio;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class PrayerUse : Ability
{
	public Core.SimpleEvent OnPrayerStart;

	[FoldoutGroup("Prayers reference", 0)]
	public BossInstantProjectileAttack multishotPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public BossAreaSummonAttack lightBeamPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public BossAreaSummonAttack flamePillarsPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public BossAreaSummonAttack divineLightPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public BossAreaSummonAttack stuntPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public BossStraightProjectileAttack crawlerBallsPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public ShieldSystemPrayer shieldPrayer;

	[FoldoutGroup("Prayers reference", 0)]
	public AlliedCherubPrayer cherubPrayer;

	public int slot;

	public string soundCast = "event:/SFX/Penitent/Prayers/PenitentFervor";

	public float soundEventTime = 2.4f;

	public string soundEventParameter = "EndPrayer";

	public float timeToUseHability = 0.02f;

	public const float TimePressedToActivate = 0f;

	private readonly int _animAuraTransform = UnityEngine.Animator.StringToHash("AuraTransform");

	private Penitent _penitent;

	private float timeToLaunchEvent;

	private bool soundEventLaunched;

	private EventInstance audioInstance = default(EventInstance);

	private float timeCasting;

	private float timeToEnd;

	private bool CanUsePrayer
	{
		get
		{
			bool result = false;
			Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(slot);
			if ((bool)prayerInSlot)
			{
				result = !base.Casting && base.EntityOwner.Status.IsGrounded && !base.IsUsingAbility && _penitent.Stats.Fervour.Current >= (float)prayerInSlot.fervourNeeded + _penitent.Stats.PrayerCostAddition.Final && !Core.Input.InputBlocked && !Core.Input.HasBlocker("CONSOLE");
			}
			return result;
		}
	}

	public Prayer GetEquippedPrayer()
	{
		return Core.InventoryManager.GetPrayerInSlot(slot);
	}

	public float GetPercentTimeCasting()
	{
		float result = 0f;
		if (base.IsUsingAbility && timeToEnd > 0f)
		{
			result = timeCasting / timeToEnd;
		}
		return result;
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		Prayer equippedPrayer = GetEquippedPrayer();
		if ((bool)equippedPrayer)
		{
			StartUsingPrayer(equippedPrayer);
		}
		if (OnPrayerStart != null)
		{
			OnPrayerStart();
		}
	}

	protected override void OnDead()
	{
		if (audioInstance.isValid())
		{
			audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = base.EntityOwner.GetComponent<Penitent>();
		if (!audioInstance.isValid())
		{
			audioInstance = Core.Audio.CreateEvent(soundCast);
			audioInstance.setParameterValue(soundEventParameter, 0f);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Rewired.GetButtonTimedPressDown(25, 0f) && !Core.Input.InputBlocked)
		{
			Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(slot);
			if ((bool)prayerInSlot)
			{
				if (!base.Casting && base.EntityOwner.Status.IsGrounded && !base.IsUsingAbility && !(_penitent.Stats.Fervour.Current >= (float)prayerInSlot.fervourNeeded + _penitent.Stats.PrayerCostAddition.Final))
				{
					UIController.instance.NotEnoughFervour();
				}
			}
			else
			{
				UIController.instance.NotEnoughFervour();
			}
			if (CanUsePrayer)
			{
				base.EntityOwner.Animator.Play(_animAuraTransform);
			}
		}
		if (base.IsUsingAbility)
		{
			timeToLaunchEvent -= Time.deltaTime;
			if (audioInstance.isValid() && !soundEventLaunched && timeToLaunchEvent <= 0f)
			{
				soundEventLaunched = true;
				audioInstance.setParameterValue(soundEventParameter, 1f);
			}
			timeCasting += Time.deltaTime;
			if (timeCasting >= timeToEnd)
			{
				timeCasting = timeToEnd;
				EndUsingPrayer();
			}
		}
	}

	private void OnDestroy()
	{
		if (audioInstance.isValid())
		{
			audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			audioInstance.release();
			audioInstance = default(EventInstance);
		}
	}

	private void StartUsingPrayer(Prayer prayer)
	{
		Fervour fervour = _penitent.Stats.Fervour;
		float num = prayer.EffectTime;
		if (num > 0f)
		{
			num += _penitent.Stats.PrayerDurationAddition.Final;
		}
		if (Math.Abs(num) > Mathf.Epsilon)
		{
			timeToLaunchEvent = num - soundEventTime;
			soundEventLaunched = false;
		}
		else
		{
			soundEventLaunched = true;
		}
		timeToEnd = num;
		timeCasting = 0f;
		fervour.Current -= (float)prayer.fervourNeeded + _penitent.Stats.PrayerCostAddition.Final;
		prayer.Use();
		if (audioInstance.isValid())
		{
			audioInstance.setParameterValue(soundEventParameter, 0f);
			audioInstance.start();
		}
	}

	public void ForcePrayerEnd()
	{
		EndUsingPrayer();
	}

	private void EndUsingPrayer()
	{
		StopCast();
		if (audioInstance.isValid() && !soundEventLaunched)
		{
			audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	private void PlaySound(string sound)
	{
		if (!string.IsNullOrEmpty(sound))
		{
			Core.Audio.PlayOneShot(sound);
		}
	}
}
