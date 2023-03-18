using UnityEngine;

namespace Gameplay.GameControllers.Environment.MovingPlatforms;

[RequireComponent(typeof(Animator))]
public class PlatformGear : MonoBehaviour
{
	public StraightMovingPlatform MovingPlatform { get; set; }

	private Animator GearAnimator { get; set; }

	private void Awake()
	{
		GearAnimator = GetComponent<Animator>();
		MovingPlatform = GetComponentInParent<StraightMovingPlatform>();
	}

	private void Update()
	{
		if ((bool)MovingPlatform)
		{
			EnableGearMotion(MovingPlatform.IsRunning);
		}
	}

	public void EnableGearMotion(bool enableGear)
	{
		GearAnimator.speed = ((!enableGear) ? 0f : 1f);
	}
}
