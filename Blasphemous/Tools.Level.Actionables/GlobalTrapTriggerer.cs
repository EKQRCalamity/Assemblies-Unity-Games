using UnityEngine;

namespace Tools.Level.Actionables;

public class GlobalTrapTriggerer : MonoBehaviour
{
	public TriggerTrapManager trapManager;

	public string TriggerTrapID = "SHOCK";

	public void Awake()
	{
		if (trapManager == null)
		{
			trapManager = Object.FindObjectOfType<TriggerTrapManager>();
		}
	}

	public void TriggerAllTrapsInTheScene()
	{
		trapManager.Trigger(TriggerTrapID);
	}
}
