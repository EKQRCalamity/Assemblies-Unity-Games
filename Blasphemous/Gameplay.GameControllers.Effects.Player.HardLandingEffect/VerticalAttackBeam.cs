using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.HardLandingEffect;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class VerticalAttackBeam : PoolObject
{
	public Animator VerticalBeamAnimator;

	public SpriteRenderer VerticalBeamRenderer;

	public void DestroyPrefab()
	{
		Destroy();
	}

	public void Pause()
	{
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
	}
}
