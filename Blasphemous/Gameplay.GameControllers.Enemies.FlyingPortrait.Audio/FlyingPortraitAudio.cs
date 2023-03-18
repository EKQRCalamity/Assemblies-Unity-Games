using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.FlyingPortrait.Audio;

public class FlyingPortraitAudio : EntityAudio
{
	private const string AttackEventKey = "FlyingPortraitAttack";

	private const string FloatingEventKey = "FlyingPortraitFloating";

	private const string DeathEventKey = "FlyingPortraitDeath";

	private const string UnlockEventKey = "FlyingPortraitUnlock";

	private EventInstance _attackEventInstance;

	private EventInstance _floatingEventInstance;

	private const string MoveParameterKey = "Moves";

	public void PlayUnlock()
	{
		PlayOneShotEvent("FlyingPortraitUnlock", FxSoundCategory.Motion);
	}

	public void PlayFloating()
	{
		PlayEvent(ref _floatingEventInstance, "FlyingPortraitFloating");
	}

	public void PlayDeath()
	{
		StopAttack();
		StopEvent(ref _floatingEventInstance);
		PlayOneShotEvent("FlyingPortraitDeath", FxSoundCategory.Damage);
	}

	public void PlayAttack()
	{
		PlayEvent(ref _attackEventInstance, "FlyingPortraitAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void SetAttackParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	public void SetMoveParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("Moves", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}
}
