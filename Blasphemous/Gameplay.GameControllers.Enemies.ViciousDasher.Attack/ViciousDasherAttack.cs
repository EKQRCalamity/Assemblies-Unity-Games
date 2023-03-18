using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Enemies.ViciousDasher.Attack;

public class ViciousDasherAttack : EnemyAttack
{
	private Hit GetHit
	{
		get
		{
			Hit result = default(Hit);
			result.AttackingEntity = base.EntityOwner.gameObject;
			result.DamageAmount = base.EntityOwner.Stats.Strength.Final;
			result.DamageType = DamageArea.DamageType.Normal;
			result.HitSoundId = HitSound;
			return result;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<ViciousDasherWeapon>();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(GetHit);
	}
}
