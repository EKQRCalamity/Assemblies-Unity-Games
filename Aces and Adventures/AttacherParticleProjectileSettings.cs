using System;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class AttacherParticleProjectileSettings : IAttacherParticleSettings
{
	private const string CAT_ADVANCED = "Advanced";

	private static readonly RangeF EMISSION = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF SIZE = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF LIFETIME = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF SPEED = new RangeF(1f, 1f, 0.33f, 3f);

	[ProtoMember(1)]
	[UIField]
	[DefaultValue(true)]
	private bool _inheritScaleFromProjectile = true;

	[ProtoMember(2)]
	[UIField(min = 0, max = 3f, stepSize = 0.01f)]
	[DefaultValue(1f)]
	private float _referenceSpeed = 1f;

	[ProtoMember(3)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _emissionMultiplier = EMISSION;

	[ProtoMember(4)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _sizeMultiplier = SIZE;

	[ProtoMember(5)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _lifetimeMultiplier = LIFETIME;

	[ProtoMember(6)]
	[UIField]
	private OptionalTints _tint;

	[ProtoMember(7)]
	[UIField(category = "Advanced")]
	private OffsetRanges _positionOffset;

	[ProtoMember(8)]
	[UIField(category = "Advanced")]
	private bool _simulateInLocalSpace;

	[ProtoMember(9)]
	[UIField(category = "Advanced")]
	private RangeF _simulationSpeed = SPEED;

	[ProtoMember(10)]
	[UIField(tooltip = "Indicates that emission multiplier should not be effected by particle quality option.")]
	private bool _ignoreParticleQualitySetting;

	private OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges());

	private bool _emissionMultiplierSpecified => _emissionMultiplier != EMISSION;

	private bool _sizeMultiplierSpecified => _sizeMultiplier != SIZE;

	private bool _lifetimeMultiplierSpecified => _lifetimeMultiplier != LIFETIME;

	private bool _simulationSpeedSpecified => _simulationSpeed != SPEED;

	private bool _positionOffsetSpecified => _positionOffset;

	public void Apply(System.Random random, AttacherParticles attacherParticles, float emissionMultiplier)
	{
		attacherParticles.inheritScale = _inheritScaleFromProjectile;
		attacherParticles.speedRange.max = _referenceSpeed;
		attacherParticles.emission.multiplier = random.Range(_emissionMultiplier) * (_ignoreParticleQualitySetting ? 1f : emissionMultiplier);
		attacherParticles.size.multiplier = random.Range(_sizeMultiplier);
		attacherParticles.lifetime.multiplier = random.Range(_lifetimeMultiplier);
		attacherParticles.color.tintColor = _tint.GetTint(random);
		attacherParticles.attach.axisOffsetDistances = positionOffset.GetOffset(random);
		attacherParticles.SetSimulationSpace((!_simulateInLocalSpace) ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local);
		attacherParticles.SetSimulationSpeed(random.Range(_simulationSpeed));
	}
}
