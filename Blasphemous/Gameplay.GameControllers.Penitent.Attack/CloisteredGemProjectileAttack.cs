using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Attack;

public class CloisteredGemProjectileAttack : BossStraightProjectileAttack
{
	public bool useStrengthDamageBonus;

	public float currentPenitentStregth;

	private Penitent penitent;

	protected override void OnStart()
	{
		base.OnStart();
		if (useStrengthDamageBonus)
		{
			penitent = Core.Logic.Penitent;
			SetProjectileDamage();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Abs(penitent.Stats.Strength.Final - currentPenitentStregth) > Mathf.Epsilon)
		{
			SetProjectileDamage();
		}
	}

	private void SetProjectileDamage()
	{
		currentPenitentStregth = penitent.Stats.Strength.Final;
		int projectileWeaponDamage = (int)((float)ProjectileDamageAmount + currentPenitentStregth);
		SetProjectileWeaponDamage(projectileWeaponDamage);
	}

	private static float Abs(float num)
	{
		return (!(num < 0f)) ? num : (0f - num);
	}
}
