using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.WaxCrawler.Animator;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.AI;

public class WaxCrawlerBehaviour : EnemyBehaviour
{
	private RaycastHit2D[] _bottomHits;

	private RaycastHit2D[] _forwardsHits;

	private float _currentTimeChasing;

	private EnemyDamageArea _damageArea;

	public WaxCrawler _waxCrawler;

	public WaxCrawlerAnimatorInyector AnimatorInyector;

	public float CurrentGroundDetection = 1f;

	public float RangeBlockDectection = 1f;

	public float Speed = 3f;

	public float TimeChasing = 2f;

	public Transform Target { get; set; }

	public Vector3 Origin { get; set; }

	public bool Awake { get; set; }

	public bool Above { get; set; }

	public bool Below { get; set; }

	public bool Melting { get; set; }

	public bool CanMove { get; set; }

	public bool TargetLost { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		_waxCrawler = (WaxCrawler)Entity;
		AnimatorInyector = _waxCrawler.AnimatorInyector;
		AnimatorInyector.AnimatorSpeed(0f);
		_waxCrawler.SpriteRenderer.enabled = false;
		_bottomHits = new RaycastHit2D[2];
		_forwardsHits = new RaycastHit2D[2];
		_damageArea = _waxCrawler.DamageArea;
		_waxCrawler.OnDeath += WaxCrawlerOnEntityDie;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Core.Logic.CurrentState == LogicStates.PlayerDead)
		{
			_waxCrawler.Attack.EnableAttackArea = false;
		}
		if (Entity.Status.Orientation == EntityOrientation.Left)
		{
			Vector2 vector = new Vector2(_damageArea.DamageAreaCollider.bounds.min.x, _damageArea.DamageAreaCollider.bounds.center.y);
			Vector2 vector2 = vector;
			Debug.DrawLine(vector2, vector2 - Vector2.up * CurrentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * CurrentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector2, vector2 - (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector2, vector2 - (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		else
		{
			Vector2 vector3 = new Vector2(_damageArea.DamageAreaCollider.bounds.max.x, _damageArea.DamageAreaCollider.bounds.center.y);
			Vector2 vector2 = vector3;
			Debug.DrawLine(vector2, vector2 - Vector2.up * CurrentGroundDetection, Color.yellow);
			base.SensorHitsFloor = Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * CurrentGroundDetection, _bottomHits, BlockLayerMask) > 0;
			Debug.DrawLine(vector2, vector2 + (Vector2)base.transform.right * RangeBlockDectection, Color.yellow);
			isBlocked = Physics2D.LinecastNonAlloc(vector2, vector2 + (Vector2)base.transform.right * RangeBlockDectection, _forwardsHits, BlockLayerMask) > 0;
		}
		CanMove = base.SensorHitsFloor && !isBlocked;
		if (base.PlayerSeen)
		{
			TargetLost = false;
			_currentTimeChasing = 0f;
		}
		else
		{
			_currentTimeChasing += Time.deltaTime;
			TargetLost = _currentTimeChasing >= TimeChasing;
		}
	}

	public void Rise()
	{
		if (!Awake)
		{
			Awake = true;
		}
		if (!Melting)
		{
			Melting = true;
		}
		AnimatorInyector.EnableSpriteRenderer = true;
		AnimatorInyector.AnimatorSpeed(1f);
		AnimatorInyector.Appear();
	}

	public void Asleep()
	{
		if (Awake)
		{
			Awake = !Awake;
		}
		_waxCrawler.AnimatorInyector.EnableSpriteRenderer = false;
		_waxCrawler.Behaviour.Melting = false;
		_waxCrawler.Behaviour.MoveToOrigin();
	}

	public void Fall()
	{
		if (Awake)
		{
			Awake = !Awake;
		}
	}

	public override void Idle()
	{
		AnimatorInyector.AnimatorSpeed(0f);
	}

	public override void Wander()
	{
	}

	public override void Chase(Transform targetPosition)
	{
		if (!(targetPosition == null) && !base.IsHurt)
		{
			EntityOrientation orientation = _waxCrawler.Status.Orientation;
			Vector2 vector = ((orientation != EntityOrientation.Left) ? Vector2.right : (-Vector2.right));
			base.transform.Translate(vector * Speed * Time.deltaTime, Space.World);
		}
	}

	public void MoveToOrigin()
	{
		base.transform.position = Origin;
	}

	public void Hide()
	{
		AnimatorInyector.Hide();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	private void WaxCrawlerOnEntityDie()
	{
		if (base.BehaviourTree.isRunning)
		{
			base.BehaviourTree.StopBehaviour();
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		base.LookAtTarget(targetPos);
	}
}
