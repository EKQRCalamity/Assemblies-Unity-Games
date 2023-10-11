using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class AttacherVFXProjectileSettings : IAttacherVFXSettings
{
	private static readonly RangeF SCALE = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF EMISSION = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF SIZE = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF LIFETIME = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF SPEED = new RangeF(1f, 1f, 0.33f, 3f);

	private static readonly RangeF CUSTOM = new RangeF(1f, 1f, 0f, 10f);

	[ProtoMember(1)]
	[UIField(tooltip = "Sets exposed float \"Scale\" of VFX Graph based on scale applied to projectile")]
	[DefaultValue(true)]
	private bool _inheritScaleFromProjectile = true;

	[ProtoMember(2)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced", tooltip = "Sets exposed float \"Scale\" of VFX Graph")]
	private RangeF _scale = SCALE;

	[ProtoMember(3)]
	[UIField(min = 0, max = 3f, stepSize = 0.01f, tooltip = "The speed (in meters per second) which is considered the top speed for sampling speed curves.")]
	[DefaultValue(1f)]
	private float _referenceSpeed = 1f;

	[ProtoMember(4)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced", tooltip = "Sets exposed float \"EmissionMultiplier\" of VFX Graph")]
	private RangeF _emissionMultiplier = EMISSION;

	[ProtoMember(5)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced", tooltip = "Sets exposed float \"SizeMultiplier\" of VFX Graph")]
	private RangeF _sizeMultiplier = SIZE;

	[ProtoMember(6)]
	[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced", tooltip = "Sets exposed float \"LifetimeMultiplier\" of VFX Graph")]
	private RangeF _lifetimeMultiplier = LIFETIME;

	[ProtoMember(7)]
	[UIField(tooltip = "Sets exposed Color \"Color\" of VFX Graph")]
	private OptionalTints _tint;

	[ProtoMember(8)]
	[UIField]
	[UIDeepValueChange]
	private OffsetRanges _positionOffset;

	[ProtoMember(9)]
	[UIField(tooltip = "Sets playRate of the VFX object, effectively scaling the delta time fed into the update of the system.")]
	private RangeF _simulationSpeed = SPEED;

	[ProtoMember(10)]
	[UIField(maxCount = 0, dynamicInitMethod = "_InitCustomPropertyMultipliers")]
	[UIFieldKey(flexibleWidth = 1f, readOnly = true, max = 32)]
	[UIFieldValue(dynamicInitMethod = "_InitCustomPropertyRange", hideInCollectionAdd = true, flexibleWidth = 2.5f)]
	[UIHideIf("_hideCustomPropertyMultipliers")]
	private Dictionary<string, RangeF> _customPropertyMultipliers;

	[ProtoMember(11)]
	[UIField(tooltip = "Indicates that emission multiplier should not be effected by particle quality option.")]
	private bool _ignoreParticleQualitySetting;

	private OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges());

	private bool _scaleSpecified => _scale != SCALE;

	private bool _emissionMultiplierSpecified => _emissionMultiplier != EMISSION;

	private bool _sizeMultiplierSpecified => _sizeMultiplier != SIZE;

	private bool _lifetimeMultiplierSpecified => _lifetimeMultiplier != LIFETIME;

	private bool _simulationSpeedSpecified => _simulationSpeed != SPEED;

	private bool _positionOffsetSpecified => _positionOffset;

	public void Apply(Random random, AttacherVFX attacherParticles, float emissionMultiplier)
	{
		attacherParticles.inheritScale = _inheritScaleFromProjectile;
		attacherParticles.scale = random.Range(_scale);
		attacherParticles.speedRange.max = _referenceSpeed;
		attacherParticles.emissionMultiplier.multiplier = random.Range(_emissionMultiplier) * ((_ignoreParticleQualitySetting || attacherParticles.emissionMultiplierIgnoresQualitySettings) ? 1f : emissionMultiplier);
		attacherParticles.sizeMultiplier.multiplier = random.Range(_sizeMultiplier);
		attacherParticles.lifetimeMultiplier.multiplier = random.Range(_lifetimeMultiplier);
		attacherParticles.color.tintColor = _tint.GetTint(random);
		attacherParticles.attach.axisOffsetDistances = positionOffset.GetOffset(random);
		attacherParticles.SetSimulationSpeed(random.Range(_simulationSpeed));
		if (!_customPropertyMultipliers.IsNullOrEmpty())
		{
			foreach (Attacher.CustomPropertyCurve customPropertyCurf in attacherParticles.customPropertyCurves)
			{
				customPropertyCurf.curves.multiplier = (_customPropertyMultipliers.ContainsKey(customPropertyCurf.propertyName) ? random.Range(_customPropertyMultipliers[customPropertyCurf.propertyName]) : customPropertyCurf.defaultValue);
			}
			return;
		}
		foreach (Attacher.CustomPropertyCurve customPropertyCurf2 in attacherParticles.customPropertyCurves)
		{
			customPropertyCurf2.curves.multiplier = customPropertyCurf2.defaultValue;
		}
	}

	private void _InitCustomPropertyRange(UIFieldAttribute uiField)
	{
		uiField.defaultValue = CUSTOM;
	}

	[ProtoAfterDeserialization]
	private void _ProtoAfterDeserialization()
	{
		if (_customPropertyMultipliers.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyValuePair<string, RangeF> item in _customPropertyMultipliers.EnumeratePairsSafe())
		{
			_customPropertyMultipliers[item.Key] = item.Value.CopyNonSerializedFieldsFrom(CUSTOM);
		}
	}
}
