using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.DistortionWave;

public class DistortionWave : MonoBehaviour
{
	public ParticleSystem DistortionParticleSystem;

	private void Start()
	{
		if (DistortionParticleSystem == null)
		{
			Debug.LogError("A particle system is needed!");
		}
	}

	public void PreWarm()
	{
		if (!(DistortionParticleSystem == null))
		{
			if (DistortionParticleSystem.isEmitting)
			{
				DistortionParticleSystem.Stop();
			}
			ParticleSystem.MainModule main = DistortionParticleSystem.main;
			main.loop = true;
			main.prewarm = true;
		}
	}

	public void Play()
	{
		if (!(DistortionParticleSystem == null))
		{
			DistortionParticleSystem.Play();
			ParticleSystem.MainModule main = DistortionParticleSystem.main;
			main.loop = false;
			main.prewarm = false;
		}
	}
}
