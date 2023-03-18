using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Sends onSuccess event if the skin is unlocked. Otherwise returns onFailure")]
public class IsSkinUnlock : FsmStateAction
{
	public FsmString skinId;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		bool flag = true;
		flag = Core.ColorPaletteManager.IsColorPaletteUnlocked(skinId.Value);
		base.Fsm.Event((!flag) ? onFailure : onSuccess);
		Finish();
	}
}
