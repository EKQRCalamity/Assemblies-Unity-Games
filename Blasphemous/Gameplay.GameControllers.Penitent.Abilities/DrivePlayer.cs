using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.States;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class DrivePlayer : Ability
{
	private Penitent _penitent;

	private Vector2 _destination;

	private EntityOrientation _finalOrientation;

	public event Core.SimpleEvent OnStartMotion;

	public event Core.SimpleEvent OnStopMotion;

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = (Penitent)base.EntityOwner;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Casting)
		{
			eControllerActions eControllerActions = ((_destination.x >= base.EntityOwner.transform.position.x) ? eControllerActions.Right : eControllerActions.Left);
			if (Mathf.Abs(_penitent.transform.position.x - _destination.x) > 0.1f)
			{
				SetAnimatorRunning();
				_penitent.PlatformCharacterController.SetActionState(eControllerActions, value: true);
				_penitent.SetOrientation((eControllerActions != eControllerActions.Right) ? EntityOrientation.Left : EntityOrientation.Right);
			}
			else
			{
				_penitent.PlatformCharacterController.SetActionState(eControllerActions, value: false);
				_penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
				SetAnimatorRunning(running: false);
				_penitent.SetOrientation(_finalOrientation);
				StartCoroutine(ClampReposition());
			}
		}
	}

	public void MoveToPosition(Vector2 position, EntityOrientation finalOrientation)
	{
		if (!base.Casting)
		{
			_destination = position;
			_finalOrientation = finalOrientation;
			Cast();
		}
	}

	private IEnumerator ClampReposition()
	{
		do
		{
			_penitent.transform.position = new Vector2(_destination.x, base.EntityOwner.transform.position.y);
			yield return null;
		}
		while (Mathf.Abs(_penitent.transform.position.x - _destination.x) > Mathf.Epsilon);
		StopCast();
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		if (this.OnStartMotion != null)
		{
			this.OnStartMotion();
		}
		_penitent.StateMachine.SwitchState<Driven>();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		if (this.OnStopMotion != null)
		{
			this.OnStopMotion();
		}
		_penitent.StateMachine.SwitchState<Playing>();
	}

	private void SetAnimatorRunning(bool running = true)
	{
		base.EntityOwner.Animator.SetBool("RUN_STEP", running);
		base.EntityOwner.Animator.SetBool("RUNNING", running);
	}
}
