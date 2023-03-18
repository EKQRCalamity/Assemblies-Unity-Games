using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Penitent.Abilities;

namespace Gameplay.GameControllers.Penitent.States;

public class Driven : State
{
	public override void OnStateEnter()
	{
		base.OnStateEnter();
		StopCastAbilities();
		SetInvincible();
		Core.Logic.Penitent.PlatformCharacterInput.ResetInputs();
		Core.Logic.Penitent.PlatformCharacterInput.ResetActions();
	}

	public override void Update()
	{
		base.Update();
		if (Core.Logic.Penitent.Status.IsGrounded)
		{
			Core.Logic.Penitent.AnimatorInyector.enabled = false;
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		Core.Logic.Penitent.AnimatorInyector.enabled = true;
		SetInvincible(enableDamageArea: false);
	}

	private void StopCastAbilities()
	{
		Ability[] componentsInChildren = Core.Logic.Penitent.GetComponentsInChildren<Ability>();
		Ability[] array = componentsInChildren;
		foreach (Ability ability in array)
		{
			if (!(ability is DrivePlayer))
			{
				ability.StopCast();
			}
		}
	}

	private void SetInvincible(bool enableDamageArea = true)
	{
		Core.Logic.Penitent.Status.Unattacable = enableDamageArea;
	}
}
