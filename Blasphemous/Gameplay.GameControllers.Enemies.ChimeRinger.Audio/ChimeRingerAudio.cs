using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ChimeRinger.Audio;

public class ChimeRingerAudio : EntityAudio
{
	private const string DeathEventKey = "ChimeRingerDeath";

	private const string CallEventKey = "ChimeRingerCall";

	public float maxAudioDistance = 20f;

	public float minAudioDistance = 7f;

	private EventInstance _callEventInstance;

	private float _lastDistance;

	private float _lastDistanceParam;

	protected Gameplay.GameControllers.Penitent.Penitent Penitent { get; set; }

	protected override void OnUpdate()
	{
	}

	public void PlayCall()
	{
		StopAll();
		Core.Audio.PlayEventWithCatalog(ref _callEventInstance, "ChimeRingerCall", base.transform.position);
	}

	public void PlayDeath()
	{
		PlayOneShotEvent("ChimeRingerDeath", FxSoundCategory.Damage);
	}

	public void StopAll()
	{
		StopEvent(ref _callEventInstance);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, maxAudioDistance);
		Gizmos.DrawWireSphere(base.transform.position, minAudioDistance);
		Gizmos.color = Color.Lerp(Color.red, Color.blue, _lastDistanceParam);
		Gizmos.DrawWireSphere(base.transform.position, _lastDistance);
	}
}
