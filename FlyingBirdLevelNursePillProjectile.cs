using UnityEngine;

public class FlyingBirdLevelNursePillProjectile : BasicProjectile
{
	public enum PillColor
	{
		Yellow,
		Blue,
		LightPink,
		DarkPink
	}

	[SerializeField]
	private GameObject yellowPill;

	[SerializeField]
	private GameObject bluePill;

	[SerializeField]
	private GameObject lightPinkPill;

	[SerializeField]
	private GameObject darkPinkPill;

	public void SetPillColor(PillColor color)
	{
		switch (color)
		{
		case PillColor.Yellow:
			yellowPill.SetActive(value: true);
			break;
		case PillColor.Blue:
			bluePill.SetActive(value: true);
			break;
		case PillColor.LightPink:
			lightPinkPill.SetActive(value: true);
			break;
		default:
			darkPinkPill.SetActive(value: true);
			break;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		Object.Destroy(base.gameObject);
	}
}
