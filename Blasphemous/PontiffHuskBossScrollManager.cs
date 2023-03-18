using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using Tools.Level.Interactables;
using UnityEngine;

public class PontiffHuskBossScrollManager : MonoBehaviour
{
	public PontiffHuskLevelScrollManager LevelScrollManager;

	public List<GameObject> Modules;

	public CameraNumericBoundaries NormalCamNumBound;

	public CameraNumericBoundaries BossCamNumBound;

	public List<GameObject> ScrollableItems;

	public float Speed = 1f;

	public float ModuleWidth = 29.6f;

	public float TimeToCycleFirstModuleAfterReset = 20f;

	public float TimeToCycleNewModule = 29.6f;

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
		NormalCamNumBound.SetBoundaries();
		if (_scrollActive)
		{
			_scrollActive = false;
			_timeUntilNextModule = TimeToCycleFirstModuleAfterReset;
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
			_nextModuleIndex = 0;
		}
	}

	public void Stop()
	{
		_modulesActive = false;
		DisableAllPlatforms();
	}

	public void ActivateModules()
	{
		_modulesActive = true;
		EnableAllPlatforms();
		_timeUntilNextModule = TimeToCycleFirstModuleAfterReset;
		GameObject gameObject = Modules[Modules.Count - 1];
		gameObject.SetActive(value: true);
		Modules[_nextModuleIndex].SetActive(value: true);
		UseStartingPlatforms();
		LevelScrollManager.Stop();
	}

	public void SetBossCamBounds()
	{
		_startingCamBoundaries = new Vector2(BossCamNumBound.LeftBoundary, BossCamNumBound.RightBoundary);
		BossCamNumBound.SetBoundaries();
	}

	public void SetExecutionCamBounds()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition();
		float num = BossCamNumBound.RightBoundary - BossCamNumBound.LeftBoundary;
		float num2 = 1f;
		FakeExecution fakeExecution = Object.FindObjectOfType<FakeExecution>();
		float num3 = -0.75f;
		DOTween.To(() => BossCamNumBound.LeftBoundary, delegate(float x)
		{
			BossCamNumBound.LeftBoundary = x;
		}, position.x - num / 2f + num3, num2);
		DOTween.To(() => BossCamNumBound.RightBoundary, delegate(float x)
		{
			BossCamNumBound.RightBoundary = x;
		}, position.x + num / 2f + num3, num2);
		DOTween.To(() => BossCamNumBound.BottomBoundary, delegate(float x)
		{
			BossCamNumBound.BottomBoundary = x;
		}, position.y - 1f, num2);
		int num4 = 100;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(num2 / (float)num4);
		sequence.OnStepComplete(delegate
		{
			BossCamNumBound.SetBoundaries();
		});
		sequence.SetLoops(num4);
		sequence.OnComplete(delegate
		{
			BossCamNumBound.UseRightBoundary = false;
		});
		sequence.Play();
	}

	public void SetHWCamBounds()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition();
		float num = BossCamNumBound.RightBoundary - BossCamNumBound.LeftBoundary;
		float num2 = 1f;
		DOTween.To(() => BossCamNumBound.LeftBoundary, delegate(float x)
		{
			BossCamNumBound.LeftBoundary = x;
		}, position.x - 2f, num2);
		DOTween.To(() => BossCamNumBound.RightBoundary, delegate(float x)
		{
			BossCamNumBound.RightBoundary = x;
		}, position.x - 2f + num, num2);
		int num3 = 100;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(num2 / (float)num3);
		sequence.OnStepComplete(delegate
		{
			BossCamNumBound.SetBoundaries();
		});
		sequence.SetLoops(num3);
		sequence.Play();
	}

	public void ActivateScroll()
	{
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
		if (_timeUntilNextModule <= 0f)
		{
			_timeUntilNextModule = TimeToCycleNewModule;
			CycleNewModule();
		}
	}

	private void CycleNewModule()
	{
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
		Modules[_nextModuleIndex].SetActive(value: true);
	}

	private void ResetAllPlatforms()
	{
		foreach (GameObject module in Modules)
		{
			WaypointsMovingPlatform[] componentsInChildren = module.GetComponentsInChildren<WaypointsMovingPlatform>();
			WaypointsMovingPlatform[] array = componentsInChildren;
			foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
			{
				waypointsMovingPlatform.ResetPlatform();
			}
			module.SetActive(value: false);
		}
	}

	private void EnableAllPlatforms()
	{
		ToggleAllPlatforms(active: true);
	}

	private void DisableAllPlatforms()
	{
		ToggleAllPlatforms(active: false);
	}

	private void ToggleAllPlatforms(bool active)
	{
		foreach (GameObject module in Modules)
		{
			WaypointsMovingPlatform[] componentsInChildren = module.GetComponentsInChildren<WaypointsMovingPlatform>();
			WaypointsMovingPlatform[] array = componentsInChildren;
			foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
			{
				waypointsMovingPlatform.enabled = active;
			}
		}
	}

	private void UseStartingPlatforms()
	{
		GameObject gameObject = Modules[Modules.Count - 1];
		WaypointsMovingPlatform[] componentsInChildren = gameObject.GetComponentsInChildren<WaypointsMovingPlatform>();
		WaypointsMovingPlatform[] array = componentsInChildren;
		foreach (WaypointsMovingPlatform waypointsMovingPlatform in array)
		{
			waypointsMovingPlatform.Use();
		}
	}
}
