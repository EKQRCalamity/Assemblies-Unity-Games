using System;
using System.Collections;
using UnityEngine;

public class ChessBOldBReduxLevelBirdie : AbstractProjectile
{
	public delegate void OnParryBirdie(bool correctBall);

	private const float ONE = 1f;

	public OnParryBirdie ParryBirdie;

	[SerializeField]
	private Color defaultColor;

	[SerializeField]
	private Color pinkColor;

	private LevelProperties.ChessBOldB.Birdie properties;

	private SpriteRenderer sprite;

	private Transform pivotPoint;

	private int timesToChangeDir;

	private float angle;

	private float loopSize;

	private float rotationTime;

	private bool isMoving;

	private bool chosenBall;

	private bool offScreen;

	protected override float DestroyLifetime => 0f;

	public void Setup(Transform pivotPoint, float angle, LevelProperties.ChessBOldB.Birdie properties, float loopSize, bool chosenBall)
	{
		this.angle = angle;
		this.pivotPoint = pivotPoint;
		this.properties = properties;
		this.loopSize = loopSize;
		isMoving = false;
		this.chosenBall = chosenBall;
		RepositionBall();
		sprite.color = defaultColor;
		SetParryable(parryable: false);
	}

	protected override void Awake()
	{
		sprite = GetComponent<SpriteRenderer>();
		base.Awake();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (isMoving)
		{
			MoveBirdie();
		}
	}

	public void StopMoving()
	{
		isMoving = false;
	}

	public void HandleMovement(float rotationTime, bool goingClockwise)
	{
		this.rotationTime = ((!goingClockwise) ? (0f - rotationTime) : rotationTime);
		isMoving = true;
	}

	public void RepositionBall()
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		angle *= (float)Math.PI / 180f;
		zero = new Vector3(Mathf.Sin(angle) * loopSize, 0f, 0f);
		zero2 = new Vector3(0f, Mathf.Cos(angle) * loopSize, 0f);
		base.transform.position = (Vector2)pivotPoint.position;
		base.transform.position += zero + zero2;
		offScreen = false;
		damageDealer.SetDamageFlags(damagesPlayer: false, damagesEnemy: false, damagesOther: false);
	}

	private void MoveBirdie()
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		angle += rotationTime * CupheadTime.FixedDelta;
		zero = new Vector3(Mathf.Sin(angle) * loopSize, 0f, 0f);
		zero2 = new Vector3(0f, Mathf.Cos(angle) * loopSize, 0f);
		base.transform.position = (Vector2)pivotPoint.position;
		base.transform.position += zero + zero2;
	}

	public override void OnParry(AbstractPlayerController player)
	{
		if (ParryBirdie != null)
		{
			isMoving = false;
			ParryBirdie(chosenBall);
			StartCoroutine(turn_off_collider_cr());
			if (!chosenBall)
			{
				StartCoroutine(attack_cr());
			}
		}
	}

	public void TurnPink()
	{
		SetParryable(parryable: true);
		sprite.color = pinkColor;
	}

	private IEnumerator turn_off_collider_cr()
	{
		GetComponent<Collider2D>().enabled = false;
		yield return CupheadTime.WaitForSeconds(this, properties.colliderOffTime);
		GetComponent<Collider2D>().enabled = true;
		offScreen = false;
		damageDealer.SetDamageFlags(damagesPlayer: true, damagesEnemy: false, damagesOther: false);
	}

	private IEnumerator attack_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		AbstractPlayerController player = PlayerManager.GetNext();
		Vector3 dir2 = player.transform.position - base.transform.position;
		float angle = MathUtils.DirectionToAngle(dir2);
		bool changedDirection = false;
		float straightTime = 0f;
		float timeToStraight = properties.timeToStraight;
		float arcTime = 0f;
		float timeToArc = properties.timeToMaxSpeed;
		MinMax xSpeedMinMax = properties.xSpeed;
		MinMax ySpeedMinMax = properties.ySpeed;
		float xSpeed = 0f;
		float ySpeed = 0f;
		StartCoroutine(check_bounds_cr());
		while (!offScreen)
		{
			if (arcTime < timeToArc)
			{
				arcTime += CupheadTime.FixedDelta;
				xSpeed = xSpeedMinMax.GetFloatAt(arcTime / timeToArc);
				ySpeed = ySpeedMinMax.GetFloatAt(1f - arcTime / timeToArc);
			}
			if (xSpeed > 0f && !changedDirection)
			{
				if (straightTime < timeToStraight)
				{
					straightTime += CupheadTime.FixedDelta;
					dir2 = player.transform.position - base.transform.position;
					angle = MathUtils.DirectionToAngle(dir2);
				}
				else
				{
					changedDirection = true;
				}
			}
			Vector3 speed2 = new Vector3(xSpeed, ySpeed);
			Quaternion rot = Quaternion.Euler(0f, 0f, angle);
			speed2 = rot * speed2;
			base.transform.position += speed2 * CupheadTime.FixedDelta;
			yield return wait;
		}
	}

	private IEnumerator check_bounds_cr()
	{
		float offset = 200f;
		while (base.transform.position.x < (float)Level.Current.Right + offset && base.transform.position.x > (float)Level.Current.Left - offset && base.transform.position.y < (float)Level.Current.Ceiling + offset && base.transform.position.y > (float)Level.Current.Ground - offset)
		{
			yield return null;
		}
		offScreen = true;
		yield return null;
	}
}
