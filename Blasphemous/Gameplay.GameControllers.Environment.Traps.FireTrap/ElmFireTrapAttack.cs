using Framework.Util;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

public class ElmFireTrapAttack : MonoBehaviour
{
	public Hit ProximityHitAttack;

	private CircleAttackArea AttackArea;

	private void Awake()
	{
		AttackArea = GetComponentInChildren<CircleAttackArea>();
		AttackArea.OnEnter += OnEnterAttackArea;
	}

	private void OnEnterAttackArea(object sender, Collider2DParam e)
	{
		IDamageable componentInParent = e.Collider2DArg.GetComponentInParent<IDamageable>();
		ProximityAttack(componentInParent);
	}

	private void ProximityAttack(IDamageable damageable)
	{
		damageable?.Damage(ProximityHitAttack);
	}

	private void OnDestroy()
	{
		if ((bool)AttackArea)
		{
			AttackArea.OnEnter -= OnEnterAttackArea;
		}
	}
}
