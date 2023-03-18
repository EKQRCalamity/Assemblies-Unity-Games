using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.DrownedCorpse.AI;
using Gameplay.GameControllers.Enemies.DrownedCorpse.Animator;
using Gameplay.GameControllers.Enemies.DrownedCorpse.Audio;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse;

public class DrownedCorpse : Enemy, IDamageable
{
	[SerializeField]
	private GameObject[] drownedCorpseHelmets;

	public NPCInputs Input { get; private set; }

	public DrownedCorpseAnimatorInjector AnimatorInyector { get; private set; }

	public EnemyDamageArea DamageArea { get; private set; }

	public DrownedCorpseBehaviour Behaviour { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public Vector2 StartPosition { get; private set; }

	public DrownedCorpseAudio Audio { get; private set; }

	public bool IsSummoned { get; set; }

	public ColorFlash ColorFlash { get; private set; }

	public DrownedCorpseHelmet Helmet { get; private set; }

	public void Damage(Hit hit)
	{
		if (!hit.HitSoundId.IsNullOrWhitespace())
		{
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
		}
		if (Status.Dead)
		{
			Behaviour.Death();
		}
		else
		{
			Behaviour.Damage();
		}
		ColorFlash.TriggerColorFlash();
		SleepTimeByHit(hit);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Input = GetComponent<NPCInputs>();
		base.Controller = GetComponent<PlatformCharacterController>();
		AnimatorInyector = GetComponentInChildren<DrownedCorpseAnimatorInjector>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		Behaviour = GetComponentInChildren<DrownedCorpseBehaviour>();
		enemyAttack = GetComponentInChildren<EnemyAttack>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		Audio = GetComponentInChildren<DrownedCorpseAudio>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		Status.CastShadow = true;
		SetPositionAtStart();
		SetHelmet();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		Status.IsGrounded = base.Controller.IsGrounded;
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float distance = Physics2D.Raycast(base.transform.position, -Vector2.up, 5f, Behaviour.BlockLayerMask).distance;
		Vector3 position = base.transform.position;
		position.y -= distance;
		base.transform.position = position;
	}

	public void SetHelmet()
	{
		if (drownedCorpseHelmets.Length > 0 && !Helmet)
		{
			int num = UnityEngine.Random.Range(0, drownedCorpseHelmets.Length);
			GameObject original = drownedCorpseHelmets[num];
			GameObject gameObject = UnityEngine.Object.Instantiate(original, base.transform.position, Quaternion.identity);
			Helmet = gameObject.GetComponent<DrownedCorpseHelmet>();
			Helmet.Initialize(this);
		}
	}

	public void ReuseObject()
	{
		IsSummoned = true;
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
