using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Raises an event on the event system.")]
public class LaunchEvent : FsmStateAction
{
	public FsmString eventName;

	public override void OnEnter()
	{
		Core.Events.LaunchEvent(eventName.Value, string.Empty);
		Finish();
	}
}
