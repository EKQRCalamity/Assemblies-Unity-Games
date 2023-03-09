using System.Collections.Generic;
using UnityEngine;

public class AbstractCollidableObject : AbstractPausableComponent
{
	private List<CollisionChild> collisionChildren = new List<CollisionChild>();

	protected virtual bool allowCollisionPlayer => true;

	protected virtual bool allowCollisionEnemy => true;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnregisterAllCollisionChildren();
	}

	protected virtual void OnTriggerEnter2D(Collider2D col)
	{
		checkCollision(col, CollisionPhase.Enter);
	}

	protected virtual void OnCollisionEnter2D(Collision2D col)
	{
		checkCollision(col.collider, CollisionPhase.Enter);
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		checkCollision(col, CollisionPhase.Stay);
	}

	protected virtual void OnCollisionStay2D(Collision2D col)
	{
		checkCollision(col.collider, CollisionPhase.Stay);
	}

	protected virtual void OnTriggerExit2D(Collider2D col)
	{
		checkCollision(col, CollisionPhase.Exit);
	}

	protected virtual void OnCollisionExit2D(Collision2D col)
	{
		checkCollision(col.collider, CollisionPhase.Exit);
	}

	protected virtual void checkCollision(Collider2D col, CollisionPhase phase)
	{
		GameObject gameObject = col.gameObject;
		OnCollision(gameObject, phase);
		if (gameObject.CompareTag("Wall"))
		{
			OnCollisionWalls(gameObject, phase);
		}
		else if (gameObject.CompareTag("Ceiling"))
		{
			OnCollisionCeiling(gameObject, phase);
		}
		else if (gameObject.CompareTag("Ground"))
		{
			OnCollisionGround(gameObject, phase);
		}
		else if (gameObject.CompareTag("Enemy"))
		{
			if (allowCollisionEnemy)
			{
				OnCollisionEnemy(gameObject, phase);
			}
		}
		else if (gameObject.CompareTag("EnemyProjectile"))
		{
			OnCollisionEnemyProjectile(gameObject, phase);
		}
		else if (gameObject.CompareTag("Player"))
		{
			if (allowCollisionPlayer)
			{
				OnCollisionPlayer(gameObject, phase);
			}
		}
		else if (gameObject.CompareTag("PlayerProjectile"))
		{
			OnCollisionPlayerProjectile(gameObject, phase);
		}
		else
		{
			OnCollisionOther(gameObject, phase);
		}
	}

	protected virtual void OnCollision(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionWalls(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionCeiling(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionEnemyProjectile(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionPlayerProjectile(GameObject hit, CollisionPhase phase)
	{
	}

	protected virtual void OnCollisionOther(GameObject hit, CollisionPhase phase)
	{
	}

	protected void RegisterCollisionChild(GameObject go)
	{
		CollisionChild component = go.GetComponent<CollisionChild>();
		if (!(component == null))
		{
			RegisterCollisionChild(component);
		}
	}

	public void RegisterCollisionChild(CollisionChild s)
	{
		collisionChildren.Add(s);
		s.OnAnyCollision += OnCollision;
		s.OnWallCollision += OnCollisionWalls;
		s.OnGroundCollision += OnCollisionGround;
		s.OnCeilingCollision += OnCollisionCeiling;
		s.OnPlayerCollision += OnCollisionPlayer;
		s.OnPlayerProjectileCollision += OnCollisionPlayerProjectile;
		s.OnEnemyCollision += OnCollisionEnemy;
		s.OnEnemyProjectileCollision += OnCollisionEnemyProjectile;
		s.OnOtherCollision += OnCollisionOther;
	}

	protected void UnregisterCollisionChild(CollisionChild s)
	{
		if (collisionChildren.Contains(s))
		{
			s.OnAnyCollision -= OnCollision;
			s.OnWallCollision -= OnCollisionWalls;
			s.OnGroundCollision -= OnCollisionGround;
			s.OnCeilingCollision -= OnCollisionCeiling;
			s.OnPlayerCollision -= OnCollisionPlayer;
			s.OnPlayerProjectileCollision -= OnCollisionPlayerProjectile;
			s.OnEnemyCollision -= OnCollisionEnemy;
			s.OnEnemyProjectileCollision -= OnCollisionEnemyProjectile;
			s.OnOtherCollision -= OnCollisionOther;
			collisionChildren.Remove(s);
		}
	}

	protected void UnregisterAllCollisionChildren()
	{
		for (int num = collisionChildren.Count - 1; num >= 0; num--)
		{
			UnregisterCollisionChild(collisionChildren[num]);
		}
	}
}
