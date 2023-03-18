using Framework.Managers;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Framework.Inventory;

public class PowerSlashesSwordHeartEffect : ObjectEffect
{
	private enum SlashColors
	{
		Blue,
		Red
	}

	[SerializeField]
	private SlashColors slashColor;

	private int prevAttackLevel;

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override bool OnApplyEffect()
	{
		SetPowerSlashes();
		return true;
	}

	protected override void OnRemoveEffect()
	{
		UnsetPowerSlashes();
		base.OnRemoveEffect();
	}

	private void SetPowerSlashes()
	{
		prevAttackLevel = Core.Logic.Penitent.PenitentAttack.CurrentLevel;
		Core.Logic.Penitent.PenitentAttack.CurrentLevel = 2;
		SetSlashColor(slashColor);
	}

	private void UnsetPowerSlashes()
	{
		Core.Logic.Penitent.PenitentAttack.CurrentLevel = prevAttackLevel;
		PenitentSword penitentSword = Core.Logic.Penitent.PenitentAttack.CurrentPenitentWeapon as PenitentSword;
		if ((bool)penitentSword)
		{
			penitentSword.SlashAnimator.ResetParameters();
		}
	}

	private void SetSlashColor(SlashColors slashColor)
	{
		PenitentSword.AttackColor attackColor = PenitentSword.AttackColor.Default;
		switch (slashColor)
		{
		case SlashColors.Blue:
			attackColor = PenitentSword.AttackColor.Default;
			break;
		case SlashColors.Red:
			attackColor = PenitentSword.AttackColor.Red;
			break;
		}
		Core.Logic.Penitent.PenitentAttack.AttackColor = attackColor;
	}
}
