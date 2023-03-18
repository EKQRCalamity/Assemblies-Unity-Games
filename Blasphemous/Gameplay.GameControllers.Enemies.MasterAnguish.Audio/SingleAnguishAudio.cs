using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MasterAnguish.Audio;

public class SingleAnguishAudio : EntityAudio
{
	private const string LanceChargeKey = "LanceChargeKey";

	private const string LanceShotKey = "LanceShotKey";

	private const string SpawnEventKey = "SpawnEventKey";

	private const string AppearEventKey = "AngustiasAppear";

	private const string MaceThrowKey = "AngustiasThrow";

	private const string EndParameterKey = "End";

	private EventInstance _maceInstance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayLanceCharge()
	{
		PlayOneShotEvent("LanceChargeKey", FxSoundCategory.Attack);
	}

	public void PlayMace()
	{
		StopEvent(ref _maceInstance);
		PlayEvent(ref _maceInstance, "AngustiasThrow");
	}

	public void StopMace()
	{
		SetEndParam(_maceInstance, 1f);
	}

	public void PlayLanceShot()
	{
		PlayOneShotEvent("LanceShotKey", FxSoundCategory.Attack);
	}

	public void PlaySpawn()
	{
		PlayOneShotEvent("SpawnEventKey", FxSoundCategory.Attack);
	}

	public void PlayAppear()
	{
		PlayOneShotEvent("AngustiasAppear", FxSoundCategory.Motion);
	}

	public void StopAll()
	{
	}

	public void SetEndParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("End", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
