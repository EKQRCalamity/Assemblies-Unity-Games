using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class StraightProjectile : Projectile
{
	public bool faceVelocityDirection;

	protected int originalDamage;

	public int OriginalDamage
	{
		get
		{
			return originalDamage;
		}
		set
		{
			originalDamage = value;
		}
	}

	public virtual void Init(Vector3 origin, Vector3 target, float speed)
	{
		Init((target - origin).normalized, speed);
		ResetTrailRendererOnEnable componentInChildren = GetComponentInChildren<ResetTrailRendererOnEnable>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Clean();
		}
	}

	public virtual void Init(Vector3 direction, float speed)
	{
		velocity = direction.normalized * speed;
		if (faceVelocityDirection)
		{
			Vector2 normalized = velocity.normalized;
			float z = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			base.transform.eulerAngles = new Vector3(0f, 0f, z);
		}
	}
}
