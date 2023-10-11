using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCurves : ACurves
{
	[Header("Particles=======================================================================================================", order = 1)]
	public FloatCurve emission;

	public FloatCurve emissionByDistance;

	public FloatCurve size;

	public FloatCurve lifetime;

	public ColorCurve color;

	public bool binaryEmission;

	private ParticleSystem _system;

	private ParticleSystem.EmissionModule _emissionModule;

	private ParticleSystem.MainModule _mainModule;

	protected ParticleSystem system
	{
		get
		{
			if (!(_system != null))
			{
				return _InitializeValues();
			}
			return _system;
		}
	}

	private ParticleSystem _InitializeValues()
	{
		_system = GetComponent<ParticleSystem>();
		_emissionModule = _system.emission;
		emission.initialValue = _emissionModule.rateOverTimeMultiplier;
		emissionByDistance.initialValue = _emissionModule.rateOverDistanceMultiplier;
		_mainModule = _system.main;
		size.initialValue = _mainModule.startSizeMultiplier;
		lifetime.initialValue = _mainModule.startLifetimeMultiplier;
		color.initialValue = _mainModule.startColor.color;
		return _system;
	}

	private void Awake()
	{
		_ = system;
	}

	protected override void _Input(float t)
	{
		_ = system;
		if (emission.enabled)
		{
			_emissionModule.rateOverTimeMultiplier = emission.GetValue(t);
		}
		if (emissionByDistance.enabled)
		{
			_emissionModule.rateOverDistanceMultiplier = emissionByDistance.GetValue(t);
		}
		if (size.enabled)
		{
			_mainModule.startSizeMultiplier = size.GetValue(t);
		}
		if (lifetime.enabled)
		{
			_mainModule.startLifetimeMultiplier = lifetime.GetValue(t);
		}
		if (color.enabled)
		{
			_mainModule.startColor = color.GetValue(_mainModule.startColor.color, t);
		}
		if (binaryEmission)
		{
			_emissionModule.enabled = t >= 1f;
		}
	}

	protected override bool _IsSafeToDeactivate()
	{
		if (base._IsSafeToDeactivate())
		{
			return system.particleCount == 0;
		}
		return false;
	}
}
