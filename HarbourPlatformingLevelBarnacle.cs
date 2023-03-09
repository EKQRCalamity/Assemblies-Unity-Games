public class HarbourPlatformingLevelBarnacle : PlatformingLevelShootingEnemy
{
	private void AttackSFX()
	{
		AudioManager.Play("harbour_barnacle_attack");
		emitAudioFromObject.Add("harbour_barnacle_attack");
	}

	protected override void Die()
	{
		AudioManager.Play("harbour_barnacle_death");
		emitAudioFromObject.Add("harbour_barnacle_death");
		base.Die();
	}
}
