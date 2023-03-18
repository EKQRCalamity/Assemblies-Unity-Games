using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.FrameworkCore;

public class FpsLimiter : MonoBehaviour
{
	private float OldTime;

	private float TargetDeltaTime;

	private float CurTime;

	private float TimeTaken;

	[PropertyRange(1.0, 100.0)]
	[OnValueChanged("UpdateTargetDeltaTime", false)]
	public int TargetFrameRate = 60;

	private void Start()
	{
		TargetFrameRate = 60;
		UpdateTargetDeltaTime();
		OldTime = Time.realtimeSinceStartup;
		SetTargetFrameRate(60);
	}

	private void UpdateTargetDeltaTime()
	{
		TargetDeltaTime = 1f / (float)TargetFrameRate;
	}

	private void Update()
	{
		if (QualitySettings.vSyncCount == 0)
		{
			SetTargetFrameRate(60);
		}
	}

	private void LateUpdate()
	{
		if (QualitySettings.vSyncCount != 0)
		{
			ForceDeltaTimeDelay();
		}
	}

	private static void SetTargetFrameRate(int targetFrameRate)
	{
		if (Application.targetFrameRate != targetFrameRate)
		{
			Application.targetFrameRate = targetFrameRate;
		}
	}

	private void ForceDeltaTimeDelay()
	{
		CurTime = Time.realtimeSinceStartup;
		TimeTaken = CurTime - OldTime;
		if (TimeTaken < TargetDeltaTime)
		{
			Thread.Sleep((int)(1000f * (TargetDeltaTime - TimeTaken)));
		}
		OldTime = Time.realtimeSinceStartup;
	}
}
