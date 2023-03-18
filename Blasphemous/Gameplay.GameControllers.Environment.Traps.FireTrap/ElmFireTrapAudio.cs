using System;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

[RequireComponent(typeof(ElmFireTrap))]
public class ElmFireTrapAudio : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string IdleEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string ShotEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string ChargeEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string InEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string OutEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	public string IdlePulsesEvent;

	private const string ChargeParam = "Charge";

	private ElmFireTrap elmFireTrap;

	private SpriteRenderer spriteRenderer;

	private EventInstance idleEventInstance;

	private EventInstance chargeEventInstance;

	public bool idleIsPlaying;

	public bool chargeIsPlaying;

	private float totalChargingTime;

	private float currentChargingTime;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		elmFireTrap = GetComponent<ElmFireTrap>();
		ElmFireTrap obj = elmFireTrap;
		obj.OnChargeStart = (Core.SimpleEventParam)Delegate.Combine(obj.OnChargeStart, new Core.SimpleEventParam(StartCharge));
		ElmFireTrap obj2 = elmFireTrap;
		obj2.OnLightningCast = (Core.SimpleEventParam)Delegate.Combine(obj2.OnLightningCast, new Core.SimpleEventParam(PlayShot));
	}

	private void Update()
	{
		if (!IsTrapVisible())
		{
			if (idleIsPlaying)
			{
				StopIdle();
			}
			if (chargeIsPlaying)
			{
				StopCharge();
			}
		}
		else if (chargeIsPlaying)
		{
			currentChargingTime += Time.deltaTime;
			float percentage = currentChargingTime / totalChargingTime;
			UpdateChargeAudioParameter(percentage);
		}
		else if (!idleIsPlaying)
		{
			PlayIdle();
		}
	}

	private void OnDisable()
	{
		StopIdle();
		if (elmFireTrap == null)
		{
			elmFireTrap = GetComponent<ElmFireTrap>();
		}
		ElmFireTrap obj = elmFireTrap;
		obj.OnChargeStart = (Core.SimpleEventParam)Delegate.Remove(obj.OnChargeStart, new Core.SimpleEventParam(StartCharge));
		ElmFireTrap obj2 = elmFireTrap;
		obj2.OnLightningCast = (Core.SimpleEventParam)Delegate.Remove(obj2.OnLightningCast, new Core.SimpleEventParam(PlayShot));
	}

	public void PlayShot(object targetPosition)
	{
		StopCharge();
		Vector3 position = ((Vector3)targetPosition + base.transform.position) / 2f;
		Core.Audio.PlayOneShot(ShotEvent, position);
	}

	public void AnimEvent_PlayIn()
	{
		PlayIn();
	}

	public void AnimEvent_PlayOut()
	{
		PlayOut();
	}

	public void PlayIn()
	{
		Core.Audio.PlayOneShot(InEvent, base.transform.position);
	}

	public void PlayOut()
	{
		StopIdle();
		Core.Audio.PlayOneShot(OutEvent, base.transform.position);
	}

	public void PlayIdle()
	{
		idleIsPlaying = true;
		idleEventInstance = default(EventInstance);
		Core.Audio.PlayEventNoCatalog(ref idleEventInstance, (elmFireTrap.linkType != ElmFireTrap.LinkType.Static) ? IdleEvent : IdlePulsesEvent, base.transform.position);
	}

	public void StopIdle()
	{
		idleIsPlaying = false;
		idleEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		idleEventInstance.release();
	}

	public void StartCharge(object chargingTime)
	{
		PlayCharge();
		totalChargingTime = (float)chargingTime;
		currentChargingTime = 0f;
	}

	public void PlayCharge()
	{
		StopIdle();
		chargeIsPlaying = true;
		chargeEventInstance = default(EventInstance);
		Core.Audio.PlayEventNoCatalog(ref chargeEventInstance, ChargeEvent, base.transform.position);
	}

	public void StopCharge()
	{
		chargeIsPlaying = false;
		chargeEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		chargeEventInstance.release();
	}

	public void UpdateChargeAudioParameter(float percentage)
	{
		try
		{
			chargeEventInstance.getParameter("Charge", out var instance);
			instance.setValue(percentage);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	private bool IsTrapVisible()
	{
		return spriteRenderer.IsVisibleFrom(UnityEngine.Camera.main);
	}
}
