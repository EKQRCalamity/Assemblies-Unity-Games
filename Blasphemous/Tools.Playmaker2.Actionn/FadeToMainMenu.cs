using Framework.Managers;
using Gameplay.UI.Widgets;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Actionn;

[ActionCategory("Blasphemous Action")]
[Tooltip("Fades to main menu")]
public class FadeToMainMenu : FsmStateAction
{
	public bool useFade = true;

	public bool useFadeLoadMenu = true;

	public override void OnEnter()
	{
		if (useFade)
		{
			FadeWidget.instance.Fade(toBlack: true, 1f, 0f, delegate
			{
				Core.Logic.LoadMenuScene(useFadeLoadMenu);
			});
		}
		else
		{
			Core.Logic.LoadMenuScene(useFadeLoadMenu);
		}
		Finish();
	}
}
