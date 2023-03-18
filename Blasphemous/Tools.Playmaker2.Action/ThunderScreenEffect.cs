using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Sets the screen to a black & white effect for a defined time.")]
public class ThunderScreenEffect : FsmStateAction
{
	public FsmFloat effectTime;

	public override void OnEnter()
	{
		Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(effectTime.Value);
		Finish();
	}
}
