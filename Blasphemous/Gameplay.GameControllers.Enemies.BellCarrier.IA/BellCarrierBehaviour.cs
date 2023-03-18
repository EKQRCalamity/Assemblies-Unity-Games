using System.Collections;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Framework.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellCarrier.IA;

public class BellCarrierBehaviour : EnemyBehaviour
{
	private BellCarrier _bellCarrier;

	private float _currentHearingTime;

	private float _currentIdleTime;

	private float _currentTimeChasing;

	private bool _lookAtTarget;

	private bool _startRun;

	private bool _stopped;

	public float AccelerationFactor = 2.5f;

	public LayerMask BlockLayers;

	public float IdleTimeUntilSleep = 10f;

	public float MaxHearingTimeUntilDetection = 2f;

	public float MaxSpeed = 3.5f;

	public float MaxTimeChasing = 4f;

	public LayerMask RaycastTarget;

	public bool stopWhileChasing { get; set; }

	public bool IsAwaken { get; set; }

	public bool Rising { get; set; }

	public bool IsAwaiting { get; set; }

	public bool IsChasingAfterSeen { get; set; }

	public bool WatchBack { get; set; }

	public bool WallHit { get; set; }

	public bool TargetInLine { get; private set; }

	private IEnumerator ReduceSpeed()
	{
		float currentSpeed = MaxSpeed;
		while (currentSpeed > 0f)
		{
			currentSpeed -= 0.3f;
			if (isBlocked)
			{
				currentSpeed = 0f;
			}
			SetMovementSpeed(currentSpeed);
			yield return null;
		}
		SetMovementSpeed(0f);
		_bellCarrier.Inputs.HorizontalInput = 0f;
		if (Mathf.Abs(_bellCarrier.Controller.PlatformCharacterPhysics.HSpeed) > 0f)
		{
			_bellCarrier.Controller.PlatformCharacterPhysics.HSpeed = 0f;
		}
	}

	private IEnumerator Accelerate()
	{
		float t = 0f;
		float currentSpeed = 0f;
		while (t <= 1f)
		{
			t += Time.deltaTime * AccelerationFactor;
			currentSpeed = Mathf.Lerp(currentSpeed, MaxSpeed, t);
			SetMovementSpeed(currentSpeed);
			yield return null;
		}
		SetMovementSpeed(MaxSpeed);
	}

	private void BellCarrierOnEntityDie()
	{
		StopMovement();
		base.BehaviourTree.StopBehaviour();
		_bellCarrier.AnimatorInyector.Death();
		_bellCarrier.Audio.PlayDeath();
		_bellCarrier.OnDeath -= BellCarrierOnEntityDie;
	}

	public override void OnAwake()
	{
		base.OnAwake();
		_bellCarrier = GetComponent<BellCarrier>();
	}

	public bool IsTurning()
	{
		return base.TurningAround;
	}

	public override void OnStart()
	{
		base.OnStart();
		Entity = _bellCarrier;
		_bellCarrier.OnDeath += BellCarrierOnEntityDie;
		_currentTimeChasing = MaxTimeChasing;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		CheckCollision();
		if (IsAwaiting && IsAwaken)
		{
			_currentIdleTime += Time.deltaTime;
			if (_currentIdleTime >= IdleTimeUntilSleep)
			{
				_currentIdleTime = 0f;
				IsAwaken = false;
				_bellCarrier.AnimatorInyector.PlayBellHidden();
				StopMovement();
			}
		}
		if (base.IsHurt)
		{
			_bellCarrier.AnimatorInyector.Chasing();
		}
		if (IsPlayerHeard())
		{
			_currentHearingTime += Time.deltaTime;
			if (_currentHearingTime >= MaxHearingTimeUntilDetection && !WatchBack)
			{
				WatchBack = true;
			}
		}
		else if (!_bellCarrier.MotionChecker.HitsFloor)
		{
			if (!WatchBack)
			{
				WatchBack = true;
			}
		}
		else
		{
			if (WatchBack)
			{
				WatchBack = false;
			}
			_currentHearingTime = 0f;
		}
		if (base.PlayerHeard || base.PlayerSeen)
		{
			_currentIdleTime = 0f;
			ResetTimeChasing();
		}
	}

	private void CheckCollision()
	{
		//Discarded unreachable code: IL_0091
		int num = ((_bellCarrier.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		RaycastHit2D[] array = Physics2D.RaycastAll(base.transform.position, Vector2.right * num, 15f, RaycastTarget);
		TargetInLine = false;
		isBlocked = false;
		RaycastHit2D[] array2 = array;
		int num2 = 0;
		if (num2 < array2.Length)
		{
			RaycastHit2D raycastHit2D = array2[num2];
			TargetInLine = raycastHit2D.collider.CompareTag("Penitent");
		}
		RaycastHit2D[] array3 = array;
		for (int i = 0; i < array3.Length; i++)
		{
			RaycastHit2D raycastHit2D2 = array3[i];
			if ((BlockLayerMask.value & (1 << raycastHit2D2.collider.gameObject.layer)) > 0)
			{
				isBlocked = raycastHit2D2.distance < 1.5f;
				if (isBlocked)
				{
					break;
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!(_bellCarrier == null))
		{
			Gizmos.color = Color.red;
			int num = ((_bellCarrier.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
			Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.right * num * 15f);
		}
	}

	public override void Idle()
	{
		if (_stopped)
		{
			_stopped = !_stopped;
		}
		StopMovement();
		if (!VisualSensor.SensorCollider2D.enabled)
		{
			VisualSensor.SensorCollider2D.enabled = true;
		}
		if (!IsAwaiting)
		{
			IsAwaiting = true;
		}
		_bellCarrier.AnimatorInyector.Stop();
	}

	public void CheckBlocked()
	{
	}

	public override void Chase(Transform targetPosition)
	{
		if (!(_bellCarrier == null))
		{
			if (!_bellCarrier.IsChasing)
			{
				_bellCarrier.IsChasing = true;
			}
			if (_stopped)
			{
				_stopped = !_stopped;
			}
			if (base.TurningAround)
			{
				base.TurningAround = !base.TurningAround;
			}
			_bellCarrier.AnimatorInyector.Chasing();
			if (IsAwaiting)
			{
				IsAwaiting = !IsAwaiting;
			}
			if (_bellCarrier.BellCarrierBehaviour.Rising)
			{
				_bellCarrier.BellCarrierBehaviour.Rising = false;
			}
		}
	}

	public void StopChase()
	{
		if (_bellCarrier.IsChasing)
		{
			_bellCarrier.IsChasing = false;
			_currentIdleTime = 0f;
		}
		StopMovement();
		_bellCarrier.AnimatorInyector.Stop();
	}

	public void StopWhileChasing()
	{
		if (!_bellCarrier.Animator.GetCurrentAnimatorStateInfo(0).IsName("TurnAround") && !stopWhileChasing)
		{
			StopMovement();
			stopWhileChasing = true;
			_bellCarrier.AnimatorInyector.PlayStopAnimation();
		}
	}

	public bool ChasingAfterSeen()
	{
		_currentTimeChasing -= Time.deltaTime;
		if (_currentTimeChasing <= 0f)
		{
			IsChasingAfterSeen = false;
		}
		else
		{
			IsChasingAfterSeen = true;
		}
		return IsChasingAfterSeen;
	}

	public void ResetTimeChasing()
	{
		_currentTimeChasing = MaxTimeChasing;
		IsChasingAfterSeen = false;
	}

	public void Block()
	{
		if (!_bellCarrier.AnimatorInyector.IsInWallCrashAnim && _startRun)
		{
			_bellCarrier.AnimatorInyector.PlayWallCrushAnimation();
			_stopped = true;
			ResetTimeChasing();
			StopMovement();
		}
	}

	public void LookAtTarget(Transform targetPosition)
	{
		if (_bellCarrier == null || _bellCarrier.Animator.GetCurrentAnimatorStateInfo(0).IsName("StopRun"))
		{
			return;
		}
		if (!isBlocked)
		{
			_bellCarrier.AnimatorInyector.Chasing();
			if (!_bellCarrier.IsChasing)
			{
				_bellCarrier.IsChasing = true;
			}
		}
		if (base.TurningAround)
		{
			return;
		}
		EntityOrientation orientation = Entity.Status.Orientation;
		if (orientation == EntityOrientation.Left)
		{
			if (Entity.transform.position.x <= targetPosition.position.x)
			{
				TurnAround();
			}
		}
		else if (Entity.transform.position.x > targetPosition.position.x)
		{
			TurnAround();
		}
	}

	private void TurnAround()
	{
		StopMovement();
		if (!base.TurningAround)
		{
			base.TurningAround = true;
		}
		_bellCarrier.AnimatorInyector.TurnAround();
	}

	public void Awaken()
	{
		if (!IsAwaken)
		{
			IsAwaken = true;
			Rising = true;
			_bellCarrier.AnimatorInyector.Awaken();
			VisualSensor.SensorCollider2D.enabled = false;
		}
	}

	public override void Damage()
	{
		if (!base.IsHurt)
		{
			base.IsHurt = true;
		}
	}

	public void EnableSensorColliders()
	{
		if (!VisualSensor.SensorCollider2D.enabled)
		{
			VisualSensor.SensorCollider2D.enabled = true;
		}
		if (!HearingSensor.SensorCollider2D.enabled)
		{
			VisualSensor.SensorCollider2D.enabled = true;
		}
	}

	public override void StopMovement()
	{
		if (!(_bellCarrier == null) && _startRun)
		{
			_startRun = false;
			StartCoroutine(ReduceSpeed());
		}
	}

	public void StartMovement()
	{
		if (!_startRun)
		{
			_startRun = true;
			_currentIdleTime = 0f;
			StartCoroutine(Accelerate());
			if (base.IsHurt)
			{
				base.IsHurt = !base.IsHurt;
			}
			float horizontalInput = ((_bellCarrier.Status.Orientation != 0) ? (-1f) : 1f);
			_bellCarrier.Inputs.HorizontalInput = horizontalInput;
		}
	}

	private void SetMovementSpeed(float newSpeed)
	{
		if (!(_bellCarrier == null))
		{
			_bellCarrier.Controller.MaxWalkingSpeed = newSpeed;
		}
	}

	public override void Attack()
	{
	}

	public override void Wander()
	{
	}
}
