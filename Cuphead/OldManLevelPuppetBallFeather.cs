using UnityEngine;

public class OldManLevelPuppetBallFeather : Effect
{
	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Vector3 vel;

	private Vector3 fallVel;

	[SerializeField]
	private float startSpeedMin = 10f;

	[SerializeField]
	private float startSpeedMax = 20f;

	[SerializeField]
	private float slowFactor = 0.95f;

	[SerializeField]
	private float fallFactorMin = 0.1f;

	[SerializeField]
	private float fallFactorMax = 0.2f;

	private float fallFactor;

	private float rotateDir;

	private bool skipFrame;

	public override Effect Create(Vector3 position, Vector3 scale)
	{
		OldManLevelPuppetBallFeather oldManLevelPuppetBallFeather = base.Create(position, scale) as OldManLevelPuppetBallFeather;
		oldManLevelPuppetBallFeather.vel = MathUtils.AngleToDirection(Random.Range(0, 360)) * (Random.Range(startSpeedMin, startSpeedMax) + ((!(Random.Range(0f, 6f) < 1f)) ? 0f : startSpeedMax));
		oldManLevelPuppetBallFeather.rotateDir = MathUtils.PlusOrMinus();
		oldManLevelPuppetBallFeather.fallFactor = Random.Range(oldManLevelPuppetBallFeather.fallFactorMin, oldManLevelPuppetBallFeather.fallFactorMax);
		oldManLevelPuppetBallFeather.fallVel.y = Random.Range(0f, 0.5f);
		oldManLevelPuppetBallFeather.anim.speed = Random.Range(0.5f, 0.75f);
		return oldManLevelPuppetBallFeather;
	}

	private void PhysicsUpdate()
	{
		if (CupheadTime.FixedDelta != 0f)
		{
			base.transform.position += vel + fallVel;
			vel *= slowFactor;
			float magnitude = vel.magnitude;
			base.transform.Rotate(new Vector3(0f, 0f, rotateDir * Mathf.InverseLerp(0f, startSpeedMax, magnitude)) * 100f);
			if (magnitude < 1f)
			{
				fallVel.y -= fallFactor;
				base.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(base.transform.eulerAngles.z, 0f, 0.5f));
			}
			if (base.transform.position.y < -560f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void FixedUpdate()
	{
		skipFrame = !skipFrame;
		if (!skipFrame)
		{
			PhysicsUpdate();
			PhysicsUpdate();
		}
	}
}
