using BezierSplines;
using Gameplay.GameControllers.Enemies.PatrollingFlyingEnemy;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Tools.Level.Utils;

public class FlyingPatrollingEnemySpawnConfigurator : EnemySpawnConfigurator
{
	public BezierSpline path;

	public AnimationCurve curve;

	public float secondsToCompletePatrol = 2f;

	protected override void OnSpawn(Enemy e)
	{
		base.OnSpawn(e);
		((PatrollingFlyingEnemy)e).SetConfig(path, curve, secondsToCompletePatrol);
	}
}
