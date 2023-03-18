using System;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

[RequireComponent(typeof(CircleCollider2D))]
public class CameraAssistedPan : MonoBehaviour
{
	public enum InfluenceDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public CircleCollider2D Collider;

	public LayerMask TargetLayer;

	public AnimationCurve InfluenceCurve;

	public bool EnabledAssistedPan;

	[Range(0f, 10f)]
	public float CameraPanValue = 5f;

	public InfluenceDirection PanDirection;

	private CameraTarget _playerCameraTarget;

	private ProCamera2D _proCamera2D;

	private CameraPlayerOffset _cameraPlayerOffset;

	private Vector2 _assistedPanValue;

	[Range(0f, 50f)]
	public float ExternalRadius = 4f;

	[Range(1f, 50f)]
	public float InternalRadius = 2f;

	private const float MinInternalOffset = 2f;

	private CameraManager _cameraManager;

	private Tween currentTweenX;

	private Tween currentTweenY;

	public Transform Target { get; private set; }

	private float DistanceToInternalCircle
	{
		get
		{
			float value = Vector2.Distance(base.transform.position, Target.position) - InternalRadius;
			return Mathf.Clamp(value, 0f, float.PositiveInfinity);
		}
	}

	private float DistanceToExternalCircle
	{
		get
		{
			float value = Vector2.Distance(base.transform.position, GetTarget.position) - ExternalRadius;
			return Mathf.Clamp(value, 0f, float.PositiveInfinity);
		}
	}

	private Transform GetTarget
	{
		get
		{
			if (Target == null && (bool)Core.Logic.Penitent)
			{
				Target = Core.Logic.Penitent.transform;
			}
			return Target;
		}
	}

	private void Awake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldlevel, Level newlevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		_cameraPlayerOffset = _cameraManager.CameraPlayerOffset;
		SetCameraPanValue();
	}

	private void Start()
	{
		_cameraManager = Core.Logic.CameraManager;
		_proCamera2D = _cameraManager.ProCamera2D;
	}

	private void LateUpdate()
	{
		if (EnabledAssistedPan && !(GetTarget == null) && DistanceToExternalCircle <= 0f && !_cameraManager.ZoomActive)
		{
			float scale = Mathf.Clamp01(DistanceToInternalCircle / Collider.radius);
			CameraPanTransition(scale);
		}
	}

	private void OnDestroy()
	{
		Target = null;
		SetDefaultCameraTargetOffsetInstant();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			SetCameraPanValue();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			SetDefaultCameraTargetOffset();
		}
	}

	private Vector2 AssistedPanValue(Vector2 defaultOffsetValue)
	{
		Vector2 result = new Vector2(defaultOffsetValue.x, defaultOffsetValue.y);
		switch (PanDirection)
		{
		case InfluenceDirection.Up:
			result.y = CameraPanValue;
			break;
		case InfluenceDirection.Down:
			result.y = 0f - CameraPanValue;
			break;
		case InfluenceDirection.Left:
			result.x = 0f - CameraPanValue;
			break;
		case InfluenceDirection.Right:
			result.x = CameraPanValue;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return result;
	}

	public void CameraPanTransition(float scale)
	{
		scale = (float)Math.Round(scale, 2);
		if (!(_cameraPlayerOffset == null))
		{
			if (PanDirection == InfluenceDirection.Up || PanDirection == InfluenceDirection.Down)
			{
				_proCamera2D.OverallOffset.y = Mathf.Lerp(_assistedPanValue.y, _cameraPlayerOffset.DefaultTargetOffset.y, InfluenceCurve.Evaluate(scale));
			}
			else
			{
				_proCamera2D.OverallOffset.x = Mathf.Lerp(_assistedPanValue.x, _cameraPlayerOffset.DefaultTargetOffset.x, InfluenceCurve.Evaluate(scale));
			}
		}
	}

	private void SetCameraPanValue()
	{
		if (!(_cameraPlayerOffset == null))
		{
			_assistedPanValue = AssistedPanValue(_cameraPlayerOffset.DefaultTargetOffset);
		}
	}

	private void SetDefaultCameraTargetOffsetInstant()
	{
		KillTweens();
		_proCamera2D.OverallOffset.x = _cameraPlayerOffset.DefaultTargetOffset.x;
		_proCamera2D.OverallOffset.y = _cameraPlayerOffset.DefaultTargetOffset.y;
	}

	private void SetDefaultCameraTargetOffset(float duration = 1f)
	{
		KillTweens();
		if (PanDirection == InfluenceDirection.Up || PanDirection == InfluenceDirection.Down)
		{
			currentTweenY = DOTween.To(delegate(float y)
			{
				_proCamera2D.OverallOffset.y = y;
			}, _proCamera2D.OverallOffset.y, _cameraPlayerOffset.DefaultTargetOffset.y, duration).SetEase(Ease.InSine);
		}
		else
		{
			currentTweenX = DOTween.To(delegate(float x)
			{
				_proCamera2D.OverallOffset.x = x;
			}, _proCamera2D.OverallOffset.x, _cameraPlayerOffset.DefaultTargetOffset.x, duration).SetEase(Ease.InSine);
		}
	}

	private void KillTweens()
	{
		if (currentTweenX != null)
		{
			currentTweenX.Kill();
		}
		if (currentTweenY != null)
		{
			currentTweenY.Kill();
		}
	}

	private void OnDrawGizmos()
	{
		if (InternalRadius >= ExternalRadius)
		{
			ExternalRadius += 2f;
		}
		Collider.radius = ExternalRadius;
		DrawCircle(ExternalRadius, Color.yellow);
		DrawCircle(InternalRadius, Color.red);
	}

	private void DrawCircle(float radius, Color color)
	{
		Gizmos.color = color;
		float f = 0f;
		float x = radius * Mathf.Cos(f);
		float y = radius * Mathf.Sin(f);
		Vector3 vector = base.transform.position + new Vector3(x, y);
		Vector3 to = vector;
		for (f = 0.1f; f < (float)Math.PI * 2f; f += 0.1f)
		{
			x = radius * Mathf.Cos(f);
			y = radius * Mathf.Sin(f);
			Vector3 vector2 = base.transform.position + new Vector3(x, y);
			Gizmos.DrawLine(vector, vector2);
			vector = vector2;
		}
		Gizmos.DrawLine(vector, to);
	}
}
