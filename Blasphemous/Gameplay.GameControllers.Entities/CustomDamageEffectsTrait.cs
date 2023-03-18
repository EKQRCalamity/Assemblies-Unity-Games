using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class CustomDamageEffectsTrait : Trait
{
	public GameObject fx;

	public GameObject bloodFx;

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(bloodFx, 3);
		PoolManager.Instance.CreatePool(fx, 3);
	}

	public void SpawnDamageEffect(Vector3 position, bool right)
	{
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(fx, position, Quaternion.identity);
		objectInstance.GameObject.GetComponent<SpriteRenderer>().flipX = right;
	}

	public void SpawnBloodEffect(Vector3 position, bool right)
	{
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(bloodFx, position, Quaternion.identity);
		objectInstance.GameObject.GetComponent<SpriteRenderer>().flipX = right;
	}
}
