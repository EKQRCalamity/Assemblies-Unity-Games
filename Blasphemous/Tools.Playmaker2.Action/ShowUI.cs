using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Show or hide ingame UI.")]
public class ShowUI : FsmStateAction
{
	public FsmBool show;

	public override void OnEnter()
	{
		bool showGamePlayUI = show != null && show.Value;
		Core.UI.ShowGamePlayUI = showGamePlayUI;
		Finish();
	}
}
