using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.WaxCrawler.AI;
using Gameplay.GameControllers.Enemies.WaxCrawler.Animator;
using Gameplay.GameControllers.Enemies.WaxCrawler.Attack;
using Gameplay.GameControllers.Enemies.WaxCrawler.Audio;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WaxCrawler;

public class WaxCrawler : Enemy, IDamageable
{
	public WaxCrawlerAnimatorInyector AnimatorInyector { get; private set; }

	public WaxCrawlerBehaviour Behaviour { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public CreepCrawlerAudio Audio { get; private set; }

	public WaxCrawlerAttack Attack { get; private set; }

	public bool CanBeAttacked { get; set; }

	public void Damage(Hit hit)
	{
		if (CanBeAttacked)
		{
			DamageArea.TakeDamage(hit);
			if (Status.Dead)
			{
				AnimatorInyector.Dead();
				Audio.Death();
			}
			else
			{
				AnimatorInyector.Hurt();
				Audio.Hurt();
			}
			SleepTimeByHit(hit);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AnimatorInyector = GetComponentInChildren<WaxCrawlerAnimatorInyector>();
		Behaviour = GetComponentInChildren<WaxCrawlerBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Audio = GetComponent<CreepCrawlerAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Attack = GetComponentInChildren<WaxCrawlerAttack>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsVisibleOnCamera = IsVisible();
		SetPositionAtStart();
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		if (!base.Landing)
		{
			float distance = Physics2D.Raycast(base.transform.position, -Vector2.up, 5f, Behaviour.BlockLayerMask).distance;
			Vector3 position = base.transform.position;
			position.y -= distance;
			base.transform.position = position;
			Behaviour.Origin = position;
			base.Landing = true;
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

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}

	public bool IsVisible()
	{
		return Entity.IsVisibleFrom(base.SpriteRenderer, UnityEngine.Camera.main);
	}
}
