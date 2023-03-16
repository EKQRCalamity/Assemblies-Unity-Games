using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelDebris : BasicProjectile
{
	private static readonly float OffsetAimXCutoff = 0f;

	private static readonly Vector3 OffsetAimAmount = new Vector3(-50f, 0f);

	private static readonly Vector3 SquashAmount = new Vector3(1.2f, 0.5f, 1f);

	private static readonly float SquashDuration = 0.4f;

	private Vector3 curveSpeed;

	private float gravity;

	private bool isCurved => gravity != 0f;

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(new Vector3(OffsetAimXCutoff, 1000f), new Vector3(OffsetAimXCutoff, -1000f));
		Vector3 position = base.transform.position;
		Vector3 vector = curveSpeed;
		for (int i = 0; i < 50; i++)
		{
			if (isCurved)
			{
				vector += new Vector3(gravity * CupheadTime.FixedDelta, 0f);
				position += vector * CupheadTime.FixedDelta;
			}
			else
			{
				position += base.transform.right * Speed * CupheadTime.FixedDelta;
			}
			Gizmos.DrawWireSphere(position, 10f);
		}
	}

	public void SetupLinearSpeed(MinMax speedRange, float speedUpDistance, Transform aimTransform)
	{
		StartCoroutine(speedUp_cr(speedRange, speedUpDistance, aimTransform));
	}

	private IEnumerator speedUp_cr(MinMax speedRange, float distance, Transform aimTransform)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		float sqrDistance = distance * distance;
		while (Vector3.SqrMagnitude(base.transform.position - aimTransform.position) > sqrDistance)
		{
			yield return wait;
		}
		float duration = KinematicUtilities.CalculateTimeToChangeVelocity(speedRange.min, speedRange.max, distance);
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			yield return wait;
			elapsedTime += CupheadTime.FixedDelta;
			Speed = Mathf.Lerp(speedRange.min, speedRange.max, elapsedTime / duration);
		}
		Speed = speedRange.max;
	}

	public void SetupVacuum(Transform aimTransform, Transform destroyTransform)
	{
		StartCoroutine(vacuumAim_cr(aimTransform, destroyTransform));
	}

	private IEnumerator vacuumAim_cr(Transform aimTransform, Transform destroyTransform)
	{
		if (isCurved)
		{
			while (curveSpeed.x < 0f)
			{
				yield return null;
			}
		}
		else if (base.transform.position.x >= OffsetAimXCutoff)
		{
			float distanceThreshold = ((!(base.transform.position.y > 0f)) ? 105f : 80f);
			base.transform.SetEulerAngles(0f, 0f, MathUtils.DirectionToAngle(aimTransform.position + OffsetAimAmount - base.transform.position));
			float x = base.transform.position.x;
			float x2 = aimTransform.position.x;
			Vector3 offsetAimAmount = OffsetAimAmount;
			if (x > x2 + offsetAimAmount.x)
			{
				while (Mathf.Abs(base.transform.position.y - aimTransform.position.y) > distanceThreshold)
				{
					yield return null;
				}
			}
			base.transform.SetEulerAngles(0f, 0f, MathUtilities.DirectionToAngle(aimTransform.position - base.transform.position));
		}
		while (base.transform.position.x < aimTransform.position.x)
		{
			yield return null;
		}
		StopAllCoroutines();
		StartCoroutine(vacuumSuckIn_cr(destroyTransform));
	}

	private IEnumerator vacuumSuckIn_cr(Transform destroyTransform)
	{
		base.transform.SetEulerAngles(0f, 0f, MathUtilities.DirectionToAngle(destroyTransform.position - base.transform.position));
		move = true;
		if (isCurved)
		{
			Speed = curveSpeed.magnitude;
		}
		Speed *= 1.25f;
		StartCoroutine(squash_cr());
		while (base.transform.position.x < destroyTransform.position.x)
		{
			yield return null;
		}
		GetComponent<SpriteRenderer>().enabled = false;
		Die();
		Object.Destroy(base.gameObject);
	}

	private IEnumerator squash_cr()
	{
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		float elapsedTime = 0f;
		while (elapsedTime < SquashDuration)
		{
			yield return wait;
			elapsedTime += wait.frameTime + wait.accumulator;
			Vector3 scale = Vector3.Lerp(Vector2.one, SquashAmount, elapsedTime / SquashDuration);
			base.transform.localScale = scale;
		}
	}

	public void ToCurve(Vector3 speed, float gravity)
	{
		curveSpeed = speed;
		this.gravity = gravity;
		StartCoroutine(gravity_cr());
	}

	private IEnumerator gravity_cr()
	{
		move = false;
		while (true)
		{
			curveSpeed += new Vector3(gravity * CupheadTime.FixedDelta, 0f);
			base.transform.Translate(curveSpeed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
	}
}
