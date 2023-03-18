using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.DataContainer;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Stop a started rumble")]
public class RumbleStop : FsmStateAction
{
	public FsmBool allRumbles;

	public RumbleData rumble;

	public override void Reset()
	{
		if (allRumbles != null)
		{
			allRumbles.Value = false;
		}
	}

	public override void OnEnter()
	{
		bool flag = allRumbles != null && allRumbles.Value;
		if (rumble == null && !flag)
		{
			LogWarning("PlayMaker Action Rumble Stop - Rumble is blank");
		}
		else if (flag)
		{
			Core.Input.StopAllRumbles();
		}
		else
		{
			Core.Input.StopRumble(rumble.name);
		}
		Finish();
	}
}
