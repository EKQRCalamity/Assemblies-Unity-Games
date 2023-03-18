using System.Collections.Generic;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using UnityEngine;

public class HighWillsBossScrollManager : MonoBehaviour
{
	public HighWillsLevelScrollManager LevelScrollManager;

	public List<GameObject> Modules;

	public CameraNumericBoundaries NormalCamNumBound;

	public CameraNumericBoundaries BossCamNumBound;

	public List<GameObject> ScrollableItems;

	public float Speed = 1f;

	public float ModuleWidth = 31f;

	public float TimeToCycleNewModule = 14f;

	private bool _modulesActive;

	private bool _scrollActive;

	private List<Vector2> _modulesPositions = new List<Vector2>();

	private List<Vector2> _scrollableItemsPositions = new List<Vector2>();

	private Vector2 _startingCamBoundaries;

	private int _nextModuleIndex;

	private float _timeUntilNextModule;

	private void Start()
	{
		foreach (GameObject module in Modules)
		{
			_modulesPositions.Add(module.transform.position);
		}
		foreach (GameObject scrollableItem in ScrollableItems)
		{
			_scrollableItemsPositions.Add(scrollableItem.transform.position);
		}
	}

	public void Reset()
	{
		Debug.LogError("Reset");
		_timeUntilNextModule = TimeToCycleNewModule;
		_scrollActive = false;
		ResetAllPlatforms();
		for (int i = 0; i < Modules.Count; i++)
		{
			Modules[i].transform.position = _modulesPositions[i];
		}
		for (int j = 0; j < ScrollableItems.Count; j++)
		{
			ScrollableItems[j].transform.position = _scrollableItemsPositions[j];
		}
		BossCamNumBound.LeftBoundary = _startingCamBoundaries.x;
		BossCamNumBound.RightBoundary = _startingCamBoundaries.y;
		NormalCamNumBound.SetBoundaries();
		_nextModuleIndex = 0;
	}

	public void Stop()
	{
		Debug.LogError("Stop");
		_modulesActive = false;
		StopAllPlatforms();
	}

	public void ActivateModules()
	{
		Debug.LogError("ActivateModules");
		_modulesActive = true;
		_timeUntilNextModule = TimeToCycleNewModule;
		Modules.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: true);
		});
		UseStartingPlatforms();
		LevelScrollManager.Stop();
	}

	public void SetBossCamBounds()
	{
		Debug.LogError("SetBossCamBounds");
		_startingCamBoundaries = new Vector2(BossCamNumBound.LeftBoundary, BossCamNumBound.RightBoundary);
		BossCamNumBound.SetBoundaries();
	}

	public void ActivateScroll()
	{
		Debug.LogError("ActivateScroll");
		_scrollActive = true;
	}

	private void LateUpdate()
	{
		if (!_modulesActive)
		{
			return;
		}
		_timeUntilNextModule -= Time.deltaTime;
		if (!_scrollActive)
		{
			return;
		}
		float num = Time.deltaTime * Speed;
		foreach (GameObject scrollableItem in ScrollableItems)
		{
			scrollableItem.transform.position += Vector3.right * num;
		}
		BossCamNumBound.LeftBoundary += num;
		BossCamNumBound.RightBoundary += num;
		BossCamNumBound.SetBoundaries();
		CheckToCycleNewModule();
	}

	private void CheckToCycleNewModule()
	{
		Debug.LogError("CheckToCycleNewModule");
		if (_timeUntilNextModule <= 0f)
		{
			_timeUntilNextModule = TimeToCycleNewModule;
			CycleNewModule();
		}
	}

	private void CycleNewModule()
	{
		Debug.LogError("CycleNewModule");
		GameObject gameObject = Modules[_nextModuleIndex];
		WaypointsMovingPlatform[] componentsInChildren = gameObject.GetComponentsInChildren<WaypointsMovingPlatform>();
		WaypointsMovingPlatform[] array = componentsInChildren;
		foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
		{
			waypointsMovingPlatform.ResetPlatform();
		}
		gameObject.transform.position += Vector3.right * ModuleWidth * Modules.Count;
		WaypointsMovingPlatform[] array2 = componentsInChildren;
		foreach (WaypointsMovingPlatform waypointsMovingPlatform2 in array2)
		{
			waypointsMovingPlatform2.Use();
		}
		_nextModuleIndex++;
		if (_nextModuleIndex == Modules.Count)
		{
			_nextModuleIndex = 0;
		}
	}

	private void ResetAllPlatforms()
	{
		Debug.LogError("ResetAllPlatforms");
		foreach (GameObject module in Modules)
		{
			WaypointsMovingPlatform[] componentsInChildren = module.GetComponentsInChildren<WaypointsMovingPlatform>();
			WaypointsMovingPlatform[] array = componentsInChildren;
			foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
			{
				waypointsMovingPlatform.ResetPlatform();
			}
		}
	}

	private void StopAllPlatforms()
	{
		Debug.LogError("StopAllPlatforms");
		foreach (GameObject module in Modules)
		{
			WaypointsMovingPlatform[] componentsInChildren = module.GetComponentsInChildren<WaypointsMovingPlatform>();
			WaypointsMovingPlatform[] array = componentsInChildren;
			foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
			{
				waypointsMovingPlatform.Use();
			}
		}
	}

	private void UseStartingPlatforms()
	{
		Debug.LogError("UseStartingPlatforms");
		GameObject gameObject = Modules[Modules.Count - 1];
		WaypointsMovingPlatform[] componentsInChildren = gameObject.GetComponentsInChildren<WaypointsMovingPlatform>();
		WaypointsMovingPlatform[] array = componentsInChildren;
		foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
		{
			waypointsMovingPlatform.Use();
		}
	}
}
