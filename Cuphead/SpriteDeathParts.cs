using UnityEngine;

public class SpriteDeathParts : AbstractCollidableObject
{
	public float bottomOffset = 100f;

	public float VelocityXMin = -500f;

	public float VelocityXMax = 500f;

	public float VelocityYMin = 500f;

	public float VelocityYMax = 1000f;

	public float GRAVITY = -100f;

	[SerializeField]
	protected bool clampFallVelocity;

	[SerializeField]
	private bool rotate;

	[SerializeField]
	private Rangef rotationSpeedRange;

	protected Vector2 velocity;

	private float accumulatedGravity;

	private float rotationSpeed;

	private float currentAngle;

	public SpriteDeathParts CreatePart(Vector3 position)
	{
		SpriteDeathParts spriteDeathParts = InstantiatePrefab<SpriteDeathParts>();
		spriteDeathParts.transform.position = position;
		return spriteDeathParts;
	}

	protected override void Awake()
	{
		base.Awake();
		velocity = new Vector2(Random.Range(VelocityXMin, VelocityXMax), Random.Range(VelocityYMin, VelocityYMax));
		if (rotate)
		{
			rotationSpeed = Random.Range(rotationSpeedRange.minimum, rotationSpeedRange.maximum) * (float)Rand.PosOrNeg();
		}
	}

	protected virtual void Update()
	{
		Step(Time.fixedDeltaTime);
	}

	protected virtual void Step(float deltaTime)
	{
		if (clampFallVelocity)
		{
			velocity.y = Mathf.Clamp(velocity.y, -5000f, float.MaxValue);
		}
		base.transform.position += (Vector3)(velocity + new Vector2(0f, accumulatedGravity)) * deltaTime;
		accumulatedGravity += GRAVITY;
		if (rotate)
		{
			currentAngle += rotationSpeed * deltaTime;
			base.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
		}
		if (base.transform.position.y < -360f - bottomOffset)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetVelocityX(float min, float max)
	{
		velocity.x = Random.Range(min, max);
	}

	public void SetVelocityY(float min, float max)
	{
		velocity.y = Random.Range(min, max);
	}
}
