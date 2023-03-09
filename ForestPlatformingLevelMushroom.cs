public class ForestPlatformingLevelMushroom : PlatformingLevelShootingEnemy
{
	protected override void Awake()
	{
		base.Awake();
		ForestPlatformingLevelMushroomProjectile.numUntilPink = base.Properties.MushroomPinkNumber.RandomInt();
	}

	protected override void Shoot()
	{
		base.Shoot();
	}

	private void EmergeFromGround()
	{
		setDirection((_target.center.x > base.transform.position.x) ? Direction.Right : Direction.Left);
	}

	private void PlayMushroomSound()
	{
		AudioManager.Play("level_mushroom_shoot");
		emitAudioFromObject.Add("level_mushroom_shoot");
	}

	protected override void Die()
	{
		AudioManager.Play("level_mermaid_turtle_shell_pop");
		emitAudioFromObject.Add("level_mermaid_turtle_shell_pop");
		base.Die();
	}
}
