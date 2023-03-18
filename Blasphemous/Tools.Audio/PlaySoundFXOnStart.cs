using FMODUnity;
using Framework.Managers;
using UnityEngine;

namespace Tools.Audio;

public class PlaySoundFXOnStart : MonoBehaviour
{
	[EventRef]
	public string eventId;

	private void Start()
	{
		Core.Audio.PlaySfx(eventId);
	}
}
