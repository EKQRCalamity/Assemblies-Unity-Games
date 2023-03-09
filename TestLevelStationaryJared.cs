using UnityEngine;

public class TestLevelStationaryJared : LevelProperties.Test.Entity
{
	[SerializeField]
	private Transform childSprite;

	private DamageReceiver damageReceiver;

	public override void LevelInit(LevelProperties.Test properties)
	{
		base.LevelInit(properties);
	}

	private void Start()
	{
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		AudioManager.Play("test_sound_2");
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
	}

	public override void OnParry(AbstractPlayerController player)
	{
		base.OnParry(player);
		player.stats.OnParry();
	}
}
