using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Damage;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class ThrowBack : Ability
{
	public float Distance = 10f;

	public LayerMask FloorCollisionLayers;

	private Rigidbody2D _rigidbody;

	private Hit _hit;

	private readonly int _fallingOverAnim = Animator.StringToHash("Falling Over");

	private readonly int _fallingBackGroundedAnim = Animator.StringToHash("Grounding Over");

	private PlatformCharacterController _controller;

	private PenitentDamageArea _damageArea;

	private bool _landing;

	private const float TimeThreshold = 0.01f;

	private float _currentTimeThreshold;

	private SmartPlatformCollider m_smartCollider;

	private static readonly int ThrowParam = Animator.StringToHash("THROW");

	public bool IsThrown { get; private set; }

	public bool IsOwnerFalling => _rigidbody.velocity.y < 0f;

	protected override void OnStart()
	{
		base.OnStart();
		m_smartCollider = Core.Logic.Penitent.GetComponentInChildren<SmartPlatformCollider>();
		_rigidbody = base.EntityOwner.GetComponent<Rigidbody2D>();
		_controller = base.EntityOwner.GetComponentInChildren<PlatformCharacterController>();
		_damageArea = base.EntityOwner.GetComponentInChildren<PenitentDamageArea>();
		PenitentDamageArea damageArea = _damageArea;
		damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(OnDamaged));
		Core.Events.OnEventLaunched += PlayerFallsTrap;
		_landing = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_rigidbody == null)
		{
			return;
		}
		_currentTimeThreshold -= Time.deltaTime;
		if (!_rigidbody.isKinematic && CalculateGroundDist() <= 0f && _rigidbody.velocity.y <= 0f && (base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Over") || base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("ThrowbackDesc")))
		{
			if (base.Casting)
			{
				StopCastThrow();
			}
			else
			{
				ToggleAbilities(toggle: true);
				StopCastThrowback();
			}
		}
		if (base.EntityOwner.Status.IsGrounded && !_landing && _currentTimeThreshold <= 0f && _rigidbody.isKinematic)
		{
			_landing = true;
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
			base.EntityOwner.Animator.Play(_fallingBackGroundedAnim);
		}
	}

	private void OnTriggerEnter2D(Collider2D obstacle)
	{
		if (!_landing)
		{
			if ((FloorCollisionLayers.value & (1 << obstacle.gameObject.layer)) > 0)
			{
				StopCastThrow();
			}
			if (obstacle.gameObject.layer == LayerMask.NameToLayer("OneWayDown") && IsOwnerFalling && IsOnCollider(obstacle))
			{
				StopCastThrow();
			}
		}
	}

	private void PlayerFallsTrap(string id, string parameter)
	{
		if (id.Equals("PENITENT_KILLED") || parameter.Equals("ABYSS") || parameter.Equals("SPIKES"))
		{
			StopCastThrowback();
		}
	}

	private void OnDestroy()
	{
		Core.Events.OnEventLaunched -= PlayerFallsTrap;
		if ((bool)_damageArea)
		{
			PenitentDamageArea damageArea = _damageArea;
			damageArea.OnDamaged = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(damageArea.OnDamaged, new PenitentDamageArea.PlayerDamagedEvent(OnDamaged));
		}
	}

	private void OnDamaged(Gameplay.GameControllers.Penitent.Penitent damaged, Hit hit)
	{
		if (hit.DamageType == DamageArea.DamageType.Heavy && _landing)
		{
			_hit = hit;
			if (!base.EntityOwner.Status.Dead)
			{
				Cast();
				return;
			}
			ToggleAbilities(toggle: false);
			CastThrowback();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		CastThrowback();
		Core.Logic.Penitent.Parry.StopCast();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		StopCastThrowback();
	}

	private void CastThrowback()
	{
		if ((bool)base.EntityOwner)
		{
			_currentTimeThreshold = 0.01f;
			_landing = false;
			Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
			base.EntityOwner.Animator.Play(_fallingOverAnim);
			SetRigidbodyType(RigidbodyType2D.Dynamic);
			Throw(GetGoal(_hit));
			if ((bool)base.EntityOwner.Shadow)
			{
				base.EntityOwner.Shadow.gameObject.SetActive(value: false);
			}
			IsThrown = true;
		}
	}

	private void StopCastThrowback()
	{
		if ((bool)base.EntityOwner && _currentTimeThreshold <= 0f)
		{
			_controller.InstantVelocity = Vector3.zero;
			_rigidbody.velocity = Vector2.zero;
			SetRigidbodyType(RigidbodyType2D.Kinematic);
			if (!base.EntityOwner.Dead)
			{
				base.EntityOwner.Animator.SetBool(ThrowParam, value: false);
			}
			if ((bool)base.EntityOwner.Shadow)
			{
				base.EntityOwner.Shadow.gameObject.SetActive(value: true);
			}
			if (base.EntityOwner.SpriteRenderer.isVisible)
			{
				Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("HardFall");
			}
			IsThrown = false;
		}
	}

	private void Throw(Vector3 goal)
	{
		float num = goal.x - base.EntityOwner.transform.position.x;
		float num2 = goal.y - base.EntityOwner.transform.position.y;
		float f = Mathf.Atan((num2 + 10f) / num);
		float num3 = num / Mathf.Cos(f);
		float x = num3 * Mathf.Cos(f);
		float y = num3 * Mathf.Sin(f);
		_rigidbody.velocity = new Vector2(x, y);
	}

	private void SetRigidbodyType(RigidbodyType2D rigidbodyType)
	{
		if (!(_rigidbody == null))
		{
			if (_rigidbody.bodyType != rigidbodyType)
			{
				_rigidbody.bodyType = rigidbodyType;
			}
			if (_rigidbody.bodyType == RigidbodyType2D.Dynamic)
			{
				_rigidbody.gravityScale = 3f;
			}
		}
	}

	private Vector3 GetGoal(Hit hit)
	{
		GameObject attackingEntity = hit.AttackingEntity;
		Vector3 position = base.EntityOwner.transform.position;
		float y = position.y;
		float num = ((!hit.ThrowbackDirByOwnerPosition) ? ((attackingEntity.GetComponent<Entity>().Status.Orientation != EntityOrientation.Left) ? (position.x + Distance * hit.Force) : (position.x - Distance * hit.Force)) : ((!(attackingEntity.transform.position.x >= base.EntityOwner.transform.position.x)) ? (position.x + Distance * hit.Force) : (position.x - Distance * hit.Force)));
		if (base.EntityOwner is Gameplay.GameControllers.Penitent.Penitent && Core.Logic.Penitent.IsGrabbingCliffLede)
		{
			num = ((base.EntityOwner.OrientationBeforeHit != 0) ? ((!(num > position.x)) ? (position.x + 0.5f) : num) : ((!(num < position.x)) ? (position.x - 0.5f) : num));
		}
		return new Vector2(num, y);
	}

	private void StopCastThrow()
	{
		if (!base.EntityOwner.Status.Dead)
		{
			StopCast();
			return;
		}
		ToggleAbilities(toggle: true);
		StopCastThrowback();
	}

	private bool IsOnCollider(Collider2D obstacleCollider2D)
	{
		float y = _damageArea.DamageAreaCollider.bounds.min.y;
		float y2 = obstacleCollider2D.bounds.max.y;
		return y >= y2 - 0.1f;
	}

	protected float CalculateGroundDist()
	{
		float num = float.MaxValue;
		for (int i = 0; i < m_smartCollider.BottomCheckPoints.Count; i++)
		{
			SmartRaycastHit smartRaycastHit = m_smartCollider.SmartRaycast(base.transform.TransformPoint(m_smartCollider.BottomCheckPoints[i]), -base.transform.up, float.MaxValue, (int)m_smartCollider.LayerCollision | (int)m_smartCollider.OneWayCollisionDown);
			if (smartRaycastHit != null)
			{
				num = Mathf.Min(num, smartRaycastHit.distance - m_smartCollider.SkinBottomWidth);
			}
		}
		return num;
	}
}
