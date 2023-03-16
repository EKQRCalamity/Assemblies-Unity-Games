using UnityEngine;

public class BasicUprightProjectile : BasicProjectile
{
	private Vector3 _direction;

	protected override Vector3 Direction => _direction;

	public override AbstractProjectile Create(Vector2 position, float rotation)
	{
		BasicUprightProjectile basicUprightProjectile = base.Create(position, 0f) as BasicUprightProjectile;
		basicUprightProjectile._direction = Quaternion.Euler(0f, 0f, rotation) * Vector3.right;
		return basicUprightProjectile;
	}
}
