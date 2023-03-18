using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.HardLandingEffect;

[RequireComponent(typeof(Animator))]
public class HardLandingEffect : PoolObject
{
	public Animator HardLandingEffectAnimator;

	public void PlayLandingEffect()
	{
		HardLandingEffectAnimator.Play("VerticalAttackLandingEffect");
	}

	public void DestroyPrefab()
	{
		Destroy();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
	}
}
