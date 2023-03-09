using System;
using System.Collections;
using UnityEngine;

public class RumRunnersLevelGrub : AbstractProjectile
{
	private static readonly float[] enterEndClipLength = new float[3] { 10f, 11f, 16f };

	private const float flipClipLength = 15f;

	private static readonly float[] flipEndClipLength = new float[4] { 17f, 12f, 9f, 19f };

	private const float START_SIZE = 0.3f;

	private const float WAIT_SIZE = 0.8f;

	private const float END_FLY_UP_TRIGGER = 0.9f;

	private const float OVERSHOOT = 10f;

	private const float FLIP_HEIGHT = 30f;

	private const float TIME_TO_FULL_X_SPEED = 0.5f;

	private static readonly Rangef BlinkLoopsRange = new Rangef(2f, 3f);

	private const float EnterYOffset = 4f;

	[SerializeField]
	private float yOffset;

	[SerializeField]
	private SpriteRenderer mainRenderer;

	[SerializeField]
	private SpriteRenderer blinkRenderer;

	[SerializeField]
	private Transform shadowTransform;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 1f;

	[SerializeField]
	private Effect deathEffect;

	private float time;

	private float hp;

	private DamageReceiver damageReceiver;

	private RumRunnersLevelSpider parent;

	private Collider2D collider;

	private bool finishedEntering;

	private RumRunnersLevelGrubPath path;

	private int enterVariant;

	private int variant;

	private int spawnOrder;

	private float wobbleTimer;

	private Vector3 basePos;

	private float shadowDist;

	private float horizontalSpeedEasingTime;

	public int x { get; private set; }

	public int y { get; private set; }

	public float speed { get; private set; }

	public bool moving { get; private set; }

	public bool startedEntering { get; private set; }

	public RumRunnersLevelGrub Create(RumRunnersLevelGrubPath path, float rotation, float speed, float time, float hp, RumRunnersLevelSpider parent, int enterVariant, int variant, int spawnOrder, int x, int y)
	{
		RumRunnersLevelGrub rumRunnersLevelGrub = base.Create(path.start, rotation) as RumRunnersLevelGrub;
		rumRunnersLevelGrub.transform.localScale = new Vector3(0.3f * Mathf.Sign(path.transform.position.x - path.start.x), 0.3f);
		rumRunnersLevelGrub.path = path;
		rumRunnersLevelGrub.speed = speed;
		rumRunnersLevelGrub.time = time;
		rumRunnersLevelGrub.hp = hp;
		rumRunnersLevelGrub.parent = parent;
		rumRunnersLevelGrub.GetComponent<Collider2D>().enabled = true;
		rumRunnersLevelGrub.enterVariant = enterVariant;
		rumRunnersLevelGrub.variant = variant;
		rumRunnersLevelGrub.animator.SetInteger("Variant", enterVariant);
		Animator obj = rumRunnersLevelGrub.animator;
		Rangef blinkLoopsRange = BlinkLoopsRange;
		int min = (int)blinkLoopsRange.minimum;
		Rangef blinkLoopsRange2 = BlinkLoopsRange;
		obj.SetInteger("BlinkLoops", UnityEngine.Random.Range(min, (int)blinkLoopsRange2.maximum + 1));
		rumRunnersLevelGrub.animator.Play("Start", 0, 0f);
		rumRunnersLevelGrub.spawnOrder = spawnOrder;
		rumRunnersLevelGrub.shadowDist = shadowTransform.localPosition.y;
		rumRunnersLevelGrub.x = x;
		rumRunnersLevelGrub.y = y;
		return rumRunnersLevelGrub;
	}

	protected override void Start()
	{
		base.Start();
		collider = GetComponent<Collider2D>();
		if ((bool)GetComponent<DamageReceiver>())
		{
			damageReceiver = GetComponent<DamageReceiver>();
			damageReceiver.OnDamageTaken += onDamageTaken;
		}
		StartCoroutine(move_cr());
	}

	protected override void OnDieDistance()
	{
	}

	protected override void OnDieLifetime()
	{
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (moving)
		{
			horizontalSpeedEasingTime += CupheadTime.FixedDelta;
			basePos.x += Mathf.Lerp(0f, speed, horizontalSpeedEasingTime / 0.5f) * CupheadTime.FixedDelta;
			if (finishedEntering)
			{
				basePos.y = RumRunnersLevel.GroundWalkingPosY(basePos, null, yOffset);
			}
			if (basePos.x > 960f || basePos.x < -960f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			base.transform.position = basePos + wobblePos();
			if (finishedEntering)
			{
				shadowTransform.position = new Vector3(base.transform.position.x, basePos.y + shadowDist);
				wobbleTimer += wobbleSpeed * CupheadTime.FixedDelta;
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void onDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			die(playSound: true);
		}
	}

	private Vector3 wobblePos()
	{
		return new Vector3(Mathf.Sin(wobbleTimer * 3f) * wobbleX, Mathf.Sin(wobbleTimer * 2f) * wobbleY, 0f);
	}

	public float GetTimeToMove()
	{
		if (moving)
		{
			return 0f;
		}
		if (!startedEntering)
		{
			return (15f + flipEndClipLength[variant]) * (1f / 24f);
		}
		int num = Animator.StringToHash(base.animator.GetLayerName(0) + ".Flip");
		if (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == num)
		{
			return (15f * (1f - base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) + flipEndClipLength[variant]) * 1f / 24f;
		}
		return flipEndClipLength[variant] * (1f - base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) * 1f / 24f;
	}

	private IEnumerator move_cr()
	{
		collider.enabled = false;
		float t = 0f;
		Vector3 destinationPoint = new Vector3(path.GetPoint(1f).x, RumRunnersLevel.GroundWalkingPosY(path.GetPoint(1f), null, yOffset) + 4f);
		float pathOffset = destinationPoint.y - path.GetPoint(1f).y;
		float orientation = Mathf.Sign(base.transform.position.x - destinationPoint.x);
		while (t <= 1f)
		{
			t += 1f / 30f;
			setSortingOrder(75 + (int)(t * 10f));
			if (t > 0.9f)
			{
				base.animator.SetTrigger("EndFlyUp");
			}
			if (t + 1f / 30f >= path.forceFGSet && t < path.forceFGSet)
			{
				setSortingLayer("Default");
			}
			base.transform.position = path.GetPoint(EaseUtils.EaseOutSine(0f, 1f, t)) + Vector2.up * pathOffset;
			base.transform.localScale = new Vector3(EaseUtils.EaseInCubic(0.3f, 0.8f, t) * orientation, EaseUtils.EaseInCubic(0.3f, 0.8f, t));
			yield return CupheadTime.WaitForSeconds(this, 1f / 30f);
		}
		base.animator.SetTrigger("EndFlyUp");
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.localScale.x) * 0.8f, 0.8f);
		base.transform.position = destinationPoint;
		Vector3 vel = destinationPoint - (Vector3)(path.GetPoint(EaseUtils.EaseOutSine(0f, 1f, t - 1f / 30f)) + Vector2.up * pathOffset);
		for (t = 0f; t < time || ((bool)parent && !parent.GrubCanEnter(base.transform.position, GetTimeToMove())); t += 1f / 30f)
		{
			base.transform.position = destinationPoint + Mathf.Sin(t * 10f) * vel * (Mathf.InverseLerp(1f, 0f, t) * 10f);
			vel = vel.magnitude * MathUtils.AngleToDirection(MathUtils.DirectionToAngle(vel) + 3f);
			yield return CupheadTime.WaitForSeconds(this, 1f / 30f);
		}
		base.transform.position = destinationPoint;
		Vector3 onGroundPoint = new Vector3(path.GetPoint(1f).x, RumRunnersLevel.GroundWalkingPosY(path.GetPoint(1f), null, yOffset));
		float timeToMove = GetTimeToMove();
		float flipLength = 0.625f;
		t = 0f;
		base.animator.SetTrigger("Enter");
		startedEntering = true;
		SFX_RUMRUN_Grub_VocalIntro();
		SFX_RUMRUN_Grub_FlyingLoop();
		for (; t < timeToMove; t += 1f / 30f)
		{
			float moveTime = Mathf.Clamp(t / flipLength, 0f, 1f);
			float flipTime = Mathf.Clamp(t / flipLength, 0f, 1f);
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Lerp(destinationPoint.y, onGroundPoint.y, moveTime) + Mathf.Sin(flipTime * (float)Math.PI) * 30f);
			base.transform.localScale = new Vector3(Mathf.Lerp(0.8f, 1f, moveTime) * Mathf.Sign(base.transform.localScale.x), Mathf.Lerp(0.8f, 1f, moveTime));
			basePos = base.transform.position;
			shadowTransform.position = new Vector3(base.transform.position.x, onGroundPoint.y + shadowDist);
			yield return CupheadTime.WaitForSeconds(this, 1f / 30f);
		}
		base.transform.position = new Vector3(base.transform.position.x, onGroundPoint.y);
		finishedEntering = true;
	}

	private void die(bool playSound)
	{
		UnityEngine.Object.Destroy(base.gameObject);
		deathEffect.Create(base.transform.position);
		SFX_RUMRUN_Grub_FlyingLoopStop();
		if (playSound)
		{
			SFX_RUMRUN_Grub_Lackey_DiePoof();
		}
	}

	private void AniEvent_EnableCollision()
	{
		collider.enabled = true;
	}

	private void AniEvent_StartMoving()
	{
		moving = true;
	}

	private void AniEvent_OnFlip()
	{
		setSortingLayer("Enemies");
		setSortingOrder(spawnOrder + 1);
		base.transform.localScale = new Vector3(Mathf.Abs(base.transform.localScale.x) * Mathf.Sign(base.transform.position.x - PlayerManager.GetNext().transform.position.x), base.transform.localScale.y);
		speed *= 0f - base.transform.localScale.x;
		base.animator.SetInteger("Variant", variant);
	}

	private void animationEvent_BlinkCompleted()
	{
		Animator obj = base.animator;
		Rangef blinkLoopsRange = BlinkLoopsRange;
		int min = (int)blinkLoopsRange.minimum;
		Rangef blinkLoopsRange2 = BlinkLoopsRange;
		obj.SetInteger("BlinkLoops", UnityEngine.Random.Range(min, (int)blinkLoopsRange2.maximum + 1));
	}

	private void setSortingLayer(string layerName)
	{
		mainRenderer.sortingLayerName = layerName;
		blinkRenderer.sortingLayerName = layerName;
	}

	private void setSortingOrder(int order)
	{
		mainRenderer.sortingOrder = order;
		blinkRenderer.sortingOrder = order + 1;
	}

	private void SFX_RUMRUN_Grub_Lackey_DiePoof()
	{
		AudioManager.Play("sfx_dlc_rumrun_lackey_poof");
		emitAudioFromObject.Add("sfx_dlc_rumrun_lackey_poof");
	}

	private void SFX_RUMRUN_Grub_VocalIntro()
	{
	}

	private void SFX_RUMRUN_Grub_FlyingLoop()
	{
		AudioManager.Play("sfx_dlc_rumrun_grub_flying_loop");
		emitAudioFromObject.Add("sfx_dlc_rumrun_grub_flying_loop");
	}

	private void SFX_RUMRUN_Grub_FlyingLoopStop()
	{
		AudioManager.Stop("sfx_dlc_rumrun_grub_flying_loop");
	}
}
