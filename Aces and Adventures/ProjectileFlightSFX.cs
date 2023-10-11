using System;
using UnityEngine;

[RequireComponent(typeof(CustomizableEnumContent))]
public class ProjectileFlightSFX : MonoBehaviour
{
	private const float MIN_SCALE = 0.01f;

	private static GameObject _Blueprint;

	public Transform _emitterTransform;

	public bool useCustomFadeOut;

	[Range(0f, 10f, order = 1)]
	[Tooltip("Additional time that projectile should stay active after fade out has completed.")]
	[HideInInspectorIf("_hideFade", false, order = 0)]
	public float additionalFadeLifetime = 3f;

	[Tooltip("Goes from 1 (fully alive) to 0 (fully faded out)")]
	[HideInInspectorIf("_hideFade", false)]
	public FloatEvent onFadeAmountChange;

	[Tooltip("Fade amount will be affected by Particle Quality emission multiplier.")]
	[HideInInspectorIf("_hideFade", false)]
	public bool multiplyFadeAmountByEmissionMultiplier;

	public Action<ProjectileFlightSFX> onFinish;

	private Transform _container;

	protected Transform _innerTransform;

	protected Transform _impactTransform;

	private Transform _frontTransform;

	private Transform _backTransform;

	private Transform _launchTransform;

	private Transform _target;

	private ProjectileMediaData _data;

	private ProjectileMediaView _view;

	private AttacherTrail _trail;

	private System.Random _random;

	private PoolListHandle<ProjectileMediaData.ProjectileFlight.ProjectileFlightParameters.FlightModifier> _modifiers;

	private ProjectileBoomerangOptions? _boomerangOptions;

	private bool _inFlight;

	private bool _hasImpacted;

	private float _timeOfCreation;

	private float _lifetime;

	private float _lifetimeAfterImpact;

	protected float _defaultRadius;

	private float _radius;

	private Vector3 _previousPosition;

	private Vector3 _initialRotation;

	private Vector3 _rotationVelocity;

	private float _startScale;

	private float _endScale;

	private bool _isFading;

	private byte _mediaIndex;

	private byte _visualIndex;

	private byte _projectileIndex;

	private byte _afterImpactIndex;

	private bool _hasUpdatedRotation;

	private Color _primaryColor;

	private float? _timeOfImpact;

	private PositionRotation _relativePositionRotation;

	private PositionRotation _impactAt;

	private PositionRotation _relativePositionRotationLaunch;

	private PositionRotation _launchFrom;

	[NonSerialized]
	public Vector3 velocity;

	private Vector3 _previousUpdatePosition;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("Gameplay/Ability/Projectiles/ProjectileFlightSFX");
			}
			return _Blueprint;
		}
	}

	public ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileVisual visual => media.visuals[_visualIndex];

	public ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia media => _data.flight.media[_mediaIndex];

	public ProjectileMediaData.ProjectileAfterImpactParameters afterImpactParameters => _data.afterImpact.parameters[_afterImpactIndex];

	public Transform impactTransform
	{
		get
		{
			if (!_impactTransform)
			{
				GameObject gameObject = new GameObject();
				_impactTransform = gameObject.transform;
				_impactTransform.SetParent(_innerTransform, worldPositionStays: false);
				_impactTransform.position = base.transform.position;
				_impactTransform.rotation = base.transform.rotation;
			}
			_impactTransform.gameObject.SetActive(value: true);
			return _impactTransform;
		}
	}

	public Transform emitterTransform
	{
		get
		{
			if (!_emitterTransform)
			{
				GameObject gameObject = new GameObject();
				_emitterTransform = gameObject.transform;
				_emitterTransform.SetParent(_innerTransform, worldPositionStays: false);
				_emitterTransform.position = _innerTransform.position + (_innerTransform.position - base.transform.position);
				_emitterTransform.rotation = base.transform.rotation.Opposite();
			}
			_emitterTransform.gameObject.SetActive(value: true);
			return _emitterTransform;
		}
	}

	public Transform frontTransform
	{
		get
		{
			if (!_frontTransform)
			{
				GameObject gameObject = new GameObject();
				_frontTransform = gameObject.transform;
				_frontTransform.SetParent(_innerTransform, worldPositionStays: false);
				_frontTransform.localPosition = new Vector3(0f, 0f, _defaultRadius * MathUtil.SqrtTwo);
			}
			_frontTransform.gameObject.SetActive(value: true);
			return _frontTransform;
		}
	}

	public Transform backTransform
	{
		get
		{
			if (!_backTransform)
			{
				GameObject gameObject = new GameObject();
				_backTransform = gameObject.transform;
				_backTransform.SetParent(_innerTransform, worldPositionStays: false);
				_backTransform.localPosition = new Vector3(0f, 0f, _defaultRadius * (0f - MathUtil.SqrtTwo));
				_backTransform.forward = -_innerTransform.forward;
			}
			_backTransform.gameObject.SetActive(value: true);
			return _backTransform;
		}
	}

	public float elapsedTime => Time.time - _timeOfCreation;

	public float lifetime
	{
		get
		{
			if (_boomerangs)
			{
				return _lifetime + _lifetime;
			}
			return _lifetime;
		}
	}

	public float lifetimeAfterImpact => _lifetimeAfterImpact + afterImpactParameters.fadeTime;

	public bool isFading => _isFading;

	public ProjectileMediaData data => _data;

	public PositionRotation relativePositionRotation
	{
		get
		{
			if (_boomeranging)
			{
				return _relativePositionRotationLaunch;
			}
			return _relativePositionRotation;
		}
	}

	public Transform target
	{
		get
		{
			if (_boomeranging)
			{
				return _launchTransform;
			}
			return _target;
		}
	}

	public Transform innerTransform => _innerTransform;

	protected Vector3 _velocity => (base.transform.position - _previousPosition) / Time.deltaTime.InsureNonZero();

	public float radius => _radius;

	protected float _scaleLerp => Mathf.Clamp01(MathUtil.GetLerpAmount(visual.transforms.minScale, visual.transforms.maxScale, base.transform.localScale.Average()));

	protected bool _boomerangs => _boomerangOptions.HasValue;

	protected bool _boomeranging
	{
		get
		{
			if (_boomerangs)
			{
				return _hasImpacted;
			}
			return false;
		}
	}

	private bool _hideFade => !useCustomFadeOut;

	private void _UpdateRotation(ProjectileUpdateData updateData)
	{
		ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileFlightMediaTransforms transforms = visual.transforms;
		bool flipOrientTo = transforms.flipOrientTo;
		Quaternion? quaternion = null;
		switch (transforms.orientTo)
		{
		case ProjectileFlightOrientType.Velocity:
			if (velocity != Vector3.zero)
			{
				quaternion = Quaternion.LookRotation(velocity).Opposite(flipOrientTo);
			}
			break;
		case ProjectileFlightOrientType.Target:
			if (updateData.end.HasValue && updateData.end.Value != base.transform.position)
			{
				quaternion = Quaternion.LookRotation(updateData.end.Value - base.transform.position).Opposite(flipOrientTo);
			}
			break;
		case ProjectileFlightOrientType.Camera:
			quaternion = base.transform.GetBillboardRotation(CameraManager.Instance.mainCamera.transform).Opposite(flipOrientTo);
			break;
		case ProjectileFlightOrientType.ImpactShape:
			quaternion = base.transform.rotation.Opposite(flipOrientTo).SlerpUnclamped(_data.impact.parameters.trackTarget ? (_target.rotation * _relativePositionRotation.rotation) : _impactAt.rotation, updateData.elapsedTime / _lifetime).Opposite(flipOrientTo);
			if (_boomeranging)
			{
				quaternion = quaternion.Value.Opposite(flipOrientTo).SlerpUnclamped(_data.impact.parameters.trackTarget ? (_launchTransform.rotation * _relativePositionRotationLaunch.rotation) : _launchFrom.rotation, 1f - updateData.elapsedTime / _lifetime).Opposite(flipOrientTo);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ProjectileFlightOrientType.LaunchDirection:
			break;
		}
		if (quaternion.HasValue)
		{
			base.transform.rotation = ((transforms.easeOrientTo.HasValue && _hasUpdatedRotation) ? Quaternion.Slerp(base.transform.rotation, quaternion.Value, MathUtil.CalculateEaseStiffnessSubjectToTime(transforms.easeOrientTo.Value, Time.deltaTime)) : quaternion.Value);
		}
		if (_rotationVelocity != Vector3.zero)
		{
			_innerTransform.localRotation = Quaternion.Euler(_initialRotation + _rotationVelocity * ((!_boomerangs || EnumUtil.HasFlag(_boomerangOptions.Value, ProjectileBoomerangOptions.ReverseRotationSpeed)) ? updateData.elapsedTime : elapsedTime));
		}
		_hasUpdatedRotation |= quaternion.HasValue;
	}

	private Collider _GetBoundingCollider()
	{
		Collider collider = base.gameObject.GetComponentInChildren<Collider>();
		if (!collider)
		{
			Renderer componentInChildren = base.gameObject.GetComponentInChildren<Renderer>();
			collider = (componentInChildren ? componentInChildren.gameObject.AddComponent<BoxCollider>() : base.gameObject.AddComponent<BoxCollider>());
		}
		collider.isTrigger = true;
		return collider;
	}

	private float _GetLifetime(ProjectileLifetimeType lifetimeType)
	{
		return lifetimeType switch
		{
			ProjectileLifetimeType.Impact => _lifetime, 
			ProjectileLifetimeType.BeginFadingOut => lifetime + _lifetimeAfterImpact, 
			ProjectileLifetimeType.FinishedFadingOut => lifetime + lifetimeAfterImpact, 
			_ => throw new ArgumentOutOfRangeException("lifetimeType", lifetimeType, null), 
		};
	}

	private Vector3 _GetPositionAtTime(float time)
	{
		ProjectileUpdateData update = new ProjectileUpdateData(time);
		foreach (ProjectileMediaData.ProjectileFlight.ProjectileFlightParameters.FlightModifier item in _modifiers.value)
		{
			item.Update(ref update);
		}
		return update.position;
	}

	private Vector3 _GetVelocityAtTime(float time, float deltaTime = 0.01f)
	{
		return (_GetPositionAtTime(time + deltaTime) - _GetPositionAtTime(time)) / deltaTime;
	}

	private bool _ShouldPlay(float playAmountRatio)
	{
		return (int)_projectileIndex % Mathf.RoundToInt(1f / playAmountRatio.InsureNonZero()) == 0;
	}

	private void _PlayImpactMedia(ProjectileMediaData.ProjectileBurstMedia<ProjectileImpactSFXType> media, Vector3? velocityOverride = null)
	{
		if (!_ShouldPlay(media.playAmountRatio))
		{
			return;
		}
		ProjectileMediaData.ProjectileBurstMedia<ProjectileImpactSFXType>.BurstMedia burstMedia = media.GetBurstMedia(_random, _mediaIndex, data.flight.media.Count);
		if (burstMedia != null)
		{
			bool playedSound = burstMedia.shouldPlaySound && burstMedia.PlaySound(_random, base.transform.position, MasterMixManager.SoundEffects, PooledAudioCategory.ProjectileImpact, _scaleLerp);
			if (burstMedia.visual.sfx.HasValue)
			{
				PositionRotation shape = new PositionRotation(base.transform.position, _data.impact.parameters.trackTarget ? (target.rotation * relativePositionRotation.rotation) : _impactAt.rotation);
				GameObject gameObject = Pools.Unpool(EnumUtil.GetResourceBlueprint(burstMedia.visual.sfx.Value), burstMedia.visual.translation.GetPosition(_random, base.transform, _target), burstMedia.visual.rotation.GetRotation(_random, base.transform, shape, target, velocityOverride ?? velocity), _container);
				gameObject.transform.localScale = burstMedia.visual.scale.GetScale(_random, base.transform);
				burstMedia.visual.ApplyCustomization(gameObject, playedSound, _ShouldPlay(burstMedia.visual.lighting.playAmountRatio), target, _random, _primaryColor, _view.emissionMultiplier);
			}
		}
	}

	private void _SignalFinished()
	{
		if (onFinish != null)
		{
			onFinish(this);
			onFinish = null;
		}
	}

	private void Awake()
	{
		GameObject gameObject = new GameObject();
		_innerTransform = gameObject.transform;
		PoolKeepItemListHandle<Transform> poolKeepItemListHandle = base.transform.ChildrenSafe();
		_innerTransform.SetParent(base.transform, worldPositionStays: false);
		Collider collider = _GetBoundingCollider();
		_defaultRadius = collider.bounds.extents.AbsMax();
		_innerTransform.transform.position = collider.bounds.center;
		foreach (Transform item in poolKeepItemListHandle)
		{
			item.SetParent(_innerTransform, worldPositionStays: true);
		}
		UnityEngine.Object.Destroy(collider);
	}

	protected virtual void OnDisable()
	{
		_container = null;
		_launchTransform = null;
		_target = null;
		_data = null;
		_trail = null;
		_random = null;
		Pools.Repool(ref _modifiers);
		_timeOfImpact = null;
		_hasUpdatedRotation = false;
		_backTransform.SetActiveSafe(active: false);
		_frontTransform.SetActiveSafe(active: false);
		_emitterTransform.SetActiveSafe(active: false);
		_impactTransform.SetActiveSafe(active: false);
		_SignalFinished();
		_view.activeProjectileCount--;
		_view = null;
	}

	public void Update()
	{
		ProjectileUpdateData update = new ProjectileUpdateData(elapsedTime);
		bool inFlight = _inFlight;
		ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileFlightMediaTransforms transforms = visual.transforms;
		if (inFlight)
		{
			foreach (ProjectileMediaData.ProjectileFlight.ProjectileFlightParameters.FlightModifier item in _modifiers.value)
			{
				item.Update(ref update);
			}
			_inFlight = update.inFlight;
			float num = update.elapsedTime / _lifetime;
			base.transform.position = update.position;
			if (!_boomeranging)
			{
				base.transform.localScale = Math.Max(0.01f, transforms.scaleTransition.Ease(_startScale, _endScale, num) * Mathf.Clamp01(update.linearDistanceTraveled / _radius)).ToVector3();
			}
			if (_data.impact.parameters.trackTarget && (bool)_target)
			{
				base.transform.position += Vector3.Lerp(_impactAt.position, _target.TransformPoint(_relativePositionRotation.position), num) - _impactAt.position;
				if (_boomeranging)
				{
					base.transform.position += Vector3.Lerp(_launchFrom.position, _launchTransform.TransformPoint(_relativePositionRotationLaunch.position), 1f - num) - _launchFrom.position;
				}
			}
			velocity = (update.position - _previousUpdatePosition) / Time.deltaTime.InsureNonZero();
		}
		if (inFlight || transforms.orientTo == ProjectileFlightOrientType.Camera)
		{
			_UpdateRotation(update);
		}
		if (inFlight)
		{
			if (_view.finishedAt == ProjectilesFinishedAt.Launched)
			{
				_SignalFinished();
			}
			if (!_hasImpacted && update.impacted)
			{
				if (_view.finishedAt == ProjectilesFinishedAt.Impact)
				{
					_SignalFinished();
				}
				if (_view.createImpactMedia)
				{
					_PlayImpactMedia(_data.impact.media);
				}
				_hasImpacted = true;
			}
			if (update.finished)
			{
				_timeOfImpact = Time.time - elapsedTime % lifetime;
				if (afterImpactParameters.animated)
				{
					ProjectileUpdateData update2 = new ProjectileUpdateData(lifetime - MathUtil.DiscretePhysicsRate);
					ProjectileUpdateData update3 = new ProjectileUpdateData(lifetime);
					foreach (ProjectileMediaData.ProjectileFlight.ProjectileFlightParameters.FlightModifier item2 in _modifiers.value)
					{
						item2.Update(ref update2);
						item2.Update(ref update3);
					}
					velocity = (update3.position - update2.position) / MathUtil.DiscretePhysicsRate;
					Pools.Unpool(ProjectileImpactAnimator.Blueprint, _container).GetComponent<ProjectileImpactAnimator>().SetData(_random, this, target, Math.Max(0f, elapsedTime - lifetime));
				}
			}
		}
		else
		{
			float num2 = Time.time - _timeOfImpact.Value - _lifetimeAfterImpact;
			float fadeTime = afterImpactParameters.fadeTime;
			if (SetPropertyUtility.SetStruct(ref _isFading, num2 >= fadeTime))
			{
				_PlayImpactMedia(_data.afterImpact.media, _velocity);
				_SignalFinished();
			}
			if (_isFading)
			{
				float num3 = ((fadeTime <= 0f) ? 0f : (1f - Mathf.Clamp01((num2 - fadeTime) / fadeTime)));
				if (useCustomFadeOut)
				{
					onFadeAmountChange?.Invoke(num3 * multiplyFadeAmountByEmissionMultiplier.ToFloat(ProjectileMediaView.EmissionMultiplier, 1f));
				}
				else
				{
					base.transform.localScale = (base.transform.localScale * num3).Max(0.01f);
				}
				if (num3 <= 0f && (!useCustomFadeOut || Time.time >= _timeOfImpact.Value + _lifetimeAfterImpact + fadeTime + additionalFadeLifetime))
				{
					base.gameObject.SetActive(value: false);
					return;
				}
			}
		}
		if (!_isFading && transforms.animateScale)
		{
			base.transform.localScale = (inFlight ? base.transform.localScale : _endScale.ToVector3()).Multiply(transforms.GetScaleAnimation(update.elapsedTime)).Max(0.01f);
		}
		_previousPosition = base.transform.position;
		if (inFlight && !update.finished)
		{
			_previousUpdatePosition = update.position;
		}
	}

	public void LateUpdate()
	{
		if ((bool)_trail)
		{
			if (!_trail.isActiveAndEnabled)
			{
				_trail = null;
			}
			else if (!_isFading)
			{
				_trail.widthMultiplier = _defaultRadius * base.transform.localScale.Average() * 2f;
			}
		}
	}

	public ProjectileFlightSFX SetData(System.Random random, ProjectileMediaView view, ProjectileMediaData data, Transform launchTransform, Transform target, PositionRotation launchFrom, PositionRotation impactAt, Vector3 up, float elapsedTime, int mediaIndex, int visualIndex, int projectileIndex)
	{
		if (useCustomFadeOut)
		{
			onFadeAmountChange?.Invoke(multiplyFadeAmountByEmissionMultiplier.ToFloat(ProjectileMediaView.EmissionMultiplier, 1f));
		}
		_isFading = false;
		_inFlight = true;
		_hasImpacted = false;
		_container = base.transform.parent;
		_previousPosition = base.transform.position;
		_previousUpdatePosition = base.transform.position;
		innerTransform.localScale = Vector3.one;
		_mediaIndex = (byte)mediaIndex;
		_visualIndex = (byte)visualIndex;
		_projectileIndex = (byte)projectileIndex;
		_afterImpactIndex = (byte)((data.afterImpact.syncWithFlightMedia && data.afterImpact.parameters.Count == data.flight.media.Count) ? _mediaIndex : random.RangeInt(0, data.afterImpact.parameters.Count, inclusiveMax: false));
		_launchTransform = launchTransform;
		_target = target;
		_launchFrom = launchFrom;
		_impactAt = impactAt;
		_relativePositionRotationLaunch = new PositionRotation(_launchTransform.InverseTransformPoint(_launchFrom.position), _launchTransform.rotation.Inverse() * _launchFrom.rotation);
		_relativePositionRotation = new PositionRotation(_target.InverseTransformPoint(_impactAt.position), _target.rotation.Inverse() * _impactAt.rotation);
		_data = data;
		_random = random;
		_timeOfCreation = Time.time - elapsedTime;
		ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileFlightMediaTransforms transforms = visual.transforms;
		_startScale = random.Range(transforms.startScale);
		_endScale = (transforms.syncScale ? transforms.endScale.Lerp(transforms.startScale.GetLerpAmount(_startScale)) : random.Range(transforms.endScale));
		base.transform.localScale = new Vector3(_startScale, _startScale, _startScale);
		_radius = Math.Max(0.01f, _defaultRadius * ((_startScale + _endScale) * 0.5f));
		_modifiers = data.flight.parameters[data.flight.syncParameterAndMediaIndex ? mediaIndex : random.Next(0, data.flight.parameters.Count)].GetModifiers(random, launchFrom.position, _impactAt.position, up, _radius, out _lifetime, out _boomerangOptions);
		_lifetimeAfterImpact = random.Range(afterImpactParameters.lifetime);
		_primaryColor = visual.ApplySettings(base.gameObject, _random) ?? Color.white;
		float emissionMultiplier = view.emissionMultiplier;
		ProjectileMediaData.ProjectileBurstMedia<ProjectileLaunchSFXType>.BurstMedia burstMedia = data.launch.media.GetBurstMedia(random, mediaIndex, data.flight.media.Count);
		if (burstMedia != null && _ShouldPlay(data.launch.media.playAmountRatio))
		{
			bool playedSound = burstMedia.shouldPlaySound && burstMedia.PlaySound(random, launchFrom.position, MasterMixManager.SoundEffects, PooledAudioCategory.ProjectileLaunch, _scaleLerp);
			if (burstMedia.visual.sfx.HasValue)
			{
				Vector3 vector = ((burstMedia.visual.rotation.orientTo == ProjectileBurstVisualOrientType.Velocity) ? _GetVelocityAtTime(0f) : Vector3.forward);
				GameObject gameObject = Pools.Unpool(EnumUtil.GetResourceBlueprint(burstMedia.visual.sfx.Value), burstMedia.visual.translation.GetPosition(random, base.transform, target), burstMedia.visual.rotation.GetRotation(random, base.transform, launchFrom, impactAt, vector), _container);
				gameObject.transform.localScale = burstMedia.visual.scale.GetScale(random, base.transform);
				burstMedia.visual.ApplyCustomization(gameObject, playedSound, _ShouldPlay(burstMedia.visual.lighting.playAmountRatio), launchTransform, _random, _primaryColor, emissionMultiplier);
			}
		}
		if (media.audio.soundLoop.HasValue && _ShouldPlay(media.audio.playAmount) && AudioPool.Instance.ShouldPlay(PooledAudioCategory.ProjectileLoop, null, launchFrom.position))
		{
			Transform transform = media.audio.emitFrom.GetTransform(this);
			Pools.Unpool(EnumUtil.GetResourceBlueprint(media.audio.soundLoop.Value), transform.position).GetComponent<ProjectileFlightAudio>().ApplySettings(random, media.audio.settings)
				.Attach(transform, _GetLifetime(media.audio.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true);
		}
		foreach (ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileParticles particle in media.particles)
		{
			if (particle.emitter.HasValue)
			{
				Transform transform2 = particle.emitFrom.GetTransform(this);
				Pools.Unpool(EnumUtil.GetResourceBlueprint(particle.emitter.Value), transform2.position, transform2.rotation).GetComponent<ProjectileFlightEmitter>().ApplySettings(random, particle.settings, emissionMultiplier)
					.Attach(transform2, _GetLifetime(particle.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true);
			}
		}
		foreach (ProjectileMediaData.ProjectileFlight.ProjectileFlightMedia.ProjectileVFX item in media.vfx)
		{
			if (item.vfx.HasValue)
			{
				Transform transform3 = item.emitFrom.GetTransform(this);
				Pools.Unpool(EnumUtil.GetResourceBlueprint(item.vfx.Value), transform3.position, transform3.rotation).GetComponent<ProjectileFlightVFX>().ApplySettings(random, item.settings, emissionMultiplier)
					.Attach(transform3, _GetLifetime(item.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true);
			}
		}
		if (media.trail.trail.HasValue)
		{
			Transform transform4 = media.trail.emitFrom.GetTransform(this);
			_trail = Pools.Unpool(EnumUtil.GetResourceBlueprint(media.trail.trail.Value), transform4.position, transform4.rotation).GetComponent<ProjectileFlightTrail>().ApplySettings(random, media.trail.settings)
				.Attach(transform4, _GetLifetime(media.trail.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true) as AttacherTrail;
		}
		if (media.rotationTrail.trail.HasValue)
		{
			Pools.Unpool(EnumUtil.GetResourceBlueprint(media.rotationTrail.trail.Value), innerTransform.position, innerTransform.rotation).GetComponent<ProjectileRotationTrail>().ApplySettings(random, media.rotationTrail.settings)
				.AttachAll(frontTransform, backTransform, _GetLifetime(media.rotationTrail.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true);
		}
		if (view.CanUseFlightLight(media.lighting.light.HasValue))
		{
			Transform transform5 = media.lighting.emitFrom.GetTransform(this);
			AttacherLight attacherLight = Pools.Unpool(EnumUtil.GetResourceBlueprint(media.lighting.light.Value), transform5.position, transform5.rotation).GetComponent<ProjectileFlightLight>().ApplySettings(random, media.lighting.settings)
				.Attach(transform5, _GetLifetime(media.lighting.activeUntil), deactivateOnDetach: true, clearOnDisable: true, setChildrenAttacherLifetimes: true, true) as AttacherLight;
			if (media.lighting.settings.inheritColor)
			{
				attacherLight.color.tintColor = _primaryColor;
			}
		}
		_initialRotation = new Vector3(random.RangeInt(transforms.xRotation), random.RangeInt(transforms.yRotation) + ((transforms.orientTo == ProjectileFlightOrientType.Camera) ? 90 : 0), random.RangeInt(transforms.zRotation));
		_innerTransform.localRotation = Quaternion.Euler(_initialRotation);
		float rotationSpeedMultiplier = transforms.GetRotationSpeedMultiplier(_lifetime);
		bool roundRotationSpeed = transforms.roundRotationSpeed;
		_rotationVelocity = new Vector3(random.Range(transforms.xRotationSpeed).RoundIf(roundRotationSpeed) * rotationSpeedMultiplier, random.Range(transforms.yRotationSpeed).RoundIf(roundRotationSpeed) * rotationSpeedMultiplier, random.Range(transforms.zRotationSpeed).RoundIf(roundRotationSpeed) * rotationSpeedMultiplier);
		_view = view;
		_view.activeProjectileCount++;
		return this;
	}
}
