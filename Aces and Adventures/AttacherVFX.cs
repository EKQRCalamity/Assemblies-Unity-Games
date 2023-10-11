using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class AttacherVFX : Attacher
{
	public const string SCALE = "Scale";

	public const string EMISSION_MULTIPLIER = "EmissionMultiplier";

	public const string SIZE_MULTIPLIER = "SizeMultiplier";

	public const string LIFETIME_MULTIPLIER = "LifetimeMultiplier";

	public const string COLOR = "Color";

	public static readonly int SCALE_ID = Shader.PropertyToID("Scale");

	public static readonly int EMISSION_MULTIPLIER_ID = Shader.PropertyToID("EmissionMultiplier");

	public static readonly int SIZE_MULTIPLIER_ID = Shader.PropertyToID("SizeMultiplier");

	public static readonly int LIFETIME_MULTIPLIER_ID = Shader.PropertyToID("LifetimeMultiplier");

	public static readonly int COLOR_ID = Shader.PropertyToID("Color");

	[Header("VFX=======================================================================================================", order = 1)]
	[Tooltip("Sets exposed float named \"Scale\" based on scale applied to Attached")]
	public bool inheritScale;

	public bool emissionMultiplierIgnoresQualitySettings;

	[Tooltip("Sets exposed float named \"EmissionMultiplier\"")]
	public AttacherCurveData emissionMultiplier;

	[Tooltip("Sets exposed float named \"SizeMultiplier\"")]
	public AttacherCurveData sizeMultiplier;

	[Tooltip("Sets exposed float named \"LifetimeMultiplier\"")]
	public AttacherCurveData lifetimeMultiplier;

	[Tooltip("Sets exposed Color named \"Color\"")]
	public AttacherGradientData color;

	[Tooltip("Allows setting custom exposed float property values based on: lifetime, speed, and detachment")]
	public List<CustomPropertyCurve> customPropertyCurves;

	private VisualEffect _vfx;

	private bool _hasEmissionMultiplier;

	private bool _hasSizeMultiplier;

	private bool _hasLifetimeMultiplier;

	private bool _hasColor;

	private bool _hasScale;

	public VisualEffect vfx
	{
		get
		{
			if (!_vfx)
			{
				this.CacheComponent(ref _vfx);
				bool flag = (_hasScale = _vfx.HasFloat(SCALE_ID));
				flag = (_hasEmissionMultiplier = _vfx.HasFloat(EMISSION_MULTIPLIER_ID));
				flag = (_hasSizeMultiplier = _vfx.HasFloat(SIZE_MULTIPLIER_ID));
				flag = (_hasLifetimeMultiplier = _vfx.HasFloat(LIFETIME_MULTIPLIER_ID));
				flag = (_hasColor = _vfx.HasVector4(COLOR_ID));
			}
			return _vfx;
		}
	}

	public float scale { get; set; } = 1f;


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

	private void Awake()
	{
		AttacherCurveData attacherCurveData = emissionMultiplier;
		AttacherCurveData attacherCurveData2 = sizeMultiplier;
		float num2 = (lifetimeMultiplier.initialValue = 1f);
		float initialValue = (attacherCurveData2.initialValue = num2);
		attacherCurveData.initialValue = initialValue;
		color.initialValue = Color.white;
		if (customPropertyCurves.IsNullOrEmpty())
		{
			return;
		}
		foreach (CustomPropertyCurve customPropertyCurf in customPropertyCurves)
		{
			customPropertyCurf.curves.initialValue = 1f;
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
		if (_hasScale)
		{
			if (inheritScale)
			{
				if ((bool)attach.to && !base._isDetaching)
				{
					vfx.SetFloat(SCALE_ID, attach.to.lossyScale.AbsMax().Max(0.0001f) * scale);
				}
			}
			else
			{
				vfx.SetFloat(SCALE_ID, scale);
			}
		}
		if (emissionMultiplier.enabled && _hasEmissionMultiplier)
		{
			vfx.SetFloat(EMISSION_MULTIPLIER_ID, emissionMultiplier.GetSampleValue(this));
		}
		if (sizeMultiplier.enabled && _hasSizeMultiplier)
		{
			vfx.SetFloat(SIZE_MULTIPLIER_ID, sizeMultiplier.GetSampleValue(this));
		}
		if (lifetimeMultiplier.enabled && _hasLifetimeMultiplier)
		{
			vfx.SetFloat(LIFETIME_MULTIPLIER_ID, lifetimeMultiplier.GetSampleValue(this));
		}
		if (color.enabled && _hasColor)
		{
			vfx.SetVector4(COLOR_ID, color.GetSampleValue(this));
		}
		if (customPropertyCurves.IsNullOrEmpty())
		{
			return;
		}
		foreach (CustomPropertyCurve customPropertyCurf in customPropertyCurves)
		{
			if (!(customPropertyCurf?.propertyName).IsNullOrEmpty() && customPropertyCurf.curves.enabled)
			{
				vfx.SetFloat(customPropertyCurf.propertyName, customPropertyCurf.curves.GetSampleValue(this));
			}
		}
	}

	protected override bool _ShouldSignalDetachComplete()
	{
		if (base._ShouldSignalDetachComplete())
		{
			return vfx.aliveParticleCount == 0;
		}
		return false;
	}

	public AttacherVFX ApplySettings(System.Random random, IAttacherVFXSettings settings, float emitMultiplier, bool applyToChildren = true)
	{
		if (applyToChildren)
		{
			foreach (AttacherVFX item in base.gameObject.GetComponentsInChildrenPooled<AttacherVFX>())
			{
				settings.Apply(random, item, emitMultiplier);
			}
			return this;
		}
		settings.Apply(random, this, emitMultiplier);
		return this;
	}

	public void SetSimulationSpeed(float speed)
	{
		vfx.playRate = speed;
	}
}
