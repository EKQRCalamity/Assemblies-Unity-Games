using Framework.Inventory;
using Framework.Managers;

namespace Tools.Items;

public class PenitentTeleportToPriedieu : ObjectEffect
{
	public string TPOAnimatorState = "RegresoAPuerto";

	protected override bool OnApplyEffect()
	{
		if (Core.Input.HasBlocker("INTERACTABLE"))
		{
			return false;
		}
		Core.Logic.Penitent.Animator.Play(TPOAnimatorState, 0, 0f);
		return true;
	}
}
