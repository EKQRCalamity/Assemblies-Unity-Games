using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.Animator;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.Attack;
using Gameplay.GameControllers.Enemies.ExplodingEnemy.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.ReekLeader;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ExplodingEnemy;

public class ExplodingEnemy : Enemy, IDamageable
{
	public float InvisibleTimeBeforeRecycle = 2f;

	private float _currentInvisibleTime;

	public NPCInputs Input { get; private set; }

	public ExplodingEnemyAnimatorInyector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public ExplodingEnemyAudio Audio { get; private set; }

	public Gameplay.GameControllers.Enemies.ReekLeader.ReekLeader ReekLeader { get; set; }

	public VisionCone VisionCone { get; private set; }

	public Vector2 StartPosition { get; private set; }

	public bool IsSummoned { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		AnimatorInyector = GetComponentInChildren<ExplodingEnemyAnimatorInyector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		base.EnemyBehaviour = GetComponentInChildren<EnemyBehaviour>();
		enemyAttack = GetComponentInChildren<ExplodingEnemyAttack>();
		Audio = GetComponentInChildren<ExplodingEnemyAudio>();
		VisionCone = GetComponentInChildren<VisionCone>();
		base.Target = Core.Logic.Penitent.gameObject;
		if (ReekLeader != null)
		{
			ReekLeader.OnDeath += OnDeathReekLeader;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Target == null)
		{
			base.Target = Core.Logic.Penitent.gameObject;
		}
		Status.IsGrounded = base.Controller.IsGrounded;
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
			StartPosition = new Vector2(base.transform.position.x, base.transform.position.y);
			base.Controller.PlatformCharacterPhysics.GravityScale = 1f;
		}
		if (!base.SpriteRenderer.isVisible)
		{
			_currentInvisibleTime += Time.deltaTime;
			if (_currentInvisibleTime >= InvisibleTimeBeforeRecycle && !base.IsFalling)
			{
				base.IsFalling = true;
				base.EnemyBehaviour.StopBehaviour();
				AnimatorInyector.Vanish();
			}
		}
		else
		{
			_currentInvisibleTime = 0f;
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
		base.Controller.PlatformCharacterPhysics.GravityScale = 0f;
	}

	public void Damage(Hit hit)
	{
		DamageArea.TakeDamage(hit);
		base.EnemyBehaviour.Damage();
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void ReuseObject()
	{
		IsSummoned = true;
	}

	public void Destroy()
	{
		if (IsSummoned)
		{
			int instanceID = base.gameObject.GetInstanceID();
			Status.CastShadow = false;
			ReekLeader.Behaviour.ReekSpawner.ResetSpawnPoint(instanceID);
		}
	}

	private void OnDeathReekLeader()
	{
		base.EnemyBehaviour.StopBehaviour();
		AnimatorInyector.Vanish();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ReekLeader != null)
		{
			ReekLeader.OnDeath -= OnDeathReekLeader;
		}
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
		throw new NotImplementedException();
	}
}
