using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Volume))]
public class DepthOfFieldShifter : MonoBehaviour
{
	public float padding = 0.01f;

	[Range(1f, 100f)]
	public float easeSpeed = 5f;

	[Range(1f, 100f)]
	public float focalLengthWhenTargetIsActive = 25f;

	[Range(1f, 100f)]
	public float focalLengthEaseSpeed = 5f;

	private Volume _depthOfFieldVolume;

	private DepthOfField _depthOfField;

	private readonly List<Transform> _targetOverrides = new List<Transform>();

	private readonly List<Transform> _targets = new List<Transform>();

	public Volume depthOfFieldVolume => this.CacheComponent(ref _depthOfFieldVolume);

	public DepthOfField depthOfField
	{
		get
		{
			if (!_depthOfField)
			{
				depthOfFieldVolume.profile.TryGet<DepthOfField>(out _depthOfField);
			}
			return _depthOfField;
		}
	}

	public List<Transform> targetOverrides => _targetOverrides;

	public Transform targetOverride
	{
		get
		{
			if (_targetOverrides.Count <= 0)
			{
				return null;
			}
			return _targetOverrides[_targetOverrides.Count - 1];
		}
	}

	public float targetFocalLength
	{
		get
		{
			float? num = focalLengthOverride;
			if (!num.HasValue)
			{
				if (_targets.Count <= 0)
				{
					return 0f;
				}
				return focalLengthWhenTargetIsActive;
			}
			return num.GetValueOrDefault();
		}
	}

	public float? focalLengthOverride { get; set; }

	private void OnEnable()
	{
		Camera.main.focalLength = 0f;
	}

	private void Update()
	{
		Camera main = Camera.main;
		float value = main.focalLength;
		MathUtil.EaseSnap(ref value, targetFocalLength, focalLengthEaseSpeed, Time.deltaTime);
		main.focalLength = value;
		if (main.focalLength <= 0f && !focalLengthOverride.HasValue)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (_targets.Count == 0 || !depthOfField)
		{
			return;
		}
		Camera main = Camera.main;
		Vector3 forward = main.transform.forward;
		Vector3 position = main.transform.position;
		float num = 0f;
		if ((bool)targetOverride)
		{
			num = Vector3.Dot(forward, targetOverride.position - position);
		}
		else
		{
			foreach (Transform target in _targets)
			{
				num = Math.Max(num, Vector3.Dot(forward, target.position - position));
			}
		}
		float value = depthOfField.focusDistance.value;
		MathUtil.EaseSnap(ref value, Math.Max(main.nearClipPlane, num) + padding, easeSpeed, Time.deltaTime, 0.001f);
		depthOfField.focusDistance.value = value;
	}

	private void OnDisable()
	{
		focalLengthOverride = null;
		_targetOverrides.Clear();
		_targets.Clear();
	}

	public void AddTarget(Transform targetToAdd)
	{
		_targets.Add(targetToAdd);
		if (_targets.Count == 1)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void RemoveTarget(Transform targetToRemove)
	{
		_targets.Remove(targetToRemove);
	}
}
