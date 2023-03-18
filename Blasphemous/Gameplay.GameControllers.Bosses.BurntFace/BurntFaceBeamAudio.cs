using FMOD.Studio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceBeamAudio : MonoBehaviour
{
	private BurntFaceAudio _audio;

	private EventInstance _beamFireEventInstance;

	private void Awake()
	{
		_audio = Object.FindObjectOfType<BurntFaceAudio>();
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
