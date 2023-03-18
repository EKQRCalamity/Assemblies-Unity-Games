using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CauldronNun.Audio;

public class CauldronNunAudio : EntityAudio
{
	private const string DeathEventKey = "CauldronNunDeath";

	private const string CallEventKey = "CauldronNunCall";

	public float maxAudioDistance = 20f;

	public float minAudioDistance = 7f;

	private EventInstance _callEventInstance;

	private float _lastDistance;

	private float _lastDistanceParam;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_callEventInstance.isValid())
		{
			Core.Audio.ApplyDistanceParam(ref _callEventInstance, minAudioDistance, maxAudioDistance, base.transform, Core.Logic.Penitent.transform);
			EntityAudio.SetPanning(_callEventInstance, base.transform.position);
		}
	}

	public void PlayCall()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			StopAll();
			Core.Audio.PlayEventWithCatalog(ref _callEventInstance, "CauldronNunCall");
		}
	}

	public void StopAll()
	{
		StopEvent(ref _callEventInstance);
	}

	public void PlayDeath()
	{
		StopAll();
		PlayOneShotEvent("CauldronNunDeath", FxSoundCategory.Damage);
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
