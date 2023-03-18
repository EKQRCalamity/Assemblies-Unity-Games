using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Legionary.Audio;

public class LegionaryAudio : EntityAudio
{
	public const string FootStepEventKey = "LegionarioRun";

	public const string SlideAttackEventKey = "LegionarioSlideAttack";

	public const string LightAttackEventKey = "LegionarioNormalAttack";

	public const string DeathEventKey = "LegionarioDeath";

	public const string ThunderEventKey = "LegionarioThunder";

	public const string GroundHitEventKey = "LegionarioGroundHit";

	private const string MoveParameterKey = "End";

	private EventInstance _slideAttackInstance;

	public void PlayWalk_AUDIO()
	{
		PlayOneShotEvent("LegionarioRun", FxSoundCategory.Damage);
	}

	public void PlayLightAttack_AUDIO()
	{
		PlayOneShotEvent("LegionarioNormalAttack", FxSoundCategory.Attack);
	}

	public void PlaySlideAttack_AUDIO()
	{
		StopSlideAttack_AUDIO();
		PlayEvent(ref _slideAttackInstance, "LegionarioSlideAttack");
	}

	public void StopSlideAttack_AUDIO()
	{
		StopEvent(ref _slideAttackInstance);
	}

	public void PlayDeath_AUDIO()
	{
		PlayOneShotEvent("LegionarioDeath", FxSoundCategory.Damage);
	}

	public void PlayGroundHit_AUDIO()
	{
		PlayOneShotEvent("LegionarioGroundHit", FxSoundCategory.Motion);
	}

	public void SetAttackParam(float value)
	{
		SetMoveParam(_slideAttackInstance, value);
	}

	private void SetMoveParam(EventInstance eventInstance, float value)
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

	private void OnDestroy()
	{
		StopSlideAttack_AUDIO();
	}
}
