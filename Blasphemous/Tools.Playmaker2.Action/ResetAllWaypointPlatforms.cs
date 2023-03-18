using System.Collections.Generic;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Resets All Waypoint Platforms.")]
public class ResetAllWaypointPlatforms : FsmStateAction
{
	private List<WaypointsMovingPlatform> platforms = new List<WaypointsMovingPlatform>();

	public override void OnEnter()
	{
		if (platforms.Count == 0)
		{
			platforms = new List<WaypointsMovingPlatform>(Object.FindObjectsOfType<WaypointsMovingPlatform>());
		}
		foreach (WaypointsMovingPlatform platform in platforms)
		{
			platform.ResetPlatform();
		}
		Finish();
	}
}
