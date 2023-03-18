using FMOD.Studio;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.NPCs;

public class NPCAudio : MonoBehaviour
{
	public bool stopOnDisable;

	private EventInstance eventInstance;

	private void OnDisable()
	{
		if (stopOnDisable)
		{
			StopEvent();
		}
	}

	private void PlayEventOneShot(string eventKey)
	{
		Core.Audio.PlayOneShot(eventKey, base.transform.position);
	}

	private void PlayEvent(string eventKey)
	{
		if (eventInstance.isValid())
		{
			StopEvent();
		}
		eventInstance = default(EventInstance);
		Core.Audio.PlayEventNoCatalog(ref eventInstance, eventKey, base.transform.position);
	}

	private void StopEvent()
	{
		if (eventInstance.isValid())
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
			eventInstance = default(EventInstance);
		}
	}
}
