using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.GuardSlide;

public class GuardSlideImpact : PoolObject
{
	public SpriteRenderer SpriteRenderer { get; private set; }

	private void Awake()
	{
		SpriteRenderer = GetComponent<SpriteRenderer>();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		SpriteRenderer.flipX = Core.Logic.Penitent.Status.Orientation == EntityOrientation.Left;
	}

	public void Dispose()
	{
		Destroy();
	}
}
