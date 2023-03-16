using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelSnake : AbstractProjectile
{
	[SerializeField]
	private BasicProjectile snakeLine;

	private LevelProperties.FlyingCowboy.SnakeAttack properties;

	private Vector3 speed;

	private float gravity;

	private float stopPosX;

	public void Move(Vector3 position, float speedX, float speedY, float stopPosX, float gravity, LevelProperties.FlyingCowboy.SnakeAttack properties)
	{
		base.transform.position = position;
		this.properties = properties;
		speed = new Vector3(speedX, speedY);
		this.stopPosX = stopPosX;
		this.gravity = gravity;
		StartCoroutine(move_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private IEnumerator move_cr()
	{
		while (base.transform.position.x < stopPosX)
		{
			speed += new Vector3(gravity * CupheadTime.FixedDelta, 0f);
			base.transform.Translate(speed * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
		BasicProjectile snake = snakeLine.Create(base.transform.position, 0f, 0f - properties.snakeSpeed);
		Object.Destroy(base.gameObject);
	}
}
