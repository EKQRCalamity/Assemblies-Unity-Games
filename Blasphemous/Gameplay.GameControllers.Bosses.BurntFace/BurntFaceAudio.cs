using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceAudio : EntityAudio
{
	private const string AppearEventKey = "BurntFaceAppear";

	private const string DisappearEventKey = "BurntFaceDisappear";

	private const string BeamFireEventKey = "BurntFaceBeamFire";

	private const string BeamChargeEventKey = "BurntFaceBeamCharge";

	private const string DeathEventKey = "BurntFaceDeath";

	private const string AttackParamKey = "Attack";

	private const string EndParamKey = "End";

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Owner.SpriteRenderer.isVisible || Owner.Status.Dead)
		{
			StopAll();
		}
	}

	public void PlayAppear()
	{
		PlayOneShotEvent("BurntFaceAppear", FxSoundCategory.Motion);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("BurntFaceDeath", FxSoundCategory.Motion);
	}

	public void PlayDisappear()
	{
		PlayOneShotEvent("BurntFaceAppear", FxSoundCategory.Motion);
	}

	public void PlayBeamCharge(ref EventInstance instance)
	{
		StopEvent(ref instance);
		PlayEvent(ref instance, "BurntFaceBeamFire");
	}

	public void PlayBeamFire(ref EventInstance instance)
	{
		SetParam(instance, "Attack", 1f);
	}

	public void StopBeamFire(ref EventInstance e)
	{
		SetParam(e, "End", 1f);
	}

	public void StopAll()
	{
	}

	public void SetParam(EventInstance eventInstance, string paramKey, float value)
	{
		try
		{
			eventInstance.getParameter(paramKey, out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
