using Cinemachine;
using UnityEngine;

public class CinemachineCameraStateData : ICinemachineCamera
{
	public CameraState state;

	public string Name => "CinemachineCameraStateData";

	public string Description => State.ToString();

	public int Priority
	{
		get
		{
			return int.MinValue;
		}
		set
		{
		}
	}

	public Transform LookAt
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public Transform Follow
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public CameraState State => state;

	public GameObject VirtualCameraGameObject => null;

	public bool IsValid => true;

	public ICinemachineCamera ParentCamera => null;

	public CinemachineCameraStateData(CameraState state)
	{
		this.state = state;
	}

	public bool IsLiveChild(ICinemachineCamera vcam, bool dominantChildOnly = false)
	{
		return false;
	}

	public void UpdateCameraState(Vector3 worldUp, float deltaTime)
	{
	}

	public void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
	}

	public void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
	}

	public void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
	}
}
