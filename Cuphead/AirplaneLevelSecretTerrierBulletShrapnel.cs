using UnityEngine;

public class AirplaneLevelSecretTerrierBulletShrapnel : BasicProjectile
{
	[SerializeField]
	private Animator anim;

	protected override Vector3 Direction => -base.transform.up;

	public override AbstractProjectile Create()
	{
		AirplaneLevelSecretTerrierBulletShrapnel airplaneLevelSecretTerrierBulletShrapnel = (AirplaneLevelSecretTerrierBulletShrapnel)base.Create();
		airplaneLevelSecretTerrierBulletShrapnel.anim.Play("Move", 0, Random.Range(0, 1));
		return airplaneLevelSecretTerrierBulletShrapnel;
	}
}
