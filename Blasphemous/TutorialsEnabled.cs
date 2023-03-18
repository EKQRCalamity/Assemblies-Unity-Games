using Framework.Managers;
using Framework.Util;
using HutongGames.PlayMaker;
using UnityEngine;

public class TutorialsEnabled : FsmStateAction
{
	public FsmEvent tutorialsEnabledEvent;

	public FsmEvent tutorialsDisabledEvent;

	public override void OnEnter()
	{
		if (tutorialsDisabledEvent != null && tutorialsEnabledEvent != null)
		{
			if (Singleton<Core>.Instance != null && Core.TutorialManager != null)
			{
				base.Fsm.Event((!Core.TutorialManager.TutorialsEnabled) ? tutorialsDisabledEvent : tutorialsEnabledEvent);
			}
			else
			{
				Debug.LogError("Can't find TutorialManager.");
			}
		}
		else
		{
			Debug.LogWarning("IsTutorialEnabled: Events are not defined");
		}
	}
}
