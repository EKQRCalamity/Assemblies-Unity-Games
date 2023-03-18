using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Environment.Traps.Turrets;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

[RequireComponent(typeof(BasicTurret))]
public class TurretProjectileIgnoreCollision : MonoBehaviour
{
	public Color collidersColor = new Color(0f, 1f, 1f, 0.5f);

	private BasicTurret turret;

	public List<Collider2D> collidersToIgnore = new List<Collider2D>();

	private void Awake()
	{
		turret = GetComponent<BasicTurret>();
		if (!turret)
		{
			Debug.LogError("Component requires a BasicTurret component!");
			UnityEngine.Object.Destroy(this);
		}
	}

	private void OnEnable()
	{
		if ((bool)turret)
		{
			BasicTurret basicTurret = turret;
			basicTurret.onProjectileFired = (Action<Projectile>)Delegate.Combine(basicTurret.onProjectileFired, new Action<Projectile>(OnProjectileFired));
		}
	}

	private void OnDisable()
	{
		if ((bool)turret)
		{
			BasicTurret basicTurret = turret;
			basicTurret.onProjectileFired = (Action<Projectile>)Delegate.Remove(basicTurret.onProjectileFired, new Action<Projectile>(OnProjectileFired));
		}
	}

	private void OnProjectileFired(Projectile p)
	{
		if (collidersToIgnore.Count == 0)
		{
			return;
		}
		Collider2D componentInChildren = p.GetComponentInChildren<Collider2D>();
		if ((bool)componentInChildren)
		{
			foreach (Collider2D item in collidersToIgnore)
			{
				if ((bool)item)
				{
					Physics2D.IgnoreCollision(item, componentInChildren);
				}
			}
			return;
		}
		Debug.LogWarning("The projectile has no collider?");
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = collidersColor;
		foreach (Collider2D item in collidersToIgnore)
		{
			if ((bool)item)
			{
				Gizmos.DrawCube(base.transform.TransformPoint(item.transform.localPosition + (Vector3)item.offset), item.bounds.size);
			}
		}
	}
}
