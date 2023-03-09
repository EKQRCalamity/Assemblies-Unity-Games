using System.Collections;
using UnityEngine;

public class SaltbakerLevelHeart : AbstractProjectile
{
	public enum LastHit
	{
		None,
		Left,
		Right,
		Up,
		Down
	}

	[SerializeField]
	private SpriteRenderer pinkSprite;

	[SerializeField]
	private SpriteRenderer regularSprite;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private Animator impactFX;

	[SerializeField]
	private Effect turnFX;

	private float ballSize;

	private float speed;

	private float angleOffset;

	private bool isMoving;

	private bool isDead;

	private LevelProperties.Saltbaker.DarkHeart properties;

	private SaltbakerLevelPillarHandler parent;

	private DamageReceiver damageReceiver;

	private Collider2D leftPillarColl;

	private Collider2D rightPillarColl;

	private Vector3 dir;

	private Vector3 lastDirNoOffset;

	private PatternString angleString;

	public LastHit lastHit;

	protected override float DestroyLifetime => 0f;

	protected override void Start()
	{
		base.Start();
		ballSize = GetComponent<Collider2D>().bounds.size.y / 2f;
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		impactFX.transform.parent = null;
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
		parent.TakeDamage(info);
	}

	public void Init(Vector3 pos, GameObject leftPillar, GameObject rightPillar, LevelProperties.Saltbaker.DarkHeart properties, SaltbakerLevelPillarHandler parent)
	{
		base.transform.position = pos;
		this.properties = properties;
		isMoving = false;
		speed = properties.heartSpeed;
		this.parent = parent;
		leftPillarColl = leftPillar.GetComponent<Collider2D>();
		rightPillarColl = rightPillar.GetComponent<Collider2D>();
		SetParryable(parryable: true);
		coll.enabled = false;
		angleString = new PatternString(properties.angleOffsetString);
		dir = MathUtils.AngleToDirection(properties.baseAngle);
		base.transform.localScale = new Vector3(0f - Mathf.Sign(dir.x), 1f);
		lastDirNoOffset = dir;
		StartCoroutine(warning_cr());
	}

	private IEnumerator warning_cr()
	{
		SFX_SALTB_HeartWarning();
		yield return base.animator.WaitForAnimationToEnd(this, "Warning");
		isMoving = true;
		coll.enabled = true;
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		if (isMoving)
		{
			base.transform.position += dir * speed * CupheadTime.Delta;
		}
		CheckBounds();
		if (!isDead)
		{
			pinkSprite.enabled = base.CanParry;
		}
		if (base.CanParry && !isDead)
		{
			pinkSprite.color = new Color(pinkSprite.color.r, pinkSprite.color.g, pinkSprite.color.b, pinkSprite.color.a + Time.deltaTime * 2f);
			regularSprite.color = new Color(regularSprite.color.r, regularSprite.color.g, regularSprite.color.b, regularSprite.color.a - Time.deltaTime * 0.5f);
		}
	}

	public override void OnParry(AbstractPlayerController player)
	{
		SetParryable(parryable: false);
		pinkSprite.color = new Color(0f, 0f, 0f, 0f);
		regularSprite.color = Color.black;
		StartCoroutine(coolDown_cr());
		StartCoroutine(colliderCoolDown_cr());
	}

	private IEnumerator coolDown_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, properties.parryTimeOut);
		SetParryable(parryable: true);
		yield return null;
	}

	private IEnumerator colliderCoolDown_cr()
	{
		damageDealer.SetDamageFlags(damagesPlayer: false, damagesEnemy: false, damagesOther: false);
		yield return CupheadTime.WaitForSeconds(this, properties.collisionTimeOut);
		damageDealer.SetDamageFlags(damagesPlayer: true, damagesEnemy: false, damagesOther: false);
		yield return null;
	}

	private void CheckBounds()
	{
		if (lastHit != LastHit.Up && base.transform.position.y > CupheadLevelCamera.Current.Bounds.yMax - ballSize)
		{
			SetNewDir(getMin: true, isX: false);
			lastHit = LastHit.Up;
		}
		else if (lastHit != LastHit.Down && base.transform.position.y < CupheadLevelCamera.Current.Bounds.yMin + ballSize)
		{
			SetNewDir(getMin: false, isX: false);
			lastHit = LastHit.Down;
		}
		else if (lastHit != LastHit.Left && base.transform.position.x < leftPillarColl.bounds.max.x + ballSize)
		{
			SetNewDir(getMin: false, isX: true);
			lastHit = LastHit.Left;
		}
		else if (lastHit != LastHit.Right && base.transform.position.x > rightPillarColl.bounds.min.x - ballSize)
		{
			SetNewDir(getMin: true, isX: true);
			lastHit = LastHit.Right;
		}
	}

	private void SetNewDir(bool getMin, bool isX)
	{
		angleOffset = angleString.PopFloat();
		Vector3 vector = lastDirNoOffset;
		if (getMin)
		{
			if (isX)
			{
				vector.x = Mathf.Min(vector.x, 0f - vector.x);
				StartCoroutine(turn_cr());
			}
			else
			{
				vector.y = Mathf.Min(vector.y, 0f - vector.y);
			}
		}
		else if (isX)
		{
			vector.x = Mathf.Max(vector.x, 0f - vector.x);
			StartCoroutine(turn_cr());
		}
		else
		{
			vector.y = Mathf.Max(vector.y, 0f - vector.y);
		}
		lastDirNoOffset = vector;
		float num = MathUtils.DirectionToAngle(vector);
		num += angleOffset;
		vector = MathUtils.AngleToDirection(num);
		dir = vector;
	}

	private IEnumerator turn_cr()
	{
		isMoving = false;
		base.animator.Play("Turn");
		impactFX.transform.position = base.transform.position;
		impactFX.transform.localScale = base.transform.localScale;
		impactFX.Play((!Rand.Bool()) ? "B" : "A", 0, 0f);
		SFX_SALTB_HeartBounce();
		yield return null;
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.18181819f)
		{
			yield return null;
		}
		isMoving = true;
		StartCoroutine(turn_fx_cr());
	}

	private IEnumerator turn_fx_cr()
	{
		Vector3 pos = base.transform.position;
		int fxCount = Random.Range(2, 4);
		for (int i = 0; i < fxCount; i++)
		{
			turnFX.Create(pos);
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0f, 0.1f));
		}
	}

	private void AniEvent_Turn()
	{
		base.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
	}

	public new void Die()
	{
		StopAllCoroutines();
		coll.enabled = false;
		isMoving = false;
		regularSprite.enabled = false;
		pinkSprite.enabled = true;
		pinkSprite.color = new Color(0f, 0f, 0f, 1f);
		isDead = true;
		base.animator.Play("Death");
		AudioManager.Play("level_explosion_boss_death");
	}

	private void SFX_SALTB_HeartBounce()
	{
		AudioManager.Play("sfx_DLC_Saltbaker_P4_Heart_Bounce");
		emitAudioFromObject.Add("sfx_DLC_Saltbaker_P4_Heart_Bounce");
	}

	private void SFX_SALTB_HeartWarning()
	{
		AudioManager.Play("sfx_DLC_Saltbaker_P4_Heart_Warning");
		emitAudioFromObject.Add("sfx_DLC_Saltbaker_P4_Heart_Warning");
	}
}
