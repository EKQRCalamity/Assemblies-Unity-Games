using System.Collections;
using UnityEngine;

public class RetroArcadeMissile : AbstractCollidableObject
{
	private LevelProperties.RetroArcade.Missile properties;

	private float loopYSize;

	private float loopXSize;

	private float circleAngle;

	private Vector3 pivotPoint;

	private DamageDealer damageDealer;

	public void Init(Vector2 pos, float rotation, LevelProperties.RetroArcade.Missile properties, Vector3 pivot)
	{
		base.transform.position = pos;
		base.transform.SetEulerAngles(0f, 0f, rotation);
		this.properties = properties;
		pivotPoint = pivot;
	}

	private void Start()
	{
		loopXSize = properties.loopXSize;
		loopYSize = properties.loopYSize;
		damageDealer = DamageDealer.NewEnemy();
		Deactivate();
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void StartCircle(bool onRight, Vector3 pivot)
	{
		circleAngle = 0f;
		pivotPoint = pivot;
		GetComponent<SpriteRenderer>().enabled = true;
		GetComponent<Collider2D>().enabled = true;
		StartCoroutine(move_in_circle_cr(onRight));
	}

	private IEnumerator move_in_circle_cr(bool onRight)
	{
		Vector3 handleRotationX = Vector3.zero;
		float rotateInCir = ((!onRight) ? 90f : (-90f));
		while (circleAngle < 6.108652f)
		{
			circleAngle += 5f * (float)CupheadTime.Delta;
			handleRotationX = ((!onRight) ? new Vector3(Mathf.Sin(circleAngle) * loopXSize, 0f, 0f) : new Vector3((0f - Mathf.Sin(circleAngle)) * loopXSize, 0f, 0f));
			Vector3 handleRotationY = new Vector3(0f, Mathf.Cos(circleAngle) * loopYSize, 0f);
			base.transform.position = pivotPoint;
			base.transform.position += handleRotationX + handleRotationY;
			TransformExtensions.SetEulerAngles(z: MathUtils.DirectionToAngle(pivotPoint - base.transform.position) + rotateInCir, transform: base.transform, x: 0f, y: 0f);
			yield return null;
		}
		Deactivate();
		yield return null;
	}

	private void Deactivate()
	{
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}
}
