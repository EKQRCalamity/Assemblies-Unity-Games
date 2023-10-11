using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemHook : MonoBehaviour
{
	private ParticleSystem _system;

	private ParticleSystem.MainModule _mainModule;

	private ParticleSystem.EmissionModule _emissionModule;

	private ParticleSystem system
	{
		get
		{
			if (!_system)
			{
				_system = GetComponent<ParticleSystem>();
				_mainModule = _system.main;
				_emissionModule = _system.emission;
			}
			return _system;
		}
	}

	public void SetTint(Color tint)
	{
		_ = system;
		_mainModule.startColor = tint;
	}

	public void SetEmissionMultiplier(float multiplier)
	{
		_ = system;
		_emissionModule.rateOverTimeMultiplier = multiplier;
		_emissionModule.rateOverDistanceMultiplier = multiplier;
		_emissionModule.enabled = multiplier > 0f;
	}
}
