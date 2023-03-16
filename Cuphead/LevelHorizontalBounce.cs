using UnityEngine;

public class LevelHorizontalBounce : AbstractCollidableObject
{
	[SerializeField]
	private bool onLeft;

	[SerializeField]
	private float fanForce = 1f;

	private LevelPlayerMotor.VelocityManager.Force scrollForce;

	private void Start()
	{
		scrollForce = new LevelPlayerMotor.VelocityManager.Force(LevelPlayerMotor.VelocityManager.Force.Type.All, (!onLeft) ? (0f - fanForce) : fanForce);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		LevelPlayerMotor component = hit.GetComponent<LevelPlayerMotor>();
		if (phase != CollisionPhase.Exit)
		{
			FanOn(component);
		}
		else
		{
			FanOff(hit.GetComponent<LevelPlayerMotor>());
		}
	}

	private void FanOn(LevelPlayerMotor player)
	{
		player.AddForce(scrollForce);
	}

	private void FanOff(LevelPlayerMotor player)
	{
		player.RemoveForce(scrollForce);
	}
}
