using System;
using FMOD.Studio;
using Gameplay.GameControllers.Bosses.BossFight;
using Gameplay.GameControllers.Entities.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.Audio;

public class EsdrasAudio : EntityAudio
{
	private const string ESDRAS_DEATH = "EsdrasDeath";

	private const string ESDRAS_NORMAL_ATTACK = "EsdrasNormalAttack";

	private const string ESDRAS_GROUND_HIT = "EsdrasGroundHit";

	private const string ESDRAS_FOOTSTEP = "EsdrasRun";

	private const string ESDRAS_HEAVY_ATTACK_SMASH = "EsdrasThunderAttack";

	public const string ESDRAS_SLIDE_ATTACK = "EsdrasSlideAttack";

	public const string ESDRAS_SPIN_LOOP = "EsdrasSpinLoop";

	public const string ESDRAS_SPINPROJECTILE = "EsdrasSpinProjectile";

	public const string ESDRAS_CALL_SISTER = "EsdrasPerpetuaCall";

	private EventInstance _slideAttackInstance;

	private EventInstance _spinLoopInstance;

	private EventInstance _spinProjectileInstance;

	private const string MoveParameterKey = "End";

	private EventInstance queuedEvent;

	private string queuedEventKey;

	public void PlayQueuedEvent()
	{
		if (!string.IsNullOrEmpty(queuedEventKey))
		{
			StopAll();
			PlayEvent(ref queuedEvent, queuedEventKey);
		}
	}

	public void QueueSlideAttack_AUDIO()
	{
		queuedEventKey = "EsdrasSlideAttack";
	}

	public void StopSlideAttack_AUDIO()
	{
		SetEndParam(1f);
	}

	public void QueueSpinLoop()
	{
		queuedEventKey = "EsdrasSpinLoop";
	}

	public void StopSpinLoop_AUDIO()
	{
		SetEndParam(1f);
	}

	public void QueueSpinProjectile()
	{
		queuedEventKey = "EsdrasSpinProjectile";
	}

	public void StopSpinProjectile_AUDIO()
	{
		SetEndParam(1f);
	}

	public void ChangeEsdrasMusic()
	{
		BossFightAudio bossFightAudio = UnityEngine.Object.FindObjectOfType<BossFightAudio>();
		bossFightAudio.SetBossTrackParam("Mixdown", 1f);
	}

	public void PlayDeath_AUDIO()
	{
		StopAll();
		PlayOneShotEvent("EsdrasDeath", FxSoundCategory.Damage);
	}

	public void PlayGroundHit_AUDIO()
	{
		PlayOneShotEvent("EsdrasGroundHit", FxSoundCategory.Damage);
	}

	public void PlayCallSister_AUDIO()
	{
		PlayOneShotEvent("EsdrasPerpetuaCall", FxSoundCategory.Attack);
	}

	public void PlayLightAttack_AUDIO()
	{
		PlayOneShotEvent("EsdrasNormalAttack", FxSoundCategory.Damage);
	}

	public void PlayHeavyAttackSmash_AUDIO()
	{
		PlayOneShotEvent("EsdrasThunderAttack", FxSoundCategory.Attack);
	}

	public void PlayFootStep_AUDIO()
	{
		PlayOneShotEvent("EsdrasRun", FxSoundCategory.Motion);
	}

	public void SetEndParam(float value)
	{
		if (queuedEvent.isValid())
		{
			SetMoveParam(queuedEvent, value);
		}
	}

	[Button("Stop events", ButtonSizes.Small)]
	public void StopAll()
	{
		StopEvent(ref queuedEvent);
	}

	private void OnDestroy()
	{
		StopAll();
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
}
