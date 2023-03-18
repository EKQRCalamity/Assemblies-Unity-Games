using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Fool.AI;

public class FoolBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	public float DistanceToTarget;

	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float MaxTimeAwaitingBeforeGoBack;

	[FoldoutGroup("Motion Settings", true, 0)]
	public LayerMask GroundLayerMask;

	private RaycastHit2D[] _bottomHits;

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	public float _myWidth;

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	public float _myHeight;

	private bool _isSpawning;

	public Fool Fool { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		Fool = (Fool)Entity;
		Fool.Target = GetTarget().gameObject;
		_bottomHits = new RaycastHit2D[2];
	}

	public bool CanWalk()
	{
		return !_isSpawning;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		DistanceToTarget = Vector2.Distance(Fool.transform.position, Fool.Target.transform.position);
		if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StartBehaviour();
		}
	}

	public override void Idle()
	{
		StopMovement();
	}

	public override void Wander()
	{
	}

	public void Chase(Vector3 position)
	{
		if (DistanceToTarget > 1f)
		{
			LookAtTarget(position);
		}
		if (!Fool.MotionChecker.HitsFloor || Fool.MotionChecker.HitsBlock || Fool.Status.Dead || Core.Logic.Penitent.Dead)
		{
			StopMovement();
			return;
		}
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		Fool.Input.HorizontalInput = horizontalInput;
		Fool.AnimatorInyector.Walk();
	}

	public void ChargeExplosion()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
	}

	public override void Damage()
	{
	}

	public void Death()
	{
		StopMovement();
		Fool.AnimatorInyector.Death();
	}

	public bool TargetCanBeVisible()
	{
		float num = Fool.Target.transform.position.y - Fool.transform.position.y;
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight;
	}

	public override void StopMovement()
	{
		Fool.Input.HorizontalInput = 0f;
		Fool.AnimatorInyector.StopWalk();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Fool.transform.position.x)
		{
			if (Fool.Status.Orientation != 0)
			{
				Fool.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				Fool.AnimatorInyector.TurnAround();
			}
		}
		else if (Fool.Status.Orientation != EntityOrientation.Left)
		{
			Fool.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			Fool.AnimatorInyector.TurnAround();
		}
	}
}
