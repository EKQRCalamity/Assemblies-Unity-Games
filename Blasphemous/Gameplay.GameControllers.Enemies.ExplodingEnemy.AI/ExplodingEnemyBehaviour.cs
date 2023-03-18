using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ExplodingEnemy.AI;

public class ExplodingEnemyBehaviour : EnemyBehaviour
{
	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	public float DistanceToTarget;

	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float MaxTimeAwaitingBeforeGoBack;

	[FoldoutGroup("Activation Settings", true, 0)]
	public bool IsChargingExplosion;

	[FoldoutGroup("Motion Settings", true, 0)]
	public LayerMask GroundLayerMask;

	private RaycastHit2D[] _bottomHits;

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	public float _myWidth;

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	public float _myHeight;

	private float _currentTimeAwaitingBeforeGoBack;

	private bool _isGoingBack;

	public ExplodingEnemy ExplodingEnemy { get; set; }

	public bool IsExploding { get; set; }

	public bool IsMelting { get; set; }

	public bool IsGoingBack => _isGoingBack;

	public override void OnStart()
	{
		base.OnStart();
		ExplodingEnemy = (ExplodingEnemy)Entity;
		_bottomHits = new RaycastHit2D[2];
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!(ExplodingEnemy.Target == null))
		{
			DistanceToTarget = Vector2.Distance(ExplodingEnemy.transform.position, ExplodingEnemy.Target.transform.position);
			if (DistanceToTarget <= ActivationDistance && !base.BehaviourTree.isRunning && !IsExploding)
			{
				base.BehaviourTree.StartBehaviour();
			}
		}
	}

	public override void Idle()
	{
		StopMovement();
		if (Vector2.Distance(Entity.transform.position, ExplodingEnemy.StartPosition) > 1f)
		{
			_currentTimeAwaitingBeforeGoBack += Time.deltaTime;
			if (_currentTimeAwaitingBeforeGoBack >= MaxTimeAwaitingBeforeGoBack)
			{
				_isGoingBack = true;
			}
		}
	}

	public override void Wander()
	{
	}

	public void GoBack()
	{
		float num = Vector2.Distance(Entity.transform.position, ExplodingEnemy.StartPosition);
		if (num > 1f)
		{
			Chase(ExplodingEnemy.StartPosition);
			return;
		}
		_isGoingBack = false;
		StopMovement();
	}

	public void Chase(Vector3 position)
	{
		LookAtTarget(position);
		if (base.IsHurt || !Grounded() || base.TurningAround || IsMelting)
		{
			StopMovement();
			return;
		}
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		ExplodingEnemy.Input.HorizontalInput = horizontalInput;
		_currentTimeAwaitingBeforeGoBack = 0f;
		ExplodingEnemy.AnimatorInyector.Walk();
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
		if (ExplodingEnemy.Status.Dead)
		{
			ExplodingEnemy.AnimatorInyector.ChargeExplosion();
		}
		else
		{
			ExplodingEnemy.AnimatorInyector.Damage();
		}
	}

	private bool Grounded()
	{
		bool flag = false;
		if (Entity.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = (Vector2)base.transform.position - ((Vector2)base.transform.right * _myWidth * 0.75f + Vector2.up * (_myHeight * 2f));
			Debug.DrawLine(vector, vector - Vector2.up * 1f, Color.yellow);
			return Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * 1f, _bottomHits, GroundLayerMask) > 0;
		}
		Vector2 vector2 = (Vector2)base.transform.position + ((Vector2)base.transform.right * _myWidth * 0.75f - Vector2.up * (_myHeight * 2f));
		Debug.DrawLine(vector2, vector2 - Vector2.up * 1f, Color.yellow);
		return Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * 1f, _bottomHits, GroundLayerMask) > 0;
	}

	public bool TargetCanBeVisible()
	{
		return ExplodingEnemy.VisionCone.CanSeeTarget(ExplodingEnemy.Target.transform, "Penitent");
	}

	public override void StopMovement()
	{
		ExplodingEnemy.Input.HorizontalInput = 0f;
		ExplodingEnemy.AnimatorInyector.StopWalk();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > ExplodingEnemy.transform.position.x)
		{
			if (ExplodingEnemy.Status.Orientation != 0)
			{
				ExplodingEnemy.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
				ExplodingEnemy.AnimatorInyector.TurnAround();
			}
		}
		else if (ExplodingEnemy.Status.Orientation != EntityOrientation.Left)
		{
			ExplodingEnemy.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			ExplodingEnemy.AnimatorInyector.TurnAround();
		}
	}
}
