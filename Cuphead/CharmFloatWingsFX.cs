using UnityEngine;

public class CharmFloatWingsFX : Effect
{
	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Vector3 vel;

	[SerializeField]
	private float startSpeedMin = 10f;

	[SerializeField]
	private float startSpeedMax = 20f;

	[SerializeField]
	private float slowFactor = 0.95f;

	[SerializeField]
	private float riseFactor = 0.02f;

	public override Effect Create(Vector3 position, Vector3 scale)
	{
		CharmFloatWingsFX charmFloatWingsFX = base.Create(position, scale) as CharmFloatWingsFX;
		charmFloatWingsFX.anim.Play("Feather", 0, Random.Range(0f, 0.5f));
		charmFloatWingsFX.vel = MathUtils.AngleToDirection(Random.Range(0, 360)) * (Random.Range(startSpeedMin, startSpeedMax) + ((!(Random.Range(0f, 6f) < 1f)) ? 0f : startSpeedMax));
		return charmFloatWingsFX;
	}

	private void FixedUpdate()
	{
		base.transform.position += vel;
		vel *= slowFactor;
		vel.y += riseFactor;
	}
}
