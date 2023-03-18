using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.DataContainer;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Rumble the pad")]
public class RumbleStart : FsmStateAction
{
	public RumbleData rumble;

	public override void OnEnter()
	{
		if ((bool)rumble)
		{
			Core.Input.ApplyRumble(rumble);
		}
		Finish();
	}
}
