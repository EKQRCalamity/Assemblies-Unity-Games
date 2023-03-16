using UnityEngine;

public class CharmFloatWingsFXAlt : Effect
{
	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Vector3 vel;

	[SerializeField]
	private float startVel;

	[SerializeField]
	private float startSpeed = 30f;

	[SerializeField]
	private float slowFactor = 0.95f;

	[SerializeField]
	private float riseFactor = 0.02f;

	public override Effect Create(Vector3 position, Vector3 scale)
	{
		CharmFloatWingsFXAlt charmFloatWingsFXAlt = base.Create(position, scale) as CharmFloatWingsFXAlt;
		charmFloatWingsFXAlt.anim.speed = 1f;
		charmFloatWingsFXAlt.anim.Play("Feather", 0, Random.Range(0f, 0.5f));
		charmFloatWingsFXAlt.vel = MathUtils.AngleToDirection(Random.Range(-45, -145) + ((!Rand.Bool()) ? (-50) : 50)) * startSpeed;
		charmFloatWingsFXAlt.vel.y = 0f;
		charmFloatWingsFXAlt.startVel = charmFloatWingsFXAlt.vel.x;
		charmFloatWingsFXAlt.transform.rotation = Quaternion.Euler(0f, 0f, MathUtils.DirectionToAngle(vel) + -90f * Mathf.Sign(startVel));
		return charmFloatWingsFXAlt;
	}

	private void FixedUpdate()
	{
		if (CupheadTime.FixedDelta > 0f)
		{
			base.transform.position += vel;
			vel -= slowFactor * startVel * Vector3.right;
			vel.y += riseFactor;
			if (Mathf.Sign(vel.x) != Mathf.Sign(startVel))
			{
				vel.x *= 0.95f;
			}
			base.transform.rotation = Quaternion.Euler(0f, 0f, MathUtils.DirectionToAngle(vel) + -90f * Mathf.Sign(startVel));
		}
	}
}
