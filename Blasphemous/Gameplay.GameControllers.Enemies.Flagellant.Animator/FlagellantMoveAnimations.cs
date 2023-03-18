using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Flagellant.Animator;

public class FlagellantMoveAnimations : EnemyAnimatorInyector
{
	private Flagellant _flagellant;

	private EnemyFloorChecker _floorChecker;

	protected override void OnStart()
	{
		base.OnStart();
		_flagellant = (Flagellant)OwnerEntity;
		_floorChecker = _flagellant.EnemyFloorChecker();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ((!_floorChecker.IsGrounded || _floorChecker.IsSideBlocked) && _flagellant.MotionLerper.IsLerping)
		{
			_flagellant.MotionLerper.StopLerping();
		}
	}

	public void PlayFootStep()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlayFootStep();
		}
	}

	public void PlayRunning()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlayRunning();
		}
	}

	public void PlayLanding()
	{
		if (_flagellant.Status.IsVisibleOnCamera)
		{
			_flagellant.Audio.PlayLandingSound();
		}
	}
}
