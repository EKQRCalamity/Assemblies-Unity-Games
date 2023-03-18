using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellCarrier.Animator;

public class BellCarrierAnimatorInyector : EnemyAnimatorInyector
{
	public enum DamageSoundType
	{
		BeforeTurnAround,
		AfterTurnAround
	}

	private readonly int _wallCrashAnimHashName = UnityEngine.Animator.StringToHash("WallCrash");

	private BellCarrier _bellCarrier;

	private CameraManager _cameraManager;

	private ColorFlash _colorFlash;

	public DamageSoundType CurrentDamageSoundType;

	public UnityEngine.Animator Animator => base.EntityAnimator;

	public bool IsInWallCrashAnim => base.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("WallCrash");

	protected override void OnAwake()
	{
		base.OnAwake();
		_colorFlash = GetComponent<ColorFlash>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_cameraManager = Core.Logic.CameraManager;
		_bellCarrier = GetComponentInParent<BellCarrier>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool targetInLine = _bellCarrier.BellCarrierBehaviour.TargetInLine;
		base.EntityAnimator.SetBool("TARGET_IN_LINE", targetInLine);
	}

	public void Idle()
	{
		if (!(base.EntityAnimator == null))
		{
		}
	}

	public void Awaken()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("AWAKE");
		}
	}

	public void Chasing()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CHASING", value: true);
		}
	}

	public void Stop()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetBool("CHASING", value: false);
			base.EntityAnimator.ResetTrigger("TURN_AROUND");
		}
	}

	public void PlayStopAnimation()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("StopRun");
		}
	}

	public void TurnAround()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("TURN_AROUND");
		}
	}

	public void ResetTurnAround()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.ResetTrigger("TURN_AROUND");
		}
	}

	public void PlayWallCrushAnimation()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play(_wallCrashAnimHashName, 0, 0f);
		}
	}

	public void PlayBellHidden()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.Play("IdleToHidden");
		}
	}

	public void Death()
	{
		if (!(base.EntityAnimator == null))
		{
			base.EntityAnimator.SetTrigger("DEATH");
		}
	}

	public void TriggerColorFlash()
	{
		if (!(_colorFlash == null))
		{
			_colorFlash.TriggerColorFlash();
		}
	}

	public void DropBellCameraShake()
	{
		if (!(_cameraManager == null) && OwnerEntity.SpriteRenderer.isVisible)
		{
			Core.Logic.CameraShakeManager.ShakeUsingPreset("SimpleHit");
		}
	}

	public void WallCrushCameraShake()
	{
		if (!(_cameraManager == null) && OwnerEntity.SpriteRenderer.isVisible)
		{
			Core.Logic.CameraShakeManager.ShakeUsingPreset("SimpleHit", shakeOverthrow: true);
		}
	}

	public void HeavyStepCameraShake()
	{
		if (!(_cameraManager == null) && OwnerEntity.SpriteRenderer.isVisible)
		{
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("LadderLanding");
		}
	}

	public void SetDamageSoundType(DamageSoundType soundType)
	{
		CurrentDamageSoundType = soundType;
	}

	public void PlayRun()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayRun();
		}
	}

	public void PlayRunStop()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayRunStop();
		}
	}

	public void PlayDropBell()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.DropBell();
		}
	}

	public void StartToRun()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayStartToRun();
		}
	}

	public void PlayTurnAround()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayTurnAround();
		}
	}

	public void PlayTurnAroundRun()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayTurnAroundRun();
		}
	}

	public void PlayFrontHit()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayBellCarrierFrontHit();
		}
	}

	public void PlayWallCrush()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayBellCarrierWallCrush();
		}
	}

	public void PlayDeath()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayDeath();
		}
	}

	public void PlayWakeUp()
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Audio.PlayWakeUp();
		}
	}
}
