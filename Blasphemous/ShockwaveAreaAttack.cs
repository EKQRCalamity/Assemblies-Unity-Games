using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;

public class ShockwaveAreaAttack : EnemyAttack
{
	private Hit _areaAttackHit;

	private const string PENITENT_TAG = "Penitent";

	[BoxGroup("Shockwave damage settings", true, false, 0)]
	public int damage = 25;

	[BoxGroup("Shockwave damage settings", true, false, 0)]
	public bool unavoidable = true;

	[BoxGroup("Shockwave damage settings", true, false, 0)]
	public bool forceGuardslide = true;

	public AttackArea circleArea { get; set; }

	public bool TargetInAttackArea { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();
		base.CurrentEnemyWeapon = GetComponentInChildren<Weapon>();
		circleArea = GetComponentInChildren<AttackArea>();
		circleArea.OnEnter += OnEnterAttackArea;
		circleArea.OnExit += OnExitAttackArea;
		Entity entityOwner = base.EntityOwner;
		_areaAttackHit = new Hit
		{
			AttackingEntity = entityOwner.gameObject,
			DamageAmount = damage,
			DamageType = DamageType,
			DamageElement = DamageElement,
			HitSoundId = HitSound,
			Unnavoidable = unavoidable,
			forceGuardslide = forceGuardslide,
			ThrowbackDirByOwnerPosition = true
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		base.CurrentEnemyWeapon.Attack(_areaAttackHit);
	}

	private void OnExitAttackArea(object sender, Collider2DParam e)
	{
	}

	private void OnDestroy()
	{
		if ((bool)circleArea)
		{
			circleArea.OnEnter -= OnEnterAttackArea;
		}
		if ((bool)circleArea)
		{
			circleArea.OnExit -= OnExitAttackArea;
		}
	}
}
