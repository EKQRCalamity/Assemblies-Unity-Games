using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Swimmer.Animator;

public class SwimmerTerrainEffect : PoolObject
{
	protected UnityEngine.Animator Animator;

	private void Awake()
	{
		Animator = GetComponent<UnityEngine.Animator>();
	}

	public void RisingEffect(bool rising)
	{
		if (Animator != null)
		{
			Animator.SetBool("RISING", rising);
		}
	}
}
