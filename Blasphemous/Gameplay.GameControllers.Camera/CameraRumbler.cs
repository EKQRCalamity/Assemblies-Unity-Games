using System;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.Managers;
using Tools.DataContainer;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

public class CameraRumbler : MonoBehaviour
{
	public RumbleData ShortRumble;

	public RumbleData NormalRumble;

	public RumbleData LongRumble;

	private float RumbleCoolDown { get; set; }

	private void Start()
	{
		ProCamera2DShake proCamera2DShake = Core.Logic.CameraManager.ProCamera2DShake;
		proCamera2DShake.OnShakeStarted = (Action<float>)Delegate.Combine(proCamera2DShake.OnShakeStarted, new Action<float>(OnShakeStarted));
		RumbleCoolDown = 0f;
	}

	private void Update()
	{
		RumbleCoolDown -= Time.deltaTime;
	}

	private void OnShakeStarted(float duration)
	{
		if (RumbleCoolDown > 0f)
		{
			return;
		}
		if (duration <= ShortRumble.duration)
		{
			if (ShortRumble != null)
			{
				if (Core.Input.AppliedRumbles().Count == 0)
				{
					Core.Input.ApplyRumble(ShortRumble);
					RumbleCoolDown = ShortRumble.duration;
				}
			}
			else
			{
				Debug.LogError("CameraRumbler::OnShakeStarted: ShortRumble is null!");
			}
		}
		else if (duration <= NormalRumble.duration)
		{
			if (NormalRumble != null)
			{
				if (Core.Input.AppliedRumbles().Count == 0)
				{
					Core.Input.ApplyRumble(NormalRumble);
					RumbleCoolDown = NormalRumble.duration;
				}
			}
			else
			{
				Debug.LogError("CameraRumbler::OnShakeStarted: NormalRumble is null!");
			}
		}
		else if (LongRumble != null)
		{
			if (Core.Input.AppliedRumbles().Count == 0)
			{
				Core.Input.ApplyRumble(LongRumble);
				RumbleCoolDown = LongRumble.duration;
			}
		}
		else
		{
			Debug.LogError("CameraRumbler::OnShakeStarted: LongRumble is null!");
		}
	}

	private void OnDestroy()
	{
		if ((bool)Core.Logic.CameraManager)
		{
			ProCamera2DShake proCamera2DShake = Core.Logic.CameraManager.ProCamera2DShake;
			proCamera2DShake.OnShakeStarted = (Action<float>)Delegate.Remove(proCamera2DShake.OnShakeStarted, new Action<float>(OnShakeStarted));
		}
	}
}
