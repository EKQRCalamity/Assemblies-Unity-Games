using UnityEngine;

public class FlyingMermaidLevelLaser : AbstractCollidableObject
{
	private float stoneTime = 5f;

	private bool checkCollider;

	public void SetStoneTime(float stoneTime)
	{
		this.stoneTime = stoneTime;
	}

	public void StartLaser()
	{
		if ((bool)GetComponent<Collider2D>())
		{
			checkCollider = true;
		}
	}

	public void StopLaser()
	{
		if ((bool)GetComponent<Collider2D>())
		{
			checkCollider = false;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (checkCollider)
		{
			hit.GetComponent<PlanePlayerController>().GetStoned(stoneTime);
		}
	}
}
