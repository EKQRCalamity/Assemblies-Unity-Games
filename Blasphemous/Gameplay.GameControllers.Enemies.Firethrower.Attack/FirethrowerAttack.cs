using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Firethrower.Attack;

public class FirethrowerAttack : EnemyAttack
{
	public enum FIRE_LEVEL
	{
		NONE,
		START,
		GROWING,
		LOOP
	}

	private Hit _weaponHit;

	private Collider2D fireStartCollider;

	private Collider2D fireMainCollider;

	private Collider2D fireEndCollider;

	[Header("FireThrower specific")]
	public AttackArea fireStartArea;

	public AttackArea fireMainArea;

	public AttackArea fireEndArea;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<FirethrowerWeapon>();
		fireStartCollider = fireStartArea.GetComponent<Collider2D>();
		fireMainCollider = fireMainArea.GetComponent<Collider2D>();
		fireEndCollider = fireEndArea.GetComponent<Collider2D>();
		fireStartArea.OnStay += OnStayFireArea;
		fireMainArea.OnStay += OnStayFireArea;
		fireEndArea.OnStay += OnStayFireArea;
	}

	protected override void OnStart()
	{
		base.OnStart();
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageType,
			DamageElement = DamageElement,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public void SetFireLevel(FIRE_LEVEL fireLevel)
	{
		switch (fireLevel)
		{
		case FIRE_LEVEL.START:
			fireStartCollider.enabled = true;
			fireMainCollider.enabled = false;
			fireEndCollider.enabled = false;
			break;
		case FIRE_LEVEL.GROWING:
			fireStartCollider.enabled = true;
			fireMainCollider.enabled = true;
			fireEndCollider.enabled = false;
			break;
		case FIRE_LEVEL.LOOP:
			fireStartCollider.enabled = true;
			fireMainCollider.enabled = true;
			fireEndCollider.enabled = true;
			break;
		case FIRE_LEVEL.NONE:
			fireStartCollider.enabled = false;
			fireMainCollider.enabled = false;
			fireEndCollider.enabled = false;
			break;
		}
	}

	private void OnStayFireArea(object sender, Collider2DParam e)
	{
		CurrentWeaponAttack();
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_weaponHit);
	}
}
