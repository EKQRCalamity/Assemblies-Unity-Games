using System;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Audio;

public class InkLadyAudio : MeltedLadyAudio
{
	private const string BeamFireEventKey = "InkLadyBeamFire";

	private const string AttackParamKey = "Attack";

	private const string EndParamKey = "End";

	private const string AttackEventKey = "MeltedLadyAttack";

	private const string AppearingEventKey = "InkLadyAppearing";

	private const string DisappearingEventKey = "InkLadyDisappearing";

	private const string DeathEventKey = "InkLadyDeath";

	private const string MeltedLadyDamage = "InkLadyHit";

	public void PlayBeamCharge(ref EventInstance beamFireInstance)
	{
		StopEvent(ref beamFireInstance);
		Core.Audio.PlayEventWithCatalog(ref beamFireInstance, "InkLadyBeamFire");
	}

	public void PlayBeamFire(ref EventInstance beamFireInstance)
	{
		SetParam(beamFireInstance, "Attack", 1f);
	}

	public void StopBeamFire(ref EventInstance beamFireInstance)
	{
		SetParam(beamFireInstance, "End", 1f);
	}

	private void SetParam(EventInstance eventInstance, string paramKey, float value)
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

	public new void Hurt()
	{
		PlayOneShotEvent("InkLadyHit", FxSoundCategory.Damage);
	}

	public new void Appearing()
	{
		PlayOneShotEvent("InkLadyAppearing", FxSoundCategory.Motion);
	}

	public new void Disappearing()
	{
		PlayOneShotEvent("InkLadyDisappearing", FxSoundCategory.Motion);
	}

	public new void PlayDeath()
	{
		PlayOneShotEvent("InkLadyDeath", FxSoundCategory.Damage);
	}
}
