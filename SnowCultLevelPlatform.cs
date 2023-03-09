using System;
using System.Collections;
using UnityEngine;

public class SnowCultLevelPlatform : LevelPlatform
{
	[SerializeField]
	private float downDist = -30f;

	[SerializeField]
	private float bounceSpeed = 20f;

	private Vector3 pivotPos;

	private bool isClockwise;

	private float angle;

	private float speed;

	private float loopSizeX;

	private float loopSizeY;

	private AbstractPlayerController player1;

	private AbstractPlayerController player2;

	private bool p1IsColliding;

	private bool p2IsColliding;

	[SerializeField]
	private float sheenTimeMin = 1f;

	[SerializeField]
	private float sheenTimeMax = 5f;

	public void SetID(int value)
	{
		base.animator.SetInteger("ID", value % 5);
	}

	public void StartRotate(float angle, Vector3 pivotPoint, float loopSizeX, float loopSizeY, float speed, float pathOffset, bool isClockwise)
	{
		this.angle = angle;
		pivotPos = pivotPoint;
		this.speed = speed;
		this.loopSizeX = loopSizeX;
		this.loopSizeY = loopSizeY;
		this.isClockwise = isClockwise;
		StartCoroutine(move_rotating_platforms_cr());
		StartCoroutine(sheen_cr());
	}

	private IEnumerator move_rotating_platforms_cr()
	{
		Vector3 handleRotationX2 = Vector3.zero;
		if (angle == 0f || angle == 180f)
		{
			base.transform.parent.position = (Vector2)pivotPos + MathUtils.AngleToDirection(angle) * loopSizeX;
		}
		else
		{
			base.transform.parent.position = (Vector2)pivotPos + MathUtils.AngleToDirection(angle) * loopSizeY;
		}
		angle *= (float)Math.PI / 180f;
		while (true)
		{
			angle += speed * CupheadTime.FixedDelta;
			handleRotationX2 = ((!isClockwise) ? new Vector3((0f - Mathf.Sin(angle)) * loopSizeX, 0f, 0f) : new Vector3(Mathf.Sin(angle) * loopSizeX, 0f, 0f));
			Vector3 handleRotationY = new Vector3(0f, Mathf.Cos(angle) * loopSizeY, 0f);
			base.transform.parent.position = pivotPos;
			base.transform.parent.position += handleRotationX2 + handleRotationY;
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator sheen_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, UnityEngine.Random.Range(sheenTimeMin, sheenTimeMax));
			base.animator.SetTrigger("Sheen");
			while (!base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Sheen"))
			{
				yield return null;
			}
			while (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Sheen"))
			{
				yield return null;
			}
		}
	}

	private void FixedUpdate()
	{
		player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player1 != null)
		{
			p1IsColliding = player1.transform.parent == base.transform;
			player1.transform.SetEulerAngles(null, null, 0f);
		}
		else
		{
			p1IsColliding = false;
		}
		if (player2 != null)
		{
			p2IsColliding = player2.transform.parent == base.transform;
			player2.transform.SetEulerAngles(null, null, 0f);
		}
		else
		{
			p2IsColliding = false;
		}
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(0f, (!p1IsColliding && !p2IsColliding) ? 0f : downDist), bounceSpeed * CupheadTime.FixedDelta);
	}
}
