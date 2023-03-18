using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Animations;
using Gameplay.GameControllers.Penitent.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Animator;

public class PenitentMoveAnimations : EntityAnimationEvents
{
	public Core.SimpleEventParam OnStep;

	private Penitent _penitent;

	private PenitentAudio _penitentAudio;

	[Header("Freze Times")]
	[Range(0f, 1f)]
	public float freezeTime = 0.2f;

	[Range(0f, 1f)]
	public float freezeTimeFactorPercentage = 0.2f;

	private StepDustSpawner _stepDustSpawner;

	private PushBackDust _pushBackDust;

	private PushBackSpark _pushBackSpark;

	private ThrowbackDust _throwbackDust;

	private GhostTrailGenerator _ghostTrailGenerator;

	private void Awake()
	{
		_penitent = GetComponentInParent<Penitent>();
	}

	private void Start()
	{
		_stepDustSpawner = _penitent.GetComponentInChildren<StepDustSpawner>();
		_pushBackDust = _penitent.GetComponentInChildren<PushBackDust>();
		_pushBackSpark = _penitent.GetComponentInChildren<PushBackSpark>();
		_throwbackDust = _penitent.GetComponentInChildren<ThrowbackDust>();
		_ghostTrailGenerator = _penitent.GetComponentInChildren<GhostTrailGenerator>();
		_penitentAudio = _penitent.Audio;
	}

	public void SetJumpOff(int jumpOff)
	{
		_penitent.IsJumpingOff = jumpOff > 0;
	}

	public void GetStepDust()
	{
		if (!(_stepDustSpawner == null))
		{
			StepDustRoot stepDustRoot = _stepDustSpawner.StepDustRoot;
			Vector2 stepDustPosition = stepDustRoot.transform.position;
			Vector2 vector = stepDustRoot.transform.localPosition;
			stepDustPosition.x = ((_penitent.Status.Orientation != EntityOrientation.Left) ? stepDustPosition.x : (stepDustPosition.x - vector.x * 2f));
			if (_penitent.PlatformCharacterController.GroundDist < 0.5f)
			{
				_stepDustSpawner.GetStepDust(stepDustPosition);
			}
		}
	}

	public void GetPushBackDust()
	{
		if (_pushBackDust != null)
		{
			_pushBackDust.TriggerPushBackDust();
		}
	}

	public void GetThrowbackDust()
	{
		if (_throwbackDust != null)
		{
			_throwbackDust.TriggerThrowbackDust();
		}
	}

	public void GetPushBackSparks()
	{
		if ((bool)_pushBackSpark && !_penitent.Dash.StopByDamage)
		{
			_pushBackSpark.TriggerPushBackSparks();
		}
	}

	public void DisablePhysics()
	{
		_penitent.Physics.Enable2DCollision(enable: false);
	}

	public void FreezeEntity(DamageArea.DamageType damageType)
	{
		float num = freezeTime;
		if (damageType != 0 && damageType == DamageArea.DamageType.Heavy)
		{
			num = freezeTime + freezeTime * freezeTimeFactorPercentage;
		}
		StartCoroutine(_penitent.FreezeAnimator(num));
	}

	public override void Rebound()
	{
		float num = -2.5f;
		if (_penitent.Status.Orientation == EntityOrientation.Left)
		{
			num *= -1f;
		}
		_penitent.PlayerHitMotionLerper.distanceToMove = num;
		_penitent.PlayerHitMotionLerper.TimeTakenDuringLerp = 0.2f;
		Vector3 forwardTangent = _penitent.GetForwardTangent(base.transform.right, _penitent.FloorChecker.BottonNormalCollision);
		_penitent.PlayerHitMotionLerper.StartLerping(forwardTangent.normalized);
	}

	public void SetDashInvulnerable()
	{
		if (!_penitent.Status.Unattacable)
		{
			_penitent.Status.Unattacable = true;
		}
	}

	public void SetDashVulnerable()
	{
		if (_penitent.Status.Unattacable)
		{
			_penitent.Status.Unattacable = false;
		}
	}

	public void ComboFinisherJump()
	{
		StartCoroutine(JumpCoroutine());
	}

	private IEnumerator JumpCoroutine()
	{
		float js = _penitent.PlatformCharacterController.JumpingSpeed;
		float jat = _penitent.PlatformCharacterController.JumpingAccTime;
		_penitent.PlatformCharacterController.JumpingSpeed *= 1.6f;
		_penitent.PlatformCharacterController.JumpingAccTime *= 1f;
		_penitent.PlatformCharacterController.SetActionState(eControllerActions.Jump, value: true);
		yield return new WaitForSeconds(1f);
		_penitent.PlatformCharacterController.JumpingSpeed = js;
		_penitent.PlatformCharacterController.JumpingAccTime = jat;
		_penitent.PlatformCharacterController.SetActionState(eControllerActions.Jump, value: false);
	}

	public void ResizeFallingDamageArea(int resize)
	{
		if ((bool)_penitent)
		{
			_penitent.DamageArea.IsFallingForwardResized = resize > 0;
		}
	}

	public void RaiseStopDust()
	{
		if (!(_penitent == null))
		{
			_penitent.DashDustGenerator.GetStopDashDust();
		}
	}

	public void EnabledGhostTrail(AttackAnimationsEvents.Activation activation)
	{
		if (!(_ghostTrailGenerator == null))
		{
			_ghostTrailGenerator.EnableGhostTrail = activation == AttackAnimationsEvents.Activation.True;
		}
	}

	public void PlayFootStep()
	{
		_penitentAudio.PlayFootStep();
		if (OnStep != null)
		{
			OnStep(_penitent.GetPosition());
		}
	}

	public void PlayRunStop()
	{
		_penitentAudio.PlayRunStopSound();
	}

	public void PlayLanding()
	{
		_penitentAudio.PlayLandingSound();
	}

	public void PlayLandingRunning()
	{
		_penitentAudio.PlayLandingForward();
	}

	public void PlayJump()
	{
		_penitentAudio.PlayJumpSound();
	}

	public void PlayDash()
	{
		_penitentAudio.PlayDashSound();
	}

	public void PlayClimbLadder()
	{
		_penitentAudio.ClimbLadder();
	}

	public void GrabCliffLede()
	{
		_penitentAudio.GrabCliffLede();
	}

	public void ClimbCliffLede()
	{
		_penitentAudio.ClimbCliffLede();
	}

	public void JumpOff()
	{
		_penitentAudio.JumpOff();
	}

	public void PlaySlidingLadder()
	{
		_penitentAudio.SlidingLadderLanding();
	}

	public void PlaySlidingLadderLanding()
	{
		_penitentAudio.SlidingLadderLanding();
	}

	public void PlayStickToWall()
	{
		_penitentAudio.PlayStickToWall();
	}

	public void PlayUnHangFromWall()
	{
		_penitentAudio.PlayUnHangFromWall();
	}

	public void PlayHardLanding()
	{
		_penitentAudio.PlayHardLanding();
	}

	public void PlayIdleSword()
	{
		_penitentAudio.PlayIdleModeSword();
	}

	public void PlayStartDialogue()
	{
		if (Core.Logic.Penitent.IsVisible())
		{
			_penitentAudio.PlayStartDialogue();
		}
	}

	public void PlayEndDialogue()
	{
		_penitentAudio.PlayEndDialogue();
	}
}
