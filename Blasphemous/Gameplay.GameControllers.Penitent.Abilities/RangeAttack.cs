using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Gizmos;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class RangeAttack : Ability
{
	public static readonly int GroundRangeAttackAnim = UnityEngine.Animator.StringToHash("GroundRangeAttack");

	public static readonly int MidAirRangeAttackAnim = UnityEngine.Animator.StringToHash("MidAirRangeAttack");

	private Player _rewired;

	private RootMotionDriver _rootMotion;

	private const float MinHeightActivation = 1f;

	[FoldoutGroup("Projectile Settings", true, 0)]
	public GameObject RangeAttackProjectile;

	[FoldoutGroup("Projectile Settings", true, 0)]
	public GameObject RangeExplosion;

	[SerializeField]
	[BoxGroup("Audio Attack", true, false, 0)]
	[EventRef]
	public string FireRangeAttackFx;

	private EventInstance _rangeAttackFxInstance;

	private bool _pressedKeyDown;

	private float abilityTimeThreshold = 0.2f;

	private float currentTimeThreshold;

	[FoldoutGroup("Projectile Settings", true, 0)]
	public bool ProjectileIsRunning { get; set; }

	[FoldoutGroup("Projectile Settings", true, 0)]
	public bool RequestProjectile { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_rootMotion = Core.Logic.Penitent.GetComponentInChildren<RootMotionDriver>();
		PoolManager.Instance.CreatePool(RangeAttackProjectile, 1);
		PoolManager.Instance.CreatePool(RangeExplosion, 1);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_rewired = Core.Logic.Penitent.PlatformCharacterInput.Rewired;
		if (_rewired == null)
		{
			return;
		}
		currentTimeThreshold += Time.deltaTime;
		Penitent penitent = Core.Logic.Penitent;
		if (RangeAttackCancelledByAbility(penitent))
		{
			return;
		}
		if (_rewired.GetButtonDown(57) && !_pressedKeyDown)
		{
			_pressedKeyDown = true;
		}
		bool buttonUp = _rewired.GetButtonUp(57);
		if (Core.Input.InputBlocked)
		{
			_pressedKeyDown = false;
		}
		else
		{
			if (!buttonUp || !_pressedKeyDown)
			{
				return;
			}
			UnlockableSkill lastUnlockedSkill = GetLastUnlockedSkill();
			if (!(lastUnlockedSkill == null) && !base.EntityOwner.Status.Dead)
			{
				base.LastUnlockedSkillId = lastUnlockedSkill.id;
				if (!base.Casting && !(currentTimeThreshold < abilityTimeThreshold) && base.HasEnoughFervour)
				{
					CastRangeAttack();
				}
			}
		}
	}

	private void CastRangeAttack()
	{
		if (base.EntityOwner.Status.IsGrounded)
		{
			Cast();
			_pressedKeyDown = false;
			base.EntityOwner.Animator.Play(GroundRangeAttackAnim);
		}
		else if (Core.Logic.Penitent.PlatformCharacterController.GroundDist >= 1f)
		{
			Cast();
			_pressedKeyDown = false;
			base.EntityOwner.Animator.Play(MidAirRangeAttackAnim);
		}
	}

	private bool RangeAttackCancelledByAbility(Penitent player)
	{
		if (ProjectileIsRunning || player.PlatformCharacterController.IsClimbing || player.IsClimbingCliffLede || player.IsGrabbingCliffLede || player.PlatformCharacterInput.IsAttacking)
		{
			currentTimeThreshold = 0f;
			return true;
		}
		return false;
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		Core.Logic.Penitent.Dash.StopCast();
		Core.Audio.EventOneShotPanned(FireRangeAttackFx, base.transform.position, out _rangeAttackFxInstance);
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		if (_rangeAttackFxInstance.isValid())
		{
			_rangeAttackFxInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_rangeAttackFxInstance.release();
		}
	}

	public void StopCastRangeAttack()
	{
		StopCast();
	}

	public Vector3 GetReverseFirePosition()
	{
		Vector3 position = _rootMotion.transform.position;
		Vector3 localPosition = _rootMotion.transform.localPosition;
		float x = position.x - localPosition.x * 2f;
		return new Vector3(x, position.y, 0f);
	}

	public void InstanceProjectile()
	{
		if (!(RangeAttackProjectile == null))
		{
			Vector3 position = ((base.EntityOwner.Status.Orientation != 0) ? GetReverseFirePosition() : _rootMotion.transform.position);
			position.y = Core.Logic.Penitent.DamageArea.Center().y + 0.2f;
			PoolManager.Instance.ReuseObject(RangeAttackProjectile, position, Quaternion.identity);
		}
	}

	public void InstantiateExplosion(Vector3 position)
	{
		PoolManager.Instance.ReuseObject(RangeExplosion, position, Quaternion.identity);
	}
}
