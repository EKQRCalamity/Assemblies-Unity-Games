using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;

namespace Gameplay.GameControllers.Enemies.Roller.Attack;

public class AxeRollerMeleeAttack : EnemyAttack, IDirectAttack
{
	[FoldoutGroup("Additional attack settings", 0)]
	public bool unavoidable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool unparriable;

	[FoldoutGroup("Additional attack settings", 0)]
	public bool unblockable;

	[FoldoutGroup("Additional attack settings", 0)]
	public float damage;

	public bool damageOnEnterArea;

	public Core.SimpleEvent OnAttackGuarded;

	public Hit WeaponHit { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		CreateHit();
		base.CurrentEnemyWeapon = GetComponentInChildren<AxeRollerMeleeWeapon>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon.AttackAreas[0].OnEnter += OnEnterAttackArea;
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		if (damageOnEnterArea)
		{
			CurrentWeaponAttack();
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(WeaponHit);
	}

	private void OnGuardCallback(Hit obj)
	{
		if (OnAttackGuarded != null)
		{
			OnAttackGuarded();
		}
	}

	public void CreateHit()
	{
		WeaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Unnavoidable = unavoidable,
			Unparriable = unparriable,
			Unblockable = unblockable,
			HitSoundId = HitSound,
			Force = Force,
			OnGuardCallback = OnGuardCallback
		};
	}

	public void SetDamage(int damage)
	{
		if (damage >= 0)
		{
			this.damage = damage;
			CreateHit();
		}
	}
}
