using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.Acolyte.Animator;

public class AcolyteMoveAnimations : EnemyAnimatorInyector
{
	private Acolyte _acolyte;

	private EnemyFloorChecker _floorChecker;

	protected override void OnStart()
	{
		base.OnStart();
		_acolyte = (Acolyte)OwnerEntity;
		_floorChecker = _acolyte.EnemyFloorChecker();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!_floorChecker.IsGrounded && _acolyte.MotionLerper.IsLerping)
		{
			_acolyte.MotionLerper.StopLerping();
		}
	}

	public void PlayFootStep()
	{
		if (_acolyte.Status.IsVisibleOnCamera)
		{
			_acolyte.Audio.PlayFootStep();
		}
	}

	public void PlayRunning()
	{
		if (_acolyte.Status.IsVisibleOnCamera)
		{
			_acolyte.Audio.PlayRunning();
		}
	}

	public void PlayStopRunning()
	{
		if (_acolyte.Status.IsVisibleOnCamera)
		{
			_acolyte.Audio.PlayStopRunning();
		}
	}

	public void PlayLanding()
	{
		if (_acolyte.Status.IsVisibleOnCamera)
		{
			_acolyte.Audio.PlayLanding();
		}
	}
}
