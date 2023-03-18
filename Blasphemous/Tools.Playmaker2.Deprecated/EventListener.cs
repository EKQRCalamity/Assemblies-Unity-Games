using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Triggers with an event from the event system.")]
public class EventListener : FsmStateAction
{
	public FsmString eventName;

	public override void OnEnter()
	{
		Core.Events.OnEventLaunched += OnEvent;
	}

	public override void OnExit()
	{
		Core.Events.OnEventLaunched -= OnEvent;
	}

	private void OnEvent(string id, string parameter)
	{
		string text = eventName.Value.ToUpper().Replace(' ', '_');
		if (text == id)
		{
			Finish();
		}
	}
}
