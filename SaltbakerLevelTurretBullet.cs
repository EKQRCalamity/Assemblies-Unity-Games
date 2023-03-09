using UnityEngine;

public class SaltbakerLevelTurretBullet : BasicProjectile
{
	private SaltbakerLevelSaltbaker parent;

	public SaltbakerLevelTurretBullet Create(Vector3 pos, float rotation, float speed, SaltbakerLevelSaltbaker parent)
	{
		SaltbakerLevelTurretBullet saltbakerLevelTurretBullet = base.Create(pos, rotation, speed) as SaltbakerLevelTurretBullet;
		saltbakerLevelTurretBullet.parent = parent;
		saltbakerLevelTurretBullet.animator.Play((!Rand.Bool()) ? "B" : "A");
		saltbakerLevelTurretBullet.animator.Update(0f);
		return saltbakerLevelTurretBullet;
	}

	protected override void Start()
	{
		base.Start();
		parent.OnDeathEvent += Die;
	}

	protected override void OnDestroy()
	{
		parent.OnDeathEvent -= Die;
		base.OnDestroy();
	}
}
