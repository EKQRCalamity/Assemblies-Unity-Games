using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

[RequireComponent(typeof(ProCamera2D))]
public class CameraPlayerOffset : MonoBehaviour
{
	public const string ForwardFocusTween = "ForwardFocus";

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private float _currentOverallYOffset;

	private float _deltaXOffsetRecoverTime;

	private EntityOrientation _playerLastOrientation;

	private EntityOrientation _playerCurrentOrientation;

	private ProCamera2D _proCamera2D;

	private ProCamera2DGeometryBoundaries _geometryBoundaries;

	[Range(0f, 10f)]
	public float ElapsedTime = 1f;

	[Range(-5f, 5f)]
	public float XOffset = 1.5f;

	public float XOffsetRecoverTime = 0.5f;

	[Range(-10f, 10f)]
	public float YOffset;

	public CameraTarget PlayerTarget { get; private set; }

	public Vector2 DefaultTargetOffset { get; private set; }

	public bool ReadyOffset { get; set; }

	public bool RestoredXOffset { get; set; }

	private void Awake()
	{
		_proCamera2D = GetComponent<ProCamera2D>();
		_geometryBoundaries = GetComponent<ProCamera2DGeometryBoundaries>();
		_penitent = null;
	}

	public void UpdateNewParams()
	{
		_penitent = Core.Logic.Penitent;
		_proCamera2D = GetComponent<ProCamera2D>();
		_geometryBoundaries = GetComponent<ProCamera2DGeometryBoundaries>();
		_playerCurrentOrientation = _penitent.Status.Orientation;
		_playerLastOrientation = _playerCurrentOrientation;
		SetCameraXOffset(_playerCurrentOrientation, 0f);
		RestoredXOffset = true;
		SetCameraTarget(_proCamera2D.CameraTargets);
		DefaultTargetOffset = _proCamera2D.OverallOffset;
	}

	private void Start()
	{
		ReadyOffset = false;
	}

	private void Update()
	{
		if (!RestoredXOffset && (bool)_penitent && _penitent.Animator.GetCurrentAnimatorStateInfo(0).IsName("Run Start"))
		{
			RestoredXOffset = true;
			SetCameraXOffset(_playerCurrentOrientation, ElapsedTime);
		}
	}

	private void LateUpdate()
	{
		if (!_penitent)
		{
			return;
		}
		_playerCurrentOrientation = _penitent.Status.Orientation;
		if (_playerLastOrientation != _playerCurrentOrientation)
		{
			if (DOTween.IsTweening("ForwardFocus"))
			{
				DOTween.Kill("ForwardFocus");
			}
			_deltaXOffsetRecoverTime += Time.deltaTime;
			if (_deltaXOffsetRecoverTime >= XOffsetRecoverTime)
			{
				_playerLastOrientation = _playerCurrentOrientation;
				SetCameraXOffset(_playerCurrentOrientation, ElapsedTime);
				_deltaXOffsetRecoverTime = 0f;
				RestoredXOffset = true;
			}
		}
		else
		{
			_deltaXOffsetRecoverTime = 0f;
		}
		if (IsCameraForwardBlocked())
		{
			StopCameraTween();
		}
	}

	private void SetCameraXOffset(EntityOrientation orientation, float elapsedTime)
	{
		if (PlayerTarget != null)
		{
			Vector2 vector = new Vector2(XOffset, YOffset);
			if (orientation == EntityOrientation.Left)
			{
				vector.x = 0f - XOffset;
			}
			DOTween.To(delegate(float x)
			{
				_proCamera2D.OverallOffset.x = x;
			}, _proCamera2D.OverallOffset.x, vector.x, elapsedTime).SetEase(Ease.OutSine).SetId("ForwardFocus");
		}
	}

	public static void StopCameraTween()
	{
		if (DOTween.IsTweening("ForwardFocus"))
		{
			DOTween.Kill("ForwardFocus");
		}
	}

	public void SetCameraTarget(IList<CameraTarget> targets)
	{
		for (byte b = 0; b < targets.Count; b = (byte)(b + 1))
		{
			if (targets[b].TargetTransform.tag.Equals("Penitent"))
			{
				PlayerTarget = targets[b];
				break;
			}
		}
		if (PlayerTarget != null)
		{
			PlayerTarget.TargetOffset = Vector2.zero;
		}
		else
		{
			Debug.LogError("NO Player Target Found!");
		}
	}

	private bool IsCameraForwardBlocked()
	{
		bool result = false;
		EntityOrientation orientation = _penitent.Status.Orientation;
		if (_proCamera2D.IsCameraPositionLeftBounded && orientation == EntityOrientation.Left)
		{
			result = true;
		}
		else if (_proCamera2D.IsCameraPositionRightBounded && orientation == EntityOrientation.Right)
		{
			result = true;
		}
		else if (_geometryBoundaries._cameraMoveInColliderBoundaries.CameraCollisionState.HTopLeft && _playerCurrentOrientation == EntityOrientation.Left)
		{
			result = true;
		}
		else if (_geometryBoundaries._cameraMoveInColliderBoundaries.CameraCollisionState.HTopRight && _playerCurrentOrientation == EntityOrientation.Right)
		{
			result = true;
		}
		return result;
	}
}
