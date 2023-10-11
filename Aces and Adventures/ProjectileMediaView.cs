using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileMediaView : MonoBehaviour
{
	public static float EmissionMultiplier = 1f;

	public static float LaunchMultiplier = 1f;

	private static GameObject _Blueprint;

	private static GameObject _LauncherBlueprint;

	public Action onFinish;

	public ProjectilesFinishedAt finishedAt;

	private List<Transform> _targets = new List<Transform>();

	private List<ProjectileLauncherView> _launchers = new List<ProjectileLauncherView>();

	private ProjectileMediaData _data;

	private ProjectileExtremaData _startData;

	private ProjectileExtremaData _endData;

	private Action<ProjectileLauncherView> _onLauncherFinished;

	private System.Random _random;

	private IProjectileExtrema _activator;

	private IProjectileExtrema _target;

	private float _flightLightEvery;

	private float _flightLightIndex;

	private bool _createImpactMedia;

	private bool _finishing;

	private float _timeOfBeginFinish;

	private bool _finishDelayComplete;

	private bool _finished;

	private ProjectileMediaView _onFinishProjectileMediaView;

	private float _emissionMultiplier;

	private float _emissionMultiplierMod = 1f;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("GameState/Ability/Projectiles/ProjectileMediaView");
			}
			return _Blueprint;
		}
	}

	protected static GameObject LauncherBlueprint
	{
		get
		{
			if (!_LauncherBlueprint)
			{
				return _LauncherBlueprint = Resources.Load<GameObject>("GameState/Ability/Projectiles/ProjectileLauncherView");
			}
			return _LauncherBlueprint;
		}
	}

	protected Action<ProjectileLauncherView> onLauncherFinished => _RemoveProjectileLauncherView;

	public ProjectileExtremaData startData => _startData;

	public ProjectileExtremaData endData => _endData;

	public Transform startTransform
	{
		get
		{
			if (!startData.subjectIsActivator)
			{
				return _target.transform;
			}
			return _activator.transform;
		}
	}

	public Transform endTransform
	{
		get
		{
			if (!endData.subjectIsActivator)
			{
				return _target.transform;
			}
			return _activator.transform;
		}
	}

	public bool canceled { get; set; }

	public int activeProjectileCount { get; set; }

	private bool finishing
	{
		get
		{
			return _finishing;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _finishing, value) && _finishing)
			{
				_OnBeginFinish();
			}
		}
	}

	private bool finishDelayComplete
	{
		get
		{
			return _finishDelayComplete;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _finishDelayComplete, value) && _finishDelayComplete)
			{
				_OnFinishDelayComplete();
			}
		}
	}

	private bool finished
	{
		get
		{
			return _finished;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _finished, value) && _finished)
			{
				_OnFinish();
			}
		}
	}

	private float finishDelay => _data.main.finishDelay;

	public bool createImpactMedia => _createImpactMedia;

	public float emissionMultiplier
	{
		get
		{
			return _emissionMultiplier * EmissionMultiplier;
		}
		private set
		{
			_emissionMultiplier = value;
		}
	}

	public PoolKeepItemListHandle<ProjectileLauncherView> launchers => Pools.UseKeepItemList(_launchers);

	public static int GetModifiedLaunchCount(int launchCount)
	{
		if (!(LaunchMultiplier >= 1f))
		{
			int val = Math.Min(launchCount, 3);
			int num = Mathf.CeilToInt((float)launchCount * LaunchMultiplier);
			return Math.Max(val, (num % 2 == launchCount % 2) ? num : (num + 1));
		}
		return launchCount;
	}

	public static ProjectileMediaView Create(System.Random random, ProjectileMediaData data, IProjectileExtrema activator, IProjectileExtrema target, ProjectileExtremaData startDataOverride = null, ProjectileExtremaData endDataOverride = null, ProjectilesFinishedAt? finishedAtOverride = null, bool createImpactMedia = true, float emissionMultiplierMod = 1f)
	{
		ProjectileMediaView component = Pools.Unpool(Blueprint).GetComponent<ProjectileMediaView>();
		component.LaunchProjectiles(random, data, activator, target, startDataOverride, endDataOverride, finishedAtOverride, createImpactMedia, emissionMultiplierMod);
		return component;
	}

	private void _RemoveProjectileLauncherView(ProjectileLauncherView launcher)
	{
		if (_launchers.Remove(launcher) && _launchers.Count == 0)
		{
			finishing = true;
		}
	}

	private float _GetAverageFlightDistance()
	{
		Vector3 vector = _launchers.Select((ProjectileLauncherView launcher) => launcher.transform.position).Average() + _data.launch.parameters.shape.shapeOffsets.GetAverageOffset(startTransform.rotation) + _startData.positionOffset.GetAverageOffset(startTransform.rotation);
		return (_targets.Select((Transform t) => t.position + _data.impact.parameters.shape.shapeOffsets.GetAverageOffset(t.rotation)).Average() + _endData.positionOffset.GetAverageOffset(endTransform.rotation) - vector).magnitude;
	}

	private void _CalculateFlightLightEveryAmount()
	{
		float distance = Math.Max(_GetAverageFlightDistance(), 0.1f);
		float val = _data.GetAverageNumberOfLivingProjectiles(distance) * (float)EnumUtil.FlagCount(_startData.cardTargets);
		_flightLightEvery = Math.Max(1f, val) / Math.Max(0.01f, EmissionMultiplier * 1.5f);
	}

	private void _OnBeginFinish()
	{
		_timeOfBeginFinish = Time.time;
		if (!_data.main.waitForProjectileMedia)
		{
			_OnFinish();
		}
	}

	private void _OnFinishDelayComplete()
	{
		ProjectileMediaPack onFinishProjectileMedia = _data.main.onFinishProjectileMedia;
		if (onFinishProjectileMedia != null && (bool)onFinishProjectileMedia && !_onFinishProjectileMediaView && !canceled)
		{
			(_onFinishProjectileMediaView = Pools.Unpool(Blueprint, base.transform.parent).GetComponent<ProjectileMediaView>()).LaunchProjectiles(_random, onFinishProjectileMedia.data, _activator, _target, onFinishProjectileMedia.startDataOverride, onFinishProjectileMedia.endDataOverride, onFinishProjectileMedia.finishedAtOverride, shouldCreateImpactMedia: true, _data.main.chainEmissionMultiplier ? _emissionMultiplier : _emissionMultiplierMod);
		}
	}

	private void _OnFinish()
	{
		onFinish?.Invoke();
		onFinish = null;
	}

	private void OnEnable()
	{
		canceled = false;
		_finished = (_finishing = (_finishDelayComplete = false));
		activeProjectileCount = 0;
	}

	private void Update()
	{
		if (finishing)
		{
			if (!finishDelayComplete && Time.time - _timeOfBeginFinish >= finishDelay)
			{
				finishDelayComplete = true;
			}
			if (finishDelayComplete && !finished && (!_data.main.waitForProjectileMedia || !_onFinishProjectileMediaView || _onFinishProjectileMediaView.finished))
			{
				finished = true;
			}
		}
		if (finished && activeProjectileCount == 0 && _launchers.Count == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		_targets.Clear();
		_launchers.Clear();
		_data = null;
		_startData = null;
		_endData = null;
		_activator = null;
		_target = null;
		_OnFinish();
		_flightLightIndex = 0f;
		_onFinishProjectileMediaView = null;
	}

	public void LaunchProjectiles(System.Random random, ProjectileMediaData data, IProjectileExtrema activator, IProjectileExtrema target, ProjectileExtremaData startDataOverride = null, ProjectileExtremaData endDataOverride = null, ProjectilesFinishedAt? finishedAtOverride = null, bool shouldCreateImpactMedia = true, float emissionMultiplierMod = 1f)
	{
		_random = random;
		_data = data;
		_activator = activator;
		_target = target;
		_startData = startDataOverride ?? data.main.startLocation;
		_endData = endDataOverride ?? data.main.endLocation;
		_createImpactMedia = shouldCreateImpactMedia;
		emissionMultiplier = data.main.emissionMultiplier * (_emissionMultiplierMod = emissionMultiplierMod);
		finishedAt = finishedAtOverride ?? data.main.finishedAt;
		using (PoolKeepItemListHandle<Transform> poolKeepItemListHandle = _startData.GetTransforms(activator, target))
		{
			_targets.Clear();
			foreach (Transform transform in _endData.GetTransforms(activator, target))
			{
				_targets.Add(transform);
			}
			foreach (Transform item in poolKeepItemListHandle)
			{
				ProjectileLauncherView projectileLauncherView = Pools.Unpool(LauncherBlueprint, item.position, item.rotation, base.transform.parent).GetComponent<ProjectileLauncherView>().SetData(random, this, data, item, _targets);
				projectileLauncherView.onFinish = onLauncherFinished;
				_launchers.Add(projectileLauncherView);
			}
		}
		if (data.flight.usesLighting)
		{
			_CalculateFlightLightEveryAmount();
		}
		if (finishedAt == ProjectilesFinishedAt.Immediate)
		{
			finishing = true;
		}
	}

	public bool CanUseFlightLight(bool usesLight)
	{
		_flightLightIndex += 1f;
		if (usesLight && _flightLightIndex >= _flightLightEvery)
		{
			_flightLightIndex -= _flightLightEvery;
			return true;
		}
		return false;
	}

	public IEnumerator WaitTillFinished()
	{
		while (!finished)
		{
			yield return null;
		}
	}

	public void Stop()
	{
		canceled = true;
		foreach (ProjectileLauncherView launcher in launchers)
		{
			foreach (ProjectileBurstView burst in launcher.bursts)
			{
				burst.gameObject.SetActive(value: false);
			}
		}
		_onFinishProjectileMediaView?.Stop();
	}
}
