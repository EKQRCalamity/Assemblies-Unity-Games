using FMODUnity;
using UnityEngine;

namespace Framework.Audio;

public class AudioReleaser : MonoBehaviour
{
	private StudioEventEmitter[] _eventEmitters;

	private void Start()
	{
		_eventEmitters = Object.FindObjectsOfType<StudioEventEmitter>();
	}

	private void OnDestroy()
	{
		if (_eventEmitters != null)
		{
			for (int i = 0; i < _eventEmitters.Length; i++)
			{
				_eventEmitters[i].Stop();
			}
		}
	}
}
