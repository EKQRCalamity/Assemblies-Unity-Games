using FMOD.Studio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Audio;

public class InkLadyBeamAudio : MonoBehaviour
{
	private InkLadyAudio _audio;

	private EventInstance _beamFireEventInstance;

	private void Awake()
	{
		_audio = Object.FindObjectOfType<InkLadyAudio>();
	}

	public void PlayBeamCharge()
	{
		_audio.PlayBeamCharge(ref _beamFireEventInstance);
	}

	public void PlayBeamFire()
	{
		_audio.PlayBeamFire(ref _beamFireEventInstance);
	}

	public void StopBeamFire()
	{
		_audio.StopBeamFire(ref _beamFireEventInstance);
	}
}
