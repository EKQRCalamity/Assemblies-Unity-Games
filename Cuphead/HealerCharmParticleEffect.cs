using UnityEngine;

public class HealerCharmParticleEffect : AbstractPausableComponent
{
	[SerializeField]
	private float initialEmissionSpeed = 400f;

	[SerializeField]
	private MinMax timeBeforeSeekRange = new MinMax(0.025f, 0.075f);

	private float timeBeforeSeek;

	[SerializeField]
	private float timeBeforeCanCollect = 0.5f;

	[SerializeField]
	private float timeBeforeLerp = 1f;

	[SerializeField]
	private float maxTime = 0.75f;

	[SerializeField]
	private MinMax accelerationRange = new MinMax(150f, 250f);

	private float acceleration;

	[SerializeField]
	private MinMax maxSpeedRange = new MinMax(1500f, 2500f);

	private float maxSpeed;

	[SerializeField]
	private float contactDistance = 25f;

	private AbstractPlayerController target;

	private float timer;

	private Vector3 vel;

	[SerializeField]
	private float frameTime = 1f / 24f;

	private float frameTimer;

	private HealerCharmSparkEffect main;

	protected override void Awake()
	{
		base.Awake();
		base.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), 1f);
		acceleration = accelerationRange.RandomFloat();
		timeBeforeSeek = timeBeforeSeekRange.RandomFloat();
		maxSpeed = maxSpeedRange.RandomFloat();
		base.animator.Play("Loop", 0, Random.Range(0f, 1f));
		base.animator.Update(0f);
	}

	public void SetVars(Vector2 newVel, AbstractPlayerController newTarget, HealerCharmSparkEffect newMain)
	{
		target = newTarget;
		vel = newVel * initialEmissionSpeed;
		main = newMain;
		base.transform.position += vel * 0.04f;
	}

	private void FixedUpdate()
	{
		if (CupheadTime.FixedDelta != 0f && !(target == null))
		{
			frameTimer += CupheadTime.FixedDelta;
			while (frameTimer > frameTime)
			{
				frameTimer -= frameTime;
				FrameUpdate();
			}
		}
	}

	private void FrameUpdate()
	{
		base.transform.position += vel * frameTime;
		base.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-25f, 25f, Mathf.InverseLerp(0f - maxSpeed, maxSpeed, vel.x)) * (0f - Mathf.Sign(base.transform.localScale.x)));
		timer += frameTime;
		if (!(timer > timeBeforeSeek))
		{
			return;
		}
		Vector3 vector = target.center - base.transform.position;
		float magnitude = vector.magnitude;
		if (magnitude < contactDistance && timer > timeBeforeCanCollect)
		{
			if ((bool)main)
			{
				main.StartPlayerFlash();
			}
			Object.Destroy(base.gameObject);
			return;
		}
		vel += vector * (timer - timeBeforeSeek) * (timer - timeBeforeSeek) * acceleration * frameTime;
		if (vel.magnitude > maxSpeed)
		{
			vel = vel.normalized * maxSpeed;
		}
		if (timer > timeBeforeLerp)
		{
			float num = Mathf.InverseLerp(timeBeforeLerp, maxTime, timer);
			num *= num;
			base.transform.position = Vector3.Lerp(base.transform.position, target.center, num);
		}
	}
}
