using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadScreenView : MonoBehaviour
{
	private static GameObject _Blueprint;

	public bool delayTransitionOneFrame = true;

	public UnityEvent OnFinishedTransition;

	public bool stopPooledAudio = true;

	[Range(0f, 10f)]
	public float stopPooledAudioFadeTime = 2f;

	protected Func<IEnumerator> _load;

	protected Func<IEnumerator> _unload;

	private IEnumerator _transition;

	private bool _transitionCalled;

	private static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/Loading Screen");
			}
			return _Blueprint;
		}
	}

	public static LoadScreenView Instance { get; private set; }

	public static bool IsLoading => Instance;

	public static bool FinishedLoading
	{
		get
		{
			if ((bool)Instance)
			{
				if (Instance._transitionCalled)
				{
					return Instance._transition == null;
				}
				return false;
			}
			return true;
		}
	}

	public static void Load(Func<IEnumerator> load, Func<IEnumerator> unload = null)
	{
		Pools.Unpool(Blueprint, null).GetComponent<LoadScreenView>()._SetData(load, unload);
	}

	public static void Load(SceneRef sceneRef, LoadSceneMode loadMode = LoadSceneMode.Single)
	{
		Load(() => sceneRef.LoadAsync(loadMode));
	}

	private void _OnSceneTransition(SceneRef currentActiveScene, SceneRef nextActiveScene)
	{
		Pools.ClearAll();
		Ids.ReleaseAllTables();
	}

	private void _OnSceneTransitionFinish(SceneRef currentActiveScene, SceneRef nextActiveScene)
	{
		_SetLightLayers();
	}

	private LoadScreenView _SetData(Func<IEnumerator> load, Func<IEnumerator> unload = null)
	{
		_unload = unload;
		_load = load;
		return this;
	}

	private IEnumerator _Transition()
	{
		SceneRef.OnSceneTransition += _OnSceneTransition;
		SceneRef.OnSceneTransitionFinish += _OnSceneTransitionFinish;
		if (delayTransitionOneFrame)
		{
			yield return null;
		}
		if (_unload != null)
		{
			IEnumerator unloadProcess2 = _unload();
			while (unloadProcess2.MoveNext())
			{
				yield return null;
			}
		}
		if (_load != null)
		{
			IEnumerator unloadProcess2 = _load();
			while (unloadProcess2.MoveNext())
			{
				yield return null;
			}
		}
		while (Job.NumberOfJobsRunningInDepartment(Department.Loading) > 0)
		{
			yield return null;
		}
		GC.Collect();
		SceneRef.OnSceneTransition -= _OnSceneTransition;
		SceneRef.OnSceneTransitionFinish -= _OnSceneTransitionFinish;
	}

	private void _SetLightLayers()
	{
		using PoolKeepItemListHandle<Light> poolKeepItemListHandle = base.gameObject.GetComponentsInChildrenPooled<Light>(includeInactive: true);
		using PoolKeepItemHashSetHandle<Light> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(poolKeepItemListHandle.value);
		foreach (Light item in GameUtil.GetComponentsInActiveScene<Light>())
		{
			if (!poolKeepItemHashSetHandle.Contains(item))
			{
				item.cullingMask &= -2097153;
			}
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnEnable()
	{
		Instance = this;
		_transitionCalled = false;
		_SetLightLayers();
	}

	private void OnDisable()
	{
		Instance = ((Instance == this) ? null : Instance);
	}

	private void Update()
	{
		if (_transition != null && !_transition.MoveNext())
		{
			_transition = null;
			_load = null;
			_unload = null;
			if (stopPooledAudio)
			{
				AudioPool.StopAllSafe(stopPooledAudioFadeTime);
			}
			OnFinishedTransition.Invoke();
		}
	}

	public void Transition()
	{
		if (!_transitionCalled)
		{
			_transition = _Transition();
			_transitionCalled = true;
		}
	}
}
