using UnityEngine;

public class ForestPlatformingLevelLobber : PlatformingLevelShootingEnemy
{
	protected override void Awake()
	{
		base.Awake();
		base.animator.Play("Idle", 0, Random.Range(0f, 1f));
	}

	protected override void Shoot()
	{
		base.Shoot();
	}

	private void PlayLobberSound()
	{
		AudioManager.Play("level_forestlobber_shoot");
		emitAudioFromObject.Add("level_forestlobber_shoot");
	}

	protected override void Die()
	{
		AudioManager.Play("level_mermaid_turtle_shell_pop");
		emitAudioFromObject.Add("level_mermaid_turtle_shell_pop");
		FrameDelayedCallback(Kill, 1);
	}

	private void Kill()
	{
		base.Die();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_shootEffect = null;
	}
}
