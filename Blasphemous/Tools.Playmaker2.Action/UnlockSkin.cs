using Framework.Managers;
using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Unlocks a Skin.")]
public class UnlockSkin : FsmStateAction
{
	[RequiredField]
	public FsmString skinId;

	public override void OnEnter()
	{
		Core.ColorPaletteManager.UnlockColorPalette(skinId.Value);
	}

	public override void OnUpdate()
	{
		if (!UIController.instance.IsUnlockActive())
		{
			Finish();
		}
	}
}
