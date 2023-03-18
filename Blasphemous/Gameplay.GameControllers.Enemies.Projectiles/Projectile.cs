using System;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class Projectile : Entity
{
	public Entity owner;

	public Vector2 velocity;

	public bool flipRenderer;

	public bool useOrientation;

	public float timeToLive = 10f;

	protected float _currentTTL;

	public event Action<Projectile> OnLifeEndedEvent;

	public void ResetTTL()
	{
		_currentTTL = timeToLive;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		UpdateOrientation();
		base.transform.Translate(velocity * Time.deltaTime, Space.World);
		_currentTTL -= Time.deltaTime;
		if (_currentTTL < 0f)
		{
			OnLifeEnded();
		}
	}

	protected void UpdateOrientation()
	{
		if (useOrientation)
		{
			EntityOrientation orientation = ((!(velocity.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
			SetOrientation(orientation, flipRenderer);
		}
	}

	protected void OnLifeEnded()
	{
		if (this.OnLifeEndedEvent != null)
		{
			this.OnLifeEndedEvent(this);
		}
	}
}
