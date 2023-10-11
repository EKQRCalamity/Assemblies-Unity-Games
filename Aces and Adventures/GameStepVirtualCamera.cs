using System.Collections;
using Cinemachine;
using UnityEngine;

public class GameStepVirtualCamera : GameStep
{
	private CinemachineVirtualCameraBase _camera;

	private float _time;

	private float _elapsedTime;

	private CinemachineBrain _brain;

	private CinemachineCameraStateData _activeVirtualCamera = new CinemachineCameraStateData(CameraState.Default);

	private int _overrideId;

	public GameStepVirtualCamera(CinemachineVirtualCameraBase virtualCamera, float time = 1f)
	{
		_camera = virtualCamera;
		_time = Mathf.Max(0.001f, time);
	}

	protected override void OnFirstEnabled()
	{
		_brain = Camera.main.GetComponent<CinemachineBrain>();
	}

	protected override void OnEnable()
	{
		_activeVirtualCamera.state = _brain.CurrentCameraState;
	}

	protected override IEnumerator Update()
	{
		while (true)
		{
			_overrideId = _brain.SetCameraOverride(_overrideId, _activeVirtualCamera, _camera, MathUtil.CubicSplineInterpolant(Mathf.Clamp01((_elapsedTime += Time.deltaTime) / _time)), -1f);
			if (_elapsedTime >= _time)
			{
				break;
			}
			yield return null;
		}
	}

	protected override void End()
	{
		base.manager.SetActiveVirtualCamera(_camera);
	}

	protected override void OnDisable()
	{
		_brain.ReleaseCameraOverride(_overrideId);
		_overrideId = -1;
	}
}
