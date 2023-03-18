using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.WallEnemy.Audio;

public class BasicWallEnemyAudio : WallEnemyAudio
{
	private const string AttackEventKey = "WallEnemyAttack";

	private const string WooshEventKey = "WallEnemyWoosh";

	private const string DamageEventKey = "WallEnemyDamage";

	private const string DeathEventKey = "WallEnemyDeath";

	private EventInstance _attackAudioInstance;

	protected override void OnStart()
	{
		base.OnStart();
		Owner.OnDeath += OnOwnerDeath;
	}

	private void OnOwnerDeath()
	{
		StopAttack();
	}

	public override void PlayAttack()
	{
		StopAttack();
		PlayEvent(ref _attackAudioInstance, "WallEnemyAttack");
	}

	public override void PlayWoosh()
	{
		PlayOneShotEvent("WallEnemyWoosh", FxSoundCategory.Damage);
	}

	public override void StopAttack()
	{
		StopEvent(ref _attackAudioInstance);
	}

	public override void PlayDamage()
	{
		PlayOneShotEvent("WallEnemyDamage", FxSoundCategory.Damage);
	}

	public override void PlayDeath()
	{
		PlayOneShotEvent("WallEnemyDeath", FxSoundCategory.Damage);
	}

	private void OnDestroy()
	{
		Owner.OnDeath -= OnOwnerDeath;
	}
}
