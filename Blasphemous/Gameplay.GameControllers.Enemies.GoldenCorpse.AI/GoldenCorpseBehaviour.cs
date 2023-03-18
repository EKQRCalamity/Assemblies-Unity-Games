using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.AI;

public class GoldenCorpseBehaviour : EnemyBehaviour
{
	public bool isAwake;

	public bool isNapping;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float ActivationDistance;

	public float DistanceToTarget;

	public float MaxVisibleHeight = 2f;

	[FoldoutGroup("Activation Settings", true, 0)]
	public bool startAwaken = true;

	[FoldoutGroup("Activation Settings", true, 0)]
	public float MaxTimeAwaitingBeforeGoBack;

	[FoldoutGroup("Motion Settings", true, 0)]
	public LayerMask GroundLayerMask;

	[SerializeField]
	[FoldoutGroup("Motion Settings", true, 0)]
	private RaycastHit2D[] _bottomHits;

	public float _myWidth;

	public float _myHeight;

	[SerializeField]
	[FoldoutGroup("Sleep Settings", true, 0)]
	private float minSleepTime = 8f;

	[SerializeField]
	[FoldoutGroup("Sleep Settings", true, 0)]
	private float maxSleepTime = 16f;

	public float sleepTime;

	public float origAnimationSpeed = 1f;

	private int totalAnimationVariants = 2;

	private bool _isSpawning;

	public GoldenCorpse GoldenCorpse { get; set; }

	public override void OnStart()
	{
		base.OnStart();
		GoldenCorpse = (GoldenCorpse)Entity;
		_bottomHits = new RaycastHit2D[2];
		int value = Random.Range(0, totalAnimationVariants);
		GoldenCorpse.Animator.SetInteger("ID", value);
		GoldenCorpse.Animator.Play("sleep" + value);
		if (Random.Range(0f, 1f) < 0.5f)
		{
			LookAtTarget(base.transform.position - Vector3.right);
		}
		if (startAwaken)
		{
			Invoke("Awaken", 1f);
		}
	}

	public bool CanWalk()
	{
		return !_isSpawning;
	}

	public bool IsAwaken()
	{
		return isAwake;
	}

	public void Awaken()
	{
		UnFreezeAnimation();
		isAwake = true;
		GoldenCorpse.AnimatorInyector.PlayAwaken();
	}

	public void SleepForever()
	{
		StopMovement();
		UnFreezeAnimation();
		isAwake = false;
		GoldenCorpse.DamageArea.DamageAreaCollider.enabled = false;
		GoldenCorpse.DamageByContact = false;
		GoldenCorpse.AnimatorInyector.PlaySleep();
		base.BehaviourTree.StopBehaviour();
	}

	public void ReAwaken()
	{
		UnFreezeAnimation();
		GoldenCorpse.AnimatorInyector.PlayAwaken();
	}

	public void OnAwakeAnimationFinished()
	{
		if (isAwake)
		{
			GoldenCorpse.DamageArea.DamageAreaCollider.enabled = true;
			GoldenCorpse.DamageByContact = true;
			base.BehaviourTree.StartBehaviour();
		}
	}

	private void Sleep()
	{
		StopMovement();
		GoldenCorpse.DamageArea.DamageAreaCollider.enabled = false;
		GoldenCorpse.DamageByContact = false;
		UnFreezeAnimation();
		isNapping = true;
		GoldenCorpse.AnimatorInyector.PlaySleep();
		sleepTime = Random.Range(minSleepTime, maxSleepTime);
		base.BehaviourTree.StopBehaviour();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		GoldenCorpse.Status.CastShadow = !isNapping && isAwake;
		if (isAwake && isNapping)
		{
			sleepTime -= Time.deltaTime;
			if (sleepTime < 0f)
			{
				isNapping = false;
				ReAwaken();
			}
		}
	}

	public override void Idle()
	{
		FreezeAnimation();
		StopMovement();
	}

	public override void Wander()
	{
	}

	private void FreezeAnimation()
	{
		if (GoldenCorpse.Animator.speed > 0.1f)
		{
			origAnimationSpeed = GoldenCorpse.Animator.speed;
			GoldenCorpse.Animator.speed = 0.01f;
		}
	}

	private void UnFreezeAnimation()
	{
		if (GoldenCorpse.Animator.speed < 0.1f)
		{
			GoldenCorpse.Animator.speed = origAnimationSpeed;
		}
	}

	public void Chase(Vector3 position)
	{
		UnFreezeAnimation();
		LookAtTarget(position);
		if (!GoldenCorpse.MotionChecker.HitsFloor || GoldenCorpse.MotionChecker.HitsBlock || GoldenCorpse.Status.Dead)
		{
			StopMovement();
			return;
		}
		float horizontalInput = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		GoldenCorpse.Input.HorizontalInput = horizontalInput;
		GoldenCorpse.AnimatorInyector.Walk();
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Attack()
	{
	}

	public override void Damage()
	{
		GoldenCorpse.Audio.PlayHit();
		Sleep();
	}

	public void Death()
	{
		StopMovement();
		GoldenCorpse.AnimatorInyector.Death();
	}

	public bool TargetCanBeVisible()
	{
		GetTarget();
		if (GoldenCorpse.Target == null)
		{
			return false;
		}
		float num = GoldenCorpse.Target.transform.position.y - GoldenCorpse.transform.position.y;
		float num2 = Mathf.Abs(GoldenCorpse.Target.transform.position.x - GoldenCorpse.transform.position.x);
		num = ((!(num > 0f)) ? (0f - num) : num);
		return num <= MaxVisibleHeight && num2 < ActivationDistance;
	}

	public override void StopMovement()
	{
		GoldenCorpse.Input.HorizontalInput = 0f;
		GoldenCorpse.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		GoldenCorpse.AnimatorInyector.StopWalk();
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > GoldenCorpse.transform.position.x)
		{
			if (GoldenCorpse.Status.Orientation != 0)
			{
				GoldenCorpse.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (GoldenCorpse.Status.Orientation != EntityOrientation.Left)
		{
			GoldenCorpse.SetOrientation(EntityOrientation.Left);
		}
	}
}
