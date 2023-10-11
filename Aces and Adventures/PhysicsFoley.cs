using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsFoley : MonoBehaviour
{
	private static System.Random _random;

	public PhysicMaterial reactOnlyTo;

	[SerializeField]
	protected PhysicsFoleySoundPack _soundPack;

	[Range(0f, 2f)]
	public float volumeMultiplier = 1f;

	[Range(0.1f, 10f)]
	public float pitchMultiplier = 1f;

	[Range(1f, 100f)]
	public float soundDistance = 10f;

	[Range(0f, 0.5f)]
	public float playVolumeThreshold = 0.05f;

	[Range(0.1f, 10f)]
	public float minMaxThresholdScale = 1f;

	[Header("Impact")]
	public bool playImpactSounds = true;

	[Range(0f, 100f)]
	public float impactThresholdMin = 1f;

	[Range(0f, 100f)]
	public float impactThresholdMax = 10f;

	[Header("Slide")]
	public bool playSlideSounds = true;

	[Range(0f, 100f)]
	public float slideThresholdMin = 1f;

	[Range(0f, 100f)]
	public float slideThresholdMax = 10f;

	[Header("Roll")]
	public bool playRollSounds = true;

	[Range(0f, 100f)]
	public float rollThresholdMin = 1f;

	[Range(0f, 100f)]
	public float rollThresholdMax = 10f;

	[Header("Drag (Air)")]
	public bool playDragSounds = true;

	public bool playDragSoundsWhileSliding;

	[Range(0f, 100f)]
	public float dragThresholdMin = 1f;

	[Range(0f, 100f)]
	public float dragThresholdMax = 10f;

	private PooledAudioSource _slideSound;

	private Vector2 _slideSoundInitialValues;

	private PooledAudioSource _rollSound;

	private Vector2 _rollSoundInitialValues;

	private PooledAudioSource _dragSound;

	private Vector2 _dragSoundInitialValues;

	private bool _currentlySliding;

	private bool? _sliding;

	private Vector3 _previousPosition;

	private Rigidbody _body;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	public Rigidbody body => this.CacheComponent(ref _body);

	public PhysicsFoleySoundPack soundPack
	{
		get
		{
			return _soundPack;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _soundPack, value))
			{
				_SetDirty();
			}
		}
	}

	private bool canPlaySounds
	{
		get
		{
			if (base.enabled)
			{
				return soundPack;
			}
			return false;
		}
	}

	private void _SetDirty()
	{
		if ((bool)_slideSound)
		{
			_slideSound = _slideSound.StopAndClear();
		}
		if ((bool)_rollSound)
		{
			_rollSound = _rollSound.StopAndClear();
		}
		if ((bool)_dragSound)
		{
			_dragSound = _dragSound.StopAndClear();
		}
	}

	private void _PlayLooped(bool shouldPlay, float min, float max, float current, ref PooledAudioSource source, ref Vector2 initialValues, PooledAudioPack pack, AnimationCurve volumeCurve, AnimationCurve pitchCurve)
	{
		if (!shouldPlay || !canPlaySounds || pack.IsNullOrEmpty())
		{
			return;
		}
		float lerpAmountClamped = MathUtil.GetLerpAmountClamped(min * minMaxThresholdScale, max * minMaxThresholdScale, current);
		float num = volumeCurve.Evaluate(lerpAmountClamped) * volumeMultiplier;
		if (num >= playVolumeThreshold && !source)
		{
			source = pack.PlaySafe(base.transform, _Random, soundPack.mixerGroup, loop: true, 0f, 1f, 0f, soundDistance);
			initialValues = new Vector2(source.volume, source.source.pitch);
		}
		if ((bool)source)
		{
			source.volume = num * initialValues.x;
			source.source.pitch = pitchCurve.Evaluate(lerpAmountClamped) * initialValues.y * pitchMultiplier;
			if (num < playVolumeThreshold)
			{
				source = source.StopAndClear();
			}
		}
	}

	private bool _IsValid(Collision collision)
	{
		if ((bool)reactOnlyTo)
		{
			return collision.collider.sharedMaterial == reactOnlyTo;
		}
		return true;
	}

	private void Start()
	{
		_previousPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		_currentlySliding = false;
	}

	private void Update()
	{
		if (SetPropertyUtility.SetStruct(ref _sliding, _currentlySliding) && !_currentlySliding)
		{
			if ((bool)_slideSound)
			{
				_slideSound = _slideSound.StopAndClear();
			}
			if ((bool)_rollSound)
			{
				_rollSound = _rollSound.StopAndClear();
			}
		}
		if (!playDragSoundsWhileSliding && _currentlySliding)
		{
			_dragSound = (_dragSound ? _dragSound.StopAndClear() : null);
		}
		else
		{
			_PlayLooped(playDragSounds, dragThresholdMin, dragThresholdMax, (base.transform.position - _previousPosition).magnitude / Time.deltaTime.InsureNonZero(), ref _dragSound, ref _dragSoundInitialValues, soundPack.dragSounds, soundPack.dragVolumeCurve, soundPack.dragPitchCurve);
		}
		_previousPosition = base.transform.position;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (playImpactSounds && canPlaySounds && !soundPack.impacts.IsNullOrEmpty() && _IsValid(collision))
		{
			float lerpAmountClamped = MathUtil.GetLerpAmountClamped(impactThresholdMin * minMaxThresholdScale, impactThresholdMax * minMaxThresholdScale, collision.relativeVelocity.magnitude);
			PooledAudioSource pooledAudioSource = soundPack.impacts.PlaySafe(collision.contacts[0].point, _Random, soundPack.mixerGroup, loop: false, 0f, 1f, 0f, createVolumeThreshold: playVolumeThreshold, volumeMultiplier: volumeMultiplier * soundPack.impactVolumeCurve.Evaluate(lerpAmountClamped), maxDistance: soundDistance);
			if ((bool)pooledAudioSource)
			{
				pooledAudioSource.source.pitch *= soundPack.impactPitchCurve.Evaluate(lerpAmountClamped) * pitchMultiplier;
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (_IsValid(collision))
		{
			_PlayLooped(playSlideSounds, slideThresholdMin, slideThresholdMax, collision.relativeVelocity.magnitude, ref _slideSound, ref _slideSoundInitialValues, soundPack.slideSounds, soundPack.slideVolumeCurve, soundPack.slidePitchCurve);
			_PlayLooped(playRollSounds, rollThresholdMin, rollThresholdMax, body.angularVelocity.magnitude, ref _rollSound, ref _rollSoundInitialValues, soundPack.rollSounds, soundPack.rollVolumeCurve, soundPack.rollPitchCurve);
			_currentlySliding = true;
		}
	}

	private void OnDisable()
	{
		_SetDirty();
	}
}
