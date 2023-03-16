using System.Collections;
using UnityEngine;

public class RetroArcadeChaser : RetroArcadeEnemy
{
	private AbstractPlayerController player;

	private Vector2 launchVelocity;

	private float homingMoveSpeed;

	private float rotationSpeed;

	private float timeBeforeDeath;

	private float timeBeforeHoming;

	private Vector2 homingDirection;

	protected Vector2 velocity;

	public bool IsDone { get; private set; }

	public bool HomingEnabled { get; set; }

	public virtual RetroArcadeChaser Init(Vector3 pos, float launchRotation, float launchSpeed, float homingMoveSpeed, float rotationSpeed, float timeBeforeDeath, float hp, AbstractPlayerController player, LevelProperties.RetroArcade.Chasers properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		homingDirection = MathUtils.AngleToDirection(launchRotation);
		launchVelocity = MathUtils.AngleToDirection(launchRotation) * launchSpeed;
		base.transform.position = pos;
		this.player = player;
		this.rotationSpeed = rotationSpeed;
		this.homingMoveSpeed = homingMoveSpeed;
		this.timeBeforeDeath = timeBeforeDeath;
		HomingEnabled = true;
		base.hp = hp;
		StartChaser();
		return this;
	}

	private void StartChaser()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		float t = 0f;
		while (t < timeBeforeDeath + timeBeforeHoming)
		{
			while (!HomingEnabled)
			{
				yield return null;
			}
			t += CupheadTime.FixedDelta;
			if (player != null && !player.IsDead)
			{
				Vector3 center = player.center;
				Vector2 direction = (center - base.transform.position).normalized;
				Quaternion b = Quaternion.Euler(0f, 0f, MathUtils.DirectionToAngle(direction));
				Quaternion a = Quaternion.Euler(0f, 0f, MathUtils.DirectionToAngle(homingDirection));
				homingDirection = MathUtils.AngleToDirection(Quaternion.Slerp(a, b, Mathf.Min(1f, CupheadTime.FixedDelta * rotationSpeed)).eulerAngles.z);
			}
			Vector2 homingVelocity = (velocity = homingDirection * homingMoveSpeed);
			if (t < timeBeforeHoming)
			{
				velocity = launchVelocity;
			}
			else if (t < timeBeforeHoming)
			{
				float t2 = EaseUtils.EaseOutSine(0f, 1f, t - timeBeforeHoming);
				velocity = Vector2.Lerp(launchVelocity, homingVelocity, t2);
			}
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
		float offset = 100f;
		while (base.transform.position.x > -640f - offset && base.transform.position.x < 640f + offset && base.transform.position.x > -360f - offset && base.transform.position.x < 360f + offset)
		{
			base.transform.position += (Vector3)velocity.normalized * homingMoveSpeed * CupheadTime.FixedDelta;
			base.transform.SetEulerAngles(0f, 0f, MathUtils.DirectionToAngle(velocity) + 180f);
			yield return new WaitForFixedUpdate();
		}
		IsDone = true;
		this.Recycle();
	}
}
