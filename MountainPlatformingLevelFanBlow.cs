using UnityEngine;

public class MountainPlatformingLevelFanBlow : AbstractCollidableObject
{
	[SerializeField]
	private MountainPlatformingLevelFan parent;

	private LevelPlayerMotor.VelocityManager.Force scrollForce;

	private void Start()
	{
		scrollForce = new LevelPlayerMotor.VelocityManager.Force(LevelPlayerMotor.VelocityManager.Force.Type.All, parent.GetSpeed());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			if ((bool)hit.GetComponent<LevelPlayerMotor>())
			{
				if (parent.fanOn && !parent.Dead)
				{
					FanOn(hit.GetComponent<LevelPlayerMotor>());
				}
				else
				{
					FanOff(hit.GetComponent<LevelPlayerMotor>());
				}
			}
			else
			{
				FanOff(hit.GetComponent<LevelPlayerMotor>());
			}
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
