using System;
using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Audio;

public class QuirceAudio : EntityAudio
{
	private const string SwordlessDashEventKey = "QuirceSwordlessDash";

	private const string DeathEventKey = "QuirceDeath";

	private const string TeleportOutEventKey = "QuirceTeleportOut";

	private const string TeleportInEventKey = "QuirceTeleportIn";

	private const string PlungeEventKey = "QuircePlunge";

	private const string PreDashEventKey = "QuircePreDash";

	private const string BigDashEventKey = "QuirceBigDash";

	private const string SwordTossEventKey = "QuirceSwordToss";

	private const string SwordStuckEventKey = "QuirceSwordHitsWall";

	private const string SpinSwordEventKey = "QuirceThrowSword";

	private const string endParamKey = "end";

	private EventInstance _spiningSword;

	public void PlayDeath()
	{
		PlayOneShotEvent("QuirceDeath", FxSoundCategory.Damage);
	}

	public void PlaySwordlessDash()
	{
		PlayOneShotEvent("QuirceSwordlessDash", FxSoundCategory.Motion);
	}

	public void PlayTeleportOut()
	{
		PlayOneShotEvent("QuirceTeleportOut", FxSoundCategory.Motion);
	}

	public void PlayTeleportIn()
	{
		PlayOneShotEvent("QuirceTeleportIn", FxSoundCategory.Motion);
	}

	public void PlayPlunge()
	{
		PlayOneShotEvent("QuircePlunge", FxSoundCategory.Attack);
	}

	public void PlayToss()
	{
		PlayOneShotEvent("QuirceSwordToss", FxSoundCategory.Attack);
	}

	public void PlayPreDash()
	{
		PlayOneShotEvent("QuircePreDash", FxSoundCategory.Attack);
	}

	public void PlayHitWall()
	{
		PlayOneShotEvent("QuirceSwordHitsWall", FxSoundCategory.Attack);
	}

	public void PlayBigDash()
	{
		PlayOneShotEvent("QuirceBigDash", FxSoundCategory.Attack);
	}

	public void PlaySpinSword()
	{
		StopSpinSword();
		PlayEvent(ref _spiningSword, "QuirceThrowSword");
	}

	public void StopSpinSword()
	{
		StopEvent(ref _spiningSword);
	}

	public void EndSwordSpinSound()
	{
		SetEndParam(_spiningSword, 1f);
	}

	private void StopAll()
	{
		StopSpinSword();
	}

	private void SetEndParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("end", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	private void OnDestroy()
	{
		StopAll();
	}
}
