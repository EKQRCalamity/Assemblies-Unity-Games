using System;
using System.Collections.Generic;
using BezierSplines;
using Gameplay.GameControllers.Bosses.BlindBaby;
using UnityEngine;

public class BlindBabyPoints : MonoBehaviour
{
	public enum WURM_PATHS
	{
		TO_RIGHT,
		RIGHT_TO_FIX,
		TO_LEFT,
		LEFT_TO_FIX
	}

	[Serializable]
	public struct WickerWurmPathConfig
	{
		public WURM_PATHS pathName;

		public AnimationCurve curve;

		public float duration;

		public BezierSpline spline;

		public WickerWurmBehaviour.WICKERWURM_SIDES side;
	}

	public List<Transform> multiAttackPointsRight;

	public List<Transform> multiAttackPointsLeft;

	public List<WickerWurmPathConfig> paths;

	public List<Transform> GetMultiAttackPoints(WickerWurmBehaviour.WICKERWURM_SIDES side)
	{
		if (side == WickerWurmBehaviour.WICKERWURM_SIDES.LEFT)
		{
			return multiAttackPointsLeft;
		}
		return multiAttackPointsRight;
	}

	public WickerWurmPathConfig GetPathConfig(WURM_PATHS name)
	{
		List<WickerWurmPathConfig> list = paths.FindAll((WickerWurmPathConfig x) => x.pathName == name);
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	public WickerWurmPathConfig GetPathConfig(int index)
	{
		return paths[index];
	}
}
