using System.Collections;
using UnityEngine;

public class SnowCultLevelSnowball : AbstractProjectile
{
	public enum Size
	{
		Small,
		Medium,
		Large
	}

	[SerializeField]
	private SnowCultLevelSnowball smallSnowballPrefab;

	[SerializeField]
	private SnowCultLevelSnowball mediumSnowballPrefab;

	[SerializeField]
	private SnowCultLevelSnowballExplosion snowballExplosion;

	public Size size;

	private LevelProperties.SnowCult.Snowball properties;

	private Vector3 velocity;

	private float gravity;

	private float speed;

	private bool hitGround;

	private bool makeSound;

	private Vector3 angle;

	[SerializeField]
	private SpriteRenderer[] glares;

	private int glareCounter = 1;

	private SnowCultLevelYeti main;

	public virtual SnowCultLevelSnowball Init(Vector3 pos, float gravity, float verticalVelocity, float horizontalVelocity, LevelProperties.SnowCult.Snowball properties, SnowCultLevelYeti main, bool makeSound)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		this.properties = properties;
		this.gravity = gravity;
		velocity.x = 0f - horizontalVelocity;
		velocity.y = verticalVelocity;
		base.transform.localScale = new Vector3(Mathf.Sign(horizontalVelocity), 1f);
		hitGround = false;
		this.main = main;
		this.makeSound = makeSound;
		base.animator.Play("Spin", 0, main.GetIceCubeStartFrame() * 0.0625f);
		StartCoroutine(move_cr());
		return this;
	}

	public virtual SnowCultLevelSnowball InitOriginal(Vector3 pos, float gravity, float speed, float angle, LevelProperties.SnowCult.Snowball properties, SnowCultLevelYeti main)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		this.speed = speed;
		base.transform.localScale = new Vector3(Mathf.Sign(angle - 90f), 1f);
		this.gravity = gravity;
		this.angle = MathUtils.AngleToDirection(angle);
		this.properties = properties;
		hitGround = false;
		this.main = main;
		makeSound = true;
		base.animator.Play("Spin", 0, main.GetIceCubeStartFrame() * 0.0625f);
		StartCoroutine(move_from_yeti_cr());
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void OnCollisionGround(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionGround(hit, phase);
		SFX_SNOWCULT_IceCubeImpact();
		hitGround = true;
	}

	private void TriggerGlare()
	{
		if (glareCounter == 0)
		{
			glares[0].enabled = false;
			glares[1].enabled = false;
		}
		glareCounter++;
		if (glareCounter == 3)
		{
			glares[0].enabled = true;
			glares[1].enabled = true;
			glareCounter = 0;
		}
	}

	private IEnumerator move_from_yeti_cr()
	{
		float accumulativeGravity = 0f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (!hitGround)
		{
			base.transform.position += angle * speed * CupheadTime.FixedDelta - new Vector3(0f, accumulativeGravity * CupheadTime.FixedDelta, 0f);
			accumulativeGravity += gravity * CupheadTime.FixedDelta;
			yield return wait;
		}
		newProjectiles();
		this.Recycle();
		yield return null;
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (!hitGround)
		{
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			velocity.y -= gravity * CupheadTime.FixedDelta;
			yield return wait;
		}
		newProjectiles();
		this.Recycle();
		yield return null;
	}

	private void newProjectiles()
	{
		SnowCultLevelSnowballExplosion snowCultLevelSnowballExplosion = snowballExplosion.Spawn();
		snowCultLevelSnowballExplosion.Init(base.transform.position, size, main);
		float num = Time.realtimeSinceStartup % 0.0001f;
		if (size == Size.Large)
		{
			SnowCultLevelSnowball snowCultLevelSnowball = mediumSnowballPrefab.Spawn();
			snowCultLevelSnowball.Init(base.transform.position + Vector3.back * num, properties.mediumGravity, properties.mediumVelocityY, properties.mediumVelocityX, properties, main, makeSound: false);
			SnowCultLevelSnowball snowCultLevelSnowball2 = mediumSnowballPrefab.Spawn();
			snowCultLevelSnowball2.Init(base.transform.position + Vector3.forward * num, properties.mediumGravity, properties.mediumVelocityY, 0f - properties.mediumVelocityX, properties, main, makeSound: true);
		}
		else if (size == Size.Medium)
		{
			SnowCultLevelSnowball snowCultLevelSnowball3 = smallSnowballPrefab.Spawn();
			snowCultLevelSnowball3.Init(base.transform.position + Vector3.back * num, properties.smallGravity, properties.smallVelocityY, properties.smallVelocityX, properties, main, makeSound: true);
			SnowCultLevelSnowball snowCultLevelSnowball4 = smallSnowballPrefab.Spawn();
			snowCultLevelSnowball4.Init(base.transform.position + Vector3.forward * num, properties.smallGravity, properties.smallVelocityY, 0f - properties.smallVelocityX, properties, main, makeSound: false);
		}
	}

	private void SFX_SNOWCULT_IceCubeImpact()
	{
		if (makeSound)
		{
			string text = "_large";
			if (size == Size.Medium)
			{
				text = "_medium";
			}
			if (size == Size.Small)
			{
				text = "_small";
			}
			AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_fridge_icecube_impact" + text);
			emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_fridge_icecube_impact" + text);
		}
	}
}
