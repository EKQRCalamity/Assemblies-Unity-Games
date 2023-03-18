using System.Collections.Generic;
using System.Linq;
using Framework.Util;
using Rewired;
using Tools.DataContainer;
using UnityEngine;

public class RumbleSystem : SingletonSerialized<RumbleSystem>
{
	private class ActiveRumble
	{
		public RumbleData rumble { get; private set; }

		public float currentTime { get; set; }

		public int currentLoop { get; set; }

		public ActiveRumble(RumbleData rumble)
		{
			this.rumble = rumble;
			currentTime = 0f;
			currentLoop = 0;
		}
	}

	private List<ActiveRumble> activeRumbles = new List<ActiveRumble>();

	private bool forceUpdate;

	public bool RumblesEnabled = true;

	public void Update()
	{
		if (!RumblesEnabled || (activeRumbles.Count == 0 && !forceUpdate))
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		forceUpdate = false;
		foreach (ActiveRumble activeRumble in activeRumbles)
		{
			activeRumble.currentTime += Time.unscaledDeltaTime;
			if (activeRumble.currentTime >= activeRumble.rumble.duration)
			{
				if (!activeRumble.rumble.loop)
				{
					continue;
				}
				activeRumble.currentLoop++;
				if (activeRumble.rumble.loopCount <= 0 || activeRumble.currentLoop <= activeRumble.rumble.loopCount)
				{
					activeRumble.currentTime -= activeRumble.rumble.duration;
				}
			}
			float time = activeRumble.currentTime / activeRumble.rumble.duration;
			if (activeRumble.rumble.type == RumbleData.RumbleType.All || activeRumble.rumble.type == RumbleData.RumbleType.Left)
			{
				num += activeRumble.rumble.left.Evaluate(time);
			}
			if (activeRumble.rumble.type == RumbleData.RumbleType.All || activeRumble.rumble.type == RumbleData.RumbleType.Rigth)
			{
				num2 = ((activeRumble.rumble.type != 0 || !activeRumble.rumble.sameCurve) ? (num2 + activeRumble.rumble.right.Evaluate(time)) : (num2 + activeRumble.rumble.left.Evaluate(time)));
			}
		}
		ReInput.players.GetPlayer(0).SetVibration(0, Mathf.Clamp(num, 0f, 1f));
		ReInput.players.GetPlayer(0).SetVibration(1, Mathf.Clamp(num2, 0f, 1f));
		activeRumbles.RemoveAll((ActiveRumble element) => element.currentTime >= element.rumble.duration);
	}

	public void ApplyRumble(RumbleData rumble)
	{
		ActiveRumble item = new ActiveRumble(rumble);
		activeRumbles.Add(item);
	}

	public void StopRumble(string name)
	{
		activeRumbles.RemoveAll((ActiveRumble element) => element.rumble.name == name);
		forceUpdate = true;
	}

	public void StopAllRumbles()
	{
		activeRumbles.Clear();
		forceUpdate = true;
	}

	public List<string> AppliedRumbles()
	{
		return activeRumbles.Select((ActiveRumble element) => element.rumble.name).ToList();
	}
}
