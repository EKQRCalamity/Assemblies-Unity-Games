using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	private static CameraManager _Instance;

	private Camera _mainCamera;

	private Camera _worldSpaceUICamera;

	private Camera _screenSpaceUICamera;

	private AudioListener _audioListener;

	private Dictionary<Transform, TransformData> _savedTransformData;

	public static CameraManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance, createSeparateGameObject: true, null, dontDestroyOnLoad: false);

	public Camera mainCamera
	{
		get
		{
			if (!_mainCamera)
			{
				return _mainCamera = Camera.main;
			}
			return _mainCamera;
		}
	}

	public Camera worldSpaceUICamera
	{
		get
		{
			if (!_worldSpaceUICamera)
			{
				return _worldSpaceUICamera = GameUtil.FindComponentWithTag<Camera>("WorldSpaceUICamera");
			}
			return _worldSpaceUICamera;
		}
	}

	public Camera screenSpaceUICamera
	{
		get
		{
			if (!_screenSpaceUICamera)
			{
				return _screenSpaceUICamera = GameUtil.FindComponentWithTag<Camera>("ScreenSpaceUICamera");
			}
			return _screenSpaceUICamera;
		}
	}

	public AudioListener audioListener
	{
		get
		{
			if (!_audioListener)
			{
				return _audioListener = mainCamera.GetComponentInChildren<AudioListener>();
			}
			return _audioListener;
		}
	}

	private Dictionary<Transform, TransformData> savedTransformData => _savedTransformData ?? (_savedTransformData = new Dictionary<Transform, TransformData>());

	private void Start()
	{
		GetCamera(CameraType.Main);
		GetCamera(CameraType.WorldSpaceUI);
	}

	public Camera GetCamera(CameraType cameraType)
	{
		return cameraType switch
		{
			CameraType.Main => mainCamera, 
			CameraType.WorldSpaceUI => worldSpaceUICamera, 
			CameraType.ScreenSpaceUI => screenSpaceUICamera, 
			_ => throw new ArgumentOutOfRangeException("cameraType", cameraType, null), 
		};
	}

	public Camera GetUICamera()
	{
		if (!screenSpaceUICamera)
		{
			if (!worldSpaceUICamera)
			{
				return mainCamera;
			}
			return worldSpaceUICamera;
		}
		return screenSpaceUICamera;
	}

	public Transform GetUICanvasTransform()
	{
		Camera uICamera = GetUICamera();
		Canvas componentInChildren = uICamera.GetComponentInChildren<Canvas>();
		if (!componentInChildren)
		{
			return uICamera.transform;
		}
		return componentInChildren.transform;
	}

	public void SaveTransformData(Transform transformToSave)
	{
		savedTransformData[transformToSave] = new TransformData(transformToSave);
	}

	public TransformData? LoadTransformData(Transform transformToLoad, bool clearSave = true)
	{
		if (!savedTransformData.ContainsKey(transformToLoad))
		{
			return null;
		}
		TransformData value = savedTransformData[transformToLoad];
		if (clearSave)
		{
			savedTransformData.Remove(transformToLoad);
		}
		return value;
	}
}
