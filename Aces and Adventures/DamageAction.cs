using ProtoBuf;

[ProtoContract]
[UIField("Damage", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", order = 1u)]
public class DamageAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _damageAmount;

	public override bool dealsDamage => true;

	public static void Damage(AAction action, ActionContext context, int damage, bool? ignoreShields = null)
	{
		context.gameState.DealDamage(context, context.gameState.ProcessAbilityDamage(context, action, damage), DamageSource.Ability, action, ignoreShields);
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			return _damageAmount.GetValue(context) > 0;
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		Damage(this, context, _damageAmount.GetValue(context));
	}

	protected override string _ToStringUnique()
	{
		return $"Deal {_damageAmount} damage to";
	}

	public override int GetPotentialDamage(ActionContext context)
	{
		return context.gameState.ProcessAbilityDamage(context, this, _damageAmount.GetValue(context));
	}
}
