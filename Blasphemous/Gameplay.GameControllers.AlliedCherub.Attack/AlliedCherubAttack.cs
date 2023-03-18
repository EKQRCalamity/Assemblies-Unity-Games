using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;

namespace Gameplay.GameControllers.AlliedCherub.Attack;

public class AlliedCherubAttack : EnemyAttack
{
	public AttackArea AttackArea { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		AttackArea.OnEnter += OnEnterAtackArea;
	}

	private void OnEnterAtackArea(object sender, Collider2DParam e)
	{
		base.CurrentEnemyWeapon.Attack(ContactHit);
	}

	protected override void OnStart()
	{
		base.OnStart();
		ContactDamageAmount *= Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final;
	}

	private void OnDestroy()
	{
		AttackArea.OnEnter -= OnEnterAtackArea;
	}
}
