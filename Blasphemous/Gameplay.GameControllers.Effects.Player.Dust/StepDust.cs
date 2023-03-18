using Framework.FrameworkCore;
using Framework.Pooling;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class StepDust : PoolObject
{
	public enum StepDustType
	{
		Running,
		StartRun,
		Landing,
		StopRunning,
		Jump,
		Crouch,
		Attack1,
		Attack2,
		FinishingAttack
	}

	private SpriteRenderer _stepDustSpriteRenderer;

	public StepDustType stepDustType;

	public Gameplay.GameControllers.Entities.Entity Owner { get; set; }

	public void SetSpriteOrientation(EntityOrientation orientation)
	{
		if (_stepDustSpriteRenderer == null)
		{
			_stepDustSpriteRenderer = GetComponent<SpriteRenderer>();
		}
		_stepDustSpriteRenderer.flipX = orientation == EntityOrientation.Left;
	}
}
