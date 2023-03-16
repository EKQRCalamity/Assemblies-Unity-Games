using System.Collections;
using UnityEngine;

public class RumRunnersLevelCopBall : AbstractProjectile
{
	private const float DIE_OFFSET_X = 500f;

	private static readonly int AudioLoopCount = 6;

	private static int CurrentAudioLoopIndex;

	private static int LastSortingIndex;

	[SerializeField]
	private BasicProjectile copBullet;

	[SerializeField]
	private BasicProjectile copBulletPink;

	[SerializeField]
	private Effect dustEffect;

	[SerializeField]
	private Effect deathEffect;

	private LevelProperties.RumRunners.CopBall properties;

	private Vector3 velocity;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private CircleCollider2D circleCollider;

	private float health;

	private float speed;

	private float offset;

	private int audioLoopNumber;

	private bool launched;

	private Transform snoutPos;

	public bool leaveScreen { get; set; }

	protected override float DestroyLifetime => 0f;

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	public void Init(Vector3 position, Vector3 velocity, float speed, float health, LevelProperties.RumRunners.CopBall properties, Transform snoutPos)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = position;
		this.properties = properties;
		this.health = health;
		this.velocity = velocity;
		offset = GetComponent<Collider2D>().bounds.size.x / 2f;
		leaveScreen = false;
		circleCollider.enabled = false;
		launched = false;
		this.snoutPos = snoutPos;
		if (properties.constSpeed)
		{
			this.speed = speed;
		}
		else
		{
			StartCoroutine(gradualSpeed_cr());
		}
		LastSortingIndex--;
		if (LastSortingIndex < 10)
		{
			LastSortingIndex = 15;
		}
		GetComponent<SpriteRenderer>().sortingOrder = LastSortingIndex;
		audioLoopNumber = CurrentAudioLoopIndex + 1;
		CurrentAudioLoopIndex = MathUtilities.NextIndex(CurrentAudioLoopIndex, AudioLoopCount);
		SFX_RUMRUN_P3_BallCop_VocalShouts_Loop(audioLoopNumber);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Death(playAudio: true);
		}
	}

	public void Launch()
	{
		StartCoroutine(move_cr());
		StartCoroutine(shoot_cr());
		StartCoroutine(checkToDie_cr());
		circleCollider.enabled = true;
		GetComponent<SpriteRenderer>().sortingLayerName = "Projectiles";
		launched = true;
	}

	private void LateUpdate()
	{
		if (!launched)
		{
			base.transform.position = snoutPos.position;
		}
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			base.transform.position += velocity * speed * CupheadTime.FixedDelta;
			CheckBounds();
			yield return wait;
		}
	}

	private void CheckBounds()
	{
		bool flag = properties.sideWallBounce && !leaveScreen;
		Quaternion? quaternion = null;
		Vector3 vector = Vector3.zero;
		if (base.transform.position.y > CupheadLevelCamera.Current.Bounds.yMax - offset && velocity.y > 0f)
		{
			velocity.y = 0f - Mathf.Abs(velocity.y);
			quaternion = Quaternion.identity;
			vector = new Vector3(0f, offset);
			SFX_RUMRUN_P3_BallCop_Bounce();
		}
		if (base.transform.position.y < (float)Level.Current.Ground + offset && velocity.y < 0f)
		{
			velocity.y = Mathf.Abs(velocity.y);
			quaternion = Quaternion.Euler(0f, 0f, 180f);
			vector = new Vector3(0f, 0f - offset);
			SFX_RUMRUN_P3_BallCop_Bounce();
		}
		if (flag && base.transform.position.x > CupheadLevelCamera.Current.Bounds.xMax - offset && velocity.x > 0f)
		{
			velocity.x = 0f - Mathf.Abs(velocity.x);
			quaternion = Quaternion.Euler(0f, 0f, 270f);
			vector = new Vector3(offset, 0f);
			SFX_RUMRUN_P3_BallCop_Bounce();
		}
		if (flag && base.transform.position.x < CupheadLevelCamera.Current.Bounds.xMin + offset && velocity.x < 0f)
		{
			velocity.x = Mathf.Abs(velocity.x);
			quaternion = Quaternion.Euler(0f, 0f, 90f);
			vector = new Vector3(0f - offset, 0f);
			SFX_RUMRUN_P3_BallCop_Bounce();
		}
		if (quaternion.HasValue)
		{
			Effect effect = dustEffect.Create(base.transform.position + vector);
			effect.transform.rotation = quaternion.Value;
			if (quaternion.Value == Quaternion.identity)
			{
				effect.transform.Find("Dirt").gameObject.SetActive(value: true);
				effect.animator.SetInteger("DirtEffect", Random.Range(0, 3));
			}
		}
	}

	private IEnumerator shoot_cr()
	{
		int copBallBulletAngleStringMainIndex = Random.Range(0, properties.copBallBulletAngleString.Length);
		string[] copBallBulletAngleString = properties.copBallBulletAngleString[copBallBulletAngleStringMainIndex].Split(',');
		int copBallBulletAngleStringIndex = Random.Range(0, copBallBulletAngleString.Length);
		int copBallBulletTypeStringMainIndex = Random.Range(0, properties.copBallBulletTypeString.Length);
		string[] copBallBulletTypeString = properties.copBallBulletTypeString[copBallBulletTypeStringMainIndex].Split(',');
		int copBallBulletTypeStringIndex = Random.Range(0, copBallBulletTypeString.Length);
		yield return CupheadTime.WaitForSeconds(this, properties.copBallShootHesitate);
		while (true)
		{
			BasicProjectile bullet = ((copBallBulletTypeString[copBallBulletTypeStringIndex][0] != 'P') ? copBullet : copBulletPink);
			float angle = 0f;
			Parser.FloatTryParse(copBallBulletAngleString[copBallBulletAngleStringIndex], out angle);
			bullet.Create(base.transform.position, angle, properties.copBallBulletSpeed);
			yield return CupheadTime.WaitForSeconds(this, properties.copBallShootDelay);
			if (copBallBulletAngleStringIndex < copBallBulletAngleString.Length - 1)
			{
				copBallBulletAngleStringIndex++;
			}
			else
			{
				copBallBulletAngleStringMainIndex = (copBallBulletAngleStringMainIndex + 1) % properties.copBallBulletAngleString.Length;
				copBallBulletAngleStringIndex = 0;
			}
			if (copBallBulletTypeStringIndex < copBallBulletTypeString.Length - 1)
			{
				copBallBulletTypeStringIndex++;
			}
			else
			{
				copBallBulletTypeStringMainIndex = (copBallBulletTypeStringMainIndex + 1) % properties.copBallBulletTypeString.Length;
				copBallBulletTypeStringIndex = 0;
			}
			yield return null;
		}
	}

	public void Death(bool playAudio)
	{
		SFX_RUMRUN_P3_BallCop_Die(audioLoopNumber, playAudio);
		deathEffect.Create(base.transform.position);
		StopAllCoroutines();
		this.Recycle();
	}

	private IEnumerator checkToDie_cr()
	{
		while (!(base.transform.position.x < -1140f) && !(base.transform.position.x > 1140f))
		{
			yield return null;
		}
		Death(playAudio: false);
	}

	private IEnumerator gradualSpeed_cr()
	{
		float t = 0f;
		float time = properties.gradualSpeedTime;
		float val = 1f;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			speed = properties.gradualSpeed.GetFloatAt(val - t / time);
			yield return null;
		}
	}

	private void SFX_RUMRUN_P3_BallCop_Bounce()
	{
		AudioManager.Play("sfx_dlc_rumrun_copball_bounce");
		emitAudioFromObject.Add("sfx_dlc_rumrun_copball_bounce");
	}

	private void SFX_RUMRUN_P3_BallCop_VocalShouts_Loop(int loopNumber)
	{
		string key = "sfx_dlc_rumrun_p3_ballcop_vocalshouts_loop_" + loopNumber;
		AudioManager.PlayLoop(key);
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_ballcop_vocalshouts_loop");
	}

	private void SFX_RUMRUN_P3_BallCop_Die(int loopNumber, bool playAudio)
	{
		string key = "sfx_dlc_rumrun_p3_ballcop_vocalshouts_loop_" + loopNumber;
		AudioManager.Stop(key);
		if (playAudio)
		{
			AudioManager.Play("sfx_dlc_rumrun_copball_bounce");
			emitAudioFromObject.Add("sfx_dlc_rumrun_copball_bounce");
		}
	}
}
