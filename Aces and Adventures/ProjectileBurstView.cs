using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBurstView : MonoBehaviour
{
	public Action<ProjectileBurstView> onFinish;

	private int _projectileIndex;

	private int _projectileCount;

	private float _duration;

	private ProjectileMediaView _view;

	private ProjectileMediaData _data;

	private System.Random _random;

	private Transform _launchTransform;

	private ProjectileLaunchPatternNodes _launchNodes;

	private List<Transform> _targets = new List<Transform>();

	private PoolListHandle<ProjectileLaunchPatternNodes> _impactNodes;

	private List<ProjectileFlightSFX> _projectiles = new List<ProjectileFlightSFX>();

	private float _elapsedTime;

	private Action<ProjectileFlightSFX> _onProjectileFinish;

	public float timeBetweenLaunches => _duration / (float)_projectileCount;

	protected Action<ProjectileFlightSFX> onProjectileFinish => _RemoveProjectile;

	private ProjectileFlightSFX _LaunchProjectile(PositionRotation launchFrom, Transform target, PositionRotation impactAt)
	{
		int num = _random.Next(0, _data.flight.media.Count);
		int num2 = _random.Next(0, _data.flight.media[num].visuals.Count);
		return Pools.Unpool(_data.flight.media[num].visuals[num2].GetBlueprint(), launchFrom.position, null, base.transform.parent).GetComponent<ProjectileFlightSFX>().SetData(_random, _view, _data, _launchTransform, target, launchFrom, impactAt, launchFrom.forward, Mathf.Max(0f, _elapsedTime), num, num2, _projectileIndex++);
	}

	private void _RemoveProjectile(ProjectileFlightSFX projectile)
	{
		if (_projectiles.Remove(projectile) && _projectiles.Count == 0 && _launchNodes.Count == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		_elapsedTime += Time.deltaTime;
		while (_elapsedTime >= timeBetweenLaunches && (bool)_launchNodes && _view.isActiveAndEnabled)
		{
			_elapsedTime -= timeBetweenLaunches;
			int index = _random.Next(_targets.Count);
			Transform transform = _targets[index];
			ProjectileLaunchPatternNodes projectileLaunchPatternNodes = _impactNodes[index];
			ProjectileFlightSFX projectileFlightSFX = _LaunchProjectile(_launchNodes.GetNextPositionRotation(new PositionRotation(_launchTransform.position, _view.startTransform.rotation)) + _view.startTransform.rotation * _view.startData.positionOffset.GetOffset(_random), transform, projectileLaunchPatternNodes.GetNextPositionRotation(transform) + _view.endTransform.rotation * _view.endData.positionOffset.GetOffset(_random));
			if (projectileLaunchPatternNodes.Count == 0)
			{
				_targets.RemoveAt(index);
				_impactNodes.RemoveAt(index);
			}
			projectileFlightSFX.onFinish = onProjectileFinish;
			_projectiles.Add(projectileFlightSFX);
			projectileFlightSFX.Update();
		}
	}

	private void OnDisable()
	{
		_projectileIndex = 0;
		Pools.Repool(ref _launchNodes);
		_launchTransform = null;
		_projectiles.Clear();
		_targets.Clear();
		Pools.Repool(ref _impactNodes);
		_random = null;
		_view = null;
		_data = null;
		_elapsedTime = 0f;
		onFinish?.Invoke(this);
		onFinish = null;
	}

	public ProjectileBurstView SetData(float startDelay, float duration, System.Random random, ProjectileMediaView view, ProjectileMediaData data, Transform launchTransform, ProjectileLaunchPatternNodes launchNodes, List<Transform> targets, PoolListHandle<ProjectileLaunchPatternNodes> impactNodes)
	{
		_view = view;
		_data = data;
		_random = random;
		_launchTransform = launchTransform;
		_launchNodes = launchNodes;
		_targets = _targets.ClearAndCopyFrom(targets);
		_impactNodes = impactNodes;
		_projectileCount = launchNodes.Count;
		_duration = duration * random.Range(data.launch.parameters.projectileTimeSpacing);
		_elapsedTime = timeBetweenLaunches - startDelay;
		return this;
	}
}
