using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(CustomizableEnumContent))]
public class ProjectileBurstSFX : MonoBehaviour
{
	public struct ParticleEmissions
	{
		public readonly ParticleSystem.EmissionModule emission;

		public readonly float rateOverTimeMultiplier;

		public readonly float rateOverDistanceMultiplier;

		public ParticleEmissions(ParticleSystem system)
		{
			emission = system.emission;
			rateOverTimeMultiplier = emission.rateOverTimeMultiplier;
			rateOverDistanceMultiplier = emission.rateOverDistanceMultiplier;
		}
	}

	public struct VFXEmissions
	{
		public readonly VisualEffect vfx;

		public readonly float emissionMultiplier;

		public VFXEmissions(VisualEffect vfx)
		{
			this.vfx = vfx;
			emissionMultiplier = vfx.GetFloat(AttacherVFX.EMISSION_MULTIPLIER_ID);
		}
	}

	private static readonly List<Light> ActiveLights = new List<Light>();

	[Range(0.1f, 10f)]
	public float lifetime = 1f;

	public bool attachToTransform;

	public bool ignoreParticleQualitySetting;

	[SerializeField]
	[HideInInspector]
	public Light mainLight;

	private float _elapsedTime;

	private bool _mainLightWasEnabled;

	private float _initialLightRange;

	private Transform _attachTo;

	private Vector3 _attachToOffset;

	private Quaternion _attachToRotation;

	private List<ParticleEmissions> _particleSystems;

	private List<VFXEmissions> _vfx;

	public Transform attachTo
	{
		get
		{
			return _attachTo;
		}
		set
		{
			_attachTo = value;
			if ((bool)value)
			{
				_attachToOffset = value.InverseTransformPoint(base.transform.position);
				_attachToRotation = value.rotation.Inverse() * base.transform.rotation;
			}
		}
	}

	private static void _FadeLight(Light light)
	{
		if ((bool)light)
		{
			light.gameObject.GetOrAddComponent<LightFader>().enabled = true;
		}
	}

	private void _DisableMainLight()
	{
		if ((bool)mainLight && _mainLightWasEnabled)
		{
			mainLight.enabled = false;
			_mainLightWasEnabled = false;
			mainLight.gameObject.GetOrAddComponent<LightFader>().enabled = false;
			ActiveLights.Remove(mainLight);
		}
	}

	private void Awake()
	{
		if ((bool)mainLight)
		{
			_initialLightRange = mainLight.range;
		}
		if (ignoreParticleQualitySetting)
		{
			return;
		}
		_particleSystems = new List<ParticleEmissions>();
		foreach (ParticleSystem item in base.gameObject.GetComponentsInChildrenPooled<ParticleSystem>(includeInactive: true))
		{
			_particleSystems.Add(new ParticleEmissions(item));
		}
		if (_particleSystems.Count == 0)
		{
			_particleSystems = null;
		}
		_vfx = new List<VFXEmissions>();
		foreach (VisualEffect item2 in base.gameObject.GetComponentsInChildrenPooled<VisualEffect>(includeInactive: true))
		{
			if (item2.HasFloat(AttacherVFX.EMISSION_MULTIPLIER_ID))
			{
				_vfx.Add(new VFXEmissions(item2));
			}
		}
		if (_vfx.Count == 0)
		{
			_vfx = null;
		}
	}

	private void OnEnable()
	{
		if (!mainLight)
		{
			return;
		}
		mainLight.range = _initialLightRange;
		_mainLightWasEnabled = true;
		mainLight.enabled = true;
		ActiveLights.Add(mainLight);
		if (ActiveLights.Count > ProfileManager.options.video.quality.maxProjectileBurstLightCount)
		{
			_FadeLight(ActiveLights[0]);
			if (!ActiveLights[0].enabled)
			{
				ActiveLights.RemoveAt(0);
			}
		}
	}

	private void Update()
	{
		if ((bool)attachTo)
		{
			base.transform.position = attachTo.TransformPoint(_attachToOffset);
			base.transform.rotation = attachTo.rotation * _attachToRotation;
		}
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= lifetime)
		{
			base.gameObject.SetActive(value: false);
		}
		else if ((bool)mainLight && !mainLight.enabled)
		{
			_DisableMainLight();
		}
	}

	private void OnDisable()
	{
		_elapsedTime = 0f;
		_attachTo = null;
		_DisableMainLight();
	}

	public void SetEmissionRates(float emissionMultiplier)
	{
		if (_particleSystems != null)
		{
			foreach (ParticleEmissions particleSystem in _particleSystems)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.rateOverTimeMultiplier = emissionMultiplier * particleSystem.rateOverTimeMultiplier;
				emission.rateOverDistanceMultiplier = emissionMultiplier * particleSystem.rateOverDistanceMultiplier;
			}
		}
		if (_vfx == null)
		{
			return;
		}
		foreach (VFXEmissions item in _vfx)
		{
			item.vfx.SetFloat(AttacherVFX.EMISSION_MULTIPLIER_ID, emissionMultiplier * item.emissionMultiplier);
		}
	}
}
