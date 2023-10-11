using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AttacherParticles : Attacher
{
	[Header("Particles=======================================================================================================", order = 1)]
	public bool inheritScale;

	public AttacherCurveData emission;

	public AttacherCurveData size;

	public AttacherCurveData lifetime;

	public AttacherGradientData color;

	private ParticleSystem _particles;

	private ParticleSystem.EmissionModule _emissionModule;

	private ParticleSystem.MainModule _mainModule;

	private bool _rateOverTimeEnabled;

	private bool _rateOverDistanceEnabled;

	public ParticleSystem particles
	{
		get
		{
			if (_particles == null)
			{
				_particles = GetComponent<ParticleSystem>();
				_emissionModule = _particles.emission;
				_rateOverTimeEnabled = _emissionModule.rateOverTimeMultiplier > 0f;
				_rateOverDistanceEnabled = _emissionModule.rateOverDistanceMultiplier > 0f;
				emission.initialValue = (_rateOverTimeEnabled ? _emissionModule.rateOverTimeMultiplier : _emissionModule.rateOverDistanceMultiplier);
				_mainModule = _particles.main;
				size.initialValue = _mainModule.startSizeMultiplier;
				lifetime.initialValue = _mainModule.startLifetimeMultiplier;
				color.initialValue = _mainModule.startColor.color;
			}
			return _particles;
		}
	}

	protected override float _offsetMultiplier
	{
		get
		{
			if (!inheritScale)
			{
				return 1f;
			}
			return base.transform.localScale.Average() * 0.5f;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_samplesDirty)
		{
			_OnSamplesChange();
		}
	}

	protected override void _OnSamplesChange()
	{
		if (inheritScale && (bool)attach.to && !base._isDetaching)
		{
			base.transform.localScale = attach.to.GetWorldScale().Max(0.01f);
		}
		_ = particles;
		if (emission.enabled)
		{
			if (_rateOverTimeEnabled)
			{
				_emissionModule.rateOverTimeMultiplier = emission.GetSampleValue(this);
			}
			if (_rateOverDistanceEnabled)
			{
				_emissionModule.rateOverDistanceMultiplier = emission.GetSampleValue(this);
			}
		}
		if (size.enabled)
		{
			_mainModule.startSizeMultiplier = size.GetSampleValue(this).Max(0.0001f);
		}
		if (lifetime.enabled)
		{
			_mainModule.startLifetimeMultiplier = lifetime.GetSampleValue(this).Max(0.001f);
		}
		if (color.enabled)
		{
			_mainModule.startColor = color.GetSampleValue(this);
		}
	}

	protected override bool _ShouldSignalDetachComplete()
	{
		if (base._ShouldSignalDetachComplete())
		{
			return particles.particleCount == 0;
		}
		return false;
	}

	public AttacherParticles ApplySettings(System.Random random, IAttacherParticleSettings settings, float emissionMultiplier, bool applyToChildren = true)
	{
		if (applyToChildren)
		{
			foreach (AttacherParticles item in base.gameObject.GetComponentsInChildrenPooled<AttacherParticles>())
			{
				settings.Apply(random, item, emissionMultiplier);
			}
			return this;
		}
		settings.Apply(random, this, emissionMultiplier);
		return this;
	}

	public void SetSimulationSpace(ParticleSystemSimulationSpace space)
	{
		if (_mainModule.simulationSpace != ParticleSystemSimulationSpace.Custom)
		{
			_mainModule.simulationSpace = space;
		}
	}

	public void SetSimulationSpeed(float speed)
	{
		_mainModule.simulationSpeed = speed;
	}
}
