using Framework.EditorScripts.BossesBalance;
using Gameplay.GameControllers.Bosses.BurntFace.Rosary;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Environment.Traps.Turrets;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurnfaceBalanceImporter : BossBalanceImporter
{
	[SerializeField]
	protected BossStraightProjectileAttack[] HomingBallAttacks;

	[SerializeField]
	protected BurntFaceBeamAttack[] BeamAttacks;

	protected override void ApplyLoadedStats()
	{
		SetHomingBallsDamage();
		SetBeamAttackDamage();
		SetRosaryBeadsDamage();
	}

	private void SetBeamAttackDamage()
	{
		BurntFaceBeamAttack[] beamAttacks = BeamAttacks;
		foreach (BurntFaceBeamAttack directAttack in beamAttacks)
		{
			SetDirectAttackDamage(directAttack, base.GetHeavyAttackDamage);
		}
	}

	private void SetHomingBallsDamage()
	{
		BossStraightProjectileAttack[] homingBallAttacks = HomingBallAttacks;
		foreach (BossStraightProjectileAttack projectileAttack in homingBallAttacks)
		{
			SetProjectileAttackDamage(projectileAttack, base.GetMediumAttackDamage);
		}
	}

	private void SetRosaryBeadsDamage()
	{
		BasicTurret[] array = Object.FindObjectsOfType<BasicTurret>();
		foreach (BasicTurret projectileAttack in array)
		{
			SetProjectileAttackDamage(projectileAttack, base.GetLightAttackDamage);
		}
	}

	private static void SetDirectAttackDamage(IDirectAttack directAttack, int damage)
	{
		directAttack?.SetDamage(damage);
	}

	private static void SetProjectileAttackDamage(IProjectileAttack projectileAttack, int damage)
	{
		projectileAttack?.SetProjectileWeaponDamage(damage);
	}
}
