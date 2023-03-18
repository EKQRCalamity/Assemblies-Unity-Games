using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment;
using Gameplay.GameControllers.Penitent.Damage;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class GrabCliffLede : MonoBehaviour
{
	private Penitent _penitent;

	public LayerMask cliffLedeLayer;

	private float deltaEnabligTimeLapse;

	private readonly float enabligTimeLapse = 0.25f;

	private BoxCollider2D grabCliffLedeCollider;

	private Collider2D _grabbedCliffLede;

	private bool _isGrabbedCliffLede;

	private bool _isAirAttacking;

	[SerializeField]
	private float grabCliffLedeCooldown = 0.2f;

	private float remainCooldown;

	public Bounds CliffColliderBoundaries => _grabbedCliffLede.bounds;

	private void Awake()
	{
		grabCliffLedeCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		_penitent = Core.Logic.Penitent;
		PenitentDamageArea damageArea = _penitent.DamageArea;
		damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(PenitentOnDamaged));
	}

	private void Update()
	{
		EnablingCollider();
		if (!_isGrabbedCliffLede)
		{
			remainCooldown -= Time.deltaTime;
		}
		if (!_grabbedCliffLede || remainCooldown > 0f)
		{
			return;
		}
		if (_penitent.IsGrabbingCliffLede && (!_grabbedCliffLede.enabled || !_grabbedCliffLede.gameObject.activeInHierarchy))
		{
			if (!_grabbedCliffLede.enabled)
			{
				_penitent.AnimatorInyector.ClimbCliffLede();
			}
			else if (!_grabbedCliffLede.gameObject.activeInHierarchy)
			{
				_penitent.AnimatorInyector.ReleaseCliffLede();
				_penitent.AnimatorInyector.ManualHangOffCliff();
			}
		}
		_isAirAttacking = false;
		if (!_penitent.Status.IsGrounded)
		{
			_isAirAttacking = _penitent.Animator.GetCurrentAnimatorStateInfo(0).IsName("Air Attack 1") || _penitent.Animator.GetCurrentAnimatorStateInfo(0).IsName("Air Attack 2");
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((cliffLedeLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			_grabbedCliffLede = other;
		}
	}

	private void OnTriggerStay2D(Collider2D cliffLedeCollider)
	{
		if ((cliffLedeLayer.value & (1 << cliffLedeCollider.gameObject.layer)) <= 0 || _penitent.IsGrabbingCliffLede || remainCooldown > 0f)
		{
			return;
		}
		CliffLede component = cliffLedeCollider.GetComponent<CliffLede>();
		if (component != null && !_penitent.IsJumpingOff && !_penitent.IsDashing && _penitent.AnimatorInyector.IsFalling && !_isGrabbedCliffLede && !_isAirAttacking && component.isClimbable && !_penitent.Status.IsGrounded)
		{
			_penitent.AnimatorInyector.UpdateAirAttackingAction();
			if (!_penitent.AnimatorInyector.IsAirAttacking)
			{
				EntityOrientation orientation = _penitent.Status.Orientation;
				_isGrabbedCliffLede = true;
				grabCliffLede(component, orientation);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (_isGrabbedCliffLede)
		{
			_isGrabbedCliffLede = !_isGrabbedCliffLede;
		}
	}

	private void PenitentOnDamaged(Penitent damaged, Hit hit)
	{
		if (_penitent.canClimbCliffLede && hit.DamageType == DamageArea.DamageType.Heavy)
		{
			_penitent.canClimbCliffLede = false;
		}
	}

	private void EnablingCollider()
	{
		deltaEnabligTimeLapse += Time.deltaTime;
		if (deltaEnabligTimeLapse >= enabligTimeLapse)
		{
			if (!grabCliffLedeCollider.enabled)
			{
				grabCliffLedeCollider.enabled = true;
			}
		}
		else if (grabCliffLedeCollider.enabled)
		{
			grabCliffLedeCollider.enabled = false;
		}
	}

	private void grabCliffLede(CliffLede cliffLede, EntityOrientation playerOrientation)
	{
		_penitent.IsGrabbingCliffLede = cliffLede.CliffLedeGrabSideAllowed == playerOrientation;
		_penitent.CliffLedeOrientation = cliffLede.CliffLedeGrabSideAllowed;
		_penitent.RootTargetPosition = cliffLede.RootTarget.transform.position;
	}

	public void ReleaseCliffLede()
	{
		_penitent.IsGrabbingCliffLede = false;
		deltaEnabligTimeLapse = 0f;
		remainCooldown = grabCliffLedeCooldown;
	}

	private void OnDestroy()
	{
		if ((bool)_penitent)
		{
			PenitentDamageArea damageArea = _penitent.DamageArea;
			damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(PenitentOnDamaged));
		}
	}
}
