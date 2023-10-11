using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncherView : MonoBehaviour
{
	private static GameObject _BurstBlueprint;

	public Action<ProjectileLauncherView> onFinish;

	private List<ProjectileBurstView> _bursts = new List<ProjectileBurstView>();

	private Action<ProjectileBurstView> _onBurstFinished;

	protected static GameObject BurstBlueprint
	{
		get
		{
			if (!_BurstBlueprint)
			{
				return _BurstBlueprint = Resources.Load<GameObject>("GameState/Ability/Projectiles/ProjectileBurstView");
			}
			return _BurstBlueprint;
		}
	}

	protected Action<ProjectileBurstView> onBurstFinished => _RemoveBurst;

	public PoolKeepItemListHandle<ProjectileBurstView> bursts => Pools.UseKeepItemList(_bursts);

	private void _RemoveBurst(ProjectileBurstView burst)
	{
		if (_bursts.Remove(burst) && _bursts.Count == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		_bursts.Clear();
		onFinish?.Invoke(this);
		onFinish = null;
	}

	public ProjectileLauncherView SetData(System.Random random, ProjectileMediaView view, ProjectileMediaData data, Transform launchTransform, List<Transform> targets)
	{
		int num = random.RangeInt(data.launch.parameters.burstCount);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			float num3 = random.Range(data.launch.parameters.timeBetweenBursts);
			int num4 = MathUtil.RoundToNearestMultipleOfInt(Math.Max(targets.Count, ProjectileMediaView.GetModifiedLaunchCount(random.RangeInt(data.launch.parameters.projectilesPerBurst))), targets.Count);
			int numberOfProjectiles = num4 / targets.Count;
			ProjectileLaunchPatternNodes nodes = data.launch.parameters.shape.GetNodes(random, num4);
			PoolListHandle<ProjectileLaunchPatternNodes> poolListHandle = Pools.UseList<ProjectileLaunchPatternNodes>();
			for (int j = 0; j < targets.Count; j++)
			{
				poolListHandle.Add(data.impact.parameters.shape.GetNodes(random, numberOfProjectiles));
			}
			ProjectileBurstView projectileBurstView = Pools.Unpool(BurstBlueprint, base.transform.position, base.transform.rotation, base.transform.parent).GetComponent<ProjectileBurstView>().SetData(num2, num3, random, view, data, launchTransform, nodes, targets, poolListHandle);
			projectileBurstView.onFinish = onBurstFinished;
			_bursts.Add(projectileBurstView);
			num2 += num3;
		}
		return this;
	}
}
