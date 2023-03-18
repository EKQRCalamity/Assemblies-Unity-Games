using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Enemies.ChainedAngel.Attack;

public class ChainedAngelAttack : EnemyAttack
{
	private bool _targetAttacked;

	private Hit _weaponHit;

	private AttackArea AttackArea { get; set; }

	private ChainedAngel ChainedAngel { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<ChainedAngelWeapon>();
		AttackArea = GetComponentInChildren<AttackArea>();
		ChainedAngel = (ChainedAngel)base.EntityOwner;
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnStay += OnStayAttackArea;
		AttackArea.OnExit += OnExitAttackArea;
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageArea.DamageType.Normal,
			DamageElement = DamageArea.DamageElement.Normal,
			forceGuardslide = true,
			DontSpawnBlood = false,
			Force = Force,
			HitSoundId = HitSound
		};
	}

	private void OnStayAttackArea(object sender, Collider2DParam e)
	{
		if (!_targetAttacked && ChainedAngel.BodyChainMaster.IsAttacking)
		{
			base.CurrentEnemyWeapon.Attack(_weaponHit);
			_targetAttacked = true;
		}
	}

	private void OnExitAttackArea(object sender, Collider2DParam e)
	{
		_targetAttacked = false;
	}

	private void OnDestroy()
	{
		AttackArea.OnStay -= OnStayAttackArea;
		AttackArea.OnExit -= OnExitAttackArea;
	}
}
