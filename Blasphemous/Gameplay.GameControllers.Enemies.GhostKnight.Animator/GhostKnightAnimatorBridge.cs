using Gameplay.GameControllers.Enemies.GhostKnight.AI;
using Gameplay.GameControllers.Enemies.GhostKnight.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GhostKnight.Animator;

public class GhostKnightAnimatorBridge : MonoBehaviour
{
	public GhostKnightAttack GhostAttack { get; private set; }

	public GhostKnight GhostKnight { get; private set; }

	private void Awake()
	{
		GhostAttack = base.transform.root.GetComponentInChildren<GhostKnightAttack>();
		GhostKnight = GhostAttack.GetComponentInParent<GhostKnight>();
	}

	public void PlayStartAttack()
	{
		GhostKnight.Audio.StartAttack();
	}

	public void Attack()
	{
		if (!(GhostKnight == null))
		{
			GhostKnight.EnemyBehaviour.Attack();
		}
	}

	public void SwordHit(DamageArea.DamageType damageType)
	{
		if (!(GhostAttack == null))
		{
			GhostAttack.CurrentWeaponAttack(damageType);
		}
	}

	public void Dissappear()
	{
		if (!(GhostKnight == null))
		{
			GhostKnightBehaviour componentInChildren = GhostKnight.GetComponentInChildren<GhostKnightBehaviour>();
			componentInChildren.Disappear(componentInChildren.TimeBecomeInVisible);
		}
	}

	public void DestroyEnemy()
	{
		Object.Destroy(GhostKnight.gameObject);
	}
}
