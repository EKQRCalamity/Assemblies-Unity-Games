namespace Gameplay.GameControllers.Entities.Animations;

public abstract class AttackAnimationsEvents : EntityAnimationEvents
{
	public enum Activation
	{
		True,
		False
	}

	public virtual void CurrentWeaponRawAttack(DamageArea.DamageType damageType)
	{
	}

	public abstract void CurrentWeaponAttack(DamageArea.DamageType damageType);

	public abstract void WeaponBlowUp(float weaponBlowUp);
}
