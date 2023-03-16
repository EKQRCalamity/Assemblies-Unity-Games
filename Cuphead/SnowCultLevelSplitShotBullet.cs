using System;
using UnityEngine;

public class SnowCultLevelSplitShotBullet : AbstractProjectile
{
	[SerializeField]
	private SnowCultLevelSplitShotBulletShattered shatteredBullet;

	[SerializeField]
	private Effect shootFX;

	[SerializeField]
	private Animator spawnFX;

	private float middleAngle;

	private float spreadAngle;

	private float rotation;

	private float speed;

	private bool moving;

	private int numOfBullets;

	private bool bulletsSpawned;

	private Collider2D coll;

	private Vector3 basePos;

	private float wobbleTimer;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 1f;

	public SnowCultLevelJackFrost main;

	private new bool dead;

	private bool startedGrowing;

	public virtual SnowCultLevelSplitShotBullet Init(Vector3 pos, float rotation, float speed, int numOfBullets, float spreadAngle, LevelProperties.SnowCult.SplitShot properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		basePos = pos;
		this.rotation = rotation;
		this.speed = speed;
		moving = false;
		this.numOfBullets = numOfBullets;
		this.spreadAngle = spreadAngle;
		coll = GetComponent<Collider2D>();
		wobbleTimer = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		coll.enabled = false;
		spawnFX.Play("Spawn" + ((!base.CanParry) ? string.Empty : "Pink"));
		return this;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (moving)
		{
			base.transform.position += (Vector3)MathUtils.AngleToDirection(rotation) * speed * CupheadTime.FixedDelta;
			if (coll.enabled && Mathf.Abs(base.transform.position.x) > (float)Level.Current.Right)
			{
				middleAngle = ((!(base.transform.position.x < 0f)) ? 180f : 0f);
				base.transform.localScale = new Vector3(0f - Mathf.Sign(base.transform.position.x), 1f);
				base.transform.position = new Vector3((float)(Level.Current.Left - 65) * (0f - Mathf.Sign(base.transform.position.x)), base.transform.position.y);
				base.animator.Play("BucketExplode");
				SFX_SNOWCULT_JackFrostSplitshotBucketImpact();
				SFX_SNOWCULT_JackFrostSplitshotBucketTravelLoopStop();
				coll.enabled = false;
				SpawnProjectiles();
				speed = 0f;
			}
		}
		else
		{
			wobbleTimer += CupheadTime.FixedDelta * wobbleSpeed;
			base.transform.position = basePos + new Vector3(Mathf.Sin(wobbleTimer * 3f) * wobbleX, Mathf.Cos(wobbleTimer * 2f) * wobbleY);
		}
	}

	public void Grow()
	{
		coll.enabled = true;
		startedGrowing = true;
		base.animator.Play("BucketStart" + ((!base.CanParry) ? string.Empty : "Pink"));
	}

	public void Fire()
	{
		moving = true;
		base.animator.Play("BucketLoop" + ((!base.CanParry) ? string.Empty : "Pink"));
		shootFX.Create(base.transform.position, new Vector3(0f - Mathf.Sign(base.transform.position.x), 1f));
		spawnFX.Play("None");
		SFX_SNOWCULT_JackFrostSplitshotBucketTravelLoop();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		SFX_SNOWCULT_JackFrostSplitshotBucketImpact();
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void AniEvent_EndExplode()
	{
		this.Recycle();
	}

	private void SpawnProjectiles()
	{
		if (!bulletsSpawned)
		{
			bulletsSpawned = true;
			float num = spreadAngle / Mathf.Round(numOfBullets / 2);
			float num2 = middleAngle - spreadAngle;
			for (int i = 0; i < numOfBullets; i++)
			{
				shatteredBullet.Create(base.transform.position, num2 + num * (float)i, speed);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (main.dead != dead)
		{
			dead = true;
			SFX_SNOWCULT_JackFrostSplitshotBucketTravelLoopStop();
			if (!startedGrowing)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("BucketStart" + ((!base.CanParry) ? string.Empty : "Pink")))
			{
				spawnFX.Play("None");
				base.animator.Play("BucketStartReverse" + ((!base.CanParry) ? string.Empty : "Pink"), 0, 1f - base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			}
		}
	}

	private void SFX_SNOWCULT_JackFrostSplitshotBucketImpact()
	{
		AudioManager.Play("sfx_dlc_snowcult_p3_snowflake_splitshot_attack_bucket_impact");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_splitshot_attack_bucket_impact");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotBucketTravelLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_bucket_travel_loop");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_bucket_travel_loop");
	}

	private void SFX_SNOWCULT_JackFrostSplitshotBucketTravelLoopStop()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p3_snowflake_splitshot_handwaving_attack_bucket_travel_loop");
	}
}
