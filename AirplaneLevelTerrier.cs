using System;
using System.Collections;
using UnityEngine;

public class AirplaneLevelTerrier : AbstractCollidableObject
{
	private static readonly int OnShockParameterID = Animator.StringToHash("OnShock");

	private const float NINETY_DEGREES = 90f;

	private const float THREE_SIXTY = 360f;

	private const float LOOP_SIZE_Y = 365f;

	private const float LOOP_SIZE_X = 750f;

	private const float LOOP_SIZE_X_SECRET_INTRO = 1000f;

	private const float LOOP_SIZE_INTRO_MOD = 0.9f;

	private const float TIME_TO_FULL_LOOP_SIZE = 1f;

	private const float UP = 344f;

	private const float UP_RIGHT_1 = 8.2f;

	private const float UP_RIGHT_2 = 31.8f;

	private const float UP_RIGHT_3 = 55.4f;

	private const float RIGHT = 79f;

	private const float DOWN_RIGHT_1 = 102.6f;

	private const float DOWN_RIGHT_2 = 126.2f;

	private const float DOWN_RIGHT_3 = 149.8f;

	private const float DOWN = 164.75f;

	private const float DOWN_LEFT_1 = 242.45f;

	private const float DOWN_LEFT_2 = 218.85f;

	private const float DOWN_LEFT_3 = 195.25f;

	private const float LEFT = 259f;

	private const float UP_LEFT_1 = 329.8f;

	private const float UP_LEFT_2 = 306.2f;

	private const float UP_LEFT_3 = 282.6f;

	[SerializeField]
	private BoxCollider2D coll;

	[SerializeField]
	private AirplaneLevelTerrierBullet regularProjectile;

	[SerializeField]
	private AirplaneLevelTerrierBullet pinkProjectile;

	[SerializeField]
	private GameObject[] terrierLayers;

	[SerializeField]
	private SpriteRenderer deathRenderer;

	[SerializeField]
	private SpriteRenderer[] rends;

	[SerializeField]
	private Vector3[] flameOffset;

	[SerializeField]
	private SpriteRenderer flame;

	[SerializeField]
	private Animator flameAnimator;

	[SerializeField]
	private SpriteRenderer barkFXRenderer;

	[SerializeField]
	private Animator barkFXAnimator;

	private LevelProperties.Airplane.Terriers properties;

	public float angle;

	private float hp;

	private float smokingThreshold;

	private int index;

	private bool isClockwise;

	private bool isPink;

	private bool isWow;

	private bool isSmoking;

	private bool gettingEaten;

	private Transform pivotPoint;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private Vector2 pivotOffset;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 1f;

	private float wobbleTimer;

	private float wobbleModifier = 1f;

	private float rotationSpeed;

	private float loopSizeX;

	private float loopSizeY;

	private bool isCurved;

	private int currentAngle;

	private float smokeDelay = 0.02f;

	private float smokeTimer;

	private bool introFinished;

	public bool lastOne;

	private Vector3 rotationOffset;

	public bool IsDead { get; private set; }

	public bool ReadyToMove { get; private set; }

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public void Init(Transform pivotPoint, float angle, LevelProperties.Airplane.Terriers properties, float hp, float pivotOffsetX, float pivotOffsetY, bool isClockwise, int index)
	{
		this.angle = angle;
		this.pivotPoint = pivotPoint;
		pivotOffset = new Vector2(pivotOffsetX, pivotOffsetY);
		this.properties = properties;
		this.hp = hp;
		smokingThreshold = hp * properties.secretHPPercentage;
		this.isClockwise = isClockwise;
		this.index = index;
		wobbleTimer = index;
		StartCoroutine(setup_dogs_cr());
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Level.Current.RegisterMinionKilled();
			Die();
		}
	}

	public bool IsSmoking()
	{
		if (!isSmoking && hp < smokingThreshold)
		{
			isSmoking = true;
			base.animator.SetTrigger(OnShockParameterID);
			AudioManager.Play("sfx_dlc_dogfight_p2_terrierjetpack_dmgsmoke");
			emitAudioFromObject.Add("sfx_dlc_dogfight_p2_terrierjetpack_dmgsmoke");
		}
		return isSmoking;
	}

	public float Health()
	{
		return hp;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		wobbleTimer += wobbleSpeed * (float)CupheadTime.Delta;
	}

	private Vector3 WobblePos()
	{
		return new Vector3(Mathf.Sin(wobbleTimer * 3f) * wobbleX, Mathf.Sin(wobbleTimer * 2f) * wobbleY, 0f) * wobbleModifier;
	}

	private IEnumerator setup_dogs_cr()
	{
		rotationOffset = Vector3.zero;
		YieldInstruction wait = new WaitForFixedUpdate();
		int indexToPlay = index;
		base.transform.SetScale((!isClockwise) ? Mathf.Abs(base.transform.localScale.x) : (0f - Mathf.Abs(base.transform.localScale.x)));
		switch (index)
		{
		case 1:
		{
			int num3;
			if (isClockwise)
			{
				int num = 3;
				num3 = num;
			}
			else
			{
				num3 = index;
			}
			indexToPlay = num3;
			break;
		}
		case 3:
		{
			int num2;
			if (isClockwise)
			{
				int num = 1;
				num2 = num;
			}
			else
			{
				num2 = index;
			}
			indexToPlay = num2;
			break;
		}
		}
		base.animator.Play("Intro_" + indexToPlay);
		int flamePos = indexToPlay * 4;
		if (indexToPlay == 3)
		{
			flamePos = 4;
		}
		flame.transform.localPosition = flameOffset[flamePos];
		if (indexToPlay == 1)
		{
			flame.transform.localPosition = new Vector3(0f - flame.transform.localPosition.x, flame.transform.localPosition.y);
		}
		angle *= (float)Math.PI / 180f;
		loopSizeX = 675f;
		loopSizeY = 328.5f;
		rotationOffset.x = Mathf.Sin(angle) * loopSizeX;
		rotationOffset.y = Mathf.Cos(angle) * loopSizeY;
		Vector3 startPos = pivotPoint.position + (Vector3)pivotOffset + rotationOffset * 2f;
		Vector3 endPos = pivotPoint.position + (Vector3)pivotOffset + rotationOffset;
		float t2 = 0f;
		float time = 0.5f;
		base.transform.position = startPos;
		while (t2 < time)
		{
			t2 += CupheadTime.FixedDelta;
			base.transform.position = Vector3.Lerp(startPos, endPos, t2 / time) + WobblePos();
			yield return wait;
		}
		base.animator.SetTrigger("ContinueIntro");
		t2 = 0f;
		while (t2 < 0.9f)
		{
			t2 += CupheadTime.FixedDelta;
			base.transform.position = endPos + WobblePos();
			wobbleModifier = Mathf.Lerp(1f, 0f, t2 / 0.9f);
			yield return wait;
		}
		wobbleModifier = 0f;
		base.animator.SetTrigger("EndIntro");
		rotationSpeed = 0f;
		ReadyToMove = true;
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		introFinished = true;
		((AirplaneLevel)Level.Current).terriersIntroFinished = true;
	}

	private IEnumerator ease_to_full_speed_and_radius_cr()
	{
		float t = 0f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < 1f)
		{
			loopSizeX = Mathf.Lerp(675f, 750f, EaseUtils.EaseOutSine(0f, 1f, t / 1f));
			loopSizeY = Mathf.Lerp(328.5f, 365f, EaseUtils.EaseOutSine(0f, 1f, t / 1f));
			rotationSpeed = Mathf.Lerp(0f, properties.rotationTime, EaseUtils.EaseInSine(0f, 1f, t / 1f));
			t += CupheadTime.FixedDelta;
			yield return wait;
		}
		loopSizeX = 750f;
		loopSizeY = 365f;
		rotationSpeed = properties.rotationTime;
	}

	public void StartMoving()
	{
		StartCoroutine(move_in_circle_cr());
		StartCoroutine(ease_to_full_speed_and_radius_cr());
	}

	private IEnumerator move_in_circle_cr()
	{
		rotationOffset = Vector3.zero;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			angle += rotationSpeed * CupheadTime.FixedDelta * (float)(isClockwise ? 1 : (-1));
			if (!gettingEaten)
			{
				rotationOffset.x = Mathf.Sin(angle) * loopSizeX;
			}
			else
			{
				bool flag = ((!isClockwise) ? (angle < (float)Math.PI) : (angle > (float)Math.PI));
				rotationOffset.x = Mathf.Sin(angle) * ((!flag) ? loopSizeX : 1000f);
				if (flag)
				{
					loopSizeY -= CupheadTime.FixedDelta * 50f;
				}
			}
			rotationOffset.y = Mathf.Cos(angle) * loopSizeY;
			Vector3 lastPos = base.transform.position;
			base.transform.position = (Vector2)pivotPoint.position + pivotOffset;
			base.transform.position += rotationOffset;
			flame.flipX = Mathf.Sign(lastPos.x - base.transform.position.x) == -1f;
			if (angle > (float)Math.PI * 2f)
			{
				angle -= (float)Math.PI * 2f;
			}
			if (angle < 0f)
			{
				angle += (float)Math.PI * 2f;
			}
			yield return wait;
		}
	}

	public Vector3 GetPredictedAttackPos()
	{
		float num = 0.125f;
		float f = angle + properties.rotationTime * num * (float)(isClockwise ? 1 : (-1));
		return (Vector2)pivotPoint.position + pivotOffset + new Vector2(Mathf.Sin(f) * 750f, Mathf.Cos(f) * 365f);
	}

	public void StartAttack(bool isPink, bool isWow)
	{
		this.isPink = isPink;
		this.isWow = isWow;
		if (isClockwise)
		{
			base.transform.SetScale(Mathf.Abs(base.transform.localScale.x));
		}
		base.animator.Play("Attack");
		SFX_DOGFIGHT_P2_TerrierJetpack_BarkShoot();
	}

	private void AniEvent_BarkFX()
	{
		barkFXRenderer.sortingLayerID = rends[currentAngle].sortingLayerID;
		barkFXRenderer.sortingOrder = rends[currentAngle].sortingOrder + ((currentAngle <= 4) ? 1 : (-1));
		barkFXRenderer.flipX = rends[currentAngle].flipX;
		barkFXRenderer.transform.localPosition = -flame.transform.localPosition * 0.5f;
		if (currentAngle == 1)
		{
			barkFXRenderer.transform.localPosition += new Vector3((!barkFXRenderer.flipX) ? 10 : (-10), 12f);
		}
		if (currentAngle == 2)
		{
			barkFXRenderer.transform.localPosition += new Vector3((!barkFXRenderer.flipX) ? (-10) : 10, 5f);
		}
		barkFXRenderer.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		barkFXAnimator.Play((!Rand.Bool()) ? "B" : "A");
	}

	private void AniEvent_ShootProjectile()
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		Vector3 vector = next.center - barkFXRenderer.transform.position;
		float acceleration = Vector3.Magnitude(rotationOffset) / 750f;
		AirplaneLevelTerrierBullet airplaneLevelTerrierBullet = ((!isPink) ? regularProjectile.Create(barkFXRenderer.transform.position, MathUtils.DirectionToAngle(vector), properties.shotSpeed, acceleration) : pinkProjectile.Create(barkFXRenderer.transform.position, MathUtils.DirectionToAngle(vector), properties.shotSpeed, acceleration));
		if (isWow)
		{
			airplaneLevelTerrierBullet.PlayWow();
		}
	}

	private void AniEvent_SetScale()
	{
		if (isClockwise)
		{
			base.transform.SetScale(Mathf.Abs(base.transform.localScale.x));
		}
	}

	public void StartSecret()
	{
		GetComponent<Collider2D>().enabled = false;
		StartCoroutine(move_into_mouth_cr());
	}

	public void PrepareForChomp()
	{
		gettingEaten = true;
		SpriteRenderer[] array = rends;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.sortingOrder = 2;
			spriteRenderer.sortingLayerName = "Foreground";
		}
	}

	private IEnumerator move_into_mouth_cr()
	{
		float t = 0f;
		float startSpeed = rotationSpeed;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < 1f)
		{
			t += CupheadTime.FixedDelta;
			rotationSpeed = Mathf.Lerp(startSpeed, 2.5f, EaseUtils.EaseInSine(0f, 1f, t));
			loopSizeX = Mathf.Lerp(750f, 600f, EaseUtils.EaseInSine(0f, 1f, t));
			loopSizeY = Mathf.Lerp(365f, 292f, EaseUtils.EaseInSine(0f, 1f, t));
			yield return wait;
		}
	}

	private void Die()
	{
		IsDead = true;
		flame.enabled = false;
		coll.enabled = false;
		base.animator.Play((!lastOne) ? "Death" : "DeathShort");
		SFX_DOGFIGHT_P2_TerrierJetpack_Explosion();
		StartCoroutine(SFX_DOGFIGHT_P2_TerrierJetpack_DeathBark_cr(index));
	}

	private void AniEvent_DeathLayering()
	{
		deathRenderer.sortingLayerName = "Background";
		deathRenderer.sortingOrder = 0;
	}

	private void AniEvent_OnDeath()
	{
		StopAllCoroutines();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CheckAllAnimations()
	{
		if (!gettingEaten)
		{
			float num = angle * 57.29578f;
			if ((num > 344f && num < 360f) || (num < 8.2f && num > 0f))
			{
				ChangeAngle(flipSprite: false, 0);
			}
			else if (num > 8.2f && num < 31.8f)
			{
				ChangeAngle(flipSprite: true, 1);
			}
			else if (num > 31.8f && num < 55.4f)
			{
				ChangeAngle(flipSprite: true, 2);
			}
			else if (num > 55.4f && num < 79f)
			{
				ChangeAngle(flipSprite: true, 3);
			}
			else if (num > 79f && num < 102.6f)
			{
				ChangeAngle(flipSprite: true, 4);
			}
			else if (num > 102.6f && num < 126.2f)
			{
				ChangeAngle(flipSprite: true, 5);
			}
			else if (num > 126.2f && num < 149.8f)
			{
				ChangeAngle(flipSprite: true, 6);
			}
			else if (num > 149.8f && num < 164.75f)
			{
				ChangeAngle(flipSprite: true, 7);
			}
			else if (num > 164.75f && num < 195.25f)
			{
				ChangeAngle(flipSprite: false, 8);
			}
			else if (num > 195.25f && num < 218.85f)
			{
				ChangeAngle(flipSprite: false, 7);
			}
			else if (num > 218.85f && num < 242.45f)
			{
				ChangeAngle(flipSprite: false, 6);
			}
			else if (num > 242.45f && num < 259f)
			{
				ChangeAngle(flipSprite: false, 5);
			}
			else if (num > 259f && num < 282.6f)
			{
				ChangeAngle(flipSprite: false, 4);
			}
			else if (num > 282.6f && num < 306.2f)
			{
				ChangeAngle(flipSprite: false, 3);
			}
			else if (num > 306.2f && num < 329.8f)
			{
				ChangeAngle(flipSprite: false, 2);
			}
			else if (num > 329.8f && num < 344f)
			{
				ChangeAngle(flipSprite: false, 1);
			}
		}
	}

	private void ChangeAngle(bool flipSprite, int layerIndex)
	{
		currentAngle = layerIndex;
		if (!isCurved != (layerIndex == 3 || layerIndex == 4 || layerIndex == 5))
		{
			flameAnimator.Play((layerIndex != 3 && layerIndex != 4 && layerIndex != 5) ? "Curve" : "Straight", 0, flameAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			isCurved = !isCurved;
		}
		GameObject[] array = terrierLayers;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(gameObject == terrierLayers[layerIndex]);
		}
		rends[layerIndex].flipX = flipSprite;
	}

	private void FixedUpdate()
	{
		if (!IsDead)
		{
			smokeTimer += CupheadTime.FixedDelta * ((!introFinished) ? 0.2f : ((!gettingEaten) ? 1f : 0.1f));
			if (smokeTimer > smokeDelay)
			{
				smokeTimer -= smokeDelay;
				((AirplaneLevel)Level.Current).CreateSmokeFX(flame.transform.position, (!introFinished) ? (MathUtils.AngleToDirection(flame.transform.eulerAngles.z - 90f) * 300f) : Vector2.zero, hp < smokingThreshold, rends[currentAngle].sortingLayerID, (currentAngle > 4) ? 30 : (-1));
			}
		}
	}

	private void LateUpdate()
	{
		if (rends[9].sprite == null)
		{
			CheckAllAnimations();
		}
		if (introFinished)
		{
			flame.sortingLayerID = rends[currentAngle].sortingLayerID;
			flame.sortingOrder = rends[currentAngle].sortingOrder - 1;
			flame.transform.localPosition = new Vector3(flameOffset[currentAngle].x * (float)((!rends[currentAngle].flipX) ? 1 : (-1)), flameOffset[currentAngle].y);
		}
	}

	public float RelativeAngle()
	{
		if (!isClockwise)
		{
			return (float)Math.PI * 2f - angle;
		}
		return angle;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		float[] array = new float[16]
		{
			344f, 8.2f, 31.8f, 55.4f, 329.8f, 306.2f, 282.6f, 164.75f, 242.45f, 218.85f,
			195.25f, 102.6f, 126.2f, 149.8f, 79f, 259f
		};
		Vector3 zero = Vector3.zero;
		float num = 400f;
		for (int i = 0; i < array.Length; i++)
		{
			zero = MathUtils.AngleToDirection(array[i] + 90f);
			if (array[i] == 344f || array[i] == 164.75f || array[i] == 259f || array[i] == 79f)
			{
				Gizmos.color = Color.blue;
			}
			else if (array[i] == 195.25f || array[i] == 282.6f || array[i] == 8.2f || array[i] == 102.6f)
			{
				Gizmos.color = Color.green;
			}
			else
			{
				Gizmos.color = Color.red;
			}
			Gizmos.DrawLine(Vector3.zero, zero * num);
		}
	}

	private void SFX_DOGFIGHT_P2_TerrierJetpack_BarkShoot()
	{
		AudioManager.Play("sfx_dlc_dogfight_p2_terrierjetpack_barkshoot");
	}

	private void SFX_DOGFIGHT_P2_TerrierJetpack_Explosion()
	{
		AudioManager.Play("sfx_dlc_dogfight_p2_terrierjetpack_explosion");
	}

	private IEnumerator SFX_DOGFIGHT_P2_TerrierJetpack_DeathBark_cr(int id)
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.Play("sfx_dlc_dogfight_p2_terrierjetpack_dmgdeath_0" + id);
	}

	private void WORKAROUND_NullifyFields()
	{
		coll = null;
		regularProjectile = null;
		pinkProjectile = null;
		terrierLayers = null;
		deathRenderer = null;
		rends = null;
		flameOffset = null;
		flame = null;
		flameAnimator = null;
		barkFXRenderer = null;
		barkFXAnimator = null;
		pivotPoint = null;
		damageDealer = null;
	}
}
