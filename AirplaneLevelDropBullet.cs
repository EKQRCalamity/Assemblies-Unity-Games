using System;
using System.Collections;
using UnityEngine;

public class AirplaneLevelDropBullet : AbstractProjectile
{
	private const float SPAWN_OFFSET = 100f;

	private const float ARC_HEIGHT = 600f;

	private Vector3 moveDir;

	private Vector3 startPos;

	private Vector3 targetPos;

	private float shootSpeed;

	private float dropSpeed;

	private float YtoSwitch;

	private float bounds;

	private bool goingDown;

	private bool onLeft;

	[SerializeField]
	private CircleCollider2D circColl;

	[SerializeField]
	private BoxCollider2D boxColl;

	[SerializeField]
	private Effect shootFX;

	[SerializeField]
	private SpriteRenderer speedLines;

	[SerializeField]
	private SpriteRenderer rend;

	private float t;

	public bool isMoving { get; private set; }

	public virtual AirplaneLevelDropBullet Init(Vector3 targetPos, Vector3 startPos, float dropSpeed, float shootSpeed, bool onLeft, bool camHorizontal)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = startPos;
		YtoSwitch = targetPos.y;
		this.shootSpeed = shootSpeed;
		this.onLeft = onLeft;
		base.transform.SetScale(onLeft ? 1 : (-1));
		rend.sortingOrder = ((!onLeft) ? 501 : 500);
		this.targetPos = targetPos;
		bounds = ((!camHorizontal) ? CupheadLevelCamera.Current.Bounds.xMax : CupheadLevelCamera.Current.Bounds.yMax);
		this.dropSpeed = dropSpeed;
		moveDir = ((!onLeft) ? Vector3.right : Vector3.left);
		goingDown = true;
		isMoving = true;
		boxColl.enabled = false;
		circColl.enabled = true;
		t = (float)Math.PI / 4f;
		this.startPos = startPos + Vector3.down * (Mathf.Sin(t) * 600f);
		return this;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(rotate_cr());
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (goingDown)
		{
			t += CupheadTime.FixedDelta * dropSpeed;
			if (t < (float)Math.PI)
			{
				base.transform.position = new Vector3(EaseUtils.EaseOutSine(startPos.x, targetPos.x, Mathf.InverseLerp((float)Math.PI / 4f, (float)Math.PI, t)), startPos.y + Mathf.Sin(t) * 600f);
			}
			else
			{
				base.transform.position += Vector3.up * (Mathf.Sin((float)Math.PI) - Mathf.Sin((float)Math.PI - CupheadTime.FixedDelta * dropSpeed)) * 600f;
			}
			if (base.transform.position.y < YtoSwitch)
			{
				base.transform.position = new Vector3(base.transform.position.x, YtoSwitch);
				moveDir = ((!onLeft) ? Vector3.left : Vector3.right);
				dropSpeed = shootSpeed;
				goingDown = false;
				base.animator.SetTrigger("ToShoot");
				boxColl.enabled = true;
				circColl.enabled = false;
				shootFX.Create(base.transform.position, base.transform.localScale);
				t = 0f;
			}
		}
		else
		{
			t += CupheadTime.FixedDelta;
			if (t > 1f / 3f)
			{
				speedLines.enabled = false;
			}
			base.transform.position += moveDir * dropSpeed * CupheadTime.FixedDelta;
			if (base.transform.position.x < 0f - bounds - 100f || base.transform.position.x > bounds + 100f)
			{
				isMoving = false;
				this.Recycle();
			}
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator rotate_cr()
	{
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		float startRotation = 360f;
		float rotateSpeed = 1f;
		float rotateTime = 0f;
		while (goingDown)
		{
			base.transform.SetEulerAngles(null, null, startRotation * rotateTime);
			rotateTime += CupheadTime.FixedDelta * rotateSpeed;
			yield return wait;
		}
		base.transform.SetEulerAngles(null, null, startRotation);
		yield return null;
	}
}
