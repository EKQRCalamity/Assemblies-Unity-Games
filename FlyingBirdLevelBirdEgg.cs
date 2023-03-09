using UnityEngine;

public class FlyingBirdLevelBirdEgg : AbstractProjectile
{
	private enum State
	{
		Idle,
		Exploded
	}

	private const float ANGLE = 45f;

	[SerializeField]
	private BasicProjectile childPrefab;

	[SerializeField]
	private Effect effectPrefab;

	private float speed;

	private State state;

	private int maxProjectiles;

	public virtual AbstractProjectile Create(float speed, Vector2 pos)
	{
		FlyingBirdLevelBirdEgg flyingBirdLevelBirdEgg = Create(pos, 0f) as FlyingBirdLevelBirdEgg;
		flyingBirdLevelBirdEgg.speed = 0f - speed;
		flyingBirdLevelBirdEgg.CollisionDeath.OnlyPlayer();
		flyingBirdLevelBirdEgg.DamagesType.OnlyPlayer();
		return flyingBirdLevelBirdEgg;
	}

	protected override void Start()
	{
		base.Start();
		switch (Level.Current.mode)
		{
		case Level.Mode.Easy:
			maxProjectiles = 2;
			break;
		case Level.Mode.Normal:
			maxProjectiles = 3;
			break;
		case Level.Mode.Hard:
			maxProjectiles = 5;
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		base.transform.position += base.transform.right * speed * CupheadTime.Delta;
		if (state == State.Idle && base.transform.position.x < -640f)
		{
			Explode();
			Die();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
			Die();
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void Explode()
	{
		AudioManager.Play("level_flying_bird_egg_explode");
		emitAudioFromObject.Add("level_flying_bird_egg_explode");
		AudioManager.Play("level_flying_bird_egg_break");
		emitAudioFromObject.Add("level_flying_bird_egg_break");
		if (state != 0)
		{
			return;
		}
		state = State.Exploded;
		effectPrefab.Create(base.transform.position);
		if (maxProjectiles == 0)
		{
			return;
		}
		Vector3 position = base.transform.position;
		position.x += 42f;
		if (maxProjectiles == 2)
		{
			childPrefab.Create(position, 90f, Vector2.one, 0f - speed);
			childPrefab.Create(position, -90f, Vector2.one, 0f - speed);
			return;
		}
		for (int i = 0; i < maxProjectiles; i++)
		{
			float num = 0f;
			childPrefab.Create(position, i switch
			{
				1 => -45f, 
				2 => 45f, 
				3 => 90f, 
				4 => -90f, 
				_ => 0f, 
			}, Vector2.one, 0f - speed);
		}
	}
}
