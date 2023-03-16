using System.Collections;

public class WeaponPushback : AbstractLevelWeapon
{
	private const float ONE = 1f;

	private const float Y_POS = 20f;

	private const float ROTATION_OFFSET = 3f;

	private int currentY;

	private float[] yPositions = new float[4] { 0f, 20f, 40f, 20f };

	private float bulletSpeed;

	private float bulletFireRate;

	private float speedTime;

	private float forceAmount;

	private bool holdingShoot;

	private bool hasForce;

	private bool facingLeft;

	private bool forceIsLeft;

	private LevelPlayerMotor.VelocityManager.Force forceLeft;

	private LevelPlayerMotor.VelocityManager.Force forceRight;

	protected override bool rapidFire => true;

	protected override float rapidFireRate => bulletFireRate;

	private void Start()
	{
		speedTime = WeaponProperties.LevelWeaponPushback.Basic.speedTime;
		StartCoroutine(determine_speed_cr());
		forceAmount = WeaponProperties.LevelWeaponPushback.Basic.pushbackSpeed;
		forceLeft = new LevelPlayerMotor.VelocityManager.Force(LevelPlayerMotor.VelocityManager.Force.Type.All, forceAmount);
		forceRight = new LevelPlayerMotor.VelocityManager.Force(LevelPlayerMotor.VelocityManager.Force.Type.All, 0f - forceAmount);
	}

	protected override AbstractProjectile fireBasic()
	{
		BasicProjectile basicProjectile = base.fireBasic() as BasicProjectile;
		basicProjectile.Speed = bulletSpeed;
		basicProjectile.Damage = WeaponProperties.LevelWeaponPushback.Basic.damage;
		basicProjectile.PlayerId = player.id;
		float y = yPositions[currentY];
		currentY++;
		if (currentY >= yPositions.Length)
		{
			currentY = 0;
		}
		basicProjectile.transform.AddPosition(0f, y);
		bool flag = player.transform.localScale.x < 0f;
		if (!hasForce)
		{
			AddHorizontalForce(flag);
		}
		return basicProjectile;
	}

	private void Update()
	{
		facingLeft = player.transform.localScale.x < 0f;
		if ((hasForce && !holdingShoot) || forceIsLeft != facingLeft)
		{
			player.motor.RemoveForce(forceLeft);
			player.motor.RemoveForce(forceRight);
			hasForce = false;
		}
	}

	private void AddHorizontalForce(bool facingLeft)
	{
		hasForce = true;
		forceIsLeft = facingLeft;
		if (facingLeft)
		{
			player.motor.AddForce(forceLeft);
		}
		else
		{
			player.motor.AddForce(forceRight);
		}
	}

	private IEnumerator determine_speed_cr()
	{
		float t = 0f;
		float speedVal = 0f;
		float fireVal = 0f;
		while (true)
		{
			if (holdingShoot)
			{
				if (speedVal < 1f)
				{
					speedVal = t / speedTime;
					fireVal = 1f - t / speedTime;
					t += (float)CupheadTime.Delta;
				}
				else
				{
					speedVal = 1f;
					t = 1f;
				}
			}
			else if (speedVal > 0f)
			{
				speedVal = t / speedTime;
				fireVal = 1f - t / speedTime;
				t -= (float)CupheadTime.Delta;
			}
			else
			{
				speedVal = 0f;
				t = 0f;
			}
			holdingShoot = player.input.actions.GetButton(3);
			bulletSpeed = WeaponProperties.LevelWeaponPushback.Basic.speed.GetFloatAt(speedVal);
			bulletFireRate = WeaponProperties.LevelWeaponPushback.Basic.fireRate.GetFloatAt(fireVal);
			yield return null;
		}
	}
}
