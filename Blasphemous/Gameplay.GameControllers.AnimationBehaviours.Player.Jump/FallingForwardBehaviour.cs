using System;
using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Jump;

public class FallingForwardBehaviour : StateMachineBehaviour
{
	private bool _colliderRepositioned;

	private Vector3 _defaultColliderSize;

	public float HorizontalCollisionOffset = 0.3f;

	private bool IsDashing;

	public LayerMask RayCastLayerDetection;

	private WaitForEndOfFrame waitForEndOfFrame;

	private Gameplay.GameControllers.Penitent.Penitent _penitent { get; set; }

	private SmartPlatformCollider Collider { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!_penitent)
		{
			_penitent = Core.Logic.Penitent;
			Gameplay.GameControllers.Penitent.Abilities.Dash dash = _penitent.Dash;
			dash.OnStartDash = (Core.SimpleEvent)Delegate.Combine(dash.OnStartDash, new Core.SimpleEvent(OnStartDash));
		}
		IsDashing = false;
		if (waitForEndOfFrame == null)
		{
			waitForEndOfFrame = new WaitForEndOfFrame();
		}
		if (Collider == null)
		{
			Collider = _penitent.PlatformCharacterController.SmartPlatformCollider;
		}
		_defaultColliderSize = _penitent.DamageArea.DefaultSkinColliderSize;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!IsSideBlocked() && _penitent.PlatformCharacterController.PlatformCharacterPhysics.Velocity.y < -3f)
		{
			SetColliderHorizontalOffset();
		}
		if (!(_penitent.PlatformCharacterController.GroundDist > 1f))
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(Core.Logic.Penitent.transform.position, Vector2.down, 1.5f, RayCastLayerDetection);
			if ((bool)raycastHit2D && (bool)raycastHit2D.transform.GetComponent<Slope>())
			{
				SetDefaultCollider();
			}
		}
	}

	private void OnStartDash()
	{
		IsDashing = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		float num = Math.Abs(_penitent.PlatformCharacterController.SlopeAngle);
		if (_penitent.Status.IsGrounded && num < 1f)
		{
			SetDelayDefaultCollider();
		}
		else
		{
			SetDefaultCollider();
		}
	}

	private bool IsSideBlocked()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(GetRayCastOrigin(1.1f), -Vector2.right, 1.25f, RayCastLayerDetection);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(GetRayCastOrigin(1.1f), Vector2.right, 1.25f, RayCastLayerDetection);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(GetRayCastOrigin(-0.1f), -Vector2.right, 1.25f, RayCastLayerDetection);
		RaycastHit2D raycastHit2D4 = Physics2D.Raycast(GetRayCastOrigin(-0.1f), Vector2.right, 1.25f, RayCastLayerDetection);
		bool flag = (bool)raycastHit2D3.collider || (bool)raycastHit2D.collider;
		bool flag2 = (bool)raycastHit2D4.collider || (bool)raycastHit2D2.collider;
		return flag || flag2;
	}

	private void SetColliderHorizontalOffset()
	{
		if (!_colliderRepositioned)
		{
			_colliderRepositioned = true;
			Collider.Size = new Vector2(Collider.Size.x + HorizontalCollisionOffset, Collider.Size.y);
		}
	}

	private void SetDefaultCollider()
	{
		if (_colliderRepositioned)
		{
			_colliderRepositioned = false;
			Collider.Size = _defaultColliderSize;
		}
	}

	private void SetDelayDefaultCollider()
	{
		if (_colliderRepositioned)
		{
			Singleton<Core>.Instance.StartCoroutine(DefaultColliderCoroutine());
		}
	}

	private IEnumerator DefaultColliderCoroutine()
	{
		yield return waitForEndOfFrame;
		if (!IsDashing)
		{
			SetDefaultCollider();
		}
		else
		{
			_colliderRepositioned = false;
		}
	}

	private Vector2 GetRayCastOrigin(float heightOffset = 0f)
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		return new Vector2(position.x, position.y + heightOffset);
	}

	private void OnDestroy()
	{
		if ((bool)_penitent)
		{
			Gameplay.GameControllers.Penitent.Abilities.Dash dash = _penitent.Dash;
			dash.OnStartDash = (Core.SimpleEvent)Delegate.Remove(dash.OnStartDash, new Core.SimpleEvent(OnStartDash));
		}
	}
}
