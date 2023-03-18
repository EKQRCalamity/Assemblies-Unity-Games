using System;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Pietat;

public class Pietat : Enemy
{
	private AudioSource pietatAudioSource;

	public Animator PietatAnimator { get; private set; }

	private void Awake()
	{
		PietatAnimator = GetComponent<Animator>();
		pietatAudioSource = GetComponent<AudioSource>();
	}

	public void PlayPietatAwakeningAudio()
	{
		if (pietatAudioSource != null)
		{
			pietatAudioSource.Play();
		}
	}

	public void SetAnimatorSpeed(float animatorSpeed)
	{
		if (PietatAnimator != null)
		{
			PietatAnimator.speed = animatorSpeed;
		}
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable)
	{
		throw new NotImplementedException();
	}
}
