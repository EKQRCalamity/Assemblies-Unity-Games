using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI.Widgets;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

public class CameraPan : MonoBehaviour
{
	public bool EnableCameraPan;

	public bool AllowXPanning;

	public bool AllowYPanning;

	private float _accumulatedPanningTime;

	public float TimeToKeyoboardPanning = 0.5f;

	private bool _panningCamera;

	private CameraTarget _panTarget;

	private const float HorizontalOffset = 1f;

	private const float VerticalOffset = 1f;

	private const float MinDistanceToHorizontalBoundaries = 15f;

	private const float MinDistanceToVerticalBoundaries = 7.5f;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public Vector2 PanSmooth;

	private Vector2 _defaultCameraSmooth;

	private float _defaultLeftFocus;

	private float _defaultRightFocus;

	[FoldoutGroup("Component Dependencies", 0)]
	public ProCamera2DPanAndZoom ProCamera2DPan;

	[FoldoutGroup("Component Dependencies", 0)]
	public CameraPlayerOffset CameraPlayerOffset;

	protected ProCamera2DLimitDistance LimitDistance;

	private const float VerticalLimitRestoreLapse = 1f;

	private float _currentVeticalLimitRestoreLapse;

	[FoldoutGroup("OffSet Panning", 0)]
	public float UpWardCameraPanOffset = 3f;

	[FoldoutGroup("OffSet Panning", 0)]
	public float DownWardCameraPanOffset = 3f;

	public bool CameraPanReady { get; private set; }

	public Player Rewired { get; private set; }

	public ProCamera2D ProCamera { get; private set; }

	public ProCamera2DNumericBoundaries CameraNumericBoundaries { get; private set; }

	public bool IsRightBounded
	{
		get
		{
			if (!CameraNumericBoundaries.UseNumericBoundaries || !CameraNumericBoundaries.UseRightBoundary)
			{
				return false;
			}
			float num = UnityEngine.Camera.main.ScreenToWorldPoint(Vector3.zero).x + ProCamera.ScreenSizeInWorldCoordinates.x;
			return num > CameraNumericBoundaries.RightBoundary - 1f;
		}
	}

	public bool IsLeftBounded
	{
		get
		{
			if (!CameraNumericBoundaries.UseNumericBoundaries || !CameraNumericBoundaries.UseLeftBoundary)
			{
				return false;
			}
			return UnityEngine.Camera.main.ScreenToWorldPoint(Vector3.zero).x < CameraNumericBoundaries.LeftBoundary + 1f;
		}
	}

	public bool IsTopBounded
	{
		get
		{
			if (!CameraNumericBoundaries.UseNumericBoundaries || !CameraNumericBoundaries.UseTopBoundary)
			{
				return false;
			}
			float num = UnityEngine.Camera.main.ScreenToWorldPoint(Vector3.zero).y + ProCamera.ScreenSizeInWorldCoordinates.y;
			return num > CameraNumericBoundaries.TopBoundary - 1f;
		}
	}

	public bool IsBottomBounded
	{
		get
		{
			if (!CameraNumericBoundaries.UseNumericBoundaries || !CameraNumericBoundaries.UseBottomBoundary)
			{
				return false;
			}
			return UnityEngine.Camera.main.ScreenToWorldPoint(Vector3.zero).y < CameraNumericBoundaries.BottomBoundary + 1f;
		}
	}

	private void Start()
	{
		Rewired = ReInput.players.GetPlayer(0);
		_penitent = null;
		ProCamera = Core.Logic.CameraManager.ProCamera2D;
		CameraNumericBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries;
		if (ProCamera2DPan != null)
		{
			_defaultCameraSmooth = new Vector2(ProCamera2DPan.ProCamera2D.HorizontalFollowSmoothness, ProCamera2DPan.ProCamera2D.VerticalFollowSmoothness);
			LimitDistance = ProCamera2DPan.ProCamera2D.GetComponent<ProCamera2DLimitDistance>();
		}
		else
		{
			Debug.LogError("A Camera Pan Target is Required");
		}
		if (CameraPlayerOffset == null)
		{
			Debug.LogError("A Camera Player Offset Component is Required");
		}
		FadeWidget.OnFadeHidedEnd += FadeWidgetOnFadeHidedEnd;
		FadeWidget.OnFadeShowEnd += DoorOnDoorEnter;
		FadeWidget.OnFadeHidedStart += DoorOnDoorExit;
		SpawnManager.OnPlayerSpawn += OnPenitentReady;
	}

	private void OnDestroy()
	{
		FadeWidget.OnFadeHidedEnd -= FadeWidgetOnFadeHidedEnd;
		FadeWidget.OnFadeShowEnd -= DoorOnDoorEnter;
		FadeWidget.OnFadeHidedStart -= DoorOnDoorExit;
		SpawnManager.OnPlayerSpawn -= OnPenitentReady;
	}

	private void OnPenitentReady(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		_penitent = penitent;
		GameObject gameObject = GameObject.Find("PC2DPanTarget");
		CameraTarget cameraTarget = new CameraTarget();
		cameraTarget.TargetTransform = gameObject.transform;
		cameraTarget.TargetInfluenceH = 1f;
		cameraTarget.TargetInfluenceV = 1f;
		cameraTarget.TargetOffset = Vector2.zero;
		CameraTarget panTarget = cameraTarget;
		_panTarget = panTarget;
	}

	private void FadeWidgetOnFadeHidedEnd()
	{
		if (!CameraPanReady)
		{
			CameraPanReady = true;
		}
	}

	private void LateUpdate()
	{
		if ((bool)_penitent)
		{
			if (!CameraPanReady)
			{
				ResetCameraPan();
			}
			else if ((!Core.Input.InputBlocked || Core.Input.HasBlocker("PLAYER_LOGIC")) && !_penitent.Status.Dead && EnableCameraPan)
			{
				ProcessCameraPanningInput();
			}
		}
	}

	private void DoorOnDoorEnter()
	{
		RemoveCameraPanFromTargets();
	}

	private void DoorOnDoorExit()
	{
	}

	private void ProcessCameraPanningInput()
	{
		if (!(_penitent == null))
		{
			Vector2 zero = Vector2.zero;
			float axis = Rewired.GetAxis(20);
			float axis2 = Rewired.GetAxis(21);
			zero.x = axis2;
			zero.y = axis;
			float offset = ((!(axis >= 0f)) ? DownWardCameraPanOffset : UpWardCameraPanOffset);
			SetCameraPan(zero, offset);
		}
	}

	public void RemoveCameraPanFromTargets()
	{
		List<CameraTarget> cameraTargets = ProCamera.CameraTargets;
		foreach (CameraTarget item in cameraTargets)
		{
			if (item.TargetTransform == ProCamera2DPan.PanTarget)
			{
				_panTarget = item;
				if (cameraTargets.Contains(item))
				{
					cameraTargets.Remove(item);
				}
				break;
			}
		}
	}

	public void AddCameraPanToTargets()
	{
		if (_panTarget != null)
		{
			_panTarget.TargetTransform.position = Core.Logic.Penitent.transform.position;
			if (!ProCamera.CameraTargets.Contains(_panTarget))
			{
				ProCamera.CameraTargets.Add(_panTarget);
			}
		}
	}

	private void SetCameraPan(Vector3 joystickPan, float offset)
	{
		if (!(ProCamera2DPan == null) && !(_penitent == null))
		{
			Vector3 vector = ResolvePaningValueConstraints(new Vector2(joystickPan.x, joystickPan.y));
			if (joystickPan.normalized.sqrMagnitude > 0f && EnableCameraPan)
			{
				SetPanCameraSmoothness();
				EnableLimitDistance(enable: false);
				_currentVeticalLimitRestoreLapse = 0f;
				Vector3 targetPosition = CameraPlayerOffset.PlayerTarget.TargetPosition;
				Vector3 vector2 = targetPosition + vector.normalized * offset;
				ProCamera2DPan.PanTarget.position = GetPanningDirectionConstraints(vector2);
				ProCamera2DPan.CheckTargetisOutsideBounds();
			}
			else
			{
				ResetCameraPan();
			}
		}
	}

	private Vector2 GetPanningDirectionConstraints(Vector2 rawPosition)
	{
		Vector2 result = rawPosition;
		if (!AllowXPanning)
		{
			result.x = _penitent.transform.position.x;
		}
		if (!AllowYPanning)
		{
			result.y = _penitent.transform.position.y;
		}
		return result;
	}

	private void ResetCameraPan()
	{
		bool flag = IsLeftBounded || IsRightBounded;
		bool flag2 = IsTopBounded || IsBottomBounded;
		if (flag)
		{
			ProCamera2DPan.PanTarget.position = new Vector2(ProCamera2DPan.PanTarget.position.x, _penitent.transform.position.y);
		}
		if (flag2)
		{
			ProCamera2DPan.PanTarget.position = new Vector2(_penitent.transform.position.x, ProCamera2DPan.PanTarget.position.y);
		}
		if (!flag && !flag2)
		{
			ProCamera2DPan.PanTarget.position = _penitent.transform.position;
		}
		SetDefaultProCameraFollowSmoothness();
		RestoreVerticalLimit();
	}

	public Vector3 ResolvePaningValueConstraints(Vector2 panValue)
	{
		Vector2 vector = new Vector2(panValue.x, panValue.y);
		if (!CameraNumericBoundaries.UseNumericBoundaries)
		{
			return vector;
		}
		if (CameraNumericBoundaries.UseLeftBoundary)
		{
			float num = Mathf.Abs(CameraNumericBoundaries.LeftBoundary - _penitent.transform.position.x);
			vector.x = ((!(num < 15f)) ? vector.x : 0f);
		}
		if (CameraNumericBoundaries.UseRightBoundary)
		{
			float num2 = Mathf.Abs(CameraNumericBoundaries.RightBoundary - _penitent.transform.position.x);
			vector.x = ((!(num2 < 15f)) ? vector.x : 0f);
		}
		if (CameraNumericBoundaries.UseBottomBoundary)
		{
			float num3 = Mathf.Abs(CameraNumericBoundaries.BottomBoundary - _penitent.transform.position.y);
			if (num3 < 7.5f)
			{
				vector.y = ((!(vector.y <= 0f)) ? vector.y : 0f);
			}
		}
		if (CameraNumericBoundaries.UseTopBoundary)
		{
			float num4 = Mathf.Abs(CameraNumericBoundaries.TopBoundary - _penitent.transform.position.y);
			if (num4 < 7.5f)
			{
				vector.y = ((!(vector.y > 0f)) ? vector.y : 0f);
			}
		}
		return vector;
	}

	private void SetDefaultProCameraFollowSmoothness()
	{
		if (!(ProCamera2DPan == null))
		{
			ProCamera2DPan.ProCamera2D.HorizontalFollowSmoothness = _defaultCameraSmooth.x;
			ProCamera2DPan.ProCamera2D.VerticalFollowSmoothness = _defaultCameraSmooth.y;
		}
	}

	private void SetPanCameraSmoothness()
	{
		if (!(ProCamera2DPan == null))
		{
			ProCamera2DPan.ProCamera2D.HorizontalFollowSmoothness = PanSmooth.x;
			ProCamera2DPan.ProCamera2D.VerticalFollowSmoothness = PanSmooth.y;
		}
	}

	private void EnableLimitDistance(bool enable)
	{
		LimitDistance.LimitVerticalCameraDistance = enable;
	}

	private void RestoreVerticalLimit()
	{
		_currentVeticalLimitRestoreLapse += Time.deltaTime;
		EnableLimitDistance(_currentVeticalLimitRestoreLapse >= 1f);
	}
}
