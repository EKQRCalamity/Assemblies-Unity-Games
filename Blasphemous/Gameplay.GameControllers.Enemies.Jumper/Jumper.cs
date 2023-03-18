using System;
using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Jumper.AI;
using Gameplay.GameControllers.Enemies.Jumper.Animator;
using Gameplay.GameControllers.Enemies.Jumper.Attack;
using Gameplay.GameControllers.Enemies.Jumper.Audio;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Jumper;

public class Jumper : Enemy, IDamageable
{
	public NPCInputs Inputs { get; private set; }

	public ColorFlash Flash { get; private set; }

	public SmartPlatformCollider Collider { get; private set; }

	public EnemyDamageArea DamageArea { get; set; }

	public JumperAnimator AnimatorInjector { get; private set; }

	public JumperAttack Attack { get; private set; }

	public JumperAudio Audio { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Inputs = GetComponentInChildren<NPCInputs>();
		Flash = GetComponentInChildren<ColorFlash>();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		Collider = GetComponentInChildren<SmartPlatformCollider>();
		base.EnemyBehaviour = GetComponentInChildren<JumperBehaviour>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		AnimatorInjector = GetComponentInChildren<JumperAnimator>();
		Attack = GetComponentInChildren<JumperAttack>();
		Audio = GetComponentInChildren<JumperAudio>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Target = Core.Logic.Penitent.gameObject;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (base.Landing && base.Controller.PlatformCharacterPhysics.GravityScale <= 0f)
		{
			base.Controller.PlatformCharacterPhysics.GravityScale = 3f;
		}
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
		}
		if (base.IsImpaled && !Status.Dead)
		{
			AnimatorInjector.Death();
		}
	}

	public void Damage(Hit hit)
	{
		if (!Status.Dead)
		{
			SleepTimeByHit(hit);
			DamageArea.TakeDamage(hit);
			Flash.TriggerColorFlash();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void GetSparks(Hit hit)
	{
		if (!(DamageArea == null))
		{
			PenitentSword penitentSword = (PenitentSword)Core.Logic.Penitent.PenitentAttack.CurrentPenitentWeapon;
			if (!(penitentSword == null))
			{
				Bounds bounds = DamageArea.DamageAreaCollider.bounds;
				Vector2 vector = default(Vector2);
				vector.x = ((Status.Orientation != EntityOrientation.Left) ? bounds.max.x : bounds.min.x);
				vector.y = bounds.max.y;
				Vector2 position = vector;
				penitentSword.GetSwordSparks(position);
			}
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
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
