using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Legionary.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Legionary.AI;

public class LegionaryBehaviour : EnemyBehaviour
{
	[MinMaxSlider(0f, 10f, false)]
	public Vector2 MoveSpeed;

	public float MinDistanceAttack = 3f;

	public int LightMeleeAttackWeight = 1;

	public int SpinMeleeAttackWeight = 1;

	public int MaxHitsWhileHurt = 3;

	[Tooltip("Time required in attack state to go patrolling if the player is not seen.")]
	public float TimeLapseToGoPatrolling = 5f;

	protected int AttackRatio;

	private int _hitsWhileHurtCounter;

	protected Legionary Legionary { get; set; }

	public bool CanWalk => !Legionary.MotionChecker.HitsBlock && Legionary.MotionChecker.HitsFloor;

	public bool CanSeeTarget => Legionary.VisionCone.CanSeeTarget(Legionary.Target.transform, "Penitent");

	public bool CanTakeHits => _hitsWhileHurtCounter < MaxHitsWhileHurt;

	public override void OnStart()
	{
		base.OnStart();
		Legionary = (Legionary)Entity;
		Legionary.StateMachine.SwitchState<LegionaryWanderState>();
		AttackRatio = LightMeleeAttackWeight + SpinMeleeAttackWeight;
		Core.Logic.Penitent.OnDeath += OnDeathPlayer;
		Legionary.OnDeath += OnDeath;
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		Legionary.AnimatorInjector.Walk();
		Move(MoveSpeed.x);
		if (!CanWalk)
		{
			ReverseOrientation();
		}
	}

	public override void Chase(Transform targetPosition)
	{
		Legionary.AnimatorInjector.Run();
		Legionary.Behaviour.LookAtTarget(targetPosition.position);
		Move(MoveSpeed.y);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > Legionary.transform.position.x)
		{
			if (Legionary.Status.Orientation != 0)
			{
				Legionary.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (Legionary.Status.Orientation != EntityOrientation.Left)
		{
			Legionary.SetOrientation(EntityOrientation.Left);
		}
	}

	public void Move(float speed)
	{
		float horizontalInput = ((Legionary.Status.Orientation != 0) ? (-1f) : 1f);
		Legionary.Controller.MaxWalkingSpeed = speed;
		Legionary.Inputs.HorizontalInput = horizontalInput;
	}

	public override void StopMovement()
	{
		Legionary.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		Legionary.Inputs.HorizontalInput = 0f;
	}

	public void Stop()
	{
		Legionary.AnimatorInjector.Walk(walk: false);
		Legionary.AnimatorInjector.Run(run: false);
		StopMovement();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		if (CanTakeHits)
		{
			_hitsWhileHurtCounter++;
			Legionary.AnimatorInjector.Hurt();
		}
		else
		{
			Legionary.CanTakeDamage = false;
			Legionary.AnimatorInjector.LightningSummon();
		}
	}

	public override void Parry()
	{
		base.Parry();
		Legionary.AnimatorInjector.Parry();
		base.GotParry = true;
		Legionary.MotionLerper.StopLerping();
	}

	public override void Alive()
	{
		base.Alive();
		base.GotParry = false;
		Legionary.Animator.Play("Idle");
	}

	public void RandMeleeAttack()
	{
		int num = UnityEngine.Random.Range(0, AttackRatio);
		if ((num -= LightMeleeAttackWeight) < 0)
		{
			MeleeLightAttack();
		}
		else if ((num -= SpinMeleeAttackWeight) < 0)
		{
			MeleeSpinAttack();
		}
	}

	public void ResetHitsCounter()
	{
		if (_hitsWhileHurtCounter >= MaxHitsWhileHurt)
		{
			_hitsWhileHurtCounter = 0;
		}
	}

	public void LightningSummonAttack()
	{
		Vector3 position = Legionary.Target.transform.position;
		Legionary.LightningSummonAttack.SummonAreaOnPoint(position);
	}

	public void MeleeLightAttack()
	{
		Legionary.AnimatorInjector.LightAttack();
	}

	public void MeleeSpinAttack()
	{
		Legionary.AnimatorInjector.SpinAttack();
	}

	private void OnDeathPlayer()
	{
		Core.Logic.Penitent.OnDeath -= OnDeathPlayer;
		Legionary.StateMachine.enabled = false;
		Stop();
	}

	private void OnDeath()
	{
		Legionary.OnDeath -= OnDeath;
		LegionaryAudio componentInChildren = Legionary.GetComponentInChildren<LegionaryAudio>();
		if ((bool)componentInChildren)
		{
			componentInChildren.StopSlideAttack_AUDIO();
		}
		Legionary.StateMachine.enabled = false;
	}
}
