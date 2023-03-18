using System;
using System.Collections;
using BezierSplines;
using DG.Tweening;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Maikel.StatelessFSM;
using Maikel.SteeringBehaviors;
using Plugins.Maikel.StateMachine;
using UnityEngine;

public class AmanecidaAxeBehaviour : EnemyBehaviour
{
	public class SplineFollowingProjectile_FollowSpline_EnemyAction : EnemyAction
	{
		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private SplineFollowingProjectile follower;

		private Vector2 origin;

		private BezierSpline spline;

		private float duration;

		private AnimationCurve curve;

		public EnemyAction StartAction(EnemyBehaviour e, SplineFollowingProjectile follower, SplineThrowData throwData, BezierSpline spline)
		{
			this.follower = follower;
			this.spline = spline;
			duration = throwData.duration;
			curve = throwData.curve;
			origin = spline.GetPoint(0f);
			return StartAction(e);
		}

		public EnemyAction StartAction(EnemyBehaviour e, SplineFollowingProjectile follower, Vector2 origin, SplineThrowData throwData, BezierSpline spline)
		{
			return StartAction(e, follower, throwData, spline);
		}

		public EnemyAction StartAction(EnemyBehaviour e, SplineFollowingProjectile follower, Vector2 origin, Vector2 end, int endPointIndex, SplineThrowData throwData, BezierSpline spline)
		{
			spline.transform.position = origin;
			Vector2 vector = spline.GetControlPoint(endPointIndex) - spline.GetControlPoint(endPointIndex - 1);
			Vector2 vector2 = spline.GetControlPoint(endPointIndex) - spline.GetControlPoint(endPointIndex + 1);
			Vector2 vector3 = spline.transform.InverseTransformPoint(end);
			spline.SetControlPoint(endPointIndex, vector3);
			spline.SetControlPoint(endPointIndex - 1, vector3 - vector);
			spline.SetControlPoint(endPointIndex + 1, vector3 - vector2);
			return StartAction(e, follower, origin, throwData, spline);
		}

		protected override void DoOnStart()
		{
			base.DoOnStart();
		}

		protected override void DoOnStop()
		{
			base.DoOnStop();
			follower.Stop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			ACT_MOVE.StartAction(owner, origin, 0.1f, Ease.InOutCubic);
			yield return ACT_MOVE.waitForCompletion;
			follower.Init(origin, spline, duration, curve);
			yield return new WaitUntil(() => !follower.IsFollowing());
			FinishAction();
		}
	}

	public SplineFollowingProjectile splineFollower;

	public SplineFollowingProjectile_FollowSpline_EnemyAction axeSplineFollowAction = new SplineFollowingProjectile_FollowSpline_EnemyAction();

	public StateMachine<AmanecidaAxeBehaviour> fsm;

	public State<AmanecidaAxeBehaviour> stControlled;

	public State<AmanecidaAxeBehaviour> stSeekTarget;

	public Transform target;

	public bool active = true;

	public AutonomousAgent agent;

	private Seek seek;

	private Animator animator;

	private BoxCollider2D boxCollider;

	private SpriteRenderer spriteRenderer;

	private TrailRenderer trailRenderer;

	private ParticleSystem particleSystem;

	private ResetTrailRendererOnEnable trailCleaner;

	private PathFollowingProjectile pathFollowingProjectile;

	public override void OnStart()
	{
		base.OnStart();
		agent = GetComponent<AutonomousAgent>();
		pathFollowingProjectile = GetComponent<PathFollowingProjectile>();
		seek = agent.GetComponent<Seek>();
		stControlled = new AmanecidaAxeSt_Controlled();
		stSeekTarget = new AmanecidaAxeSt_SeekPlayer();
		fsm = new StateMachine<AmanecidaAxeBehaviour>(this, stControlled);
	}

	public void InitDamageData(Hit h)
	{
		GetComponent<PathFollowingProjectile>().SetHit(h);
	}

	public void SetSeek(bool free)
	{
		if (free)
		{
			fsm.ChangeState(stSeekTarget);
		}
		else
		{
			fsm.ChangeState(stControlled);
		}
	}

	public override void OnUpdate()
	{
		if (active)
		{
			base.OnUpdate();
			fsm.DoUpdate();
		}
	}

	public void SeekTarget()
	{
		seek.target = target.position;
	}

	public void SeekTarget(Vector2 targetPosition)
	{
		seek.target = targetPosition;
	}

	public void ActivateAgent(bool active)
	{
		agent.enabled = active;
	}

	public void Show()
	{
		if (trailCleaner == null)
		{
			trailCleaner = GetComponentInChildren<ResetTrailRendererOnEnable>();
		}
		if (trailCleaner != null)
		{
			trailCleaner.Clean();
		}
		if (trailRenderer == null)
		{
			trailRenderer = GetComponentInChildren<TrailRenderer>();
		}
		if (trailRenderer != null)
		{
			trailRenderer.enabled = true;
		}
		if (particleSystem == null)
		{
			particleSystem = GetComponentInChildren<ParticleSystem>();
		}
		if (particleSystem != null)
		{
			particleSystem.Play();
		}
		animator.SetBool("HIDE", value: false);
	}

	public void Hide()
	{
		if (trailRenderer == null)
		{
			trailRenderer = GetComponentInChildren<TrailRenderer>();
		}
		if (trailRenderer != null)
		{
			trailRenderer.enabled = false;
		}
		if (particleSystem == null)
		{
			particleSystem = GetComponentInChildren<ParticleSystem>();
		}
		if (particleSystem != null)
		{
			particleSystem.Stop();
		}
		animator.SetBool("HIDE", value: true);
	}

	public void SetRepositionMode(bool isInReposition)
	{
		if (boxCollider == null)
		{
			boxCollider = GetComponentInChildren<BoxCollider2D>();
		}
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}
		if (boxCollider != null)
		{
			boxCollider.enabled = !isInReposition;
		}
		pathFollowingProjectile.leaveSparks = !isInReposition;
		if (isInReposition)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public void SetVisible(bool v)
	{
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}
		animator.gameObject.SetActive(v);
		active = v;
		GetComponent<PathFollowingProjectile>().leaveSparks = v;
	}
}
