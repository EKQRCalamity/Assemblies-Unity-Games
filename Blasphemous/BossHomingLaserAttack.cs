using System;
using System.Collections;
using Framework.FrameworkCore;
using UnityEngine;

public class BossHomingLaserAttack : MonoBehaviour
{
	public TileableBeamLauncher beamLauncher;

	private Transform _target;

	public float accuracy = 0.01f;

	public float lowAngleLimit = 240f;

	public float highAngleLimit = 300f;

	public Vector2 targetOffset;

	private EntityOrientation orientation;

	private bool forceOrientation;

	public event Action<BossHomingLaserAttack> OnAttackFinished;

	private void Awake()
	{
		beamLauncher.displayEndAnimation = false;
	}

	public void DelayedTargetedBeam(Transform target, float warningDelay, float duration, EntityOrientation orientation = EntityOrientation.Right, bool forceOrientation = false)
	{
		_target = target;
		beamLauncher.ActivateDelayedBeam(warningDelay, warningAnimation: true);
		this.orientation = orientation;
		this.forceOrientation = forceOrientation;
		SetRotationToTarget();
		StartCoroutine(DelayedDeactivation(duration));
	}

	private void SetRotationToTarget()
	{
		Vector2 vector = _target.transform.position + (Vector3)targetOffset - base.transform.position;
		if (forceOrientation)
		{
			vector.x = ((orientation != 0) ? (0f - Mathf.Abs(vector.x)) : Mathf.Abs(vector.x));
		}
		float targetAngle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		targetAngle = ClampTargetAngleAtStart(targetAngle);
		base.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
	}

	public void Clear()
	{
		StopBeam();
		beamLauncher.ClearAll();
		StopAllCoroutines();
	}

	private IEnumerator DelayedDeactivation(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		StopBeam();
	}

	public void StopBeam()
	{
		beamLauncher.ActivateBeamAnimation(active: false);
		if (this.OnAttackFinished != null)
		{
			this.OnAttackFinished(this);
		}
	}

	private void Update()
	{
		if ((bool)_target)
		{
			UpdateAimingAngle();
		}
	}

	private void UpdateAimingAngle()
	{
		Vector2 vector = _target.transform.position + (Vector3)targetOffset - base.transform.position;
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		Quaternion b = Quaternion.Euler(0f, 0f, z);
		z = ClampTargetAngle(Quaternion.Slerp(base.transform.rotation, b, accuracy).eulerAngles.z);
		base.transform.rotation = Quaternion.Euler(0f, 0f, z);
	}

	private void DrawDebugLine(Color c)
	{
		Vector2 vector = _target.transform.position + (Vector3)targetOffset - base.transform.position;
		Debug.DrawLine(_target.transform.position, _target.transform.position + (Vector3)vector, c, 2f);
	}

	private float ClampTargetAngleAtStart(float targetAngle)
	{
		if (targetAngle < 0f)
		{
			targetAngle += 360f;
		}
		if (targetAngle > lowAngleLimit && targetAngle < highAngleLimit)
		{
			targetAngle = CloseToBoundary(lowAngleLimit, highAngleLimit, targetAngle);
		}
		return targetAngle;
	}

	private float CloseToBoundary(float min, float max, float val)
	{
		float num = Mathf.Abs(val - min);
		float num2 = Mathf.Abs(val - max);
		return (!(num < num2)) ? max : min;
	}

	private float ClampTargetAngle(float targetAngle)
	{
		if (targetAngle < 0f)
		{
			targetAngle += 360f;
		}
		if (targetAngle > lowAngleLimit && targetAngle < highAngleLimit)
		{
			float z = base.transform.rotation.eulerAngles.z;
			if (z <= lowAngleLimit && z > 90f)
			{
				targetAngle = lowAngleLimit;
				DrawDebugLine(Color.yellow);
			}
			else if ((z <= 90f && z >= 0f) || (z < 0f && z >= highAngleLimit))
			{
				targetAngle = highAngleLimit;
				DrawDebugLine(Color.red);
			}
			else
			{
				DrawDebugLine(Color.magenta);
				targetAngle = ((!(z < 270f)) ? highAngleLimit : lowAngleLimit);
			}
		}
		return targetAngle;
	}
}
