using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class CrawlerProjectile : StraightProjectile
{
	public EntityMotionChecker motionChecker;

	public EntityOrientation debugOrientation;

	private Quaternion crawlUp;

	private Quaternion crawlDown;

	public Vector2 acceleration;

	public Animator anim;

	public float minAnimSpeed = 0.6f;

	public float maxAnimSpeed = 1f;

	private float maxSpeed = 16f;

	private float minSpeed = 4f;

	public Transform particleTransform;

	public override void Init(Vector3 direction, float speed)
	{
		base.Init(direction, speed);
		if (direction.x < 0f)
		{
			SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			particleTransform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			particleTransform.localScale = new Vector3(1f, 1f, 1f);
		}
		motionChecker.SnapToGround(base.transform, 10f);
	}

	public override void Init(Vector3 origin, Vector3 target, float speed)
	{
		Init(target - origin, speed);
	}

	protected override void OnStart()
	{
		base.OnStart();
		crawlUp = Quaternion.Euler(0f, 0f, 90f);
		crawlDown = Quaternion.Euler(0f, 0f, -90f);
	}

	protected override void OnUpdate()
	{
		CheckGround();
		UpdateMovement();
		_currentTTL -= Time.deltaTime;
		if (_currentTTL < 0f)
		{
			OnLifeEnded();
		}
		debugOrientation = Status.Orientation;
		if (Status.Orientation == EntityOrientation.Left)
		{
			velocity -= acceleration * Time.deltaTime;
		}
		else
		{
			velocity += acceleration * Time.deltaTime;
		}
		UpdateAnimSpeed();
	}

	private void UpdateAnimSpeed()
	{
		float speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, (Mathf.Abs(velocity.x) - minSpeed) / (maxSpeed - minSpeed));
		anim.speed = speed;
	}

	private void CheckGround()
	{
		Quaternion quaternion = ((Status.Orientation != 0) ? crawlDown : crawlUp);
		Quaternion quaternion2 = ((Status.Orientation != 0) ? crawlUp : crawlDown);
		if (motionChecker.HitsBlock)
		{
			base.transform.rotation = quaternion * base.transform.rotation;
			motionChecker.SnapToGround(base.transform, 2f);
		}
		else if (!motionChecker.HitsFloor)
		{
			base.transform.rotation = quaternion2 * base.transform.rotation;
			motionChecker.SnapToGround(base.transform, 2f);
		}
	}

	private void UpdateMovement()
	{
		base.transform.Translate(velocity * Time.deltaTime, Space.Self);
	}

	private void UpdateRotation()
	{
		if (faceVelocityDirection)
		{
			Vector2 normalized = velocity.normalized;
			float num = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			if (normalized.x < 0f)
			{
				num += 180f;
			}
			spriteRenderer.transform.eulerAngles = new Vector3(0f, 0f, num);
		}
	}
}
