using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.GhostTrap;

public class GhostTriggerEffect : PoolObject
{
	public int NumAnimations = 3;

	protected Animator Animator { get; set; }

	private void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		PlayRandomAnim();
	}

	public void PlayRandomAnim()
	{
		int value = Random.Range(1, NumAnimations + 1);
		Animator.SetInteger("FLY_PAGES", value);
		Animator.speed = Random.Range(0.5f, 1f);
	}

	public void ResetAnimatorParameter()
	{
		Animator.SetInteger("FLY_PAGES", 0);
		Destroy();
	}
}
