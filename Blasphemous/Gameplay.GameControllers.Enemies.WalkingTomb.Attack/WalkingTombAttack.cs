using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Enemies.WalkingTomb.Attack;

public class WalkingTombAttack : EnemyAttack
{
	private Hit GetHit
	{
		get
		{
			Hit result = default(Hit);
			result.HitSoundId = HitSound;
			result.AttackingEntity = base.EntityOwner.gameObject;
			result.DamageAmount = base.EntityOwner.Stats.Strength.Final;
			result.DamageElement = DamageArea.DamageElement.Normal;
			result.DamageType = DamageType;
			result.Force = Force;
			result.Unnavoidable = true;
			return result;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponent<WalkingTombWeapon>();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(GetHit);
	}
}
