using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.LanceAngel.Audio;

public class LanceAngelAudio : EntityAudio
{
	public const string FlyEventKey = "AngelFly";

	public const string AttackEventKey = "AngelAttack";

	public const string DeathEventKey = "AngelDeath";

	private const string MoveParameterKey = "Moves";

	private EventInstance _attackEventInstance;

	public void PlayFlap()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("AngelFly", FxSoundCategory.Motion);
		}
	}

	public void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackEventInstance, "AngelAttack");
	}

	public void StopAttack()
	{
		StopEvent(ref _attackEventInstance);
	}

	public void SetAttackParam(float value)
	{
		SetMoveParam(_attackEventInstance, value);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("AngelDeath", FxSoundCategory.Damage);
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

	private void OnDestroy()
	{
		StopAttack();
	}
}
