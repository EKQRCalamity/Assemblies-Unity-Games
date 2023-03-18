using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;

namespace Gameplay.GameControllers.Environment.DamageObject;

public class BushDamageObject : DamageObject
{
	protected ColorFlash ColorFlash;

	protected override void OnAwake()
	{
		base.OnAwake();
		ColorFlash = GetComponentInChildren<ColorFlash>();
	}

	public override void Damage(Hit hit)
	{
		base.Damage(hit);
		ColorFlash.TriggerColorFlash();
	}

	public void Destroy()
	{
	}
}
