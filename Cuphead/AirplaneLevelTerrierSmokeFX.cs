using UnityEngine;

public class AirplaneLevelTerrierSmokeFX : Effect
{
	public SpriteRenderer rend;

	public Vector3 vel;

	public bool dead;

	public new bool inUse = true;

	public Transform myTransform;

	public void Step(float t)
	{
		myTransform.position += vel * t;
	}

	protected override void OnEffectComplete()
	{
		if (dead)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			inUse = false;
		}
	}
}
