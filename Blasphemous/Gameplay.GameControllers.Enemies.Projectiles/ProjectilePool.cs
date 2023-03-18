using System.Collections.Generic;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class ProjectilePool : MonoBehaviour
{
	public Enemy enemyOwner;

	public Projectile projectilePrefab;

	private readonly List<Projectile> instantiatedProjectiles = new List<Projectile>();

	public Projectile Spawn(Vector3 position, Entity owner)
	{
		Projectile projectile = null;
		if (instantiatedProjectiles.Count > 0)
		{
			projectile = instantiatedProjectiles[instantiatedProjectiles.Count - 1];
			instantiatedProjectiles.Remove(projectile);
			projectile.gameObject.SetActive(value: true);
			projectile.transform.position = position;
			projectile.transform.rotation = Quaternion.identity;
		}
		else
		{
			projectile = Object.Instantiate(projectilePrefab, position, Quaternion.identity);
		}
		projectile.owner = owner;
		return projectile;
	}

	public void StoreProjectile(Projectile p)
	{
		if (!instantiatedProjectiles.Contains(p))
		{
			instantiatedProjectiles.Add(p);
			p.gameObject.SetActive(value: false);
		}
	}

	public void Initialize(int n)
	{
		for (int i = 0; i < n; i++)
		{
			Projectile p = Object.Instantiate(projectilePrefab, base.transform.position, Quaternion.identity);
			StoreProjectile(p);
		}
	}

	private void OnDestroy()
	{
		if (instantiatedProjectiles.Count > 0)
		{
			instantiatedProjectiles.Clear();
		}
	}
}
