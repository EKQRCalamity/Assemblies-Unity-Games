using Framework.Managers;
using Gameplay.GameControllers.Bosses.HighWills.Attack;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Attack;

public class PontiffHuskRangedVariantAttack : EnemyAttack
{
	public GameObject MinePrefab;

	public GameObject LeftShootingPoint;

	public GameObject RightShootingPoint;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(MinePrefab, 6);
	}

	public RangedMine Shoot()
	{
		Vector3 position = ((!base.EntityOwner.SpriteRenderer.flipX) ? LeftShootingPoint.transform.position : RightShootingPoint.transform.position);
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(MinePrefab, position, Quaternion.Euler(0f, 0f, -90f));
		return objectInstance.GameObject.GetComponent<RangedMine>();
	}
}
